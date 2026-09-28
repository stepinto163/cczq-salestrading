using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace STC.Common.Model
{
    /// <summary>
    /// IPlugin
    /// 
    /// 插件接口，所有插件都实现这个接口
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
    public interface IPlugin
    {
        /// <summary>
        /// Application 应用程序提供的服务接口，用于将插件注册到应用程序中
        /// </summary>
        IForm Application { get; set; }

        /// <summary>
        /// OnLoading 生命周期回调，当插件加载完毕被调用。可以从PluginUtils是获取主应用传递的参数来初始化插件
        /// </summary>
        void OnLoading();

        /// <summary>
        /// BeforeTerminating 生命周期回调，卸载插件前调用
        /// </summary>
        void BeforeTerminating();

        int Tag { get; set; }
        /// <summary>
        /// Enabled 插件是否启用
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// 是否主页
        /// </summary>
        bool IsMain { get; set; }

        /// <summary>
        /// 是否唯一
        /// </summary>
        bool IsAlone { get; set; }

        /// <summary>
        /// 是否填满整个window
        /// </summary>
        bool IsFullScreen { get; set; }

        /// <summary>
        /// PluginKey 插件关键字，不同的插件其Key是不一样的
        /// </summary>
        int PluginKey { get; }

        /// <summary>
        /// ServiceName 插件提供的服务的名字	
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Description 插件的描述信息	
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Version 插件版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Author 作者
        /// </summary>
        string Author { get; }

        void onNotify(string name, object o);

        string Title { get; }

        int DisplayOrder { get; }
        
        /// <summary>
        /// 用于关联插件初始化所需历史数据
        /// </summary>
        object PluginData { get; set; }
    }

    public class PluginHelper
    {
        public static string PluginSign1 = "Plugin.dll"; //所有的插件都以"Plugins.dll"结尾
        
        #region 字典集合到List转换
        public static List<IPlugin> DicToIlist(IEnumerable<KeyValuePair<int, IPlugin>> dic)
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


    }
    public class PluginComparer : IComparer<IPlugin>
    {
        public int Compare(IPlugin x, IPlugin y)
        {
            int result = 0;
            if (x.Tag > y.Tag)
            {
                result = 1;
            }
            if (x.Tag < y.Tag)
            {
                result = -1;
            }
            if (x.Tag == y.Tag)
            {
                result = 0;
            }
            return result;
        }
    }

    public class PluginComparer2 : IComparer<IPlugin>
    {
        public int Compare(IPlugin x, IPlugin y)
        {
            int result = 0;
            if (x.Tag < y.Tag)
            {
                result = 1;
            }
            if (x.Tag < y.Tag)
            {
                result = -1;
            }
            if (x.Tag == y.Tag)
            {
                result = 0;
            }
            return result;
        }
    }
}
