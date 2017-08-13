module Main

open FSharp.Data
open System.IO
open System.Reflection
open System
open ApplicationExceptions
open LanguageEx

type OperationConfig = JsonProvider<"OperationConfigExample.json">
type CodeGenerationConfig = JsonProvider<"CodeGenerationConfigExample.json">

let inline checkParameterNotEmpty (ex : ^e)  = function
    | null     ->  raise ex
    | ""       ->  raise ex
    | _        -> ()

let inline checkExistsFile (ex : ^e) path = 
    checkParameterNotEmpty ex path
    if not <| File.Exists path then
        raise ex

let inline emptyDirectory (ex : ^e) (directory:string) = 
    ignore <| match directory with
                | null                                  ->  raise ex
                | value when value.Length < 5           ->  raise ex
                | value when Directory.Exists(value)    ->  
                    Directory.Delete(directory, true)
                    Directory.CreateDirectory (directory)
                | _                                     -> 
                    Directory.CreateDirectory (directory)

let argumentsHandler (argv : string[]) = 
    printfn "Application for C++\CLI and C++ codegen from C# assembly. Just mark codegen target classes by CPPCLIBridge attribute and configure config"
    printfn "Application arguments: \"path to operation json config\""
    printfn "Started application with arguments: %A" argv
    if argv.Length < 1 then
        raise (ApplicationArgumentError "Empty command line")
    let operationConfigPath = argv.[0]
    checkParameterNotEmpty (ApplicationArgumentError "Wrong \"path to operation json config\"") operationConfigPath
    let operationConfigPath = Path.GetFullPath operationConfigPath
    checkExistsFile (ApplicationArgumentError "Wrong \"fulled path to operation json config\"") operationConfigPath
    (operationConfigPath)

let prepareOperation (config : OperationConfig.Root) = 
    Console.WriteLine()
    checkParameterNotEmpty (ApplicationParameterError "Wrong TempDirectory") config.TempDirectory
    checkParameterNotEmpty (ApplicationParameterError "Wrong DestinationDirectory") config.DestinationDirectory
    checkParameterNotEmpty (ApplicationParameterError "Wrong SecondLevelDestinationDirectory") config.SecondLevelDestinationDirectory
    checkParameterNotEmpty (ApplicationParameterError "Wrong BridgeNameAttribute") config.BridgeNameAttribute
    Array.iter (checkParameterNotEmpty <| ApplicationParameterError "Wrong SourceAssemblies") config.SourceAssemblies
    let config = OperationConfig.Root(Array.map Path.GetFullPath config.SourceAssemblies, Path.GetFullPath config.TempDirectory, Path.GetFullPath config.DestinationDirectory, Path.GetFullPath config.SecondLevelDestinationDirectory, config.BridgeNameAttribute)
    Array.iter (checkExistsFile <| ApplicationParameterError "Wrong fulled path SourceAssemblies") config.SourceAssemblies
    emptyDirectory (ApplicationParameterError "Wrong fulled path TempDirectory") config.TempDirectory
    emptyDirectory (ApplicationParameterError "Wrong fulled path DestinationDirectory") config.DestinationDirectory
    emptyDirectory (ApplicationParameterError "Wrong fulled path SecondLevelDestinationDirectory") config.SecondLevelDestinationDirectory
    printfn "TempDirectory \"%s\"" config.TempDirectory
    printfn "DestinationDirectory \"%s\"" config.DestinationDirectory
    printfn "SecondLevelDestinationDirectory \"%s\"" config.SecondLevelDestinationDirectory
    printfn "BridgeNameAttribute \"%s\"" config.BridgeNameAttribute
    Array.iter (fun name -> printfn "SourceAssembly \"%s\"" name) config.SourceAssemblies
    config

let typeAsCPPCLIBridgeAttributed (attributeBridgeName : string) (type_ : System.Type) =
    type_.GetCustomAttributes(typeof<Bridge.CPPCLIBridgeAttribute>, false) |>
    Seq.tryFind (function
        | :? Bridge.CPPCLIBridgeAttribute as target when target.BridgeName = attributeBridgeName       -> true
        | _                                                                                            -> false) |>
    Option.map (fun a -> a :? Bridge.CPPCLIBridgeAttribute)

(*let typeIsCPPCLIBridgeAttributed attributeBridgeName type_ =
    match typeAsCPPCLIBridgeAttributed attributeBridgeName type_ with
    | None  -> false
    | _     -> true*)

let findClassesMarkedForGeneration attributeBridgeName sourceAssemblies = 
     sourceAssemblies |>
     Array.map Assembly.LoadFrom |>
     Seq.map (fun a -> a.Modules |>
                       Seq.map (fun m -> m.GetTypes() |>
                                         Seq.map (fun t -> (t,  typeAsCPPCLIBridgeAttributed attributeBridgeName t)) |>
                                                          Seq.filter (fun (_, attr) -> Option.isSome attr) |>
                                                                                       Seq.map (fun (t, attr) -> (a, m, t, attr)))) |>
     Seq.concat |> Seq.concat


[<EntryPoint>]
let main argv = 
    let applicationResult = 
        try
            let (operationConfigPath) = argumentsHandler argv
            let opConfig = OperationConfig.Load operationConfigPath
            let assemblyDirectory = Assembly.GetCallingAssembly().Location |> Path.GetDirectoryName
            let genConfig = Path.Combine (assemblyDirectory, @"CodeGenerationConfigExample.json") |> OperationConfig.Load
            let opConfig = prepareOperation opConfig
            let markedForGeneration = Array.ofSeq <| findClassesMarkedForGeneration opConfig.BridgeNameAttribute opConfig.SourceAssemblies
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