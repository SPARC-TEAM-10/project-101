import { useEffect, useRef, useState } from "react";

import { SelectField } from "./SelectField";

interface DateFieldProps {
  id: string;
  /** ISO "YYYY-MM-DD", or "" when unset. */
  value: string;
  /** Emits an ISO "YYYY-MM-DD" string, matching a native <input type="date">'s onChange value. */
  onChange: (value: string) => void;
  /** ISO "YYYY-MM-DD" — days after this are shown disabled. */
  max?: string;
  /** ISO "YYYY-MM-DD" — days before this are shown disabled. */
  min?: string;
  placeholder?: string;
  invalid?: boolean;
  describedBy?: string;
  disabled?: boolean;
}

const MONTH_LABELS = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];
const WEEKDAY_LABELS = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];
const YEARS_BACK = 120;
const GRID_CELLS = 42; // 6 weeks x 7 days — fixed size so the grid doesn't reflow month to month

function pad(n: number): string {
  return String(n).padStart(2, "0");
}

function toISODate(date: Date): string {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

function parseISODate(value: string | undefined): Date | null {
  if (!value) return null;
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return null;
  const date = new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
  return Number.isNaN(date.getTime()) ? null : date;
}

function formatDisplay(value: string): string {
  const date = parseISODate(value);
  return date ? `${pad(date.getDate())}-${pad(date.getMonth() + 1)}-${date.getFullYear()}` : "";
}

function getCalendarDays(viewYear: number, viewMonth: number): Date[] {
  const firstOfMonth = new Date(viewYear, viewMonth, 1);
  const start = new Date(viewYear, viewMonth, 1 - firstOfMonth.getDay());
  return Array.from({ length: GRID_CELLS }, (_, i) => new Date(start.getFullYear(), start.getMonth(), start.getDate() + i));
}

// Custom-styled calendar — a native <input type="date">'s popup is OS-rendered chrome that can't
// be restyled with CSS in any browser (same limitation as a native <select>, see SelectField),
// so its hover/selection states can't follow the app's clay/blood theme. This component owns its
// own popup markup instead.
export function DateField({ id, value, onChange, max, min, placeholder = "dd-mm-yyyy", invalid, describedBy, disabled }: DateFieldProps) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const today = new Date();
  const anchor = parseISODate(value) ?? parseISODate(max) ?? today;
  const [viewMonth, setViewMonth] = useState(anchor.getMonth());
  const [viewYear, setViewYear] = useState(anchor.getFullYear());

  useEffect(() => {
    if (!open) return;
    const resync = parseISODate(value) ?? parseISODate(max) ?? new Date();
    setViewMonth(resync.getMonth());
    setViewYear(resync.getFullYear());

    function handleClickOutside(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  function goToPreviousMonth() {
    const d = new Date(viewYear, viewMonth - 1, 1);
    setViewMonth(d.getMonth());
    setViewYear(d.getFullYear());
  }

  function goToNextMonth() {
    const d = new Date(viewYear, viewMonth + 1, 1);
    setViewMonth(d.getMonth());
    setViewYear(d.getFullYear());
  }

  function selectDate(date: Date) {
    onChange(toISODate(date));
    setOpen(false);
  }

  const todayIso = toISODate(today);
  const currentYear = today.getFullYear();
  const monthOptions = MONTH_LABELS.map((label, index) => ({ value: String(index), label }));
  const yearOptions = Array.from({ length: YEARS_BACK + 1 }, (_, i) => {
    const year = currentYear - i;
    return { value: String(year), label: String(year) };
  });

  return (
    <div ref={rootRef} className="relative">
      <button
        type="button"
        id={id}
        disabled={disabled}
        aria-haspopup="dialog"
        aria-expanded={open}
        aria-invalid={invalid}
        aria-describedby={describedBy}
        onClick={() => setOpen((o) => !o)}
        className={`flex h-[50px] w-full items-center justify-between rounded-sm border-[1.5px] bg-cream px-4 text-base outline-none transition-colors focus:border-clay ${
          invalid ? "border-error" : "border-line-strong"
        } ${disabled ? "cursor-not-allowed text-ink-off" : ""}`}
      >
        <span className={value ? "" : "text-ink-off"}>{value ? formatDisplay(value) : placeholder}</span>
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="flex-none text-ink-3">
          <rect x="3.5" y="5" width="17" height="15" rx="2" />
          <path d="M3.5 9.5h17" />
          <path d="M8 3v4M16 3v4" />
        </svg>
      </button>

      {open && (
        <div role="dialog" aria-label="Choose a date" className="absolute z-10 mt-1.5 w-[300px] rounded-sm border-[1.5px] border-line-strong bg-cream p-3 shadow-[var(--e1)]">
          <div className="mb-2.5 flex items-center gap-1.5">
            <button
              type="button"
              aria-label="Previous month"
              onClick={goToPreviousMonth}
              className="flex h-8 w-8 flex-none items-center justify-center rounded-sm text-ink-2 transition-colors hover:bg-sand-2"
            >
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="m15 18-6-6 6-6" />
              </svg>
            </button>
            <div className="flex flex-1 gap-1.5">
              <div className="flex-1">
                <SelectField
                  id={`${id}-month`}
                  ariaLabel="Month"
                  value={String(viewMonth)}
                  onChange={(v) => setViewMonth(Number(v))}
                  options={monthOptions}
                  placeholder="Month"
                />
              </div>
              <div className="w-[92px] flex-none">
                <SelectField
                  id={`${id}-year`}
                  ariaLabel="Year"
                  value={String(viewYear)}
                  onChange={(v) => setViewYear(Number(v))}
                  options={yearOptions}
                  placeholder="Year"
                />
              </div>
            </div>
            <button
              type="button"
              aria-label="Next month"
              onClick={goToNextMonth}
              className="flex h-8 w-8 flex-none items-center justify-center rounded-sm text-ink-2 transition-colors hover:bg-sand-2"
            >
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="m9 6 6 6-6 6" />
              </svg>
            </button>
          </div>

          <div className="grid grid-cols-7 gap-y-1 text-center text-[11px] font-semibold text-ink-3">
            {WEEKDAY_LABELS.map((label) => (
              <span key={label} className="flex h-6 items-center justify-center">
                {label}
              </span>
            ))}
          </div>
          <div className="grid grid-cols-7 gap-y-1">
            {getCalendarDays(viewYear, viewMonth).map((date) => {
              const iso = toISODate(date);
              const inMonth = date.getMonth() === viewMonth;
              const isSelected = iso === value;
              const isToday = iso === todayIso;
              const isDisabled = (!!max && iso > max) || (!!min && iso < min);
              return (
                <div key={iso} className="flex items-center justify-center">
                  <button
                    type="button"
                    disabled={isDisabled}
                    onClick={() => selectDate(date)}
                    className={`flex h-8 w-8 items-center justify-center rounded-full text-[13px] transition-colors ${
                      isSelected
                        ? "bg-clay font-semibold text-white"
                        : isDisabled
                          ? "cursor-not-allowed text-ink-off"
                          : `${inMonth ? "text-ink" : "text-ink-off"} hover:bg-clay-tint hover:text-clay-deep`
                    } ${isToday && !isSelected ? "ring-1 ring-inset ring-clay" : ""}`}
                  >
                    {date.getDate()}
                  </button>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
