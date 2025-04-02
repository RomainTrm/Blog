---
title: "State-based and event-based approaches"
date: 2024-11-20T13:02:48+01:00
tags: [post, en]
draft: false
aliases: ["/posts/2024-11-20/"]
---

I've recently gave a [talk]("/posts/cqrs-es-nos-heuristiques-apres-plusieurs-annees-de-production") with my friend [Aurélien](https://bsky.app/profile/boudoux.fr) about the heuristics we've developed after using CQRS/ES for several years.  

After our talk, we had a chat with some developers. We concluded that choosing a _state-based_ oriented approach (like [CRUD](https://en.wikipedia.org/wiki/Create,_read,_update_and_delete)) seems to be the default solution, such choice seems to remain unchallenged. On the opposite side, choosing an _event-based_ systems (event sourced or event driven) will very often be heavily challenged.  

## Thinking in events

In a [previous post](/posts/2016-12-20/), I've expressed some kind of thought experiment to highlight how natural events can be:  

> _Me_: "Where do you live?"  
> _You_: "I leave at _place A_"  
> _Me_: "Ok, did you already live there four years ago?"  
> _You_: "No, I was at _place B_"  
> _Me_: "What happened?"  
> _You_: "Well, I had a child and we needed a bigger house, so we moved to this new place"

So we're able to formulate how things change over time. We can also indicate which event caused the change, such as your address changed because you moved to another location.  

Another thing to note is our ability to identify the causality effect: you had a newborn, so your flat became too small, so you decided to move to a bigger place.  

This looks like our thoughts can be arranged as some kind of _event-based_ representation.

## Thinking in states

This representation of the human memory seemed rather natural to me until my colleague Mickael gave me a counter-argument : sometimes you know something has changed, you can express the change over time, but you're just unable to explain what caused this change.  

We could argue that in such cases, we memorize that "well, something changed" but it doesn't sound natural.  

Also, I realized that for accessing some information in my memory, often I don't have to recall everything that happened to me. I can access this information immediately because I just know it (this kind of fast thinking is prone to huge biases).

But this is not always true, sometimes I have to do some intense thinking to retrieve information. In these cases I correlate events with states until I've achieved my goal (or gave up).  

I am not a neurologist or cognition scientist, but I'm making an easy assumption here: we're memorizing states and events, and we're able to make causality relation between them. However, sometimes we're only remembering partial information. To fill the gaps, we are forced to infer states or events in our lives.  

So, our thinking and memory aren't fully _state-based_ neither fully _event-based_. Both concepts should be natural for us.

## Software complexity

Back to software development! Most of us have worked on _state-based_ applications like CRUD, this is probably one of the most common architecture pattern. These concepts are actually quite easy to grasp and understand.  

On the other side, I believe a lot of developers had never worked with event sourced (or event driven) applications. These patterns are considered complex and indeed, as a seasoned CQRS/ES pattern architecture developer, I can attest they are. But it also provides a huge benefit: you append and save events (rather easy, it can only fail if you encounter a conflict because someone else added new events), then you apply these events to your system. If it fails while applying, you can retry, fix the code, etc. No impact on your business because you didn't lose any data.

From my point of view, the missing separation between making/saving decision then applying it is the biggest weakness for CRUD applications.  

Indeed, you must apply all your business rules (making decision and applying it) in a single point in time. But what happens if your code had a bug? Or if you've made decision on stalled information? You may have corrupted or lost some data and users will be mad at you. To prevent this kind of scenario, we're using defensive patterns like pessimistic locks or some questionable versioning implementations. These add a lot of complexity to our software.  

That's why I'm arguing that _state-based_ isn't that simple when you have to implement it.

In defense of CRUD pattern, I believe there's one scenario where it shines: you store data as you received it **without** applying any form business rule, exception made for rules that can reject new inputs. If I rephrase this: when receiving an input, you can **C**reate, **U**pdate or **D**elete an entry in the database or reject the input. If you need to **R**ead data to make a decision, CRUD can put you in troubles.  

## Habits

The thing is, I'm biased. I don't know how much, I just know I am.  

I've been mostly working on _Event Sourced_ system over the last four years. So when designing software, this _event-based_ thinking is probably way more natural to me now than for most developers.  

And this is probably the point: if you do something a lot, it turns easier and more natural for you.  
Software has been built with _state-based_ approach for decades now. We're so used to them that they are considered as normality. So normal that we're almost unable to see and recognize their flaws.  

In this blog post, I’m not making an argument against CRUD, I’m just trying to challenge the false belief that CRUD is simple.  

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
