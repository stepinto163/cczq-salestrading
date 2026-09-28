using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// 交易包管理插件类
    /// </summary>
    public class BookManagementPlugin : AbstractPlugin
    {
        public override string Author
        {
            get
            {
                return "严晓丽";
            }
        }

        public override string Description
        {
            get
            {
                return "交易包管理";
            }
        }


        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }


        protected new bool enabled = false;
        public override bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
            }
        }

        private bool isFullScreen = true;
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

        public override int PluginKey
        {
            get
            {
                return(int)PluginKeyEnum.BookManagementPlugin;
            }
        }

        public override string PluginName
        {
            get
            {
                return "交易包管理插件";
            }
        }

        private int _tag = 10;
        public override int Tag
        {
            get
            {
                return _tag;
            }

            set
            {
                _tag = value;
            }
        }
        

        protected override void load()
        {
            BookHandler.Instance.init();
            BookHandler.Instance.RequestAccountInfoQuery();
            //LoadStyle("/STC.Frame;component/Resources/StyleDict.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/STC.SkinStyle.xaml");
            //LoadStyle("/STC.Frame;component/Resources/DatePickerDict.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/STC.SkinStyle.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/GridSplitterStyleDictionary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/ListBoxStyleDictinary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/ButtonStyleDictionary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/ComboBoxStyleDictionary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/ButtonStyleDictionary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/ToggleButtonStyleDictionary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/ListBoxStyleDictinary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/RadioButtonStyleDictionary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/TextBoxStyleDictionary.xaml");
            //LoadStyle("/STC.XBond.Plugin;component/Resources/DataGridStyleDictionary.xaml");


            LoadConfig.LoadBond = true;
            BookManagementView view = new BookManagementView();
            TabPageHelper.OpenTabPage(this.Description, view, PluginKey);
            base.load();
        }

        private void LoadStyle(string styleFile)
        {
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(
                System.Windows.Application.LoadComponent(
                    new Uri(styleFile, UriKind.Relative)) as System.Windows.ResourceDictionary);
        }


        public override void BeforeTerminating()
        {
        }

        public override void onNotify(string name, object o)
        {
        }
    }
}
