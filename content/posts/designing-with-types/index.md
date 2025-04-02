---
title: "DESIGNING WITH TYPES"
date: 2025-04-02T09:00:00+02:00
tags: [post, en]
draft: false
---

When writing software, we're using types to represent the information we're manipulating. Values are express through primitive types like `bool`, `int` or `string`. We're building complex data representation by composing these types. This composition is done by defining our own types, usually with classes or tuples.  

Even if there are several ways to store and represent the same information, these ways are not all equivalent. Some are too permissive and allows states that should be considered as illegal regarding our business rules. Others are well defined and only represent legal states, meaning we don't have to write code to defend ourselves against malformed data.  

Did you know there's a mathematical way to compute the number of possible states of your model? We can then compare this number to the number of possible combinations for a business case and determine if our model allows illegal states or not.

## TYPE'S CARDINALITY: THE NUMBER OF POSSIBLE STATES

Every type we're using has a [cardinality](https://en.wikipedia.org/wiki/Cardinality). This is the number of possible states it allows.  

For example, a `bool` has a cardinality of 2 (`true` or `false`), a `byte` has a cardinality of 2<sup>8</sup> (256 values from 0 to 255) and a `string` has virtually an infinity of values possible (in reality there's a hard limit defined by the available memory).  

As I've mentioned it, we're able to compose these types, so we should be able to compute the number of values possible.  

For a pair of `bool`, we have 4 possible values (`false, false`, `true, false`, `false, true` and `true, true`). If we add a third `bool`, this number doubles to 8 possibilities. So types like classes or tuples act as mathematical products, they multiply the cardinality of each of their members.  

There is a second category of types that acts as mathematical sums: union types. In F#, they are expressed like this:  

```fsharp
type MyUnionType =
    | Foo of bool
    | Bar
```

With these, we only have one of the possible values at the time. So our value will be either `Foo` that is a `bool` or `Bar` that is a kind of constant. So our cardinality is then 2 + 1 = 3 possible values.

> Note these sum types are not natively supported in C#, even if some tricks exist to approach this behavior. [Oskar Dudycz](https://bsky.app/profile/oskardudycz.bsky.social) did a great job in his [blog post](https://event-driven.io/en/union_types_in_csharp/) explaining those.  
>
> Though one limitation remains: the compiler is unaware of the number of possible types. Even if the implementation of new types is restricted, there's theoretically an infinity of possible extensions of the base type. The only solution I've found and used to avoid this issue is to implement the [visitor pattern](https://refactoring.guru/design-patterns/visitor). But this is an extremely verbose solution.  

We're now able to compute the number of possible states for our model.

## USE CASE: THE TENNIS KATA

We will use the [Tennis Kata](https://sammancoaching.org/kata_descriptions/tennis.html) to highlight the use of the cardinality. We will compute the total number of states of our model and ensure we can't produce an invalid state. Our goal is to compute the new score of a "game".  

### THE RULES TO IMPLEMENT

There are two players: the "serving player" and his opponent. Each mark points: "love" (0 points), 15, 30 and then 40 points.  

A score is expressed as a pair of points, like "15-30" or "40-love". By convention, the point of the serving player is always the first.  

To win the game, a player must mark a new point when his score is 40. In the case of a "deuce" (40-40), the player who marks gets an "advantage", if he marks again then he wins, otherwise the score goes back to "deuce".

### THE NUMBER OF POSSIBLE STATES

Now let's compute the number of possible states. We've got two players, each of them can have 4 possible points values ("love", 15, 30, 40). Each player can gain the advantage (2 possible values) and win the game (2 possible values).  

So our cardinality is:  

```text
4 points * 4 points + 2 combinations of advantage + 2 combinations of games won
4 * 4 + 2 + 2
20
```

Our model should have a cardinality of 20 possible states.

### NAIVE IMPLEMENTATION

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

### CORRECT IMPLEMENTATION

Let try another model.

```fsharp
type Player = Serving | Opponent
type Point = Love | Fifteen | Thrity | Forty
type Score =
    | Points of Point * Point
    | Advantage of Player
    | Game of Player
```

Our model has a better look new, and we're even finding some of our business semantic. Let's compute our cardinality.  

```text
type Player = 2 
type Point = 4
type Score =
    | Points = 4 * 4
    | Advantage = 2
    | Game = 2

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
    | Points (pointServingPlayer, pointOpponent), Serving -> 
        Points (addPoint pointServingPlayer, pointOpponent)
    | Points (pointServingPlayer, pointOpponent), Opponent -> 
        Points (pointServingPlayer, addPoint pointOpponent)
    | Game player, _ -> Game player
```

The complete code for this implementation is available [here](tennis-kata-solution-1.fsx).

If you're unfamiliar with F#, this is basically a function that takes the current score and the player who won the point, and it returns the new score. The `match ... with` syntax is kinda like a `switch` that will return the result associated with the first pattern it matches. For example, for our pair `currentScore, pointTo`, if we match the pattern `Points (Forty, _), Serving`, this means the serving player won the point and already had 40 points. Opponent score is not checked (we used the `_` wildcard) and can be of any value except 40 because it should have been caught by the previous pattern `Points (Forty, Forty), _`.

This model is rather good. If we treat our function `computeScore` as a black box, every possible input will have an associated output, there is no unexpected side effect and the code is deterministic. However, if we look closely at the implementation, there are some tensions in our algorithm.  

Indeed, the value `Forty` seems to be special as we have to check for it in our main `match`. It also forces the order of the patterns to check. In addition, our inner `addPoint` function can return `Forty` but never check for it as an input. This is something the compiler warns us about:  

```text
tennis-kata-solution-1.fsx(10,15): warning FS0025: Incomplete pattern matches on this expression.
For example, the value 'Forty' may indicate a case not covered by the pattern(s).
```

> Tip: This ability to detect incomplete `match` and to suggest missing cases is a cool feature from the F# compiler. In the company I'm working at, we turned this warning as a compilation error for `release` builds to avoid runtime errors in our production environment. Keeping it as a warning for `debug` builds is more convenient while developing features.

We could keep this model and eliminate this warning by removing the `addPoint` function and getting a more exhaustive pattern in the main `match`. Though, the `Forty` value remains a special case.  

### FINAL IMPLEMENTATION

Another possibility is to remove `Forty` from our `Point` type and introduce new cases to the `Score` type. We have to make a difference between the "40-40" aka "Deuce" and the other combinations. Our new model is now:  

```fsharp
type Player = Serving | Opponent
type Point = Love | Fifteen | Thrity
type Score =
    | Points of Point * Point
    | Forty of Player * Point
    | Deuce
    | Advantage of Player
    | Game of Player
```

In the case of the `Forty`, the `Player` represents the one who has 40 points, the `Point` represents the points of the other player.

Once again, let's compute our cardinality.  

```text
type Player = 2 
type Point = 3
type Score =
    | Points = 3 * 3
    | Forty = 2 * 3
    | Deuce = 1
    | Advantage = 2
    | Game = 2

type Score = 3 * 3 + 2 * 3 + 1 + 2 + 2
type Score = 9 + 6 + 1 + 2 + 2
type Score = 20
```

Great! Our cardinality remains correct, we can now use it and it will drive the code implementation.  

First, our `addPoint` function cannot remain like this: it was taking a `Point` and returned a `Point`. Now, if we try to increase `Thrity`, we have to return the type (not the value) `Forty`. So our function must evolve in order to return a `Score` instead of a `Point`, this means we need points of both players as input. If the serving player marked, the function looks like:  

```fsharp
let addPointToServingPlayer pointServingPlayer pointOpponent = 
    match pointServingPlayer with
    | Love -> Points (Fifteen, pointOpponent)
    | Fifteen -> Points (Thrity, pointOpponent)
    | Thrity -> Forty (Serving, pointOpponent)
```

We add another function to handle the case when the opponent player won the point:

```fsharp
let addPointToOpponentPlayer pointServingPlayer pointOpponent = 
    match pointOpponent with
    | Love -> Points (pointServingPlayer, Fifteen)
    | Fifteen -> Points (pointServingPlayer, Thrity)
    | Thrity -> Forty (Opponent, pointServingPlayer)
```

Then, we now have to handle the `Forty` type. Once again, we're not only incrementing the opponent's `Point` value: if the value is `Thrity`, then next score will be `Deuce`. Let's add a dedicated function:  

```fsharp
let addPointToOtherPlayer fortyPlayer otherPlayerPoint = 
    match otherPlayerPoint with
    | Love -> Forty (fortyPlayer, Fifteen)
    | Fifteen -> Forty (fortyPlayer, Thrity)
    | Thrity -> Deuce
```

Finally, we now have to handle our new types `Forty` and `Deuce` in our main `match` and use our new functions:

```fsharp
let computeScore (currentScore: Score) (pointTo: Player) =
    match currentScore, pointTo with
    | Points (pointServingPlayer, pointOpponent), Serving -> 
        addPointToServingPlayer pointServingPlayer pointOpponent
    | Points (pointServingPlayer, pointOpponent), Opponent ->  
        addPointToOpponentPlayer pointServingPlayer pointOpponent
    | Forty (player, _), _ when player = pointTo -> Game player
    | Forty (player, otherPlayerPoints), _ -> 
        addPointToOtherPlayer player otherPlayerPoints
    | Deuce, _ -> Advantage pointTo
    | Advantage player, _ when player = pointTo -> Game player
    | Advantage _, _ -> Deuce
    | Game player, _ -> Game player
```

The complete code for this implementation is available [here](tennis-kata-solution-2.fsx).

## COMPARE INFINITES

Right now, you may think: "That's great, but I can't use it for my software. I'm using strings and large numbers. The cardinality will always be infinite." And you're right!  

Keep in mind that computing cardinality is useful for an algorithm. There is just no point to compute this value for our whole software. Instead, we have to consider only relevant values in our data. The tennis kata is somehow a special case: the whole model is used by our `computeScore` function to make a decision. But most of the time, a model carries a lot of data, and only a fraction of it is used by the algorithm.  

So, to avoid this infinite issue, I use two tricks to reduce the cardinality to a manageable number:  

First, I replace the unused values by our algorithm with a cardinality of 1. Here's an example:

```fsharp
type MyType = {
    ValueA: bool
    ValueB: string
}

let myFunction (x: MyType) = x.ValueA
```

Only the `ValueA` is used, `ValueB` has no impact on our function. So we can compute the cardinality of `MyType` for `myFunction` like this:  

```text
type MyType = {
    ValueA: bool
    ValueB: string // not used by myFunction
}

ValueA = 2
ValueB = 1
type MyType = 2 * 1 = 2
```

Second trick, values can be compared to fulfill some conditions. A typical example is the comparison of IDs, by definition these types have a large cardinality as they usually use types like `int`, `string` or `UUID`. But at the end of the day it is the equality that matter. In such cases, we can reduce the cardinality to two cases: it is either the expected value or not.

```fsharp
type MyType = {
    Id: int
    ValueA: bool
    ValueB: string
}

let isTheExpectedValue (expectedId: int) (x: MyType) = 
    x.Id = expectedId
```

If we consider `expectedId`, the value to match, as some kind of constant, then we can reduce the cardinality of `Id` to two cases: `Equals | NotEquals`.  

```text
type MyType = {
    Id: int // compare to expectedId
    ValueA: bool // not used
    ValueB: string // not used
}

Id = Equals | NotEquals = 2
ValueA = 1
ValueB = 1
type MyType = 2 * 1 * 1 = 2
```

## CONCLUSION

In this post, we've learned how to compute the number of possible states for our model. Then we saw how it can drive the implementation. Finally, I've suggested some tricks to help us compute cardinality for models that are embedding types with a large number of possible values.  

Aiming to tackle illegal states is great, though it can also introduce complexity to the code:  

- We haven't really eliminated illegal states, we've just pushed them out of the domain. This means complex models will also be complex to build from external inputs. We'll have to parse and validate inputs, then map to our domain model.
- Sometimes, the model isn't at the correct level of abstraction for some treatments. For example, to access a value, we may find it in several places; Each time we need a new value, we have to write a new function that goes across the whole model. If you want to experience this, with our latest model try to write a function to retrieve the points of the serving player and then another for his opponent.  

To conclude, type cardinality helps us gain confidence in our model and identify unhandled use cases. If you have an opportunity to remove illegal states, go for it. But keep in mind that allowing some illegal states in order to simplify the development and model manipulation is OK as long as these cases are properly identified and handled.

---

## COMMENTS

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
