namespace MusicStore.Test.Unit.ApiModel

open MusicStore.ApiModel
open Swensen.Unquote
open Xunit
open FsCheck.Xunit

module Tests =
    [<Fact>]
    let ``Retrieve should return a track``() =
        let getTrack id = {
                Name = "track"
                Genre = Folk
                Artist = "artist"
                Album = "album"
            }

        let expected = {
                Name = "track"
                Genre = Folk
                Artist = "artist"
                Album = "album"
            }

        (retrieve getTrack 1) =! expected

    [<Property>]
    let ``Retrieve should return the correct track``
        (trackMetadata: TrackMetadata)
        (trackNumber: int) =
        let getTrack (x:int) = trackMetadata
        (retrieve getTrack trackNumber) =! trackMetadata

module RoutesTests =
    open Suave.Http

    open MusicStore.Routes

    [<Fact>]
    let ``When we have an empty request, we get a 404 response``() =
        let context = HttpContext.empty
        let asyncResult = routes context

        let result = Async.RunSynchronously(asyncResult)
        
        match result with
        | Some httpContext -> httpContext.response.status =! HttpCode.HTTP_404.status
        | None -> "Result" =! "was None when expected Some _"

    [<Fact>]
    let ``When we have a GET request for hello, we get a 200 and some content``() =
        let request = {
                HttpRequest.empty with                   
                    method = HttpMethod.GET
                    url = System.Uri("http://localhost:8080/hello")
                    
            } 

        let context = { 
                HttpContext.empty with request = request
            }

        let asyncResult = routes context

        let result = Async.RunSynchronously(asyncResult)
        
        match result with
        | Some httpContext -> 
            httpContext.response.status =! HttpCode.HTTP_200.status
            match httpContext.response.content with
            | Bytes bt -> UTF8.toString(bt) =! "Got hello successfully"
            | _ -> true =! false
        | None -> "Result" =! "was None when expected Some _"