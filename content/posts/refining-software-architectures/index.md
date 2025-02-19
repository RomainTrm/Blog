---
title: "REFINING SOFTWARE ARCHITECTURES"
date: 2025-03-05T09:00:24+01:00
tags: [post, en]
draft: true
---

To develop a software, as developers we have to choose between several architectures. Our choice must be based on various constraints like the type of problem we're trying to solve, but also the load or the level of fiability and resiliency targeted. We also have to consider the availlable skills in the team.  

In this blog post, I want to iterate through several back-end architectures I've encountered and used during my carreer. For each of them, I'll be highlighting their limitations and then introducing an improvement. To do so, I'll start from the (in)famous "CRUD app", the assumed simplest architecture, the one that will not give you one of your worst headache (until it does), and I'll finish with some advanced solutions.  

## DEFINING CRUD

First thing first, what does *CRUD* means? This is the acronym for **C**reate, **R**ead, **U**pdate and **D**elete. These are the four basics operations to manipulate data in a database. Sometimes you'll also find a fifth verb which is **S**earch, but to me it falls within the **R**ead concerns.  

So, if we try to define in a strict and naive way a "CRUD application", this is basically a form that manipulate raw data from the database without any other form of business logic. Such solution may fit if our only concern is about storing and accessing the data.

Having an application that just mimic the database schema is so trivial that many off-the-shelf solutions exist. I can think of [Adminer](https://www.adminer.org/) that provides you a web interface, or code generators (like [JHipster](https://www.jhipster.tech/)) that generates REST API on top of the database. These are easy to use solutions that can be implemented in a few hours without any specific skills.

```goat
.-------------------.                                              
| Application layer |
'- - - - - - - - - -' 
| Data access layer |
'--------+----------'
```

Even if we can think of some valid use cases, those are rare. Let's face it! No one is paying a team of developpers to only build a *CRUD* application, this is way too trivial to justify such a financial expense. There is always at least some validations/business rules on top of it.

## MAKING USEFULL SOFTWARE: ADDING LOGIC

If I try to redefine our "CRUD application" with a broader definition: it's a application with a central data model and some business rules upon it. Now we have some reasons to hire a developper!

Basically, we want to add an intermediate layer between our form and the database, this layer will handle the business logic. This is the emergence of a layer architecture.

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

First, we now have a dedicated space in our code where we can develop business logic. The most basic way to use it is to receive an input from the *Application layer*, load some data from the *Data access layer*, then make a decision and update the data. Here's a pseudo-code example:  

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

Secondly, it reduces the coupling to the data model for the consumer. In the "pure CRUD application" solution, our data model was leaking to the outside world and clients had to conform with this model to manipulate the data. This is an issue because if we decide to change our data model, it will break our consumers. Now, we can expose to the outside world some behaviors and abstractions, and keep our data model internal (like in the previous code example). It can be beneficial for commands because we're now exposing only relevant fields for each use case and not the entire model.

Finally, we can also choose to introduce a *rich model*: util now we were manipulating an *anemic model*. An *anemic model* is a basic POJO (*Plain Old Java Object*) that only carries the data. This is often the data model stored in the database. We have to use a *service* to apply business rules (mutations) to this POJO. This separation between data and behavior is not an issue by itself, it's even very common in *functional programming*. The problem is that our business logic conform and works with the storage data model that may not fit well with business constraints. With a *rich model*, we're introducing a new model that is designed to fit with the business, that can possibly enforce some rules with strong typing and may regroup data and behaviors in the same place. Yes it requires some two-directions mapping logic between these two models but this is a fair tradeoff.

## DRIVING BY BUSINESS: GROWING IN COMPLEXITY

Until now, our architecture remains *data-centric*. What I mean here is that is we draw an arrow to represent the data flow, we'll realize that our data model is in the center of our application.

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

This was OK, but now our users are now asking for new cool features that will introduce more complexity to our software, models will grow  and we will heavily rely on automated tests to ensure there is no regression. This is were I think this *layered architecture* shows its limits.

### TESTABILITY

Let's adress testability first. The layers are already testable but with a huge constraint: the higher is the layer we want to test, the more layers to include in our test. This means to test our *Domain layer*, where sits all our business logic (the main thing to test), we have to always include the *Data access layer* to the tests because we're depending of it. That is painfull for several reasons: we must setup data in the database to run a business test, this adds useless complexity and noise to the tests. This is even worse if the *Data access layer* is using an ORM that is mapping foreign keys as objects, in such case we may manipulate a complete object tree. Also, tests relying on database are slower and more prone to side-effects (concurent tests on the same model).

The simplest move I see there is called the *"sandwich pattern"*. The idea is, inside the *Domain layer*, to split data access logic from business logic. This way, we'll be able to test our logic whitout worrying about other concerns, but if we want to also to test this data access logic, we'll have to fallback to our tests with data setup in database. If I apply this pattern to our previous pseudo-code example:  

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

This pattern is called *"sandwich"* because a business logic slice is always surrounded by two data access slices, like the ham between two slices of bread. I would now advice to rework this `void businessLogic(data, value)` to make it pure (and even easier to test because it's deterministic) instead of mutating the `data` input.

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

And *voilà*! We've implemented a new architecture pattern called [*Functional core, Imperative shell*](https://kennethlange.com/functional-core-imperative-shell/). In our previous example, the *functional core* is our `businessLogic` function while the *imperative shell*, it's surrounded by operations in the *imperative shell* defined by the `domainLayerFunction` method.  

### DOMAIN CENTRIC

The *sandwich* trick is great but the architecture still remains *data-centric*, I want to go further!  

In my previous code example, I've stick to an *anemic model* defined by the database schema, if I wanted to introduce my *rich model*, I must define the mapping inside de *Domain layer*: this layer defines the *rich model*, and as it depends on the *data access layer*, so *data access layer* is unaware of the *rich model* and then cannot do the mapping.  

```csharp
void domainLayerFunction(id, value) {
  var dbModel = dataLayer.load(id)
  var richModel = mapToRichModel(dbModel)
  var updatedRichModel = richModel.businessLogic(value)
  var updatedDataModel = mapToDataModel(updatedRichModel)
  dataLayer.save(updatedDataModel)
}
```

There's an alternative way to reduce this mapping problem: we can bring out commands on the *data access layer*. In the following example it takes the form of a specialized function to update data `dataLayer.updateValue(id, updatedRichModel.value)`.

```csharp
void domainLayerFunction(id, value) {
  var dbModel = dataLayer.load(id)
  var richModel = mapToRichModel(dbModel)
  var updatedRichModel = richModel.businessLogic(value)
  dataLayer.updateValue(id, updatedRichModel.value)
}
```

If we choose to go this way, this is a very strong signal that we want to drive our *data layer* by the business. In our *Domain layer*, we don't want to be constraint and even aware of the data model anymore. For now we have to rebuild a valid and complete object/tree in order to persist our business operation (unless we make the commands emerge on the *data access layer*). So it's time to introduce a new trick: the [*dependency inversion*](https://en.wikipedia.org/wiki/Dependency_inversion_principle) (the **I** in SOLID). This is a straightforward technique, instead of a direct call to a dependency, we're defining a contract (often it's an `interface`) that the dependency must fullfill. Then our *Domain layer* got it injected and make the calls through the contract.

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

From the testability point of view, it's get way easier to test the data access logic as we can replace the dependency with any kind of *test double* (*spy*, *stub*, *fake* or whatever pattern you need) to setup your test case. Even if it's not our goal here, it's worth mentioning that this dependency swap is also possible with production implementations, and it allows us to delay some decision making like "Which database should we use to store our data?".  

You may have noticed we're not using nor mapping the data model anymore. Thanks to this technique we've reversed the dependency between the *Domain layer* and the *Data access layer*. *Data access* now knows about our *rich model* and can map it from its own model. We finally have a *domain centric* architecture!

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

> This structure is one of the recommended approach highlighted by the paper "[Out of the Tar Pit](https://curtclifton.net/papers/MoseleyMarks06a.pdf)". It states that side-effects should be pushed to the boundaries of the system, the domain should be pure (side-effect free), and to connect those two part there is an intermediate layer (our service).

Now, we're driving our design with by the *Domain*. We can define, code and test our *rich model* without worrying about data persistance. Once done, we can move and focus on implementing dependencies and their specific constraints.

This is the core concept behing a whole familly of architectures: [*hexagonal architecture*](https://en.wikipedia.org/wiki/Hexagonal_architecture_(software)) (aka *ports and adapters*), *onion architecture* and *clean architecture*. I will not take the time here to explain the differencies between them, mostly because I tend to see them as [bikeshedding](https://en.wiktionary.org/wiki/bikeshedding).

## DATA CONSISTENCY: SHOULD WE REFUSE INPUTS?

I'm doing a small interlude here on our architectures refinement to talk about data consitency. On the first versions we've explored, the *data model* was central and therefore was carrying a huge responsability on data consistency. This takes the form of several database mechanics like *foreign keys* and constraints like `NOT NULL` or `UNIQUE`. Now, we've got a strong *domain model* responsible for business rules, we may consider more supple rules on the database side. Here's my point: should we refuse a command from the user because our *data model* says the operation is inconsistent? This question may seem weird, but sometimes there are good reasons to accept an input and assume our system is out of sync with the reality. Here's are two use cases:  

Fews years ago, I was on a trip and I had few troubles on a connection between two flights. That day, the weather was bad, a lot of clouds and dense fog, which resulted in many delayed flights. My first plane landed late and we were rushing to catch our second flight. After a long run through the airport, we managed to arrive at the gate right on time, but our boarding passes were refused by the system, we didn't get onboard. Indeed the system had determine before the onboarding that we couldn't get there on time, so it automatically registered us in a flight later in the day. I don't now if the system reused our seats for other people, but if it didn't, this is a case where the system was out of sync with reality and have blocked users. However, our boarding passes could have been a good proof that we were there, ready to get in the plane.

Now let's imagine we're working on a warehouse. Items get in, are moved, stored on shelves, and then loaded in trucks for delivery. To track items, each of them is marked with unique codes (barcodes, QR codes) that are normaly scanned at each step. When loading a truck, we're scanning items until the system detect an inconsistency on one of them because it supposed to be on a shelf. Do you think the system should prevent us from loading it into the truck? The barcode gives us a high confidence that this is the correct item we're trying to load.

The way to solve these cases are business, not technical decisions. So we should be carefull not to build systems that enforce this type of consistency without asking domain experts first.

## DISTINCT USE CASES: COMMANDS AND QUERIES

Our previous architecture is already pretty good, I think it's even becoming a standard in any team this some good crafting skills, but let's refine it more.  

In most softwares, reading and writing are not using the same representations of the data. The information displayed to the user is often an aggregation of entities. On the other side, the application of a command is generally more focus on individual entities, and can be easily abstracted with dedicated types containing only revelant values for executing the command. This is an issue as our model is torn by contradictory constraints, getting a model that satisfies both readings and writtings will get more an more difficult.  

Also, you may have heard about the [Pareto principle](https://en.wikipedia.org/wiki/Pareto_principle), this is the famous "80/20 rule". This is not a rule specific to software, but in the case of management software or e-commerce website, one way to formulate this principle is the read/write ratio: in such softwares, most of accesses are for reading and the writing are less frequents. This means, if we want to improve performances, we can easily target 80% of the use cases just by focussing on the read accesses.  

To address these distinct reprensentation needs and unbalanced load, we can try to make two models emerge, one specialized for reads and the other for writes. In OOP, there's a pattern called *CQS* ([*Command-Query Separation*](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation)). The core principle is simple: a method on an object is either a *Command* or a *Query*. A *Command* produce a side-effect and is not supposed to return any value, on the opposite, a *Query* is used to get a value and must not produce any side-effect.  

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

This pattern as been scalled to the architecture level, this is called *CQRS* ([*Command and Query Responsibility Segregation*](https://en.wikipedia.org/wiki/Command_Query_Responsibility_Segregation)). We now have a model dedicated to *Commands* that is responsible of business decisions making and one model (or several if the variety of data representations needed justifies it) dedicated to *Queries* which only returns data in formats useful for consumers (aka *readmodels*).  

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

With *CQRS*, our data model may tend to become more specialized for writing as it becomes the *source of truth*. To fullfill reads requirements we have several solutions: use *SQL views*, write some code to map the *readmodel* at every query or even create and fill dedicated *SQL tables*. With all these solutions, this read/write separation becomes explicit in the code and we can evolve one part with a minimum impact on the other.

## EXECUTING SIDE-EFFECTS: CONSISTENCY AS A TARGET

Now we have isolated our business logic and made a clean separation between reads and writes, we should focus on what makes a software usefull: side-effects.  

So far, I've presented side-effects as something bad that must be pushed as far as we can from our domain, and there is good a reason for that: by definition it's not deterministic, this means for the same input we will not always get the same output. Sometimes it's a different value, sometimes it can be more anoying like an error or a crash. This makes side-effects difficult to test and to gain confidence that our code is correct. But side-effects are also necessary, they're the state changing in the database, the mail sent to the customer, or any other type of IO operations. Everything valuable produced by our software that is observable from the outside.  

When our software is making changes, we're seeking for consistency. We're implicitly making a decision and at the same time applying it in the form of one or many side-effects. But what we wants here is to make sure eveything works fine, making a correct transition from a state A to a state B. If one of the side-effect fail, we can put ourselves in some inconsistent situation and this can be an issue: we may display wrong information the the user, corrupted data may influence futur decision-making, generating even more bad data. Making sure our system is always in a consistent state can be very complex or simply unachievable.  

So we have to ask ourselves the hard question: What happen if something fails and how do we deal with failures? Yes, when talking about IO operations, failures will happen, never doubt that!

If we focus only on the database, there is a mechanism that can almost guaranty us a safe transition from state A to state B, even on complex queries combining multiple inserts, updates and deletes while complying to schema constraints: a `TRANSACTION`. Nothing new here, I bet you already knew about this and you were already using it in the previous architectures we've explored. Though, the "almost" word was important, be aware transactions are not perfect and some inconsistencies are still possible, expecially if you're using database replication and/or partionning. Don't try to develop your own solution to protect yourself from such cases, unless you're an expert, there is no chance that you do a better job than your database.  

> On this very specific topic, I recommend you the book [Designing Data-Intensive Application](https://www.oreilly.com/library/view/designing-data-intensive-applications/9781491903063/) by [Martin Kleppmann](https://bsky.app/profile/martin.kleppmann.com). It does an amazing job describing the internal operations of databases, how they deal with concurency, replications, etc, and explaining how things can go wrong.  

It's getting even more complicated if our side-effects are spread accross several dependencies, like updating our database and in the same time making a call to a third-party API. I such cases we've got no mecanism like `TRANSACTION` to guaranty consistency. In case of a failure, we may need to setup compensation strategies to move back to a new consistent state. In the real-world, such scenarios already exists and are well known by domain experts: "Sorry, the item you bought on our site is no longer available, we will proceed to your refund". New scenarios like this will emerge as our software is now part of business processes. We should educate domain expert on them, explaining technical contraints and why these problems may occurs. But we should also try to avoid as much failure scenarios as we can because they undermine the whole business.  

This compensation solution is necessary when we attempt something impossible or invalid from the business's point of view, but sometimes we're facing technical issues: the business intent is correct, but for some technical reasons we just cannot proceed the operation right now (for example, the third-party API is unavailable). So, why not to defer this operation and retry it if it fails?

A solution is to isolate these IO operations (other than database calls) in dedicated processes. By doing so, we're making sure it will not block the other side-effects and we now have the hability to retry them independently. A way to do this is to enqueue jobs in whatever suits your needs, then to execute them asynchronously in new processes.  

> Hint: the database can make the job, have a look to [PostgreSQL](https://docs.postgresql.fr/current/sql-listen.html)'s `LISTEN` for example.

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

With such solution, a compensation may not be necessary anymore as our system is becoming *eventual consistent*. If something fail, it can be fixed and retried until it works. By the end of the day, our system will be consistent. Also, as processes are now isolated, we've added a level of protection against cascading failures in our software, making it more resilient. I made a more detailled [post](/posts/using-processes-for-better-resilience/) on this specific topic.  

## EXPLICIT DECISION-MAKING

We're not done yet, I think we can go even further. As I've already mentioned it in the previous section, we're implicitly making and applying decisions at the same time. This has a consequence, we may have made a valid decision but fail to apply it. In such case our decision has been rolled-back and discarded. Now I want to change this behaviour, making decisions explicit and not loose them if something else fails!  

From now on, I'll refere to *readmodels* updating and jobs enqueuing as *effects*.  

If we go back to our previous *CQRS* architecture, this means we should be able to update and save our write model. If it doesn't fail, then we can process our *effects*. With this, failing to run one of these *effects* doesn't cancel the decision made and should not affect other *effects* (to be applied or already applied).  

> Hint: I would advice to update readmodels then enqueue jobs as job processing may sometimes rely on dedicated readmodels.

One way to implement such strategy is to aim for an *event-driven* architecture: our *domain* express decisions made in the form of *events*. Our code can look like something like this:  

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

These *events* are then spread accross our architecture and applied with *event-handlers*: first to our write model, and if no failure occured (decision made on an obsolete version, the data violate one of the constraints of the database scheme), then we apply our *effects*.  

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

If one of our *effects* failed to apply, we now have the hability to retry later: on failure we may log the error, the *event* and the *event-handler* that failed. Be carefull though **not to leak** sensitive information into the logs.  

To be honest, from all the architectures explored in this blog post, this one is the one with which I have the least experience. I didn't achieved an implementation of the logging/retry mechanism that really satisfised me, but with more hindsight I can think of two strategies that I believe are viable:  

- store *events* in a dedicted table, each of them as an associated unique id, then log the combination `Event Id * Event Handler * Error`
- store the combination `Event Data * Event Handler * Error` in a dedicated table

In both cases, the informations available allows us to understand the failure and retry the *effects*. *Events* remains volatile and are not meant to be stored for long durations, so we need to put a flushing policy, like a dedicated process run daily that removes old *events*/errors. It can be after a week, a month.  

My personal opinion is that making this split between decision-making and the implementation of the decision is the strongest move to improve your software architecture. It creates an highly resilent system with a souple design thanks decoupled elements (*events* and *handlers*). Though, I must highlight few things here: with *event-driven*, business processes are not always easy to follow and analyze as they're decomposed and spread accross a lot places in our system. Also, our software became even more *eventual consistent* as we can fix unexpected errors and retry any *effect* later: this is all about error managment. I know with *eventual consistent* it can be tempting to apply *effects* asynchronously but this can have major impacts on the user experience. Think how it can be disturbing if on your favorite e-commerce website an item doesn't appear in your cart after you have chosen to add one.  

> Hint: By default I apply my *effects* synchronously. In case of long operations, I use a dedicated *readmodel* where I write "processing", then I enqueue a job to run the process in an asynchronous way. This way I'm not blocking the user and she has a feedback that the operation she asked is ongoing.

## DON'T LOOSE DATA

Here's the final refinement I want to suggest. All our previous architectures suffer from the same flaw: our database model remained handled in a *CRUD* fashion. This can be an issue in the write model as we're implicitly loosing data everytime we're updating a value. Yes, Update is an implicit Delete as we're loosing the replaced value.  

New features may not be compatible with already existing data. Missing data can also prevent us from correcting incorrect data due to bugs or mishandling by users. Analyzing the past is an opportunity to discover trends, doing statistics and improve future business decisions.  

To mitigate this issue, I want to move from an *event-driven CQRS* architecture to the *CQRS/Event-Sourcing* architecture. To do this, we "just" have to store our *events* instead of a state in our write model. By doing so, our model is now in *append only* mode, technical issues put aside, the only way to fail at write is a conflict. This allows us to easily detect concurrent operations on a same *event stream* (the list of the *events* that were emitted by an *aggregate*), the version of the *event stream* is the number of *events* that composes it: if we try to insert an event with a number that already exists, we're in a conflict.  

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

We also have to rebuilt the internal state of our *aggregate* from an *event stream*, this operation can be expressed as a simple `leftFold` ([`Aggregate`](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.aggregate) in .Net, [`redude`](https://developer.mozilla.org/fr/docs/Web/JavaScript/Reference/Global_Objects/Array/reduce) in Javascript).  

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

However, don't believe CQRS/ES is "just" storing and folding *events* of our initial *event-driven* architecture. In addition of communication, our *events* are now used for storage purposes (this is their main role), this means they're not volatile anymore.  

We'll have to setup a good serialisation strategy as well as a versioning/migration policy. These topics are critical and must be considered seriously.  

From the developer point-of-view, *event streams* also require good knowledge about the application: what is a valid *stream*? What *events* are in that stream? In what order? This can be tricky to hold all these information in our mind, especially for complex *aggregates*.  

We also have to put extra care into our *events* design as we'll have to manipulate them over time. A bad design can really hurt our development velocity in the long run. I wrote a dedicated [post](/posts/cqrs-es-how-to-achieve-a-good-event-granularity/) where I explain my heuristics to find good *events*.  

Finally, *event-sourcing* doesn't tells us the whole story: unless we treat business errors as *events*, we will only observe in our *events* the result of successfull commands. When we analize trends in production, this is something that we need to keep in mind.  

*CQRS/Event-Sourcing* is the most advanced architecture pattern I've used in production during my carreer. I love it because it allows me to write reliable and resilient software, but as I've highlighted it, this is not an easy pattern to use. There is probably some new refinments we could imagine to improve this architecture, but they're out of my current knowledge.  

## CONCLUSION

In this blog post, we've navigated through several architecture by adding improvements. As they grow in capabilities, they also grow in complexity.  

Keep in mind that not all applications need the most advanced *CQRS/Event-Sourcing* pattern. Choose the architecture that is good enough for the problems you're trying to solve. Even more, we don't have to write an entire software using a single architecture. In a same context, some parts are well defined with clear life-cycles and therefore can be *event-sourced*. Other parts may be well-suited for a simple *CRUD* approach as they have no life-cycle nor business rules. Mixing these patterns isn't an issue at all.  

I'm also aware we've not explored all architectures that exist out-there. For example I know about the *[actor-model pattern](https://en.wikipedia.org/wiki/Actor_model)* or the *[vertical slice architecture](https://www.jimmybogard.com/vertical-slice-architecture/)* but I chose not to talk about these in this post as I wanted to focus on architectures I've used in a production context.  

Finally, even if our architectures have increased in complexity, it doesn't mean that the cost of the software is growing with it. Sure, it requires more skilled developers and the initial cost of a new feature may be higher, but we have to consider how the code complexity is contained over time, how costly it is to run and maintain the software in production, etc. Increased initial development cost can result in lower maintenance, production and evolution costs. That's why only looking at the development costs to drive a technical choice doesn't make sense to me. This is also the reason why I don’t think these advanced architectures are necessarily more expensive.  

## plan

- Have you already heard/said "this app is just a CRUD"?
  - common assertion
  - I don't believe any of us have encounter such software

- Define CRUD
  - DB access operations : Create, Read, Update, Delete
  - "CRUD app" strict definion: your app is basically a form to manipulate data in the DB
  - something that can be generated or use off-the-shelf solution
  - only concern is storing data, no business logic
  - you don't pay devs only for a CRUD

- Add logic: business rules
  - "CRUD app" loosy definition: a single model with some rules upon it
  - validation rules (how to hightlight making business action on a DB model can be hard?)
  - data model vs domain model
    - anemic and rich models
    - on commands, we want to provide only relevant values, not all the model
    - reason of architectures like nTiers: introduce abstractions
  
- Driving by business (how to highlight issues with DB centric?)
  - from data modification (db centric) to business decision (domain centric)
  - business decisions may require complex data structures that not fits DB models
  - issue: the central model (db or domain) is the one that influence the code
  -> domain centric is easier to test as side-effects are pushed to the boundary
  - time for dependency injection, from nTiers to hexa/onion/clean

- data consistency: should we refuse an input from the user because the data in the DB says so?
  - example: missed connection between two flights
  - example: truck loading at a warehouse
  -> should we refuse a command because the data state is not synchronized with real life ?

- Pareto law: commands and queries
  - pareto law: 20% write, 80% read
  - data representation are not the same
  - make it explicit: use CQRS
    - CQS at scale
    - usage of SQL views -> query models
    - SQL views are good enough as a first step, no need for dedicated models
    - make it explicit in the code

- About IO/side-effects: applying effects
  - define effects: change of state in the system, IO, etc
  - aiming for consistency
    - changing state and other IO all at once
      -> implicit decision
      -> can be very complex
    - what happen if something fail?
      - database focus: -> reason of mecanisms like transactions, locks, etc (from one state to the next)
      - other kinds of dependencies like apis
    - any possibility for compensation?
    - isolate and delay some effects: run new processes
    -> more resilient software

- Explicit decisions: splitting decision making from decision execution
  - decision and execution are two distinct concerns
  - a valid decision should not fail because of the execution
  - all effects are separated from decision making
    - emiting events
    - execution can be retried
      - failure to apply effects: log handler that failed and the event associated
      - caveats: carefull not to leak sensitive data in logs
      - 2 strategies:
        - store events in a table + log 'handler failed for event id #, error xxx'
        - dedicated table 'handler - event data - error'
        => ever way: add policy to flush old data
        => events remains volatile
    - executions are more isolated
  - personal opinion: one of the strongest move to make
  -> even more resilient software

- don't loose data
  - storing state = losing past (Update is implicit delete)
  - from event-driven to event-sourced: events as a storing strategie and not just a communication
  - event-sourcing constraint:
    - rebuild states from events
    - event versioning and serialisation
    - bad event design is more painfull compared to event-driven (must understand business)
    - unless errors are events, we only see what succeed, this is not the whole story

- conclusion
  - not a single solution: we can mix these stategies in the same context
  - other paths we didn't explore in this post
  - growing in skill required != growing costs
    - coding cost vs production costs
    - considering accidental complexity
    - personal opinion = more advanced architectures are not more expensive

## note

chercher les contraintes/difficultés introduites, surtout à partir de event-driven

---

## COMMENTS

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
