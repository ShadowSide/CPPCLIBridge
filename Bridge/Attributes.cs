using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge
{
    public class CPPCLIBridgeAttribute : Attribute
    {
        public CPPCLIBridgeAttribute(string bridgeName)
        {
            BridgeName = bridgeName;
        }

        public string BridgeName { get; private set; }
    }
}
