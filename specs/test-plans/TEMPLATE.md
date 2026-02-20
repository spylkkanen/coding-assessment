# Test Plan: [Feature Name]

**Spec ID:** NNN
**Feature Spec:** [specs/features/NNN-name.md](../features/NNN-name.md)
**Status:** Draft | Active | Complete
**Author:** [Name]
**Date:** YYYY-MM-DD

## Scope

[What is being tested and what is out of scope.]

## Test Strategy

| Level | Framework | Location |
|---|---|---|
| Unit | xUnit | `tests/OrderTransformer.Tests/Services/` |
| Integration | xUnit + Docker | `tests/OrderTransformer.Tests/Integration/` |
| E2E | Docker Compose | Manual / CI |

## Test Cases

### Unit Tests

| ID | Test | Input | Expected | Priority |
|---|---|---|---|---|
| UT-001 | [Description] | [Input] | [Expected result] | High/Med/Low |

### Integration Tests

| ID | Test | Setup | Expected | Priority |
|---|---|---|---|---|
| IT-001 | [Description] | [Prerequisites] | [Expected result] | High/Med/Low |

### E2E Verification

```bash
# Steps to verify end-to-end
```

## Acceptance Criteria Traceability

| Acceptance Criteria | Test ID(s) | Status |
|---|---|---|
| [From feature spec] | UT-001, IT-001 | Pass/Fail/Pending |

## Run Commands

```bash
# Run all tests for this feature
dotnet test tests/OrderTransformer.Tests --filter "FullyQualifiedName~[FilterPattern]"
```
