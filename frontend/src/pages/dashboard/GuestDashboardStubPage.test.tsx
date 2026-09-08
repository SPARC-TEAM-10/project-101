import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { GuestDashboardStubPage } from "./GuestDashboardStubPage";

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/dashboard/guest"]}>
      <Routes>
        <Route path="/dashboard/guest" element={<GuestDashboardStubPage />} />
        <Route path="/blood-requests/new" element={<div>Blood Request Form</div>} />
      </Routes>
    </MemoryRouter>,
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

  it("'Search Emergency Hub' is disabled and does not navigate", async () => {
    const user = userEvent.setup();
    renderPage();

    const searchButton = screen.getByRole("button", { name: /search emergency hub/i });
    expect(searchButton).toBeDisabled();

    await user.click(searchButton);

    expect(screen.getByText("Guest Access")).toBeInTheDocument();
    expect(screen.queryByText("Blood Request Form")).not.toBeInTheDocument();
  });
});
