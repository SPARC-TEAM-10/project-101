import { render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { http, HttpResponse } from "msw";

import { server } from "../../../tests/setup";
import { INDIVIDUALS_ME_URL, BLOOD_REQUESTS_MINE_URL } from "../../../tests/msw/handlers";
import { IndividualDashboardPage } from "./IndividualDashboardPage";

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

  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={["/dashboard/individual"]}>
        <Routes>
          <Route path="/dashboard/individual" element={<IndividualDashboardPage />} />
          <Route path="/login" element={<div>Login page</div>} />
          <Route path="/blood-requests/new" element={<div>New request form</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("IndividualDashboardPage", () => {
  it("renders the profile summary and request history once loaded", async () => {
    server.use(http.get(BLOOD_REQUESTS_MINE_URL, () =>
      HttpResponse.json({
        items: [
          {
            id: "1",
            patientName: "John Doe",
            bloodGroup: "O+",
            unitsRequired: 2,
            locationCityArea: "Kaloor, Kochi",
            searchRadiusKm: 10,
            urgency: "Standard",
            status: "Expired",
            createdAtUtc: "2026-08-01T00:00:00.000Z",
            expiresAtUtc: "2026-08-01T06:00:00.000Z",
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      }),
    ));

    renderPage();

    expect(await screen.findByText("Ananya Nair")).toBeInTheDocument();
    expect(screen.getAllByText(/Kaloor, Kochi/).length).toBeGreaterThan(0);
    expect(await screen.findByText(/2 units/)).toBeInTheDocument();
  });

  it("shows a registration prompt when the profile returns 404", async () => {
    server.use(http.get(INDIVIDUALS_ME_URL, () => new HttpResponse(null, { status: 404 })));

    renderPage();

    expect(await screen.findByText(/finish setting up your profile/i)).toBeInTheDocument();
  });

  it("shows the three honest empty states for not-yet-built sections", async () => {
    renderPage();

    await waitFor(() => expect(screen.getByText("Ananya Nair")).toBeInTheDocument());

    expect(screen.getByText("No donor responses yet")).toBeInTheDocument();
    expect(screen.getByText("Nothing to read yet")).toBeInTheDocument();
    expect(screen.getByText("No events near you yet")).toBeInTheDocument();
  });

  it("shows an empty state for request history when there are no requests", async () => {
    renderPage();

    expect(await screen.findByText(/haven't created a blood request yet/i)).toBeInTheDocument();
  });
});
