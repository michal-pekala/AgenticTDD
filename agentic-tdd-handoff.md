# Handoff: AgenticTDD pilot2 (from claude.ai chat to the Code session)

> Lifetime (decided 2026-10-19): keep this file until the controller's
> instructions exist. Then fold the open items (deferred decisions 2, 7, 8,
> remaining plan steps, hooks idea) into them or the decision log, and
> delete this file in the same commit.

Read this first, then verify it against the actual repo files. This summary was
written from a chat that could only read the repo through GitHub; the files on
disk are the source of truth.

## Who and why

The engineer owns `michal-pekala/AgenticTDD` and wants to LEARN how to build
an agentic TDD ecosystem. Pilot2 is the vehicle for that learning. So:

- Guide step by step. Stop at every significant point and whenever an
  engineering decision is needed. Explain the reasoning, offer options with a
  recommendation, then wait.
- Ask one decision at a time.
- Never commit, push, or create branches without explicit approval. The
  engineer reviews changes in Visual Studio (Git Changes) and commits
  themselves unless they say otherwise. Keep Edit mode (approve each change).

## Repo facts (verify on disk)

- Branch: `agentic-tdd-pilot2` (created by the engineer, from main).
- Layout on main: `README.txt` (manifest), `pilot/TransactionDeduplicator.txt`
  (customer task), `pilot/analyst/` (`SKILL.md`,
  `TransactionDeduplicator-Acceptance-Criteria.txt`, `behaviour-decisions.md`),
  `pilot/coder/` (`TransactionDeduplicator.sln`, `src/`, `tests/`,
  `engineering/` with `SKILL.md` (~20 KB), `TransactionDeduplicator-Design.md`,
  `engineering-decisions.md`).
- Layout change (2026-10-19, step 3 started): `pilot/coder/` and the old
  coder `SKILL.md` are gone. `engineering/` moved to `pilot/engineering/`.
  The customer task moved to
  `pilot/TransactionDeduplicator/TransactionDeduplicator.txt`.
  Pilot 1's code now lives in `pilot/TransactionDeduplicator/pilot1/`:
  `TransactionDeduplicatorPilot1.sln`,
  `src/TransactionDeduplicatorPilot1.csproj`,
  `tests/TransactionDeduplicatorPilot1.Tests.csproj` (no per-project
  subfolders)
  (namespaces unchanged; `InternalsVisibleTo` updated). The solution has a
  `.claude` solution folder (agents, skills) for review in Visual Studio.
  Pilot 1's design doc and decision log still mention `pilot/coder/` paths;
  they are left as history.
- Existing workflow: analyst -> coder (RED, GREEN, optional REFACTOR) ->
  independent reviewer (described in README, no skill file yet). Append-only
  decision logs. Engineer approval after EVERY commit. Commit names:
  `<behavior> [RED|GREEN|REFACTOR]`. Visual Studio `engineering` solution
  folder shows the skill, design and decision log.

## Plan (the engineer's)

1. New branch `agentic-tdd-pilot2`. (done)
2. Split the single "coder" skill into three skills: test writer (RED),
   implementer (GREEN, src only), refactorer.
3. Analyse the existing engineering docs and reorganize them around the three
   new agents.
4. Start `pilot2` with the new agentic workflow, on a new small problem.

## Decisions made so far

- Shared rules live in ONE common file, `engineering/common.md`, referenced by
  all three skills (not repeated in each). Intended path:
  `pilot/coder/engineering/common.md` (may move in step 3).
- `common.md` was moved (2026-10-19) to the Claude Code skill
  `.claude/skills/tdd-common-rules/SKILL.md`, which all three agents
  preload via their `skills:` field so the rules are always in context.
  Do not set `disable-model-invocation` on it: that also blocks
  preloading. It was reviewed with the engineer in the Code session on 2026-10-19.
  Changes agreed there:
  - Worktree rule dropped: agents work in the engineer's checkout on the
    pilot branch and do not create, switch, or delete branches without
    approval.
  - Coding agents never edit acceptance criteria; they link from the design
    to the criteria, not the other way round.
  - Observable behavior and API contract get separate tests.
  - New "Changing guidance" section: agents do not edit `common.md` or skill
    files without approval and propose where new rules belong.
  - Separate RED commits are a pilot-only exception; the target workflow
    combines RED and GREEN in one commit.

## How I sorted the old coder skill (proposal, not yet approved)

- Test writer: test style (`Sut`, expression-bodied tests, one act/assert per
  line), randomized inputs, fake clock, `Fixture/` and `Extensions/` helpers,
  test class grouping, `RunConcurrent`, test naming, coverage list, `[RED]`.
- Implementer: behavior/API constraints as spec, design preference
  (ConcurrentDictionary + cleaner), constructor rules, validation helpers,
  blank-line style in source, smallest change only, `[GREEN]`.
- Refactorer: when a refactor is worthwhile, recording a skipped refactor,
  behavior-preserving only, `[REFACTOR]`.
- Shared (-> common.md): source of truth, decision-log protocol, repo
  boundaries, behavior-vs-design rule, commit naming and approval gates,
  evidence and handoff.

## Open decisions (queue, one at a time)

1. ~~Review/approve the draft `common.md`.~~ Done (see above).
2. ~~Who owns benchmarks and per-run reports.~~ Deferred (2026-10-19).
   Pilot 2's problem must have no performance requirement; decide ownership
   when a problem needs it. Pilot 1 defined benchmark rules (C-007, C-008)
   but never built a benchmark. Options kept for later:
   - A: test writer writes the benchmark and target check, implementer
     optimizes, whoever runs it writes the report.
   - B: a fourth performance agent owns benchmark code, runs and reports.
   - C: the reviewer (or engineer) runs benchmarks as a final review gate.
3. ~~Who owns shared test helpers (`Fixture/`, `Extensions/`).~~ Decided
   (2026-10-19): split by origin. The test writer may add a helper when the
   new failing test needs it. Extracting existing duplication into shared
   helpers and migrating old tests to them is the refactorer's job. The
   implementer never touches `tests/`. Put into the skills in step 2.
4. ~~Each agent's write scopes.~~ Decided (2026-10-19):

   | | `tests/` | `src/` | decision log | design doc | `common.md`, skills | `analyst/` |
   |---|---|---|---|---|---|---|
   | Test writer | new test + helpers it needs | signature-only stubs | append | no | propose only | no |
   | Implementer | no | yes | append | no | propose only | no |
   | Refactorer | behavior-preserving only | behavior-preserving only | append | yes | propose only | no |

   Stubs: new types, interfaces, methods, properties; bodies only return a
   default or throw `NotImplementedException`, no logic. Reason: a C# RED
   test must compile to fail for the intended reason. The test writer must
   show the test actually fails, since a default return can pass by
   accident.
5. ~~Cross-agent rules.~~ Decided (2026-10-19). "Migrate all matching
   assertions" is the refactorer's job (decision 3). Shared values such as
   error message templates are defined once in `src`; the test writer may
   add them as part of a stub; the implementer and tests reuse them and
   never duplicate the text.
6. ~~Names for the three skills.~~ Decided (2026-10-19): role names
   `test-writer`, `implementer`, `refactorer` (phase names would go stale
   once RED and GREEN merge into one commit). Filenames and folders are
   deferred to step 3, together with which tool runs the agents (Copilot
   and Claude Code each expect skills in their own place and format).
   Update (2026-10-19): the tool is Claude Code, and the three roles are
   Claude Code subagents (`.claude/agents/<name>.md`), each with its own
   context. The main session acts as controller: it starts each agent,
   checks its report, and stops at the engineer's approval gates. Write
   scopes are enforced by instructions and diff review for now; hooks that
   reject out-of-scope writes are an option for later.
   Commits (2026-10-19): the agents commit their own stage ([RED], [GREEN],
   [REFACTOR]) and stop; the engineer reviews the commit and approves or
   rejects it. In the design session, the engineer still commits.
   Drafted: `.claude/agents/test-writer.md`, `.claude/agents/implementer.md`,
   `.claude/agents/refactorer.md`.
   Guidance changes (2026-10-19): agents only propose; the controller
   session applies approved changes in a commit of its own, never in a TDD
   commit (pilot 1 put them in REFACTOR commits). Recorded in the common
   skill's "Changing guidance" section.
   For the refactorer draft: test renames are the refactorer's job (pilot 1
   allowed them in GREEN, but the implementer may not touch `tests/`).
   Conventions were also read from pilot 1's code, not only its old skill;
   do the same for the refactorer.
7. (Deferred to step 3, once the folder layout settles.) Where agents find
   the source-of-truth files: `common.md` names the acceptance criteria,
   `behaviour-decisions.md` and `engineering-decisions.md` but not their
   paths. Write them as a layout convention in `common.md` or in a small
   per-pilot file.
8. (Later, with agent handoffs.) Once RED and GREEN share one commit, the
   test writer needs another way to hand the failing test to the
   implementer (working tree or a handoff note).

## Reference

DeviaTDD (github.com/wernerbisschoff/deviatdd, MIT, Werner Bisschoff) has
separate RED/GREEN/JUDGE/REFACTOR phases and path-scoped GREEN writes (GREEN
may only write to `src/` and permitted paths; a reviewer rejects violations).
Use it only as a comparison; the engineer wants to design their own.

## Note

The claude.ai chat's GitHub connection is read-only (403 on writes), so all
file writing should happen here in the Code session.
