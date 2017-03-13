namespace TsdGen

open System
open System.IO
open System.Reflection

module Dll =
    let currentAd = AppDomain.CurrentDomain;

    (*let loader (sender: Object) (args: ResolveEventArgs): Assembly =
        System.Diagnostics.Trace.TraceInformation("Failed resolving Assembly {0} for reflection", args.Name)
        null*)

    let currentAssemblyKey = "CurrentReflectionAssemblyBase"

    let customReflectionOnlyResolver(sender: Object)  (e: ResolveEventArgs): Assembly =
    
        let name = AssemblyName(e.Name)
        let dir = """C:\Windows\Microsoft.NET\Framework64\v4.0.30319""" //currentAd.GetData(currentAssemblyKey) :?> string
        let assemblyPath = Path.Combine(dir, name.Name + ".dll")

        if File.Exists(assemblyPath) then        
            // The dependency was found in the same directory as the base
            Assembly.ReflectionOnlyLoadFrom(assemblyPath);        
        else        
            // Wasn't found on disk, hopefully we can find it in the GAC...
            Assembly.ReflectionOnlyLoad(name.Name);

    let reflectionOnlyLoadFrom(assemblyPath:string ): Assembly =    
        let customResolveHandler = new ResolveEventHandler(customReflectionOnlyResolver)
        //currentAd.add_ReflectionOnlyAssemblyResolve customResolveHandler

        // Store the base directory from which we're loading in ALS
        currentAd.SetData(currentAssemblyKey, Path.GetDirectoryName(assemblyPath))

        // Now load the assembly, and force the dependencies to be resolved
        let assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath)
        let types = assembly.GetTypes()

        // Lastly, reset the ALS entry and remove our handler
        currentAd.SetData(currentAssemblyKey, null)
        //currentAd.remove_ReflectionOnlyAssemblyResolve customResolveHandler

        assembly
    

    currentAd.add_ReflectionOnlyAssemblyResolve(new ResolveEventHandler(customReflectionOnlyResolver))


    (*let probePath (dir: string) (name: AssemblyName): string option =
        name.Name
        |> sprintf "%s.dll"
        |> fun file -> Directory.GetFiles(dir, file)
        |> Array.tryHead*)

    let load (path: string): Assembly =
        Console.Error.WriteLine(path)
        reflectionOnlyLoadFrom path

    (*let loadReferenced path (asm: Assembly) =
        Console.WriteLine(asm.Location)
        
        asm.GetReferencedAssemblies()
        |> Seq.collect (probePath path >> Option.toList)
        |> Seq.map load
        |> Seq.iter Console.WriteLine
        
        asm*)

    let loadAsm path = 
        path
        |> load
        //|> loadReferenced (Path.GetDirectoryName(path))
    
    let getPublicTypes paths =
        paths
        |> Seq.map loadAsm
        |> Seq.collect (fun asm -> asm.GetExportedTypes())
        
    let loadClasses paths =
        getPublicTypes paths
        |> Seq.filter (fun t -> t.IsClass)
