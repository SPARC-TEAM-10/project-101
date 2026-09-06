import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { http, HttpResponse } from "msw";

import { OtpVerificationPage } from "./OtpVerificationPage";
import { server } from "../../../tests/setup";
import { OTP_REQUEST_URL, gatewayErrorHandler, verifyInvalidOtpHandler } from "../../../tests/msw/handlers";

const FAR_FUTURE = new Date(Date.now() + 5 * 60 * 1000).toISOString();

function renderAt(state: unknown) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[{ pathname: "/otp-verify", state }]}>
        <Routes>
          <Route path="/otp-verify" element={<OtpVerificationPage />} />
          <Route path="/login" element={<div>Mobile Entry Screen</div>} />
          <Route path="/verified" element={<div>Verified Screen</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

function stateWithResendAt(resendAvailableAtUtc: string) {
  return {
    mobileNumber: "9876543210",
    maskedMobileNumber: "********10",
    otpExpiresAtUtc: FAR_FUTURE,
    resendAvailableAtUtc,
  };
}

const validState = stateWithResendAt(FAR_FUTURE);

async function typeCode(user: ReturnType<typeof userEvent.setup>, code: string) {
  for (let i = 0; i < code.length; i++) {
    await user.type(screen.getByLabelText(`Digit ${i + 1}`), code[i]);
  }
}

describe("OtpVerificationPage", () => {
  it("redirects to /login when no router state is present", () => {
    renderAt(undefined);

    expect(screen.getByText("Mobile Entry Screen")).toBeInTheDocument();
  });

  it("renders 6 empty digit boxes and the masked number", () => {
    renderAt(validState);

    for (let i = 1; i <= 6; i++) {
      expect(screen.getByLabelText(`Digit ${i}`)).toHaveValue("");
    }
    expect(screen.getByText("********10")).toBeInTheDocument();
  });

  it("auto-advances focus as digits are typed", async () => {
    const user = userEvent.setup();
    renderAt(validState);

    await user.type(screen.getByLabelText("Digit 1"), "4");
    expect(screen.getByLabelText("Digit 2")).toHaveFocus();
  });

  it("moves focus back on backspace from an empty box", async () => {
    const user = userEvent.setup();
    renderAt(validState);

    await user.type(screen.getByLabelText("Digit 1"), "4");
    await user.type(screen.getByLabelText("Digit 2"), "{Backspace}");
    expect(screen.getByLabelText("Digit 1")).toHaveFocus();
  });

  it("keeps the Verify OTP button disabled until all 6 digits are entered", async () => {
    const user = userEvent.setup();
    renderAt(validState);

    expect(screen.getByRole("button", { name: /verify otp/i })).toBeDisabled();
    await typeCode(user, "42715");
    expect(screen.getByRole("button", { name: /verify otp/i })).toBeDisabled();
  });

  it("auto-submits once all 6 digits are filled and navigates to /verified on success (AC1)", async () => {
    const user = userEvent.setup();
    renderAt(validState);

    await typeCode(user, "427159");

    expect(await screen.findByText("Verified Screen")).toBeInTheDocument();
  });

  it("on 422, clears all boxes, shows the AC2 message, and refocuses box 1", async () => {
    server.use(verifyInvalidOtpHandler);
    const user = userEvent.setup();
    renderAt(validState);

    await typeCode(user, "427159");

    expect(await screen.findByText("Invalid OTP. Please try again.")).toBeInTheDocument();
    for (let i = 1; i <= 6; i++) {
      expect(screen.getByLabelText(`Digit ${i}`)).toHaveValue("");
    }
    expect(screen.getByLabelText("Digit 1")).toHaveFocus();
  });

  it("disables Resend while the timer is active and enables it once it elapses (AC3)", async () => {
    renderAt(stateWithResendAt(new Date(Date.now() + 1000).toISOString()));

    expect(screen.getByText(/resend in/i)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /resend otp/i })).not.toBeInTheDocument();

    await waitFor(
      () => expect(screen.getByRole("button", { name: /resend otp/i })).toBeEnabled(),
      { timeout: 3000 },
    );
  });

  it("clicking active Resend calls requestOtp again and resets the timer", async () => {
    server.use(
      http.post(OTP_REQUEST_URL, () =>
        HttpResponse.json({
          maskedMobileNumber: "********10",
          otpExpiresAtUtc: FAR_FUTURE,
          resendAvailableAtUtc: FAR_FUTURE,
        }),
      ),
    );
    const user = userEvent.setup();
    renderAt(stateWithResendAt(new Date(Date.now() + 300).toISOString()));

    await waitFor(() => expect(screen.getByRole("button", { name: /resend otp/i })).toBeEnabled(), {
      timeout: 3000,
    });
    await user.click(screen.getByRole("button", { name: /resend otp/i }));

    await waitFor(() => expect(screen.getByText(/resend in/i)).toBeInTheDocument());
  });

  it("shows a resend error and keeps Resend usable if resend fails", async () => {
    server.use(gatewayErrorHandler);
    const user = userEvent.setup();
    renderAt(stateWithResendAt(new Date(Date.now() + 300).toISOString()));

    await waitFor(() => expect(screen.getByRole("button", { name: /resend otp/i })).toBeEnabled(), {
      timeout: 3000,
    });
    await user.click(screen.getByRole("button", { name: /resend otp/i }));

    expect(await screen.findByText(/couldn't resend the code/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /resend otp/i })).toBeInTheDocument();
  });

  it('"Change Number" navigates to /login', async () => {
    const user = userEvent.setup();
    renderAt(validState);

    await user.click(screen.getByRole("button", { name: /change number/i }));

    expect(screen.getByText("Mobile Entry Screen")).toBeInTheDocument();
  });
});
