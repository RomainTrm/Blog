---
title: "USING PROCESSES FOR BETTER RESILIENCE"
date: 2025-01-08T09:00:00+01:00
tags: [post, en]
draft: false
aliases: ["/posts/2025-01-08/"]
---

In early 2020, I've read the book [Programming Elixir 1.6](https://pragprog.com/titles/elixir16/programming-elixir-1-6/). At that time I had one goal: to have an introduction to the _[actor model](https://en.wikipedia.org/wiki/Actor_model)_ with a language that supports it by design, in this case _Elixir_. I think it was a good read and I achieved my goal, even though I didn't feel able to design a complete system using this _pattern_.  

However, I realized I'm using some _actor model_ concepts for a few years now. In my [previous post](/posts/2024-12-18), I've mentioned types of _effects_ produced in a _CQRS/ES_ system, one of them is triggering new _processes_.  

## _ACTOR MODEL_ IN FEW LINES

Here's my attempt to explain the _actor model_ in a very simple and coarse way:  

In the _actor model_, the main building blocks are _processes_ (aka _actors_). There are different kinds of _actors_, some will execute business logic, some are storing data, others have to monitor their children to spawn new _actors_ when needed. Each one of them has the capability to interact with other _actors_ by sending messages and handling others' messages. This mechanism provides a very high level of isolation of the execution among all _actors_.  

### LET IT CRASH

In a regular code base, we have to put some care to error handling and design mechanisms for recovery. What is worse than an uncaught exception going through the callstack and eventually crashing our application?  

The error handling philosophy is very different when using the _actor model_, it can be formulated as _"let it crash"_.  

Indeed, thanks to process isolation, when an _actor_ crash it cannot break the other _actors_ (at least not in a way as spectacular as an exception). Then, we have to choose how to recover. There are several strategies, including spawning a new _actor_ or choosing to do nothing. When spawning an _actor_, it has the advantage of starting from a known and clean state rather than an unknown and potentially flawed one.  

This is one of the main reasons why systems based on the _actor model_ are often considered to be very stable.

## TRIGGERING NEW PROCESSES

Back to my _CQRS/ES_ architecture and my _effects_!  

When handling an _event_, we may want to do some business operations like issuing an invoice, sending an email, executing a new _command_, etc. There are several issues with such operations, they can be long to execute and/or error-prone. This can affect the overall execution of our software: we don't want it to be blocked by a bottleneck or crash because of something that could be executed asynchronously.  

That's why in my company, for most operations other than a database call, we decided to execute them in some isolated _processes_. To do so, when handling an _event_, instead of running the business operation right away, we enqueue what we call a _job_. Such _job_ is then executed asynchronously and in isolation, and if it fails we just let it crash.  

In case of a crash, the _job_ is flagged as failed with the associated error code or exception attached to it. With this information, our team can monitor the production and analyze errors with less pressure (the website still behave normally for our customers, they will just receive their invoice with some delay). Some errors may be transient (like an unavailable third-party API) and jobs are just retried later, or we may need to patch our software before trying again. As every business operations are isolated in dedicated _jobs_, we can replay them without worrying about running other operations several times.  

## THROTTLING PROCESSES

Enqueuing these _jobs_ gives us a lot of flexibility, we have the choice between several strategies for executing them. Some _jobs_ may require a high priority, some can be parallelized, others may require a sequential execution. To do so, we're using dedicated _channels_ depending on the _jobs_' types.

The principle is straightforward, we're using _job handlers_ to execute our _jobs_. For a sequential execution, we use a single instance, so we can only run one _job_ at a time.  

```goat
.----------------.              
| Event handlers +-.   \                      .---------.      .--------------------.
'-+--------------' +-. -+-> enqueue Jobs --> |  Channel  | --> | Single job handler |
  '-+--------------' | /                      '---------'      '--------------------'
    '----------------'
```

For parallelized execution, we want to dispatch the _jobs_ across several handler instances. The dispatcher logic also provides flexibility, we can choose how many concurrent _jobs_ we want to execute at a time, and how to dispatch them.

```goat
                                                                                    .-------------.
                                                                                  > | Job handler |
                                                                                 /  '-------------'
.----------------.                                                              / 
| Event handlers +-.   \                      .---------.      .------------.  /    .-------------.
'-+--------------' +-. -+-> enqueue Jobs --> |  Channel  | --> | Dispatcher +-+---> | Job handler |
  '-+--------------' | /                      '---------'      '------------'  \    '-------------'
    '----------------'                                                          \  
                                                                                 \  .-------------.
                                                                                  > | Job handler |
                                                                                    '-------------'
```

### CIRCUIT BREAKERS

Theses _channels_ act as buffers, there is some delay between enqueuing and execution time for a _job_. We can choose to increase this delay on purpose to preserve our system.  

This is the core principle behind a _pattern_ called _circuit breaker_. Sometimes, our _jobs_ face a high failure rate for various reasons: a bug, an unavailable API, etc. When detecting such high failure rate, the _circuit breaker_ opens  itself and stop executing _jobs_ (what's the point if we know it will fail anyway?) This has the double benefit of relieving pressure on the system (or third-party API) and giving us time to investigate/fix the issue. Once the issue resolved, we can close the _circuit breaker_ and resume _jobs_ processing. After being open for a while, smart _circuit breakers_ can even probe the system's state by attempting to run a _job_ and decide to close themselves if it doesn't fail.  

> Even if in this blog post my primary focus is not about cross software integration, all these _patterns_ are well described in the book [Enterprise Integration Patterns](https://martinfowler.com/books/eip.html).  

## THE THREATS OF ASYNCHRONOUS PROCESSING

Be aware there are two threats with asynchronous _jobs_ execution.  

First, the _job_ execution can act as a bottleneck in our software: _event handlers_ can enqueue _jobs_ faster than _job handlers_ can process them. This means we'll observe increasing delays before a _job_ is processed. For parallel execution, it can possibly be fixed by adding more computation power (more _job handlers_). For sequential execution, this requires some rework of the code architecture.

Second, when processing _jobs_, especially with parallel calls, we have to make sure we're not overwhelming external dependencies (like APIs) capacities. In this case, these dependencies are the bottleneck of our system. This has two consequences: our system execute in a suboptimal way and we risk breaking the dependency.  

From my understanding, this is because of these threats that _Elixir_ and _Erlang_ developers are not using asynchronous _actor_ communication by default.  

## CONCLUSION

This _pattern_ brings us a lot of stability to our software, it protects it from cascading failures. Thanks to this _"let it crash"_ philosophy, we're not forced to overcomplicate these sections of the code with a defensive coding style.  

In case of failure in these _processes_, the overall impact on the business remains relatively low as it only delays some operations until the issue is solved. This brings more serenity for the development team and for the whole company.  

Finally, having the capability to observe _jobs_ gives us a good view of our production environment. We can see what type of _processes_ are triggered, how many they are, how they're distributed over time, why some of them are failing, etc. This is a key feature for operating software in a production environment.

> Note to myself: maybe I will read this book (release in winter 2025): [Real-World Event Sourcing](https://pragprog.com/titles/khpes/real-world-event-sourcing/)

---

## COMMENTS

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
