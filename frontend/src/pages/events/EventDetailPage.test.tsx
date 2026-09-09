import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi, beforeEach, afterEach } from "vitest";

import { EventDetailPage } from "./EventDetailPage";
import { server } from "../../../tests/setup";
import {
  EVENT_DETAIL_ID,
  EVENT_DETAIL_URL,
  EVENT_RSVP_URL,
  getEventNotGoingHandler,
  getEventFullHandler,
  getEventNotFoundHandler,
  rsvpToEventFullHandler,
} from "../../../tests/msw/handlers";

// RSVP/Cancel mutate server state, then the page refetches GET /events/{id} — a static fixture
// handler can't reflect that transition, so these two flows use a small in-memory toggle instead.
function statefulRsvpHandlers(initialStatus: "Going" | null) {
  let status = initialStatus;
  const detail = () =>
    HttpResponse.json({
      id: EVENT_DETAIL_ID,
      title: "Community blood drive — Kaloor",
      eventType: "BloodDonationCamp",
      description: "Walk-in donors welcome.",
      facilityName: "Kochi Metro Hospital",
      venueName: "Kaloor Community Hall",
      venueAddress: "Stadium Link Road, Kaloor, Kochi 682017",
      latitude: 9.996,
      longitude: 76.299,
      startAtUtc: "2026-09-13T09:00:00.000Z",
      endAtUtc: "2026-09-13T14:00:00.000Z",
      capacity: 60,
      spotsRemaining: status === "Going" ? 17 : 18,
      coordinatorName: "Dr Anitha Varghese",
      coordinatorContact: "9000010023",
      rsvpCutoffAtUtc: null,
      status: "Published",
      distanceKm: 4.2,
      myRsvpStatus: status,
      myReferenceCode: status === "Going" ? "A24" : null,
    });

  return [
    http.get(EVENT_DETAIL_URL, detail),
    http.post(EVENT_RSVP_URL, () => {
      status = "Going";
      return HttpResponse.json({ eventId: EVENT_DETAIL_ID, status: "Going", referenceCode: "A24", spotsRemaining: 17 });
    }),
    http.delete(EVENT_RSVP_URL, () => {
      status = null;
      return HttpResponse.json({ eventId: EVENT_DETAIL_ID, status: "Cancelled", referenceCode: null, spotsRemaining: 18 });
    }),
  ];
}

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return { ...actual, useAuth: () => mockUseAuth() };
});

function renderPage(role: "Individual" | "Hospital" = "Individual") {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role, expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[`/events/${EVENT_DETAIL_ID}`]}>
        <Routes>
          <Route path="/events/:id" element={<EventDetailPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("EventDetailPage", () => {
  beforeEach(() => {
    vi.spyOn(window, "confirm").mockReturnValue(true);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("shows the event's before-RSVP state with an RSVP button (AC1)", async () => {
    server.use(getEventNotGoingHandler);
    renderPage();

    expect(await screen.findByText("Community blood drive — Kaloor")).toBeInTheDocument();
    expect(screen.getByText(/Kochi Metro Hospital/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "RSVP" })).toBeInTheDocument();
  });

  it("RSVPs and shows the going state with a reference code (AC1)", async () => {
    server.use(...statefulRsvpHandlers(null));
    renderPage();

    await screen.findByRole("button", { name: "RSVP" });
    fireEvent.click(screen.getByRole("button", { name: "RSVP" }));

    expect((await screen.findAllByText(/You're going/)).length).toBeGreaterThan(0);
    expect((await screen.findAllByText(/reference A24/)).length).toBeGreaterThan(0);
  });

  it("disables RSVP and shows Event Full when there's no remaining capacity (AC2)", async () => {
    server.use(getEventFullHandler);
    renderPage();

    const button = await screen.findByRole("button", { name: "Event Full" });
    expect(button).toBeDisabled();
  });

  it("surfaces the 422 error message when RSVPing to a now-full event", async () => {
    server.use(getEventNotGoingHandler, rsvpToEventFullHandler);
    renderPage();

    await screen.findByRole("button", { name: "RSVP" });
    fireEvent.click(screen.getByRole("button", { name: "RSVP" }));

    expect(await screen.findByText("This event is full.")).toBeInTheDocument();
  });

  it("cancels an active RSVP after confirmation (Edge Case)", async () => {
    server.use(...statefulRsvpHandlers("Going"));
    renderPage();

    await screen.findByRole("button", { name: "Cancel my RSVP" });
    fireEvent.click(screen.getByRole("button", { name: "Cancel my RSVP" }));

    await waitFor(() => expect(screen.getByRole("button", { name: "RSVP" })).toBeInTheDocument());
  });

  it("hides the RSVP action bar for a non-Individual viewer", async () => {
    server.use(getEventNotGoingHandler);
    renderPage("Hospital");

    await screen.findByText("Community blood drive — Kaloor");
    expect(screen.queryByRole("button", { name: "RSVP" })).not.toBeInTheDocument();
  });

  it("shows a not-found message for a missing event", async () => {
    server.use(getEventNotFoundHandler);
    renderPage();

    expect(await screen.findByText(/couldn't be found/i)).toBeInTheDocument();
  });
});
