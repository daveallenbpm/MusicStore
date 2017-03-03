namespace MusicStore.Test.Unit.ApiModel

open MusicStore.ApiModel
open MusicStore.DataLayer
open Swensen.Unquote
open Xunit
open FsCheck.Xunit

module Tests =
    [<Fact>]
    let ``Retrieve should return a track``() =
        let getTrack trackId =
            {   
                Id = 1
                Metadata = 
                {
                    Name = "Track"
                    Genre = Folk
                    Artist = "Artist"
                    Album = "Album"
                }
            }

        let expected = 
            {   
                Id = 1
                Metadata = 
                {
                    Name = "Track"
                    Genre = Folk
                    Artist = "Artist"
                    Album = "Album"
                }
            }

        (retrieve getTrack 1) =! expected

    [<Property>]
    let ``Retrieve should return the correct track``
        (trackMetadata: TrackMetadata)
        (trackNumber: int) =
        let getTrack (x:int) = trackMetadata
        (retrieve getTrack trackNumber) =! trackMetadata

module RoutingTests =
    open Suave.Http

    open MusicStore

    let routeTestHelper (routes: WebPart) (httpMethod: HttpMethod) (requestUrl: string) (responseChecks: List<HttpContext -> unit>) =
        let request = {
            HttpRequest.empty with                   
                method = httpMethod
                url = System.Uri(requestUrl)       
            } 

        let context = { 
                HttpContext.empty with request = request
            }

        let asyncResult = routes context

        let result = Async.RunSynchronously(asyncResult)
        
        match result with
        | Some httpContext -> 
            responseChecks |> List.iter (fun check -> check httpContext)
        | None -> "Result" =! "was None when expected Some _"

    module Checks = 
        let checkStatus (expectedStatusCode: HttpCode) (context: HttpContext) =
            context.response.status =! expectedStatusCode.status

        let checkContent (expectedContent: string) (context: HttpContext) =
            match context.response.content with
            | Bytes bt -> UTF8.toString(bt) =! "Got hello successfully"
            | _ -> true =! false
        
        
    [<Property>]
    let ``When we search for a route that doesn't exist, we get a 404 response``
        (httpMethod: HttpMethod) =

        let checks = 
            [
                Checks.checkStatus HttpCode.HTTP_404
            ]

        routeTestHelper Routes.routes httpMethod "http://musicstore.com/notfound" checks

    [<Fact>]
    let ``When we have a GET request for hello, we get a 200 and some content``() =

        let checks =
            [
                Checks.checkStatus HttpCode.HTTP_200
                Checks.checkContent "Got hello successfully"
            ]

        routeTestHelper Routes.routes HttpMethod.GET "http://musicstore.com/hello" checks
