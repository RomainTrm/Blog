---
title: "REFINING SOFTWARE ARCHITECTURES"
date: 2025-03-12T09:00:24+01:00
tags: [post, en]
draft: false
---

To develop software, as developers we have to choose between several architectures. Our choice must be based on various constraints like the type of problem we're trying to solve, but also the load or the level of reliability and resiliency targeted. We also have to consider the available skills in the team.  

In this blog post, I want to iterate through several back-end architectures I've encountered and used during my career. For each of them, I'll be highlighting their limitations and then introducing an improvement. To do so, I'll start from the (in)famous "CRUD app", the assumed simplest architecture, the one that will not give you one of your worst headache (until it does), and I'll finish with some advanced solutions.  

## DEFINING CRUD

First thing first, what does *CRUD* means? This is the acronym for **C**reate, **R**ead, **U**pdate and **D**elete. These are the four basic operations to manipulate data in a database. Sometimes you'll also find a fifth verb which is **S**earch, but to me it falls within the **R**ead concerns.  

So, if we try to define in a strict and naive way a "CRUD application", this is basically a form that manipulates raw data from the database without any other form of business logic. Such solution may fit if our only concern is about storing and accessing the data.

Having an application that just mimic the database schema is so trivial that many off-the-shelf solutions exist. I can think of [Adminer](https://www.adminer.org/) that provides you a web interface, or code generators (like [JHipster](https://www.jhipster.tech/)) that generates REST API on top of the database. These are easy to use solutions that can be implemented in a few hours without any specific skills.

```goat
.-------------------.                                              
| Application layer |
'- - - - - - - - - -' 
| Data access layer |
'--------+----------'
```

Even if we can think of some valid use cases, those are rare. Let's face it! No one is paying a team of developers to only build a *CRUD* application, this is way too trivial to justify such a financial expense. There is always at least some validations/business rules on top of it.

## MAKING USEFUL SOFTWARE: ADDING LOGIC

If I try to redefine our "CRUD application" with a broader definition: it's an application with a central data model and some business rules upon it. Now we have some reasons to hire a developer!

Basically, we want to add an intermediate layer between our form and the database, this layer will handle the business logic. This is the emergence of a layered architecture.

```goat
.-------------------.                                                
| Application layer |
'--------+----------' 
         |
         v
.-------------------.
|   Domain layer    |
'--------+----------' 
         |
         v
.-------------------. 
| Data access layer |
'-------------------'

---> Depends on
```

This architecture allows some new capabilities.  

First, we now have a dedicated space in our code where we can develop business logic. The most basic way to use it is to receive input from the *Application layer*, load some data from the *Data access layer*, then make a decision and update the data. Here's a pseudo-code example:  

```csharp
void domainLayerFunction(id, value) {
  var data = dataLayer.load(id)
  // apply some logic based on data and value
  if (value % 2 == 0 && data.value % 2 == 1) {
    data.value = value
  }
  dataLayer.save(data)
}
```

Secondly, it reduces the coupling to the data model for the consumer. In the "pure CRUD application" solution, our data model was leaking to the outside world and clients had to conform to this model to manipulate the data. This is an issue because if we decide to change our data model, it will break our consumers. Now, we can expose some behaviors and abstractions and keep our data model internal (like in the previous code example). It can be beneficial for commands because we're now exposing only relevant fields for each use case and not the entire model.

Finally, we can also choose to introduce a *rich model*: until now we were manipulating an *anemic model*. An *anemic model* is a basic POXO (like *Plain Old Java Object* or *Plain Old C# Object*) that only carries the data. This is often the data model stored in the database. We have to use a *service* to apply business rules (mutations) to this POXO. This separation between data and behavior is not an issue by itself, it's even very common in *functional programming*. The problem is that our business logic conform and works with the storage data model that may not fit well with business constraints. With a *rich model*, we're introducing a new model that is designed to fit with the business, that can possibly enforce some rules with strong typing and may regroup data and behaviors in the same place. Yes, it requires some two-direction mapping logic between these two models but this is a fair tradeoff.

## DRIVING BY BUSINESS: GROWING IN COMPLEXITY

Until now, our architecture remained *data-centric*. What I mean here is that is we draw an arrow to represent the data flow, we'll realize that our data model is in the center of our application.

```goat
      workflow
     o         ^                                      
.----|---------|----.                Alternative concentric representation
| App|ication l|yer |                                
'----|---+-----|----'                .----------------------------------.
     |   |     |                    |          Application layer         |
     |   v     |                    |    .--------------------------.    |
.----|---------|----.               |   |        Domain layer        |   |
|   D|main laye|    |               '   |    .------------------.    |   |
'----|---+-----|----'              workflow |  Data access layer |   |   |
     |   |     |                  o---------------------------------------->
     |   v     |                    |   |   |                    |   |   |
.----|---------|----.               |   |    '------------------'    |   |
| Dat| access l|yer |               |    '--------------------------'    |
'----|---+-----|----'                '----------------------------------' 
      '-------'    

---> Depends on       
o--> Workflow                             
```

This was OK, but now our users are asking for new cool features that will introduce more complexity to our software, models will grow and we will have to heavily rely on automated tests to ensure there is no regression. This is where I think this *layered architecture* shows its limits.

### TESTABILITY

Let's address testability first. The layers are already testable but with a huge constraint: the higher is the layer we want to test, the more layers to include in our test. This means to test our *Domain layer*, where sits all our business logic (the main thing to test), we have to always include the *Data access layer* because we're depending on it. That is painful for several reasons: we have to set up data in the database to run a business test, this adds useless complexity and noise to the tests. This is even worse if the *Data access layer* is using an ORM that is mapping foreign keys as objects, in such case we may manipulate a complete object tree. Also, tests relying on the database are slower and more prone to side-effects (concurrent tests on the same model).

The simplest move I see there is called the *"sandwich pattern"*. The idea is, inside the *Domain layer*, to split data access logic from business logic. This way, we'll be able to test our logic without worrying about other concerns. But if we also want to test this data access logic, we fallback to our tests with data set up in the database. If I apply this pattern to our previous pseudo-code example:  

```csharp
// Isolated and easy to test logic
void businessLogic(data, value) {
  if (value % 2 == 0 && data.value % 2 == 1) {
    data.value = value
  }
}

// Function called by the Application layer
void domainLayerFunction(id, value) {
  // Data access logic
  var data = dataLayer.load(id)
  // Business logic
  businessLogic(data, value)
  // Data access logic
  dataLayer.save(data)
}
```

This pattern is called *"sandwich"* because a business logic slice is surrounded by two data access slices, like the ham between two slices of bread. I would now advise reworking this `void businessLogic(data, value)` method to make it pure (and even easier to test because it's deterministic) instead of mutating the `data` input.

```csharp
Data businessLogic(data, value) {
  if (value % 2 == 0 && data.value % 2 == 1) {
    return {...data, value}
  }
  return data
}

void domainLayerFunction(id, value) {
  var data = dataLayer.load(id)
  var updatedData = businessLogic(data, value)
  dataLayer.save(updatedData)
}
```

And *voilà*! We've implemented a new architecture pattern called [*Functional core, Imperative shell*](https://kennethlange.com/functional-core-imperative-shell/). In our previous example, the *functional core* is our `businessLogic` function, it's surrounded by the *imperative shell* defined by the `domainLayerFunction` method.  

### DOMAIN CENTRIC

The *sandwich* trick is great but the architecture still remains *data-centric*. I want to go further!  

In my previous code example, I used an *anemic model* defined by the database schema. If I want to introduce my *rich model*, I must define the mapping inside the *Domain layer*: *Domain layer* defines the *rich model*, and depends on the *Data access layer*. So *Data access layer* is unaware of the *rich model* and then cannot do the mapping.  

```csharp
void domainLayerFunction(id, value) {
  var dbModel = dataLayer.load(id)
  var richModel = mapToRichModel(dbModel)
  var updatedRichModel = richModel.businessLogic(value)
  var updatedDataModel = mapToDataModel(updatedRichModel)
  dataLayer.save(updatedDataModel)
}
```

There's an alternative way to reduce this mapping problem: we can bring out commands on the *Data access layer*. In the following example, it takes the form of a specialized function to update data, `dataLayer.updateValue(id, updatedRichModel.value)`.

```csharp
void domainLayerFunction(id, value) {
  var dbModel = dataLayer.load(id)
  var richModel = mapToRichModel(dbModel)
  var updatedRichModel = richModel.businessLogic(value)
  dataLayer.updateValue(id, updatedRichModel.value)
}
```

If we choose to go this way, this is a very strong signal that we want to drive our *data layer* by the business. In our *Domain layer*, we no longer want to be constraints and even aware of the data model anymore. For now we have to rebuild a valid and complete object/tree in order to persist our business operation (unless we make the commands emerge on the *Data access layer*). So it's time to introduce a new trick: the [*dependency inversion*](https://en.wikipedia.org/wiki/Dependency_inversion_principle) (the **D** in [SOLID](https://en.wikipedia.org/wiki/SOLID)). This is a straightforward technique. Instead of a direct call to a dependency, we're defining a contract (often it's an `interface`) that the dependency must fulfill. Then we got it injected and make the calls through the contract. In our case, we want the *Domain layer* to define the contract and the *Data access layer* to implement it:

```csharp
interface DataAccess {
  RichModel load(id)
  void save(richModel)
}

class DomainService(DataAccess dataAccess) {
  void domainLayerFunction(id, value) {
    var richModel = this.dataAccess.load(id)
    var updatedRichModel = richModel.businessLogic(value)
    this.dataAccess.save(updatedRichModel)
  }
}
```

From the testability point of view, it gets way easier to test the data access logic as we can replace the dependency with any kind of *test double* (*spy*, *stub*, *fake* or whatever pattern you need) to set up our test case. Even if it's not our goal here, it's worth mentioning that this dependency swap is also possible with production implementations, and it allows us to delay some decision-making like "Which database should we use to store our data?".  

You may have noticed we're not using nor mapping the data model anymore. Thanks to this technique, we've reversed the dependency between the *Domain layer* and the *Data access layer*. *Data access* now knows about our *rich model* and can map it from its own model. We finally have a *domain centric* architecture!

```goat
.-------------------.                                
| Application layer |                                             Concentric representation 
'--------+----------'                                                        
         |                                                .---------------------------------------. 
         |Injects Data access layer's implementations    |    Application && data access layers    |
         v                                               |    .-------------------------------.    |
.-------------------.                                    |   | Domain layer (imperative shell) |   |
|   Domain layer    |                                    '   |    .- - - - - - - - - - - -.    |   |
'-------------------'                                   workflow | Pure logic (func. core) |   |   |
         ^                                             o--------------------------------------------->
         |Implements Domain layer's interfaces           |   |   |                         |   |   |
         |                                               |   |    '- - - - - - - - - - - -'    |   |
.--------+----------.                                    |    '-------------------------------'    |
| Data access layer |                                     '---------------------------------------' 
'-------------------'       

---> Depends on       
o--> Workflow                                                           
```

> This structure is one of the recommended approaches highlighted by the paper "[Out of the Tar Pit](https://curtclifton.net/papers/MoseleyMarks06a.pdf)". It states that side-effects should be pushed to the boundaries of the system, the domain should be pure (side-effect free), and to connect those two parts there is an intermediate layer (our service).

Now, we're driving our design by the *Domain*. We can define, code and test our *rich model* without worrying about data persistence. Once done, we can move on and focus on implementing dependencies and their specific constraints.

This is the core concept behind a whole family of architectures: [*hexagonal architecture*](https://en.wikipedia.org/wiki/Hexagonal_architecture_(software)) (aka *ports and adapters*), *onion architecture* and *clean architecture*. I will not take the time here to explain the differences between them, mostly because I tend to see them as [bikeshedding](https://en.wiktionary.org/wiki/bikeshedding).

## DATA CONSISTENCY: SHOULD WE REFUSE INPUTS?

I'm doing a small interlude here on our architectures refinement to talk about data consistency. On the first versions we've explored, the *data model* was central and therefore was carrying a huge responsibility on data consistency. This takes the form of several database mechanics like *foreign keys* and constraints like `NOT NULL` or `UNIQUE`. Now, we've got a strong *domain model* responsible for business rules, we may consider more supple rules on the database side. Here's my point: should we refuse a command from the user because our *data model* says the operation is inconsistent? This question may seem weird, but sometimes there are good reasons to accept an input and assume our system is out of sync with reality. Here are two use cases:  

A few years ago, I was on a trip and I had few troubles on a connection between two flights. That day, the weather was bad, a lot of clouds and dense fog, which resulted in many delayed flights. My first plane landed late and we were rushing to catch our second flight. After a long run through the airport, we managed to arrive at the gate right on time, but our boarding passes were refused by the system, we didn't get onboard. Indeed the system had determined before the onboarding that we couldn't get there on time, so it automatically registered us in a flight later in the day. I don't know if the system reused our seats for other people, but if it didn't, this is a case where the system was out of sync with reality and have blocked users. Yet, our boarding passes could have been a good proof that we were there, ready to get on the plane.

Now let's imagine we're working on a warehouse. Items are brought in, moved, stored on shelves and then loaded in trucks for delivery. To track items, each of them is marked with unique codes (barcodes, QR codes) that are normally scanned at each step. When loading a truck, we're scanning items until the system detect an inconsistency on one of them because it was supposed to be on a shelf. Do you think the system should prevent us from loading it into the truck? The barcode gives us a high confidence that this is the correct item we're trying to load.

The way to solve these cases are businesses, not technical decisions. So we should be careful not to build systems that enforce this type of consistency without asking domain experts first.

## DISTINCT USE CASES: COMMANDS AND QUERIES

Our previous architecture is already pretty good, I think it's even becoming a standard in any team this some good crafting skills, but let's refine it more.  

In most software, reading and writing are not using the same representations of the data. The information displayed to the user is often an aggregation of entities. On the other side, the application of a command is generally more focus on individual entities, and can be easily abstracted with dedicated types containing only revelant values to execute the command. This is an issue as our model is torn by contradictory constraints, getting a model that satisfies both readings and writing will get more and more difficult.  

Also, you may have heard about the [Pareto principle](https://en.wikipedia.org/wiki/Pareto_principle): the famous "80/20 rule". This is not a rule specific to software, but in the case of management software or e-commerce website, one way to formulate this principle is the read/write ratio: in such software, most accesses are for reading and the writing appears to be less frequent. This means, if we want to improve performances, we can easily target 80% of the use cases just by focusing on readings.  

To address these distinct representation needs and unbalanced load, we can try to make two models emerge, one specialized for reads and the other for writes. In OOP, there's a pattern called *CQS* ([*Command-Query Separation*](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation)). The core principle is simple: a method on an object is either a *Command* or a *Query*. A *Command* produce a side-effect and is not supposed to return any value, on the opposite, a *Query* is used to get a value and must not produce any side-effect.  

```csharp
// CQS example
class Counter() {
  private int value = 0

  // Command: produces a side effect, doesn't return any value
  void Increment() {
    this.value = this.value + 1
  }

  // Query: return a value, getting the value doesn't affect the object's state
  void Get() {
    return this.value
  }
}
```

This pattern has been scaled to the architecture level, this is called *CQRS* ([*Command and Query Responsibility Segregation*](https://en.wikipedia.org/wiki/Command_Query_Responsibility_Segregation)). We now have a model dedicated to *Commands* that is responsible for business decisions-making and one model (or more, if justified by the variety of data representations required) dedicated to *Queries* which only returns data in formats useful for consumers (aka *readmodels*).  

```goat
       .-------------------------.                      
       |    Application layer    |          
       '----+-----------------+--'          
.- - - - - -|- - - - - - - - -|- - - - -.
| Commands  v       | Queries |         |
     .------------.           |         
|    |   Domain   | |         |         |
     '------------'           |         
|           ^       |         |         | 
'- - - - - -|- - - - - - - - -v- - - - -'
       .----+-----------------+--.
       |    Data access layer    |
       '-------------------------'

---> Depends on   
```

With *CQRS*, our data model may tend to become more specialized for writing as it becomes the *source of truth*. To fulfill reads requirements, we have several solutions: use *SQL views*, write some code to map the *readmodel* from the write model at runtime, or create and fill dedicated *SQL tables*. With all these solutions, this read/write separation becomes explicit in the code and we can evolve one part with a minimum impact on the other.

## EXECUTING SIDE-EFFECTS: CONSISTENCY AS A TARGET

Now we have isolated our business logic and made a clean separation between reads and writes, we should focus on what makes software useful: side-effects.  

So far, I've presented side-effects as something bad that must be pushed as far as we can from our domain, and there is good reason for that: by definition it's not deterministic, this means for the same input we will not always get the same output. Sometimes it's a different value, sometimes it can be more annoying like an error or a crash. Side-effects are difficult to test and it is hard to make sure our code is correct. But they're also necessary, they're the state changing in the database, the mail sent to the customer, or any other type of IO operations. Everything valuable produced by our software that is observable from the outside.  

When our software is making changes, we’re seeking consistency, we want to make sure everything works fine, making a correct transition from a state A to a state B. That’s why we’re implicitly making a decision and at the same time applying it in the form of one or many side-effects. But if one of the side-effect fails, we can put ourselves in some inconsistent situation and this can be an issue: we may display wrong information to users, corrupted data may influence future decision-making, generating even more bad data. Making sure our system is always in a consistent state can be very complex or simply unachievable.

So we have to ask ourselves the hard question: What happens if something fails and how do we deal with failures? Yes, when talking about IO operations, failures will happen, never doubt that!

If we focus only on the database, there is a mechanism that can almost guarantee us a safe transition from state A to state B, even on complex queries combining multiple inserts, updates and deletes while complying with schema constraints: a `TRANSACTION`. Nothing new here, I bet you already knew about this and you were already using it in the previous architectures we've explored. Though, the "almost" word was important. Be aware transactions are not perfect and some inconsistencies are still possible, especially if you're using database replication and/or partitioning. Don't try to develop your own solution to protect yourself from such cases, unless you're an expert, there is no chance that you do a better job than your database.  

> On this very specific topic, I recommend to you the book [Designing Data-Intensive Application](https://www.oreilly.com/library/view/designing-data-intensive-applications/9781491903063/) by [Martin Kleppmann](https://bsky.app/profile/martin.kleppmann.com). It does an amazing job describing the internal operations of databases, how they deal with concurrence, replications, etc., and explaining how things can go wrong.  

It's getting even more complicated if our side-effects are spread across several dependencies, like updating our database and at the same time making a call to a third-party API. In such cases we've got no mechanism like `TRANSACTION` to guaranty consistency. In case of a failure, we may need to set up compensation strategies to move back to a new consistent state. In the real world, such scenarios already exist and are well known by domain experts: "Sorry, the item you bought on our site is no longer available, we will proceed to your refund". New scenarios like this will emerge as our software is now part of business processes. We should educate domain experts on them, explaining technical constraints and why these problems may occur. But we should also try to avoid as much failure scenarios as we can because they can undermine the whole business.  

This compensation solution is necessary when we attempt something impossible or invalid from the business's point of view, but sometimes we're facing technical issues: the business intent is correct, but for some technical reasons we just cannot execute the operation right now (for example, the third-party API is unavailable). So, why not to defer this operation and retry it later?

A solution is to isolate these IO operations (other than database calls) in dedicated processes. By doing so, we're making sure it will not block the other side-effects and we now have the ability to retry them independently. A way to do this is to enqueue jobs in whatever suits your needs, then to execute them asynchronously in new processes.  

> Hint: the database can do the job, have a look to [PostgreSQL](https://docs.postgresql.fr/current/sql-listen.html)'s `LISTEN` for example.

```goat
     .-------------------.                      
     | Application layer |          
     '--------+----------'    
              |    
.- - - - - - -|- - - - - - -.      .-+---------.   
| Commands    v             |    .-+---------. |
     .--------------------.    .-+---------. +-'    .-------------.
|    |       Domain       | |  |  Process  +-' ==> | IO operations |
     '--+---------------+-'    '---+-------'        '-------------'
|       ^               ^   |      ^  
'- - - -|- - - - Enqueue|- -'      |Spawn
        |               |          |   
.-------+-----------. .-+----------+--.
| Data access layer | |  Message bus  |
'-------------------' '---------------'

---> Depends on   
===> Execute
```

With such solution, a compensation may not be necessary anymore as our system is becoming *eventual consistent*. If something fails, it can be fixed and retried until it works. By the end of the day, our system will be consistent. Also, as processes are now isolated, we've added a level of protection against cascading failures in our software, making it more resilient. I made a more detailed [post](/posts/using-processes-for-better-resilience/) on this specific topic.  

## EXPLICIT DECISION-MAKING

We're not done yet, I think we can go even further. As I've already mentioned it in the previous section, we're implicitly making and applying decisions at the same time. This has a consequence, we may have made a valid decision but fail to apply it. In such case our decision has been rolled back and discarded. Now I want to change this behavior, making decisions explicit and not lose them if something else fails!  

From now on, I'll refer to *readmodels* updating and jobs enqueuing as *effects*.  

If we go back to our previous *CQRS* architecture, this means we should be able to update and save our write model. If it doesn't fail, then we can process our *effects*. With this, failing to run one of these *effects* doesn't cancel the decision made and should not affect other *effects* (to be applied or already applied).  

> Hint: I would advise updating readmodels then enqueue jobs as job processing may sometimes rely on dedicated readmodels.

One way to implement such a strategy is to aim for an *event-driven* architecture: our *domain* express decisions made in the form of *events*. Our code can look like something like this:  

```csharp
class EventA(values);
class EventB(values);

class Aggregate(state) {
  List<Events> command(value) {
    if (value % 2 == 0 && state.value % 2 == 1) {
      yield return EventA(value)
    }
  }
}

class DomainService(DataAccess dataAccess) {
  void domainCommand(id, value) {
    var aggregate = this.dataAccess.load(id)
    var events = aggregate.command(value)
    this.dataAccess.save(id, events)
  }
}
```

These *events* are then spread across our architecture and applied using *event-handlers*: first into our write model, and if no failure occurred (decision made on an obsolete version or the data violate one of the constraints of the database scheme), then we apply our *effects*.  

```goat
.- - - - - - - - -.- - - - - - - -.                                   
| Commands        | Queries       |                                
   .------------.                                            
|  |   Domain   | |               |                                
   '----o-------'                                               
|       |Emits    |               |
        v                          
|     .-+------.  |               |
    .-+------.  |                    
| .-+------.  +-' |               |
  |  Event  +-'                     
|  '----o---'     |               |
        |'---> (1) ok?  o------------.                                  
|       |         .      \        |   \                           
'- - - -|(1) - - -'- - - -|(2) - -'    |(3)                          
        v                 v            v      
  .-----+-------.   .-----+-----.   .--+----------.
  | Write model |   | Readmodel |   | Message bus |
  '-------------'   '-----------'   '-------------'

o--> Workflow 
(1) Update write model
(2) Update readmodel
(3) Enqueue jobs  
```

An *event-handler* is a straightforward piece of code:  

```csharp
class EventHandler(DbAccess dbAccess) {
  void Handle(EventA @event) {
    // apply effect for EventA
    dbAccess.execute(
      "UPDATE myTable SET value = @value WHERE ...", 
      @event.value)
  }

  void Handle(EventB @event) {
    // apply effect for EventB
  }

  // void Handle...
}
```

If one of our *effects* failed to apply, we now have the ability to retry later by logging the error, the *event* and the *event-handler* that failed. Be careful though **not to leak** sensitive information into the logs.  

To be honest, from all the architectures explored in this blog post, this one is the one with which I have the least experience. I didn't achieve an implementation of the logging/retry mechanisms that really satisfied me, but with more hindsight I can think of two strategies that I believe are viable:  

- store *events* in a dedicated table, each of them as an associated unique id, then log the combination `Event Id * Event Handler * Error`
- store the combination `Event Data * Event Handler * Error` in a dedicated table

In both cases, the information available allows us to understand the failure and retry the *effects*. *Events* remains volatile and are not meant to be stored for long duration, so we need to put a flushing policy, like a dedicated process run daily that removes old *events*/errors. It may be after a week, a month.  

My opinion is that making this split between decision-making and the implementation of the decision is the strongest move to improve your software architecture. It creates a highly resilient system with a supple design thanks decoupled elements (*events* and *handlers*). Though, I must highlight few things here: with *event-driven*, business processes are not always easy to follow and analyze as they're decomposed and spread across a lot places in our system. Also, our software became even more *eventual consistent* as we can fix unexpected errors and retry any *effect* later: this is all about error management. I know with *eventual consistency* it can be tempting to apply *effects* asynchronously but this can have major impacts on the user experience. Think how it can be disturbing if on your favorite e-commerce website an item doesn't appear in your cart after you have chosen to add one.  

> Hint: By default I apply all my *effects* synchronously. In case of long operations, I use a dedicated *readmodel* where I write "processing", then I enqueue a job to run the process in an asynchronous way. This way I'm not blocking the user and she has a feedback that the operation she asked is ongoing.

## DO NOT LOSE DATA

Here's the final refinement I want to suggest. All our previous architectures suffer from the same flaw: our database model remained handled in a *CRUD* fashion. This can be an issue in the write model as we're implicitly losing data every time we're updating a value. Yes, Update is an implicit Delete as we lose the replaced value.  

New features may not be compatible with existing data. Missing data may also prevent us from correcting incorrect values due to bugs or mishandling by users. Analyzing the past is an opportunity to discover trends, doing statistics and improve future business decisions.  

To mitigate this issue, I want to move from an *event-driven CQRS* architecture to the *CQRS/Event-Sourcing* architecture. To do this, we "just" have to store our *events* instead of a state in our write model. By doing so, our model is now in *append only* mode, technical issues put aside, the only way to fail is a conflict. This allows us to easily detect concurrent operations on a same *event stream* (the list of the *events* that were emitted by an *aggregate*), the version of the *event stream* is the number of *events* that composes it: if we try to insert an event with a number that already exists, we're in a conflict.  

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

We also have to rebuild the internal state of our *aggregate* from an *event stream*, this operation can be expressed as a simple `leftFold` ([`Aggregate`](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.aggregate) in .Net, [`redude`](https://developer.mozilla.org/fr/docs/Web/JavaScript/Reference/Global_Objects/Array/reduce) in Javascript).  

```csharp
class Aggregate() {
  private State state

  ctor(List<Events> history) {
    this.state = 
      history.leftFold((state, @event) => {
        @event switch {
          case EventA eventA => // apply event
          case EventB eventB => // apply event
          default => state
        }
      }, initialState)
  }
}
```

However, don't believe CQRS/ES is "just" storing and folding *events* of our initial *event-driven* architecture. In addition to communication, our *events* are now used for storage purposes (this is their main role), this means they're not volatile anymore.  

We'll have to set up a good serialization strategy as well as a versioning/migration policy. These topics are critical and must be considered seriously.  

From the developer point of view, handling *event streams* also require good knowledge about the application: what is a valid *stream*? What *events* are in that stream? In what order? This can be tricky to hold all this information in our mind, especially for complex *aggregates*.  

We also have to put extra care into our *events* design as we'll have to manipulate them over long periods. A bad design can really hurt our development velocity in the long run. I wrote a dedicated [post](/posts/cqrs-es-how-to-achieve-a-good-event-granularity/) where I explain my heuristics to find good *events*.  

Loading several objects in the form of a tree becomes really tricky and it's way better to link aggregates using references (pass an id instead of the object). I would advise you to do this even before considering *event-sourcing*, but with this architecture you don't really have a choice.  

Finally, *event-sourcing* doesn't tell us the whole story: unless we treat business errors as *events*, we will only observe in our *events* the result of successful commands. When we analyze trends in production, this is something that we need to keep in mind.  

*CQRS/Event-Sourcing* is the most advanced architecture pattern I've used in production during my career. I love it because it allows me to write reliable and resilient software, but as I've highlighted it, this is not an easy pattern to use. There are probably some new refinements we could imagine to improve this architecture, but they're out of my current knowledge.  

## CONCLUSION

In this blog post, we've gone through several architectures by adding improvements. As they grow in capabilities, they also grow in complexity.  

Keep in mind that not all applications need the most advanced *CQRS/Event-Sourcing* pattern. Choose the architecture that is good enough for the problems you're trying to solve. Even more, we don't have to write an entire software using a single architecture. In a given context, some parts are well defined with clear life-cycles and therefore can be *event-sourced*. Other parts may be well suited for a simple *CRUD* approach as they have no life-cycle nor business rules. Mixing these patterns isn't an issue at all.  

I'm also aware we've not explored all architectures that exist out-there. For example, I know about the *[actor-model pattern](https://en.wikipedia.org/wiki/Actor_model)* or the *[vertical slice architecture](https://www.jimmybogard.com/vertical-slice-architecture/)* but I chose not to talk about these in this post as I wanted to focus on architectures I've used in a production context.  

Finally, even if our architectures have increased in complexity, it doesn't mean that the cost of the software is growing with it. Sure, it requires more skilled developers and the initial cost of a new feature may be higher, but we have to consider how the code complexity is contained over time, how costly it is to run and maintain the software in production, etc. Increased initial development cost can result in lower maintenance, production and evolution costs. That's why only looking at the development costs to drive a technical choice doesn't make sense to me. This is also the reason why I don’t think these advanced architectures are necessarily more expensive.  

---

## COMMENTS

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
