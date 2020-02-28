using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public static class ObjectSerializer
{
    // Converts an Object, to a byte[]
    public static byte[] ObjectToByteArray(this object p)
    {
        if (p == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, p);
        return ms.ToArray();
    }

    // Converts a byte[] to an Object<T>
    public static T ByteArrayToObject<T>(this byte[] arrBytes)
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        ms.Write(arrBytes, 0, arrBytes.Length);
        ms.Seek(0, SeekOrigin.Begin);
        T obj = (T)bf.Deserialize(ms);

        return obj;
    }

}
