# Proposal: Java support in the coding-agent guidance

Status: proposal by the pilot 3 architect, 2026-11-24. Not applied. Per
`tdd-common-rules` ("Changing guidance"), the engineer approves it and the
controller applies it in a commit of its own.

## Problem

`.claude/agents/test-writer.md` and `.claude/skills/tdd-common-rules/SKILL.md`
are written for C#: xUnit and FluentAssertions, `sealed`, `internal`,
expression-bodied members, extension methods under `Extensions/`, `Fixture/`,
`using` aliases, `Task.WhenAll`, `NotImplementedException`, `tests/`.
`implementer.md` and `refactorer.md` name `src/` and `tests/` folders, while
Maven uses `src/main/java` and `src/test/java`. For pilot 3 the Java
mapping is engineer decision C-024 in
[engineering-decisions.md](engineering-decisions.md), which overrides the
C# style for this pilot until the guidance changes.

## Proposed change

Keep each rule's intent, and move language-specific forms into a
per-language section, so that one agent file serves both languages:

1. **tdd-common-rules, "Code conventions"**: state the intent once, then
   one line per language:
   - C#: `sealed`; extension-method classes under `Extensions/`;
     expression-bodied one-liners.
   - Java: `final`; no extension methods, so stateless helpers are
     `static` methods in a `final` class with a private constructor;
     expression-bodied and `Extensions/` rules do not apply.
   - `static` only for constant holders and helper classes (both).
2. **tdd-common-rules, "Repository boundaries"**: code folders are
   `src/` and `tests/` (C#) or `src/main/java` and `src/test/java` (Java,
   Maven).
3. **test-writer, "What you may write"**: name both test and source roots;
   stub bodies throw `NotImplementedException` (C#) or
   `UnsupportedOperationException` (Java).
4. **test-writer, "Test style"**: replace the first line with "Use the
   pilot's test framework and the style its decision log records; the
   rules below are the C# form", and add a "Java form" subsection with the
   items of C-024: JUnit 5 + AssertJ; package-private `final` test
   classes in the source package; `protected final` options; lazy `sut()`
   closed in `@AfterEach`; snake_case method names; static imports in
   place of the `Error` alias; `@ParameterizedTest` + `@ValueSource` in
   place of `InlineData`; `ThreadLocalRandom` in place of `Random.Shared`;
   `advance(Duration)` in place of `Now += ...`; `Duration.ofSeconds(..)`
   / `minusMillis(1)` in place of `10.Seconds()`; `assertException` helper
   on the base class wrapping `assertThatThrownBy(..)
   .isExactlyInstanceOf(..).hasMessage(..)`; `runConcurrent(Callable<T>,
   int)` with a fixed pool and a `CountDownLatch` start gate in place of
   `Task.WhenAll`.
5. **implementer and refactorer**: name the Maven commands (`mvn -q test`,
   `mvn -q -Dtest=Class#method test`) next to `dotnet test`, or point to
   the pilot log for the build command.

## Alternative

Leave the guidance as it is and rely on C-024 for pilot 3. It is cheaper
now, but every Java stage then reads guidance that contradicts the log,
and the common rules tell the agent to stop and ask on such a conflict.
The controller would have to repeat "C-024 overrides the test style" in
every agent prompt.
