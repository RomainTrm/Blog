---
title: "Reactive Programming"
date: 2025-11-01T14:29:13+01:00
tags: [post, en]
draft: true
---

- Backend developper, but several UI experiences with desktop apps (WPF, UWP) and Web (Knockout, React).  
- Two main patterns used: MVVM & reactive, I want to focus here on reactive
- What I mean by reactive:  
  - observables values you can subscribe to
  - when a value change, you can react in consequence
  - example: A -> B -> C  
        I update A, B is notifed, if it updates itself, then C is notified, etc
  - favor composition : easy to add a new element
  - this can be done with a proper framework or with few callbacks injected into components
- two massive setbacks  
  - forms computing prices (HT, taxes, total amount) with some additional complexity
  - not a single flow of computing, we could change either from HT to total or on the oposite direction
  - in one of the two case, that was needed by users
  - for the second, it would have been probably better to code two versions
- when reactive turns to an impediment
  - flow direction matters
  - when several elements subscribes to each others, we loose flow quickly
  - example: A <-> B <-> C <-> D
        desired flow: left to right  
        I update B, A & C gets notified  
        A should do nothing, C updates itself  
        B gets notified C changed: how do we know if we should react or not?  
          note: you can argue that B is already correct regarding C's value, this is not always true, I single rounding can ruin this assumption (this is a morphism with an abstraction, resulting to some information loss)  
  - when several flow directions are possible, this cascading effect is tricky and a nightmare to debug
- my intuition to handle these cases
  - didn't test it in these scenarios by lack of time (yes, my time was at better use on other topics)
  - get rid of this cascading effect: 
    - can try to store the intent somewhere: 
      - "B is the value getting edited by the user"
      - not convince by that, probably brittle, components must know where they're placed in the flow
      - must known when cascading is over
    - get the intention and compute everything at once  
      - a solution MVU and the Elm architecture
      - radical change of paradigm

<!--more-->

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
