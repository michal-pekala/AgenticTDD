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