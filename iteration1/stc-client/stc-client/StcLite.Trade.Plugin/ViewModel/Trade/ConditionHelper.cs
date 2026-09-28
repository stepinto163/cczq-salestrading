using BDB.Trade.Plugin.Handler;
using STC.Common.Utils;
using STC.Common.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class ConditionHelper
    {
        public static bool ContainSettle(Dictionary<string, IList> settleDic, string content)
        {
            bool result = false;
            if (settleDic != null && settleDic.ContainsKey(CommonConstants.SETTLE))
            {
                StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();
                IList settlTypeList = settleDic[CommonConstants.SETTLE];


                if (settlTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem settlType in settlTypeList)
                    {
                        string settltStr = settlType.Content.ToString();
                        if (settlType.Content.ToString() == content)
                        {
                            result = true;
                            break;
                        }
                    }

                }
                else
                {
                    result = true;
                }

            }
            else
            {
                result = true;
            }

            return result;
        }


        public static Dictionary<string, IList> GetDicBySettle(string settle)
        {
            Dictionary<string, IList> result = new Dictionary<string, IList>();

            List<ListBoxItem> list = new List<ListBoxItem>();
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = settle;

            list.Add(listBoxItem);

            result.Add(CommonConstants.SETTLE, list);

            return result;
        }

        public static bool GetSettleType(Dictionary<string, IList> settleDic, string settle)
        {
            bool result = false;

            if (settleDic != null && settleDic.ContainsKey(CommonConstants.SETTLE) && !string.IsNullOrEmpty(settle))
            {
                IList settlTypeList = settleDic[CommonConstants.SETTLE];

                if (settlTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem settlType in settlTypeList)
                    {
                        string settltStr = settlType.Content.ToString();
                        if ((settlType.Content.ToString() == CommonConstants.ALL_PARTY || settltStr == CommonConstants.ALL) && (settle == CommonConstants.ALL_PARTY || settle == CommonConstants.ALL))
                        {
                            result = true;

                            break;
                        }
                        else if ((settlType.Content.ToString() == CommonConstants.NOW) && (settle == CommonConstants.ALL_PARTY || settle == CommonConstants.ALL))
                        {
                            result = true;

                            break;
                        }
                        else if ((settlType.Content.ToString() == "+1" || settltStr == CommonConstants.T2) && (settle == "+1" || settle == CommonConstants.T2))
                        {
                            result = true;

                            break;
                        }
                        else if ((settlType.Content.ToString() == "+0" || settltStr == CommonConstants.T1) && (settle == "+0" || settle == CommonConstants.T1))
                        {
                            result = true;

                            break;
                        }
                        else if ((settlType.Content.ToString() == "远" || settltStr == CommonConstants.FWD) && (settle == "远" || settle == CommonConstants.FWD))
                        {
                            result = true;

                            break;
                        }
                    }

                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        public static string GetNewPriceViewCondition(Dictionary<string, IList> settleDic, Dictionary<string, IList> _currentFilterDic)
        {
            string newFilter = string.Empty;
            if (settleDic != null && settleDic.ContainsKey(CommonConstants.SETTLE))
            {
                StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();
                IList settlTypeList = settleDic[CommonConstants.SETTLE];
                //List<ListBoxItem> list = new List<ListBoxItem>();
                //ListBoxItem listBoxItem = new ListBoxItem();
                //if (settlTypeList.Count == 5)
                //{
                //    listBoxItem.Content = CommonConstants.ALL_PARTY;
                //}
                //else if (settlTypeList.Count == 3)
                //{
                //    listBoxItem.Content = CommonConstants.T2_EN;
                //}
                //else if (settlTypeList.Count == 1)
                //{
                //    listBoxItem.Content = (settlTypeList[0].ToString() == CommonConstants.T1_ONE) ? CommonConstants.T1_EN : CommonConstants.FWD;
                //}
                //list.Add(listBoxItem);
                bestCacheSettleFitlerBuf = ConditionHelper.GetSettleCondition(settlTypeList);

                newFilter = bestCacheSettleFitlerBuf.ToString();
            }

            //全局Broker条件
            if (_currentFilterDic != null && _currentFilterDic.ContainsKey(CommonConstants.BROKER))
            {
                IList mainVenueList = _currentFilterDic[CommonConstants.BROKER];
                StringBuilder sbBroker = ConditionHelper.GetBrokerCondition(mainVenueList);
                if (sbBroker.Length > 0)
                {
                    newFilter = !string.IsNullOrEmpty(newFilter) ? string.Format("{0} and ({1})", newFilter, sbBroker) : sbBroker.ToString();
                }
            }

            return newFilter;
        }


        public static string GetPriceViewCondition(Dictionary<string, List<string>> _filterDic, Dictionary<string, IList> _currentFilterDic)
        {
            string newFilter = string.Empty;
            if (_filterDic.ContainsKey(CommonConstants.SETTLE))
            {
                StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();
                IList settlTypeList = _filterDic[CommonConstants.SETTLE];
                List<ListBoxItem> list = new List<ListBoxItem>();
                ListBoxItem listBoxItem = new ListBoxItem();
                if (settlTypeList.Count == 5)
                {
                    listBoxItem.Content = CommonConstants.ALL_PARTY;
                }
                else if (settlTypeList.Count == 3)
                {
                    listBoxItem.Content = CommonConstants.T2_EN;
                }
                else if (settlTypeList.Count == 1)
                {
                    listBoxItem.Content = (settlTypeList[0].ToString() == CommonConstants.T1_ONE) ? CommonConstants.T1_EN : CommonConstants.FWD;
                }
                list.Add(listBoxItem);
                bestCacheSettleFitlerBuf = ConditionHelper.GetSettleCondition(list);

                newFilter = bestCacheSettleFitlerBuf.ToString();
            }

            //全局Broker条件
            if (_currentFilterDic.ContainsKey(CommonConstants.BROKER))
            {
                IList mainVenueList = _currentFilterDic[CommonConstants.BROKER];
                StringBuilder sbBroker = ConditionHelper.GetBrokerCondition(mainVenueList);
                if (sbBroker.Length > 0)
                {
                    newFilter = !string.IsNullOrEmpty(newFilter) ? string.Format("{0} and ({1})", newFilter, sbBroker) : sbBroker.ToString();
                }
            }

            return newFilter;
        }


        public static List<string> GetMainSettle(Dictionary<string, IList> filterDic)
        {
            List<string> settleList = new List<string>();

            if (filterDic != null && filterDic.ContainsKey(CommonConstants.SETTLE))
            {
                IList list = filterDic[CommonConstants.SETTLE];

                foreach (ListBoxItem item in list)
                {
                    settleList.Add(item.Content.ToString());
                }
            }

            return settleList;
        }

        public static bool IsContainSettle(string settle, IList list)
        {
            bool result = false;
            foreach (ListBoxItem item in list)
            {
                if (item.Content.ToString() == settle)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public static List<ListBoxItem> GetMainSettle(List<string> settleList)
        {
            List<ListBoxItem> result = new List<ListBoxItem>();

            foreach (string str in settleList)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = str;

                result.Add(listBoxItem);
            }
            

            return result;
        }

        public static StringBuilder GetPriceTableViewCondition(Dictionary<string, IList> filterDic)
        {
            StringBuilder sbTable = new StringBuilder();

            if (filterDic.ContainsKey(CommonConstants.BOND_TYPE))
            {
                StringBuilder inFilterSB = new StringBuilder();
                IList bondList = filterDic[CommonConstants.BOND_TYPE];
                foreach (System.Windows.Controls.ListBoxItem lbi in bondList)
                {
                    if (lbi.Content.ToString() == CommonConstants.ALL)
                    {
                        inFilterSB.Clear();

                        break;
                    }

                    if (lbi.Content.ToString() == "国债")
                    {
                        AddListFilter(inFilterSB, "'A'");
                    }
                    else if (lbi.Content.ToString() == "央票")
                    {
                        AddListFilter(inFilterSB, "'B'");
                    }
                    else if (lbi.Content.ToString() == "金融债")
                    {
                        AddListFilter(inFilterSB, "'C'");
                    }
                    else if (lbi.Content.ToString() == "地方债")
                    {
                        AddListFilter(inFilterSB, "'D'");
                    }
                }

                if (inFilterSB.Length > 0)
                {
                    if (sbTable.Length > 0)
                    {
                        sbTable.Append(" or ");
                    }
                    sbTable.Append($"{PriceTableViewModel.BOND_CATEGORY_FIELD} in ({inFilterSB})");
                }
            }

            if (filterDic.ContainsKey(CommonConstants.REMAINING_TIME))
            {
                StringBuilder filterRemainingTime = new StringBuilder();
                IList remaingTimeList = filterDic[CommonConstants.REMAINING_TIME];
                foreach (System.Windows.Controls.ListBoxItem lbi in remaingTimeList)
                {
                    if (lbi.Content.ToString() == CommonConstants.ALL)
                    {
                        filterRemainingTime.Clear();

                        break;
                    }

                    if (filterRemainingTime.Length > 0)
                        filterRemainingTime.Append(" or ");

                    if (lbi.Content.ToString() == "0~1Y")
                    {
                        filterRemainingTime.Append($"({PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} >= 0 and {PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.Content.ToString() == "1~3Y")
                    {
                        filterRemainingTime.Append($"({PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.Content.ToString() == "3~5Y")
                    {
                        filterRemainingTime.Append($"({PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.Content.ToString() == "5~7Y")
                    {
                        filterRemainingTime.Append($"({PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.Content.ToString() == "7~10Y")
                    {
                        filterRemainingTime.Append($"({PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.Content.ToString() == ">10Y")
                    {
                        filterRemainingTime.Append($"({PriceTableViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
                    }
                }

                if (filterRemainingTime.Length > 0)
                {
                    if (sbTable.Length > 0)
                    {
                        sbTable.Append(" and ");
                    }
                    sbTable.Append($"({filterRemainingTime})");
                }
            }

            if (filterDic.ContainsKey(CommonConstants.SECURITYID))
            {
                StringBuilder securityBuf = new StringBuilder();
                IList securityList = filterDic[CommonConstants.SECURITYID];
                if (securityList.Count > 0)
                {
                    securityBuf.Append($"{PriceTableViewModel.SECURITY_ID_FIELD} = '{securityList[0]}'");
                }

                if (securityBuf.Length > 0)
                {
                    if (sbTable.Length > 0)
                    {
                        sbTable.Append(" and ");
                    }
                    sbTable.Append($"({securityBuf})");
                }
            }

            return sbTable;
        }

        private static void AddListFilter(StringBuilder filterSB, string listFilterItems)
        {
            if (filterSB.Length > 0)
            {
                filterSB.Append(",");
            }
            filterSB.Append(listFilterItems);
        }

        public static StringBuilder GetSettleCondition(IList settlTypeList)
        {
            StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();

            if (settlTypeList.Count > 0)
            {
                foreach (System.Windows.Controls.ListBoxItem settlType in settlTypeList)
                {
                    if (bestCacheSettleFitlerBuf.Length > 0)
                        bestCacheSettleFitlerBuf.Append(" or ");

                    string settltStr = settlType.Content.ToString();
                    if (settlType.Content.ToString() == CommonConstants.ALL_PARTY || settltStr == CommonConstants.ALL)
                    {
                        bestCacheSettleFitlerBuf.Clear();

                        break;
                    }
                    else if (settlType.Content.ToString() == CommonConstants.NOW)
                    {
                        bestCacheSettleFitlerBuf.Append($"(({CTPriceControler.SETTL_TYPE_FIELD} = '1' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,1,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,1' or {CTPriceControler.SETTL_TYPE_FIELD} like '1,%') or ({CTPriceControler.SETTL_TYPE_FIELD} = '2' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,2,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '2,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,2') or ({CTPriceControler.SETTL_TYPE_FIELD} = '3' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,3,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '3,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,3'))");

                    }
                    else if (settlType.Content.ToString() == "+1" || settltStr == CommonConstants.T2)
                    {
                        bestCacheSettleFitlerBuf.Append($"(({CTPriceControler.SETTL_TYPE_FIELD} = '2' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,2,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '2,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,2') or ({CTPriceControler.SETTL_TYPE_FIELD} = '3' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,3,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '3,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,3'))");

                    }
                    else if (settlType.Content.ToString() == "+0" || settltStr == CommonConstants.T1)
                    {
                        bestCacheSettleFitlerBuf.Append($"({CTPriceControler.SETTL_TYPE_FIELD} = '1' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,1,%' or {CTPriceControler.SETTL_TYPE_FIELD} like '%,1' or {CTPriceControler.SETTL_TYPE_FIELD} like '1,%')");
                    }
                    else 
                    {
                        bestCacheSettleFitlerBuf.Append($"{CTPriceControler.SETTL_TYPE_YUAN_FIELD} <> ''");
                        //bestCacheSettleFitlerBuf.Append($"{CTPriceControler.SETTL_TYPE_FIELD} <> '1' and {CTPriceControler.SETTL_TYPE_FIELD} <> '2' and {CTPriceControler.SETTL_TYPE_FIELD} <> '3' and {CTPriceControler.SETTL_TYPE_FIELD} <> '2,3'");
                    }
                }

            }

            return bestCacheSettleFitlerBuf;
        }

        public static StringBuilder GetBrokerCondition(IList venuesList)
        {
            StringBuilder inFilterSB_venues = new StringBuilder();
            if (venuesList.Count > 0)
            {
                foreach (System.Windows.Controls.ListBoxItem venues in venuesList)
                {
                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(" or ");
                    }

                    if (venues.Content.ToString() == CommonConstants.AllBroker)
                    {
                        inFilterSB_venues.Clear();

                        //inFilterSB_venues.Append($"({CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CNEX_EN}' or {CTPriceControler.SOURCE_FIELD} = '{CommonConstants.TPSC_EN}' or {CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CCTB_EN}' or {CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CTTJ_EN}' or {CTPriceControler.SOURCE_FIELD} = '{CommonConstants.PTCN_EN}')");
                        break;
                    }

                    switch (venues.Content.ToString())
                    {
                        case CommonConstants.CNEX:
                            inFilterSB_venues.Append($"{CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CNEX_EN}'");
                            break;
                        case CommonConstants.TPSC:
                            inFilterSB_venues.Append($"{CTPriceControler.SOURCE_FIELD} = '{CommonConstants.TPSC_EN}'");
                            break;
                        case CommonConstants.CCTB:
                            inFilterSB_venues.Append($"{CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CCTB_EN}'");
                            break;
                        case CommonConstants.CTTJ:
                            inFilterSB_venues.Append($"{CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CTTJ_EN}'");
                            break;
                        case CommonConstants.PTCN:
                            inFilterSB_venues.Append($"{CTPriceControler.SOURCE_FIELD} = '{CommonConstants.PTCN_EN}'");
                            break;
                        case CommonConstants.UEDA:
                            inFilterSB_venues.Append($"{CTPriceControler.SOURCE_FIELD} = '{CommonConstants.UEDA_EN}'");
                            break;
                    }

                }
            }

            return inFilterSB_venues;
        }

        public static bool CheckSettleConditionBySelf(Dictionary<string, IList> filterDic, string settle)
        {
            bool result = false;

            if (filterDic == null || filterDic.Count == 0)
            {
                result = true;
            }
            else
            {
                List<string> settleList = GetMainSettle(filterDic);

                foreach (string str in settleList)
                {
                    if (str == settle)
                    {
                        result = true;

                        break;
                    }
                }
            }

            return result;
        }

        public static bool CheckSettleCondition(Dictionary<string, IList> filterDic, string settles)
        {
            bool result = false;

            if (filterDic == null || filterDic.Count == 0)
            {
                result = true;
            }
            else
            {
                List<string> settleList = GetMainSettle(filterDic);

                foreach (string str in settleList)
                {
                    string[] settleArr = settles.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string settle in settleArr)
                    {
                        if ((str == CommonConstants.ALL_PARTY)
                            || (str == CommonConstants.T1_EN && settle == CommonConstants.T1_ONE)
                            || (str == CommonConstants.T2_EN && CommonConstants.T2_T3.Contains(settle))
                            || (str == CommonConstants.FWD && !(settle == CommonConstants.T1_ONE || CommonConstants.T2_T3.Contains(settle))))
                        {
                            result = true;

                            break;
                        }
                    }
                }
            }

            return result;
        }

        public static StringBuilder GetCardBrokerCondition(int selectIndex)
        {
            StringBuilder sbBroker = new StringBuilder();

            if (selectIndex == 0)
            {
                //sbBroker.Append($"({CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CNEX_EN}' or {CTPriceControler.SOURCE_FIELD} = '{CommonConstants.TPSC_EN}' or {CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CCTB_EN}' or {CTPriceControler.SOURCE_FIELD} = '{CommonConstants.CTTJ_EN}' or {CTPriceControler.SOURCE_FIELD} = '{CommonConstants.PTCN_EN}' or {CTPriceControler.SOURCE_FIELD} = '')");

            }
            else if (selectIndex > 0)
            {
                string brokerEN = PermisionHandler.Instance.GetBrokerByIndex(selectIndex);
                sbBroker.Append($"{CTPriceControler.SOURCE_FIELD} = '{brokerEN}'");

            }

            return sbBroker;
        }

        public static string GetCardOrderCondition(int selectIndex, Dictionary<string, IList> settleDic)
        {
            string result = string.Empty;

            StringBuilder sbBroker = GetCardBrokerCondition(selectIndex);

            string newFilter = string.Empty;
            if (settleDic != null && settleDic.ContainsKey(CommonConstants.SETTLE))
            {
                StringBuilder bestCacheSettleFitlerBuf = new StringBuilder();
                IList settlTypeList = settleDic[CommonConstants.SETTLE];

                bestCacheSettleFitlerBuf = ConditionHelper.GetSettleCondition(settlTypeList);

                newFilter = bestCacheSettleFitlerBuf.ToString();
            }

            if (sbBroker.Length > 0 && !string.IsNullOrEmpty(newFilter))
            {
                result = string.Format("{0} and ({1})", newFilter, sbBroker.ToString());
            }
            else
            {
                if (sbBroker.Length == 0)
                    result = newFilter;
                else
                    result = sbBroker.ToString();
            }

            return result;
        }

        public static object SettleListToDic(List<string> settleList)
        {
            Dictionary<string, IList> dic = new Dictionary<string, IList>();
            List<ListBoxItem> listBoxItems = GetMainSettle(settleList);

            dic.Add(CommonConstants.SETTLE, listBoxItems);

            return dic;
        }
        
    }
}
