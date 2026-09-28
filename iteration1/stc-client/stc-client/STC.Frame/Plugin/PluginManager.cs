using System;
using System.Collections.Generic;

using STC.Frame.Plugin.WindowsPlugin;
using STC.Frame.Utils;
using STC.Common.Utils;
using com.gateway.log;

namespace STC.Frame.Plugin
{
    /// <summary>
    /// PluginManager
    /// 
    /// 插件管理默认实现  用于加载/卸载，管理各种插件
    /// 
    /// 修改纪录
    ///   
    ///         2010.8.8 版本：1.0 孔德帅 添加
    /// 
    /// 版本：1.0
    /// 
    /// <author>
    ///        <name>孔德帅</name>
    ///        <date>2010.8.8</date>
    /// </author> 
    /// </summary>
    public class PluginManager : IPluginManager
    {
        #region 属性

        private readonly IDictionary<int, IPlugin> _dicPlugin = new Dictionary<int, IPlugin>();
        private bool _copyToMemory = false;
        public IForm Application { get; set; }

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
                return PluginHelper.DicToIlist(_dicPlugin);
            }
        }
        private string _pluginSign=PluginHelper.PluginSign1;
        public string PluginSign
        {
            set { _pluginSign = value; }
        }
      
        #endregion

        #region 构造函数
        public PluginManager(IForm application)
        {
            Application = application;
            PluginChanged += delegate { };
        }
        public PluginManager()
        {
            PluginChanged += delegate { };
        }
        #endregion

        #region 方法 加载 卸载 插件

        public void Init()
        {
            LoadDefault( _pluginSign);
        }

        public void Loading()
        {
            List<IPlugin> plugins = PluginHelper.DicToIlist(_dicPlugin);
            plugins.Sort(new PluginComparer());
            foreach (IPlugin plugin in plugins)
            {
                try
                {
                    if (plugin.Enabled)
                    {
                        plugin.OnLoading();
                        PluginChanged();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }

        }
        public void Unloading()
        {
            List<IPlugin> plugins = PluginHelper.DicToIlist(_dicPlugin);
            plugins.Sort(new PluginComparer2());
            foreach (IPlugin plugin in plugins)
            {
                try
                {
                    plugin.BeforeTerminating();
                    PluginChanged();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }
        #region 加载插件


        public void LoadDefault(string pluginSign)
        {
            _pluginSign = pluginSign;
            LoadAllPlugins(AppDomain.CurrentDomain.BaseDirectory, true, _pluginSign);
        }
        public void LoadAllPlugins(string pluginFolderPath, bool searchChildFolder, string pluginSign)
        {
            var config = new TypeLoadConfig(CopyToMemory, false, pluginSign);
            IList<Type> pluginTypeList = ReflectionHelper.LoadDerivedType(typeof(IPlugin), pluginFolderPath, searchChildFolder, config);
            var pluginList = new List<IPlugin>();
            for (int i = 0; i < pluginTypeList.Count; i++)
            {
                var plugin = (IPlugin)Activator.CreateInstance(pluginTypeList[i]);
                pluginList.Add(plugin);
            }
            pluginList.Sort(new PluginComparer());
            for (int i = 0; i < pluginList.Count; i++)
            {
                IPlugin plugin = pluginList[i];
                if (_dicPlugin.ContainsKey(plugin.PluginKey))
                {
                    _dicPlugin.Remove(plugin.PluginKey);
                }
                _dicPlugin.Add(plugin.PluginKey, plugin);
                plugin.Application = Application;
                



            }

        }
        #endregion

        #region 卸载插件
        public void Clear()
        {
            if (_dicPlugin.Count > 0)
            {
                foreach (IPlugin plugin in _dicPlugin.Values)
                {
                    try
                    {
                        plugin.BeforeTerminating();
                    }
                    catch (Exception ex)
                    { LogHelper.Error(ex);}
                }
                _dicPlugin.Clear();
                PluginChanged();
            }
        }
        public void DynRemovePlugin(int pluginKey)
        {
            try
            {
                IPlugin plugin = GetPlugin(pluginKey);
                if (plugin != null)
                {
                    plugin.Enabled = false;
                    try
                    {
                        plugin.BeforeTerminating();
                    }
                    catch (Exception ex)
                    { LogHelper.Error(ex);}
                    _dicPlugin.Remove(pluginKey);
                    PluginChanged();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }
        #endregion

        #region 启动指定插件
        public void EnablePlugin(int pluginKey)
        {
            try
            {
                IPlugin plugin = GetPlugin(pluginKey);
                if (plugin != null && !plugin.Enabled)
                {
                    plugin.Enabled = true;
                    plugin.OnLoading();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }
        #endregion

        #region 停止指定插件
        public void DisEnablePlugin(int pluginKey)
        {
            IPlugin plugin = GetPlugin(pluginKey);
            if (plugin != null)
            {
                plugin.Enabled = false;
                plugin.BeforeTerminating();
            }
        }

        #endregion

        #region 通过插件Key获取指定插件
        public IPlugin GetPlugin(int pluginKey)
        {
            IPlugin plugin = null;
            if (_dicPlugin.ContainsKey(pluginKey))
            {
                plugin = _dicPlugin[pluginKey];
            }
            return plugin;
        }
        #endregion

        #region 是否包含指定插件
        public bool ContainsPlugin(int pluginKey)
        {
            return _dicPlugin.ContainsKey(pluginKey);
        }
        #endregion

        #endregion

        #region 事件 插件改变后出发
        public event PluginChanged PluginChanged;
        #endregion

        #region 字典集合到List转换
        private List<IPlugin> dicToIlist(IEnumerable<KeyValuePair<int, IPlugin>> dic)
        {
            List<IPlugin> pluginList = new List<IPlugin>();
            foreach (KeyValuePair<int, IPlugin> kvp in dic)
            {
                if (kvp.Value != null)
                {
                    pluginList.Add(kvp.Value);
                }
            }
            return pluginList;
        }
        #endregion

        #region 插件排序

        #endregion
    }
}
