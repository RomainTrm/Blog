---
title: "Learning Elixir"
date: 2025-06-17T13:17:56+02:00
tags: [post, en]
draft: true
---

In a previous [post](../using-processes-for-better-resilience/), I've mentioned I've read the book [Programming Elixir 1.6](https://pragprog.com/titles/elixir16/programming-elixir-1-6/).  

Recently, because I was planning to read [Real-World Event Sourcing](https://pragprog.com/titles/khpes/real-world-event-sourcing/), I chose to conduct a second reading because my limited knowledge and skills with Elixir were eroded over time. I took time to take many notes which are available on a dedicated [repository](https://github.com/RomainTrm/Book-ElixirExercices).  

In this post, I want to present to you the Elixir language, the Erlang VM and give you a taste of why you should learn it even if you will not use it in production.

## Elixir lang

Elixir is a functional language that runs on the Erlang VM, it has been created by [José Valim](https://bsky.app/profile/josevalim.bsky.social). The syntax is highly inspired by the Ruby syntax. With Elixir comes [IEx](https://hexdocs.pm/iex/1.12/IEx.html), a REPL that highly facilitate exploration and experimentation.  

```elixir
$ iex
Erlang/OTP 27 [erts-15.2.5] [source] [64-bit] [smp:8:8] [ds:8:8:10] [async-threads:1] [jit:ns]

Interactive Elixir (1.18.3) - press Ctrl+C to exit (type h() ENTER for help)
iex(1)> IO.puts "Hello Elixir"
Hello Elixir
:ok
```

It also provides access to APIs' documentation.

```elixir
iex(2)> h(Enum.map)

                            def map(enumerable, fun)

  @spec map(t(), (element() -> any())) :: list()

Returns a list where each element is the result of invoking fun on each
corresponding element of enumerable.

For maps, the function expects a key-value tuple.

## Examples

    iex> Enum.map([1, 2, 3], fn x -> x * 2 end)
    [2, 4, 6]

    iex> Enum.map([a: 1, b: 2], fn {k, v} -> {k, -v} end)
    [a: -1, b: -2]
```

Elixir also benefits from [mix](https://hexdocs.pm/mix/1.12/Mix.html), a complete build tool (like `npm` or `dotnet`) that provides project creation, compiling, testing, dependencies management and many more capabilities.

### A functional langage

With this language, everything is patterns and expressions, no mutation is involved.  
Nothing special here for a seasoned F# developer like me. But if you're unfamiliar with pattern matching, we reason in terms of equality instead of affectation:  

```elixir
iex> a = 1 # We declare a value "a" which value is equal to 1 
1
iex> a + 2
3
iex> 1 = a
1
iex> 2 = a
** (MatchError) no match of right hand side value: 1
iex> [1, b, 3] = [1, 2, 3] # We declare a value "b" wich value is equal to the second element of the three elements list
[1, 2, 3]
iex> b
2
iex> [c, c] = [1, 1] # We expect a list of two elements that are equals
[1, 1]
iex> [c, c] = [1, 2]
** (MatchError) no match of right hand side value: [1, 2]
```

Pattern matching is also available for functions signatures:  

```elixir
# A function `nextState` defined with 3 distinct patterns
def nextState(_current, 3), do: :alive
def nextState(:alive, 2), do: :alive
def nextState(_current, _nb_living_surrounding), do: :dead
```

As a functional language, Elixir isn't as strict as some languages like Haskell (sorry for the functional purists) but it's designed in a way that incites you to write code in a functional way: Mostly pure functions (some IO is still possible). As I'm writing this, I realize I don't know how to mutate an existing value and I don't even know if this is something possible. However we have the ability to store states with OTP (more on this later).

### Type system

Elixir is a dynamically typed language. This is a voluntary choice made by José, and it makes perfect sense: As Elixir is running on the Erlang VM, it provides the hot-upgrade capability. Because of this, it would be impossible for an old piece of code to interact at runtime with an upgraded code that defines new unknown types.

At runtime, though, we have many types and structures available to represent our data: primitives, lists, maps (dictionaries), structs, etc. This has the consequence of forcing developers to be very explicit about the data structures they are handling, which leads to verbose code in my opinion (unless we declare specific [structure definitions](https://hexdocs.pm/elixir/main/structs.html)).  

We also have some tooling to check the validity of our code: A compiler that is able to spot missing or invalid function signatures and a static analysis tools like [dialyzer](https://www.erlang.org/doc/apps/dialyzer/dialyzer.html). These analyzers use annotations added to the code by developers to check consistency across functions declarations and invocations (note these annotations are optional):  

```elixir
defmodule Simple do
  @type atom_list :: list(atom)
  @spec count_atoms(atom_list) :: non_neg_integer
  def count_atoms(list) do
    length list
  end
end
```

More details on the typespecs [documentation](https://hexdocs.pm/elixir/typespecs.html).

#### Atoms

I would like to focus on a specific kind of data that exist on the Erlang VM: Atoms.  

Atoms are constants that only carries their name as information, for example `:apple` or `:ok`.  
This might sound kinda useless but they appear to be extremely powerful. For instance, they are constantly used with pattern matching:  

```elixir
def handle_event({:order_created, data}) do
  # do something...
end

def handle_event({:order_issued, data}) do
  # do something else...
end
```

In a dynamically typed environment, they're also a good way to communicate across an evolving codebase: We can compile two codebases and then connect them at runtime, they will be able to communicate as long as they use the same atoms.  

Anyway, under the hood, almost everything is implemented using atoms in the Erlang VM.

### Homoiconicity

As a final point about Elixir itself, I want to highlight that it is an homoiconic language. You may never have heard of this property as almost all our modern languages do not have it.  
Here's a definition: A language is homoiconic if a program written in it can be manipulated as data using the language. The most famous homoiconic language is probably Lisp.  

With Elixir, a simple expression like `1 + 2` can be decomposed in as an [abstract syntax tree (AST)](https://en.wikipedia.org/wiki/Abstract_syntax_tree) and then interpreted:

```elixir
iex> quote do: 1 + 2
{:+, [context: Elixir, imports: [{1, Kernel}, {2, Kernel}]], [1, 2]} 
#{function name, metadata, arguments list}
iex> Code.eval_quoted {:+, [], [1, 2]}
{3, []}
```

With such property, it enables metaprogramming like macros, allowing developers to (re)define code, functions and behaviors at runtime.  

## The Erlang VM



- langage:  
  - ruby syntax
  - REPL
  - FP
  - types (dynamic typing & static analysis)
  - atoms
  - homoconoicity
- BEAM:  
  - nodes
  - actor model
  - hot-upgrade
- OTP:
  - Server
  - Supervisor
  - no concurrency (process isolation)

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
