---
title: "Estimates: commitments and budgets"
date: 2026-07-07T12:22:58+02:00
tags: [post, en]
draft: true
---

With years of experience, I've learned what activities I like and the ones I dislike about my job. Among the second category seats in a high place estimates. By chance, I worked on different teams with different relations to estimates, and I think I now have a clear idea of how I can work with it.

Truth is, estimates don't bring much value to the developers. Often, it is a difficult process of guessing with a poor overall result. I may even be tempted to think we only produce accurate estimate by accident. However, management requests it because they need it for decision-making and budget tracking.

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

Then, by experience, most of the team members discover the new task at the moment they're asked for an estimate. So they ask for precision and challenge the feature. This is the right thing to do but not the right time, this discovery phase comes too late and in an inefficient way. I would prefer to run dedicated [example mapping](https://cucumber.io/blog/bdd/example-mapping-introduction) sessions beforehand for exploration and specification.

Finally, there is usually an error made when estimating the "effort" that a team can manage for the next sprint: we plan as if the team was only working on these tasks, disregarding other activities such as managing production operations or conducting job interviews to find a new hire.

## A path I'm willing to follow

The following strategy is similar to what I have experienced in recent years. First, let's assume we have a decent understanding of the upcoming features to develop (thanks to example mapping sessions, mockups, etc.).

Now, the management is asking us for estimates. We can give them a coarse estimate; an order of magnitude. How much time for this feature? Is it a few hours? A day? Maybe a couple? A week? We should not seek precision here, this estimate exercise should be as fast as possible. If can't even estimate the order of magnitude, then the task may be too big or requires some exploration first.

Then, it's up to the management to decide how much time it wants to invest in a feature given the estimated time and the business priority. This investment can cover the entire scope or only a sub-part of a feature.  

Here's a subtle but important difference with the commitments:

- Commitments define the result we expect after a given period. If at the end of the period, we don't have what we were hoping for, then we fail to achieve our objective. We are somehow result-driven.
- The solution I suggest is cost-driven. The work is done at a constant and sustainable pace and we define how much time we want to allow for a given task. If we finish within the planned budget, it’s a success. If instead the expected output is not there yet, then we do a quick estimate of what's left. Maybe we'll get extra credits to continue the task right away. Maybe the management is satisfied with what he got, and the remaining work will be postponed because it doesn't bring enough value to be done now.


- mon opinion à propos de Scrum :  
  - une méthodologie pour poser un cadre à une équipe qui n'en a pas encore
  - n'a pas vocation a rester tel quel, l'équipe doit adapté les process à son fonctionnement
  - => se revendiquer "by the book" est un anti-pattern
  - critique : beaucoup trop de cérémonie et de temps d'interuption de l'équipe, notamment sur les estimates et les affinages (autre sujet, mais pour faire court, le format n'est pas le bon selon moi)
- les estimates ont très peu de valeur pour l'équipe, sert avant tout au mgmt et au suivi des coûts
- par expérience, les estimates ont tendance à
  - être transformés en engagements
  - infantiliser les équipes ("pourquoi t'as pris plus de temps ?")
  - ne jamais être correct
- par ailleurs, quand on planifie un sprint, on a tendance à prévoir de la charge pour tout le sprint, le temps réellement disponible est rarement pris en compte (si on retire les temps de réunion, temps de maintenance de la prod, RH, autre)
- => génère chez moi beaucoup de frustration
- mon expérience : des très petites équipes qui sont responsable des développements et de la prod
- stratégie alternative :
  - quand possible, estimation à la louche (heure, demi-journée, journée)
  - priorisation des tâches
  - un budget aloué par tâche (ou sous partie)
  - si le budget est dépassé, alors on arrête et on analyse pourquoi et on repriorise

- manque une conclusion/bénéfices

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
