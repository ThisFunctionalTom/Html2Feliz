#r "nuget: FsHttp"

open System.IO
open System.Text.RegularExpressions
open FsHttp
open FsHttp.Dsl

let htmlNameDiffs =
    let regex =
        Regex(@"static member inline ([a-zA-Z']+) .* = Interop.reactElement[^ ]* ""(\w+)""")

    let response =
        http { GET "https://raw.githubusercontent.com/Zaid-Ajaj/Feliz/master/Feliz/Html.fs" }

    let content =
        response.content.ReadAsStringAsync().Result

    content.Split("\n")
    |> Array.choose
        (fun line ->
            let m = regex.Match(line)

            if m.Success then
                Some(m.Groups.[2].Value, m.Groups.[1].Value)
            else
                None)
    |> Array.distinct
    |> Array.filter (fun (htmlName, felizName) -> htmlName <> felizName)

let propNameDiffs =
    let regex =
        Regex(@"static member inline ([a-zA-Z0-9']+) .* = Interop.mkAttr[^ ]* ""(\w+)""")

    let response =
        http { GET "https://raw.githubusercontent.com/Zaid-Ajaj/Feliz/master/Feliz/Properties.fs" }

    let content =
        response.content.ReadAsStringAsync().Result

    content.Split("\n")
    |> Array.skipWhile (fun line -> not (line.StartsWith "type prop"))
    |> Array.takeWhile (fun line -> not (line.StartsWith "module prop"))
    |> Array.choose
        (fun line ->
            let m = regex.Match(line)

            if m.Success then
                Some(m.Groups.[2].Value, m.Groups.[1].Value)
            else
                None)
    |> Array.distinct
    |> Array.filter (fun (attrName, propName) -> attrName <> propName)

let propTypes =
    let regex =
        Regex(@"static member inline \w+ \([^:]+: (\w+)\) = Interop.mkAttr[^ ]* ""(\w+)""")

    let response =
        http { GET "https://raw.githubusercontent.com/Zaid-Ajaj/Feliz/master/Feliz/Properties.fs" }

    let content =
        response.content.ReadAsStringAsync().Result

    content.Split("\n")
    |> Array.skipWhile (fun line -> not (line.StartsWith "type prop"))
    |> Array.takeWhile (fun line -> not (line.StartsWith "module prop"))
    |> Array.choose
        (fun line ->
            let m = regex.Match(line)

            if m.Success then
                Some(m.Groups.[2].Value, m.Groups.[1].Value)
            else
                None)
    |> Array.distinct
    |> Array.filter
        (fun (attrName, propType) ->
            [ "bool"; "int"; "float" ]
            |> List.contains propType)

let htmlNameMaps =
    [ "let htmlNames ="
      "    ["
      for htmlName, felizName in htmlNameDiffs do
          $@"        ""{htmlName}"", ""{felizName}"""
      "    ]"
      "    |> Map.ofList" ]

let propNameMaps =
    [ "let propNames ="
      "    ["
      for attrName, propName in propNameDiffs do
          $@"        ""{attrName}"", ""{propName}"""
      "    ]"
      "    |> Map.ofList" ]

let propTypesMaps =
    [ "let propertyTypes ="
      "    ["
      for attrName, propType in propTypes do
          $@"        ""{attrName}"", ""{propType}"""
      "    ]"
      "    |> List.groupBy fst"
      "    |> List.map (fun (name, values) -> name, values |> List.map snd)"
      "    |> Map.ofList" ]

let lines =
    [ "module Mappings"
      ""
      yield! htmlNameMaps
      ""
      yield! propNameMaps
      ""
      yield! propTypesMaps ]

File.WriteAllLines(__SOURCE_DIRECTORY__ + "/Mappings.fs", lines)
