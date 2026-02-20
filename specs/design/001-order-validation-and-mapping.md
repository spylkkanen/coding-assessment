# Design: Order Data Validation and Field Mapping

**Spec ID:** 001
**Feature Spec:** [specs/features/001-order-validation-and-mapping.md](../features/001-order-validation-and-mapping.md)
**Status:** Implemented
**Author:** Tech Lead
**Date:** 2026-02-19

## Approach

Implement two existing stub services in the pipeline. Validator collects all errors in a single pass. Mapper uses dictionary lookups with passthrough for unknown values.

## Where It Fits

```
Parse XML → [Validate] → [Map Fields] → Transform to JSON
              ▲ NEW        ▲ NEW
```

Both services already have interfaces and are wired into the pipeline. Only the method bodies need implementation.

## Validation Design

- Iterate each order in the batch
- Check required fields first (skip format check if field is empty)
- Apply regex patterns for format validation
- Check numeric ranges on items and totals
- Collect ALL errors into a flat list — don't stop at the first failure
- Tag each error with order ID, field path, message, and category (REQUIRED / FORMAT / RANGE)

## Mapping Design

- Use static dictionaries for each mapping type (country, product, status)
- Lookup with fallback — if code not found, return original value unchanged
- Return a new batch instance (records are immutable, use `with` expressions)
- Apply all mappings to every order in the batch

## Test Strategy

- One test per validation rule with valid and invalid inputs
- Use `[Theory]` with `[InlineData]` for multiple invalid format examples
- One test per mapping type with known values + one for unknown values
- Test with multi-order batches to verify all orders are processed

## Files to Change

| File | What |
|---|---|
| `Services/OrderValidatorService.cs` | Implement `Validate()` |
| `Services/FieldMappingService.cs` | Implement `MapFields()` |
| `Tests/Services/OrderValidatorServiceTests.cs` | Add validation tests |
| `Tests/Services/FieldMappingServiceTests.cs` | Add mapping tests |
