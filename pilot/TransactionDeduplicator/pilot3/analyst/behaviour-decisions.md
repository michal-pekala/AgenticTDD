# Behaviour decisions log

Append-only history of explicit engineer decisions for the
TransactionDeduplicator pilot 3 analysis. Pilot 3 implements the same
customer task (`pilot/TransactionDeduplicator/TransactionDeduplicator.txt`)
as pilots 1 and 2, in Java 17 instead of C#. It reuses the earlier engineer
decisions as answers and asks only about gaps that the change of language
opens.

Sources of reused entries:

- `pilot2 behaviour-decisions.md` =
  `pilot/TransactionDeduplicator/pilot2/analyst/behaviour-decisions.md`
- `pilot1 behaviour-decisions.md` =
  `pilot/TransactionDeduplicator/pilot1/analyst/behaviour-decisions.md`
- `pilot1 engineering-decisions.md` =
  `pilot/TransactionDeduplicator/pilot1/engineering/engineering-decisions.md`

Pilot 2's log already reused all of pilot 1's analyst decisions; entries
D-001 to D-016 below copy pilot 2's entries and name both sources where
pilot 2 itself reused pilot 1. Reused entries carry the original decision
time only where the source captured it. "Recorded at" is when the entry was
written into this log.

## Models

- Analyst session (2026-11-24): Opus 5.5 (`claude-opus-5-5`). Confirmed by
  the engineer at the start of the analysis.

## Decisions

### D-001 - Duplicate attempts refresh expiration

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Define whether a repeated transaction ID extends its
  active period or expires based on its first accepted attempt.
- Engineer decision: Every duplicate attempt refreshes that ID's expiration.
- Affected criteria: AC-2 Active duplicate; AC-3 Expiration boundary.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-001 (from pilot1
  behaviour-decisions.md D-001; restoration confirmed by pilot1
  engineering-decisions.md C-044).

### D-002 - Capacity exhaustion uses a dedicated exception (design constraint)

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Exact deduplication with bounded memory needs behavior
  when the configured maximum is reached; deliberately throwing
  `OutOfMemoryException` would misrepresent runtime memory exhaustion.
- Engineer decision: Use a dedicated `CapacityExceededException` for a new ID
  when capacity is full. Do not throw `OutOfMemoryException`.
- Affected criteria: AC-6 Capacity limit (mechanism only).
- Status: Active as a design constraint for the architect; not a business
  acceptance criterion (see D-012). The Java mapping is left to the
  architect (D-019).
- Source: pilot2 behaviour-decisions.md D-002 (from pilot1
  behaviour-decisions.md D-002).

### D-003 - Capacity is configurable and measured

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: The maximum active-ID count needs to be testable and
  appropriate for the deployment workload.
- Engineer decision: Make capacity configurable; determine a suitable
  production-like configured value using a load test.
- Affected criteria: Configuration; AC-6 Capacity limit; AC-11 Performance
  verification.
- Status: Active; refined by D-009, which specifies the default value.
- Source: pilot2 behaviour-decisions.md D-003 (from pilot1
  behaviour-decisions.md D-003).

### D-004 - Invalid IDs are rejected without changing state

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Define behavior for null, empty, or whitespace-only
  transaction IDs.
- Engineer decision: Throw `ArgumentException` and do not change stored state.
- Affected criteria: AC-4 GUID identity and invalid input. The behavior
  (rejected, no state change) is in the criterion; the exception type is a
  design constraint for the architect, mapped to Java by the architect
  (D-019).
- Status: Active; malformed GUID handling is further specified by D-008.
- Source: pilot2 behaviour-decisions.md D-004 (from pilot1
  behaviour-decisions.md D-004).

### D-005 - Expire at the exact boundary

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Determine whether an ID is still active at exactly
  last-attempt time plus the expiration duration.
- Engineer decision: The ID expires at the exact boundary and may be accepted
  again at that instant.
- Affected criteria: AC-3 Expiration boundary.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-005 (from pilot1
  behaviour-decisions.md D-005).

### D-006 - State is process-local and in-memory

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Clarify whether the pilot requires durability or shared
  deduplication across instances.
- Engineer decision: The pilot uses in-memory, single-process state only.
  State is not shared between instances or retained after restart.
- Affected criteria: Scope; AC-8 Process-local behavior; Out of scope.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-006 (from pilot1
  behaviour-decisions.md D-006).

### D-007 - Expected O(1) is the target; linear worst case is allowed

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Strict worst-case O(1) for arbitrary identifiers conflicts
  with a conventional exact hash-table approach and accepting every ID until
  capacity.
- Engineer decision: Linear worst-case complexity is acceptable; expected
  O(1) is the performance target. GUIDs are the identifier domain.
- Affected criteria: AC-4 GUID identity; AC-9 Complexity.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-007 (from pilot1
  behaviour-decisions.md D-007).

### D-008 - Normalize GUIDs and reject malformed values

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Textual GUIDs can differ in casing or supported format
  while representing the same identifier.
- Engineer decision: Parse and normalize GUID representations so equivalent
  GUIDs identify the same transaction. Reject malformed strings with
  `ArgumentException`.
- Affected criteria: AC-4 GUID identity and invalid input.
- Status: Active; the supported formats are fixed by D-017, and the
  exception type is mapped to Java by the architect (D-019).
- Source: pilot2 behaviour-decisions.md D-008 (from pilot1
  behaviour-decisions.md D-008).

### D-009 - Configuration names and defaults

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Establish configuration names and defaults after
  initially leaving capacity configurable for load-test selection.
- Engineer decision: `UniqnessCheckExpiration` is configurable with a default
  of 10 seconds. `UniqnessBufferCapacity` is configurable with a default of
  1M active IDs. Load testing may inform a deployment-specific override.
- Affected criteria: Configuration; AC-6 Capacity limit; AC-10, AC-11
  Performance.
- Status: Active; refines D-003 without removing its load-test requirement.
  The Java casing of the names is left to the architect (D-019); pilot 1's
  spelling ("Uniqness") stands.
- Source: pilot2 behaviour-decisions.md D-009 (from pilot1
  behaviour-decisions.md D-009).

### D-010 - Analyst does not prescribe implementation data structures

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Implementation strategy belongs to design, not analysis.
- Engineer decision: The analyst specifies behavior and constraints, not a
  data structure. The design approach is proposed for engineer approval by
  the architect.
- Affected criteria: AC-9; Out of scope.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-010 (from pilot1
  behaviour-decisions.md D-010).

### D-011 - Throughput and duplicate-mix target

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Make the performance requirement measurable by defining
  total call rate and duplicate proportion.
- Engineer decision: Target 100,000 total calls per second, with 80% of calls
  being duplicates and 20% new IDs. Record benchmark environment, duration,
  throughput, and relevant latency; do not claim success without measurements.
- Affected criteria: AC-10 Throughput target; AC-11 Performance verification.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-011 (from pilot1
  behaviour-decisions.md D-011).

### D-012 - Keep technical API choices out of business acceptance criteria

- Recorded at: 2026-11-24T12:17:25+01:00
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
- Source: pilot2 behaviour-decisions.md D-012 (from pilot1
  behaviour-decisions.md D-012 and pilot1 engineering-decisions.md D-012).

### D-013 - Invalid configuration is reported on the operation path

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured (retrospective)
- Context/question: Behavior when the configured values are invalid, given
  that the engineer requires constructors not to throw.
- Engineer decision: Configuration values must be positive. Invalid
  configuration is reported clearly from an operation call before any state
  changes. The constructor must not throw for invalid configuration.
- Affected criteria: Configuration; AC-12 Invalid configuration.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-013 (from pilot1
  engineering-decisions.md D-013).

### D-014 - Expired IDs are reclaimed before the capacity check

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: 2026-10-19T22:07:57+02:00
- Context/question: Whether expired IDs count toward capacity.
- Engineer decision: Expired IDs never count toward capacity: a new ID is
  rejected for capacity only if the buffer is full of IDs that are still
  active at the time of the call. Stale entries and expiry bookkeeping must
  not grow without bound.
- Affected criteria: AC-5 Expired-entry reclamation; AC-6 Capacity limit.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-014.

### D-015 - Racing calls for the same unseen ID accept it at most once

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: 2026-10-19T22:07:57+02:00
- Context/question: Race semantics for concurrent calls with the same
  previously unseen ID.
- Engineer decision: When concurrent calls race for the same unseen ID, at
  most one returns true (exactly one when capacity allows the ID); every
  other call observes it as a duplicate. Capacity enforcement, duplicate
  refresh, and expiration stay consistent under concurrent calls.
- Affected criteria: AC-7 Thread safety.
- Status: Active
- Source: pilot2 behaviour-decisions.md D-015.

### D-016 - Allocation and latency are measured and reported, not gated

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: 2026-10-19T22:08:43+02:00
- Context/question: The task asks for low managed-heap allocation (GC Gen 0/1
  friendly) and D-011 asks only to record latency; neither has a pass/fail
  threshold.
- Engineer decision: The benchmark records allocated bytes per call on the
  duplicate and new-ID paths, Gen 0/1 GC collection counts, and p50/p99
  latency. No pass/fail threshold is set; the engineer judges the results.
- Affected criteria: AC-11 Performance verification.
- Status: Active; the GC metrics for the JVM are specified by D-018.
- Source: pilot2 behaviour-decisions.md D-016.

### D-017 - Supported GUID text formats match pilot 2

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured exactly; answered shortly before
  2026-11-24T12:14:54+01:00
- Context/question: Pilot 2 defined "supported GUID text format" as what
  .NET's `Guid.TryParse` accepts: N (32 hex digits), D (hyphenated
  8-4-4-4-12), B (D in braces), P (D in parentheses) and X
  (`{0x........,0x....,0x....,{0x..,...}}`), any letter casing, with
  surrounding whitespace. Java 17's `UUID.fromString` accepts only the
  hyphenated form, rejects surrounding whitespace, and accepts non-GUIDs
  such as `1-1-1-1-1` (verified on this machine). Which formats does pilot
  3 accept?
- Engineer decision: The same set as pilot 2, named explicitly: N, D, B, P
  and X formats, any letter casing, surrounding whitespace allowed. Every
  other string, including `1-1-1-1-1` and other short hex groups, is
  malformed. Equal GUIDs in any accepted format are the same transaction
  (D-008).
- Affected criteria: AC-4 GUID identity and invalid input.
- Status: Active; makes D-008's "supported format" exact for pilot 3.

### D-018 - JVM garbage-collection metrics replace Gen 0/1 counts

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured exactly; answered shortly before
  2026-11-24T12:16:30+01:00
- Context/question: D-016 asks for .NET Gen 0 and Gen 1 collection counts.
  The JVM has no such generations; its collectors report young-generation
  and old/full collections, and results depend on the collector and heap
  size.
- Engineer decision: The benchmark records young-generation and old/full
  collection counts and total GC pause time, and records the collector and
  heap settings as part of the environment. Allocated bytes per call (on the
  duplicate and new-ID paths) and p50/p99 latency stay as in D-016. Results
  are reported, not gated.
- Affected criteria: AC-11 Performance verification.
- Status: Active; replaces only the Gen 0/1 part of D-016.

### D-019 - The architect maps .NET-named constraints to Java

- Recorded at: 2026-11-24T12:17:25+01:00
- Decision time: Not captured exactly; answered shortly before
  2026-11-24T12:17:25+01:00
- Context/question: The reused design constraints name .NET things:
  `ArgumentException` (D-004, D-008), "never `OutOfMemoryException`"
  (D-002), the C#-style entry point `IsUniqueAndProcess` and the
  configuration names `UniqnessCheckExpiration` and `UniqnessBufferCapacity`
  (D-009). Decide the Java mapping now, or leave it to the architect?
- Engineer decision: Keep the reused constraints' intent and leave the Java
  mapping to the architect, who records it as engineering decisions (for
  example an `IllegalArgumentException` family for bad IDs, never
  `OutOfMemoryError`, Java casing such as `isUniqueAndProcess`). The
  acceptance criteria name concepts, not Java types. Behavior is unchanged:
  invalid input and capacity rejections stay distinguishable and change no
  state.
- Affected criteria: Scope; Configuration; AC-4; AC-6; AC-12 (mechanisms
  only).
- Status: Active

### D-020 - Criteria approved, including their assumptions

- Recorded at: 2026-11-24T12:31:28+01:00
- Decision time: Not captured exactly; answered shortly before
  2026-11-24T12:31:28+01:00
- Context/question: Approve the pilot 3 acceptance criteria AC-1 to AC-12,
  including two assumptions carried from pilot 2's design: the all-zero
  GUID is a valid ID, and the architect names the exact set of
  surrounding-whitespace characters accepted around a GUID (AC-4).
- Engineer decision: Approved as written, including both assumptions.
- Affected criteria: All; AC-4 assumptions.
- Status: Active

### D-021 - Criteria document is Markdown

- Recorded at: 2026-11-24T12:34:48+01:00
- Decision time: Not captured exactly; answered shortly before
  2026-11-24T12:34:48+01:00
- Context/question: The analyst skill asked for a plain-text criteria file
  (`TransactionDeduplicator-Acceptance-Criteria.txt`). The engineer asked
  for Markdown instead and for the analyst skill to be updated.
- Engineer decision: The criteria document is
  `TransactionDeduplicator-Acceptance-Criteria.md`, in Markdown. The
  approved content (D-020) is unchanged; only the format changed. The
  `.txt` file is removed.
- Affected criteria: None (format only).
- Status: Active

## Design constraints for the architect

Engineer decisions that are API or technical choices, kept out of the
acceptance criteria. Per D-019, the architect maps each to Java and records
the mapping as an engineering decision:

- D-002: capacity rejection uses a dedicated `CapacityExceededException`;
  never the runtime's out-of-memory error (`OutOfMemoryException` in .NET,
  `OutOfMemoryError` in Java).
- D-004, D-008: null, empty, whitespace-only, and malformed GUID input is
  rejected with the platform's argument exception (`ArgumentException` in
  .NET).
- D-009: configuration names `UniqnessCheckExpiration` and
  `UniqnessBufferCapacity` (pilot 1 spelling), in Java casing.
- D-013: the constructor does not throw for invalid configuration; the error
  surfaces from the operation call.
- D-017: the supported GUID formats are named explicitly; Java's
  `UUID.fromString` alone does not implement them (it is narrower in format
  and more lenient in group length).
- D-010: the analyst prescribes no data structure, locking, or eviction
  algorithm.
- The entry point is the task's `IsUniqueAndProcess(string transactionId)`
  returning a boolean, in Java naming (D-019).

The task also asks to prefer non-blocking or lightweight lock-free
synchronization where possible. That is task guidance for design, not an
engineer decision or an acceptance criterion.

Pilot 2's engineering decisions (for example invalid configuration
exception type, error messages, options read once at construction, the
per-ID claim for same-ID races) are engineering decisions, not analyst
decisions; the architect decides whether to reuse them.

## Consistency review

- D-001 and D-005 agree: expiration is measured from the most recent attempt
  (accepted or duplicate), and the ID is expired at exactly that time plus
  `UniqnessCheckExpiration`.
- D-003 and D-009 are compatible: capacity is configurable, defaults to 1M,
  and a deployment value may come from load testing.
- D-007 targets expected O(1) and allows linear worst case; it prescribes no
  data structure, consistent with D-010.
- D-012 classifies the mechanisms in D-002, D-004, D-008 and D-013 as
  design constraints. The behaviors (reject, no state change, distinguishable
  outcome) stay in the criteria.
- D-014 is consistent with D-012: reclamation happens first, so only active
  IDs can cause a capacity rejection.
- D-015 is consistent with D-001 and D-014: losers of a race see a duplicate
  (and refresh expiration); at capacity, no racer is accepted.
- D-017 refines D-008 without changing it: equivalent GUIDs are still one
  transaction and malformed strings are still rejected; it only names the
  accepted formats, which are the same as pilot 2's.
- D-018 replaces only the .NET-specific Gen 0/1 metric in D-016; the rest of
  D-016 and D-011 holds, including "record, do not claim without
  measurement".
- D-019 changes no behavior; it moves the Java naming of D-002, D-004,
  D-008 and D-009 mechanisms to the architect, consistent with D-012.
- D-020 confirms the criteria and the two AC-4 assumptions; both agree with
  D-017 (pilot 2's accepted set, which includes the all-zero GUID and
  surrounding whitespace).

No unresolved contradiction remains.
