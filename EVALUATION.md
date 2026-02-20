# Evaluation Rubric — .NET Coding Assessment

## Assessment Structure

| Phase | Duration | Interviewer Role | Focus |
|-------|----------|-----------------|-------|
| Phase 1: Design | ~15 minutes | Product Owner | Understanding requirements, designing solution on whiteboard |
| Phase 2: Implementation | ~45-60 minutes | Tech Lead | Coding, testing, using AI tools, working software |

---

## Phase 1 — Design (30% of total score)

The Product Owner explains the current pipeline and describes the new validation + mapping requirements. The candidate designs their solution on the whiteboard.

### What PO Presents

> "We have a data transformation pipeline that monitors blob storage for XML order files. When a file arrives, the system parses it, converts to JSON, and stores the output. It works well.
>
> Now we need two additions:
> 1. **Validation** — before converting, validate the order data (required fields, format checks for order IDs, emails, country codes, currency codes, and range checks for quantities and prices)
> 2. **Field mapping** — map coded values to human-readable names (country codes to country names, product codes to categories, status codes to display labels)
>
> Invalid orders should still appear in the output, marked with their validation errors. The system should never crash on bad data. We also need unit tests."
>
> Hand the candidate the [NEW-FEATURE.md](NEW-FEATURE.md) document for reference.

### Design Scoring

| Criteria | Excellent (5) | Good (3-4) | Needs Work (1-2) |
|----------|--------------|------------|------------------|
| **Pipeline understanding** | Correctly identifies Parse→Validate→Map→Transform flow and where new code fits | Understands main flow, minor gaps in sequencing | Does not understand where validation/mapping fit in the pipeline |
| **Service design** | Proposes clean approach matching existing interface pattern, discusses immutable records | Reasonable design, some coupling or missing details | Monolithic approach, no separation of concerns |
| **Error handling** | Validation errors collected per order, pipeline continues, errors included in output JSON | Some error handling discussion, may miss details | No error handling strategy discussed |
| **Mapping approach** | Dictionary/lookup-based, discusses unknown value handling, immutable update strategy | Reasonable approach, may miss edge cases | Hard-coded if/else or incomplete approach |
| **Test strategy** | Identifies specific test scenarios (valid, invalid per rule, edge cases, multiple orders) | Mentions testing, lists some scenarios | No test discussion or very vague |

**Phase 1 Total: ___ / 25**

---

## Phase 2 — Implementation (70% of total score)

The Tech Lead provides the candidate with the repo and says:

> "Good design. The project is ready on this machine. It builds and all 16 existing tests pass. There are stub services for `OrderValidatorService` and `FieldMappingService` already wired into the pipeline — they currently do nothing. Your task is:
>
> 1. Implement validation logic in `OrderValidatorService.cs`
> 2. Implement mapping logic in `FieldMappingService.cs`
> 3. Add unit tests for both
>
> Use AI tools — Copilot, Claude Code, ChatGPT, whatever you prefer. We want to see how you work with them.
>
> Start with: `docker compose up -d` to get Azurite running, then `dotnet test` to verify everything passes."

### Implementation Scoring

| Criteria | Excellent (5) | Good (3-4) | Needs Work (1-2) |
|----------|--------------|------------|------------------|
| **Validation implementation** | All rules implemented correctly, good regex patterns, clear error messages with proper ErrorCodes | Most rules work, minor regex issues or missing a rule | Few rules implemented, broken patterns, incomplete |
| **Mapping implementation** | Clean dictionary-based approach, all three mappings work, unknown values handled, immutable updates correct | Mappings work, minor issues (e.g., doesn't handle unknown values) | Incomplete or broken mappings |
| **Code quality** | Follows existing patterns, clean C#, good naming, consistent with codebase style | Readable code, minor style inconsistencies | Messy, inconsistent, doesn't follow existing patterns |
| **Test coverage** | Multiple test cases per rule, edge cases covered, uses `[Theory]` where appropriate | Tests exist for main paths, some gaps | Few tests, no edge cases, or tests don't work |
| **AI tool usage** | Effective prompting, reviews AI output, catches/corrects errors, iterates intelligently | Uses AI productively, mostly effective | Doesn't use AI, or blindly copies without understanding |
| **Working software** | App builds, all tests pass, end-to-end works (XML in → JSON out with validation + mapping) | Most things work, minor issues | Does not build or significant functionality broken |
| **Problem solving** | Reads existing code first, understands patterns, debugs issues independently | Some code reading, asks good questions | Doesn't examine existing code, stuck frequently |

**Phase 2 Total: ___ / 35**

---

## Overall Score

| Component | Score | Max |
|-----------|-------|-----|
| Phase 1: Design | | 25 |
| Phase 2: Implementation | | 35 |
| **Total** | | **60** |

### Rating Scale

| Score | Rating | Recommendation |
|-------|--------|---------------|
| 48-60 | Excellent | Strong hire |
| 36-47 | Good | Hire |
| 24-35 | Adequate | Borderline, discuss |
| Below 24 | Below expectations | No hire |

---

## Key Observations to Note

### Red Flags
- Cannot explain their own design on the whiteboard
- Blindly accepts AI output without reviewing
- Doesn't read existing code before implementing
- Ignores existing patterns/conventions
- Cannot debug build or test failures
- Gives up quickly on errors

### Green Flags
- Reads `TransformationPipeline.cs` and existing tests first to understand patterns
- Asks clarifying questions about requirements
- Reviews and edits AI-generated code
- Runs tests incrementally (not just at the end)
- Handles edge cases proactively
- Clean, consistent code style

### AI Usage Notes
Document how the candidate uses AI tools:
- What tool(s) did they use?
- How did they prompt? (vague vs. specific)
- Did they review output or paste blindly?
- Did they iterate when AI output was wrong?
- Did they combine AI help with their own knowledge?
