#r @"packages/FAKE.4.52.0/tools/FakeLib.dll"
open Fake

// Properties
let buildDir = "./build/"
let testDir  = "./test/"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir]
)

Target "BuildApp" (fun _ ->
    !! "MusicStore/MusicStore.fsproj"
        |> MSBuildDebug buildDir "Build"
        |> Log "AppBuild-Output: "
)

Target "BuildTest" (fun _ ->
    !! "MusicStore.Test.Unit/MusicStore.Test.Unit.fsproj"
        |> MSBuildDebug testDir "Build"
        |> Log "TestBuild-Output: "
)

Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// Dependencies 
"BuildApp"
  ==> "BuildTest"
  ==> "Default"

// start build
RunTargetOrDefault "BuildApp"