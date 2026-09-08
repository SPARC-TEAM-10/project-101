import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { RoleSelectionPage } from "./RoleSelectionPage";

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/register"]}>
      <Routes>
        <Route path="/register" element={<RoleSelectionPage />} />
        <Route path="/register/individual" element={<div>Individual Registration</div>} />
        <Route path="/facility/register" element={<div>Facility Registration</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("RoleSelectionPage", () => {
  it("renders all three account type cards", () => {
    renderPage();

    expect(screen.getByRole("button", { name: /individual/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /hospital/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /ngo/i })).toBeInTheDocument();
  });

  it("'Individual' navigates to /register/individual", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /individual/i }));

    expect(await screen.findByText("Individual Registration")).toBeInTheDocument();
  });

  it("'Hospital' navigates to /facility/register", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /hospital/i }));

    expect(await screen.findByText("Facility Registration")).toBeInTheDocument();
  });

  it("'NGO' navigates to /facility/register", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /ngo/i }));

    expect(await screen.findByText("Facility Registration")).toBeInTheDocument();
  });
});
