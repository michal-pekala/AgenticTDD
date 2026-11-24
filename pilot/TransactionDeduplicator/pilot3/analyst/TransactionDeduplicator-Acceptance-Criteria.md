# Transaction Deduplicator – Acceptance Criteria (Pilot 3, Java)

## Purpose

This document states the observable behavior of the TransactionDeduplicator component for pilot 3, which implements the pilot 1 and 2 customer task in Java 17. It is the analyst handoff to the architect (`/tdd-architect`) and the coding agents. It states behavior, not design: data structures, synchronization, eviction algorithms, exception types, result shapes and Java names are left to the architect. Engineer decisions that fix such choices are recorded as design constraints in `behaviour-decisions.md`, not here.

**Status:** approved by the engineer on 2026-11-24, including the assumptions below (D-020).

Decision references (D-NNN) point to `behaviour-decisions.md` in this folder. Behavior is the same as pilot 2 except where the change of platform needed an explicit decision: the accepted GUID formats (**D-017**) and the GC metrics of the benchmark (**D-018**).

## Scope

The component provides the entry point named by the customer task:

```
bool IsUniqueAndProcess(string transactionId)
```

in Java naming chosen by the architect (D-019): it takes the transaction ID as a string and returns a boolean.

It decides, in real time, whether a transaction ID has already been seen within a configurable expiration window, and records it if not. It runs in-memory inside a single process (one JVM) (D-006). It does not provide durable, distributed, or ACID guarantees.

**Terminology**

- **Active ID:** an ID whose most recent attempt was less than `UniqnessCheckExpiration` ago.
- **Attempt:** a call to `IsUniqueAndProcess` with a valid ID that returns true or false.
- **Capacity rejection:** the outcome for a new ID that cannot be stored because the buffer is full of active IDs.

## Configuration

| Setting | Meaning | Default | Decisions |
|---|---|---|---|
| `UniqnessCheckExpiration` | The expiration window | 10 seconds | D-009 |
| `UniqnessBufferCapacity` | Maximum number of active IDs retained; a deployment value may come from load tests | 1,000,000 | D-003, D-009 |

- Both values must be positive (greater than zero). (D-013)
- Invalid configuration does not prevent creating the component. It is reported clearly by the operation call, before any state changes. (D-013; see AC-12)

The names are the concepts decided in pilot 1 (spelling "Uniqness" kept); their Java casing is the architect's choice (D-019).

## Acceptance criteria

### AC-1. First-seen transaction
Given a valid GUID that is not currently active, when it is processed, the call returns true and the ID becomes active.

### AC-2. Active duplicate
Given an active ID, when the same ID is processed, the call returns false and the ID's expiration restarts from the time of this duplicate attempt. (D-001)

### AC-3. Expiration boundary
An ID is active until its most recent attempt plus `UniqnessCheckExpiration`. Strictly before that instant a new attempt returns false; at exactly that instant, or later, the ID is expired and a new attempt returns true. (D-001, D-005)

### AC-4. GUID identity and invalid input
A valid transaction ID is a GUID written in one of these text formats, in any letter casing, optionally with surrounding whitespace (D-017):

| Format | Example |
|---|---|
| N – 32 hex digits | `0f8fad5bd9cb469fa16570867728950e` |
| D – hyphenated 8-4-4-4-12 | `0f8fad5b-d9cb-469f-a165-70867728950e` |
| B – D in braces | `{0f8fad5b-d9cb-469f-a165-70867728950e}` |
| P – D in parentheses | `(0f8fad5b-d9cb-469f-a165-70867728950e)` |
| X – hex structure | `{0x0f8fad5b,0xd9cb,0x469f,{0xa1,0x65,0x70,0x86,0x77,0x28,0x95,0x0e}}` |

Strings in any of these formats that represent the same GUID are the same transaction ID. (D-007, D-008, D-017)

Null, empty, whitespace-only, or malformed input is rejected as invalid input. Malformed means any string not in the formats above, including hyphenated strings with short groups such as `1-1-1-1-1`. The rejection is distinguishable from true, false, and a capacity rejection, and it changes no state: afterwards every ID behaves exactly as if the call had not been made. (D-004, D-008, D-017)

### AC-5. Expired-entry reclamation
Expired IDs never count toward capacity: they are reclaimed before the capacity check for a new ID. Stale entries and expiration bookkeeping do not grow without bound, including when duplicates repeatedly refresh the same ID. (D-014)

### AC-6. Capacity limit
The number of retained active IDs never exceeds `UniqnessBufferCapacity`. When a valid new ID arrives and the buffer is still full of active IDs after reclamation, the ID is not accepted, the outcome is a capacity rejection that the caller can distinguish from true, false, and invalid input, and no state changes. (D-012, D-014)

A duplicate of an active ID is still recognized when the buffer is full: the call returns false and refreshes that ID's expiration (AC-2). Once an active ID expires, a previously rejected new ID can be accepted.

### AC-7. Thread safety
Concurrent calls are safe and every call gets one of the outcomes above. When concurrent calls race for the same previously unseen ID, at most one returns true (exactly one when capacity allows the ID); all others return false as duplicates. Capacity enforcement (AC-6), duplicate refresh (AC-2), and expiration (AC-3, AC-5) remain correct under concurrent calls. (D-015)

### AC-8. Process-local behavior
State lives in memory in this process only. It is not kept across process restarts and not shared with other instances. A new component instance starts with no active IDs. (D-006)

### AC-9. Complexity
Lookup and registration target expected O(1) time per call; linear worst case is allowed. Retained IDs and expiration bookkeeping stay bounded by `UniqnessBufferCapacity`. (D-007, D-010)

### AC-10. Throughput target
Target: 100,000 total calls per second, with 80% duplicates and 20% new IDs. With the default 10-second window this implies about 200,000 active IDs at steady state, below the default capacity of 1,000,000. (D-011)

### AC-11. Performance verification
A representative benchmark runs the AC-10 workload and records:

- hardware and runtime environment (JVM vendor and version, garbage collector, heap settings), and run duration;
- achieved throughput, and whether it meets 100,000 calls/second;
- p50 and p99 latency per call;
- allocated bytes per call, separately for the duplicate path and the new-ID path;
- young-generation and old/full garbage collection counts, and total GC pause time.

Results are reported, not gated: the engineer judges allocation, GC and latency. The target is not claimed as met without measured results. (D-011, D-016, D-018)

### AC-12. Invalid configuration
A component configured with a non-positive `UniqnessCheckExpiration` or `UniqnessBufferCapacity` can be created. Each call to `IsUniqueAndProcess` on it reports the invalid configuration clearly, distinguishable from true, false, invalid input, and a capacity rejection, and changes no state. (D-013)

## Out of scope

- Durable deduplication across process restarts.
- Coordination or shared state across service instances.
- ACID guarantees for downstream transaction execution.
- Non-GUID identifier formats, and GUID text formats other than those in AC-4.
- Hard pass/fail thresholds for latency, allocation and GC (D-016, D-018).
- Choice of data structure, synchronization, eviction algorithm, exception types, result shapes and Java names; these belong to the architect (D-010, D-012, D-019).

## Assumptions, open questions, and risks

**Open questions:** none.

**Assumptions** (confirmed by the engineer, D-020):

- The task's "sliding time window of X seconds" is `UniqnessCheckExpiration`.
- "Surrounding whitespace" in AC-4 means leading and trailing whitespace characters around an otherwise valid GUID, as in pilot 2; the architect names the exact character set in the design. A whitespace-only string is invalid input.
- The all-zero GUID (`00000000-0000-0000-0000-000000000000`) is a valid ID, as in pilot 2.

**Risks and limitations:**

- The task asks for strict O(1); the engineer accepted expected O(1) with linear worst case (D-007). The component does not claim strict O(1).
- The task cites ACID consistency and double-spend prevention; this process-local, in-memory component cannot guarantee either across restarts or instances (D-006). Callers need other safeguards for that.
- At capacity, new transactions are rejected (AC-6). A capacity value that is too small for real traffic blocks legitimate transactions; D-003 asks for load testing to size it.
- The 100,000 calls/second target depends on hardware, and on the JVM's just-in-time warm-up and garbage collector; AC-11 records the environment so results can be compared.
- Java's standard UUID parser does not implement AC-4 on its own (it accepts only the D form and accepts short groups); a test-first implementation must cover each format and the malformed cases explicitly.
