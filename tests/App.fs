module Tests

open System
open Fable.Mocha

let allTests = testList "All tests" [
    FormattingTests.formattingTests
]

Mocha.runTests allTests |> ignore
