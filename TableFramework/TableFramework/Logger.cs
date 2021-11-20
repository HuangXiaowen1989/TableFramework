using System;
using System.IO;
using System.Text;

public class Logger
{
    static StringBuilder LogStringBuilder = new StringBuilder();
    static StringBuilder LogWarningStringBuilder = new StringBuilder();
    static StringBuilder LogErrorStringBuilder = new StringBuilder();

    const string  LogFile = "../Logs";

    public static void LogImp(string str)
    {
        lock (LogStringBuilder)
        {
            LogStringBuilder.Append(DateTime.Now.ToString()).Append(" ").Append(str).AppendLine(); ;
        }
        Console.WriteLine(str);

    }

    public static void LogWarnImp(string str)
    {

        lock (LogWarningStringBuilder)
        {
            LogStringBuilder.Append(DateTime.Now.ToString()).Append(" ").Append(str).AppendLine();
        }

        Console.WriteLine(str);

    }

    public static void LogErrorImp(string str)
    {
        lock (LogErrorStringBuilder)
        {
            LogStringBuilder.Append(DateTime.Now.ToString()).Append(" ").Append(str).AppendLine(); ;
        }
        Console.WriteLine(str);
    }

    public static void LogDebug(string str)
    {
        LogImp($"[Debug] {str}");
    }

    public static void LogError(string str)
    {
        LogErrorImp($"[Error] {str}");
    }

    public static void LogWarning(string str)
    {
        LogWarnImp($"[Warning] {str}");
    }

    public static void LogException(string str)
    {
        throw new Exception($"[Exception] {str}");
    }
    public static void LogErrorFormat(string content, params object[] objs)
    {
        LogErrorImp(string.Format($"[Error]{content} ",objs));
    }

    public static void LogError(object obj)
    {
        LogErrorImp($"[Error] {obj.ToString()}");
    }


    /// <summary>
    /// 保存输出
    /// </summary>
    public static void SaveLog()
    {
        string logFile = $"{LogFile}/";
        if (!Directory.Exists(logFile))
            Directory.CreateDirectory(logFile);


        Utility.WriteFileEncoding($"{logFile}{DateTime.Now.ToString("yyyyMMddhhmmss")}.txt", LogStringBuilder.ToString(), Encoding.UTF8);
    }

    public static void Clear()
    {
        LogWarningStringBuilder.Clear();
        LogErrorStringBuilder.Clear();
        LogStringBuilder.Clear();
    }
}
