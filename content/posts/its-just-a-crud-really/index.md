---
title: "IT'S JUST A CRUD! REALLY?"
date: 2025-03-05T09:00:24+01:00
tags: [post, en]
draft: true
---

## plan

- Have you already heard/said "this app is just a CRUD"?
  - common assertion
  - I don't believe any of us have encounter such software

- Define CRUD
  - DB access operations : Create, Read, Update, Delete
  - "CRUD app" strict definion: your app is basically a form to manipulate data in the DB
  - something that can be generated or use off-the-shelf solution
  - only concern is storing data, no business logic
  - you don't pay devs only for a CRUD

- Add logic: business rules
  - "CRUD app" loosy definition: a single model with some rules upon it
  - validation rules (how to hightlight making business action on a DB model can be hard?)
  - data model vs domain model
    - anemic and rich models
    - on commands, we want to provide only relevant values, not all the model
    - reason of architectures like nTiers: introduce abstractions
  
- Driving by business (how to highlight issues with DB centric?)
  - from data modification (db centric) to business decision (domain centric)
  - business decisions may require complex data structures that not fits DB models
  - issue: the central model (db or domain) is the one that influence the code
  -> domain centric is easier to test as side effects are pushed to the boundary
  - time for dependency injection, from nTiers to hexa/onion/clean

- data consistency: should we refuse an input from the user because the data in the DB says so?
  - example: missed connection between two flights
  - example: truck loading at a warehouse
  -> should we refuse a command because the data state is not synchronized with real life ?

- Pareto law: commands and queries
  - pareto law: 20% write, 80% read
  - data representation are not the same
  - usage of SQL views -> query models
  - make it explicit: use CQRS
    - CQS at scale
    - SQL views are good enough as a first step, no need for dedicated models
    - make it explicit in the code

- About IO/side effects: applying effects
  - define effects: change of state in the system, IO, etc
  - aiming for consistency
    - changing state and other IO all at once
      -> implicit decision
      -> can be very complex
    - what happen if something fail?
      -> reason of mecanisms like transactions, locks, etc (from one state to the next)
    - any possibility for compensation?
  - isolate and delay some effects: run new processes

- Explicit decisions: splitting decision making from decision execution
  - decision and execution are two distinct concerns
  - a decision should not fail because of the execution
  - all effects are separated from decision making
    - emiting events
    - execution can be retried
    - executions are more isolated

- don't loose data
  - storing state = losing past (Update is implicit delete)
  - from event-driven to event-sourced: events as a storing strategie and not just a communication
  - event-sourcing constraint:
    - rebuild states from events
    - event versioning and serialisation
    - bad event design is more painfull compared to event-driven

- (about ad-hoc attempts to store previous states/logs without ES ??)

- not a single solution: we can mix these stategies in the same context

## notes

- two levels : DB model and domain model, focus on both
- quid dependency inversion?
