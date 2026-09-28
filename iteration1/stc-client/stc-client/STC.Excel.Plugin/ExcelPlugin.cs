using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Model.MM;
using STC.Common.View.UI;
using STC.Excel.Plugin.View.Setting;
using STC.Excel.Plugin.ViewModel.Setting;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.Excel.Plugin
{
    public class ExcelPlugin : AbstractPlugin
    {
        #region 插件信息
        private int _tag = 5;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get {return (int)PluginKeyEnum.ExcelPlugin; }
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
            get { return "EXCEL插件"; }
        }

        public override string Description
        {
            get { return "EXCEL插件"; }
        }

        public override string Author
        {
            get { return "陈彦畅"; }
        }

        #endregion


        #region load,unload

        //Id parentID
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4009";
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
                ExcelViewModel excelView = new ExcelViewModel();
                bool isNeedUpgrade = excelView.IsNeedUpgrade();
                DesignHandler.Instance.SettingChange(new NewSettingMsg() { key = "ExcelPlugInShowRedSign", value = isNeedUpgrade ? "1" : "0" });
                excelView.Dispose();
            });
        }

        private void Instance_OnSettingChange(object obj)
        {
            if (obj is NewSettingMsg)
            {
                NewSettingMsg msg = (NewSettingMsg)obj;
                if (msg.key == "ExcelPlugInShowRedSign")
                {
                    if (sv!=null)
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
                    TabItem excelTabItem = new TabItem();
                    ExcelView excelView = new ExcelView();
                    sv = excelTabItem;
                    excelTabItem.Content = excelView;
                    excelTabItem.Header = this.Description;
                    settingView.AddTabPage(excelTabItem);
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
