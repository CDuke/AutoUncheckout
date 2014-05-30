#r @"tools/FAKE/tools/FakeLib.dll"
open Fake
open System.Xml

// Properties
let buildDirBase = "./bin/"
let buildDir = buildDirBase


// files
let slnReferences = !!("./src/AutoUncheckout.sln")

// Targets
Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "BuildApp" (fun _ ->
    slnReferences
        |> MSBuildRelease  buildDir "Build"
        |> Log "AppBuild-Output: "
)

Target "All" DoNothing


// Dependencies
"Clean"
    ==> "BuildApp"
    ==> "All"

// start build
RunTargetOrDefault "All"