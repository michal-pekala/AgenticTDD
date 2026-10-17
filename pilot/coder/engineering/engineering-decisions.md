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
- Status: Superseded in part by C-025; the solution and separate-directory
  requirements remain active.

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

### C-015 - Use generated GUIDs in generic happy-path tests

- Recorded at: 2026-10-16T12:42:05+02:00
- Decision time: Not captured (retrospective)
- Context/question: A fixed GUID in a general first-seen test could be
  mistaken for a special whitelisted value rather than an example of the
  supported GUID domain.
- Engineer decision: Use `Guid.NewGuid().ToString()` in generic happy-path
  tests to make clear that any valid GUID serialized in the default format is
  accepted. Use fixed GUIDs only when identity or formatting is the subject
  of the test. Add this guidance to the coding skill.
- Affected work: First-seen test data and `pilot/coder/engineering/SKILL.md`.
- Status: Active

### C-016 - Keep simple tests expression-bodied

- Recorded at: 2026-10-16T12:52:51+02:00
- Decision time: 2026-10-16T12:52:51+02:00
- Context/question: Reduce boilerplate in the growing test suite without
  hiding the act/assert behavior behind another abstraction.
- Engineer decision: Keep the system under test as a private readonly
  class-level property named `Sut`. For a test with one operation and one
  assertion, use an expression-bodied test that directly chains the operation
  into the assertion; do not use an intermediate result variable or add
  `Act()`/`Assert()` helper methods.
- Affected work: First-seen test and `pilot/coder/engineering/SKILL.md`.
- Status: Active

### C-017 - Consolidate all engineer decisions in the coding skill

- Recorded at: 2026-10-16T12:53:23+02:00
- Decision time: 2026-10-16T12:53:23+02:00
- Context/question: The coding skill summarized some decisions but did not
  yet provide a single, self-contained checklist of the complete engineer
  decision set.
- Engineer decision: Include all applicable engineer decisions in
  `pilot/coder/engineering/SKILL.md`, while retaining `engineering-decisions.md` as the
  detailed, append-only history.
- Affected work: `pilot/coder/engineering/SKILL.md` and future coding handoffs.
- Status: Active

### C-018 - Put expression-bodied test arrow on its own line

- Recorded at: 2026-10-16T12:54:21+02:00
- Decision time: 2026-10-16T12:54:21+02:00
- Context/question: Make the act/assert chain easier to scan in concise
  expression-bodied tests.
- Engineer decision: Place `=>` on a separate line after the test method
  declaration, then indent `Sut` and the operation/assertion chain beneath
  it.
- Affected work: First-seen test and `pilot/coder/engineering/SKILL.md`.
- Status: Active

### C-019 - Align operation chain with Sut in expression-bodied tests

- Recorded at: 2026-10-16T12:55:18+02:00
- Decision time: 2026-10-16T12:55:18+02:00
- Context/question: Clarify the exact whitespace/layout for the preferred
  concise act/assert expression after reviewing the formatted example.
- Engineer decision: Put the method declaration on its own line, `=> Sut` on
  the next line, and each chained operation/assertion on its own subsequent
  line aligned with `Sut`.
- Affected work: First-seen test and `pilot/coder/engineering/SKILL.md`.
- Status: Active; clarifies C-018.

### C-020 - Indent chained calls beneath expression-bodied Sut

- Recorded at: 2026-10-16T12:55:40+02:00
- Decision time: 2026-10-16T12:55:40+02:00
- Context/question: Specify the exact indentation in the approved
  expression-bodied test layout.
- Engineer decision: The method declaration is followed by a line containing
  `=> Sut`; each chained operation/assertion is on a following line indented
  one level further than the `=> Sut` line.
- Affected work: First-seen test and `pilot/coder/engineering/SKILL.md`.
- Status: Active; clarifies C-019.

### C-021 - Keep simple FluentAssertions chains on one line

- Recorded at: 2026-10-16T12:55:54+02:00
- Decision time: 2026-10-16T12:55:54+02:00
- Context/question: Avoid unnecessary line breaks in a short assertion
  expression.
- Engineer decision: Keep simple FluentAssertions chains, such as
  `.Should().BeTrue()`, on one line.
- Affected work: First-seen test and `pilot/coder/engineering/SKILL.md`.
- Status: Active

### C-023 - Keep the first-seen GREEN change minimal

- Recorded at: 2026-10-16T13:03:14+02:00
- Decision time: 2026-10-16T13:03:14+02:00
- Context/question: The first-seen RED test only asserts that processing a
  valid GUID returns true. The initial GREEN implementation added GUID
  parsing and a ConcurrentDictionary before tests exercised those behaviors.
- Engineer decision: For this focused GREEN step, make the simplest
  one-line change that passes the test. Defer parsing and state storage until
  focused tests require them in subsequent criteria.
- Affected work: Criterion 1 GREEN implementation and following RED stages.
- Status: Active

### C-027 - Use behavioral test names and TDD commit subjects

- Recorded at: 2026-10-16T13:36:18+02:00
- Decision time: 2026-10-16T13:36:18+02:00
- Context/question: `First_valid_transaction_is_accepted` implied sequence
  history and input validation that the test does not demonstrate.
- Engineer decision: Rename the method to
  `New_transaction_can_be_processed`. Use the concise behavior name followed
  by `[RED]`, `[GREEN]`, or `[REFACTOR]` for each corresponding TDD commit
  subject. Include the method rename in the GREEN commit.
- Affected work: First-seen test, TDD commit history, and coding skill.
- Status: Active.

### C-025 - Use a class library instead of a console application

- Recorded at: 2026-10-16T13:07:11+02:00
- Decision time: 2026-10-16T13:07:11+02:00
- Context/question: The scaffolded `Program.cs` contained only the template's
  Hello World entry point and was unrelated to the component tests. Removing
  it from an executable project would leave the project without an entry
  point.
- Engineer decision: Remove the console-app requirement and use a C# class
  library for the deduplicator, retaining a separate test project and
  solution. This supersedes C-001's console-application requirement; keep
  C-001's requirement for a solution and separate source/test directories.
- Affected work: Project type/layout, solution, project references, and
  `Program.cs`.
- Status: Active; supersedes the console-app portion of C-001.

### C-030 - Co-locate TDD documentation with its implementation stage

- Recorded at: 2026-10-16T14:19:27+02:00
- Decision time: 2026-10-16T14:19:27+02:00
- Context/question: Keep engineering guidance and design/decision history
  reviewable alongside the source and test changes they explain.
- Engineer decision: For future criteria, use one behavior-focused test name
  across the RED, GREEN, and REFACTOR commit subjects, suffixed with the
  corresponding stage label. Include engineering-document and decision-log
  updates in the matching TDD-stage commit when directly related to that
  stage; keep unrelated documentation changes out of TDD commits. The setup
  commit remains `Add engineering metadata`. Preserve the RED -> GREEN ->
  REFACTOR sequence and engineer approval gate after every commit.
- Affected work: Coding skill, decision log, and subsequent TDD commits.
- Status: Active.

### C-031 - Name test classes by transaction scenario size

- Recorded at: 2026-10-16T14:30:19+02:00
- Decision time: 2026-10-16T14:30:19+02:00
- Context/question: Start test coverage with simple single-transaction
  scenarios before expanding to cases involving multiple transactions.
- Engineer decision: Rename `FirstSeenTransactionTests` to
  `SingleTransactionTests`; use a single-transaction test class for this
  initial scenario and introduce clearly named multiple-transaction test
  classes as those scenarios are added. Include the rename and guidance in
  the `[REFACTOR]` commit.
- Affected work: First-seen test class, coding skill, and REFACTOR commit.
- Status: Active.

### C-032 - Assert full invalid-ID message with generated input

- Recorded at: 2026-10-16T14:41:48+02:00
- Decision time: 2026-10-16T14:41:48+02:00
- Context/question: A fixed invalid ID and partial message match would not
  show that arbitrary non-GUID input is rejected with a useful diagnostic.
- Engineer decision: Generate an invalid string value, use the same variable
  in the operation and expected assertion, and assert the full message:
  `TransactionId value '<invalidId>' is not a valid Guid.`
- Affected work: Malformed-ID RED test and coding skill.
- Status: Active.

### C-033 - Randomize examples for generic input categories

- Recorded at: 2026-10-16T14:44:35+02:00
- Decision time: 2026-10-16T14:44:35+02:00
- Context/question: A fixed value in a generic-behavior test can imply that
  only that specific value is supported or rejected, rather than proving the
  behavior for the broader input category.
- Engineer decision: When testing generic behavior over an input category,
  use a randomized representative from that category instead of a specific
  constant. This applies to valid and invalid examples; use the same generated
  value wherever the test needs to refer to it.
- Affected work: Test data conventions and coding skill.
- Status: Active.

### C-035 - Reuse source error templates in test assertions

- Recorded at: 2026-10-16T16:08:16+02:00
- Decision time: 2026-10-16T16:08:08+02:00
- Context/question: The malformed-ID test duplicated the error-message text
  that the implementation emitted, and direct formatting required an
  unreadable call with unrelated parameters.
- Engineer decision: Define reusable error-message templates in a separate
  `ValidationError` class and reference the source constant in test
  assertions instead of duplicating the text. Format templates through a
  `StringExtensions.Format` extension to keep call sites concise and use
  invariant culture. More generally, replace unreadable constructions with
  unrelated parameters with small, meaningfully named extension methods
  focused on readable call sites.
- Affected work: Validation error messages, tests, and coding skill.
- Status: Active.

### C-036 - Skip refactors without a demonstrated benefit

- Recorded at: 2026-10-16T16:56:00+02:00
- Decision time: 2026-10-16T16:56:00+02:00
- Context/question: Lazy dictionary initialization added complexity and
  offered no tested behavior or worthwhile benefit for the current stage.
- Engineer decision: Retain simple eager dictionary initialization when its
  small allocation is acceptable. Refactoring is optional; only make
  behavior-preserving changes when a focused test or concrete readability or
  maintenance concern demonstrates a worthwhile benefit. Skip redundant or
  speculative refactors and do not create an empty REFACTOR commit.
- Affected work: TDD commit workflow, design status, and future refactor
  stages.
- Status: Active.

### C-037 - Inject a DateTimeOffset provider for expiry tests

- Recorded at: 2026-10-17T18:44:37+02:00
- Decision time: 2026-10-17T18:44:37+02:00
- Context/question: Expiration refresh and exact-boundary behavior require
  deterministic control of current time in tests.
- Engineer decision: Add an internal `IDateTimeProvider` with a minimal
  `DateTimeOffset CurrentTime` property and inject it through an internal
  constructor. The production provider reads `DateTimeOffset.UtcNow`; tests
  use a fake with explicitly advanceable time. This supersedes the monotonic
  clock proposal and accepts the risk of wall-clock adjustments.
- Affected work: Expiry behavior, test seam, and design.
- Status: Active.

### C-038 - Configure expiry through options in focused tests

- Recorded at: 2026-10-17T19:04:27+02:00
- Decision time: 2026-10-17T19:04:27+02:00
- Context/question: Expiry tests need to demonstrate that behavior follows
  the configured expiration rather than a fixed/default duration.
- Engineer decision: Use a `TransactionDeduplicatorOptions` object with
  `UniqnessCheckExpiration` and `UniqnessBufferCapacity`, and configure a
  randomized expiration explicitly in each expiry test. Keep one class-level
  `Sut` and fake date-time provider per test-class instance, expose fake time
  through a `Now` property, and split within-window duplicate and
  post-expiration acceptance assertions into separate tests.
- Affected work: Options API, expiry test structure, and coding guidance.
- Status: Active.

### C-039 - Place acts and assertions on separate lines

- Recorded at: 2026-10-17T19:10:17+02:00
- Decision time: 2026-10-17T19:10:17+02:00
- Context/question: Act and assertion expressions are easier to scan when
  visually separated.
- Engineer decision: Keep the act and its FluentAssertions assertion on
  separate physical lines in the same expression. Do not add an intermediate
  result variable solely for separation. Apply this immediately to new tests
  and when editing existing tests.
- Affected work: Test style and coding skill.
- Status: Active.

### C-040 - Defer synchronization while implementing single-threaded expiry

- Recorded at: 2026-10-17T19:27:01+02:00
- Decision time: 2026-10-17T19:27:01+02:00
- Context/question: The expiration GREEN implementation used an unbounded
  retry loop to address concurrent dictionary update/removal races, although
  current focused tests exercise only single-threaded behavior.
- Engineer decision: For the current expiration implementation, use a direct
  dictionary lookup and timestamp replacement based on whether the ID is
  present and still active. Defer thread synchronization rather than
  retaining the retry loop. This knowingly leaves the approved concurrency
  criterion unimplemented until separately tested and addressed.
- Affected work: Expiration GREEN/REFACTOR implementation and concurrency
  acceptance gap.
- Status: Active.

### C-041 - Defer duplicate expiration refresh from current scope

- Recorded at: 2026-10-17T19:31:22+02:00
- Decision time: 2026-10-17T19:31:10+02:00
- Context/question: The current focused tests verify duplicate rejection
  within the configured expiration window and acceptance at the boundary,
  but do not verify refresh from a duplicate attempt.
- Engineer decision: Remove timestamp refresh on active duplicates. Expire
  IDs based on the last accepted attempt, while still returning `false` for
  active duplicates. This supersedes the original analyst handoff and D-001
  requirement to refresh expiration; that requirement is out of the current
  implementation scope until explicitly restored with a focused test.
- Affected work: Duplicate expiration semantics, skill, and design.
- Status: Active; supersedes D-001 and the handoff's duplicate-refresh
  requirement for current scope.
