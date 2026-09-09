import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { EventAnalyticsPage } from "./EventAnalyticsPage";
import { server } from "../../../tests/setup";
import {
  EVENT_DETAIL_ID,
  getAttendanceSummarySuccessHandler,
  getAttendanceSummaryNotFoundHandler,
  getAttendanceParticipantsSuccessHandler,
  exportAttendanceCsvSuccessHandler,
} from "../../../tests/msw/handlers";

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
      <MemoryRouter initialEntries={[`/events/${EVENT_DETAIL_ID}/analytics`]}>
        <Routes>
          <Route path="/events/:id/analytics" element={<EventAnalyticsPage />} />
          <Route path="/events/:id/manage" element={<div>Manage page</div>} />
          <Route path="/events/mine" element={<div>My events page</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("EventAnalyticsPage", () => {
  it("shows the summary stat cards", async () => {
    server.use(getAttendanceSummarySuccessHandler, getAttendanceParticipantsSuccessHandler);
    renderPage();

    expect(await screen.findByText("Community blood drive — Kaloor")).toBeInTheDocument();
    expect(screen.getByText("Attendance rate")).toBeInTheDocument();
    expect(screen.getByText("72%")).toBeInTheDocument();
    expect((await screen.findAllByText("31")).length).toBeGreaterThan(0);
  });

  it("shows the funnel bars with notified/rsvp'd/attended/no-show values", async () => {
    server.use(getAttendanceSummarySuccessHandler, getAttendanceParticipantsSuccessHandler);
    renderPage();

    await screen.findByText("From notified to attended");
    expect(screen.getByText("340")).toBeInTheDocument();
  });

  it("shows the participant list", async () => {
    server.use(getAttendanceSummarySuccessHandler, getAttendanceParticipantsSuccessHandler);
    renderPage();

    expect(await screen.findByText("Rahul Suresh")).toBeInTheDocument();
    expect(screen.getByText("Tom Jacob")).toBeInTheDocument();
    expect(screen.getByText("Vishnu Nair")).toBeInTheDocument();
    expect((await screen.findAllByText("No-show")).length).toBeGreaterThan(0);
  });

  it("filters participants when a status chip is clicked", async () => {
    server.use(getAttendanceSummarySuccessHandler, getAttendanceParticipantsSuccessHandler);
    renderPage();

    await screen.findByText("Rahul Suresh");
    fireEvent.click(screen.getByRole("button", { name: /^No-show/ }));

    await waitFor(() => expect(screen.queryByText("Rahul Suresh")).not.toBeInTheDocument());
    expect(await screen.findByText("Tom Jacob")).toBeInTheDocument();
  });

  it("shows a not-found message when the event doesn't exist", async () => {
    server.use(getAttendanceSummaryNotFoundHandler);
    renderPage();

    expect(await screen.findByText(/couldn't be found/i)).toBeInTheDocument();
  });

  // jsdom logs a harmless "Not implemented: navigation" stderr line here — the mocked
  // URL.createObjectURL (tests/setup.ts) returns a fake blob: URL, and jsdom's anchor-click
  // handling tries (and fails) to navigate to it; the download itself is a no-op under jsdom
  // either way, so the assertion below only checks the button re-enables afterward.
  it("triggers a CSV export when Export CSV is clicked", async () => {
    server.use(getAttendanceSummarySuccessHandler, getAttendanceParticipantsSuccessHandler, exportAttendanceCsvSuccessHandler);
    renderPage();

    await screen.findByText("Community blood drive — Kaloor");
    fireEvent.click(screen.getByRole("button", { name: /export csv/i }));

    await waitFor(() => expect(screen.getByRole("button", { name: /export csv/i })).toBeEnabled());
  });
});
