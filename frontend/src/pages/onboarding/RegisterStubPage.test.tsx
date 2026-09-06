import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { RegisterStubPage } from "./RegisterStubPage";

describe("RegisterStubPage", () => {
  it("renders without crashing", () => {
    render(<RegisterStubPage />);

    expect(screen.getByText("Create Account")).toBeInTheDocument();
  });
});
