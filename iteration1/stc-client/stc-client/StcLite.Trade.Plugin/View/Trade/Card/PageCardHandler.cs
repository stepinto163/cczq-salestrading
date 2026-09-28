using com.gateway.log;
using Newtonsoft.Json;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using STC.Common.Model.Admin;
using System.Windows;
using System.Collections.Concurrent;

namespace BDB.Trade.Plugin.View.Trade.Card
{
    public class PageCardHandler
    {

        private static PageCardHandler pageCardHandler = new PageCardHandler();

        public static PageCardHandler Instance { get { return pageCardHandler; } }

        public PageCardHandler()
        {
            AddPageCardItemView(string.Empty);//默认添加主界面的PageCardItemView
            CommonHandler.Instance.OnCardLoadEnd += Instance_OnCardLoadEnd;
        }

        public static string GetWindowHashKey(FrameworkElement ownUI)
        {
            string key = string.Empty;
            if (ownUI != null)
            {
                Window w = Window.GetWindow(ownUI);
                if (w != null && !(w is STC.Frame.Window2))
                {
                    key = w.GetHashCode().ToString();
                }
                //else
                //{
                //    var pcv = CommonUtils.GetParentObject<PageCardView>(ownUI);
                //}

                if (w == null)
                {

                }
            }
            return key;
        }

        private void Instance_OnCardLoadEnd(string key)
        {
            if (showPageCardItemViewDic.ContainsKey(key))
            {
                PageCardItemViewModel pageCardItemViewModel = showPageCardItemViewDic[key].DataContext as PageCardItemViewModel;

                if (pageCardItemViewModel != null)
                {
                    pageCardItemViewModel.LoadData();
                }
            }
        }

        private ConcurrentDictionary<string, PageCardItemView> showPageCardItemViewDic = new ConcurrentDictionary<string, PageCardItemView>();

        public void AddPageCardItemView(string key)
        {
            if (!showPageCardItemViewDic.ContainsKey(key))
            {
                showPageCardItemViewDic[key] = new PageCardItemView();
            }
        }
        public void RemovePageCardItemView(string key)
        {
            if (showPageCardItemViewDic.ContainsKey(key))
            {
                PageCardItemView removeView;
                showPageCardItemViewDic.TryRemove(key, out removeView);
                if (removeView != null)
                {
                    removeView.ClearUI(true);
                }
            }
        }

        public bool IsCreateCardView(string key)
        {
            if (showPageCardItemViewDic.ContainsKey(key))
            {
                PageCardItemViewModel pageCardItemViewModel = showPageCardItemViewDic[key].DataContext as PageCardItemViewModel;

                if (pageCardItemViewModel != null)
                    return pageCardItemViewModel.GetPriceAndOrderViewModelList().Count > 0;
            }

            return false;
        }

        public void ClearCardView(string key)
        {
            if (showPageCardItemViewDic.ContainsKey(key))
            {
                showPageCardItemViewDic[key].ClearUI(true);
            }
        }

        public void ClearUI(string key)
        {
            if (showPageCardItemViewDic.ContainsKey(key))
            {
                showPageCardItemViewDic[key].ClearUI(false);
            }
        }

        public void ResetData(TabItem tabItem, string groupId, int showPageIndex, string key)
        {
            if (showPageCardItemViewDic.ContainsKey(key))
            {
                PageCardItemView showPageCardItemView = showPageCardItemViewDic[key];
                if (showPageCardItemView.Parent != null && showPageCardItemView.Parent is TabItem)
                {
                    (showPageCardItemView.Parent as TabItem).Content = null;
                }
                tabItem.Content = showPageCardItemView;

                List<HomeGroupSecurityIDsInfo> homeGroupSecurityIDsInfos = GetPageSecurityIDList(groupId, showPageIndex, DataType.SQLite);
                showPageCardItemView.ResetData(groupId, showPageIndex, homeGroupSecurityIDsInfos);

                QuoteConfigHandler.Instance.UpdateSelectPage(groupId, showPageIndex);
            }
        }


        public List<HomeGroupSecurityIDsInfo> GetPageSecurityIDList(string groupId, int showPageIndex, DataType dataType)
        {
            List<HomeGroupSecurityIDsInfo> securityList = new List<HomeGroupSecurityIDsInfo>();
            HomeGroupInfo copyGroup = null;
            if (dataType == DataType.DB)
            {
                HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(groupId);

                string groupJson = JsonConvert.SerializeObject(homeGroupInfo);

                copyGroup = JsonConvert.DeserializeObject<HomeGroupInfo>(groupJson);

                bool isContain = SortCache.Instance.ContainGroup(copyGroup.id);
                if (isContain)
                {
                    foreach (HomeGroupSecurityIDsInfo securityIDsInfo in homeGroupInfo.securityids)
                    {
                        int cusSort = SortCache.Instance.GetSettleById(copyGroup.id, securityIDsInfo.id);
                        securityIDsInfo.sort = cusSort;
                    }

                    homeGroupInfo.securityids.Sort(new HomeSecurityIdCompare());
                }
            }
            else if (dataType == DataType.SQLite)
            {
                copyGroup = new HomeGroupInfo();
                copyGroup.id = groupId;
                copyGroup.securityids = new List<HomeGroupSecurityIDsInfo>();
                QuoteConfigPo quoteConfigPo = QuoteConfigHandler.Instance.GetConfigByGroupId(groupId);//QueryConfig(groupId);

                if (quoteConfigPo != null)//改变从cache获取设置
                {
                    foreach (QcPagePo qcPagePo in quoteConfigPo.Pages)
                    {
                        if (qcPagePo.Index == showPageIndex)
                        {
                            foreach (QcPanelPo panelPo in qcPagePo.Panels)
                            {
                                HomeGroupSecurityIDsInfo securityIDsInfo = new HomeGroupSecurityIDsInfo();
                                securityIDsInfo.groupId = groupId;
                                securityIDsInfo.securityid = panelPo.BondCode;
                                securityIDsInfo.id = panelPo.Id;

                                copyGroup.securityids.Add(securityIDsInfo);
                            }

                            break;
                        }

                    }
                }
                else
                {
                    copyGroup.securityids = GetPageSecurityIDList(groupId, showPageIndex, DataType.DB);
                }
            }
            else
            {
                copyGroup = new HomeGroupInfo();
                copyGroup.securityids = new List<HomeGroupSecurityIDsInfo>();
            }

            int row = LayoutSetttingCache.Instance.layoutSettingPo.Row;
            int col = LayoutSetttingCache.Instance.layoutSettingPo.Col;
            int pageCount = LayoutSetttingCache.Instance.layoutSettingPo.Row * LayoutSetttingCache.Instance.layoutSettingPo.Col;


            if (copyGroup.securityids.Count > 0)
            {

                int maxCount = copyGroup.securityids.Count;

                int index = 0;
                while (true)
                {
                    if (index < maxCount)
                    {
                        securityList.Add(copyGroup.securityids[index]);
                    }
                    else
                    {
                        break;
                    }
                    index++;
                }

                if (securityList.Count < pageCount)//页面设置的债券不够，补充空券
                {
                    int interval = pageCount - securityList.Count;
                    for (int i = 0; i < interval; i++)
                    {
                        securityList.Add(null);
                    }
                }
            }

            if (dataType == DataType.AddEmptyPage)
            {
                for (int i = 0; i < pageCount; i++)
                {
                    securityList.Add(null);
                }

            }


            return securityList;
        }

        public void ResetPage(TabItem tabItem, string groupId, int showPageIndex, DataType dataType, string key)
        {
            if (showPageCardItemViewDic.ContainsKey(key))
            {
                PageCardItemView showPageCardItemView = showPageCardItemViewDic[key];
                showPageCardItemView.ClearUI(false);
                //tabItem.Content = null;

                int row = LayoutSetttingCache.Instance.layoutSettingPo.Row;
                int col = LayoutSetttingCache.Instance.layoutSettingPo.Col;
                List<HomeGroupSecurityIDsInfo> homeGroupSecurityIDsInfos = GetPageSecurityIDList(groupId, showPageIndex, dataType);

                tabItem.Content = showPageCardItemView;

                showPageCardItemView.Init(homeGroupSecurityIDsInfos, row, col, groupId, showPageIndex);

                showPageCardItemView.PageCardItemView_SizeChanged(null, null);
            }
        }
    }

    public class HomeSecurityIdCompare : IComparer<HomeGroupSecurityIDsInfo>
    {
        public int Compare(HomeGroupSecurityIDsInfo x, HomeGroupSecurityIDsInfo y)
        {
            if (x == null || y == null)
                return 0;
            else
                return x.sort >= y.sort ? 1 : -1;
        }
    }
}
