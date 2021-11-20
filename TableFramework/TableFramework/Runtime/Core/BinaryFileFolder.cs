using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TableFramework
{
    public struct RowInfo
    {
        public int start;
        public int end;
    }

    public class BinaryFileFolder
    {

        static List<BinaryFileFolder> BinaryFileFolderPool = new List<BinaryFileFolder>();

        private string m_tableName;
        public string TableName
        {
            get { return m_tableName; }
            set { m_tableName = value; }
        }
        public int m_col;
        public int Col
        {
            get { return m_col; }
        }
        public int m_row;
        public int Row
        {
            get { return m_row; }
        }

        public ulong size
        {
            get
            {
                return (ulong)m_reader.Length;
            }
        }



        public ulong ContentSize
        {
            get
            {
                return (ulong)m_contentTrunkLen;
            }
        }

        public string[] m_properyNames;
        public string[] m_properyTypes;

        int m_primarykeyCount = 1; //默认有一个唯一Id
        string m_primarykey;

        public string Primarykey
        {
            get { return m_primarykey; }
        }

        int m_primarykeyLen;
        public int PrimarykeyLen
        {
            get { return m_primarykeyLen; }
        }

        //行信息块长度
        int m_rowInfoTrunkLen;
        int m_infoTrunkLen;
        int m_contentTrunkLen;

        //类型信息
        PropertyInfo[] m_propertyInfos;
        //行位置信息
        RowInfo[] m_rowInfoArray;
        public RowInfo[] RowInfoArray
        {
            get { return TryGetAllRowInfos(); }
        }

        //文件流读取
        Reader m_reader = null;
        public BinaryFileFolder(string path, byte[] bytes)
        {
            m_tableName = path;

            Reader reader = new Reader();
            reader.Load(bytes, 0, bytes.Length);

            m_reader = reader;
            m_reader.ReadFix(ref m_infoTrunkLen);
            InitInfoTrunk();
#if UNITY_EDITOR
            BinaryProfiler.LoadTable(m_tableName, bytes.Length, Row);
#endif
        }

        /// <summary>
        /// 初始化表格信息
        /// </summary>
        void InitInfoTrunk()
        {
            Reader reader = m_reader;
            reader.Read(ref m_col);
            if (m_col <= 0)
            {
                Logger.LogError($"{m_tableName},{nameof(BinaryFileFolder)}.{nameof(InitInfoTrunk)} 失败，数据列为 0");
                return;
            }
            m_properyTypes = new string[m_col];
            m_properyNames = new string[m_col];
            for (int i = 0; i < m_col; i++)
            {
                reader.Read(ref m_properyTypes[i]);
                reader.Read(ref m_properyNames[i]);
            }

            m_primarykey = m_properyNames[0];

            reader.Read(ref m_primarykeyLen);
            reader.Read(ref m_rowInfoTrunkLen);
            reader.Read(ref m_row);
            reader.Read(ref m_contentTrunkLen);
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public PropertyInfo[] GetPropertyInfos<TValue>()
        {
            if (m_propertyInfos != null)
                return m_propertyInfos;

            m_propertyInfos = new PropertyInfo[m_col];

            //排序了
            for (int i = 0; i < m_col; i++)
            {
                m_propertyInfos[i] = typeof(TValue).GetProperty(m_properyNames[i]);
            }

            return m_propertyInfos;
        }


        /// <summary>
        /// 获取索引块
        /// </summary>
        /// <param name="len"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal int GetIndexTrunkPosition()
        {
            int position = m_infoTrunkLen + 4;
            return position;
        }

        public void Reset()
        {
            m_reader?.Reset();
        }

        /// <summary>
        /// 获取内容块位置
        /// </summary>
        /// <returns></returns>
        internal int GetContenTrunkPosition()
        {
            int position = m_infoTrunkLen + 4;
            position += m_primarykeyLen;

            position += m_rowInfoTrunkLen;
            return position;
        }


        public void Close()
        {
            m_reader?.Close();
            m_reader = null;
        }

        /// <summary>
        /// 获取行位置信息
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal RowInfo TryGetRowInfo(int index)
        {
            if (m_rowInfoArray == null)
                ReadRowInfoTrunk();

            if (m_rowInfoArray == null || m_rowInfoArray.Length <= 0)
            {
                Logger.LogError($"{m_tableName},{nameof(TryGetRowInfo)} 读取行位置数据失败");
                return default;
            }

            if (index >= m_rowInfoArray.Length)
            {
                Logger.LogError($"{m_tableName},{nameof(TryGetRowInfo)} 超出总行数长度{m_rowInfoArray.Length},查询长度:{index}");
                return default;
            }

            return m_rowInfoArray[index];
        }




        internal RowInfo[] TryGetAllRowInfos()
        {
            if (m_rowInfoArray == null)
                ReadRowInfoTrunk();

            if (m_rowInfoArray == null || m_rowInfoArray.Length <= 0)
            {
                Logger.LogError($"{m_tableName},{nameof(TryGetRowInfo)} 读取行位置数据失败");
                return default;
            }

            return m_rowInfoArray;
        }



        /// <summary>
        /// 检查索引存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool CheckPrimarykeyExist()
        {
            if (m_primarykeyCount <= 0)
                return false;

            if (string.IsNullOrEmpty(m_primarykey))
                return false;

            return true;
        }

        /// <summary>
        /// 读取每行的位置和长度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        void ReadRowInfoTrunk()
        {
            int len = m_rowInfoTrunkLen;

            int position = m_infoTrunkLen + 4;
  
            position += m_primarykeyLen;

            if (len <= 0 || position <= 0)
            {
                Logger.LogError($"{m_tableName}:{nameof(ReadRowInfoTrunk)} 读取行位置块失败！");
                return;
            }

            m_rowInfoArray = new RowInfo[m_row];
            Reader reader = GetReader(position);
            for (int i = 0; i < m_row; i++)
            {
                RowInfo rowInfo = new RowInfo();
                reader.Read(ref rowInfo.start);
                reader.Read(ref rowInfo.end);
                m_rowInfoArray[i] = rowInfo;
            }
        }


        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("表名:").Append(m_tableName).Append(Environment.NewLine);
            stringBuilder.Append("-------------------------------------------------------").Append(Environment.NewLine);
            stringBuilder.Append($" 描述块长度{m_infoTrunkLen}").Append(" 列数:").Append(m_col).Append(" 行数:").Append(m_row).Append(Environment.NewLine);

            for (int i = 0; i < m_properyNames.Length; i++)
            {
                stringBuilder.Append(m_properyNames[i]).Append(":").Append(m_properyTypes[i]).Append(" ");
            }
            stringBuilder.Append(Environment.NewLine);

            if (m_primarykeyCount > 0)
            {
                stringBuilder.Append($"有{m_primarykeyCount}个索引").Append(":");
                stringBuilder.Append(m_primarykey).Append(": 索引块长度").Append(m_primarykeyLen);
            }
            else
            {


                stringBuilder.Append($"无索引").Append($"行索引长度:{m_rowInfoTrunkLen}");
            }

            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("内容块长度:").Append(m_contentTrunkLen).Append(Environment.NewLine);
            stringBuilder.Append("-------------------------------------------------------");

            return stringBuilder.ToString();
        }


        #region
        /// <summary>
        /// 读取内存块
        /// </summary>
        /// <param name="length"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        internal Reader GetReader(long startIndex = -1)
        {
            if (m_reader == null)
            {
                byte[] bytes = BinaryManager.LoadBytes(m_tableName);
                Reader reader = new Reader();
                reader.Load(bytes, 0, bytes.Length);
                m_reader = reader;

            }

            m_reader.index = (int)startIndex;
            return m_reader;
        }

        internal Reader GetContentTrunkReader()
        {
            int position = GetContenTrunkPosition();
            Reader reader = GetReader(position);
            return reader;
        }

        /// <summary>
        /// 读取行数据块
        /// </summary>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal Reader ReadRow(RowInfo info, int offset)
        {
            if (info.start < 0 || info.end <= 0)
            {
                Logger.LogError($"{m_tableName},{nameof(ReadRow)} 行数据异常 start = {info.start},end ={info.end}");
                return null;
            }

            int len = info.end - info.start;
            int startIndex = offset + info.start;
            return GetReader(startIndex);
        }

        #endregion
    }
}