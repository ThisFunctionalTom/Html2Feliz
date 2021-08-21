module Html2Feliz

open System
open FSharp.Data

let capitalize (s: string) =
    s.[0].ToString().ToUpperInvariant()
    + s.[1..].ToLowerInvariant()

let toCamelCase (words: string []) =
    let convertWord i word =
        if i > 0 then
            capitalize word
        else
            word.ToLowerInvariant()

    words |> Array.mapi convertWord

let nameMaps =
    [ "type", "type'"
      "fieldset", "fieldSet" ]
    |> Map.ofList

let formatAttributeName (attr: string) =
    let name =
        attr.Split('-') |> toCamelCase |> String.concat ""

    nameMaps
    |> Map.tryFind name
    |> Option.defaultValue name

let rec compressSpaces (text: string) =
    let compressed = text.Replace("  ", " ")

    if compressed <> text then
        compressSpaces compressed
    else
        compressed

type ChildPosition =
    | FirstChild
    | MiddleChild
    | LastChild
    | SingleChild

let toPositionedChildren (children: HtmlNode list) =
    match children with
    | [] -> []
    | [ child ] -> [ SingleChild, child ]
    | _ ->
        let lastIdx = children.Length - 1

        children
        |> List.mapi
            (fun idx child ->
                if idx = 0 then FirstChild, child
                elif idx = lastIdx then LastChild, child
                else MiddleChild, child)

let rec formatTextProp (pos: ChildPosition) (text: string) =
    let formatted =
        text.Replace("\n", " ").Replace("\r", " ")
        |> compressSpaces

    match pos with
    | FirstChild -> formatted.TrimStart()
    | LastChild -> formatted.TrimEnd()
    | SingleChild -> formatted.Trim()
    | MiddleChild -> formatted

let formatAttribute indent level (HtmlAttribute (name, value)) =
    let indentStr = String(' ', indent * level)

    match name with
    | "class" ->
        let classes = value.Split(' ')

        match classes with
        | [| single |] -> sprintf "%sprop.className \"%s\"" indentStr single
        | multi ->
            let classes =
                multi
                |> Array.map (sprintf "\"%s\"")
                |> String.concat "; "

            sprintf "%sprop.classes [ %s ]" indentStr classes
    | _ -> sprintf $@"{indentStr}prop.{formatAttributeName name} ""{value}"""

let containsOnlyCommentsOrEmptyText (elements: HtmlNode list) =
    elements
    |> List.forall
        (function
        | HtmlComment _
        | HtmlText "" -> true
        | _ -> false)

let emptyChildren (elements: HtmlNode list) =
    elements.IsEmpty
    || containsOnlyCommentsOrEmptyText elements

let (|EmptyChildren|SingleTextNode|Children|) (elements: HtmlNode list) =
    match elements with
    | children when emptyChildren children -> EmptyChildren
    | [ HtmlText text ] -> SingleTextNode text
    | _ -> Children elements


let formatNodeName (name: string) =
    nameMaps
    |> Map.tryFind name
    |> Option.defaultValue name

let rec formatNode indent level (pos: ChildPosition, node: HtmlNode) =
    let line level text =
        let indentStr = String(' ', indent * level)
        sprintf "%s%s" indentStr text

    let nodeBlock name content =
        seq {
            line level (sprintf "Html.%s [" name)
            yield! content
            line level "]"
        }

    seq {
        match node with
        | HtmlText "" -> ()
        | HtmlComment _comment -> ()
        | HtmlText text -> line level $"Html.text \"{formatTextProp pos text}\""
        | HtmlElement (name, [], children) when emptyChildren children -> line level ($"Html.{formatNodeName name} []")
        | HtmlElement (name, [], SingleTextNode text) ->
            line level ($"Html.{formatNodeName name} \"{formatTextProp pos text}\"")
        | HtmlElement (name, attrs, EmptyChildren) ->
            line level ($"Html.{formatNodeName name} [")

            for attr in attrs do
                formatAttribute indent (level + 1) attr

            line level "]"
        | HtmlElement (name, [], children) ->
            // when there are no attributes
            // no need for prop.children
            line level $"Html.{formatNodeName name} ["

            for child in toPositionedChildren children do
                yield! formatNode indent (level + 1) child

            line level "]"
        | HtmlElement (name, attrs, children) ->
            line level $"Html.{formatNodeName name} ["

            for attr in attrs |> List.sort do
                formatAttribute indent (level + 1) attr

            match children with
            | EmptyChildren -> ()
            | SingleTextNode text -> line (level + 1) $"prop.text \"{formatTextProp pos text}"
            | Children children ->
                line (level + 1) "prop.children ["

                for child in toPositionedChildren children do
                    yield! formatNode indent (level + 2) child

                line (level + 1) "]"

            line level "]"
        | HtmlCData _content -> ()
    }

let format (nodes: HtmlNode list) =
    [ for node in toPositionedChildren nodes do
          yield! formatNode 4 0 node ]
    |> String.concat "\n"
