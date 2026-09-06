const DROP_PATH = "M12 2C12 2 4.5 11.5 4.5 16A7.5 7.5 0 0 0 19.5 16C19.5 11.5 12 2 12 2Z";

export type DripLoaderSize = "sm" | "md" | "lg" | "xl" | "btn";

// Blood-drip loader — from the design canvas's FeedbackComponents artboard
// (https://claude.ai/code/artifact/6b185d14-a32d-4647-a2b7-7366b07c2b75, page 2). Styling lives
// in index.css (.drip-loader) since the fall/ripple keyframes need CSS custom properties per size.
export function DripLoader({ size = "md" }: { size?: DripLoaderSize }) {
  return (
    <div className={`drip-loader ${size}`} role="status" aria-label="Loading">
      <svg className="duct" viewBox="0 0 24 24" aria-hidden="true">
        <path d={DROP_PATH} />
        <ellipse className="shine" cx="9.3" cy="12" rx="1.5" ry="2.4" />
      </svg>
      <svg className="drop" viewBox="0 0 24 24" aria-hidden="true">
        <path d={DROP_PATH} />
      </svg>
      <div className="ripple" />
    </div>
  );
}
