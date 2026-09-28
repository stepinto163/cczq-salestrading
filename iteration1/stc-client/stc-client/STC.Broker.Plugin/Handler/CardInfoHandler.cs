using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Card;
using STC.Broker.Plugin.ViewModel.UI;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Handler
{
    public class CardInfoHandler
    {
        private readonly string dbFile = AppDomain.CurrentDomain.BaseDirectory + @"data\CardInfo_1.3.data";

        private string GrUserFavoritesSettings = "GR_User_Favorites_Setting";

        private static CardInfoHandler instance = new CardInfoHandler();
        public static CardInfoHandler Instance
        {
            get { return instance; }
        }

        public CardInfoHandler()
        {
            
        }

        /// <summary>
        /// 从本地配置获取组债券列表
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public QuotePageModel QueryQuotePageConfig(string groupId)
        {
            try
            {
                List<QuotePageModel> list = GetAllConfigByCondtion(groupId);
                if (list.Count > 0)
                {
                   return list[0];
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("QueryConfigHandler QueryConfig exception:", e);
            }

            return null;
        }


        private Dictionary<string, QuotePageModel> DictQuotePage = new Dictionary<string, QuotePageModel>();
        /// <summary>
        /// 从服务器查询组债券列表
        /// </summary>
        /// <param name="groupId"></param>
        /// 
        public List<QuotePageModel> QueryGrUserFavoritesSettings(string groupId = "")
        {
            List<QuotePageModel> res = new List<QuotePageModel>();

            try
            {
                SystemParameterReq req = new SystemParameterReq();
                req.ParamKey = GrUserFavoritesSettings;
                req.Username = LocalEnv.UserInfo.UserName;
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                {
                    Dictionary<string, QuotePageModel> dictPage = new Dictionary<string, QuotePageModel>();
                    if (JsonSplit.IsJson(reply.Datas.tbookSystemParameter.param_value))
                    {
                        dictPage = CommonUtils.DeserializeObject<Dictionary<string, QuotePageModel>>(reply.Datas.tbookSystemParameter.param_value);
                        foreach (QuotePageModel vr in dictPage.Values)
                        {
                            DictQuotePage[vr.GroupId] = vr;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(groupId))
                {
                    if (DictQuotePage.ContainsKey(groupId))
                    {
                        res.Add(DictQuotePage[groupId]);
                    }
                }
                else
                {
                    res.AddRange(DictQuotePage.Values);
                }
            }
            catch 
            {
                res = null;
            }

            return res;
        }

        /// <summary>
        /// 保存 GR债券列表组 到服务器,程序退出时保存到服务器
        /// </summary>
        /// <param name="qpm"></param>
        public void SaveGrUserFavoritesSettings(QuotePageModel qpm)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                try
                {
                    DictQuotePage[qpm.GroupId] = qpm;
                    //2更新；3删除
                    SystemGlobalParamReq req = new SystemGlobalParamReq();
                    req.action = "2";
                    req.list = new List<TTbookSystemParameter>();
                    TTbookSystemParameter param = new TTbookSystemParameter();
                    param.param_key = GrUserFavoritesSettings;
                    param.username = LocalEnv.UserInfo.UserName;
                    param.param_value = CommonUtils.SerializeObject(DictQuotePage);
                    param.param_desc = "GR债券列表组";
                    req.list.Add(param);

                    PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {
                            LogHelper.Info($"保存GR债券列表组成功");
                        }
                        else
                        {
                            LogHelper.Error($"保存GR债券列表组失败！原因:{reply.Msg}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"保存GR债券列表组失败！原因:{ex.Message}");
                }
            });
        }

        #region 新的方法
        /// <summary>
        /// 根据保存的债券信息，重新排版
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="qpm"></param>
        /// <returns></returns>
        public QuotePageModel ResetCurQuoteConfigPo(string groupId, string groupName, ObservableCollection<CardItem> qpm)
        {
            int pageCount = LocalEnv.GrLayoutSetting.Row * LocalEnv.GrLayoutSetting.Col;
            List<CardItem> listCardItem = qpm.Where(o => !string.IsNullOrEmpty(o.SecurityID)).ToList();
            int maxCount = listCardItem.Count;
            int pageNum = maxCount % pageCount == 0 ? (maxCount / pageCount) : (maxCount / pageCount + 1);

            QuotePageModel quoteConfigPo = new QuotePageModel();
            quoteConfigPo.GroupId = groupId;
            quoteConfigPo.GroupName = groupName;
            quoteConfigPo.UserName = LocalEnv.UserInfo.UserName;

            if (pageNum > 0)
            {
                for (int j = 0; j < pageNum; j++)
                {
                    int index = j * pageCount;

                    for (int i = 0; i < pageCount; i++)
                    {
                        QuoteGroupConfigModel panelPo = new QuoteGroupConfigModel();
                        if (index < maxCount)
                        {
                            CardItem po = listCardItem[index];
                            panelPo.Id = po.Id;
                            panelPo.ParentId = po.ParentId;
                            panelPo.SecurityID = po.SecurityID ?? "";
                            panelPo.SymbolName = po.SymbolName ?? "";
                            panelPo.SettleIndex = po.SettleIndex;
                            panelPo.RemaingingTime = po.RemaingingTime;
                            panelPo.RemaingingTimeConvert = po.RemaingingTimeConvert;
                            panelPo.BondCategory = po.BondCategory;
                            panelPo.BondType = po.BondType;
                        }

                        panelPo.Page = j;
                        panelPo.CustomSort = i;
                        panelPo.GroupId = quoteConfigPo.GroupId;
                        quoteConfigPo.Panels.Add(panelPo);

                        index++;
                    }
                }
            }
            else
            {

                for (int i = 0; i < pageCount; i++)
                {
                    QuoteGroupConfigModel panelPo = new QuoteGroupConfigModel();
                    panelPo.Page = 0;
                    panelPo.CustomSort = i;
                    panelPo.GroupId = quoteConfigPo.GroupId;
                    quoteConfigPo.Panels.Add(panelPo);
                }
            }

            return quoteConfigPo;

        }

        /// <summary>
        /// 重置页码
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="qpm"></param>
        /// <returns></returns>
        public QuotePageModel ResetCurQuoteConfigPageNo(string groupId, string groupName, QuotePageModel qpm)
        {
            List<QuoteGroupConfigModel> qpmlist = qpm.Panels.ToList();
            int pageCount = LocalEnv.GrLayoutSetting.Row * LocalEnv.GrLayoutSetting.Col;
            int maxCount = qpmlist.Count;
            QuotePageModel newquotePo = new QuotePageModel();
            newquotePo.GroupId = groupId;
            newquotePo.GroupName = groupName;
            newquotePo.UserName = LocalEnv.UserInfo.UserName;
            // 初始化索引位置
            int currentIndex = 0;
            int pageno = 0;
            // 循环提取数据直至提取完成
            while (currentIndex < maxCount)
            {
                // 使用 LINQ 的 Take 方法提取当前批次的数据
                List<QuoteGroupConfigModel> batch = qpmlist.Skip(currentIndex).Take(pageCount).ToList();
                int i = 0;
                foreach (QuoteGroupConfigModel rm in batch)
                {
                    rm.Page = pageno;
                    rm.CustomSort = i;
                    i++;
                }
                currentIndex += pageCount;
                newquotePo.Panels.AddRange(batch);
                pageno++;

            }

            return newquotePo;
        }


        #endregion
        /// <summary>
        /// 保存最新更新的组债券信息
        /// </summary>
        /// <param name="qpm"></param>
        /// <param name="groupId"></param>
        public void SaveConfig(QuotePageModel data, string groupId)
        {
            try
            {
                List<QuotePageModel> allUserConfig = GetAllConfigByCondtion(string.Empty);
                if (allUserConfig != null && allUserConfig.Count > 0)
                {
                    QuotePageModel curdata = allUserConfig.Where(p => p.GroupId == groupId).FirstOrDefault();
                    if (curdata != null)
                    {
                        curdata.Panels.Clear();
                        curdata.GroupId = data.GroupId;
                        curdata.GroupName = data.GroupName;
                        curdata.Panels.AddRange(data.Panels.ToList());
                    }
                    else
                        allUserConfig.Add(data);
                }
                else
                {
                    allUserConfig.Add(data);
                }

                FileHelper.WriteFullText(dbFile, STC.Common.Utils.CommonUtils.SerializeObject(allUserConfig));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 删除组
        /// </summary>
        /// <param name="qpm"></param>
        /// <param name="groupId"></param>
        public void DelAndSaveConfig(string groupId)
        {
            try
            {
                List<QuotePageModel> allUserConfig = GetAllConfigByCondtion(string.Empty);
                if (allUserConfig != null && allUserConfig.Count > 0)
                {
                    QuotePageModel curdata = allUserConfig.Where(p => p.GroupId == groupId).FirstOrDefault();
                    allUserConfig.Remove(curdata);
                }

                if (DictQuotePage.ContainsKey(groupId))
                {
                    DictQuotePage.Remove(groupId);
                }

                FileHelper.WriteFullText(dbFile, STC.Common.Utils.CommonUtils.SerializeObject(allUserConfig));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 更新组名
        /// </summary>
        /// <param name="qpm"></param>
        /// <param name="groupId"></param>
        public void UpdateConfig(QuotePageModel data, string groupId)
        {
            try
            {
                List<QuotePageModel> allUserConfig = GetAllConfigByCondtion(string.Empty);
                if (allUserConfig != null && allUserConfig.Count > 0)
                {
                    QuotePageModel curdata = allUserConfig.Where(p => p.GroupId == groupId).FirstOrDefault();
                    if (curdata != null)
                    {
                        curdata.GroupName = data.GroupName;
                    }
                }

                FileHelper.WriteFullText(dbFile, STC.Common.Utils.CommonUtils.SerializeObject(allUserConfig));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
        /// <summary>
        /// 根据数据
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="securityIDsInfosList"></param>
        /// <returns></returns>
        public QuotePageModel ResetQuote(string groupId, string groupName, List<HomeGroupSecurityIDsInfo> securityIDsInfosList)
        {
            int pageCount = LocalEnv.GrLayoutSetting.Row * LocalEnv.GrLayoutSetting.Col;
            int maxCount = securityIDsInfosList.Count;
            int pageNum = maxCount % pageCount == 0 ? (maxCount / pageCount) : (maxCount / pageCount + 1);

            QuotePageModel quoteConfigPo = new QuotePageModel();
            quoteConfigPo.GroupId = groupId;
            quoteConfigPo.GroupName = groupName;
            quoteConfigPo.UserName = LocalEnv.UserInfo.UserName;
            
            if (pageNum > 0)
            {
                for (int j = 0; j < pageNum; j++)
                {
                    int index = j * pageCount;

                    for (int i = 0; i < pageCount; i++)
                    {
                        QuoteGroupConfigModel panelPo = new QuoteGroupConfigModel();
                        if (index < securityIDsInfosList.Count)
                        {
                            HomeGroupSecurityIDsInfo securityIDsInfo = securityIDsInfosList[index];
                            if (securityIDsInfo != null && !string.IsNullOrEmpty(securityIDsInfo.securityid))
                            {
                                panelPo.SecurityID = securityIDsInfo.securityid;
                                panelPo.SymbolName = securityIDsInfo.symbol;
                                panelPo.ParentId = securityIDsInfo.id;
                                panelPo.GroupId = groupId;
                            }
                        }

                        panelPo.Page = j;
                        panelPo.CustomSort = i;
                        panelPo.GroupId = groupId;
                        quoteConfigPo.Panels.Add(panelPo);

                        index++;
                    }
                }
            }
            else
            {
                
                for (int i = 0; i < pageCount; i++)
                {
                    QuoteGroupConfigModel panelPo = new QuoteGroupConfigModel();
                    panelPo.Page = 0;
                    panelPo.CustomSort = i;
                    panelPo.GroupId = groupId;
                    quoteConfigPo.Panels.Add(panelPo);
                }
            }

            return quoteConfigPo;
        }



        /// <summary>
        /// 查询用户的组债券列表
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<QuotePageModel> GetAllConfigByCondtion(string groupId = "")
        {
            string userName = LocalEnv.UserInfo.UserName;

            List<QuotePageModel> res = new List<QuotePageModel>();
            List<QuotePageModel> list = new List<QuotePageModel>();
            try
            {
                if (File.Exists(dbFile))
                {
                    string str = FileHelper.ReadFullText(dbFile);
                    if (JsonSplit.IsJson(str))
                    {
                        list = STC.Common.Utils.CommonUtils.DeserializeObject<List<QuotePageModel>>(str);
                        if (list.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(groupId))
                            {
                                QuotePageModel qpm = list.Where(p => p.UserName == userName && p.GroupId == groupId).FirstOrDefault();
                                if (qpm != null)
                                {
                                    res.Add(qpm);
                                }
                            }
                            else
                            {
                                List<QuotePageModel> qpm = list.Where(p => p.UserName == userName).ToList();
                                if (qpm.Count > 0)
                                {
                                    res.AddRange(qpm.ToList());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }


            return res;
        }


    }
}
