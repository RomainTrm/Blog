---
title: "Learning Elixir"
date: 2025-06-20T09:00:00+02:00
tags: [post, en]
draft: true
---

In a previous [post](../using-processes-for-better-resilience/), I've mentioned I've read the book [Programming Elixir 1.6](https://pragprog.com/titles/elixir16/programming-elixir-1-6/).  

As I was planning to read [Real-World Event Sourcing](https://pragprog.com/titles/khpes/real-world-event-sourcing/), I chose to conduct a second reading because my limited knowledge and skills with Elixir were eroded over time. I took time to take many notes which are available on a dedicated [repository](https://github.com/RomainTrm/Book-ElixirExercices).  

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

Elixir also benefits from [`mix`](https://hexdocs.pm/mix/1.12/Mix.html), a complete build tool (like `npm` or `dotnet`) that provides project creation, compiling, testing, dependencies management and many more capabilities.

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
iex> [c, c] = [1, 1] # We expect a list of two elements that are equal
[1, 1]
iex> [c, c] = [1, 2] # Value "c" cannot be equal to 1 and 2 at the same time
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

We also have some tooling to check the validity of our code: A compiler that is able to spot missing or invalid function signatures and some static analysis tools like [dialyzer](https://www.erlang.org/doc/apps/dialyzer/dialyzer.html). These analyzers use annotations added to the code by developers to check consistency across functions declarations and invocations (note these annotations are optional):  

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

Atoms are constants that only carries their name as information, for example `:ok`, `:apple` or `:"my atom"`.  
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

Such property enables metaprogramming like macros, allowing developers to (re)define code, functions and behaviors at runtime.  

## The Erlang VM

Elixir runs on the Erlang VM and benefits from these capabilities.  

### Actor model

First of all, this environment is fully designed to be used with the [actor model](https://en.wikipedia.org/wiki/Actor_model). With this pattern, we use processes as the basic building bloc. A process run some code logic, store an internal state, send messages to other processes to communicate, can spawn new processes, etc.  

As an example, the following code implements an in-memory stack. Once initiated, it waits for messages to add or return values. Internal state is stored by using recursion on the `loop` function.

```elixir
defmodule Stack do
  def init() do
    loop([])
  end

  def loop(values) do
    receive do
      :pop ->
        case values do
          [] ->
            IO.puts("Stack is empty")
            loop(values)
          [head|tail] ->
            IO.puts("Pop: #{head}")
            loop(tail)
        end
      {:push, value} ->
        loop([value|values])
    end
  end
end
```

We spawn a `Stack` instance in a dedicated process (here `#PID<0.110.0>`) then we send messages:

```elixir
iex> stack = spawn(fn -> Stack.init() end)
#PID<0.110.0>
iex> send(stack, :pop)
Stack is empty
:pop
iex> send(stack, {:push, 1})
{:stack, 1}
iex> send(stack, {:push, 2})
{:stack, 2}
iex> send(stack, {:push, 3})
{:stack, 3}
iex> send(stack, :pop)
Pop: 3
:pop
iex> send(stack, :pop)
Pop: 2
:pop
```

Even though we can manipulate these processes with few functions from Elixir, this example is a "low-level" implementation. Processes are rarely managed this way.

Most of the use cases seems to be handled with modules [`Task`](https://hexdocs.pm/elixir/1.12/Task.html) and [`Agent`](https://hexdocs.pm/elixir/1.12/Agent.html) or with high-level libraries like [`GenServer`](https://hexdocs.pm/elixir/1.12/GenServer.html) or [`GenStage`](https://hexdocs.pm/gen_stage/GenStage.html). For the following code examples, I will use `GenServer`.

#### Server

With [`GenServer`](https://hexdocs.pm/elixir/GenServer.html), the main building bloc is a server. This is a process dedicated to a specific work. The following example is the same `Stack` as before:

```elixir
defmodule Stack do
  use GenServer

  ### Public API
  def start_link do
    initial_stack = []
    GenServer.start_link(__MODULE__, initial_stack, name: __MODULE__)
  end

  def push(item) do
    GenServer.cast(__MODULE__, {:push, item})
  end

  def pop do
    response = GenServer.call(__MODULE__, :pop)
    IO.puts(response)
  end

  ### Process logic
  def init(initial_stack) do
    {:ok, initial_stack}
  end

  def handle_cast({:push, item}, stack_content) do
    {:noreply, [item|stack_content]}
  end

  def handle_call(:pop, _from, []) do
    {:reply, "Stack is empty", []}
  end

  def handle_call(:pop, _from, [head|tail]) do
    {:reply, "Pop: #{head}", tail}
  end
end
```

Behavior remains the same. However we don't have to specify the `PID` as it is managed by the public api (the `:name` parameter of the `start_link` function). With this specific implementation, we can only instantiate one `Stack` process.

```elixir
iex> Stack.start_link
{:ok, #PID<0.110.0>}
iex> Stack.start_link
{:error, {:already_started, #PID<0.110.0>}}
iex> Stack.pop
Stack is empty
:ok
iex> Stack.push(1)
:ok
iex> Stack.push(2)
:ok
iex> Stack.push(3)
:ok
iex> Stack.pop
Pop: 3
:ok
iex> Stack.pop
Pop: 2
:ok
```

Though, this example needs some explanations:  

The code is split in two sections `Public API` and `Process logic`. `Public API` exposes all the available behaviors to the outside world and send messages to the `Stack` process using `GenServer`. `Process logic` implement `GenServer`'s handlers with business logic and state management.

First we spawn a new process with the public api `GenServer.start_link`, it calls the `init` function that only returns to `GenServer` the initial state of our process.  

Then we have the `push` function that pushes a new item into our `Stack` by using the `GenServer.cast` function. This sends a message to our process without expecting a response, `handle_cast` is an asynchronous operation.  

Finally, we have the `pop` function that runs synchronously with the `GenServer.call` function. This also sends a message to the process but waits for a response. Note we're using pattern matching for our `handle_call(:pop, _from, ...` as our behavior depends on the state of the process.

#### No concurrency

You may wonder what happens if our `Stack` is called by multiple processes at the same time?  
I think it's worth mentioning that concurrency is handled by design with this messaging system. Every process has their own mailbox, processing one message at the time.

#### Supervisor

Sometimes, though, processes face (un)recoverable errors and crashes. When this happens, we have to decide how to handle this situation: Should we restart the process or not? When restarting, should we only restart the crashed process or should we also include some linked processes? Once restarted, should we resume the last known state or have a new state?  

These responsibilities belong to a specific kind of process called supervisors (see [`Supervisor`](https://hexdocs.pm/elixir/Supervisor.html)). Thanks to them, we are able to build resilient/self-healing systems.

### Nodes

The Erlang VM allows connecting several nodes in a fairly easy way, providing a high level of abstraction in the code. Each node is a running Erlang VM instance, these can be running and connected from the same hardware or over a network.  
This is a great feature as this allows high scalability and load balancing across several instances.

As an example, in the following code we connect two nodes together, then from one node we launch some code execution on the second node:  

```elixir
# Window 1
...> iex --sname one
iex(one@machine-name)>

# Window 2
...> iex --sname two
iex(two@machine-name)> Node.connect :"one@machine-name"
true

# Window 1
iex(node_one@machine-name)> func = fn -> IO.inspect Node.self end
#Function<43.81571850/0 in :erl_eval.expr/6>
# => Prints information about the Node that runs the function

iex(node_one@machine-name)> spawn(func)
:"node_one@machine-name"
# => Runs on node one

iex(node_one@machine-name)> Node.spawn :"node_one@machine-name", func
:"node_one@machine-name"
#PID<0.116.0>
# => Runs on node one

iex(node_one@machine-name)> Node.spawn :"node_two@machine-name", func
:"node_two@machine-name"
#PID<13771.116.0>
# => Runs on node two
# Note 1: first field of the return PID isn't zero, meaning we are not running the code on the local node
# Note 2: As `func` has been defined on node one, it uses IO of node one to print information
```

### Hot-upgrades

I have already mentioned it, the Erlang VM provides the hot-upgrade capability. This means developers can update code behaviors and data structure stored in the memory without stopping the application.  

To do so, there are some nice and simple APIs:  

```elixir
defmodule MyModule do
  # ...

  # Code for updating state's data structure of the process
  def code_change(_old_vsn, value, _extra) do
    {:ok, [value]}
  end
end
```

I will not go too deep in this topic, I think this is a nice feature but maybe not as useful as it may sound now that we're used to run our applications with several instances and to perform rolling updates.  
Furthermore, it seems these upgrades were supported by `mix` with the help of some packages, but it doesn't seem to work anymore with the latest versions of OTP without some manual configuration from the developers.  

Despite my best efforts, I didn't manage to achieve one of these updates successfully with [`distillery`](https://hexdocs.pm/distillery/home.html) or [`castle`](https://hex.pm/packages/castle). But I am pretty sure these tools work and the issue comes from my environment and/or my skills.

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
