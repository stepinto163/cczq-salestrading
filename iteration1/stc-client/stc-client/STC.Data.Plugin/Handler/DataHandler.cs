using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.Utils.SqlLite;
//using STC.Data.Plugin.Sqlite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Data.Plugin.Handler
{
    /// <summary>
    /// 债券基础数据比较器
    /// </summary>
    public class BondComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            //string x_BondID = x.Substring(0, x.IndexOf('-'));
            //string y_BondID = y.Substring(0, y.IndexOf('-'));
            int resutl = x.Length.CompareTo(y.Length);
            if (resutl == 0)
            {
                return x.CompareTo(y);
            }
            else
            {
                return resutl;
            }
        }
    }

    public class DataHandler
    {
        private List<bool> isEndOfLoadFromDB = new List<bool>(new bool[] { false });

        private static BondComparer bondComparer = new BondComparer();
        private bool IsSearching = false;
        private int PageSize = 2000;
        private int maxCount = 0;
        private int pageNum = 0;

        //bond dic方式
        public readonly SortedDictionary<string, SimpleBondsInfoPo> bondDic = new SortedDictionary<string, SimpleBondsInfoPo>(bondComparer);
        //broker
        private readonly DataTable dt = new DataTable();
        //trader
        private readonly DataTable dtContact = new DataTable();
        //inst
        private readonly DataTable dtInst = new DataTable();
        //inst dic方式
        private readonly Dictionary<string, InstInfoPo> instDic = new Dictionary<string, InstInfoPo>();

        private static DataHandler brokerHandler = new DataHandler();
        public static DataHandler Instance { get { return brokerHandler; } }
        
        public object updlock = new object();
        private DataHandler()
        {
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    //从本地加载
                    InitBondData();
                    //如果没有数据
                    if (bondDic.Count == 0)
                    {
                        SyncBond();

                    }

                    //InitData();

                    //InitInstData();

                    //InitContactData();

                    while (LocalEnv.ConfInfo.CanGoodsQuickSearch)
                    {
                        SearchBonds();
                        Thread.Sleep(10 * 60 * 1000);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
                finally
                {
                    Task.Factory.StartNew(() =>
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    });
                }
            });
        }

        #region bond 债卷


        public List<bool> GetEndOfLoadFromDB()
        {
            return isEndOfLoadFromDB;
        }

        /// <summary>
        /// 从本地sqlite数据库加载缓存的债券基本信息
        /// </summary>
        private void InitBondData()
        {
            try
            {
                var list = BondsInfoSqliteManager.Instance.GetAllSimpleBonds();
                if (list != null && list.Count > 0)
                {
                    foreach (var i in list)
                    {
                        CacheSimpleBondsInfo(i, "1");
                    }

                    isEndOfLoadFromDB[0] = true;
                    //CommonUtils.InitDictBondsInfoPoAndHashSetBondsInfoPo();
                    BaseHandler.Instance.SetLoadDataStatus(true);
                }

                list = null;
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }
        }
        
        /// <summary>
        /// 用户是否支持goodstype债券类型
        /// </summary>
        /// <param name="goodstype"></param>
        /// <returns></returns>
        private bool IsContain(string goodstype)
        {
            List<string> termidsList = LocalEnv.UserInfo.Termids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            if (termidsList != null && termidsList.Count > 0)
                return termidsList.Contains(goodstype);

            return false;
        }

        /// <summary>
        /// 债券的key
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private string GetBondKey(BondsInfoPo info)
        {
            return string.Format("{0}-{1}-{2}-{3}", info.Bond_ID, info.Short_Name, info.pinYin, info.pinyin_full);
        }

        //private void CacheMaxTime(string bondlistinfo_modify_date, string bond_modify_date, string bondmaster_modify_date)
        //{
        //    if (bondlistinfo_modify_date.CompareTo(BondListInfoModifyDate_Max) > 0 )
        //    {
        //        BondListInfoModifyDate_Max = bondlistinfo_modify_date;
        //    }
        //    if (bond_modify_date.CompareTo(BondModifyDate_Max) > 0)
        //    {
        //        BondModifyDate_Max = bond_modify_date;
        //    }
        //    if (bondmaster_modify_date.CompareTo(BondMasterModifyDate_Max) > 0)
        //    {
        //        BondMasterModifyDate_Max = bondmaster_modify_date;
        //    }

        //    STCRocksDb4BasicDataHandler.Instance.Put(STCRocksDb4BasicDataHandler.DbBaseMaxTime, nameof(BondListInfoModifyDate_Max), BondListInfoModifyDate_Max);
        //    STCRocksDb4BasicDataHandler.Instance.Put(STCRocksDb4BasicDataHandler.DbBaseMaxTime, nameof(BondModifyDate_Max), BondModifyDate_Max);
        //    STCRocksDb4BasicDataHandler.Instance.Put(STCRocksDb4BasicDataHandler.DbBaseMaxTime, nameof(BondMasterModifyDate_Max), BondMasterModifyDate_Max);
        //}

        /// <summary>
        /// 获取所有债券基础数据
        /// </summary>
        /// <returns></returns>
        public SortedDictionary<string, SimpleBondsInfoPo> GetAllBondDic()
        {
            return bondDic;
        }

        public void CacheSimpleBondsInfo(SimpleBondsInfoPo i, string delflag)
        {
            lock (bondDic)
            {
                // 计算剩余期限
                i.RemaingingTime = STCHelper.SetShowTenor(i);

                // 修改MarketIndicator
                if (string.IsNullOrEmpty(i.MarketIndicator))
                    i.MarketIndicator = "4";

                bondDic[i.Bond_ID] = i;
            }
        }

        ///// <summary>
        ///// 把债券数据加入到缓存中
        ///// </summary>
        ///// <param name="i"></param>
        ///// <returns></returns>
        //public bool AddBondRow(BondsInfoPo i)
        //{
        //    bool isExit = false;
        //    //增加按登录账号加载对应类型数据到内存
        //    //if (string.IsNullOrEmpty(i.goodstype) || !IsContain(i.goodstype))
        //    //    return false;

        //    lock (bondDic)
        //    {
        //        if (string.IsNullOrEmpty(i.MarketIndicator) || i.MarketIndicator.Equals("4"))
        //        {
        //            if (!string.IsNullOrEmpty(i.delflag) && i.delflag.Equals("0"))// && !string.IsNullOrEmpty(i.RemaingingTime) && !i.RemaingingTime.Equals("0"))
        //            {
        //                i.RemaingingTime = STCHelper.SetShowTenor(i);

        //                //if (!string.IsNullOrEmpty(i.RemaingingTime))
        //                //{
        //                //    string key = GetBondKey(i);
        //                //    if (bondDic.ContainsKey(key))
        //                //        bondDic[key] = i;
        //                //    else
        //                //        bondDic.Add(key, i);
        //                //}
        //            }

        //            if (string.IsNullOrEmpty(i.MarketIndicator))
        //                i.MarketIndicator = "4";

        //            i.searchKey = GetBondKey(i);

        //            //try
        //            //{
        //            //    List<string> signlist = new List<string>();
        //            //    List<Dictionary<string, string>> signdiclist = new List<Dictionary<string, string>>();
        //            //    ParserBondSigns(i.bondSigns, ref signdiclist, ref signlist);
        //            //    i.SignDicList = signdiclist;
        //            //    i.SignList = signlist;
        //            //}
        //            //catch (Exception ex)
        //            //{
        //            //    LogHelper.Error($"ParserBondSigns Error:{i.Bond_ID} {i.bondSigns} ErrorMessage:{ex.Message}");
        //            //}

        //            if (bondDic.ContainsKey(i.Bond_ID))
        //            {
        //                bondDic[i.Bond_ID].Short_Name = i.Short_Name;
        //                bondDic[i.Bond_ID].pinYin = i.pinYin;
        //                bondDic[i.Bond_ID].pinyin_full = i.pinyin_full;
        //                bondDic[i.Bond_ID].flag = i.flag;
        //                bondDic[i.Bond_ID].symbolType = i.symbolType;
        //            }
        //            else
        //            {
        //                SimpleBondsInfoPo simpleB = new SimpleBondsInfoPo();
        //                simpleB.Bond_ID = i.Bond_ID;
        //                simpleB.Short_Name = i.Short_Name;
        //                simpleB.pinYin = i.pinYin;
        //                simpleB.pinyin_full = i.pinyin_full;
        //                simpleB.flag = i.flag;
        //                simpleB.Security_Type_Id = i.Security_Type_Id;
        //                simpleB.listed_Market = i.listed_Market;
        //                simpleB.MarketIndicator = i.MarketIndicator;
        //                simpleB.symbolType = i.symbolType;
        //                bondDic[i.Bond_ID] = simpleB;
        //            }

        //            //Common.Data.STCRocksDbHandler.Instance.UpdBondsInfoPo(i);
        //            //i.searchKey = GetBondKey(i);

        //            //bondDic[i.Bond_ID] = i;
        //        }
        //        else
        //        {
        //            //string key = GetBondKey(i);
        //            //if (bondDic.ContainsKey(key))
        //            //    bondDic[key] = i;
        //            //else
        //            //    bondDic.Add(key, i);
        //            bondDic[i.Bond_ID].Short_Name = i.Short_Name;
        //            bondDic[i.Bond_ID].pinYin = i.pinYin;
        //            bondDic[i.Bond_ID].pinyin_full = i.pinyin_full;
        //            bondDic[i.Bond_ID].flag = i.flag;
        //            bondDic[i.Bond_ID].symbolType = i.symbolType;
        //            //Common.Data.STCRocksDbHandler.Instance.UpdBondsInfoPo(i);
        //        }

        //    }

        //    return isExit;
        //}

        /// <summary>
        /// 搜索债券
        /// </summary>
        private void SearchBonds()
        {
            if (IsSearching)
                return;
            IsSearching = true;

            try
            {
                SearchBonds1();
            }
            catch (Exception ex)
            {
                LogHelper.Error("Search Bonds failed!", ex);
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        private void ClearDic()
        {
            lock(bondDic)
            {
                bondDic.Clear();
            }
        }


        /// <summary>
        /// 每日同步bond数据
        /// </summary>
        public void SyncBond()
        {
            BondsSearchRequestPo request = new BondsSearchRequestPo();
            request.PageSize = 1;
            request.PageNum = 1;

            var reply = BaseHandler.Instance.SearchBonds(request);//ClientApi.Instance.SearchBonds(request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.Total;
                LogHelper.Info("Bond maxCount:" + maxCount + ", bondDic.Rows:" + bondDic.Count);
                if (maxCount > 0 && maxCount != bondDic.Count)
                {
                    LogHelper.Info("Clear Bond dictionary.");
                    //ClearDataTable(dtBond);
                    //ClearDic();
                    List<string> keyList = bondDic.Keys.ToList();


                    int pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                    ThreadPool.SetMaxThreads(pageNum * 2, pageNum * 2);

                    Task[] lstTask = new Task[pageNum];
                    List<BondsInfoPo> datas = new List<BondsInfoPo>();
                    for (int i = 0; i < pageNum; i++)
                    {
                        BondsSearchRequestPo request2 = new BondsSearchRequestPo();
                        request2.PageNum = i + 1;
                        request2.PageSize = PageSize;

                        lstTask[i] = new Task(() =>
                        {
                            var reply1 = BaseHandler.Instance.SearchBonds(request2);//ClientApi.Instance.SearchBonds(request);
                            if (reply1 != null && reply1.datas != null && reply1.datas.Count > 0)
                            {
                                datas.AddRange(reply1.datas);
                                foreach (BondsInfoPo a in reply1.datas)
                                {
                                    keyList.Remove(a.Bond_ID);
                                    SimpleBondsInfoPo simpleBondsInfoPo = null;
                                    if (bondDic.ContainsKey(a.Bond_ID))
                                    {
                                        simpleBondsInfoPo = bondDic[a.Bond_ID];
                                    }
                                    else
                                    {
                                        simpleBondsInfoPo = new SimpleBondsInfoPo();
                                        simpleBondsInfoPo.Bond_ID = a.Bond_ID;
                                        simpleBondsInfoPo.Short_Name = a.Short_Name;
                                        simpleBondsInfoPo.pinYin = a.pinYin;
                                        simpleBondsInfoPo.pinyin_full = a.pinyin_full;
                                        simpleBondsInfoPo.flag = a.flag;
                                        simpleBondsInfoPo.Security_Type_Id = a.Security_Type_Id;
                                        simpleBondsInfoPo.listed_Market = a.listed_Market;
                                        simpleBondsInfoPo.MarketIndicator = a.MarketIndicator;
                                        simpleBondsInfoPo.symbolType = a.symbolType;
                                        simpleBondsInfoPo.Option_Date = a.Option_Date;
                                        simpleBondsInfoPo.Option_Type = a.Option_Type;
                                        simpleBondsInfoPo.Maturity_Date = a.Maturity_Date;
                                        simpleBondsInfoPo.Full_Name_EN = a.Full_Name_EN;
                                    }

                                    CacheSimpleBondsInfo(simpleBondsInfoPo, a.delflag);
                                    a.RemaingingTime = simpleBondsInfoPo.RemaingingTime;
                                    a.MarketIndicator = simpleBondsInfoPo.MarketIndicator;
                                }
                                //SqliteManager.Instance.Insert(reply1.datas);
                            }
                        });

                        lstTask[i].Start();
                    }

                    Task.WaitAll(lstTask);

                    foreach (string key in keyList)
                    {
                        // 去除不要的数据
                        bondDic.Remove(key);
                    }

                    isEndOfLoadFromDB[0] = true;
                    //CommonUtils.InitDictBondsInfoPoAndHashSetBondsInfoPo();
                    BaseHandler.Instance.SetLoadDataStatus(true);
                    
                    WriteLocalDb(datas);
                    datas.Clear();
                }
            }
            else
            {
                if (reply != null)
                {
                    LogHelper.Error($"Sync bond exception.{reply.Msg}");
                }
                else
                {
                    LogHelper.Error($"SyncBond 异常，请重新启动系统！");
                }
            }
        }

        /// <summary>
        /// 写入本地数据库
        /// </summary>
        public void WriteLocalDb(List<BondsInfoPo> datas)
        {
            Task.Run(() => 
            {
                try
                {
                    LogHelper.Info("Clear Bond db.");
                    BondsInfoSqliteManager.Instance.DeleteBond();
                    LogHelper.Info("Start sync bond: reply count:" + maxCount + ", db count:" + bondDic.Count);
                    BondsInfoSqliteManager.Instance.Insert(datas);
                    datas.Clear();
                    datas =  null;
                    LogHelper.Info("Sync Bond db succeed!");
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("Sync bond exception.");
                    LogHelper.Error(ex);
                }
            });
        }

        private void SearchBonds1()
        {
            BondsSearchRequestPo request = new BondsSearchRequestPo();
            request.PageSize = 1;
            request.PageNum = 1;

            var pre = BondsInfoSqliteManager.Instance.GetMaxDate();
            if (pre != null)
            {
                request.BondListInfoModifyDate = pre.bondlistinfo_modify_date;
                request.BondModifyDate = pre.bond_modify_date;
                request.BondMasterModifyDate = pre.bondmaster_modify_date;
            }

            var reply = BaseHandler.Instance.SearchBonds(request);//ClientApi.Instance.SearchBonds(request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.Total;

                if (reply.IsAll != null && reply.IsAll.HasValue && reply.IsAll.Value == 0)
                {
                    LogHelper.Debug("Start get all bond data.");
                    SyncBond();

                    return;
                }


                int pageNum = 0;
                if (maxCount > 0)
                {
                    pageNum = maxCount % PageSize == 0 ? (maxCount / PageSize) : (maxCount / PageSize + 1);
                }
                else
                    pageNum = 0;

                for (int i = 0; i < pageNum; i++)
                {
                    request.PageNum = i + 1;
                    request.PageSize = PageSize;

                    var reply1 = BaseHandler.Instance.SearchBonds(request);
                    if (reply1 != null && reply1.datas != null && reply1.datas.Count > 0)
                    {
                        BondsInfoSqliteManager.Instance.Insert(reply1.datas);

                        foreach (var a in reply1.datas)
                        {
                            

                            SimpleBondsInfoPo simpleBondsInfoPo = null;
                            if (bondDic.ContainsKey(a.Bond_ID))
                            {
                                simpleBondsInfoPo = bondDic[a.Bond_ID];
                            }
                            else
                            {
                                simpleBondsInfoPo = new SimpleBondsInfoPo();
                                simpleBondsInfoPo.Bond_ID = a.Bond_ID;
                                simpleBondsInfoPo.Short_Name = a.Short_Name;
                                simpleBondsInfoPo.pinYin = a.pinYin;
                                simpleBondsInfoPo.pinyin_full = a.pinyin_full;
                                simpleBondsInfoPo.flag = a.flag;
                                simpleBondsInfoPo.Security_Type_Id = a.Security_Type_Id;
                                simpleBondsInfoPo.listed_Market = a.listed_Market;
                                simpleBondsInfoPo.MarketIndicator = a.MarketIndicator;
                                simpleBondsInfoPo.symbolType = a.symbolType;
                                simpleBondsInfoPo.Option_Date = a.Option_Date;
                                simpleBondsInfoPo.Option_Type = a.Option_Type;
                                simpleBondsInfoPo.Maturity_Date = a.Maturity_Date;
                                simpleBondsInfoPo.Full_Name_EN = a.Full_Name_EN;
                            }

                            CacheSimpleBondsInfo(simpleBondsInfoPo, a.delflag);
                            a.RemaingingTime = simpleBondsInfoPo.RemaingingTime;
                            a.MarketIndicator = simpleBondsInfoPo.MarketIndicator;

                        }
                    }
                }


                isEndOfLoadFromDB[0] = true;
                //CommonUtils.InitDictBondsInfoPoAndHashSetBondsInfoPo();
                BaseHandler.Instance.SetLoadDataStatus(true);
            }
            else
            {
                if (reply != null)
                {
                    LogHelper.Error($"Sync bond exception.{reply.Msg}");
                }
            }
        }

        

        #endregion


        #region Inst 机构

        /// <summary>
        /// 获取所有机构信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, InstInfoPo> GetAllInstInfo()
        {
            return instDic;
        }

        private bool AddInstRow(InstInfoPo i)
        {
            bool result = false;
            lock (instDic)
            {
                if (!instDic.ContainsKey(i.PARTY_CODE6))
                    instDic.Add(i.PARTY_CODE6, i);
                else
                    instDic[i.PARTY_CODE6] = i;
            }
            return result;
        }



        #endregion

        #region function
        
        public void ParserBondSigns(string bondSigns, ref List<Dictionary<string, string>> SignDicList,ref List<string> SignList)
        {
            if (!string.IsNullOrEmpty(bondSigns))
            {
                string[] arr = bondSigns.Split(Consts.COMMA_CHAR);

                int i = 0;
                foreach (string sign in arr)
                {
                    SignList.Add(sign);
                    if (i == 0)
                    {
                        SignDicList.Add(ParseSign("sign", sign.ConvertToLong()));
                    }
                    else if (i == 1)
                    {
                        SignDicList.Add(ParseSign("sign1", sign.ConvertToLong()));
                    }
                    else if (i == 2)
                    {
                        SignDicList.Add(ParseSign("sign2", sign.ConvertToLong()));
                    }
                    else if (i == 3)
                    {
                        SignDicList.Add(ParseSign("sign3", sign.ConvertToLong()));
                    }

                    i++;
                }
            }
        }

        private Dictionary<string, string> ParseSign(string signField, long sign)
        {
            Dictionary<string, Common.Model.Table.QuoteTag> quoteTagDic = BaseHandler.Instance.GetQuoteTagDic();
            string a = STC.Common.Utils.CommonUtils.ToBinary(sign);
            if (!string.IsNullOrEmpty(a))
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (KeyValuePair<string, Common.Model.Table.QuoteTag> pair in quoteTagDic)
                {
                    if (pair.Value.signField == signField)
                    {
                        dic[pair.Value.tagType] = a.Substring(pair.Value.tagTypeStartIndex, pair.Value.tagTypeLength);
                    }
                }
                return dic;
            }
            return null;
        }

        #endregion
    }
}
