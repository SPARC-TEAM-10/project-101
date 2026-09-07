import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { GuestDashboardStubPage } from "./GuestDashboardStubPage";

describe("GuestDashboardStubPage", () => {
  it("renders without crashing", () => {
    render(<GuestDashboardStubPage />);

    expect(screen.getByText("Guest Dashboard")).toBeInTheDocument();
  });
});
