import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";

type ToastKind = "ok" | "err" | "info";

interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

interface ToastContextValue {
  success: (message: string, autoDismissMs?: number) => void;
  error: (message: string, autoDismissMs?: number | null) => void;
  info: (message: string, autoDismissMs?: number) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

const DEFAULT_AUTO_DISMISS_MS = 4000;

function ToastIcon({ kind }: { kind: ToastKind }) {
  if (kind === "ok") {
    return (
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <circle cx="12" cy="12" r="8.5" />
        <path d="M8.4 12.3 11 15l4.6-5.4" />
      </svg>
    );
  }
  if (kind === "err") {
    return (
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M12 4.5 21 19.5H3L12 4.5Z" />
        <path d="M12 10v4M12 16.8v.1" />
      </svg>
    );
  }
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <circle cx="12" cy="12" r="8.5" />
      <path d="M12 11v5.5M12 7.9v.1" />
    </svg>
  );
}

// Toast host — from the design canvas's FeedbackComponents artboard, page 2
// (https://claude.ai/code/artifact/6b185d14-a32d-4647-a2b7-7366b07c2b75). Top-center, fixed
// position, styled via index.css's .toast-host/.toast rules.
export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const dismiss = useCallback((id: number) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const push = useCallback(
    (kind: ToastKind, message: string, autoDismissMs?: number | null) => {
      const id = Date.now() + Math.random();
      setToasts((prev) => [...prev, { id, kind, message }]);
      if (autoDismissMs !== null) {
        setTimeout(() => dismiss(id), autoDismissMs ?? DEFAULT_AUTO_DISMISS_MS);
      }
    },
    [dismiss],
  );

  const value = useMemo<ToastContextValue>(
    () => ({
      success: (message, autoDismissMs) => push("ok", message, autoDismissMs),
      error: (message, autoDismissMs) => push("err", message, autoDismissMs),
      info: (message, autoDismissMs) => push("info", message, autoDismissMs),
    }),
    [push],
  );

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toast-host">
        {toasts.map((t) => (
          <div key={t.id} className={`toast ${t.kind}`}>
            <ToastIcon kind={t.kind} />
            <span className="msg">{t.message}</span>
            <button type="button" className="x" onClick={() => dismiss(t.id)} aria-label="Dismiss">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="m6.5 6.5 11 11M17.5 6.5l-11 11" />
              </svg>
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextValue {
  const ctx = useContext(ToastContext);
  if (!ctx) {
    throw new Error("useToast must be used within a ToastProvider");
  }
  return ctx;
}
