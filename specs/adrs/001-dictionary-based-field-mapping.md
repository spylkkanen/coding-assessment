# ADR-001: Dictionary-Based Field Mapping

**Status:** Accepted
**Date:** 2026-02-19

## Context

We need to map coded values (country codes, product codes, statuses) to human-readable names. Options include hard-coded conditionals, dictionary lookups, external config files, or database-backed mappings.

## Decision

Use in-memory dictionaries within the service. Unknown values pass through unchanged.

## Consequences

- Simple and fast for a small, stable set of mappings
- Adding new mappings requires a code change — acceptable given the current scale
- Can be extracted to configuration later if business needs change
