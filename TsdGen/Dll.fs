namespace TsdGen

open System
open System.IO
open System.Reflection

module Dll =
    open Syntax

    let private loadReferenced (asm: Assembly) =
        asm.GetReferencedAssemblies()
        |> Seq.map (fun asmName -> asmName.FullName)
        |> Seq.iter (Assembly.ReflectionOnlyLoad >> ignore)
        asm

    let private loadAsm path = 
        path
        |> File.ReadAllBytes
        |> Assembly.ReflectionOnlyLoad
        |> loadReferenced
    
    let private getPublicTypes paths =
        paths
        |> Seq.map loadAsm
        |> Seq.collect (fun asm -> asm.GetExportedTypes())
        
    let loadClasses paths =
        getPublicTypes paths
        |> Seq.filter (fun t -> t.IsClass)
