using TableFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public static class BinaryManager
{

    static Dictionary<string, IBinary> IBinaryDic = new Dictionary<string, IBinary>();
    static Dictionary<Type, string> TableTypePathDic = new Dictionary<Type, string>();


    /// <summary>
    /// 释放缓存
    /// </summary>
    public static void OnPreloadFight(bool uload)
    {
        if (IBinaryDic.Count <= 0)
            return;

        foreach (var item in IBinaryDic)
        {
            item.Value?.OnReleaseForFight(uload);
        }
    }


    public static void Release(string table)
    {
        if (IBinaryDic.Count <= 0)
            return;

        if (IBinaryDic.TryGetValue(table, out IBinary binary))
        {
            IBinaryDic.Remove(table);
            binary.Release();
        }
    }

    public static string GetTableBytesPath<TValue>()
    {
        Type type = typeof(TValue);
        if (!TableTypePathDic.TryGetValue(type, out string path))
        {
            TableAttribute attribute = type.GetCustomAttribute<TableAttribute>();

            if (attribute == null)
            {
                Logger.LogError($"未找到类型为 {nameof(TValue)} 的数值表");
                return null;
            }
            path = attribute.tablePath;

            TableTypePathDic.Add(type, path);
        }

        return path;
    }

    public static TValue GetTableElement<TKey, TValue>(TKey key) where TValue : class, new()
    {
        string path = GetTableBytesPath<TValue>();

        if (!IBinaryDic.TryGetValue(path, out IBinary binary))
        {
            IBinaryTable<TKey, TValue> iTable = new BinaryTableDic<TKey, TValue>();

            if (iTable == null || !iTable.Open(path))
                return null;

            binary = iTable as IBinary;


            if (!IBinaryDic.ContainsKey(path))
                IBinaryDic.Add(path, binary);

        }
        IBinaryTable<TKey, TValue> table = binary as IBinaryTable<TKey, TValue>;

        return table[key];
    }

    /// <summary>
    /// 读取一整个表,不缓存，直接释放
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    /// <returns>字典</returns>
    public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(Func<TValue, TKey> handler = null) where TValue : class, new()
    {
        IBinaryTable<TKey, TValue> binary = new BinaryTableDic<TKey, TValue>();

        string path = GetTableBytesPath<TValue>();


        if (binary == null || !binary.Open(path))
            return new Dictionary<TKey, TValue>();

        Dictionary<TKey, TValue> dic = binary.ReadAllContent(handler);
        binary.Release();
        return dic;
    }

    /// <summary>
    /// 读取一整个表,不缓存，直接释放
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="path"></param>
    /// <returns>列表</returns>
    public static List<TValue> ReadList<TValue>() where TValue : class, new()
    {
        string path = GetTableBytesPath<TValue>();


        IBinaryList<TValue> binary = new BinaryTableList<TValue>();

        if (binary == null || !binary.Open(path))
            return new List<TValue>();

        List<TValue> list = binary.ReadList();
        binary.Release();
        return list;
    }

    /// <summary>
    /// 返回表的句柄，按需读取、缓存
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="path"></param>
    /// <returns>字典</returns>
    public static IBinaryTable<TKey, TValue> ReadTableDic<TKey, TValue>() where TValue : class, new()
    {
        string path = GetTableBytesPath<TValue>();

        IBinaryTable<TKey, TValue> binary = new BinaryTableDic<TKey, TValue>();
        if (binary == null || !binary.Open(path))
            return null;

        IBinary bin = binary as IBinary;


        if (!IBinaryDic.ContainsKey(path))
            IBinaryDic.Add(path, bin);

        return binary;
    }


    private const string BYTES_PREFIX = "Assets/Temp/Bytes/";
    private const string TABLE_BYTES_DIRECTORY = "../Build/Binary/";

    private const string SUFFIX = ".bytes";

    public static bool Init()
    {
        BinaryHelper.Init();
        IBinaryDic.Clear();
        return true;
    }


    public static byte[] LoadBytes(string path)
    {
        try
        {
            return ReadBytes(path);
        }
        catch (Exception e)
        {
            Logger.LogError($"{nameof(BinaryManager)}.{nameof(LoadBytes)}加载二进制文件失败 {Path.GetFullPath(path)} err:{e}");
            return null;
        }
    }




    public static byte[] ReadBytes(string path)
    {
        using (FileStream fileStream = new FileStream($"{TABLE_BYTES_DIRECTORY}/{path}", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            int size = (int)fileStream.Length;
            byte[] binary = new byte[size];
            fileStream.Read(binary, 0, size);
            return binary;
        }
    }

}
