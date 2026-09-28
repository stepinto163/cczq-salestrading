using com.gateway.log;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Frame.UI
{
    /// <summary>
    /// FindSecurityView.xaml 的交互逻辑
    /// </summary>
    public partial class FindSecurityView : UserControl
    {

        private bool isPermission = false;
        public FindSecurityView()
        {
            InitializeComponent();

            this.IsVisibleChanged += FindSecurityView_IsVisibleChanged;
            //Tbook权限
            isPermission = LocalEnv.IsHaveTradeBookPermission || LocalEnv.ConfInfo.IsPAYL;
        }

        private void FindSecurityView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                        new Action(() => Keyboard.Focus(this.tbSearch)));
            }
        }


        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsEsc = true;

                if (string.IsNullOrEmpty(tbSearch.Text))
                {
                    var p = this.Parent as Popup;
                    if (p != null)
                        p.IsOpen = false;
                }

                tbSearch.Text = string.Empty;
                dgResult.DataContext = null;
            }
            else
                IsEsc = false;

            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                dgResult_MouseDoubleClick(null, null);
            }
        }

        private BondGridDataEquality _BondGridDataEq = new BondGridDataEquality();
        private bool IsEsc = false;
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbSearch.Text))
            {
                List<SimpleBondsInfoPo> result = new List<SimpleBondsInfoPo>();
                string txt = this.tbSearch.Text.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<SimpleBondsInfoPo> selectDatas = CommonUtils.GetBondGridData(str, 20);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = CommonUtils.GetBondGridData(tbSearch.Text.Trim(), 20);

                result = result.Distinct(_BondGridDataEq).ToList();
                //result.Sort(_BondGridDataSort);

                dgResult.DataContext = result;
                dgResult.SelectedIndex = 0;
            }
            else
            {
                if (!IsEsc)
                {
                    var p = this.Parent as Popup;
                    if (p != null)
                        p.IsOpen = false;
                }
            }
        }

        private void tbSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgResult.SelectedIndex > 0)
                    dgResult.SelectedIndex--;

                if (dgResult.SelectedItem != null)
                    dgResult.ScrollIntoView(dgResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgResult.SelectedIndex < dgResult.Items.Count - 1)
                    dgResult.SelectedIndex++;

                if (dgResult.SelectedItem != null)
                    dgResult.ScrollIntoView(dgResult.SelectedItem);
            }
        }


        private void dgResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SimpleBondsInfoPo dv = dgResult.SelectedItem as SimpleBondsInfoPo;
            if (dv != null)
            {
                string securityID = dv.Bond_ID;
                var p = this.Parent as Popup;
                if (p != null)
                {
                    dgResult.DataContext = null;
                    p.IsOpen = false;
                }

                object tabItem = AppUtils.Application.Panel.SelectedItem;
                if (tabItem.GetType() == typeof(TabItem))
                {
                    int pluginKey = (int)((tabItem as TabItem).Tag);
                    if (pluginKey == (int)Frame.Common.PluginKeyEnum.GRBroker || pluginKey == (int)Frame.Common.PluginKeyEnum.GRTransaction || pluginKey == (int)Frame.Common.PluginKeyEnum.GRHistory)
                    {
                        //发布消息
                        STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "PopupBorkerQuotesMiniWindows", Info1 = securityID });
                    }
                    else
                    {
                       
                        if (isPermission)
                        {
                            // 使用插件形式打开深度行情页面
                            int tradeBookPluginKey = (int)PluginKeyEnum.TradeBookPlugin;
                            IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(tradeBookPluginKey);
                            if (plugin != null)
                            {
                                plugin.PluginData = securityID;
                                AppUtils.Application.PluginManager.EnablePlugin(tradeBookPluginKey);
                                plugin.PluginData = null;
                                // pluginKey是22，表示TradeBook插件， 可以多次打开
                                // 到load方法中判断 是否需要新建页面
                                if (plugin.PluginKey == tradeBookPluginKey)
                                {
                                    plugin.Enabled = false;
                                }
                            }
                        }
                        else
                        {
                            STC.Common.Utils.CommonUtils.Prompt2Second("提示","没有权限!");
                        }
                        
                    }
                }

                
            }
        }
    }
}
