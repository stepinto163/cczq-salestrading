using com.gateway.log;
using STC.Common.Handler.CDH;
using STC.Common.Handler.PT;
using STC.Common.Model.Admin;
using STC.Common.Model.Execute;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.PT.Plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
//using ComboSourceUtil = STC.PT.Plugin.Utils.ComboSourceUtil;
//using CommonUtils = STC.PT.Plugin.Utils.CommonUtils;

namespace STC.PT.Plugin.Handler.Process
{
    public class PTExecOrderProcesser
    {
        private PTExecOrderProcesser() { }
        private static PTExecOrderProcesser _execOrderProcesser = new PTExecOrderProcesser();
        public static PTExecOrderProcesser Instance { get { return _execOrderProcesser; } }
        public string[] SettleTypeENUM = { "+0", "+1" };
        /// <summary>
        /// 我的订单资源锁
        /// </summary>
        private object _lockObj = new object();
        public object LockObj
        {
            get
            {
                return _lockObj;
            }
        }

        public static List<KeyValuePair<string, string>> SettletypeItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("settletype");
            }
        }

        #region 表格字段
        public const string ClOrdID = "ClOrdID";
        public const string ExecID = "ExecID";
        public const string Matchstatus = "Matchstatus";
        public const string Relatestatus = "Relatestatus";
        public const string Sendstatus = "Sendstatus";
        public const string Securityid = "Securityid";
        public const string Symbol = "Symbol";
        public const string Symboltype = "Symboltype";
        public const string Tag1 = "Tag1";
        public const string Venuetypegw = "Venuetypegw";
        public const string Venues = "Venues";
        public const string Side = "Side";
        public const string LastPx = "LastPx";
        public const string DISPLAY_LASTPX_FIELD = "LastPx_Display";
        public const string NetPrice = "NetPrice";
        public const string LastQty = "LastQty";
        public const string TradeDate = "TradeDate";
        public const string ExecutionTime = "ExecutionTime";
        public const string SettlType = "SettlType";
        public const string StrategyOrderId = "StrategyOrderId";

        public const string InstitutionName = "InstitutionName";
        public const string TraderName = "TraderName";
        public const string InstitutionID = "InstitutionID";
        public const string TraderID = "TraderID";
        public const string Quotemethod = "Quotemethod";//报价方式
        public const string CounterPartyName = "CounterPartyName";//对手方
        public const string CounterPartySubName = "CounterPartySubName";//对手交易员
        public const string CounterPartyid = "CounterPartyid";//对手方
        public const string CounterPartySubid = "CounterPartySubid";//对手交易员
        public const string PartySubName = "PartySubName";//本方交易员


        public const string AccountId = "AccountId";
        public const string AccountName = "AccountName";
        public const string Remark = "Remark";
        public const string Venueexecid = "Venueexecid";
        public const string Venueid = "Venueid";
        public const string UpdateTime = "UpdateTime";
        public const string ShowDeleteBtn = "ShowDeleteBtn";
        public const string ShowSendBtn = "ShowSendBtn";
        public const string ShowEditBtn = "ShowEditBtn";
        public const string Seatnumber = "seatnumber";//席位号
        public const string RemainingTime = "RemainingTime";//剩余期限
        public const string Approvalstatus = "Approvalstatus";//审批状态
        public const string IsMe = "IsMe";
        public const string Sign = "Sign";
        public const string Sign1 = "Sign1";
        public const string Sign2 = "Sign2";
        public const string Sign3 = "Sign3";
        public const string SignDic = "SignDic";
        public const string SignDic1 = "SignDic1";
        public const string SignDic2 = "SignDic2";
        public const string SignDic3 = "SignDic3";
        public const string MaturityDays = "MaturityDays";//剩余期限
        public const string BusinessType = "BusinessType";//业务类别
        public const string Bookid = "Bookid";
        public const string Bookname = "Bookname";
        public Dictionary<string, string> BookDict = new Dictionary<string, string>();
        public Dictionary<string, string> BookAllDict = new Dictionary<string, string>();
        public ObservableCollection<TBookBookTreeItemModel> BookList = new ObservableCollection<TBookBookTreeItemModel>();
        public string partyId = "";
        public string partyName = "";
        public string SZNumber = "";
        public string SHNumber = "";
        public List<StcTbookUsers> UserList = new List<StcTbookUsers>();
        public List<StcTbookUsers> InvestorsList = new List<StcTbookUsers>();

        //private DataTable _ExeOrdersDT;
        //public DataTable ExeOrdersDT
        //{
        //    get
        //    {
        //        lock (_lockObj)
        //        {
        //            if (_ExeOrdersDT == null)
        //            {
        //                _ExeOrdersDT = new DataTable();
        //                _ExeOrdersDT.Columns.Add(ClOrdID, typeof(string));
        //                _ExeOrdersDT.Columns.Add(ExecID, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Matchstatus, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Relatestatus, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Sendstatus, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Securityid, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Side, typeof(string));
        //                _ExeOrdersDT.Columns.Add(LastPx, typeof(decimal));
        //                _ExeOrdersDT.Columns.Add(DISPLAY_LASTPX_FIELD, typeof(string));
        //                _ExeOrdersDT.Columns.Add(NetPrice, typeof(string));
        //                _ExeOrdersDT.Columns.Add(LastQty, typeof(string));
        //                _ExeOrdersDT.Columns.Add(TradeDate, typeof(string));
        //                _ExeOrdersDT.Columns.Add(ExecutionTime, typeof(string));
        //                _ExeOrdersDT.Columns.Add(SettlType, typeof(string));
        //                _ExeOrdersDT.Columns.Add(StrategyOrderId, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Venueexecid, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Venueid, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Symbol, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Symboltype, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Tag1, typeof(string));
        //                _ExeOrdersDT.Columns.Add(InstitutionName, typeof(string));
        //                _ExeOrdersDT.Columns.Add(TraderName, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Venuetypegw, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Venues, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Quotemethod, typeof(string));
        //                _ExeOrdersDT.Columns.Add(CounterPartyName, typeof(string));
        //                _ExeOrdersDT.Columns.Add(CounterPartySubName, typeof(string));
        //                _ExeOrdersDT.Columns.Add(PartySubName, typeof(string));
        //                _ExeOrdersDT.Columns.Add(AccountId, typeof(string));
        //                _ExeOrdersDT.Columns.Add(AccountName, typeof(string));
        //                _ExeOrdersDT.Columns.Add(Remark, typeof(string));
        //                _ExeOrdersDT.Columns.Add(UpdateTime, typeof(string));
        //                _ExeOrdersDT.DefaultView.Sort = $"{UpdateTime} desc";
        //                _ExeOrdersDT.DefaultView.Table.PrimaryKey = new DataColumn[] { _ExeOrdersDT.Columns[ExecID] };
        //            }
        //            return this._ExeOrdersDT;
        //        }
        //    }
        //}
        #endregion


        //public Dictionary<string, string> sideDic = new Dictionary<string, string>();
        //public Dictionary<string, string> wayDic = new Dictionary<string, string>();
        //private bool isInit = false;
        //public void Init()
        //{
        //    ThreadPool.QueueUserWorkItem(o =>
        //    {
        //        try
        //        {
        //            if (!isInit)
        //            {
        //                sideDic.Add("Bid", "1");
        //                sideDic.Add("Ofr", "2");

        //                wayDic.Add("待定", "0");
        //                wayDic.Add("发请求", "1");
        //                wayDic.Add("收请求", "2");
        //                wayDic.Add("发对话", "3");
        //                wayDic.Add("收对话", "4");
        //                //ExecOrderQuery();

        //                isInit = true;
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            LogHelper.Error("ExecuterOrderHandler init:", e);
        //        }
        //    });
        //}


        //private void Instance_OnExec(List<TbookPtExecorders> list)
        //{
        //    foreach (TbookPtExecorders eo in list)
        //    {
        //        OnOrder(eo);
        //        //_CachExecDic.AddOrUpdate(eo.Execid, eo, (key, value) => eo);
        //    }
        //}

        //public void OnOrder(TbookPtExecorders order)
        //{
        //    try
        //    {
        //        lock (_lockObj)
        //        {
        //            if (_ExeOrdersDT == null)
        //                return;
        //            DataRow dataRow = _ExeOrdersDT.Rows.Find(order.Execid);
        //            if (dataRow == null)
        //            {
        //                dataRow = _ExeOrdersDT.NewRow();
        //                dataRow[ExecID] = order.Execid;
        //                _ExeOrdersDT.Rows.InsertAt(dataRow, 0);
        //            }
        //            else
        //            {
        //                if ("1".Equals(order.Delflag))
        //                {//删除
        //                    this._ExeOrdersDT.Rows.Remove(dataRow);
        //                    //dataRow.AcceptChanges();
        //                    return;
        //                }
        //            }
        //            dataRow[ClOrdID] = order.Clordid;
        //            dataRow[ExecID] = order.Execid;
        //            dataRow[Matchstatus] = order.Matchstatus;
        //            dataRow[Relatestatus] = order.Relatestatus;
        //            dataRow[Sendstatus] = order.Sendstatus;
        //            dataRow[Securityid] = order.Securityid;
        //            dataRow[Side] = order.Side;
        //            dataRow[LastPx] = string.IsNullOrEmpty(order.Yield) ? "0" : order.Yield;
        //            dataRow[DISPLAY_LASTPX_FIELD] = Common.Utils.CommonUtils.FormatPrice(order.Yield + "");// 用于显示的成交价
        //            dataRow[NetPrice] = order.Netprice;
        //            dataRow[LastQty] = order.Qty;
        //            dataRow[TradeDate] = order.Settldate;
        //            dataRow[ExecutionTime] = order.Transacttime;
        //            dataRow[SettlType] = order.Settltype;
        //            dataRow[StrategyOrderId] = order.Strategyorderid;
        //            dataRow[Venueexecid] = order.Venueexecid;
        //            dataRow[Venueid] = order.Venueid;
        //            dataRow[Symbol] = order.Symbol;
        //            dataRow[Symboltype] = order.Symboltype;
        //            dataRow[Tag1] = order.Tag1;
        //            dataRow[InstitutionName] = order.Institutionname;
        //            dataRow[TraderName] = order.Tradername;
        //            dataRow[Venuetypegw] = order.Venuetypegw;
        //            dataRow[Venues] = order.Venues;
        //            dataRow[Quotemethod] = order.Quotemethod;
        //            dataRow[CounterPartyName] = order.Counterpartyname;
        //            dataRow[CounterPartySubName] = order.Counterpartysubname;
        //            dataRow[PartySubName] = order.Partysubname;
        //            dataRow[AccountId] = order.Accountid;
        //            dataRow[AccountName] = order.Accountname;
        //            dataRow[Remark] = order.Remark;

        //            dataRow.AcceptChanges();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogHelper.Error("OnOrder", e);
        //    }
        //}


        //public TbookPtExecorders GetExecByGroupExid(string exid)
        //{
        //    if (_CachExecDic != null)
        //    {
        //        return _CachExecDic[exid];
        //    }
        //    return null;
        //}


        //public void clear()
        //{
        //    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    //{
        //        lock (this._lockObj)
        //        {
        //            if (_ExeOrdersDT != null)
        //            {
        //                _ExeOrdersDT.Rows.Clear();
        //            }
        //        }
        //    //}));
        //}
    }
}
