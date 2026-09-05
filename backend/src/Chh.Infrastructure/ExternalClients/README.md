# Chh.Infrastructure / ExternalClients

`IHttpClientFactory` typed clients for the SMS gateway, Firebase Cloud Messaging,
and the maps/geo API. All API keys come from Azure Key Vault, never from
`appsettings.json` (`.claude/rules/api-standards.md` §5).

The SMS provider is undecided (open PRD question) — implement behind
`ISmsGatewayClient` so it stays swappable.

Empty by design.
