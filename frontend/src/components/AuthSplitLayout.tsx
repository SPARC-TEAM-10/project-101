import type { ReactNode } from "react";

interface AuthSplitLayoutProps {
  imageSrc: string;
  imageAlt: string;
  children: ReactNode;
}

// Desktop/web: illustration panel on the left, asymmetric (not a rigid 50/50 split — the form
// side needs more breathing room than the image does). The source illustrations are portrait
// compositions with their own soft-pink circle backgrounds and transparent edges, so they're
// shown with object-contain on a matching tinted panel rather than object-cover, which would
// crop them into an arbitrary, un-designed-looking slice. Mobile: image is hidden entirely and
// the form takes the full width, unchanged from the mobile-first layout.
export function AuthSplitLayout({ imageSrc, imageAlt, children }: AuthSplitLayoutProps) {
  return (
    <div className="grid min-h-screen md:grid-cols-[minmax(0,40%)_1fr]">
      <div className="relative hidden items-center justify-center overflow-hidden bg-gradient-to-br from-clay-tint via-blood-tint to-sand-2 p-10 md:flex">
        <img src={imageSrc} alt={imageAlt} className="max-h-[85vh] w-full max-w-md object-contain drop-shadow-xl" />
      </div>
      {children}
    </div>
  );
}
