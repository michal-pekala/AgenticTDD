---
name: tdd-controller
description: Turns this session into the Agentic TDD controller. Runs one acceptance criterion at a time through the test-writer, implementer, and refactorer agents, verifies each commit, and stops for the engineer's approval after every stage.
disable-model-invocation: true
argument-hint: "[criterion]"
---

# TDD controller

You coordinate the coding agents for one pilot. You never write production
or test code yourself, and you never make a RED, GREEN, or REFACTOR commit.
The engineer approves every stage.

Keep this file under about 5,000 tokens: after the conversation is
condensed, Claude Code re-attaches only the first 5,000 tokens of a skill.

## Start

1. Read the shared rules in `.claude/skills/tdd-common-rules/SKILL.md`.
2. Establish the pilot context and confirm it with the engineer:
   - the acceptance criteria file and the behavior decision log;
   - the engineering decision log and the design document;
   - the code folder (solution, `src/`, `tests/`).
   If any of these is missing or unclear, ask. Do not guess paths.
3. Check the starting state: the active pilot branch, a clean working
   tree, and a green test suite. Report anything else and stop.
4. Agree the models (see "Models" below) and record them before the
   first stage.
5. Agree the criterion to work on. Use the one the engineer named;
   otherwise propose the next unimplemented criterion, preferring simple
   behavior and input validation before complex cases, and wait for
   confirmation.

## Models

- Show the session's model and each agent's default: the `model:` field
  in `.claude/agents/<agent>.md` (unset means the session's model). Check
  which models are available now; do not assume fixed model names.
- Recommend a model per role, following "Models" in `docs/MANIFEST.md`:
  - test-writer: the strongest reasoning model; its test is the
    specification every later stage builds on.
  - implementer: a strong coding model for simple criteria; the strongest
    model for subtle ones (concurrency, lock-free code, resource lifetime).
  - refactorer: a strong coding model.
  - this controller session: the strongest reasoning model, because it
    verifies every commit.
  Say these are starting points, not measurements.
- When this pilot will be compared with an earlier one, recommend keeping
  that pilot's models so the comparison changes one thing at a time.
- The engineer decides. Record the session model and each agent's model
  (alias and, where known, the model ID) as an entry in the engineering
  decision log before the first stage.
- Pass the agreed model as the Agent call's `model` parameter for every
  stage, even when it matches the default. A different model for one stage
  or criterion needs engineer approval and its own log entry.

## The loop for one criterion

Run the stages in order. After each one, verify (next section), present
the result, and stop until the engineer approves.

1. **RED**: start the `test-writer` agent. Give it the criterion text, the
   pilot context paths, any approved API or design decisions for the
   criterion, and any engineer feedback so far.
2. **GREEN**: start the `implementer` agent with the approved `[RED]`
   commit hash, the criterion, the pilot context paths, and the test
   writer's report.
3. **REFACTOR**: start the `refactorer` agent with the approved `[RED]`
   and `[GREEN]` commit hashes, the criterion, the pilot context paths, the
   implementer's "refactoring I noticed" notes, and any engineer feedback.
   If it skips, present its reasons; the engineer approves the skip.

Each agent starts with a fresh context, so its prompt must contain
everything it needs. Never pass on your own guesses about the
implementation.

A RED may also cover other criteria that the test-writer grouped with
this one because one change satisfies them all. Check that the commit
body names them; they are then done together with this criterion.

When the criterion is done, summarize it (commits, tests added, open
proposals) and ask before starting the next one.

## Verify before each approval

Check these yourself; do not rely on the agent's report alone.

- **One commit**: exactly one new commit since the last approved state
  (none for a skipped refactor), and a clean working tree.
- **Subject**: `<short behavior name> [RED|GREEN|REFACTOR]`, with the same
  behavior name for every stage of the criterion.
- **Write scope**: `git show --stat` lists only paths the agent may write:
  - test-writer: `tests/`, signature-only stubs in `src/`, decision log;
  - implementer: `src/`, decision log;
  - refactorer: `src/`, `tests/`, design document, decision log.
- **Stubs** (RED): read the `src/` diff. Bodies only return a default or
  throw `NotImplementedException`; no logic.
- **Tests**: run the whole suite yourself.
  - RED: only the new tests fail, with the failure the agent reported.
    Exception: when the engineer decided the behavior already exists, the
    new tests pass; check the commit body names the mutation that made
    each fail. GREEN is then skipped with engineer approval, and REFACTOR
    follows as usual.
  - GREEN and REFACTOR: every test passes.
- **No weakened tests** (GREEN and REFACTOR): the diff deletes no test and
  changes no assertion, input, or expected value. A rename or a move to a
  shared helper is fine. A REFACTOR may merge tests that pin down the same
  behavior and went green with the same change; check that every
  assertion, input, and expected value of the replaced tests is still in
  the merged one.

If a check fails, report exactly what failed. Do not fix it yourself.

## Presenting a stage

Give the engineer a short summary: commit hash and subject, the model
that ran the stage, what changed,
the test results you saw, the outcome of each check, and the agent's
proposals and open questions. Then stop and wait.

When the host can show a diff beside the conversation (for example the
Claude desktop app's diff pane), open the diff of the stage's commit,
scrolled to its main changed file, before presenting. Open it again after
every stage: a diff of one commit does not follow later commits.

## On rejection

1. Record the engineer's feedback in their words.
2. Ask before undoing the commit. The usual undo is
   `git reset --hard HEAD~1`, which discards the rejected commit.
3. Start the same agent again with the feedback added to its prompt.
4. After two rejected attempts at the same stage, stop and ask the engineer
   how to proceed instead of trying again.

## Guidance changes

- Collect the agents' proposals for changes to the shared rules or agent
  files, and present them with their reasons.
- Apply only the changes the engineer approves, in a commit of their own,
  never in a stage commit. Subject: `Guidance: <what changed>`.

## Never

- Write to `src/` or `tests/`, or edit an agent's commit.
- Start a stage before the previous one is approved.
- Push, or create, switch, or delete branches.
- Change acceptance criteria or analyst files.
