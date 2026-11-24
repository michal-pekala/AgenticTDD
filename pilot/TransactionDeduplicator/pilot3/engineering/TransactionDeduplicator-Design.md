# TransactionDeduplicator pilot 3 (Java): design

## Status

Approved by the engineer at 2026-11-24T12:49:53+01:00 (C-028), including
the items marked "Proposal" and the "Java mapping (proposal)" parts of the
engineering log.

## Acceptance basis

- Criteria: [TransactionDeduplicator-Acceptance-Criteria.md](../analyst/TransactionDeduplicator-Acceptance-Criteria.md)
  (approved 2026-11-24, D-020).
- Behavior log: [behaviour-decisions.md](../analyst/behaviour-decisions.md)
  (D-001 to D-021).
- Engineering log: [engineering-decisions.md](engineering-decisions.md)
  (C-001 to C-028).

In scope for the TDD cycle: AC-1 to AC-9 and AC-12, verified by tests.
AC-10 and AC-11 are verified by the benchmark module added after those tests
pass (C-007).

Design constraints from the behavior log, mapped to Java (D-019):

| Constraint | Java | Log |
|---|---|---|
| Dedicated capacity exception, never out-of-memory (D-002) | `CapacityExceededException extends RuntimeException`; never `OutOfMemoryError` | C-016 |
| Argument exception for invalid IDs (D-004, D-008) | `IllegalArgumentException` | C-021 |
| Setting names (D-009) | `uniqnessCheckExpiration`, `uniqnessBufferCapacity` (getters/setters), "Uniqness" kept | C-014 |
| Constructor does not throw for invalid configuration (D-013) | Validation on the call, `IllegalStateException` | C-015 |
| Explicit GUID formats (D-017) | Own parser; `UUID.fromString` is not used | C-022, below |
| Entry point (D-019) | `boolean isUniqueAndProcess(String transactionId)` | — |

## Project layout and frameworks

```text
pilot/TransactionDeduplicator/pilot3/
  pom.xml                                       one module, jar (C-008)
  .gitignore                                    target/
  src/main/java/transactiondeduplication/       production code
  src/test/java/transactiondeduplication/       tests, same package (C-024, C-026)
  analyst/                                      criteria, behavior log
  engineering/                                  this document, the log,
                                                the guidance proposal
  benchmarks/                                   later, JMH module (C-007)
```

Stack (engineer decisions C-001 to C-006, versions C-027):

- Java 17, compiler `release` 17, on Eclipse Temurin 17.0.21 (HotSpot)
  (C-001).
- Maven 3.9.16, standard layout. Run all tests with `mvn -q test`; a single
  test with `mvn -q -Dtest=ClassName#method test` (C-002).
- Code in `pilot/TransactionDeduplicator/pilot3/`, documents in
  `pilot3/engineering/` (C-003).
- Tests: JUnit 5 (Jupiter) through `org.junit:junit-bom` 5.14.4
  (`junit-jupiter`, which includes parameterized tests), AssertJ
  (`assertj-core` 3.27.7). Mockito only if a test needs it; it is not added
  now (C-004).
- Plugins: `maven-compiler-plugin` 3.16.0, `maven-surefire-plugin` 3.6.1
  (C-027).
- The engineer uses IntelliJ IDEA 2025.3 or VS Code; agents use Maven from
  the terminal only, and no IDE files are committed (C-005).
- Exact versions are pinned in `pom.xml` (C-006).
- Source encoding UTF-8 (C-025, Proposal).
- Benchmarks later, with JMH in a separate module (C-007).
- Package `transactiondeduplication` for source and tests (C-024). No
  `module-info.java` (C-026, Proposal).

## Public API

```java
package transactiondeduplication;

public final class TransactionDeduplicatorOptions {                 // C-014, D-009
    public Duration getUniqnessCheckExpiration();                    // default Duration.ofSeconds(10)
    public void setUniqnessCheckExpiration(Duration value);
    public int getUniqnessBufferCapacity();                          // default 1_000_000
    public void setUniqnessBufferCapacity(int value);
}

public final class TransactionDeduplicator implements AutoCloseable { // C-018
    public TransactionDeduplicator();                                 // defaults
    public TransactionDeduplicator(TransactionDeduplicatorOptions options);
    public boolean isUniqueAndProcess(String transactionId);          // AC-1..AC-7, AC-12
    @Override public void close();                                    // idempotent, no checked exception
}

public final class CapacityExceededException extends RuntimeException { // D-002, C-016
    public int getCapacity();
}
```

| Outcome | How it is reported | Criteria |
|---|---|---|
| New or expired ID accepted | returns `true` | AC-1, AC-3 |
| Active duplicate | returns `false`, expiry refreshed | AC-2, AC-6 |
| Null ID | `IllegalArgumentException`, message `NULL_TRANSACTION_ID`, no state change (C-021, C-023) | AC-4 |
| Empty, whitespace-only, malformed ID | `IllegalArgumentException`, message `INVALID_TRANSACTION_ID` formatted with the original input, no state change (C-021, C-023) | AC-4 |
| New ID, buffer full of active IDs | `CapacityExceededException`, message `CAPACITY_EXCEEDED` formatted with the capacity, no state change (C-016) | AC-6 |
| Non-positive (or null) setting | `IllegalStateException` naming the setting and its value, no state change (C-015) | AC-12 |
| Call after `close()` | `IllegalStateException`, message `CLOSED` (Proposal) | — |
| Earlier background sweep failed | `IllegalStateException`, message `CLEANUP_FAILED`, the failure as its cause (Proposal) | — |

The four AC outcomes are distinguishable by type: no exception type above
is a supertype of another, and none is used for two AC outcomes.

### Error templates (C-013, C-023)

`final class ValidationError` (package-private) holds the templates as
`static final String` constants in `String.format` syntax and one
`static String format(String template, Object... args)` using
`Locale.ROOT`. Tests use static imports of both (C-024).

```java
INVALID_TRANSACTION_ID = "TransactionId value '%s' is not a valid Guid.";  // C-023
NULL_TRANSACTION_ID    = "TransactionId must not be null.";                 // C-023
CAPACITY_EXCEEDED      = "Uniqueness buffer capacity of %d active transaction IDs is exceeded.";
```

`CAPACITY_EXCEEDED` is pilot 2's text (Proposal). The configuration, closed
and cleanup-failure texts are written by the test-writer as stubs when their
first test needs them; the configuration message names the setting and its
value (C-015).

### Construction and validation order

- The constructor copies the option values and never validates them (D-013,
  C-014). It starts no thread (C-018). A `null` options argument is a
  programming error and throws `NullPointerException` from
  `Objects.requireNonNull`; this is not configuration validation, so D-013
  is unaffected (Proposal).
- A `null` expiration in the options counts as invalid configuration and is
  reported on the call, like a non-positive value (Proposal). The
  constructor does no arithmetic on the expiration, so a huge `Duration`
  cannot make it throw; the call path saturates instead of overflowing
  (see "Operation").
- `isUniqueAndProcess` checks, in order: closed, a stored cleaner failure,
  configuration (C-015), then the ID (C-021). All checks happen before any
  state is touched (AC-4, AC-12). Configuration comes before the ID because
  a misconfigured instance cannot process any call (Proposal, as pilot 2).

### GUID parsing (D-017, C-009, C-022)

A package-private `final class TransactionIdParser` turns the input into a
`java.util.UUID`, the map key. Equal GUIDs in any format give equal keys
(D-008). Steps:

1. `null` → null-ID error.
2. `s = input.strip()` (C-022). Empty → invalid-ID error (this covers empty
   and whitespace-only input).
3. Exactly one of these shapes, by length and delimiters; anything else is
   malformed:

   | Format | Length | Shape |
   |---|---|---|
   | N | 32 | 32 hex digits |
   | D | 36 | 8-4-4-4-12 hex digits, hyphens at 8, 13, 18, 23 |
   | B | 38 | `{` D `}` |
   | P | 38 | `(` D `)` |
   | X | 68 | `{0xHHHHHHHH,0xHHHH,0xHHHH,{0xHH,0xHH,0xHH,0xHH,0xHH,0xHH,0xHH,0xHH}}` |

   Hex digits are `0-9a-fA-F`; the `0x` prefix accepts `0x` or `0X`. Group
   widths are exact (so `1-1-1-1-1` is malformed), there is no whitespace
   inside the GUID, and brackets must match.
4. The 128 bits are assembled into two `long`s, `new UUID(msb, lsb)`. The
   all-zero GUID is valid (D-020).

`UUID.fromString` is not used: it accepts only the D shape and also accepts
short groups (D-017).

### Configuration validation (C-009, C-015)

A package-private helper checks the copied values: expiration not `null`
and greater than `Duration.ZERO`; capacity greater than zero. The first
invalid setting, in the order expiration then capacity, is reported
(Proposal).

## Internal design

### State

```java
ConcurrentHashMap<UUID, Entry> entries;        // C-010
ConcurrentHashMap<UUID, Boolean> claims;       // C-020; at most one key per in-flight new-ID call
AtomicLong retained;                           // reserved or stored entries (C-019)
ReentrantLock sweepLock;                       // only one sweep at a time
AtomicReference<ScheduledExecutorService> cleaner;  // created lazily (C-018)
AtomicReference<Throwable> cleanerFailure;
volatile boolean closed;

static final class Entry { final AtomicLong lastAttemptNanos; }  // REMOVED = Long.MIN_VALUE
```

Time is a `long` of nanoseconds since the epoch, taken once per call from
`InstantSource.instant()` (C-017). The expiration is held as nanoseconds,
saturated at `Long.MAX_VALUE`; `lastAttempt + expiration` uses saturating
addition, so a very long window means "never expires" instead of
overflowing. An entry is active when `now < lastAttempt + expiration`
(D-005).

One entry per retained ID; no per-attempt history, so expiry bookkeeping is
bounded by the number of entries (AC-5, AC-9). `entries.size()` is not used
for capacity decisions.

### Operation (C-019, C-020)

1. Validate (above), then start the cleaner if it is not yet started
   (`compareAndSet` on `cleaner`; the loser shuts down its own executor).
2. Loop:
   - **Hit** (`entries.get(id)` returns `e`): read `old`.
     - `old == REMOVED`: a sweep is removing it. Help:
       `entries.remove(id, e)`; on success decrement `retained`. Retry.
     - Expired: `compareAndSet(old, now)`. Success returns `true` (the entry
       is reused; `retained` is unchanged; nothing is allocated). Failure
       retries.
     - Active: if `now <= old`, return `false` (another call already
       refreshed to a later time). Otherwise `compareAndSet(old, now)`;
       success returns `false`, failure retries.
   - **Miss**: claim the ID with `claims.putIfAbsent(id, TRUE)` (C-020). A
     call that loses the claim calls `Thread.yield()` and retries. The claim
     holder re-checks `entries.containsKey(id)` (present: retry as a hit).
     Otherwise it reserves a slot: `retained.incrementAndGet()`. If that
     exceeds capacity it undoes the increment, runs a pressure sweep and
     tries once more; if still over, it undoes and throws
     `CapacityExceededException`. With a slot reserved it puts
     `new Entry(now)`, returns `true`, and removes its claim in `finally`.
3. Retries on a hit are caused by another call's successful CAS, insert or
   removal, so the hit path is lock-free. The miss path for one ID is not:
   same-ID racers wait for the claim holder (C-020).

Why the criteria hold:

- **AC-7, exactly one `true`**: for an unseen ID only the claim holder
  inserts it; for an expired ID only one CAS from the expired value
  succeeds. Every other racer then sees an active entry and returns
  `false`. A racer is rejected for capacity only while holding the claim
  with the ID absent, so it cannot lose to another racer holding the last
  slot.
- **AC-6, bound**: `retained` counts reserved plus stored entries and is
  decremented only by the call whose `remove(id, e)` succeeded, so stored
  entries never exceed `retained`, which never stays above capacity.
- **AC-6, duplicates at capacity**: the hit path never touches `retained`.
- **AC-4, AC-6, AC-12, no state change on rejection**: validation precedes
  any state access; a capacity rejection undoes its reservation and stores
  nothing. A pressure sweep may run first, but it removes only expired
  entries, which AC-5 treats as not retained.

### Sweeping (C-018, D-014)

`sweepExpired(now)` runs under `sweepLock` and iterates `entries` (weakly
consistent iteration, no lock on the map). For each expired entry not
`REMOVED`, it CASes the time to `REMOVED`; on success it calls
`entries.remove(id, e)` and, if that succeeds, decrements `retained`. A
concurrent refresh either wins its CAS first (the entry stays) or sees
`REMOVED` and helps remove it before re-adding the ID as new.

- **Timer sweep**: every 1 second by default, at the time source's current
  time, on the cleaner thread.
- **Pressure sweep**: inline, by a new-ID call that found the buffer full,
  before the capacity decision (D-014). If a sweep is already running, the
  call waits on `sweepLock`, then rechecks.
- No sweep decides whether an ID is active; each call does, from its own
  `now`. Sweep timing cannot change any call's result.

### Resource lifetime and failure handling (C-018)

- The executor is `Executors.newSingleThreadScheduledExecutor` with a thread
  factory that makes one daemon thread named `transaction-deduplicator-cleaner`.
  It is created on the first valid call, never in the constructor, and
  schedules the sweep with `scheduleWithFixedDelay`.
- `close()` is idempotent: it sets `closed` and calls `shutdownNow()` on the
  executor if one exists. It does not wait for a running sweep to finish.
  Later calls throw `IllegalStateException` (`CLOSED`). A call already in
  progress may complete (Proposal).
- A `ScheduledExecutorService` silently stops a periodic task whose run
  throws. The sweep task therefore catches every `Throwable`, stores the
  first in `cleanerFailure`, and the next `isUniqueAndProcess` call throws
  `IllegalStateException` (`CLEANUP_FAILED`) with it as the cause. No
  expected failure path exists in a sweep; this only makes a defect visible
  (Proposal).

### Allocation

Expectations for the benchmark to confirm (D-016, D-018), not claims:

- Duplicate path: one `UUID` from parsing; `strip()` returns the same string
  when there is no surrounding whitespace; `InstantSource.instant()` returns
  an `Instant` (the JIT may remove it by escape analysis). The refresh
  itself is a CAS on an existing object.
- New-ID path: additionally one `Entry`, its `AtomicLong`, the map node,
  and a claim node that is removed again.
- If measurements show the per-call `UUID` or `Instant` matters, a later,
  measured change can key the map differently or read `millis()`; not now.

## Testability seams (C-017, C-026)

- Package-private constructor `TransactionDeduplicator(
  TransactionDeduplicatorOptions options, InstantSource time, Duration
  cleanerInterval)`. The public constructors pass `Clock.systemUTC()` and
  1 second. Tests pass a fake `InstantSource` and `null` for the interval,
  which means no timer is ever scheduled; expiry tests never sleep.
- Package-private `void sweepExpired()`: one sweep at the time source's
  current time, so cleanup is triggered deterministically.
- Package-private `int retainedCount()`: the number of stored entries, so
  tests can show that duplicates, expiry and capacity keep bookkeeping
  bounded (AC-5, AC-6, AC-9).
- Concurrency tests (AC-7) use `runConcurrent` (C-024) on the same ID or on
  distinct IDs near capacity and assert the outcome counts; no extra seam.

## Benchmark (later, C-007)

Added after the AC-1 to AC-9 and AC-12 tests pass: a separate Maven module
under `pilot3/benchmarks/` with JMH (throughput, latency, allocation via the
GC profiler) and a rate-controlled runner for the 100,000 calls/second,
80/20 workload (AC-10, AC-11). It records young and old/full GC counts,
total GC pause time, the collector and heap settings (D-018). It holds the
global-lock baseline (one lock around a `HashMap<UUID, Long>` with the same
sweep rules), which must pass the same behavior tests (C-011). Every run
produces a report (C-012). Its design is settled when it starts.

## Open risks and assumptions

- **Not presumed faster.** The CAS design is preferred, not measured. The
  global-lock baseline may win, especially with few threads.
- **Sweep cost.** A sweep is a full iteration, O(retained). At capacity with
  no expired entries, every rejected new ID pays a full sweep under
  `sweepLock`, so rejection under sustained overload could be slow. Measure
  before tuning.
- **Hot-key contention.** Many concurrent duplicates of one ID contend on
  one CAS.
- **Wall clock.** C-017 accepts that clock adjustments can shorten or
  extend a window. A backwards jump can make `now <= old`; refresh then
  keeps the later time.
- **JIT warm-up and GC.** Throughput depends on warm-up and the collector;
  the benchmark records both (AC-11).
- **Allocation per call.** Unlike .NET's `Guid` struct, `UUID` is an object,
  so the duplicate path allocates (see "Allocation").
- **X format strictness.** The parser requires the exact widths shown above
  and no whitespace inside the GUID. .NET's parser is more lenient in X
  format (shorter hex groups, inner whitespace); D-017 names the shape with
  fixed widths, so pilot 3 follows that. Raise it with the analyst if
  .NET's leniency is wanted.
- **Whitespace set differs from pilot 2** (C-022): U+00A0, U+2007, U+202F,
  U+0085 rejected; U+001C to U+001F accepted.
- **JUnit 6** exists (Java 17 baseline); the engineer chose JUnit 5 (C-004).
- **Capacity bounds entries, not bytes.** Memory per entry is reported by
  the benchmark, not promised.
- **Proposals in this document** (marked "Proposal") are decided by its
  approval: `NullPointerException` for null options, `null` expiration as
  invalid configuration, the validation order, the order in which invalid
  settings are reported, `IllegalStateException` after `close()` and after
  a cleaner failure, pilot 2's capacity message, UTF-8 encoding (C-025),
  package-private seams without `module-info.java` (C-026), and the pinned
  versions (C-027).
