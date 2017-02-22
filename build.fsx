// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing.Expecto

let configuration = getBuildParamOrDefault "configuration" "Debug"
// Pattern specifying assemblies to be tested using expecto

// Directories
let buildDir  = "./build/"
let deployDir = "./deploy/"


// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"

let testExecutables = !! (buildDir + "./*.Tests.exe")

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuild buildDir "Build" ["Configuration", configuration ] appReferences
    |> Log "AppBuild-Output: "
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*")
    -- "*.zip"
    |> Zip buildDir (deployDir + "ApplicationName." + version + ".zip")
)

Target "RunTests" (fun _ ->
    testExecutables
    |> Expecto id
    |> ignore
)


// Build order
"Clean"
  ==> "Build"
  ==> "RunTests"
  ==> "Deploy"

// start build
RunTargetOrDefault "Build"
