module App

open Elmish
open Elmish.React
open Feliz
open Feliz.Bulma
open FSharp.Data.LiteralProviders
open Fable.Core.JsInterop
open Zanaptak.TypedCssClasses
open Fable.SimpleHttp
open FSharp.Data

type FA = CssClasses<"../node_modules/@fortawesome/fontawesome-free/css/all.min.css", Naming.PascalCase>
type Css = CssClasses<"../src/styles/main.scss", Naming.PascalCase>

module textarea =
    open Fable.Core

    [<Erase>]
    type wrap =
        static member inline off = Interop.mkAttr "wrap" "off"
        static member inline soft = Interop.mkAttr "wrap" "soft"
        static member inline hard = Interop.mkAttr "wrap" "hard"

importSideEffects "./styles/main.scss"

let examples =
    let simpleHeader =
        TextFile<"examples/SimpleHeader.html">.Text

    let simpleBody =
        TextFile<"examples/SimpleBody.html">.Text

    let paraWithClass =
        TextFile<"examples/ParagraphWithStyle.html">.Text

    let comments =
        TextFile<"examples/CommentNode.html">.Text

    let selfClosing =
        TextFile<"examples/SelfClosingElement.html">.Text

    let emptyElement =
        TextFile<"examples/EmptyElement.html">.Text

    let emptyWithText =
        TextFile<"examples/EmptyElementWithText.html">
            .Text

    let textWithSpace =
        TextFile<"examples/TextWithSignificantSpaces.html">
            .Text

    [| "simple header", simpleHeader
       "simple body", simpleBody
       "paragraph with class", paraWithClass
       "comments are ignored", comments
       "self closing element", selfClosing
       "empty element", emptyElement
       "empty element with text", emptyWithText
       "significant spaces", textWithSpace |]

type Model =
    { Examples: Map<string, (string * string) array>
      Input: string
      Output: Result<HtmlNode list, string>
      DropdownIsActive: bool
      ExpandedExamples: Set<string> }

type Msg =
    | InputChanged of string
    | SelectExample of string * string
    | BulmaExampleLoaded of
        {| Page: string
           StatusCode: int
           Content: string |}
    | ToggleDropdown
    | ToggleExampleExpanded of string

module BulmaExamples =
    let docs =
        "https://raw.githubusercontent.com/jgthms/bulma/master/docs/documentation/"

    let getExamples pages =
        let toMessage page (statusCode, content) =
            BulmaExampleLoaded
                {| Page = page
                   StatusCode = statusCode
                   Content = content |}

        Cmd.batch [
            for page in pages do
                Cmd.OfAsync.perform Http.get (sprintf "%s/%s.html" docs page) (toMessage page)
        ]

    let elements =
        [ "block"
          "box"
          "button"
          "content"
          "delete"
          "form"
          "icon"
          "image"
          "notification"
          "progress"
          "table"
          "tag"
          "title" ]
        |> List.map (sprintf "elements/%s")

    let components =
        [ "breadcrumb"
          "card"
          "dropdown"
          "level"
          "media"
          "menu"
          "message"
          "modal"
          "nav"
          "navbar"
          "pagination"
          "panel"
          "tabs" ]
        |> List.map (sprintf "components/%s")

    let getAllExamples =
        List.concat [ elements; components ]
        |> getExamples

let parse htmlStr =
    try
        HtmlNode.Parse htmlStr |> Ok
    with
    | err -> Error $"{err}"

let init () : Model * Cmd<Msg> =
    let cmd = BulmaExamples.getAllExamples
    let input = snd (Array.head examples)

    { Examples = Map.ofList [ "Simple", examples ]
      Input = input
      Output = parse input
      DropdownIsActive = false
      ExpandedExamples = Set.singleton "Simple" },
    cmd

let update (msg: Msg) (model: Model) =
    match msg with
    | InputChanged content ->
        { model with
              Input = content
              Output = parse content },
        Cmd.none
    | SelectExample (page, name) ->
        let content =
            model.Examples.[page]
            |> Array.pick (fun (n, c) -> if n = name then Some c else None)

        { model with
              Input = content
              Output = parse content
              DropdownIsActive = false },
        Cmd.none
    | ToggleDropdown ->
        { model with
              DropdownIsActive = not model.DropdownIsActive },
        Cmd.none
    | ToggleExampleExpanded page ->
        let expanded =
            if model.ExpandedExamples.Contains page then
                Set.remove page model.ExpandedExamples
            else
                Set.add page model.ExpandedExamples

        { model with
              ExpandedExamples = expanded },
        Cmd.none
    | BulmaExampleLoaded result ->
        let examples =
            BulmaExampleParser.getExamples result.Content
            |> Array.choose
                (fun (name, example) ->
                    match parse example with
                    | Ok _ -> Some(name, example)
                    | _ -> None)

        if Array.isEmpty examples then
            model, Cmd.none
        else
            { model with
                  Examples = Map.add result.Page examples model.Examples },
            Cmd.none

module Extensions =
    open Browser.Dom

    let copyToClipboard selector =
        window.getSelection().removeAllRanges ()
        let node = document.querySelector selector
        let range = document.createRange ()
        range.selectNode node
        window.getSelection().addRange (range)

        try
            document.execCommand ("copy") |> ignore
            window.getSelection().removeAllRanges ()
        with
        | _ -> ()

let examplesMenu model dispatch =
    let menuIcon faIcon =
        Bulma.icon [
            icon.isSmall
            prop.children [
                Html.i [
                    prop.classes [ "fas"; faIcon ]
                ]
            ]
        ]

    Html.aside [
        prop.className "menu"
        prop.children [
            for kv in model.Examples do
                let page = kv.Key
                let isExpanded = model.ExpandedExamples.Contains page

                Html.p [
                    prop.className "menu-label"
                    prop.style [ style.cursor.pointer ]
                    prop.onClick (fun _ -> dispatch (ToggleExampleExpanded page))
                    prop.children [
                        Html.span page
                        if isExpanded then
                            menuIcon FA.FaAngleRight
                        else
                            menuIcon FA.FaAngleDown
                    ]
                ]

                if isExpanded then
                    Html.ul [
                        prop.className "menu-list"
                        prop.children [
                            for name, _content in kv.Value do
                                Html.li [
                                    Html.a [
                                        prop.text name
                                        prop.onClick (fun _ -> dispatch (SelectExample(page, name)))
                                    ]
                                ]
                        ]
                    ]
        ]
    ]

let navbar =
    let brand =
        Bulma.navbarBrand.div [
            Html.div [
                Bulma.title.h3 "Xml2Feliz"
                Bulma.subtitle.p "Convert HTML into Feliz style code to learn the syntax"
            ]
        ]

    let menu =
        Bulma.navbarMenu [
            Bulma.navbarEnd.div [
                Bulma.navbarItem.div [
                    Html.a [
                        prop.href "https://github.com/ThisFunctionalTom/Xml2Feliz"
                        prop.children [
                            Bulma.icon [
                                size.isSize2
                                prop.classes [
                                    FA.Fab
                                    FA.FaGithubSquare
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    Bulma.navbar [ brand; menu ]

let listOfExamplesCol model dispatch =
    Bulma.column [
        column.is2
        prop.children [
            examplesMenu model dispatch
        ]
    ]

let inputCol model dispatch =
    let lineCount =
        model.Input
        |> Seq.sumBy (fun c -> if c = '\n' then 1 else 0)

    Bulma.column [
        column.is4
        prop.children [
            Bulma.box [
                Bulma.textarea [
                    textarea.wrap.off
                    prop.rows (min (lineCount + 1) 50)
                    prop.style [
                        style.minWidth (length.percent 40)
                    ]
                    prop.valueOrDefault model.Input
                    prop.onChange (InputChanged >> dispatch)
                ]
            ]
        ]
    ]

let outputHtml2Feliz model dispatch =
    Bulma.column [
        column.is6
        prop.children [
            Bulma.box [
                prop.id "output2"
                prop.children [
                    Bulma.button.button [
                        color.isPrimary
                        prop.className Css.CopyButton2
                        prop.text "Copy"
                        prop.onClick (fun _ -> Extensions.copyToClipboard "#output2>pre")
                    ]
                    match model.Output with
                    | Ok htmlNodes -> Html.pre (Html2Feliz.format htmlNodes)
                    | Error err ->
                        Bulma.notification [
                            color.isDanger
                            prop.text err
                        ]
                ]
            ]
        ]
    ]

let content model dispatch =
    Bulma.columns [
        listOfExamplesCol model dispatch
        inputCol model dispatch
        outputHtml2Feliz model dispatch
    ]

let view (model: Model) dispatch =
    Bulma.container [
        container.isFluid
        prop.children [
            navbar
            content model dispatch
        ]
    ]

Program.mkProgram init update view
|> Program.withReactSynchronous "feliz-app"
|> Program.withConsoleTrace
|> Program.run
