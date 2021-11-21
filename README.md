# TableFramework
## Excel->二进制 表格生成和读写方案（可以在Unity中使用，lua也可以使用）
  在 Unity 游戏项目中，随着版本的推移，发现用到的数值表越来越多，由于项目之初没有做好加载数值表的管理（均在游戏开始就把数值表全部加载），导致内存越来越大。
  在不改变原来的旧得加载流程的情况下，想到的一个思路：支持数值表按需动态加载某一条数据（某一行），假如原来数值表有1000条目，正常操作一般都是整表IO然后整表反序列，相当于这1000条条目无论是否访问到都会被加载进内存。
  
  首先想到的一个思路是Sqlcipher（sqlite开源库，支持加密）
  
  优点：
  
    1. 支持动态查询。如："select * from 'XTableActivity' Where "Id" = 10001"
    
    2. 业务上可以达到减少内存目的。
    
  缺点（需要解决的问题）：
  
    1. 引入第三方库（深渊巨坑）。虽然Sqlcipher官方提供了源码和编译方案，但是真正编译多平台库实在太过硬核，需要解决来自各平台奇奇怪怪的问题，尤其Sqlcipher还依赖另一个第三方库OPENSSL用于加密。
    
    2. Lua兼容性问题。lua对原版sqlite有比较好的支持，但是对于Sqlcipher，需要编写部分c源码，增加加密接口绑定。除了增加代码支持，还有一个更加操蛋的问题，如果把sql数据的加载全部写在C#中，在lua对数据的访问就会很不方便，一方面，lua call C#存在一个效率问题，不能频繁查询，另一方面数据缓存，在经过lua call C#过程中也产生了大量的缓存占用。所以只能把sql的加载解释部分写到lua里面，这个又回到了第一个问题，首先要升级xlua，然后需要把Sqlcipher源码，luaSqlite源码编译到xlua里面，多平台编译！
    
    3. 加密。因为Sqlite是比较大众的存储方案，市面上随意一个数据库软件都可以轻松打开Sqlite，所以他的文件毫无秘密可言，又由于Sqlite只提供路径加载接口，所以无法使用自定义的加密方案。所以使用Sqlcipher的加密方案。当解决上面的所有问题后，发现，加密后的数据加载速度比没加密慢了十几倍。
    
    4. 需要存在可读写目录。也就是说不能随包打包。
    
    5. 出现问题难以定位问题。毕竟Unity调试库是一件非常麻烦的事。
    
  除了上述的问题外，还有其他零零碎碎的问题。关于Sql方案大家对那个环节（建立Sql，查询，多平台编译库，lua源码）有兴趣可以跟我交流。
  
  么得办法，最后决定自己模仿Sql造轮子。
  ## 目前该方案的设计：
  ### 存储顺序：【描述内存块】【索引内存块】【行索引内存块】【内容内存块】
  
    【描述内存块】=【描述块长度(int32 不可变长)】【列数(int)】【列1类型(int) 名字(string)】 【列2类型(int) 名字(string)】... 【索引(string) 长度(int)】【行索引块长度(int)】【总行数(int)】【内容块长度(int64)】
    
    【索引内存块】=【主键(具体类型看索引 如:1001)】 【主键(1002)】 【主键(1003)】...... 
    
    【行索引内存块】=【start end】【start end】【start end】...... 
    
    【内容内存块】=【行1.......】【行2.......】【行3.....】....
 ### 数据存储方式
    1. 数值类型 int float bool 使用变长参数正整数保存，牺牲最高位，1代表下一个字节也是属于当前正整数。
    如：128 = 1000000 00000001 00000000  00000000
               0       128
    需要注意的问题负数，负数的保存使用了补码，也就是一个很大的Int32数，正整数。
    Int32补码：uint temp = value < 0 ? (uint)(~Math.Abs(value) + 1) : (uint)value;
    在Int32中会将第一位作为符号位
    如 -1 = 1111 1111 1111 1111 1111 1111 1111 1111 在C#中读取四字节存放到int32中，取值是正确的-1.当时在lua中，number是8字节的long类型，这时候lua取出来的值就是错误的 4,294,967,295。

    2. float 暂时 *10000，存在误差值，float不应该存在数值表当中，应该以千分比或者万分比表示，由业务去解释。
    
    3. fix使用字符串保存。为了不改变定点数。
    
    4. 字符串string 使用utf8保存
      英文 1字节 0 - 127   
      中文 3字节 128以上   
### 数据类型  
       字符串 public const string String = "string";
       整形 public const string Int = "int";
       布尔 public const string Boolean = "bool";
       列表:使用标准的Json格式配表 
            public const string ListInt = "list<int>";
            public const string ListString = "list<string>";
            public const string ListBoolean = "list<bool>";
            public const string ListIntInt = "list<list<int>>";
            public const string ListStringString = "list<list<string>>";
       字典：使用标准的Json格式配表     
            public const string DicIntInt = "map<int,int>";
            public const string DicIntString = "map<int,string>";
            public const string DicStringInt = "map<string,int>";
            public const string DicStringString = "map<string,string>";
### 配表
  ![image](https://github.com/SihaoLiang/TableFramework/blob/main/Icons/table1.png)  
    第一行 中文描述
    第二行 字段名称
    第三行 客户端类型 可以不填，不填就不导出
    第四行 服务端类型
  
### 导表
    导表配置：[BuildSetting.xml]{https://github.com/SihaoLiang/TableFramework/blob/main/TableFramework/TableFramework/bin/BuildSetting.xml}
    
      <?xml version="1.0" encoding="utf-8"?>
        <TableBuildSetting xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
          <TableBinaryOutPutPath>../Build/Binary/</TableBinaryOutPutPath>//导出目录
          <TableSourcePath>../../Tool/table/</TableSourcePath>//Excel目录
          <TabBuildScriptPath>Assets\BinaryFramework\Gen\</TabBuildScriptPath>//生成文件
        </TableBuildSetting>
        
      导表接口:'TableBuilder.BuildTable();'
        
  ### 读取查询
  统一管理：可以自定管理方案
  
    TableBarrier table = BinaryManager.GetTableElement<int, TableBarrier>(10101);
  
  全表读取：适合频繁操作的数值表，需要遍历的表格
  
    Dictionary<int, TableBarrier> iTable = BinaryManager.ReadDictionary<int, TableBarrier>();

  自行管理：适合按需加载卸载
  
    IBinaryTable<int, TableBarrier> iBinaryTable = BinaryManager.ReadTableDic<int, TableBarrier>();
    iBinaryTable.Release();

  ### 导出文件
    using System.Collections.Generic;
    using TableFramework;
    
    [Table("Barrier.bytes")]
    public partial class TableBarrier : ITable 
    {
      [Tooltip("ID")]
      public int Id;
      [Tooltip("章节编号")]
      public int chapterId;
      [Tooltip("章节名称")]
      public string sectionName;
      [Tooltip("关卡名称")]
      public string chapterName;
      [Tooltip("对应等级")]
      public int level;
      [Tooltip("每天进入次数")]
      public int limit;
      [Tooltip("单次进入消耗体力")]
      public int costPower;
      [Tooltip("关卡类型")]
      public int type;
      [Tooltip("关卡类型（策划用）")]
      public string typeDesc;
      [Tooltip("推荐战力")]
      public int fightForce;
      [Tooltip("时间限制")]
      public int time;
      [Tooltip("怪物编号")]
      public int monsters;
      [Tooltip("通关奖励")]
      public IReadOnlyList<IReadOnlyList<int>> reward;
      [Tooltip("章节图(资源)")]
      public string map;

      public void Deserialize(Reader reader)
      {
        Id = reader.ReadInt();
        chapterId = reader.ReadInt();
        sectionName = reader.ReadString();
        chapterName = reader.ReadString();
        level = reader.ReadInt();
        limit = reader.ReadInt();
        costPower = reader.ReadInt();
        type = reader.ReadInt();
        typeDesc = reader.ReadString();
        fightForce = reader.ReadInt();
        time = reader.ReadInt();
        monsters = reader.ReadInt();
        reward = reader.ReadListIntInt();
        map = reader.ReadString();
      }

    }

    交流或者联系 QQ 928441097
