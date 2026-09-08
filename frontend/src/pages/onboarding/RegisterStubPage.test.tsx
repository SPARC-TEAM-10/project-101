import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { RegisterStubPage } from "./RegisterStubPage";
import { ToastProvider } from "../../context/ToastProvider";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function makeToken(sub: string): string {
  const payload = btoa(JSON.stringify({ sub })).replace(/\+/g, "-").replace(/\//g, "_");
  return `header.${payload}.signature`;
}

function renderPage(setSession = vi.fn()) {
  mockUseAuth.mockReturnValue({
    session: { token: makeToken("9876543210"), role: "Guest", expiresAtUtc: "2099-01-01T00:00:00.000Z" },
    setSession,
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
            <Route path="/register/individual" element={<RegisterStubPage />} />
            <Route path="/register" element={<div>Role Selection</div>} />
            <Route path="/redirecting" element={<div>Redirecting</div>} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

function fillValidForm() {
  fireEvent.change(screen.getByLabelText(/full name/i), { target: { value: "Asha Menon" } });
  fireEvent.change(screen.getByLabelText(/email/i), { target: { value: "asha.menon@example.com" } });
  fireEvent.click(screen.getByLabelText(/blood group/i));
  fireEvent.click(screen.getByRole("option", { name: "O+" }));
  fireEvent.click(screen.getByLabelText(/date of birth/i));
  fireEvent.click(screen.getByLabelText(/^year$/i));
  fireEvent.click(screen.getByRole("option", { name: "2000" }));
  fireEvent.click(screen.getByLabelText(/^month$/i));
  fireEvent.click(screen.getByRole("option", { name: "January" }));
  fireEvent.click(screen.getByRole("button", { name: "15" }));
  fireEvent.click(screen.getByLabelText(/gender/i));
  fireEvent.click(screen.getByRole("option", { name: "Female" }));
  fireEvent.change(screen.getByLabelText(/location \(city/i), { target: { value: "Kochi, Ernakulam" } });
}

describe("RegisterStubPage (CHH-F02 Individual Registration)", () => {
  it("renders all mandatory personal-detail fields", () => {
    renderPage();

    expect(screen.getAllByText("Create Account").length).toBeGreaterThan(0);
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/blood group/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/date of birth/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/gender/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/location \(city/i)).toBeInTheDocument();
  });

  it("renders the five health-screening checkboxes (US-CHH-002-02 AC1)", () => {
    renderPage();

    expect(screen.getByLabelText(/chronic illness/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/recent surgery/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/infectious disease/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/currently underweight/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^other$/i)).toBeInTheDocument();
  });

  it("defaults to the Eligible donor banner", () => {
    renderPage();

    expect(screen.getByText("Eligible donor")).toBeInTheDocument();
  });

  it("switches to the Receiver only banner as soon as a restriction is checked (AC2)", () => {
    renderPage();

    fireEvent.click(screen.getByLabelText(/chronic illness/i));

    expect(screen.getByText("Receiver only")).toBeInTheDocument();
  });

  it("shows a conditional, required 'Specify other illness' field only when Other is checked", async () => {
    renderPage();

    expect(screen.queryByLabelText(/specify other illness/i)).not.toBeInTheDocument();

    fireEvent.click(screen.getByLabelText(/^other$/i));
    expect(screen.getByLabelText(/specify other illness/i)).toBeInTheDocument();

    fillValidForm();
    fireEvent.click(screen.getByRole("button", { name: "Create Account" }));

    await waitFor(() => expect(screen.getByText("Please specify other illness.")).toBeInTheDocument());
  });

  it("shows validation errors and stays on the page when submitted empty", () => {
    renderPage();

    fireEvent.click(screen.getByRole("button", { name: "Create Account" }));

    expect(
      screen.getByText("Please enter your full name. Name must be between 2 and 50 characters."),
    ).toBeInTheDocument();
  });

  it("registers, promotes the session to Individual, and redirects on success", async () => {
    const setSession = vi.fn();
    renderPage(setSession);

    fillValidForm();
    fireEvent.click(screen.getByRole("button", { name: "Create Account" }));

    await waitFor(() => expect(screen.getByText("Redirecting")).toBeInTheDocument());
    expect(setSession).toHaveBeenCalledWith(expect.objectContaining({ role: "Individual" }));
  });
});
