module Tests

open System
open Fable.Mocha

type String with
    member str.ShowWs =
        str.Replace(' ', '.')
            // .Replace(' ', '␠')
            // .Replace('\r', '␍')
            // .Replace('\n', '␤')

let testConversion (name: string, htmlStr: string, expected: string) =
    testCase name <| fun _ ->
        let actual =
            Html2Feliz.parse htmlStr
            |> Html2Feliz.formatNode 4 0
            |> String.concat "\n"
        let expected' = expected.Replace("\r", "")
        Expect.equal actual.ShowWs expected'.ShowWs "should be equal"

let tests =
    [
        "simple span",
            "<span>Hello, World!</span>",
            @"Html.span ""Hello, World!"""

        "div with attributes",
            @"<div id=""some-id"" class=""first""></div>",
            """Html.div [
    prop.className "first"
    prop.id "some-id"
]"""

        "div with text",
            """<div class="container" id="value" style="color: red">
  <div class="notification is-primary">
    This container is <strong>centered</strong> on desktop and larger viewports.
  </div>
</div>""",
            """Html.div [
    prop.className "container"
    prop.id "value"
    prop.style "color: red"
    prop.children [
        Html.div [
            prop.className [ "notification"; "is-primary" ]
            prop.children [
                Html.text " This container is "
                Html.strong "centered"
                Html.text " on desktop and larger viewports. "
            ]
        ]
    ]
]"""
    ]
    |> List.map testConversion
    |> testList "All tests"

Mocha.runTests tests |> ignore
