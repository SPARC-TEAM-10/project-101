import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { CreateEventPage } from "./CreateEventPage";
import { ToastProvider } from "../../context/ToastProvider";
import { server } from "../../../tests/setup";
import { createEventForbiddenHandler, createEventSuccessHandler } from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();
const mockNavigate = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return { ...actual, useAuth: () => mockUseAuth() };
});

vi.mock("react-router-dom", async (importOriginal) => {
  const actual = await importOriginal<typeof import("react-router-dom")>();
  return { ...actual, useNavigate: () => mockNavigate };
});

function renderPage() {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Hospital", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <MemoryRouter initialEntries={["/events/new"]}>
          <Routes>
            <Route path="/events/new" element={<CreateEventPage />} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

function futureLocalValue(hoursFromNow: number): string {
  const d = new Date(Date.now() + hoursFromNow * 60 * 60 * 1000);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function fillValidForm() {
  fireEvent.change(screen.getByLabelText(/event title/i), { target: { value: "Community blood drive — Kaloor" } });
  fireEvent.change(screen.getByLabelText(/event type/i), { target: { value: "BloodDonationCamp" } });
  fireEvent.change(screen.getByLabelText(/description/i), {
    target: { value: "Walk-in donors welcome. Bring a photo ID. Refreshments provided." },
  });
  fireEvent.change(screen.getByLabelText(/venue name/i), { target: { value: "Kaloor Community Hall" } });
  fireEvent.change(screen.getByLabelText(/venue address/i), { target: { value: "Stadium Link Road, Kaloor, Kochi 682017" } });
  fireEvent.change(screen.getByLabelText(/starts/i), { target: { value: futureLocalValue(48) } });
  fireEvent.change(screen.getByLabelText(/ends/i), { target: { value: futureLocalValue(53) } });
  fireEvent.change(screen.getByLabelText(/capacity/i), { target: { value: "60" } });
  fireEvent.change(screen.getByLabelText(/coordinator name/i), { target: { value: "Dr Anitha Varghese" } });
  fireEvent.change(screen.getByLabelText(/coordinator contact/i), { target: { value: "9000010023" } });
}

describe("CreateEventPage", () => {
  it("renders all mandatory fields (AC1)", () => {
    renderPage();

    expect(screen.getByText("New event")).toBeInTheDocument();
    expect(screen.getByLabelText(/event title/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/event type/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/venue name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/venue address/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/capacity/i)).toBeInTheDocument();
  });

  it("shows a past-start validation error and does not submit (AC2)", async () => {
    server.use(createEventSuccessHandler);
    renderPage();
    fillValidForm();
    fireEvent.change(screen.getByLabelText(/starts/i), { target: { value: futureLocalValue(-1) } });

    fireEvent.click(screen.getAllByRole("button", { name: /publish event/i })[0]);

    await waitFor(() => expect(screen.getByText(/event must start in the future/i)).toBeInTheDocument());
    expect(mockNavigate).not.toHaveBeenCalledWith("/dashboard/facility");
  });

  it("publishes successfully and navigates to the facility dashboard", async () => {
    server.use(createEventSuccessHandler);
    renderPage();
    fillValidForm();

    await waitFor(() => expect(screen.getByText(/resolved from the address/i)).toBeInTheDocument(), { timeout: 3000 });
    fireEvent.click(screen.getAllByRole("button", { name: /publish event/i })[0]);

    await waitFor(() => expect(mockNavigate).toHaveBeenCalledWith("/dashboard/facility"));
  });

  it("shows a 403 toast when the facility isn't verified", async () => {
    server.use(createEventForbiddenHandler);
    renderPage();
    fillValidForm();

    await waitFor(() => expect(screen.getByText(/resolved from the address/i)).toBeInTheDocument(), { timeout: 3000 });
    fireEvent.click(screen.getAllByRole("button", { name: /publish event/i })[0]);

    await waitFor(() => expect(screen.getByText(/verified facility/i)).toBeInTheDocument());
  });

  it("reveals the RSVP cut-off field only once the toggle is on", () => {
    renderPage();

    expect(screen.queryByLabelText(/rsvp cut-off/i)).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole("switch", { name: /close rsvps early/i }));

    expect(screen.getByLabelText(/rsvp cut-off/i)).toBeInTheDocument();
  });
});
