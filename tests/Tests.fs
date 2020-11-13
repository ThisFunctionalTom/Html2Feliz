module Tests

open Fable.Mocha

let tests = testList "All tests" [
    testCase "can convert simple span" <| fun _ ->
        let actual =
            Html2Feliz.parse "<span>Hello, World!</span>"
            |> Html2Feliz.formatDocument 4
            |> String.concat "\n"
        Expect.equal "Html.span \"Hello, World!\"" actual "should be equal"
]

Mocha.runTests tests |> ignore
