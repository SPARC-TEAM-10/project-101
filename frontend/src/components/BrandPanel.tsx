interface BrandPanelProps {
  heading: string;
  description: string;
  /** Sizing/positioning for the outer panel — differs per screen (e.g. sticky + fixed width vs a proportional grid column). */
  className?: string;
}

// Solid clay-deep brand panel — desktop-only left column used by screens whose design specifies
// this treatment (icon mark, heading, supporting copy, soft decorative circle) rather than a
// photo/illustration (that's AuthSplitLayout's job instead).
export function BrandPanel({ heading, description, className = "" }: BrandPanelProps) {
  return (
    <div className={`relative hidden flex-col overflow-hidden bg-clay-deep px-12 py-14 text-white md:flex ${className}`}>
      <div aria-hidden="true" className="pointer-events-none absolute -bottom-[70px] -right-[70px] h-[220px] w-[220px] rounded-full bg-white/5" />
      <div className="relative mb-8 flex h-10 w-10 items-center justify-center rounded-full bg-white/[.18]">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
          <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
        </svg>
      </div>
      <h2 className="relative mb-3.5 max-w-[13ch] text-[28px] font-extrabold leading-[1.22] tracking-tight">
        {heading}
      </h2>
      <p className="relative max-w-[28ch] text-sm leading-relaxed text-white/80">{description}</p>
    </div>
  );
}
