import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it } from "vitest";
import type { ReactNode } from "react";

import { useFacilityRegistration } from "./useFacilityRegistration";
import { server } from "../../../tests/setup";
import { createFacilityValidationErrorHandler } from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

const validDetails = {
  facilityName: "Kochi Metro Hospital",
  category: "Hospital" as const,
  licenseNumber: "KL-HOSP-448120",
  address: "4th Block, Marine Drive, Ernakulam, Kochi 682031",
};

function fillDetails(result: { current: ReturnType<typeof useFacilityRegistration> }) {
  result.current.setDetailsField("facilityName", validDetails.facilityName);
  result.current.setDetailsField("category", validDetails.category);
  result.current.setDetailsField("licenseNumber", validDetails.licenseNumber);
  result.current.setDetailsField("address", validDetails.address);
}

describe("useFacilityRegistration", () => {
  it("starts on the details step with one empty contact", () => {
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    expect(result.current.step).toBe("details");
    expect(result.current.contacts).toHaveLength(1);
  });

  it("does not advance to contacts when details are invalid (AC1)", () => {
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    act(() => {
      result.current.goToContacts();
    });

    expect(result.current.step).toBe("details");
    expect(result.current.detailsTouched).toBe(true);
    expect(result.current.detailsErrors.facilityName?.[0]).toBe("Facility name must be at least 3 characters.");
  });

  it("advances to contacts once all details fields are valid (AC1)", () => {
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    act(() => {
      fillDetails(result);
    });
    act(() => {
      result.current.goToContacts();
    });

    expect(result.current.step).toBe("contacts");
  });

  it("respects the 1–3 contact bound when adding and removing (AC2)", () => {
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    act(() => {
      result.current.addContact();
      result.current.addContact();
      result.current.addContact();
    });
    expect(result.current.contacts).toHaveLength(3);

    act(() => {
      result.current.removeContact(0);
      result.current.removeContact(0);
      result.current.removeContact(0);
    });
    expect(result.current.contacts).toHaveLength(1);
  });

  it("flags a duplicate mobile number across contacts", () => {
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    act(() => {
      result.current.addContact();
      result.current.setContactField(0, "mobile", "9876500112");
      result.current.setContactField(1, "mobile", "9876500112");
    });

    expect(result.current.duplicateMobileIndex).toBe(1);
  });

  it("submits successfully once details and contacts are valid", async () => {
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    act(() => {
      fillDetails(result);
      result.current.setContactField(0, "name", "Anitha Varghese");
      result.current.setContactField(0, "designation", "Blood bank officer");
      result.current.setContactField(0, "mobile", "9876500112");
    });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({
      ok: true,
      data: expect.objectContaining({ verificationStatus: "Pending" }),
    });
  });

  it("blocks submit and reports errors when a contact is incomplete", async () => {
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    act(() => {
      fillDetails(result);
    });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({ ok: false });
    expect(result.current.contactsTouched).toBe(true);
    expect(result.current.contactErrors[0].mobile?.[0]).toBe("Enter all 10 digits of the mobile number.");
  });

  it("clears a prior submit error once a contact field is edited again", async () => {
    server.use(createFacilityValidationErrorHandler);
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    act(() => {
      fillDetails(result);
      result.current.setContactField(0, "name", "Anitha Varghese");
      result.current.setContactField(0, "designation", "Blood bank officer");
      result.current.setContactField(0, "mobile", "9876500112");
    });

    await act(async () => {
      await result.current.submit();
    });
    await waitFor(() => expect(result.current.error).not.toBeNull());

    act(() => {
      result.current.setContactField(0, "name", "Anitha V");
    });

    expect(result.current.error).toBeNull();
  });

  it("surfaces a validation error from the API", async () => {
    server.use(createFacilityValidationErrorHandler);
    const { result } = renderHook(() => useFacilityRegistration("token"), { wrapper });

    act(() => {
      fillDetails(result);
      result.current.setContactField(0, "name", "Anitha Varghese");
      result.current.setContactField(0, "designation", "Blood bank officer");
      result.current.setContactField(0, "mobile", "9876500112");
    });

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() =>
      expect(result.current.error?.message).toBe("Licence number can contain letters, numbers and hyphens only."),
    );
  });
});
