﻿using System.Text;
using System.Runtime.Serialization.Json;
using WindControlLib;

namespace Neuromaster_Demo_Library_Reduced__netx
{
    public static class CDataInJSONSerializer
    {
        public static byte[] Serialize(CDataIn data)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CDataIn));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, data);
                //return Encoding.UTF8.GetString(ms.ToArray());
                return ms.ToArray();
            }
        }
    }
}