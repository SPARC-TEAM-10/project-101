import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { NewUserGuestDecisionPage } from "./NewUserGuestDecisionPage";

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/welcome"]}>
      <Routes>
        <Route path="/welcome" element={<NewUserGuestDecisionPage />} />
        <Route path="/register" element={<div>Register Screen</div>} />
        <Route path="/dashboard/guest" element={<div>Guest Dashboard Screen</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("NewUserGuestDecisionPage", () => {
  it("renders the Welcome heading and both cards", () => {
    renderPage();

    expect(screen.getByText("Welcome")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /create account/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /emergency guest access/i })).toBeInTheDocument();
  });

  it('"Create Account" navigates to /register', async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /create account/i }));

    expect(screen.getByText("Register Screen")).toBeInTheDocument();
  });

  it('"Continue as Guest" navigates to /dashboard/guest', async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(screen.getByRole("button", { name: /emergency guest access/i }));

    expect(screen.getByText("Guest Dashboard Screen")).toBeInTheDocument();
  });
});
