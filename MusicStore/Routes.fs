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

type BasicResponse = {
    message: string
}

type SerializationType =
| Json
| Xml

type ISerializer =
    abstract member serialize: 'a -> string

type JsonSerializer() =
    interface ISerializer with
        member this.serialize (data: 'a) : string = JsonConvert.SerializeObject data

type XmlSerializer() =
    interface ISerializer with
        member this.serialize (data: 'a) : string = "<xml><test></test></xml>"

let createBasicResponse (s: SerializationType) message : string =
    let serialize =
        match s with
        | Json -> (JsonSerializer() :> ISerializer).serialize
        | Xml -> (XmlSerializer() :> ISerializer).serialize
    serialize { message = message }

let apiRoutes (serializationType: SerializationType) =
    let mimeType =
        match serializationType with
        | Json -> "application/json"
        | Xml -> "application/xml"
        
    let serializer =
        match serializationType with
        | Json -> JsonSerializer() :> ISerializer
        | Xml -> XmlSerializer() :> ISerializer
    choose 
        [
            path "/hello" >=> GET >=> OK "Got hello successfully"
            path "/track" >=> POST >=> hasHeader ("content-type", "application/json") >=> CREATED ((createBasicResponse serializationType "Track created"))
            pathScan "/track/%i" (fun trackId ->
                    (ApiModel.retrieve DataLayer.getTrack trackId)
                    |> serializer.serialize
                    |> OK
                )
            path "/eshdui" >=> GET >=> OK (serializer.serialize { message = "12345" })
            NotFound (createBasicResponse serializationType "The resource you're looking for isn't here")
        ] >=> Writers.setMimeType (sprintf "%s; charset=utf-8" mimeType)

let routes =
    choose
        [ 
            hasHeader ("accept", "application/json") >=> (apiRoutes SerializationType.Json)
            hasHeader ("accept", "application/xml") >=> (apiRoutes SerializationType.Xml)
            BAD_REQUEST (createBasicResponse SerializationType.Json "Only JSON here you turd.")
        ]
