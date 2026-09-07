import { useEffect, useState } from "react";
import { useMutation } from "@tanstack/react-query";

import { registerIndividual, type IndividualProfileDto } from "../../api/individualsApi";
import { ApiError } from "../../api/httpClient";
import type { BloodGroup } from "../../lib/validation/bloodRequestSchemas";
import {
  createIndividualProfileSchema,
  type CreateIndividualProfileFormValues,
  type Gender,
} from "../../lib/validation/individualSchemas";
import { useGeolocation } from "../bloodRequest/useGeolocation";

export interface IndividualRegistrationError {
  status: number | null;
  message: string;
}

export interface IndividualRegistrationSubmitResult {
  ok: boolean;
  data?: IndividualProfileDto;
  error?: IndividualRegistrationError;
}

const initialValues: Partial<CreateIndividualProfileFormValues> = {
  fullName: "",
  email: "",
  locationCityArea: "",
  isChronicIllness: false,
  hasRecentSurgery: false,
  isInfectiousDisease: false,
  isUnderweight: false,
  isOtherIllness: false,
};

function toRegistrationError(err: unknown): IndividualRegistrationError {
  if (err instanceof ApiError) {
    if (err.status === 409) {
      return { status: 409, message: "A profile already exists for this mobile number." };
    }
    return { status: err.status, message: err.problem.detail ?? "Couldn't complete registration. Try again." };
  }
  return { status: null, message: "Couldn't complete registration. Try again." };
}

// mobileNumber comes from the OTP-verified session (AuthProvider), passed in by the page —
// mirrors useCreateBloodRequest(accessToken) taking its auth input as a parameter rather than
// reading context directly, so this hook stays free of React Router/JSX per the layer-isolation
// rule (frontend/CLAUDE.md).
export function useIndividualRegistration(mobileNumber: string | undefined) {
  const [values, setValues] = useState<Partial<CreateIndividualProfileFormValues>>(initialValues);
  const [touched, setTouched] = useState(false);
  const geolocation = useGeolocation();

  const parsed = createIndividualProfileSchema.safeParse(values);
  const fieldErrors = parsed.success ? {} : parsed.error.flatten().fieldErrors;

  const mutation = useMutation<IndividualProfileDto, unknown, CreateIndividualProfileFormValues>({
    mutationFn: (formValues) => {
      if (!mobileNumber) {
        throw new Error("Not authenticated");
      }
      return registerIndividual({ mobileNumber, ...formValues });
    },
  });

  // Auto-fills the location text once "Use current location" resolves a readable address — same
  // pattern as useCreateBloodRequest; the field stays a plain manual fallback if this never fires.
  useEffect(() => {
    if (geolocation.addressLabel) {
      setValues((prev) => ({ ...prev, locationCityArea: geolocation.addressLabel! }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [geolocation.addressLabel]);

  function setField<K extends keyof CreateIndividualProfileFormValues>(
    key: K,
    value: CreateIndividualProfileFormValues[K],
  ) {
    setValues((prev) => ({ ...prev, [key]: value }));
    if (mutation.isError) {
      mutation.reset();
    }
  }

  async function submit(): Promise<IndividualRegistrationSubmitResult> {
    setTouched(true);
    if (!parsed.success) {
      return { ok: false };
    }
    try {
      const data = await mutation.mutateAsync(parsed.data);
      return { ok: true, data };
    } catch (err) {
      // Returned directly (not read back from the hook's `error` state) — that state updates on
      // the next render, so a caller checking it synchronously right after this await would see
      // a stale value from before this attempt.
      return { ok: false, error: toRegistrationError(err) };
    }
  }

  return {
    values,
    setFullName: (v: string) => setField("fullName", v),
    setEmail: (v: string) => setField("email", v),
    setBloodGroup: (v: BloodGroup) => setField("bloodGroup", v),
    setDateOfBirth: (v: string) => setField("dateOfBirth", v),
    setGender: (v: Gender) => setField("gender", v),
    setLocationCityArea: (v: string) => setField("locationCityArea", v),
    setIsChronicIllness: (v: boolean) => setField("isChronicIllness", v),
    setHasRecentSurgery: (v: boolean) => setField("hasRecentSurgery", v),
    setIsInfectiousDisease: (v: boolean) => setField("isInfectiousDisease", v),
    setIsUnderweight: (v: boolean) => setField("isUnderweight", v),
    setIsOtherIllness: (v: boolean) => setField("isOtherIllness", v),
    setOtherIllnessDetails: (v: string) => setField("otherIllnessDetails", v),
    isValid: parsed.success,
    fieldErrors,
    touched,
    geolocation,
    submit,
    isPending: mutation.isPending,
    error: mutation.error ? toRegistrationError(mutation.error) : null,
  };
}
