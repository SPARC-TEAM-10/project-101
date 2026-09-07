import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { RegisterIndividualPage } from "./RegisterIndividualPage";
import { ToastProvider } from "../../context/ToastProvider";
import { server } from "../../../tests/setup";
import { registerIndividualConflictHandler } from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();
const mockSetSession = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderPage() {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Guest", expiresAtUtc: "2099-01-01T00:00:00.000Z", mobileNumber: "9876543210" },
    setSession: mockSetSession,
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <MemoryRouter initialEntries={["/register/individual"]}>
          <Routes>
            <Route path="/register/individual" element={<RegisterIndividualPage />} />
            <Route path="/dashboard/individual" element={<div>Individual Dashboard</div>} />
            <Route path="/dashboard/guest" element={<div>Guest Dashboard</div>} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

function fillMandatoryFields() {
  fireEvent.change(screen.getByLabelText(/full name/i), { target: { value: "Jane Doe" } });
  fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "jane@example.com" } });
  fireEvent.click(screen.getByRole("button", { name: "O+" }));
  fireEvent.change(screen.getByLabelText(/date of birth/i), { target: { value: "1998-05-10" } });
  fireEvent.click(screen.getByRole("button", { name: "Female" }));
  fireEvent.change(screen.getByLabelText(/location \(city\/area\)/i), { target: { value: "Kaloor, Kochi" } });
}

describe("RegisterIndividualPage", () => {
  beforeEach(() => {
    mockSetSession.mockClear();
  });

  it("renders all mandatory fields and the submit button", () => {
    renderPage();

    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByText("Blood group")).toBeInTheDocument();
    expect(screen.getByLabelText(/date of birth/i)).toBeInTheDocument();
    expect(screen.getByText("Gender")).toBeInTheDocument();
    expect(screen.getByLabelText(/location \(city\/area\)/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /complete registration/i })).toBeInTheDocument();
  });

  it("renders a chip button per blood group", () => {
    renderPage();

    expect(screen.getByRole("button", { name: "O+" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "AB-" })).toBeInTheDocument();
  });

  it("selecting a blood group chip marks it pressed", () => {
    renderPage();

    const oPositive = screen.getByRole("button", { name: "O+" });
    fireEvent.click(oPositive);

    expect(oPositive).toHaveAttribute("aria-pressed", "true");
  });

  it("defaults the eligibility preview to Eligible Donor", () => {
    renderPage();

    expect(screen.getByText("Eligible Donor")).toBeInTheDocument();
  });

  it("flips the eligibility preview to Receiver Only when a health flag is checked", () => {
    renderPage();

    fireEvent.click(screen.getByLabelText("Chronic illness"));

    expect(screen.getByText("Receiver Only")).toBeInTheDocument();
    expect(screen.queryByText("Eligible Donor")).not.toBeInTheDocument();
  });

  it("reveals the other-illness detail field only when Other is checked", () => {
    renderPage();

    expect(screen.queryByLabelText(/specify other illness/i)).not.toBeInTheDocument();

    fireEvent.click(screen.getByLabelText("Other"));

    expect(screen.getByLabelText(/specify other illness/i)).toBeInTheDocument();
  });

  it("shows validation errors after a submit attempt with empty mandatory fields", async () => {
    renderPage();

    fireEvent.click(screen.getByRole("button", { name: /complete registration/i }));

    expect(await screen.findByText(/please enter your full name/i)).toBeInTheDocument();
  });

  it("requires other-illness details once Other is checked and submission is attempted", async () => {
    renderPage();

    fillMandatoryFields();
    fireEvent.click(screen.getByLabelText("Other"));
    fireEvent.click(screen.getByRole("button", { name: /complete registration/i }));

    expect(await screen.findByText(/please specify other illness/i)).toBeInTheDocument();
  });

  it("submits successfully, bumps the session role, and navigates to the individual dashboard", async () => {
    renderPage();

    fillMandatoryFields();
    fireEvent.click(screen.getByRole("button", { name: /complete registration/i }));

    await waitFor(() => expect(screen.getByText("Individual Dashboard")).toBeInTheDocument());
    expect(mockSetSession).toHaveBeenCalledWith(expect.objectContaining({ role: "Individual" }));
  });

  it("shows the manual-fallback message when current-location detection is denied", async () => {
    Object.defineProperty(global.navigator, "geolocation", {
      configurable: true,
      value: {
        getCurrentPosition: (_success: PositionCallback, error: PositionErrorCallback) =>
          error({ code: 1, message: "denied" } as GeolocationPositionError),
      },
    });
    renderPage();

    fireEvent.click(screen.getByRole("button", { name: /use current location/i }));

    expect(
      await screen.findByText(/we couldn't determine your location\. please select your city\/area manually\./i),
    ).toBeInTheDocument();
  });

  it("shows an error banner and stays on the page when the API rejects the submission", async () => {
    server.use(registerIndividualConflictHandler);
    renderPage();

    fillMandatoryFields();
    fireEvent.click(screen.getByRole("button", { name: /complete registration/i }));

    // The same message also renders in a toast, so assert at least one match rather than a
    // single unique node.
    expect(
      (await screen.findAllByText("A profile already exists for this mobile number.")).length,
    ).toBeGreaterThan(0);
    expect(mockSetSession).not.toHaveBeenCalled();
  });

  it("navigates back to the guest dashboard via the Back link", () => {
    renderPage();

    fireEvent.click(screen.getByRole("link", { name: /back/i }));

    expect(screen.getByText("Guest Dashboard")).toBeInTheDocument();
  });
});
