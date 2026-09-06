import { useEffect, useRef } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";

import { AuthSplitLayout } from "../../components/AuthSplitLayout";
import { useOtpVerify } from "../../features/auth/useOtpVerify";
import { useAuth } from "../../context/AuthProvider";

interface OtpVerificationState {
  mobileNumber: string;
  maskedMobileNumber: string;
  otpExpiresAtUtc: string;
  resendAvailableAtUtc: string;
}

function isOtpVerificationState(state: unknown): state is OtpVerificationState {
  return typeof state === "object" && state !== null && "maskedMobileNumber" in state;
}

function formatTimer(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${m}:${s.toString().padStart(2, "0")}`;
}

export function OtpVerificationPage() {
  const location = useLocation();
  const navigate = useNavigate();

  if (!isOtpVerificationState(location.state)) {
    return <Navigate to="/login" replace />;
  }

  return <OtpVerificationScreen state={location.state} onNavigate={navigate} />;
}

function OtpVerificationScreen({
  state,
  onNavigate,
}: {
  state: OtpVerificationState;
  onNavigate: ReturnType<typeof useNavigate>;
}) {
  const { mobileNumber, maskedMobileNumber, resendAvailableAtUtc } = state;
  const {
    digits,
    setDigit,
    error,
    isPending,
    resendSecondsLeft,
    canResend,
    resendError,
    isResending,
    resend,
    submit,
  } = useOtpVerify(mobileNumber, resendAvailableAtUtc);
  const { setSession } = useAuth();
  const inputRefs = useRef<Array<HTMLInputElement | null>>([]);

  useEffect(() => {
    if (error) {
      inputRefs.current[0]?.focus();
    }
  }, [error]);

  async function verifyAndNavigate() {
    const result = await submit();
    if (result.ok && result.data) {
      setSession({
        token: result.data.accessToken,
        role: result.data.role,
        expiresAtUtc: result.data.tokenExpiresAtUtc,
      });
      onNavigate("/redirecting");
    }
  }

  // AC1: auto-submit as soon as all six digits are entered.
  useEffect(() => {
    if (digits.join("").length === 6 && !isPending && !error) {
      verifyAndNavigate();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [digits]);

  const isComplete = digits.every(Boolean);

  function handleChange(index: number, rawValue: string) {
    const value = rawValue.replace(/[^0-9]/g, "").slice(-1);
    setDigit(index, value);
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  }

  function handleKeyDown(index: number, e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Backspace" && !digits[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  }

  function handlePaste(e: React.ClipboardEvent<HTMLInputElement>) {
    const pasted = e.clipboardData.getData("text").replace(/[^0-9]/g, "").slice(0, 6);
    if (!pasted) return;
    e.preventDefault();
    pasted.split("").forEach((char, i) => setDigit(i, char));
    inputRefs.current[Math.min(pasted.length, 5)]?.focus();
  }

  const otpStateClass = error ? "border-error" : isComplete ? "border-clay" : "border-line-strong";

  return (
    <AuthSplitLayout imageSrc="/images/auth-otp-verify.png" imageAlt="">
    <div className="flex min-h-screen flex-col bg-sand font-sans text-ink">
      <div className="flex h-[58px] flex-none items-center gap-3 border-b border-line bg-cream px-4">
        <a
          href="/login"
          onClick={(e) => {
            e.preventDefault();
            onNavigate("/login");
          }}
          className="inline-flex items-center gap-1 text-sm font-semibold text-ink-2 underline underline-offset-2 hover:text-ink"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
          Back
        </a>
        <b className="mr-9 flex-1 text-center text-[15px] font-bold">Verify OTP</b>
      </div>

      <div className="flex flex-col items-center px-7 pt-11">
        <h1 className="mb-2 text-center text-[22px] font-extrabold tracking-tight">Enter the code</h1>
        <p className="mb-7 text-center text-sm text-ink-2">
          Sent to <b className="text-ink [font-variant-numeric:tabular-nums]">{maskedMobileNumber}</b>
        </p>

        <div className="mb-4 flex gap-2">
          {digits.map((digit, i) => (
            <input
              key={i}
              ref={(el) => (inputRefs.current[i] = el)}
              aria-label={`Digit ${i + 1}`}
              maxLength={1}
              inputMode="numeric"
              autoComplete={i === 0 ? "one-time-code" : "off"}
              value={digit}
              disabled={isPending}
              onChange={(e) => handleChange(i, e.target.value)}
              onKeyDown={(e) => handleKeyDown(i, e)}
              onPaste={handlePaste}
              className={`h-11 w-9 rounded-sm border-[1.5px] bg-cream text-center text-lg font-bold [font-variant-numeric:tabular-nums] ${otpStateClass}`}
            />
          ))}
        </div>

        <div className="mb-4 flex min-h-[18px] items-center gap-1.5 text-[13px] text-error" role="alert">
          {error && (
            <>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M12 4.5 21 19.5H3L12 4.5Z" />
                <path d="M12 10v4M12 16.8v.1" />
              </svg>
              {error}
            </>
          )}
        </div>

        <button
          type="button"
          disabled={!isComplete || isPending}
          onClick={verifyAndNavigate}
          className={`mb-5 h-11 w-full max-w-[260px] rounded-md text-sm font-semibold transition-colors ${
            isComplete && !isPending
              ? "bg-clay text-white hover:bg-clay-hover"
              : "cursor-not-allowed bg-sand-2 text-ink-off"
          }`}
        >
          {isPending ? "Verifying…" : "Verify OTP"}
        </button>

        {canResend ? (
          <button
            type="button"
            disabled={isResending}
            onClick={() => resend()}
            className="mb-2 border-none bg-transparent p-0 text-sm font-semibold text-clay disabled:text-ink-off"
          >
            {isResending ? "Resending…" : "Resend OTP"}
          </button>
        ) : (
          <div className="mb-2 text-sm text-ink-2">
            Resend in <b className="text-ink [font-variant-numeric:tabular-nums]">{formatTimer(resendSecondsLeft)}</b>
          </div>
        )}

        <div className="mb-4 min-h-[16px] text-[13px] text-error" role="alert">
          {resendError}
        </div>

        <button type="button" onClick={() => onNavigate("/login")} className="text-[13.5px] text-ink-2 underline">
          Change Number
        </button>
      </div>
    </div>
    </AuthSplitLayout>
  );
}
