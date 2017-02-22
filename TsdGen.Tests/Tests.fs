namespace TsdGen.Tests

open Expecto
open TsdGen

module Tests = 
  open Syntax

  [<Tests>]
  let tests =
    testList "Generator" [
      testCase "Namespace outputs namespace keyword" <| fun _ ->
        [Namespace ("x", [])]
        |> Generator.generate 
        |> fun code -> Expect.stringContains code "namespace x {" "Generates code for namespace"

      testCase "Interface outputs interface keyword" <| fun _ ->
        [Interface ("c", [])]
        |> Generator.generate 
        |> fun code -> Expect.stringContains code "interface c {" "Generates code for interface"

      testCase "string property" <| fun _ ->
        [Interface ("c", [Property ("p", String)])]
        |> Generator.generate 
        |> fun code -> Expect.stringContains code "p: string" "Generates code for property"

      testCase "Union property" <| fun _ ->
        [Interface ("c", [Property ("p", Union ([String; Number]))])]
        |> Generator.generate 
        |> fun code -> Expect.stringContains code "p: string | number" "Generates code for union type"
    ]

  