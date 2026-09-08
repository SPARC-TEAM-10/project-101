import { useQuery, useQueryClient } from "@tanstack/react-query";

import { getMyNotifications, markNotificationRead, type DonorNotificationDto } from "../../api/notificationApi";
import type { PagedResponse } from "../../api/bloodRequestApi";

// 30s poll — CHH-34's "real-time" intent without a websocket; short enough that a new proximity
// alert shows up quickly, long enough not to hammer the API from an idle tab.
const POLL_INTERVAL_MS = 30_000;

function queryKey(accessToken: string | undefined) {
  return ["notifications", "mine", accessToken] as const;
}

/**
 * Fetches the caller's own notifications (CHH-34), polling for near-real-time updates, and
 * exposes a mark-read mutation that updates the cache optimistically-on-success (no need to
 * refetch the whole list just to flip one row's isRead).
 */
export function useNotifications(accessToken: string | undefined) {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: queryKey(accessToken),
    queryFn: () => getMyNotifications(accessToken!),
    enabled: Boolean(accessToken),
    refetchInterval: POLL_INTERVAL_MS,
  });

  const notifications = query.data?.items ?? [];
  const unreadCount = notifications.filter((n) => !n.isRead).length;

  async function markRead(id: string) {
    if (!accessToken) return;
    const updated = await markNotificationRead(accessToken, id);
    queryClient.setQueryData<PagedResponse<DonorNotificationDto>>(queryKey(accessToken), (prev) =>
      prev
        ? { ...prev, items: prev.items.map((n) => (n.id === updated.id ? updated : n)) }
        : prev,
    );
  }

  return {
    notifications,
    unreadCount,
    isLoading: query.isLoading,
    isError: query.isError,
    markRead,
  };
}
