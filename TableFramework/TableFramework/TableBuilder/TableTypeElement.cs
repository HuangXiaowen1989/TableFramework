using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TableTypeElement
{
    public string tablePath;
    public string tableName;
    public string tableRelateName; //相对路径

    public string[] tableColNameArray;
    public string[] tableColClientTypeArray;
    public string[] tableColSeverTypeArray;
    public string[] tableColDescArray;

    public int colCount;
    public int rowCount;

    public Writer headWriter;
    public Writer lineWriter;
    public Writer contentWriter;
    public Writer indexWriter;

    public void Init()
    {
        headWriter = new Writer();
        indexWriter = new Writer();
        lineWriter = new Writer();
        contentWriter = new Writer();

    }

    public void Save(string file)
    {
        headWriter.Write(colCount);//列数
        for (int i = 0; i < tableColNameArray.Length; i++)
        {
            string propertyName = tableColNameArray[i];
            string propertyType = tableColClientTypeArray[i];

            headWriter.Write(propertyName);//列名
            headWriter.Write(propertyType);//类型
        }

        headWriter.Write(indexWriter.Length());
        headWriter.Write(lineWriter.Length());
        headWriter.Write(rowCount);
        headWriter.Write(contentWriter.Length());

        Writer writer = new Writer();
        writer.Write((int)headWriter.Length(), false);
        writer.Write(headWriter);
        writer.Write(indexWriter);
        writer.Write(lineWriter);
        writer.Write(contentWriter);

        string path = Path.GetFullPath($"{file}/{tableRelateName}");

        if (!Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        Logger.LogDebug(path);

        File.WriteAllBytes(path, writer.GetBuffer());

        headWriter.Close();
        contentWriter.Close();
        lineWriter?.Close();
        indexWriter?.Close();

        writer.Close();
    }

}
