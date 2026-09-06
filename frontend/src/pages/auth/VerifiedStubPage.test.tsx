import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { VerifiedStubPage } from "./VerifiedStubPage";

function renderAt(state: unknown) {
  return render(
    <MemoryRouter initialEntries={[{ pathname: "/verified", state }]}>
      <Routes>
        <Route path="/verified" element={<VerifiedStubPage />} />
        <Route path="/login" element={<div>Mobile Entry Screen</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("VerifiedStubPage", () => {
  it("redirects to /login when no router state is present", () => {
    renderAt(undefined);

    expect(screen.getByText("Mobile Entry Screen")).toBeInTheDocument();
  });

  it("renders the masked mobile number from state", () => {
    renderAt({ maskedMobileNumber: "********10", verifiedAtUtc: "2026-09-06T10:00:00.000Z" });

    expect(screen.getByText(/\*\*\*\*\*\*\*\*10/)).toBeInTheDocument();
  });
});
