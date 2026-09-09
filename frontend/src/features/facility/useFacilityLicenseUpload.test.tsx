import { act, renderHook, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";

import { useFacilityLicenseUpload } from "./useFacilityLicenseUpload";
import { installFakeXhr, networkFailure, progressThenSuccess, respondWithError } from "../../../tests/fakeXhr";

const FACILITY_ID = "22222222-2222-2222-2222-222222222222";

function makeFile(name: string, type: string, sizeBytes: number): File {
  return new File([new Uint8Array(sizeBytes)], name, { type });
}

const successResponse = {
  id: FACILITY_ID,
  facilityName: "Kochi Metro Hospital",
  category: "Hospital",
  licenseNumber: "KL-HOSP-448120",
  address: "4th Block, Marine Drive, Ernakulam, Kochi 682031",
  contacts: [],
  verificationStatus: "Pending",
  licenseDocumentUrl: "https://blob.example/kochi-metro-licence.pdf",
  createdAtUtc: "2026-09-07T00:00:00.000Z",
};

let restoreXhr: (() => void) | null = null;

afterEach(() => {
  restoreXhr?.();
  restoreXhr = null;
});

describe("useFacilityLicenseUpload", () => {
  it("starts empty with no file selected", () => {
    const { result } = renderHook(() => useFacilityLicenseUpload("token", FACILITY_ID));

    expect(result.current.status).toBe("empty");
    expect(result.current.file).toBeNull();
  });

  it("rejects a non-PDF/JPG/PNG file client-side without attempting a network call", async () => {
    const { result } = renderHook(() => useFacilityLicenseUpload("token", FACILITY_ID));

    await act(async () => {
      await result.current.selectFile(makeFile("licence.docx", "application/msword", 1024));
    });

    expect(result.current.status).toBe("invalid");
    expect(result.current.errorMessage).toBe("Invalid file format. Please upload PDF or Image.");
  });

  it("rejects a file over 5MB client-side without attempting a network call", async () => {
    const { result } = renderHook(() => useFacilityLicenseUpload("token", FACILITY_ID));

    await act(async () => {
      await result.current.selectFile(makeFile("licence.pdf", "application/pdf", 6 * 1024 * 1024));
    });

    expect(result.current.status).toBe("invalid");
    expect(result.current.errorMessage).toBe("File too large. Maximum size is 5MB.");
  });

  it("accepts a valid file, reports progress, and reaches uploaded on success", async () => {
    restoreXhr = installFakeXhr(progressThenSuccess(successResponse));
    const { result } = renderHook(() => useFacilityLicenseUpload("token", FACILITY_ID));

    await act(async () => {
      await result.current.selectFile(makeFile("licence.pdf", "application/pdf", 1024));
    });

    expect(result.current.isUploaded).toBe(true);
    expect(result.current.status).toBe("uploaded");
    expect(result.current.progressPct).toBe(100);
  });

  it("surfaces a validation error from the API without crashing", async () => {
    restoreXhr = installFakeXhr(respondWithError(422, "Invalid file format. Please upload PDF or Image."));
    const { result } = renderHook(() => useFacilityLicenseUpload("token", FACILITY_ID));

    await act(async () => {
      await result.current.selectFile(makeFile("licence.pdf", "application/pdf", 1024));
    });

    expect(result.current.status).toBe("networkFailed");
    expect(result.current.errorMessage).toBe("Invalid file format. Please upload PDF or Image.");
  });

  it("retry() re-attempts the same file after a network failure", async () => {
    restoreXhr = installFakeXhr(networkFailure());
    const { result } = renderHook(() => useFacilityLicenseUpload("token", FACILITY_ID));

    await act(async () => {
      await result.current.selectFile(makeFile("licence.pdf", "application/pdf", 1024));
    });
    expect(result.current.status).toBe("networkFailed");

    restoreXhr?.();
    restoreXhr = installFakeXhr(progressThenSuccess(successResponse));
    await act(async () => {
      await result.current.retry();
    });

    await waitFor(() => expect(result.current.status).toBe("uploaded"));
  });

  it("does nothing if retry() is called with no file selected yet", async () => {
    const { result } = renderHook(() => useFacilityLicenseUpload("token", FACILITY_ID));

    await act(async () => {
      await result.current.retry();
    });

    expect(result.current.status).toBe("empty");
  });
});
