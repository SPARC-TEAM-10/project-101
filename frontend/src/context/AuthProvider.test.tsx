import { renderHook, act } from "@testing-library/react";
import type { ReactNode } from "react";
import { beforeEach, describe, expect, it } from "vitest";

import { AuthProvider, useAuth } from "./AuthProvider";

const FUTURE = new Date(Date.now() + 60 * 60 * 1000).toISOString();
const STORAGE_KEY = "chh.auth.session";

function wrapper({ children }: { children: ReactNode }) {
  return <AuthProvider>{children}</AuthProvider>;
}

beforeEach(() => {
  localStorage.clear();
});

describe("AuthProvider / useAuth", () => {
  it("starts with a null session when localStorage is empty", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.session).toBeNull();
  });

  it("setSession stores the session and persists it to localStorage", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => result.current.setSession({ token: "abc", role: "Individual", expiresAtUtc: FUTURE }));

    expect(result.current.session).toEqual({ token: "abc", role: "Individual", expiresAtUtc: FUTURE });
    expect(JSON.parse(localStorage.getItem(STORAGE_KEY)!)).toEqual({
      token: "abc",
      role: "Individual",
      expiresAtUtc: FUTURE,
    });
  });

  it("clearSession resets the session to null and removes it from localStorage", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => result.current.setSession({ token: "abc", role: "Individual", expiresAtUtc: FUTURE }));
    act(() => result.current.clearSession());

    expect(result.current.session).toBeNull();
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it("rehydrates a session already in localStorage on mount (survives a page refresh)", () => {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ token: "persisted", role: "Guest", expiresAtUtc: FUTURE }),
    );

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.session).toEqual({ token: "persisted", role: "Guest", expiresAtUtc: FUTURE });
  });

  it("falls back to a null session when localStorage holds corrupted JSON", () => {
    localStorage.setItem(STORAGE_KEY, "{not valid json");

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.session).toBeNull();
  });

  it("falls back to a null session when the stored value is missing required fields", () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ token: "abc" }));

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.session).toBeNull();
  });

  it("throws when used outside an AuthProvider", () => {
    expect(() => renderHook(() => useAuth())).toThrow(
      "useAuth must be used within an AuthProvider",
    );
  });
});
