namespace TsdGen.Tests

open Expecto
open TsdGen

module Tests = 
  open Syntax

  type TestClass2() = 
    class
    end

  type TestClass() =
    member this.X = ""
    member this.Y = new TestClass2()
    member this.L: string list = []

  module Expect =
    let generatedCode expected ast = 
      ast
      |> Generator.generate 
      |> fun output -> Expect.stringContains output expected "Correct code"

  [<Tests>]
  let tests =
    testList "All" [
      testList "Generator" [
        testCase "Namespace outputs namespace keyword" <| fun _ ->
          [Namespace (NsName "x", [])]
          |> Expect.generatedCode "declare namespace x {"

        testCase "Interface outputs interface keyword" <| fun _ ->
          [Interface (IfaceName "c", None, [])]
          |> Expect.generatedCode "interface c {"

        testCase "Interface with base outputs interface with extends" <| fun _ ->
          [Interface (IfaceName "A", Some <| Extends (NsName "Bla", IfaceName "B"), [])]
          |> Expect.generatedCode "interface A extends Bla.B {"

        testCase "bool property" <| fun _ ->
          [Interface (IfaceName "c", None, [Property (PropName "p", Bool)])]
          |> Expect.generatedCode "p: boolean"

        testCase "string property" <| fun _ ->
          [Interface (IfaceName "c", None, [Property (PropName "p", String)])]
          |> Expect.generatedCode "p: string"

        testCase "list property" <| fun _ ->
          [Interface (IfaceName "c", None, [Property (PropName "p", List String)])]
          |> Expect.generatedCode "p: string[]"

        testCase "Union property" <| fun _ ->
          [Interface (IfaceName "c", None, [Property (PropName "p", Union ([String; Number]))])]
          |> Expect.generatedCode "p: (string | number)"
      ]

      testList "Transform" [
        testCase "Test class is transformed to interface" <| fun _ -> 
          typeof<TestClass>
          |> Transform.fromClass 
          |> fun decl -> 
            Expect.equal 
              decl 
              (Namespace (NsName "TsdGen.Tests", 
                [
                  (Interface (IfaceName "TestClass", 
                    None, 
                    [
                      Property (PropName "X", String)
                      Property (PropName "Y", Union [Object (NsName "TsdGen.Tests", IfaceName "TestClass2"); Null])
                      Property (PropName "L", List String)
                    ]
                  ))
                ]
              ))
              ""
      ]

      testList "Simplify" [
        testCase "interfaces in the same namespace are grouped" <| fun _ ->             
              [
                (Namespace (NsName "TsdGen.Tests", 
                  [(Interface (IfaceName "Test1", None, []))]
                ))
                (Namespace (NsName "TsdGen.Tests", 
                  [(Interface (IfaceName "Test2", None, []))]
                ))
              ]
              |> Optimize.combineNamespaces
              |> fun actual -> 
                Expect.equal
                  actual
                  [
                    (Namespace (NsName "TsdGen.Tests", 
                      [
                        (Interface (IfaceName "Test1", None, []))
                        (Interface (IfaceName "Test2", None, []))
                      ]
                    ))
                  ]
                  ""
      ]
    ]