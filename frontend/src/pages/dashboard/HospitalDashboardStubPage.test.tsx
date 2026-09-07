import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { HospitalDashboardStubPage } from "./HospitalDashboardStubPage";

describe("HospitalDashboardStubPage", () => {
  it("renders without crashing", () => {
    render(<HospitalDashboardStubPage />);

    expect(screen.getByText("Hospital Dashboard")).toBeInTheDocument();
  });
});
