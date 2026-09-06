# Chh.Application / Abstractions

Shared cross-cutting abstractions (e.g. `IDateTimeProvider`, `PagedResponse<T>`,
domain exception base types mapped to RFC 7807 ProblemDetails).

`ChhException` is the shared base; `ChhValidationException` (carries per-field
failures) gets its own file. Simple, single-message domain exceptions don't —
group them by feature area in one file (e.g. `OtpExceptions.cs`,
`IndividualRegistrationExceptions.cs`) rather than one file per class. Each
stays its own *type* even when grouped — `ProblemDetailsServiceCollectionExtensions`
maps types to HTTP statuses via Hellang's type-based dispatch, so merging the
classes themselves would break that.
