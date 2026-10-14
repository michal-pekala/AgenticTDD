Agentic TDD. I'd like to creata 4 AI agents to develop software in TDD approach. 

Agent 1 - controller
  - coordinates other agents,
  - monitors progress,
  - acts when some action takes too long,
  - reports progres general progres (observability), 
  - reports any issues in the pipeline,

Agent 2 - task analyst
  - able to translate customer input into acceptance criteria,
  - able to ask questions,
  - should create a list of detailed requirements described as facts about the system behaviour
  - define expected performance criteria / execution context, scale, expected load etc
  - as a result generates detailed task description

Agent 3 - coding: works in a loop:
1. take next acceptance criteria / requirement, 
  - it should start from the simplest requirements, edge cases, input params validators etc gradualy extending solution with more complex behaviour.
2. implement related tests/facts 
  - XUnit + Fluent Assertions preffered,
  - extremal human-readability expected,
  - only in test project.
  - tests should fail at that point [RED]
  - commit
3. implement a minimal change that makes tests passing [GREEN]
  - commit
4. Rigorous refactor (src & test code):
  - removes any code duplications,
  - improve readability,
  - keeps SOLID principles,
  - extracts abstractions or improve existing ones, including an extensive redesign of existing code if necessary.
  - commit
5. asks engineer in case of any ambiguity. After implementation engineer's decisions should be used to update coding skill. 
6. finally commits the change with full explanation of decisions made.
7. after each loop asks Agent 4 for quality approval: if approved goes to step 1, else apply required improvements and update its codin skills.

Agent 4 - quality guard 
- checks if each line of src code is covered by tests. There might be exceptions, but must be accepted by engineer.
- checks if there is any verbosity in the code that obscures the main logic (src or tests), extremal human-readability expected (this can be a skill built with engineer)
- checks if methods/classes/dependencies are well defined, compliant with SOLID principles and any other custom skills already defined 
- double checks if implemented tests are matching initial requirements
- if any criteria criteria is not met - returns feedback to Agent 3 that should either start from step 1 or just update the changes as part of the step 4,
- if there are any doubts asks engineer to double check the quality and approve, then based on engineer's decision - improves the quality check skill


Suggested delivery approach
===========================

Start with a small, human-supervised pilot rather than four autonomous agents. Use a task analyst, coding agent, and independent quality reviewer first. Keep the controller's scheduling, deadlines, retries, and status tracking in deterministic workflow logic where possible; use an agent for decisions that require reasoning, not as a continuously polling timer.

Work tracking and handoffs
--------------------------

- Break work into small, independently verifiable acceptance criteria.
- Track each criterion's status and link it to its tests, implementation change, review result, and any engineer decision.
- Define explicit handoffs, timeout behavior, retry limits, and escalation to an engineer. A failed or stalled task must leave the branch and work status in a known, recoverable state.
- Keep CI as the reproducible source of truth for builds, tests, formatting, and static analysis.

TDD and commits
--------------

For each criterion, write a focused behavior test and run it first to verify it fails for the intended reason (RED). Implement the smallest change that makes it pass (GREEN), then refactor production and test code while keeping the suite green. Commit at useful, reviewable points. A RED-stage test does not need to be committed while failing; avoid leaving shared branches in a broken state.

Quality gates
-------------

- Prefer tests that verify required behavior over a target of 100% line coverage. Set meaningful coverage goals for important behavior, and consider branch coverage or mutation testing where useful.
- Have the reviewer independently compare the acceptance criteria, tests, and implementation. A different model from the coding agent can help reduce shared blind spots.
- Make review feedback actionable and tied to a criterion or quality rule. Set a bounded revision loop and escalate unresolved disagreements or ambiguity to an engineer.
- Keep engineer decisions as recorded project guidance. Propose changes to agent instructions or skills for review; do not silently let agents rewrite their own rules.

Agent and model selection
-------------------------

Choose models by capability and evaluate them on representative tasks from the target repository. Favor strong reasoning and instruction-following for analysis and review, strong coding ability and repository context for implementation, and speed and cost-efficiency for routine orchestration. Model availability changes by Copilot plan and organization, so validate the currently available options rather than relying on a fixed model-name recommendation.

Pilot sequence
--------------

1. Start with one small, low-risk feature and explicit engineer approval between stages.
2. Measure whether the workflow produces correct, maintainable changes and catches missed requirements; record cycle time, review iterations, and CI failures.
3. Improve the prompts, workflow, and quality gates based on observed results.
4. Increase autonomy only after the pilot is reliable; retain human approval for ambiguous requirements and significant design decisions.