# Spec-Driven Development

This directory contains all specification artifacts that drive implementation. Specs are the **source of truth** — code follows specs, not the other way around.

## Lifecycle

```
Feature Spec → Design Doc → ADR (if needed) → Implementation → Test Plan → Review
```

## Directory Structure

```
specs/
  features/       Feature specifications with acceptance criteria
  design/         Technical design documents
  adrs/           Architecture Decision Records
  test-plans/     Test strategies and verification plans
```

## Workflow

1. **Feature Spec** — Product Owner or engineer writes a feature spec in `specs/features/`
2. **Design Doc** — Engineer creates a design doc in `specs/design/` referencing the feature spec
3. **ADR** — For significant architectural decisions, create an ADR in `specs/adrs/`
4. **Implementation** — Use Claude Code `/code` command with the spec as context
5. **Test Plan** — Write or generate test plan in `specs/test-plans/`
6. **Verification** — Use Claude Code `/test` and `/build` commands to verify

## Naming Convention

- Features: `NNN-short-name.md` (e.g., `001-order-validation.md`)
- Design docs: `NNN-short-name.md` matching feature number
- ADRs: `NNN-title.md` with sequential numbering
- Test plans: `NNN-short-name.md` matching feature number

## Claude Code Integration

Feature specs can be passed directly to Claude Code commands:
```bash
# Implement a feature from its spec
claude "/code specs/features/002-new-feature.md"

# Generate tests from a test plan
claude "/test specs/test-plans/002-new-feature.md"
```
