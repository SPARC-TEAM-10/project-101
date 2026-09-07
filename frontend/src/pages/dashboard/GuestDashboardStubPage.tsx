// Placeholder for the real Guest Dashboard — a future ticket. Reachable only via
// RequireAuth roles={["Guest"]} (see router.tsx).
export function GuestDashboardStubPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-2 bg-sand px-7 font-sans text-ink">
      <h1 className="text-[26px] font-extrabold tracking-tight">Guest Dashboard</h1>
      <p className="text-center text-[14.5px] text-ink-2">Coming soon.</p>
    </div>
  );
}
