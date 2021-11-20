using TableFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class BytesArray
{
    public uint capacity = 8;
    public byte[] array;
    public BytesArray()
    {
        capacity = 8;
        array = new byte[capacity];
    }

    public void Expand()
    {
        capacity = (uint)(capacity << 1);
        array = new byte[capacity];
    }

    public void Check(uint length)
    {
        if (length > capacity)
        {
            uint len = length;
            int bit = 0;
            while (len > 0)
            {
                len = len >> 1;
                bit++;
            }

            capacity = (uint)(1 << (bit + 1));
            array = new byte[capacity];
        }
    }

    public void Covert(uint lenth, ref ulong value)
    {
        value = BitConverter.ToUInt64(array, 0);
    }

    public void Covert(int lenth, ref int value)
    {
        value = 0;
        for (int i = lenth - 1; i >= 0; i--)
        {
            value |= array[i];
            value = value << 8;
        }
    }
}


public class Binary
{
    public const int FLOAT_TO_INT = 10000;
    public static Encoding UTF8 = new System.Text.UTF8Encoding(false);
    public static Dictionary<Type, int> TypeBytesLengthDic = new Dictionary<Type, int>()
    {
        [typeof(int)] = 4,
        [typeof(ulong)] = 8

    };

    static BytesArray bytesArray = new BytesArray();
    FileStream m_fileStream;
    public ulong len { get; set; }

    public Binary(string path)
    {
        m_fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        len = (ulong)m_fileStream.Length;
    }

    public void Close()
    {
        m_fileStream.Close();
        m_fileStream = null;
    }

    /// <summary>
    /// 读取第一个长度字段（int32）
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Binary Read(long startIndex, ref int value)
    {
        if (startIndex >= m_fileStream.Length)
            return this;

        int length = TypeBytesLengthDic[typeof(int)];
        m_fileStream.Seek(startIndex, SeekOrigin.Begin);

        byte[] array = new byte[length];
        m_fileStream.Read(array, 0, length);
        value = BitConverter.ToInt32(array, 0);

        return this;
    }


    /// <summary>
    /// 读取内存块
    /// </summary>
    /// <param name="length"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public Reader ReadBytes(int length, long startIndex = -1)
    {
        if (startIndex > 0 && startIndex < m_fileStream.Length)
        {
            m_fileStream.Seek(startIndex, SeekOrigin.Begin);
        }

        if (length > int.MaxValue)
        {
            Logger.LogError($"{length} 超出int.MaxValue {int.MaxValue}边间");
        }

        bytesArray.Check((uint)length);
        m_fileStream.Read(bytesArray.array, 0, length);

        Reader reader = new Reader();
        reader.Load(bytesArray.array, 0, length);
        return reader;
    }

    public void Reset()
    {
        m_fileStream.Seek(0, SeekOrigin.Begin);
    }
}

