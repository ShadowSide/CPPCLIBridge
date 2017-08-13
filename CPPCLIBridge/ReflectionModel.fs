module ReflectionModel

type DTypeData = 
    {
        Namespace: string; 
        typeName: string; 
        bridgeAttribute: Option<Bridge.CPPCLIBridgeAttribute>
    }

type DEnum = 
    {
        data: DTypeData
        values: (string*int)[]; 
    }

type DInterface = 
    {
        data: DTypeData
        
    }
type DMembery = DClass | DInterface
type DType = DMembery | DEnum