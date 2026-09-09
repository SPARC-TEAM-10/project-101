import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it, vi } from "vitest";

import { FacilityDetailModal } from "./FacilityDetailModal";
import { server } from "../../../tests/setup";
import {
  getFacilityByIdSuccessHandler,
  getFacilityByIdNoCoordinatesHandler,
  getFacilityByIdNotFoundHandler,
} from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderModal(onClose: () => void = vi.fn()) {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Guest", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <FacilityDetailModal facilityId="44444444-4444-4444-4444-444444444441" onClose={onClose} />
    </QueryClientProvider>,
  );
}

describe("FacilityDetailModal", () => {
  it("renders Facility Name, Category, Full Address, and Contacts on success", async () => {
    server.use(getFacilityByIdSuccessHandler);
    renderModal();

    expect(await screen.findByText("City General Hospital")).toBeInTheDocument();
    expect(screen.getByText("Hospital")).toBeInTheDocument();
    expect(screen.getByText("MG Road, Kochi")).toBeInTheDocument();
    expect(screen.getByText("Anitha Kurian")).toBeInTheDocument();
  });

  it("renders each contact's mobile number as a tel: link", async () => {
    server.use(getFacilityByIdSuccessHandler);
    renderModal();

    const callLink = await screen.findByRole("link", { name: /call anitha kurian/i });
    expect(callLink).toHaveAttribute("href", "tel:9876500111");
  });

  it("shows a 'View on Map' link with the correct href when coordinates exist", async () => {
    server.use(getFacilityByIdSuccessHandler);
    renderModal();

    const mapLink = await screen.findByRole("link", { name: /view on map/i });
    expect(mapLink).toHaveAttribute("href", "https://www.google.com/maps/search/?api=1&query=9.9816,76.2999");
  });

  it("does not show a 'View on Map' link when coordinates are absent", async () => {
    server.use(getFacilityByIdNoCoordinatesHandler);
    renderModal();

    await screen.findByText("City General Hospital");
    expect(screen.queryByRole("link", { name: /view on map/i })).not.toBeInTheDocument();
  });

  it("renders an error state, and the header Close button still works", async () => {
    const onClose = vi.fn();
    server.use(getFacilityByIdNotFoundHandler);
    renderModal(onClose);

    expect(await screen.findByText(/couldn't load this facility/i)).toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: "Close" }));
    expect(onClose).toHaveBeenCalledOnce();
  });
});
