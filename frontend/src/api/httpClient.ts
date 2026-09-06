export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails;

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `Request failed with status ${status}`);
    this.status = status;
    this.problem = problem;
  }
}

// VITE_API_BASE_URL points at the deployed backend origin (e.g. "http://13.63.179.146:5000")
// in production/preview — see frontend/CLAUDE.md's Deployment section. Falls back to a relative
// path when unset (local dev against a same-origin proxy, or MSW-mocked tests).
const API_BASE_URL = `${import.meta.env.VITE_API_BASE_URL ?? ""}/api/v1`;

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers,
    },
  });

  if (!res.ok) {
    const problem = (await res.json().catch(() => ({}))) as ProblemDetails;
    throw new ApiError(res.status, problem);
  }

  return res.json() as Promise<T>;
}
