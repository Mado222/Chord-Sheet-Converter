using System.Runtime.Serialization.Json;
using WindControlLib;

namespace Neuromaster_Demo_Library_Reduced__netx
{
    public static class CDataInJSONSerializer
    {
        public static byte[] Serialize(CDataIn data)
        {
            DataContractJsonSerializer serializer = new(typeof(CDataIn));

            using MemoryStream ms = new();
            serializer.WriteObject(ms, data);
            return ms.ToArray();
        }
    }
}
