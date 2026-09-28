using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using MDT.Tools.Aliyun.Common.Oss;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ats.Foundation.Message;
using System.Configuration;
using System.Globalization;
using YamlDotNet.RepresentationModel;

namespace MDT.Tools.AutoUpdater.Config
{
    class Program
    {

        //获取当前目录
        //static string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static string currentDirectory = System.Environment.CurrentDirectory;
        //服务端xml文件名称
        static string serverXmlName = "AutoUpdateService.xml";
        private static string clientXmlName = "autoupdater.config";
        private static string autoUpdaterUrl = "autoUpdaterUrl.txt";
        private static string filedetailname = "filedetail.config";
        private static string filedetailUrl = "control/filedetailservice.xml";
        //更新文件URL前

        static string clientAutoUpdaterUrl = System.Configuration.ConfigurationSettings.AppSettings["clientAutoUpdaterUrl"];


        static string url = System.Configuration.ConfigurationSettings.AppSettings["url"];



        private static string enableClientStr = System.Configuration.ConfigurationSettings.AppSettings["enableClient"];
        private static string clientPath = System.Configuration.ConfigurationSettings.AppSettings["clientPath"];
        private static bool enableClient = false;

        private static string enableEncrpterRunTimeConfigStr = System.Configuration.ConfigurationSettings.AppSettings["enableEncrpterRunTimeConfig"];
        private static bool enableEncrpterRunTimeConfig = true;

        private static string Type = System.Configuration.ConfigurationSettings.AppSettings["type"];
        private static int type = 0;

        private static string RunTimeConfigPath = System.Configuration.ConfigurationSettings.AppSettings["RunTimeConfigPath"];
        private static string privateKey = System.Configuration.ConfigurationSettings.AppSettings["privateKey"];

        private static string enableAliyunStr = System.Configuration.ConfigurationSettings.AppSettings["enableAliyun"];
        private static bool enableAliyun = true;
        private static string accessId = System.Configuration.ConfigurationSettings.AppSettings["accessId"];
        private static string accessKey = System.Configuration.ConfigurationSettings.AppSettings["accessKey"];
        private static string bucketName = System.Configuration.ConfigurationSettings.AppSettings["bucketName"];
        private static string prefix = System.Configuration.ConfigurationSettings.AppSettings["prefix"];
        static OssHelper ossHelper = new OssHelper();
        private static bool isGenDiff = false;

        private static string md5 = "md5.txt";
        private static string md5_new = "md5_New.txt";
        private static string diffMd5 = "md5_Diff.txt";
        private static string yamlpath = System.Configuration.ConfigurationSettings.AppSettings["yamlpath"];
        private static string loginEnv = System.Configuration.ConfigurationSettings.AppSettings["loginEnv"];
        private static string catalog = System.Configuration.ConfigurationSettings.AppSettings["catalog"];
        private static string appexe = System.Configuration.ConfigurationSettings.AppSettings["AppExe"];
        private static string appname = System.Configuration.ConfigurationSettings.AppSettings["AppName"];

        public static void Main(string[] args)
        {
            string version = "1.0.0.0";
            string minimumRequiredVersion = "1.0.0.0";
            string comment = "";
            enableAliyun = false;
            if (enableAliyun)
            {
                ossHelper.OssConfig = new OssConfig() { AccessId = accessId, AccessKey = accessKey, BucketName = bucketName };
            }
            if (args != null && args.Length >= 0)
            {
                for (int i = 0; i < args.Length; i = i + 2)
                {
                    Console.Write(args[i] + "||" + args[i + 1] + "    ");
                    switch (args[i])
                    {
                        case "-t":
                            yamlpath = args[i + 1];
                            break;
                        case "-e":
                            loginEnv = args[i + 1];
                            break;
                        case "-ae":
                            appexe = args[i + 1];
                            break;
                        case "-ap":
                            catalog = args[i + 1];
                            break;
                        case "-u":
                            url = args[i + 1];
                            break;
                        case "-uc":
                            clientAutoUpdaterUrl = args[i + 1];
                            break;
                        case "-v":
                            version = args[i + 1];
                            break;
                        case "-m":
                            minimumRequiredVersion = args[i + 1];
                            break;
                        case "-c":
                            comment = args[i + 1];
                            break;
                        case "-d"://差异检查
                            isGenDiff = true;
                            break;
                        case "-p":
                            prefix = args[i + 1];
                            break;
                        case "-z":
                            upLoad(args, i);
                            return;
                    }
                }

            }
            
            if (!string.IsNullOrWhiteSpace(yamlpath))
            {
                GetYaml();
            }
            if (!string.IsNullOrWhiteSpace(catalog) && !string.IsNullOrWhiteSpace(appexe))
            {
                SetUpdateConfig();
            }

            bool.TryParse(enableClientStr, out enableClient);


            int.TryParse(Type, out type);
            bool.TryParse(enableEncrpterRunTimeConfigStr, out enableEncrpterRunTimeConfig);

            bool.TryParse(enableAliyunStr, out enableAliyun);
            enableAliyun = false;
           
            if (isGenDiff)
            {
                genMd5Diff();
            }
            else
            {
                alterFileDeatil();
                Console.WriteLine(clientPath + filedetailname + " 保存成功");
                genAutoUpdater(version, comment, minimumRequiredVersion);
            }
        }

        private static void upLoad(string[] args, int i)
        {
            string fileName = args[i + 1];
            if (File.Exists(fileName))
            {
                FileInfo file = new FileInfo(fileName);
                ossHelper.UpLoad(fileName, prefix + file.Name);
            }
            else
            {
                Console.WriteLine("{0} is not exist.", fileName);
            }
        }

        //根据md5生成变动文件清单
        private static void genMd5Diff()
        {
            DirectoryInfo dicInfo = new DirectoryInfo(currentDirectory);
            Dictionary<string, string> md5Dic = new Dictionary<string, string>();
            #region md5
            if (File.Exists(md5))
            {
                string[] strs = File.ReadAllLines(md5);
                foreach (var str in strs)
                {
                    string[] s = str.Split('|');
                    if (s.Length == 2)
                    {
                        if (!md5Dic.ContainsKey(str))
                        {
                            md5Dic.Add(str, s[1]);
                        }
                        else
                        {
                            Console.Error.WriteLine(s[1] + " 存在多份,发布将会有问题.");
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine(s[1] + " 格式错误.");
                    }
                }
            }

            #endregion
            File.Delete(diffMd5);
            #region diffMd5

            StreamWriter mdt_new = File.CreateText(md5_new);
            using (StreamWriter diffMd5sw = File.CreateText(diffMd5))
                PopuAllDirectory(mdt_new, diffMd5sw, md5Dic, dicInfo);
            mdt_new.Close();
            File.Delete(md5);
            File.Move(md5_new, md5);
            #endregion

        }

        private static void PopuAllDirectory(StreamWriter mdt_new, StreamWriter diffMd5sw, Dictionary<string, string> md5Dic, DirectoryInfo dicInfo)
        {

            //排除当前目录中生成xml文件的工具文件
            List<string> lt = new List<string>();
            lt.Add("ConsoleApplication1.exe".ToLower());
            lt.Add("AutoupdateService.xml".ToLower());
            lt.Add("AutoUpdater.config".ToLower());
            lt.Add("MDT.Tools.AutoUpdater.exe".ToLower());
            lt.Add("AutoUpdater.exe".ToLower());
            lt.Add(autoUpdaterUrl.ToLower());
            lt.Add(md5.ToLower());
            lt.Add(md5_new.ToLower());
            lt.Add(diffMd5.ToLower());
            lt.Add("MDT.Tools.AutoUpdater.Config.exe".ToLower());
            lt.Add("MDT.Tools.AutoUpdater.Config.exe.config".ToLower());
            foreach (FileInfo f in dicInfo.GetFiles())
            {

                if (!lt.Contains(f.Name.ToLower()) && !f.Name.EndsWith("pdb"))
                {
                    string path = dicInfo.FullName.Replace(currentDirectory, "").Replace("\\", "/").TrimStart('/');
                    if (!string.IsNullOrEmpty(path))
                    {
                        path = path + "/";
                    }
                    string str = url + prefix + path + f.Name;

                    string path1 = f.Name;
                    if (!string.IsNullOrEmpty(path))
                    {
                        path1 = path + f.Name;

                    }
                    path1 = path1.Trim('/');
                    if (!(str.IndexOf("svn") > 0))
                    {
                        string fmd5 = ByteArrayToHexString(HashData(f.OpenRead(), "md5"));
                        if (!md5Dic.ContainsKey(fmd5 + "|" + f.Name))
                        {
                            diffMd5sw.WriteLine(string.Format("{0}"
                                                              , path1));
                        }
                        mdt_new.WriteLine(string.Format("{0}|{1}", fmd5
                                                              , f.Name));
                    }
                }
            }

            foreach (DirectoryInfo di in dicInfo.GetDirectories())
                PopuAllDirectory(mdt_new, diffMd5sw, md5Dic, di);

        }
        private static void alterFileDeatil()
        {
            XmlDocument clientDoc = new XmlDocument();
            XmlDeclaration clientxmldecl = clientDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            clientDoc.AppendChild(clientxmldecl);
            XmlElement clientConfig = clientDoc.CreateElement("FileDetail");
            clientDoc.AppendChild(clientConfig);
            XmlElement clientServerUrl = clientDoc.CreateElement("ServerUrl");
            if (type == 0)
            {
                clientServerUrl.InnerText = clientAutoUpdaterUrl + filedetailUrl;
            }
            else
            {
                clientServerUrl.InnerText = clientAutoUpdaterUrl;
            }
            XmlElement clientType = clientDoc.CreateElement("Type");
            clientType.InnerText = type + "";
            clientConfig.AppendChild(clientType);
            clientConfig.AppendChild(clientServerUrl);
            clientDoc.Save(clientPath + filedetailname);
        }

        private static void genAutoUpdater(string version, string comment, string minimumRequiredVersion)
        {
            if (enableAliyun)
            {
                ossHelper.Delete(prefix);
            }

            #region client

            XmlDocument clientDoc = new XmlDocument();
            XmlDeclaration clientxmldecl = clientDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            clientDoc.AppendChild(clientxmldecl);
            XmlElement clientConfig = clientDoc.CreateElement("Config");
            clientDoc.AppendChild(clientConfig);

            //XmlElement clientEnabled = clientDoc.CreateElement("Enabled");
            //clientEnabled.InnerText = "true";
            //clientConfig.AppendChild(clientEnabled);
            XmlElement clientServerUrl = clientDoc.CreateElement("ServerUrl");
            if (type == 0)
            {
                clientServerUrl.InnerText = clientAutoUpdaterUrl + autoUpdaterUrl;
            }
            else
            {
                clientServerUrl.InnerText = clientAutoUpdaterUrl;
            }
            Console.WriteLine(type);
            clientConfig.AppendChild(clientServerUrl);
            XmlElement clientVersion = clientDoc.CreateElement("Version");
            clientVersion.InnerText = version;
            clientConfig.AppendChild(clientVersion);
            //XmlElement clientType = clientDoc.CreateElement("Type");
            //clientType.InnerText = type + "";
            //clientConfig.AppendChild(clientType);
            XmlElement clientRoot = clientDoc.CreateElement("UpdateFileList");

            var f = File.CreateText(autoUpdaterUrl);
            f.WriteLine(url + prefix + serverXmlName);
            f.Close();

            #endregion

            //创建文档对象
            XmlDocument doc = new XmlDocument();

            //创建根节点
            XmlElement root = doc.CreateElement("config");

            XmlElement versionE = doc.CreateElement("version");
            versionE.InnerText = version;
            XmlElement commentE = doc.CreateElement("comment");
            commentE.InnerText = comment;

            XmlElement minimumRequiredVersionE = doc.CreateElement("minimumrequiredversion");
            minimumRequiredVersionE.InnerText = minimumRequiredVersion;

            XmlElement updateFilesE = doc.CreateElement("updateFiles");
            //头声明
            XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(xmldecl);
            //获取当前目录对象
            DirectoryInfo dicInfo = new DirectoryInfo(currentDirectory);
            //调用递归方法组装xml文件
            PopuAllDirectory(doc, updateFilesE, clientDoc, clientRoot, dicInfo);
            root.AppendChild(versionE);
            root.AppendChild(minimumRequiredVersionE);
            root.AppendChild(commentE);
            root.AppendChild(updateFilesE);
            //追加节点
            doc.AppendChild(root);
            //保存文档

            doc.Save(serverXmlName);
            Console.WriteLine(serverXmlName + " 保存成功");
            clientConfig.AppendChild(clientRoot);
            if (enableClient)
            {
                clientDoc.Save(clientPath + clientXmlName);
                Console.WriteLine(clientPath + clientXmlName + " 保存成功");

                if (enableAliyun)
                {
                    ossHelper.UpLoad(clientPath + clientXmlName, prefix + clientPath + clientXmlName);
                }
            }

            if (enableAliyun)
            {
                ossHelper.UpLoad(serverXmlName, prefix + serverXmlName);
            }

            encryptRunTimeConfig();
        }

        private static void encryptRunTimeConfig()
        {
            if (enableEncrpterRunTimeConfig)
            {
                var fi = new FileInfo(RunTimeConfigPath);
                StreamReader sr = fi.OpenText();
                string content = sr.ReadToEnd();
                sr.Close();
                //File.WriteAllText(RunTimeConfigPath, EncrypterHelper.EncryptRASString(content, privateKey));
            }
        }

        //递归组装xml文件方法
        private static void PopuAllDirectory(XmlDocument doc, XmlElement root, XmlDocument clientDoc, XmlElement clientRoot, DirectoryInfo dicInfo)
        {
            //排除当前目录中生成xml文件的工具文件
            List<string> lt = new List<string>();
            lt.Add("ConsoleApplication1.exe".ToLower());
            lt.Add("AutoupdateService.xml".ToLower());
            lt.Add("AutoUpdater.config".ToLower());
            lt.Add("MDT.Tools.AutoUpdater.exe".ToLower());
            lt.Add("AutoUpdater.exe".ToLower());
            lt.Add(autoUpdaterUrl.ToLower());
            lt.Add(md5.ToLower());
            lt.Add(md5_new.ToLower());
            lt.Add(diffMd5.ToLower());
            //lt.Add("MDT.Tools.AutoUpdater.exe.config".ToLower());
            lt.Add("MDT.Tools.AutoUpdater.Config.exe".ToLower());
            lt.Add("MDT.Tools.AutoUpdater.Config.exe.config".ToLower());
            foreach (FileInfo f in dicInfo.GetFiles())
            {
                if (ymalst != null && ymalst.Count > 0)
                {
                    if (!ymalst.Contains(f.Name))
                        continue;
                }
                if (!lt.Contains(f.Name.ToLower()) && !f.Name.EndsWith("pdb"))
                {
                    string path = dicInfo.FullName.Replace(currentDirectory, "").Replace("\\", "/").TrimStart('/');
                    if (!string.IsNullOrEmpty(path))
                    {
                        path = path + "/";
                    }
                    string str = url + prefix + path + f.Name;

                    string path1 = f.Name;
                    if (!string.IsNullOrEmpty(path))
                    {
                        path1 = path + f.Name;

                    }
                    path1 = path1.Trim('/');
                    if (!(str.IndexOf("svn") > 0))
                    {
                        string versionStr = FileVersionInfo.GetVersionInfo(f.FullName).FileVersion;
                        Version version = new Version(1, 0, 0, 0);
                        try
                        {
                            if (!string.IsNullOrEmpty(versionStr))
                            {
                                version = new Version(versionStr);
                            }
                        }
                        catch
                        {


                        }
                        XmlElement child = doc.CreateElement("file");
                        child.SetAttribute("path", f.Name);
                        child.SetAttribute("url", url + prefix + path + HttpUtility.UrlEncode(f.Name).Replace("+", "%20"));
                        child.SetAttribute("lastver", version.ToString());
                        child.SetAttribute("size", f.Length.ToString());
                        child.SetAttribute("md5", ByteArrayToHexString(HashData(f.OpenRead(), "md5")));
                        child.SetAttribute("needRestart", "true");
                        //switch (f.Name)
                        //{
                        //    case "IDC.Bill.Plugin.dll":
                        //    case "IDC.Bill.Plugin.Themes.dll":
                        //        child.SetAttribute("type", "Bill"); 
                        //        break;
                        //    case "IDC.Broker.Plugin.dll":
                        //        child.SetAttribute("type", "Bond");
                        //        break;
                        //    case "IDC.Matching.Plugin.dll":
                        //        child.SetAttribute("type", "MP");
                        //        break;
                        //    case "IDC.Sys.Plugin.dll":
                        //        child.SetAttribute("type", "Sys");
                        //        break;
                        //    default:
                        //        child.SetAttribute("type", "System");
                        //        break;
                        //}
                       
                        root.AppendChild(child);


                        XmlElement clientChild = clientDoc.CreateElement("LocalFile");
                        clientChild.SetAttribute("path", f.Name);

                        clientChild.SetAttribute("lastver", version.ToString());
                        clientChild.SetAttribute("size", f.Length.ToString());
                        clientChild.SetAttribute("md5", ByteArrayToHexString(HashData(f.OpenRead(), "md5")));
                        clientChild.SetAttribute("needRestart", "true");
                        //switch (f.Name)
                        //{
                        //    case "IDC.Bill.Plugin.dll":
                        //    case "IDC.Bill.Plugin.Themes.dll":
                        //        clientChild.SetAttribute("type", "Bill");
                        //        break;
                        //    case "IDC.Broker.Plugin.dll":
                        //        clientChild.SetAttribute("type", "Bond");
                        //        break;
                        //    case "IDC.Matching.Plugin.dll":
                        //        clientChild.SetAttribute("type", "MP");
                        //        break;
                        //    case "IDC.Sys.Plugin.dll":
                        //        clientChild.SetAttribute("type", "Sys");
                        //        break;
                        //    default:
                        //        clientChild.SetAttribute("type", "System");
                        //        break;
                        //}
                        clientRoot.AppendChild(clientChild);

                        if (enableAliyun)
                        {
                            path1 = prefix + path1;
                            ossHelper.UpLoad(f, path1);

                        }
                    }
                }
            }

            foreach (DirectoryInfo di in dicInfo.GetDirectories())
                PopuAllDirectory(doc, root, clientDoc, clientRoot, di);


        }

        #region
        /// <summary>
        /// 计算文件的 MD5 值
        /// </summary>
        /// <param name="fileName">要计算 MD5 值的文件名和路径</param>
        /// <returns>MD5 值16进制字符串</returns>
        public static string MD5File(string fileName)
        {
            return HashFile(fileName, "md5");
        }

        /// <summary>
        /// 计算文件的哈希值
        /// </summary>
        /// <param name="fileName">要计算哈希值的文件名和路径</param>
        /// <param name="algName">算法:sha1,md5</param>
        /// <returns>哈希值16进制字符串</returns>
        public static string HashFile(string fileName, string algName)
        {
            if (!System.IO.File.Exists(fileName))
                return string.Empty;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] hashBytes = HashData(fs, algName);
            fs.Close();
            return ByteArrayToHexString(hashBytes);
        }

        /// <summary>
        /// 计算哈希值
        /// </summary>
        /// <param name="stream">要计算哈希值的 Stream</param>
        /// <param name="algName">算法:sha1,md5</param>
        /// <returns>哈希值字节数组</returns>
        public static byte[] HashData(Stream stream, string algName)
        {
            HashAlgorithm algorithm;
            if (algName == null)
            {
                throw new ArgumentNullException("algName 不能为 null");
            }
            if (string.Compare(algName, "sha1", true) == 0)
            {
                algorithm = SHA1.Create();
            }
            else
            {
                if (string.Compare(algName, "md5", true) != 0)
                {
                    throw new Exception("algName 只能使用 sha1 或 md5");
                }
                algorithm = MD5.Create();
            }
            return algorithm.ComputeHash(stream);
        }
        /// <summary>
        /// 字节数组转换为16进制表示的字符串
        /// </summary>
        public static string ByteArrayToHexString(byte[] buf)
        {
            //int iLen = 0;

            //// 通过反射获取 MachineKeySection 中的 ByteArrayToHexString 方法，该方法用于将字节数组转换为16进制表示的字符串。
            //Type type = typeof(System.Web.Configuration.MachineKeySection);
            //MethodInfo byteArrayToHexString = type.GetMethod("ByteArrayToHexString", BindingFlags.Static | BindingFlags.NonPublic);

            //// 字节数组转换为16进制表示的字符串
            //return (string)byteArrayToHexString.Invoke(null, new object[] { buf, iLen });
            return byteToHexStr(buf);
        }
        #endregion

        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        //public static List<string> CLIENT = new List<string>();
        //public static List<string> CONTROL = new List<string>();
        //public static List<string> BIN = new List<string>();
        //public static List<string> PLUGIN = new List<string>();
        public static List<string> ymalst = new List<string>();
        //public static string path = "";
        public static Dictionary<string, List<string>> yamlMap = new Dictionary<string, List<string>>();
        public static void GetYaml()
        {
            yamlMap.Clear();
            yamlpath = Path.GetFullPath("..") + yamlpath;
            TextReader reader = File.OpenText(yamlpath);
            var yaml = new YamlStream();
            yaml.Load(reader);
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            var temp = (YamlMappingNode)mapping["idc.setup"]["envs"][loginEnv];
            foreach (var entry in temp.Children)
            {
                Type m = entry.Value.GetType();
                if (entry.Value is YamlDotNet.RepresentationModel.YamlSequenceNode)
                {
                    string group = entry.Key.ToString();
                    YamlSequenceNode node = (YamlSequenceNode)entry.Value;
                    getValues(group, node);

                }
            }
            foreach(List<string> li in yamlMap.Values)
            {
                ymalst.AddRange(li);
            }
            if (ymalst.Contains("YamlDotNet.dll"))
                ymalst.Remove("YamlDotNet.dll");
            string url = string.Format("{0}\\{1}", Path.GetFullPath(".."), catalog);
            if (yamlMap != null && yamlMap.Count > 0)
            {
                DeleteFolder("client", url);
            }
        }

        private static void getValues(string key, YamlSequenceNode entry)
        {
            List<string> valuelist = new List<string>();
            if (!yamlMap.ContainsKey(key))
            {
                yamlMap.Add(key.ToLower(), valuelist);
            }
            foreach (var entry2 in (YamlSequenceNode)entry)
            {
                if (entry2 is YamlDotNet.RepresentationModel.YamlMappingNode)
                {
                    YamlMappingNode key2 = (YamlMappingNode)entry2;
                    foreach (var entry3 in (YamlMappingNode)entry2)
                    {
                        string d = entry3.Value.ToString();

                        if (entry3.Value is YamlDotNet.RepresentationModel.YamlSequenceNode)
                        {
                            valuelist.Add(entry3.Key.ToString());
                            YamlSequenceNode node = (YamlSequenceNode)entry3.Value;
                            getValues(entry3.Key.ToString(), node);
                        }
                        else
                            valuelist.Add(d);
                    }
                }
            }
        }
        public static void DeleteFolder(string key, string url)
        {
            foreach (string d in Directory.GetFileSystemEntries(url))
            {
                if (Directory.Exists(d))
                {
                    DirectoryInfo dir = new DirectoryInfo(d);
                    if (dir.Attributes.ToString().IndexOf("ReadOnly") != -1 || (yamlMap.ContainsKey(key.ToLower()) && yamlMap.ContainsKey(dir.Name.ToLower())))
                        DeleteFolder(dir.Name, d);
                    else
                        Directory.Delete(d, true);//直接删除其中的文件  
                }
                else if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    //File.SetAttributes(fi.Name, System.IO.FileAttributes.Normal);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1 || (yamlMap.ContainsKey(key.ToLower()) && yamlMap[key.ToLower()].Contains(fi.Name)))
                        continue;
                    else
                        File.Delete(d);//直接删除其中的文件  
                }
            }
        }
        
        public static void SetUpdateConfig()
        {
            string exePath = ""; string exePath2 = ""; string exePath3 = "";
            try
            {
                exePath = Path.Combine(Path.GetFullPath("..") + "\\" + catalog, "MDT.Tools.AutoUpdater.exe");
                exePath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "MDT.Tools.AutoUpdater.exe");
                exePath3 = Path.Combine(Path.GetFullPath("..") + "\\" + catalog, appexe + ".exe");
                Console.Write("exePath:{0} ", exePath);
                Console.Write("exePath2:{0} ", exePath2);
                if (!File.Exists(exePath)|| !File.Exists(exePath2)|| !File.Exists(exePath3))
                    return;
                Console.Write("exePath3:{0} ", exePath3);
                Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);
                Configuration config2 = ConfigurationManager.OpenExeConfiguration(exePath2);
                Configuration config3 = ConfigurationManager.OpenExeConfiguration(exePath3);
                if (config3.AppSettings.Settings["AppExe"] != null)
                {
                    string appexe = config3.AppSettings.Settings["AppExe"].Value;
                    config.AppSettings.Settings["AppExe"].Value = appexe;
                    config2.AppSettings.Settings["AppExe"].Value = appexe;
                }

                if (config3.AppSettings.Settings["AppName"] != null)
                {
                    string appname = config3.AppSettings.Settings["AppName"].Value;
                    config.AppSettings.Settings["AppName"].Value = appname.Contains("Client") ? appname : string.Format("{0} Client", appname);
                    config2.AppSettings.Settings["AppName"].Value = appname.Contains("Client") ? appname : string.Format("{0} Client", appname);
                }
                config.Save(ConfigurationSaveMode.Modified);//保存
                config2.Save(ConfigurationSaveMode.Modified);//保存
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                Console.Write("SetUpdateConfig:{0}", ex);
            }
        }
    }
}
