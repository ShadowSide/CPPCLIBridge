module WorkWithModel

open ReflectionModel
open LanguageEx
open FSharp.Core
open Microsoft.FSharp.Collections

type TypesMap<'Key,'Value when 'Key: comparison> (*()*)(values:Map<'Key, 'Value>) =
    (*[<DefaultValue>]
    val mutable types : Map<'Key, 'Value> 
    do this.types <- Map.empty*)
    let mutable types = values

    new () = TypesMap(Map.empty)
    new (initializer: ('Key*'Value)[]) = TypesMap(Map.ofArray initializer)

    member this.findType = flip Map.tryFind types
    member this.containsType = this.findType >> Option.isSome
    member this.addType dTypeInfo newType = 
            types <- Map.add dTypeInfo newType types
            newType
    member this.getType typeConstructor = function
        | dTypeInfo when this.containsType dTypeInfo       -> Map.find dTypeInfo types
        | dTypeInfo                                        -> this.addType dTypeInfo <| typeConstructor()

type TypesExported = TypesMap<DTypeInfo, DType>
type TypesEmbeded = TypesMap<DTypeInfo, DEmbededType>

type TypesColection (typesEmbeded: TypesEmbeded) =
    member public this.typesEmbeded = typesEmbeded
    member public this.typesExported = new TypesExported()

    member this.findType dTypeInfo = 
        let typeExported = Option.defaultValue (this.typesExported.findType dTypeInfo) None
        let typeEmbeded = (Option.map (Some << DDEmbededType) (this.typesEmbeded.findType dTypeInfo))
        let dType = Option.defaultValue typeExported typeEmbeded
        dType
    member this.containsType dTypeInfo = this.typesEmbeded.containsType dTypeInfo || this.typesExported.containsType dTypeInfo
    member this.addType = this.typesExported.addType
    member this.getType typeConstructor = function
        | dTypeInfo when this.typesEmbeded.containsType dTypeInfo ->
            this.typesEmbeded.findType dTypeInfo |> Option.get |> DDEmbededType
        | dTypeInfo -> 
            this.typesExported.getType typeConstructor dTypeInfo

let modelFactory = function
| "Simple" -> Simple
| "List" -> List
| model -> failwith <| sprintf "Unknown model %s" model