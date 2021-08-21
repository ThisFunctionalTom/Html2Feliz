namespace Fable.IO

type StringReader(str: string) =
    let mutable pos = 0

    member x.IsEndOfString = pos >= str.Length

    member x.PeekChar() =
        if not x.IsEndOfString then
            str.[pos]
        else
            char -1

    member x.ReadChar() =
        if not x.IsEndOfString then
            let char = str.[pos]
            pos <- pos + 1
            char
        else
            char -1

    member x.ReadNChar(count) =
        if not x.IsEndOfString then
            let str = str.[pos..pos + count]
            pos <- pos + str.Length
            str
        else
            ""
