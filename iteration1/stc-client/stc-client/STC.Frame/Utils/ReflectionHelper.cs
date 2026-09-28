using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Windows;
using STC.Common.Utils;
using com.gateway.log;

namespace STC.Frame.Utils
{
    public class ReflectionHelper
    {
        #region 加载指定目录下派生类型

        /// <summary>
        ///  加载指定目录下所有程序集中的所有派生自baseType的类型
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="directorySearched">搜索的目录</param>
        /// <param name="searchChildFolder">是否搜索子目录中的程序集</param>
        /// <param name="config">高级配置，可以传入null采用默认配置</param>        
        /// <returns>所有从BaseType派生的类型列表</returns>
        public static IList<Type> LoadDerivedType(Type baseType, string directorySearched, bool searchChildFolder, TypeLoadConfig config)
        {
            if (config == null)
            {
                config = new TypeLoadConfig();
            }

            IList<Type> derivedTypeList = new List<Type>();
            if (searchChildFolder)
            {
                LoadDerivedTypeInAllFolder(baseType, derivedTypeList, directorySearched, config);
            }
            else
            {
                LoadDerivedTypeInOneFolder(baseType, derivedTypeList, directorySearched, config);
            }

            return derivedTypeList;
        }
        #endregion

        #region LoadDerivedTypeInAllFolder
        private static void LoadDerivedTypeInAllFolder(Type baseType, IList<Type> derivedTypeList, string folderPath, TypeLoadConfig config)
        {
            LoadDerivedTypeInOneFolder(baseType, derivedTypeList, folderPath, config);
            string[] folders = Directory.GetDirectories(folderPath);
            foreach (string nextFolder in folders)
            {
                LoadDerivedTypeInAllFolder(baseType, derivedTypeList, nextFolder, config);
            }
        }
        #endregion
        public static List<string> getPostfixs(string _pluginSign)
        {
                List<string> lt = new List<string>();
                if (!string.IsNullOrEmpty(_pluginSign))
                {
                    string[] strs = _pluginSign.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    lt.AddRange(strs);
                }
                return lt;
             
        }
        #region LoadDerivedTypeInOneFolder
        private static void LoadDerivedTypeInOneFolder(Type baseType, IList<Type> derivedTypeList, string folderPath, TypeLoadConfig config)
        {
            
            string[] files = Directory.GetFiles(folderPath);
            List<string> postfixs = getPostfixs(config.TargetFilePostfix);
            foreach (string file in files)
            {
                string[] fileName = file.Split(new string[] { "\\" },StringSplitOptions.RemoveEmptyEntries);
                if (config.TargetFilePostfix != null)
                {
                   bool isContinue = false;
                    foreach (string str in postfixs) {
                        if (fileName[fileName.Length-1].StartsWith(str) && fileName[fileName.Length - 1].EndsWith(".dll")) {
                            isContinue = true;
                        }
                    }
                    if (!isContinue)
                    {
                        continue;
                    }
                }

                Assembly asm = null;

                #region Asm 加载程序集到内存中
                try
                {
                    if (config.CopyToMemory)
                    {
                        byte[] addinStream = FileHelper.ReadFileReturnBytes(file);
                       
                        asm = Assembly.Load(addinStream);
                    }
                    else
                    {
                        asm = Assembly.UnsafeLoadFrom(file);//.LoadFrom(file);
                    }
               

                if (asm == null)
                {
                    continue;
                }
                #endregion

                Type[] types = asm.GetTypes();

                foreach (Type t in types)
                {
                    if (t.IsPublic && (t.IsSubclassOf(baseType) || baseType.IsAssignableFrom(t)))
                    {
                        bool canLoad = config.LoadAbstractType || (!t.IsAbstract);
                        if (canLoad)
                        {
                            derivedTypeList.Add(t);
                        }
                    }
                }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }

        }
        #endregion

        #region 获取dll编译时间
        public static DateTime GetPe32Time(Assembly assembly)
        {
            DateTime dt=new DateTime(2000,1,1);
            string fileName = assembly.Location;
            int seconds;
            using (var br = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read)))
            {
                var bs = br.ReadBytes(2);
                const string msg = "非法的PE32文件";
                if (bs.Length != 2) throw new Exception(msg);
                if (bs[0] != 'M' || bs[1] != 'Z') throw new Exception(msg);
                br.BaseStream.Seek(0x3c, SeekOrigin.Begin);
                var offset = br.ReadByte();
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                bs = br.ReadBytes(4);
                if (bs.Length != 4) throw new Exception(msg);
                if (bs[0] != 'P' || bs[1] != 'E' || bs[2] != 0 || bs[3] != 0) throw new Exception(msg);
                bs = br.ReadBytes(4);
                if (bs.Length != 4) throw new Exception(msg);
                seconds = br.ReadInt32();
            }
              dt = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc).
               AddSeconds(seconds).ToLocalTime();
            //dt =dt.AddDays(assembly.GetName().Version.Build);
            return dt;
        }
        #endregion

        #region 获取dll版本
        public static string GetVersion(Assembly assembly)
        {
            return assembly.GetName().Version.ToString();
        }

        public static Version GetFrameVersion()
        {
            return Application.Current.MainWindow.GetType().Assembly.GetName().Version;
        }
        #endregion

        #region 判断程序集是否Debug,Release

        public static bool IsDebugVersion(string assemblyName)
        {
            Assembly assembly = Assembly.LoadFile(assemblyName);
            Debug.Assert(assembly != null);
            foreach (DebuggableAttribute attribute in assembly.GetCustomAttributes(typeof(DebuggableAttribute), false))
            {
                if (attribute.IsJITTrackingEnabled) return true;
            }
            return false;
        }
        #endregion


        #region
        #region 私有方法
        #region 获得对象属性名
        //private static Dictionary<string, Dictionary<string, string>> dic2 = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, Dictionary<string, PropertyInfo>> dic3 = new Dictionary<string, Dictionary<string, PropertyInfo>>();
        /// <summary>
        /// 获得对象属性名
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>对象属性名集合</returns>
        public static Dictionary<string, PropertyInfo> GetObjectPropertyName(Type type)
        {
            Dictionary<string, PropertyInfo> dicp = null;
            if (object.Equals(type, null))
            {
                throw new System.Exception("对象不能为空");
            }
            Type objectType = type;
            string objectName = objectType.ToString();
            if (!dic3.ContainsKey(objectName))
            {
                PropertyInfo[] objectPropertyInfo = objectType.GetProperties();

                dicp = new Dictionary<string, PropertyInfo>();
                foreach (PropertyInfo pt in objectPropertyInfo)
                {
                    dicp.Add(pt.Name.ToUpper(), pt);
                }
                dic3.Add(objectName, dicp);
            }
            dicp = dic3[objectName];
            return dicp;
        }
        #endregion

        private static Dictionary<string, Dictionary<string, MethodInfo>> dic5 = new Dictionary<string, Dictionary<string, MethodInfo>>();

        public static Dictionary<string, MethodInfo> GetObjectMethodName(Type type)
        {
            Dictionary<string, MethodInfo> dicp = null;
            if (object.Equals(type, null))
            {
                throw new System.Exception("对象不能为空");
            }
            Type objectType = type;
            string objectName = objectType.ToString();
            if (!dic5.ContainsKey(objectName))
            {
                MethodInfo[] objectMethodInfo = objectType.GetMethods();

                dicp = new Dictionary<string, MethodInfo>();
                foreach (MethodInfo pt in objectMethodInfo)
                {
                    if (!dicp.ContainsKey(pt.Name))
                        dicp.Add(pt.Name, pt);
                }
                dic5.Add(objectName, dicp);
            }
            dicp = dic5[objectName];
            return dicp;
        }
        public static MethodInfo GetMethodInfo(Type type,string methodName)
        {
            MethodInfo methodInfo = null;
            Dictionary<string, MethodInfo> dic = GetObjectMethodName(type);
            if(dic.ContainsKey(methodName))
            {
                methodInfo = dic[methodName];
            }
            return methodInfo;
        }

        #region 获得对象属性值
        private static Dictionary<string, Dictionary<string, string>> dicp = new Dictionary<string, Dictionary<string, string>>();
        public static Dictionary<string, string> GetObjectPropertyValue(object o)
        {
            Dictionary<string, string> dic = null;
            if (object.Equals(o, null))
            {
                throw new System.Exception("对象o不能为NULL");
            }
            Type objectType = o.GetType();
            if (!dicp.ContainsKey(objectType.ToString()))
            {
                PropertyInfo[] objectPropertyInfo = objectType.GetProperties();
                dic = new Dictionary<string, string>();
                foreach (PropertyInfo pt in objectPropertyInfo)
                {
                    object value = pt.GetValue(o, null);
                    if (object.Equals(value, null))//判断值是否是空值
                    {
                        value = "";
                    }
                    dic.Add(pt.Name, value.ToString());
                }
                dicp.Add(objectType.ToString(), dic);
            }
            dic = dicp[objectType.ToString()] as Dictionary<string, string>;
            return dic;
        }
        #endregion
        #region 获得对象名
        /// <summary>
        /// 获得对象名
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>对象名字</returns>
        public static string GetObjectName(object o)
        {
            if (object.Equals(o, null))
            {
                throw new System.Exception("对象不能为空");
            }
            Type objectType = o.GetType();
            return objectType.Name;
        }
        #endregion
        #region 获得对象单一属性值
        public static string GetObjectPropertyValue(object o, string propertyName)
        {
            if (object.Equals(o, null))
            {
                throw new System.Exception("对象不能为空");
            }
            Type objectType = o.GetType();
            PropertyInfo pt = objectType.GetProperty(propertyName);//获得属性信息
            object value = pt.GetValue(o, null);//得到值
            if (object.Equals(value, null))//判断值是否是空值
            {
                value = "";
            }
            return value.ToString();
        }
        #endregion


        #endregion
        #region 类型转换
        public static Object StringToObject(Type t, String str, ref bool isSuccess)
        {
            Object o = str;
            if (t == typeof(string))
            {
                o = str;
                isSuccess = true;
            }
            if (t == typeof(DateTime) || t == typeof(DateTime?))
            {
                DateTime dt = new DateTime();
                if (DateTime.TryParse(str, out dt))
                {
                    o = dt;
                    isSuccess = true;
                }

            }
            if (t == typeof(int) || t == typeof(int?))
            {
                int i = 0;
                if (int.TryParse(str, out i))
                {
                    o = i;
                    isSuccess = true;
                }
            }
            if (t == typeof(decimal) || t == typeof(decimal?))
            {
                decimal d = 0;
                if (decimal.TryParse(str, out d))
                {
                    o = d;
                    isSuccess = true;
                }
            }
            if (t == typeof(float) || t == typeof(float?))
            {
                float d = 0;
                if (float.TryParse(str, out d))
                {
                    o = d;
                    isSuccess = true;
                }
            }
            if (t == typeof(double) || t == typeof(double?))
            {
                double d = 0;
                if (double.TryParse(str, out d))
                {
                    o = d;
                    isSuccess = true;
                }
            }

            return o;
        }

        #endregion

        #endregion

    }
    #region TypeLoadConfig
    public class TypeLoadConfig
    {
        #region Ctor
        public TypeLoadConfig()
        {
            LoadAbstractType = false;
        }

        public TypeLoadConfig(bool copyToMem, bool loadAbstract, string postfix)
        {
            CopyToMemory = copyToMem;
            LoadAbstractType = loadAbstract;
            _targetFilePostfix = postfix;
        }
        #endregion

        #region 拷贝到内存

        /// <summary>
        /// CopyToMem 是否将程序集拷贝到内存后加载
        /// </summary>
        public bool CopyToMemory { get; set; }

        #endregion

        #region 加载抽象类型

        /// <summary>
        /// LoadAbstractType 是否加载抽象类型
        /// </summary>
        public bool LoadAbstractType { get; set; }

        #endregion

        #region 目标程序集的后缀名
        private string _targetFilePostfix = ".dll";
        /// <summary>
        /// TargetFilePostfix 搜索的目标程序集的后缀名
        /// </summary>
        public string TargetFilePostfix
        {
            get { return _targetFilePostfix; }
            set { _targetFilePostfix = value; }
        }
        #endregion
    }
    #endregion
}

