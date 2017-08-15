module ConfigHandlers

open System
open System.IO
open ApplicationExceptions
open ConfigDataProviders

let inline checkParameterNotEmpty (ex : ^e)  = function
    | null     -> raise ex
    | ""       -> raise ex
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


    //Seq.concat
    //List.collect
let typesCollectionLoad (config:CodeGenerationConfig.Root) = 
    let inline mappingConfigToKeyValue (mappingDestinationMember:(^b -> ^c)) (mapping:^a) = 
        let {Name=Name; Namespace=Namespace; IsRef=IsRef; Model=modelFactory Model; WrapName=WrapName} = mappingDestinationMember mapping
        Array.map (fun sourceType ->
                            let {name=name; Namespace=Namespace} as source = sourceType
                            let result = 
                                ({
                                    DTypeInfo.typeName=source.name; 
                                    Namespace=source.Namespace
                                 }, {isRef=IsRef; typeInfo = {Namespace=Namespace, typeName=Name}; model=Model; wrapName=WrapName})
                            result)
    config.MappingTypes |> 
    Array.map (mappingConfigToKeyValue (fun mapping -> mapping.CppType)) |>
    //cartesian m.CSharpType m.CppcliType |> 
    TypesEmbeded |>
    TypesColection
