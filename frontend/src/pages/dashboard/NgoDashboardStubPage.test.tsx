import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { NgoDashboardStubPage } from "./NgoDashboardStubPage";

describe("NgoDashboardStubPage", () => {
  it("renders without crashing", () => {
    render(<NgoDashboardStubPage />);

    expect(screen.getByText("NGO Dashboard")).toBeInTheDocument();
  });
});
