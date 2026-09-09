import { useEffect, useState } from "react";
import { useMutation } from "@tanstack/react-query";

import { updateMyProfile, type IndividualProfileDto } from "../../api/individualApi";
import { ApiError } from "../../api/httpClient";
import {
  individualProfileUpdateSchema,
  isReceiverOnly,
  type IndividualProfileUpdateFormValues,
} from "../../lib/validation/individualSchemas";
import { useGeolocation } from "../shared/useGeolocation";

export interface IndividualProfileUpdateFormError {
  status: number | null;
  message: string;
}

export interface IndividualProfileUpdateSubmitResult {
  ok: boolean;
  data?: IndividualProfileDto;
  error?: IndividualProfileUpdateFormError;
}

function toFormValues(profile: IndividualProfileDto): IndividualProfileUpdateFormValues {
  return {
    locationCityArea: profile.locationCityArea,
    isChronicIllness: profile.isChronicIllness,
    hasRecentSurgery: profile.hasRecentSurgery,
    isInfectiousDisease: profile.isInfectiousDisease,
    isUnderweight: profile.isUnderweight,
    isOtherIllness: profile.isOtherIllness,
    otherIllnessDetails: profile.otherIllnessDetails ?? "",
    // The API returns null (not undefined) when unset — coerce it, since the Zod schema's
    // z.number().optional() rejects null and would otherwise fail validation on every submit
    // for a profile that hasn't shared a location yet.
    latitude: profile.latitude ?? undefined,
    longitude: profile.longitude ?? undefined,
  };
}

function toFormError(err: unknown): IndividualProfileUpdateFormError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't save your changes. Try again." };
  }
  return { status: null, message: "Couldn't save your changes. Try again." };
}

// Pre-fills from the caller's existing profile (CHH-F02 profile edit) — location and the
// health-screening flags are the only editable fields; see UpdateIndividualProfileRequest.
export function useUpdateIndividualProfile(accessToken: string | null, profile: IndividualProfileDto | undefined) {
  const [values, setValues] = useState<Partial<IndividualProfileUpdateFormValues>>({});
  const [touched, setTouched] = useState(false);
  const [isSharingLocation, setIsSharingLocation] = useState(false);
  const geolocation = useGeolocation();

  useEffect(() => {
    if (profile) {
      setValues(toFormValues(profile));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [profile?.id]);

  useEffect(() => {
    if (geolocation.addressLabel) {
      setValues((prev) => ({ ...prev, locationCityArea: geolocation.addressLabel! }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [geolocation.addressLabel]);

  useEffect(() => {
    if (geolocation.coordinates) {
      setValues((prev) => ({
        ...prev,
        latitude: geolocation.coordinates!.latitude,
        longitude: geolocation.coordinates!.longitude,
      }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [geolocation.coordinates]);

  const parsed = individualProfileUpdateSchema.safeParse(values);
  const fieldErrors = parsed.success ? {} : parsed.error.flatten().fieldErrors;

  const mutation = useMutation<IndividualProfileDto, unknown, IndividualProfileUpdateFormValues>({
    mutationFn: (formValues) => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return updateMyProfile(accessToken, formValues);
    },
  });

  function setField<K extends keyof IndividualProfileUpdateFormValues>(
    key: K,
    value: IndividualProfileUpdateFormValues[K],
  ) {
    setValues((prev) => ({ ...prev, [key]: value }));
    if (mutation.isError) {
      mutation.reset();
    }
  }

  async function submit(): Promise<IndividualProfileUpdateSubmitResult> {
    setTouched(true);
    if (!parsed.success) {
      return { ok: false };
    }
    try {
      const data = await mutation.mutateAsync(parsed.data);
      return { ok: true, data };
    } catch (err) {
      return { ok: false, error: toFormError(err) };
    }
  }

  // CHH-85: caller requests the device's coordinates directly, without entering the full edit
  // form. `isSharingLocation` lets ProfilePage know a resolved coordinate should be auto-saved
  // via the same `submit()` path (and its existing toast/cache-sync handling) rather than
  // waiting for a manual "Save changes" click.
  function shareLocation() {
    setIsSharingLocation(true);
    geolocation.request();
  }

  return {
    values,
    locationShared: profile?.latitude != null && profile?.longitude != null,
    shareLocation,
    isSharingLocation,
    clearSharingLocation: () => setIsSharingLocation(false),
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
    isSuccess: mutation.isSuccess,
    error: mutation.error ? toFormError(mutation.error) : null,
    submit,
  };
}
