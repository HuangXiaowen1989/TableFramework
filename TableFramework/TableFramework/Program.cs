using System;
using System.Collections.Generic;

namespace TableFramework
{
    class Program
    {
        static void Main(string[] args)
        {
           // ========================================
            //生成
            //TableBuilder.BuildTable();
            //Logger.SaveLog();

            BinaryManager.Init();
            TableBarrier table = BinaryManager.GetTableElement<int, TableBarrier>(10101);


            Logger.LogError(table.Id);
            Logger.LogError(table.sectionName);


            Dictionary<int, TableBarrier> iTable = BinaryManager.ReadDictionary<int, TableBarrier>();

            foreach (var item in iTable)
            {
                Logger.LogError($"{item.Value.Id} {item.Value.sectionName} {item.Value.chapterName} {item.Value.costPower}");
            }


            IBinaryTable<int, TableBarrier> iBinaryTable = BinaryManager.ReadTableDic<int, TableBarrier>();

            Logger.LogError(iTable[10101].Id);
            Logger.LogError(iTable[10101].sectionName);

            iBinaryTable.Release();

            Console.ReadLine();
        }
    }
}
