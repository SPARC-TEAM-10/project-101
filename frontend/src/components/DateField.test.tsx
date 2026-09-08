import { render, screen, fireEvent } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { DateField } from "./DateField";

describe("DateField", () => {
  it("shows the placeholder when no value is set", () => {
    render(<DateField id="dob" value="" onChange={vi.fn()} placeholder="dd-mm-yyyy" />);

    expect(screen.getByText("dd-mm-yyyy")).toBeInTheDocument();
  });

  it("displays a set value formatted as dd-mm-yyyy", () => {
    render(<DateField id="dob" value="2000-01-15" onChange={vi.fn()} />);

    expect(screen.getByText("15-01-2000")).toBeInTheDocument();
  });

  it("opens the calendar on click and navigates to the requested month/year", () => {
    render(<DateField id="dob" value="" onChange={vi.fn()} />);

    fireEvent.click(screen.getByRole("button", { name: "dd-mm-yyyy" }));
    fireEvent.click(screen.getByLabelText(/^year$/i));
    fireEvent.click(screen.getByRole("option", { name: "2000" }));
    fireEvent.click(screen.getByLabelText(/^month$/i));
    fireEvent.click(screen.getByRole("option", { name: "January" }));

    expect(screen.getByRole("button", { name: "15" })).toBeInTheDocument();
  });

  it("selecting a day calls onChange with an ISO date and closes the calendar", () => {
    const onChange = vi.fn();
    render(<DateField id="dob" value="" onChange={onChange} />);

    fireEvent.click(screen.getByRole("button", { name: "dd-mm-yyyy" }));
    fireEvent.click(screen.getByLabelText(/^year$/i));
    fireEvent.click(screen.getByRole("option", { name: "2000" }));
    fireEvent.click(screen.getByLabelText(/^month$/i));
    fireEvent.click(screen.getByRole("option", { name: "January" }));
    fireEvent.click(screen.getByRole("button", { name: "15" }));

    expect(onChange).toHaveBeenCalledWith("2000-01-15");
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("disables days after the max date", () => {
    render(<DateField id="dob" value="" onChange={vi.fn()} max="2020-06-15" />);

    fireEvent.click(screen.getByRole("button", { name: "dd-mm-yyyy" }));
    fireEvent.click(screen.getByLabelText(/^year$/i));
    fireEvent.click(screen.getByRole("option", { name: "2020" }));
    fireEvent.click(screen.getByLabelText(/^month$/i));
    fireEvent.click(screen.getByRole("option", { name: "June" }));

    expect(screen.getByRole("button", { name: "16" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "15" })).toBeEnabled();
  });

  it("closes the calendar when clicking outside", () => {
    render(
      <div>
        <DateField id="dob" value="" onChange={vi.fn()} />
        <button type="button">Outside</button>
      </div>,
    );

    fireEvent.click(screen.getByRole("button", { name: "dd-mm-yyyy" }));
    expect(screen.getByRole("dialog")).toBeInTheDocument();

    fireEvent.mouseDown(screen.getByText("Outside"));

    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });
});
