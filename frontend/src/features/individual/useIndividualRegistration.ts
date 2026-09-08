import { useEffect, useState } from "react";
import { useMutation } from "@tanstack/react-query";

import { registerIndividual, type IndividualProfileDto } from "../../api/individualApi";
import { ApiError } from "../../api/httpClient";
import {
  individualRegistrationSchema,
  isReceiverOnly,
  type Gender,
  type IndividualRegistrationFormValues,
} from "../../lib/validation/individualSchemas";
import type { BloodGroup } from "../../lib/validation/bloodRequestSchemas";
import { useGeolocation } from "../shared/useGeolocation";

export interface IndividualRegistrationFormError {
  status: number | null;
  message: string;
}

export interface IndividualRegistrationSubmitResult {
  ok: boolean;
  data?: IndividualProfileDto;
  error?: IndividualRegistrationFormError;
}

const initialValues: Partial<IndividualRegistrationFormValues> = {
  fullName: "",
  email: "",
  dateOfBirth: "",
  locationCityArea: "",
  isChronicIllness: false,
  hasRecentSurgery: false,
  isInfectiousDisease: false,
  isUnderweight: false,
  isOtherIllness: false,
  otherIllnessDetails: "",
};

function toIndividualFormError(err: unknown): IndividualRegistrationFormError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't create your account. Try again." };
  }
  return { status: null, message: "Couldn't create your account. Try again." };
}

export function useIndividualRegistration(mobileNumber: string | null) {
  const [values, setValues] = useState<Partial<IndividualRegistrationFormValues>>(initialValues);
  const [touched, setTouched] = useState(false);
  const geolocation = useGeolocation();

  const parsed = individualRegistrationSchema.safeParse(values);
  const fieldErrors = parsed.success ? {} : parsed.error.flatten().fieldErrors;

  const mutation = useMutation<IndividualProfileDto, unknown, IndividualRegistrationFormValues>({
    mutationFn: (formValues) => {
      if (!mobileNumber) {
        throw new Error("Not authenticated");
      }
      return registerIndividual({ mobileNumber, ...formValues });
    },
  });

  // Auto-fills the location once "Use current location" resolves a readable address — same
  // convenience as the blood-request form (features/shared/useGeolocation).
  useEffect(() => {
    if (geolocation.addressLabel) {
      setValues((prev) => ({ ...prev, locationCityArea: geolocation.addressLabel! }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [geolocation.addressLabel]);

  function setField<K extends keyof IndividualRegistrationFormValues>(
    key: K,
    value: IndividualRegistrationFormValues[K],
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
      return { ok: false, error: toIndividualFormError(err) };
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
    setChronicIllness: (v: boolean) => setField("isChronicIllness", v),
    setRecentSurgery: (v: boolean) => setField("hasRecentSurgery", v),
    setInfectiousDisease: (v: boolean) => setField("isInfectiousDisease", v),
    setUnderweight: (v: boolean) => setField("isUnderweight", v),
    setOtherIllness: (v: boolean) => setField("isOtherIllness", v),
    setOtherIllnessDetails: (v: string) => setField("otherIllnessDetails", v),
    fieldErrors,
    touched,
    geolocation,
    isReceiverOnly: isReceiverOnly({
      isChronicIllness: values.isChronicIllness ?? false,
      hasRecentSurgery: values.hasRecentSurgery ?? false,
      isInfectiousDisease: values.isInfectiousDisease ?? false,
      isUnderweight: values.isUnderweight ?? false,
      isOtherIllness: values.isOtherIllness ?? false,
    }),
    isPending: mutation.isPending,
    error: mutation.error ? toIndividualFormError(mutation.error) : null,
    submit,
  };
}
