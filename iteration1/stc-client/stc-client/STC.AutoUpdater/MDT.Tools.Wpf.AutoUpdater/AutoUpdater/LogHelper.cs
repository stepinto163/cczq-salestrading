using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace MDT.Tools.Wpf.AutoUpdater
{
    public class LogHelper
    {
        private static string sysStartupPath = AppDomain.CurrentDomain.BaseDirectory;
        private static bool isDebug = true;
        public static bool IsDebug
        {
            get { return isDebug; }
            set { isDebug = value; }
        }

        public static void Debug(string str)
        {
            if (IsDebug)
            {
                string fileName = datePath();
                write(string.Format("{0} Debug {1}", DateTime.Now.ToString(), str), fileName);
            }
        }
        public static void Warn(string str)
        {
            string fileName = datePath();
            write(string.Format("{0} Warn {1}", DateTime.Now.ToString(), str), fileName);
        }
        public static void Info(string str)
        {
            string fileName = datePath();
            write(string.Format("{0} Info {1}", DateTime.Now.ToString(), str), fileName);
        }

        public static void Error(string str)
        {
            string fileName = datePath();
            write(string.Format("{0} Error {1}", DateTime.Now.ToString(), str), fileName);
        }

        public static void Error(Exception ex)
        {
            Error(ex.Message);
        }

        public static string datePath()
        {
            string path = sysStartupPath;
            DateTime dt = DateTime.Now;

            path += string.Format("\\logs\\AutoUpdater_{0:0000}{1:00}{2:00}.LOG", dt.Year, dt.Month, dt.Day);

            return path;
        }

        private static void write(string message, string path)
        {
            try
            {


                if (!File.Exists(path))
                {
                    CreateDirectory(path);
                    try
                    {
                        string dPath = sysStartupPath + "\\logs";
                        try
                        {

                            File.Delete(dPath + "\\network.log");
                        }
                        catch (Exception e)
                        {

                            Error(e);
                        }
                        var a = Directory.GetFiles(dPath);
                        var date = DateTime.Now.AddDays(-10).ToString("yyyyMMdd");
                        foreach (var s in a)
                        {

                            var t = s.Replace(dPath, "").Split(new char[] { '_', '.' });

                            if (t.Length == 3 && t[1].CompareTo(date) < 0)
                            {
                                try
                                {

                                    File.Delete(s);
                                }
                                catch (Exception e)
                                {

                                    Error(e);
                                }

                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);

                    }
                }


                FileInfo fi = new FileInfo(path);
                StreamWriter sw = fi.AppendText();
                sw.WriteLine(message);
                sw.Close();
            }
            catch
            {

                //MessageBox.Show(ex.Message);
            }

        }

        private static string CreateDirectory(string fileName)
        {
            string path = "";
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Trim(new char[] { '\\' });
                string[] paths = fileName.Split(new char[] { '\\' });

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
    }
}
