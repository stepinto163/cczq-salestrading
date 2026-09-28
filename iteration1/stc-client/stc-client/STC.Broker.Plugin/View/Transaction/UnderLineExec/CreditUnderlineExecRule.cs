using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Transaction.UnderLineExec;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.View.Transaction
{
    /// <summary>
    /// 对用户输入的线下成交单字符串进行分析；分析出成交单的代码，结果，数量，结算类型
    /// </summary>
    public class CreditUnderlineExecRule : BaseUnderlineExec
    {
        private static CreditUnderlineExecRule instance = new CreditUnderlineExecRule();

        public static CreditUnderlineExecRule Instance { get { return instance; } }

        private readonly List<string> DefaultSettleArray = new List<string>();

        private CreditUnderlineExecRule()
        {
            DefaultSettleArray.Add("+0");
            DefaultSettleArray.Add("+1");
            DefaultSettleArray.Add("明天+0");
            DefaultSettleArray.Add("明天+1");
            DefaultSettleArray.Add("周一+0");
            DefaultSettleArray.Add("周一+1");
            DefaultSettleArray.Add("周二+0");
            DefaultSettleArray.Add("周二+1");
            DefaultSettleArray.Add("周三+0");
            DefaultSettleArray.Add("周三+1");
            DefaultSettleArray.Add("周四+0");
            DefaultSettleArray.Add("周四+1");
            DefaultSettleArray.Add("周五+0");
            DefaultSettleArray.Add("周五+1");
            DefaultSettleArray.Add("周六+0");
            DefaultSettleArray.Add("周六+1");
            DefaultSettleArray.Add("周日+0");
            DefaultSettleArray.Add("周日+1");
        }
        /// <summary>
        /// 解析信用台格式
        /// </summary>
        /// <param name="index"></param>
        /// <param name="changeOrder"></param>
        /// <param name="lineData"></param>
        public override void Analysis(int index, OrderDetailPo changeOrder, string[] lineData)
        {
            if ((string.IsNullOrEmpty(changeOrder.PartySubID) && string.IsNullOrEmpty(changeOrder.CounterPartySubID))
                || (string.IsNullOrEmpty(changeOrder.PartySubID) && !string.IsNullOrEmpty(changeOrder.CounterPartySubID)))
            {
                changeOrder.PartySubID = LocalEnv.UserInfo.UserId;
                changeOrder.PartySubName = LocalEnv.UserInfo.FullName;
            }

            decimal d = 0;
            //债券代码
            bool isSetCode = false;
            //价格
            bool isSetPrice = false;
            //数量
            bool isSetQty = false;
            //结算方式
            bool isSetSettle = false;
            //ofr方的交易员
            bool isOfr = false;
            //bidbroker
            bool isBid = false;
            //ofr方的交易员列表
            DataTable temDt = null;
            foreach (string str in lineData)
            {
                //债券代码
                string temStr = str.Trim();
                BondGridDataPo bestBond = GetBestBond(temStr, 20);
                if (bestBond != null && !isSetCode)
                {
                    changeOrder.SecurityID = bestBond.BondID;
                    changeOrder.goodsShortName = bestBond.ShortName;
                    changeOrder.timeToMaturity = bestBond.RemaingingTime;
                    changeOrder.goodstype = bestBond.Goodstype;
                    changeOrder.goodsSubType = bestBond.Subtype;

                    isSetCode = true;

                    continue;
                }
                //数量
                if (decimal.TryParse(DataAnalysisHelper.AnalysisQty(temStr), out d) && d < 2000000 && d > 200 && !isSetQty)
                {
                    decimal temD = 0;
                    if (decimal.TryParse(temStr, out temD))
                    {
                        if (temD < 99999)
                        {
                            changeOrder.qty = new decimal?(d);
                            isSetQty = true;
                        }
                    }
                    else
                    {
                        changeOrder.qty = new decimal?(d);
                        isSetQty = true;
                    }

                    continue;
                }
                //价格
                if (!temStr.Contains("+") && decimal.TryParse(temStr, out d) && d < 200 && !isSetPrice)
                {
                    changeOrder.price = d;
                    isSetPrice = true;
                    continue;
                }

                string[] arr = temStr.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 1)
                {
                    string result = AnalysisSettle(arr[0]);
                    if (!string.IsNullOrEmpty(result) && !isSetSettle)
                    {
                        changeOrder.SettlType2 = result;

                        result = AnalysisSettle(arr[1]);
                        if (!string.IsNullOrEmpty(result))
                        {
                            changeOrder.SettlType = result;

                            isSetSettle = true;

                            continue;
                        }

                    }
                }
                else
                {
                    string result = AnalysisSettle(arr[0]);
                    if (!string.IsNullOrEmpty(result) && !isSetSettle)
                    {
                        changeOrder.SettlType = changeOrder.SettlType2 = result;

                        isSetSettle = true;

                    }
                }
                //ofr方的交易员
                if (!isOfr)
                {
                    temDt = new DataTable();//1 CRMService.Instance.GetContactByName(temStr, 1);
                    if (temDt.Rows.Count > 0)
                    {
                        string instid = temDt.Rows[0]["instid"] as string;
                        if (!string.IsNullOrEmpty(instid))
                        {
                            DataTable dt = new DataTable();//2 CRMService.Instance.GetInstByIn(string.Format("'{0}'", instid), 1);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                changeOrder.InstitutionID = dt.Rows[0]["idbkey"] + "";
                                changeOrder.InstitutionName = GetInstName(dt.Rows[0], changeOrder.SecurityID);
                                //changeOrder.bankcodeshorta = dt.Rows[0]["shortnamecn"].ToString();
                            }
                        }
                        changeOrder.TraderName = temDt.Rows[0]["namecn"].ToString();
                        changeOrder.TraderID = temDt.Rows[0]["idbkey"] + "";
                        isOfr = true;
                        continue;
                    }
                    else
                    {
                        isOfr = false;
                    }
                }
                //bidbroker
                if (!isBid)
                {
                    DataTable dt = new DataTable();//3 CRMService.Instance.GetBrokerByGoodstype(temStr, "", 1, changeOrder == null ? string.Empty : changeOrder.goodstype);
                    if (dt.Rows.Count > 0)
                    {
                        changeOrder.CounterPartySubName = dt.Rows[0]["AccountName"].ToString();
                        changeOrder.CounterPartySubID = dt.Rows[0]["Idb_Key"] + "";
                        isBid = true;
                        continue;
                    }
                    else
                    {
                        isBid = false;
                    }
                }
            }
            //如果本次未识别则清楚上次识别结果
            if (!isOfr)
            {
                changeOrder.InstitutionID = string.Empty;
                changeOrder.InstitutionName = string.Empty;
                changeOrder.TraderName = string.Empty;
                changeOrder.TraderID = string.Empty;
            }
            if (!isBid)
            {
                changeOrder.CounterPartySubName = string.Empty;
                changeOrder.CounterPartySubID = string.Empty;
            }


            if (!isSetCode)
                changeOrder.SecurityID = changeOrder.goodsShortName = changeOrder.timeToMaturity = string.Empty;
            if (!isSetPrice)
                changeOrder.price = null;
            if (!isSetQty)
                changeOrder.qty = 3000;
            if (!isSetSettle)
                changeOrder.SettlType = changeOrder.SettlType2 = "明天+0";
        }

        public string AnalysisSettle(string str)
        {
            if (str.Contains("+") && !str.Contains("Y") && !str.Contains("A"))
            {
                foreach (string s in DefaultSettleArray)
                {
                    if (s.Equals(str))
                        return str;
                }

                string[] temArr = str.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                if (temArr != null && temArr.Length > 1)
                {
                    DateTime dt;
                    if (DateTime.TryParseExact(temArr[0], "MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                        && (temArr[1].Equals("0") || temArr[1].Equals("1")))
                        return str;
                }
            }


            return string.Empty;
        }
    }
}
