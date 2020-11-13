module App

(**
 The famous Increment/Decrement ported from Elm.
 You can find more info about Elmish architecture and samples at https://elmish.github.io/
*)

open Elmish
open Elmish.React
open Feliz
open Html2Feliz

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

let view (model:Model) dispatch =

  Html.div [
    Html.textarea [
      prop.rows 25
      prop.cols 80
      prop.valueOrDefault model.Input
      prop.onChange (InputChanged >> dispatch)
    ] |> Html.div
    Html.button [
      prop.text "Convert"
      prop.onClick (fun _ -> dispatch Convert)
    ]
    Html.textarea [
      prop.rows 25
      prop.cols 80
      prop.text (sprintf "%A" model.Output)
    ] |> Html.div
    Html.textarea [
      prop.rows 25
      prop.cols 80
      prop.text (formatDocument 4 model.Output |> String.concat "\n")
    ] |> Html.div
  ]

// App
Program.mkSimple init update view
|> Program.withReactSynchronous "feliz-app"
|> Program.withConsoleTrace
|> Program.run
