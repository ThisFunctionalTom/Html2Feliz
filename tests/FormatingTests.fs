module FormattingTests

open System
open Fable.Mocha
open FSharp.Data.LiteralProviders

type String with
    member str.ShowWs =
        str
            .Replace(' ', '.')
            // .Replace(' ', '␠')
            .Replace(
                '\r',
                '␍'
            )
            .Replace('\n', '␤')

let testConversion (name: string, htmlStr: string, expected: string) =
    testCase name
    <| fun _ ->
        let actual =
            Xml2Feliz.parse htmlStr |> Xml2Feliz.format

        let expected' = expected.Replace("\r", "")
        Expect.equal actual.ShowWs expected'.ShowWs "should be equal"

let formattingTests =
    [ "simple span", TextFile.data.``SimpleSpan.html``.Text, TextFile.data.``SimpleSpan.fs``.Text

      "div with attributes", TextFile<"data/DivWithAttributes.html">.Text, TextFile<"data/DivWithAttributes.fs">.Text

      "div with text", TextFile<"data/DivWithText.html">.Text, TextFile<"data/DivWithText.fs">.Text

      "paragraph with class", TextFile<"data/ParagraphWithClass.html">.Text, TextFile<"data/ParagraphWithClass.fs">.Text

      "div with text and children",
      TextFile<"data/DivWithTextAndChildren.html">.Text,
      TextFile<"data/DivWithTextAndChildren.fs">.Text ]
    |> List.map testConversion
    |> testList "Formatting tests"
