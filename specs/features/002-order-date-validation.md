# New Feature: Order Date Validation

**Spec ID:** 002
**Status:** Draft
**Author:** Product Owner
**Date:** 2026-02-20

## Background

We have a working transformation pipeline that validates order data — checking required fields, formats, and value ranges. However, the OrderDate field is only checked for being non-empty. It is not validated as an actual date. Our fulfillment team has received orders with dates like "yesterday", "2024-13-45", and timestamps from the year 2099. These slip through into the JSON output and cause errors downstream when fulfillment systems try to parse them.

## Business Need

The fulfillment system rejects orders with invalid dates, which triggers a manual review process. Last month, 12% of flagged orders were simply bad dates that could have been caught earlier in the pipeline. The partner support team spends time investigating these when a simple validation would surface the problem immediately with a clear error message.

We also need to catch future-dated orders. Some partners have accidentally sent test data with dates set years ahead. These orders should not silently enter the fulfillment queue — they need to be flagged so the operations team can follow up with the partner.

## Requirements

### Order Date Format Validation

Add a new validation rule to the pipeline:

- The `OrderDate` field must be a valid ISO 8601 date/time string (e.g., `2024-01-15T10:30:00Z`)
- If the value cannot be parsed as a valid date, produce a FORMAT validation error
- If the date is in the future (after the current UTC time), produce a RANGE validation error

**Examples of invalid dates:**
- `"yesterday"` — not a date format → FORMAT error
- `"2024-13-45T00:00:00Z"` — invalid month/day → FORMAT error
- `"2099-01-01T00:00:00Z"` — future date → RANGE error
- `"not-a-date"` — garbage value → FORMAT error

**Examples of valid dates:**
- `"2024-01-15T10:30:00Z"` — valid past date
- Today's date — valid (not in the future)

### Error Details

Validation errors should follow the same pattern used by existing validation rules in the system.

### Existing Behavior Unchanged

- The existing check for empty OrderDate must still work
- All other validation rules remain unchanged
- The pipeline must still not crash on invalid data

### Unit Tests

Add tests for the new validation rule:
- Valid ISO 8601 date passes validation
- Unparseable string produces FORMAT error
- Invalid date components (bad month/day) produce FORMAT error
- Future date produces RANGE error
- Empty OrderDate still produces only a REQUIRED error (existing behavior preserved)
- Follow existing test patterns in the project

## Design Phase — What to Draw

Before coding, draw a diagram of the changed process on the whiteboard. The diagram must show:

### Validation Flow
- The current validation sequence and where the new date validation fits in
- The two possible error outcomes (FORMAT vs. RANGE) and when each applies

### Decision Logic
- Show the order of checks for the OrderDate field: what happens when it's empty vs. unparseable vs. future-dated
- Should the format check run if the field is empty, or only when a value is present?

### Questions to Answer on the Whiteboard
- Where in the codebase does validation happen today? How is it structured?
- How do existing format validations decide when to skip checking (e.g., empty fields)?
- How do you test "future date" without making tests depend on the current time?

## Acceptance Criteria

- [ ] Unparseable OrderDate produces a FORMAT validation error
- [ ] Future-dated OrderDate produces a RANGE validation error
- [ ] Valid past/present dates pass validation without errors
- [ ] Empty OrderDate still produces only a REQUIRED error (no duplicate FORMAT error)
- [ ] Existing validation rules are unchanged
- [ ] Unit tests cover valid dates, invalid formats, and future dates
- [ ] All existing tests still pass
- [ ] Application builds and runs successfully
