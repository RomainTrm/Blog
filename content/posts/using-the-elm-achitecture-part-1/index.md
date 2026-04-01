---
title: "Using the Elm Achitecture - Part 1"
date: 2026-04-01T09:53:00+02:00
tags: [post, en]
draft: true
---

<!-- - update previous post
- explain MVU architecture
- core concept: a reduction
- introduce elm and elmish and mention redux
- first example: counter app (ignore effects)
- second example: customer page (use effects)
- third example: split customer page (use intents)
- additional resources : https://zaid-ajaj.github.io/the-elmish-book/#/
 -->

In my previous post [Reaching a limit of Reactive Programming](../reaching-a-limit-of-reactive-programming/), I've mentioned the MVU (*Model-View-Update*) pattern could be a solution to the problems I was encountering with reactive programming. I'm now using MVU since a few months on other projects and I want to write my own article where I present it in more details.  

In this blog post, I'll assume we're working on a webpage, but this pattern can be use in other contexts like desktop applications.

## *Model-View-Update* pattern: core concepts

The simplest way to represent the *MVU* pattern is a loop between the three elements:  

- A *Model* that represent the state of the webpage.
- A *View* function that takes the *Model* as an input to render the page.
- A *Update* function that applies a command to the current *Model* and returns an updated version of it.

```goat
+-------+                 +-------+                               
| Model +---- render ---->+ View  |
+---+---+      view       +---+---+
    ^                         |
  update    +--------+      send
  model ----+ Update +<-- commands
            +--------+
```

The whole pattern lies on a reduction using the *Update* function: `(Command, Model) -> Model`. If you are not familiar with the concept of reduction, just think about a loop that sequentially applies each command to the model then send the result to the *View* function. This is the glue between our two functions.  

## Existing technologies

One of the most known implementation of this pattern is [React Redux](https://react-redux.js.org/). Even if I've never used it, I've easily recognized the pattern simply by reading the tutorial in the official website.  

The other main implementation of *MVU* is [Elm](https://elm-lang.org/) and his famous [Elm architecture](https://guide.elm-lang.org/architecture/). If you never played with it, I think you should give it a try, the Elm's compiler is realy didactic. This artchitecture has also been recoded with the [Elmish](https://github.com/elmish/elmish) project that transpile F# code to a React application.  

For the rest of this post, I will reproduce this second implementation.  

## Recoding the Elm architecture with Typescript and PReact

For this implementation, I use [PReact](https://preactjs.com/) to render my web application. As you will see later, except for rendering and the main component, there is almost no dependency to the framework, meaning it should be easy to replace with something else.  

> All the following code is available in my [github repository](https://github.com/RomainTrm/Sandbox-Elmish-PReact/blob/main/src/elmish.tsx).  
> It is very similar to the [F# implementation](https://github.com/elmish/elmish/blob/v5.x/src/program.fs) of Elmish.

First, we have to code an PReact component to execute our *MVU* architecture. Let's define the properties to pass to our component:  

```typescript
// elmish.tsx
type Dispatch<TCommand> = (cmd: TCommand) => void
type ElmishViewProps<TModel, TCommand, TEffect> = {
    init: { model: TModel, effects: TEffect[] }
    update: (cmd: TCommand, model: TModel) => { model: TModel, effects: TEffect[] }
    view: (model: TModel, dispatch: Dispatch<TCommand>) => VNode
    executeEffect: (effect: TEffect, dispatch: Dispatch<TCommand>) => Promise<void>
}
```

We already recognize our `update` and `view` functions. The `Dispatch<TCommand>` dependency passed as parameter of the `view` is simply a function that allows the *View* to send commands to the *Update* function.  
I haven't mentioned effects so far. To keep things simple, let just ignore them for now and we'll get back to that later.  
Finally, we also pass as parameter an inital state for initializing our page.  

> To keep code example simple and avoid distractions, I've removed some parts of the code that are mainly related to PReact's lifecycle and concurent execution constraints. Once again, check my [repository](https://github.com/RomainTrm/Sandbox-Elmish-PReact/blob/main/src/elmish.tsx) for full code.  

Now we can define our PReact component:  

```typescript
// elmish.tsx
export class ElmishView<TModel, TCommand, TEffect> 
    extends Component<ElmishViewProps<TModel, TCommand, TEffect>, TModel> 
{
    private readonly program: Program<TModel, TCommand, TEffect>;
    private dispatcher: Dispatch<TCommand> = //...;

    constructor(props: ElmishViewProps<TModel, TCommand, TEffect>) {
        super(props);
        this.program = {
            init: props.init,
            update: props.update,
            executeEffect: props.executeEffect,
            // ...
        }
    }

    override componentDidMount() : void {
        runWithDispatch(this.program)
    }

    // ...

    override render() : VNode {
        return this.props.view(this.state, this.dispatcher)
    }
}
```

## Additional resources

If you liked this content and you are looking for a more detailed resources to implement this pattern on your own, I recommand you reading the [Elmish book](https://zaid-ajaj.github.io/the-elmish-book/) wrote by [Zaid Ajaj](https://github.com/Zaid-Ajaj).

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
