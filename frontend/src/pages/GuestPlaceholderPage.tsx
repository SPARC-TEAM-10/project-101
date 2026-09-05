import { Link } from "react-router-dom";

// Stub — no Jira ticket exists yet for the Guest Dashboard.
export function GuestPlaceholderPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-sand px-7 font-sans text-ink">
      <h1 className="text-[26px] font-extrabold tracking-tight">Guest Dashboard coming soon</h1>
      <p className="max-w-[34ch] text-center text-[14.5px] text-ink-2">
        Emergency guest access isn&apos;t built yet. Check back soon, or go back to the home page.
      </p>
      <Link to="/" className="text-clay hover:text-clay-hover">
        Back to home
      </Link>
    </div>
  );
}
