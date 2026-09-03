import { renderHook, act } from "@testing-library/react";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";

import { AuthProvider, useAuth } from "./AuthProvider";

function wrapper({ children }: { children: ReactNode }) {
  return <AuthProvider>{children}</AuthProvider>;
}

describe("AuthProvider / useAuth", () => {
  it("starts with a null session", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.session).toBeNull();
  });

  it("setSession stores the session", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => result.current.setSession({ token: "abc", role: "Individual" }));

    expect(result.current.session).toEqual({ token: "abc", role: "Individual" });
  });

  it("clearSession resets the session to null", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => result.current.setSession({ token: "abc", role: "Individual" }));
    act(() => result.current.clearSession());

    expect(result.current.session).toBeNull();
  });

  it("throws when used outside an AuthProvider", () => {
    expect(() => renderHook(() => useAuth())).toThrow(
      "useAuth must be used within an AuthProvider",
    );
  });
});
