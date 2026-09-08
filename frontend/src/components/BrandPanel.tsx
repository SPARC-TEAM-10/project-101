import type { ReactNode } from "react";

export interface BrandPanelFeature {
  icon: ReactNode;
  label: string;
}

interface BrandPanelProps {
  heading: string;
  description: string;
  /** Defaults to the droplet mark used by most screens — override for a screen-specific icon (e.g. New User/Guest's two-person icon). */
  icon?: ReactNode;
  /** Optional bullet list below the description (Mobile Entry / OTP Verify's "why sign in" pitch). */
  features?: BrandPanelFeature[];
  /** Sizing/positioning for the outer panel — differs per screen (e.g. sticky + fixed width vs a proportional grid column). */
  className?: string;
}

const DROPLET_ICON = (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
    <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
  </svg>
);

// Solid clay-deep brand panel — desktop-only left column used across the auth/onboarding flow
// (icon mark, heading, supporting copy, optional feature bullets, soft decorative circle) instead
// of a photo/illustration. Content is vertically centered so it doesn't read as stranded near the
// top on screens whose right-column content is short relative to the viewport.
export function BrandPanel({ heading, description, icon = DROPLET_ICON, features, className = "" }: BrandPanelProps) {
  return (
    <div className={`relative hidden flex-col justify-center overflow-hidden bg-clay-deep px-12 py-14 text-white md:flex ${className}`}>
      <div aria-hidden="true" className="pointer-events-none absolute -bottom-[70px] -right-[70px] h-[220px] w-[220px] rounded-full bg-white/5" />
      <div className="relative mb-8 flex h-10 w-10 items-center justify-center rounded-full bg-white/[.18]">
        {icon}
      </div>
      <h2 className="relative mb-3.5 max-w-[13ch] text-[28px] font-extrabold leading-[1.22] tracking-tight">
        {heading}
      </h2>
      <p className="relative max-w-[28ch] text-sm leading-relaxed text-white/80">{description}</p>
      {features && features.length > 0 && (
        <ul className="relative mt-7 flex flex-col gap-4">
          {features.map((feature) => (
            <li key={feature.label} className="flex items-center gap-3 text-sm text-white/90">
              <span className="flex-none">{feature.icon}</span>
              {feature.label}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
