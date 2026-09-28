using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Model.MM;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Python.Plugin.View.Setting;
using STC.Python.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.Python.Plugin
{
    public class PythonPlugin : AbstractPlugin
    {
        #region 插件信息
        private int _tag = 6;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.PythonPlugin; }
        }


        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }


        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private bool isFullScreen = false;
        public override bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }

            set
            {
                isFullScreen = value;
            }
        }

        public override string PluginName
        {
            get { return "PYTHON插件"; }
        }

        public override string Description
        {
            get { return "PYTHON插件"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion


        #region load,unload

        //Id parentID
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4015";
        private bool _isSubSetting = false;
        private TabItem sv;
        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;

            if (!_isSubSetting)
            {
                InitSetting();
                _isSubSetting = true;
            }
            base.load();
        }

        protected override void unload()
        {
            base.unload();
            _isSubSetting = false;
        }

        private void InitSetting()
        {
            DesignHandler.Instance.OnShowSetting += Instance_ShowDesign;
            DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
            ///检查是否需要升级，如果需要升级发送消息给系统窗口
            CheckExcelPlugInVer();
        }

        private async void CheckExcelPlugInVer()
        {
            await Task.Run(() =>
            {
                PythonViewModel excelView = new PythonViewModel();
                //bool isNeedUpgrade = excelView.IsNeedUpgrade();
                //DesignHandler.Instance.SettingChange(new NewSettingMsg() { key = "PythonPlugInShowRedSign", value = isNeedUpgrade ? "1" : "0" });
                excelView.Dispose();
            });
        }

        private void Instance_OnSettingChange(object obj)
        {
            if (obj is NewSettingMsg)
            {
                NewSettingMsg msg = (NewSettingMsg)obj;
                if (msg.key == "PythonPlugInShowRedSign")
                {
                    if (sv != null)
                        sv.Tag = (msg.value == "1") ? true : false;
                }
            }
        }

        private void Instance_ShowDesign(object obj)
        {
            if (obj != null)
            {
                SettingView settingView = obj as SettingView;
                if (settingView != null)
                {
                    TabItem pythonTabItem = new TabItem();
                    PythonView pythonView = new PythonView();
                    sv = pythonTabItem;
                    pythonTabItem.Content = pythonView;
                    pythonTabItem.Header = this.Description;
                    settingView.AddTabPage(pythonTabItem);
                }
            }
        }

        public override void BeforeTerminating()
        {
            DesignHandler.Instance.OnSettingChange -= Instance_OnSettingChange;
        }
        #endregion
    }
}
