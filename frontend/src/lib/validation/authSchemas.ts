import { z } from "zod";

export const mobileNumberSchema = z
  .string()
  .regex(/^[0-9]{10}$/, "Please enter a valid 10-digit mobile number");
