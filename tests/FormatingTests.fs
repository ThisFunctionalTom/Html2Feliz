module FormattingTests

open System
open Fable.Mocha
open FSharp.Data.LiteralProviders

type String with
    member str.ShowWs =
        str.Replace(' ', '.')
            // .Replace(' ', '␠')
           .Replace('\r', '␍')
           .Replace('\n', '␤')

let testConversion (name: string, htmlStr: string, expected: string) =
    testCase name <| fun _ ->
        let actual =
            Html2Feliz.parse htmlStr
            |> Html2Feliz.formatNode 4 0
            |> String.concat "\n"
        let expected' = expected.Replace("\r", "")
        Expect.equal actual.ShowWs expected'.ShowWs "should be equal"

let formattingTests =
    [
        "simple span",
            TextFile.data.``SimpleSpan.html``.Text,
            TextFile.data.``SimpleSpan.fs``.Text

        "div with attributes",
            TextFile<"data/DivWithAttributes.html">.Text,
            TextFile<"data/DivWithAttributes.fs">.Text

        "div with text",
            TextFile<"data/DivWithText.html">.Text,
            TextFile<"data/DivWithText.fs">.Text
    ]
    |> List.map testConversion
    |> testList "Formatting tests"
