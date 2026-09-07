import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { RoleRedirectPage } from "./RoleRedirectPage";
import type { AuthSession, Role } from "../../context/AuthProvider";

const mockUseAuth = vi.fn();
const mockClearSession = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderWithSession(session: AuthSession | null) {
  mockUseAuth.mockReturnValue({ session, setSession: vi.fn(), clearSession: mockClearSession });

  return render(
    <MemoryRouter initialEntries={["/redirecting"]}>
      <Routes>
        <Route path="/redirecting" element={<RoleRedirectPage />} />
        <Route path="/login" element={<div>Login Screen</div>} />
        <Route path="/dashboard/individual" element={<div>Individual Dashboard</div>} />
        <Route path="/dashboard/guest" element={<div>Guest Dashboard</div>} />
        <Route path="/dashboard/hospital" element={<div>Hospital Dashboard</div>} />
        <Route path="/dashboard/ngo" element={<div>NGO Dashboard</div>} />
        <Route path="/dashboard/admin" element={<div>Admin Dashboard</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("RoleRedirectPage", () => {
  beforeEach(() => {
    mockClearSession.mockClear();
  });

  it("redirects to /login when there is no session", () => {
    renderWithSession(null);

    expect(screen.getByText("Login Screen")).toBeInTheDocument();
  });

  it("redirects to /dashboard/individual for an Individual role", () => {
    renderWithSession({ token: "t", role: "Individual", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Individual Dashboard")).toBeInTheDocument();
  });

  it("redirects to /dashboard/guest for a Guest role", () => {
    renderWithSession({ token: "t", role: "Guest", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Guest Dashboard")).toBeInTheDocument();
  });

  it("redirects to /dashboard/hospital for a Hospital role", () => {
    renderWithSession({ token: "t", role: "Hospital", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Hospital Dashboard")).toBeInTheDocument();
  });

  it("redirects to /dashboard/ngo for a Ngo role", () => {
    renderWithSession({ token: "t", role: "Ngo", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("NGO Dashboard")).toBeInTheDocument();
  });

  it("redirects to /dashboard/admin for a SystemAdmin role", () => {
    renderWithSession({ token: "t", role: "SystemAdmin", expiresAtUtc: "2099-01-01T00:00:00.000Z" });

    expect(screen.getByText("Admin Dashboard")).toBeInTheDocument();
  });

  describe("when the role has no mapped dashboard route", () => {
    // A role value the frontend's Role union doesn't recognize yet — e.g. the backend rolling
    // out a new role ahead of a frontend release. Deliberately cast past the type system to
    // exercise this defensive branch, which real user input can never trigger via the UI itself.
    const unmappableSession = {
      token: "t",
      role: "SomeFutureRole" as Role,
      expiresAtUtc: "2099-01-01T00:00:00.000Z",
    };

    it("renders the 'couldn't open your dashboard' error instead of navigating anywhere", () => {
      renderWithSession(unmappableSession);

      expect(screen.getByText("We couldn't open your dashboard")).toBeInTheDocument();
      expect(screen.queryByText("Login Screen")).not.toBeInTheDocument();
      expect(screen.queryByText(/Dashboard$/)).not.toBeInTheDocument();
    });

    it("'Sign in again' clears the session and navigates to /login", async () => {
      const user = userEvent.setup();
      renderWithSession(unmappableSession);

      await user.click(screen.getByRole("button", { name: "Sign in again" }));

      expect(mockClearSession).toHaveBeenCalledTimes(1);
      expect(screen.getByText("Login Screen")).toBeInTheDocument();
    });

    it("'Try again' does not clear the session", async () => {
      const user = userEvent.setup();
      renderWithSession(unmappableSession);

      await user.click(screen.getByRole("button", { name: "Try again" }));

      expect(mockClearSession).not.toHaveBeenCalled();
    });
  });
});
