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

let (|Text|SingleTextChild|Attributes|Children|Complex|) (node: XmlElement) =
    let hasAttrs = not node.Attributes.IsEmpty
    match hasAttrs, node.Children with
    | false, [] ->
        if node.IsTextNode
        then Text(sanitizeContent node.Content)
        else SingleTextChild(node.Name, sanitizeContent node.Content)
    | false, children -> Children(node.Name, children)
    | true, [] -> Attributes(node.Name, node.Attributes)
    | true, children -> Complex(node.Name, node.Attributes, children)

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
        | Text text when (String.IsNullOrWhiteSpace text) -> () // ignore empty text elements
        | Text text -> line level (sprintf @"Html.text ""%s""" text)
        | SingleTextChild (node, text) -> line level (sprintf "Html.%s \"%s\"" node text)
        | Attributes (name, attrs) ->
            yield!
                nodeBlock
                    name
                    [ for attr in attrs do
                        formatAttribute indent (level + 1) attr ]
        | Children (name, children) ->
            yield!
                nodeBlock
                    name
                    [ for child in children do
                        yield! formatNode indent (level + 1) child ]
        | Complex (name, attrs, children) ->
            yield!
                nodeBlock
                    name
                    [ for attr in attrs do
                        formatAttribute indent (level + 1) attr
                      line (level + 1) "prop.children ["
                      for child in children do
                          yield! formatNode indent (level + 2) child
                      line (level + 1) "]" ]
    }

let format (node: XmlElement) = formatNode 4 0 node

let parse = SimpleXml.parseElement
