import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";

import { NotificationsPage } from "./NotificationsPage";

describe("NotificationsPage", () => {
  it("renders the empty state and a link back to the dashboard", () => {
    render(
      <MemoryRouter initialEntries={["/notifications"]}>
        <Routes>
          <Route path="/notifications" element={<NotificationsPage />} />
          <Route path="/dashboard/individual" element={<div>Dashboard</div>} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText("Nothing to read yet")).toBeInTheDocument();
    expect(screen.getByText(/CHH-34/)).toBeInTheDocument();
  });
});
