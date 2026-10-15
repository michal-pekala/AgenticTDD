# Behaviour decisions log

Append-only history of explicit engineer decisions affecting project behavior
and analyst handoffs. Timestamps below record when decisions were entered into
this log. Their original decision times were not captured, so they are marked
retrospective; no exact historical decision times are inferred.

## Decisions

### D-001 - Duplicate attempts refresh expiration

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Define whether a repeated transaction ID extends its
  active period or expires based on its first accepted attempt.
- Engineer decision: Every duplicate attempt refreshes that ID's expiration.
- Affected criteria: Active duplicate; expiration boundary.
- Status: Active

### D-002 - Capacity exhaustion uses a dedicated exception

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Exact deduplication with bounded memory needs behavior
  when the configured maximum is reached; deliberately throwing
  `OutOfMemoryException` would misrepresent runtime memory exhaustion.
- Engineer decision: Throw a dedicated `CapacityExceededException` for a new
  ID when capacity is full. Do not throw `OutOfMemoryException`.
- Affected criteria: Capacity limit; API error behavior.
- Status: Active

### D-003 - Capacity is configurable and measured

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: The initial maximum active-ID count needs to be testable
  and appropriate for the deployment workload.
- Engineer decision: Make capacity configurable; determine a suitable
  production-like configured value using a load test.
- Affected criteria: Configuration; capacity; performance verification.
- Status: Active; refined by D-009, which specifies the default value.

### D-004 - Invalid IDs throw without changing state

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Define behavior for null, empty, or whitespace-only
  transaction IDs.
- Engineer decision: Throw `ArgumentException` and do not change stored state.
- Affected criteria: GUID identity and invalid input.
- Status: Active; malformed GUID handling is further specified by D-008.

### D-005 - Expire at the exact boundary

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Determine whether an ID is still active at exactly
  last-attempt time plus the expiration duration.
- Engineer decision: The ID expires at the exact boundary and may be accepted
  again at that instant.
- Affected criteria: Expiration boundary.
- Status: Active

### D-006 - Pilot state is process-local and in-memory

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Clarify whether the pilot requires durability or shared
  deduplication across instances.
- Engineer decision: The pilot uses in-memory, single-process state only.
  State is not shared between instances or retained after restart.
- Affected criteria: Scope; process-local behavior.
- Status: Active

### D-007 - Expected O(1) is the target; linear worst case is allowed

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Strict worst-case O(1) for arbitrary identifiers conflicts
  with a conventional exact hash-table approach and accepting every ID until
  capacity.
- Engineer decision: Linear worst-case complexity is acceptable; expected
  O(1) is the performance target. GUIDs are the identifier domain.
- Affected criteria: Performance and bounded memory; identifier format.
- Status: Active

### D-008 - Normalize GUIDs and reject malformed values

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Textual GUIDs can differ in casing or supported format
  while representing the same identifier.
- Engineer decision: Parse and normalize GUID representations so equivalent
  GUIDs identify the same transaction. Reject malformed strings with
  `ArgumentException`.
- Affected criteria: GUID identity and invalid input.
- Status: Active

### D-009 - Configuration names and defaults

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Establish pilot configuration names and defaults after
  initially leaving capacity configurable for load-test selection.
- Engineer decision: `UniqnessCheckExpiration` is configurable with a default
  of 10 seconds. `UniqnessBufferCapacity` is configurable with a default of
  1M active IDs. Load testing may inform a deployment-specific override.
- Affected criteria: Configuration; capacity; performance verification.
- Status: Active; refines D-003 without removing its load-test requirement.

### D-010 - Analyst does not prescribe implementation data structures

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: The analyst handoff initially included a hash-table
  recommendation, but implementation strategy belongs to design.
- Engineer decision: The analyst specifies behavior and constraints, not a
  data structure. The coding agent proposes an approach for engineer approval.
- Affected criteria: Analyst responsibilities; coding-agent design approval.
- Status: Active

### D-011 - Throughput and duplicate-mix target

- Recorded at: 2026-10-15T12:00:51+02:00
- Decision time: Not captured (retrospective)
- Context/question: Make the performance requirement measurable by defining
  total call rate and duplicate proportion.
- Engineer decision: Target 100,000 total calls per second, with 80% of calls
  being duplicates and 20% new IDs. Record benchmark environment, duration,
  throughput, and relevant latency; do not claim success without measurements.
- Affected criteria: Performance and bounded memory; performance verification.
- Status: Active

## Consistency review

These entries were backfilled from the pilot discussion. D-003 and D-009 are
compatible: capacity remains configurable, defaults to 1M, and a
deployment-specific value can be selected or overridden based on load-test
results. D-007 allows linear worst-case behavior while targeting expected
O(1); it does not prescribe a data structure, consistent with D-010.

No unresolved contradiction was identified in the recorded decisions at the
time this log was created. Re-check the relevant sequence whenever a new
explicit engineer decision is added and before publishing revised acceptance
criteria.
