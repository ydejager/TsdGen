namespace TsdGen.Tests

open Expecto
open TsdGen

module Main =
    open Syntax
    
    [<EntryPoint>]
    let main argv =
        Tests.runTestsInAssembly defaultConfig argv