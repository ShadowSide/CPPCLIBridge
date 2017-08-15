module ConfigDataProviders

open FSharp.Data

type OperationConfig = JsonProvider<"OperationConfigExample.json">
type CodeGenerationConfig = JsonProvider<"CodeGenerationConfig.json">