namespace TsdGen

module Optimize =
    open Syntax

    let rec private stripNs (rootNs: Ns) decl = 
        match decl with
        | Namespace (subNs, ds) -> 
            ds
            |> List.collect (stripNs (rootNs @ subNs))
        | other -> [(rootNs, other)]

    let combineNamespaces declarations =
        declarations
        |> List.collect (stripNs [])
        |> List.groupBy fst
        |> List.map (fun (ns, l) -> ns, List.map snd l)
        |> List.map Namespace