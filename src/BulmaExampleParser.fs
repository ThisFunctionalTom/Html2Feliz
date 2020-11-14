module BulmaExampleParser
let private findAllIndexes predicate array =
    array
    |> Array.indexed
    |> Array.choose (fun (idx, item) -> if predicate item then Some idx else None)
let getExamples (examplesHtml: string) =
    let lines = examplesHtml.Split('\n', '\r')
    let startIdx =
        lines
        |> findAllIndexes (fun line -> line.StartsWith("{% capture") && line.EndsWith("_example %}"))
    let endIdx =
        lines
        |> findAllIndexes (fun line -> line = "{% endcapture %}")
    let getName (nameLine: string) =
        nameLine.Split(' ').[2]

    Array.zip startIdx endIdx
    |> Array.map (fun (s, e) -> getName lines.[s], lines.[s+1..e-1] |> String.concat "\n")
