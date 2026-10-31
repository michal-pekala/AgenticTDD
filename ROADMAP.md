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
- [ ] **Pilot 5: repeat pilot 4's task, with memory monitoring.** Apply
  pilot 4's conclusions and run the same task again to measure the effect.
  The profiler is extended to monitor memory usage as well as speed.

## Later

- [ ] **Documentation agent** (planned during pilots 4–6): an agent that
  writes and updates the project documentation from the decision logs,
  the design, and the code.
- [ ] **Pilot 6: code quality agent.** An independent agent that oversees
  code quality against detailed metrics: class and method size,
  encapsulation, object-oriented principles and SOLID, and test coverage,
  including mutation testing to check that the important paths are really
  covered. Possibly integrated with tools such as SonarQube. The agent
  works with the architect and the coding agents, and their skills are
  extended to match.
- [ ] **Pilot 7: repeat pilot 6's task** with the latest version of the
  skills, then evaluate and compare the results with pilot 6.
- [ ] **Pilots 8–10: work in existing codebases.** Tasks inside an existing
  repository instead of a new solution, with fixes, extensions, and an
  evaluation after each pilot:
  - pilot 8: a small task in a repository under 100 lines of code;
  - pilot 9: a repository under 200 lines of code;
  - pilot 10: a repository under 500 lines of code.
- [ ] **Beyond C#/.NET.** The pilots are in C# today; the workflow itself is
  defined as agent skills and Markdown rules, and the goal is to extend
  it to other languages.
- [ ] **Full evaluation after pilot 10**, and a decision on the project's
  further direction.
