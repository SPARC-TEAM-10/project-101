import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { http, HttpResponse } from "msw";

import { NotificationsPage } from "./NotificationsPage";
import { ToastProvider } from "../../context/ToastProvider";
import { server } from "../../../tests/setup";
import { getMyNotificationsSuccessHandler, acceptNotificationNoLongerActiveHandler } from "../../../tests/msw/handlers";

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
        <MemoryRouter initialEntries={["/notifications"]}>
          <Routes>
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/dashboard/individual" element={<div>Dashboard</div>} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

describe("NotificationsPage", () => {
  it("renders the empty state when there are no notifications", async () => {
    renderPage();

    expect(await screen.findByText("Nothing to read yet")).toBeInTheDocument();
  });

  it("renders a notification with its blood group, urgency, distance, and Accept/Decline actions", async () => {
    server.use(getMyNotificationsSuccessHandler);
    renderPage();

    expect(await screen.findByText(/2 units needed/)).toBeInTheDocument();
    expect(screen.getByText("Emergency")).toBeInTheDocument();
    expect(screen.getByText(/Kaloor, Kochi/)).toBeInTheDocument();
    expect(screen.getByLabelText("Unread")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Accept" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Decline" })).toBeInTheDocument();
  });

  it("marks a notification read when clicked", async () => {
    server.use(getMyNotificationsSuccessHandler);
    server.use(
      http.patch("/api/v1/notifications/:id/read", () =>
        HttpResponse.json({
          id: "44444444-4444-4444-4444-444444444444",
          bloodRequestId: "11111111-1111-1111-1111-111111111111",
          bloodGroup: "O+",
          unitsRequired: 2,
          urgency: "Emergency",
          distanceKm: 4.2,
          areaLabel: "Kaloor, Kochi",
          isRead: true,
          createdAtUtc: new Date().toISOString(),
          responseStatus: "Pending",
        }),
      ),
    );
    renderPage();

    const row = await screen.findByText(/2 units needed/);
    fireEvent.click(row.closest("button")!);

    await waitFor(() => expect(screen.queryByLabelText("Unread")).not.toBeInTheDocument());
  });

  it("accepts a request and shows the requester's contact and location", async () => {
    server.use(getMyNotificationsSuccessHandler);
    renderPage();

    fireEvent.click(await screen.findByRole("button", { name: "Accept" }));

    expect(await screen.findByText("You're confirmed to help")).toBeInTheDocument();
    expect(screen.getByText("9123456789")).toBeInTheDocument();
    expect(screen.getByText(/Location: Kaloor, Kochi/)).toBeInTheDocument();
    expect(screen.getByText("Accepted")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Accept" })).not.toBeInTheDocument();
  });

  it("shows an error toast when the request is no longer active", async () => {
    server.use(getMyNotificationsSuccessHandler);
    server.use(acceptNotificationNoLongerActiveHandler);
    renderPage();

    fireEvent.click(await screen.findByRole("button", { name: "Accept" }));

    expect(await screen.findByText("This request is no longer active")).toBeInTheDocument();
    // Still Pending — the failed accept must not flip the UI to Accepted.
    expect(screen.getByRole("button", { name: "Accept" })).toBeInTheDocument();
  });

  it("declines a request and hides the Accept/Decline actions", async () => {
    server.use(getMyNotificationsSuccessHandler);
    renderPage();

    fireEvent.click(await screen.findByRole("button", { name: "Decline" }));

    expect(await screen.findByText("Declined")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Decline" })).not.toBeInTheDocument();
  });
});
