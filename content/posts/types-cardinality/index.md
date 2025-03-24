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

We're now able to compute the number of possible states for our model.

### USE CASE: THE TENNIS KATA

We will use the [Tennis Kata](https://sammancoaching.org/kata_descriptions/tennis.html) to highlight the use of the cardinality. We will compute the total number of states of our model and ensure we can't produce an invalid state. Our goal is to compute the new score of a "game".  

#### THE RULES TO IMPLEMENT

There are two players: the "serving player" and his opponent. Each of them mark points: "love" (0 point), 15, 30 and then 40 points.  

A score is expressed as a pair of points, like "15-30" or "40-love". By convention, the points of the serving player is always the first.  

To win the game, a player must mark a new point when his score is 40. In the case of a "deuce" (40-40), the player who marks get an "advantage", if he marks again then he wins, otherwise the score goes back to "deuce".

#### THE NUMBER OF POSSIBLE STATES

Now let's compute the number of possible states. We've got two players, each of them can have 4 possible points values ("love", 15, 30, 40). Each player can gain the advantage (2 possible values) and win the game (2 possible values).  

So our cardinality is:  

```text
4 points * 4 points + 2 combinations of advantage + 2 combinations of game won
4 * 4 + 2 + 2
20
```

Our model should have a cardinality of 20 possibles states.

#### IMPLEMENTATION 1

The naive (and tempting?) way to implement the score is a pair of numbers (let says `byte`).  

```fsharp
type Score = byte * byte
```

So our cardinality is:

```text
byte * byte
256 * 256
65536
```

Few illegal states here...

#### IMPLEMENTATION 2

Let try another model.

```fsharp
type Player = Serving | Opponent
type Point = Love | Fifteen | Thrity | Forty
type Score =
    | Points of Point * Point
    | Advantage of Player
    | Game of Player
```

Our model has a better look new, and we're even finding some of our business lexic. Let's compute our cardinality.  

```text
type Player = 2 
type Point = 4
type Score =
    | Points of 4 * 4
    | Advantage of 2
    | Game of 2

type Score = 4 * 4 + 2 + 2
type Score = 20
```

Our cardinality is correct, we can't produce illegal states. If we implement our rules, it looks like this:  

```fsharp
let computeScore (currentScore: Score) (pointTo: Player) =
    let addPoint point = 
        match point with
        | Love -> Fifteen
        | Fifteen -> Thrity
        | Thrity -> Forty

    match currentScore, pointTo with
    | Advantage player, _ when player = pointTo -> Game player
    | Advantage _, _ -> Points (Forty, Forty)
    | Points (Forty, Forty), _ -> Advantage pointTo
    | Points (Forty, _), Serving -> Game Serving
    | Points (_, Forty), Opponent -> Game Opponent
    | Points (pointServingPlayer, pointOpponent), Serving -> Points (addPoint pointServingPlayer, pointOpponent)
    | Points (pointServingPlayer, pointOpponent), Opponent -> Points (pointServingPlayer, addPoint pointOpponent)
    | Game player, _ -> Game player
```

Complete code for this implementation is available [here](tennis-kata-solution-1.fsx).

If you're unfamilliar with F#, this is basically a function that takes the current score and the player who won the point, and it returns the new score. The `match ... with` syntax is kinda like a `switch` that will return the result associated to the first pattern it matches. For example, for our pair `currentScore, pointTo`, if we match the pattern `Points (Forty, _), Serving`, this means the serving player won the point and already had 40 points. Opponent score is not checked (we used the `_` wildcard) and can be any value except 40 because it should have be caught by the previous pattern `Points (Forty, Forty), _`.

This model is rather good. If we treat our function `computeScore` as a black box, every possible input will have an associated output, there is no unexpected side-effect and the code is deterministic. However, if we look closely at the implementation, there is some tensions in our algorithm.  

Indeed, the value `Forty` seems to be special as we have to check for it in our main `match`. On the opposit, our inner `addPoint` function can return `Forty` but never check for it as an input. Even more, this is something the compiler is warning us about:  

```text
tennis-kata-solution-1.fsx(10,15): warning FS0025: Incomplete pattern matches on this expression.
For example, the value 'Forty' may indicate a case not covered by the pattern(s).
```

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
