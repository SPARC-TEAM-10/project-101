import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { RoleRedirectPage } from "./RoleRedirectPage";
import type { AuthSession } from "../../context/AuthProvider";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderWithSession(session: AuthSession | null) {
  mockUseAuth.mockReturnValue({ session, setSession: vi.fn(), clearSession: vi.fn() });

  return render(
    <MemoryRouter initialEntries={["/redirecting"]}>
      <Routes>
        <Route path="/redirecting" element={<RoleRedirectPage />} />
        <Route path="/login" element={<div>Login Screen</div>} />
        <Route path="/dashboard/individual" element={<div>Individual Dashboard</div>} />
        <Route path="/welcome" element={<div>Welcome Screen</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("RoleRedirectPage", () => {
  it("redirects to /login when there is no session", () => {
    renderWithSession(null);

    expect(screen.getByText("Login Screen")).toBeInTheDocument();
  });

  it("redirects to /dashboard/individual for an Individual role", () => {
    renderWithSession({ token: "t", role: "Individual", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Individual Dashboard")).toBeInTheDocument();
  });

  it("redirects to /welcome for a Guest role (CHH-11)", () => {
    renderWithSession({ token: "t", role: "Guest", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Welcome Screen")).toBeInTheDocument();
  });
});
