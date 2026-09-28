using com.gateway.log;
using STC.Common.Model.Base;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using STC.PT.Plugin.Utils;
using STC.PT.Plugin.ViewModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommonUtils = STC.Common.Utils.CommonUtils;

namespace STC.PT.Plugin.View.Component
{
    public class CustomExecDataGrid : ExDataGrid
    {
        protected override void OnExecutedCopy(ExecutedRoutedEventArgs args)
        {
            try
            {
                ClientDeal();
            }
            catch (Exception ex)
            {
                LogHelper.Error("CustomExecDataGrid OnExecutedCopy", ex);
            }
        }


        public void copyDeal()
        {
            Sub_copyDealNew(this.SelectedItems as IList);
            //Sub_copyDeal(this.SelectedItems as IList);
        }

        public void Sub_copyDeal(IList datas)
        {
            StringBuilder plainTxt = new StringBuilder();
            IEnumerable<PtExecordersData> list = datas.Cast<PtExecordersData>();
            if (list != null)
            {
                Dictionary<string, List<PTdataCopy>> dic = new Dictionary<string, List<PTdataCopy>>();
                foreach (PtExecordersData data in list)
                {
                    PTdataCopy t = new PTdataCopy();
                    t.securityid = data.SecurityID + "";
                    t.symbol = data.Symbol + "";
                    t.side = data.Side + "";
                    string counterPartyName = data.CounterPartyName + "";//对手方
                    //if (string.IsNullOrEmpty(counterPartyName))
                    //    counterPartyName = "--";
                    t.counterPartyName = counterPartyName;

                    t.counterPartySubName = data.CounterPartySubName + ""; ;

                    string partySubName = data.PartySubName + "";//本方交易员
                    //if (string.IsNullOrEmpty(partySubName))
                    //    partySubName = "--";
                    t.partySubName = partySubName;
                    //t.qty = data[PTExecOrderProcesser.LastQty] + "";
                    //t.lastQty = decimal.Parse(t.qty);
                    //string settle = data[PTExecOrderProcesser.SettlType] + "";
                    //t.settletype = ComboSourceUtil.Instance.FindEnum(settle, "settletype_myorder");
                    string quote = data.Quotemethod + "";//报价方式
                    t.quotemethod = ComboSourceUtil.Instance.FindEnum(quote, "quotemethod_pt");
                    string settle = data.SettlType + "";
                    t.settletype = ComboSourceUtil.Instance.FindEnum(settle, "settletype_ptcopy");
                    string remainTime = data.RemainingTime + "";
                    t.remainTime = remainTime;
                    decimal qty = data.LastQty.ToString().ConvertToDecimal();
                    decimal qty2 = Math.Round(qty);
                    t.qty = qty2 + "";
                    t.lastQty = qty2;

                    decimal price;//data[PTExecOrderProcesser.LastPx].ToString().ConvertToDecimal();
                    if (decimal.TryParse(data.LastPx.ToString(), out price))
                    {
                        t.price = price.ToString("N4");
                    }
                    t.seatNumber = data.Seatnumber + "";
                    t.comment = data.Remark + "";
                    getIsBank(t);
                    t.quoteWay = ComboSourceUtil.Instance.FindEnum(data.Quotemethod + "", "quotemethod_pt");
                    if (t.quoteWay.Contains("待定"))
                    {
                        t.quote = "报价方式待定";
                    }
                    else if (t.quoteWay.Contains("收"))
                    {
                        t.instname = t.counterPartyName;
                        t.quote = string.IsNullOrEmpty(t.quoteWay) ? "" : t.quoteWay.Replace("收", "");
                        t.quoteWay1 = "发";
                    }
                    else if (t.quoteWay.Contains("发"))
                    {
                        t.instname = PTInnerSettingViewModel.Instance.PartyName; //InstitutionName_Edit;
                        t.quote = string.IsNullOrEmpty(t.quoteWay) ? "" : t.quoteWay.Replace("发", "");
                        t.quoteWay1 = "发";
                    }

                    string key = "";
                    if (string.IsNullOrEmpty(t.counterPartyName) || "--".Equals(t.counterPartyName)
                        || (string.IsNullOrEmpty(t.counterPartySubName) || "--".Equals(t.counterPartySubName)))
                    {
                        string id = data.ExecID + "";
                        key = t.securityid + "," + t.side + "," + t.price + "," + t.counterPartyName + "," + t.counterPartySubName + id;
                    }
                    else
                    {
                        key = t.securityid + "," + t.side + "," + t.price + "," + t.counterPartyName + "," + t.counterPartySubName;
                    }
                    if (!dic.ContainsKey(key))
                    {
                        List<PTdataCopy> li = new List<PTdataCopy>();
                        li.Add(t);
                        dic.Add(key, li);
                    }
                    else
                    {
                        dic[key].Add(t);
                    }
                }
                  string tabStr = PTInnerSettingViewModel.Instance.TabStr;
                string PartyName = PTInnerSettingViewModel.Instance.PartyName;
                foreach (KeyValuePair<string, List<PTdataCopy>> kvp in dic)
                {
                    List<PTdataCopy> copyList = kvp.Value;
                    PTdataCopy t = copyList[0];
                    decimal TotalQty = 0;
                    foreach (PTdataCopy v in copyList)
                    {
                        TotalQty += v.lastQty;
                    }
                    string endTxt = string.Empty;
                    //if (t.side == "1")
                    //{
                    //    endTxt = $"{t.securityid} {t.symbol} {t.price}% {TotalQty}w {t.counterPartyName} 出给 {t.partySubName} {t.settletype}";
                    //}
                    //else
                    //    endTxt = $"{t.securityid} {t.symbol} {t.price}% {TotalQty}w {t.partySubName} 出给 {t.counterPartyName} {t.settletype}";
                    if (t.side == "1")
                    {
                        if (t.isBank)//交易所
                        {
                            string xw = "席位号待定";//string.IsNullOrEmpty(t.seatNumber) ? "席位号待定" : t.seatNumber;
                            string yd = string.IsNullOrEmpty(t.comment) ? "约定号待定" : "约定号" + t.comment;
                            string xwNum = t.securityid.EndsWith(".SH") ? PTInnerSettingViewModel.Instance.SHNumber : PTInnerSettingViewModel.Instance.SZNumber;
                            endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{t.counterPartyName}{xw}{tabStr}to{tabStr}{PartyName}{xwNum}{tabStr}{yd}";
                            if (string.IsNullOrEmpty(t.counterPartyName) && string.IsNullOrEmpty(t.counterPartySubName))
                            {
                                endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}对手待定{xw}{tabStr}to{tabStr}{PartyName}{xwNum}{tabStr}{yd}";
                            }
                        }
                        else
                        {
                            endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{t.counterPartyName}-{t.counterPartySubName}{tabStr}to{tabStr}{PartyName}-{t.partySubName}{tabStr}{t.quoteWay1}{t.instname}{t.quote}";
                            if (string.IsNullOrEmpty(t.counterPartyName) && string.IsNullOrEmpty(t.counterPartySubName))
                            {
                                endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}对手待定{tabStr}to{tabStr}{PartyName}-{t.partySubName}{tabStr}{t.quoteWay1}{t.instname}{t.quote}";
                            }
                        }
                    }
                    else
                    {
                        if (t.isBank)//交易所
                        {
                            string xw = "席位号待定";//string.IsNullOrEmpty(t.seatNumber) ? "席位号待定" : t.seatNumber;
                            string yd = string.IsNullOrEmpty(t.comment) ? "约定号待定" : "约定号" + t.comment;
                            string xwNum = t.securityid.EndsWith(".SH") ? PTInnerSettingViewModel.Instance.SHNumber : PTInnerSettingViewModel.Instance.SZNumber;
                            endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{PartyName}{xwNum}{tabStr}to{tabStr}{t.counterPartyName}{xw}{tabStr}{yd}";
                            if (string.IsNullOrEmpty(t.counterPartyName) && string.IsNullOrEmpty(t.counterPartySubName))
                            {
                                endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{PartyName}{xwNum}{tabStr}to{tabStr}对手待定{xw}{tabStr}{yd}";
                            }
                        }
                        else
                        {
                            endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{PartyName}-{t.partySubName}{tabStr}to{tabStr}{t.counterPartyName}-{t.counterPartySubName}{tabStr}{t.quoteWay1}{t.instname}{t.quote}";
                            if (string.IsNullOrEmpty(t.counterPartyName) && string.IsNullOrEmpty(t.counterPartySubName))
                            {
                                endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{PartyName}-{t.partySubName}{tabStr}to{tabStr}对手待定{tabStr}{t.quoteWay1}{t.instname}{t.quote}";
                            }
                        }
                    }
                    if (plainTxt.Length > 0)
                    {
                        plainTxt.Append("\n");
                    }
                    plainTxt.Append(endTxt);
                }

                //复制,不加try会导致程序无法退出
                try
                {
                    if (plainTxt.Length > 0)
                    {
                        Common.Utils.ClipboardHelper.CopyToClipboard(plainTxt.ToString());
                    }
                }
                catch { }
            }
        }

        public async void Sub_copyDealNew(IList datas)
        {
            if (datas == null || datas.Count == 0)
                return;
            try
            {
                // 异步执行耗时操作
                string result = await Task.Run(() => ProcessDealData(datas));
                if (!string.IsNullOrEmpty(result))
                {

                    Common.Utils.ClipboardHelper.CopyToClipboard(result);

                }
            }
            catch (Exception ex)
            {

            }
        }

        private string ProcessDealData(IList datas)
        {
            StringBuilder plainTxt = new StringBuilder();
            IEnumerable<PtExecordersData> list = datas.Cast<PtExecordersData>();
            if (list != null)
            {
                string tabStr = PTInnerSettingViewModel.Instance.TabStr;
                Dictionary<string, List<PTdataCopy>> dic = new Dictionary<string, List<PTdataCopy>>();
                foreach (PtExecordersData data in list)
                {
                    PTdataCopy t = new PTdataCopy();
                    string venues = data.Venues + "";//报价方式
                    t.venues = ComboSourceUtil.Instance.FindEnum(venues, "venues_pt");
                    t.securityid = data.SecurityID + "";
                    t.symbol = data.Symbol + "";
                    t.side = "1".Equals(data.Side) ? "买入" : "卖出";
                    string counterPartyName = data.CounterPartyName + "";//对手方
                    t.counterPartyName = counterPartyName;
                    //t.counterPartySubName = data.CounterPartySubName;
                    t.realcounterpartynameText = data.Realcounterpartyname + ""; ;

                    string partySubName = data.PartySubName + "";//本方交易员
                    t.partySubName = partySubName;

                    if (!string.IsNullOrEmpty(data.TradeDate))
                    {
                        DateTime date = DateTime.ParseExact(data.TradeDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        string tradedate = date.ToString("MM.dd", CultureInfo.InvariantCulture);
                        t.tradedate = tradedate;
                    }
                    string settle = data.SettlType + "";
                    t.settletype = ComboSourceUtil.Instance.FindEnum(settle, "settletype_ptcopy");
                    string remainTime = data.RemainingTime + "";
                    t.remainTime = remainTime;
                    decimal qty = data.LastQty.ToString().ConvertToDecimal();
                    decimal qty2 = Math.Round(qty);
                    t.qty = getQtyText(qty);
                    t.lastQty = qty2;

                    decimal price;//data[PTExecOrderProcesser.LastPx].ToString().ConvertToDecimal();
                    if ("0".Equals(data.IsExeCdcType))
                    {
                        if (decimal.TryParse(data.Yieldtoexecution.ToString(), out price))
                        {
                            t.price = string.Format("{0}行权", price.ToString("N4"));
                        }
                    }
                    else
                    {
                        if (decimal.TryParse(data.LastPx.ToString(), out price))
                        {
                            t.price = price.ToString("N4");
                        }
                    }
                    t.comment = data.Remark + "";
                    //交易商代码
                    if (t.securityid.ToUpper().EndsWith(".SZ"))
                    {
                        StringBuilder codetext = new StringBuilder();
                        if (!string.IsNullOrEmpty(data.Szedealercode))
                        {
                            codetext.Append("交易商代码").Append(data.Szedealercode);
                        }
                        if (!string.IsNullOrEmpty(data.Szetradingentitycode))
                        {
                            codetext.Append(",").Append("交易主体代码").Append(data.Szetradingentitycode);
                        }
                        if (!string.IsNullOrEmpty(data.Szetradercode))
                        {
                            string code = Regex.Match(data.Szetradercode, @"\(([^()]*)\)").Groups[1].Value;
                            string trader = Regex.Replace(data.Szetradercode, @"\([^()]*\)", "").Trim();
                            codetext.Append(",").Append(trader).Append(code);
                        }
                        if (codetext.Length > 0)
                            t.codeText = string.Format("({0})", codetext.ToString());
                    }
                    else if (t.securityid.ToUpper().EndsWith(".SH"))
                    {
                        t.SHNumber = PTExecOrderProcesser.Instance.SHNumber;
                        StringBuilder codetext = new StringBuilder();
                        if (!string.IsNullOrEmpty(data.Sseinstitutioncode))
                        {
                            codetext.Append(data.Sseinstitutioncode);
                        }
                        if (codetext.Length > 0)
                            t.codeText = string.Format("{0}", codetext.ToString());
                        if (!string.IsNullOrEmpty(data.TrdMatchId))
                            t.trdMatchId = "约定号" + data.TrdMatchId;
                    }
                    //发单交易员
                    if (!string.IsNullOrEmpty(data.CounterPartySubName) && t.securityid.ToUpper().EndsWith(".IB") && "1".Equals(data.TrdType))
                    {
                        t.counterPartySubName = data.CounterPartySubName + tabStr;
                    }

                    t.paymentRate = string.IsNullOrEmpty(data.BridgePaymentFixedRate) ? "" : data.BridgePaymentFixedRate + "厘";
                    t.netPrice = data.CleanPrice2 + "";

                    string key = "";
                    if (string.IsNullOrEmpty(t.counterPartyName) || "--".Equals(t.counterPartyName)
                        || (string.IsNullOrEmpty(t.counterPartySubName) || "--".Equals(t.counterPartySubName)))
                    {
                        string id = data.ExecID + "";
                        key = t.securityid + "," + t.side + "," + t.price + "," + t.counterPartyName + "," + t.counterPartySubName + id;
                    }
                    else
                    {
                        key = t.securityid + "," + t.side + "," + t.price + "," + t.counterPartyName + "," + t.counterPartySubName;
                    }
                    if (!dic.ContainsKey(key))
                    {
                        List<PTdataCopy> li = new List<PTdataCopy>();
                        li.Add(t);
                        dic.Add(key, li);
                    }
                    else
                    {
                        dic[key].Add(t);
                    }
                }
                string PartyName = PTInnerSettingViewModel.Instance.PartyName;
                foreach (KeyValuePair<string, List<PTdataCopy>> kvp in dic)
                {
                    List<PTdataCopy> copyList = kvp.Value;
                    PTdataCopy t = copyList[0];
                    decimal TotalQty = 0;
                    foreach (PTdataCopy v in copyList)
                    {
                        TotalQty += v.lastQty;
                    }
                    string endTxt = string.Empty;
                    if (t.side == "买入")
                    {
                        endTxt = $"【{t.venues}】{tabStr}{t.side}{tabStr}{t.remainTime}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.price}{tabStr}{t.qty}{t.qtyTxt}{tabStr}{t.tradedate}{t.settletype}{tabStr}{t.realcounterpartynameText}{tabStr}出给{tabStr}{PartyName}{t.SHNumber}{tabStr}发{t.counterPartyName}{t.codeText}{tabStr}{t.counterPartySubName}{t.paymentRate}{tabStr}净价{t.netPrice}{tabStr}{t.trdMatchId}{tabStr}";
                    }
                    else
                    {
                        endTxt = $"【{t.venues}】{tabStr}{t.side}{tabStr}{t.remainTime}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.price}{tabStr}{t.qty}{t.qtyTxt}{tabStr}{t.tradedate}{t.settletype}{tabStr}{PartyName}{t.SHNumber}{tabStr}出给{tabStr}{t.realcounterpartynameText}{tabStr}发{t.counterPartyName}{t.codeText}{tabStr}{t.counterPartySubName}{t.paymentRate}{tabStr}净价{t.netPrice}{tabStr}{t.trdMatchId}{tabStr}";

                    }
                    if (plainTxt.Length > 0)
                    {
                        plainTxt.Append("\n");
                    }
                    plainTxt.Append(endTxt);
                }
            }
            return plainTxt.ToString();
        }


        public void ClientDeal()
        {
            Sub_copyDealNew(this.SelectedItems as IList);
        }

        public void Sub_ClientDeal(IList datas)
        {
            StringBuilder plainTxt = new StringBuilder();
            IEnumerable<PtExecordersData> list = datas.Cast<PtExecordersData>();
            if (list != null)
            {
                Dictionary<string, List<PTdataCopy>> dic = new Dictionary<string, List<PTdataCopy>>();
                foreach (PtExecordersData data in list)
                {
                    PTdataCopy t = new PTdataCopy();
                    t.securityid = data.SecurityID + "";
                    t.symbol = data.Symbol + "";
                    t.side = data.Side + "";
                    t.counterPartyName = data.CounterPartyName + "";//对手方
                    t.counterPartySubName = data.CounterPartySubName + "";
                    t.partySubName = data.PartySubName + "";//本方交易员
                    //t.qty = data[PTExecOrderProcesser.LastQty] + "";
                    //t.lastQty = decimal.Parse(t.qty);
                    string institutionName = data.InstitutionName + "";
                    //if (string.IsNullOrEmpty(institutionName))
                    //    institutionName = "--";
                    t.institutionName = institutionName; //客户机构

                    t.traderName = data.TraderName + "";//客户
                    //if (string.IsNullOrEmpty(traderName))
                    //    t.traderName = "--";

                    string quote = data.Quotemethod + "";//报价方式
                    t.quotemethod = ComboSourceUtil.Instance.FindEnum(quote, "quotemethod_pt");
                    string settle = data.SettlType + "";
                    t.settletype = ComboSourceUtil.Instance.FindEnum(settle, "settletype_ptcopy");
                    string remainTime = data.RemainingTime + "";
                    t.remainTime = remainTime;
                    decimal qty = data.LastQty.ToString().ConvertToDecimal();
                    decimal qty2 = Math.Round(qty);
                    t.qty = qty2 + "";
                    t.lastQty = qty2;

                    decimal price;//data[PTExecOrderProcesser.LastPx].ToString().ConvertToDecimal();
                    if (decimal.TryParse(data.LastPx.ToString(), out price))
                    {
                        t.price = price.ToString("N4");
                    }
                    t.seatNumber = data.Seatnumber + "";
                    t.comment = data.Remark + "";
                    getIsBank(t);
                    t.quoteWay = ComboSourceUtil.Instance.FindEnum(data.Quotemethod + "", "quotemethod_pt");
                    if (t.quoteWay.Contains("待定"))
                    {
                        t.quote = "报价方式待定";
                    }
                    else if (t.quoteWay.Contains("收"))
                    {
                        t.instname = PTInnerSettingViewModel.Instance.PartyName;
                        t.quote = string.IsNullOrEmpty(t.quoteWay) ? "" : t.quoteWay.Replace("收", "");
                        t.quoteWay1 = "发";
                    }
                    else if (t.quoteWay.Contains("发"))
                    {
                        t.instname = t.institutionName;//InstitutionName_Edit;
                        t.quote = string.IsNullOrEmpty(t.quoteWay) ? "" : t.quoteWay.Replace("发", "");
                        t.quoteWay1 = "发";
                    }
                  
                    string key = "";
                    if (string.IsNullOrEmpty(t.institutionName) || "--".Equals(t.institutionName)
                        || string.IsNullOrEmpty(t.traderName) || "--".Equals(t.traderName))
                    {
                        string id = data.ExecID + "";
                        key = t.securityid + "," + t.side + "," + t.price + "," + t.institutionName + "," + t.traderName + "," + t.quotemethod + id;
                    }
                    else
                    {
                        key = t.securityid + "," + t.side + "," + t.price + "," + t.institutionName + "," + t.traderName + "," + t.quotemethod;
                    }
                    if (!dic.ContainsKey(key))
                    {
                        List<PTdataCopy> li = new List<PTdataCopy>();
                        li.Add(t);
                        dic.Add(key, li);
                    }
                    else
                    {
                        dic[key].Add(t);
                    }
                }
                string tabStr = PTInnerSettingViewModel.Instance.TabStr;
                string PartyName = PTInnerSettingViewModel.Instance.PartyName;
                foreach (KeyValuePair<string, List<PTdataCopy>> kvp in dic)
                {
                    List<PTdataCopy> copyList = kvp.Value;
                    PTdataCopy t = copyList[0];
                    decimal TotalQty = 0;
                    foreach (PTdataCopy v in copyList)
                    {
                        TotalQty += v.lastQty;
                    }
                    string endTxt = string.Empty;


                    //if (t.side == "1")
                    //{
                    //    endTxt = $"{t.securityid} {t.symbol} {t.price}% {TotalQty}w {t.partySubName}({t.quotemethod}) to {t.institutionName} {t.settletype}";
                    //}
                    //else
                    //    endTxt = $"{t.securityid} {t.symbol} {t.price}% {TotalQty}w {t.institutionName} to {t.partySubName}({t.quotemethod}) {t.settletype}";
                   
                    //string instname = PTInnerSettingViewModel.Instance.PartyName;
                    if (t.side == "1")
                    {
                        if (t.isBank)//交易所
                        {
                            string xw = string.IsNullOrEmpty(t.seatNumber) ? "席位号待定" : t.seatNumber;
                            string yd = string.IsNullOrEmpty(t.comment) ? "约定号待定" : "约定号" + t.comment;
                            string xwNum = t.securityid.EndsWith(".SH") ? PTInnerSettingViewModel.Instance.SHNumber : PTInnerSettingViewModel.Instance.SZNumber;
                            endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{PartyName}{xwNum}{tabStr}to{tabStr}{t.institutionName}{xw}{tabStr}{yd}";
                        }
                        else
                        {
                            endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{PartyName}-{LocalEnv.UserInfo.FullName}{tabStr}to{tabStr}{t.institutionName}-{t.traderName}{tabStr}{t.quoteWay1}{t.instname}{t.quote}";
                        }
                    }
                    else
                    {
                        if (t.isBank)//交易所
                        {
                            string xw = string.IsNullOrEmpty(t.seatNumber) ? "席位号待定" : t.seatNumber;
                            string yd = string.IsNullOrEmpty(t.comment) ? "约定号待定" : "约定号" + t.comment;
                            string xwNum = t.securityid.EndsWith(".SH") ? PTInnerSettingViewModel.Instance.SHNumber : PTInnerSettingViewModel.Instance.SZNumber;
                            endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{t.institutionName}{xw}{tabStr}to{tabStr}{PartyName}{xwNum}{tabStr}{yd}";
                        }
                        else
                        {
                            endTxt = $"{t.settletype}{tabStr}{t.securityid}{tabStr}{t.symbol}{tabStr}{t.remainTime}{tabStr}{t.price}%{tabStr}{t.qty}{t.qtyTxt}{tabStr}{t.institutionName}-{t.traderName}{tabStr}to{tabStr}{PartyName}-{LocalEnv.UserInfo.FullName}{tabStr}{t.quoteWay1}{t.instname}{t.quote}";
                        }
                    }


                    if (plainTxt.Length > 0)
                    {
                        plainTxt.Append("\n");
                    }
                    plainTxt.Append(endTxt);
                }

                //复制,不加try会导致程序无法退出
                try
                {
                    if (plainTxt.Length > 0)
                    {
                        Common.Utils.ClipboardHelper.CopyToClipboard(plainTxt.ToString());
                    }
                }
                catch { }
            }
        }


        public void copyId()
        {
            Sub_copyId(this.SelectedItems as IList);
        }

        public void Sub_copyId(IList datas)
        {
            StringBuilder plainTxt = new StringBuilder();
            IEnumerable<PtExecordersData> list = datas.Cast<PtExecordersData>();
            if (list != null)
            {
                foreach (PtExecordersData data in list)
                {
                    if (plainTxt.Length > 0)
                    {
                        plainTxt.Append("\n");
                    }
                    string id = data.Venueexecid + "";
                    plainTxt.Append(id);
                }
                //复制,不加try会导致程序无法退出
                try
                {
                    if (plainTxt.Length > 0)
                    {
                        Common.Utils.ClipboardHelper.CopyToClipboard(plainTxt.ToString());
                    }
                }
                catch { }
            }
        }

        private string Txt_KW = "kw";
        private string Txt_W = "w";
        private string Txt_IRS = "百万";
        private void getIsBank(PTdataCopy t, bool isBillon = false)
        {
            t.isBank = STC.PT.Plugin.Utils.CommonUtils.IsBank(t.securityid);
            if (!t.isBank)
            {
                if (isBillon)
                {
                    t.qtyTxt = Txt_IRS;
                }
                else
                {
                    t.qtyTxt = Txt_KW;
                }
                var x = CommonUtils.GetBondsInfoPoBySecurityID(t.securityid);
                if (x != null)
                {
                    if ((x as BondsInfoPo).MarketIndicator == "2")
                    {
                        t.qtyTxt = Txt_IRS;
                        t.isIRS = true;
                    }
                    else
                    {
                        t.isIRS = false;
                    }
                }
            }
            else
            {
                t.qtyTxt = Txt_W;
            }
            if (Txt_KW.Equals(t.qtyTxt))
            {
                t.qty = (decimal.Parse(t.qty) / 1000).ToString();
            }
        }

        private string getQtyText(decimal qty) 
        {
            string result = "";
            if (qty >= 10000)
            {
                result = decimal.Divide(qty, 10000).ToString("G29") +"E";
            }
            else 
            {
                result = qty.ToString();
            }
            return result;
        }

        public void Sub_CopyData_SecurityID(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();

            IEnumerable<PtExecordersData> list = datas.Cast<PtExecordersData>();
            foreach (PtExecordersData data in list)
            {
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                plainTxt.Append(data.SecurityID);
            }

            try
            {
                if (plainTxt.Length > 0)
                {
                    Common.Utils.ClipboardHelper.CopyToClipboard(plainTxt.ToString());
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        public void CopyData_SecurityID(bool needPrompt = true, string copyMode = "1")
        {
            Sub_CopyData_SecurityID(this.SelectedItems as IList, needPrompt, copyMode);
        }

        public void Sub_CopyData_Symbol(IList datas, bool needPrompt = true, string copyMode = "1")
        {
            StringBuilder plainTxt = new StringBuilder();

            IEnumerable<PtExecordersData> list = datas.Cast<PtExecordersData>();

            foreach (PtExecordersData data in list)
            {
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                plainTxt.Append(data.Symbol);
            }

            try
            {
                if (plainTxt.Length > 0)
                {
                    Common.Utils.ClipboardHelper.CopyToClipboard(plainTxt.ToString());
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }

        public void CopyData_Symbol(bool needPrompt = true, string copyMode = "1")
        {
            Sub_CopyData_Symbol(this.SelectedItems as IList, needPrompt, copyMode);
        }
    }
    
}
