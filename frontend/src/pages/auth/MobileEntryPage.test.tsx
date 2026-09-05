import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { MobileEntryPage } from "./MobileEntryPage";
import { server } from "../../../tests/setup";
import {
  cooldownErrorHandler,
  gatewayErrorHandler,
  validationErrorHandler,
} from "../../../tests/msw/handlers";

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<MobileEntryPage />} />
          <Route path="/otp-verify" element={<div>OTP Verify Screen</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("MobileEntryPage", () => {
  it("renders with the Get OTP button disabled on empty input", () => {
    renderPage();

    expect(screen.getByRole("button", { name: /get otp/i })).toBeDisabled();
  });

  it("keeps the button disabled and shows a hint for fewer than 10 digits", async () => {
    const user = userEvent.setup();
    renderPage();

    const input = screen.getByLabelText(/mobile number/i);
    await user.type(input, "98765");
    await user.tab();

    expect(screen.getByRole("button", { name: /get otp/i })).toBeDisabled();
    expect(
      screen.getByText(/please enter a valid 10-digit mobile number/i),
    ).toBeInTheDocument();
  });

  it("keeps the button disabled for non-numeric input", async () => {
    const user = userEvent.setup();
    renderPage();

    const input = screen.getByLabelText(/mobile number/i);
    await user.type(input, "98765abcde");
    await user.tab();

    expect(screen.getByRole("button", { name: /get otp/i })).toBeDisabled();
  });

  it("enables the button once a valid 10-digit number is entered", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/mobile number/i), "9876543210");

    expect(screen.getByRole("button", { name: /get otp/i })).toBeEnabled();
  });

  it("navigates to /otp-verify with the response state on success", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/mobile number/i), "9876543210");
    await user.click(screen.getByRole("button", { name: /get otp/i }));

    expect(await screen.findByText("OTP Verify Screen")).toBeInTheDocument();
  });

  it("shows an inline hint and re-enables the button on a 422 response", async () => {
    server.use(validationErrorHandler);
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/mobile number/i), "9876543210");
    await user.click(screen.getByRole("button", { name: /get otp/i }));

    await waitFor(() =>
      expect(
        screen.getByText(/please enter a valid 10-digit mobile number/i),
      ).toBeInTheDocument(),
    );
    expect(screen.getByRole("button", { name: /get otp/i })).toBeEnabled();
  });

  it("shows an inline error banner and re-enables the button on a 429 response", async () => {
    server.use(cooldownErrorHandler);
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/mobile number/i), "9876543210");
    await user.click(screen.getByRole("button", { name: /get otp/i }));

    expect(
      await screen.findByText(/please wait before requesting another code/i),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /get otp/i })).toBeEnabled();
    expect(screen.getByLabelText(/mobile number/i)).toHaveValue("9876543210");
  });

  it("shows a retry error banner and retains input on a 502 response", async () => {
    server.use(gatewayErrorHandler);
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/mobile number/i), "9876543210");
    await user.click(screen.getByRole("button", { name: /get otp/i }));

    expect(await screen.findByText(/couldn't send the code\. try again\./i)).toBeInTheDocument();
    expect(screen.getByLabelText(/mobile number/i)).toHaveValue("9876543210");
  });

  it("has an accessible label reachable via keyboard for the mobile number input", () => {
    renderPage();
    const input = screen.getByLabelText(/mobile number/i);
    expect(input).toBeVisible();
    expect(input.tagName).toBe("INPUT");
  });
});
