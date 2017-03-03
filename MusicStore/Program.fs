// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help. 
open System
open System.Threading

open Suave

open MusicStore

[<EntryPoint>]
let main argv =
    let cts = new CancellationTokenSource()
    let conf = { defaultConfig with cancellationToken = cts.Token }
    let listening, server = startWebServerAsync conf Routes.routes
    Async.Start(server, cts.Token)
    printfn "Make requests now"
    Console.ReadKey true |> ignore
    cts.Cancel()
    1
