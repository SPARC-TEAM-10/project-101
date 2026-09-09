import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { BloodRequestFormModal } from "./BloodRequestFormModal";
import { ToastProvider } from "../../context/ToastProvider";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

// react-leaflet doesn't tear down cleanly under jsdom (a real gap shared with
// EventDiscoveryMap.tsx's test — see that file's comment); only relevant here because this is
// the first test in this file to resolve geolocation far enough to mount the map at all.
vi.mock("../../components/RadiusMap", () => ({
  RadiusMap: () => <div data-testid="radius-map" />,
}));

function renderModal() {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Individual" },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <MemoryRouter initialEntries={["/blood-requests/new"]}>
          <Routes>
            <Route path="/blood-requests/new" element={<BloodRequestFormModal />} />
            <Route path="/" element={<div>Home</div>} />
            <Route path="/blood-requests/:id/matches" element={<div>Match status page</div>} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

describe("BloodRequestFormModal", () => {
  it("TC-CHH-F04-16: renders as a dialog with all mandatory fields and the submit button", () => {
    renderModal();

    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(screen.getByLabelText(/your name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/patient name/i)).toBeInTheDocument();
    expect(screen.getByText("Blood group")).toBeInTheDocument();
    expect(screen.getByLabelText(/units required/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/location \(city\/area\)/i)).toBeInTheDocument();
    expect(screen.getByText("Urgency")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /notify donors/i })).toBeInTheDocument();
  });

  it("TC-CHH-F04-17: renders a chip button per blood group instead of a dropdown", () => {
    renderModal();

    expect(screen.getByRole("button", { name: "O+" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "AB-" })).toBeInTheDocument();
  });

  it("TC-CHH-F04-18: selecting a blood group chip marks it pressed", () => {
    renderModal();

    const oPositive = screen.getByRole("button", { name: "O+" });
    fireEvent.click(oPositive);

    expect(oPositive).toHaveAttribute("aria-pressed", "true");
  });

  it("TC-CHH-F04-19: closes and navigates home when the close button is clicked", () => {
    renderModal();

    fireEvent.click(screen.getByRole("button", { name: /close/i }));

    expect(screen.getByText("Home")).toBeInTheDocument();
  });

  it("TC-CHH-F04-20: updates the radius readout when the slider changes", () => {
    renderModal();

    fireEvent.change(screen.getByLabelText(/search radius in kilometers/i), { target: { value: "50" } });

    // Shows in both the header badge and the radius preview label.
    expect(screen.getAllByText("50 km").length).toBeGreaterThan(0);
  });

  it("TC-CHH-F04-21: shows the Use current location button", () => {
    renderModal();

    expect(screen.getByRole("button", { name: /use current location/i })).toBeInTheDocument();
  });

  it("TC-CHH-F04-22: shows validation errors after a submit attempt with empty mandatory fields", async () => {
    renderModal();

    fireEvent.click(screen.getByRole("button", { name: /notify donors/i }));

    expect(await screen.findByText(/please enter the patient's name/i)).toBeInTheDocument();
  });

  it("CHH-85 regression: navigates to the match-status page (not the dashboard) after a successful submit", async () => {
    Object.defineProperty(global.navigator, "geolocation", {
      configurable: true,
      value: {
        getCurrentPosition: (success: PositionCallback) =>
          success({ coords: { latitude: 9.9312, longitude: 76.2673 } } as GeolocationPosition),
      },
    });

    renderModal();

    fireEvent.change(screen.getByLabelText(/patient name/i), { target: { value: "Jane Doe" } });
    fireEvent.click(screen.getByRole("button", { name: "O+" }));
    fireEvent.change(screen.getByLabelText(/location \(city\/area\)/i), { target: { value: "Kaloor, Kochi" } });
    fireEvent.click(screen.getByRole("button", { name: "Standard" }));
    fireEvent.click(screen.getByRole("button", { name: /use current location/i }));
    await waitFor(() => expect(screen.getByText(/location detected/i)).toBeInTheDocument());

    fireEvent.click(screen.getByRole("button", { name: /notify donors/i }));

    expect(await screen.findByText("Match status page")).toBeInTheDocument();
    expect(screen.queryByText("Home")).not.toBeInTheDocument();
  });
});
