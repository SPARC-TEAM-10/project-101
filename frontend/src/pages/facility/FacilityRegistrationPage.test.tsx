import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";

import { FacilityRegistrationPage } from "./FacilityRegistrationPage";
import { ToastProvider } from "../../context/ToastProvider";
import { server } from "../../../tests/setup";
import { createFacilityNetworkErrorHandler } from "../../../tests/msw/handlers";
import { installFakeXhr, networkFailure, progressThenSuccess, respondWithError } from "../../../tests/fakeXhr";

const mockUseAuth = vi.fn();

vi.mock("../../context/AuthProvider", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../../context/AuthProvider")>();
  return {
    ...actual,
    useAuth: () => mockUseAuth(),
  };
});

function renderPage() {
  mockUseAuth.mockReturnValue({
    session: null,
    setSession: vi.fn(),
    clearSession: vi.fn(),
  });

  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <MemoryRouter initialEntries={[{ pathname: "/facility/register", state: { category: "Hospital" } }]}>
          <Routes>
            <Route path="/facility/register" element={<FacilityRegistrationPage />} />
            <Route path="/register" element={<div>Role Selection</div>} />
            <Route path="/" element={<div>Home</div>} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

function fillDetailsStep() {
  fireEvent.change(screen.getByLabelText(/facility name/i), { target: { value: "Kochi Metro Hospital" } });
  fireEvent.click(screen.getByLabelText(/sub-category/i));
  fireEvent.click(screen.getByRole("option", { name: "Government" }));
  fireEvent.change(screen.getByLabelText(/licence number/i), { target: { value: "KL-HOSP-448120" } });
  fireEvent.change(screen.getByLabelText(/address/i), {
    target: { value: "4th Block, Marine Drive, Ernakulam, Kochi 682031" },
  });
  fireEvent.click(screen.getByRole("button", { name: /continue to contacts/i }));
}

async function completeContactsStep() {
  fireEvent.change(screen.getByLabelText(/^name/i), { target: { value: "Anitha Varghese" } });
  fireEvent.change(screen.getByLabelText(/designation/i), { target: { value: "Blood bank officer" } });
  fireEvent.change(screen.getByLabelText(/mobile/i), { target: { value: "9876500112" } });
  fireEvent.click(screen.getByRole("button", { name: /continue to licence/i }));
  await waitFor(() => expect(screen.getByText("Step 3 of 3")).toBeInTheDocument());
}

function makeFile(name: string, type: string, sizeBytes: number): File {
  return new File([new Uint8Array(sizeBytes)], name, { type });
}

const uploadSuccessResponse = {
  id: "22222222-2222-2222-2222-222222222222",
  facilityName: "Kochi Metro Hospital",
  category: "Hospital",
  licenseNumber: "KL-HOSP-448120",
  address: "4th Block, Marine Drive, Ernakulam, Kochi 682031",
  contacts: [],
  verificationStatus: "Pending",
  licenseDocumentUrl: "https://blob.example/kochi-metro-licence.pdf",
  createdAtUtc: "2026-09-07T00:00:00.000Z",
};

// The upload path uses XMLHttpRequest directly (not MSW-intercepted fetch) — see
// tests/fakeXhr.ts's doc comment for why MSW can't be used for this specific call.
let restoreXhr: (() => void) | null = null;

afterEach(() => {
  restoreXhr?.();
  restoreXhr = null;
});

describe("FacilityRegistrationPage", () => {
  it("renders Step 1 with all mandatory fields, unguarded (no auth required)", () => {
    renderPage();

    expect(screen.getByText("Facility details")).toBeInTheDocument();
    expect(screen.getByLabelText(/facility name/i)).toBeInTheDocument();
    expect(screen.getByText("Category")).toBeInTheDocument();
    expect(screen.getByText("Hospital")).toBeInTheDocument();
    expect(screen.getByLabelText(/sub-category/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/licence number/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/address/i)).toBeInTheDocument();
  });

  it("the header back button returns to /register (Role Selection), not the landing page", () => {
    renderPage();

    fireEvent.click(screen.getByLabelText(/back to account type/i));

    expect(screen.getByText("Role Selection")).toBeInTheDocument();
  });

  it("redirects to /register when no category was chosen (direct visit, no route state)", () => {
    mockUseAuth.mockReturnValue({ session: null, setSession: vi.fn(), clearSession: vi.fn() });
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });

    render(
      <QueryClientProvider client={queryClient}>
        <ToastProvider>
          <MemoryRouter initialEntries={["/facility/register"]}>
            <Routes>
              <Route path="/facility/register" element={<FacilityRegistrationPage />} />
              <Route path="/register" element={<div>Role Selection</div>} />
            </Routes>
          </MemoryRouter>
        </ToastProvider>
      </QueryClientProvider>,
    );

    expect(screen.getByText("Role Selection")).toBeInTheDocument();
  });

  it("shows validation hints and stays on Step 1 when Continue is clicked with an empty form", () => {
    renderPage();

    fireEvent.click(screen.getByRole("button", { name: /continue to contacts/i }));

    expect(screen.getByText("Facility name must be at least 3 characters.")).toBeInTheDocument();
    expect(screen.getByText("Facility details")).toBeInTheDocument();
  });

  it("advances to Step 2 (Contacts) once Step 1 is valid", () => {
    renderPage();

    fillDetailsStep();

    expect(screen.getByText("Step 2 of 3")).toBeInTheDocument();
    expect(screen.getByLabelText(/^name/i)).toBeInTheDocument();
  });

  it("adds up to 3 contacts then disables the add button", () => {
    renderPage();
    fillDetailsStep();

    const addButton = screen.getByRole("button", { name: /add another contact/i });
    fireEvent.click(addButton);
    fireEvent.click(screen.getByRole("button", { name: /add another contact/i }));

    expect(screen.getByRole("button", { name: /three contacts is the maximum/i })).toBeDisabled();
  });

  it("shows a duplicate-mobile error when two contacts share a number", () => {
    renderPage();
    fillDetailsStep();

    fireEvent.click(screen.getByRole("button", { name: /add another contact/i }));
    const mobileInputs = screen.getAllByLabelText(/mobile/i);
    fireEvent.change(mobileInputs[0], { target: { value: "9876500112" } });
    fireEvent.change(mobileInputs[1], { target: { value: "9876500112" } });
    fireEvent.click(screen.getByRole("button", { name: /continue to licence/i }));

    expect(screen.getByText("Contact 1 already uses this number. Enter a different one.")).toBeInTheDocument();
  });

  it("returns to Step 1 with prior values retained when Back is clicked", () => {
    renderPage();
    fillDetailsStep();

    fireEvent.click(screen.getByRole("button", { name: /^back$/i }));

    expect(screen.getByDisplayValue("Kochi Metro Hospital")).toBeInTheDocument();
  });

  it("advances to Step 3 (Upload) once Step 2 submits successfully, no terminal toast", async () => {
    renderPage();
    fillDetailsStep();
    await completeContactsStep();

    expect(screen.getByText("Licence document")).toBeInTheDocument();
    expect(screen.queryByText("Facility details saved. Licence upload coming soon.")).not.toBeInTheDocument();
  });

  it("shows a toast on network failure during facility creation and retains the form", async () => {
    server.use(createFacilityNetworkErrorHandler);
    renderPage();
    fillDetailsStep();

    fireEvent.change(screen.getByLabelText(/^name/i), { target: { value: "Anitha Varghese" } });
    fireEvent.change(screen.getByLabelText(/designation/i), { target: { value: "Blood bank officer" } });
    fireEvent.change(screen.getByLabelText(/mobile/i), { target: { value: "9876500112" } });
    fireEvent.click(screen.getByRole("button", { name: /continue to licence/i }));

    await waitFor(() => expect(screen.getByText("Couldn't save the facility. Try again.")).toBeInTheDocument());
    expect(screen.getByDisplayValue("Anitha Varghese")).toBeInTheDocument();
  });

  it("shows the AC2 error inline for an invalid file type, without attempting an upload", async () => {
    renderPage();
    fillDetailsStep();
    await completeContactsStep();

    const input = document.getElementById("license-file-input") as HTMLInputElement;
    fireEvent.change(input, { target: { files: [makeFile("licence.docx", "application/msword", 1024)] } });

    await waitFor(() =>
      expect(screen.getByText("Invalid file format. Please upload PDF or Image.")).toBeInTheDocument(),
    );
  });

  it("shows the AC3 error inline for an oversized file, without attempting an upload", async () => {
    renderPage();
    fillDetailsStep();
    await completeContactsStep();

    const input = document.getElementById("license-file-input") as HTMLInputElement;
    fireEvent.change(input, { target: { files: [makeFile("licence.pdf", "application/pdf", 6 * 1024 * 1024)] } });

    await waitFor(() => expect(screen.getByText("File too large. Maximum size is 5MB.")).toBeInTheDocument());
  });

  it("uploads a valid file and enables Submit for verification; submitting shows the final success toast", async () => {
    restoreXhr = installFakeXhr(progressThenSuccess(uploadSuccessResponse));
    renderPage();
    fillDetailsStep();
    await completeContactsStep();

    const input = document.getElementById("license-file-input") as HTMLInputElement;
    fireEvent.change(input, { target: { files: [makeFile("licence.pdf", "application/pdf", 1024)] } });

    await waitFor(() => expect(screen.getByText("Uploaded")).toBeInTheDocument());
    const submitButton = screen.getByRole("button", { name: /submit for verification/i });
    expect(submitButton).toBeEnabled();

    fireEvent.click(submitButton);

    await waitFor(() => expect(screen.getByText("Submitted for verification.")).toBeInTheDocument());
  });

  it("shows a retry option on upload network failure, and retry succeeds once the network recovers", async () => {
    restoreXhr = installFakeXhr(networkFailure());
    renderPage();
    fillDetailsStep();
    await completeContactsStep();

    const input = document.getElementById("license-file-input") as HTMLInputElement;
    fireEvent.change(input, { target: { files: [makeFile("licence.pdf", "application/pdf", 1024)] } });

    const retryButton = await screen.findByRole("button", { name: /retry upload/i });
    restoreXhr();
    restoreXhr = installFakeXhr(progressThenSuccess(uploadSuccessResponse));
    fireEvent.click(retryButton);

    await waitFor(() => expect(screen.getByText("Uploaded")).toBeInTheDocument());
  });

  it("surfaces a server-side validation error from the upload endpoint", async () => {
    restoreXhr = installFakeXhr(respondWithError(422, "Invalid file format. Please upload PDF or Image."));
    renderPage();
    fillDetailsStep();
    await completeContactsStep();

    const input = document.getElementById("license-file-input") as HTMLInputElement;
    fireEvent.change(input, { target: { files: [makeFile("licence.pdf", "application/pdf", 1024)] } });

    await waitFor(() =>
      expect(screen.getByText("Invalid file format. Please upload PDF or Image.")).toBeInTheDocument(),
    );
  });
});
