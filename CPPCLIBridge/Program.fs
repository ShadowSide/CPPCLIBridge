// Дополнительные сведения о F# см. на http://fsharp.org
// Дополнительную справку см. в проекте "Учебник по F#".
open FSharp.Data
open System.IO
open System.Reflection
open System
//open Bridge

exception ApplicationParameterError of string

type OperationConfig = JsonProvider<"OperationConfigExample.json">

let inline checkParameterNotEmpty (ex : ^e)  parameter = 
    match parameter with
    | null     ->  raise ex
    | ""       ->  raise ex
    | _        -> ()

let inline emptyDirectory (ex : ^e) (directory:string) = 
    ignore <| match directory with
                | null                                  ->  raise ex
                | value when value.Length < 5           ->  raise ex
                | value when Directory.Exists(value)    ->  
                    Directory.Delete(directory, true)
                    Directory.CreateDirectory (directory)
                | _                                     -> 
                    Directory.CreateDirectory (directory)

let prepareOperation (config : OperationConfig.Root) = 
    emptyDirectory (ApplicationParameterError "TempDirectory") config.TempDirectory
    emptyDirectory (ApplicationParameterError "DestinationDirectory") config.DestinationDirectory
    emptyDirectory (ApplicationParameterError "SecondLevelDestinationDirectory") config.SecondLevelDestinationDirectory
    checkParameterNotEmpty (ApplicationParameterError "BridgeAssembly") config.BridgeAssembly
    checkParameterNotEmpty (ApplicationParameterError "BridgeNameAttribute") config.BridgeNameAttribute
    Array.iter (checkParameterNotEmpty <| ApplicationParameterError "SourceAssemblies") config.SourceAssemblies

(*let inline asType<'T when 'T:not struct> (value:obj) = 
  match value with
  | :? 'T as t -> Some(t)
  | _ -> None*)

//let flip f a b = f b a

let typeAsCPPCLIBridgeAttributed attributeBridgeName (type_ : System.Type) =
    type_.GetCustomAttributes(typeof<Bridge.CPPCLIBridgeAttribute>, false) |>
    Seq.tryFind (fun attribute -> 
                            match attribute with
                            | :? Bridge.CPPCLIBridgeAttribute as target when target.BridgeName = attributeBridgeName       -> true
                            | _                                                                                            -> false) |>
    Option.map (fun a -> a :? Bridge.CPPCLIBridgeAttribute)

let typeIsCPPCLIBridgeAttributed attributeBridgeName type_ =
    match typeAsCPPCLIBridgeAttributed attributeBridgeName type_ with
    | None  -> false
    | _     -> true

let findClassesMarkedForGeneration attributeBridgeName sourceAssemblies = 
     sourceAssemblies |>
     Array.map Assembly.LoadFrom |>
     Seq.map (fun a -> a.Modules |>
                       Seq.map (fun m -> m.GetTypes() |>
                                         Seq.filter (fun t -> typeIsCPPCLIBridgeAttributed attributeBridgeName t) |>
                                                              Seq.map (fun t -> (a, m, t)))) |>
        Seq.concat |> Seq.concat
    
[<EntryPoint>]
let main argv = 
    try
        printfn "Started application with arguments: %A" argv
        let opConfig = OperationConfig.Load @"C:\Sergey\Projects\CPPCLIBridge\CPPCLIBridge\OperationConfigExample.json"        
        prepareOperation opConfig
        let markedForGeneration = Array.ofSeq <| findClassesMarkedForGeneration opConfig.BridgeNameAttribute opConfig.SourceAssemblies
        markedForGeneration |> Array.iter (fun (a,m,t) ->  printfn "Will exported: %s %s %s" (a.GetName() |> string) m.Name t.Name)
        ()
    with
        | ex     -> printfn "Error occured: %s" (string ex)
    ignore <| Console.ReadLine()
    0