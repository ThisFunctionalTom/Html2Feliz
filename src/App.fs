module App

(**
 The famous Increment/Decrement ported from Elm.
 You can find more info about Elmish architecture and samples at https://elmish.github.io/
*)

open Elmish
open Elmish.React
open Feliz
open Feliz.Bulma
open Html2Feliz

Fable.Core.JsInterop.importSideEffects "./styles/main.scss"

// MODEL

type Model = {
  Input: string
  Output: HtmlDocument
}

type Msg =
| InputChanged of string
| Convert


let example = """
<div class="container">
  <div class="notification is-primary">
    This container is <strong>centered</strong> on desktop and larger viewports.
  </div>
</div>
"""

let exampl2 = """
<nav class="level">
  <div class="level-left">
    <div class="level-item">
      <p class="subtitle is-5"><strong>123</strong> posts</p>
    </div>
    <div class="level-item">
      <div class="field has-addons">
        <p class="control">
          <input class="input" type="text" placeholder="Find a post" />
        </p>
        <p class="control">
          <button class="button">Search</button>
        </p>
      </div>
    </div>
  </div>
  <div class="level-right">
    <p class="level-item"><strong>All</strong></p>
    <p class="level-item"><a>Published</a></p>
    <p class="level-item"><a>Drafts</a></p>
    <p class="level-item"><a>Deleted</a></p>
    <p class="level-item"><a class="button is-success">New</a></p>
  </div>
</nav>
"""

let init() : Model =
  { Input = example
    Output = parse example }

// UPDATE
let update (msg:Msg) (model:Model) =
    match msg with
    | InputChanged content -> { model with Input = content }
    | Convert -> { model with Output = parse model.Input }

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

let view (model:Model) dispatch =
    Bulma.container [
        container.isFluid
        prop.children [
            Bulma.title.h1 "Html2Feliz"
            Bulma.columns [
                Bulma.column [
                    Bulma.textarea [
                        prop.rows 25
                        prop.cols 80
                        prop.valueOrDefault model.Input
                        prop.onChange (InputChanged >> dispatch)
                    ] |> Html.div
                    Html.button [
                        prop.text "Convert"
                        prop.onClick (fun _ -> dispatch Convert)
                    ]
                ]
                Bulma.column [
                    Bulma.box [
                        prop.id "output"
                        prop.rows 25
                        prop.cols 80
                        prop.children (formatDocument 4 model.Output |> String.concat "\n" |> Html.pre)
                    ] |> Html.div
                    Html.button [
                        prop.text "Copy"
                        prop.onClick (fun _ -> Extensions.copyToClipboard "output")
                    ]
                ]
            ]
        ]
    ]

// App
Program.mkSimple init update view
|> Program.withReactSynchronous "feliz-app"
|> Program.withConsoleTrace
|> Program.run
