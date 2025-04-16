// Functional core
type PrinterState = {
    NumberOfPagesToPrint: int
    NeedToBeReloaded: bool
}

type Commands =
    | Print of int
    | Reload

let decide (state: PrinterState) = function
    | Print nbOfPagesToPrint ->
        let nbOfPagesLeft = max 0 (state.NumberOfPagesToPrint - nbOfPagesToPrint)
        { 
            NumberOfPagesToPrint = nbOfPagesLeft
            NeedToBeReloaded = nbOfPagesLeft <= 10
        }
    | Reload -> 
        { 
            NumberOfPagesToPrint = 100
            NeedToBeReloaded = false
        }

// Imperative shell
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
