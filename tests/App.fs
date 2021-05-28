module Tests

open System
open Fable.Mocha

let allTests =
    testList
        "All tests"
        [ FormattingTests.formattingTests
          BulmaExamplesParserTests.bulmaExamplesParserTests ]

Mocha.runTests allTests |> ignore
