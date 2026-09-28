using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.WorkTable.Plugin.Command;
using STC.WorkTable.Plugin.Utils.Data.CRM.Model.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.WorkTable.Plugin.Utils
{
    class CommonUtils
    {
        /// <summary>
        /// 弹窗命令处理类
        /// </summary>
        public static PopupWindowCommand PopupWindowCommand = new PopupWindowCommand();

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


        /// <summary>
        /// 根据bond_subType和 ent_cor获得债券类型名称
        /// </summary>
        /// <param name="bond_subType"></param>
        /// <returns>债券类型名称</returns>
        public static string GetBondCategoryName(string securityTypeID)
        {
            if (string.IsNullOrEmpty(securityTypeID) == false)
            {
                switch (securityTypeID)
                {
                    // 【利率债】 
                    case "100001":
                        return "国债";
                    case "100003":
                        return "政策性金融债";
                    case "100011":
                        return "地方政府债";
                    case "100002":
                        return "央票";
                    // 【信用债】
                    case "100027":
                    case "100052":
                        return "政府支持机构债";
                    case "100004":
                    case "100006":
                    case "100010":
                    case "100029":
                    case "100050":
                    case "100055":
                    case "100057":
                    case "100073":
                    case "100025":
                        return "公司债";
                    case "100005":
                    case "100007":
                    case "100009":
                    case "100022":
                    case "100023":
                    case "100024":
                    case "100026":
                    case "100028":
                    case "100053":
                    case "100054":
                    case "100056":
                    case "100082":
                    case "100083":
                        return "金融债";
                    case "100041":
                        return "NCD";
                    case "100071":
                    case "100072":
                        return "ABS";
                }
            }
            return string.Empty;
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

        private const string InstitutionKey = "111_CRM_Inst"; //机构表
        private const string ContactKey = "111_CRM_Contact"; //交易员表
        /// <summary>
        /// 获取机构信息
        /// </summary>
        /// <param name="condition">关键字</param>
        /// <param name="resultNum">数量</param>
        /// <returns></returns>
        public static List<CrmInst> GetInstitutionsInfoPo(string condition, int resultNum, List<string> instidList = null)
        {
            List<CrmInst> result = new List<CrmInst>();
            SortedDictionary<string, CrmInst> dic = (SortedDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                int i = 0;
                var v = (from d in dic where d.Key.ToLower().Contains(condition) select d);

                if (instidList == null || instidList.Count == 0)
                {
                    result = dic.Where(d => d.Value.shortnamecn.Contains(condition)
                                              || d.Value.pinyin.Contains(condition)
                                              || d.Value.pinyinfull.Contains(condition))
                              .Select(d => d.Value)
                              .Take(resultNum)
                              .ToList();//多条件筛选，新功能增加
                }
                else
                {
                    result = dic.Where(d => (d.Value.shortnamecn.Contains(condition)
                                              || d.Value.pinyin.Contains(condition)
                                              || d.Value.pinyinfull.Contains(condition)) && instidList.Contains(d.Value.idbkey))
                              .Select(d => d.Value)
                              .Take(resultNum)
                              .ToList();//多条件筛选，新功能增加
                }
               
                return result;
            }
        }
		
        /// <summary>
        /// 根据输入文字筛选交易员
        /// @ requirement 1.8.1.:zs51
        ///@author sz:lihuan
        ///@data 2021/12/16 10:08
        /// </summary>
        /// <param name="condition">输入文字</param>
        /// <returns></returns>
        public static List<CrmContact> GetContactInfoPo(string condition, List<string> instList = null)
        {
            List<CrmContact> result = new List<CrmContact>();
            SortedDictionary<string, CrmContact> dic = (SortedDictionary<string, CrmContact>)AppUtils.Application.GetObject(ContactKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {
                var v = (from d in dic where d.Key.ToLower().Contains(condition) select d);
                if (instList == null || instList.Count == 0)
                {
                    result = dic.Where(d => d.Value.namecn.Contains(condition))
                           .Select(d => d.Value)
                           .ToList();//多条件筛选，新功能增加
                }
                else
                {
                    result = dic.Where(d => d.Value.namecn.Contains(condition) && instList.Contains(d.Value.instid))
                           .Select(d => d.Value)
                           .ToList();//多条件筛选，新功能增加
                }

                return result;

            }
        }

        /// <summary>
        /// 根据机构id获取交易员
        /// @ requirement 1.8.1.:zs51
        ///@author sz:lihuan
        ///@data 2021/12/16 
        /// </summary>
        /// <param name="insid">机构id</param>
        /// <returns></returns>
        public static List<CrmContact> GetContactidInfoPo(string insid)
        {
            List<CrmContact> result = new List<CrmContact>();
            CrmInst resultins = new CrmInst();
            SortedDictionary<string, CrmContact> dicContant = (SortedDictionary<string, CrmContact>)AppUtils.Application.GetObject(ContactKey);
            if (dicContant == null)
            {
                return null;
            }
            lock (dicContant)
            {
                    result = dicContant.Where(d => d.Value.instid.Contains(insid))
                                .Select(d => d.Value)
                                .ToList();
                return result;
            }
        }

        public static List<CrmContact> GetContacts(string instId = null, string trdId = null)
        {
            List<CrmContact> result = new List<CrmContact>();

            SortedDictionary<string, CrmContact> dicContant = (SortedDictionary<string, CrmContact>)AppUtils.Application.GetObject(ContactKey);

            if (dicContant == null)
            {
                return null;
            }
            lock (dicContant)
            {
                result = dicContant.Where(
                    d =>
                    {
                        if (!string.IsNullOrEmpty(instId) && string.IsNullOrEmpty(trdId))
                            return d.Value.instid == instId;
                        if (string.IsNullOrEmpty(instId) && !string.IsNullOrEmpty(trdId))
                            return d.Value.idbkey == trdId;
                        if (!string.IsNullOrEmpty(instId) && !string.IsNullOrEmpty(trdId))
                            return d.Value.instid == instId && d.Value.idbkey == trdId;
                        return true;
                    }
                    )
                .Select(d => d.Value)
                .ToList();
                return result;
            }
        }


        /// <summary>
        /// 根据机构id 获取机构名称
        ///  @ requirement 1.8.1.:zs51
        ///@author sz:lihuan
        ///@data 2021/12/16 10:08
        /// </summary>
        /// <param name="insid">机构id</param>
        /// <returns></returns>
        public static List<CrmInst> GetInstitutionsIDInfoPo(string insid)
        {
            List<CrmInst> result = new List<CrmInst>();

            SortedDictionary<string, CrmInst> dicContant = (SortedDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (dicContant == null)
            {
                return null;
            }
            lock (dicContant)
            {
                result = dicContant.Where(d => d.Value.idbkey.Contains(insid))
                        .Select(d => d.Value)
                            .ToList();
                return result;
            }
        }
       
        public static List<CrmInst> GetInstitutionsInfoPo(List<string> instidList = null)
        {
            List<CrmInst> result;
            SortedDictionary<string, CrmInst> dic = (SortedDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (instidList == null || instidList.Count == 0)
            {
                result = dic.Select(d => d.Value).ToList();
            }
            else
            {
                result = dic.Where(d => instidList.Contains(d.Value.idbkey))
                           .Select(d => d.Value)
                           .ToList();
            }
            return result;
        }
        /// <summary>
        /// 从交易员集合中进行搜索
        /// </summary>
        /// <param name="contant"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<CrmContact> GetContacttextInfoPo(List<CrmContact> contant, string text)
        {
            List<CrmContact> result = new List<CrmContact>();
            lock (contant)
            {

                result = contant.Where(d => d.namecn.Contains(text))
                            .Select(d => d)
                            .ToList();
                return result;
            }
            
        }
    }
}
