# Chh.Application / Dtos

Request/response types crossing the wire.

**Contract-first (hard rule, `.claude/rules/api-standards.md` §3):** DTOs in this
folder are *generated* from `contracts/chh-api.v1.yaml`. Hand-editing a generated
DTO is a Critical review finding.

`contracts/chh-api.v1.yaml` does not exist in the repo yet — it is a prerequisite
for any controller or DTO work. This folder stays empty until it lands.
