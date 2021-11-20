using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class Utility
{

    public static readonly ParallelOptions ParallelOptions = new ParallelOptions()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount * 2
    };
    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="str"></param>
    /// <param name="encoding"></param>
    public static void WriteFileEncoding(string path, string str, Encoding encoding)
    {
        string fullPath = Path.GetFullPath(path);

        FileMode fileMode = FileMode.OpenOrCreate;
        if (File.Exists(fullPath))
            fileMode = FileMode.Truncate;

        using (FileStream fileStream = new FileStream(fullPath, fileMode, FileAccess.Write))
        {
            byte[] buffer = encoding.GetBytes(str);
            fileStream.Position = 0;
            fileStream.Write(buffer, 0, buffer.Length);
        }
    }
    public static bool DeleteAllFile(string fullPath)
    {
        //获取指定路径下面的所有资源文件  然后进行删除
        if (Directory.Exists(fullPath))
        {
            string[] files = Directory.GetFiles(fullPath, "*.db", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }
            return true;
        }
        return false;
    }

    public static bool DeleteFile(string fullPath)
    {
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }
        return false;
    }



    public static void ClearDirectory(string path, string suffix = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return;
        }

        string[] files = Directory.GetFiles(path, $"*{suffix}", searchOption);
        Parallel.ForEach(files, ParallelOptions, file =>
        {
            if (suffix == null || file.EndsWith(suffix, StringComparison.InvariantCulture))
            {
                File.Delete(file);
            }
        });
    }

    public static string ReadFileEncoding(string path, Encoding encoding)
    {
        if (!File.Exists(path))
        {
            Logger.LogError($"文件不存在：{path}");
            return null;
        }

        using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            int size = (int)fileStream.Length;
            byte[] binary = new byte[size];
            fileStream.Read(binary, 0, size);
            return encoding.GetString(binary);
        }
    }


    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public static void CopyFile(string from, string to)
    {
        if (!Directory.Exists(Path.GetDirectoryName(to)))
            Directory.CreateDirectory(Path.GetDirectoryName(to));

        if (!File.Exists(from))
        {
            Logger.LogError($" 拷贝文件失败 文件{from} 不存在");
            return;
        }
        File.Copy(from, to, true);
    }
}
