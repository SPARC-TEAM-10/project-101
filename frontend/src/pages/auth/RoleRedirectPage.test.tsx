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
        <Route path="/dashboard/facility" element={<div>Facility Dashboard</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("RoleRedirectPage", () => {
  it("TC-CHH-F01-54: redirects to /login when there is no session", () => {
    renderWithSession(null);

    expect(screen.getByText("Login Screen")).toBeInTheDocument();
  });

  it("TC-CHH-F01-55: redirects to /dashboard/individual for an Individual role", () => {
    renderWithSession({ token: "t", role: "Individual", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Individual Dashboard")).toBeInTheDocument();
  });

  it("TC-CHH-F01-56: redirects to /welcome for a Guest role (CHH-11)", () => {
    renderWithSession({ token: "t", role: "Guest", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Welcome Screen")).toBeInTheDocument();
  });

  it("TC-CHH-F01-10: redirects to /dashboard/facility for a Hospital role (CHH-28)", () => {
    renderWithSession({ token: "t", role: "Hospital", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Facility Dashboard")).toBeInTheDocument();
  });

  it("TC-CHH-F01-11: redirects to /dashboard/facility for an Ngo role (CHH-28)", () => {
    renderWithSession({ token: "t", role: "Ngo", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Facility Dashboard")).toBeInTheDocument();
  });
});
