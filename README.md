# Order Transformer — Coding Assessment

A .NET 9 Worker Service that monitors Azure Blob Storage for incoming XML order files, transforms them to JSON, and stores the output back to blob storage.

## Interview Plan

In this assessment you have tools:
- VS Code
- Claude Code
- Azure Storage Explorer
- Docker Desktop
- Terminal

### Phase 1: Read and Explain (~15 minutes)

**Goal:** Candidate reads the codebase and demonstrates understanding by drawing and explaining.

1. **Read the code** — Start from `Program.cs` and trace the execution flow through the application
2. **Draw the process** — On the whiteboard, draw a diagram showing:
   - How the application starts and what triggers processing
   - The pipeline stages and what each service does
   - How data flows from XML input to JSON output
   - What happens to files after processing (success and failure paths)
3. **Explain the architecture** — Walk through the diagram and explain:
   - Why is each service behind an interface?
   - Why are models defined as records with `init` setters?
   - How does the pipeline handle validation errors — does it stop or continue?
   - What happens if one file fails — does it affect other files?

### Phase 2: Design and Implement (~30 minutes)

**Goal:** Candidate receives a new feature requirement, designs the solution, then implements it.

1. **Receive the requirement** — Product Owner presents the new feature spec from `specs/features/`
2. **Design first** — Before writing any code:
   - Read the existing code to find where the change belongs
   - Draw the updated validation flow on the whiteboard
   - Explain the approach: what changes, what stays the same
   - Discuss edge cases and testing strategy
3. **Create the spec** — Write or update the feature spec with design decisions
4. **Implement** — Write the code:
   - Follow existing patterns in the codebase
   - Use AI tools (Copilot, Claude Code, etc.) — we want to see how you work with them
   - Run tests incrementally, not just at the end
5. **Verify** — Build, test, and run end-to-end with Docker

### Start Services

```bash
# Start infrastructure
docker compose up -d

# Verify seed data
docker compose logs azurite-init

# Run tests (should all pass)
dotnet test tests/OrderTransformer.Tests

# Run the app
dotnet run --project src/OrderTransformer
```

### Clean Up

```bash
docker compose --profile app down -v
```