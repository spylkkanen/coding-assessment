# ADR-002: Immutable Records for Domain Models

**Status:** Accepted
**Date:** 2026-02-19

## Context

Data flows through multiple pipeline stages. We need to prevent accidental mutation between stages while keeping transformations readable.

## Decision

Use C# records with `init` setters. Transformations create new instances via `with` expressions.

## Consequences

- Thread-safe by default, no shared mutable state
- Clear data ownership — each stage gets input, returns new output
- Nested updates can be verbose but remain explicit
- All services can safely be singletons
