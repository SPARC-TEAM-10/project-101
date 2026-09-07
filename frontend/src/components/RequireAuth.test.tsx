import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { RequireAuth } from "./RequireAuth";
import type { AuthSession, Role } from "../context/AuthProvider";

const FUTURE = new Date(Date.now() + 60 * 60 * 1000).toISOString();
const mockUseAuth = vi.fn();

vi.mock("../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderProtected(session: AuthSession | null, roles?: Role[]) {
  mockUseAuth.mockReturnValue({ session, setSession: vi.fn(), clearSession: vi.fn() });

  return render(
    <MemoryRouter initialEntries={["/protected"]}>
      <Routes>
        <Route
          path="/protected"
          element={
            <RequireAuth roles={roles}>
              <div>Protected content</div>
            </RequireAuth>
          }
        />
        <Route path="/login" element={<div>Login Screen</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("RequireAuth", () => {
  it("redirects to /login when there is no session", () => {
    renderProtected(null);

    expect(screen.getByText("Login Screen")).toBeInTheDocument();
    expect(screen.queryByText("Protected content")).not.toBeInTheDocument();
  });

  it("redirects to /login when the session role is not in the allowed roles", () => {
    renderProtected({ token: "t", role: "Individual", expiresAtUtc: FUTURE }, ["Guest"]);

    expect(screen.getByText("Login Screen")).toBeInTheDocument();
  });

  it("renders children when authenticated and no roles restriction is given", () => {
    renderProtected({ token: "t", role: "Individual", expiresAtUtc: FUTURE });

    expect(screen.getByText("Protected content")).toBeInTheDocument();
  });

  it("renders children when authenticated and the role matches", () => {
    renderProtected({ token: "t", role: "Individual", expiresAtUtc: FUTURE }, ["Individual", "Guest"]);

    expect(screen.getByText("Protected content")).toBeInTheDocument();
  });

  it("redirects to /login when the session has expired", () => {
    const past = new Date(Date.now() - 1000).toISOString();
    renderProtected({ token: "t", role: "Individual", expiresAtUtc: past });

    expect(screen.getByText("Login Screen")).toBeInTheDocument();
    expect(screen.queryByText("Protected content")).not.toBeInTheDocument();
  });

  it("renders children when the session has not yet expired and the role matches", () => {
    renderProtected({ token: "t", role: "Guest", expiresAtUtc: FUTURE }, ["Guest"]);

    expect(screen.getByText("Protected content")).toBeInTheDocument();
  });
});
