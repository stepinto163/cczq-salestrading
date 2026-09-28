using System;
using System.Collections.Generic;
using System.Text;
using STC.Common.Utils;
using com.gateway.log;
using STC.Common.Model;

namespace STC.Frame.Plugin
{
    /// <summary>
    /// 基础类
    /// </summary>
    public abstract class AbstractPlugin : IPlugin
    {
        protected bool isLoad = false;
        private IForm application;


        public IForm Application
        {
            get { return application; }
            set { application = value; }
        }

        public virtual void OnLoading()
        {
            if (!isLoad)
            {
                load();
            }
        }
        protected virtual void load()
        {
            LogHelper.Info(string.Format("{0}:{1} load", PluginName, Version));
        }
        protected virtual void unload()
        {
        }

        public virtual void BeforeTerminating()
        {
            if (isLoad)
            {
                unload();
            }
        }
        public virtual void onNotify(string name, object o)
        {
        }

        protected void subscribe(string name, IPlugin plugin)
        {
            application.Subscribe(name, plugin);
        }
        protected void unsubscribe(string name, IPlugin plugin)
        {
            application.Unsubscribe(name, plugin);
        }
        protected void broadcast(string name, object o)
        {
            application.BroadCast(name, o);
        }

        protected string getPluginShareKey(int pluginKey, string name)
        {
            return string.Format("{0}_{1}", pluginKey, name);
        }

        protected void registerObject(string name, object obj)
        {
            string key = getPluginShareKey(PluginKey, name);
            application.RegisterObject(key, obj);
        }

        protected object getObject(int pluginKey, string name)
        {
            string key = getPluginShareKey(pluginKey, name);
            return application.GetObject(key);
        }

        protected void remove(string name)
        {
            string key = getPluginShareKey(PluginKey, name);
            application.Remove(key);
        }

        public abstract int Tag { get; set; }


        protected bool isMain = false;
        public abstract bool IsMain { get; set; }

        protected bool isAlone = true;
        public abstract bool IsAlone { get; set; }



        protected bool enabled = true;
        public abstract bool Enabled { get; set; }
        
        public abstract bool IsFullScreen { get; set; }

        public abstract int PluginKey
        {
            get;
        }
        public abstract string PluginName { get; }
        public abstract string Description { get; }

        public string Version
        {
            get
            {
                string str="";
                try
                {
                    Type type = this.GetType();//+ "Build:(" + ReflectionHelper.GetPe32Time(type.Assembly.Location) + ")"; ;
                      str = ReflectionHelper.GetVersion(type.Assembly) + "(build:" + ReflectionHelper.GetPe32Time(type.Assembly).ToString("yyyyMMdd") + ")";
                }
                catch { }
                return str;
                }
        }
        public abstract string Author { get; }

      

        public   string Title { get; }
        public   int DisplayOrder { get; }

        public object PluginData { get; set; }
    }
}
