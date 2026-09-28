using STC.Common.Handler.Admin;
using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STC.Common.View.UI
{
    /// <summary>
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : Window
    {
        public SettingView()
        {
            InitializeComponent();
            this.Loaded += SettingView_Loaded;
            this.Unloaded += SettingView_Unloaded;
            Data.DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
        }

        private void SettingView_Loaded(object sender, RoutedEventArgs e)
        {
            //如果没有报价权限则隐藏
            bool isAllow = AdminHandler.Instance.CheckPermission("1003", null);
            string isShowBlackListSet = ConfigurationManager.AppSettings["ShowBlackListSetting"];
            isShowBlackListSet = string.IsNullOrEmpty(isShowBlackListSet) ? "0" : isShowBlackListSet;


            // 客户端设置
            TabItem tab_client_setting = new TabItem();
            tab_client_setting.Header = "客户端设置";
            ClientSettingView client_setting = new ClientSettingView();
            tab_client_setting.Content = client_setting;
            bool hasBrokerClient = false;
            for (int i = tcPage.Items.Count - 1; i >= 0; i--)
            {
                TabItem headerTI = (TabItem)tcPage.Items.GetItemAt(i);
                if (headerTI.Header.ToString() == "管理设置")
                {
                    hasBrokerClient = true;
                    tcPage.Items.Insert(i, tab_client_setting);
                }
            }

            if (!hasBrokerClient)
            {
                this.AddTabPage(tab_client_setting);
            }


            if (isAllow && isShowBlackListSet == "1" && !STC.Common.Utils.CommonUtils.IsBDBAppName())
            {
                //当加载完所有插件的设置界面后，在最后加载
                TabItem span = new TabItem();
                span.Header = "特殊审批";
                span.Content = null;
                span.IsEnabled = false;
                span.IsSelected = false;
                this.AddTabPage(span);

                TabItem tab_whiteSecurity = new TabItem();
                tab_whiteSecurity.Header = "黑名单";
                WhiteSecuritySettingView whiteSecurity = new WhiteSecuritySettingView();
                tab_whiteSecurity.Content = whiteSecurity;
                this.AddTabPage(tab_whiteSecurity);
            }
        }

        private void SettingView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= SettingView_Unloaded;
            tcPage.Items.Clear();
        }

        public TabControl GetTabControl()
        {
            return tcPage;
        }

        public void AddTabPage(TabItem tabItem)
        {
            bool status = true;
            foreach(TabItem ti in tcPage.Items)
            {
                if (ti.Header.Equals(tabItem.Header))
                {
                    status = false;
                    break;
                }                    
            }
            if (status)
            {
                tcPage.Items.Add(tabItem);
                ////if ("双边做市" == tabItem.Header.ToString())
                ////{
                ////    tcPage.Items.Add(tabItem);
                ////}
                ////else
                ////{
                ////    int index = -1;
                ////    for(int i = 0; i < tcPage.Items.Count; i++)
                ////    {
                ////        if ("双边做市" == ((TabItem)tcPage.Items[i]).Header.ToString())
                ////        {
                ////            index = i;
                ////        }
                ////    }
                ////    if (index >= 0)
                ////    {
                ////        tcPage.Items.Insert(index, tabItem);
                ////    }
                ////    else
                ////    {
                ////        tcPage.Items.Add(tabItem);
                ////    }    
                ////}
            }
        }

        public void SetInitSelect()
        {
            if (tcPage.Items != null && tcPage.Items.Count > 0)
                tcPage.SelectedIndex = 0;
        }

        /// <summary>
        /// 打开后选择消息设置
        /// </summary>
        public void SetMsgAlertSettingSelect()
        {
            if (tcPage.Items != null && tcPage.Items.Count > 0)
            {
                foreach (TabItem ti in tcPage.Items)
                {
                    if (ti.Content is MsgAlertSettingView)
                    {
                        tcPage.SelectedItem = ti;
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// 打开后选择平安持仓养老设置
        /// </summary>
        public void SetCopyPositionSettingSelect()
        {
            if (tcPage.Items != null && tcPage.Items.Count > 0)
            {
                foreach (TabItem ti in tcPage.Items)
                {
                    string tabtype = ti.Content.GetType().ToString();
                    if (tabtype.Contains("CopyPositionFieldSettingView"))
                    {
                        tcPage.SelectedItem = ti;
                        break;
                    }
                }
            }
        }

        private void TcPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcPage.SelectedItem != null)
            {
                ISetting setting = (tcPage.SelectedItem as TabItem).Content as ISetting;
                if (setting != null)
                {
                    tbTitle.Text = setting.GetName();

                    btnSave.Visibility = setting.NeedSaveButton() ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = false;
            if (tcPage.SelectedItem != null)
            {
                ISetting setting = (tcPage.SelectedItem as TabItem).Content as ISetting;
                if (setting != null)
                    setting.OnSave();

                
            }
            btnSave.IsEnabled = true;
        }
        

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();


                }
                catch { }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 设置行情面板布局后自动关闭设置界面
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnSettingChange(object obj)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                if (obj != null)
                {
                    string s = Utils.CommonUtils.SerializeObject(obj);
                    if (Utils.JsonSplit.IsJson(s))
                    {
                        if (s.Contains("ShowModel") && s.Contains("Row") && s.Contains("Col"))
                        {
                            this.Close();
                        }
                    }
                }
            }));
        }

        /// <summary>
        /// 打开后选择设置
        /// </summary>
        public void SetInnerSettingSelect(string name)
        {
            if (tcPage.Items != null && tcPage.Items.Count > 0)
            {
                foreach (TabItem ti in tcPage.Items)
                {
                    if (ti.Header.ToString() == name)
                    {
                        tcPage.SelectedItem = ti;
                        break;
                    }
                }
            }
        }
    }
}
