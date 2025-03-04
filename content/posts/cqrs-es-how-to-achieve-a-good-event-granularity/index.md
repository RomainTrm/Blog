---
title: "CQRS/ES: HOW TO ACHIEVE A GOOD EVENT GRANULARITY?"
date: 2024-12-18T09:00:00+01:00
tags: [post, en]
draft: false
aliases: ["/posts/2024-12-18/"]
---

If you've already developed a software using the _event sourcing_ pattern, you've probably faced difficulty: How-to design good _events_? What is a good _event_ granularity?

Indeed it's difficult to produce good _events_ that will not harm our design. As a seasoned developer with _event sourcing_, I'm still struggling with this, even if I've developed several heuristics over time.  

In this blog post, I will share with you these heuristics. But keep in mind this is not some kind of best practices. Best practices are useful for contexts where we can apply a method without any (major) form of adaptation, there's nothing that simple when developing a custom software for business. The following heuristics are rather a way to ask ourselves good questions and drive our thinking.

## EVENTS OWNERSHIP

I've mentioned _event sourcing_, but what I'm thinking of is a _CQRS/ES_ implementation. _Event sourcing_ is about persistence, but associated with _CQRS_, _events_ have a double responsibility:

1. _Events_ represent the decisions (and the associated information) we want to store.
2. _Events_ are a communication contract for elements inside a _CQRS/ES_ context. Yes, _CQRS/ES_ is also _event-driven_.  

An _event_ is commonly associated with an _aggregate_. This is true, it is the _aggregate_'s responsibility to emit these events. But an _event_ belongs to a _business context_ because other actors will consume it to produce _effects_.

> Tip: An _event_ is an implementation detail in a given _context_, don't use them as a contract for cross-context communication, use dedicated messages instead.

## DEFINING EFFECTS

In a _CQRS/ES_ implementation, we are emitting _events_ to express decisions we made. By applying these decisions, we're producing _effects_. I can think of three categories of _effects_:  

1. update the state of the emitter _aggregate_
2. update dedicated system projections (aka _readmodels_)
3. trigger new processes (send emails, generate files, apply new _commands_, etc.)

Each _effect_ has his own data requirements, sometimes we can reuse an _event_ for several _effects_, sometimes we'll need dedicated _events_.

One of the first things to do is identifying the _effects_ we want to produce.

> Tip: We're storing information in the _aggregate_'s state for future decision-making. Sometimes we can replace a data provided by a _command_ with a data stored in an _event_ of the _aggregate_'s history. Anticipating future _effects_ (and associated information) can highly simplify our software.

## AUTONOMOUS EVENTS

Good _events_ are autonomous _events_. This means they carry all the data they need to apply an _effect_ (ideally). In other words, when applying an _event_ we're not supposed to compute any data, we should only do some mapping and aggregation logic. There is a good reason for this. By storing _events_, we're storing decisions over time, these decisions are associated with _business rules_. If we're missing some data in the _event_, applying a _business rules_ to fill the gap is a potential issue because we're applying the actual version of this _rule_, not the one that was applied then the _event_ was emitted.

Here's an example: we're running a business and selling a service to our customers. When issuing an invoice, we chose to only store the amount without taxes. At first glance, this looks like a harmless design decision. But when it's time to pay taxes, we'll need to compute how much we've perceived from our customers. Problem: the tax rate to apply have possibly changed over time, maybe only for customers of a specific region, etc. This can get complicated very quickly. That’s why we want to include the rate and amount of taxes in our _event_.  

I believe autonomous _events_ can be achieved for my first two categories of _effects_ (_aggregate_'s state and _readmodels_), but it's not always possible for the third one (triggering new processes). Sometimes, the _effect_ we're triggering need information from a larger scope than the scope controlled by the _aggregate_. In these cases, we read information from dedicated _readmodels_.

> Tip: It's OK to repeat the same information in several _events_.

## BUSINESS INTENTS

So far, we've talked about _effects_ on the system. An _effect_ is how the state of our system changes, it's a side effect. But observing an _effect_ does not tell us why it occurs, for that we must capture _intents_. Indeed, there are several reasons for our system to send an email...  

An _effect_ is _what_ happened, the _intent_ is _why_ it happened. Our _events_ are driving the _effects_ but they're also responsible for describing the _intents_ associated with them. The _why_ carries a lot of value because it provides inputs to business people, it's a good way to support future business decisions beyond the software.  

> Reminder: Perhaps you've already heard about the DRY ([Don't Repeat Yourself](/posts/2021-05-26)) principle. It's quite often misunderstood because this notion of repetition is not about the code, it's about business behaviors. You can have some duplicated code, but if they're called for different business reasons, it is probably a good thing to keep duplication because they may evolve differently.  

So we have to ask ourselves _why_ we want to produce an _effect_. For the same _effect_ with the same _intent_, we want to produce the same _event_. For the same _effect_ with distinct _intents_, we want to produce different _events_. Different _events_ are important for future code updates, this will allow us to easily modify an _effect_ for a given _intent_ without impacting the others.  

There are two ways to encode an _intent_ in an _event_: in the type or in a property. Both options have their own tradeoffs for future code updates. Choosing a property is making the assumption that _effects_ will evolve in a very similar way for all _intents_, choosing a dedicated type results in some code duplication but simplify code updates when _effects_ tend to differ over time. Personally, I tend to choose type encoding by default.  

> Tip: Multiple _commands_ can raise the same _event_ as long as they share the same _intent_.

## SNAPSHOTS AND LIFECYCLES

One thing I remember from when I was learning about _CQRS/ES_: _snapshots_ were a recurring topic.  

After several years, in all the code bases I've worked with, I have never used _snapshots_ and I have never encountered any use case that could justify using it. Today, I even tend to think _snapshots_ can be considered as _code smell_ for most use cases.  

When designing an _aggregate_, we want to control its lifecycle, how it starts, how and when it ends. With _event sourcing_, this means we must limit _event stream_ length. Using _snapshots_ potentially means our _stream_ is not bounded because we've not defined a clear end to the _aggregate_'s lifecycle.

> To me, an _event stream_ of 10 or 30 _events_ looks normal depending on the _aggregate_'s complexity, a 150-_events stream_ is a big one but it doesn't require a _snapshot_ yet. There's no hard limit, just be aware of the scales in your own systems.

To bound an _event stream_, we have to define an end we will always encounter:  

### BUSINESS RELATED LIMIT

Sometimes this emerges very naturally, for example an event `Issued` for an `Invoice`. Sometimes we have to define more arbitrary limits.

One of my customers was running a business with several agencies in France for the purchase and sale of valuables. We had one software to track these valuables as they needed to be moved several times for expertise before being sold again. To avoid long _event stream_ for these objects, the solution was to end the _aggregate_'s lifecycle every time they leave a place (sold or transferred) and initiating a new _aggregate_ when entering a new place (bought or transferred) with the previous valuable identity as a property.  

### TIME RELATED LIMIT

Think how we can design a bank account with all its associated operations. A single _aggregate_ isn't suitable because it can last for a very long time, maybe even longer than the lifetime of its owner. In this use case, we can place time-related limits, for example a month duration lifespan. At the beginning of each month, we're initiating a new _aggregate_ with the last known balance of the bank account.

## CONCLUSION

To summarize, _events_ are the central building block of a _CQRS/ES_ implementation, they're used for data storage and for inner communication. When designing events, we need an overall view of our system to define _effects_ and _intents_. I think this is the main reason why _CQRS/ES_ is a complex pattern to use. We also have to carefully think how long an _aggregate_ will be used.  

I hope you found these heuristics useful, it took me some time to structure my thoughts in order to formulate them.  

---

## COMMENTS

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
