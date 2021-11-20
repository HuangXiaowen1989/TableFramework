using System.Collections;
using System.Collections.Generic;

public class TableBuildSetting //:  JsonDataBase<TableBuildSetting>
{
    public TableBuildSetting()
    {
        TableBinaryOutPutPath = TableBuildConst.TableBuildDefaultPath;
        TableSourcePath = TableBuildConst.TableSourceDefaultPath;
        TabBuildScriptPath = TableBuildConst.BuildScriptPath;
    }

    public string TableBinaryOutPutPath; //输出路径
    public string TableSourcePath;    //原Excel路径
    public string TabBuildScriptPath; //生成脚本路径
}
