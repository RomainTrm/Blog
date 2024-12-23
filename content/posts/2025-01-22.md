---
title: "THE \"TECHNOLOGICAL ADULTHOOD\""
date: 2025-01-08T09:00:00+01:00
tags: [post, en]
draft: true
---

A couple months ago, I've read this article: [How We Built a Self-Healing System to Survive a Terrifying Concurrency Bug At Netflix](https://pushtoprod.substack.com/p/netflix-terrifying-concurrency-bug).

What I loved is how unconventional the solution was. However, unconventional doesn't mean irrelevant, their solution kept the software running during the weekend, and this without any human intervention. The solution wasn't perfect, but it was "good enought" and even more, respectfull of people's time.

By the end, it concludes with a concept that somehow inspired me: "technological adulthood".

## MY PERSONNAL EXPERIENCE

In the early years of my career, I've worked for a company that was starting a greenfield project. When I joined the team, there were already some developers and a certified expert for the cloud provider we used. I still remember how frustrating it was for me to work with this expert, nothing personal, he was a nice guy, but our goals were not aligned.  

When I had to work on an integration point with the cloud, I was trying to only use the features I needed at that exact moment to achieve my goal. Then he was systematically asking me to rework my code to match the cloud provider's _best practices_, usually with a link to the documentation containing over simplistic examples. Even if I argued, I always complied because I wasn't in a position to refuse, but I remember at that time the business goal I was trying to achieve was never driving the solution, it was always _best practices_.  

With more experience, I believe none of us was entirely right or wrong, I hadn't thought enought about how the system would grow and become more complex. But I still don't believe we needed to setup all the requirements decribed in these _best practices_.  

## TECHNOLOGIES AS TOOLBOXES

Here's my conception of a technology: it's a tool aiming to adress a certain category of issues. To do so, it provides a set of capabilites to work around the issue we're dealing with. But, it also introduce new contraints to our software, it musts comply with how the technology is supposed to be used. To reduce this constraint, we can choose among all capabilities provided only those we need. Our goal then is to choose the correct technology to adress our problem, and to use it properly.  

Building a software may (often) required several capabilities that cannot be provided by a single technology. In such case, we have to compose them, like choosing a database for storing data, then a framework to expose a REST API, etc.  

If I try a metaphor here, its like building your kitchen: you're using several toolboxes. At some point you'll be working on electricity, so you will use the dedicated toolbox, with the dedicated tools you need. Then you'll switch to plumbing or carpentry, by doing so you will also change the tools you need.  

## CRAFTING PRACTICES

Like technologies, crafting practices can be seen as another set of tools. I'm thinking of practices like _Test-Driven Development_, _Behavior-Driven Development_, _Domain-Driven Design_ or workshops like _example mapping_. Each of these brings us value for specific set of problems, but it just doesn't make sense to use them all the time.  

Yes, there's moments when I don't use _Test-Driven Development_! 

  - details

## EVOLVING SOFTWARES

If like me you are working on software running in production, or even more, the product your company is selling, you must know there is nothing such as a finished software. The only reason a software doesn't evolves anymore is because no one is using it or you decided to kill the product.

This means our code is always evolving for technical or business reasons. Virtually, every piece of code we're writting can be subject to future change (even if the likelihood of change is not evenly distributed). What's exactly why I tend to see more and more the code I write as temporary.  

It doesn't mean I don't put any care to craft good code, I'm just looking for something "good enought" as I'm writing code instead of the best solution. Yes, the code could always be better, more optimized, but we will probably never hit its limits with the current form of our business (features available, traffic and workload).  

## "TECHNOLOGICAL ADULTHOOD"

  - build a solution that fits our goals, not industry standards
  - build something practical, that suits human/operational constraints
  - anticipate neer futur, dont over-anticipate long term futur
  - KISS and YAGNI ?
