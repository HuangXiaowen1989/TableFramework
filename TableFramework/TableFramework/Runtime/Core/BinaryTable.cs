using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TableFramework
{
    public interface IBinary
    {
        void OnReleaseForFight(bool uload);

        void Release();
    }

    public interface IBinaryList<TXTable> : IBinary where TXTable : class, new()
    {
        bool Open(string path);
        List<TXTable> ReadList();
    }


    public interface IBinaryTable<TPKey, TXTable> : IBinary, IDictionary<TPKey, TXTable> where TXTable : class, new()
    {
        bool Open(string path);
        TXTable Get(TPKey key);
        Dictionary<TPKey, TXTable> ReadAllContent(Func<TXTable, TPKey> handler);
    }

    public class BinaryTableList<TXTable> : BinaryTable, IBinaryList<TXTable> where TXTable : class, new()
    {
        public void Release()
        {

            Close();

        }

        public List<TXTable> ReadList()
        {
            if (m_binaryFileFolder == null)
                return null;

            Reader reader = m_binaryFileFolder.GetContentTrunkReader();
            int row = m_binaryFileFolder.Row;
            int col = m_binaryFileFolder.Col;

            List<TXTable> list = new List<TXTable>(row);

            for (int i = 0; i < row; i++)
            {
                TXTable t = new TXTable();
                ITable tableSerilize = t as ITable;
                tableSerilize.Deserialize(reader);

                list.Add(t);
            }
            return list;
        }

        public void OnReleaseForFight(bool uload)
        {
        }
    }



    public class BinaryTableDic<TPKey, TXTable> : BinaryTable, IBinaryTable<TPKey, TXTable> where TXTable : class, new()
    {
        List<TPKey> m_primarykeyList;
        Dictionary<TPKey, int> m_primarykeyDic;

        public Func<TXTable, TPKey> Handler { get; set; }

        /// <summary>
        /// 主键列表对应行
        /// </summary>
        public TXTable this[TPKey column]
        {
            get
            {
                return Get(column);
            }
        }
        //索引
        Dictionary<TPKey, TXTable> m_xTableDic = new Dictionary<TPKey, TXTable>();

        public ICollection<TPKey> Keys
        {
            get
            {
                if (m_primarykeyList == null)
                    ReadIndexTrunk(m_binaryFileFolder.Primarykey);
                return m_primarykeyList;
            }

        }
        public ICollection<TXTable> Values
        {
            get
            {
                if (m_xTableDic.Count != Length)
                    ReadAll();
                return m_xTableDic.Values;
            }

        }

        public int Count => Length;

        public bool IsReadOnly => true;

        public void OnReleaseForFight(bool uload)
        {
            m_xTableDic?.Clear();
            if (uload)
                m_binaryFileFolder.Close();
        }

        TXTable IDictionary<TPKey, TXTable>.this[TPKey key] { get => Get(key); set { } }

        public TXTable Get(TPKey key)
        {
            TXTable element = ReadElement(key);

            if (element == null)
            {
                Logger.LogError($"{m_binaryFileFolder.TableName},{nameof(ReadElement)},查询失败，未找到条目 {m_binaryFileFolder.Primarykey} = {key}");
                return null;
            }
            return element;
        }



        public TXTable TryGetValue(TPKey key)
        {
            return ReadElement(key);
        }
        /// <summary>
        /// 读取单行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private TXTable ReadElement(TPKey value)
        {

            TXTable element;

            if (m_xTableDic.ContainsKey(value))
                return m_xTableDic[value];
            //如果已经加载过
            if (m_xTableDic.TryGetValue(value, out element))
                return element;

            //读取主键列表
            if (m_primarykeyDic == null)
                ReadIndexTrunk(m_binaryFileFolder.Primarykey);

            if (m_primarykeyDic.TryGetValue(value, out int i))
            {
                RowInfo info = m_binaryFileFolder.TryGetRowInfo(i);
                element = ReadElementInner(info, i, value);

            }
            if (element == null)
            {
                return null;
            }


            m_xTableDic.Add(value, element);

            return element;
        }

        TXTable ReadElementInner(RowInfo info, int index, TPKey value)
        {

            int position = m_binaryFileFolder.GetContenTrunkPosition();

            Reader reader = m_binaryFileFolder.ReadRow(info, position);
            if (reader == null)
            {
                Logger.LogError($"{m_binaryFileFolder.TableName},{nameof(ReadElement)} 查询数据失败 {m_binaryFileFolder.Primarykey} = {value}");
                return null;
            }

            TXTable t = new TXTable();
            ITable tableSerilize = t as ITable;
            tableSerilize.Deserialize(reader);

            return t;
        }


        /// <summary>
        /// 全表读取
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Dictionary<TPKey, TXTable> ReadAllContent(Func<TXTable, TPKey> handler)
        {
            if (m_binaryFileFolder == null)
                return null;


            //读取主键列表
            if (m_primarykeyDic == null)
                ReadIndexTrunk(m_binaryFileFolder.Primarykey);

            Reader reader = m_binaryFileFolder.GetContentTrunkReader();

            int row = m_binaryFileFolder.Row;

            Dictionary<TPKey, TXTable> dic = new Dictionary<TPKey, TXTable>(row);

            for (int i = 0; i < row; i++)
            {
                TXTable t = new TXTable();


                ITable tableSerilize = t as ITable;
                tableSerilize.Deserialize(reader);
                TPKey key;

                if (handler != null)
                    key = handler(t);
                else
                    key = m_primarykeyList[i];

                if (!dic.ContainsKey(key))
                    dic.Add(key, t);

            }
            return dic;
        }


        /// <summary>
        /// 全表读取
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        void ReadAll()
        {
            if (m_binaryFileFolder == null)
                return;

            if (m_primarykeyList == null)
                ReadIndexTrunk(m_binaryFileFolder.Primarykey);

            Reader reader = m_binaryFileFolder.GetContentTrunkReader();

            int row = m_binaryFileFolder.Row;
            int col = m_binaryFileFolder.Col;

            //读取主键列表

            m_xTableDic.Clear();

            for (int i = 0; i < row; i++)
            {
                TXTable t = new TXTable();

                ITable tableSerilize = t as ITable;
                tableSerilize.Deserialize(reader);

                TPKey pKey = m_primarykeyList[i];

                if (!m_xTableDic.ContainsKey(pKey))
                    m_xTableDic.Add(pKey, t);
            }
        }

        /// <summary>
        /// 读取索引块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ReadIndexTrunk(string key)
        {
            int len = m_binaryFileFolder.PrimarykeyLen;
            int position = m_binaryFileFolder.GetIndexTrunkPosition();

            if (len <= 0 || position <= 0)
            {
                Logger.LogError($"{m_binaryFileFolder.TableName},{nameof(ReadIndexTrunk)} 读取索引块失败！！key = {key}");
                return false;
            }

            m_primarykeyList = new List<TPKey>(m_binaryFileFolder.Row);
            m_primarykeyDic = new Dictionary<TPKey, int>(m_binaryFileFolder.Row);
            Reader reader = m_binaryFileFolder.GetReader(position);

            for (int i = 0; i < m_binaryFileFolder.Row; i++)
            {
                TPKey temp = BinaryConverter<Reader, TPKey>.Handler(reader);
                if (m_primarykeyDic.ContainsKey(temp))
                {
                    Logger.LogError($"ID 重复 ：{temp} ");
                    continue;
                }
                m_primarykeyList.Add(temp);
                m_primarykeyDic.Add(temp, i);
            }
            //  reader.Close();
            return true;
        }

        public void Release()
        {
#if UNITY_EDITOR
            BinaryProfiler.ReleaseTable(m_binaryFileFolder.TableName);
#endif
            Close();
            m_xTableDic?.Clear();
            m_xTableDic = null;
            m_primarykeyList?.Clear();
            m_primarykeyList = null;

            m_primarykeyDic?.Clear();
            m_primarykeyDic = null;
        }

        public void Add(TPKey key, TXTable value)
        {
            //throw new NotImplementedException();
        }

        public bool ContainsKey(TPKey key)
        {
            if (m_primarykeyDic == null)
                ReadIndexTrunk(m_binaryFileFolder.Primarykey);

            return m_primarykeyDic.ContainsKey(key);
        }

        public bool Remove(TPKey key)
        {
            return false;
        }

        public bool TryGetValue(TPKey key, out TXTable value)
        {
            if (m_xTableDic.TryGetValue(key, out value))
                return true;

            value = TryGetValue(key);

            if (value != null)
                return true;

            return false;
        }

        public void Add(KeyValuePair<TPKey, TXTable> item)
        {
        }

        public void Clear()
        {
            m_xTableDic.Clear();
        }

        public bool Contains(KeyValuePair<TPKey, TXTable> item)
        {
            return m_xTableDic.Contains(item);
        }

        public void CopyTo(KeyValuePair<TPKey, TXTable>[] array, int arrayIndex)
        {

        }

        public bool Remove(KeyValuePair<TPKey, TXTable> item)
        {
            return false;
        }

        public IEnumerator<KeyValuePair<TPKey, TXTable>> GetEnumerator()
        {
            if (m_xTableDic.Count != Length)
                ReadAll();
            return m_xTableDic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (m_xTableDic.Count != Length)
                ReadAll();
            return m_xTableDic.GetEnumerator();
        }
    }

    public class BinaryTable
    {

        static Dictionary<int, string> typeName = new Dictionary<int, string>()
        {
            [1] = "bool",
            [2] = "string",
            [3] = "fix",
            [4] = "List<string>",
            [5] = "List<bool>",
            [6] = "List<int>",
            [7] = "List<float>",
            [8] = "List<fix>",
            [9] = "Dictionary<string,string>",
            [10] = "Dictionary<int,int>",
            [11] = "Dictionary<int,string>",
            [12] = "Dictionary<string,int>",
            [13] = "Dictionary<int,float>",
            [14] = "int",
            [15] = "float",
        };


        /// <summary>
        /// 文件信息夹
        /// </summary>
        protected BinaryFileFolder m_binaryFileFolder;

        /// <summary>
        /// 打开二进制文件
        /// </summary>
        /// <param name="path"></param>
        public virtual bool Open(string path)
        {
            try
            {
                byte[] bytes = BinaryManager.LoadBytes(path);
                if (bytes == null)
                {
                    Logger.LogError($"{path},{nameof(Open)} 二进制文件打开失败！");
                    return false;
                }

                m_binaryFileFolder = new BinaryFileFolder(path, bytes);
                Logger.LogError(m_binaryFileFolder);
            }
            catch (Exception e)
            {
                Logger.LogError($"{path},{nameof(Open)} 二进制文件打开失败！！{e}");
                return false;
            }

            if (m_binaryFileFolder == null)
            {
                return false;
            }
            return true;
        }

        public virtual void Close()
        {
            m_binaryFileFolder?.Close();
            m_binaryFileFolder = null;
        }

        public override string ToString()
        {
            return m_binaryFileFolder.ToString();
        }

        public int Length { get { return m_binaryFileFolder.Row; } }

        public int Col { get { return m_binaryFileFolder.Col; } }

        public ulong Size { get { return m_binaryFileFolder.size; } }

        public ulong ContentSize { get { return m_binaryFileFolder.ContentSize; } }

    }
}