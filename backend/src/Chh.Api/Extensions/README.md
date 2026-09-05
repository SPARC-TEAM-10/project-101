# Chh.Api / Extensions

`IServiceCollection` / `WebApplicationBuilder` registration helpers, one file per
concern (e.g. `AddChhPersistence`, `AddChhAuthentication`, `AddChhBackgroundJobs`,
`AddChhExternalClients`). This is the only place in `Chh.Api` allowed to reference
`Chh.Infrastructure` types.

Empty by design — `Program.cs` currently registers only controllers, Swagger,
Serilog, and ProblemDetails.
