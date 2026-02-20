# ADR-003: Polling Over Event-Driven Blob Processing

**Status:** Accepted
**Date:** 2026-02-19

## Context

We need to detect new XML files in blob storage. Options: event-driven (BlobTrigger, EventGrid) or timer-based polling.

## Decision

Use a BackgroundService with a configurable polling interval. Track in-progress files in memory to prevent duplicate processing.

## Consequences

- Simple — no EventGrid, Functions, or message bus infrastructure needed
- Works identically with Azurite and production Azure Storage
- Small delay between upload and processing (up to one poll interval)
- Processing state lost on restart — acceptable since file moves make it idempotent
- Can migrate to event-driven later without changing pipeline logic
