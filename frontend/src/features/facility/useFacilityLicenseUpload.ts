import { useState } from "react";

import { uploadFacilityLicense } from "../../api/facilityApi";
import { ApiError } from "../../api/httpClient";
import { validateFacilityLicenseFile } from "../../lib/validation/facilityUploadValidation";

export type FacilityLicenseUploadStatus = "empty" | "uploading" | "uploaded" | "invalid" | "networkFailed";

export function useFacilityLicenseUpload(accessToken: string | undefined, facilityId: string | null) {
  const [file, setFile] = useState<File | null>(null);
  const [status, setStatus] = useState<FacilityLicenseUploadStatus>("empty");
  const [progressPct, setProgressPct] = useState(0);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function attemptUpload(candidate: File) {
    if (!facilityId) {
      setStatus("networkFailed");
      setErrorMessage("Couldn't upload the file. Try again.");
      return;
    }
    setStatus("uploading");
    setProgressPct(0);
    setErrorMessage(null);
    try {
      await uploadFacilityLicense(accessToken, facilityId, candidate, setProgressPct);
      setStatus("uploaded");
    } catch (err) {
      setStatus("networkFailed");
      setErrorMessage(err instanceof ApiError ? err.problem.detail ?? err.message : "Upload failed. Check your connection, then retry.");
    }
  }

  async function selectFile(candidate: File) {
    setFile(candidate);
    const validationError = validateFacilityLicenseFile(candidate);
    if (validationError) {
      setStatus("invalid");
      setErrorMessage(validationError);
      return;
    }
    await attemptUpload(candidate);
  }

  async function retry() {
    if (!file) return;
    await attemptUpload(file);
  }

  return {
    file,
    status,
    progressPct,
    errorMessage,
    selectFile,
    retry,
    isUploaded: status === "uploaded",
  };
}
