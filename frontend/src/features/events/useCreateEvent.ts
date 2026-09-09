import { useState } from "react";
import { useMutation } from "@tanstack/react-query";

import { createEvent, type EventDto } from "../../api/eventApi";
import { ApiError } from "../../api/httpClient";
import {
  createEventSchema,
  type CreateEventFormValues,
  type EventType,
} from "../../lib/validation/eventSchemas";
import { useVenueGeocoding } from "./useVenueGeocoding";

export interface EventFormError {
  status: number | null;
  message: string;
}

export interface EventSubmitResult {
  ok: boolean;
  data?: EventDto;
  error?: EventFormError;
}

type Values = Partial<CreateEventFormValues> & { rsvpCutoffEnabled?: boolean };

function initialValues(): Values {
  return {
    title: "",
    eventType: undefined,
    description: "",
    venueName: "",
    venueAddress: "",
    startAtUtc: "",
    endAtUtc: "",
    capacity: undefined,
    coordinatorName: "",
    coordinatorContact: "",
    rsvpCutoffEnabled: false,
    rsvpCutoffAtUtc: "",
  };
}

function toEventFormError(err: unknown): EventFormError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't publish the event. Try again." };
  }
  return { status: null, message: "Couldn't publish the event. Try again." };
}

export function useCreateEvent(accessToken: string | undefined) {
  const [values, setValues] = useState<Values>(initialValues);
  const [touched, setTouched] = useState(false);
  const geocoding = useVenueGeocoding(values.venueAddress ?? "");

  const candidate: Partial<CreateEventFormValues> = {
    ...values,
    latitude: geocoding.coordinates?.latitude,
    longitude: geocoding.coordinates?.longitude,
    rsvpCutoffAtUtc: values.rsvpCutoffEnabled ? values.rsvpCutoffAtUtc : undefined,
  };
  const parsed = createEventSchema.safeParse(candidate);
  const fieldErrors = parsed.success ? {} : parsed.error.flatten().fieldErrors;

  const mutation = useMutation<EventDto, unknown, CreateEventFormValues>({
    mutationFn: (formValues) => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return createEvent(accessToken, {
        title: formValues.title,
        eventType: formValues.eventType,
        description: formValues.description,
        venueName: formValues.venueName,
        venueAddress: formValues.venueAddress,
        latitude: formValues.latitude,
        longitude: formValues.longitude,
        startAtUtc: new Date(formValues.startAtUtc).toISOString(),
        endAtUtc: new Date(formValues.endAtUtc).toISOString(),
        capacity: formValues.capacity,
        coordinatorName: formValues.coordinatorName,
        coordinatorContact: formValues.coordinatorContact,
        rsvpCutoffAtUtc: formValues.rsvpCutoffAtUtc ? new Date(formValues.rsvpCutoffAtUtc).toISOString() : null,
      });
    },
  });

  function setField<K extends keyof Values>(key: K, value: Values[K]) {
    setValues((prev) => ({ ...prev, [key]: value }));
    if (mutation.isError) {
      mutation.reset();
    }
  }

  async function submit(): Promise<EventSubmitResult> {
    setTouched(true);
    if (!parsed.success) {
      return { ok: false };
    }
    try {
      const data = await mutation.mutateAsync(parsed.data);
      return { ok: true, data };
    } catch (err) {
      return { ok: false, error: toEventFormError(err) };
    }
  }

  return {
    values,
    setTitle: (v: string) => setField("title", v),
    setEventType: (v: EventType) => setField("eventType", v),
    setDescription: (v: string) => setField("description", v),
    setVenueName: (v: string) => setField("venueName", v),
    setVenueAddress: (v: string) => setField("venueAddress", v),
    setStartAtUtc: (v: string) => setField("startAtUtc", v),
    setEndAtUtc: (v: string) => setField("endAtUtc", v),
    setCapacity: (v: number) => setField("capacity", v),
    setCoordinatorName: (v: string) => setField("coordinatorName", v),
    setCoordinatorContact: (v: string) => setField("coordinatorContact", v),
    setRsvpCutoffEnabled: (v: boolean) => setField("rsvpCutoffEnabled", v),
    setRsvpCutoffAtUtc: (v: string) => setField("rsvpCutoffAtUtc", v),
    geocoding,
    setManualVenueCoordinates: geocoding.setManualCoordinates,
    fieldErrors,
    touched,
    submit,
    isPending: mutation.isPending,
    error: mutation.error ? toEventFormError(mutation.error) : null,
  };
}
