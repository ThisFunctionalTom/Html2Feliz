module App

(**
 The famous Increment/Decrement ported from Elm.
 You can find more info about Elmish architecture and samples at https://elmish.github.io/
*)

open Elmish
open Elmish.React
open Feliz
open Feliz.Bulma
open Fable.SimpleXml
open FSharp.Data.LiteralProviders
open Fable.Core.JsInterop
open Zanaptak.TypedCssClasses
open Fable.SimpleHttp

type FA = CssClasses<"../node_modules/@fortawesome/fontawesome-free/css/all.min.css", Naming.PascalCase>

importSideEffects "./styles/main.scss"

// MODEL

let examples = [|
    "simple header", TextFile<"examples/SimpleHeader.html">.Text
    "simple body", TextFile<"examples/SimpleBody.html">.Text
    "paragraph with class", TextFile<"examples/ParagraphWithStyle.html">.Text
|]

type Model = {
  Examples: Map<string, (string*string) array>
  Input: string
  Output: XmlElement list
  DropdownIsActive: bool
  ExpandedExamples: Set<string>
}

type Msg =
| InputChanged of string
| SelectExample of string*string
| BulmaExampleLoaded of {| Page: string; StatusCode: int; Content: string |}
| ToggleDropdown
| ToggleExampleExpanded of string

module BulmaExamples =
    let docs = "https://raw.githubusercontent.com/jgthms/bulma/master/docs/documentation/"

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
          "title" ] |> List.map (sprintf "elements/%s")

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
          "tabs" ] |> List.map (sprintf "components/%s")

    let getAllExamples =
        List.concat [ elements; components ]
        |> getExamples

let init () : Model*Cmd<Msg> =
    let cmd = BulmaExamples.getAllExamples
    let input = snd (Array.head examples)
    { Examples = Map.ofList [ "Simple", examples ]
      Input = input
      Output = Html2Feliz.parse input
      DropdownIsActive = false
      ExpandedExamples = Set.singleton "Simple" }, cmd

// UPDATE
let update (msg:Msg) (model:Model) =
    match msg with
    | InputChanged content ->
        { model with Input = content; Output = Html2Feliz.parse content }, Cmd.none
    | SelectExample (page, name) ->
        let content =
            model.Examples.[page]
            |> Array.pick (fun (n, c) -> if n = name then Some c else None)
        { model with
            Input = content;
            Output = Html2Feliz.parse content
            DropdownIsActive = false }, Cmd.none
    | ToggleDropdown -> { model with DropdownIsActive = not model.DropdownIsActive }, Cmd.none
    | ToggleExampleExpanded page ->
        let expanded =
            if model.ExpandedExamples.Contains page
            then Set.remove page model.ExpandedExamples
            else Set.add page model.ExpandedExamples
        { model with ExpandedExamples = expanded }, Cmd.none
    | BulmaExampleLoaded result ->
        let examples =
            BulmaExampleParser.getExamples result.Content
            |> Array.choose (fun (name, example) ->
                Html2Feliz.tryParse example
                |> Option.map (fun _ -> name, example))
        if Array.isEmpty examples
        then model, Cmd.none
        else
            { model with
                Examples = Map.add result.Page examples model.Examples }, Cmd.none

module Extensions =
    open Browser.Dom
    let copyToClipboard nodeId =
        let node = document.querySelector (sprintf "#%s" nodeId)
        let range = document.createRange()
        range.selectNode node
        window.getSelection().addRange(range)

        try
            document.execCommand("copy") |> ignore
            window.getSelection().removeAllRanges()
        with
        _ -> ()

let icon faIcon =
    Bulma.icon [
        icon.isSmall
        prop.children [
            Html.i [
                prop.classes [ "fas"; faIcon ]
            ]
        ]
    ]

let examplesMenu model dispatch =
    Html.aside [
        prop.className "menu"
        prop.children [
            for kv in model.Examples do
                let page = kv.Key
                Html.p [
                    prop.className "menu-label"
                    prop.text page
                    prop.style [ style.cursor.pointer ]
                    prop.onClick (fun _ -> dispatch (ToggleExampleExpanded page))
                ]
                if model.ExpandedExamples.Contains page then
                    Html.ul [
                        prop.className "menu-list"
                        prop.children [
                            for name, _content in kv.Value do
                                Html.li [
                                    Html.a [
                                        prop.text name
                                        prop.onClick (fun _ -> dispatch (SelectExample (page, name)))
                                    ]
                                ]
                        ]
                    ]
        ]
    ]

let view (model:Model) dispatch =
    Bulma.container [
        container.isFluid
        prop.children [
            Bulma.title.h1 "Html2Feliz"
            Bulma.columns [
                Bulma.column [
                    column.is2
                    prop.children [
                        examplesMenu model dispatch
                    ]
                ]
                Bulma.column [
                    column.is4
                    prop.children [
                        Bulma.textarea [
                            prop.rows 25
                            prop.cols 80
                            prop.style [ style.minWidth 400 ]
                            prop.valueOrDefault model.Input
                            prop.onChange (InputChanged >> dispatch)
                        ] |> Html.div
                    ]
                ]
                Bulma.column [
                    column.is6
                    prop.children [
                        Bulma.button.button [
                            color.isPrimary
                            prop.text "Copy"
                            prop.onClick (fun _ -> Extensions.copyToClipboard "output")
                        ]
                        Bulma.box [
                            prop.id "output"
                            prop.rows 25
                            prop.cols 80
                            prop.children [
                                Html2Feliz.format model.Output
                                |> Html.pre
                            ]
                        ] |> Html.div
                    ]
                ]
            ]
        ]
    ]

// App
Program.mkProgram init update view
|> Program.withReactSynchronous "feliz-app"
|> Program.withConsoleTrace
|> Program.run
