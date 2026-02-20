# Feature: Order Data Validation and Field Mapping

**Spec ID:** 001
**Status:** Implemented
**Author:** Product Owner
**Date:** 2026-02-19

## Background

The pipeline currently converts XML to JSON without checking data quality. Our fulfillment team has reported receiving malformed orders — missing customer emails, invalid country codes, nonsensical quantities. They need the pipeline to catch these problems early. Additionally, downstream systems expect human-readable values (e.g., "Finland" not "FI") but our source data uses coded values.

## Business Requirements

### Why Validation Matters
- Bad data reaching fulfillment causes manual intervention and delays
- The operations team needs to see exactly what's wrong with an order, not just "it failed"
- We cannot reject bad orders entirely — the fulfillment team still needs to see them, marked with their problems, so they can follow up with the partner

### Why Field Mapping Matters
- Our dashboards and reports display data to non-technical staff who don't know ISO country codes
- Product categories are needed for reporting — source data only has product codes
- Order status labels must be customer-friendly for the partner portal

### Error Visibility
- Each validation error must identify: which order, which field, what's wrong, and the error type
- Error types should be categorized (missing data vs. wrong format vs. out of range) so the ops team can prioritize
- The JSON output must include both the order data AND any validation errors — downstream systems decide how to handle them

## Requirements

### Data Validation

Before converting to JSON, validate each order:

**Required fields** — must be present and non-empty:
- Order ID, Order Date
- Customer ID, Customer Name, Customer Email

**Format checks:**
- Order ID follows our standard pattern: `ORD-YYYY-NNNNNN`
- Email is a valid format
- Country code is ISO 3166-1 alpha-2 (2 uppercase letters)
- Currency code is ISO 4217 (3 uppercase letters)

**Range checks:**
- Item quantity must be positive
- Item unit price must be non-negative
- Totals (subtotal, tax, total) must be non-negative

**Error handling:**
- Invalid orders should still appear in the JSON output
- Validation errors are collected and included alongside the data
- The pipeline must never crash on bad data
- Each error needs: which order, which field, what went wrong, error category

### Field Value Mapping

After validation, map coded values to display names:

**Country codes** — Nordic markets plus our key international markets:
FI → Finland, SE → Sweden, NO → Norway, DK → Denmark, US → United States, GB → United Kingdom, DE → Germany

**Product codes** — our current product catalog:
PROD-001 → Widgets, PROD-002 → Gadgets, PROD-003 → Premium Widgets

**Order status** — lifecycle stages shown in partner portal:
draft → Draft, confirmed → Order Confirmed, processing → In Processing, shipped → Shipped, delivered → Delivered, cancelled → Cancelled

If a code isn't in the mapping, leave it as-is — new codes may appear before we update the mapping, and we shouldn't break processing.

### Unit Tests

- Test each validation rule individually
- Test each mapping with known and unknown values
- Test batch processing with multiple orders
- Follow existing test patterns in the project

## Acceptance Criteria

- [x] All validation rules return correct errors
- [x] All field mappings produce expected output
- [x] Unknown values pass through unchanged
- [x] Pipeline does not crash on invalid data
- [x] Validation errors appear in JSON output
- [x] Tests cover each rule and mapping
- [x] All existing tests still pass
- [x] Application builds and runs end-to-end

## Out of Scope

- Dynamic or configurable mapping tables (current catalog is small and stable)
- Validation that crosses order boundaries (e.g., duplicate order detection)
- Auto-correction of invalid data
