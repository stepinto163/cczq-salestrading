using STC.Frame.Plugin;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Text;


namespace STC.Lua.Plugin
{
    public class LuaScriptPlugin : AbstractPlugin
    {
        private ILuaEngine luaEngine;
        public ILuaEngine LuaEngine { get { return luaEngine; } set { luaEngine = value; } }
        #region 插件信息

        public string fileName = "";
        private int _tag = 0;
        private string _description;
        private string _author;
        private string _pluginName;
        private int _pluginKey;
        private string _version;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return _pluginKey; }
        }

        public override string PluginName
        {
            get { return _pluginName; }
        }

        public override string Description
        {
            get { return _description; }
        }

        public override string Author
        {
            get { return _author; }
        }
        public new string Version
        {
            get
            {
                return _version;
            }
        }
        public LuaScriptPlugin()
        {
            enabled = false;
        }

        public LuaScriptPlugin(int tag, int pluginKey, string pluginName, string description, string author,string version)
        {
            _tag = tag;
            _pluginKey = pluginKey;
            _pluginName = pluginName;
            _description = description;
            _author = author;
            _version = version;
            enabled = true;
        }

        protected override void load()
        {
            try
            {
                luaEngine.Invoke("load");
                base.load();
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("load fileName:{0}[{1}]", _pluginKey, ex.Message));
            }
        }
        protected override void unload()
        {
            try
            {
                luaEngine.Invoke("unload");
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("unload fileName:{0}[{1}]", fileName, ex.Message));
            }
        }

        public override void onNotify(string name, object o)
        {
            try
            {
                luaEngine.Invoke("onNotify", name, o);
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("onNotify fileName:{0}[{1}]", fileName, ex.Message));
            }
        }
        #endregion
    }
}

