import { render, screen, fireEvent } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { MyEventsPage } from "./MyEventsPage";
import { server } from "../../../tests/setup";
import { getMyEventsSuccessHandler, getMyEventsEmptyHandler } from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return { ...actual, useAuth: () => mockUseAuth() };
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
      <MemoryRouter initialEntries={["/events/mine"]}>
        <Routes>
          <Route path="/events/mine" element={<MyEventsPage />} />
          <Route path="/events/:id/manage" element={<div>Manage page</div>} />
          <Route path="/events/new" element={<div>Create event page</div>} />
          <Route path="/dashboard/facility" element={<div>Facility dashboard</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("MyEventsPage", () => {
  it("lists the facility's own events", async () => {
    server.use(getMyEventsSuccessHandler);
    renderPage();

    expect(await screen.findByText("Community blood drive — Kaloor")).toBeInTheDocument();
  });

  it("shows an empty state with a link to plan an event", async () => {
    server.use(getMyEventsEmptyHandler);
    renderPage();

    expect(await screen.findByText(/haven't published any events yet/i)).toBeInTheDocument();
  });

  it("navigates to the manage page when an event is clicked", async () => {
    server.use(getMyEventsSuccessHandler);
    renderPage();

    fireEvent.click(await screen.findByText("Community blood drive — Kaloor"));

    expect(await screen.findByText("Manage page")).toBeInTheDocument();
  });
});
