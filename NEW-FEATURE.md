# New Feature: Order Data Validation and Field Mapping

## Background

We have a working data transformation pipeline that monitors blob storage for incoming XML order files, converts them to JSON, and stores the output. The system handles orders from multiple tenants with customers, line items, and totals.

## Requirements

The Product Owner has identified two new capabilities needed in the transformation pipeline:

### 1. Data Validation

Before converting to JSON, incoming order data must be validated. The following rules apply:

**Required fields** (must be present and non-empty):
- `Header.OrderId`
- `Header.OrderDate`
- `Customer.CustomerId`
- `Customer.Name`
- `Customer.Email`

**Format validations:**
- **OrderId** must match pattern `ORD-YYYY-NNNNNN` (e.g., `ORD-2024-001234`)
- **Email** must be a valid format (contains `@` with text before and after: `[^@]+@[^@]+\.[^@]+`)
- **Country code** must be exactly 2 uppercase letters — ISO 3166-1 alpha-2 (e.g., `FI`, `SE`, `US`)
- **Currency code** must be exactly 3 uppercase letters — ISO 4217 (e.g., `EUR`, `USD`, `GBP`)

**Range validations (per item):**
- `Quantity` must be greater than 0
- `UnitPrice` must be non-negative (>= 0)

**Range validations (totals):**
- `Subtotal`, `TaxAmount`, `Total` must be non-negative (>= 0)

**Error handling:**
- Orders that fail validation should still be included in the JSON output
- Validation errors must be collected and included in the output
- The pipeline must **not** crash on invalid data
- Each error should include: OrderId, field name, human-readable message, and error code (`REQUIRED`, `FORMAT`, or `RANGE`)

### 2. Field Value Mapping

After validation, certain field values must be mapped to human-readable names:

**Country codes → full country names** (in `Customer.Address.Country`):

| Code | Name |
|------|------|
| FI | Finland |
| SE | Sweden |
| NO | Norway |
| DK | Denmark |
| US | United States |
| GB | United Kingdom |
| DE | Germany |

**Product codes → categories** (in `OrderItem.Description`, append or replace):

| Code | Category |
|------|----------|
| PROD-001 | Widgets |
| PROD-002 | Gadgets |
| PROD-003 | Premium Widgets |

**Order status → display labels** (in `Header.Status`):

| Status | Label |
|--------|-------|
| draft | Draft |
| confirmed | Order Confirmed |
| processing | In Processing |
| shipped | Shipped |
| delivered | Delivered |
| cancelled | Cancelled |

**If a value is not found in the mapping, it should remain unchanged.**

### 3. Unit Tests

Add unit tests for the new validation and mapping logic:

- Test each validation rule individually (valid input, invalid input, edge cases)
- Test each mapping (known values, unknown values)
- Test with multiple orders in a batch
- Use the existing test patterns in the project as reference

## Where to Implement

The pipeline already has stub services wired in that currently do nothing:

| File | What to Implement |
|------|-------------------|
| `src/OrderTransformer/Services/OrderValidatorService.cs` | Validation logic in the `Validate()` method |
| `src/OrderTransformer/Services/FieldMappingService.cs` | Mapping logic in the `MapFields()` method |
| `tests/OrderTransformer.Tests/Services/OrderValidatorServiceTests.cs` | Tests for each validation rule |
| `tests/OrderTransformer.Tests/Services/FieldMappingServiceTests.cs` | Tests for each mapping |

The existing `TransformationPipeline` already calls these services in the correct order:
```
Parse XML → Validate → Map Fields → Transform to JSON
```

No changes to the pipeline, worker, or DI configuration are needed.

## Design Phase — What to Draw

Before coding, design your solution on the whiteboard. Consider drawing:

### Class / Service Structure
```
TransformationPipeline
    │
    ├── XmlParserService          (existing, fully implemented)
    │
    ├── OrderValidatorService     ← YOUR DESIGN
    │   ├── What methods/helpers?
    │   ├── How to iterate orders?
    │   └── How to collect errors?
    │
    ├── FieldMappingService       ← YOUR DESIGN
    │   ├── Where to store mappings?
    │   ├── How to handle immutable records?
    │   └── How to handle unknown values?
    │
    └── JsonTransformerService    (existing, fully implemented)
```

### Data Flow with Validation Errors
- Show how validation errors flow through the pipeline
- Show how they appear in the JSON output
- Show that invalid orders are still processed (not rejected)

### Mapping Strategy
- Dictionary-based lookup? Configuration file? Constants?
- How to update immutable records (C# `with` expressions)
- Where in the pipeline does mapping happen relative to validation?

### Test Strategy
- What test cases per validation rule?
- Edge cases: empty strings, null values, boundary values
- How to construct test data (helper methods)

## Acceptance Criteria

- [ ] All validation rules implemented and returning correct errors
- [ ] All field mappings working (country, product, status)
- [ ] Unknown values pass through unchanged
- [ ] Pipeline does not crash on invalid data
- [ ] Validation errors appear in JSON output
- [ ] Unit tests cover each validation rule
- [ ] Unit tests cover each mapping
- [ ] All existing tests still pass
- [ ] Application builds and runs successfully
