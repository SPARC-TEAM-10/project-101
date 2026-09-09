import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { http, HttpResponse } from "msw";

import { EmergencyHubPage } from "./EmergencyHubPage";
import { server } from "../../../tests/setup";
import {
  FACILITIES_SEARCH_URL,
  searchFacilitiesSuccessHandler,
  searchFacilitiesEmptyHandler,
  searchFacilitiesErrorHandler,
  getFacilityByIdSuccessHandler,
} from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function mockGeolocationDenied() {
  Object.defineProperty(global.navigator, "geolocation", {
    configurable: true,
    value: {
      getCurrentPosition: (_success: PositionCallback, error: PositionErrorCallback) =>
        error({ code: 1, message: "denied" } as GeolocationPositionError),
    },
  });
}

function mockGeolocationResolved() {
  Object.defineProperty(global.navigator, "geolocation", {
    configurable: true,
    value: {
      getCurrentPosition: (success: PositionCallback) =>
        success({ coords: { latitude: 9.9312, longitude: 76.2673 } } as GeolocationPosition),
    },
  });
}

function renderPage(geolocation: "denied" | "resolved" = "denied") {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Guest", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });
  if (geolocation === "resolved") {
    mockGeolocationResolved();
  } else {
    mockGeolocationDenied();
  }

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={["/emergency"]}>
        <Routes>
          <Route path="/emergency" element={<EmergencyHubPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("EmergencyHubPage", () => {
  it("renders result cards on a successful search", async () => {
    server.use(searchFacilitiesSuccessHandler);
    renderPage();

    expect(await screen.findByText("City General Hospital")).toBeInTheDocument();
  });

  it("renders the 'No results found' empty state, and Clear Filters resets the filters", async () => {
    const user = userEvent.setup();
    server.use(searchFacilitiesEmptyHandler);
    renderPage();

    expect(await screen.findByText("No results found")).toBeInTheDocument();

    const searchInput = screen.getByLabelText(/search by name or area/i);
    await user.type(searchInput, "City Hospital");
    await waitFor(() => expect(searchInput).toHaveValue("City Hospital"));

    await user.click(await screen.findByRole("button", { name: /hospital/i }));
    await user.click(await screen.findByRole("button", { name: /clear filters/i }));

    await waitFor(() => expect(searchInput).toHaveValue(""));
  });

  it("renders the manual-entry prompt when geolocation is denied", async () => {
    server.use(searchFacilitiesEmptyHandler);
    renderPage();

    expect(await screen.findByLabelText(/enter city\/area manually/i)).toBeInTheDocument();
  });

  it("renders an inline error state with a working Retry button on search failure", async () => {
    const user = userEvent.setup();
    server.use(searchFacilitiesErrorHandler);
    renderPage();

    expect(await screen.findByText(/couldn't load results/i)).toBeInTheDocument();

    server.use(searchFacilitiesSuccessHandler);
    await user.click(screen.getByRole("button", { name: /retry/i }));

    expect(await screen.findByText("City General Hospital")).toBeInTheDocument();
  });

  it("re-triggers the search with the selected category", async () => {
    const user = userEvent.setup();
    let lastCategory: string | null = null;
    server.use(
      http.get(FACILITIES_SEARCH_URL, ({ request }) => {
        lastCategory = new URL(request.url).searchParams.get("category");
        return HttpResponse.json({ items: [], totalCount: 0, page: 1, pageSize: 20 });
      }),
    );
    renderPage();
    await screen.findByText("No results found");

    await user.click(screen.getByRole("button", { name: "Ambulance" }));

    await waitFor(() => expect(lastCategory).toBe("Ambulance"));
  });

  it("hides the manual-entry banner and omits distance when geolocation resolves without a stored-coordinate match", async () => {
    server.use(
      http.get(FACILITIES_SEARCH_URL, () =>
        HttpResponse.json({
          items: [
            {
              id: "55555555-5555-5555-5555-555555555555",
              facilityName: "Rural Health Clinic",
              category: "Ngo",
              subCategory: "Trust",
              address: "Vypin, Kochi",
              latitude: null,
              longitude: null,
              contacts: [],
              distanceKm: null,
            },
          ],
          totalCount: 1,
          page: 1,
          pageSize: 20,
        }),
      ),
    );
    renderPage("resolved");

    expect(await screen.findByText("Rural Health Clinic")).toBeInTheDocument();
    expect(screen.queryByText(/km away/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/enter city\/area manually/i)).not.toBeInTheDocument();
  });

  it("opens the facility detail modal when a result card is clicked", async () => {
    const user = userEvent.setup();
    server.use(searchFacilitiesSuccessHandler, getFacilityByIdSuccessHandler);
    renderPage();

    await user.click(await screen.findByText("City General Hospital"));

    const modal = await screen.findByRole("dialog");
    expect(await within(modal).findByRole("link", { name: /view on map/i })).toBeInTheDocument();
  });
});
