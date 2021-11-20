using System;
using System.Collections.Generic;
using System.IO;

namespace TableFramework
{
    public class Reader
    {

        public const int FLOAT_TO_INT = 10000;
        public static System.Text.Encoding UTF8 = new System.Text.UTF8Encoding(false);
        public Reader()
        {
            m_stream = new MemoryStream();
            m_binaryReader = new System.IO.BinaryReader(m_stream);
        }

        System.IO.BinaryReader m_binaryReader = null;
        MemoryStream m_stream = null;
        int m_index = 0;
        byte[] m_buffer;
        ulong len = 0;
        public void Load(byte[] data, int index, int size)
        {
            m_stream.Write(data, index, size);
            m_stream.Position = 0;
            m_index = 0;
            m_buffer = m_stream.GetBuffer();
            len = (ulong)size;
        }

        public System.IO.BinaryReader reader
        {
            get { return m_binaryReader; }
        }

        public MemoryStream stream
        {
            get { return m_stream; }
        }

        public ulong Length
        {
            get { return len; }
        }

        public int index
        {
            set { m_index = value; stream.Seek(index, SeekOrigin.Begin); }
            get { return m_index; }
        }

        public System.IO.BinaryReader binaryReader
        {
            get { return m_binaryReader; }
        }

        public Reader Read(ref bool value)
        {
            if (m_index < m_buffer.Length)
            {
                UInt32 temp = ReadUInt32Variant();
                value = temp == 1;
            }

            return this;
        }

        public Reader Read(ref List<bool> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    if (value == null)
                        value = new List<bool>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        UInt32 temp = ReadUInt32Variant();
                        value.Add(temp == 1);
                    }
                }
            }

            return this;
        }

        public Reader ReadFix(ref int value)
        {
            value = m_binaryReader.ReadInt32();
            m_index += 4;
            return this;
        }


        public Reader Read(ref int value)
        {
            if (m_index < m_buffer.Length)
            {
                value = ReadInt32Variant();
            }

            return this;
        }


        public Reader Read(ref List<List<string>> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    if (value == null)
                        value = new List<List<string>>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        List<string> temp = null;
                        Read(ref temp);
                        value.Add(temp);
                    }
                }
            }

            return this;
        }

        public Reader Read(ref List<List<int>> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    if (value == null)
                        value = new List<List<int>>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        List<int> temp = null;
                        Read(ref temp);
                        value.Add(temp);
                    }
                }
            }

            return this;
        }

        public Reader Read(ref List<int> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();
                if (count > 0)
                {
                    if (value == null)
                        value = new List<int>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        int temp = ReadInt32Variant();
                        value.Add(temp);
                    }
                }
            }

            return this;
        }

        public Reader Read(ref uint value)
        {
            if (m_index < m_buffer.Length)
            {
                value = ReadUInt32Variant();
            }
            return this;
        }

        public Reader Read(ref List<uint> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();
                if (count > 0)
                {
                    if (value == null)
                        value = new List<uint>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        uint temp = ReadUInt32Variant();
                        value.Add(temp);
                    }
                }
            }

            return this;
        }

        public Reader Read(ref long value)
        {
            if (m_index < m_buffer.Length)
            {
                value = ReadInt64Variant();
            }

            return this;
        }

        public Reader Read(ref List<long> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();
                if (count > 0)
                {
                    if (value == null)
                        value = new List<long>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        long temp = ReadInt64Variant();
                        value.Add(temp);
                    }
                }
            }

            return this;
        }

        public Reader Read(ref ulong value)
        {
            if (m_index < m_buffer.Length)
            {
                value = ReadUInt64Variant();
            }
            return this;
        }

        public Reader Read(ref List<ulong> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();
                if (count > 0)
                {
                    if (value == null)
                        value = new List<ulong>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        UInt64 temp = ReadUInt64Variant();
                        value.Add(temp);
                    }
                }
            }

            return this;
        }

        public Reader Read(ref float value)
        {
            if (m_index < m_buffer.Length)
            {
                int temp = 0;
                Read(ref temp);
                value = ((float)temp) / FLOAT_TO_INT;
            }

            return this;
        }

        public Reader Read(ref List<float> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();
                if (count > 0)
                {
                    if (value == null)
                        value = new List<float>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        float temp = 0;
                        Read(ref temp);
                        value.Add(temp);
                    }
                }
            }

            return this;
        }

        public Reader Read(ref double value)
        {
            if (m_index < m_buffer.Length)
            {
                value = m_binaryReader.ReadDouble();
                m_index += 8;
            }

            return this;
        }

        public Reader Read(ref List<double> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();
                if (count > 0)
                {
                    if (value == null)
                        value = new List<double>((int)count);

                    for (int i = 0; i < count; i++)
                    {
                        double temp = m_binaryReader.ReadDouble();
                        m_index += 8;
                        value.Add(temp);
                    }
                }
            }

            return this;
        }

        public Reader Read(ref string value)
        {
            if (m_index < m_buffer.Length)
            {
                int length = 0;
                try
                {
                    while (0 < m_buffer[m_index + length])
                        ++length;
                }
                catch
                {
                    Logger.LogError(m_index);
                }
                if (m_index + length >= m_buffer.Length)
                {
                    throw new Exception("Index was outside the bounds of the array.");
                }

                value = UTF8.GetString(m_buffer, m_index, length);

                if (string.IsNullOrEmpty(value))
                    value = null;

                m_index += length + 1;
                m_stream.Position += length + 1;
            }

            return this;
        }

        public Reader Read(ref List<string> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    value = new List<string>((int)count);
                    for (int i = 0; i < count; i++)
                    {
                        string temp = null;
                        Read(ref temp);
                        value.Add(temp);
                    }
                }
            }

            return this;
        }




        #region Variant

        public int ReadInt32Variant()
        {
            return (int)ReadUInt32Variant();
        }

        public uint ReadUInt32Variant()
        {
            uint value = 0;
            byte tempByte = 0;
            int index = 0;
            do
            {
                tempByte = m_binaryReader.ReadByte();
                uint temp1 = (uint)((tempByte & 0x7F) << index);  // 0x7F (1<<7)-1  127
                value |= temp1;
                index += 7;
                m_index++;
            }
            while ((tempByte >> 7) > 0);
            return value;
        }

        public long ReadInt64Variant()
        {
            return (long)ReadUInt64Variant();
        }

        public ulong ReadUInt64Variant()
        {
            ulong value = 0;
            byte tempByte = 0;
            int index = 0;
            do
            {
                tempByte = m_binaryReader.ReadByte();
                ulong temp1 = (ulong)((tempByte & 0x7F) << index);  // 0x7F (1<<7)-1  127
                value |= temp1;
                index += 7;
                m_index++;
            }
            while ((tempByte >> 7) > 0);
            return value;
        }

        #endregion

        public void Clear()
        {
            m_stream.SetLength(0);
            m_buffer = null;
            m_index = 0;
        }


        public void Reset()
        {
            m_index = 0;
        }

        public void Close()
        {
            m_buffer = null;
            m_binaryReader.Close();
            m_stream.Close();
        }


        public string ReadString()
        {
            string value = null;
            Read(ref value);
            return value;
        }

        public int ReadInt()
        {
            int value = 0;
            Read(ref value);
            return value;
        }

        public float ReadFloat()
        {
            float value = 0;
            Read(ref value);
            return value;
        }

        public bool ReadBool()
        {
            bool value = false;
            Read(ref value);
            return value;
        }

        public List<string> ReadListString()
        {
            List<string> value = new List<string>();
            Read(ref value);
            return value;
        }

        public List<int> ReadListInt()
        {
            List<int> value = new List<int>();
            Read(ref value);
            return value;
        }


        public List<List<int>> ReadListIntInt()
        {
            List<List<int>> value = new List<List<int>>();
            Read(ref value);
            return value;
        }


        public List<List<string>> ReadListStringString()
        {
            List<List<string>> value = new List<List<string>>();
            Read(ref value);
            return value;
        }

        public List<bool> ReadListBool()
        {
            List<bool> value = new List<bool>();
            Read(ref value);
            return value;
        }

        public List<float> ReadListFloat()
        {
            List<float> value = new List<float>();
            Read(ref value);
            return value;
        }


        public Dictionary<string, string> ReadDicStringString()
        {
            Dictionary<string, string> value = new Dictionary<string, string>();
            Read(ref value);
            return value;
        }


        public Dictionary<string, int> ReadDicStringInt()
        {
            Dictionary<string, int> value = new Dictionary<string, int>();
            Read(ref value);
            return value;
        }


        public Dictionary<int, string> ReadDicIntString()
        {
            Dictionary<int, string> value = new Dictionary<int, string>();
            Read(ref value);
            return value;
        }

        public Dictionary<int, float> ReadDicIntFloat()
        {
            Dictionary<int, float> value = new Dictionary<int, float>();
            Read(ref value);
            return value;
        }

        public Dictionary<int, int> ReadDicIntInt()
        {
            Dictionary<int, int> value = new Dictionary<int, int>();
            Read(ref value);
            return value;
        }

        public Reader Read(ref Dictionary<string, string> value)
        {

            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    value = new Dictionary<string, string>((int)count);
                    for (int i = 0; i < count; i++)
                    {
                        string key = null;
                        Read(ref key);

                        string temp = null;
                        Read(ref temp);

                        value.Add(key, temp);
                    }
                }
            }
            return this;
        }

        public Reader Read(ref Dictionary<string, int> value)
        {

            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    value = new Dictionary<string, int>((int)count);
                    for (int i = 0; i < count; i++)
                    {
                        string key = null;
                        Read(ref key);

                        int temp = 0;
                        Read(ref temp);

                        value.Add(key, temp);
                    }
                }
            }
            return this;
        }

        public Reader Read(ref Dictionary<int, string> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    value = new Dictionary<int, string>((int)count);
                    for (int i = 0; i < count; i++)
                    {
                        int key = 0;
                        Read(ref key);

                        string temp = null;
                        Read(ref temp);

                        value.Add(key, temp);
                    }
                }
            }
            return this;
        }
        public Reader Read(ref Dictionary<int, int> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    value = new Dictionary<int, int>((int)count);
                    for (int i = 0; i < count; i++)
                    {
                        int key = 0;
                        Read(ref key);

                        int temp = 0;
                        Read(ref temp);

                        value.Add(key, temp);
                    }
                }
            }
            return this;
        }


        public Reader Read(ref Dictionary<int, float> value)
        {
            if (m_index < m_buffer.Length)
            {
                uint count = ReadUInt32Variant();

                if (count > 0)
                {
                    value = new Dictionary<int, float>((int)count);
                    for (int i = 0; i < count; i++)
                    {
                        int key = 0;
                        Read(ref key);

                        float temp = 0;
                        Read(ref temp);

                        value.Add(key, temp);
                    }
                }
            }
            return this;
        }
    }
}
