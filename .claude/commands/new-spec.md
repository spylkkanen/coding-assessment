# New Spec Generator

Help the user create a new feature specification following the spec-driven development pattern.

## Steps

1. Ask the user to describe the feature they want to build
2. Determine the next spec ID by checking existing specs in `specs/features/`
3. Create the following artifacts:
   - Feature spec in `specs/features/NNN-name.md` using the template
   - Design doc in `specs/design/NNN-name.md` using the template
   - Test plan in `specs/test-plans/NNN-name.md` using the template
4. If an architectural decision is involved, create an ADR in `specs/adrs/`

## Templates

Templates are located at:
- `specs/features/TEMPLATE.md`
- `specs/design/TEMPLATE.md`
- `specs/adrs/TEMPLATE.md`
- `specs/test-plans/TEMPLATE.md`

## Naming Convention

- Use lowercase kebab-case for filenames
- Prefix with zero-padded 3-digit ID (001, 002, etc.)
- Example: `002-user-authentication.md`

## After Creating

Print a summary of all created files and remind the user to:
1. Review and refine the spec
2. Get spec approved before implementation
3. Use `/spec` command to implement from the spec
