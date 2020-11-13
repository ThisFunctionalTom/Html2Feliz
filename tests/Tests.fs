module Tests

open System
open Fable.Mocha

type String with
    member str.ShowWs =
        str
            .Replace(' ', '␠')
            .Replace('\r', '␍')
            .Replace('\n', '␤')

let testConversion (name: string, htmlStr: string, expected: string) =
    testCase name <| fun _ ->
        let actual =
            Html2Feliz.parse htmlStr
            |> Html2Feliz.formatDocument 4
            |> String.concat "\n"
        let expected' = expected.Replace("\r", "")
        Expect.equal actual.ShowWs expected'.ShowWs "should be equal"

let tests =
    [
        "can convert simple span",
            "<span>Hello, World!</span>",
            @"Html.span ""Hello, World!"""

        "can convert element with attributes",
            @"<div id=""some-id"" class=""first""></div>",
            """Html.div [
    prop.id "some-id"
    prop.className "first"
]"""
    ]
    |> List.map testConversion
    |> testList "All tests"

Mocha.runTests tests |> ignore
