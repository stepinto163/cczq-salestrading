using com.gateway.log;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace STC.Common.Utils
{
    /// <summary>
    /// IO文件操作类
    /// </summary>
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

        public static string GetFileNameNoPath(string fileNamePath)
        {
            return Path.GetFileName(fileNamePath);
        }

        #endregion

        public static bool WriteFullText(string filePath, string content)
        {
            CreateDirectory(filePath);
            if (File.Exists(filePath))
                File.Delete(filePath);

            // 使用 StreamWriter 写入文件
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(content);
                writer.Close();
            }

            return true;
        }

        /// <summary>
        /// 读取文本文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadFullText(string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            string res = string.Empty;

            System.Text.Encoding encoding = GetType(filePath); 
            // 使用 StreamWriter 写入文件
            using (StreamReader reader = new StreamReader(filePath, encoding))
            {
                res = reader.ReadToEnd();
                reader.Close();
            }

            return res;
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
            try
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
            catch { }
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

        public static bool FileIsExists(string fileName)
        {
            bool isExist = false;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (File.Exists(fileName)) 
                {
                    isExist = true;
                }
            }
            return isExist;
        }

        public static string[] FileNameNotAllowed = new string[] { "<", ">", "/", "\\", "|", ":", "\"", "*", "?" };

        public static bool SendLogs(int defautLogFileCount = 20)
        {
            bool result = false;
            try
            {
                for (int i = 0; i < LocalEnv.ConfInfo.SendDays; i++)
                {
                    var zipFullName = ZipLogs(DateTime.Now.AddDays(-i), 20);
                    if (string.IsNullOrEmpty(zipFullName))
                    {
                       LogHelper.Debug($"压缩失败, defautLogFileCount={defautLogFileCount}");
                    }
                    else
                    {
                        //todo...
                        ////MailHelper.SendMail(zipFullName);
                        result = true;
                    }
                }
                LogHelper.Info("发送日志完成");
            }
            catch (Exception ex)
            {
                LogHelper.Error("日志发送失败", ex);
            }
            return result;
        }


        public static string ZipLogs(DateTime seletedDay, int defautLogFileCount = 20)
        {
            string result = string.Empty;
            const long MaxSize = 1024 * 1024 * 20;
            try
            {
                string tempLogFolder = SeleteLogFileToTemp(seletedDay, defautLogFileCount);
                string zipFullName = string.Empty;

                if (!string.IsNullOrEmpty(tempLogFolder) && ZipFolder(tempLogFolder, $"log_{seletedDay.ToString("yyyyMMdd")}.zip", out zipFullName))
                {
                    FileInfo zipFile = new FileInfo(zipFullName);
                    if (zipFile.Length > MaxSize)
                    {
                        if (defautLogFileCount > 0)
                        {
                            LogHelper.Info($"defautLogFileCount={defautLogFileCount},压缩文件过大需要丢弃一个");
                            result = ZipLogs(seletedDay, --defautLogFileCount);
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        result = zipFullName;
                    }
                }
                else
                {
                    LogHelper.Info($"{seletedDay:yyyyMMdd} 日志的压缩失败");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("压缩失败", ex);
            }
            return result;
        }

        public static bool ZipFolder(string folderPath, string targetName, out string fullName)
        {
            bool result = false;
            fullName = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(folderPath) && !string.IsNullOrEmpty(targetName))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                    if (dirInfo.Exists && dirInfo.GetFiles().Length > 0)
                    {
                        FileInfo fi = new FileInfo(targetName);
                        if (fi.Exists)
                        {
                            fi.Delete();
                        }
                        //todo...
                       ////ZipFile.CreateFromDirectory(folderPath, targetName);

                        FileInfo zipFile = new FileInfo(targetName);
                        if (zipFile.Exists)
                        {
                            fullName = zipFile.FullName;
                            result = true;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                LogHelper.Error("压缩日志出错", e);
                LogHelper.Error(e.StackTrace.ToString());
                throw;
            }
            finally
            {
                new DirectoryInfo(folderPath).Delete(true);
            }
            return result;
        }


        public static string SeleteLogFileToTemp(DateTime seletedDay, int logCount = 20)
        {
            var resultFolderPath = string.Empty;
            var logPath = LocalEnv.LogInfo.LogPath;
            var logFullPath = Path.GetFullPath(logPath);

            // 不是真的文件，而是日志前缀
            var fileInfo = new FileInfo(logFullPath);

            var folder = fileInfo.Directory;
            if (folder != null)
            {
                try
                {
                    // 先获取子文件夹，避免取到临时创建的文件夹
                    var subFolders = folder.GetDirectories();

                    resultFolderPath = Path.Combine(folder.FullName, Guid.NewGuid().ToString("N"));
                    DirectoryInfo resultFolder = new DirectoryInfo(resultFolderPath);
                    resultFolder.Create();

                    Action<DirectoryInfo, DirectoryInfo> action = (srcfolderInfo, desFolder) =>
                    {
                        var files = srcfolderInfo.GetFiles();
                        var orderFiles = files.OrderBy(x =>
                        {
                            var last = x.Name.Split('.').Last();
                            int num = 0;
                            if (int.TryParse(last, out num))
                            {
                                return num;
                            }
                            else
                            {
                                return 0;
                            }
                        });

                        int count = 0;
                        foreach (var file in orderFiles)
                        {
                            if (count > logCount)
                            {
                                break;
                            }

                            var newPath = Path.Combine(desFolder.FullName, file.Name);
                            if (file.Name.StartsWith(fileInfo.Name) || "AutoUpdater".StartsWith(file.Name))
                            {
                                if (GetDateByLogName(file.Name, fileInfo.Name) == seletedDay.Date)
                                {
                                    file.CopyTo(newPath, true);
                                    if (file.Name.StartsWith(fileInfo.Name))
                                    {
                                        count++;
                                    }
                                }
                            }
                        }
                    };

                    action(folder, resultFolder);


                    if (subFolders != null)
                    {
                        foreach (var item in subFolders)
                        {
                            var newSubFolder = Path.Combine(resultFolderPath, item.Name);
                            var desSubFolder = new DirectoryInfo(newSubFolder);
                            desSubFolder.Create();

                            action(item, desSubFolder);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("SeleteLogFileToTemp error", e);
                }

            }

            return resultFolderPath;
        }


        public static DateTime GetDateByLogName(string logName, string logPrefix)
        {
            const char dot = '.';
            DateTime result = DateTime.MinValue.Date;
            var splits = logName.Split(dot);
            if (splits.Length > 0 && splits[0].Length > logPrefix.Length)
            {
                var dayStr = splits[0].Remove(0, logPrefix.Length);
                // 20220509
                if (dayStr.Length == 8)
                {
                    var year = dayStr.Substring(0, 4);
                    var month = dayStr.Substring(4, 2);
                    var day = dayStr.Substring(6, 2);
                    result = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                }
            }
            return result;
        }

        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            System.IO.FileStream fs = new System.IO.FileStream(FILE_NAME, System.IO.FileMode.Open,
            System.IO.FileAccess.Read);
            System.Text.Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        /// 通过给定的文件流，判断文件的编码类型
        /// <param name="fs">文件流</param>
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetType(System.IO.FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            System.Text.Encoding reVal = System.Text.Encoding.Default;

            System.IO.BinaryReader r = new System.IO.BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = System.Text.Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = System.Text.Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = System.Text.Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }

        /// 判断是否是不带 BOM 的 UTF8 格式
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
    }
}
