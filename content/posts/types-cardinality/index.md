---
title: "TYPES CARDINALITY"
date: 2025-04-03T09:00:00+02:00
tags: [post, en]
draft: true
---

When writing a software, we're using types to represent the information we're manipulating. Values are express through primitive types like `bool`, `int` or `string`. We're building complex data representation by composing these types. This composition is done by defining our own types, usually with classes or tuples.  

Even if there's several ways to store and represent the same information, these ways are not all equivalent. Some are too permissive and alows states that should be considered as illegal regarding our business rules. Others are well defined and only represent legal states, meaning we don't have to write code to defend ourselves against bad data.  

Did you now there's a mathematical way to compute the number of possible states of your model? We can then compare this number to the number of possible combination for a business case and determine if our model allows illegal states or not.

### TYPE CARDINALITY

Every type we're using has a [cardinality](https://en.wikipedia.org/wiki/Cardinality). It is the number of possible states it allows.  

For example, a `bool` has a cardinality of 2 (`true` or `false`), a `byte` has a cardinality of 2<sup>8</sup> (256 values from 0 to 255) and a `string` has virtually an infinity of values possible (in reality there's a hard limit defined by the available memory).  

As I've mentioned it, we're able to compose these types, so we should be able to compute the number of values possible.  

For a pair of `bool`, we have 4 possible values (`false, false`, `true, false`, `false, true` and `true, true`). If we add a third `bool`, this number double to 8 possibilites. So types like classes or tuples act as mathematical products, they multiply the cardinality of each of their members.  

There is a second category of types that acts as mathematical sums: union types. In F#, they are expressed like this:  

```fsharp
type MyUnionType =
    | Foo of bool
    | Bar
```

With these, we only have one of the possible values at the time. So our value will be either `Foo` that is a `bool` or `Bar` that is a kind of flag`. So our cardinality is then 2 + 1 = 3 possible values.

> Note these sum types are not nativelly supported in C#, even if some tricks exists to approach this behavior ([Oskar Dudycz](https://bsky.app/profile/oskardudycz.bsky.social)
 did a geat job in his [blog post](https://event-driven.io/en/union_types_in_csharp/)).  
>
> Though one limitation remains: the compiler is unaware of the number of possible types: even if the implementation of new types is restricted, there's theorically an infinity of possible extensions of the base type. The only solution I've found and used to avoid this issue is to implement the [visitor pattern](https://refactoring.guru/design-patterns/visitor). But this is an extremely verbose solution.  

- tennis kata
  - rules
  - implementation
  - compiler help
- limitations
  - model can be harder to manipulate
  - not always adapted to consumers (ex: REST API)

---

## COMMENTS

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
