import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { AdminDashboardStubPage } from "./AdminDashboardStubPage";

describe("AdminDashboardStubPage", () => {
  it("renders without crashing", () => {
    render(<AdminDashboardStubPage />);

    expect(screen.getByText("Admin Command Center")).toBeInTheDocument();
  });
});
