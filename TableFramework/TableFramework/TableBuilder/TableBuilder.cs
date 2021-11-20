using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TableFramework;
public class TableBuilder
{
    public static class XConverter<T1, T2>
    {
        public static Func<T1, T2> Handler;
    }

    static Dictionary<string, TableTypeElement> TableTypeElementDic = new Dictionary<string, TableTypeElement>();

    const int startCol = 0;
    const int startRow = 4;

    static HashSet<int> ValidColumn = new HashSet<int>();

    public static TableBuildSetting BuildSetting = null;

    public static void Init()
    {
        BinaryHelper.Init();
        BuildSetting = XmlHelper.Read<TableBuildSetting>(TableBuildConst.BuildSettingFile);
        if (BuildSetting == null)
            BuildSetting = new TableBuildSetting();

        XConverter<string, string>.Handler = value => value;
        XConverter<int, string>.Handler = value => string.Intern(value.ToString());
        XConverter<string, bool>.Handler = value => !string.IsNullOrEmpty(value) && int.Parse(value) != 0;
        XConverter<string, int>.Handler = value => string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
        XConverter<int, int>.Handler = value => value;
        XConverter<string, float>.Handler = value => string.IsNullOrEmpty(value) ? 0f : float.Parse(value);

    }

    private static class XAccessor
    {

        public static TProperty SetSingleProperty<TProperty>(string value)
        {
            return XConverter<string, TProperty>.Handler(value);
        }

        public static List<TListType> SetListProperty<TListType>(string value)
        {
            List<TListType> list = new List<TListType>();

            if (string.IsNullOrEmpty(value))
            {
                return list;
            }

            //if (!value.StartsWith("[") || !value.EndsWith("]"))
            //    return list;

            //value = value.Substring(1, value.Length - 2);

            //string[] values = value.Split(',');


            list = JsonConvert.DeserializeObject<List<TListType>>(value);

            //if (value == null || value.Length <= 0)
            //{
            //    throw new NullReferenceException($"{nameof(XAccessor)} {nameof(SetListProperty)} {nameof(value)} is null");
            //}

            if (list == null)
            {
                throw new NullReferenceException($"{nameof(XAccessor)} {nameof(SetListProperty)} {nameof(list)} is null");
            }

            //for (int i = 0; i < values.Length; i++)
            //{
            //    string temp = values[i];
            //    if (!string.IsNullOrEmpty(temp))
            //    {
            //        list.Add(XConverter<string, TListType>.Handler(temp));
            //    }

            //}
            return list;
        }

        public static Dictionary<TKey, TValue> SetDictionaryProperty<TKey, TValue>(string value)
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

            if (string.IsNullOrEmpty(value))
                return dictionary;


            dictionary = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(value);


            //value = value.Substring(1, value.Length - 2);


            //string[] values = value.Split(',');

            //if (values == null || values.Length <= 0)
            //{
            //    return dictionary;
            //}

            //if (dictionary == null)
            //{
            //    throw new NullReferenceException($"{nameof(XAccessor)} {nameof(SetDictionaryProperty)} {nameof(dictionary)} is null");
            //}

            //for (int i = 0; i < values.Length; i++)
            //{
            //    string temp = values[i];

            //    int signIndex = temp.IndexOf("=");

            //    string tempKey = temp.Substring(0, signIndex);
            //    string tempValue = temp.Substring(signIndex + 1, temp.Length - (signIndex + 1));

            //    tempKey = tempKey.Trim();
            //    tempValue = tempValue.Trim();

            //    if (!string.IsNullOrEmpty(tempKey) && !string.IsNullOrEmpty(tempValue))
            //    {
            //        dictionary.Add(XConverter<string, TKey>.Handler(tempKey), XConverter<string, TValue>.Handler(tempValue));
            //    }

            //}
            return dictionary;
        }



    }

    public static string[] CollectTableOnly()
    {
        string suffix = "*.xlsx";
        string[] tablesPath = Directory.GetFiles(BuildSetting.TableSourcePath, suffix, SearchOption.AllDirectories);
        return tablesPath;
    }

    public static void BuildTable()
    {
        try
        {
            Init();
            string[] tables = Directory.GetFiles(BuildSetting.TableSourcePath, "*.xlsx", SearchOption.AllDirectories);
            BuildTableWithFilePath(tables);
            WrithAllTableInBinary(TableTypeElementDic);
            BuildBinaryGen(TableTypeElementDic);

        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
        }

        Logger.LogDebug("导出完成");
    }


    static TableTypeElement ConvertTableToBinary(string filePath, string[][] content)
    {
        if (content == null || content.Length <= 3)
        {
            Logger.LogError($"{filePath} 是空表");
            return null;
        }

        ValidColumn.Clear();

        //表头
        List<string> names = new List<string>();
        List<string> types = new List<string>();
        List<string> descs = new List<string>();

        string[] firstRow = content[0];



        string[] secondRow = content[1];

        for (int idx = startCol; idx < secondRow.Length; idx++)
        {
            string cellValue = secondRow[idx];
            if (string.IsNullOrEmpty(cellValue))
            {
                Logger.LogWarning($"Table:{filePath} 第{idx}列 表头为空！！");
                continue;
            }

            if (names.Contains(cellValue))
            {
                Logger.LogError($"Table:{filePath} 第{idx}列 字段名不能重复:{cellValue}");
                break;
            }

            string desc = string.Empty;
            if (firstRow.Length > idx)
            {
                desc = firstRow[idx];
            }

            names.Add(cellValue);
            descs.Add(desc);
        }

        //第二行客户端类型信息secondRow 
        string[] triRow = content[2];

        for (int idx = startCol; idx < triRow.Length; idx++)
        {
            string cellValue = triRow[idx];

            if (!string.IsNullOrEmpty(cellValue))
            {
                ValidColumn.Add(idx);
            }
            types.Add(cellValue.ToLower());
        }

        int validColumnCount = ValidColumn.Count;

        TableTypeElement tableTypeElement = new TableTypeElement();
        //初始化行列,表头
        tableTypeElement.Init();
        string[] validNames = new string[validColumnCount];
        string[] validTypes = new string[validColumnCount];
        string[] validDesc = new string[validColumnCount];

        int validIdx = 0;
        for (int i = 0; i < names.Count; i++)
        {
            if (!ValidColumn.Contains(i + startCol))//过滤无效的列
                continue;

            validNames[validIdx] = names[i];
            validTypes[validIdx] = types[i];
            validDesc[validIdx] = descs[i];

            validIdx++;
        }

        tableTypeElement.colCount = validIdx;
        tableTypeElement.tableColNameArray = validNames;
        tableTypeElement.tableColClientTypeArray = validTypes;
        tableTypeElement.tableColDescArray = validDesc;
        //=============================================================

        string primaryKey = validNames[0];
        Writer writer = tableTypeElement.contentWriter;
        for (int index = startRow; index < content.Length; index++)
        {
            string[] row = content[index];
            Writer indexWriter = null;
            int start = writer.position;

            for (int j = startCol; j < row.Length; j++)
            {
                if (!ValidColumn.Contains(j))//过滤无效的列
                    continue;
                int idx = j - startCol;
                if (idx < 0 || idx >= names.Count)
                {
                    Logger.LogError($"{filePath}:获取属性名字失败 第 {idx} 列");
                    continue;
                }
                string propertyName = names[j - startCol];
                //制作索引
                if (primaryKey.Equals(propertyName))
                    indexWriter = tableTypeElement.indexWriter;
                else
                    indexWriter = null;


                string propertyType = types[j - startCol];
                string val = row[j];
                try
                {
                    if (propertyType.Equals(TableBuildConst.TableTypeIndex.Int))
                    {
                        int value = XAccessor.SetSingleProperty<int>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }
                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.String))
                    {
                        string value = XAccessor.SetSingleProperty<string>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);

                    }
                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.Boolean))
                    {
                        bool value = XAccessor.SetSingleProperty<bool>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }

                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.ListInt))
                    {
                        List<int> value = XAccessor.SetListProperty<int>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }
                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.ListString))
                    {
                        List<string> value = XAccessor.SetListProperty<string>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }

                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.ListBoolean))
                    {
                        List<bool> value = XAccessor.SetListProperty<bool>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }

                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.DicIntInt))
                    {
                        Dictionary<int, int> value = XAccessor.SetDictionaryProperty<int, int>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }
                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.DicIntString))
                    {
                        Dictionary<int, string> value = XAccessor.SetDictionaryProperty<int, string>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }
                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.DicStringInt))
                    {

                        Dictionary<string, int> value = XAccessor.SetDictionaryProperty<string, int>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);

                    }
                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.DicStringString))
                    {
                        Dictionary<string, string> value = XAccessor.SetDictionaryProperty<string, string>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }

                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.ListIntInt))
                    {
                        List<List<int>> value = XAccessor.SetListProperty<List<int>>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }

                    else if (propertyType.Equals(TableBuildConst.TableTypeIndex.ListStringString))
                    {
                        List<List<string>> value = XAccessor.SetListProperty<List<string>>(val);
                        writer?.Write(value);
                        indexWriter?.Write(value);
                    }

                }
                catch (Exception e)
                {
                    Logger.LogError($"{filePath}:{propertyName} = {val} 不能转换到类型: {propertyType} {e}");
                }
            }

            tableTypeElement.lineWriter?.Write(start);
            tableTypeElement.lineWriter?.Write(writer.position);
            tableTypeElement.rowCount++;
        }
        return tableTypeElement;
    }

    static void BuildTableWithFilePath(string[] files)
    {
        TableTypeElementDic.Clear();
        if (files == null || files.Length <= 0)
        {
            Logger.LogError("请选择需要导出的Excel!!!");
            return;
        }
        int len = files.Length;

        string tablePath = BuildSetting.TableSourcePath;

        for (int i = 0; i < len; i++)
        {
            string path = files[i];
            if (string.IsNullOrEmpty(path))
                continue;
            string relatePath = path.Replace(tablePath, "").Replace("xlsx", "bytes");

            string fileName = Path.GetFileNameWithoutExtension(path);
            string[][] content = null;
            try
            {
                content = ExcelHelper.ReadTable(path);
            }
            catch (Exception exce)
            {
                Logger.LogError($"表格读取失败:{path}，是否被WPS打开保存过{exce}");
                continue;
            }

            TableTypeElement element = ConvertTableToBinary(fileName, content);
            Logger.LogDebug($"转换二进制：{path}");


            element.tableRelateName = relatePath;
            element.tableName = fileName;
            element.tablePath = path;
            TableTypeElementDic.Add(path, element);
        }
    }


    static void WrithAllTableInBinary(Dictionary<string, TableTypeElement> tables)
    {

        Logger.LogDebug($"开始写入二进制");

        foreach (var item in tables)
        {
            item.Value.Save(BuildSetting.TableBinaryOutPutPath);
        }
    }


    public static void BuildBinaryGen(Dictionary<string, TableTypeElement> tables)
    {
        Logger.LogDebug("生成反序列化文件");

        StringBuilder buildCode = new StringBuilder();
        StringBuilder stringFieldBuilder = new StringBuilder();
        StringBuilder stringMethodBuilder = new StringBuilder();


        foreach (var t in tables)
        {
            stringFieldBuilder.Clear();
            stringMethodBuilder.Clear();
            buildCode.Clear();

            buildCode.AppendLine("using System.Collections.Generic;\r\nusing System;\r\nusing TableFramework;").AppendLine();
            buildCode.AppendLine($"[Table(\"{t.Value.tableRelateName.Replace("\\","/")}\")]");
            buildCode.Append($"public partial class Table{t.Value.tableName} : ITable \r\n{{").AppendLine();

            for (int i = 0; i < t.Value.colCount; i++)
            {
                string fieldName = t.Value.tableColNameArray[i];
                string fieldType = t.Value.tableColClientTypeArray[i];
                string fieldDesc = t.Value.tableColDescArray[i];


                if (!TableBuildConst.ReaderGenFieldDic.ContainsKey(fieldType))
                {
                    Logger.LogError($"表格：{t.Value.tableName},TableBuildConst.ReaderGenFieldDic 找不到类型：{fieldType}");
                    continue;
                }
                if (!TableBuildConst.ReaderGenTypeDic.ContainsKey(fieldType))
                {
                    Logger.LogError($"表格：{t.Value.tableName},TableBuildConst.ReaderGenTypeDic 找不到类型：{fieldType}");
                    continue;
                }

                string fieldTypeStr = TableBuildConst.ReaderGenFieldDic[fieldType];
                string fieldMethodStr = TableBuildConst.ReaderGenTypeDic[fieldType];

                stringFieldBuilder.AppendLine($"\t[Tooltip(\"{fieldDesc}\")]").Append($"\t{fieldTypeStr} {fieldName};").AppendLine();
                stringMethodBuilder.Append($"\t\t{fieldName} = {fieldMethodStr};").AppendLine();
            }

            buildCode.Append(stringFieldBuilder).AppendLine();
            buildCode.Append("\tpublic void Deserialize(Reader reader)\r\n\t{").AppendLine();
            buildCode.Append(stringMethodBuilder);

            buildCode.Append("\t}").AppendLine();
            buildCode.Append("}").AppendLine(); ;

            buildCode.AppendLine();
            string path = $"{BuildSetting.TabBuildScriptPath}/Table{t.Value.tableName}.cs";

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            Utility.WriteFileEncoding(path, buildCode.ToString(), Encoding.UTF8);
        }

        Logger.LogDebug("生成反序列化文件完成");


        // AssetDatabase.Refresh();
    }


}
