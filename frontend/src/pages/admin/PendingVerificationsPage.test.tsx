import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { PendingVerificationsPage } from "./PendingVerificationsPage";
import { server } from "../../../tests/setup";
import {
  pendingFacilitiesEmptyHandler,
  pendingFacilitiesErrorHandler,
  reviewFacilityErrorHandler,
} from "../../../tests/msw/handlers";

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
    session: { token: "test-token", role: "SystemAdmin", expiresAtUtc: new Date(Date.now() + 3600_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={["/admin"]}>
        <Routes>
          <Route path="/admin" element={<PendingVerificationsPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("PendingVerificationsPage", () => {
  it("renders pending facility rows once the list loads", async () => {
    renderPage();

    await waitFor(() => expect(screen.getAllByText("Sreedhara Multispeciality").length).toBeGreaterThan(0));
    expect(screen.getAllByText("Vayali Jeevan Trust").length).toBeGreaterThan(0);
  });

  it("shows the AC2 empty-state message when there are no pending facilities", async () => {
    server.use(pendingFacilitiesEmptyHandler);
    renderPage();

    await waitFor(() =>
      expect(screen.getByText("No pending verifications at this time.")).toBeInTheDocument(),
    );
  });

  it("shows an error state with a retry action on request failure", async () => {
    server.use(pendingFacilitiesErrorHandler);
    renderPage();

    await waitFor(() => expect(screen.getByText("The verification queue didn't load.")).toBeInTheDocument());
    expect(screen.getByRole("button", { name: /try again/i })).toBeInTheDocument();
  });

  it("shows Missing licence badges since CHH-73's data never populates a document URL", async () => {
    renderPage();

    await waitFor(() => expect(screen.getAllByText("Missing").length).toBeGreaterThan(0));
  });

  it("links to the Users page and disables the Verified/Rejected tabs", async () => {
    renderPage();

    await waitFor(() => expect(screen.getAllByText("Sreedhara Multispeciality").length).toBeGreaterThan(0));
    expect(screen.getAllByRole("link", { name: /users/i }).length).toBeGreaterThan(0);
    expect(screen.getAllByTitle("Coming in a later story").length).toBeGreaterThan(0);
  });

  it("opens the review modal, shows the missing-document warning, and disables Approve", async () => {
    renderPage();
    await waitFor(() => expect(screen.getAllByText("Sreedhara Multispeciality").length).toBeGreaterThan(0));

    fireEvent.click(screen.getAllByRole("button", { name: /^review/i })[0]);

    await waitFor(() => expect(screen.getByText("Document missing")).toBeInTheDocument());
    expect(screen.getByRole("button", { name: /approve/i })).toBeDisabled();
  });

  it("approves a facility with a document and closes the review modal", async () => {
    renderPage();
    await waitFor(() => expect(screen.getAllByText("Vayali Jeevan Trust").length).toBeGreaterThan(0));

    const rows = screen.getAllByRole("button", { name: /^review/i });
    fireEvent.click(rows[rows.length - 1]);

    const approveButton = await screen.findByRole("button", { name: /approve/i });
    expect(approveButton).not.toBeDisabled();
    fireEvent.click(approveButton);

    await waitFor(() => expect(screen.queryByRole("button", { name: /approve/i })).not.toBeInTheDocument());
  });

  it("requires a rejection reason before confirming a reject", async () => {
    renderPage();
    await waitFor(() => expect(screen.getAllByText("Sreedhara Multispeciality").length).toBeGreaterThan(0));

    fireEvent.click(screen.getAllByRole("button", { name: /^review/i })[0]);
    fireEvent.click(await screen.findByRole("button", { name: /^reject$/i }));

    const confirmButton = screen.getByRole("button", { name: /confirm rejection/i });
    expect(confirmButton).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/reason for rejection/i), { target: { value: "Licence expired" } });
    expect(confirmButton).not.toBeDisabled();
  });

  it("shows an inline error when the review request fails", async () => {
    server.use(reviewFacilityErrorHandler);
    renderPage();
    await waitFor(() => expect(screen.getAllByText("Sreedhara Multispeciality").length).toBeGreaterThan(0));

    fireEvent.click(screen.getAllByRole("button", { name: /^review/i })[1]);
    fireEvent.click(await screen.findByRole("button", { name: /approve/i }));

    await waitFor(() => expect(screen.getByText(/couldn't submit the review/i)).toBeInTheDocument());
  });

  it("re-fetches when Try again is clicked after an error", async () => {
    server.use(pendingFacilitiesErrorHandler);
    renderPage();
    await waitFor(() => expect(screen.getByRole("button", { name: /try again/i })).toBeInTheDocument());

    server.resetHandlers();
    fireEvent.click(screen.getByRole("button", { name: /try again/i }));

    await waitFor(() => expect(screen.getAllByText("Sreedhara Multispeciality").length).toBeGreaterThan(0));
  });
});
