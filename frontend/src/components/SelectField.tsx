import { useEffect, useRef, useState, type KeyboardEvent } from "react";

export interface SelectFieldOption<T extends string> {
  value: T;
  label: string;
}

interface SelectFieldProps<T extends string> {
  id: string;
  value: T | "";
  onChange: (value: T) => void;
  options: SelectFieldOption<T>[];
  placeholder: string;
  invalid?: boolean;
  describedBy?: string;
  disabled?: boolean;
  /** For a standalone field with no paired <label for>, e.g. a compact picker inside a popover. */
  ariaLabel?: string;
}

// Custom-styled listbox — a native <select>'s open dropdown is OS-rendered chrome that can't be
// restyled with CSS in any browser, so it can't follow the app's clay/blood theme on hover. This
// component owns its own popup markup instead, styled the same way the rest of the app is.
export function SelectField<T extends string>({
  id,
  value,
  onChange,
  options,
  placeholder,
  invalid,
  describedBy,
  disabled,
  ariaLabel,
}: SelectFieldProps<T>) {
  const [open, setOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const rootRef = useRef<HTMLDivElement>(null);
  const listRef = useRef<HTMLUListElement>(null);

  const selected = options.find((o) => o.value === value);

  useEffect(() => {
    if (!open) return;
    listRef.current?.focus();

    function handleClickOutside(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [open]);

  function openList() {
    if (disabled) return;
    const currentIndex = options.findIndex((o) => o.value === value);
    setActiveIndex(currentIndex >= 0 ? currentIndex : 0);
    setOpen(true);
  }

  function selectOption(index: number) {
    const option = options[index];
    if (!option) return;
    onChange(option.value);
    setOpen(false);
  }

  function handleButtonKeyDown(e: KeyboardEvent<HTMLButtonElement>) {
    if (disabled) return;
    if (e.key === "ArrowDown" || e.key === "ArrowUp" || e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      openList();
    }
  }

  function handleListKeyDown(e: KeyboardEvent<HTMLUListElement>) {
    if (e.key === "ArrowDown") {
      e.preventDefault();
      setActiveIndex((i) => Math.min(options.length - 1, i + 1));
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setActiveIndex((i) => Math.max(0, i - 1));
    } else if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      selectOption(activeIndex);
    } else if (e.key === "Escape") {
      e.preventDefault();
      setOpen(false);
    } else if (e.key === "Tab") {
      setOpen(false);
    }
  }

  return (
    <div ref={rootRef} className="relative">
      <button
        type="button"
        id={id}
        disabled={disabled}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-invalid={invalid}
        aria-describedby={describedBy}
        aria-label={ariaLabel}
        onClick={() => (open ? setOpen(false) : openList())}
        onKeyDown={handleButtonKeyDown}
        className={`flex h-[50px] w-full items-center justify-between rounded-sm border-[1.5px] bg-cream px-4 text-base outline-none transition-colors focus:border-clay ${
          invalid ? "border-error" : "border-line-strong"
        } ${disabled ? "cursor-not-allowed text-ink-off" : ""}`}
      >
        <span className={selected ? "" : "text-ink-off"}>{selected ? selected.label : placeholder}</span>
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth={1.75}
          strokeLinecap="round"
          strokeLinejoin="round"
          aria-hidden="true"
          className={`flex-none text-ink-3 transition-transform ${open ? "rotate-180" : ""}`}
        >
          <path d="m6 9 6 6 6-6" />
        </svg>
      </button>

      {open && (
        <ul
          ref={listRef}
          role="listbox"
          tabIndex={-1}
          aria-label={placeholder}
          onKeyDown={handleListKeyDown}
          className="absolute z-10 mt-1.5 max-h-60 w-full overflow-auto rounded-sm border-[1.5px] border-line-strong bg-cream py-1 shadow-[var(--e1)] outline-none"
        >
          {options.map((option, index) => {
            const isSelected = option.value === value;
            const isActive = index === activeIndex;
            return (
              <li
                key={option.value}
                role="option"
                aria-selected={isSelected}
                onMouseEnter={() => setActiveIndex(index)}
                onClick={() => selectOption(index)}
                className={`cursor-pointer px-4 py-2.5 text-base transition-colors ${
                  isSelected || isActive ? "bg-clay-tint text-clay-deep" : "text-ink"
                } ${isSelected ? "font-semibold" : ""}`}
              >
                {option.label}
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
