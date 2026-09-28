using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.Common.Utils
{
    /// <summary>
    /// TabPage处理工具类
    /// </summary>
    public class TabPageHelper
    {
        /// <summary>
        /// 打开TabPage
        /// </summary>
        /// <param name="header"></param>
        /// <param name="userControl"></param>
        /// <param name="tag"></param>
        public static void OpenTabPage(object header, object userControl, object tag)
        {
            bool isExist = IsExistTabItem(header.ToString());

            if (isExist)
            {
                return;
            }

            TabItem tabItem = new TabItem();
            tabItem.Header = header;
            tabItem.Content = userControl;
            if (tag != null)
            {
                int index = tag.ToString().IndexOf('-');
                if (index > -1)
                {
                    tabItem.Tag = tag.ToString().Substring(0, index).ConvertToInt();
                    tabItem.ToolTip = tag.ToString().Substring(index + 1);
                }
                else
                {
                    tabItem.Tag = tag;
                }
            }
            
            tabItem.IsSelected = true;
            if (tag.ToString() == "80")
                AppUtils.Application.Panel.Items.Insert(0, tabItem);
            else
            { 
                AppUtils.Application.Panel.Items.Add(tabItem);
                if (tabItem.GetType() == typeof(TabItem))
                {
                    if (((TabItem)tabItem).Content.GetType().ToString() == "STC.Broker.Plugin.View.HomePage.HomePageView")
                    {
                        AppUtils.Application.Panel.SelectedItem = tabItem;
                    }
                }
            }

            // 加入tabpage
            //TabItem item = new TabItem();
            //item.Unloaded += Item_Unloaded;
            //item.Header = header;
            //item.Content = userControl;
            //item.Tag = tag;
            //item.IsSelected = true;
            //CommonUtils.Application.Panel.Items.Add(item);
        }

        /// <summary>
        /// 根据tagHeader判断tabpage是否存在
        /// </summary>
        /// <param name="tagHeader"></param>
        /// <returns></returns>
        public static bool IsExistTabItem(string tagHeader)
        {
            bool isExist = false;
            try
            {
                TabItem currentSelectedItem = (TabItem)AppUtils.Application.Panel.SelectedItem;
                if (tagHeader != null && currentSelectedItem != null)
                {
                    if (!currentSelectedItem.Header.ToString().Equals(tagHeader))
                    {
                        foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
                        {
                            if (tabItem.Header.ToString().Equals(tagHeader))
                            {
                                AppUtils.Application.Panel.SelectedItem = tabItem;
                                isExist = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        isExist = true;
                    }
                }
            }
            catch { }

            return isExist;
        }

        public static TabItem GetExistTabItem(string tagHeader)
        {
            TabItem tabItem = null;
            try
            {
                if (tagHeader != null)
                {
                    foreach (TabItem item in AppUtils.Application.Panel.Items)
                    {
                        if (item.Header.ToString().Equals(tagHeader))
                        {
                            tabItem = item;
                            break;
                        }
                    }
                }
            }
            catch { }
            return tabItem;
        }

        public static void SwitchTradeBookHeader()
        {
            foreach (TabItem item in AppUtils.Application.Panel.Items)
            {
                if (item.Tag.ConvertToString() == "22")
                {
                    string securityid = item.ToolTip.ConvertToString();
                    Common.Model.Base.BondsInfoPo bi = CommonUtils.GetBondsInfoPoBySecurityID(securityid);
                    // 如果是信用债，显示债券名
                    if (CommonUtils.TradeBookHeader == "1" && bi != null)
                    {
                        item.Header = bi.Short_Name;
                    }
                    // 其他
                    else
                    {
                        item.Header = securityid;
                    }
                }
            }
        }
    }
}
