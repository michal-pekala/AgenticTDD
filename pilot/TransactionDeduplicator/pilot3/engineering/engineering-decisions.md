# Engineering decisions log

Append-only record of technical and design decisions for the
TransactionDeduplicator pilot 3 (Java 17). Behavior is recorded in
[behaviour-decisions.md](../analyst/behaviour-decisions.md) and is not copied
here; entries below link to its D-NNN entries where relevant.

Sources of reused entries:

- `pilot2 engineering-decisions.md` =
  `pilot/TransactionDeduplicator/pilot2/engineering/engineering-decisions.md`
- `pilot1 engineering-decisions.md` =
  `pilot/TransactionDeduplicator/pilot1/engineering/engineering-decisions.md`

Pilot 2's log already reused pilot 1's technical decisions. Entries below
name both sources where pilot 2 itself reused pilot 1. Only technical and
design decisions are reused. Workflow decisions (commit gates, already-satisfied
criteria, per-role models: pilot 2 C-018, C-019, C-021) are skipped, because
the agents' and controller's instructions cover them. Pilot 2's C#
test-style rules live in the agent guidance; the Java test style is a new
decision here (C-024), because that guidance is written for C#.

Stack entries C-001 to C-007 are engineer decisions given to the architect
through the upper session (2026-11-24) before this session started.

D-019 hands the Java mapping of .NET-named constraints to the architect.
Where an entry maps a reused decision to Java, the mapping is marked
"Java mapping (proposal)" and is decided by the design approval, not by
this entry.

## Models

- Architect session (2026-11-24): Opus 5.5 (`claude-opus-5-5`). Confirmed by
  the engineer at the start of the design.

## Decisions

### C-001 - Java 17 on Eclipse Temurin

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (given before this session, 2026-11-24)
- Context/question: Platform for pilot 3.
- Engineer decision: Java 17, compiled with `release` 17. JDK: Eclipse
  Temurin 17.0.21 (HotSpot), installed at
  `C:\Program Files\Eclipse Adoptium\jdk-17.0.21.10002-hotspot`.
- Affected work: Scaffold (`pom.xml`); benchmark environment (AC-11).
- Status: Active

### C-002 - Maven 3.9 with the standard layout

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (given before this session, 2026-11-24)
- Context/question: Build tool, layout and commands.
- Engineer decision: Maven 3.9.16 (on the user PATH). Standard layout:
  `pom.xml`, `src/main/java`, `src/test/java`. Commands: `mvn -q test`;
  a single test: `mvn -q -Dtest=ClassName#method test`.
- Affected work: Scaffold; every RED, GREEN and REFACTOR stage.
- Status: Active

### C-003 - Code and document folders

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (given before this session, 2026-11-24)
- Context/question: Where pilot 3's code and engineering documents go.
- Engineer decision: Code in `pilot/TransactionDeduplicator/pilot3/`
  (`pom.xml`, `src/`); engineering documents in
  `pilot/TransactionDeduplicator/pilot3/engineering/`.
- Affected work: Scaffold; controller handoff.
- Status: Active

### C-004 - JUnit 5 and AssertJ

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (given before this session, 2026-11-24)
- Context/question: Test framework and assertion library.
- Engineer decision: JUnit 5 (Jupiter) run by maven-surefire-plugin 3.x.
  AssertJ for assertions, in place of FluentAssertions. Mockito only if a
  test needs it; it is not added at scaffold.
- Affected work: Scaffold; tests.
- Status: Active

### C-005 - Agents build with Maven from the terminal

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (given before this session, 2026-11-24)
- Context/question: Tooling used by the engineer and by the agents.
- Engineer decision: The engineer uses IntelliJ IDEA 2025.3 or VS Code with
  the Java extension pack. Agents use Maven from the terminal only; no IDE
  project files are generated or committed.
- Affected work: Every coding stage.
- Status: Active

### C-006 - Pin exact dependency versions

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (given before this session, 2026-11-24)
- Context/question: Reproducible builds.
- Engineer decision: Pin exact dependency and plugin versions in `pom.xml`
  at scaffold and record them in this log.
- Affected work: Scaffold. The versions are recorded in C-027.
- Status: Active

### C-007 - Benchmark later, with JMH in a separate module

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (given before this session, 2026-11-24)
- Context/question: Where the AC-11 benchmark and the global-lock baseline
  (C-011) live, and when they are created.
- Engineer decision: As pilot 2 C-016: scaffold only the library and its
  tests now. Once the AC-1 to AC-9 and AC-12 tests pass, add a separate
  benchmark module using JMH, plus a rate-controlled runner for the AC-10
  workload, holding the global-lock baseline. The baseline must pass the
  same behavior tests.
- Affected work: Scaffold scope; later benchmark work; AC-10, AC-11.
- Status: Active
- Source: pilot2 engineering-decisions.md C-016 (JMH replaces
  BenchmarkDotNet, by engineer input).

### C-008 - Library with separate source and test folders

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (retrospective)
- Context/question: Project shape.
- Engineer decision: Build the component as a class library with separate
  source and test code. No console host.
- Java mapping (proposal): one Maven module with `jar` packaging;
  `src/main/java` and `src/test/java` (C-002).
- Affected work: Scaffold.
- Status: Active
- Source: pilot2 engineering-decisions.md C-001 (from pilot1
  engineering-decisions.md C-001 and C-025).

### C-009 - Separate validation and parsing helpers

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (retrospective)
- Context/question: Keep input and configuration checks from obscuring the
  core deduplication path.
- Engineer decision: Extract GUID parsing and parameter/configuration
  validation from the core method into separate helpers.
- Affected work: Public operation implementation.
- Status: Active
- Source: pilot2 engineering-decisions.md C-002 (from pilot1
  engineering-decisions.md C-002).

### C-010 - Prefer a concurrent map with background cleanup

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (retrospective)
- Context/question: Reduce the contention risk of a global lock and keep
  collection cleaning off the normal call path where possible.
- Engineer decision: Prefer a concurrent dictionary over a global lock and
  delegate collection cleaning to a separate background component. Compare
  performance before claiming the preferred approach is faster.
- Java mapping (proposal): `ConcurrentHashMap`.
- Affected work: Internal design; benchmarks.
- Status: Active preference, subject to measurement (C-011). Made concrete
  by C-019 and C-020.
- Source: pilot2 engineering-decisions.md C-003 (from pilot1
  engineering-decisions.md C-003).

### C-011 - Benchmark against a global-lock baseline and measure memory

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (retrospective)
- Context/question: Verify the concurrency preference empirically.
- Engineer decision: Benchmark the global-lock and concurrent-map
  approaches with multiple threads. Start with small workloads, then scale
  up; measure memory as well as performance.
- Affected work: Benchmark module (C-007); AC-10, AC-11 (D-011, D-016,
  D-018).
- Status: Active
- Source: pilot2 engineering-decisions.md C-004 (from pilot1
  engineering-decisions.md C-007).

### C-012 - Report every benchmark run

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (retrospective)
- Context/question: Make benchmark results reproducible and avoid
  unsupported performance claims.
- Engineer decision: Generate a detailed report for each benchmark run.
- Affected work: Benchmark reports; AC-11.
- Status: Active
- Source: pilot2 engineering-decisions.md C-005 (from pilot1
  engineering-decisions.md C-008).

### C-013 - Shared error templates, reused by tests

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: 2026-10-16T14:41:48+02:00 and 2026-10-16T16:08:08+02:00
  (pilot 1)
- Context/question: Give errors a useful, stable message without
  duplicating message text between source and tests.
- Engineer decision: Error-message templates live as constants in a
  `ValidationError` class and are formatted with an invariant culture;
  tests reference the source constant instead of copying the text. The
  malformed-ID message names the offending value.
- Java mapping (proposal): `final class ValidationError` with
  `static final String` templates in `String.format` syntax and a
  `static String format(String template, Object... args)` that uses
  `Locale.ROOT` (Java has no extension methods; this replaces
  `StringExtensions.Format`). Tests use static imports in place of the
  `Error` alias. The exact texts are in C-023.
- Affected work: Validation helpers, error messages, tests.
- Status: Active
- Source: pilot2 engineering-decisions.md C-006 (from pilot1
  engineering-decisions.md C-032 and C-035).

### C-014 - Options object, settable, read once at construction

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: 2026-10-17T19:04:27+02:00 (pilot 1 C-038);
  2026-11-22T22:19:17+01:00 (pilot 2 C-020);
  2026-11-23T01:05:16+01:00 (pilot 2 C-023)
- Context/question: Supply configuration (D-009) to the component.
- Engineer decision: An options object holds `UniqnessCheckExpiration` and
  `UniqnessBufferCapacity`. Its values are settable, so tests can set them
  as their first statements. The deduplicator copies the values once, at
  construction; later changes to the options object have no effect.
- Java mapping (proposal): `public final class
  TransactionDeduplicatorOptions` with getters and setters
  `getUniqnessCheckExpiration()` / `setUniqnessCheckExpiration(Duration)`
  (default `Duration.ofSeconds(10)`) and `getUniqnessBufferCapacity()` /
  `setUniqnessBufferCapacity(int)` (default `1_000_000`). The "Uniqness"
  spelling is kept (D-009, D-019).
- Affected work: Public API; tests.
- Status: Active
- Source: pilot2 engineering-decisions.md C-008 (from pilot1
  engineering-decisions.md C-038), C-020 and C-023.

### C-015 - Invalid configuration is an illegal-state error

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: 2026-10-19T22:27:29+02:00 (pilot 2)
- Context/question: D-013 reports invalid configuration from the operation
  call; AC-12 requires it to be distinguishable from invalid input.
- Engineer decision: Throw the platform's invalid-operation exception, with
  a message that names the invalid setting and its value; not a subtype of
  the argument exception used for invalid IDs.
- Java mapping (proposal): `IllegalStateException`. It is not related to
  `IllegalArgumentException` by inheritance, so AC-12 and AC-4 outcomes stay
  distinguishable by type.
- Affected work: Configuration validation helper; AC-12.
- Status: Active
- Source: pilot2 engineering-decisions.md C-012.

### C-016 - CapacityExceededException

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: 2026-10-19T22:27:46+02:00 (pilot 2)
- Context/question: D-002 requires a dedicated capacity exception, never
  the runtime's out-of-memory error.
- Engineer decision: A public, sealed `CapacityExceededException` whose
  message states the capacity and which exposes the configured capacity.
- Java mapping (proposal): `public final class CapacityExceededException
  extends RuntimeException` with `int getCapacity()`. Unchecked, because a
  Java `Exception` subclass would be a checked exception and force every
  caller of `isUniqueAndProcess` to declare or catch it, which .NET's
  `Exception` does not. Never `OutOfMemoryError` (D-002, D-019).
- Affected work: Public API; AC-6.
- Status: Active
- Source: pilot2 engineering-decisions.md C-013.

### C-017 - Time seam is java.time.InstantSource

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured exactly; answered between
  2026-11-24T12:39:58+01:00 and 2026-11-24T12:46:19+01:00 (InstantSource).
  The reused part: 2026-10-17T18:44:37+02:00 (pilot 1 C-037).
- Context/question: Expiration refresh and the exact boundary (D-001,
  D-005) need deterministic control of the current time in tests. Pilot 2
  used an internal `IDateTimeProvider`. Options for Java: `InstantSource`,
  `java.time.Clock`, or an own interface.
- Engineer decision: Reused: an injected wall-clock time source, passed
  through a non-public constructor; tests use a fake whose time they move
  forward explicitly; no monotonic clock; the risk of wall-clock adjustments
  is accepted. New: the seam type is `java.time.InstantSource` (Java 17),
  not `Clock` and not an own interface. Production passes
  `Clock.systemUTC()`, which implements `InstantSource`.
- Affected work: Expiry logic; test seam (a test fake implementing
  `InstantSource`).
- Status: Active
- Source: pilot2 engineering-decisions.md C-007 (from pilot1
  engineering-decisions.md C-037), for the reused part.

### C-018 - Background cleaner: AutoCloseable owner, daemon executor

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured exactly; answered between
  2026-11-24T12:39:58+01:00 and 2026-11-24T12:46:19+01:00 (executor and
  owner). The reused part: 2026-10-19T22:28:43+02:00 (pilot 2 C-014).
- Context/question: How the background cleaner (C-010) runs and who owns
  it. Pilot 2 used `IDisposable` and `System.Threading.Timer`.
- Engineer decision: Reused: the cleaner starts on the first valid call, so
  the constructor stays trivial (D-013); default cadence 1 second,
  adjustable internally and tuned by benchmark; tests trigger a sweep
  through a non-public method, not the timer; calls still check expiry
  exactly, and a full buffer is swept inline (D-014). New: the deduplicator
  implements `AutoCloseable` and owns a single-thread
  `ScheduledExecutorService` on a named daemon thread, created lazily.
  `close()` shuts it down and is idempotent. A forgotten `close()` cannot
  block JVM exit.
- Affected work: Internal design; resource lifetime; testability seams.
- Status: Active
- Source: pilot2 engineering-decisions.md C-014, for the reused part.

### C-019 - Compare-and-swap synchronization

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: 2026-10-19T22:32:43+02:00 (pilot 2)
- Context/question: How calls synchronize within the concurrent map
  (C-010).
- Engineer decision: Each entry holds its last-attempt time; refresh and
  re-acceptance of an expired ID are compare-and-swaps; a new ID reserves a
  slot with an atomic counter and then inserts; a lock is taken only to
  sweep. Not presumed faster than the global-lock baseline until measured
  (C-011).
- Java mapping (proposal): an `AtomicLong` (or `VarHandle` field) per
  entry, `compareAndSet`; an `AtomicLong` slot counter; `putIfAbsent` for
  the insert; a `ReentrantLock` (or `synchronized`) sweep guard.
- Affected work: Internal design; AC-7 (D-015).
- Status: Active; refined by C-020 for the miss path.
- Source: pilot2 engineering-decisions.md C-015.

### C-020 - Per-ID claim before reserving a slot for a new ID

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: 2026-11-24T11:43:48+01:00 (pilot 2 C-028, retrospective
  per its correction)
- Context/question: Under C-019 alone, a call racing for the same unseen ID
  can be rejected for capacity while another racer holds the last slot
  between reservation and insert (pilot 2 RED 5f41d2b).
- Engineer decision: On a miss, a call first claims the ID in a second
  concurrent map. A call that loses the claim yields and retries. The claim
  holder re-checks the entries, reserves a slot, inserts, and releases the
  claim in `finally`. Same-ID racers wait for the claim holder, so the miss
  path for one ID is not lock-free; calls for different IDs, hits and the
  sweep are unaffected.
- Java mapping (proposal): `ConcurrentHashMap<UUID, Boolean>` claims with
  `putIfAbsent`; the loser calls `Thread.yield()` and retries.
- Affected work: Operation miss path; AC-7 (D-015).
- Status: Active; refines C-019.
- Source: pilot2 engineering-decisions.md C-027, approved by C-028.

### C-021 - Every invalid transaction ID throws IllegalArgumentException

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured exactly; answered shortly before
  2026-11-24T12:39:58+01:00
- Context/question: D-004 and D-019 require an argument exception. Java's
  idiom for null, `NullPointerException`, is not an
  `IllegalArgumentException`. Java has no `ParamName` and appends no message
  suffix, so pilot 2's reasons for its two custom types (C-022, C-025)
  do not apply. Options: plain `IllegalArgumentException`; two
  `IllegalArgumentException` subclasses as in pilot 2;
  `NullPointerException` (conflicts with D-004).
- Engineer decision: Plain `IllegalArgumentException` for null, empty,
  whitespace-only and malformed IDs. No custom exception types. Null keeps
  its own message (as pilot 2 C-022); empty, whitespace-only and malformed
  IDs use the invalid-ID message. Tests tell null from the other cases by
  the message.
- Affected work: ID validation; AC-4 tests.
- Status: Active

### C-022 - Surrounding whitespace is the String.strip() set

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured exactly; answered between
  2026-11-24T12:39:58+01:00 and 2026-11-24T12:46:19+01:00
- Context/question: D-017 allows surrounding whitespace around a GUID, and
  D-020 leaves the exact character set to the architect. Pilot 2 used
  .NET's `char.IsWhiteSpace` (Unicode White_Space). Options:
  `String.strip()`, the Unicode White_Space set written by hand, ASCII
  only.
- Engineer decision: Leading and trailing characters for which
  `Character.isWhitespace` is true are allowed, as removed by
  `String.strip()`: U+0009 to U+000D, U+001C to U+001F, U+0020, and the
  Unicode space, line and paragraph separators (Zs, Zl, Zp) except the
  no-break spaces U+00A0, U+2007 and U+202F. Unlike pilot 2, U+00A0,
  U+2007, U+202F and U+0085 are not whitespace, so a GUID surrounded by
  them is malformed; and U+001C to U+001F are whitespace, which pilot 2
  did not accept. A string that is empty after stripping is invalid
  input (D-004).
- Affected work: GUID parsing helper; AC-4 tests.
- Status: Active

### C-023 - Error messages without the .NET parameter suffix

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured exactly; answered between
  2026-11-24T12:39:58+01:00 and 2026-11-24T12:46:19+01:00
- Context/question: Pilot 2's invalid-ID templates end with
  ` (Parameter 'transactionId')` (C-024, C-026), the suffix .NET appends to
  `ArgumentException`. Java appends nothing and has no parameter name.
- Engineer decision: Drop the suffix. Templates:
  `TransactionId value '%s' is not a valid Guid.` (empty, whitespace-only,
  malformed; the original, unstripped input is the value) and
  `TransactionId must not be null.` C-013 still holds: the templates live in
  `ValidationError` and tests reuse them.
- Affected work: `ValidationError`; AC-4 tests.
- Status: Active; replaces the suffix part of pilot 2 C-024 and C-026 for
  pilot 3.

### C-024 - Java test style

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured exactly; answered shortly before
  2026-11-24T12:46:19+01:00
- Context/question: `.claude/agents/test-writer.md` and the shared
  `tdd-common-rules` are written for C#, xUnit and FluentAssertions. Which
  Java conventions do the coding agents follow? The architect proposed a
  mapping that keeps each C# rule's intent.
- Engineer decision: Approved as proposed:
  - JUnit 5 and AssertJ (C-004); run with `mvn -q test` (C-002).
  - Package `transactiondeduplication` for source and tests; tests share
    the package so they reach package-private seams.
  - Test classes are package-private `final class`; a shared test base is
    an `abstract class`; fakes and helpers are package-private. One test
    class per scenario.
  - Options: `protected final TransactionDeduplicatorOptions options = new
    TransactionDeduplicatorOptions();` (mutable, setters; C-014). Values a
    test depends on are set as its first statements; happy-path values come
    from common setup.
  - The system under test comes from `sut()`, created lazily on first call;
    an `@AfterEach` method closes it (C-018).
  - No import alias in Java: tests use static imports of the
    `ValidationError` templates and `format(...)` (C-013).
  - Names: snake_case test methods, no `@DisplayName`, for example
    `void expired_item_can_be_processed_again()`.
  - Layout: one act and assertion per statement, for example
    `assertThat(sut().isUniqueAndProcess(id)).isTrue();`; a blank line
    between steps of a scenario; each time move on its own line.
  - Inputs: `UUID.randomUUID().toString()` for any valid ID; generic
    invalid values built as `"not-valid-" + UUID.randomUUID()`; literal
    cases through `@ParameterizedTest` with `@ValueSource`; randomized
    durations from `ThreadLocalRandom`.
  - Time: a fake `InstantSource` (C-017) starts at `Instant.now()`, captured
    once, and moves only by `advance(Duration)`, for example
    `advance(expiration.minusMillis(1))`; never set to an absolute time.
  - Errors: compare the full message, built from the source template:
    `assertException(input, IllegalArgumentException.class,
    format(INVALID_TRANSACTION_ID, input))`, a protected helper on the
    shared base class wrapping `assertThatThrownBy(...)
    .isExactlyInstanceOf(...).hasMessage(...)`.
  - Reusable assertions are protected helper methods on the shared abstract
    test base class (Java's substitute for C# extension methods on the
    system under test); not an AssertJ custom assert.
  - Concurrency: one helper `List<T> runConcurrent(Callable<T> call, int
    calls)` (default 16) on a fixed thread pool with a `CountDownLatch`
    start gate, so calls overlap.
  - Common code rules in Java: `final` in place of `sealed`; `static` only
    for constant holders and stateless helpers; the `Extensions/` and
    expression-bodied rules do not apply.
- Affected work: Every RED and REFACTOR stage; proposed guidance changes
  in [agent-guidance-proposal.md](agent-guidance-proposal.md). Until the
  controller applies approved guidance changes, this entry overrides the
  C# test style in `test-writer.md` for pilot 3.
- Status: Active

### C-025 - UTF-8 source encoding

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (architect proposal; decided by the design
  approval)
- Context/question: The machine's platform encoding is Cp1250 (`mvn -v`,
  locale pl_PL), and the repository path contains a non-ASCII character
  (`Michał`). Without an explicit encoding, Maven warns and compiles
  sources in the platform encoding.
- Proposal: set `project.build.sourceEncoding` and
  `project.reporting.outputEncoding` to UTF-8 in `pom.xml`; error templates
  stay ASCII.
- Affected work: Scaffold.
- Status: Proposal

### C-026 - Package-private seams in place of InternalsVisibleTo

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (architect proposal; decided by the design
  approval)
- Context/question: Pilot 2 exposed its seams (time provider, cleaner
  interval, `SweepExpired`, `RetainedCount`) as `internal` members through
  `InternalsVisibleTo`. Java's closest equivalent is package-private
  visibility, with tests in the same package (C-024).
- Proposal: the seams are package-private members of
  `TransactionDeduplicator`: a constructor taking options, an
  `InstantSource` and a cleaner interval (`Duration`, where `null`
  disables the timer), `void sweepExpired()`, and `int retainedCount()`.
  No Java module (`module-info.java`) is declared.
- Affected work: Testability seams; scaffold.
- Status: Proposal

### C-027 - Pinned versions

- Recorded at: 2026-11-24T12:46:19+01:00
- Decision time: Not captured (architect proposal under C-006; decided by
  the design approval)
- Context/question: C-006 asks for exact versions. Chosen as the latest
  releases on Maven Central on 2026-11-24 within the engineer's stack
  (JUnit 5, not JUnit 6; surefire 3.x).
- Proposal:
  - `org.junit:junit-bom` 5.14.4 (`junit-jupiter`, test scope; includes
    `junit-jupiter-params`);
  - `org.assertj:assertj-core` 3.27.7 (test scope);
  - `maven-compiler-plugin` 3.16.0 (`release` 17);
  - `maven-surefire-plugin` 3.6.1.
  Mockito is not added (C-004).
- Affected work: Scaffold (`pom.xml`).
- Status: Proposal

## Consistency review

- C-001, C-002, C-003, C-008 and C-027 together fix the scaffold. No
  conflict.
- Four outcomes stay distinguishable by type (AC-4, AC-6, AC-12): `boolean`,
  `IllegalArgumentException` (C-021), `CapacityExceededException extends
  RuntimeException` (C-016) and `IllegalStateException` (C-015). None is a
  supertype of another.
- C-021 keeps D-004 and D-019 (argument-exception family) and drops pilot 2's
  custom types; it does not change any outcome.
- C-022 names the set D-020 left to the architect. It differs from pilot
  2's set (four code points fewer, four control characters more), which
  the criteria allow, since they leave the set to the architect.
- C-018 keeps the constructor free of validation and of thread start-up,
  consistent with D-013.
- C-019 and C-020 port pilot 2's synchronization; C-011 keeps the
  global-lock baseline as the comparator.
- C-024 overrides the C# test style in the agent guidance only for pilot 3;
  the guidance files themselves are unchanged until the engineer approves
  the proposed changes.

No unresolved conflict.

### C-028 - Design approved, including its proposals

- Recorded at: 2026-11-24T12:49:53+01:00
- Decision time: 2026-11-24T12:49:53+01:00
- Context/question: Approve `TransactionDeduplicator-Design.md`, including
  the items it marks "Proposal" and the "Java mapping (proposal)" parts of
  C-008, C-010, C-013 to C-016, C-019 and C-020.
- Engineer decision: Approved as written, including:
  - a `null` options argument throws `NullPointerException` from the
    constructor (not configuration validation, so D-013 is unaffected);
  - a `null` expiration is invalid configuration, reported on the call;
    expiration is checked before capacity;
  - `isUniqueAndProcess` checks in order: closed, a stored cleaner
    failure, configuration (C-015), then the ID (C-021), all before any
    state is touched;
  - calls after `close()` throw `IllegalStateException`; `close()` is
    idempotent; a sweep failure is stored and the next call throws
    `IllegalStateException` with it as the cause;
  - the capacity message is pilot 2's text;
  - strict X-format parsing (exact widths, no whitespace inside the GUID);
  - C-025 (UTF-8), C-026 (package-private seams, no `module-info.java`) and
    C-027 (pinned versions), which become Active.
- Affected work: Public API, failure handling, scaffold, controller handoff.
- Status: Active

### C-029 - Agent guidance made reusable for Java

- Recorded at: 2026-11-24T12:59:28+01:00
- Decision time: 2026-11-24T12:59:28+01:00 (shortly before)
- Context/question: C-024 overrides the C# test style for pilot 3 until the
  guidance changes; `agent-guidance-proposal.md` proposed the change.
- Engineer feedback (verbatim): "prepare data and new pilot3 dir for
  controller. Update skill where necessary to be reusable for java as
  well".
- Engineer decision: Make the coding-agent guidance language-neutral, with
  a Java form next to the C# rules: a "Languages" mapping in
  `tdd-common-rules`, a "Java form" in `test-writer.md` (from C-024) and in
  `implementer.md`, small wording changes in `refactorer.md`,
  `tdd-controller` and `tdd-architect`. The C# rules are unchanged. The
  change is applied as its own `Guidance:` commit by the session that owns
  the worktree's `.claude/` folder; this architect session cannot write
  there.
- Affected work: Every coding stage of pilot 3 and later Java pilots. C-024
  stays the pilot 3 rule; once the guidance commit lands, the two agree.
- Status: Active
