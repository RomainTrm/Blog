---
title: "THE \"TECHNOLOGICAL ADULTHOOD\""
date: 2025-02-19T09:00:00+01:00
tags: [post, en]
draft: false
---

Few months ago, I've read this article: [How We Built a Self-Healing System to Survive a Terrifying Concurrency Bug At Netflix](https://pushtoprod.substack.com/p/netflix-terrifying-concurrency-bug).

What I loved is how unconventional the solution was. However, unconventional doesn't mean irrelevant, their solution kept the software running during the weekend, and this without any human intervention. The solution wasn't perfect, but it was "good enough" and even more, respectful of people's time.

By the end, it concludes with a concept that somehow inspired me: "technological adulthood".

## MY PERSONAL EXPERIENCE

In the early years of my career, I've worked for a company that was starting a greenfield project. When I joined the team, there were already some developers and a certified expert for a technology we used. I still remember how frustrating it was for me to work with this expert, nothing personal, he was a nice guy, but our goals were not aligned.  

When I had to work on an integration point with the technology, I was trying to use only the features I needed at that exact moment to achieve my goal. Then he was systematically asking me to rework my code to match the provider's _best practices_, usually with a link to the documentation containing over simplistic examples. Even if I argued, I always complied because I wasn't in a position to refuse, but I remember at that time the business goal I was trying to achieve was never driving the solution, it was always _best practices_.  

With more experience, I believe none of us was entirely right or wrong. Clearly, I hadn't thought enough about how the system would grow and become more complex: some of these _best practices_ were good advices. But I still don't believe we needed to set up all of them.  

## TECHNOLOGIES AS TOOLBOXES

Here's my conception of technology: it's a tool aiming to address a certain category of issues. To do so, it provides a set of capabilities to work around the issue we're dealing with. But it also introduces new constraints to our software: we must comply with how the technology is supposed to be used. To reduce this constraint, we can choose among all capabilities provided only those we need. Our goal then is to choose the correct technology to address our problem, and to use it properly.  

Building software may (often) required several capabilities that cannot be provided by a single technology. In such case, we have to compose them, like choosing a database for storing data, then a framework to expose a REST API, etc.  

If I try a metaphor here, its like building your kitchen: you're using several toolboxes. At some point you'll be working on electricity, so you will use the dedicated toolbox, with the dedicated tools you need. Then you'll switch to something else like plumbing, by doing so you will also change the tools you need.  

## CRAFTING PRACTICES

As with technologies, crafting practices can be seen as another set of tools. I'm thinking of practices like _Test-Driven Development_, _Behavior-Driven Development_, _Domain-Driven Design_ or workshops like _[example mapping](https://cucumber.io/blog/bdd/example-mapping-introduction/)_ or _[event storming](https://www.eventstorming.com/)_. Each of these brings us value for specific set of problems, but it just doesn't make sense to use them all the time.  

For example, _event storming_ is a great workshop to explore the domain and understand the challenges faced by the users: we're evolving in the _problem space_. _Example mapping_ is another exploratory workshop, but it's designed to find good examples (test cases) for a feature or a set of rules, and to ensure we've considered all cases before developing: we're already designing our software, we're in the _solution space_.

The same is true for crafting techniques like _Test-Driven Development_, it's great when we know what to develop, but we're probably not gonna use it for an exploratory session when we're mostly sense-probing the problem we're trying to solve.

## EVOLVING SOFTWARE

If, like me, you are working on software running in production, or even more, the product your company is selling, you must know finished software does not exist. The only reason software doesn't evolve anymore is because no one is using it or you decided to kill the product.

This means our code is always evolving for technical or business reasons. Virtually, every piece of code we're writing can be subject to future change (even if the likelihood of change is not evenly distributed). What's exactly why I tend to see more and more code I write as temporary.  

It doesn't mean I don't put any care to craft a good code, I'm just looking for something "good enough" as I'm writing code instead of the best solution. Yes, the code could always be better, more optimized, but we will probably never hit its limits with the current form of our business (features available, traffic and workload).  

## "TECHNOLOGICAL ADULTHOOD"

That's why I loved so much this "technological adulthood" concept. It summarizes in two words the idea of picking the right tool(s) to respond to a situation (even if it's a very uncommon one) in the best way possible relative to the socio-technological context.  

To do so, we must always focus on our business goals. We're not using a technology as it is supposed to be? Maybe that's OK because we're benefiting from this unconventional use. Or maybe it will hurt us badly and we should consider switching to something else, more adapted to our needs.  

Every company has some constraints (financial, operational, human) we must deal with. We have to build solutions that fit these constraints. In my post ["Using processes for better resilience"](/posts/using-processes-for-better-resilience), I tried to highlight how these patterns are helping us deal with production incidents. Thanks to these kinds of strategies, a team of two or three developers can develop, maintain and run a complete system for a small company (However, this is not enough, we also need the correct culture and team organization).  

Somehow, I see a trap that must be avoided: don't over anticipate the future because it's too uncertain. Making strong design decisions for the future is then a risk of misalignment between the needs and the solutions available. We should try to delay such decisions as much as we can until we've got enough inputs to make the correct choices. Meanwhile the best thing to do is probably to aim for designs that keep options open for the software to evolve in different ways.
