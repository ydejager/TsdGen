namespace TsdGen

module Generator =
    open Syntax
    
    let indent i = 
        "    " 
        |> List.replicate i
        |> String.concat ""

    let rec type' (t: Type): string = 
        match t with
        | String -> "string"
        | Null -> "null"
        | Number -> "number"
        | Union ts -> ts |> List.map type' |> String.concat " | "

    let member' (i: int) (m: Member): (string * int) seq = 
        seq {
            match m with
            | Property (name, typ) -> 
                yield sprintf "%s: %s" name (type' typ), i
        }

    let rec declaration i ast: (string * int) seq = 
        seq {
            match ast with
            | Namespace (ns, decls) -> 
                yield sprintf "namespace %s {" ns, i
                for decl in decls do
                    yield! declaration (i + 1) decl
                yield "}", i

            | Interface (name, ms) ->
                yield sprintf "interface %s {" name, i
                for m in ms do
                    yield! member' (i + 1) m
                yield "}", i
        }

    let generate declarations =     
        declarations
        |> Seq.collect (declaration 0)
        |> Seq.map (fun (l, i) -> (indent i) + l)
        |> String.concat "\n"
            
    let check = 
        generate <| 
            [ Namespace("Umbrella", 
                [ Interface("Blaat", 
                        [ Property("X", Union ([String; Null])) ]
                ) ]
            )]