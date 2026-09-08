import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { RequesterMatchDashboardPage } from "./RequesterMatchDashboardPage";
import { ToastProvider } from "../../context/ToastProvider";
import { server } from "../../../tests/setup";
import {
  getBloodRequestMatchStatusNoMatchesHandler,
  getBloodRequestMatchStatusWithDonorsHandler,
  updateBloodRequestRadiusSuccessHandler,
} from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderPage() {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Individual", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <MemoryRouter initialEntries={["/blood-requests/11111111-1111-1111-1111-111111111111/matches"]}>
          <Routes>
            <Route path="/blood-requests/:id/matches" element={<RequesterMatchDashboardPage />} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

describe("RequesterMatchDashboardPage", () => {
  it("shows the no-eligible-donors state with an Increase Radius action", async () => {
    server.use(getBloodRequestMatchStatusNoMatchesHandler);
    renderPage();

    expect(await screen.findByText("No eligible donors found within 10km")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Increase Radius" })).toBeInTheDocument();
  });

  it("increases the radius and re-fetches the match status", async () => {
    server.use(getBloodRequestMatchStatusNoMatchesHandler);
    renderPage();

    fireEvent.click(await screen.findByRole("button", { name: "Increase Radius" }));
    expect(screen.getByText(/Expand to 20km/)).toBeInTheDocument();

    server.use(updateBloodRequestRadiusSuccessHandler);
    fireEvent.click(screen.getByRole("button", { name: "Confirm" }));

    await waitFor(() => expect(screen.getByText(/radius 20km/)).toBeInTheDocument());
  });

  it("renders notified/viewed/accepted counts and the donor list", async () => {
    server.use(getBloodRequestMatchStatusWithDonorsHandler);
    renderPage();

    expect(await screen.findByText("Matched donors")).toBeInTheDocument();
    expect(screen.getByText("Donor 1")).toBeInTheDocument();
    expect(screen.getByText("Donor 3")).toBeInTheDocument();
    expect(screen.getByText("Ravi Kumar")).toBeInTheDocument();
    expect(screen.getByText("9123456789")).toBeInTheDocument();
    // Two "Pending" pills (Donor 1, Donor 3); "Accepted" also appears as the stat-card label.
    expect(screen.getAllByText("Pending")).toHaveLength(2);
    expect(screen.getAllByText("Accepted").length).toBeGreaterThan(0);
  });
});
