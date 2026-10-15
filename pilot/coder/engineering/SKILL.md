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

Before handing off, report completed test/build/benchmark commands, link the
per-run reports, identify any unapproved decisions or unresolved risks, and
make clear that the component is process-local and in-memory only.
