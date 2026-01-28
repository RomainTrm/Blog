---
title: "Events as a means of communication"
date: 2026-01-22T09:00:00+01:00
tags: [post, en]
draft: true
---

I've recently finished reading [Real-World Event Sourcing](https://pragprog.com/titles/khpes/real-world-event-sourcing/). It was a fun reading, but I found the book unclear on a very specific point and I want to express my thoughts on it in this blog post. I may have misinterpreted the author's explanations but this doesn't invalidate the following reflection.

## How to define an *event stream*?

As the book is about *Event Sourcing* and *CQRS* (*Command Query Responsibility Segregation*), *events* are naturally the central building block: they're used to store states and trigger effects in the system.

Two building blocks are also introduced:  

- *Injectors* that receives external events and can choose to inject them into the *event stream*.  
- *Notifiers* that react to internal events and choose to broadcast them or trigger effects (calls to external systems).

This got me confused: How to define an *event stream*? Who emits the events that compose the *stream*? Do we have *events* from the outside world inside the *event stream*?  

The author never answers explicitly to these questions, but it seems OK for him to inject external *events* in the *stream* and broadcast our own *events* to the outside world.  

> Note: The book introduces [*Cloud-Events*](https://cloudevents.io/) but this is just a convention for cross-application communication, it says nothing about the *event*'s payload.  

## Three use cases

I can identify three ways to use *events* in an application:  

- Data persistence: we store our state by saving the *events* in an append only fashion, this is what I call an *event stream*.
- Choreography: inside our application, we react to *events* to trigger new operations, build data projections, etc.
- External communication: we notify the external world that an important business event occurred.  

Data persistence and choreography are internal uses of our *events*, they are core concepts of *CQRS/ES* architecture. We have full control on how they're produced and consumed.  

However, we do not have such control for external communication: these applications are maintained by their own teams, following their own release frequency. This means that the *events* producer can break listeners at any moment with a new version.  

Broadcasting *events* as a way to communicate with other applications is not a new idea, there is a lot of literature about it like [Enterprise Integration Patterns](https://martinfowler.com/books/eip.html).  

What I want to highlight here, in an *event-sourcing* context, is using the same *events* for internal and external uses is, in my opinion, an anti-pattern in many situations.

## Events as a means of communication

Let’s imagine for a moment we do share our internal *events* with other applications.  

For some reason we need to evolve the internal structure of one of our *events*:  

- If we choose to introduce a new version: Consummers need to know about it and code dedicated logic. If they don't, they will suddenly stop receiving the old *event* and quickly get out of sync.  
- If we choose to migrate the structure: Consummers will continue to receive the *event* but as the contract has changed, this may break their application at runtime.

Note that, in both cases, consumers need to release a new version of their application at the same time as we do if they want to process new *events* properly (in reality, we can mitigate this by relying on queue systems).  

This was only one example, we have many ways to change the composition of our *event stream*: we change the content/structure of an *event*, an *event* gets split, we don't use it anymore, we introduce new steps in the process with new *events*, etc.  

All these changes are driven by the behaviors and the structure of our application. The problem is such changes break other applications. If we want to avoid these, then we cannot evolve our own application anymore.

What we've got here is the same coupling problem as a shared database: if you've already been there, you know this is not a good place to be!

## *Domain-Driven Design* and Strategic Patterns

### Being coupled by the model

What we've just described is defined with *Domain-Driven Design* as the [*Conformist* pattern](http://ddd-practitioners.com/home/glossary/bounded-context/bounded-context-relationship/conformist/). This pattern implies a high level of dependence from the *conformist* to the *provider*'s model. In organizational terms, the *provider* and the *conformist* must be maintained by the same team or by two teams with a high level of collaboration, otherwise this pattern isn't sustainable.

> If this high level of dependence isn't an issue for you but the strong ownership of the model by the *provider* is, you should consider [*Partnership*](https://ddd-practitioners.com/home/glossary/bounded-context/bounded-context-relationship/partnership/) and [*Shared Kernel*](https://ddd-practitioners.com/home/glossary/bounded-context/bounded-context-relationship/shared-kernel/) patterns as alternative solutions.

Another interesting point: *events* are relevant in a given context. When we choose to go for the *conformist* pattern, we accept using the *provider*'s context and semantic. As an example, an `InvoiceIssued` *event* have different meanings depending on the context: it can be an invoice we issue to one of our users, or an invoice a user issue to one of its own customers. Both cases can exist in the application at the same time so we must know which context emitted it.

### Defining dedicated contracts

When trying to reduce friction between applications, we usually define some API that is supposed to:  

- have contracts that remain stable
- have a public communication when some changes are introduced, like publishing a changelog
- give time to consumers to migrate by ensuring retrocompatibility

This way, the provider of a service can evolve without breaking consumer applications, even when it doesn't know some consumers exists! *Events* as means of cross-context communication are a kind of API contracts, so they should respect these rules.

This can be implemented with a combination of patterns:  

- [*Published Langage*](https://ddd-practitioners.com/home/glossary/bounded-context/bounded-context-relationship/published-language/): We define dedicated *events* that act as API contracts and that are only used for external communication.  
It is the *notifiers*' responsibility to build and publish them. This way, we've decoupled our internal model from the public one, and we can now easily see when our clients will be impacted by a change.  

> To avoid confusion, this is probably a good idea to use another name than *events* for these contracts. Personally, I like to refer to them as *messages*.

- [*Anticorruption Layer*](https://ddd-practitioners.com/home/glossary/bounded-context/bounded-context-relationship/anticorruption-layer/): *Injectors* receive outside-world notifications, perform necessary validation and mapping before injecting anything into our system.  
This can be achieved by using *aggregates*: a *message* is converted by the *injector* into a *command* and then processed. The *aggregate* processing this *command* should always return an *event* (as long as the operation isn't idempotent) to store received values. Depending on the validations rules, it can emit additional *events* to trigger processing inside our system through choreography.  
In a way, we've imported external *events*, but we kept control of the structure and they remain isolated in some dedicated *events streams*.

However, I must confess, this solution can necessitate a lot of extra work and introduce some complexity, but it can be really beneficial in the long run.

## Conclusion

*Events* are a natural communication means, but we should distinguish between interal and external uses.  

For internal uses, *events* are primarily persistence contracts, so we have full control of their structure and how they evolve.  
For external uses, *events* act like any API contracts, so they should have their own versioning strategy and we should communicate about it.  

As I tried to highlight it, sharing internal *events* (as a producer or a consumer) with other applications have some strong impacts on application execution, teams organization and communication, deployment constraints. This solution may be a good tradeoff in your context.  
The other solution is to build and publish dedicated *messages* for public communication and to "translate" and validate consumed *messages* before processing them.

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
