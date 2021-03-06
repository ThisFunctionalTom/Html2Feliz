module Html2Feliz

open System
open System.Collections.Generic
open Fable.SimpleXml

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

let formatAttributeName (attr: string) =
    attr.Split('-') |> toCamelCase |> String.concat ""

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

let toPositionedChildren (children: XmlElement list) =
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

let (|CommentNode|TextNode|SelfClosingElement|EmptyElement|EmptyElementWithText|MixedNode|EmptyTextNode|)
    (node: XmlElement)
    =
    if node.IsComment then
        CommentNode node.Content
    elif
        node.IsTextNode
        && String.IsNullOrWhiteSpace(node.Content.Trim())
    then
        EmptyTextNode
    elif node.IsTextNode then
        TextNode node.Content
    elif node.SelfClosing then
        SelfClosingElement(node.Name, node.Attributes)
    elif List.isEmpty node.Children && node.Content = "" then
        EmptyElement(node.Name, node.Attributes)
    elif List.isEmpty node.Children && node.Content <> "" then
        EmptyElementWithText(node.Name, node.Attributes, node.Content)
    else
        MixedNode(node.Name, node.Attributes, node.Children)

let formatAttribute indent level (attr: KeyValuePair<string, string>) =
    let indentStr = String(' ', indent * level)

    if attr.Key = "class" then
        let classes = attr.Value.Split(' ')

        match classes with
        | [| single |] -> sprintf "%sprop.className \"%s\"" indentStr single
        | multi ->
            let classes =
                multi
                |> Array.map (sprintf "\"%s\"")
                |> String.concat "; "

            sprintf "%sprop.classes [ %s ]" indentStr classes
    else
        sprintf @"%sprop.%s ""%s""" indentStr (formatAttributeName attr.Key) (attr.Value)

let containsOnlyCommentsOrEmptyText (elements: list<XmlElement>) =
    elements
    |> List.forall
        (fun element ->
            let isComment = element.IsComment

            let isEmptyText =
                element.IsTextNode
                && String.IsNullOrWhiteSpace(element.Content.Trim())

            isComment || isEmptyText)

let rec formatNode indent level (pos: ChildPosition, node: XmlElement) =
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
        | EmptyTextNode -> ()
        | CommentNode _ -> ()
        | TextNode text -> line level (sprintf "Html.text \"%s\"" (formatTextProp pos text))
        | SelfClosingElement (name, attrs)
        | EmptyElement (name, attrs) ->
            if Map.isEmpty attrs then
                line level (sprintf "Html.%s []" name)
            else
                line level (sprintf "Html.%s [" name)

                for attr in attrs do
                    formatAttribute indent (level + 1) attr

                line level "]"
        | EmptyElementWithText (name, attrs, text) ->
            if Map.isEmpty attrs then
                line level (sprintf "Html.%s \"%s\"" name (formatTextProp pos text))
            else
                line level (sprintf "Html.%s [" name)

                for attr in attrs do
                    formatAttribute indent (level + 1) attr

                line (level + 1) (sprintf "prop.text \"%s\"" (formatTextProp pos text))
                line level "]"
        | MixedNode (name, attrs, children) ->
            if not attrs.IsEmpty then
                line level (sprintf "Html.%s [" name)

                for attr in attrs do
                    formatAttribute indent (level + 1) attr

                if
                    not children.IsEmpty
                    && not (containsOnlyCommentsOrEmptyText children)
                then
                    line (level + 1) "prop.children ["

                    for child in toPositionedChildren children do
                        yield! formatNode indent (level + 2) child

                    line (level + 1) "]"

                line level "]"
            else
                // when there are no attributes
                // no need for prop.children
                line level (sprintf "Html.%s [" name)

                for child in toPositionedChildren children do
                    yield! formatNode indent (level + 1) child

                line level "]"
    }

let format (nodes: XmlElement list) =
    [ for node in toPositionedChildren nodes do
          yield! formatNode 4 0 node ]
    |> String.concat "\n"

let parse = SimpleXml.parseManyElements

let tryParse = SimpleXml.tryParseManyElements
