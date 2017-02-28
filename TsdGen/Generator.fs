namespace TsdGen

module Generator =
    open Syntax
    
    let indent i = 
        "    " 
        |> List.replicate i
        |> String.concat ""

    let rec type' (t: Type): string = 
        match t with
        | Null -> "null"
        | String -> "string"
        | Number -> "number"
        | Bool -> "boolean"
        | List t -> sprintf "%s[]" (type' t)
        | Object (NsName ns, IfaceName s) -> sprintf "%s.%s" ns s
        | Union ts -> ts |> List.map type' |> String.concat " | " |> sprintf "(%s)"

    let member' (i: int) (m: Member): (string * int) seq = 
        seq {
            match m with
            | Property (PropName name, typ) -> 
                yield sprintf "%s: %s" name (type' typ), i
        }

    let rec declaration i ast: (string * int) seq = 
        seq {
            match ast with
            | Namespace (NsName ns, decls) -> 
                yield sprintf "declare namespace %s {" ns, i
                for decl in decls do
                    yield! declaration (i + 1) decl
                yield "}", i

            | Interface (IfaceName name, extends, ms) ->
                let extendString = 
                    match extends with
                    | Some (Extends (NsName ns, IfaceName iface)) -> sprintf " extends %s.%s" ns iface
                    | None -> ""

                yield sprintf "interface %s%s {" name extendString, i
                for m in ms do
                    yield! member' (i + 1) m
                yield "}", i
        }

    let generate declarations =     
        declarations
        |> Seq.collect (declaration 0)
        |> Seq.map (fun (l, i) -> (indent i) + l)
        |> String.concat "\n"        