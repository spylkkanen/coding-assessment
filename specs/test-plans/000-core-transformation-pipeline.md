# Test Plan: Core Transformation Pipeline

**Spec ID:** 000
**Feature Spec:** [specs/features/000-core-transformation-pipeline.md](../features/000-core-transformation-pipeline.md)
**Status:** Active

## Strategy

Unit test each pipeline stage independently. Mock dependencies in the pipeline orchestrator. Verify end-to-end manually via Docker Compose.

## Unit Tests

**XML Parser**
- Valid XML produces correct domain model (tenant, header, customer, items, totals)
- Invalid XML throws an exception

**JSON Transformer**
- Valid batch produces parseable JSON
- Output includes order data and metadata
- Validation errors included when present, omitted when absent

**Pipeline Orchestrator**
- Valid input returns success with JSON
- Validation errors still result in success (errors ride along)
- Services are called in correct order
- Parse failure returns failure result with error message

## E2E Verification

1. Start Azurite with seed data via `docker compose up -d`
2. Run the app — it should process the seed XML
3. Verify `output/` blob exists with correct JSON
4. Verify `processed/` blob exists (input moved)

## Run Commands

```bash
dotnet test tests/OrderTransformer.Tests --verbosity normal
```
