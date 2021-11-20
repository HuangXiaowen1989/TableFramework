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

