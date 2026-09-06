import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";

import { LandingPage } from "./LandingPage";

// jsdom has no rAF timing guarantees, so the count-up test forces
// prefers-reduced-motion (the hook's own reduced-motion path sets the
// final value synchronously) rather than asserting mid-animation frames.
// Uses vi.spyOn (not a raw reassignment) so vi.restoreAllMocks() in
// afterEach correctly restores tests/setup.ts's matchMedia polyfill for
// every other test in this file.
function mockReducedMotion(matches: boolean) {
  vi.spyOn(window, "matchMedia").mockReturnValue({
    matches,
    media: "",
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  } as MediaQueryList);
}

function renderLanding() {
  return render(
    <MemoryRouter initialEntries={["/"]}>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<div>Mobile Entry Screen</div>} />
        <Route path="/guest" element={<div>Guest Screen</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("LandingPage", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("navigates to /login when the nav Log in button is clicked", async () => {
    const user = userEvent.setup();
    renderLanding();

    await user.click(screen.getAllByRole("button", { name: /log in/i })[0]);

    expect(await screen.findByText("Mobile Entry Screen")).toBeInTheDocument();
  });

  it("navigates to /login when the hero secondary Log in button is clicked", async () => {
    const user = userEvent.setup();
    renderLanding();

    const loginButtons = screen.getAllByRole("button", { name: /log in/i });
    await user.click(loginButtons[loginButtons.length - 1]);

    expect(await screen.findByText("Mobile Entry Screen")).toBeInTheDocument();
  });

  it("navigates to /guest when Continue as Guest is clicked", async () => {
    const user = userEvent.setup();
    renderLanding();

    await user.click(screen.getAllByRole("button", { name: /continue as guest/i })[0]);

    expect(await screen.findByText("Guest Screen")).toBeInTheDocument();
  });

  it("renders the final stat counter values", () => {
    mockReducedMotion(true);
    renderLanding();

    expect(screen.getAllByText("2,300+").length).toBeGreaterThan(0);
    expect(screen.getAllByText("340+").length).toBeGreaterThan(0);
  });

  it("renders keyboard-reachable slideshow dots and switches the active slide on click", async () => {
    const user = userEvent.setup();
    renderLanding();

    const dots = screen.getAllByRole("button", { name: /slide \d/i });
    expect(dots).toHaveLength(4);

    await user.click(dots[1]);

    expect(dots[1]).toHaveAttribute("aria-current", "true");
    expect(dots[0]).toHaveAttribute("aria-current", "false");
  });

  it("hides inactive slides from assistive technology", () => {
    renderLanding();

    const hiddenSlides = document.querySelectorAll('[aria-hidden="true"]');
    expect(hiddenSlides.length).toBeGreaterThan(0);
  });
});
