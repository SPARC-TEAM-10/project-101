import { DripLoader } from "./DripLoader";

// Full-screen overlay loader (blood-drip, design canvas FeedbackComponents artboard) — used
// wherever a submit/mutation needs to block the whole screen rather than just its own button.
export function LoadingOverlay({ message }: { message: string }) {
  return (
    <div className="fixed inset-0 z-[60] flex flex-col items-center justify-center gap-4 bg-[var(--overlay)] font-sans">
      <DripLoader size="xl" />
      <p className="text-base font-semibold text-white">{message}</p>
    </div>
  );
}
