namespace TsdGen.Cli

open TsdGen


module Main =
    open Dll
    open Transform
    open Generator

    [<EntryPoint>]
    let main argv =
        loadClasses argv
        |> Seq.map fromClass
        |> Seq.toList
        |> Optimize.combineNamespaces
        |> generate
        |> printf "%s"
        0 // return an integer exit code
