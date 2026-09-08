// Reads the JWT's `sub` claim (the mobile number — see backend's JwtTokenGenerator.cs) purely to
// populate a request field the app already implicitly knows; every endpoint that actually needs
// the mobile number still verifies the token server-side, so this is not a trust boundary.
export function getMobileNumberFromToken(token: string): string | null {
  try {
    const payload = token.split(".")[1];
    if (!payload) {
      return null;
    }
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const claims = JSON.parse(atob(base64)) as { sub?: string };
    return claims.sub ?? null;
  } catch {
    return null;
  }
}
