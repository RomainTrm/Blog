---
title: "Reaching the limits of Reactive Programming"
date: 2025-11-12T09:00:00+01:00
tags: [post, en]
draft: true
---
<!-- 
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
      - radical change of paradigm -->

In my career, I had (and still have) many opportunities to develop UIs on desktop and web applications using various technologies. With these experiences I've been using two main patterns: *MVVM* ([Model-View-ViewModel](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel)) and a *Reactive* approach.  

In this post, I want to talk about the *Reactive* approach and especially about two features where it has become a burden to me.

## What I mean by *Reactive*

I am not talking about a specific framework here. By *Reactive* I think of a specific way to code and organize behaviors for a UI.  

Imagine a user interface with several fields. Each of them manages one value, and other fields can subscribe to be notified when the value has changed. When notified, we can make decisions, change our value, and by doing so, notify our subscribers. Here's an example:  

We have three fields `A`, `B` and `C`. `B` listen to `A` updates and `C` listen to `B`.  

```goat
 .-.  listen to  .-.  listen to  .-.                              
| A +<----------+ B +<----------+ C |
 '-'             '-'             '-' 

           Flow of updates  --->
```

When `A` is updated, `B` is notified. If `B` decides to update itself, then `C` will be notified.  

I can see various benefits to this strategy:  

- Each field is responsible of its own value and knows how to compute it. This gives small pieces of code that are easy to grasp and maintain.
- It favors composition, it is easy to introduce a new field in the flow.
- Some dedicated frameworks exist, but this can be easily achieved by yourself using some injected callbacks.

## Two massive setbacks

Though, twice in my career, I have encountered features for which, using *Reactive* turned out to be a real pain. Both were computing prices with associated taxes.

In the first setback, the form was used by the company's sellers to negotiate with customers. The seller could use a price with or without taxes as a base of negotiations, choose to round the final price, etc. For example, they could agree on a price of 19,2&nbsp;€ excluding taxes, add 20&nbsp;% of taxes and then choose to round the total amount of 23,04&nbsp;€ to 23&nbsp;€. The real feature was more complex with some additional fields but you get the idea.  

The second setback was in reality two features for which we chose to reuse some components. It was a form used by users to declare incomes. Initially, users had to enter the amount without taxes and the associated tax percentage, the form then computes the final amount automatically. Later, we've added a second use case: sometimes we already knew the final amount as it was imported from some bank operations. Users had to fill the associated taxes for the form to compute correct amount without taxes. Here again, this is the general idea, the form was more complex than that.

These may seem like a good use case for a *Reactive* approach. Indeed, users change a value in a way that impacts other values displayed on the screen, so they must be recalculated.  

## When *Reactive* turns to an impediment

You may have already spotted something in common with these cases: the computation flow is multidirectional.  

```goat
 .-.  listen to  .-.  listen to  .-.                              
| A +<--------->+ B +<--------->+ C |
 '-'             '-'             '-' 

     <---  Flow of updates  --->
```

This could work smoothly on one condition: all transformations must be *isomorphisms*.  

> ### What is an *isomorphism*?
>
> If you're not familiar with this concept, this term comes from the [*Category Theory*](https://en.wikipedia.org/wiki/Category_theory).  
>
> A *morphism* is a transformation: `f: a -> b`.  
> We have the *identity morphism*: `id: a -> a`, this returns the value it gets as input.  
> We can compose *morphisms*: `f.id = f` (left identity) and `id.f = f` (right identity)  
>
> An *isomorphism* is a special case where, for a transformation `f: a -> b`, we have another opposite transformation `g: b -> a` that satisfies `f.g = id` and `g.f = id`.  
> These are transformations without loss of information (no abstraction).

In such case, if we choose to update the value `B`, then `A` and `C` are notified and updated. This notify back `B`, but as the computed value is equal to the actual value, nothing happens and the update is complete.  

Computing amounts with and without tax rates are, from a mathematical point of view, multiplications and divisions. So yes, if we ignore the divide and the multiply by zero, they are *isomorphims*, except when we introduce some roundings in the operations.

In such cases, when the field `B` is notified back, the computed value does not always match with its current value, so it updates itself. This leads to cascading updates until we find a result that does not suffer from rounding errors. Such cascading effect turns out to be a nightmare to debug very quickly, and the worst part of it: the initial user-defined value may have been lost in the process.  

You may suggest here to do all the calculation first, then do the rounding of the final values for display. Unfortunately, this is not a solution as it can break the following equalities:  

- `amount without taxes * taxe rate = taxes`
- `amount without taxes + taxes = amount with taxes`

## My intuition to solve these cases

I can think of another approach to deal with this kind of feature requiring a multidirectional calculation flow. However, I must say that I did not have the opportunity to test it on these specific cases because my time was of better use on other subjects, so **I may be wrong** here.

I believe these cases would be way easier to handle if we replaced the *Reactive* approach with a *MVU* architecture (Model-View-Update, also known as the [Elm architecture](https://guide.elm-lang.org/architecture/)). Yes, instead of fully embracing the *Reactive* approach with some elegant workaround (that I wanted to find), I totally give it up.  

In a very simple way, the *MVU* architecture is a loop between three elements:  

```goat
+-------+                 +-------+                               
| Model +---- render ---->+ View  |
+---+---+      view       +---+---+
    ^                         |
  update    +--------+      send
  model ----+ Update +<--- commands
            +--------+

```

> Here's a good [blog post](https://thomasbandt.com/model-view-update) I found if you want more content about *MVU*.

To solve my initial problem, we have to look at the *Update* part.  

This is a function `(model, command) => model` that returns a new `model` by applying a `command` to the current state. I can see several benefits of this function:  

- We can express clear intents with dedicated `commands`.
- By knowing the intent, we can do the whole computation all at once without any cascading effect.
- With a dedicated piece of code per `command`, we handle only one use case at the time, that makes the code simpler (even if it can mean more code).
- Bonus: we can declare some user-defined values as impossible due to rounding.

## Conclusion

*Reactive* is a good way to organize the code, but as every solution, it comes with some tradeoffs. In this post, we saw that the computation flow must be either unidirectional or must not suffer from loss of data over transformations. If we don't respect at least one of these conditions, then *Reactive* is probably not the best choice for our feature. In such cases, turning to a strategy where every intent is handled by a dedicated piece of code seems to be a better tradeoff.

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
