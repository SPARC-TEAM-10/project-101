import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { GuestDashboardStubPage } from "./GuestDashboardStubPage";

describe("GuestDashboardStubPage", () => {
  it("renders without crashing", () => {
    render(
      <MemoryRouter>
        <GuestDashboardStubPage />
      </MemoryRouter>,
    );

    expect(screen.getByText("Guest Dashboard")).toBeInTheDocument();
  });

  it("links to the individual registration page", () => {
    render(
      <MemoryRouter>
        <GuestDashboardStubPage />
      </MemoryRouter>,
    );

    expect(screen.getByRole("link", { name: /complete your profile/i })).toHaveAttribute(
      "href",
      "/register/individual",
    );
  });
});
