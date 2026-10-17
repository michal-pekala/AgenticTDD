# TransactionDeduplicator pilot: solution design

## Status

Engineer-approved design (approved 2026-10-15T17:03:20+02:00), with the
project-type revision in C-025. The class library and test projects are
scaffolded. The new-transaction happy-path test has completed its RED, GREEN,
and REFACTOR commits. The malformed-ID validation has completed RED, GREEN,
and REFACTOR; its parser now returns the normalized GUID for subsequent
identity handling. Active-duplicate detection has completed RED and GREEN:
GUIDs are registered in a concurrent dictionary, so repeats are rejected as
duplicates. Configurable expiration and exact-boundary behavior are
implemented using the injected `IDateTimeProvider`. Duplicate expiration
refresh has completed RED, GREEN, and REFACTOR per C-044; timestamp storage
is shared across the active-duplicate and new/expired-ID paths. Expiry
handling uses direct dictionary lookup; synchronization is explicitly
deferred as requested and remains an unresolved acceptance gap.
No REFACTOR commit was needed for basic duplicate detection; eager dictionary
initialization is retained because its small allocation is acceptable. The
expiry test fake is shared from the test `Fixture` directory, and
FluentAssertions' duration extensions are reused. Capacity and cleanup remain
future tested stages. No benchmark project has been created yet.

## Approved acceptance basis

The approved analyst handoff is the behavioral contract. The decision log
entries D-001 through D-013 were checked against the feature source and
acceptance criteria. No unresolved conflict blocks design:

- The source's strict worst-case O(1) claim is superseded by D-007 and
  acceptance criterion 9: expected O(1) is the target; linear worst-case is
  allowed.
- The source's ACID/double-spend guarantees are narrowed by D-006 and the
  approved process-local scope: no durability, cross-instance sharing, or
  ACID guarantee is claimed.
- D-003 and D-009 are compatible: capacity is configurable, defaults to
  1,000,000, and deployment overrides can be informed by measurement.
- D-002 remains an approved API/design constraint requiring a dedicated
  exception for capacity exhaustion. D-012 explicitly classifies it as
  separate from business acceptance criteria; criterion 6 specifies only
  rejection without active-state change and continued duplicate recognition.
- D-013 requires invalid configuration to be reported on the operation path,
  not by the constructor.

Approved behavior includes the `IsUniqueAndProcess(string transactionId)`
entry point; normalized GUID identity; true for first-seen IDs; false plus
expiration refresh for active duplicates; expiration at the exact
last-attempt-plus-duration boundary; positive configurable expiration and
capacity; invalid inputs throwing `ArgumentException` without state changes;
expired-ID reclamation before capacity enforcement; rejection of a new ID
without active-state change when full; continued recognition of active
duplicates at capacity; thread safety; process-local in-memory state; and
the 100,000 calls/second target with an 80/20 duplicate/new mix. No benchmark
success is claimed before measurements.

API/design constraint, not a business acceptance criterion: D-002 requires a
dedicated `CapacityExceededException` rather than `OutOfMemoryException`.
The design below carries that constraint separately. Its inheritance and
additional properties are coding proposals that still require engineer
approval.

## Project layout and framework proposal

The repository's current `origin/main` has no C# solution or existing
project/test/benchmark conventions. The installed SDK is 10.0.400. The
engineer superseded the initial console-app decision (C-001) and approved a
class library with a separate test project. Layout:

```text
pilot/coder/TransactionDeduplicator.sln
pilot/coder/src/TransactionDeduplicator/
pilot/coder/tests/TransactionDeduplicator.Tests/
pilot/coder/benchmarks/TransactionDeduplicator.Benchmarks/
pilot/coder/benchmarks/reports/
```

Target `net10.0`. The library contains the public deduplicator component;
the separate test project references it. A console host is not required.
Use XUnit and FluentAssertions, consistent with the README preference,
subject to package compatibility. All coding-related artifacts, including
the design and decision log, remain under `pilot/coder`.

## Public API and execution-path validation proposal

- `TransactionDeduplicator.IsUniqueAndProcess(string transactionId)` is the
  core public operation.
- Configuration is supplied through options with
  `UniqnessCheckExpiration` defaulting to 10 seconds and
  `UniqnessBufferCapacity` defaulting to 1,000,000. The instance snapshots
  these scalar values at construction; live reconfiguration is not part of
  the approved scope.
- The constructor only copies supplied values or defaults. It does not
  validate configuration, throw for invalid values, or create locks, timers,
  or a worker. The small initial dictionary allocation is accepted; more
  expensive state and background resources are deferred.
- Separate helpers validate options and parse the input. Positive expiration
  and capacity are checked from the execution path and invalid configuration
  throws a clear `ArgumentOutOfRangeException` there. Parse using
  `Guid.TryParse`, returning a normalized `Guid`. Null, empty,
  whitespace-only, or malformed input throws `ArgumentException`. No state,
  clock, or cleaner is touched for invalid IDs. The malformed-GUID error text
  comes from the shared `ValidationError` template constant, reused in tests
  and formatted with `StringExtensions.Format` so the message is not
  duplicated. `Guid.Empty` is valid.
- D-002 requires the capacity case to use a dedicated
  `CapacityExceededException`, not `OutOfMemoryException`. This is an
  API/design decision, not criterion 6. Confirm the exception's inheritance
  and any extra properties with the engineer before implementing them.

## Concurrency, capacity, and expiration proposal

The preferred candidate uses a `ConcurrentDictionary<Guid, Entry>`. Each
entry holds one last-attempt `DateTimeOffset` and acts as its per-entry
synchronization gate. Duplicate
calls on different IDs do not contend on a single global lock; duplicate
calls on the same ID serialize only while checking or refreshing that entry.
Refreshing mutates the existing entry, so it does not add an expiry record or
allocate a new entry per duplicate.

Use a narrow admission/capacity gate only for unseen-ID insertion, exact
entry-count changes, and capacity-pressure cleanup. The ordinary active
duplicate path does not take this gate. All insertion and removal paths use
the same gate and update the retained count exactly once, keeping the map
count at or below the configured capacity. A valid cache miss at apparent
capacity triggers synchronous expiry reclamation and a repeat lookup before
the dedicated capacity exception. Active duplicates are recognized and
refreshed before capacity handling.

Operation outline:

1. Validate options and parse the GUID through separate helpers. Invalid
   input/configuration fails before lazy state or worker initialization.
2. Use the instance map and lazily start the cleaner after valid input.
3. On a map hit, lock only that entry and recheck that it is still the current
   map value. If `currentTime < lastAttempt + expiration`, update
   last-attempt time and return `false`.
4. If that entry is expired at
   `currentTime >= lastAttempt + expiration`, release its entry
   gate before acquiring the admission gate. Revalidate it, remove it once,
   and retry as a new ID. This keeps lock ordering consistent and avoids a
   per-entry-to-admission lock inversion.
5. On a miss, enter the admission gate and recheck the map. Remove expired
   entries before testing capacity. Add and count the new entry if there is
   room; otherwise throw `CapacityExceededException` without registering it.
6. The `ConcurrentDictionary` insertion plus admission gate ensures racing
   first-seen calls cannot both return `true`; waiters recheck and become
   duplicates.

A separate lazily started background cleaner periodically scans the
dictionary and removes expired entries using the same admission/entry
coordination. Proposed initial cadence is 100 ms (shortened for shorter
expiration settings), subject to benchmark tuning. The cadence affects only
how soon idle expired entries are reclaimed: API calls check the requested
ID's exact expiry, and a full-capacity call performs an on-demand sweep, so
the cleaner cannot defer semantic expiry or capacity correctness.

The design stores no refresh-history queue or stale heap records. Each
retained ID has one map entry; therefore expiry bookkeeping is bounded by the
same configured entry capacity. If the cleaner or map is unavailable, surface
the failure on the execution/disposal path rather than silently ignoring it.
A containing host owns and disposes the cleaner; provide deterministic
disposal so a DI container can stop it. State remains process-local and
instance-local.

Use an internal `IDateTimeProvider` exposing a `DateTimeOffset CurrentTime`
property. The default provider reads `DateTimeOffset.UtcNow`; tests inject a
fake provider whose time can be advanced deterministically. This engineer-
selected wall-clock approach is simpler to inject but may be affected by
system-clock adjustments, unlike the original monotonic-clock proposal.
Sample time consistently with entry synchronization. Exact-boundary tests
must use the fake provider and never sleep. Expiration tests explicitly
configure a randomized duration; separate tests check active behavior before
the boundary, refresh from a duplicate attempt, and acceptance at the
boundary. The test fake supports concise advancement such as
`Now += 10.Seconds()`.

## Benchmark comparator

Keep a global-lock implementation as a benchmark-only comparator. Both
variants must implement the same behavior and pass the same contract tests.
The concurrent-dictionary design is preferred, not presumed faster. Its
admission gate, hot per-entry synchronization, and periodic map scanning may
be bottlenecks; the global-lock version may win for small workloads.

## Testing plan

Use XUnit and FluentAssertions if compatible with the selected .NET 10
package versions. Use an injected fake `IDateTimeProvider` and an internal
manual cleaner trigger for deterministic tests.

- Defaults and positive configuration; invalid configuration throws from the
  operation path, not the constructor.
- First-seen true; active duplicate false; duplicate refresh; just-before and
  exact expiration boundary.
- GUID casing and supported formats normalize; invalid IDs throw without
  mutating state or starting the cleaner.
- At capacity: active duplicate remains false and refreshes; unseen ID throws
  the D-002 API exception and is not registered; expired IDs are reclaimed
  before capacity checks. Keep the behavioral assertions (rejection and
  unchanged active state) distinct from the API-contract assertion (exception
  type).
- Repeated duplicate refresh does not increase entry/expiry metadata count;
  retained state never exceeds capacity.
- Concurrent same-ID first calls yield exactly one true. Concurrent distinct
  IDs never exceed capacity. Refresh/cleaner races cannot remove a refreshed
  active ID or decrement count twice.
- Separate instances do not share state; verify cleaner disposal and
  background-failure visibility.

## TDD and commit approval gates

For each approved criterion, follow the engineer's requested sequence:

1. Add a focused test and verify intended RED; commit and stop for engineer
   approval.
2. Implement the smallest change to reach GREEN; commit and stop for engineer
   approval.
3. Refactor while green; commit and stop for engineer approval.

Do not continue to the next stage or criterion before the preceding commit is
approved. This explicit pilot workflow takes precedence over the README's
optional suggestion that a failing RED test need not be committed.

## Benchmark and per-run report plan

Compare the global-lock and ConcurrentDictionary candidates with multiple
threads and identical workloads. Begin with small pilot runs (for example,
1-2 workers and a small ID pool), verify correctness and memory reporting,
then scale to more threads and working sets including 200,000 active IDs.
Use BenchmarkDotNet for saturated throughput, allocations, and GC diagnostics.
Use a rate-controlled load run for the exact 100,000 calls/second target.

For the target run, use 80% active duplicates and 20% previously unseen IDs,
the default 10-second expiration and 1,000,000 capacity, a warm-up of at
least one full expiry window, and a proposed 60-second steady-state
measurement. Keep a repeatedly refreshed hot duplicate set and issue new
GUIDs at 20,000/second. The expected steady-state population of new active
IDs is approximately 200,000. Validate that the workload contains actual
duplicates and does not build an unbounded producer backlog.

Generate a detailed report for every benchmark run under
`pilot/coder/benchmarks/reports/`. Each report records run ID and commit,
variant, CPU/model/core count, RAM, OS, SDK/runtime, build configuration,
worker count, transaction pool/active-set sizes, duplicate ratio,
expiration/capacity, warm-up and measurement durations, offered and completed
throughput, p50/p95/p99/max latency, errors/backlog, working-set/private
memory deltas, allocations per operation, and GC counts. Include a clear
pass/fail statement for 100,000 completed calls/second. No target-success
claim is made without measured output.

## Open risks and approval points

- The repository had no existing .NET target; `net10.0` was approved based on
  the installed SDK.
- The admission gate can bottleneck new-ID registration. Per-entry gates can
  bottleneck a hot duplicate key. Compare both hot-key and distributed
  duplicate patterns.
- Periodic full-map cleanup is simple and bounded but may consume CPU at high
  capacity. Measure scan cost, cadence, memory, and throughput before tuning.
- Capacity bounds entry count, not exact bytes. Report memory at small and
  production-like sizes; do not claim a byte-level limit.
- Benchmark reports will state environment, duration, achieved throughput,
  memory, and latency. Until runs complete, whether 100K/sec is achievable
  remains unknown.

## Approval boundary

This document is a proposal. Engineer approval of this design is required
before creating the solution, tests, benchmark project, or implementation.
Even after design approval, implementation begins only under the separately
approved per-commit RED/GREEN/refactor workflow.
