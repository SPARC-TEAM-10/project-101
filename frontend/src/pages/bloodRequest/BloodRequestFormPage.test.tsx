import { render, screen, fireEvent } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { BloodRequestFormPage } from "./BloodRequestFormPage";

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
    session: { token: "fake-jwt", role: "Individual" },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={["/blood-requests/new"]}>
        <Routes>
          <Route path="/blood-requests/new" element={<BloodRequestFormPage />} />
          <Route path="/" element={<div>Home</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("BloodRequestFormPage", () => {
  it("renders all mandatory fields and the submit button", () => {
    renderPage();

    expect(screen.getByLabelText(/patient name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/blood group/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/units required/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/location \(city\/area\)/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/urgency/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /notify donors/i })).toBeInTheDocument();
  });

  it("shows the radius preview reflecting the default radius", () => {
    renderPage();

    expect(screen.getByText("10km radius")).toBeInTheDocument();
  });

  it("updates the radius preview when the slider changes", () => {
    renderPage();

    fireEvent.change(screen.getByLabelText(/search radius in kilometers/i), { target: { value: "50" } });

    expect(screen.getByText("50km radius")).toBeInTheDocument();
  });

  it("shows validation errors after a submit attempt with empty mandatory fields", async () => {
    renderPage();

    fireEvent.click(screen.getByRole("button", { name: /notify donors/i }));

    expect(await screen.findByText(/please enter the patient's name/i)).toBeInTheDocument();
  });
});
