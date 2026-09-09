import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { EventDiscoveryPage } from "./EventDiscoveryPage";
import { server } from "../../../tests/setup";
import { searchEventsEmptyHandler, searchEventsSuccessHandler } from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return { ...actual, useAuth: () => mockUseAuth() };
});

// Leaflet's Circle (vector/SVG layer) throws under jsdom's incomplete SVG renderer support —
// a real gap shared with RadiusMap.tsx, just never exercised there since no existing test
// resolves geolocation far enough to mount it. Marker-only map components (VenuePinMap) are
// unaffected and don't need this. Map-view rendering itself is verified visually (Puppeteer).
vi.mock("../../components/EventDiscoveryMap", () => ({
  EventDiscoveryMap: () => <div data-testid="event-discovery-map" />,
}));

function mockGeolocationSuccess() {
  Object.defineProperty(global.navigator, "geolocation", {
    configurable: true,
    value: {
      getCurrentPosition: (success: PositionCallback) =>
        success({ coords: { latitude: 9.9312, longitude: 76.2673 } } as GeolocationPosition),
    },
  });
}

function mockGeolocationDenied() {
  Object.defineProperty(global.navigator, "geolocation", {
    configurable: true,
    value: {
      getCurrentPosition: (_success: PositionCallback, error: PositionErrorCallback) =>
        error({ code: 1, message: "denied" } as GeolocationPositionError),
    },
  });
}

function renderPage() {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Individual", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={["/events"]}>
        <Routes>
          <Route path="/events" element={<EventDiscoveryPage />} />
          <Route path="/dashboard/individual" element={<div>Individual dashboard</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("EventDiscoveryPage", () => {
  it("shows the events list within the default radius (AC1)", async () => {
    mockGeolocationSuccess();
    server.use(searchEventsSuccessHandler);
    renderPage();

    expect(await screen.findByText("Community blood drive — Kaloor")).toBeInTheDocument();
    expect(screen.getByText("Free health screening camp")).toBeInTheDocument();
  });

  it("shows the AC2 empty-state message with no events found", async () => {
    mockGeolocationSuccess();
    server.use(searchEventsEmptyHandler);
    renderPage();

    expect(await screen.findByText(/no events found in your area/i)).toBeInTheDocument();
  });

  it("shows the manual-location fallback when GPS is denied (Edge Case)", async () => {
    mockGeolocationDenied();
    renderPage();

    expect(await screen.findByText(/location is switched off for this site/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/search from a city or area/i)).toBeInTheDocument();
  });

  it("switches to Map view when the Map toggle is clicked", async () => {
    mockGeolocationSuccess();
    server.use(searchEventsSuccessHandler);
    renderPage();

    await screen.findByText("Community blood drive — Kaloor");
    fireEvent.click(screen.getByRole("button", { name: "Map" }));

    expect(await screen.findByTestId("event-discovery-map")).toBeInTheDocument();
    expect(screen.queryByText("Community blood drive — Kaloor")).not.toBeInTheDocument();
  });

  it("filters by event type when a type chip is clicked", async () => {
    mockGeolocationSuccess();
    server.use(searchEventsSuccessHandler);
    renderPage();

    await screen.findByText("Community blood drive — Kaloor");
    fireEvent.click(screen.getByRole("button", { name: "Health Camp" }));

    await waitFor(() => expect(screen.queryByText("Community blood drive — Kaloor")).not.toBeInTheDocument());
  });
});
