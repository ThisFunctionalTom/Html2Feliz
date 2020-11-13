module Html2Feliz

open System

type HtmlAttribute = { Name: string; Value: string }

type HtmlNode =
    { Name: string
      Attributes: HtmlAttribute list
      Elements: HtmlNode list
      DirectInnerText: string option }

type HtmlDocument = { Elements: HtmlNode list }

let (|Text|SingleTextChild|Attributes|Children|Complex|) (node: HtmlNode) =
    let attrs = node.Attributes
    let hasAttrs = not attrs.IsEmpty
    let children = node.Elements
    let hasChildren = not children.IsEmpty

    let hasSingleTextChild =
        not hasAttrs
        && not hasChildren
        && Option.isSome node.DirectInnerText

    let name = node.Name

    if hasSingleTextChild then
        SingleTextChild(name, node.DirectInnerText |> Option.defaultValue "")
    else
        match hasChildren, hasAttrs with
        | false, false -> Text(node.DirectInnerText |> Option.defaultValue "")
        | false, true -> Attributes(name, attrs)
        | true, false -> Children(name, children)
        | true, true -> Complex(name, attrs, children)

let formatAttribute indent level (attr: HtmlAttribute) =
    let indentStr = String(' ', indent * level)
    if attr.Name = "class" then
        let classes = attr.Value.Split(' ')
        match classes with
        | [| single |] -> sprintf "%sprop.className \"%s\"" indentStr single
        | multi ->
            let classNames =
                multi
                |> Array.map (sprintf "\"%s\"")
                |> String.concat "; "

            sprintf "%sprop.className [ %s ]" indentStr classNames
    else
        sprintf "%sprop.%s \"%s\"" indentStr attr.Name attr.Value

let rec formatNode indent level (node: HtmlNode) =
    let indentStr = String(' ', indent * level)
    let line level text = sprintf "%s%s" indentStr text

    let nodeBlock name content =
        seq {
            line level (sprintf "Html.%s [" name)
            yield! content
            line level "]"
        }

    seq {
        match node with
        | Text text -> line level (sprintf "Html.text \"%s\"" text)
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

let formatDocument indent (html: HtmlDocument) =
    seq {
        for node in html.Elements do
            yield! formatNode indent 0 node
    }

open Fable.Core
open Fable.Core.JsInterop

[<Emit("Object.entries($0)")>]
let objectEntries (jsObj: obj): (string * string) array = jsNative

let parse (htmlString: string): HtmlDocument =
    let handler = createEmpty<Htmlparser2.Handler>
    let mutable nodes = List.empty
    let mutable current = List.empty

    handler.onopentag <-
        fun (name: string) (attributes: obj) ->
            let attrs =
                objectEntries attributes
                |> List.ofArray
                |> List.map (fun (name, value) -> { Name = name; Value = value })

            current <-
                { Name = name
                  Attributes = attrs
                  Elements = List.empty
                  DirectInnerText = None }
                :: current

    handler.ontext <-
        fun text ->
            match current with
            | node :: parents ->
                current <-
                    { node with
                          DirectInnerText = Some text }
                    :: parents
            | _ -> ()

    handler.onclosetag <-
        fun (name: string) ->
            match current with
            | node :: parent :: parents ->
                current <-
                    { parent with
                          Elements = parent.Elements @ [ node ] }
                    :: parents
            | _ -> ()

    let parser =
        Htmlparser2.exports.Parser.Create(handler)

    parser.write (htmlString)
    { Elements = current }
