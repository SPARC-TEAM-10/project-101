import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { OtpVerificationPage } from "./OtpVerificationPage";

function renderAt(state: unknown) {
  return render(
    <MemoryRouter initialEntries={[{ pathname: "/otp-verify", state }]}>
      <Routes>
        <Route path="/otp-verify" element={<OtpVerificationPage />} />
        <Route path="/login" element={<div>Mobile Entry Screen</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("OtpVerificationPage", () => {
  it("redirects to /login when no router state is present", () => {
    renderAt(undefined);

    expect(screen.getByText("Mobile Entry Screen")).toBeInTheDocument();
  });

  it("shows the masked mobile number when valid state is present", () => {
    renderAt({
      mobileNumber: "9876543210",
      maskedMobileNumber: "********10",
      otpExpiresAtUtc: "2026-09-03T10:05:00.000Z",
      resendAvailableAtUtc: "2026-09-03T10:02:00.000Z",
    });

    expect(screen.getByText(/\*\*\*\*\*\*\*\*10/)).toBeInTheDocument();
  });
});
