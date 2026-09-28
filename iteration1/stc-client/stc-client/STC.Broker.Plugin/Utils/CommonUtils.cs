using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.IDCEnum;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.Utils
{
    class CommonUtils
    {

        // <summary>
        /// 把基础剩余时间转换成时间段, 转成‘年’ 
        /// 如：2Y+ 5M + 6D -> 2 + 5 / 12 + 6 / 365 
        /// </summary>
        /// <param name="remainingTime">基础剩余时间</param>
        /// <returns>筛选时间段</returns>
        public static decimal ConvertRemainingTime(string remainingTime)
        {
            string remainingTimeLevel = string.Empty;
            decimal convertTime = 0;
            if (string.IsNullOrEmpty(remainingTime) == false)
            {
                string[] splitArr = remainingTime.Split(CommonConstants.PLUS_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr != null && splitArr.Length > 0)
                {
                    for (int i = 0; i < splitArr.Length; i++)
                    {
                        string data;
                        data = splitArr[i].Trim();
                        if (data.EndsWith("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            string yearStr = data.ToUpper().Replace("Y", "");
                            decimal year = yearStr.ConvertToDecimal();
                            convertTime += year;
                        }
                        else if (data.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                        {
                            string monStr = data.ToUpper().Replace("M", "");
                            decimal month = monStr.ConvertToDecimal() / 12;
                            convertTime += month;
                        }
                        else if (data.EndsWith("D"))
                        {
                            string dayStr = data.ToUpper().Replace("D", "");
                            decimal day = dayStr.ConvertToDecimal() / 365;
                            convertTime += day;
                        }
                    }
                }
            }

            return convertTime;
        }


        public static void Prompt2Second(string title, string text)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Sub_Prompt2Second(title, text);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Sub_Prompt2Second(title, text);
                }));
            }
        }


        private static void Sub_Prompt2Second(string title, string text)
        {
            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.PromptTitle = title;
            prompt.PromptContext = text;
            prompt.Owner = Application.Current.MainWindow;
            prompt.Show();
        }

        public static string ToBinary(long x)
        {
            char[] buff = new char[64];

            for (int i = 63; i >= 0; i--)
            {
                buff[i] = x % 2 == 1 ? '1' : '0';
                x = x / 2;
            }

            return new string(buff);
        }

        private const string InstitutionKey = "113_CRM_Inst"; //机构表
        private static readonly string ContactKey = ((int)PluginKeyEnum.CrmPlugin).ToString() + "_CRM_Contact"; //交易员表
        private const string BrokerKey = "113_CRM_Broker"; //broker表
        private const string BondKey = "2_Bond";
        public static List<CrmInst> GetInstitutionsInfoPo(string condition, int resultNum)
        {
            List<CrmInst> result = new List<CrmInst>();
            ConcurrentDictionary<string, CrmInst> dic = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (dic == null)
            {
                return result;
            }
            result = dic.Where(d =>
           (!string.IsNullOrEmpty(d.Value.taglist) && d.Value.taglist.Contains("BOND"))
           &&
           (string.IsNullOrEmpty(condition)
                                     || (d.Value.shortnamecn != null && d.Value.shortnamecn.ToUpper() == condition.ToUpper())
                                     || (d.Value.pinyin != null && d.Value.pinyin.ToUpper() == condition.ToUpper())
                                     || (d.Value.pinyinfull != null && d.Value.pinyinfull.ToUpper() == condition.ToUpper())
                                     || (d.Value.bncshortnamelist != null && d.Value.bncshortnamelist.ToUpper() == condition.ToUpper())
                                     || (d.Value.bcoshortnamelist != null && d.Value.bcoshortnamelist.ToUpper() == condition.ToUpper())
                                     || (d.Value.ncdshortnamelist != null && d.Value.ncdshortnamelist.ToUpper() == condition.ToUpper())
                                     ))
                     .Select(d => d.Value)
                     .Take(resultNum)
                     .ToList();//多条件筛选，新功能增加


         if (result == null || result.Count<=0)
            {
                result = dic.Where(d =>
          (!string.IsNullOrEmpty(d.Value.taglist) && d.Value.taglist.Contains("BOND"))
          &&
          (string.IsNullOrEmpty(condition)
                                    || (d.Value.shortnamecn != null && d.Value.shortnamecn.ToUpper().Contains(condition.ToUpper()))
                                    || (d.Value.pinyin != null && d.Value.pinyin.ToUpper().Contains(condition.ToUpper()))
                                    || (d.Value.pinyinfull != null && d.Value.pinyinfull.ToUpper().Contains(condition.ToUpper()))
                                    || (d.Value.bncshortnamelist != null && d.Value.bncshortnamelist.ToUpper().Contains(condition.ToUpper()))
                                    || (d.Value.bcoshortnamelist != null && d.Value.bcoshortnamelist.ToUpper().Contains(condition.ToUpper()))
                                    || (d.Value.ncdshortnamelist != null && d.Value.ncdshortnamelist.ToUpper().Contains(condition.ToUpper()))
                                    ))
                    .Select(d => d.Value)
                    .Take(resultNum)
                    .ToList();
                List<CrmInst> filterResult = new List<CrmInst>();
                if (result == null || result.Count > 0) {
                    int index = 0;
                    foreach (CrmInst dr in result)
                    {
                        string shortname = dr.shortnamecn;
                        //机构中包含
                        //资管的要过滤掉
                        if (index < resultNum && !string.IsNullOrEmpty(shortname) && shortname.IndexOf("资管") < 0)
                        {
                            filterResult.Add(dr);
                            index++;
                        }
                        if (index == resultNum)
                            break;
                    }
                    if (index == 0)
                    {
                        foreach (CrmInst dr  in result)
                        {
                            filterResult.Add(dr);
                            index++;
                            if (index == resultNum)
                                break;
                        }
                    }
                }
                return filterResult;
            }
            return result;
        }


        public static List<CrmInst> GetInstitutionsTipsNameInfo(string condition, int resultNum)
        {
            List<CrmInst> crmInsts = GetInstitutionsInfoPo(condition, resultNum);
            if (crmInsts != null && crmInsts.Count > 0)
            {
                foreach (var item in crmInsts)
                {
                    item.TipsShortnamelist = GetInstName(item, "");
                }
            }
            return crmInsts;
        }


        /// <summary>
        /// 获取机构台子简称，根据机构
        /// BCO——信用 1
        /// BNC——利率 2
        /// ABS——ABS  11
        /// NCD——NCD  12
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="goodstype"></param>
        /// <returns></returns>
        public static string GetInstName(CrmInst dr, string goodstype)
        {
            string result = string.Empty;

            if (dr == null)
                return result;
            ///根据债券获取机构名称
            if (!string.IsNullOrEmpty(goodstype))
            {
                if (goodstype == "2" || goodstype == "BNC")
                    result = dr.bncshortnamelist + "";
                else if (goodstype == "1" || goodstype == "11" || goodstype == "BCO" || goodstype == "ABS")
                    result = dr.bcoshortnamelist + "";
                else if (goodstype == "12" || goodstype == "NCD")
                    result = dr.ncdshortnamelist + "";
            }
            else
            {
                ///根据用户权限获取机构名称
                if (IsContain("2"))
                {
                    result = dr.bncshortnamelist + "";
                    if (string.IsNullOrEmpty(result))
                    {
                        if (IsContain("1") || IsContain("11"))
                            result = dr.bcoshortnamelist + "";
                    }

                    if (string.IsNullOrEmpty(result) && IsContain("12"))
                        result = dr.ncdshortnamelist + "";

                }
                else if (IsContain("1") || IsContain("11"))
                {
                    result = dr.bcoshortnamelist + "";
                    if (string.IsNullOrEmpty(result) && IsContain("12"))
                        result = dr.ncdshortnamelist + "";
                }
                else if (IsContain("12"))
                    result = dr.bncshortnamelist + "";

            }


            if (string.IsNullOrEmpty(result))
                result = dr.shortnamecn + "";

            if (result.Contains(","))
            {
                string[] arr = result.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                    result = arr[0];
            }

            return result;
        }
        private static bool IsContain(string goodstype)
        {
            List<string> termidsList = LocalEnv.UserInfo.Termids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (termidsList != null && termidsList.Count > 0)
                return termidsList.Contains(goodstype);

            return false;
        }
        /// <summary>
        /// 获取机构台子简称，根据机构id
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="goodstype"></param>
        /// <returns></returns>
        public static string GetInstName2(string instid, string goodstype = "")
        {
            string result = string.Empty;
            ConcurrentDictionary<string, CrmInst> dic = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);
            if (dic == null)
            {
                return result;
            }
            List<CrmInst> list = dic.Where(d =>
            (!string.IsNullOrEmpty(d.Value.taglist) && d.Value.taglist.Contains("BOND"))
            && ((d.Value.idbkey != null && d.Value.idbkey.Contains(instid)))
            )
                       .Select(d => d.Value)
                       .Take(1)
                       .ToList();//多条件筛选，新功能增加
            if (list != null && list.Count > 0)
                result = list[0].shortnamecn;
            return result;
        }

        /// <summary>
        /// 根据idbKey获取机构信息
        /// </summary>
        /// <param name="idbKey"></param>
        /// <returns></returns>
        public static CrmInst GetInstByIdbKey(string idbKey)
        {
            CrmInst result = new CrmInst();
            ConcurrentDictionary<string, CrmInst> dic = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);
            if (dic == null)
            {
                return result;
            }
            List<CrmInst> list = dic.Where(d =>
            (!string.IsNullOrEmpty(d.Value.taglist) && d.Value.taglist.Contains("BOND"))
            && ((d.Value.idbkey != null && d.Value.idbkey.Contains(idbKey)))
            )
                       .Select(d => d.Value)
                       .Take(1)
                       .ToList();//多条件筛选，新功能增加
            if (list != null && list.Count > 0)
                result = list[0];
            return result;
        }


        public static List<DataGridDataPo> GetContactByInstName(string instName, int resultNum, string goodstype)
        {
            List<DataGridDataPo> dataList = new List<DataGridDataPo>();

            List<CrmContact> result = new List<CrmContact>();
            instName = instName.Replace("'", "").Replace("*", "");
            List<CrmInst> dt = GetInstitutionsInfoPo(instName, resultNum);
            if (dt.Count > 0)
            {
                Dictionary<string, CrmInst> instDic = new Dictionary<string, CrmInst>();
                List<string> insts = new List<string>();
                foreach (CrmInst dr in dt)
                {
                    string instkey = dr.idbkey;
                    if (!instDic.ContainsKey(instkey))
                        instDic.Add(instkey, dr);

                    if (!insts.Contains(instkey))
                    {
                        insts.Add(instkey);
                    }
                }
                List<CrmContact> contactList = GetContactInfoByInID(insts, resultNum);
                if (contactList.Count > 0)
                {
                    foreach (CrmContact drCon in contactList)
                    {
                        string instid = drCon.instid;
                        if (instDic.ContainsKey(instid))
                        {
                            DataGridDataPo dg = SetDataGridDataPo(GetInstName(instDic[instid]), drCon.namecn, drCon.idbkey, drCon.instid);
                            if (dataList.Count >= resultNum)
                                return dataList;
                            dataList.Add(dg);
                        }
                    }
                }
                if (dataList.Count > resultNum)
                    dataList = dataList.Take(resultNum).ToList();
            }
            return dataList;
        }

        /// <summary>
        /// 获取交易员的拼音信息
        /// </summary>
        /// <param name="idbKey"></param>
        /// <returns></returns>
        public static DataGridDataPo GetContactByIdbKey(string idbKey)
        {
            List<CrmContact> dt = GetContactInfoPo(idbKey, 20, "");
            DataGridDataPo dg = new DataGridDataPo();
            foreach (CrmContact dr in dt)
            {
                if (dr.idbkey.Equals(idbKey))
                {
                    dg.InstName = dr.namecn;
                    dg.InstIdbKey = dr.idbkey;
                    dg.instcode = dr.instid;
                    dg.PinYin = dr.pinyin;
                    dg.PinYinFull = dr.pinyinfull;
                    break;
                }
            }
            return dg;
        }

        public static List<DataGridDataPo> GetContactByContactName(string contactName, int resultNum)
        {
            contactName = contactName.Replace("'", "");
            List<CrmContact> dt = GetContactInfoPo(contactName, resultNum, "");
            List<DataGridDataPo> dataList = new List<DataGridDataPo>();
            foreach (CrmContact dr in dt)
            {
                DataGridDataPo dg = new DataGridDataPo()
                {
                    InstTraderName = string.Format("{0}({1})", dr.instname, (dr.namecn).Trim()),
                    IdbKey = dr.idbkey,
                    TraderName = dr.namecn,
                    InstIdbKey = dr.instid,
                    InstName = dr.instname

                };
                dataList.Add(dg);
            }
            return dataList;
        }


        public static List<CrmContact> GetContactInfoPo(string condition, int resultNum, string instid)
        {
            List<CrmContact> result = new List<CrmContact>();
            ConcurrentDictionary<string, CrmContact> dic = (ConcurrentDictionary<string, CrmContact>)AppUtils.Application.GetObject(ContactKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                //result = dic.Where(d => (d.Value.childTagName != null && d.Value.childTagName.Contains(condition))
                //                         || (d.Value.pinyin != null && d.Value.pinyin.Contains(condition)))
                //         .Select(d => d.Value)
                //         .Take(resultNum)
                //         .ToList();//多条件筛选，新功能增加
                List<CrmContact> temp = dic.Where(d => (string.IsNullOrEmpty(condition) 
                || (d.Value.namecn != null && d.Value.namecn.ToUpper().Contains(condition.ToUpper()))
                || (d.Value.pinyin != null && d.Value.pinyin.ToUpper().Contains(condition.ToUpper()))
                || (d.Value.pinyinfull != null && d.Value.pinyinfull.ToUpper().Contains(condition.ToUpper()))
                || d.Value.idbkey == condition)
                                            && (string.IsNullOrEmpty(instid) || d.Value.instid == instid))
                       .Select(d => d.Value)
                       //.Take(resultNum)
                       .ToList();//多条件筛选，新功能增加
                string goodstype = BizTypeConvert(LocalEnv.UserInfo.Termids);
                string[] goodstypeArr = goodstype.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //int i = 0;
                //foreach (CrmContact po in temp)
                //{
                //    foreach (string gt in goodstypeArr)
                //    {
                //        if (!string.IsNullOrEmpty(po.biztype) && po.biztype.Contains(gt))
                //        {
                //            if (i < resultNum)
                //            {
                //                result.Add(po);
                //                i++;
                //                break;
                //            }
                //            else
                //            {
                //                continue;
                //            }
                           
                //        }
                //    }
                //}
                return temp.OrderBy(o => o.namecn).ToList();
            }
        }

        /// <summary>
        /// 获取机构下当前台子交易员
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <param name="instid"></param>
        /// <returns></returns>
        public static List<CrmContact> GetContactInfoPoByTermids(string condition, int resultNum, string instid)
        {
            List<CrmContact> result = new List<CrmContact>();
            ConcurrentDictionary<string, CrmContact> dic = (ConcurrentDictionary<string, CrmContact>)AppUtils.Application.GetObject(ContactKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                List<CrmContact> temp = dic.Where(d => (string.IsNullOrEmpty(condition)
                || (d.Value.namecn != null && d.Value.namecn.ToUpper().Contains(condition.ToUpper()))
                || (d.Value.pinyin != null && d.Value.pinyin.ToUpper().Contains(condition.ToUpper()))
                || (d.Value.pinyinfull != null && d.Value.pinyinfull.ToUpper().Contains(condition.ToUpper()))
                || d.Value.idbkey == condition)
                                            && (string.IsNullOrEmpty(instid) || d.Value.instid == instid))
                       .Select(d => d.Value)
                       //.Take(resultNum)
                       .ToList();//多条件筛选，新功能增加
                string goodstype = BizTypeConvert(LocalEnv.UserInfo.SubCompanyId);
                string[] goodstypeArr = goodstype.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (CrmContact po in temp)
                {
                    foreach (string gt in goodstypeArr)
                    {
                        if (!string.IsNullOrEmpty(po.biztype) && po.biztype.Contains(gt))
                        {
                            result.Add(po);
                        }
                    }
                }
                return result.OrderBy(o => o.namecn).ToList();
            }
        }


        public static List<CrmContact> GetContactInfoByInID(List<string> instid, int resultNum)
        {
            List<CrmContact> result = new List<CrmContact>();
            ConcurrentDictionary<string, CrmContact> dic = (ConcurrentDictionary<string, CrmContact>)AppUtils.Application.GetObject(ContactKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                List<CrmContact> temp = dic.Where(_ => (instid != null && instid.Count > 0 && instid.Contains(_.Value.instid)))
                       .Select(d => d.Value)
                       .ToList();//多条件筛选，新功能增加
                string goodstype = BizTypeConvert(LocalEnv.UserInfo.Termids);
                string[] goodstypeArr = goodstype.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int i = 0;
                foreach (CrmContact po in temp)
                {
                    foreach (string gt in goodstypeArr)
                    {
                        if (!string.IsNullOrEmpty(po.biztype) && po.biztype.Contains(gt))
                        {
                            if (i < resultNum)
                            {
                                result.Add(po);
                                break;
                            }
                            else
                            {
                                continue;
                            }
                            i++;
                        }
                    }
                }
                return result.OrderBy(o => o.namecn).ToList();
            }
        }

        public static List<BrokerInfoPo> GetBrokerInfoPo(string brokerName, string goodstype, int resultNum)
        {
            List<BrokerInfoPo> result = new List<BrokerInfoPo>();

            brokerName = brokerName.Replace("'", "").Replace("*", "");
            string name = BizTypeConvert(LocalEnv.UserInfo.Termids);
            string[] goodstypeArr = name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            goodstype = tConvert(goodstype);
            ConcurrentDictionary<string, BrokerInfoPo> dic = (ConcurrentDictionary<string, BrokerInfoPo>)AppUtils.Application.GetObject(BrokerKey);
            List<BrokerInfoPo> temp = new List<BrokerInfoPo>();
            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                temp = dic.Where(d => ((!string.IsNullOrEmpty(brokerName)&&(!string.IsNullOrWhiteSpace(d.Value.Term_ids)&& d.Value.Term_ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(goodstype))
                                        && ((d.Value.Account != null && d.Value.Account.ToUpper().Contains(brokerName.ToUpper())) || (d.Value.AccountName != null && d.Value.AccountName.ToUpper().Contains(brokerName.ToUpper()))
                                        || (d.Value.PinYin != null && d.Value.PinYin.ToUpper().Contains(brokerName.ToUpper())))))
                                        )
                   .Select(d => d.Value)
                   .ToList();//多条件筛选，新功能增加
                             // return result;
                int i = 0;
                foreach (BrokerInfoPo po in temp)
                {
                    if (i < resultNum)
                        result.Add(po);
                    else
                        break;
                    i++;
                    //foreach (string gt in goodstypeArr)
                    //{
                    //    if ((gt == "BNC" && goodstype == "2") || (gt == "BCO" && goodstype == "1") || (gt == "ABS" && goodstype == "11") || (gt == "NCD" && goodstype == "12"))
                    //    {
                    //        if (po.Term_ids.Contains(goodstype) && i < resultNum)
                    //            result.Add(po);
                    //        else
                    //            break;
                    //        i++;
                    //    }
                    //}
                }
            }
            return result.OrderBy(o => o.AccountName).ToList();
        }

        /// <summary>
        /// 获取broker信息
        /// </summary>
        /// <param name="brokerName"></param>
        /// <param name="filterName"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public static List<BrokerInfoPo> GetBroker(string brokerName, List<string> filterNameList, int resultNum)
        {
            List<BrokerInfoPo> result = new List<BrokerInfoPo>();

            brokerName = brokerName.Replace("'", "").Replace("*", "");
            string name = LocalEnv.UserInfo.Termids;
            string[] goodstypeArr = name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            ConcurrentDictionary<string, BrokerInfoPo> dic = (ConcurrentDictionary<string, BrokerInfoPo>)AppUtils.Application.GetObject(BrokerKey);
            List<BrokerInfoPo> temp = new List<BrokerInfoPo>();
            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                temp = dic.Where(d => (string.IsNullOrEmpty(brokerName)
                                          || (d.Value.Account != null && d.Value.Account.ToUpper().Contains(brokerName.ToUpper()))
                                          || (d.Value.AccountName != null && d.Value.AccountName.ToUpper().Contains(brokerName.ToUpper()))
                                          || (d.Value.PinYin != null && d.Value.PinYin.ToUpper().Contains(brokerName.ToUpper())))
                                      && (filterNameList == null || filterNameList.Count == 0 || !filterNameList.Contains(d.Value.AccountName))
                                      && (d.Value.Term_ids != null && d.Value.Term_ids.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries).Any(item => goodstypeArr.Contains(item))
                                      ))
                   .Select(d => d.Value).Take(resultNum)
                   .ToList();
            }
            return temp;
        }


        /// <summary>
        /// 根据登录名查询id
        /// </summary>
        /// <param name="accoutName"></param>
        /// <returns></returns>
        public static string GetBrokerIdByAccount(string accoutName)
        {
            var id = string.Empty;
            List<BrokerInfoPo> result = new List<BrokerInfoPo>();
            ConcurrentDictionary<string, BrokerInfoPo> dic = (ConcurrentDictionary<string, BrokerInfoPo>)AppUtils.Application.GetObject(BrokerKey);
            if (dic == null)
            {
                return id;
            }
            lock (dic)
            {
                result = dic.Where(d => ((!string.IsNullOrEmpty(accoutName)
                                        && ((d.Value.Account != null && d.Value.AccountName == accoutName))
                                        )))
                   .Select(d => d.Value)
                   .ToList();//多条件筛选，新功能增加
                if (result != null && result.Count > 0)
                {
                    id = result[0].Idb_key + "";
                }
                return id;
            }
        }

        /// <summary>
        /// 根据台子获取broker
        /// </summary>
        /// <param name="brokerName"></param>
        /// <param name="resultNum">为0时查找全部</param>
        /// <returns></returns>
        public static List<BrokerInfoPo> GetBrokerByTermids(int resultNum)
        {
            List<BrokerInfoPo> result = new List<BrokerInfoPo>();
            string name = LocalEnv.UserInfo.Termids;//BizTypeConvert(LocalEnv.UserInfo.Termids);
            string[] goodstypeArr = name.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            ConcurrentDictionary<string, BrokerInfoPo> dic = (ConcurrentDictionary<string, BrokerInfoPo>)AppUtils.Application.GetObject(BrokerKey);
            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                if (resultNum > 0)
                {
                    result = dic.Where(d => (
                                d.Value.Term_ids != null && d.Value.Term_ids.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries).Any(item => goodstypeArr.Contains(item))
                                      ))
                   .Select(d => d.Value).Take(resultNum)
                   .ToList();//多条件筛选，新功能增加  
                }
                else
                {
                    result = dic.Where(d => (
                                   d.Value.Term_ids != null && d.Value.Term_ids.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries).Any(item => goodstypeArr.Contains(item))
                                         ))
                      .Select(d => d.Value)
                      .ToList();//多条件筛选，新功能增加  
                }
            }
            return result.OrderBy(o => o.AccountName).ToList();
        }

        /// <summary>
        /// 获取债券基本信息
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public static List<BondsInfoPo> GetBondsInfoPo(string condition, int resultNum, string listed_market = null, string marketindicatorfilter = null)
        {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            //string key = GetPluginShareKey(2, "Bond");
            SortedDictionary<string, BondsInfoPo> dic = (SortedDictionary<string, BondsInfoPo>)AppUtils.Application.GetObject(BondKey);
            List<string> termIds = LocalEnv.UserInfo.Termids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (dic == null)
            {
                return result;
            }

            if (resultNum == 1 && listed_market == null)
            {
                if (dic.ContainsKey(condition))
                    result.Add(dic[condition]);
                return result;
            }

            lock (dic)
            {
                IEnumerable<KeyValuePair<string, BondsInfoPo>> v = null;
                if (listed_market != null)
                {
                    if (listed_market == "SSE" || listed_market == "SSZ")
                        v = (from d in dic
                             where d.Value.searchKey.ToLower().Contains(condition.ToLower()) && d.Value.listed_Market == listed_market
                             select d);
                    else
                        v = (from d in dic where d.Value.searchKey.ToLower().Contains(condition.ToLower()) && d.Value.listed_Market == listed_market && string.IsNullOrEmpty(d.Value.Security_Type_Id) == false select d);
                }
                else
                {
                    if (marketindicatorfilter == null)
                        v = (from d in dic where d.Value.searchKey.ToLower().Contains(condition.ToLower()) select d);
                    else
                        v = (from d in dic where d.Value.searchKey.ToLower().Contains(condition.ToLower()) && d.Value.MarketIndicator == marketindicatorfilter select d);
                }
                int i = 0;
                foreach (KeyValuePair<string, BondsInfoPo> kv in v)
                {
                    if (termIds.Contains(tConvert(kv.Value.SECURITY_TYPE)))
                    {
                        if (i < resultNum)
                            result.Add(kv.Value);
                        else
                            break;

                        i++;
                    }
                }
            }

            return result;
        }

        public static BondsInfoPo GetBondsInfoByCode(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return null;
            BondsInfoPo result = null;
            string key = GetPluginShareKey(EPluginShareKey.BondByCode);
            int i = 0;
            List<string> termIds = LocalEnv.UserInfo.Termids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            SortedDictionary<string, BondsInfoPo> dic = (SortedDictionary<string, BondsInfoPo>)AppUtils.Application.GetObject(BondKey);
            if (dic != null && dic.Count > 0)
            {
                BondsInfoPo value;
                if (dic.TryGetValue(condition, out value))
                {
                    if (termIds.Contains(tConvert(value.SECURITY_TYPE)))
                    {
                        return value;
                    }
                }
            }
            return result;
        }

        private static string GetPluginShareKey(EPluginShareKey pluginShareKey)
        {
            return string.Format("{0}_{1}", Math.Floor((int)pluginShareKey / 10.0), pluginShareKey.ToString());
        }
        public static string tConvert(string type)
        {
            string result = "";
            switch (type)
            {
                case "BCO":
                    result = "1";
                    break;
                case "BNC":
                    result = "2";
                    break;
                case "ABS":
                    result = "11";
                    break;
                case "NCD":
                    result = "12";
                    break;
                case "IRS":
                    result = "14";
                    break;
            }
            return result;
        }

        /// <summary>
        /// 台子类型转换
        /// BCO——信用 1
        /// BNC——利率 2
        /// ABS——ABS  11
        /// NCD——NCD  12
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static string BizTypeConvert(string ids)
        {
            string result = string.Empty;
            string[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string id in idArr)
            {
                if (id == "1")
                    //result += string.IsNullOrEmpty(result) ? "BCO" : ",BCO";
                    stringBuilder.Append(",BCO");
                else if (id == "2")
                    //result += string.IsNullOrEmpty(result) ? "BNC" : ",BNC";
                    stringBuilder.Append(",BNC");
                else if (id == "11")
                    //result += string.IsNullOrEmpty(result) ? "ABS" : ",ABS";
                    stringBuilder.Append(",ABS");
                else if (id == "12")
                    //result += string.IsNullOrEmpty(result) ? "NCD" : ",NCD";
                    stringBuilder.Append(",NCD");
                else if (id == "14")
                    //result += string.IsNullOrEmpty(result) ? "IRS" : ",IRS";
                    stringBuilder.Append(",IRS");
            }
            result = stringBuilder.ToString().Trim(',');

            return result;
        }

        private static string GetInstName(CrmInst dr)
        {
            string result = string.Empty;

            if (dr == null)
                return result;

            result = dr.bncshortnamelist;//["bncshortnamelist"] + "";
            if (string.IsNullOrEmpty(result))
                result = dr.shortnamecn;//["shortnamecn"] + "";
            return result;
        }

        private static DataGridDataPo SetDataGridDataPo(string instname, string traderName, string traderIdbkey, string instid)
        {
            return new DataGridDataPo()
            {
                InstTraderName = string.Format("{0}({1})", instname, traderName.Trim()),
                IdbKey = traderIdbkey,
                InstIdbKey = instid,
                TraderName = traderName,
                InstName = instname
            };
        }

        /// <summary>
        /// 台子类型转换
        /// BCO——信用 1
        /// BNC——利率 2
        /// ABS——ABS  11
        /// NCD——NCD  12
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static string BizTypeConvertToCode(string ids)
        {
            string result = ids;
            switch (ids)
            {
                case "BNC":
                    result = "2";
                    break;
                case "BCO":
                    result = "1";
                    break;
                case "ABS":
                    result = "11";
                    break;
                case "NCD":
                    result = "12";
                    break;
                case "IRS":
                    result = "14";
                    break;
            }
            return result;
        }
    }
}
