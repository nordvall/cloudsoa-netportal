using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CloudSOA.Portal.Tokens
{
    /// <summary>
    /// Serializes .NET objects to byte arrays.
    /// </summary>
    /// <remarks>
    /// Code from https://msdn.microsoft.com/en-us/library/azure/dn690521.aspx
    /// </remarks>
    public static class BinarySerializerExtensions
    {
        public static T GetAs<T>(this IDatabase cache, string key)
        {
            var obj = cache.StringGet(key);
            T result = Deserialize<T>(obj);
            return result;
        }

        private static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        public static void Set(this IDatabase cache, string key, object value)
        {
            byte[] objectAsBytes = Serialize(value);
            cache.StringSet(key, objectAsBytes);
        }

        private static byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }
    }
}
