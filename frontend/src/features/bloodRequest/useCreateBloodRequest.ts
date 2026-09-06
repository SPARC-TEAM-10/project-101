import { useState } from "react";
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
}

const initialValues: Partial<CreateBloodRequestFormValues> = {
  patientName: "",
  locationCityArea: "",
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
    if (geolocation.status !== "resolved") {
      geolocation.request();
      return { ok: false };
    }
    try {
      const data = await mutation.mutateAsync(parsed.data);
      return { ok: true, data };
    } catch {
      return { ok: false };
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
