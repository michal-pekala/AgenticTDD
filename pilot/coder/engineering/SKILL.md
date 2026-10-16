# TransactionDeduplicator coding skill

## Purpose and scope

Implement the AgenticTDD TransactionDeduplicator pilot against the approved
analyst handoff, with explicit engineer control over design and each TDD
commit. Work only in the active AgenticTDD worktree; do not write to the
user's main checkout, or access or modify the separate TransactionDeduplicator
learning repository.

All coding-related changes belong under `pilot/coder`, including the solution,
source, tests, benchmarks, per-run reports, design, decision log, and this
skill. The analyst source artifacts belong under `pilot/analyst`.
Keep the coding skill, design, and engineering-decision log visible in Visual
Studio under the `engineering` solution folder.

## Source of truth and decision recording

- Use `pilot/analyst/TransactionDeduplicator-Acceptance-Criteria.txt` for
  approved business behavior and `pilot/analyst/behaviour-decisions.md` for
  the analyst's behavior-decision history.
- `pilot/coder/engineering/engineering-decisions.md` is the append-only record of all
  explicit engineer decisions for the coding work. Read it before design or
  implementation. Append a timestamped entry before relying on a new explicit
  engineer decision; preserve prior entries. If the original decision time is
  not known, record the entry time with its UTC offset and mark the original
  time as not captured (retrospective).
- Keep recommendations and unapproved design choices labeled as proposals.
  Do not silently turn them into requirements or decisions.

## Consolidated engineer decisions

This section consolidates the active decisions in
`pilot/coder/engineering-decisions.md` and the approved behavior decisions
D-001 through D-013 in `pilot/analyst/behaviour-decisions.md`. The decision
logs remain the detailed historical record; this skill is the coding
checklist. If a newer decision changes one of these instructions, append it
to the coder log and update this section without deleting the history.

### Required behavior and API/design constraints

- Implement `bool IsUniqueAndProcess(string transactionId)` for an in-memory,
  process-local instance. Do not claim persistence across restarts, sharing
  between instances/processes/machines, or ACID guarantees.
- Accept valid GUID values. Use generated default-format GUIDs
  (`Guid.NewGuid().ToString()`) for generic happy-path tests. Normalize
  supported GUID representations so equivalent values identify one
  transaction. Use fixed GUIDs only when identity or formatting itself is
  under test.
- Null, empty, whitespace-only, and malformed IDs throw `ArgumentException`
  without changing state.
- A first-seen active-window ID returns `true`. An active duplicate returns
  `false` and refreshes expiration from that duplicate attempt. Expiration is
  exactly at last attempt plus configured duration; at the boundary the ID
  is expired and can be accepted again.
- `UniqnessCheckExpiration` defaults to 10 seconds and
  `UniqnessBufferCapacity` defaults to 1,000,000 active IDs. Both are
  configurable and positive. Invalid configuration is reported clearly from
  an operation call before state changes; the constructor does not throw for
  invalid options.
- Reclaim expired IDs before capacity handling. Active duplicates remain
  recognized at capacity. If a valid unseen ID arrives when capacity remains
  full after reclamation, do not accept it or change active state.
- D-002 separately requires a dedicated `CapacityExceededException`, not
  `OutOfMemoryException`. This is an approved API/design constraint, not a
  business acceptance criterion. Test the behavioral result (rejected new
  ID, unchanged active state, duplicates still recognized) separately from
  the exception-type API contract.
- Calls must be thread-safe: concurrent first calls for the same unseen ID
  produce at most one `true`, and expiration, refresh, and capacity remain
  consistent.
- Aim for expected O(1), while recognizing linear worst-case behavior is
  allowed. Retained IDs and expiration bookkeeping must stay bounded by
  configured capacity, including under repeated duplicate refresh.

### Repository, project, and design decisions

- Work only in the active AgenticTDD worktree. Never write to the user's main
  checkout and never access or modify the separate
  TransactionDeduplicator learning repository.
- Keep all coding-related artifacts under `pilot/coder`: the solution,
  source, tests, benchmarks, reports, design, decision log, and coding skill.
  The analyst handoff remains under `pilot/analyst`; refresh it from main
  before coding when directed.
- Build a C# console application and solution. Keep source and test projects
  in separate directories. The approved target is `net10.0` based on SDK
  10.0.400. Use XUnit and FluentAssertions if compatible.
- Extract GUID parsing and configuration/parameter validation into separate
  helpers; keep the core operation readable.
- The constructor must not throw for invalid configuration or initialize
  collections, locks, timers, or background workers. Validate on the
  operation path; lazily create expensive state/resources after valid input.
- The approved design prefers `ConcurrentDictionary` and a separate
  background cleaner over a global lock. Compare it against a global-lock
  baseline; this preference is not a measured performance claim. Preserve
  exact expiry/capacity behavior and bounded state, and surface cleaner
  failures instead of silently swallowing them.

### Engineer approvals and TDD workflow

- The design at `pilot/coder/TransactionDeduplicator-Design.txt` is approved.
  Design approval is not a substitute for the approval gate after each TDD
  commit.
- For each criterion: add and run the RED test, commit it, and stop for
  engineer approval; implement the minimum GREEN change, test and commit it,
  and stop for approval; refactor while green, commit, and stop for approval.
  Do not continue to the next stage or criterion without approval. This
  overrides general guidance that a failing RED test need not be committed.
- Keep a timestamped, append-only record of explicit engineer decisions in
  `engineering-decisions.md`. Preserve history. When the original decision
  time is unknown, record entry time with UTC offset and label it
  retrospective.

### Test style and benchmark requirements

- Use a fake monotonic clock for deterministic expiry tests; do not sleep to
  test boundaries. Cover parsing, configuration, first-seen and duplicate
  behavior, refresh, exact-boundary expiry, capacity, invalid-input
  no-mutation, bounded bookkeeping, concurrent calls/cleanup, instance
  isolation, and cleaner disposal/failure visibility.
- Keep tests minimal. Use a private readonly class-level system-under-test
  property named `Sut` where appropriate. For a single act and assertion,
  prefer an expression-bodied test with the method declaration on one line
  and `=> Sut` on the next. Put each operation/assertion call on its own
  following line indented one level further than `=> Sut`. Keep simple
  FluentAssertions chains such as `.Should().BeTrue()` on one line. Do not
  create an intermediate result variable or `Act()`/`Assert()` abstraction.
- Name tests for the behavior they demonstrate, without claims the test does
  not verify (for example, use `New_transaction_can_be_processed` rather
  than implying sequence history or input validation). Use the same short
  behavior name for each TDD commit, followed by `[RED]`, `[GREEN]`, or
  `[REFACTOR]`.
- Benchmark global-lock and ConcurrentDictionary variants with multiple
  threads. Start with small pilot runs and measure memory as well as
  performance, then scale up.
- Verify 100,000 total calls/second with 80% active duplicates and 20% new
  IDs, using the default 10-second window and 1,000,000 capacity. The
  expected steady-state population of new active IDs is about 200,000.
- Produce a detailed report for every run under
  `pilot/coder/benchmarks/reports/`, recording run/commit, variant, machine
  and runtime, workload/configuration, worker count, warm-up and measurement
  duration, offered/completed throughput, latency, failures/backlog, memory,
  allocations, and GC data. Explicitly state whether the target was met; do
  not claim success without measurements.

## Acceptance criteria versus API/design decisions

Acceptance criteria state observable business/system behavior. Do not place
technical or API mechanisms (exception types, method signatures, data
structures, algorithms, lock strategies, or frameworks) inside a behavioral
criterion, even when separately approved by the engineer. Keep approved
technical/API decisions in the decision log and a distinct design-constraint
section; link them without presenting them as business acceptance criteria.

In particular, D-002 remains an approved API/design decision requiring a
dedicated `CapacityExceededException` rather than `OutOfMemoryException`, but
it is not part of capacity acceptance criterion 6. That criterion requires
that a new unseen ID is not accepted and active state does not change after
expired IDs are reclaimed, while active duplicates remain recognized. Verify
the dedicated exception separately as an API contract.

The approved behavior also requires GUID normalization; `ArgumentException`
for null, empty, whitespace-only, or malformed IDs without state changes;
positive configuration with invalid values reported from an operation call
before state changes; duplicate expiration refresh; expiration at the exact
boundary; reclaiming expired IDs before capacity checks; thread safety;
instance/process-local memory; and the stated performance and memory-bound
criteria. Do not claim durability, cross-instance sharing, ACID guarantees,
or performance success without evidence.

## Project and API decisions

- Build a C# console application and solution. Keep source and test projects
  in separate directories under `pilot/coder`.
- The installed .NET SDK is 10.0.400; the engineer-approved target is
  `net10.0`.
- Use XUnit and FluentAssertions if compatible with the selected package
  versions and project setup.
- Keep GUID parsing and configuration/parameter validation in separate
  helpers so the core operation remains readable.
- The constructor must not throw for invalid configuration or initialize
  collections, locks, timers, or background workers. Validate configuration
  on the operation path before state changes; create expensive state and
  background resources lazily after valid input.

## Design approval and current proposal

The design proposal at
`pilot/coder/engineering/TransactionDeduplicator-Design.md` is
approved. Implement only under the engineer's separately approved commit
gates below; design approval does not bypass approval after each TDD commit.
Initialize the coding skill, engineering decision log, and design proposal
together in the pilot setup commit titled `Add engineering metadata`.

The approved design prefers a `ConcurrentDictionary<Guid, Entry>` with
per-entry synchronization and a separate lazy background expiration cleaner.
A narrowly scoped admission/capacity gate may protect insertion, removal,
and exact count updates while leaving normal duplicate lookups off a single
global lock. Compare this design against a global-lock baseline; preference
is not evidence of better performance. Preserve exact expiry and capacity
semantics, keep retained state and expiry bookkeeping bounded by capacity,
and surface cleaner failures instead of swallowing them.

## Required TDD and commit gates

For each acceptance criterion, work in this order:

1. Add a focused test and run it to verify the intended RED result. Commit
   this test and stop for engineer approval.
2. Implement the smallest change that makes the test GREEN. Run the relevant
   tests, commit the implementation, and stop for engineer approval.
3. Refactor while tests remain green. Commit the refactor and stop for
   engineer approval.

Do not proceed to the next stage or criterion until the preceding commit is
approved. Each commit must remain focused and explain its purpose. The RED
test commit and per-commit approval are explicit engineer requirements even
if general repository guidance permits other workflows.

## Benchmark requirements

- Compare the global-lock and ConcurrentDictionary approaches with multiple
  threads and identical workloads. Start with small pilot runs (few threads
  and a small transaction pool), verify memory measurement, then scale up.
- Verify the exact target workload separately: 100,000 total calls/second,
  80% active duplicates and 20% new IDs, using the default 10-second
  expiration and 1,000,000 capacity. Expected new active IDs at steady state
  are about 200,000.
- Measure throughput, latency, memory, allocations, and GC behavior. Record
  environment and run durations.
- Produce a detailed report for every benchmark run under
  `pilot/coder/benchmarks/reports/`. Include configuration, variant, machine
  and runtime, workload, warm-up/measurement duration, offered/completed
  throughput, latency percentiles, errors/backlog, memory, allocations, and
  GC data. Clearly state whether the target was met. Do not infer success
  from a microbenchmark or an unmeasured estimate.

## Quality and handoff

Keep tests deterministic with an injected fake monotonic clock; do not rely on
sleeping for expiration-boundary tests. Cover capacity and concurrency races,
including repeated duplicate refresh and cleanup. Preserve state-count
invariants and avoid unbounded refresh bookkeeping.

For a generic happy-path test that needs any valid GUID, pass
`Guid.NewGuid().ToString()` rather than a fixed GUID literal. Use fixed GUID
values only when a test specifically needs stable identity or formatting
comparisons.

Keep tests minimal and readable as the suite grows. When appropriate, expose
the system under test as a private readonly class-level property named `Sut`.
For a test with one act and one assertion, prefer an expression-bodied test
with the method declaration followed by a line containing `=> Sut`, then
each operation and chained FluentAssertions assertion on its own line,
indented one level further than `=> Sut`. Keep simple assertions such as
`.Should().BeTrue()` on one line. Do not add an intermediate result variable
or extra Act/Assert helper abstraction.

Before handing off, report completed test/build/benchmark commands, link the
per-run reports, identify any unapproved decisions or unresolved risks, and
make clear that the component is process-local and in-memory only.
