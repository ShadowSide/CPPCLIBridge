module Main

open System.IO
open System.Reflection
open System
open ConfigDataProviders

[<EntryPoint>]
let main argv = 
    let applicationResult = 
        try
            let (operationConfigPath) = ConfigHandlers.argumentsHandler argv
            let opConfig = OperationConfig.Load operationConfigPath
            let assemblyDirectory = Assembly.GetCallingAssembly().Location |> Path.GetDirectoryName
            let genConfig = Path.Combine (assemblyDirectory, @"CodeGenerationConfigExample.json") |> OperationConfig.Load
            let opConfig = ConfigHandlers.prepareOperation opConfig
            let markedForGeneration = Array.ofSeq <| Reflections.findClassesMarkedForGeneration opConfig.BridgeNameAttribute opConfig.SourceAssemblies
            Console.WriteLine()
            markedForGeneration |> Array.iter (fun (a,m,t,attr) ->  printfn "Marked for export: %s %s.%s" (a.GetName() |> string) t.Namespace t.Name)
            Console.WriteLine()
            ()//implement
            0
        with
            | ex     -> 
                printfn "Error occured: %s" (string ex)
                1
    ignore <| Console.ReadLine()
    applicationResult