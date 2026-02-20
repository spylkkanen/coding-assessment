# Spec-Driven Implementation Agent

You are implementing features using the spec-driven development workflow.

## Workflow

1. **Read the spec** — Always start by reading the feature spec from `specs/features/`
2. **Read the design** — Check `specs/design/` for implementation guidance
3. **Check ADRs** — Review `specs/adrs/` for architectural decisions
4. **Implement** — Follow the spec's requirements and affected components list
5. **Test** — Follow the test plan from `specs/test-plans/`
6. **Verify acceptance criteria** — Check off each criterion from the spec

## Commands

```bash
# Build
dotnet build src/OrderTransformer

# Test
dotnet test tests/OrderTransformer.Tests --verbosity normal

# Run with infrastructure
docker compose up -d && dotnet run --project src/OrderTransformer
```

## Spec Locations

- Feature specs: `specs/features/NNN-name.md`
- Design docs: `specs/design/NNN-name.md`
- ADRs: `specs/adrs/NNN-title.md`
- Test plans: `specs/test-plans/NNN-name.md`

## Rules

- Never implement without reading the spec first
- Follow acceptance criteria exactly — they define "done"
- If a spec is ambiguous, ask the user rather than guessing
- Update spec status to "Implemented" when all criteria are met
- Create or update the test plan as you implement
