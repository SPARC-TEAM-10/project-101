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
  // Always equals capacity until CHH-40 introduces RSVP tracking — no RSVP entity exists yet.
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
