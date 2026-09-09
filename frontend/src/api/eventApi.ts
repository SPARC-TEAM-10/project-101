import { apiFetch } from "./httpClient";
import type { EventType } from "../lib/validation/eventSchemas";

export interface CreateEventRequest {
  title: string;
  eventType: EventType;
  description: string;
  venueName: string;
  venueAddress: string;
  latitude: number;
  longitude: number;
  startAtUtc: string;
  endAtUtc: string;
  capacity: number;
  coordinatorName: string;
  coordinatorContact: string;
  rsvpCutoffAtUtc?: string | null;
}

export interface EventDto extends CreateEventRequest {
  id: string;
  facilityId: string;
  status: "Published" | "Cancelled";
  createdAtUtc: string;
  updatedAtUtc: string;
  // Set only when status is "Cancelled" (CHH-41).
  cancellationReason?: string | null;
}

// Matches contracts/chh-api.v1.yaml's POST /events (CHH-38). [Authorize(Roles = "Hospital,Ngo")]
// plus a server-side verified-facility check on the backend.
export function createEvent(accessToken: string | undefined, request: CreateEventRequest): Promise<EventDto> {
  return apiFetch<EventDto>("/events", {
    method: "POST",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
    body: JSON.stringify(request),
  });
}

export interface EventSummaryDto {
  id: string;
  title: string;
  eventType: EventType;
  facilityName: string;
  venueName: string;
  latitude: number;
  longitude: number;
  startAtUtc: string;
  endAtUtc: string;
  distanceKm: number;
  capacity: number;
  // capacity minus the event's active (non-cancelled) RSVP count (CHH-40).
  spotsRemaining: number;
}

export interface SearchEventsParams {
  latitude: number;
  longitude: number;
  radiusKm: number;
  eventType?: EventType;
}

// Matches contracts/chh-api.v1.yaml's GET /events/search (CHH-39). Open to any authenticated
// role — radiusKm is clamped server-side to [5,100], never rejected.
export function searchEvents(accessToken: string | undefined, params: SearchEventsParams): Promise<EventSummaryDto[]> {
  const query = new URLSearchParams({
    latitude: String(params.latitude),
    longitude: String(params.longitude),
    radiusKm: String(params.radiusKm),
  });
  if (params.eventType) {
    query.set("eventType", params.eventType);
  }
  return apiFetch<EventSummaryDto[]>(`/events/search?${query.toString()}`, {
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
  });
}

export type EventRsvpStatus = "Going" | "Cancelled";

export interface EventDetailDto {
  id: string;
  title: string;
  eventType: EventType;
  description: string;
  facilityName: string;
  venueName: string;
  venueAddress: string;
  latitude: number;
  longitude: number;
  startAtUtc: string;
  endAtUtc: string;
  capacity: number;
  spotsRemaining: number;
  coordinatorName: string;
  coordinatorContact: string;
  rsvpCutoffAtUtc?: string | null;
  status: "Published" | "Cancelled";
  // Set only when status is "Cancelled" (CHH-41). Shown verbatim to attendees.
  cancellationReason?: string | null;
  // Only present when latitude/longitude were passed to getEventById.
  distanceKm?: number | null;
  // The caller's own RSVP status — null if they've never RSVP'd (or aren't an Individual).
  myRsvpStatus?: EventRsvpStatus | null;
  myReferenceCode?: string | null;
}

export interface RsvpResponseDto {
  eventId: string;
  status: EventRsvpStatus;
  referenceCode?: string | null;
  spotsRemaining: number;
}

// Matches contracts/chh-api.v1.yaml's GET /events/{id} (CHH-40). Open to any authenticated role.
export function getEventById(
  accessToken: string | undefined,
  eventId: string,
  coordinates?: { latitude: number; longitude: number },
): Promise<EventDetailDto> {
  const query = new URLSearchParams();
  if (coordinates) {
    query.set("latitude", String(coordinates.latitude));
    query.set("longitude", String(coordinates.longitude));
  }
  const queryString = query.toString();
  const suffix = queryString.length > 0 ? `?${queryString}` : "";
  return apiFetch<EventDetailDto>(`/events/${eventId}${suffix}`, {
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
  });
}

// Matches contracts/chh-api.v1.yaml's POST /events/{id}/rsvp (CHH-40). [Authorize(Roles = "Individual")]
export function rsvpToEvent(accessToken: string | undefined, eventId: string): Promise<RsvpResponseDto> {
  return apiFetch<RsvpResponseDto>(`/events/${eventId}/rsvp`, {
    method: "POST",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
  });
}

// Matches contracts/chh-api.v1.yaml's DELETE /events/{id}/rsvp (CHH-40). [Authorize(Roles = "Individual")]
export function cancelEventRsvp(accessToken: string | undefined, eventId: string): Promise<RsvpResponseDto> {
  return apiFetch<RsvpResponseDto>(`/events/${eventId}/rsvp`, {
    method: "DELETE",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
  });
}

export interface UpdateEventRequest {
  title?: string;
  eventType?: EventType;
  description?: string;
  venueName?: string;
  venueAddress?: string;
  latitude?: number;
  longitude?: number;
  startAtUtc?: string;
  endAtUtc?: string;
  capacity?: number;
  coordinatorName?: string;
  coordinatorContact?: string;
  rsvpCutoffAtUtc?: string | null;
}

export interface CancelEventRequest {
  reason: string;
}

// Matches contracts/chh-api.v1.yaml's GET /events/mine (CHH-41). [Authorize(Roles = "Hospital,Ngo")]
export function getMyEvents(accessToken: string | undefined): Promise<EventDto[]> {
  return apiFetch<EventDto[]>("/events/mine", {
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
  });
}

// Matches contracts/chh-api.v1.yaml's PATCH /events/{id} (CHH-41). [Authorize(Roles = "Hospital,Ngo")]
export function updateEvent(accessToken: string | undefined, eventId: string, request: UpdateEventRequest): Promise<EventDto> {
  return apiFetch<EventDto>(`/events/${eventId}`, {
    method: "PATCH",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
    body: JSON.stringify(request),
  });
}

// Matches contracts/chh-api.v1.yaml's POST /events/{id}/cancel (CHH-41). [Authorize(Roles = "Hospital,Ngo")]
export function cancelEvent(accessToken: string | undefined, eventId: string, request: CancelEventRequest): Promise<EventDto> {
  return apiFetch<EventDto>(`/events/${eventId}/cancel`, {
    method: "POST",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
    body: JSON.stringify(request),
  });
}
