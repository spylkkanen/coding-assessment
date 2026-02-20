# Design: Core XML-to-JSON Transformation Pipeline

**Spec ID:** 000
**Feature Spec:** [specs/features/000-core-transformation-pipeline.md](../features/000-core-transformation-pipeline.md)
**Status:** Implemented
**Author:** Tech Lead
**Date:** 2026-02-19

## Approach

.NET Worker Service with a background poller that watches blob storage and runs files through a linear pipeline of services.

## Architecture

```
Blob Storage (input/)
    │
    ▼
Polling Worker (BackgroundService)
    │
    ▼
Transformation Pipeline
    ├── Parse XML → domain model
    ├── Validate → collect errors
    ├── Map fields → enrich data
    └── Transform → JSON output
    │
    ▼
Blob Storage (output/ or processed/ or failed/)
```

## Key Decisions

- **Pipeline pattern** — each stage is a separate service behind an interface, composed via DI
- **Immutable models** — C# records with `init` setters, transformations return new instances
- **All-errors validation** — collect every error rather than fail-fast, so consumers see the full picture
- **Validation doesn't block** — invalid orders still produce output, errors travel alongside data
- **Polling over events** — simpler for local dev with Azurite, no EventGrid dependency

## Service Decomposition

| Service | Responsibility |
|---|---|
| XML Parser | Convert XML string to domain model |
| Validator | Check data quality, return error list |
| Field Mapper | Enrich coded values with display names |
| JSON Transformer | Serialize to JSON with metadata envelope |
| Blob Storage | Read/write/move blobs |
| Polling Worker | Timer loop, orchestrates file lifecycle |

## File Lifecycle

| State | Location | Trigger |
|---|---|---|
| New | `input/` | External upload |
| Processing | `input/` (in-memory tracking) | Worker picks up |
| Done | `output/` (JSON) + `processed/` (XML) | Pipeline success |
| Failed | `failed/` (XML) | Pipeline exception |

## Infrastructure

- Azurite emulator in Docker for local blob storage
- Init container seeds test data on startup
- App can run locally against Azurite or in Docker alongside it

## Risks

- **Polling delay** — up to one interval between upload and processing. Acceptable for this use case.
- **No persistence of processing state** — restart reprocesses if files weren't moved. Acceptable since processing is idempotent.
