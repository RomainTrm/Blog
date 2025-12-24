---
title: "About Events"
date: 2025-12-24T09:00:00+01:00
tags: [post, en]
draft: true
---

<!--
- finished reading
  - fun reading 
  - spotted something quickly
- in the book:
  - events are everywhere, central building block (state, persistance, choregraphy with other building blocks)
  - no clear distinction between my events and outside-world events (cf injectors & notifiers)
  - to me, this is an issue, events as proposed in the book is an anti-pattern
- events for my app and for communicating with others are not the same
  - internal: persistance & choregraphy (event sourcing)
  - external: communication (aka API contracts)
- about using external events/sharing my events: 
  - I do migrate my events -> this break contracts
  - I use versioning : new versions means update in every client or they will no receive new values
    - also means we need to support new events (in our domain logic) every time a provider change some events
  - Changes in my events may be for internal logic only (outside world doesn't need to known)
  - We need to decorelate internal and external needs
- DDD contexts & strategic patterns:
  - context matters:
    - event's semantic is linked to a context
  - all contexts are not linked the same way
    - shared kernel -> shared by two contexts, very high level of colaboration
    - conformist -> one context drives the other, high level of colaboration
    - open host service with published language -> dedicated contracts, providers may not now about consumers
- how I handle these cases:  
  - events doesn't leak outside of a context (even with modular monolith)
  - send to we outside world: notifiers produces dedicated messages or call external apis instead of sending our events 
  - receive from the outside world: injectors act as ACL and produce internal events
    - you may want to produces your own events without the aggregate
    - we use aggregates to validate inputs before injecting them in the system 
-->

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
