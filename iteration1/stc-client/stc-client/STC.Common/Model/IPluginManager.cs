using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model
{
    /// <summary>
    /// IPluginManager
    /// 
    /// 插件管理接口  用于加载/卸载，管理各种插件
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
    public interface IPluginManager
    {
        #region 属性

        IForm Application { get; set; }
        /// <summary>
        /// CopyToMemory 是否将插件拷贝到内存后加载
        /// </summary>
        bool CopyToMemory { get;set;}

        /// <summary>
        /// AddinList 已加载的插件列表
        /// </summary>
        IList<IPlugin> PluginList { get;} //集合中为IPlugin

        #endregion

        #region 方法 加载 卸载 插件

        #region
        void Init();
        void Loading();
        void Unloading();
        #endregion

        #region 加载插件
        /// <summary>
        /// LoadDefault 加载当前目录或子目录下的所有有效插件
        /// </summary>
        void LoadDefault(string pluginSign);

        /// <summary>
        /// LoadAllAddins 加载指定目录下的所有插件
        /// </summary>      
        void LoadAllPlugins(string pluginFolderPath, bool searchChildFolder, string pluginSign);       
        #endregion

        #region 卸载插件
        /// <summary>
        /// Clear 清空所有已经加载的插件
        /// </summary>
        void Clear();

        /// <summary>
        /// DynRemoveAddin 卸载指定的插件
        /// </summary>       
        void DynRemovePlugin(int pluginKey);
        #endregion

        #region 启动指定插件
        /// <summary>
        /// EnablePlugin 启用指定的插件
        /// </summary>       
        void EnablePlugin(int pluginKey);
        #endregion

        #region 停止指定插件
        /// <summary>
        /// DisEnablePlugin 禁用指定的插件
        /// </summary> 
        void DisEnablePlugin(int pluginKey);

        #endregion

        #region 通过插件Key获取指定插件
        IPlugin GetPlugin(int pluginKey);
        #endregion

        #region 是否包含指定插件
        bool ContainsPlugin(int pluginKey);       
        #endregion

        #endregion

        #region 事件 插件改变后出发       
        event PluginChanged PluginChanged;
        #endregion
    }
    public delegate void PluginChanged();
}
