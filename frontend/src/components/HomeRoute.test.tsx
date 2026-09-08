import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { HomeRoute } from "./HomeRoute";

const mockUseAuth = vi.fn();

vi.mock("../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderHome() {
  return render(
    <MemoryRouter initialEntries={["/"]}>
      <Routes>
        <Route path="/" element={<HomeRoute />} />
        <Route path="/dashboard/individual" element={<div>Individual dashboard</div>} />
        <Route path="/dashboard/guest" element={<div>Guest dashboard</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("HomeRoute", () => {
  it("shows the landing page when there is no session", () => {
    mockUseAuth.mockReturnValue({ session: null, setSession: vi.fn(), clearSession: vi.fn() });

    renderHome();

    expect(screen.getAllByText(/community health hub/i).length).toBeGreaterThan(0);
  });

  it("redirects an active Individual session straight to the dashboard, never the landing page", () => {
    mockUseAuth.mockReturnValue({
      session: { token: "fake-jwt", role: "Individual", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
      setSession: vi.fn(),
      clearSession: vi.fn(),
    });

    renderHome();

    expect(screen.getByText("Individual dashboard")).toBeInTheDocument();
  });

  it("falls back to the landing page when the session has expired", () => {
    mockUseAuth.mockReturnValue({
      session: { token: "fake-jwt", role: "Individual", expiresAtUtc: new Date(Date.now() - 60_000).toISOString() },
      setSession: vi.fn(),
      clearSession: vi.fn(),
    });

    renderHome();

    expect(screen.getAllByText(/community health hub/i).length).toBeGreaterThan(0);
  });
});
