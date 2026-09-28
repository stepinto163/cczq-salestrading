using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using System.IO;
using MDT.Tools.Lua.Plugin.Utils;

using STC.Frame.Plugin;
using STC.Lua.Plugin;
using STC.Frame.Utils;

namespace STC.Lua.Plugin
{
    public class LuaPlugin : AbstractPlugin, IPluginManager
    {
        #region 插件信息

        private int _tag = 0;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return 31; }
        }

        public override string PluginName
        {
            get { return "lua脚本插件引擎"; }
        }

        public override string Description
        {
            get { return "lua脚本插件引擎,可以编写Lua脚本来增加插件"; }
        }

        public override string Author
        {
            get { return "孔德帅"; }
        }
        public LuaPlugin()
        { }
        protected override void load()
        {
            Init();
            Loading();
        }
        protected override void unload()
        {
            Unloading();
        }
        public override void onNotify(string name, object o)
        {
            onNotifyLua(name, o);
        }

        #endregion

        #region lua Function

        #region 加载脚本文件

        Dictionary<int, IPlugin> luaPlugins = new Dictionary<int, IPlugin>();
        
        

        private void loadLuaScriptAllfolders(string luaScriptPath, string pluginSign)
        {
            loadLuaScriptFile(luaScriptPath, pluginSign);
            string[] folders = Directory.GetDirectories(luaScriptPath);
            foreach (string nextFolder in folders)
            {
                loadLuaScriptAllfolders(nextFolder, pluginSign);
            }
        }
        private void loadLuaScriptFile(string luaScriptPath, string pluginSign)
        {
            string[] files = Directory.GetFiles(luaScriptPath);
            foreach (string fileName in files)
            {
                if (fileName.EndsWith(pluginSign))
                {
                    try
                    {
                        ILuaEngine luaEngine = LuaHelper.CreateLuaEngine();
                        luaEngine.BindLuaFunctions(this);
                        //LogHelper.Debug(luaEngine.ToString());
                        luaEngine.DoFile(fileName);
                        object[] luaPa = luaEngine.Invoke("init");
                        if (luaPa != null && luaPa.Length == 6)
                        {
                            int temp = 0;
                            int tag = 0;
                            int pluginKey = 0;
                            if(int.TryParse(luaPa[0] + "", out temp))
                               tag = temp;

                            if (int.TryParse(luaPa[1] + "", out temp))
                            {
                                pluginKey = temp;
                            }
                            LuaScriptPlugin lsp = new LuaScriptPlugin(tag, pluginKey, luaPa[2] + "", luaPa[3] + "", luaPa[4] + "", luaPa[5] + "");
                            lsp.LuaEngine = luaEngine;
                            lsp.fileName = fileName.Replace(System.Windows.Forms.Application.StartupPath+"\\","");
                            lsp.Application = Application;
                            if (luaPlugins.ContainsKey(lsp.PluginKey))
                            {
                                luaPlugins.Remove(lsp.PluginKey);
                            }
                            luaPlugins.Add(lsp.PluginKey, lsp);
                            
                        }

                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(string.Format("{0}:{1}",fileName,ex.StackTrace));
                    }
                }
            }
        }
        #endregion



        public void onNotifyLua(string name, object o)
        {
            if (luaTopics.ContainsKey(name))
            {
                List<int> subscribers = luaTopics[name];
                foreach (int luaPluginKey in subscribers)
                {
                    if (luaPlugins.ContainsKey(luaPluginKey))
                    {
                        try
                        {
                            luaPlugins[luaPluginKey].onNotify(name, o);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(string.Format("{0}:{1}", luaPluginKey, ex.StackTrace));
                        }
                    }
                }

            }
        }
        [AttrLuaFunc("registerObject", "注册插件之间共享的信息", "插件key","信息key", "信息内容")]
        public void registerObject(int pluginKey,string name, object obj)
        {
            string key = getPluginShareKey(pluginKey, name);
            Application.RegisterObject(key, obj);
        }
        [AttrLuaFunc("getObject", "获取插件之间共享的信息", "信息key", "信息内容")]
        public new object getObject(int pluginKey, string name)
        {
            return base.getObject(pluginKey, name);
        }
        [AttrLuaFunc("removeObject", "移除插件之间共享的信息", "信息key")]
        public void removeObject(string name)
        {
            base.remove(name);
        }
        Dictionary<string, List<int>> luaTopics = new Dictionary<string, List<int>>();
        [AttrLuaFunc("unsubscribe", "退订插件之间时时改变的信息", "信息key", "Lua插件的key")]
        public void unsubscribe(string name, int pluginKey)
        {

            if (luaTopics.ContainsKey(name))
            {
                List<int> subscribers = luaTopics[name];
                if (subscribers.Contains(pluginKey))
                {
                    subscribers.Remove(pluginKey);
                }
                if (subscribers.Count == 0)//没有lua插件订阅
                {
                    unsubscribe(name, this);
                }
            }


        }
        [AttrLuaFunc("subscribe", "订阅插件之间时时改变的信息", "信息key", "Lua插件的key")]
        public void subscribe(string name, int pluginKey)
        {

            List<int> subscribers = null;
            if (luaTopics.ContainsKey(name))//获取该主题对应的订阅者
            {
                subscribers = luaTopics[name];
            }
            else
            {
                subscribers = new List<int>();
                luaTopics.Add(name, subscribers);
                subscribe(name, this);//初次订阅
            }
            if (!subscribers.Contains(pluginKey))
            {
                subscribers.Add(pluginKey);
            }
        }
        [AttrLuaFunc("broadcast", "广播插件之间时时改变的信息", "信息key", "信息内容")]
        public new void broadcast(string name, object o)
        {
            base.broadcast(name, o);
        }
        [AttrLuaFunc("getPluginShareKey", "获取插件共享的key", "信息key", "Lua插件的key")]
        public string getPluginShareKey(string name, int pluginKey)
        {
            return getPluginShareKey(pluginKey, name);
        }

        #region 创建界面
        [AttrLuaFunc("getApplication", "获取IApplication")]
        public IApplication getApplication()
        {
            return Application;
        }
        #endregion

        #region log
        [AttrLuaFunc("debug", "调试日志记录", "日志内容")]
        public void debug(object str)
        {
            LogHelper.Debug(str + "");
        }
        [AttrLuaFunc("info", "业务日志记录", "日志内容")]
        public void info(object str)
        {
            LogHelper.Info(str + "");
        }
        [AttrLuaFunc("warn", "警告日志记录", "日志内容")]
        public void warn(object str)
        {
            LogHelper.Warn(str + "");
        }
        [AttrLuaFunc("error", "错误日志记录", "日志内容")]
        public void error(object ex)
        {
            LogHelper.Error(new Exception(ex + ""));
        }
        #endregion

        #region DataRow
        [AttrLuaFunc("getDataRowValue", "获取DataRowValue", "DataRow", "列名")]
        public object getDataRowValue(System.Data.DataRow dr, string columnName)
        {
            object value= dr[columnName];
            if (value == DBNull.Value)
            {
                value = "";
            }
            return value;
        }
        [AttrLuaFunc("setDataRowValue", "设置DataRowValue", "DataRow", "列名", "列value")]
        public void setDataRowValue(System.Data.DataRow dr, string columnName, object value)
        {
            dr[columnName] = value;
        }

        [AttrLuaFunc("getType", "获取getType", "Type name")]
        public Type getType(string name)
        {
            return Type.GetType(name, false, true);
        }

        [AttrLuaFunc("getDataRows", "获取getDataRows", "DataSet", "表名", "过滤条件")]
        public DataRow[] getDataRows(DataSet ds,string tableName, string filter)
        {
            DataRow[] drs= ds.Tables[tableName].Select(filter);
            return drs;
        }

        #endregion

        #region dataTable
        //[AttrLuaFunc("getDistinctDataTable", "获取getDistinctDataTable", "DataTable","是否Distinct", "列名")]
        //public System.Data.DataTable getDistinctDataTable(System.Data.DataView dv,bool isDistinct, LuaInterface.LuaTable columnNames)
        //{
        //    int count = columnNames.Values.Count;//dt.Merge(dt,true,System.Data.MissingSchemaAction)
        //    string[] strs = new string[count];
        //    columnNames.Values.CopyTo(strs, 0);
        //    System.Data.DataTable temp= dv.ToTable(isDistinct, strs);//temp.Select("")
            
        //    return temp;
        //}
        #endregion

        

        #endregion


        #region IPluginManager



        private bool _copyToMemory = false;

        public bool CopyToMemory
        {
            get
            {
                return _copyToMemory;
            }
            set
            {
                _copyToMemory = value;
            }
        }

        public IList<IPlugin> PluginList
        {
            get
            {
                return PluginHelper.DicToIlist(luaPlugins);
            }
        }
        private string _pluginSign="plugin.lua";
        public string PluginSign
        {
            set { _pluginSign = value; }
        }

      

      

        static bool isInit = false;
        public void Init()
        {
            if (!isInit)
            {
                isInit = true;
                LoadDefault(_pluginSign);
            }
        }

        static bool isLoaded = false;
        public void Loading()
        {
            if (!isLoaded)
            {
                List<IPlugin> plugins = PluginHelper.DicToIlist(luaPlugins);
                plugins.Sort(new PluginComparer());
                foreach (IPlugin plugin in plugins)
                {
                    try
                    {
                        plugin.OnLoading();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(string.Format("{0}:{1}", plugin.PluginKey, ex.StackTrace));
                    }
                }
                isLoaded = true;
            }

        }
        static bool isUnloaded = false;
        public void Unloading()
        {
            if (!isUnloaded)
            {
                List<IPlugin> plugins = PluginHelper.DicToIlist(luaPlugins);
                plugins.Sort(new PluginComparer2());
                foreach (IPlugin plugin in plugins)
                {
                    try
                    {
                        plugin.BeforeTerminating();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(string.Format("{0}:{1}", plugin.PluginKey, ex.StackTrace));
                    }
                }
                isUnloaded = true;
            }
        }

        public void LoadDefault(string pluginSign)
        {
            _pluginSign = pluginSign;

            LoadAllPlugins(FilePathHelper.LuaScriptPath, true, _pluginSign);
        }

        public void LoadAllPlugins(string pluginFolderPath, bool searchChildFolder, string pluginSign)
        {
            loadLuaScriptFile(pluginFolderPath, pluginSign);
            string[] folders = Directory.GetDirectories(pluginFolderPath);
            foreach (string nextFolder in folders)
            {
                loadLuaScriptAllfolders(nextFolder, pluginSign);
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void DynRemovePlugin(int pluginKey)
        {
            throw new NotImplementedException();
        }

        public void EnablePlugin(int pluginKey)
        {
            throw new NotImplementedException();
        }

        public void DisEnablePlugin(int pluginKey)
        {
            throw new NotImplementedException();
        }

        public IPlugin GetPlugin(int pluginKey)
        {
            IPlugin plugin = null;
            if (ContainsPlugin(pluginKey))
            {
                plugin = luaPlugins[pluginKey];
            }
            return plugin;
        }

        public bool ContainsPlugin(int pluginKey)
        {
            return luaPlugins.ContainsKey(pluginKey);
        }

        public event PluginChanged PluginChanged;
        #endregion


    }
}
