import { describe, expect, it } from "vitest";
import { http, HttpResponse } from "msw";

import { apiFetch, ApiError } from "./httpClient";
import { server } from "../../tests/setup";

describe("apiFetch", () => {
  it("returns the parsed JSON body on a successful response", async () => {
    server.use(
      http.get("/api/v1/ping", () => HttpResponse.json({ ok: true })),
    );

    const result = await apiFetch<{ ok: boolean }>("/ping");

    expect(result).toEqual({ ok: true });
  });

  it("throws a typed ApiError with the parsed problem details on a non-OK response", async () => {
    server.use(
      http.get("/api/v1/ping", () =>
        HttpResponse.json({ title: "Bad request", status: 400, detail: "nope" }, { status: 400 }),
      ),
    );

    await expect(apiFetch("/ping")).rejects.toMatchObject({
      status: 400,
      problem: { detail: "nope" },
    });
  });

  it("falls back to an empty problem object when the error body isn't valid JSON", async () => {
    server.use(http.get("/api/v1/ping", () => new HttpResponse("not json", { status: 500 })));

    await expect(apiFetch("/ping")).rejects.toBeInstanceOf(ApiError);
    await expect(apiFetch("/ping")).rejects.toMatchObject({ status: 500, problem: {} });
  });
});
