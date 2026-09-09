import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { ProfilePage } from "./ProfilePage";
import { ToastProvider } from "../../context/ToastProvider";
import { server } from "../../../tests/setup";
import { INDIVIDUALS_ME_URL } from "../../../tests/msw/handlers";
import { http, HttpResponse } from "msw";

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
        <MemoryRouter initialEntries={["/profile"]}>
          <Routes>
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/dashboard/individual" element={<div>Dashboard</div>} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

describe("ProfilePage", () => {
  it("renders the profile details once loaded", async () => {
    renderPage();

    expect(await screen.findByText("Ananya Nair")).toBeInTheDocument();
    expect(screen.getByText("O+")).toBeInTheDocument();
    expect(screen.getByText("Eligible to donate")).toBeInTheDocument();
  });

  it("switches to an editable form pre-filled with the current location when Edit is clicked", async () => {
    renderPage();

    fireEvent.click(await screen.findByRole("button", { name: "Edit" }));

    const locationInput = await screen.findByLabelText(/Location \(City \/ Area\)/);
    expect(locationInput).toHaveValue("Kaloor, Kochi");
    expect(screen.getByRole("checkbox", { name: /Chronic illness/ })).not.toBeChecked();
  });

  it("saves changes and returns to the read-only view", async () => {
    renderPage();

    fireEvent.click(await screen.findByRole("button", { name: "Edit" }));

    const locationInput = await screen.findByLabelText(/Location \(City \/ Area\)/);
    fireEvent.change(locationInput, { target: { value: "Ernakulam" } });
    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));

    await waitFor(() => expect(screen.queryByRole("button", { name: "Saving…" })).not.toBeInTheDocument());
    expect(screen.getAllByText("Ernakulam").length).toBeGreaterThan(0);
  });

  it("shows the other-illness textarea only once Other is checked, and requires details", async () => {
    renderPage();

    fireEvent.click(await screen.findByRole("button", { name: "Edit" }));
    await screen.findByLabelText(/Location \(City \/ Area\)/);

    expect(screen.queryByLabelText(/Specify other illness/)).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole("checkbox", { name: "Other" }));
    expect(screen.getByLabelText(/Specify other illness/)).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));
    expect(await screen.findByText("Please specify other illness.")).toBeInTheDocument();
  });

  it("discards edits when Cancel is clicked", async () => {
    renderPage();

    fireEvent.click(await screen.findByRole("button", { name: "Edit" }));
    const locationInput = await screen.findByLabelText(/Location \(City \/ Area\)/);
    fireEvent.change(locationInput, { target: { value: "Somewhere Else" } });

    fireEvent.click(screen.getByRole("button", { name: "Cancel" }));

    expect(screen.queryByLabelText(/Location \(City \/ Area\)/)).not.toBeInTheDocument();
    expect(screen.getAllByText("Kaloor, Kochi").length).toBeGreaterThan(0);
  });

  describe("CHH-85: location sharing", () => {
    it("shows a 'Share my location' action when the profile has no coordinates yet", async () => {
      renderPage();

      expect(await screen.findByRole("button", { name: "Share my location" })).toBeInTheDocument();
    });

    it("auto-saves and shows a 'Shared' badge once geolocation resolves", async () => {
      mockGeolocationSuccess();
      renderPage();

      fireEvent.click(await screen.findByRole("button", { name: "Share my location" }));

      expect(await screen.findByText("Shared")).toBeInTheDocument();
      expect(screen.queryByRole("button", { name: "Share my location" })).not.toBeInTheDocument();
    });

    it("shows a 'Shared' badge on load when the profile already has coordinates", async () => {
      server.use(
        http.get(INDIVIDUALS_ME_URL, () =>
          HttpResponse.json({
            id: "33333333-3333-3333-3333-333333333333",
            fullName: "Ananya Nair",
            bloodGroup: "O+",
            isReceiverOnly: false,
            locationCityArea: "Kaloor, Kochi",
            createdAtUtc: "2026-08-01T00:00:00.000Z",
            isChronicIllness: false,
            hasRecentSurgery: false,
            isInfectiousDisease: false,
            isUnderweight: false,
            isOtherIllness: false,
            otherIllnessDetails: null,
            latitude: 9.9312,
            longitude: 76.2673,
          }),
        ),
      );
      renderPage();

      expect(await screen.findByText("Shared")).toBeInTheDocument();
    });

    it("shows a denial message with a retry action when permission is denied", async () => {
      mockGeolocationDenied();
      renderPage();

      fireEvent.click(await screen.findByRole("button", { name: "Share my location" }));

      expect(await screen.findByText("Permission denied")).toBeInTheDocument();
      expect(screen.getByRole("button", { name: "Try again" })).toBeInTheDocument();
    });
  });
});
