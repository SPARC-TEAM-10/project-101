import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { FacilityDashboardPage } from "./FacilityDashboardPage";
import { server } from "../../../tests/setup";
import {
  getMyFacilityApprovedHandler,
  getMyFacilityNotFoundHandler,
  getMyFacilityPendingHandler,
  getMyFacilityRejectedHandler,
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
    session: { token: "fake-jwt", role: "Hospital", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={["/dashboard/facility"]}>
        <Routes>
          <Route path="/dashboard/facility" element={<FacilityDashboardPage />} />
          <Route path="/login" element={<div>Login page</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("FacilityDashboardPage", () => {
  it("shows 'Pending Approval' status when the facility is pending (AC1)", async () => {
    server.use(getMyFacilityPendingHandler);
    renderPage();

    expect(await screen.findByText("Pending verification")).toBeInTheDocument();
    expect(screen.getByText("We're reviewing your licence")).toBeInTheDocument();
  });

  it("disables restricted actions while pending (AC2)", async () => {
    server.use(getMyFacilityPendingHandler);
    renderPage();

    const tile = await screen.findByRole("button", { name: /plan an event/i });
    expect(tile).toHaveAttribute("aria-disabled", "true");
    expect(screen.getAllByText("Locked").length).toBe(3);
  });

  it("shows the restriction explanation when a locked action is attempted (AC3)", async () => {
    server.use(getMyFacilityPendingHandler);
    renderPage();

    const tile = await screen.findByRole("button", { name: /plan an event/i });
    fireEvent.click(tile);

    expect(await screen.findByText("Feature locked until account verification.")).toBeInTheDocument();
    expect(screen.getByText(/still pending verification, so events, blood requests and inventory stay locked/i)).toBeInTheDocument();
  });

  it("shows Approved and enables all actions once verified (AC4)", async () => {
    server.use(getMyFacilityApprovedHandler);
    renderPage();

    await waitFor(() => expect(screen.getByText("What you can publish")).toBeInTheDocument());
    expect(screen.queryByText("Pending verification")).not.toBeInTheDocument();
    expect(screen.queryByText("Locked")).not.toBeInTheDocument();

    const tile = screen.getByRole("button", { name: /plan an event/i });
    expect(tile).toHaveAttribute("aria-disabled", "false");
  });

  it("shows the rejection reason and reviewed date when rejected", async () => {
    server.use(getMyFacilityRejectedHandler);
    renderPage();

    expect(await screen.findByText("Verification was not approved")).toBeInTheDocument();
    expect(screen.getByText(/licence document expired on 31 March 2025/)).toBeInTheDocument();
  });

  it("shows an error state when the facility can't be loaded", async () => {
    server.use(getMyFacilityNotFoundHandler);
    renderPage();

    expect(await screen.findByText(/couldn't load your facility/i)).toBeInTheDocument();
  });
});
