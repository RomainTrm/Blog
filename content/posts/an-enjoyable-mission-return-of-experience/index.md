---
title: "An enjoyable mission: return of experience"
date: 2022-09-01T20:53:08+02:00
tags: [post, en]
draft: false
aliases: ["/posts/2022-09-01/"]
---

I had the chance to work few months in [Agicap](https://agicap.com/), an enterprise producing a cashflow management SAAS for businesses.  
It was a great mission, my team worked a way that I consider to be, so far, the most efficient and pleasant in my career. We managed to produce value at a constant speed while keeping a full control of our code, not allowing any kind of quality depreciation over time.

This blog post is just a recap of all practices we had in place and why I believe it was beneficial for us. Please, keep in mind we chose these regarding our context, this is not some sort of silver bullets list.

Some context first, we worked on a new application, this was a green field project integrated in a distributed system. To do so, we were eight experienced and skilled people&nbsp;:

- one product manager
- one UX/UI designer
- one front-end developer
- five back-end developers (I was one of them)
- The team worked in full remote as team members lives in various places in France.

We were also supported by an SRE team for infrastructure and delivery automation concerns.

In less than two months, we had a first MVP in production with some real users, then we kept delivering features on a regular basis (more on this later) while maintaining a high code quality.

## How did we organize?

We worked in an Agile way, we had some rituals like daily meetings, retrospectives and some feature refinements, but we never chose an agile framework to work with. The reason is we wanted a process that match our way of working, not some « by the book » that no one really understands. Nevertheless, as we put a high focus monitoring our work in progress (WIP), Kanban is probably the closest to our way of working.

To help you realize how important WIP was for us, with five back-end developers, we tried to avoid developing more than two features in parallel, this mean we were working most of our time in pair/mob programming (I would say approximately 90% of mine). The rest of the time was dedicated to minor tasking and meetings with other teams. We sometime did more parallel work and it resulted in some organizational clutter and a lower overall quality.

We never had to do any kind of estimate, every task we did take as long as needed. I believe it helps reduce the WIP as you’re not constraint by time, so you’re not tempted to rush a feature to start the next one. As you’re not rushing the work, the overall quality benefits from it and you reduce the probability of a defect that requires you to stop what you’re doing to fix it. If we considered that one task was taking much longer than expected (and it happened several times), we just stopped it, step back to understand why and then we acted to solve bottlenecks and issues.

It doesn’t mean we did not have any planning at all, we were following a three-month roadmap with all the main features expected from this period. The trick was the planned workload was always lower than the team’s capacity (we were confident we could achieve our goals). By doing so, you’re preventing delays from unexpected constraints and complexity. Spare time is available for the team to do some refactoring, addressing code pain points, keeping the code base healthy. This time can also be used investing more in automation.

## Discovering a new feature

When developing a new feature, it is important to spot complexity, edge cases and unexpected interactions with existing features as soon as possible. To do so, we had short meetings (half an hour) on a regular basis (usually twice a week). During these, we presented new features, challenged UI mockups, and we tried to create an exhaustive listing of every use cases.

Example Mapping is an amazing workshop to work around those corner cases as it provides a good support for reasoning. I will not explain here how the workshop works, I recommend that you read [The BDD books: Discovery](https://www.bddbooks.com/) for more details. Also, a benefit we didn’t expect when introducing example mapping: it allowed us to isolate some problematic business rules among a feature, so we managed to develop it without the rule to avoid this unnecessary complexity.

## Tests and refactoring

All our developments were test-driven, we put a lot of efforts in our test suite to be clean, easy to evolve and not too coupled to the production code. Tests require as much effort as the rest of the code to remain clean and evolutive, don’t hesitate to invest in builders and abstractions. It allowed us to do some heavy refactoring on our model without any major impacts in our test suite.

Refactoring isn’t just cleaning process or a response to remove some pain in the code. Software evolves and new features may not match the existing model. We broke ours several times to prepare for future developments. Fortunately, we worked in a context where everybody understands technical stakes, so we never had to justify any of our refactoring. Occasionally, we developed some temporary features to help our PM solve business issues, but these never lasted more than a few weeks before being superseded by longer-term solutions.

## Experiments and failures

We tried to define and follow a norm for our developments, but it had to continuously evolve as new features may introduce new requirements. This is a moment when the team discussed and try to choose an adapted solution. More than once we didn’t agree, so instead of losing time in useless discussions, we stopped and tried several solutions in a time-boxed period (no more than a few hours). Then we compared results and defined the way we wanted to implement new behaviors. Quite often, it was a mix of several solutions.

Other times, we were pretty confident with what we developed, and then later (during development or even after deploying it to production) we realized it wasn’t that great. In this case, we only had to answer the following questions:

- If already developed and deployed, is this piece of code subject to future evolution or not?
- Does it have an impact on our productivity when developing features? (It could be the case for some architectural elements.)

Depending on the answers, we chose to stick with the current implementation, or we evolved it.

## Continuous deployment

I’m doing a short parenthesis here to introduce you [Accelerate](https://itrevolution.com/product/accelerate/)‘s metrics:

- Lead time
- Deployment frequency
- Percentage of failures
- Mean time to restore

I want to highlight something: by increasing deployment frequency, you naturally reduce lead time. Also, as the batches you’re deploying are smaller, you reduce the number of changes to the previous version, so you decrease the probability of a defect. If you detect a defect anyway, as the number of changes is small, you will probably isolate the issue faster, so you can lower your mean time to restore.

For these reasons, the team was aiming for continuous deployment, our goal was to deploy in production every time we merged something on our main branch. As we were learning how to do so, we didn’t achieve that goal before the end of my mission, but we were already deploying at least once per day and we were already benefiting from it. Continuous deployment isn’t binary, there are different grades, each one adds more problematic to consider. Here’s one example: we had to set up a zero downtime deployment process as our users can use our service at any time and we don’t want to interrupt them. If your users only use your service during office hours, and you’re OK with a single deployment per day, automatizing a night deployment can be an excellent tradeoff to avoid zero downtime concerns.

Here’s the list of practices we had to deal with to achieve our goal: API versioning, feature toggling, rolling updates, graceful shutdown, zero downtime database schema migration, deployment automation. These can look rather simple in theory, but appears to be challenging when applied. For this reason, we included them progressively in our practices, so we could master them more easily.

These allowed us to work effectively with a trunk-based approach, this way, we were constantly integrating our developments and reducing merging costs. We also became more confident in our deployments as we had various ways to isolate potential defects (using feature toggles or downgrading to the previous version for worst-case scenarios). Feature toggling has other interesting aspects: as we could activate a toggle for some specific users, we used it to test new features directly in production. Once validated, our PM could communicate freely with users and choose when to make it accessible without depending on the developers.

## To conclude

I really enjoyed this mission, this way of working matches how I imagine my work. It requires skills and rigor, but to me, it removes a lot of pain that I consider avoidable. Anyway, I’m conscious this is highly contextual, and not every company and teams have the means to do so.

At the beginning of this project, I had the feeling we could move faster and develop more. I’m happy we didn’t because it wouldn’t have been sustainable after a few months.

By my experience, the software industry seems to be addicted to sensation of speed. We have to learn to slow down to reach a sustainable and constant speed of production.

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
