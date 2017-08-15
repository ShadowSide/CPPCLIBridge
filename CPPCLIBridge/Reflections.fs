module Reflections

open System.Reflection

let typeAsCPPCLIBridgeAttributed (attributeBridgeName : string) (type_ : System.Type) =
    type_.GetCustomAttributes(typeof<Bridge.CPPCLIBridgeAttribute>, false) |>
    Seq.tryFind (function
        | :? Bridge.CPPCLIBridgeAttribute as target when target.BridgeName = attributeBridgeName       -> true
        | _                                                                                            -> false) |>
    Option.map (fun a -> a :? Bridge.CPPCLIBridgeAttribute)

let findClassesMarkedForGeneration attributeBridgeName sourceAssemblies = 
     sourceAssemblies |>
     Array.map Assembly.LoadFrom |>
     Seq.map (fun a -> a.Modules |>
                       Seq.map (fun m -> m.GetTypes() |>
                                         Seq.map (fun t -> (t,  typeAsCPPCLIBridgeAttributed attributeBridgeName t)) |>
                                                          Seq.filter (fun (_, attr) -> Option.isSome attr) |>
                                                                                       Seq.map (fun (t, attr) -> (a, m, t, attr)))) |>
     Seq.concat |> Seq.concat

//let fillEmbededTypes
(*let fillTypesCollection types = Array.iter 
    let findType*)