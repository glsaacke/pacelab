# PaceLab — Copilot Instructions

## Purpose

- Guide Copilot to generate code, tests, and suggestions that match PaceLab's architecture, style, and safety rules.
- Ensure frontend and backend contributions follow the project's conventions and are reviewable, testable, and secure.

## How to use this file

- When editing files under `api/` follow the "Backend" section below.
- When editing files under `client/` follow the "Frontend" section below.
- If an edit touches both areas, apply both rule sets and prefer the most restrictive guidance.

---

## Quick repo map

- Backend: `api/` — .NET API
- Frontend: `client/` — SvelteKit + TypeScript

---

## Global rules (apply everywhere)

- Prefer existing project patterns and types.
- Keep changes small and focused; prefer atomic, reviewable diffs.
- Don’t add secrets or environment values in source code. Use environment variables and CI secrets.
- If unsure about a design decision, add a TODO with rationale and request maintainer review.

---

## Backend (api/) — guidance

Stack and architecture
- .NET 8, EF Core, controller/service/repository pattern. Use DI and register services in `Program.cs`.
- DbContext: `api/src/data/PaceLabContext.cs`. Keep EF entities and migrations consistent with existing mappings.

Coding conventions
- Use PascalCase for public types and members; camelCase for private fields and parameters.
- Async methods should end with `Async` and accept `CancellationToken` when applicable.
- Controllers should be thin: validation, mapping, and response codes only.
- Put business logic in services and data access in repositories (use the existing `I*Repository` interfaces).
- Return DTOs from controllers. Avoid returning EF entities directly.

Error handling and logging
- Use typed exceptions for expected errors and map them to HTTP responses in controllers.
- Use `ILogger<T>` for structured logs.

API and external integrations
- Centralize Strava/HTTP client logic, add retry/backoff and rate-limit handling, and encapsulate mapping into adapters.

Backend PR checklist
- Follows controller→service→repository separation.
- No hard-coded connection strings or secrets.
- Includes/updates unit tests for changed behavior.
- Adds DB migrations only when necessary and documents deployment steps.

---

## Frontend (client/) — guidance

Stack and architecture
- SvelteKit + TypeScript. Keep code in `client/src/`. Shared utilities in `client/src/lib`.

Coding conventions
- Use TypeScript with strict types. Explicitly type public props, stores, and utilities.
- Prefer small, focused components and separate UI from state logic.
- Follow SvelteKit routing and load conventions.

Accessibility & UX
- Add accessible attributes (aria-label, roles) for interactive elements and ensure keyboard navigation.

Frontend PR checklist
- Runs type checks and linting.
- No secrets embedded in client code.
- Include visual or snapshot tests for UI changes when applicable.

---

## File-path based directives (how Copilot should pick rules)

- If the file path starts with `api/` => apply the Backend rules above.
- If the file path starts with `client/` => apply the Frontend rules above.
- For other repo files (docs, CI, workflows) follow the "Global rules" section.

---

## When unsure

- Prefer adding a TODO and asking maintainers over guessing implementation details that affect design or security.
