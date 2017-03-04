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

type ISerializer =
    abstract member serialize: 'a -> string

type JsonSerializer() =
    interface ISerializer with
        member this.serialize (data: 'a) : string = JsonConvert.SerializeObject data

type XmlSerializer() =
    interface ISerializer with
        member this.serialize (data: 'a) : string = "<xml><test></test></xml>"

type BasicResponse = {
    message: string
}

let createBasicResponse (s: ISerializer) message : string =
    s.serialize{ message = message }

let apiRoutes (serializer: ISerializer) (mimeType: string) =
    choose 
        [
            path "/hello" >=> GET >=> OK "Got hello successfully"
            path "/track" >=> POST >=> hasHeader ("content-type", "application/json") >=> CREATED ((createBasicResponse serializer "Track created"))
            pathScan "/track/%i" (fun trackId ->
                    (ApiModel.retrieve DataLayer.getTrack trackId)
                    |> serializer.serialize
                    |> OK
                )
            path "/eshdui" >=> GET >=> OK (serializer.serialize { message = "12345" })
            NotFound (createBasicResponse serializer "The resource you're looking for isn't here")
        ] >=> Writers.setMimeType (sprintf "%s; charset=utf-8" mimeType)

let routes =
    let jsonSerializer = JsonSerializer() :> ISerializer
    let xmlSerializer = XmlSerializer() :> ISerializer

    choose
        [ 
            hasHeader ("accept", "application/json") >=> (apiRoutes jsonSerializer"application/json")
            hasHeader ("accept", "application/xml") >=> (apiRoutes xmlSerializer "application/xml")
            BAD_REQUEST (createBasicResponse jsonSerializer "Only JSON here you turd.")
        ]
