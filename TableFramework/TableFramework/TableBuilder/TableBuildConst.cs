using System.Collections;
using System.Collections.Generic;
using System.IO;
public class EventId
{
    public static readonly string EVENT_PROGRESS_START = "EVENT_PROGRESS_START";
    public static readonly string EVENT_PROGRESS = "EVENT_PROGRESS";
    public static readonly string EVENT_PROGRESS_COMPLETE = "EVENT_PROGRESS_COMPLETE";
}


public enum SvnState
{
    Normal,
    Add,
    Modify,
    ALL
}

public class TableBuildConst
{

    public const string TableSourceDefaultPath = "Table/";
    public const string TableBuildDefaultPath = "Build/";

    public static readonly string BuildSettingFile = "../BuildSetting.xml";
    public static readonly string BuildScriptPath = "Assets/BineryFramework/Gen/";
    public static readonly string LogPath = "BuildLog/";

    public class TableTypeIndex
    {
        public const string String = "string";
        public const string Int = "int";
        public const string Boolean = "bool";

        public const string ListInt = "list<int>";
        public const string ListString = "list<string>";
        public const string ListBoolean = "list<bool>";
        public const string ListIntInt = "list<list<int>>";
        public const string ListStringString = "list<list<string>>";

        public const string DicIntInt = "map<int,int>";
        public const string DicIntString = "map<int,string>";
        public const string DicStringInt = "map<string,int>";
        public const string DicStringString = "map<string,string>";

    }

    public static readonly Dictionary<string, string> ReaderGenTypeDic = new Dictionary<string, string>()
    {
        {TableTypeIndex.String,"reader.ReadString()"},
        {TableTypeIndex.Int,"reader.ReadInt()"},
        {TableTypeIndex.Boolean,"reader.ReadBool()"},
        {TableTypeIndex.ListString,"reader.ReadListString()"},
        {TableTypeIndex.ListInt,"reader.ReadListInt()"},
        {TableTypeIndex.ListBoolean,"reader.ReadListBool()"},
        {TableTypeIndex.ListIntInt,"reader.ReadListIntInt()"},
        {TableTypeIndex.ListStringString,"reader.ReadListStringString()"},

        {TableTypeIndex.DicStringString,"reader.ReadDicStringString()"},
        {TableTypeIndex.DicStringInt,"reader.ReadDicStringInt()"},
        {TableTypeIndex.DicIntInt,"reader.ReadDicIntInt()"},
        {TableTypeIndex.DicIntString,"reader.ReadDicIntString()"},
    };

    public static readonly Dictionary<string, string> ReaderGenFieldDic = new Dictionary<string, string>()
    {
        {TableTypeIndex.String,"public string"},
        {TableTypeIndex.Int,"public int"},
        {TableTypeIndex.Boolean,"public bool"},
        {TableTypeIndex.ListString,"public IReadOnlyList<string>"},
        {TableTypeIndex.ListInt,"public IReadOnlyList<int>"},
        {TableTypeIndex.ListBoolean,"public IReadOnlyList<bool>"},

        {TableTypeIndex.ListIntInt,"public IReadOnlyList<IReadOnlyList<int>>"},
        {TableTypeIndex.ListStringString,"public IReadOnlyList<IReadOnlyList<string>>"},

        {TableTypeIndex.DicStringString,"public IReadOnlyDictionary<string,string>"},
        {TableTypeIndex.DicStringInt,"public IReadOnlyDictionary<string,int>"},
        {TableTypeIndex.DicIntInt,"public IReadOnlyDictionary<int,int>"},
        {TableTypeIndex.DicIntString,"public IReadOnlyDictionary<int,string>"},
    };

}
