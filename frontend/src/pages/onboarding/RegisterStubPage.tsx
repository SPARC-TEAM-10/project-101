// Placeholder for the real CHH-F02 individual registration form — a future ticket.
// The backend endpoint (POST /individuals) already exists; only the frontend form is pending.
// Reachable only via RequireAuth roles={["Guest"]} (see router.tsx).
export function RegisterStubPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-2 bg-sand px-7 font-sans text-ink">
      <h1 className="text-[26px] font-extrabold tracking-tight">Create Account</h1>
      <p className="text-center text-[14.5px] text-ink-2">Registration is coming soon.</p>
    </div>
  );
}
