namespace Fable.Collections

type Stack<'a>() =
    let mutable (items: 'a list) = List.empty

    member x.Count = items.Length

    member x.Push item = items <- item :: items

    member x.Pop() : 'a =
        match items with
        | item :: rest ->
            items <- rest
            item
        | [] -> failwith "Empty stack"
