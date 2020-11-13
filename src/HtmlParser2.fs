// ts2fable 0.5.2
module rec Htmlparser2

open System
open Fable.Core

// type Writable = Stream.Writable

type [<AllowNullLiteral>] IExports =
    // abstract WritableStream: WritableStreamStatic
    abstract Parser: ParserStatic

type [<AllowNullLiteral>] Handler =
    abstract onopentag: (string -> obj -> unit) with get, set
    abstract onopentagname: (string -> unit) with get, set
    abstract onattribute: (string -> string -> unit) with get, set
    abstract ontext: (string -> unit) with get, set
    abstract onclosetag: (string -> unit) with get, set
    abstract onprocessinginstruction: (string -> string -> unit) with get, set
    abstract oncomment: (string -> unit) with get, set
    abstract oncommentend: (unit -> unit) with get, set
    abstract oncdatastart: (unit -> unit) with get, set
    abstract oncdataend: (unit -> unit) with get, set
    abstract onerror: (Exception -> unit) with get, set
    abstract onreset: (unit -> unit) with get, set
    abstract onend: (unit -> unit) with get, set

type [<AllowNullLiteral>] Options =
    abstract xmlMode: bool option with get, set
    abstract decodeEntities: bool option with get, set
    abstract lowerCaseTags: bool option with get, set
    abstract lowerCaseAttributeNames: bool option with get, set
    abstract recognizeCDATA: bool option with get, set
    abstract recognizeSelfClosing: bool option with get, set

// type [<AllowNullLiteral>] WritableStream =
//     inherit Writable

// type [<AllowNullLiteral>] WritableStreamStatic =
//     [<Emit "new $0($1...)">] abstract Create: handler: Handler * ?options: Options -> WritableStream

type [<AllowNullLiteral>] Parser =
    abstract write: input: string -> unit
    abstract parseChunk: input: string -> unit
    abstract ``end``: unit -> unit
    abstract ``done``: unit -> unit
    abstract parseComplete: input: string -> unit
    abstract reset: unit -> unit

type [<AllowNullLiteral>] ParserStatic =
    [<Emit "new $0($1...)">] abstract Create: handler: Handler * ?options: Options -> Parser

let [<Import("*","htmlparser2")>] exports :IExports = jsNative
