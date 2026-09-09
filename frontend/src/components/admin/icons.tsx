// Inline icon set mirroring the design canvas's sprite defs (AdminQueueWeb/Mobile.dc.html) —
// stroke="currentColor" line icons at the same viewBox/weight, just as plain components instead
// of an SVG <use> sprite (no shared sprite sheet exists in this codebase yet).
interface IconProps {
  className?: string;
}

const BASE = { fill: "none", stroke: "currentColor", strokeWidth: 1.9, strokeLinecap: "round" as const, strokeLinejoin: "round" as const };

export function ShieldIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={20} height={20} className={className} {...BASE} aria-hidden="true">
      <path d="M12 3.5 19 6v6c0 4-3 6.7-7 8.5C8 18.7 5 16 5 12V6Z" />
      <path d="m9 12 2 2 4-4" />
    </svg>
  );
}

export function UsersIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={20} height={20} className={className} {...BASE} aria-hidden="true">
      <circle cx="9" cy="8.5" r="3.4" />
      <path d="M3.5 19.5c.6-3.2 2.9-5 5.5-5s4.9 1.8 5.5 5" />
      <path d="M16 5.6a3.4 3.4 0 0 1 0 5.9M17.5 14.9c2 .6 3.4 2.3 3.9 4.6" />
    </svg>
  );
}

export function BellIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <path d="M18 10a6 6 0 1 0-12 0c0 5-2 6.5-2 6.5h16S18 15 18 10Z" />
      <path d="M13.7 20a2 2 0 0 1-3.4 0" />
    </svg>
  );
}

export function FileIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <path d="M13.5 3.5H7a1.5 1.5 0 0 0-1.5 1.5v14A1.5 1.5 0 0 0 7 20.5h10a1.5 1.5 0 0 0 1.5-1.5V8.5Z" />
      <path d="M13.5 3.5v5h5" />
    </svg>
  );
}

export function FileWarnIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <path d="M13.5 3.5H7a1.5 1.5 0 0 0-1.5 1.5v14A1.5 1.5 0 0 0 7 20.5h10a1.5 1.5 0 0 0 1.5-1.5V8.5Z" />
      <path d="M13.5 3.5v5h5" />
      <path d="M12 11.5v3.2M12 17.4v.1" />
    </svg>
  );
}

export function CalendarIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <rect x="3.5" y="5" width="17" height="15.5" rx="2.5" />
      <path d="M3.5 10h17M8 3.5v3M16 3.5v3" />
    </svg>
  );
}

export function InboxIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={32} height={32} className={className} {...BASE} strokeWidth={1.6} aria-hidden="true">
      <path d="M3.5 13.5h4l1.5 3h6l1.5-3h4" />
      <path d="M5.6 5.2h12.8l2.1 8.3v4a1.5 1.5 0 0 1-1.5 1.5H5a1.5 1.5 0 0 1-1.5-1.5v-4Z" />
    </svg>
  );
}

export function CloudOffIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={32} height={32} className={className} {...BASE} strokeWidth={1.6} aria-hidden="true">
      <path d="M7 18.5h9.5a4 4 0 0 0 1.2-7.8A6 6 0 0 0 8 8.4" />
      <path d="M4 20 20 4" />
    </svg>
  );
}

export function RetryIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <path d="M20 12a8 8 0 1 1-2.6-5.9" />
      <path d="M20 4v4h-4" />
    </svg>
  );
}

export function ChevronLeftIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <path d="M14 6l-6 6 6 6" />
    </svg>
  );
}

export function ChevronRightIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <path d="M10 6l6 6-6 6" />
    </svg>
  );
}

export function CheckIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.4} aria-hidden="true">
      <path d="M5 12.5l4.5 4.5L19 7.5" />
    </svg>
  );
}

export function XIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <path d="M6 6l12 12M18 6 6 18" />
    </svg>
  );
}

export function SearchIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={16} height={16} className={className} {...BASE} strokeWidth={2} aria-hidden="true">
      <circle cx="10.5" cy="10.5" r="6.5" />
      <path d="m20 20-4.3-4.3" />
    </svg>
  );
}

export function BanIcon({ className }: IconProps) {
  return (
    <svg viewBox="0 0 24 24" width={14} height={14} className={className} {...BASE} strokeWidth={2.2} aria-hidden="true">
      <circle cx="12" cy="12" r="8.5" />
      <path d="m6.5 6.5 11 11" />
    </svg>
  );
}
