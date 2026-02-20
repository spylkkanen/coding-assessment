# Test Plan: Order Validation and Field Mapping

**Spec ID:** 001
**Feature Spec:** [specs/features/001-order-validation-and-mapping.md](../features/001-order-validation-and-mapping.md)
**Status:** Active

## Strategy

Test each validation rule and mapping individually. Use parameterized tests for rules with multiple invalid inputs. Verify edge cases like zero values and unknown codes.

## Validation Tests

**Required fields** — one test per field, verify REQUIRED error when empty

**Format validation** — parameterized tests with several invalid examples per rule:
- OrderId: wrong patterns (missing digits, wrong prefix, lowercase)
- Email: missing @, missing domain, missing TLD
- Country code: too long, too short, lowercase
- Currency code: too long, too short, lowercase

**Range validation** — test boundary values:
- Quantity: 0 and negative should fail, 1 should pass
- UnitPrice: negative should fail, 0 should pass
- Totals: negative should fail, 0 should pass

**Edge cases:**
- Missing OrderId should use "unknown" in error reporting
- Valid batch should return zero errors

## Mapping Tests

**Known values** — parameterized test covering all entries in each mapping table

**Unknown values** — one test per mapping type verifying passthrough

**Batch processing** — multiple orders in one batch, all should be mapped

## Run Commands

```bash
dotnet test tests/OrderTransformer.Tests --filter "FullyQualifiedName~OrderValidator"
dotnet test tests/OrderTransformer.Tests --filter "FullyQualifiedName~FieldMapping"
```
