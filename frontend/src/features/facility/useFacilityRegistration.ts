import { useState } from "react";
import { useMutation } from "@tanstack/react-query";

import { createFacility, type FacilityDto } from "../../api/facilityApi";
import { ApiError } from "../../api/httpClient";
import {
  contactSchema,
  facilityDetailsSchema,
  findDuplicateMobileIndex,
  MAX_CONTACTS,
  MIN_CONTACTS,
  type ContactFormValues,
  type FacilityCategory,
  type FacilityDetailsFormValues,
} from "../../lib/validation/facilitySchemas";

export type FacilityRegistrationStep = "details" | "contacts";

export interface FacilityFormError {
  status: number | null;
  message: string;
}

export interface FacilityRegistrationSubmitResult {
  ok: boolean;
  data?: FacilityDto;
  error?: FacilityFormError;
}

type DetailsValues = Partial<FacilityDetailsFormValues>;

function emptyContact(): ContactFormValues {
  return { name: "", designation: "", mobile: "" };
}

function toFacilityFormError(err: unknown): FacilityFormError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't save the facility. Try again." };
  }
  return { status: null, message: "Couldn't save the facility. Try again." };
}

export function useFacilityRegistration(accessToken: string | undefined, category: FacilityCategory) {
  const [step, setStep] = useState<FacilityRegistrationStep>("details");
  const [details, setDetails] = useState<DetailsValues>({
    facilityName: "",
    licenseNumber: "",
    address: "",
    category,
  });
  const [detailsTouched, setDetailsTouched] = useState(false);
  const [contacts, setContacts] = useState<ContactFormValues[]>([emptyContact()]);
  const [contactsTouched, setContactsTouched] = useState(false);

  const detailsParsed = facilityDetailsSchema.safeParse(details);
  const detailsErrors = detailsParsed.success ? {} : detailsParsed.error.flatten().fieldErrors;

  const contactParses = contacts.map((c) => contactSchema.safeParse(c));
  const contactErrors = contactParses.map((p) => (p.success ? {} : p.error.flatten().fieldErrors));
  const duplicateMobileIndex = findDuplicateMobileIndex(contacts);
  const contactsValid =
    contactParses.every((p) => p.success) && contacts.length >= MIN_CONTACTS && duplicateMobileIndex === null;

  const mutation = useMutation<FacilityDto, unknown, void>({
    mutationFn: () => {
      if (!detailsParsed.success) {
        throw new Error("Facility details are invalid");
      }
      return createFacility(accessToken, {
        ...detailsParsed.data,
        contacts: contacts.map((c) => contactSchema.parse(c)),
      });
    },
  });

  function setDetailsField<K extends keyof FacilityDetailsFormValues>(
    key: K,
    value: FacilityDetailsFormValues[K],
  ) {
    setDetails((prev) => ({ ...prev, [key]: value }));
  }

  function goToContacts() {
    setDetailsTouched(true);
    if (!detailsParsed.success) return;
    setStep("contacts");
  }

  function goBack() {
    setStep("details");
  }

  function addContact() {
    setContacts((prev) => (prev.length >= MAX_CONTACTS ? prev : [...prev, emptyContact()]));
  }

  function removeContact(index: number) {
    setContacts((prev) => (prev.length <= MIN_CONTACTS ? prev : prev.filter((_, i) => i !== index)));
  }

  function setContactField<K extends keyof ContactFormValues>(index: number, key: K, value: ContactFormValues[K]) {
    setContacts((prev) => prev.map((c, i) => (i === index ? { ...c, [key]: value } : c)));
    if (mutation.isError) {
      mutation.reset();
    }
  }

  async function submit(): Promise<FacilityRegistrationSubmitResult> {
    setContactsTouched(true);
    if (!detailsParsed.success || !contactsValid) {
      return { ok: false };
    }
    try {
      const data = await mutation.mutateAsync();
      return { ok: true, data };
    } catch (err) {
      return { ok: false, error: toFacilityFormError(err) };
    }
  }

  return {
    step,
    details,
    setDetailsField,
    detailsErrors,
    detailsTouched,
    goToContacts,
    goBack,
    contacts,
    addContact,
    removeContact,
    setContactField,
    contactErrors,
    duplicateMobileIndex,
    contactsTouched,
    isPending: mutation.isPending,
    error: mutation.error ? toFacilityFormError(mutation.error) : null,
    submit,
  };
}
