namespace TsdGen

open System
open System.Reflection

module Transform =

    open Syntax
    let private isSystemObject t = t = typeof<Object>

    let private ns (t: System.Type) = NsName t.Namespace
    let private iface (t: System.Type) = IfaceName t.Name

    let private full (t: System.Type) = ns t, iface t

    let private toExtends (t: System.Type) =
        Option.ofObj (t.BaseType)
        |> Option.filter (isNull >> not)
        |> Option.filter (isSystemObject >> not)
        |> Option.map (full >> Extends)

    let rec private typeMember = function
        | x when x = typeof<Int16> -> Number
        | x when x = typeof<Int32> -> Number
        | x when x = typeof<Int64> -> Number
        | x when x = typeof<UInt16> -> Number
        | x when x = typeof<UInt32> -> Number
        | x when x = typeof<UInt64> -> Number
        | x when x = typeof<Boolean> -> Bool
        | x when x = typeof<System.String> -> String
        | x when x.IsGenericType && 
                 typeof<System.Collections.IEnumerable>.IsAssignableFrom(x) -> List (x.GetGenericArguments() |> Seq.head |> typeMember)
        | x -> Union [full x |> Object; Null]

    let private prop (p: PropertyInfo): Member =
        Property (PropName p.Name, typeMember p.PropertyType)

    let private props = Seq.map prop

    let fromClass (t: System.Type): Declaration =        
        Interface (iface t, toExtends t, t.GetProperties() |> props |> Seq.toList)
        |> List.singleton
        |> fun l -> (ns t, l)
        |> Namespace 
            