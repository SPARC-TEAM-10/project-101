import { z } from "zod";

// Mirrors CreateEventRequestValidator (backend) — see CHH-38/US-CHH-005-01, spec §6.2.
export const EVENT_TYPES = ["BloodDonationCamp", "HealthCamp", "AwarenessSession", "Other"] as const;
export type EventType = (typeof EVENT_TYPES)[number];

export const EVENT_TYPE_LABELS: Record<EventType, string> = {
  BloodDonationCamp: "Blood Donation Camp",
  HealthCamp: "Health Camp",
  AwarenessSession: "Awareness Session",
  Other: "Other",
};

export const MIN_TITLE_LENGTH = 5;
export const MAX_TITLE_LENGTH = 100;
export const MIN_DESCRIPTION_LENGTH = 20;
export const MAX_DESCRIPTION_LENGTH = 1000;
export const MIN_VENUE_NAME_LENGTH = 3;
export const MAX_VENUE_NAME_LENGTH = 100;
export const MIN_VENUE_ADDRESS_LENGTH = 10;
export const MAX_VENUE_ADDRESS_LENGTH = 250;
export const MIN_CAPACITY = 1;
export const MAX_CAPACITY = 1000;
export const MIN_COORDINATOR_NAME_LENGTH = 2;
export const MAX_COORDINATOR_NAME_LENGTH = 50;
export const MIN_LEAD_TIME_MS = 60 * 60 * 1000;
export const MIN_CANCELLATION_REASON_LENGTH = 10;
export const MAX_CANCELLATION_REASON_LENGTH = 300;

const MOBILE_PATTERN = /^\d{10}$/;

const baseEventSchema = z.object({
  title: z
    .string()
    .trim()
    .min(MIN_TITLE_LENGTH, `Title must be between ${MIN_TITLE_LENGTH} and ${MAX_TITLE_LENGTH} characters.`)
    .max(MAX_TITLE_LENGTH, `Title must be between ${MIN_TITLE_LENGTH} and ${MAX_TITLE_LENGTH} characters.`),
  eventType: z.enum(EVENT_TYPES, { message: "Please select an event type." }),
  description: z
    .string()
    .trim()
    .min(MIN_DESCRIPTION_LENGTH, `Description must be between ${MIN_DESCRIPTION_LENGTH} and ${MAX_DESCRIPTION_LENGTH} characters.`)
    .max(MAX_DESCRIPTION_LENGTH, `Description must be between ${MIN_DESCRIPTION_LENGTH} and ${MAX_DESCRIPTION_LENGTH} characters.`),
  venueName: z
    .string()
    .trim()
    .min(MIN_VENUE_NAME_LENGTH, "Please enter a valid venue name.")
    .max(MAX_VENUE_NAME_LENGTH, "Please enter a valid venue name."),
  venueAddress: z
    .string()
    .trim()
    .min(MIN_VENUE_ADDRESS_LENGTH, "Please enter a valid venue address and location.")
    .max(MAX_VENUE_ADDRESS_LENGTH, "Please enter a valid venue address and location."),
  latitude: z.number({ message: "Please select a valid location on the map." }),
  longitude: z.number({ message: "Please select a valid location on the map." }),
  startAtUtc: z
    .string()
    .refine((v) => new Date(v).getTime() >= Date.now() + MIN_LEAD_TIME_MS, "Event must start in the future."),
  endAtUtc: z.string(),
  capacity: z
    .number({ message: "Capacity must be between 1 and 1,000." })
    .int()
    .min(MIN_CAPACITY, "Capacity must be between 1 and 1,000.")
    .max(MAX_CAPACITY, "Capacity must be between 1 and 1,000."),
  coordinatorName: z
    .string()
    .trim()
    .min(MIN_COORDINATOR_NAME_LENGTH, "Please enter a valid coordinator name.")
    .max(MAX_COORDINATOR_NAME_LENGTH, "Please enter a valid coordinator name."),
  coordinatorContact: z.string().trim().regex(MOBILE_PATTERN, "Please enter a valid coordinator contact number."),
  rsvpCutoffAtUtc: z.string().optional(),
});

export const createEventSchema = baseEventSchema
  .refine((v) => new Date(v.endAtUtc).getTime() > new Date(v.startAtUtc).getTime(), {
    message: "End time must be after start time.",
    path: ["endAtUtc"],
  })
  .refine((v) => !v.rsvpCutoffAtUtc || new Date(v.rsvpCutoffAtUtc).getTime() < new Date(v.startAtUtc).getTime(), {
    message: "RSVP cut-off must be before the event start time.",
    path: ["rsvpCutoffAtUtc"],
  });

export type CreateEventFormValues = z.infer<typeof baseEventSchema>;
