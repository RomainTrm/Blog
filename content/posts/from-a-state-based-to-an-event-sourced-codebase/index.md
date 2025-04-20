---
title: "From a state-based to an event-sourced codebase"
date: 2025-04-23T09:00:00+02:00
tags: [post, en]
draft: true
---

A few weeks ago, I've published a [post](/posts/refining-software-architectures) where I described improvements for various architectures, following a logical path from a CRUD architecture to a CQRS/ES implementation.  

Since then, I participated in [Lyon Craft](https://lyon-craft.fr/) 2025, a local conference focusing on the software craftsmanship mindset and practices. For this edition, we had the pleasure to invite [Jérémie Chassaing](https://thinkbeforecoding.com/) for an *event-sourcing* workshop. I had the opportunity to discuss with him and attend his workshop.  

During this, he described his `Decider` pattern: I will not detail the pattern here but I encourage you to read Jérémie's [dedicated post](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider) on this topic. I already knew about it and about *event-sourcing* in general so I didn't learn anything new, but it wasn't why I decided to attend this workshop anyway.  

What I was looking for is how Jérémie introduces *event-sourcing* to newcomers and how he refactors a *state-based* codebase to an *event-sourcing* implementation, and I loved what I found! Jérémie's approach reminds me [mine](/posts/refining-software-architectures) except he uses tiny steps I didn't think of. In this post I want to explain these in order to avoid another Big Bang refactoring.

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

For this blog post, we will use a codebase that emulates a printer. Users can do two actions: print pages and reload it with paper. If the printer runs out of paper, then it can't print pages anymore. When the remaining paper in the printer allows a maximum of ten pages to be printed, then a flag is raised, asking for a refill.

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
    Load: unit -> PrinterState
    Save: PrinterState -> unit
}

let execute (deps: InfraDependencies) (command: Commands) =
    let state = deps.Load ()
    let newState = command |> decide state
    deps.Save newState

let print (deps: InfraDependencies) (nbOfPagesToPrint: int) =
    Print nbOfPagesToPrint 
    |> execute deps

let reload (deps: InfraDependencies) =
    Reload 
    |> execute deps
```

The complete code example is available [here](0-state-based.fsx).

In this first version of our feature, the *imperative shell* loads the `PrinterState`, applies a `Command` to it, then saves the final *state* it gets as a result from the *functional core*. On every step we're manipulating a *state*, no *event* involved so far.  

I believe it's worth mentioning that with such a model, we don't know how many pages have been printed for a given `Print` command, and if anything has been printed at all.

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

And finally, we update our `decide` function. Depending of the command and the current *state* of our printer, we may decide to return zero, one or two *events*. This forces us to manipulate an `Events list`. Then we immediatly apply them to our current `state` by folding:

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

I'm fully aware that this code implementation already contains some early optimizations: conditions on `PagesPrinted` and `Reloaded` events are not mandatory as raising them or not doesn't change behavior for an external observer. I chose to do it anyway to make future changes easier.

The rest of the code (the *imperative shell*) remains the same, you can check it [here](1-events-in-a-black-box.fsx).

## Step 2: Retrieve events from the *functional core*

Second refactor now: we will retrieve events from our *functional core* and rebuild *state* into the *imperative shell* before saving it. This way, we change the interface between these two layers but it doesn't affect our dependencies yet.  

I am used to keeping the `evolve` function hidden as an internal implementation detail of an *aggregate*, called through the `decide` function. But as I'm following Jérémie's technique here, we will keep these two functions separated as it will help us for the upcoming refactoring.

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

Then we apply these *events* with the `evolve` function to the *state* into the `execute` function:

```fsharp
let execute (deps: InfraDependencies) (command: Commands) =
    let state = deps.Load ()
    // Retrive events
    let events = command |> decide state
    // Apply events to the previous state
    let newState = events |> List.fold evolve state 
    deps.Save newState
```

We don't have to apply any change to our `InfraDependencies` type, meaning the applicative/infrastructure layer remains unaware of this change. The complete code example is available [here](2-retrieve-events-from-funcitonal-core.fsx).

## Step 3: Saving events

For this step, we will not have to modify our *functional core*. There is only one missing requirement there for an *event-sourced* implementation that we will introduce in step 4. All the other upcoming changes will impact the *imperative shell* and the application/infrastructure layer.

To save our events, first we must change our dependencies to save an `Events list` with our `PrinterState`:  

```fsharp
type InfraDependencies = {
    Load: unit -> PrinterState
    // Gets the new state and new events
    Save: PrinterState * Events list -> unit 
}
```

Then we update the code to match this new signature:  

```fsharp
let execute (deps: InfraDependencies) (command: Commands) =
    let state = deps.Load ()
    let events = command |> decide state
    let newState = events |> List.fold evolve state 
    // Pass events
    deps.Save (newState, events)
```

The complete code example is available [here](3-saving-events.fsx).

This refactoring looks simple, but keep in mind that for storing our *events*, we also have to handle serialization in the infrastructure layer that doesn't appear in my code example. This can be a non-trivial topic and we have to come up with a proper strategy.  

Note that now, as our *events* are exposed outside of the domain layer, we can know if something happened or not in our system: if no *event* is returned, then we have a proof that no decision has been made.

Also, keeping *states* alongside our newly saved *events* is a very convenient solution: it helps maintain the current model without building new projections (aka *readmodels*). Business people may be used to go check things in the database, even if *events* bring new information, *states* remains more practical to query for them.

## Step 4: Loading events, our first *event-sourced* implementation

Now we can implement an *event-sourced* feature with our next refactoring. To do so we have to load from the infrastructure layer an `Events list` instead of a `PrinterState`. Let's modify the dependencies:  

```fsharp
type InfraDependencies = {
    // Load events
    Load: unit -> Events list
    Save: PrinterState * Events list -> unit 
}
```

And then we try to rebuild the `PrinterState` in our *imperative shell*:  

```fsharp
let execute (deps: InfraDependencies) (command: Commands) =
    // Load printer's history
    let history = deps.Load ()
    // Build printer's state
    let state = history |> List.fold evolve ??????
    let events = command |> decide state
    let newState = events |> List.fold evolve state 
    deps.Save (newState, events)
```

We are runing into an issue here: until now we had a *state* on which to apply our *events*, but now we have only *events*. We are missing an `initialState` for our printer when nothing has happened yet. I usually define it in the *functional core*:

```fsharp
// A new printer is empty and needs to be loaded
let initialState : PrinterState = {
    NumberOfPagesRemaining = 0
    NeedToBeReloaded = true
}
```

And now we can use it to build our *state* by replacing the `??????` with `initialState`:

```fsharp
let state = history |> List.fold evolve initialState
```

The complete code example is available [here](4-loading-events.fsx).

As for step 3, my code example doesn't show the whole story here: I didn't implement the infrastructure layer where we will have to deserialize *events* once loaded from the database.

## Step 5: removing *state* from the infrastructure layer

For this final refactoring, we will remove the `PrinterState` from the infrastructure layer, meaning we will only load and save `Events list`. This is straightforward as we will only remove code. Note that it is possible to achieve this step before the step 4.  

First, let's change our dependencies:  

```fsharp
type InfraDependencies = {
    Load: unit -> Events list
    // Only saves events
    Save: Events list -> unit 
}
```

And finally we update our *imperative shell*:

```fsharp
let execute (deps: InfraDependencies) (command: Commands) =
    let history = deps.Load ()
    let state = history |> List.fold evolve initialState
    let events = command |> decide state
    // Doesn't build new state, only pass new events
    deps.Save events
```

The final implementation is available [here](5-removing-state.fsx).

## Remarks

Keep in mind though that my code example is a simplified version of what a real implementation looks like, especially in the *imperative shell* where I've decided to remove some noise for the seek of the demonstration. Indeed, systems manipulating a single stream of *events* are rare, we usually have to provide an ID for loading and saving. Also, we often provide the version of the stream we used to make our decision, this allows us to detect potential concurrent executions of *command*.

```goat
Initial event stream     Add event: No conflict    Add event: Conflict 
 
  .-----------.            .-----------.             .-----------.        
  |  Event 1  |            |  Event 1  |             |  Event 1  |        
  '-----------'            '-----------'             '-----------'        
  |  Event 2  |            |  Event 2  |             |  Event 2  |
  '-----------'            '-----------'             '-----------'        
  |  Event …  |            |  Event …  |             |  Event …  |        
  '-----------'            '-----------'             '-----------'        
  |  Event N  |            |  Event N  |             |  Event N  |        
  '-----------'            '------+----'------.      '-----------'    .-----------.
                              <---| Event N+1 |      | Event N+1 |<-x-| Event N+1 |
                                  '-----------'      '-----------'    '-----------'

<--- Valid insertion
<-x- Conflicting insertion
```

With these constraints in mind, a more realistic implementation could look like this:

```fsharp
type InfraDependencies = {
    Load: PrinterId -> Events list
    Save: SaveParams -> unit 
}
and SaveParams = {
    PrinterId: PrinterId
    Version: int
    Events: Events list
 }

let execute (deps: InfraDependencies) (printerId: PrinterId) (command: Commands) =
    let history = deps.Load printerId
    let state = history |> List.fold evolve initialState
    let events = command |> decide state
    deps.Save {
        PrinterId = printerId
        Version = List.length history
        Events = events
    }
```

## Conclusion

In this post, we've explored how to gradually move from a *state-based* code base to an *event-sourced* one. Each of these steps is a valid solution that you can choose to go to production with. Just pick the one that matches your needs and you're comfortable with.  

As I've already mentioned it in the introduction, Jérémie goes further in his workshop as he also introduces his `Decider` pattern. If you have an opportunity to participate in this workshop, give it a try because you will learn more from it than with this post. 

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
