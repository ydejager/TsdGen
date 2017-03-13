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
    member this.Y = TestClass2()
    member this.L: string list = []

  type ClassWithTupleProp() =
    member this.X = (1, "2")

  type GenericClass<'T>() =
    class
    end

  module Expect =
    let declarationCode expected ast = 
      ast
      |> Generator.generate 
      |> fun output -> Expect.stringContains output expected "Correct code"

    let typeCode expected t = 
      t
      |> Generator.type' 
      |> fun output -> Expect.stringContains output expected "Correct code"

  [<Tests>]
  let tests =
    testList "All" [
      testList "Generator" [
        testCase "Namespace outputs namespace keyword" <| fun _ ->
          [Namespace ([Id "x"], [])]
          |> Expect.declarationCode "declare namespace x {"
        
        testList "Interfaces" [
          testCase "Interface outputs interface keyword" <| fun _ ->
            [Interface (Id "c", [], None, [])]
            |> Expect.declarationCode "interface c {"

          testCase "Interface with base outputs interface with extends" <| fun _ ->
            [Interface (Id "A", [], Some <| Extends ([Id "Bla"], Id "B"), [])]
            |> Expect.declarationCode "interface A extends Bla.B {"

          testCase "Generic interface with one type" <| fun _ ->
            [Interface (Id "A", [Id "T"], None, [])]
            |> Expect.declarationCode "interface A<T> {"
        ]
        testList "Properties" [        
          testCase "bool property" <| fun _ ->
            [Interface (Id "c", [], None, [Property (PropName "p", Bool)])]
            |> Expect.declarationCode "p: boolean"

          testCase "string property" <| fun _ ->
            [Interface (Id "c", [], None, [Property (PropName "p", String)])]
            |> Expect.declarationCode "p: string"

          testCase "list property" <| fun _ ->
            [Interface (Id "c", [], None, [Property (PropName "p", List String)])]
            |> Expect.declarationCode "p: string[]"

          testCase "Union property" <| fun _ ->
            [Interface (Id "c", [], None, [Property (PropName "p", Union ([String; Number]))])]
            |> Expect.declarationCode "p: (string | number)"
        ]

        testList "Types" [
          testCase "Generic type" <| fun _ ->
            Generic (Id "X", [Bool])
            |> Expect.typeCode "X<boolean>"
        ]
      ]

      testList "Transform" [
        testCase "Test class is transformed to interface" <| fun _ -> 
          typeof<TestClass>
          |> Transform.fromClass 
          |> fun decl -> 
            Expect.equal 
              decl 
              (Namespace ([Id "TsdGen"; Id "Tests"], 
                [
                  (Interface (Id "TestClass",
                    [],
                    None, 
                    [
                      Property (PropName "X", String)
                      Property (PropName "Y", Union [Object ([Id "TsdGen"; Id "Tests"], Id "TestClass2"); Null])
                      Property (PropName "L", List String)
                    ]
                  ))
                ]
              ))
              ""
          
        testCase "Tuple class is transformed" <| fun _ -> 
          typeof<ClassWithTupleProp>
          |> Transform.fromClass 
          |> fun decl -> 
            Expect.equal 
              decl 
              (Namespace ([Id "TsdGen"; Id "Tests"], 
                [
                  (Interface (Id "ClassWithTupleProp",
                    [],
                    None, 
                    [
                      Property (PropName "X", Generic (Id "Tuple", [Number; String]))
                    ]
                  ))
                ]
              ))
              ""

        testCase "Generic class is transformed" <| fun _ -> 
          typedefof<GenericClass<_>>
          |> Transform.fromClass 
          |> fun decl -> 
            Expect.equal 
              decl 
              (Namespace ([Id "TsdGen"; Id "Tests"], 
                [
                  (Interface (Id "GenericClass", [Id "T"], None, []))
                ]
              ))
              ""
      ]

      testList "Simplify" [
        testCase "interfaces in the same namespace are grouped" <| fun _ ->             
              [
                (Namespace ([Id  "TsdGen.Tests"], 
                  [(Interface (Id "Test1", [], None, []))]
                ))
                (Namespace ([Id  "TsdGen.Tests"], 
                  [(Interface (Id "Test2", [], None, []))]
                ))
              ]
              |> Optimize.combineNamespaces
              |> fun actual -> 
                Expect.equal
                  actual
                  [
                    (Namespace ([Id  "TsdGen.Tests"], 
                      [
                        (Interface (Id "Test1", [], None, []))
                        (Interface (Id "Test2", [], None, []))
                      ]
                    ))
                  ]
                  ""
      ]
    ]