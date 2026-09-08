import { describe, expect, it } from "vitest";

import { getMobileNumberFromToken } from "./authToken";

function makeToken(payload: object): string {
  const base64 = btoa(JSON.stringify(payload)).replace(/\+/g, "-").replace(/\//g, "_");
  return `header.${base64}.signature`;
}

describe("getMobileNumberFromToken", () => {
  it("extracts the sub claim", () => {
    expect(getMobileNumberFromToken(makeToken({ sub: "9876543210" }))).toBe("9876543210");
  });

  it("returns null when the token has no payload segment", () => {
    expect(getMobileNumberFromToken("not-a-jwt")).toBeNull();
  });

  it("returns null when the payload isn't valid JSON", () => {
    expect(getMobileNumberFromToken("header.not-base64-json.signature")).toBeNull();
  });

  it("returns null when the sub claim is absent", () => {
    expect(getMobileNumberFromToken(makeToken({ role: "Guest" }))).toBeNull();
  });
});
