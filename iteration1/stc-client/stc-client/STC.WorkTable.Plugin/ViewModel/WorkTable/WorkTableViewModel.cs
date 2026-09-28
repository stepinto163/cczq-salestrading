using STC.Common.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Handler.OwnExec;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Frame.UI;
using STC.Frame.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace STC.WorkTable.Plugin.ViewModel
{
    /// <summary>
    /// WorkTableView的数据内容
    /// </summary>
    public class WorkTableViewModel : ViewModelBase
    {

        /// <summary>
        /// 打开历史查询界面
        /// </summary>
        public static void OpenHistoryPage(bool isExecution)
        {
            // 使用插件形式打开历史页面
            int tradeBookPluginKey = (int)PluginKeyEnum.HistoryPlugin;

            IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(tradeBookPluginKey);
            if (plugin != null)
            {
                bool isExist = TabPageHelper.IsExistTabItem(plugin.Description);
                if (isExist == false)
                {
                    AppUtils.Application.PluginManager.EnablePlugin(tradeBookPluginKey);
                }


                if (AppUtils.Application.Panel.SelectedItem != null
                && AppUtils.Application.Panel.SelectedItem is TabItem)
                {
                    TabItem tabItem = AppUtils.Application.Panel.SelectedItem as TabItem;
                    //if (tabItem.Content is HistoryView)
                    //{

                    //    HistoryView historyView = tabItem.Content as HistoryView;
                    //    if (isExecution)
                    //    {
                    //        historyView.SelectDealView();
                    //    }
                    //    else
                    //    {
                    //        historyView.SelectOrderView();
                    //    }
                    //}
                    try
                    {
                        if (tabItem.Content != null)
                        {
                            if (isExecution)
                            {
                                MethodInfo mi = tabItem.Content.GetType().GetMethod("SelectDealView");
                                if (mi != null)
                                {
                                    mi.Invoke(tabItem.Content, null);
                                }
                            }
                            else
                            {
                                MethodInfo mi = tabItem.Content.GetType().GetMethod("SelectOrderView");
                                if (mi != null)
                                {
                                    mi.Invoke(tabItem.Content, null);
                                }
                            }
                            
                        }
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }
                }
            }
        }
    }
}
