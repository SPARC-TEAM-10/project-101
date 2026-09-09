import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { GuestDashboardStubPage } from "./GuestDashboardStubPage";
import { AuthProvider } from "../../context/AuthProvider";

function renderPage() {
  localStorage.setItem(
    "chh.auth.session",
    JSON.stringify({ token: "fake-jwt", role: "Guest", expiresAtUtc: new Date(Date.now() + 60_000).toISOString() }),
  );

  return render(
    <AuthProvider>
      <MemoryRouter initialEntries={["/dashboard/guest"]}>
        <Routes>
          <Route path="/dashboard/guest" element={<GuestDashboardStubPage />} />
          <Route path="/blood-requests/new" element={<div>Blood Request Form</div>} />
          <Route path="/emergency" element={<div>Emergency Services Hub</div>} />
          <Route path="/" element={<div>Landing Page</div>} />
          <Route path="/welcome" element={<div>Welcome Page</div>} />
        </Routes>
      </MemoryRouter>
    </AuthProvider>,
  );
}

describe("GuestDashboardStubPage", () => {
  it("renders both entry points", () => {
    renderPage();

    expect(screen.getByRole("button", { name: /request blood/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /search emergency hub/i })).toBeInTheDocument();
  });

  it("navigates to /blood-requests/new when 'Request Blood' is clicked", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /request blood/i }));

    expect(await screen.findByText("Blood Request Form")).toBeInTheDocument();
  });

  it("navigates to /emergency when 'Search Emergency Hub' is clicked", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /search emergency hub/i }));

    expect(await screen.findByText("Emergency Services Hub")).toBeInTheDocument();
  });

  it("logs out and lands back on the landing page", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /log out/i }));

    expect(await screen.findByText("Landing Page")).toBeInTheDocument();
    expect(localStorage.getItem("chh.auth.session")).toBeNull();
  });

  it("navigates back to /welcome when the back button is clicked", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /back/i }));

    expect(await screen.findByText("Welcome Page")).toBeInTheDocument();
  });

  it("shows the session's remaining time", () => {
    localStorage.setItem(
      "chh.auth.session",
      JSON.stringify({
        token: "fake-jwt",
        role: "Guest",
        expiresAtUtc: new Date(Date.now() + (23 * 60 + 45) * 60_000).toISOString(),
      }),
    );

    render(
      <AuthProvider>
        <MemoryRouter initialEntries={["/dashboard/guest"]}>
          <Routes>
            <Route path="/dashboard/guest" element={<GuestDashboardStubPage />} />
          </Routes>
        </MemoryRouter>
      </AuthProvider>,
    );

    expect(screen.getAllByText(/Session expires in 23h 4[45]m/)[0]).toBeInTheDocument();
  });
});
