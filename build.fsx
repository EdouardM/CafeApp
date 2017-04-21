// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing.Expecto

// Directories
let buildDir  = "./build/"

let testDir = "./tests/"

let deployDir = "./deploy/"


// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"
    -- "/**/*.Tests.fsproj"

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "BuildApp" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
    |> Log "AppBuild-Output: "
)

Target "BuildTests" (fun _ -> 
    !! "src/**/*.Tests.fsproj"
    |> MSBuildDebug testDir "Build"
    |> Log "BuildTests-Output: " 
)

let testExecutables = !! (testDir + "/*.Tests.exe") 

Target "RunTests" (fun _ -> 
    testExecutables
    |> Expecto (fun p -> 
        {
            p with 
                Debug = true
                Parallel = true
        }
    )
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*")
    -- "*.zip"
    |> Zip buildDir (deployDir + "ApplicationName." + version + ".zip")
)

// Build order
"Clean"
  ==> "BuildApp"
  ==> "BuildTests"
  ==> "RunTests"
  //==> "Deploy"

// start build
RunTargetOrDefault "BuildApp"
