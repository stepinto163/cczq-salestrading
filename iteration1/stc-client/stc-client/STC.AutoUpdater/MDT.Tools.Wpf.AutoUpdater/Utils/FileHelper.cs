using System;
using System.IO;
using System.Text;

namespace MDT.Tools.Wpf.Utils
{
    public class FileHelper
    {
        #region ReadFileReturnBytes
        /// <summary>
        /// ReadFileReturnBytes 从文件中读取二进制数据
        /// </summary>      
        public static byte[] ReadFileReturnBytes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var br = new BinaryReader(fs);

            byte[] buff = br.ReadBytes((int)fs.Length);

            br.Close();
            fs.Close();

            return buff;
        }
        #endregion

        public static string GetFileNameNoPath(string fileNamePath)
        {
            return Path.GetFileName(fileNamePath);
        }

        
        public static bool Write(string fileName, string[] strs)
        {
            return Write(fileName, strs, Encoding.UTF8,false);
        }
        public static bool WriteANSI(string fileName, string[] strs)
        {
            return Write(fileName, strs, Encoding.Default, false);
        }
        public static bool Write(string fileName, string[] strs, Encoding encoding)
        {
            return Write(fileName, strs, encoding, false);
        }
        public static bool Write(string fileName, string[] strs, bool isAppend)
        {
            return Write(fileName, strs, Encoding.UTF8, isAppend);
        }
        public static bool Write(string fileName, string[] strs,Encoding encoding,bool isAppend)
        {
            bool status = true;
            try
            {
                CreateDirectory(fileName);
                if (!isAppend)
                File.Delete(fileName);
                var f = new FileInfo(fileName);
                FileStream fs = f.OpenWrite();
                foreach (string str in strs)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        byte[] b = encoding.GetBytes(str + "\n");
                        fs.Write(b, 0, b.Length);
                    }
                }
                fs.Close();
                
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }
        public static void DeleteDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                foreach (string d in Directory.GetFileSystemEntries(filePath))
                {
                    if (File.Exists(d))
                    {
                        File.Delete(d);
                    }
                    else
                        DeleteDirectory(d);
                }
                Directory.Delete(filePath, true);
            }
        }

        public static string CreateDirectory(string fileName)
        {
            string path = "";
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Trim(new[] { '\\' });
                string[] paths = fileName.Split(new[] { '\\' });

                for (int i = 0; i < paths.Length - 1; i++)
                {
                    path = path + paths[i];
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    path = path + "\\";
                }
            }
            return path;
        }

        public static string[] FileNameNotAllowed = new string[] { "<", ">", "/", "\\", "|", ":", "\"", "*", "?" };
    }
}
