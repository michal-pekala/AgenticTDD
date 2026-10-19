Agentic TDD
===========

Project manifest

Purpose
-------

Explore a reliable, human-supervised workflow in which AI agents help deliver
software using test-driven development (TDD). The project begins with a small
pilot and adds automation only after the workflow demonstrates that it can
produce correct, understandable, maintainable changes.

The engineer remains accountable for product decisions, design approval,
exceptions to quality gates, and final acceptance.

Goals
-----

- Translate customer requests into clear, testable acceptance criteria.
- Implement behavior incrementally using a red-green-refactor cycle.
- Keep changes reviewable, tests meaningful, and delivery results observable.
- Make handoffs, decisions, failures, and recovery steps explicit.
- Measure quality, iteration cost, and delivery performance before increasing
  agent autonomy.

Initial workflow
----------------

Begin with a task analyst, three coding agents (test writer, implementer,
refactorer) run by a controller session, and an independent quality
reviewer. The engineer approves decisions and every commit.
Use deterministic workflow logic for status, deadlines, retries, and routing
where practical; these concerns do not require an always-running AI agent.

Roles
-----

1. Controller
   - Today: a Claude Code session started with `/tdd-controller`
     (`.claude/skills/tdd-controller/`). It runs one criterion at a time
     through the coding agents, verifies each commit, and stops for engineer
     approval. It never writes production or test code itself.
   - Later automation:
     - Coordinates work and routes artifacts between roles.
     - Tracks status, deadlines, retries, and blockers.
     - Reports progress and pipeline issues.
     - Uses deterministic automation for timers and state transitions where
       possible; escalates stalled or unrecoverable work to an engineer.

2. Task analyst
   - Clarifies the customer's goal, scope, terminology, and unanswered
     questions before implementation.
   - Produces observable, independently verifiable acceptance criteria.
   - Defines expected execution context, scale, workload, and measurable
     performance requirements when relevant.
   - Separates confirmed requirements from assumptions and open decisions.
   - Does not prescribe implementation details unless the customer explicitly
     requires them.

3. Coding agents
   Three Claude Code subagents (`.claude/agents/`), each with its own
   context, sharing the rules in `.claude/skills/tdd-common-rules/`:
   - Test writer (RED): writes the failing test for one criterion, with
     signature-only stubs so it compiles, and commits [RED].
   - Implementer (GREEN): makes the smallest production change that passes
     it, without touching tests, and commits [GREEN].
   - Refactorer (REFACTOR, optional): makes behavior-preserving cleanups of
     source and tests when a concrete benefit exists, and commits
     [REFACTOR]; otherwise it reports a skip.
   - All three explain decisions, tradeoffs, and remaining limitations, and
     propose guidance updates; the controller applies approved changes in a
     separate commit.

4. Quality reviewer (no agent yet)
   - Independently checks the implementation and tests against the approved
     acceptance criteria.
   - Verifies that tests assert required behavior, including relevant edge
     cases, concurrency, and failure behavior.
   - Reviews readability, unnecessary complexity, duplication, design
     boundaries, and applicable SOLID principles.
   - Checks test, build, formatting, static-analysis, and coverage results
     required by the repository.
   - Gives specific, actionable feedback tied to a criterion or quality gate.
     Unresolved ambiguity or disagreement is escalated to an engineer.
   - Proposes quality-guidance updates for engineer review; it does not
     silently change its own rules.

Delivery process
----------------

1. Intake: The analyst asks questions, records scope and assumptions, and
   drafts acceptance criteria. The engineer resolves open decisions and
   approves the criteria before coding begins.
2. Design: For decisions with meaningful tradeoffs, the coding agent presents
   an implementation proposal. The engineer approves it before implementation.
3. Red: The coding agent adds focused tests in the test project for one
   approved criterion and runs them to verify they fail for the intended
   reason. Do not leave a shared branch broken solely to preserve a RED-stage
   commit. Pilot exception: during pilots, RED is committed on its own so
   the engineer can review the test before any implementation exists; the
   target workflow combines RED and GREEN in one commit.
4. Green: Implement the smallest production change that satisfies the tests.
   Run the targeted tests and relevant existing checks.
5. Refactor: Improve production and test code for clarity and maintainability
   while keeping the suite green. Avoid abstractions that do not serve the
   requirements.
6. Review: The independent reviewer checks the change against the approved
   criteria and quality gates. The coding agent addresses feedback in a
   bounded revision loop; escalate persistent disagreements to the engineer.
7. Repeat: After approval, proceed to the next criterion. The engineer reviews
   the complete change before merge.

Commit at useful, reviewable checkpoints, with passing tests for shared
changes. Commit messages and the final handoff should explain the behavior
delivered and important decisions.

Requirements and work tracking
------------------------------

- Keep each acceptance criterion small enough to verify independently.
- Track status and link every criterion to its tests, implementation change,
  review outcome, and relevant engineer decision.
- Define handoff ownership, timeout behavior, retry limits, and escalation
  before automating the workflow.
- Make failures visible. A stalled or failed task must leave work status and
  repository state understandable and recoverable.
- Keep CI as the reproducible source of truth for builds, tests, formatting,
  and static analysis.

Quality principles
------------------

- Optimize for tests that verify behavior, not for a line-coverage number
  alone. Set meaningful coverage expectations for important behavior and
  document engineer-approved exceptions.
- Consider branch coverage or mutation testing when they add useful evidence.
- Prefer readable production code and tests over verbosity or unnecessary
  abstractions.
- Use an independent reviewer; a different model from the coding agent may
  help reduce shared blind spots.
- Record engineer decisions as project guidance. Changes to agent instructions
  or skills are proposals until reviewed and approved.

Models
------

Choose models by capability and evaluate them on representative tasks from the
target repository. Favor strong reasoning and careful instruction-following
for analysis and review, strong coding ability and repository context for
implementation, and speed and cost-efficiency for routine orchestration.
The agents run in Claude Code. Each agent file can set its own model with
the `model` field; when it is not set, the agent uses the session's model.
Model availability changes by Claude plan and organization; validate the
options currently available rather than relying on fixed model names.

Pilot and success measures
--------------------------

1. Use one small, low-risk feature with explicit engineer approval between
   analysis, design, implementation, and final review.
2. Record missed requirements, review iterations, CI failures, cycle time, and
   whether the resulting change is understandable and maintainable.
3. For performance requirements, record benchmark workload, environment,
   duration, throughput, and relevant latency. Do not claim a target is met
   without measurements.
4. Improve prompts, workflow, and quality gates based on observed results.
5. Automate repeatable handoffs and increase autonomy only after the pilot is
   reliable. Keep human approval for ambiguous requirements and significant
   design decisions.

Open questions
--------------

- Next step: run pilot 2 with the three coding agents and the controller,
  on a new, small problem with no performance requirement.
- Benchmark ownership (deferred until a problem needs one). Options: the
  test writer writes the benchmark and target check and the implementer
  optimizes; a fourth performance agent owns benchmarks and reports; or the
  reviewer runs benchmarks as a final review gate. Pilot 1 defined
  benchmark rules but never built a benchmark.
- RED and GREEN in one commit (the target): the test writer then needs
  another way to hand the failing test to the implementer, such as the
  working tree or a handoff note.
- Enforcing write scopes: today they rely on agent instructions, the
  controller's checks, and engineer review. Per-agent hooks that reject
  out-of-scope writes are an option if agents stray.
- Quality reviewer: no agent yet.

Scope boundaries
----------------

This project develops and evaluates an agent-assisted software delivery
workflow. An agent's implementation must not claim guarantees that the
designed system does not provide. Product-specific guarantees, data durability,
security, and operational requirements must be explicit acceptance criteria
and verified independently.
