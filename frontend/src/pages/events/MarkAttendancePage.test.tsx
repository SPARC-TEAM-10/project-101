import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

import { MarkAttendancePage } from "./MarkAttendancePage";
import { server } from "../../../tests/setup";
import {
  EVENT_DETAIL_ID,
  searchParticipantsSuccessHandler,
  searchParticipantsEmptyHandler,
  markAttendedSuccessHandler,
  markAttendedAlreadyAttendedHandler,
} from "../../../tests/msw/handlers";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return { ...actual, useAuth: () => mockUseAuth() };
});

function renderPage() {
  mockUseAuth.mockReturnValue({
    session: { token: "fake-jwt", role: "Hospital", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() },
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[`/events/${EVENT_DETAIL_ID}/attendance`]}>
        <Routes>
          <Route path="/events/:id/attendance" element={<MarkAttendancePage />} />
          <Route path="/events/:id/manage" element={<div>Manage page</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("MarkAttendancePage", () => {
  it("shows the nothing-searched-yet state before a valid search", () => {
    renderPage();

    expect(screen.getByText("Nothing searched yet")).toBeInTheDocument();
  });

  it("shows the too-short error for a 1-2 character search", () => {
    renderPage();

    fireEvent.change(screen.getByLabelText(/search rsvp'd participants/i), { target: { value: "ni" } });

    expect(screen.getByText(/enter at least 3 characters/i)).toBeInTheDocument();
  });

  it("shows matching participants for a valid search, including an already-attended one", async () => {
    server.use(searchParticipantsSuccessHandler);
    renderPage();

    fireEvent.change(screen.getByLabelText(/search rsvp'd participants/i), { target: { value: "nith" } });

    expect(await screen.findByText("Nithya Menon")).toBeInTheDocument();
    expect(screen.getByText("Nithya Rajan")).toBeInTheDocument();
    expect(screen.getByText("Attended")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Mark attended" })).toBeInTheDocument();
  });

  it("shows an empty match state when the search finds nobody", async () => {
    server.use(searchParticipantsEmptyHandler);
    renderPage();

    fireEvent.change(screen.getByLabelText(/search rsvp'd participants/i), { target: { value: "nobody" } });

    expect(await screen.findByText(/0 rsvp'd participants match/i)).toBeInTheDocument();
  });

  it("marks a participant attended and shows the just-marked confirmation", async () => {
    server.use(searchParticipantsSuccessHandler, markAttendedSuccessHandler);
    renderPage();

    fireEvent.change(screen.getByLabelText(/search rsvp'd participants/i), { target: { value: "nith" } });
    await screen.findByText("Nithya Menon");
    fireEvent.click(screen.getByRole("button", { name: "Mark attended" }));

    expect(await screen.findByText("Nithya Menon marked attended")).toBeInTheDocument();
  });

  it("surfaces a 409 error when the participant is already attended", async () => {
    server.use(searchParticipantsSuccessHandler, markAttendedAlreadyAttendedHandler);
    renderPage();

    fireEvent.change(screen.getByLabelText(/search rsvp'd participants/i), { target: { value: "nith" } });
    await screen.findByText("Nithya Menon");
    fireEvent.click(screen.getByRole("button", { name: "Mark attended" }));

    await waitFor(() => expect(screen.getByText("This participant has already been marked attended.")).toBeInTheDocument());
  });
});
