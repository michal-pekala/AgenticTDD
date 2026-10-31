# Behaviour decisions log

Append-only history of explicit engineer decisions for the
TransactionDeduplicator pilot 2 analysis. Pilot 2 reruns the same customer
task (`pilot/TransactionDeduplicator/TransactionDeduplicator.txt`) from
scratch, reusing pilot 1's engineer decisions as answers.

Sources of reused entries:

- `pilot1 behaviour-decisions.md` =
  `pilot/TransactionDeduplicator/pilot1/analyst/behaviour-decisions.md`
- `pilot1 engineering-decisions.md` =
  `pilot/TransactionDeduplicator/pilot1/engineering/engineering-decisions.md`

Reused entries carry the original decision time only where the source
captured it. "Recorded at" is when the entry was written into this log.

## Decisions

### D-001 - Duplicate attempts refresh expiration

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Define whether a repeated transaction ID extends its
  active period or expires based on its first accepted attempt.
- Engineer decision: Every duplicate attempt refreshes that ID's expiration.
  (Pilot 1 temporarily deferred this in C-041 and restored it in C-044; the
  requirement stands.)
- Affected criteria: AC-2 Active duplicate; AC-3 Expiration boundary.
- Status: Active
- Source: pilot1 behaviour-decisions.md D-001; restoration confirmed by
  pilot1 engineering-decisions.md C-044.

### D-002 - Capacity exhaustion uses a dedicated exception (design constraint)

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Exact deduplication with bounded memory needs behavior
  when the configured maximum is reached; deliberately throwing
  `OutOfMemoryException` would misrepresent runtime memory exhaustion.
- Engineer decision: Use a dedicated `CapacityExceededException` for a new ID
  when capacity is full. Do not throw `OutOfMemoryException`.
- Affected criteria: AC-6 Capacity limit (mechanism only).
- Status: Active as a design constraint for the architect; not a business
  acceptance criterion (see D-012).
- Source: pilot1 behaviour-decisions.md D-002.

### D-003 - Capacity is configurable and measured

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: The maximum active-ID count needs to be testable and
  appropriate for the deployment workload.
- Engineer decision: Make capacity configurable; determine a suitable
  production-like configured value using a load test.
- Affected criteria: Configuration; AC-6 Capacity limit; AC-11 Performance
  verification.
- Status: Active; refined by D-009, which specifies the default value.
- Source: pilot1 behaviour-decisions.md D-003.

### D-004 - Invalid IDs are rejected without changing state

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Define behavior for null, empty, or whitespace-only
  transaction IDs.
- Engineer decision: Throw `ArgumentException` and do not change stored state.
- Affected criteria: AC-4 GUID identity and invalid input. The behavior
  (rejected, no state change) is in the criterion; the exception type is a
  design constraint for the architect.
- Status: Active; malformed GUID handling is further specified by D-008.
- Source: pilot1 behaviour-decisions.md D-004.

### D-005 - Expire at the exact boundary

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Determine whether an ID is still active at exactly
  last-attempt time plus the expiration duration.
- Engineer decision: The ID expires at the exact boundary and may be accepted
  again at that instant.
- Affected criteria: AC-3 Expiration boundary.
- Status: Active
- Source: pilot1 behaviour-decisions.md D-005.

### D-006 - State is process-local and in-memory

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Clarify whether the pilot requires durability or shared
  deduplication across instances.
- Engineer decision: The pilot uses in-memory, single-process state only.
  State is not shared between instances or retained after restart.
- Affected criteria: Scope; AC-8 Process-local behavior; Out of scope.
- Status: Active
- Source: pilot1 behaviour-decisions.md D-006.

### D-007 - Expected O(1) is the target; linear worst case is allowed

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Strict worst-case O(1) for arbitrary identifiers conflicts
  with a conventional exact hash-table approach and accepting every ID until
  capacity.
- Engineer decision: Linear worst-case complexity is acceptable; expected
  O(1) is the performance target. GUIDs are the identifier domain.
- Affected criteria: AC-4 GUID identity; AC-9 Complexity.
- Status: Active
- Source: pilot1 behaviour-decisions.md D-007.

### D-008 - Normalize GUIDs and reject malformed values

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Textual GUIDs can differ in casing or supported format
  while representing the same identifier.
- Engineer decision: Parse and normalize GUID representations so equivalent
  GUIDs identify the same transaction. Reject malformed strings with
  `ArgumentException`.
- Affected criteria: AC-4 GUID identity and invalid input.
- Status: Active
- Source: pilot1 behaviour-decisions.md D-008.

### D-009 - Configuration names and defaults

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Establish configuration names and defaults after
  initially leaving capacity configurable for load-test selection.
- Engineer decision: `UniqnessCheckExpiration` is configurable with a default
  of 10 seconds. `UniqnessBufferCapacity` is configurable with a default of
  1M active IDs. Load testing may inform a deployment-specific override.
- Affected criteria: Configuration; AC-6 Capacity limit; AC-10, AC-11
  Performance.
- Status: Active; refines D-003 without removing its load-test requirement.
- Source: pilot1 behaviour-decisions.md D-009.

### D-010 - Analyst does not prescribe implementation data structures

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Implementation strategy belongs to design, not analysis.
- Engineer decision: The analyst specifies behavior and constraints, not a
  data structure. The design approach is proposed for engineer approval (in
  pilot 2, by the architect).
- Affected criteria: AC-9; Out of scope.
- Status: Active
- Source: pilot1 behaviour-decisions.md D-010.

### D-011 - Throughput and duplicate-mix target

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Make the performance requirement measurable by defining
  total call rate and duplicate proportion.
- Engineer decision: Target 100,000 total calls per second, with 80% of calls
  being duplicates and 20% new IDs. Record benchmark environment, duration,
  throughput, and relevant latency; do not claim success without measurements.
- Affected criteria: AC-10 Throughput target; AC-11 Performance verification.
- Status: Active
- Source: pilot1 behaviour-decisions.md D-011.

### D-012 - Keep technical API choices out of business acceptance criteria

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: 2026-10-15T14:15:21+02:00
- Context/question: The capacity criterion specified a particular exception
  type, which is an API/technical choice rather than business behavior.
- Engineer decision: Business acceptance criteria describe the outcome:
  reject a new unseen ID at capacity without changing active state, while
  continuing to recognize active duplicates. Keep the mechanism for
  communicating capacity rejection out of business criteria. Preserve D-002
  as a separately recorded API/design decision.
- Affected criteria: AC-6 Capacity limit; classification of D-002, D-004,
  D-008, D-013 mechanisms as design constraints.
- Status: Active
- Source: pilot1 behaviour-decisions.md D-012 (same decision also recorded
  as pilot1 engineering-decisions.md D-012).

### D-013 - Invalid configuration is reported on the operation path

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Pilot 1's original handoff allowed invalid configuration
  to be reported during configuration or construction, while the engineer
  requires constructors not to throw. This entry is missing from the pilot 1
  analyst log; the engineer confirmed on 2026-10-19T21:56:43+02:00 that pilot
  2 reuses it.
- Engineer decision: Configuration values must be positive. Invalid
  configuration is reported clearly from an operation call before any state
  changes. The constructor must not throw for invalid configuration.
- Affected criteria: Configuration; AC-12 Invalid configuration.
- Status: Active
- Source: pilot1 engineering-decisions.md D-013.

### D-014 - Expired IDs are reclaimed before the capacity check

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: 2026-10-19T22:07:57+02:00
- Context/question: Pilot 1's approved criterion 5 required expired IDs to be
  reclaimed before applying the capacity check for a new ID, but no decision
  log entry backed it. Should pilot 2 adopt it?
- Engineer decision: Adopt. Expired IDs never count toward capacity: a new
  ID is rejected for capacity only if the buffer is full of IDs that are
  still active at the time of the call. Stale entries and expiry bookkeeping
  must not grow without bound.
- Affected criteria: AC-5 Expired-entry reclamation; AC-6 Capacity limit.
- Status: Active

### D-015 - Racing calls for the same unseen ID accept it at most once

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: 2026-10-19T22:07:57+02:00
- Context/question: Pilot 1's approved criterion 7 defined race semantics
  for concurrent calls with the same previously unseen ID, but no decision
  log entry backed it. Should pilot 2 adopt it?
- Engineer decision: Adopt. When concurrent calls race for the same unseen
  ID, at most one returns true (exactly one when capacity allows the ID);
  every other call observes it as a duplicate. Capacity enforcement,
  duplicate refresh, and expiration stay consistent under concurrent calls.
- Affected criteria: AC-7 Thread safety.
- Status: Active

### D-016 - Allocation and latency are measured and reported, not gated

- Recorded at: 2026-10-19T22:10:00+02:00
- Decision time: 2026-10-19T22:08:43+02:00
- Context/question: The task asks for low managed-heap allocation (GC Gen 0/1
  friendly) and D-011 asks only to record latency; neither has a pass/fail
  threshold.
- Engineer decision: The benchmark records allocated bytes per call on the
  duplicate and new-ID paths, Gen 0/1 GC collection counts, and p50/p99
  latency. No pass/fail threshold is set; the engineer judges the results.
- Affected criteria: AC-11 Performance verification.
- Status: Active

## Design constraints for the architect

Engineer decisions that are API or technical choices, kept out of the
acceptance criteria:

- D-002: capacity rejection uses a dedicated `CapacityExceededException`;
  never `OutOfMemoryException`.
- D-004, D-008: null, empty, whitespace-only, and malformed GUID input throws
  `ArgumentException`.
- D-009: configuration names `UniqnessCheckExpiration` and
  `UniqnessBufferCapacity` (spelling as decided in pilot 1).
- D-013: the constructor does not throw for invalid configuration; the error
  surfaces from the operation call.
- D-010: the analyst prescribes no data structure, locking, or eviction
  algorithm.

The task also asks to prefer non-blocking or lightweight lock-free
synchronization where possible. That is task guidance for design, not an
engineer decision or an acceptance criterion.

## Consistency review

- D-001 and D-005 agree: expiration is measured from the most recent attempt
  (accepted or duplicate), and the ID is expired at exactly that time plus
  `UniqnessCheckExpiration`.
- D-003 and D-009 are compatible: capacity is configurable, defaults to 1M,
  and a deployment value may come from load testing.
- D-007 targets expected O(1) and allows linear worst case; it prescribes no
  data structure, consistent with D-010.
- D-012 reclassifies the mechanisms in D-002, D-004, D-008 and D-013 as
  design constraints. The behaviors (reject, no state change, distinguishable
  outcome) stay in the criteria; nothing is reversed.
- D-013 refines pilot 1's original "during configuration or construction"
  wording; pilot 2 criteria use only the D-013 timing.
- D-014 is consistent with D-012: reclamation happens first, so only active
  IDs can cause a capacity rejection.
- D-015 is consistent with D-001 and D-014: losers of a race see a duplicate
  (and refresh expiration); at capacity, no racer is accepted.
- D-016 extends D-011 with allocation and GC measurements and keeps its
  "record, do not claim without measurement" stance.
- Pilot 1 recorded two D-012 entries (analyst log and engineering log) with
  the same substance; D-012 here reuses both.

No unresolved contradiction remains.
