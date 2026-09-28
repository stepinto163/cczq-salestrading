using com.gateway.log;
using Newtonsoft.Json;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.ViewModel.Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Utils.SqlLite;
using STC.Common.Utils;
using STC.Common.Model.Admin;

namespace BDB.Trade.Plugin.Handler
{
    public class QuoteConfigHandler
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        public bool IsResetMainWin = false;

        private AsyncThreadQueue<UserConfigPo> userConfigThread;

        private Dictionary<string, List<HomeGroupSecurityIDsInfo>> preCardListDic = new Dictionary<string, List<HomeGroupSecurityIDsInfo>>();

        public event Action<QcPagePo> OnDeletePage;
        public event Action<QcPagePo> OnAddPanl;

        public event Action<string, string, string, FavoriteTypeEnum> OnGroupTableChange;

        private Dictionary<string, QuoteConfigPo> configDic = new Dictionary<string, QuoteConfigPo>();

        ~QuoteConfigHandler()
        {
            if (userConfigThread != null)
            {
                userConfigThread.Clear();
                userConfigThread.Dispose();
            }
        }

        public int GetSymbolNumByGroup(string groupId)
        {
            int result = 0;
            QuoteConfigPo quoteConfigPo = GetConfigByGroupId(groupId);
            if (quoteConfigPo != null && quoteConfigPo.Pages != null)
            {
                foreach (QcPagePo pagePo in quoteConfigPo.Pages)
                {
                    if (pagePo.Panels != null)
                    {
                        foreach (QcPanelPo panelPo in pagePo.Panels)
                        {
                            if (!string.IsNullOrEmpty(panelPo.BondCode))
                            {
                                result++;
                            }
                        }
                    }
                }
            }

            return result;
        }


        private void AddOrUpdate(QuoteConfigPo configPo)
        {
            if (configPo != null && !string.IsNullOrEmpty(configPo.GroupId))
            {
                if (configDic.ContainsKey(configPo.GroupId))
                {
                    configDic[configPo.GroupId] = configPo;
                }
                else
                {
                    configDic.Add(configPo.GroupId, configPo);
                }
            }
        }

        public QuoteConfigPo GetConfigByGroupId(string groupId)
        {
            if (!string.IsNullOrEmpty(groupId))
            {
                if (configDic.ContainsKey(groupId))
                {
                    return configDic[groupId];
                }
            }

            return null;
        }


        private QuoteConfigHandler()
        {
            //SqlCreator.SpecCreate<UserConfigPo>("Config", "varchar2(4000)");
            //SqlCreator.SpecCreate<UserConfigPo>("UserName", "varchar2(100)");
            //List<string> pkColList = new List<string>();
            //pkColList.Add("UserName");
            //pkColList.Add("GroupId");
            //SqlliteUtils.CreateTable<UserConfigPo>(pkColList);


            userConfigThread = new AsyncThreadQueue<UserConfigPo>(UpdateData);


            this.LoadSettingData();
        }

        private void LoadSettingData()
        {
            List<QuoteConfigPo> quoteConfigs = QuoteConfigs();

            foreach (QuoteConfigPo configPo in quoteConfigs)
            {
                this.AddOrUpdate(configPo);
            }
        }


        private static QuoteConfigHandler quoteConfigHandler;
        public static QuoteConfigHandler Instance
        {
            get
            {
                if (quoteConfigHandler == null)
                {
                    quoteConfigHandler = new QuoteConfigHandler();
                }
                return quoteConfigHandler;
            }
        }

        public void NotifyOnGroupTableUpdate(string groupId, string id, string securityId, FavoriteTypeEnum favoriteTypeEnum)
        {
            OnGroupTableChange?.Invoke(groupId, id, securityId, favoriteTypeEnum);
        }


        public void NotifyAddPanl(QcPagePo qcPagePo)
        {
            OnAddPanl?.Invoke(qcPagePo);
        }

        public void NotifyDeletePage(QcPagePo qcPagePo)
        {
            OnDeletePage?.Invoke(qcPagePo);
        }


        public List<QuoteConfigPo> QuoteConfigs()
        {
            List<QuoteConfigPo> result = new List<QuoteConfigPo>();

            try
            {
                List<UserConfigPo> list = CardFileManager.Instance.GetAllConfigByCondtion(LocalEnv.UserInfo.UserName, string.Empty);
                    //SqlliteUtils.Query<UserConfigPo>
                    //("select * from UserConfigPo where UserName=?",
                    //new object[] { LocalEnv.UserInfo.UserName });

                if (list.Count > 0)
                {
                    foreach (UserConfigPo userConfigPo in list)
                    {
                        if (!string.IsNullOrEmpty(userConfigPo.Config))
                        {
                            QuoteConfigPo po = JsonConvert.DeserializeObject<QuoteConfigPo>(userConfigPo.Config);
                            result.Add(po);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("QueryConfigHandler QuoteConfigs exception:", e);
            }

            return result;
        }


        public QuoteConfigPo QuoteOneConfig()
        {
            try
            {
                List<UserConfigPo> list = CardFileManager.Instance.GetAllConfigByCondtion(LocalEnv.UserInfo.UserName, string.Empty);
                //SqlliteUtils.Query<UserConfigPo>
                //("select * from UserConfigPo where UserName=?",
                //new object[] { LocalEnv.UserInfo.UserName });
                if (list.Count > 0)
                {
                    UserConfigPo userConfig = list[0];
                    if (!string.IsNullOrEmpty(userConfig.Config))
                    {
                        QuoteConfigPo quoteConfigPo = JsonConvert.DeserializeObject<QuoteConfigPo>(userConfig.Config);

                        this.UpdateSortCache(quoteConfigPo, false);

                        //this.AddOrUpdate(quoteConfigPo);
                        return quoteConfigPo;

                    }
                }
                //else
                //{
                //    QuoteConfigPo defaultConfigPo = new QuoteConfigPo();
                //    defaultConfigPo.Rows = 2;
                //    defaultConfigPo.Cols = 4;

                //    return defaultConfigPo;
                //}
            }
            catch (Exception e)
            {
                LogHelper.Error("QueryConfigHandler QuoteOneConfig exception:", e);
            }
            return null;
        }

        public QuoteConfigPo QueryConfig(string groupId)
        {
            try
            {
                List<UserConfigPo> list = CardFileManager.Instance.GetAllConfigByCondtion(LocalEnv.UserInfo.UserName, groupId);
                //SqlliteUtils.Query<UserConfigPo>
                //("select * from UserConfigPo where UserName=? and GroupId=?",
                //new object[] { LocalEnv.UserInfo.UserName, groupId });
                if (list.Count > 0)
                {
                    UserConfigPo userConfig = list[0];
                    if (!string.IsNullOrEmpty(userConfig.Config))
                    {
                        QuoteConfigPo po = JsonConvert.DeserializeObject<QuoteConfigPo>(userConfig.Config);
                        this.AddOrUpdate(po);
                        return po;
                    }
                }
                //else
                //{
                //    QuoteConfigPo defaultConfigPo = new QuoteConfigPo();
                //    defaultConfigPo.Rows = 2;
                //    defaultConfigPo.Cols = 4;

                //    return defaultConfigPo;
                //}
            }
            catch (Exception e)
            {
                LogHelper.Error("QueryConfigHandler QueryConfig exception:", e);
            }
            return null;
        }

        public void DeleteConfig()
        {
            SetPreCardList();

            configDic.Clear();

            CardFileManager.Instance.Delete(LocalEnv.UserInfo.UserName);

            //string sql = "delete from UserConfigPo where UserName=? and GroupId != 'Mode'";

            //SqlliteUtils.Delete(sql, new object[] { LocalEnv.UserInfo.UserName });
        }

        public List<HomeGroupSecurityIDsInfo> GetPreCardListByGroupId(string groupId)
        {
            if (preCardListDic.ContainsKey(groupId))
            {
                return preCardListDic[groupId];
            }

            return null;
        }

        private void SetPreCardList()
        {
            preCardListDic.Clear();
            if (configDic.Count > 0)
            {
                foreach (KeyValuePair<string, QuoteConfigPo> kv in configDic)
                {
                    if (!preCardListDic.ContainsKey(kv.Key))
                    {
                        preCardListDic.Add(kv.Key, new List<HomeGroupSecurityIDsInfo>());

                        // 使用 LINQ 对列表按 Index 升序排序
                        List<QcPagePo> sortedPages = kv.Value.Pages.OrderBy(page => page.Index).ToList();

                        foreach (QcPagePo qcPagePo in sortedPages)
                        {
                            foreach (QcPanelPo qcPanelPo in qcPagePo.Panels)
                            {
                                if (!string.IsNullOrEmpty(qcPanelPo.BondCode))
                                {
                                    HomeGroupSecurityIDsInfo securityIDsInfo = new HomeGroupSecurityIDsInfo();
                                    securityIDsInfo.securityid = qcPanelPo.BondCode;
                                    securityIDsInfo.id = qcPanelPo.Id;
                                    securityIDsInfo.groupId = qcPagePo.GroupId;

                                    preCardListDic[kv.Key].Add(securityIDsInfo);
                                }
                            }
                        }

                    }
                }
            }
        }

        private void UpdateOneCard(string groupId, string actionType, string id)
        {
            QuoteConfigPo quoteConfigPo = configDic[groupId];
            preCardListDic.Remove(groupId);

            preCardListDic.Add(groupId, new List<HomeGroupSecurityIDsInfo>());

            // 使用 LINQ 对列表按 Index 升序排序
            List<QcPagePo> sortedPages = quoteConfigPo.Pages.OrderBy(page => page.Index).ToList();

            foreach (QcPagePo qcPagePo in sortedPages)
            {
                foreach (QcPanelPo qcPanelPo in qcPagePo.Panels)
                {
                    if (!string.IsNullOrEmpty(qcPanelPo.BondCode))
                    {
                        HomeGroupSecurityIDsInfo securityIDsInfo = new HomeGroupSecurityIDsInfo();
                        securityIDsInfo.securityid = qcPanelPo.BondCode;
                        securityIDsInfo.id = qcPanelPo.Id;
                        securityIDsInfo.groupId = qcPagePo.GroupId;

                        preCardListDic[groupId].Add(securityIDsInfo);
                    }
                }
            }

        }

        public void UpdateConfigByGroupId(string groupId, string id, string securityId, FavoriteTypeEnum favoriteTypeEnum)
        {
            if (configDic.ContainsKey(groupId))
            {
                QuoteConfigPo quoteConfigPo = configDic[groupId];

                // 使用 LINQ 对列表按 Index 升序排序
                List<QcPagePo> sortedPages = quoteConfigPo.Pages.OrderBy(page => page.Index).ToList();
                List<HomeGroupSecurityIDsInfo> securityIDsInfoList = new List<HomeGroupSecurityIDsInfo>();
                foreach (QcPagePo qcPagePo in sortedPages)
                {
                    foreach (QcPanelPo qcPanelPo in qcPagePo.Panels)
                    {
                        if (!string.IsNullOrEmpty(qcPanelPo.BondCode))
                        {
                            HomeGroupSecurityIDsInfo securityIDsInfo = new HomeGroupSecurityIDsInfo();
                            securityIDsInfo.securityid = qcPanelPo.BondCode;
                            securityIDsInfo.id = qcPanelPo.Id;
                            securityIDsInfo.groupId = qcPagePo.GroupId;

                            if (favoriteTypeEnum == FavoriteTypeEnum.DelBondFromTable && qcPagePo.GroupId == groupId && qcPanelPo.Id == id)
                            {
                                //删除
                            }
                            else
                            {
                                securityIDsInfoList.Add(securityIDsInfo);
                            }
                        }
                    }
                }

                if (favoriteTypeEnum == FavoriteTypeEnum.AddBond)
                {
                    if (!securityIDsInfoList.Any(o => o.id == id && o.groupId == groupId))
                    {
                        HomeGroupSecurityIDsInfo newSecurity = new HomeGroupSecurityIDsInfo();
                        newSecurity.securityid = securityId;
                        newSecurity.id = id;
                        newSecurity.groupId = groupId;

                        securityIDsInfoList.Add(newSecurity);
                    }
                }


                QuoteConfigPo newQuote = ResetQuote(groupId, securityIDsInfoList);

                this.SaveConfig(newQuote, groupId);

            }
        }


        public QuoteConfigPo ResetQuote(string groupId, List<HomeGroupSecurityIDsInfo> securityIDsInfosList)
        {
            int pageIndex = 0;

            int pageCount = LayoutSetttingCache.Instance.layoutSettingPo.Row * LayoutSetttingCache.Instance.layoutSettingPo.Col;
            int maxCount = securityIDsInfosList.Count;
            int pageNum = maxCount % pageCount == 0 ? (maxCount / pageCount) : (maxCount / pageCount + 1);

            QuoteConfigPo quoteConfigPo = new QuoteConfigPo();

            quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;

            quoteConfigPo.Pages = new List<QcPagePo>();
            quoteConfigPo.GroupId = groupId;
            quoteConfigPo.CurrentDisplayMode = DisplayMode.Card;
            quoteConfigPo.Rows = LayoutSetttingCache.Instance.layoutSettingPo.Row;
            quoteConfigPo.Cols = LayoutSetttingCache.Instance.layoutSettingPo.Col;

            if (pageNum > 0)
            {
                for (int j = 0; j < pageNum; j++)
                {
                    int index = j * pageCount;
                    
                    QcPagePo pagePo = new QcPagePo();
                    if (pageIndex == index)
                        pagePo.IsSeleted = true;

                    pagePo.GroupId = groupId;
                    pagePo.Index = j + 1;
                    pagePo.Panels = new List<QcPanelPo>();
                    quoteConfigPo.Pages.Add(pagePo);

                    for (int i = 0; i < pageCount; i++)
                    {
                        QcPanelPo panelPo = new QcPanelPo();
                        if (index < securityIDsInfosList.Count)
                        {
                            HomeGroupSecurityIDsInfo securityIDsInfo = securityIDsInfosList[index];
                            if (securityIDsInfo != null && !string.IsNullOrEmpty(securityIDsInfo.securityid))
                            {
                                panelPo.BondCode = securityIDsInfo.securityid;
                                panelPo.Id = securityIDsInfo.id;


                            }
                        }
                        panelPo.Row = (int)(i / quoteConfigPo.Cols) + 1;
                        panelPo.Col = (i % quoteConfigPo.Cols == 0) ? quoteConfigPo.Cols : (i % quoteConfigPo.Cols + 1);
                        pagePo.Panels.Add(panelPo);

                        index++;
                    }

                }
            }
            else
            {
                QcPagePo pagePo = new QcPagePo();
                pagePo.GroupId = groupId;
                pagePo.Index = 1;
                pagePo.Panels = new List<QcPanelPo>();
                quoteConfigPo.Pages.Add(pagePo);
                for (int i = 0; i < pageCount; i++)
                {
                    QcPanelPo panelPo = new QcPanelPo();

                    panelPo.BondCode = string.Empty;
                    panelPo.Id = string.Empty;

                    panelPo.Row = (int)(i / quoteConfigPo.Cols) + 1;
                    panelPo.Col = (i % quoteConfigPo.Cols == 0) ? quoteConfigPo.Cols : (i % quoteConfigPo.Cols + 1);

                    pagePo.Panels.Add(panelPo);
                }

            }

            return quoteConfigPo;
        }


        public void DeleteConfigByGroupId(string groupId, bool needCache)
        {
            if (configDic.ContainsKey(groupId))
            {
                //string sql = "delete from UserConfigPo where UserName=? and GroupId =?";

                //SqlliteUtils.Delete(sql, new object[] { LocalEnv.UserInfo.UserName, groupId });

                CardFileManager.Instance.DeleteConfigByGroupId(LocalEnv.UserInfo.UserName, groupId);

                if (needCache)
                {
                    //UpdateOneCard(groupId);
                }

                if (preCardListDic.ContainsKey(groupId))
                    preCardListDic.Remove(groupId);

                configDic.Remove(groupId);

                SortCache.Instance.RemoveByGroupId(groupId);
                RememberHelper.Instance.WriteCustomSort(groupId);
            }
        }

        private void UpdateSortCache(QuoteConfigPo po, bool needRemember)
        {
            List<QcPagePo> pageList = po.Pages;
            if (pageList != null && pageList.Count > 0)
            {
                SortCache.Instance.RemoveByGroupId(po.GroupId);
                int sort = 0;
                foreach (QcPagePo pagePo in pageList)
                {
                    List<QcPanelPo> panelList = pagePo.Panels;
                    
                    foreach (QcPanelPo panel in panelList)
                    {
                        if (!string.IsNullOrEmpty(panel.Id) && !string.IsNullOrEmpty(panel.BondCode))
                        {
                            SortCache.Instance.AddOrUpdate(po.GroupId, panel.Id, sort);
                            sort++;
                        }
                    }
                }
            }

            if (needRemember)
            {
                RememberHelper.Instance.WriteCustomSort(po.GroupId);
            }
        }

        public void SaveConfig(QuoteConfigPo po, string groupId)
        {
            this.AddOrUpdate(po);

            this.UpdateSortCache(po, true);

            string config = JsonConvert.SerializeObject(po);
            UserConfigPo userConfig = new UserConfigPo();
            userConfig.UserName = LocalEnv.UserInfo.UserName;
            userConfig.Config = config;
            userConfig.GroupId = groupId;

            userConfigThread.Enqueue(userConfig);

            
        }

        public void IntoSaveQueu(UserConfigPo configPo)
        {
            userConfigThread.Enqueue(configPo);
        }

        public int GetSecurityIndexById()
        {
            int result = -1;
            if (LayoutSetttingCache.Instance.CurrentUpdatePanle != null)
            {
                int row = LayoutSetttingCache.Instance.CurrentUpdatePanle.Row;
                int col = LayoutSetttingCache.Instance.CurrentUpdatePanle.Col;
                string groupId = LayoutSetttingCache.Instance.CurrentUpdatePanle.GroupId;
                int pageIndex = LayoutSetttingCache.Instance.CurrentUpdatePanle.PageIndex;

                QuoteConfigPo quoteConfig = GetConfigByGroupId(groupId);
                if (quoteConfig != null)
                {
                    List<QcPagePo> pagePos = quoteConfig.Pages;

                    foreach (QcPagePo pagePo in pagePos)
                    {
                        if (pagePo.Index < pageIndex)
                        {
                            List<QcPanelPo> panelPos = pagePo.Panels;
                            foreach (QcPanelPo panelPo in panelPos)
                            {
                                if (!string.IsNullOrEmpty(panelPo.Id))
                                {
                                    result++;
                                }
                            }
                        }
                        if (pagePo.Index == pageIndex)
                        {
                            List<QcPanelPo> panelPos = pagePo.Panels;
                            foreach (QcPanelPo panelPo in panelPos)
                            {
                                if (panelPo.Row < row && !string.IsNullOrEmpty(panelPo.Id))
                                {
                                    result++;
                                }
                                else if (panelPo.Row == row && panelPo.Col < col && !string.IsNullOrEmpty(panelPo.Id))
                                {
                                    result++;
                                }
                            }
                        }


                    }
                }
            }

            if (result >= 0)
            {
                result++;//修改位置
            }

            return result;
        }

        private void UpdateData(UserConfigPo userConfigPo)
        {
            CardFileManager.Instance.Insert(userConfigPo);
            //SqlliteUtils.Replace<UserConfigPo>(userConfigPo);
        }



        public void UpdateSelectPage(string groupId, int selectPageNum)
        {
            QuoteConfigPo quoteConfigPo = this.GetConfigByGroupId(groupId);
            if (quoteConfigPo != null)
            {
                foreach (QcPagePo qcPage in quoteConfigPo.Pages)
                {
                    qcPage.IsSeleted = qcPage.Index == selectPageNum;

                }

                this.SaveConfig(quoteConfigPo, groupId);
            }
        }
    }
}
