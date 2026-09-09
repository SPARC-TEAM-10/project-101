import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { http, HttpResponse } from "msw";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { EventManagePage } from "./EventManagePage";
import { server } from "../../../tests/setup";
import { EVENT_DETAIL_ID, EVENT_DETAIL_URL, EVENT_CANCEL_URL } from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return { ...actual, useAuth: () => mockUseAuth() };
});

function eventDetailFixture(overrides: Record<string, unknown> = {}) {
  return {
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
    spotsRemaining: 17,
    coordinatorName: "Dr Anitha Varghese",
    coordinatorContact: "9000010023",
    rsvpCutoffAtUtc: null,
    status: "Published",
    cancellationReason: null,
    distanceKm: null,
    myRsvpStatus: null,
    myReferenceCode: null,
    ...overrides,
  };
}

function renderPage() {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Hospital", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[`/events/${EVENT_DETAIL_ID}/manage`]}>
        <Routes>
          <Route path="/events/:id/manage" element={<EventManagePage />} />
          <Route path="/events/mine" element={<div>My events page</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("EventManagePage", () => {
  it("shows the editable form for an upcoming event", async () => {
    server.use(http.get(EVENT_DETAIL_URL, () => HttpResponse.json(eventDetailFixture())));
    renderPage();

    expect(await screen.findByDisplayValue("Community blood drive — Kaloor")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /save and notify attendees/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Cancel event" })).toBeEnabled();
  });

  it("shows the blocked state once the event has started", async () => {
    server.use(
      http.get(EVENT_DETAIL_URL, () =>
        HttpResponse.json(eventDetailFixture({ startAtUtc: new Date(Date.now() - 60_000).toISOString() })),
      ),
    );
    renderPage();

    expect(await screen.findByText(/already started/i)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /save and notify attendees/i })).not.toBeInTheDocument();
  });

  it("shows the cancelled banner with the reason for a cancelled event", async () => {
    server.use(
      http.get(EVENT_DETAIL_URL, () =>
        HttpResponse.json(eventDetailFixture({ status: "Cancelled", cancellationReason: "The hall is unavailable after storm damage." })),
      ),
    );
    renderPage();

    expect(await screen.findByText("This event is cancelled")).toBeInTheDocument();
    expect(screen.getByText("The hall is unavailable after storm damage.")).toBeInTheDocument();
  });

  it("saves an edit and calls the update endpoint", async () => {
    server.use(
      http.get(EVENT_DETAIL_URL, () => HttpResponse.json(eventDetailFixture())),
      http.patch(EVENT_DETAIL_URL, async ({ request }) => {
        const body = (await request.json()) as Record<string, unknown>;
        return HttpResponse.json(eventDetailFixture(body));
      }),
    );
    renderPage();

    const titleInput = await screen.findByDisplayValue("Community blood drive — Kaloor");
    fireEvent.change(titleInput, { target: { value: "Updated blood drive title" } });
    fireEvent.click(screen.getByRole("button", { name: /save and notify attendees/i }));

    await waitFor(() => expect(screen.getByRole("button", { name: /save and notify attendees/i })).toBeEnabled());
  });

  it("opens the cancel modal and cancels with a valid reason", async () => {
    server.use(
      http.get(EVENT_DETAIL_URL, () => HttpResponse.json(eventDetailFixture())),
      http.post(EVENT_CANCEL_URL, async ({ request }) => {
        const body = (await request.json()) as { reason: string };
        return HttpResponse.json(eventDetailFixture({ status: "Cancelled", cancellationReason: body.reason }));
      }),
    );
    renderPage();

    await screen.findByDisplayValue("Community blood drive — Kaloor");
    fireEvent.click(screen.getByRole("button", { name: "Cancel event" }));

    expect(await screen.findByText("Cancel this event?")).toBeInTheDocument();
    const cancelButtons = screen.getAllByRole("button", { name: "Cancel event" });
    const confirmButton = cancelButtons[cancelButtons.length - 1];
    expect(confirmButton).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/why is it cancelled/i), {
      target: { value: "The hall is unavailable after storm damage." },
    });
    expect(confirmButton).toBeEnabled();
    fireEvent.click(confirmButton);

    await waitFor(() => expect(screen.queryByText("Cancel this event?")).not.toBeInTheDocument());
  });
});
