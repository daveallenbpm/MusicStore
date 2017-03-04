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

let serialize data =
    JsonConvert.SerializeObject data

type BasicResponse = {
    message: string
}

let createBasicResponse message =
    serialize { message = message }

let routes =
    choose
        [ 
            (hasHeader ("accept", "application/json")) >=> choose 
                [
                    path "/hello" >=> GET >=> OK "Got hello successfully"
                    path "/track" >=> POST >=> hasHeader ("content-type", "application/json") >=> CREATED (createBasicResponse "Track created")
                    pathScan "/track/%i" (fun trackId ->
                            (ApiModel.retrieve DataLayer.getTrack trackId)
                            |> serialize
                            |> OK
                        )

                    NotFound (createBasicResponse "The resource you're looking for isn't here")
                ] >=> Writers.setMimeType "application/json; charset=utf-8"
            BAD_REQUEST (createBasicResponse "Only JSON here you wanker")
        ]
