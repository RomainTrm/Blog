---
title: "Migrate from State-Based to Event-Based"
date: 2025-04-30T09:00:00+02:00
tags: [post, en]
draft: true
---

Few weeks ago, I've published a [post](/posts/refining-software-architectures) where I described improvements for various architectures, following a logical path from a CRUD architecture to a CQRS/ES implementation.  

Since then, I have participated to [Lyon Craft](https://lyon-craft.fr/) 2025, a local conference focusing on software craftsmanship mindset and practices. For this edition, we had the pleasure to invite [Jérémie Chassaing](https://thinkbeforecoding.com/) for a CQRS/ES workshop. I had the opportunity to discuss with him and attend his workshop.  

During it, he described his `Decider` pattern: I will not detail the pattern here but I encourage you to read Jérémie's [dedicated post](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider) on this topic. I already knew about it and about CQRS/ES in general so I didn't learn anything new, but it wasn't why I decided to attend this workshop.  

What I was looking for is how Jérémie introduces *event-sourcing* to newcomers and how he refactors a *state-based* codebase to an *event-sourcing* implementation, and I loved what I found! Jérémie's approach reminds me [mine](/posts/refining-software-architectures), except he uses tiny steps I didn't think of. In this post I want to explain them in order to avoid another big-bang refactoring.

- load state, state as output, save sate
- load state, events in black box (state as output), save sate
- load state, events as output, save state
- load state, events as output, save state and events
- load events, events as output, save state and events
- load events, events as output, save events
- conclusion 
    - progressive
    - each step is a valid solution to go to production with
    - choose what you're confortable with

---

## COMMENTS

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
