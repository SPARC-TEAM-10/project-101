import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { IndividualDashboardStubPage } from "./IndividualDashboardStubPage";

describe("IndividualDashboardStubPage", () => {
  it("renders without crashing", () => {
    render(<IndividualDashboardStubPage />);

    expect(screen.getByText("Individual Dashboard")).toBeInTheDocument();
  });
});
