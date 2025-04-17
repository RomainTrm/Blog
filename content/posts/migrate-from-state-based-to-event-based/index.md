---
title: "Migrate from State-Based to Event-Based"
date: 2025-04-30T09:00:00+02:00
tags: [post, en]
draft: true
---

Few weeks ago, I've published a [post](/posts/refining-software-architectures) where I described improvements for various architectures, following a logical path from a CRUD architecture to a CQRS/ES implementation.  

Since then, I have participated to [Lyon Craft](https://lyon-craft.fr/) 2025, a local conference focusing on software craftsmanship mindset and practices. For this edition, we had the pleasure to invite [Jérémie Chassaing](https://thinkbeforecoding.com/) for a *event-sourcing* workshop. I had the opportunity to discuss with him and attend his workshop.  

During it, he described his `Decider` pattern: I will not detail the pattern here but I encourage you to read Jérémie's [dedicated post](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider) on this topic. I already knew about it and about CQRS/ES in general so I didn't learn anything new, but it wasn't why I decided to attend this workshop anyway.  

What I was looking for is how Jérémie introduces *event-sourcing* to newcomers and how he refactors a *state-based* codebase to an *event-sourcing* implementation, and I loved what I found! Jérémie's approach reminds me [mine](/posts/refining-software-architectures), except he uses tiny steps I didn't think of. In this post I want to explain them in order to avoid another big-bang refactoring.

## Initial architecture: manipulating states

Let's start with a simple feature implemented with the [*Functional core, Imperative shell*](https://kennethlange.com/functional-core-imperative-shell/) pattern and in a *state-based* fashion.  

```goat
    .---------------------------------------. 
   |    Application && data access layers    |
   |    .-------------------------------.    |
   |   | Domain layer (imperative shell) |   |
   '   |    .- - - - - - - - - - - -.    |   |
  workflow | Pure logic (func. core) |   |   |
 o--------------------------------------------->                               
   |   |   |                         |   |   |
   |   |    '- - - - - - - - - - - -'    |   |
   |    '-------------------------------'    |
    '---------------------------------------' 
         
o--> Workflow  
```

Let's build a code that emulate a printer. Users can do two actions: print pages and reload it with paper. If the printer runs out of pper, then it can't print pages anymore. When the remaining paper in the printer allows a maximum of ten pages to be print, then a flag asking for a reload is raised.

Here's the first implementation of the *functional core* part of the feature:  

```fsharp
type PrinterState = {
    NumberOfPagesRemaining: int
    NeedToBeReloaded: bool
}

type Commands =
    | Print of int
    | Reload

let decide (state: PrinterState) = function
    | Print nbOfPagesToPrint ->
        let nbOfPagesLeft = max 0 (state.NumberOfPagesRemaining - nbOfPagesToPrint)
        { 
            NumberOfPagesRemaining = nbOfPagesLeft
            NeedToBeReloaded = nbOfPagesLeft <= 10
        }
    | Reload -> 
        { 
            NumberOfPagesRemaining = 100
            NeedToBeReloaded = false
        }
```

*Imperative shell* part can be implemented like this:

```fsharp
type InfraDependencies = {
    load: unit -> PrinterState
    save: PrinterState -> unit
}

let execute (deps: InfraDependencies) (command: Commands) =
    let state = deps.load ()
    let newState = command |> decide state
    deps.save newState

let print (deps: InfraDependencies) (nbOfPagesToPrint: int) =
    Print nbOfPagesToPrint 
    |> execute deps

let reload (deps: InfraDependencies) =
    Reload 
    |> execute deps
```

Complete code example available [here](0-state-based.fsx).

In this first version of our feature, the *imperative shell* loads the `PrinterState`, applies a `Command` to it then saves the final state it gets as a result form the *functional core*. On every steps we're manipulating a state, no *event* involved so far.  

I believe it's worth mentionning that with such a model, we don't know how many pages have been printed for a given `Print` command, and if anything has been printed at all.

## Step 1: Events in a black box

Let's proceed to our first move to introduce *events*. The idea here is to modify our *aggregate* (inside the *functional core*) to use *events* in the way that keeps the *imperative shell* unaware of the changes. So our commands have to keep consuming and returning `PrinterState`.  

One possible solution for our *events*:

```fsharp
type Events =
    | PagesPrinted of int
    | LowPaperReserveRaised
    | Reloaded
```

Now we have to introduce a new function that apply an `Events` to a `PrinterState`:

```fsharp
let evolve (state: PrinterState) = function
    | PagesPrinted nbOfPagesToPrint -> 
        let nbOfPagesLeft = state.NumberOfPagesRemaining - nbOfPagesToPrint
        { state with NumberOfPagesRemaining = nbOfPagesLeft }
    | LowPaperReserveRaised -> { state with NeedToBeReloaded = true }
    | Reloaded -> { NumberOfPagesRemaining = 100; NeedToBeReloaded = false }
```

And finally, we update our `decide` function. Depending of the command and the current state of our printer, we may decide to return zero, one or two *events*. It forces us to manipulate an `Events list`. Then we immediatly apply them to our current `state` by folding:

```fsharp
let decide (state: PrinterState) = function
    | Print nbOfPagesToPrint ->
        [
            let nbOfPagesPrinted = min state.NumberOfPagesRemaining nbOfPagesToPrint
            if nbOfPagesPrinted <> 0
            then PagesPrinted nbOfPagesPrinted
            
            let nbOfPagesLeft = state.NumberOfPagesRemaining - nbOfPagesPrinted
            if not state.NeedToBeReloaded && nbOfPagesLeft <= 10
            then LowPaperReserveRaised
        ]
        |> List.fold evolve state
    | Reload -> 
        [
            if state.NumberOfPagesRemaining <> 100
            then Reloaded
        ]
        |> List.fold evolve state
```

I'm fully aware that this code implementation already contains some early optimisations: conditions on `PagesPrinted` and `Reloaded` events are not mandatory as emitting them or not doesn't change behavior for an external observer. I chose to do it anyway to make future changes easier.

The rest of the code (the *imperative shell*) remains the same, you can check it [here](1-events-in-a-black-box.fsx).

## Step 2: Retrieve events from the *functional core*

Second refactor now: we will retrieve events from our *functional core* and rebuild state into the *imperative shell* before saving it. This way, we change the interface between these two layers but it doesn't affect our dependencies yet.  

I am used to keep the `evolve` function hidden as an internal implementation detail of an *aggregate*, called through the `evolve` function. But as I'm following Jérémie's technique here, we will keep these two functions separated as it will help us for the upcoming refactorings.

The code change here is quite simple: we will move the folding from the *functional core* to the *imperative shell*.

First, we remove the folding from the `decide` function and return an `Events list` instead of a `PrinterState`:

```fsharp
let decide (state: PrinterState) = function
    // Returns Events list instead of PrinterState
    | Print nbOfPagesToPrint ->
        [
            let nbOfPagesPrinted = min state.NumberOfPagesRemaining nbOfPagesToPrint
            if nbOfPagesPrinted <> 0
            then PagesPrinted nbOfPagesPrinted
            
            let nbOfPagesLeft = state.NumberOfPagesRemaining - nbOfPagesPrinted
            if not state.NeedToBeReloaded && nbOfPagesLeft <= 10
            then LowPaperReserveRaised
        ]
    | Reload -> 
        [
            if state.NumberOfPagesRemaining <> 100
            then Reloaded
        ]
```

Then we apply theses *events* with the `evolve` function to the state into the `execute` function:

```fsharp
let execute (deps: InfraDependencies) (command: Commands) =
    let state = deps.load ()
    // Retrive events
    let events = command |> decide state
    // Apply events to the previous state
    let newState = events |> List.fold evolve state 
    deps.save newState
```

We don't have to apply any change to our `InfraDependencies` type, meaning the applicative/infrastructure layer remains unaware of this change. Complete code example available [here](2-retrieve-events-from-funcitonal-core.fsx).

## Step 3: Saving events

From now on, we will not have to modify our *functional core* anymore, all requirements for an *event-sourced* implementation are already in place there. All the upcomming changes will impact the *imperative shell* and the application/infrastructure layer.

So we change our dependencies to save an `Events list` with our `PrinteState`:  

```fsharp
type InfraDependencies = {
    load: unit -> PrinterState
    // Gets the new state and new events
    save: PrinterState * Events list -> unit 
}
```

Then we update the code to match this new signature:  

```fsharp
let execute (deps: InfraDependencies) (command: Commands) =
    let state = deps.load ()
    let events = command |> decide state
    let newState = events |> List.fold evolve state 
    // Pass events
    deps.save (newState, events)
```

Complete code example available [here](3-saving-events.fsx).

This refactoring looks simple, but keep in mind that we also have to handle events serialization in the infrastructure layer that doesn't appear in my code example. This can be a non-trivial topic and we have to come out with a proper strategy.  

Note that now, as our *events* are exposed outside of the domain layer, we can know if something happened or not in our system: if no *event* is returned, then we have a proof that nothing happened.

## Step 4: Loading events

- load state, state as output, save sate
- load state, events in black box (state as output), save sate
- load state, events as output, save state
- load state, events as output, save state and events
  - note: now we have a proof nothing happened
- load events, events as output, save state and events
- load events, events as output, save events
- conclusion
  - progressive
  - each step is a valid solution to go to production with
  - choose what you're confortable with
  - Jeremie's workshop is more detailed and I recommand you to attend it if you have the opportunity
- TODO: add link with edit to "refining software architecture"

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
