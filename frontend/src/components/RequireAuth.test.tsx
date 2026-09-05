import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { RequireAuth } from "./RequireAuth";
import type { AuthSession } from "../context/AuthProvider";

const mockUseAuth = vi.fn();

vi.mock("../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderProtected(session: AuthSession | null, roles?: string[]) {
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
    renderProtected({ token: "t", role: "Individual" }, ["HospitalAdmin"]);

    expect(screen.getByText("Login Screen")).toBeInTheDocument();
  });

  it("renders children when authenticated and no roles restriction is given", () => {
    renderProtected({ token: "t", role: "Individual" });

    expect(screen.getByText("Protected content")).toBeInTheDocument();
  });

  it("renders children when authenticated and the role matches", () => {
    renderProtected({ token: "t", role: "Individual" }, ["Individual", "HospitalAdmin"]);

    expect(screen.getByText("Protected content")).toBeInTheDocument();
  });
});
