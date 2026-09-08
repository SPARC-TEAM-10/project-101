import { apiFetch } from "./httpClient";
import type { PagedResponse } from "./bloodRequestApi";
import type { BloodGroup, UrgencyLevel } from "../lib/validation/bloodRequestSchemas";

export interface DonorNotificationDto {
  id: string;
  bloodRequestId: string;
  bloodGroup: BloodGroup;
  unitsRequired: number;
  urgency: UrgencyLevel;
  distanceKm: number;
  areaLabel: string;
  isRead: boolean;
  createdAtUtc: string;
}

// [Authorize]-protected (CHH-34) — the caller's own notifications, newest first.
export function getMyNotifications(
  accessToken: string,
  page = 1,
  pageSize = 20,
): Promise<PagedResponse<DonorNotificationDto>> {
  return apiFetch<PagedResponse<DonorNotificationDto>>(`/notifications/mine?page=${page}&pageSize=${pageSize}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

// [Authorize]-protected (CHH-34) — marks one of the caller's own notifications read.
export function markNotificationRead(accessToken: string, id: string): Promise<DonorNotificationDto> {
  return apiFetch<DonorNotificationDto>(`/notifications/${id}/read`, {
    method: "PATCH",
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}
