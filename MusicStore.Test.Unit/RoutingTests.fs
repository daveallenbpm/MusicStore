namespace MusicStore.Test.Unit

open ROP

open MusicStore
open MusicStore.DataLayer

open Suave.Http

open Swensen.Unquote
open Xunit
open FsCheck.Xunit

module RoutingTests =
    let routeTestHelper (routes: WebPart) (httpMethod: HttpMethod) (requestUrl: string) (responseChecks: List<HttpContext -> unit>) =
        let request = {
            HttpRequest.empty with                   
                method = httpMethod
                url = System.Uri(requestUrl)
                headers = [("accept", "application/json")]    
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
            | Bytes bt -> UTF8.toString(bt) =! expectedContent
            | _ -> true =! false
        
        
    [<Property>]
    let ``When we search for a route that doesn't exist, we get a 404 response``
        (httpMethod: HttpMethod) =

        let checks =
            [
                Checks.checkStatus HttpCode.HTTP_404
            ]

        let failure (ctx: HttpContext) =
            async.Return (None)

        routeTestHelper (Routes.routes failure)  httpMethod "http://musicstore.com/notfound" checks

    [<Fact>]
    let ``Get /track/{id} should return 200 and the track data if the result is found``() =
        let checks =
            [
                Checks.checkStatus HttpCode.HTTP_200
                Checks.checkContent """{"Id":1,"Metadata":{"Name":"Track","Genre":{"Case":"Folk"},"Artist":"Artist","Album":"Album"}}"""
            ]

        let mockTrack = {   
            Id = 1
            Metadata = 
            {
                Name = "Track"
                Genre = Folk
                Artist = "Artist"
                Album = "Album"
            }
        }

        let serializer = Routes.JsonSerializer()
        let routeToCheck = Routes.getTrackById serializer (fun id -> Success mockTrack)

        routeTestHelper (Routes.routes routeToCheck) HttpMethod.GET "http://musicstore.com/track/1" checks

    [<Fact>]
    let ``Get /track/{id} should return 404 if the requested track is not found``()=
        let checks =
            [
                Checks.checkStatus HttpCode.HTTP_404
            ]

        let serializer = Routes.JsonSerializer()
        let routeToCheck = Routes.getTrackById serializer (fun id -> Failure DataLayer.Error.NotFoundError)

        routeTestHelper (Routes.routes routeToCheck) HttpMethod.GET "http://musicstore.com/track/1" checks

    [<Fact>]
    let ``Get /track/{id} should return 500 if there is a database error``()=
        let checks =
            [
                Checks.checkStatus HttpCode.HTTP_500
            ]

        let serializer = Routes.JsonSerializer()
        let routeToCheck = Routes.getTrackById serializer (fun id -> Failure DataLayer.Error.DatabaseError)

        routeTestHelper (Routes.routes routeToCheck) HttpMethod.GET "http://musicstore.com/track/1" checks
