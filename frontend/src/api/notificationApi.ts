import { apiFetch } from "./httpClient";
import type { PagedResponse } from "./bloodRequestApi";
import type { BloodGroup, UrgencyLevel } from "../lib/validation/bloodRequestSchemas";

export type DonorResponseStatus = "Pending" | "Accepted" | "Declined";

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
  responseStatus: DonorResponseStatus;
}

export interface DonorResponseResultDto {
  notificationId: string;
  responseStatus: DonorResponseStatus;
  requesterMobileNumber?: string;
  locationCityArea?: string;
  latitude?: number;
  longitude?: number;
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

// [Authorize]-protected (CHH-35 AC1) — accepts the matched blood request. 422 ("This request is
// no longer active") if fulfilled/expired or the last unit was just taken by another donor; 409
// if already responded.
export function acceptNotification(accessToken: string, id: string): Promise<DonorResponseResultDto> {
  return apiFetch<DonorResponseResultDto>(`/notifications/${id}/accept`, {
    method: "PATCH",
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

// [Authorize]-protected (CHH-35 AC2) — declines the matched blood request. 409 if already responded.
export function declineNotification(accessToken: string, id: string): Promise<DonorResponseResultDto> {
  return apiFetch<DonorResponseResultDto>(`/notifications/${id}/decline`, {
    method: "PATCH",
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}
