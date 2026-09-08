import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { UsersPage } from "./UsersPage";
import { server } from "../../../tests/setup";
import { searchAdminUsersEmptyHandler, searchAdminUsersErrorHandler, suspendUserErrorHandler } from "../../../tests/msw/handlers";

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
      <MemoryRouter initialEntries={["/admin/users"]}>
        <Routes>
          <Route path="/admin/users" element={<UsersPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("UsersPage", () => {
  it("renders user rows once the list loads", async () => {
    renderPage();

    await waitFor(() => expect(screen.getByText("Ananya Nair")).toBeInTheDocument());
    expect(screen.getByText("Ravi Kumar")).toBeInTheDocument();
  });

  it("shows Active/Suspended status badges", async () => {
    renderPage();

    await waitFor(() => expect(screen.getByText("Active")).toBeInTheDocument());
    expect(screen.getAllByText("Suspended").length).toBeGreaterThan(0);
  });

  it("shows the empty state when no users are registered", async () => {
    server.use(searchAdminUsersEmptyHandler);
    renderPage();

    await waitFor(() => expect(screen.getByText("No individual accounts registered yet.")).toBeInTheDocument());
  });

  it("shows an error state on request failure", async () => {
    server.use(searchAdminUsersErrorHandler);
    renderPage();

    await waitFor(() => expect(screen.getByText("The user list didn't load.")).toBeInTheDocument());
  });

  it("disables Suspend for an already-suspended account", async () => {
    renderPage();

    await waitFor(() => expect(screen.getByText("Ravi Kumar")).toBeInTheDocument());
    expect(screen.getByRole("button", { name: "Suspended" })).toBeDisabled();
  });

  it("opens the suspend modal, requires a reason, and suspends on confirm", async () => {
    renderPage();
    await waitFor(() => expect(screen.getByText("Ananya Nair")).toBeInTheDocument());

    fireEvent.click(screen.getByRole("button", { name: "Suspend" }));

    const confirmButton = await screen.findByRole("button", { name: /suspend account/i });
    expect(confirmButton).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/reason for suspension/i), {
      target: { value: "Repeated no-shows" },
    });
    expect(confirmButton).not.toBeDisabled();

    fireEvent.click(confirmButton);

    await waitFor(() => expect(screen.queryByRole("button", { name: /suspend account/i })).not.toBeInTheDocument());
  });

  it("shows an inline error when the suspend request fails", async () => {
    server.use(suspendUserErrorHandler);
    renderPage();
    await waitFor(() => expect(screen.getByText("Ananya Nair")).toBeInTheDocument());

    fireEvent.click(screen.getByRole("button", { name: "Suspend" }));
    fireEvent.change(await screen.findByLabelText(/reason for suspension/i), {
      target: { value: "Repeated no-shows" },
    });
    fireEvent.click(screen.getByRole("button", { name: /suspend account/i }));

    await waitFor(() => expect(screen.getByText(/couldn't suspend this account/i)).toBeInTheDocument());
  });
});
