using com.gateway.log;
using netapi.Api;
using STC.Common.Api;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.Common;
using STC.WorkBench.Plugin.Command;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.WorkBench.Plugin.Utils
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
            string remainingTime_temp = string.Empty;
            if (string.IsNullOrEmpty(remainingTime) == false)
            {
                string[] arr = remainingTime.Split(CommonConstants.PLUS_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                {
                    remainingTime_temp = arr[0];
                }
                string[] splitArr = remainingTime_temp.Split(CommonConstants.PLUS_CHAR, StringSplitOptions.RemoveEmptyEntries);
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

        

        public static List<CrmContact> GetDBContactInfoPo(string conditon, string instCode, string traderid = null)
        {
            //List<CrmContact> result = new List<CrmContact>();

            CRMRequestParamPo request = new CRMRequestParamPo();
            request.PageIndex = 0;
            request.PageSize = 2000;
            request.dealerStr = conditon;
            request.instCode = instCode;
            if (!string.IsNullOrEmpty(traderid))
            {
                request.id = traderid.ConvertToInt();
            }

            var reply = ApiManager.Instance.GetDefaultApi().Request<CrmContactReply>(ApiConsts.CRMSvr, ApiConsts.TOPIC_CRM_CONTACTDB, request);
            if (reply != null && "0000".Equals(reply.Code))
            {
                int maxCount = reply.totalSize;
                LogHelper.Debug("MaxCount:" + maxCount);
                if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
                {
                    return reply.Datas;
                }
            }
            return null;
        }

        private const string InstitutionKey = "113_CRM_Inst"; //机构表
        private const string ContactKey = "113_CRM_Contact"; //交易员表
        /// <summary>
        /// 获取机构信息
        /// </summary>
        /// <param name="condition">关键字</param>
        /// <param name="resultNum">数量</param>
        /// <returns></returns>
        public static List<CrmInst> GetInstitutionsInfoPo(string condition, int resultNum, List<string> instidList = null)
        {
            List<CrmInst> result = new List<CrmInst>();
            ConcurrentDictionary<string, CrmInst> dic = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (dic == null)
            {
                return result;
            }

            //int i = 0;
            //var v = (from d in dic where d.Key.ToLower().Contains(condition) select d);

            if (instidList == null || instidList.Count == 0)
            {
                result = dic.Where(d => (d.Value.shortnamecn != null && d.Value.shortnamecn.Contains(condition))
                                          || (d.Value.pinyin != null && d.Value.pinyin.Contains(condition))
                                          || (d.Value.pinyinfull != null && d.Value.pinyinfull.Contains(condition)))
                          .Select(d => d.Value)
                          .Take(resultNum)
                          .ToList();//多条件筛选，新功能增加
            }
            else
            {
                result = dic.Where(d => ((d.Value.shortnamecn != null && d.Value.shortnamecn.Contains(condition))
                                          || (d.Value.pinyin != null && d.Value.pinyin.Contains(condition))
                                          || (d.Value.pinyinfull != null && d.Value.pinyinfull.Contains(condition)))
                                          && instidList.Contains(d.Value.idbkey))
                          .Select(d => d.Value)
                          .Take(resultNum)
                          .ToList();//多条件筛选，新功能增加
            }

            return result;
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

            ConcurrentDictionary<string, CrmInst> dicContant = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

            if (dicContant == null)
            {
                return null;
            }

            result = dicContant.Where(d => d.Value.idbkey != null && d.Value.idbkey == insid)
                    .Select(d => d.Value)
                        .ToList();
            return result;
        }
       
        public static List<CrmInst> GetInstitutionsInfoPo(List<string> instidList = null)
        {
            List<CrmInst> result;
            ConcurrentDictionary<string, CrmInst> dic = (ConcurrentDictionary<string, CrmInst>)AppUtils.Application.GetObject(InstitutionKey);

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

        public static List<CrmContact> GetContactInfoPo(string condition, string instid)
        {
            List<CrmContact> result = new List<CrmContact>();
            ConcurrentDictionary<string, CrmContact> dic = (ConcurrentDictionary<string, CrmContact>)AppUtils.Application.GetObject(ContactKey);

            if (dic == null)
            {
                return result;
            }
            lock (dic)
            {

                result = dic.Where(d => (string.IsNullOrEmpty(condition) || (!string.IsNullOrEmpty(condition) 
                                            && ((d.Value.namecn != null && d.Value.namecn.Contains(condition)) || d.Value.idbkey == condition))) 
                                            && (string.IsNullOrEmpty(instid) || (!string.IsNullOrEmpty(instid) && d.Value.instid == instid)))
                       .Select(d => d.Value)
                       .ToList();//多条件筛选，新功能增加

                return result;

            }
        }


    }
}
