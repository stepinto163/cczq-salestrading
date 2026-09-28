using com.gateway.log;
using Dragablz;
using STC.Common.Data;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.View.Component;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Common.View.UI
{
    /// <summary>
    /// ClientSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class ClientSettingView : UserControl, ISetting
    {
        public ClientSettingView()
        {
            InitializeComponent();

            bool isDpsPermission = Handler.Admin.AdminHandler.Instance.CheckPermisionById("5000", "5012");

            if (!isDpsPermission)
            {
                this.StartDpsChk.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.StartDpsChk.IsChecked = LocalEnv.ConfInfo.StartDps == "1";
            }
        }

        public string GetName()
        {
            return "客户端设置";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public void OnSave()
        {
            throw new NotImplementedException();
        }

        private string GetCacheKey()
        {
            return string.Format("{0}-{1}", OpenPlugin, LocalEnv.UserInfo.UserName);
        }


        private void WriteHistory()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                IEnumerable<DragablzItem> items = AppUtils.Application.TabablzPanel.GetOrderedHeaders();
                foreach (DragablzItem tab in items)
                {
                    TabItem temItem = tab.DataContext as TabItem;
                    int pluginKey = (int)temItem.Tag;
                    if (pluginKey < 0)//TradeBookPlugin特殊处理
                        pluginKey = 22;
                    IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(pluginKey);
                    if (plugin != null)
                    {
                        sb.Append(string.Format("{0}-{1},", pluginKey, temItem.Header));
                    }
                }

                string selectPlugin = sb.ToString();
                selectPlugin = selectPlugin.Length > 0 ? selectPlugin.Substring(0, selectPlugin.Length - 1) : string.Empty;

                TabItem tabItem = AppUtils.Application.Panel.SelectedItem as TabItem;
                if (tabItem != null)
                {
                    IniFileHelper.WriteValue(GetCacheKey(), SelectedTagKey, tabItem.Header as string);
                }

                IniFileHelper.WriteValue(GetCacheKey(), OpenPluginKey, selectPlugin);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }


        public void SaveOpenWindows()
        {
            LogHelper.Info("Save open popups...");
            //XBond 记忆行情小窗窗口
            string[] keys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("QuotesMiniWindows")).ToArray();
            var strkeys = string.Join(",", keys).Replace("QuotesMiniWindows-", "");
            IniFileHelper.WriteValue($"QuotesMiniWindows-{LocalEnv.UserInfo.UserName}", "Keys", strkeys);

            //记忆GR 报价小窗
            string[] grkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("BrokerCardPriceViewWindows")).ToArray();
            var strgrkeys = string.Join(",", grkeys).Replace("BrokerCardPriceViewWindows-", "");
            IniFileHelper.WriteValue($"BrokerCardPrice-{LocalEnv.UserInfo.UserName}", "Keys", strgrkeys);

            //记忆GR 明细小窗，过桥小窗
            string[] cpkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("Transaction")).ToArray();
            var cpgrkeys = string.Join(",", cpkeys).Replace("Transaction-", "");
            IniFileHelper.WriteValue($"GrWin-{LocalEnv.UserInfo.UserName}", "Keys", cpgrkeys);

            //XBond 记忆打开的窗口
            string[] XBondwkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.XBond.Plugin")).ToArray();
            var XBondstrwkeys = string.Join(",", XBondwkeys).Replace("STC.XBond.Plugin-", "");
            IniFileHelper.WriteValue($"STC.XBond.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", XBondstrwkeys);


            // 双边做市 记忆策略小窗窗口
            string[] Mmwkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.Mm.Plugin")).ToArray();
            var Mmstrwkeys = string.Join(",", Mmwkeys).Replace("STC.Mm.Plugin-", "");
            IniFileHelper.WriteValue($"STC.Mm.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", Mmstrwkeys);


            // 双边做市 记忆策略行情小窗
            string[] OCwkeysorder = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("OrderControl")).ToArray();
            var OCstrwkeysorder = string.Join(",", OCwkeysorder).Replace("OrderControl-", "");
            IniFileHelper.WriteValue($"OrderControl-{LocalEnv.UserInfo.UserName}", "Keys", OCstrwkeysorder);

            // 策略控制台 记忆窗口
            string[] quantwkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.quant.plugin")).ToArray();
            var quantstrwkeys = string.Join(",", quantwkeys).Replace("STC.quant.plugin-", "");
            IniFileHelper.WriteValue($"STC.quant.plugin-{LocalEnv.UserInfo.UserName}", "Keys", quantstrwkeys);

            // 新需求工作台 记忆窗口
            string[] WorkTablewkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.WorkTable.Plugin")).ToArray();
            var WorkTablestrwkeys = string.Join(",", WorkTablewkeys).Replace("STC.WorkTable.Plugin-", "");
            IniFileHelper.WriteValue($"STC.WorkTable.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", WorkTablestrwkeys);

            // 需求工作台 记忆窗口
            string[] WorkBenchwkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.WorkBench.Plugin")).ToArray();
            var WorkBenchstrwkeys = string.Join(",", WorkBenchwkeys).Replace("STC.WorkBench.Plugin-", "");
            IniFileHelper.WriteValue($"STC.WorkBench.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", WorkBenchstrwkeys);

            // 经纪商成交 记忆窗口
            string[] Executewkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.Execute.Plugin")).ToArray();
            var Executestrwkeys = string.Join(",", Executewkeys).Replace("STC.Execute.Plugin-", "");
            IniFileHelper.WriteValue($"STC.Execute.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", Executestrwkeys);

            // 交易所做市 记忆策略小窗窗口
            string[] Mmshewkeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("STC.Mmshe.Plugin")).ToArray();
            var Mmshestrwkeys = string.Join(",", Mmshewkeys).Replace("STC.Mmshe.Plugin-", "");
            IniFileHelper.WriteValue($"STC.Mmshe.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", Mmshestrwkeys);

            // 交易所做市 记忆策略行情小窗
            string[] OCHwkeysorder = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("OrderControlShe")).ToArray();
            var OCHstrwkeysorder = string.Join(",", OCHwkeysorder).Replace("OrderControlShe-", "");
            IniFileHelper.WriteValue($"OrderControlShe-{LocalEnv.UserInfo.UserName}", "Keys", OCHstrwkeysorder);

            // BDB 
            string[] dbdKeys = WindowManager.Instance._UniqueKeyMapWindowDic.Keys.Where(x => x.Contains("BDB.Price.Plugin")).ToArray();
            var dbdkey = string.Join(",", dbdKeys).Replace("BDB.Price.Plugin-", "");
            IniFileHelper.WriteValue($"BDB.Price.Plugin-{LocalEnv.UserInfo.UserName}", "Keys", dbdkey);
        }

        private const string OpenPlugin = "OpenPlugin";
        private const string OpenPluginKey = "Plugin";
        private const string SelectedTagKey = "FocusPlugin";
        private const string OpenType = "OpenType";

        private void repairDataBtn_Click(object sender, RoutedEventArgs e)
        {
            // 弹框
            if (CustomMessageBox.Question("数据修复需要重启客户端, 是否继续?","提示", Window.GetWindow(this)))
            {
                Window.GetWindow(this).Close();

                try
                {
                    //1.创建temp.db， 表示重启后需要删除db文件
                    if (!FileHelper.FileIsExists(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\STCTemp.db")))
                    {
                        FileStream fs = File.Create(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\STCTemp.db"));
                        fs.Close();
                    }
                    if (!FileHelper.FileIsExists(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\TagsTemp.db")))
                    {
                        FileStream fs = File.Create(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\TagsTemp.db"));
                        fs.Close();
                    }
                    if (!FileHelper.FileIsExists(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\WorkbenchTemp.db")))
                    {
                        FileStream fs = File.Create(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\WorkbenchTemp.db"));
                        fs.Close();
                    }

                    //2.重启
                    LogHelper.Info("repairData Restart one...");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SaveOpenWindows();
                    });
                    // WindowManager.Instance.CloseAllWindow();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WriteHistory();
                    });

                    LogHelper.Info("repairData Restart two...");
                    IniFileHelper.WriteValue("StartType", OpenType, "1");
                    LogHelper.Info("repairData Restart three...");
                    //Process.Start(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalEnv.ServerInfo.AppExe));

                    System.Windows.Forms.Application.Restart();
                    LogHelper.Info("Restart four...");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LogHelper.Info("Restart five...");
                        Application.Current.Shutdown();
                        LogHelper.Info("Restart six...");
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }

        private void StartDpsChk_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 弹框
            if (CustomMessageBox.Question("修改DPS启动方式需重启客户端, 是否继续?", "提示", Window.GetWindow(this)))
            {
                try
                {
                    //1.修改config文件
                    Configuration config = ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                    config.AppSettings.Settings["StartDps"].Value = StartDpsChk.IsChecked == false ? "1" : "0";
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    LogHelper.Info($"StartDps：{config.AppSettings.Settings["StartDps"].Value}");

                    //2.重启
                    LogHelper.Info("repairStartDps Restart one...");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SaveOpenWindows();
                    });
                    // WindowManager.Instance.CloseAllWindow();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WriteHistory();
                    });

                    LogHelper.Info("repairStartDps Restart two...");
                    IniFileHelper.WriteValue("StartType", OpenType, "1");
                    LogHelper.Info("repairStartDps Restart three...");
                    //Process.Start(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalEnv.ServerInfo.AppExe));

                    System.Windows.Forms.Application.Restart();
                    LogHelper.Info("Restart four...");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LogHelper.Info("Restart five...");
                        Application.Current.Shutdown();
                        LogHelper.Info("Restart six...");
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
