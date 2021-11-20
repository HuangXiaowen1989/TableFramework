using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

public class IExcel
{

}

public class ExcelHelper
{

    /// <summary>
    /// 将Excel导入Dictionary
    /// </summary>
    /// <param name="filePath">导入的文件路径（包括文件名）</param>
    /// <param name="key">键值</param>
    /// <returns>DataTable</returns>
    public static string[][] ReadTable(string filePath)
    {

        ISheet sheet = null;//工作表

        if (!File.Exists(filePath))
        {
            Logger.LogDebug($"表格:{filePath} 不存在！！！");
            return null;
        }

        FileStream fs;
        using (fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
        {
            IWorkbook workbook = null;

            if (filePath.EndsWith(".xlsx"))
                workbook = new XSSFWorkbook(fs);
            else if (filePath.EndsWith(".xls"))
                workbook = new HSSFWorkbook(fs);

            int count = workbook.NumberOfSheets; //获取所有SheetName

            List<string[]> table = new List<string[]>();
            int sheetIndex = 0;
            //for (int sheetIndex = 0; sheetIndex < count; sheetIndex++)
            //{
            sheet = workbook.GetSheetAt(sheetIndex);

            if (sheet == null)
                return null;

            //第一行列名
            IRow firstRow = sheet.GetRow(0);
            if (firstRow == null)
                return null;


            int colCount = firstRow.LastCellNum; //行最后一个cell的编号 即总的列数
            int rowCount = sheet.LastRowNum;

            //读数据行
            for (int i = 0; i <= rowCount; i++)
            {
                IRow rowData = sheet.GetRow(i);
                if (rowData == null)
                {
                    Logger.LogError($"Table:{filePath} 第{i}行 空行");
                    continue;
                }

                string[] colContent = new string[colCount];

                ICell firstCell = rowData.GetCell(0);
                if (firstCell == null || string.IsNullOrEmpty(firstCell.ToString()))
                    continue;

                for (int j = 0; j < colCount; j++)
                {
                    ICell cell = rowData.GetCell(j);
                    if (cell == null)
                        continue;


                    if (cell.CellType == CellType.Numeric)
                        colContent[j] = cell.NumericCellValue.ToString();
                    if (cell.CellType == CellType.Formula)
                        colContent[j] = cell.NumericCellValue.ToString();
                    else
                        colContent[j] = cell.ToString();

                }


                table.Add(colContent);

            }


            string[][] temp = new string[table.Count][];

            for (int i = 0; i < table.Count; i++)
            {
                temp[i] = table[i];
            }
            return temp;
        }

    }

    /// <summary>
    /// 将DataTable导入到Excel
    /// </summary>
    /// <param name="data">要导入的数据</param>
    /// <param name="filepath">导入的文件路径（包含文件名称）</param>
    /// <returns>导入Excel的行数</returns>
    public static IWorkbook WriteTable(DataTable dataTable, string filePath)
    {

        if (File.Exists(filePath))
            File.Delete(filePath);


        string path = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        ISheet sheet = null;
        FileStream fs;
        int index = 0;
        IWorkbook workbook = null;
        using (fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {

            workbook = new XSSFWorkbook();

            if (workbook != null)
            {
                sheet = workbook.CreateSheet();
            }
            else
            {
                return null;
            }

            int rowCount = dataTable.Rows.Count;//行数  
            int columnCount = dataTable.Columns.Count;//列数  

            IRow headRow = sheet.CreateRow(index);
            if (index == 0)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    DataColumn dataColumn = dataTable.Columns[j];
                    headRow.CreateCell(j).SetCellValue(dataColumn.ColumnName);
                }
                index++;
            }

            for (int i = 0; i < rowCount; i++)
            {
                IRow row = sheet.CreateRow(index);
                for (int j = 0; j < columnCount; j++)
                {
                    row.CreateCell(j).SetCellValue(dataTable.Rows[i][j].ToString());
                }
                index++;
            }
            workbook.Write(fs); //写入到excel
            workbook.Close();
        }
        return workbook;

    }



    /// <summary>
    /// 将DataTable导入到Excel
    /// </summary>
    /// <param name="data">要导入的数据</param>
    /// <param name="filepath">导入的文件路径（包含文件名称）</param>
    /// <returns>导入Excel的行数</returns>
    public static IWorkbook WriteTable<A, T>(Dictionary<A, T> dic, string filePath) where T : IExcel
    {

        if (File.Exists(filePath))
            File.Delete(filePath);

        ISheet sheet = null;
        FileStream fs;
        int index = 0;
        IWorkbook workbook = null;
        using (fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {

            workbook = new XSSFWorkbook();

            if (workbook != null)
            {
                sheet = workbook.CreateSheet();
            }
            else
            {
                return null;
            }


            foreach (var item in dic)
            {
                IRow row = sheet.CreateRow(index);
                T rowData = item.Value;
                if (rowData == null)
                    continue;

                PropertyInfo[] propertys = rowData.GetType().GetProperties();
                if (index == 0)
                {
                    for (int j = 0; j < propertys.Length; j++)
                    {
                        row.CreateCell(j).SetCellValue(propertys[j].Name);
                    }
                    index++;
                    row = sheet.CreateRow(index);
                }

                for (int i = 0; i < propertys.Length; i++)
                {
                    PropertyInfo property = propertys[i];
                    if (property == null)
                        continue;

                    if (property.GetValue(rowData) == null)
                        continue;

                    row.CreateCell(i).SetCellValue(propertys[i].GetValue(rowData).ToString());
                }
                index++;
            }
            workbook.Write(fs); //写入到excel
            workbook.Close();
        }
        return workbook;

    }




    /// <summary>
    /// 将DataTable导入到Excel
    /// </summary>
    /// <param name="data">要导入的数据</param>
    /// <param name="filepath">导入的文件路径（包含文件名称）</param>
    /// <returns>导入Excel的行数</returns>
    public static IWorkbook WriteTable<A, T>(Dictionary<A, T> dic, string filePath, string sheetName) where T : IExcel
    {

        if (File.Exists(filePath))
            File.Delete(filePath);

        ISheet sheet = null;
        FileStream fs;
        int index = 0;
        IWorkbook workbook = null;
        using (fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            workbook = new XSSFWorkbook();

            if (workbook != null)
            {
                sheet = workbook.CreateSheet(sheetName);
            }
            else
            {
                return null;
            }


            foreach (var item in dic)
            {
                IRow row = sheet.CreateRow(index);
                T rowData = item.Value;

                if (rowData == null)
                    continue;

                PropertyInfo[] propertys = rowData.GetType().GetProperties();
                if (index == 0)
                {
                    for (int j = 0; j < propertys.Length; j++)
                    {
                        row.CreateCell(j).SetCellValue(propertys[j].Name);
                    }
                    index++;
                    row = sheet.CreateRow(index);
                }

                for (int i = 0; i < propertys.Length; i++)
                {
                    PropertyInfo property = propertys[i];
                    if (property == null)
                        continue;

                    if (property.GetValue(rowData) == null)
                        continue;

                    row.CreateCell(i).SetCellValue(propertys[i].GetValue(rowData).ToString());
                }
                index++;
            }
            workbook.Write(fs); //写入到excel
        }
        return workbook;

    }
    /// <summary>
    /// 将DataTable导入到Excel
    /// </summary>
    /// <param name="data">要导入的数据</param>
    /// <param name="filepath">导入的文件路径（包含文件名称）</param>
    /// <returns>导入Excel的行数</returns>
    public static IWorkbook AppeddWriteTable<A, T>(IWorkbook workbook, Dictionary<A, T> dic, string filePath, string sheetName) where T : IExcel
    {
        ISheet sheet = null;
        FileStream fs;
        int index = 0;

        if (workbook == null)
            return workbook;

        using (fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {

            if (workbook != null)
            {
                sheet = workbook.CreateSheet(sheetName);
            }
            else
            {
                return workbook;
            }


            foreach (var item in dic)
            {
                IRow row = sheet.CreateRow(index);
                T rowData = item.Value;
                if (rowData == null)
                    continue;

                PropertyInfo[] propertys = rowData.GetType().GetProperties();
                if (index == 0)
                {
                    for (int j = 0; j < propertys.Length; j++)
                    {
                        row.CreateCell(j).SetCellValue(propertys[j].Name);
                    }
                    index++;
                    row = sheet.CreateRow(index);
                }

                for (int i = 0; i < propertys.Length; i++)
                {
                    PropertyInfo property = propertys[i];
                    if (property == null)
                        continue;

                    if (property.GetValue(rowData) == null)
                        continue;

                    row.CreateCell(i).SetCellValue(propertys[i].GetValue(rowData).ToString());
                }
                index++;
            }
            workbook.Write(fs); //写入到excel
        }
        return workbook;
    }
}
