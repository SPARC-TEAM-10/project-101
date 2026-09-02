---

agent: ef-migration-skill
attached_to: coding-agent, database-agent
-----------------------------------------

# EF Core Migration Skill

Creates, validates, reviews, and applies Entity Framework Core migrations for .NET projects. **This skill must never apply a migration to a shared, staging, or production database without explicit developer approval.** The skill must prioritize detecting destructive or potentially data-loss operations before applying any migration.

---

## Universal Rules (apply to every caller)

* **Always inspect before modifying.** The agent must inspect the target project's `DbContext`, entity/model changes, existing migrations, connection configuration, and target framework before creating a migration.
* **Never silently modify an existing migration.** If the migration has already been committed or applied, create a new migration instead of editing the existing one.
* **Never remove migrations automatically.** `dotnet ef migrations remove` requires explicit developer approval.
* **Never apply migrations to shared/staging/production databases automatically.**
* **Always review generated migration code.** The agent must inspect the generated `Up()` and `Down()` methods before considering the migration valid.
* **Always check for destructive operations.** Flag operations such as `DropTable`, `DropColumn`, data truncation, destructive type changes, or foreign-key changes that may cause data loss.
* **Never drop tables or columns to resolve migration errors without explicit developer approval.**
* **Never delete existing migration history to resolve conflicts.**
* **Never use `EnsureCreated()` as a replacement for EF Core migrations in a migration-based application.**
* **Always report the migration name and affected database objects.**
* If a migration is potentially destructive, stop and ask the developer for approval before applying it.

---

## Trigger Point

The calling agent MUST invoke this skill when any of the following is required:

* A new entity or property has been added or removed.
* An existing entity or property has been changed.
* A relationship or foreign key has changed.
* An index or constraint has changed.
* A database schema change is required.
* A migration needs to be created.
* Existing migrations need to be reviewed.
* A migration needs to be validated.
* A migration needs to be applied to a local development database.
* A migration conflict or failure needs to be investigated.
* The developer explicitly asks to add, remove, update, validate, or apply an EF Core migration.

The skill MUST NOT be invoked merely for normal application code changes that do not affect the EF Core model or database schema.

---

## Input

| Parameter              | Type    | Required | Description                                                 |
| ---------------------- | ------- | -------- | ----------------------------------------------------------- |
| `ProjectPath`          | string  | Yes      | Path to the .NET project containing the EF Core `DbContext` |
| `DbContextName`        | string  | No       | Specific `DbContext` to use when multiple contexts exist    |
| `MigrationName`        | string  | Yes      | Name of the migration to create                             |
| `StartupProject`       | string  | No       | Startup project used by `dotnet ef`                         |
| `TargetDatabase`       | string  | No       | Database/environment targeted by the migration              |
| `ApplyMigration`       | boolean | No       | Whether the migration should be applied after validation    |
| `ApprovalRequired`     | boolean | No       | Whether developer approval is required before applying      |
| `ConnectionStringName` | string  | No       | Configuration key containing the database connection string |

---

## Pre-Migration Checks

Before creating a migration, the skill MUST:

1. Verify that the .NET SDK is installed.
2. Verify the project's target framework.
3. Verify that EF Core tooling is available.
4. Locate the `DbContext`.
5. Verify that the project builds successfully.
6. Inspect the current migration list.
7. Check for pending model changes.
8. Check whether there are multiple `DbContext` instances.
9. Verify that the correct startup project is being used.
10. Verify that the migration name is meaningful and unique.

Example:

```bash
dotnet build

dotnet ef migrations list \
  --project "<ProjectPath>" \
  --startup-project "<StartupProject>" \
  --context "<DbContextName>"
```

---

## Create Migration

After the pre-migration checks pass:

```bash
dotnet ef migrations add "<MigrationName>" \
  --project "<ProjectPath>" \
  --startup-project "<StartupProject>" \
  --context "<DbContextName>"
```

If `DbContextName` or `StartupProject` is not required, omit the corresponding option.

---

## Migration Review

After creation, inspect the generated migration.

The skill MUST review:

* `Up()` method
* `Down()` method
* Added tables
* Removed tables
* Added columns
* Removed columns
* Column type changes
* Nullable/non-nullable changes
* Default values
* Primary keys
* Foreign keys
* Indexes
* Unique constraints
* Rename operations
* Data migrations
* Seed-data changes

The skill must classify operations as:

| Risk      | Examples                                             | Action                    |
| --------- | ---------------------------------------------------- | ------------------------- |
| 🟢 Low    | Add nullable column, add index                       | Continue                  |
| 🟡 Medium | Add non-nullable column, alter column type           | Warn developer            |
| 🔴 High   | Drop column, drop table, destructive type conversion | Stop and request approval |

---

## Destructive Migration Rules

The skill MUST stop and request developer approval when the migration contains operations such as:

```csharp
migrationBuilder.DropTable(...);

migrationBuilder.DropColumn(...);

migrationBuilder.Sql(...); // when potentially destructive

migrationBuilder.AlterColumn(...); // when data conversion may be destructive
```

The skill must explain:

1. What database object will be affected.
2. What data could be lost.
3. Whether the operation can be made backward compatible.
4. A safer alternative, if available.

For example:

> Migration `Remove_Company_Seed` attempts to remove a table referenced by an existing foreign key. The migration should not be applied until the dependency and migration order are reviewed.

---

## Migration Validation

After creating the migration, run:

```bash
dotnet ef migrations has-pending-model-changes \
  --project "<ProjectPath>" \
  --startup-project "<StartupProject>" \
  --context "<DbContextName>"
```

Then verify the generated SQL:

```bash
dotnet ef migrations script \
  --project "<ProjectPath>" \
  --startup-project "<StartupProject>" \
  --context "<DbContextName>"
```

The generated SQL must be reviewed for:

* Unexpected `DROP`
* Unexpected `DELETE`
* Unexpected `ALTER`
* Foreign-key ordering problems
* Duplicate constraints
* Duplicate indexes
* Invalid SQL
* Unexpected data modifications

---

## Apply Migration

Applying a migration is a separate operation from creating one.

For a local development database:

```bash
dotnet ef database update \
  --project "<ProjectPath>" \
  --startup-project "<StartupProject>" \
  --context "<DbContextName>"
```

The skill MUST NOT execute this command against staging or production unless the developer explicitly approves it.

Before applying:

* Confirm the target database.
* Confirm the migration to be applied.
* Confirm that the database backup/recovery process is appropriate when required.
* Confirm that no destructive operation is present without approval.

---

## Migration Status

The skill should provide:

* Current migration
* Pending migrations
* Latest migration
* Whether the database is up to date
* Whether model changes are pending

Example:

```bash
dotnet ef migrations list \
  --project "<ProjectPath>" \
  --startup-project "<StartupProject>" \
  --context "<DbContextName>"
```

---

## Migration Removal

Removing the latest migration requires explicit developer approval.

Command:

```bash
dotnet ef migrations remove \
  --project "<ProjectPath>" \
  --startup-project "<StartupProject>" \
  --context "<DbContextName>"
```

The skill MUST verify that:

* The migration is the latest migration.
* The migration has not already been applied to a shared/staging/production database.
* No other migration depends on it.

If the migration has already been applied to a shared database, do not remove it. Create a corrective migration instead.

---

## Migration Conflict Handling

When migration conflicts occur:

1. Do not delete the migration history.
2. Do not drop tables automatically.
3. Inspect the migration chain.
4. Inspect the database migration history table.
5. Identify the migration that introduced the conflict.
6. Check foreign-key dependencies.
7. Check whether the migration has already been applied.
8. Propose the safest corrective migration.
9. Ask for developer approval before destructive remediation.

For PostgreSQL, inspect:

```sql
SELECT *
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
```

For SQL Server, inspect:

```sql
SELECT *
FROM "__EFMigrationsHistory"
ORDER BY MigrationId;
```

---

## Rollback Rules

The skill MUST NOT automatically roll back a shared/staging/production database.

If rollback is explicitly approved:

```bash
dotnet ef database update "<PreviousMigration>" \
  --project "<ProjectPath>" \
  --startup-project "<StartupProject>" \
  --context "<DbContextName>"
```

Before rollback, inspect the `Down()` method and determine whether it causes data loss.

---

## Output

| Field                | Description                                                         |
| -------------------- | ------------------------------------------------------------------- |
| `Status`             | `Created` | `Validated` | `Applied` | `Failed` | `ApprovalRequired` |
| `MigrationName`      | Name of the migration                                               |
| `PreviousMigration`  | Migration that preceded the new migration                           |
| `AffectedObjects`    | Tables, columns, indexes, constraints, and relationships affected   |
| `DestructiveChanges` | List of potentially destructive operations                          |
| `SqlScript`          | Generated SQL script when requested                                 |
| `Applied`            | Whether the migration was applied                                   |
| `Error`              | Populated only when the operation fails                             |

---

## Error Handling

* `dotnet build` fails → `Status: Failed`; do not create the migration.
* EF tooling is unavailable → `Status: Failed`; report the required tooling.
* `DbContext` cannot be created → `Status: Failed`; report the design-time creation error.
* Multiple `DbContext` instances exist and no context was specified → `Status: Failed`; request the correct context.
* Migration creation fails → `Status: Failed`; report the EF error.
* Generated migration contains destructive operations → `Status: ApprovalRequired`; do not apply.
* Database update fails → `Status: Failed`; report the database/EF error and do not retry automatically.
* Migration already exists → inspect existing migration; do not overwrite it.
* Migration has already been applied → do not remove or modify it; create a corrective migration if necessary.
* Foreign-key conflict occurs → stop, inspect dependencies, and report the conflict.
* Database is unavailable → `Status: Failed`; do not repeatedly retry.
* `__EFMigrationsHistory` is inconsistent with the migration files → stop and request developer review.

---

## Required Tools

| Tool Purpose                   | Tool               |
| ------------------------------ | ------------------ |
| Build project                  | Bash               |
| Create migration               | Bash               |
| List migrations                | Bash               |
| Generate migration SQL         | Bash               |
| Apply migration                | Bash               |
| Inspect migration files        | File system / Bash |
| Inspect database when required | Database CLI       |
| Review generated code          | File system / Bash |

---

## Safety Requirements

The skill MUST follow these rules:

* Never drop a table to make a migration succeed.
* Never drop a column automatically.
* Never delete rows automatically as part of migration troubleshooting.
* Never modify `__EFMigrationsHistory` manually unless explicitly directed by the developer.
* Never edit an already-applied migration.
* Never use `EnsureCreated()` to bypass migrations.
* Never reset the database to solve a migration problem unless explicitly approved.
* Prefer additive and backward-compatible migrations.
* Prefer a new corrective migration over modifying migration history.
* Always show destructive changes to the developer before applying them.
* Always report the final migration name and status.

---
