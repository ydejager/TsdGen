# TsdGen

## Overview

TsdGen is a lightweight TypeScript definition generator that takes as input .NET DLL paths and produces a typescript declaration on stdout.

TsdGen is focussed on producing clean output declarations that are as strictly typed as allowed by TypeScript. This means that using the ouput currently requires that you compile TypeScript code using --strictNullChecks.

## Status

This is currently alpha. Known issues:
 * ```enum``` support
 * extending generic types is currently broken
 * Generic properties using generic type parameters are broken


## Usage

Programmatically
```fsharp
let paths = ["""C:\MyPackage\My.dll"""]
let types = 
    loadClasses paths
    |> Seq.map fromClass
    |> Seq.toList

let code =
    types
    |> Optimize.combineNamespaces
    |> generate
```

CLI:
```bash
TsdGen.Cli.exe C:\MyPackage\My.dll > My.d.ts
```
## Roadmap

### 1.0
 * ~~Represent nullaboility as type with ```| null```~~
 * ~~Optimize namespaces~~
 * ~~Support objects and valuetypes with correct nullability~~
 * Allow specifying (non-)nullability on struct/class properties
 * Generate enum types using TypeScript's discriminated unions
