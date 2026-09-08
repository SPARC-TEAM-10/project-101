import { vi } from "vitest";

// Test double for XMLHttpRequest, used only for facilityApi.uploadFacilityLicense's upload path.
//
// Why not MSW: @mswjs/interceptors' XHR interceptor hangs indefinitely under jsdom when the
// request body is a FormData containing any Blob/File — reproduced with content, an empty File,
// and a plain Blob; a FormData with no file entry works fine. This is a known interop gap between
// jsdom's Blob/File implementation and the interceptor's body-reading logic, not a bug in the
// application code (a real browser has no such issue). Every other facilityApi call goes through
// apiFetch and is still tested via real MSW handlers — only the raw-XHR upload path uses this.

export type FakeXhrScript = (xhr: FakeXMLHttpRequest) => void;

export class FakeXMLHttpRequest {
  method = "";
  url = "";
  status = 0;
  responseText = "";
  requestHeaders: Record<string, string> = {};
  upload: { onprogress: ((event: { lengthComputable: boolean; loaded: number; total: number }) => void) | null } = {
    onprogress: null,
  };
  onload: (() => void) | null = null;
  onerror: (() => void) | null = null;

  private script: FakeXhrScript;

  constructor(script: FakeXhrScript) {
    this.script = script;
  }

  open(method: string, url: string) {
    this.method = method;
    this.url = url;
  }

  setRequestHeader(name: string, value: string) {
    this.requestHeaders[name] = value;
  }

  send(_body: FormData) {
    // Deferred so callers can attach onload/onerror/upload.onprogress before the script runs,
    // matching real XHR's asynchronous behavior.
    queueMicrotask(() => this.script(this));
  }
}

/**
 * Installs a FakeXMLHttpRequest as the global XMLHttpRequest for the duration of one test. Uses
 * vi.stubGlobal — jsdom defines XMLHttpRequest as a non-writable property, so a plain assignment
 * (globalThis.XMLHttpRequest = ...) throws "Cannot assign to read only property" in strict mode.
 * Call the returned restore() (or vi.unstubAllGlobals()) at the end of the test/in afterEach.
 */
export function installFakeXhr(script: FakeXhrScript): () => void {
  vi.stubGlobal(
    "XMLHttpRequest",
    class extends FakeXMLHttpRequest {
      constructor() {
        super(script);
      }
    },
  );
  return () => vi.unstubAllGlobals();
}

export function progressThenSuccess(responseBody: unknown): FakeXhrScript {
  return (xhr) => {
    xhr.upload.onprogress?.({ lengthComputable: true, loaded: 512, total: 1024 });
    xhr.upload.onprogress?.({ lengthComputable: true, loaded: 1024, total: 1024 });
    xhr.status = 200;
    xhr.responseText = JSON.stringify(responseBody);
    xhr.onload?.();
  };
}

export function respondWithError(status: number, problemDetail: string): FakeXhrScript {
  return (xhr) => {
    xhr.status = status;
    xhr.responseText = JSON.stringify({ title: "Error", status, detail: problemDetail });
    xhr.onload?.();
  };
}

export function networkFailure(): FakeXhrScript {
  return (xhr) => {
    xhr.onerror?.();
  };
}
