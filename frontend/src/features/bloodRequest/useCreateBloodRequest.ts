import { useEffect, useState } from "react";
import { useMutation } from "@tanstack/react-query";

import { createBloodRequest, type BloodRequestDto } from "../../api/bloodRequestApi";
import { ApiError } from "../../api/httpClient";
import {
  createBloodRequestSchema,
  MAX_SEARCH_RADIUS_KM,
  MIN_SEARCH_RADIUS_KM,
  type BloodGroup,
  type CreateBloodRequestFormValues,
  type UrgencyLevel,
} from "../../lib/validation/bloodRequestSchemas";
import { useGeolocation } from "./useGeolocation";

export interface BloodRequestFormError {
  status: number | null;
  message: string;
}

export interface BloodRequestSubmitResult {
  ok: boolean;
  data?: BloodRequestDto;
  error?: BloodRequestFormError;
}

const initialValues: Partial<CreateBloodRequestFormValues> = {
  patientName: "",
  locationCityArea: "",
  unitsRequired: 1,
  searchRadiusKm: 10,
};

function toBloodRequestFormError(err: unknown): BloodRequestFormError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't submit the request. Try again." };
  }
  return { status: null, message: "Couldn't submit the request. Try again." };
}

export function useCreateBloodRequest(accessToken: string | undefined) {
  const [values, setValues] = useState<Partial<CreateBloodRequestFormValues>>(initialValues);
  const [touched, setTouched] = useState(false);
  const geolocation = useGeolocation();

  const parsed = createBloodRequestSchema.safeParse(values);
  const fieldErrors = parsed.success ? {} : parsed.error.flatten().fieldErrors;

  const mutation = useMutation<BloodRequestDto, unknown, CreateBloodRequestFormValues>({
    mutationFn: (formValues) => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      if (!geolocation.coordinates) {
        throw new Error("Location not resolved");
      }
      return createBloodRequest(accessToken, {
        ...formValues,
        latitude: geolocation.coordinates.latitude,
        longitude: geolocation.coordinates.longitude,
      });
    },
  });

  // Auto-fills the location text once "Use current location" resolves a readable address —
  // the whole point of that button is to save the user from typing it themselves.
  useEffect(() => {
    if (geolocation.addressLabel) {
      setValues((prev) => ({ ...prev, locationCityArea: geolocation.addressLabel! }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [geolocation.addressLabel]);

  function setField<K extends keyof CreateBloodRequestFormValues>(key: K, value: CreateBloodRequestFormValues[K]) {
    setValues((prev) => ({ ...prev, [key]: value }));
    if (mutation.isError) {
      mutation.reset();
    }
  }

  async function submit(): Promise<BloodRequestSubmitResult> {
    setTouched(true);
    if (!parsed.success) {
      return { ok: false };
    }
    // Location detection is its own explicit button in the UI (not triggered implicitly here) —
    // a hidden "first click detects location, second click submits" flow was confusing (a real
    // user-reported issue: clicking Submit appeared to do nothing on the first click).
    if (geolocation.status !== "resolved") {
      return { ok: false };
    }
    try {
      const data = await mutation.mutateAsync(parsed.data);
      return { ok: true, data };
    } catch (err) {
      // Returned directly (not read back from the hook's `error` state) — that state updates on
      // the next render, so a caller checking it synchronously right after this await would see
      // a stale value from before this attempt.
      return { ok: false, error: toBloodRequestFormError(err) };
    }
  }

  return {
    values,
    setPatientName: (v: string) => setField("patientName", v),
    setBloodGroup: (v: BloodGroup) => setField("bloodGroup", v),
    setUnitsRequired: (v: number) => setField("unitsRequired", v),
    setLocationCityArea: (v: string) => setField("locationCityArea", v),
    setSearchRadiusKm: (v: number) => setField("searchRadiusKm", v),
    setUrgency: (v: UrgencyLevel) => setField("urgency", v),
    isValid: parsed.success,
    fieldErrors,
    touched,
    geolocation,
    minRadiusKm: MIN_SEARCH_RADIUS_KM,
    maxRadiusKm: MAX_SEARCH_RADIUS_KM,
    submit,
    isPending: mutation.isPending,
    error: mutation.error ? toBloodRequestFormError(mutation.error) : null,
  };
}
