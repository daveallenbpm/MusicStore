module MusicStore.Routes

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors

open Newtonsoft.Json

open MusicStore

let NotFound (message: string) =
    fun ctx -> { ctx with response = { ctx.response with status = HTTP_404.status; content = Bytes (UTF8.bytes message) }} |> succeed

let hasHeader (header: string * string) (context: HttpContext) =
    let (headerKey, headerValue) = header
    async.Return (
        let res = 
            context.request.headers
            |> Seq.tryFind (fun (h, v) -> h = headerKey && v = headerValue)
        match res with
        | Some (x, y) -> Some context
        | None -> None
    )

let jsonSerialize data =
    JsonConvert.SerializeObject data

let xmlSerialize data =
    "<xml><test></test></xml>"

type BasicResponse = {
    message: string
}

let createBasicResponse (s: 'a -> string) message : string =
    s { message = message }

let useJsonSerializer (f: ('a -> string) -> WebPart) =
    f jsonSerialize

let useXmlSerializer (f: ('a -> string) -> WebPart) =
    f xmlSerialize

let apiRoutes (serialize: ('a -> string)) =
    choose 
        [
            path "/hello" >=> GET >=> OK "Got hello successfully"
            path "/track" >=> POST >=> hasHeader ("content-type", "application/json") >=> CREATED ((createBasicResponse serialize "Track created"))
            pathScan "/track/%i" (fun trackId ->
                    (ApiModel.retrieve DataLayer.getTrack trackId)
                    |> serialize
                    |> OK
                )
            NotFound (createBasicResponse serialize "The resource you're looking for isn't here")
        ] >=> Writers.setMimeType "application/json; charset=utf-8"

let routes =
    choose
        [ 
            hasHeader ("accept", "application/json") >=> (apiRoutes jsonSerialize)
            hasHeader ("accept", "application/xml") >=> (apiRoutes xmlSerialize)
            BAD_REQUEST (createBasicResponse jsonSerialize "Only JSON here you turd.")
        ]

//let test a b c =
//    [a; b; c]
//
//let x = test 1 2 3
//let y = test "a" "b" "c"
//
//let test2<'a> (f: 'a -> 'a -> 'a -> List<'a>) : ('a -> 'a -> 'a -> List<'a>) =
//    f
//
//let resFunc = test2 test
//let res = resFunc 1 2 3
//let res = resFunc "a" "b" "c"

let foo (a: string)(b: 'm) =
    printfn "%A - %A" a b 
let fooWithA= foo  "hello"