module MusicStore.Routes

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

let NotFound (message: string) =
    fun ctx -> { ctx with response = { ctx.response with status = HTTP_404.status; content = Bytes (UTF8.bytes message) }} |> succeed

let routes =
    choose
        [ 
            GET >=> path "/hello" >=> OK "Got hello successfully"
            NotFound "The resource you're looking for isn't here"
        ]
