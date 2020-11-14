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

Fable.Core.JsInterop.importSideEffects "./styles/main.scss"

// MODEL

type Model = {
  Input: string
  Output: XmlElement
  DropdownIsActive: bool
}

type Msg =
| InputChanged of string
| SelectExample of string
| ToggleDropdown

let examples = [
    "Bulma container", TextFile<"examples/Bulma container.html">.Text
    "Bulma navbar", TextFile<"examples/Bulma navbar.html">.Text
]

let init() : Model =
    let input = snd (List.head examples)
    { Input = input
      Output = Html2Feliz.parse input
      DropdownIsActive = false }

// UPDATE
let update (msg:Msg) (model:Model) =
    match msg with
    | InputChanged content -> { model with Input = content; Output = Html2Feliz.parse content }
    | SelectExample name ->
        let content = examples |> List.pick (fun (n, c) -> if n = name then Some c else None)
        { model with
            Input = content;
            Output = Html2Feliz.parse content
            DropdownIsActive = false }
    | ToggleDropdown -> { model with DropdownIsActive = not model.DropdownIsActive }

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

let examplesDropdown active dispatch =
    Bulma.dropdown [
        if active then dropdown.isActive
        prop.children [
            Bulma.dropdownTrigger [
                Bulma.button.button [
                    prop.ariaControls "dropdown-menu"
                    prop.text "Examples"
                    prop.onClick (fun _ -> dispatch ToggleDropdown)
                ]
                Html.span [
                    prop.className [ "icon"; "is-small" ]
                    prop.children [
                        Html.i [
                            prop.ariaHidden true
                            prop.className [ "fas"; "fa-angle-down" ]
                        ]
                    ]
                ]
            ]
            Bulma.dropdownMenu [
                prop.id "dropdown-menu"
                prop.children [
                    Bulma.dropdownContent [
                        for name, _ in examples do
                            Bulma.dropdownItem.a [
                                prop.text name
                                prop.onClick (fun _ -> dispatch (SelectExample name))
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
                    examplesDropdown model.DropdownIsActive dispatch
                    Bulma.textarea [
                        prop.rows 25
                        prop.cols 80
                        prop.valueOrDefault model.Input
                        prop.onChange (InputChanged >> dispatch)
                    ] |> Html.div
                ]
                Bulma.column [
                    Bulma.button.button [
                        color.isPrimary
                        prop.text "Copy"
                        prop.onClick (fun _ -> Extensions.copyToClipboard "output")
                    ]
                    Bulma.box [
                        prop.id "output"
                        prop.rows 25
                        prop.cols 80
                        prop.children (Html2Feliz.formatNode 4 0 model.Output |> String.concat "\n" |> Html.pre)
                    ] |> Html.div
                ]
            ]
        ]
    ]

// App
Program.mkSimple init update view
|> Program.withReactSynchronous "feliz-app"
|> Program.withConsoleTrace
|> Program.run
