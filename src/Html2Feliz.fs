module Html2Feliz

open System
open System.Collections.Generic
open Fable.SimpleXml

let rec sanitizeContent (content: string) =
    let sanitized = content.Replace("\n", " ").Replace("\r", " ").Replace("  ", " ")
    if content <> sanitized
    then sanitizeContent sanitized
    else content

let capitalize ( s : string ) =
    s.[ 0 ].ToString().ToUpperInvariant() + s.[ 1 .. ].ToLowerInvariant()

let toCamelCase (words: string []) =
    words
    |> Array.mapi (fun i s -> if i > 0 then capitalize s else s.ToLowerInvariant())

let sanitizeAttributeName (attr: string) =
    attr.Split('-')
    |> toCamelCase
    |> String.concat ""

let (|CommentNode|TextNode|SelfClosingElement|EmptyElement|EmptyElementWithText|MixedNode|) (node: XmlElement) =
    if node.IsComment then CommentNode node.Content
    elif node.IsTextNode then TextNode node.Content
    elif node.SelfClosing then SelfClosingElement (node.Name, node.Attributes)
    elif List.isEmpty node.Children && node.Content = "" then EmptyElement (node.Name, node.Attributes)
    elif List.isEmpty node.Children && node.Content <> "" then EmptyElementWithText (node.Name, node.Attributes, node.Content)
    else MixedNode (node.Name, node.Attributes, node.Children)

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
        sprintf @"%sprop.%s ""%s""" indentStr (sanitizeAttributeName attr.Key) (attr.Value)

let rec formatNode indent level (node: XmlElement) =
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
        | CommentNode _ -> ()
        | TextNode text -> line level (sprintf "Html.text \"%s\"" text)
        | SelfClosingElement (name, attrs)
        | EmptyElement (name, attrs) ->
            if Map.isEmpty attrs
            then line level (sprintf "Html.%s []" name)
            else
                line level (sprintf "Html.%s [" name)
                for attr in attrs do
                    formatAttribute indent (level + 1) attr
                line level "]"
        | EmptyElementWithText (name, attrs, text) ->
            if Map.isEmpty attrs
            then line level (sprintf "Html.%s \"%s\"" name text)
            else
                line level (sprintf "Html.%s [" name)
                for attr in attrs do
                    formatAttribute indent (level + 1) attr
                line (level+1) (sprintf "prop.text \"%s\"" text)
                line level "]"
        | MixedNode (name, attrs, children) ->
            line level (sprintf "Html.%s [" name)
            for attr in attrs do
                formatAttribute indent (level + 1) attr
            line (level+1) "prop.children ["
            for child in children do
                yield! formatNode indent (level+2) child
            line (level+1) "]"
            line level "]"
    }

let format (nodes: XmlElement list) =
    [ for node in nodes do
        yield! formatNode 4 0 node ]
    |> String.concat "\n"

let parse = SimpleXml.parseManyElements

let tryParse = SimpleXml.tryParseManyElements
