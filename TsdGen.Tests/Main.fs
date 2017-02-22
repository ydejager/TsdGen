namespace TsdGen.Tests

open Expecto
open TsdGen

module Main =
    open Syntax
    
    [<EntryPoint>]
    let main argv =
        let x = Namespace ("df", [])
        Tests.runTestsInAssembly defaultConfig argv
