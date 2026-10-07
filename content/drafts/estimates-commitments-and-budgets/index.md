---
title: "Estimates: commitments and budgets"
date: 2026-10-14T09:00:00+02:00
tags: [post, en]
draft: true
---

With years of experience, I've learned what activities I like and the ones I dislike about my job. Among the second category seats in a high place estimates. By chance, I worked on different teams with different relations to estimates, and I think I now have a clear idea of how I can work with it.

Truth is, they don't bring much value to the developers. Often, it is a difficult process of guessing with a poor overall result. I may even be tempted to think we only produce accurate estimate by accident. However, legitimately, management requests it because they need it for decision-making and budget tracking.

Today, I see two ways to cope with estimates, one I will try to avoid, and one that makes sense to me.

## The path I don't want to follow

Even though they are not presented as such, the typical way to use estimates is to use them as team commitments: You think this new feature should take you 2 days of work? Now, you’d better meet that deadline or be prepared for someone to ask you why his burndown chart is drifting.

If it sounds exaggerated to you, I have witnesses such interactions between developers and agile coaches/scrum masters.

Commitments tend to put pressure on the team, pushing them to take shortcuts and favor quick and dirty solutions to respect deadlines, which can be harmful to the project in the long term. Moreover, respecting a deadline is considered as normal while failure is something for which the team will be held responsible, which can be dangerous for team morale.

### Scrum and Poker Planning

For me, all these ~~estimates~~ commitments occurred in Scrum environments. I could write a opiniated post about Scrum, but I'll keep it short: It should be used as a starting point, a framework that sets a workflow for a team that doesn't have one yet. Then, this workflow should evolve to adapt the team’s needs (What does work? What doesn't? What's useless? Etc.) If applying Scrum "by the book" is a goal, then it is an anti-pattern.

In this workflow seats estimate ceremonies, the infamous "Poker Planning". In my opinion one of the most wasteful workshops I've attended in my career: basically you review upcoming tasks, let the team debate and then drop in numbers, sometimes from the Fibonacci suite, to estimate effort (someone will secretly convert them later in time).  

To me, the format of this event is the issue.

First, I'm not sure mobilizing an entire team to decide whether a task should take 2 or 3 hours to develop is a good use of its time.

Then, by experience, most of the team members discover the new task at the moment they're asked for an estimate, so they ask for precision and challenge the feature. This is the right thing to do but not the right time, this discovery phase comes too late and in an inefficient way. I would prefer to run dedicated [example mapping](https://cucumber.io/blog/bdd/example-mapping-introduction) sessions beforehand for exploration and specification.

Finally, there is usually an error made when estimating the "effort" that a team can manage for the next sprint: we plan as if the team was only working on these tasks, disregarding other activities such as managing production operations or conducting job interviews to find a new hire.

## What is a correct estimate anyway?

I'm fully aware that some frameworks and methodologies exist for producing "goods" estimate. I can mention [PERT](https://en.wikipedia.org/wiki/Program_evaluation_and_review_technique) that includes some mathematical theory for estimating the time required for a given task. Truth is, I've never used such framework, and I don't think anyone wants to do it for a conventional IT project.

At this point, I think it's also worth mentioning the [Hofstadter's law](https://en.wikipedia.org/wiki/Hofstadter%27s_law): a task will always take more time than anticipated, even when Hofstadter's law is taken into account. We could understand it as *we always underestimate* but I think it also highlights another side effect: the more time is available, the more we take to accomplish something. It doesn't mean we procrastinate, it is rather a matter of adding extra work/steps to achieve the desired result.

Note that the overall quality of the result will also depend on the time allocated to the task, which means there's a minimum amount of time needed to achieve the expected quality.

![Three drawings of Spiderman, the first one is of high quality and took 10 minutes, the second took 1 minute and has fewer details, the third took 12 seconds and seems scribbled.]({{< relref "spiderman.webp" >}})

## A path I'm willing to follow

The following strategy is similar to what I have experienced in recent years. First, let's assume we have a decent understanding of the upcoming features to develop (thanks to example mapping sessions, mockups, etc.).

Now, the management is asking us for estimates. We can give them a coarse estimate; an order of magnitude. How much time for this feature? Is it a few hours? A day? Maybe a couple? A week? We should not seek precision here, this estimate exercise should be as fast as possible. If we can't even estimate the order of magnitude, then the task may be too big or requires some exploration first.

Then, it's up to the management to decide how much time it wants to invest in a feature given the estimated time, the quality expected and the business priority. This investment can cover the entire scope or only a sub-part of a feature.

If we finish within the planned budget, it’s a success. If instead the expected output is not there yet, then we do a quick estimate of what's left. Maybe we'll get extra credits to continue the task right away. Maybe the management will be satisfied with what he got, and the remaining work will be postponed because it doesn't bring enough value to be done now.

Sometimes we realize that we've used the wrong order of magnitude. In this case, it is important to understand why and maybe start another round of exploration before investing again in the feature.

There's a subtle but important difference with the commitments here:

- Commitments define the result we expect after a given period. If at the end of the period, we don't have what we were hoping for, then we failed to achieve our objective. We are somehow result-driven.
- The solution I suggest is cost-driven. The work is done at a constant and sustainable pace and we define after the fact if the time we've allocated was sufficient to achieve our goal.

## Conclusion

What I really appreciate about the approach I suggest is it still provides useful insights for decision-making, but at a lower cost and with less frustration for the devs. It also replaces an important lever for the installation of a blame culture by a simpler management where the results are correlated to the time invested. This moves the responsibility of the respect of the budget and deliveries from individuals (the devs) to the organization.

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
