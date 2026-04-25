---
title: "Using the Elm Architecture - Part 4: View composition"
date: 2026-05-13T09:00:00+02:00
tags: [post, en]
draft: true
---

*This blog post is the last of a series where we're using The Elm Architecture (TEA). If you haven't, I strongly recommend reading the previous articles first.*

So far we've learned how to [build an application](/posts/using-the-elm-architecture-part-2) and [run side-effects](/posts/using-the-elm-architecture-part-3). But as our programs grow, we may feel the need to break things down. Today we'll see how to split our page in different modules. This can be for isolating some logic and reduce cognitive load, favor views composition or allow reusability.  

I will use the example from the previous post, our goal is to extract the edit form into a dedicated component.

> The following code is available in my [github repository](https://github.com/RomainTrm/Sandbox-Elmish-PReact/tree/main/src/customer-v2).

## Existing codebase

Before starting refactoring, let's see the existing codebase. First the logic:  

```typescript
// customer/customer.app.ts
export type Model = {
    customerId: CustomerId
    loading: boolean
    error: string | null
    customer: CustomerDto | null
    customerEdition: CustomerDto | null
}

export type Command = 
    | { kind: "NotifyCustomerLoaded", customer: CustomerDto }
    | { kind: "NotifyLoadingError", error: string }
    | { kind: "EditCustomer" }
    | { kind: "UpdatePremiumSubscription", value: boolean }
    | { kind: "SaveCustomer" }
    | { kind: "CancelEdit" }
    | { kind: "NotifySaveSucceeded" }
    | { kind: "NotifySaveFailed", error: string }

export type Effect = 
    | { kind: "LoadCustomer", customerId: CustomerId }
    | { kind: "SaveCustomer", customer: CustomerDto }

export function init(customerId: CustomerId) : { model: Model, effects: Effect[] } {
    return {
        model: { 
            customerId: customerId,
            customer: null,
            customerEdition: null,
            error: null,
            loading: true,
        },
        effects: [
            { kind: "LoadCustomer", customerId }
        ],
    }
}

export function update(command: Command, model: Model) : { model: Model, effects: Effect[] } {
    return match(command)
        .returnType<{ model: Model, effects: Effect[] }>()
        .with({ kind: "NotifyCustomerLoaded" }, ({ customer }) => {
            // Irrelevant
        })
        .with({ kind: "NotifyLoadingError" }, ({ error }) => {
            // Irrelevant
        })
        .with({ kind: "EditCustomer" }, () => {
            if (model.customer === null) return { model, effects:[] }

            const newModel: Model = {
                ...model,
                customerEdition: model.customer,
            }
            return { model: newModel, effects: [] }
        })
        .with({ kind: "UpdatePremiumSubscription" }, ({ value }) => {
            if (!model.customerEdition) return { model, effects: [] }

            const newModel: Model = {
                ...model,
                customerEdition: {
                    ...model.customerEdition, 
                    premiumSubscription: value,
                },
            }
            return { model: newModel, effects: [] }
        })
        .with({ kind: "CancelEdit" }, () => {
            const newModel: Model = {
                ...model,
                customerEdition: null,
            }
            return { model: newModel, effects: [] }
        })
        .with({ kind: "SaveCustomer" }, () => {
            if (!model.customerEdition) return { model, effects: [] }

            const newModel: Model = {
                ...model,
                loading: true,
            }
            const effects: Effect[] = [
                { kind: "SaveCustomer", customer: model.customerEdition }
            ]
            return { model: newModel, effects }
        })
        .with({ kind: "NotifySaveSucceeded" }, () => {
            // Irrelevant
        })
        .with({ kind: "NotifySaveFailed" }, ({ error }) => {
            // Irrelevant
        })
        .exhaustive()
}

export function executeEffect(effect: Effect, dispatch: Dispatch<Command>, api: Api) : Promise<void> {
    // Irrelevant
}
```

And the view:

```typescript
// customer/customer.view.tsx
function DisplayCustomerView({ customer, dispatch }: { customer: CustomerDto, dispatch: Dispatch<Command> }) {
    return <>
        <div>Name: {customer.name}</div>
        <div>Premium subscription: {customer.premiumSubscription ? "yes" : "no"}</div>
        <button onClick={_ => dispatch({ kind: "EditCustomer" })}>Edit</button>
    </>
}

function EditCustomerView({ customer, dispatch }: { customer: CustomerDto, dispatch: Dispatch<Command> }) {
    return <>
        <div>Name: {customer.name}</div>
        <div>
            <input id="premiumSubscription"
                type="checkbox"
                checked={customer.premiumSubscription}
                onChange={e => dispatch({ kind: "UpdatePremiumSubscription", value: e.currentTarget.checked })}/>
            <label for="premiumSubscription">
                Premium subscription
            </label>
        </div>
        <div>
            <button onClick={_ => dispatch({ kind: "SaveCustomer" })}>Save</button>
            <button onClick={_ => dispatch({ kind: "CancelEdit"})}>Cancel</button>
        </div>
    </>
}

export function View({ model, dispatch }: { model: Model, dispatch: Dispatch<Command> }) {
    if (model.loading)
        return <>Loading</>

    return <>
        {model.error && <>{`An error occured: ${model.error}`}</>}
        {model.customer && !model.customerEdition && 
            <DisplayCustomerView customer={model.customer} dispatch={dispatch} />
        }
        {model.customerEdition &&
            <EditCustomerView customer={model.customerEdition} dispatch={dispatch} />
        }
    </>
}
```

Some positive stuff here, our `View` and `Model` are already quite decoupled. In the `View`, the edition is already isolated in a dedicated component `EditCustomerView`. The `Model` uses a dedicated `customerEdition: CustomerDto | null` property, this allows us to know when to display the edit form and do whatever we need for an edition without erasing the customer's initial state (stored in the property `customer`). This way, if the user chooses to give up his changes, we simply need to set `customerEdition` to `null`.  

However, the `Command` type is a bit messier as it mixes loading, saving, mode switching and edition commands. In my opinion, three of them belong to our future new component: `UpdatePremiumSubscription`, `SaveCustomer` and `CancelEdit`.

## Start extracting code

We can easily move some code in new files. For the logic, let's just extract types and the `init` function for now:  

```typescript
// customer/edit/edit.app.ts
export type Model = CustomerDto

export type Command = 
    | { kind: "UpdatePremiumSubscription", value: boolean }
    | { kind: "SaveCustomer" }
    | { kind: "CancelEdit" }

export type Effect = never

export function init(customer: CustomerDto) : { model: Model, effects: Effect[] } {
    return {
        model: customer,
        effects: [],
    }
}
```

And we update the parent in consequence:  

```typescript {hl_lines=["2-3",10,17,28,31]}
// customer/customer.app.ts
import type { Command as EditCommand, Model as EditModel } from "./edit/edit.app"
import { init as editFormInit } from "./edit/edit.app"

export type Model = {
    customerId: CustomerId
    loading: boolean
    error: string | null
    customer: CustomerDto | null
    customerEdition: EditModel | null
}

export type Command = 
    | { kind: "NotifyCustomerLoaded", customer: CustomerDto }
    | { kind: "NotifyLoadingError", error: string }
    | { kind: "EditCustomer" }
    | EditCommand
    | { kind: "NotifySaveSucceeded" }
    | { kind: "NotifySaveFailed", error: string }

export function update(command: Command, model: Model) : { model: Model, effects: Effect[] } {
    return match(command)
        .returnType<{ model: Model, effects: Effect[] }>()
        // ...
        .with({ kind: "EditCustomer" }, () => {
            if (model.customer === null) return { model, effects:[] }
            
            const { model: customerEdition } = editFormInit(model.customer)
            const newModel: Model = {
                ...model,
                customerEdition,
            }
            return { model: newModel, effects: [] }
        })
        // ...
        .exhaustive()
}
```

The `EditCustomerView` component can be moved as it is without any other change than referencing our new `Model` and `Command`.  

## Move dedicated logic

Now we would like the extract the logic seating in the `update` function into our new component. Let's declare our new function first:  

```typescript
// customer/edit/edit.app.ts
export function update(command: Command, model: Model) : { model: Model, effects: Effect[] } {
    return match(command)
        .returnType<{ model: Model, effects: Effect[] }>()
        .with({ kind: "UpdatePremiumSubscription" }, ({ value }) => {
            const newModel: Model = {
                ...model,
                premiumSubscription: value,
            }
            return { model: newModel, effects: [] }
        })
        .with({ kind: "CancelEdit" }, () => {
            return { model: model, effects: [] }
        })
        .with({ kind: "SaveCustomer" }, () => {
            return { model: model, effects: [] }
        })
        .exhaustive()
}
```

As we no longer have access to the `Effect` type, `SaveCustomer` and `CancelEdit` do nothing here, but we will need to notify the parent to trigger the save.

> I made the choice to let the save logic inside the parent. In terms of responsibility, this can be challenged but it's a refactoring that is easier to detail in a blog post. Another possibility would have been to also move the save logic with the `Effect` into our new component, and only notify the parent of the result. Though, this implies more communication between the two components.  
> If you want to have a look, I've also coded [this variant](https://github.com/RomainTrm/Sandbox-Elmish-PReact/tree/main/src/customer-v3).  

As we've now isolated the logic of the edit form commands, we must also isolate it in the parent. To do so, we update once again the `Command` type to wrap the command of the edit form:  

```typescript {hl_lines=[3,9,"17-27"]}
// customer/customer.app.ts
import type { Command as EditCommand, Model as EditModel } from "./edit/edit.app"
import { init as editFormInit, update as editFormUpdate } from "./edit/edit.app"

export type Command = 
    | { kind: "NotifyCustomerLoaded", customer: CustomerDto }
    | { kind: "NotifyLoadingError", error: string }
    | { kind: "EditCustomer" }
    | { kind: "EditCommand", subCommand: EditCommand }
    | { kind: "NotifySaveSucceeded" }
    | { kind: "NotifySaveFailed", error: string }

export function update(command: Command, model: Model) : { model: Model, effects: Effect[] } {
    return match(command)
        .returnType<{ model: Model, effects: Effect[] }>()
        // ...
        .with({ kind: "EditCommand" }, ({ subCommand }) => {
            if (!model.customerEdition) return { model, effects: [] }

            const result = editFormUpdate(subCommand, model.customerEdition)
            const newModel: Model = { 
                ...model, 
                customerEdition: result.model,
            }

            return { model: newModel, effects: [] }
        })
        // ...
        .exhaustive()
}
```

We also need to update our `View` as commands received from our `EditCustomerView` must now be wrapped into our new `EditCommand` command:  

```typescript {hl_lines=[14]}
// customer/customer.view.tsx
export function View({ model, dispatch }: { model: Model, dispatch: Dispatch<Command> }) {
    if (model.loading)
        return <>Loading</>

    return <>
        {model.error && <>{`An error occured: ${model.error}`}</>}
        {model.customer && !model.customerEdition && 
            <DisplayCustomerView customer={model.customer} dispatch={dispatch} />
        }
        {model.customerEdition &&
            <EditCustomerView 
                customer={model.customerEdition} 
                dispatch={(cmd) => dispatch({ kind: "EditCommand", subCommand: cmd })} 
            />
        }
    </>
}
```

## How to send signals to the parent component?

Now we have to send to our parent a signal when the edit form process a `SaveCustomer` or a `CancelEdit` command.  

One solution could be to look at the `subCommand` when handling the `EditCommand` and intercept the signal for `SaveCustomer` or `CancelEdit`. Something like:  

```typescript
// customer/customer.app.ts
export function update(command: Command, model: Model) : { model: Model, effects: Effect[] } {
    return match(command)
        .returnType<{ model: Model, effects: Effect[] }>()
        // ...
        .with({ kind: "EditCommand" }, ({ subCommand }) => {
            return match(subCommand)
                .with({ kind: "SaveCustomer" }, () => {
                    if (!model.customerEdition) return { model, effects: [] }
                    const newModel: Model = {
                        ...model,
                        loading: true,
                    }
                    const effects: Effect[] = [
                        { kind: "SaveCustomer", customer: model.customerEdition }
                    ]
                    return { model: newModel, effects }

                })
                .with({ kind: "CancelEdit" }, () => {
                    const newModel: Model = {
                        ...model,
                        customerEdition: null,
                    }
                    return { model: newModel, effects: [] }
                })
                .otherwise(_ => {
                    if (!model.customerEdition) return { model, effects: [] }
                    const result = editFormUpdate(subCommand, model.customerEdition)
                    const newModel: Model = { 
                        ...model, 
                        customerEdition: result.model,
                    }
                    return { model: newModel, effects: [] }
                })
        })
        // ...
        .exhaustive()
}
```

This could work but I've got two issues with this solution:  

- Intercepted commands are never forwarded to the edit form's `update` function, which breaks the contract established by this architecture.
- The parent is coupled to the child's `Command` type whereas this is an implementation detail of the child.

The other solution is to define a dedicated contract to send signals to the parent.

## The `Intent` pattern

Our edit form should declare a public contract `Intent` with all signals our component can send to its parent:  

```typescript
// customer/edit/edit.app.ts
export type Intent = 
    | { kind: "SaveCustomer", customer: CustomerDto }
    | { kind: "CancelEdit" }
```

Then we must update our `update` function's signature to return the `Intent`:

```typescript {hl_lines=[2,4,10,"13-16","19-22"]}
// customer/edit/edit.app.ts
export function update(command: Command, model: Model) : { model: Model, effects: Effect[], intents: Intent[] } {
    return match(command)
        .returnType<{ model: Model, effects: Effect[], intents: Intent[] }>()
        .with({ kind: "UpdatePremiumSubscription" }, ({ value }) => {
            const newModel: Model = {
                ...model,
                premiumSubscription: value,
            }
            return { model: newModel, effects: [], intents: [] }
        })
        .with({ kind: "CancelEdit" }, () => {
            const intents: Intent[] = [
                { kind: "CancelEdit" }
            ]
            return { model: model, effects: [], intents }
        })
        .with({ kind: "SaveCustomer" }, () => {
            const intents: Intent[] = [
                { kind: "SaveCustomer", customer: model }
            ]
            return { model: model, effects: [], intents }
        })
        .exhaustive()
}
```

And finally, the parent should catch them and make decisions. This is, in a way, a child command that immediately triggers a command in its parent:  

```typescript {hl_lines=["15-18","24-49"]}
// customer/customer.app.ts
export function update(command: Command, model: Model) : { model: Model, effects: Effect[] } {
    return match(command)
        .returnType<{ model: Model, effects: Effect[] }>()
        // ...
        .with({ kind: "EditCommand" }, ({ subCommand }) => {
            if (!model.customerEdition) return { model, effects: [] }

            const result = editFormUpdate(subCommand, model.customerEdition)
            const newModel: Model = { 
                ...model, 
                customerEdition: result.model,
            }

            return result.intents.reduce(
                applyIntent,
                { model: newModel, effects: [] }
            )
        })
        // ...
        .exhaustive()
}

function applyIntent(
    state: { model: Model, effects: Effect[] }, 
    intent: EditIntent,
) : { model: Model, effects: Effect[] } {
    return match(intent)
        .returnType<{ model: Model, effects: Effect[] }>()
        .with({ kind: "SaveCustomer" }, ({ customer }) => {
            const newModel: Model = {
                ...state.model,
                loading: true,
            }
            const newEffects: Effect[] = [
                ...state.effects,
                { kind: "SaveCustomer", customer },
            ]
            return { model: newModel, effects: newEffects }
        })
        .with({ kind: "CancelEdit" }, () => {
            const newModel: Model = {
                ...state.model,
                customerEdition: null,
            }
            return { model: newModel, effects: state.effects }
        })
        .exhaustive()
}
```

> This pattern is one solution to solve this *child to parent* message issue. So far, this is the only one I've been using, but know they are [others](https://rchavesferna.medium.com/child-parent-communication-in-elm-outmsg-vs-translator-vs-nomap-patterns-f51b2a25ecb1) (`Intent` is referred as `OutMsg`) like the [translator pattern](https://medium.com/@alex.lew/the-translator-pattern-a-model-for-child-to-parent-communication-in-elm-f4bfaa1d3f98).

## Conclusion

In this post, we've seen how to extract a subcomponent and how to compose a view. We didn't have to handle effects for our child component but the idea remains the same: wrap the effects and the commands dispatched by them. The [alternative version](https://github.com/RomainTrm/Sandbox-Elmish-PReact/tree/main/src/customer-v3) mentioned earlier does exactly that.  

## Additional resources

If you liked this series and you are looking for a more detailed resources to implement this pattern on your own, I recommend you reading:

- the [Elm patterns](https://sporto.github.io/elm-patterns/) (Elm)
- the [Elmish book](https://zaid-ajaj.github.io/the-elmish-book/) (F#) wrote by [Zaid Ajaj](https://github.com/Zaid-Ajaj).

---

## Comments

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
