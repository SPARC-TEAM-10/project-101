import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

interface Slide {
  key: string;
  tagClass: string;
  tag: string;
  title: string;
  body: string;
}

const SLIDES: Slide[] = [
  {
    key: "blood",
    tagClass: "text-blood-deep",
    tag: "Blood donation",
    title: "One donation can save up to three lives.",
    body: "Post or answer a request by blood group and distance — donors are notified in seconds, not phone calls.",
  },
  {
    key: "amber",
    tagClass: "text-amber",
    tag: "Emergency response",
    title: "When minutes matter, help is one tap away.",
    body: "Emergency requests reach nearby verified donors and hospitals first — no forty phone calls at 2 a.m.",
  },
  {
    key: "leaf",
    tagClass: "text-leaf",
    tag: "Everyday wellness",
    title: "Small daily care adds up to a healthier you.",
    body: "Medicine and check-up reminders that repeat what you entered — never medical advice, never a changed dose.",
  },
  {
    key: "clay",
    tagClass: "text-clay-deep",
    tag: "Trusted network",
    title: "2,300+ donors and 340+ facilities, verified.",
    body: "Hospitals and NGOs publish real inventory, so a request only reaches people and places that can help.",
  },
];

const STATS = [
  { value: 2300, suffix: "+", label: "Donors" },
  { value: 340, suffix: "+", label: "Facilities" },
];

function usePrefersReducedMotion() {
  const [reduced, setReduced] = useState(false);
  useEffect(() => {
    const mq = window.matchMedia("(prefers-reduced-motion: reduce)");
    setReduced(mq.matches);
    const onChange = () => setReduced(mq.matches);
    mq.addEventListener("change", onChange);
    return () => mq.removeEventListener("change", onChange);
  }, []);
  return reduced;
}

function useCountUp(target: number, reducedMotion: boolean) {
  const [value, setValue] = useState(reducedMotion ? target : 0);
  useEffect(() => {
    if (reducedMotion) {
      setValue(target);
      return;
    }
    let raf: number;
    const start = performance.now();
    const duration = 900;
    function tick(now: number) {
      const p = Math.min(1, (now - start) / duration);
      const eased = 1 - Math.pow(1 - p, 3);
      setValue(Math.round(target * eased));
      if (p < 1) raf = requestAnimationFrame(tick);
    }
    raf = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(raf);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [target, reducedMotion]);
  return value;
}

function StatCounter({ value, suffix, label }: { value: number; suffix: string; label: string }) {
  const reducedMotion = usePrefersReducedMotion();
  const count = useCountUp(value, reducedMotion);
  return (
    <div className="flex flex-col items-center gap-0.5">
      <b className="text-xl font-extrabold tracking-tight text-ink [font-variant-numeric:tabular-nums]">
        {count.toLocaleString()}
        {suffix}
      </b>
      <span className="text-[10.5px] font-semibold text-ink-3">{label}</span>
    </div>
  );
}

export function LandingPage() {
  const navigate = useNavigate();
  const reducedMotion = usePrefersReducedMotion();
  const [activeSlide, setActiveSlide] = useState(0);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    if (reducedMotion) return;
    timerRef.current = setInterval(() => {
      setActiveSlide((i) => (i + 1) % SLIDES.length);
    }, 4600);
    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, [reducedMotion, activeSlide]);

  function selectSlide(i: number) {
    setActiveSlide(i);
  }

  return (
    <div className="landing-page min-h-screen bg-sand font-sans text-ink">
      <header className="sticky top-0 z-40 border-b border-line bg-cream/90 backdrop-blur">
        <div className="mx-auto flex max-w-[1180px] items-center justify-between gap-4 px-6 py-3.5">
          <span className="flex items-center gap-2.5 text-[16.5px] font-extrabold tracking-tight text-ink">
            <span className="flex h-8 w-8 items-center justify-center rounded-full bg-blood text-white">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
              </svg>
            </span>
            Community Health Hub
          </span>
          <div className="flex items-center gap-2.5">
            <button
              type="button"
              onClick={() => navigate("/login")}
              className="h-10 rounded-sm border-[1.5px] border-line-strong px-4.5 text-sm font-bold text-ink transition-colors hover:bg-sand-2"
            >
              Log in
            </button>
            <button
              type="button"
              onClick={() => navigate("/guest")}
              className="h-10 rounded-sm bg-clay px-4.5 text-sm font-bold text-white transition-colors hover:bg-clay-hover"
            >
              Continue as Guest
            </button>
          </div>
        </div>
      </header>

      <main>
        <section className="px-6 pt-8">
          <div className="mx-auto max-w-[1180px] overflow-hidden rounded-xl border border-line shadow-[var(--e3)] md:flex md:min-h-[460px]">
            <div className="flex w-full flex-none flex-col items-center bg-cream px-6 py-12 text-center md:w-[400px] md:justify-center md:border-r md:border-line">
              <div className="mb-4 flex h-20 w-20 items-center justify-center">
                <span className="flex h-13 w-13 items-center justify-center rounded-full bg-blood text-white shadow-[var(--e1)]">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                    <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
                  </svg>
                </span>
              </div>
              <h1 className="mb-3 text-[24px] font-extrabold tracking-tight">Community Health Hub</h1>
              <p className="max-w-[34ch] text-[14.5px] leading-relaxed text-ink-2">
                Blood donation, hospital inventory and wellness — behind one sign-in.
              </p>

              <div className="mt-6 flex w-full items-center justify-center gap-6">
                <StatCounter value={STATS[0].value} suffix={STATS[0].suffix} label={STATS[0].label} />
                <span className="h-6.5 w-px bg-line-strong" />
                <StatCounter value={STATS[1].value} suffix={STATS[1].suffix} label={STATS[1].label} />
                <span className="h-6.5 w-px bg-line-strong" />
                <div className="flex flex-col items-center gap-0.5">
                  <b className="text-xl font-extrabold tracking-tight text-ink [font-variant-numeric:tabular-nums]">
                    &lt;90s
                  </b>
                  <span className="text-[10.5px] font-semibold text-ink-3">Median match</span>
                </div>
              </div>

              <div className="mt-6 flex w-full flex-col gap-3">
                <button
                  type="button"
                  onClick={() => navigate("/guest")}
                  className="flex h-[50px] w-full items-center justify-center gap-2 rounded-md bg-clay text-[15px] font-bold text-white shadow-[var(--e1)] transition-colors hover:bg-clay-hover"
                >
                  Continue as Guest
                </button>
                <button
                  type="button"
                  onClick={() => navigate("/login")}
                  className="flex h-[50px] w-full items-center justify-center gap-2 rounded-md border-[1.5px] border-line-strong text-[15px] font-bold text-ink transition-colors hover:bg-sand-2"
                >
                  Log in
                </button>
              </div>
            </div>

            <div className="relative min-h-[260px] flex-1 overflow-hidden">
              {SLIDES.map((slide, i) => (
                <div
                  key={slide.key}
                  aria-hidden={i !== activeSlide}
                  className={`flex h-full flex-col items-center gap-4 p-6 text-center transition-opacity duration-500 md:flex-row md:items-center md:gap-6 md:p-12 md:text-left ${
                    i === activeSlide ? "opacity-100" : "pointer-events-none absolute inset-0 opacity-0"
                  }`}
                >
                  <div className="min-w-0 flex-1">
                    <span className={`mb-3 inline-block text-[11.5px] font-extrabold uppercase tracking-widest ${slide.tagClass}`}>
                      {slide.tag}
                    </span>
                    <h2 className="mb-3 text-[19px] font-extrabold leading-tight tracking-tight text-ink md:text-[24px]">
                      {slide.title}
                    </h2>
                    <p className="max-w-[34ch] text-sm leading-relaxed text-ink-2">{slide.body}</p>
                  </div>
                </div>
              ))}

              <div className="absolute bottom-4 left-0 right-0 z-[3] flex justify-center gap-1.5">
                {SLIDES.map((slide, i) => (
                  <button
                    key={slide.key}
                    type="button"
                    aria-label={`Slide ${i + 1}`}
                    aria-current={i === activeSlide}
                    onClick={() => selectSlide(i)}
                    className={`h-1.5 rounded-full transition-all ${
                      i === activeSlide ? "w-7 bg-ink" : "w-5 bg-ink/20"
                    }`}
                  />
                ))}
              </div>
            </div>
          </div>
        </section>

        <div className="border-y border-line bg-sand-2">
          <div className="mx-auto grid max-w-[1180px] grid-cols-2 gap-6 px-6 py-8 text-center md:grid-cols-4 md:px-12">
            <div>
              <b className="block text-[32px] font-extrabold tracking-tight text-ink [font-variant-numeric:tabular-nums]">
                12,400+
              </b>
              <span className="mt-1 block text-[13px] text-ink-2">OTP logins / month</span>
            </div>
            <div>
              <b className="block text-[32px] font-extrabold tracking-tight text-ink [font-variant-numeric:tabular-nums]">
                340
              </b>
              <span className="mt-1 block text-[13px] text-ink-2">Partner facilities</span>
            </div>
            <div>
              <b className="block text-[32px] font-extrabold tracking-tight text-ink [font-variant-numeric:tabular-nums]">
                2,300+
              </b>
              <span className="mt-1 block text-[13px] text-ink-2">Verified donors</span>
            </div>
            <div>
              <b className="block text-[32px] font-extrabold tracking-tight text-ink [font-variant-numeric:tabular-nums]">
                &lt;90s
              </b>
              <span className="mt-1 block text-[13px] text-ink-2">Median match time</span>
            </div>
          </div>
        </div>

        <section className="px-6 py-[var(--s9,64px)]">
          <div className="mx-auto grid max-w-[1180px] gap-12 md:grid-cols-[1.1fr_0.9fr] md:items-center">
            <div>
              <span className="mb-4 inline-flex items-center gap-2 rounded-full bg-blood-tint px-3.5 py-1.5 text-xs font-extrabold uppercase tracking-widest text-blood-deep">
                About us
              </span>
              <h2 className="mb-4 text-[clamp(26px,3.2vw,36px)] font-extrabold tracking-tight">
                Finding a donor shouldn&apos;t take forty phone calls.
              </h2>
              <p className="max-w-[52ch] text-[16.5px] leading-relaxed text-ink-2">
                Community Health Hub started with a simple frustration: at 2 a.m., in a hospital
                corridor, the hardest part of an emergency was often just finding out who to call
                next. So we built one platform that connects verified donors, hospitals and
                NGOs — matched by blood group and distance, notified in seconds.
              </p>
              <p className="mt-4 max-w-[52ch] text-[16.5px] leading-relaxed text-ink-2">
                Alongside emergencies, the same account carries the everyday: medicine reminders,
                wellness check-ins, and a dashboard that adapts to whether you&apos;re a donor, a
                facility, or someone who just needs help right now.
              </p>
              <blockquote className="mt-6 rounded-r-md border-l-4 border-clay bg-cream px-5 py-4 text-[17px] font-semibold italic text-ink shadow-[var(--e1)]">
                &quot;Every donor is someone&apos;s reason to hope.&quot;
              </blockquote>
            </div>
            <div className="relative flex min-h-[260px] items-center justify-center py-8" aria-hidden="true">
              <span className="absolute h-[260px] w-[260px] rounded-full border-[1.5px] border-dashed border-line-strong" />
              <span className="absolute h-[200px] w-[200px] rounded-full border-[1.5px] border-dashed border-line-strong" />
              <div className="relative z-10 flex h-[150px] w-[150px] items-center justify-center rounded-full bg-blood text-white shadow-[var(--e2)]">
                <svg width="68" height="68" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
                </svg>
              </div>
            </div>
          </div>
        </section>

        <section className="px-6 pb-[var(--s9,64px)]">
          <div className="mx-auto max-w-[1180px]">
            <div className="mx-auto mb-12 max-w-[640px] text-center">
              <span className="mb-4 inline-flex items-center gap-2 rounded-full bg-clay-tint px-3.5 py-1.5 text-xs font-extrabold uppercase tracking-widest text-clay-deep">
                Why Community Health Hub
              </span>
              <h2 className="text-[clamp(28px,3.4vw,40px)] font-extrabold tracking-tight">
                Three problems, one login.
              </h2>
              <p className="mt-3 text-[17px] leading-relaxed text-ink-2">
                Blood donation, hospital inventory, and everyday wellness usually live in three
                different apps. Here they share one account and one verified identity.
              </p>
            </div>

            <div className="grid gap-6 md:grid-cols-3">
              <div className="rounded-lg border border-line bg-cream p-6 shadow-[var(--e1)] transition-shadow hover:shadow-[var(--e2)]">
                <div className="mb-5 flex h-13 w-13 items-center justify-center rounded-md bg-blood-tint text-blood-deep">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round">
                    <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
                  </svg>
                </div>
                <h3 className="mb-2 text-[19px] font-extrabold tracking-tight">Donors, matched fast</h3>
                <p className="text-[14.5px] leading-relaxed text-ink-2">
                  Post or answer a blood request by group and distance — donors are notified in
                  seconds, not phone calls.
                </p>
              </div>

              <div className="rounded-lg border border-line bg-cream p-6 shadow-[var(--e1)] transition-shadow hover:shadow-[var(--e2)]">
                <div className="mb-5 flex h-13 w-13 items-center justify-center rounded-md bg-clay-tint text-clay">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round">
                    <path d="M4 20.5V8.2l8-4.7 8 4.7v12.3" />
                    <path d="M9.5 20.5v-5h5v5M12 9v4M10 11h4" />
                  </svg>
                </div>
                <h3 className="mb-2 text-[19px] font-extrabold tracking-tight">Real hospital stock</h3>
                <p className="text-[14.5px] leading-relaxed text-ink-2">
                  Facilities publish live inventory by blood group, so a request only reaches
                  places that can actually help.
                </p>
              </div>

              <div className="rounded-lg border border-line bg-cream p-6 shadow-[var(--e1)] transition-shadow hover:shadow-[var(--e2)]">
                <div className="mb-5 flex h-13 w-13 items-center justify-center rounded-md bg-leaf-tint text-leaf">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round">
                    <path d="M12 21s7-5.4 7-11a7 7 0 1 0-14 0c0 5.6 7 11 7 11Z" />
                    <circle cx="12" cy="10" r="2.6" />
                  </svg>
                </div>
                <h3 className="mb-2 text-[19px] font-extrabold tracking-tight">Everyday wellness</h3>
                <p className="text-[14.5px] leading-relaxed text-ink-2">
                  Medicine and check-up reminders that repeat what you entered — never medical
                  advice, never a changed dose.
                </p>
              </div>
            </div>
          </div>
        </section>
      </main>

      <footer className="border-t border-line py-8">
        <div className="mx-auto flex max-w-[1180px] flex-wrap justify-between gap-3 px-6 text-[13px] text-ink-3">
          <span>© 2026 Community Health Hub</span>
          <span>
            Emergency? Call your local blood bank directly — this platform does not replace
            emergency services.
          </span>
        </div>
      </footer>
    </div>
  );
}
