# Engineering decisions log

Append-only record of technical and design decisions for the
TransactionDeduplicator pilot 2. Behavior is recorded in
[behaviour-decisions.md](../analyst/behaviour-decisions.md) and is not copied
here; entries below link to its D-NNN entries where relevant.

Source of reused entries: `pilot1 engineering-decisions.md` =
`pilot/TransactionDeduplicator/pilot1/engineering/engineering-decisions.md`.
Only technical and design decisions are reused. Pilot 1's workflow and
test-style entries (commit gates, stage rules, test formatting) are skipped,
because the pilot 2 agents' instructions cover them. Pilot 1's design
document is not reused (C-010).

## Decisions

### C-001 - Class library with a solution and separate source and test folders

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-16T13:07:11+02:00 (C-025); C-001 not captured
- Context/question: Establish the project shape.
- Engineer decision: Build the component as a C# class library with a
  separate test project, both in one solution, with source and tests in
  separate directories. No console host.
- Affected work: Scaffold, solution, project layout.
- Status: Active
- Source: pilot1 engineering-decisions.md C-001 (solution and separate
  directories) and C-025 (class library, superseding C-001's console app).

### C-002 - Separate validation and parsing helpers

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Keep input and configuration checks from obscuring the
  core deduplication path.
- Engineer decision: Extract GUID parsing and parameter/configuration
  validation from the core method into separate helpers.
- Affected work: Public operation implementation.
- Status: Active
- Source: pilot1 engineering-decisions.md C-002.

### C-003 - Prefer a concurrent dictionary with background cleanup

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Reduce the contention risk of a global lock and keep
  collection cleaning off the normal call path where possible.
- Engineer decision: Prefer a `ConcurrentDictionary` approach over a global
  lock and delegate collection cleaning to a separate background component.
  Compare performance before claiming the preferred approach is faster.
- Affected work: Internal design; benchmarks.
- Status: Active preference, subject to measurement (C-004). Made concrete by
  C-014 and C-015.
- Source: pilot1 engineering-decisions.md C-003.

### C-004 - Benchmark against a global-lock baseline and measure memory

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Verify the concurrency preference empirically.
- Engineer decision: Benchmark the global-lock and `ConcurrentDictionary`
  approaches with multiple threads. Start with small workloads, then scale
  up; measure memory as well as performance.
- Affected work: Benchmark project (C-016); AC-10, AC-11 (D-011, D-016).
- Status: Active
- Source: pilot1 engineering-decisions.md C-007.

### C-005 - Report every benchmark run

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: Not captured (retrospective)
- Context/question: Make benchmark results reproducible and avoid unsupported
  performance claims.
- Engineer decision: Generate a detailed report for each benchmark run.
- Affected work: Benchmark reports; AC-11.
- Status: Active
- Source: pilot1 engineering-decisions.md C-008.

### C-006 - Malformed-ID message and shared error templates

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-16T14:41:48+02:00 (C-032);
  2026-10-16T16:08:08+02:00 (C-035)
- Context/question: Give invalid-ID errors a useful, stable message without
  duplicating message text between source and tests.
- Engineer decision: The malformed-ID message is
  `TransactionId value '<invalidId>' is not a valid Guid.` Error-message
  templates live as constants in a `ValidationError` class, are formatted
  through a `StringExtensions.Format` extension using invariant culture, and
  tests reference the source constant instead of copying the text.
- Affected work: Validation helpers, error messages, tests.
- Status: Active
- Source: pilot1 engineering-decisions.md C-032 (message text) and C-035
  (templates and formatting). C-032's test-data guidance is not reused.

### C-007 - Injected wall-clock time provider

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-17T18:44:37+02:00
- Context/question: Expiration refresh and the exact boundary (D-001, D-005)
  need deterministic control of current time in tests.
- Engineer decision: Add an internal `IDateTimeProvider` with a minimal
  `DateTimeOffset CurrentTime` property, injected through an internal
  constructor. The production provider reads `DateTimeOffset.UtcNow`; tests
  use a fake with explicitly advanceable time. A monotonic clock is not used;
  the risk of wall-clock adjustments is accepted.
- Affected work: Expiry logic, test seam.
- Status: Active
- Source: pilot1 engineering-decisions.md C-037.

### C-008 - Options object for configuration

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-17T19:04:27+02:00
- Context/question: Supply configuration (D-009) to the component.
- Engineer decision: Use a `TransactionDeduplicatorOptions` object with
  `UniqnessCheckExpiration` and `UniqnessBufferCapacity`.
- Affected work: Public API.
- Status: Active
- Source: pilot1 engineering-decisions.md C-038 (options part only; its test
  structure guidance is not reused).

### C-009 - Folder layout mirrors pilot 1

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: Not captured (answered between 2026-10-19T22:10 and
  22:27:09+02:00)
- Context/question: Where pilot 2's engineering documents and code go.
- Engineer decision: Engineering documents in
  `pilot/TransactionDeduplicator/pilot2/engineering/`. Code in
  `pilot/TransactionDeduplicator/pilot2/`: `TransactionDeduplicatorPilot2.sln`,
  `src/`, `tests/`.
- Affected work: Scaffold; controller handoff.
- Status: Active

### C-010 - Reuse pilot 1 decisions, not its design document

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: Not captured (answered between 2026-10-19T22:10 and
  22:27:09+02:00)
- Context/question: Whether to reuse pilot 1's technical decisions and its
  design document.
- Engineer decision: Reuse the logged technical decisions only; design pilot
  2 from scratch without pilot 1's design document.
- Affected work: This log; the design document.
- Status: Active

### C-011 - Target framework and test libraries

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-19T22:27:09+02:00
- Context/question: Pilot 1 chose frameworks in its design document, not in
  its log (not reused under C-010).
- Engineer decision: Same as pilot 1: `net10.0`; xUnit 2.9.3,
  xunit.runner.visualstudio 3.1.4, Microsoft.NET.Test.Sdk 17.14.1,
  coverlet.collector 6.0.4, FluentAssertions 8.11.0.
- Affected work: Scaffold.
- Status: Active. FluentAssertions 8 is free only for non-commercial use
  (see design risks).

### C-012 - Invalid configuration throws InvalidOperationException

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-19T22:27:29+02:00
- Context/question: D-013 reports invalid configuration from
  `IsUniqueAndProcess`; AC-12 requires it to be distinguishable from invalid
  input, which throws `ArgumentException` (D-004).
- Engineer decision: Throw `InvalidOperationException`, whose message names
  the invalid setting and its value. Not `ArgumentOutOfRangeException`,
  which derives from `ArgumentException`.
- Affected work: Configuration validation helper; AC-12.
- Status: Active

### C-013 - CapacityExceededException shape

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-19T22:27:46+02:00
- Context/question: D-002 requires a dedicated capacity exception; its base
  type and members were open.
- Engineer decision: `public sealed class CapacityExceededException :
  Exception`. Its message states the capacity; a `Capacity` property holds
  the configured limit.
- Affected work: Public API; AC-6.
- Status: Active

### C-014 - Background cleaner: lazily started timer, IDisposable owner

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-19T22:28:43+02:00
- Context/question: How the background cleaner from C-003 runs and who owns
  it.
- Engineer decision: The deduplicator is `IDisposable` and owns a
  `System.Threading.Timer`, started on the first valid call so the
  constructor stays trivial (D-013). Default cadence 1 second, adjustable
  internally and tuned by benchmark. Tests trigger a sweep through an
  internal method, not the timer. Calls still check expiry exactly, and a
  full buffer is swept inline (D-014).
- Affected work: Internal design; disposal; testability seams.
- Status: Active

### C-015 - Lock-free compare-and-swap synchronization

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-19T22:32:43+02:00
- Context/question: How calls synchronize within the C-003 concurrent
  dictionary. Options were lock-free CAS, pilot 1's per-entry lock plus
  admission gate, or a single global lock.
- Engineer decision: Lock-free CAS. Each entry holds its last-attempt ticks;
  refresh and re-acceptance of an expired ID are `Interlocked`
  compare-and-swaps; a new ID reserves a slot with an `Interlocked` counter
  and then `TryAdd`s; a lock is taken only to sweep. Not presumed faster than
  the global-lock baseline until measured (C-004).
- Affected work: Internal design; AC-7 (D-015).
- Status: Active

### C-016 - Benchmark project added after the behavior tests pass

- Recorded at: 2026-10-19T22:35:00+02:00
- Decision time: 2026-10-19T22:33:43+02:00
- Context/question: Where the AC-11 benchmark and the C-004 global-lock
  baseline live, and when they are created.
- Engineer decision: Scaffold only `src` and `tests` now. Once the AC-1 to
  AC-10 tests pass, add a benchmark project (BenchmarkDotNet plus a
  rate-controlled load runner) holding the global-lock baseline. The
  baseline must pass the same behavior tests, run against both
  implementations.
- Affected work: Scaffold scope; later benchmark work; AC-11.
- Status: Active

## Consistency review

- C-001, C-009 and C-011 together fix the scaffold. No conflict.
- C-012 and C-013 make the four outcomes of AC-4, AC-6 and AC-12
  distinguishable by type: `bool`, `ArgumentException` (D-004, D-008),
  `CapacityExceededException : Exception`, and `InvalidOperationException`.
  No exception type is a base of another.
- C-014 keeps the constructor free of validation and of resource start-up,
  consistent with D-013. Sweeps never decide whether an ID is active; that
  decision is made exactly on each call, so the cadence cannot affect AC-3,
  AC-5 or AC-6.
- C-015 refines C-003. It does not conflict with C-004: the global-lock
  baseline stays as the comparator (C-016).
- C-007 accepts wall-clock adjustment risk; this is noted in the design's
  risks and does not change any criterion.

No unresolved conflict.
