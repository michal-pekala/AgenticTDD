# TransactionDeduplicator pilot 2: design

## Status

Proposed. Awaiting engineer approval.

## Acceptance basis

- Criteria: [TransactionDeduplicator-Acceptance-Criteria.txt](../analyst/TransactionDeduplicator-Acceptance-Criteria.txt)
  (approved 2026-10-19).
- Behavior log: [behaviour-decisions.md](../analyst/behaviour-decisions.md)
  (D-001 to D-016).
- Engineering log: [engineering-decisions.md](engineering-decisions.md)
  (C-001 to C-016).

In scope for the TDD cycle: AC-1 to AC-9 and AC-12, verified by tests.
AC-10 and AC-11 are verified by the benchmark project added after those tests
pass (C-016).

Design constraints from the behavior log: `CapacityExceededException`, never
`OutOfMemoryException` (D-002); `ArgumentException` for invalid IDs (D-004,
D-008); setting names `UniqnessCheckExpiration` and `UniqnessBufferCapacity`
(D-009); the constructor does not throw for invalid configuration (D-013);
no data structure is prescribed by the analyst (D-010).

## Project layout and frameworks

```text
pilot/TransactionDeduplicator/pilot2/
  TransactionDeduplicatorPilot2.sln
  src/TransactionDeduplicatorPilot2.csproj          class library (C-001)
  tests/TransactionDeduplicatorPilot2.Tests.csproj  xUnit test project
  engineering/                                      this document, the log
  benchmarks/                                       later, per C-016
```

- Target `net10.0`, `ImplicitUsings` and `Nullable` enabled (C-011).
- Tests: xUnit 2.9.3, xunit.runner.visualstudio 3.1.4,
  Microsoft.NET.Test.Sdk 17.14.1, coverlet.collector 6.0.4,
  FluentAssertions 8.11.0 (C-011).
- The test project references the source project. The source project grants
  `InternalsVisibleTo("TransactionDeduplicatorPilot2.Tests")` for the seams
  below.
- Namespaces: `TransactionDeduplication` (source) and
  `TransactionDeduplication.Tests` (tests). This avoids a namespace with the
  same name as the `TransactionDeduplicator` class, which would make the
  bare class name ambiguous in tests.

## Public API

```csharp
public sealed class TransactionDeduplicatorOptions                  // C-008, D-009
{
    public TimeSpan UniqnessCheckExpiration { get; init; } = TimeSpan.FromSeconds(10);
    public int UniqnessBufferCapacity { get; init; } = 1_000_000;
}

public sealed class TransactionDeduplicator : IDisposable             // C-014
{
    public TransactionDeduplicator();                                  // defaults
    public TransactionDeduplicator(TransactionDeduplicatorOptions options);
    public bool IsUniqueAndProcess(string transactionId);              // AC-1..AC-7, AC-12
    public void Dispose();
}

public sealed class CapacityExceededException : Exception            // D-002, C-013
{
    public int Capacity { get; }
}
```

| Outcome | How it is reported | Criteria |
|---|---|---|
| New or expired ID accepted | returns `true` | AC-1, AC-3 |
| Active duplicate | returns `false`, expiry refreshed | AC-2, AC-6 |
| Null, empty, whitespace, malformed | `ArgumentException`, no state change; malformed message per C-006 | AC-4 |
| New ID, buffer full of active IDs | `CapacityExceededException`, no state change | AC-6 |
| Non-positive setting | `InvalidOperationException` naming the setting and value, no state change | AC-12 (C-012) |
| Call after `Dispose` | `ObjectDisposedException` | (proposal, see below) |

- The constructor copies the option values and never validates or throws for
  them (D-013). A `null` options argument is a programming error and throws
  `ArgumentNullException`; this is not configuration validation.
  (Proposal.)
- Validation order inside `IsUniqueAndProcess`: disposed check, then
  configuration (C-012), then the ID (D-004, D-008). Configuration comes
  first because a misconfigured instance cannot process any call. All checks
  happen before any state is touched. (Proposal.)
- Parsing uses `Guid.TryParse`, so every format it accepts is a "supported
  GUID text format" (AC-4 assumption): `N`, `D`, `B`, `P`, `X`, any letter
  casing. `Guid.Empty` is a valid ID. Parsing and validation live in
  separate helpers (C-002); error templates live in `ValidationError` and are
  formatted with `StringExtensions.Format` (C-006).
- Options are read once, at construction. Changing the options object later
  has no effect.

## Internal design

### State

```csharp
ConcurrentDictionary<Guid, Entry> _entries;   // C-003
long _retained;                               // reserved or stored entries, maintained with Interlocked
object _sweepLock;                            // only one sweep at a time
Timer? _cleaner;                              // started lazily, C-014

sealed class Entry { long LastAttemptTicks; }  // UTC ticks; Removed = long.MinValue
```

One entry per retained ID; no queue, heap or history per attempt, so expiry
bookkeeping is bounded by the number of entries (AC-5, AC-9).
`_entries.Count` is not used on the hot path because it locks every bucket.

### Operation (C-015)

`now` is sampled once per call from `IDateTimeProvider` (C-007) as UTC ticks.
An entry is active when `now < LastAttemptTicks + expiration` (D-005).

1. Validate (above), then start the cleaner timer if not yet started
   (`Interlocked.CompareExchange` on `_cleaner`).
2. Loop:
   - **Hit** (`TryGetValue` finds `e`): read `old = e.LastAttemptTicks`.
     - `old == Removed`: a sweeper is removing it. Help:
       `_entries.TryRemove(KeyValuePair(id, e))`; on success decrement
       `_retained`. Retry the loop.
     - Expired: CAS `old -> now`. Success: return `true`; the entry is reused,
       so `_retained` is unchanged and nothing is allocated. Failure: retry.
     - Active: if `now <= old`, return `false` (another call already refreshed
       to a later time). Otherwise CAS `old -> now`; success returns `false`,
       failure retries.
   - **Miss**: reserve a slot: `Interlocked.Increment(ref _retained)`. If the
     result exceeds capacity, undo the increment, run a pressure sweep
     (below) and try to reserve once more; if it is still over, undo and
     throw `CapacityExceededException`. With a slot reserved,
     `TryAdd(id, new Entry(now))`: success returns `true`; failure (another
     call added the same ID) undoes the reservation and retries the loop,
     where it is then a hit.
3. Every retry is caused by another call's successful CAS, insert or
   removal, so the loop is lock-free: some call always makes progress.

Why the criteria hold:

- **AC-7, exactly one `true`**: for an unseen ID, only one `TryAdd` succeeds;
  for an expired ID, only one CAS from the expired value succeeds. Every
  other racer then sees an active entry and returns `false`.
- **AC-6, bound**: `_retained` counts reserved plus stored entries and is
  decremented only by the call whose `TryRemove` succeeded, so the number of
  stored entries never exceeds `_retained`, which never stays above
  capacity. A transient increment over capacity is undone before the call
  continues and never results in a stored entry.
- **AC-6, duplicates at capacity**: the hit path never touches `_retained`,
  so duplicates are recognized and refreshed when the buffer is full.
- **AC-4, AC-6, AC-12, no state change on rejection**: validation precedes
  any state access; a capacity rejection undoes its reservation and stores
  nothing. A pressure sweep may run first, but it removes only expired
  entries, which AC-5 treats as not retained.

### Sweeping (C-014, D-014)

`SweepExpired(now)` runs under `_sweepLock` and enumerates `_entries`
(lock-free enumeration). For each entry that is expired and not `Removed`,
it CASes the ticks to `Removed`; on success it calls
`TryRemove(KeyValuePair(id, e))` and, if that succeeds, decrements
`_retained`. A call that refreshes the same entry concurrently either wins
its CAS first (the entry stays) or sees `Removed` and helps remove it before
re-adding the ID as new.

- **Timer sweep**: every 1 second by default (C-014), using the provider's
  current time. Keeps idle expired entries from lingering.
- **Pressure sweep**: run inline by a new-ID call that found the buffer full,
  before the capacity decision (D-014). If another sweep is already running,
  the call waits for it on `_sweepLock` and then rechecks.
- Neither sweep decides whether an ID is active; each call does, from its
  own `now`. Sweep timing therefore cannot change any call's result.

### Resource lifetime and failure handling

- The timer is created on the first valid call, never in the constructor.
- `Dispose` is idempotent: it stops and disposes the timer and marks the
  instance disposed. Later calls throw `ObjectDisposedException`. A call
  already in progress during `Dispose` may complete. (Proposal.)
- The timer callback must not let an exception escape, because that would
  crash the process. It catches any exception, stores the first one, and the
  next `IsUniqueAndProcess` call rethrows it (preserving its stack via
  `ExceptionDispatchInfo`). No expected failure path exists in a sweep; this
  only makes a defect visible. (Proposal.)

### Allocation

The duplicate path is designed to allocate nothing: `Guid.TryParse` on a
`string` and `TryGetValue` do not allocate, and refresh is a CAS on an
existing object. The new-ID path allocates one `Entry` plus the
dictionary's own node. Re-accepting an expired ID reuses its entry. These
are expectations for the benchmark to confirm (D-016), not claims.

## Testability seams

- `IDateTimeProvider` (internal, C-007), injected through an internal
  constructor `TransactionDeduplicator(TransactionDeduplicatorOptions,
  IDateTimeProvider, TimeSpan cleanerInterval)`. Tests use a fake clock and
  pass `Timeout.InfiniteTimeSpan` so the timer never fires; expiry tests
  never sleep.
- `internal void SweepExpired()`: runs one sweep at the provider's current
  time, so cleanup is triggered deterministically (C-014).
- `internal int RetainedCount`: the number of stored entries, so tests can
  show that duplicates, expiry and capacity keep bookkeeping bounded (AC-5,
  AC-6, AC-9).
- Concurrency tests (AC-7) start many tasks on the same ID or on distinct IDs
  near capacity and assert the outcome counts; they need no extra seam.

## Benchmark (later, C-016)

Added after the AC-1 to AC-9 and AC-12 tests pass:
`benchmarks/TransactionDeduplicatorPilot2.Benchmarks` with BenchmarkDotNet
(allocations, GC counts) and a rate-controlled runner for the 100,000
calls/second, 80/20 workload (AC-10, AC-11). It holds the global-lock
baseline (one `lock` around a `Dictionary<Guid, long>` and the same sweep
rules), which must pass the same behavior tests (C-004, C-016). Every run
produces a report (C-005). Design details for it are settled when it starts.

## Open risks and assumptions

- **Not presumed faster.** The lock-free design is preferred, not measured.
  The global-lock baseline may win, especially with few threads.
- **Sweep cost.** A sweep is a full enumeration, O(retained). At 200,000
  entries every second this costs CPU; at capacity with no expired entries,
  every rejected new ID pays a full sweep under `_sweepLock`. Rejection under
  sustained overload could therefore be slow. Measure before tuning.
- **Hot-key contention.** Many concurrent duplicates of one ID contend on one
  CAS; retries are bounded by contention, not by a constant.
- **Wall clock.** C-007 accepts that clock adjustments can shorten or extend
  a window. A backwards jump can make `now <= old`; refresh then keeps the
  later time.
- **Capacity bounds entries, not bytes.** Memory per entry is reported by the
  benchmark, not promised.
- **FluentAssertions 8** is free for non-commercial use only; a commercial
  use would need a licence or a move to 7.x.
- **Proposals in this document** (marked "Proposal") are part of the design
  and are decided by its approval: `ArgumentNullException` for null options,
  the validation order, `ObjectDisposedException` after `Dispose`, and
  rethrowing a cleaner failure on the next call.
