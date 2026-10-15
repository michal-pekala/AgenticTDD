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

### D-012 - Capacity behavior criteria exclude API mechanism

- Recorded at: 2026-10-15T14:17:36+02:00
- Decision time: Not captured (retrospective)
- Context/question: The analyst handoff described capacity exhaustion by
  naming `CapacityExceededException`, which prescribes a technical/API
  mechanism instead of stating only the required behavior.
- Engineer decision: Keep D-002 in the decision history as an approved
  API/design decision requiring a dedicated capacity exception, but do not
  make the exception type a business acceptance criterion. Criterion 6 must
  state that an unseen ID is not accepted and active state is unchanged after
  expiry reclamation; active duplicates remain recognized. The analyst skill
  must keep approved technical/API decisions separate from behavioral
  criteria.
- Affected criteria: Capacity limit (criterion 6); analyst skill guidance;
  coder design handoff.
- Status: Active; refines the classification of D-002 without deleting or
  reversing its approved API/design constraint.

### D-013 - Invalid configuration is reported on the operation path

- Recorded at: 2026-10-15T14:17:36+02:00
- Decision time: Not captured (retrospective)
- Context/question: The prior handoff allowed invalid configuration to be
  reported during configuration or construction, while the engineer requires
  constructors to avoid throwing and configuration errors to surface during
  execution.
- Engineer decision: Configuration values remain required to be positive;
  invalid configuration is reported clearly from an operation call before
  state changes. The constructor must not throw for invalid configuration.
- Affected criteria: Configuration; constructor and operation behavior.
- Status: Active; refines invalid-configuration error timing.

## Consistency review after D-012 and D-013

D-012 does not remove D-002: the dedicated exception remains an approved
API/design constraint, distinct from business acceptance criterion 6.
D-013 changes the handoff's invalid-configuration timing to the operation
path while preserving positive-value validation. The revised handoff and
analyst skill reflect both decisions; no unresolved contradiction remains.

## Coding workflow decisions

The following decisions were made for the coder pilot. Their original
decision times were not captured; timestamps record when they were entered
into this log.

### C-001 - Console application and separated project directories

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Establish the pilot's runnable project shape and separate
  production and test projects.
- Engineer decision: Implement the pilot as a C# console application, create
  a solution file, and place source and test projects in separate
  directories.
- Affected work: `pilot/coder` solution and project layout.
- Status: Active

### C-002 - Separate validation and parsing helpers

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Keep input and configuration checks from obscuring the
  core deduplication path.
- Engineer decision: Extract GUID parsing and other parameter/configuration
  validation from the core method into separate helpers.
- Affected work: Public API implementation and tests.
- Status: Active

### C-003 - Prefer concurrent dictionary with background cleanup

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Reduce the contention risk of a global lock and keep
  collection cleaning off the normal call path where possible.
- Engineer decision: Prefer a `ConcurrentDictionary` approach over a global
  lock and delegate collection cleaning to a separate background component.
  Compare performance before claiming the preferred approach is faster.
- Affected work: Design and comparative benchmarks.
- Status: Active preference, subject to measured results and engineer design
  approval.

### C-004 - Refresh analyst files from main

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Ensure coding work consumes the latest approved analyst
  handoff.
- Engineer decision: Refresh the analyst artifacts from the main branch in
  this worktree before coding.
- Affected work: `pilot/analyst` handoff files.
- Status: Active; refreshed from `origin/main` in this worktree, then updated
  per D-012 and D-013.

### C-005 - Keep coding artifacts under pilot/coder

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Keep pilot implementation and related engineering
  artifacts scoped to a dedicated directory.
- Engineer decision: Place all coding-related changes under `pilot/coder`.
- Affected work: Solution, source, tests, benchmarks, reports, design, and
  coder decision log.
- Status: Active

### C-006 - Commit and approve each TDD stage

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Make each incremental TDD step reviewable and explicitly
  supervised.
- Engineer decision: For each criterion, commit the RED test, then commit the
  GREEN implementation, then commit the refactor. Obtain engineer approval
  after each commit before continuing.
- Affected work: Implementation sequence and commit/approval gates.
- Status: Active; takes precedence over README guidance that a failing RED
  test need not be committed.

### C-007 - Compare benchmark strategies and measure memory

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Verify the concurrency preference empirically under
  representative multithreaded loads.
- Engineer decision: Benchmark global-lock and ConcurrentDictionary
  approaches with multiple threads. Start with small pilot workloads, then
  scale up; measure memory as well as performance.
- Affected work: Benchmark project and strategy selection evidence.
- Status: Active

### C-008 - Report every benchmark run

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Make benchmark results reproducible and avoid unsupported
  performance claims.
- Engineer decision: Generate a detailed report for each benchmark run.
- Affected work: Benchmark report generation and stored run artifacts.
- Status: Active

### C-009 - Repository boundary

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Limit the pilot to the intended repository.
- Engineer decision: Work only in AgenticTDD. Do not access or modify the
  separate TransactionDeduplicator learning repository.
- Affected work: Repository and filesystem scope.
- Status: Active

### C-010 - Separate design and implementation approvals

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Keep implementation behind explicit engineer review of
  the proposed solution design.
- Engineer decision: Present the design and wait for engineer approval before
  implementation. Design approval alone does not bypass the separate
  per-commit approvals in C-006.
- Affected work: Design handoff and implementation gate.
- Status: Active

### C-011 - Keep an append-only coder decision log

- Recorded at: 2026-10-15T14:19:12+02:00
- Decision time: Not captured (retrospective)
- Context/question: Make all engineer directions available to the coding
  agent in one durable project artifact.
- Engineer decision: Record all explicit engineer decisions in a log under
  `pilot/coder`, preserving the prior behavior-decision history and appending
  new decisions rather than erasing or rewriting history.
- Affected work: This file and future design/implementation handoffs.
- Status: Active

## Consistency review after coder decisions

C-003 is a preference whose performance must be demonstrated; C-007 requires
the global-lock comparison. C-004 refreshed the latest analyst artifacts in
this worktree. D-002 remains an API/design constraint, distinct from business
acceptance criterion 6 under D-012. No new contradiction with D-013 was
identified.

### C-012 - Consolidate engineer decisions in the coding skill

- Recorded at: 2026-10-15T14:20:22+02:00
- Decision time: Not captured (retrospective)
- Context/question: Ensure the coder can find and consistently apply the
  engineer's general project, workflow, approval, and design decisions.
- Engineer decision: Add a coding skill under `pilot/coder` containing all
  general engineer decisions, with the append-only engineering decision log
  remaining the detailed history.
- Affected work: `pilot/coder/engineering/SKILL.md` and future coding handoffs.
- Status: Active

### C-013 - Keep changes out of the main checkout

- Recorded at: 2026-10-15T14:21:06+02:00
- Decision time: Not captured (retrospective)
- Context/question: Protect the user's primary working copy while preparing
  pilot artifacts.
- Engineer decision: Make changes only in the active AgenticTDD worktree; do
  not write to the user's main checkout.
- Affected work: All repository file operations.
- Status: Active

### C-014 - Approve the revised design

- Recorded at: 2026-10-15T17:03:20+02:00
- Decision time: 2026-10-15T17:03:20+02:00
- Context/question: Approval request for the revised solution design and the
  separation of D-002 from business acceptance criterion 6.
- Engineer decision: Approve the revised design. Proceed with implementation
  only through the separately specified TDD stages and engineer approval
  after each commit.
- Affected work: Design approval gate; implementation start.
- Status: Active

### C-026 - Group coder guidance and design artifacts

- Recorded at: 2026-10-16T13:15:19+02:00
- Decision time: 2026-10-16T13:15:19+02:00
- Context/question: Group the coder's engineering decisions, skill guidance,
  and design proposal separately from implementation files.
- Engineer decision: Keep `engineering-decisions.md`, `SKILL.md`, and
  `TransactionDeduplicator-Design.md` under `pilot/coder/engineering/`.
- Affected work: Coder documentation paths and references.
- Status: Active.

### C-028 - Record engineering metadata in the setup commit

- Recorded at: 2026-10-16T13:57:04+02:00
- Decision time: 2026-10-16T13:48:23+02:00
- Context/question: Keep engineering guidance and design available from the
  start of implementation and distinguish their setup commit from focused
  TDD stage commits.
- Engineer decision: Initialize `engineering-decisions.md`, `SKILL.md`, and
  `TransactionDeduplicator-Design.md` together in the pilot setup commit
  titled `Add engineering metadata`.
- Affected work: Pilot commit history and coder engineering documentation.
- Status: Active.

### C-029 - Show engineering documents in a solution folder

- Recorded at: 2026-10-16T14:08:01+02:00
- Decision time: 2026-10-16T14:08:01+02:00
- Context/question: Make the pilot's engineering documentation easy to find
  in Visual Studio alongside the solution projects.
- Engineer decision: Add an `engineering` solution folder and include the
  coding skill, design document, and engineering decision log as solution
  items.
- Affected work: `TransactionDeduplicator.sln` and engineering documentation.
- Status: Active.
