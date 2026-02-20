# Feature: Core XML-to-JSON Transformation Pipeline

**Spec ID:** 000
**Status:** Implemented
**Author:** Product Owner
**Date:** 2026-02-19

## Background

Our partners and external systems send us order data as XML files. These files are uploaded to Azure Blob Storage by existing integration processes. Currently, our downstream analytics and fulfillment systems only consume JSON. We need an automated service that bridges this gap — picking up XML orders as they arrive, converting them to JSON, and making the results available without any manual steps.

## Business Requirements

### Why This Matters
- Our fulfillment team cannot process XML — they need JSON to feed into their dashboards and order management tools
- Manual file conversion is error-prone and creates a bottleneck during peak order periods
- We need full traceability: every incoming file must end up in either "processed" or "failed", never silently lost

### Multi-Tenant Support
- We serve multiple tenants (business customers) through the same pipeline
- Each XML batch carries a tenant identifier that must be preserved in the JSON output
- Downstream systems use the tenant ID to route orders to the correct fulfillment workflow

### Order Data Structure
- Each batch file contains one or more orders from the same tenant
- An order includes: header (ID, date, status), customer details with address, line items with pricing, and order totals
- This structure reflects our standard order schema used across systems

### Processing Guarantees
- Every file that lands in the input folder must be processed — no silent drops
- If something goes wrong with one file, other files must still be processed
- The service should run continuously as a background process
- We need clear visibility into what succeeded and what failed

## Requirements

### XML Input
- Orders arrive as XML files in a designated input folder in blob storage
- Each file contains a tenant identifier and one or more orders
- Orders include header info, customer details, line items, and totals

### JSON Output
- Each processed XML file produces a single JSON file in an output folder
- JSON should include a processing timestamp and order count for auditing
- Property names should follow camelCase convention for consistency with our API standards

### File Lifecycle
- Successfully processed files move to a "processed" folder
- Files that fail processing move to a "failed" folder
- Input folder should be clean after processing — operations team monitors it for stuck files

### Reliability
- The service should poll for new files on a regular interval
- Processing failures on one file should not prevent processing of others
- The service should not process the same file twice in one cycle
- The system should recover gracefully from transient errors

### Infrastructure
- Use Azurite emulator for local development
- Docker Compose for running the full stack locally
- Seed data for testing the end-to-end flow

## Acceptance Criteria

- [x] New XML files in the input folder are automatically detected
- [x] Valid XML produces correct JSON in the output folder
- [x] Processed files are moved out of the input folder
- [x] Failed files are moved to a separate failure folder
- [x] The service keeps running after encountering bad files
- [x] Multiple orders per file are handled
- [x] Tenant ID is preserved in the output
- [x] Docker Compose starts everything with seed data

## Out of Scope

- Real-time event-driven triggers (polling is sufficient for current volumes)
- Authentication or authorization (handled by blob storage access policies)
- Multiple storage accounts or containers
- Retry logic for transient blob storage failures
