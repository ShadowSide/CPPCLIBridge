module ReflectionModel

type DTypeInfo = 
    {
        Namespace: string; 
        typeName: string; 
    }

type DEnumEssentialAttributes = unit

type DEnum = 
    {
        typeInfo: DTypeInfo;
        values: (string*int)[]; 
        essentialAttributes: DEnumEssentialAttributes;
    }

type DKeywordType = 
    | DClass
    | DStruct
    | DInterface

type DAccess = 
    | DPublic
    | DPrivate
    | DProtected
    | DUnsupported

type DProperyEssentialAttributes = unit

type DProperty = 
    {
        name: string;
        value: DTypeInfo;
        accessSetGet: (DAccess*DAccess);
        essentialAttributes: DProperyEssentialAttributes;
    }

type DArgEssentialAttributes = unit

type DArg = 
    {
        typeInfo: DTypeInfo;
        essentialAttributes: DArgEssentialAttributes;
    }

type DMethodEssentialAttributes = unit

type DMethod = 
    {
        name: string;
        result: Option<DTypeInfo>;
        args: DArg[];
        essentialAttributes: DMethodEssentialAttributes;
    }

type DMemberyTypeEssentialAttributes = 
    {
        bridgeAttribute: Option<Bridge.CPPCLIBridgeAttribute>;
    }

type DMemberyType = 
    {
        keywordType: DKeywordType;
        typeInfo: DTypeInfo;
        publicProperties: DProperty[];
        publicMethods: DMethod[];
        publicConstructors: DMethod[];
        haveDestructor: bool
        essentialAttributes: DMemberyTypeEssentialAttributes;
    }

type DType = DMemberyType | DEnum