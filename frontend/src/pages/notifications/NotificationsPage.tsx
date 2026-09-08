import { useNavigate } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import { useNotifications } from "../../features/notifications/useNotifications";
import type { DonorNotificationDto } from "../../api/notificationApi";

// "Emergency" gets the same high-visibility red as the blood-request form's urgency chip — the
// ticket's UI note calls for high-visibility styling specifically for Emergency notifications.
const URGENCY_CLASSES: Record<string, string> = {
  Emergency: "bg-error-tint text-error",
  Urgent: "bg-[#fdecd9] text-[#e07b1a]",
  Standard: "bg-sand-2 text-ink-2",
};

function timeAgo(isoDate: string): string {
  const seconds = Math.floor((Date.now() - new Date(isoDate).getTime()) / 1000);
  if (seconds < 60) return "Just now";
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  return new Date(isoDate).toLocaleDateString(undefined, { day: "numeric", month: "short" });
}

function NotificationRow({ notification, onMarkRead }: { notification: DonorNotificationDto; onMarkRead: (id: string) => void }) {
  return (
    <button
      type="button"
      onClick={() => !notification.isRead && onMarkRead(notification.id)}
      className={`flex w-full flex-col gap-1.5 rounded-md border px-4 py-3.5 text-left transition-colors ${
        notification.isRead ? "border-line bg-cream" : "border-clay-tint bg-clay-tint/40"
      }`}
    >
      <div className="flex items-center gap-2">
        <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-blood-tint text-[13px] font-extrabold text-blood-deep">
          {notification.bloodGroup}
        </span>
        <div className="min-w-0 flex-1">
          <div className="truncate text-[14.5px] font-semibold text-ink">
            {notification.unitsRequired} unit{notification.unitsRequired === 1 ? "" : "s"} needed &middot; ~{notification.distanceKm.toFixed(1)}km away
          </div>
          <div className="text-[12.5px] text-ink-3">
            {notification.areaLabel} &middot; {timeAgo(notification.createdAtUtc)}
          </div>
        </div>
        {!notification.isRead && <span className="h-2.5 w-2.5 shrink-0 rounded-full bg-clay" aria-label="Unread" />}
      </div>
      <span
        className={`inline-block w-fit rounded-full px-[10px] py-[3px] text-[11.5px] font-semibold ${
          URGENCY_CLASSES[notification.urgency] ?? URGENCY_CLASSES.Standard
        }`}
      >
        {notification.urgency}
      </span>
    </button>
  );
}

/**
 * Notification center (CHH-34). Lists the caller's own proximity alerts, newest first (AC3),
 * with a distinct "Unread" indicator and high-visibility styling for Emergency notifications
 * (ticket UI note). Deliberately never shows the patient's name or exact address (AC2).
 */
export function NotificationsPage() {
  const navigate = useNavigate();
  const { session } = useAuth();
  const { notifications, isLoading, isError, markRead } = useNotifications(session?.token);

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-3 border-b border-line bg-cream px-4 lg:px-8">
        <button
          type="button"
          onClick={() => navigate("/dashboard/individual")}
          aria-label="Back to dashboard"
          className="flex h-9 w-9 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2 hover:text-ink"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
        </button>
        <b className="text-[15px] font-bold">Notifications</b>
      </div>

      <div className="mx-auto max-w-xl px-4 py-6 lg:px-0">
        {isLoading && <p className="text-sm text-ink-2">Loading…</p>}

        {isError && (
          <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
            Couldn&apos;t load your notifications. Try refreshing the page.
          </div>
        )}

        {!isLoading && !isError && notifications.length === 0 && (
          <div className="flex flex-col items-center rounded-md border border-dashed border-line-strong bg-cream px-4 py-10 text-center">
            <div className="mb-3 flex h-[68px] w-[68px] items-center justify-center rounded-full bg-sand-2 text-ink-3">
              <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M18 8a6 6 0 1 0-12 0c0 6-2 7-2 7h16s-2-1-2-7" />
                <path d="M10.5 20a1.8 1.8 0 0 0 3 0" />
              </svg>
            </div>
            <h3 className="mb-1 text-base font-bold text-ink">Nothing to read yet</h3>
            <p className="max-w-[36ch] text-[13px] leading-[19px] text-ink-2">
              Requests matching your blood group and area will land here as soon as they come in.
            </p>
          </div>
        )}

        {!isLoading && notifications.length > 0 && (
          <div className="flex flex-col gap-2">
            {notifications.map((notification) => (
              <NotificationRow key={notification.id} notification={notification} onMarkRead={markRead} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
