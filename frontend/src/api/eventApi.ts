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
