module MusicStore.Routes

open ROP

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.ServerErrors
open Suave.RequestErrors

open Newtonsoft.Json

open MusicStore

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

type BasicResponse = {
    message: string
}

let createBasicResponse (s: ISerializer) message : string =
    s.serialize{ message = message }

let getTrackById (serializer: ISerializer) (getTrack: int -> Result<DataLayer.Track, DataLayer.Error>) =
    let genericResponse message = createBasicResponse serializer message
    GET >=> pathScan "/track/%i" (fun trackId ->
        let res = (ApiModel.retrieve getTrack trackId)
        match res with
        |Success track ->
            track
            |> serializer.serialize 
            |> OK
        |Failure err ->
            match err with
            | DataLayer.Error.DatabaseError -> INTERNAL_ERROR (genericResponse "Database error :(")
            | DataLayer.Error.NotFoundError -> NOT_FOUND (genericResponse (sprintf "The requested track with id=%d was not found" trackId))
    )

let apiRoutes (serializer: ISerializer) (mimeType: string) =
    choose 
        [
            path "/track" >=> POST >=> hasHeader ("content-type", "application/json") >=> CREATED ((createBasicResponse serializer "Track created"))
            (getTrackById serializer DataLayer.getTrack)
            NOT_FOUND (createBasicResponse serializer "The resource you're looking for isn't here")
        ] >=> Writers.setMimeType (sprintf "%s; charset=utf-8" mimeType)

let jsonApiroutes = apiRoutes (JsonSerializer() :> ISerializer) "application/json"

let routes routesToUse =
    let jsonSerializer = JsonSerializer() :> ISerializer

    choose
        [ 
            hasHeader ("accept", "application/json") >=> routesToUse
            NOT_FOUND (createBasicResponse jsonSerializer "The endpoint was not found. Try adding Accept:application/json to your request")
        ]
