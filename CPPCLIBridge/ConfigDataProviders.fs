module ConfigDataProviders

open FSharp.Data

type OperationConfig = JsonProvider<"OperationConfigExample.json">
type CodeGenerationConfig = JsonProvider<"CodeGenerationConfigExample.json">