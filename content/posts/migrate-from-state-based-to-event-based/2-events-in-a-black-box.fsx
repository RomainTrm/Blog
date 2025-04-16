// Functional core
type PrinterState = {
    NumberOfPagesToPrint: int
    NeedToBeReloaded: bool
}

type Commands =
    | Print of int
    | Reload

type Events =
    | PagesPrinted of int
    | LowInkRaised
    | Reloaded

let evolve (state: PrinterState) = function
    | PagesPrinted nbOfPagesToPrint -> 
        let numberOfPagesToPrint = state.NumberOfPagesToPrint - nbOfPagesToPrint
        { state with NumberOfPagesToPrint = numberOfPagesToPrint }
    | LowInkRaised -> { state with NeedToBeReloaded = true }
    | Reloaded -> { NumberOfPagesToPrint = 100; NeedToBeReloaded = false }

let decide (state: PrinterState) = function
    | Print nbOfPagesToPrint ->
        [
            let nbOfPagesPrinted = min state.NumberOfPagesToPrint nbOfPagesToPrint
            if nbOfPagesPrinted <> 0
            then PagesPrinted nbOfPagesPrinted
            
            let nbOfPagesLeft = state.NumberOfPagesToPrint - nbOfPagesPrinted
            if not state.NeedToBeReloaded && nbOfPagesLeft <= 10
            then LowInkRaised
        ]
        |> List.fold evolve state
    | Reload -> 
        [
            if state.NumberOfPagesToPrint <> 100
            then Reloaded
        ]
        |> List.fold evolve state

// Imperative shell: code didn't change
type InfraDependencies = {
    load: unit -> PrinterState
    save: PrinterState -> unit
}

let print (deps: InfraDependencies) (nbOfPagesToPrint: int) =
    let state = deps.load ()
    let newState = Print nbOfPagesToPrint |> decide state
    deps.save newState

let reload (deps: InfraDependencies) =
    let state = deps.load ()
    let newState = Reload |> decide state
    deps.save newState

// Tests
let testDependency (initialState: PrinterState) = 
    let mutable state = initialState
    {
        load = fun () -> state
        save = fun newState ->
            state <- newState
    }

let expect (expected: PrinterState) (deps: InfraDependencies) =
    let result = deps.load ()
    if expected <> result
    then failwith $"Expected: %A{expected}; Received: %A{result}"

let reloadReturns (expected: PrinterState) (initialState: PrinterState) =
    let dependencies = testDependency initialState
    reload dependencies
    expect expected dependencies

let printReturns (nbOfPagesToPrint: int) (expected: PrinterState) (initialState: PrinterState) =
    let dependencies = testDependency initialState
    print dependencies nbOfPagesToPrint
    expect expected dependencies

let loadedPrinter = { NumberOfPagesToPrint = 100; NeedToBeReloaded = false }

{ NumberOfPagesToPrint = 0; NeedToBeReloaded = true } |> reloadReturns loadedPrinter
{ NumberOfPagesToPrint = 10; NeedToBeReloaded = true } |> reloadReturns loadedPrinter
{ NumberOfPagesToPrint = 100; NeedToBeReloaded = false } |> reloadReturns loadedPrinter

loadedPrinter |> printReturns 5 { NumberOfPagesToPrint = 95; NeedToBeReloaded = false }
loadedPrinter |> printReturns 50 { NumberOfPagesToPrint = 50; NeedToBeReloaded = false }
loadedPrinter |> printReturns 89 { NumberOfPagesToPrint = 11; NeedToBeReloaded = false }
loadedPrinter |> printReturns 90 { NumberOfPagesToPrint = 10; NeedToBeReloaded = true }
loadedPrinter |> printReturns 100 { NumberOfPagesToPrint = 0; NeedToBeReloaded = true }
loadedPrinter |> printReturns 150 { NumberOfPagesToPrint = 0; NeedToBeReloaded = true }
{ NumberOfPagesToPrint = 50; NeedToBeReloaded = false } |> printReturns 5 { NumberOfPagesToPrint = 45; NeedToBeReloaded = false } 
{ NumberOfPagesToPrint = 10; NeedToBeReloaded = true } |> printReturns 5 { NumberOfPagesToPrint = 5; NeedToBeReloaded = true } 
