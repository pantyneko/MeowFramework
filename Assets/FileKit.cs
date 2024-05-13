using System;
using System.IO;
using System.Text;

namespace Panty
{
    public static class FileKit
    {
        public static bool TryCreateDirectory(string path)
        {
            if (Directory.Exists(path)) return false;
            Directory.CreateDirectory(path);
            return true;
        }
        public static void CreateOrWrite(string filePath, string msg)
        {
#if DEBUG
            if (msg == null) throw new ArgumentNullException("字符串为空");
#endif
            using (FileStream fs = File.Create(filePath))
            {
                var chars = msg.ToCharArray();
                byte[] buff = Encoding.Default.GetBytes(chars, 0, chars.Length);
                fs.Write(buff, 0, buff.Length);
            }
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path">带文件的完整路径</param>
        public static void WriteFile(string path, Action<FileStream> callback)
        {
#if DEBUG
            if (callback == null) throw new Exception("回调函数为空 该方法无意义");
#endif
            var file = new FileInfo(path);
            // 如果当前目录存在该文件 打开并写入文件 否则 创建存档文件
            using (FileStream fs = file.Exists ? file.OpenWrite() : file.Create())
            {
                if (file.Exists)
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                }
                callback.Invoke(fs);
            }
        }
    }
}