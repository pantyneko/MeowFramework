using System;
using System.IO;
using System.Text;

namespace Panty
{
    public static class FileKit
    {
        public static bool EnsurePathExists(ref string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return false;
                if (path.ContainsInvalidPathCharacters()) return false;

                if (Path.HasExtension(path))
                {
                    path = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(path)) return false;
                }
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                e.Log();
#endif
                return false;
            }
        }
        public static bool TryCreateDirectory(string path)
        {
            if (Directory.Exists(path)) return false;
            Directory.CreateDirectory(path);
            return true;
        }
        public static void WriteFile(string filePath, string content)
        {
            if (string.IsNullOrEmpty(content)) return;
            var spanContent = content.AsSpan();
            var encode = Encoding.UTF8;
            int maxByteCount = encode.GetMaxByteCount(spanContent.Length);

            using (FileStream fs = File.Create(filePath))
            {
                if (maxByteCount < 1024)
                {// 如果最大字节数小于某个阈值，使用 stackalloc
                    Span<byte> byteBuffer = stackalloc byte[maxByteCount];
                    int bytesWritten = encode.GetBytes(spanContent, byteBuffer);
                    fs.Write(byteBuffer.Slice(0, bytesWritten));
                }
                else // 否则使用堆分配
                {
                    byte[] byteBuffer = new byte[maxByteCount];
                    int bytesWritten = encode.GetBytes(spanContent, byteBuffer);
                    fs.Write(byteBuffer, 0, bytesWritten);
                }
            }
        }
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path">带文件的完整路径</param>
        public static void WriteFile(string path, Action<FileStream> callback)
        {
#if DEBUG
            ThrowEx.EmptyCallback(callback);
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