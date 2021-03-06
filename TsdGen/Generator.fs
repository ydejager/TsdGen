namespace TsdGen

module Generator =
    open Syntax
    
    let indent i = "    " |> List.replicate i |> String.concat ""
    let concatIds sep = List.map (fun (Id s) -> s) >> String.concat sep
    let ns = concatIds "." 

    let rec type' (t: Type): string = 
        match t with
        | Null -> "null"
        | String -> "string"
        | Number -> "number"
        | Bool -> "boolean"
        | List t -> sprintf "%s[]" (type' t)
        | Object (nss, Id s) -> 
            sprintf "%s.%s" (ns nss) s
        | Generic (Id name, ts) ->
            ts 
            |> Seq.map type'
            |> String.concat "," 
            |> sprintf "%s<%s>" name
        | Union ts -> ts |> List.map type' |> String.concat " | " |> sprintf "(%s)"

    let member' (i: int) (m: Member): (string * int) seq = 
        seq {
            match m with
            | Property (PropName name, typ) -> 
                yield sprintf "%s: %s" name (type' typ), i
        }
        
    let flip f a b = f b a

    let rec declaration i ast: (string * int) seq = 
        seq {
            match ast with
            | Namespace (nss, decls) -> 
                yield sprintf "declare namespace %s {" (ns nss), i
                for decl in decls do
                    yield! declaration (i + 1) decl
                yield "}", i

            | Interface (Id name, generics, extends, ms) ->
                let genericsString =
                    generics 
                    |> concatIds ", " 
                    |> Some 
                    |> Option.filter (fun s -> s.Length > 0)
                    |> Option.map (sprintf "<%s>")
                    |> (flip defaultArg) ""

                let extendString = 
                    match extends with
                    | Some (Extends (nss, Id iface)) -> sprintf "extends %s.%s " (ns nss) iface
                    | None -> ""

                yield sprintf "interface %s%s %s{" name genericsString extendString, i
                for m in ms do
                    yield! member' (i + 1) m
                yield "}", i
        }

    let generate declarations =     
        declarations
        |> Seq.collect (declaration 0)
        |> Seq.map (fun (l, i) -> (indent i) + l)
        |> String.concat "\n"        