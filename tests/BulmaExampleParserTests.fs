module BulmaExamplesParserTests

open System
open Fable.Mocha
open FSharp.Data.LiteralProviders
open BulmaExampleParser

let bulmaExamplesParserTests =
    testList "Bulma examples parser tests" [
        testCase "can parse breadcrum example" <| fun _ ->
            let examples =
                TextFile.data.``BulmaDocsExample.html``.Text
                |> getExamples
            printfn "%A" examples
            Expect.isFalse (Array.isEmpty examples)  "Examples should not be empty"

    ]
