using com.gateway.log;
using STC.Common.Handler.PT;
using STC.Common.Model.Base;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.MyTrading;
using STC.PT.Plugin.Handler.Process;
using STC.PT.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.PT.Plugin.ViewModel.ExecManage
{
    public class ExecHistoryRecordViewModel : ViewModelBase
    {
        #region 定义
        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }
        private ObservableCollection<PtExecordersData> _PTMvtOrders = new ObservableCollection<PtExecordersData>();

        public ObservableCollection<PtExecordersData> PTMvtOrders
        {
            get
            {
                if (_PTMvtOrders == null)
                    _PTMvtOrders = new ObservableCollection<PtExecordersData>();
                return _PTMvtOrders;
            }
            set
            {
                _PTMvtOrders = value;
                this.OnPropertyChanged(nameof(PTMvtOrders));
            }
        }
        private Visibility loadmaskvisible = Visibility.Collapsed;
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public Visibility LoadMaskVisible
        {
            get
            {
                return loadmaskvisible;
            }

            set
            {
                loadmaskvisible = value;
                this.OnPropertyChanged(nameof(LoadMaskVisible));
            }
        }
        public static List<KeyValuePair<string, string>> MatchStatusSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("matchstatus");
            }
        }
        public static List<KeyValuePair<string, string>> SendstatusSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Sendstatus");
            }
        }
        public static List<KeyValuePair<string, string>> SideItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Side");
            }
        }
        #endregion

        public void Init(string execid)
        {
            PTMvtOrders.Clear();
            this.Mv.Source = PTMvtOrders;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;

            this.Mv.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
            try
            {
                ThreadPoolHelper.InvokeThreadPool((o) =>
                {
                        PtExecOrderQueryReq req = new PtExecOrderQueryReq();
                        req.execid = execid;
                        ExecOrderQuery(req);
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("ExecuterOrderHandler init:", e);
            }
        }
        public async void ExecOrderQuery(PtExecOrderQueryReq req)
        {
            try
            {
                bool flag = true;

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    while (flag)
                    {
                        PtExecordersReplyPo reply = PTHandler.Instance.ExecOrderMvtQuery(req);
                        if (reply == null || reply.Datas == null)
                        {
                            flag = false;
                            break;
                        }
                        if (reply != null && reply.Datas != null)
                        {
                            foreach (TbookPtExecorders eo in reply.Datas)
                            {
                                OnOrder(eo);
                            }
                            if (reply.Datas.Count < req.pageSize)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    OrderByUpdateTime();
                }));

            }
            catch (Exception e)
            {
                LogHelper.Error("ExecOrderQuery error:", e);
            }
            finally
            {
            }
        }
        private void OrderByUpdateTime()
        {
            var sortItems = new ObservableCollection<PtExecordersData>(PTMvtOrders.OrderByDescending(item => item.UpdateTime));
            PTMvtOrders.Clear();
            for (int i = 0; i < sortItems.Count; i++)
            {
                PtExecordersData po = sortItems[i];
                if (i == sortItems.Count - 1) 
                {
                    po.CloseByIsHighlighted = false;
                    po.DelflagIsHighlighted = false;
                    po.SideIsHighlighted = false;
                    po.PriceIsHighlighted = false;
                    po.BridgePFRateIsHighlighted = false;
                    po.CouponrateIsHighlighted = false;
                    po.Yield2IsHighlighted = false;
                    po.Yieldtoexe2IsHighlighted = false;
                    po.CleanPrice2IsHighlighted = false;
                    po.LastQtyIsHighlighted = false;
                    po.MatchQtyIsHighlighted = false;
                    po.SettlAmountIsHighlighted = false;
                    po.SettlTypeIsHighlighted = false;
                    po.CounterPartyNameIsHighlighted = false;
                    po.CounterPartySubNameIsHighlighted = false;
                    po.TrdMatchIdIsHighlighted = false;
                    po.RealcounterIsHighlighted = false;
                    po.MatchStatusIsHighlighted = false;
                    po.SendstatusIsHighlighted = false;
                    po.Operationtype = "新增";
                } 
                else 
                { 
                    PtExecordersData next = sortItems[i+1];
                    po.CloseByIsHighlighted = (!string.IsNullOrEmpty(po.CloseBy) && !string.IsNullOrEmpty(next.CloseBy) && !po.CloseBy.Equals(next.CloseBy));
                    po.DelflagIsHighlighted = (!string.IsNullOrEmpty(po.Delflag) && !string.IsNullOrEmpty(next.Delflag) && !po.Delflag.Equals(next.Delflag));
                    po.SideIsHighlighted = (!string.IsNullOrEmpty(po.Side) && !string.IsNullOrEmpty(next.Side) && !po.Side.Equals(next.Side)); 
                    po.PriceIsHighlighted = (!string.IsNullOrEmpty(po.LastPx_Display) && !string.IsNullOrEmpty(next.LastPx_Display) && !po.LastPx_Display.Equals(next.LastPx_Display)); 
                    po.BridgePFRateIsHighlighted = (!string.IsNullOrEmpty(po.BridgePaymentFixedRate) && !string.IsNullOrEmpty(next.BridgePaymentFixedRate) && !po.BridgePaymentFixedRate.Equals(next.BridgePaymentFixedRate));
                    po.CouponrateIsHighlighted = (!string.IsNullOrEmpty(po.Couponrate) && !string.IsNullOrEmpty(next.Couponrate) && !po.Couponrate.Equals(next.Couponrate)); 
                    po.Yield2IsHighlighted = po.Yield2 != next.Yield2; 
                    po.Yieldtoexe2IsHighlighted = po.Yieldtoexecution2 != next.Yieldtoexecution2; 
                    po.CleanPrice2IsHighlighted = po.CleanPrice2 != next.CleanPrice2; 
                    po.LastQtyIsHighlighted = (!string.IsNullOrEmpty(po.LastQty) && !string.IsNullOrEmpty(next.LastQty) && !po.LastQty.Equals(next.LastQty)); 

                    po.MatchQtyIsHighlighted = (!string.IsNullOrEmpty(po.MatchQty) && !string.IsNullOrEmpty(next.MatchQty) && !po.MatchQty.Equals(next.MatchQty));
                    po.SettlAmountIsHighlighted = po.SettlAmount != next.SettlAmount;
                    po.SettlTypeIsHighlighted = (!string.IsNullOrEmpty(po.SettlType) && !string.IsNullOrEmpty(next.SettlType) && !po.SettlType.Equals(next.SettlType));
                    po.CounterPartyNameIsHighlighted = (!string.IsNullOrEmpty(po.CounterPartyName) && !string.IsNullOrEmpty(next.CounterPartyName) && !po.CounterPartyName.Equals(next.CounterPartyName));
                    po.CounterPartySubNameIsHighlighted = (!string.IsNullOrEmpty(po.CounterPartySubName) && !string.IsNullOrEmpty(next.CounterPartySubName) && !po.CounterPartySubName.Equals(next.CounterPartySubName));
                    po.TrdMatchIdIsHighlighted = (!string.IsNullOrEmpty(po.TrdMatchId) && !string.IsNullOrEmpty(next.TrdMatchId) && !po.TrdMatchId.Equals(next.TrdMatchId));
                    po.RealcounterIsHighlighted = (!string.IsNullOrEmpty(po.Realcounterpartyname) && !string.IsNullOrEmpty(next.Realcounterpartyname) && !po.Realcounterpartyname.Equals(next.Realcounterpartyname));
                    po.MatchStatusIsHighlighted = (!string.IsNullOrEmpty(po.Matchstatus) && !string.IsNullOrEmpty(next.Matchstatus) && !po.Matchstatus.Equals(next.Matchstatus));
                    po.SendstatusIsHighlighted = (!string.IsNullOrEmpty(po.Sendstatus) && !string.IsNullOrEmpty(next.Sendstatus) && !po.Sendstatus.Equals(next.Sendstatus));
                    po.Operationtype = "要素变更";
                }
                PTMvtOrders.Add(po);
            }
            /*foreach (var item in sortItems)
            {
                PTMvtOrders.Add(item);
            }*/
        }
        public async Task<PtExecordersReplyPo> ExecOrderQueryAsync(PtExecOrderQueryReq req)
        {
            PtExecordersReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = PTHandler.Instance.ExecOrderQuery(req);
            });

            return reply;
        }
        public void OnOrder(TbookPtExecorders order)
        {
            try
            {
                PtExecordersData dataRow;
                    dataRow = this.PTMvtOrders.FirstOrDefault(o => o.ID == order.Id);
                bool isold = false;
                if (dataRow != null)
                {
                    isold = true;
                }
                else
                {
                    dataRow = new PtExecordersData();
                    dataRow.ID = order.Id;
                }

                dataRow.ExecID = order.Execid;
                dataRow.ClOrdID = order.Clordid;
                dataRow.Matchstatus = order.Matchstatus;
                dataRow.Relatestatus = order.Relatestatus;
                dataRow.Sendstatus = order.Sendstatus;
                dataRow.SecurityID = order.Securityid;
                dataRow.Side = order.Side;
                dataRow.LastPx = string.IsNullOrEmpty(order.Yield) ? "0" : order.Yield;
                dataRow.LastPx_Display = Common.Utils.CommonUtils.FormatPrice(order.Yield + "");// 用于显示的成交价
                dataRow.NetPrice = Common.Utils.CommonUtils.ConvertToString(order.Netprice);
                dataRow.LastQty = Common.Utils.CommonUtils.ConvertToString(order.Qty);
                dataRow.TradeDate = order.Tradedate;//order.Settldate;
                dataRow.ExecutionTime = order.Transacttime;
                dataRow.SettlType = string.IsNullOrEmpty(order.Settltype) ? "" : order.Settltype;
                dataRow.StrategyOrderId = order.Strategyorderid;
                dataRow.Venueexecid = order.Venueexecid;
                dataRow.Venueid = order.Venueid;
                dataRow.Symbol = order.Symbol;
                dataRow.Symboltype = "BNC".Equals(order.Symboltype)? "利率债" : "信用债";
                dataRow.Tag1 = order.Tag1;
                dataRow.InstitutionName = order.Institutionname;
                dataRow.InstitutionID = order.Institutionid;
                dataRow.TraderName = order.Tradername;
                dataRow.TraderID = order.Traderid;
                dataRow.Venuetypegw = order.Venuetypegw;
                dataRow.Venues = order.Venues;
                dataRow.Quotemethod = order.Quotemethod;
                dataRow.CounterPartyId = order.Counterpartyid;
                dataRow.CounterPartyName = order.Counterpartyname;
                dataRow.CounterPartySubId = order.Counterpartysubid;
                dataRow.CounterPartySubName = order.Counterpartysubname;
                dataRow.PartySubName = order.Partysubname;
                dataRow.AccountId = order.Accountid;
                dataRow.AccountName = order.Accountname;
                dataRow.Remark = order.Remark;
                dataRow.UpdateTime = order.Updatetime;
                dataRow.ShowSendBtn_FQ = (dataRow.ShowSendBtn == "0" && LocalEnv.ConfInfo.IsFQ) ? "0" : "1";
                dataRow.Seatnumber = order.Seatnumber;
                dataRow.Approvalstatus = order.Approvalstatus;
                dataRow.Approvalorderid = order.Approvalorderid;
                dataRow.Approvaltime = order.Approvaltime;
                dataRow.Approvalterminal = order.Approvalterminal;
                dataRow.IsMe = order.Userid == LocalEnv.UserInfo.UserId;
                dataRow.Username = order.Username;
                dataRow.Settldate = order.Settldate;
                if (!string.IsNullOrEmpty(order.Inf1))
                {
                    if (JsonSplit.IsJson(order.Inf1))
                    {
                        ParserInf inf1po = CommonUtils.DeserializeObject<ParserInf>(order.Inf1);
                        if (inf1po != null)
                        {
                            dataRow.Sign = inf1po.Sign;
                            dataRow.Sign1 = inf1po.Sign1;
                            dataRow.Sign2 = inf1po.Sign2;
                            dataRow.Sign3 = inf1po.Sign3;
                        }
                    }
                }
                dataRow.MaturityDays = order.MaturityDays;
                dataRow.IssuerCode = order.IssuerCode;
                dataRow.WarranterCode = order.WarranterCode;
                dataRow.DayOffStr = order.dayOffStr;
                dataRow.Blocknum = order.Blocknum;
                dataRow.Createtime = order.Createtime;
                dataRow.BusinessType = order.BusinessType;
                dataRow.Bookid = order.Bookid;
                dataRow.BookName = order.BookName;
                dataRow.MarketIndicator = order.Marketindicator;
                dataRow.Offsetflag = order.Offsetflag;
                dataRow.Yieldtoexecution = Common.Utils.CommonUtils.ConvertToString(order.Yieldtoexecution);
                dataRow.FullPrice = Common.Utils.CommonUtils.ConvertToString(order.Dirtyprice);
                dataRow.Realcounterpartyname = order.Realcounterpartyname;
                dataRow.TrdSubType = order.TrdSubType;
                dataRow.TrdType = order.TrdType;
                dataRow.SettlAmount = order.SettlAmount != null ? order.SettlAmount.Value : decimal.Zero;
                dataRow.BridgePaymentFixedRate = order.BridgePaymentFixedRate;
                dataRow.Couponrate = order.Couponrate; 
                dataRow.Yield2 = order.Yield2 !=null ? order.Yield2.Value:decimal.Zero;
                dataRow.CleanPrice2 = order.CleanPrice2 != null ? order.CleanPrice2.Value : decimal.Zero;
                dataRow.TrdMatchId = order.TrdMatchId;
                dataRow.CloseBy = order.CloseBy;
                dataRow.Operationtype = "DistFQ".Equals(order.Venuetypegw)?"接口":"";
                dataRow.Delflag = order.Delflag;
                dataRow.Entrytype = "0".Equals(order.Entrytype) ? "接口" :"手工";
                dataRow.Yieldtoexecution2 = order.Yieldtoexecution2 != null ? order.Yieldtoexecution2.Value : decimal.Zero;
                dataRow.FullPrice2 = order.FullPrice2 != null ? order.FullPrice2.Value : decimal.Zero;
                dataRow.AllocText = order.AllocText;
                //成交日期？？？？

                if (string.IsNullOrEmpty(order.BookName) && !string.IsNullOrEmpty(order.Bookid) && PTExecOrderProcesser.Instance.BookAllDict.ContainsKey(order.Bookid))
                {
                    string bookName = "";
                    if (PTExecOrderProcesser.Instance.BookAllDict.TryGetValue(order.Bookid, out bookName))
                    {
                        dataRow.BookName = bookName;
                    }
                }
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(order.Securityid);
                if (bondsInfoPo != null)
                {
                    if (string.IsNullOrEmpty(bondsInfoPo.RemaingingTime) && "4".Equals(bondsInfoPo.MarketIndicator))
                    {
                        dataRow.RemainingTime = "已到期";
                    }
                    else
                    {
                        dataRow.RemainingTime = bondsInfoPo.RemaingingTime;
                    }
                }
                dataRow.Riskflag = order.Riskflag;
                dataRow.RiskRemark = order.Riskremark;
                dataRow.Partymanagername = order.Partymanagername;
                dataRow.Partymanagerid = order.Partymanagerid;
                dataRow.Sseinstitutioncode = order.Sseinstitutioncode;
                dataRow.Szedealercode = order.Szedealercode;
                dataRow.Szetradingentitycode = order.Szetradingentitycode;
                dataRow.Szetradercode = order.Szetradercode;
                if (!isold)
                {
                    PTMvtOrders.Insert(0, dataRow);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("OnOrder", e);
            }
        }
    }
}
