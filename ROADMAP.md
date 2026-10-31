# Roadmap

Agentic TDD grows through a series of pilots. Each pilot runs the workflow
on a real task, and its results decide what changes in the agents, their
skills, and the engineering rules before the next one.

## Now

- [ ] **Finish pilot 2.** Complete the TransactionDeduplicator task with the
  split workflow: analyst, architect, controller, and the three coding
  agents (test writer, implementer, refactorer).
- [ ] **Compare pilots 1 and 2.** Single coding agent versus three
  specialised agents: number and kind of engineer corrections, commit
  discipline, test style, and how often the agents built ahead of the
  tests. Turn the conclusions into skill and rule updates.

## Next

- [ ] **Pilot 3: performance.** The same task, extended with a new
  performance agent, a profiler that writes and runs benchmarks.
- [ ] **Model the agent interactions.** From pilot 3's results, define how
  the profiler, the architect, and the refactorer work together: who
  raises a performance problem, who decides on a design change, and who
  carries it out.
- [ ] **Pilot 4: a new, larger task with more autonomy.** The agents get a
  limit of steps they may take on their own before stopping for the
  engineer's approval.
- [ ] **Pilot 5: repeat pilot 4's task.** Apply pilot 4's conclusions and run
  the same task again to measure the effect.

## Later

- [ ] **Documentation agent** (planned during pilots 4–6): an agent that
  writes and updates the project documentation from the decision logs,
  the design, and the code.
- [ ] **Continue the process up to pilot 10**, each pilot followed by
  conclusions and skill updates.
- [ ] **Beyond C#/.NET.** The pilots are in C# today; the workflow itself is
  defined as agent skills and Markdown rules, and the goal is to extend
  it to other languages.
- [ ] **Full evaluation after pilot 10**, and a decision on the project's
  further direction.
