namespace TsdGen

module Optimize =
    open Syntax

    let rec private stripNs ns decl = 
        match decl with
        | Namespace (NsName subNs, ds) -> 
            ds
            |> List.collect (stripNs (ns + subNs))
        | x -> [(ns, x)]

    let combineNamespaces declarations =
        declarations
        |> List.collect (stripNs "")
        |> List.groupBy fst
        |> List.map (fun (ns, l) -> NsName ns, List.map snd l)
        |> List.map Namespace