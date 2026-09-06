import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { GuestPlaceholderPage } from "./GuestPlaceholderPage";

describe("GuestPlaceholderPage", () => {
  it("renders a message and a link back to the home page", () => {
    render(
      <MemoryRouter initialEntries={["/guest"]}>
        <Routes>
          <Route path="/guest" element={<GuestPlaceholderPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText(/guest dashboard coming soon/i)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /back to home/i })).toHaveAttribute("href", "/");
  });
});
