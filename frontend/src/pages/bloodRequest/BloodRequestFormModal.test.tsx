import { render, screen, fireEvent } from "@testing-library/react";
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
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

describe("BloodRequestFormModal", () => {
  it("renders as a dialog with all mandatory fields and the submit button", () => {
    renderModal();

    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(screen.getByLabelText(/patient name/i)).toBeInTheDocument();
    expect(screen.getByText("Blood group")).toBeInTheDocument();
    expect(screen.getByLabelText(/units required/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/location \(city\/area\)/i)).toBeInTheDocument();
    expect(screen.getByText("Urgency")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /notify donors/i })).toBeInTheDocument();
  });

  it("renders a chip button per blood group instead of a dropdown", () => {
    renderModal();

    expect(screen.getByRole("button", { name: "O+" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "AB-" })).toBeInTheDocument();
  });

  it("selecting a blood group chip marks it pressed", () => {
    renderModal();

    const oPositive = screen.getByRole("button", { name: "O+" });
    fireEvent.click(oPositive);

    expect(oPositive).toHaveAttribute("aria-pressed", "true");
  });

  it("closes and navigates home when the close button is clicked", () => {
    renderModal();

    fireEvent.click(screen.getByRole("button", { name: /close/i }));

    expect(screen.getByText("Home")).toBeInTheDocument();
  });

  it("updates the radius readout when the slider changes", () => {
    renderModal();

    fireEvent.change(screen.getByLabelText(/search radius in kilometers/i), { target: { value: "50" } });

    // Shows in both the header badge and the radius preview label.
    expect(screen.getAllByText("50 km").length).toBeGreaterThan(0);
  });

  it("shows the Use current location button", () => {
    renderModal();

    expect(screen.getByRole("button", { name: /use current location/i })).toBeInTheDocument();
  });

  it("shows validation errors after a submit attempt with empty mandatory fields", async () => {
    renderModal();

    fireEvent.click(screen.getByRole("button", { name: /notify donors/i }));

    expect(await screen.findByText(/please enter the patient's name/i)).toBeInTheDocument();
  });
});
