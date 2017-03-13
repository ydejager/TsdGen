namespace TsdGen

open System
open System.Reflection

module Transform =
    open Syntax
    let private isSystemObject t = t = typeof<Object>

    let idWithoutGeneric (t: System.Type): Id =
        t.Name 
        |> fun s -> s.Replace(sprintf "`%i" (t.GetGenericArguments().Length), "") 
        |> Id
    let ns (t: System.Type): Ns = t.Namespace.Split('.') |> Seq.map Id |> List.ofSeq
    let iface (t: System.Type): TypeName = idWithoutGeneric t
    let full (t: System.Type) = ns t, iface t
    let nsT<'t> = ns typeof<'t>
    let ifaceT<'t> = iface typeof<'t>
    let fullT<'t> = nsT<'t>, ifaceT<'t>

    let private toExtends (t: System.Type) =
        Option.ofObj (t.BaseType)
        |> Option.filter (isNull >> not)
        |> Option.filter (isSystemObject >> not)
        |> Option.map (full >> Extends)

    let private toGenerics (t: System.Type): Id list =
        if t.IsGenericTypeDefinition then 
            t.GetGenericArguments()
            |> Seq.map (fun x -> Id x.Name)
            |> List.ofSeq
        else
            []
    
    let rec typeMember = function
        | x when x = typeof<Int16> -> Number
        | x when x = typeof<Int32> -> Number
        | x when x = typeof<Int64> -> Number
        | x when x = typeof<UInt16> -> Number
        | x when x = typeof<UInt32> -> Number
        | x when x = typeof<UInt64> -> Number
        | x when x = typeof<Boolean> -> Bool
        | x when x = typeof<System.String> -> String
        | x when x.IsGenericType -> 
            let innerTypes = 
                x.GetGenericArguments() 
                |> Seq.map typeMember
                |> List.ofSeq

            if typeof<System.Collections.IEnumerable>.IsAssignableFrom(x) then
                List innerTypes.[0]
            else if x.GetGenericTypeDefinition().Name = "Nullable`1" then
                Union [innerTypes.[0]; Null]
            else 
                Generic (idWithoutGeneric x, innerTypes)
        | x -> 
            full x 
            |> Object 
            |> fun t -> if x.IsValueType then t else Union [t; Null]

    let fromProperty (p: PropertyInfo): Member =
        Property (PropName p.Name, typeMember p.PropertyType)

    let private fromProperties = Seq.map fromProperty

    let fromClass (t: System.Type): Declaration =        
        Interface (iface t, toGenerics t, toExtends t, t.GetProperties() |> fromProperties |> Seq.toList)
        |> List.singleton
        |> fun l -> (ns t, l)
        |> Namespace 