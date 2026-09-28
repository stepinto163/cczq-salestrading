using com.gateway.log;
using STC.Common.Model.Execute;
using STC.Common.Utils;
using STC.Execute.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ComboSourceUtil = STC.Execute.Plugin.Utils.ComboSourceUtil;
using CommonUtils = STC.Execute.Plugin.Utils.CommonUtils;

namespace STC.Execute.Plugin.Handler.Process
{
    public class ExecOrderProcesser
    {
        private ExecOrderProcesser() { }
        private static ExecOrderProcesser _execOrderProcesser = new ExecOrderProcesser();

        private bool isSubscribeForInnerOrder = false;
        public static ExecOrderProcesser Instance { get { return _execOrderProcesser; } }
        public event Action<DataRow> OnDetailOrder;
        public const string _ModelId = "5000";
        public const string _Code = "5001";
        public string[] SettleTypeENUM = { "+0", "+1" };
        #region 表格字段

        public const string AllocQty = "AllocQty";
        public const string AvgPx = "AvgPx";//成交价
        public const string AvgPx2 = "AvgPx2";//到期收益率
        public const string BrokerExecID = "BrokerExecID";
        public const string PartyID = "PartyID";
        public const string PartyName = "PartyName";
        public const string PartySubID = "PartySubID";
        public const string PartySubName = "PartySubName";
        public const string CounterPartyName = "CounterPartyName";
        public const string CounterPartySubName = "CounterPartySubName";
        public const string DeliveryType = "DeliveryType";
        public const string IndividualAllocID = "IndividualAllocID";
        public const string MarketIndicator = "MarketIndicator";
        public const string MaturityDate = "MaturityDate";
        public const string OnBehalfOfCompID = "OnBehalfOfCompID";
        public const string SecondaryTradeID = "SecondaryTradeID";
        public const string SecurityID = "SecurityID";
        public const string SecurityType = "SecurityType";
        public const string SendTime = "SendTime";
        public const string SettlDate = "SettlDate";
        public const string SettlType = "SettlType";
        public const string Side = "Side";
        public const string Symbol = "Symbol";
        public const string TradeDate = "TradeDate";
        public const string TradeID = "TradeID";
        public const string TradeMatchTimestamp = "TradeMatchTimestamp";
        public const string TradeRequestID = "TradeRequestID";
        public const string TransType = "TransType";
        public const string TransactTime = "TransactTime";
        public const string TrdMatchID = "TrdMatchID";
        public const string TrdSubType = "TrdSubType";
        public const string TrdType = "TrdType";
        public const string AlloAccount = "AlloAccount";
        public const string TradeStat = "TradeStat";
        public const string UpdateFields = "UpdateFields";
        public const string CreateTime = "CreateTime";
        public const string CreateTime_Sort = "CreateTime_Sort";
        public const string UpdateTime = "UpdateTime";
        public const string TradeType = "TradeType";
        public const string CleanPrice = "CleanPrice";
        public const string DirtyPrice = "DirtyPrice";
        public const string AllocText = "AllocText";
        public const string Text = "Text";
        public const string StipulationValue = "StipulationValue";
        public const string Yield = "Yield";
        public const string CounterPartyID = "CounterPartyID";
        public const string CounterPartySubID = "CounterPartySubID";
        public const string BridgePaymentFixedRate = "BridgePaymentFixedRate";
        public const string PaymentFixedRate = "PaymentFixedRate";
        public const string PaymentAmount = "PaymentAmount";
        public const string TransactionPriceType = "TransactionPriceType";
        public const string Remark = "Remark";
        public const string EditRemark = "EditRemark";
        public const string CloseBy = "CloseBy";
        public const string EditBtnVisible = "EditBtnVisible";
        public const string SaveBtnVisible = "SaveBtnVisible";
        public const string ChangeFlag = "ChangeFlag";
        public const string ChangeMsg = "ChangeMsg";
        public const string IsDelete = "IsDelete";

        public const string Sender = "Sender";
        public const string UserName = "UserName";
        //是否有操作权限
        public const string ConfirmEnable = "ConfirmEnable";
        public const string ConfirmID = "ConfirmID";
        //为啥要加 isShow  是因为自动确认如果是开着的时候  我这边直接给你是自动确认状态  你那边无法定位是否展示  所以需要加这个字段
        public const string IsShow = "IsShow";
        //推送 isShow=1 && tradeType=1 历史记录不展示  其他都展示
        //查询历史记录 tradeType=1 && tradeStat=1不要展示
        public const string IsOwn = "IsOwn";
        #endregion


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

        /// <summary>
        /// 我的订单数据源
        /// </summary>
        private DataTable _ExeOrdersDT;
        public DataTable ExeOrdersDT
        {
            get
            {
                lock (_lockObj)
                {
                    if (_ExeOrdersDT == null)
                    {
                        _ExeOrdersDT = new DataTable();
                        _ExeOrdersDT.Columns.Add(AllocQty, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(AvgPx, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(AvgPx2, typeof(string));
                        _ExeOrdersDT.Columns.Add(BrokerExecID, typeof(string));
                        _ExeOrdersDT.Columns.Add(PartyID, typeof(string));
                        _ExeOrdersDT.Columns.Add(PartyName, typeof(string));
                        _ExeOrdersDT.Columns.Add(PartySubID, typeof(string));
                        _ExeOrdersDT.Columns.Add(PartySubName, typeof(string));
                        _ExeOrdersDT.Columns.Add(CounterPartyName, typeof(string));
                        _ExeOrdersDT.Columns.Add(CounterPartySubName, typeof(string));
                        _ExeOrdersDT.Columns.Add(DeliveryType, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(IndividualAllocID, typeof(string));
                        _ExeOrdersDT.Columns.Add(MarketIndicator, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(MaturityDate, typeof(string));
                        _ExeOrdersDT.Columns.Add(OnBehalfOfCompID, typeof(string));
                        _ExeOrdersDT.Columns.Add(SecondaryTradeID, typeof(string));
                        _ExeOrdersDT.Columns.Add(SecurityID, typeof(string));
                        _ExeOrdersDT.Columns.Add(SecurityType, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(SendTime, typeof(string));
                        _ExeOrdersDT.Columns.Add(SettlDate, typeof(string));
                        _ExeOrdersDT.Columns.Add(SettlType, typeof(string));
                        _ExeOrdersDT.Columns.Add(Side, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(Symbol, typeof(string));
                        _ExeOrdersDT.Columns.Add(TradeDate, typeof(string));
                        _ExeOrdersDT.Columns.Add(TradeRequestID, typeof(string));
                        _ExeOrdersDT.Columns.Add(TradeID, typeof(string));
                        _ExeOrdersDT.Columns.Add(TradeMatchTimestamp, typeof(string));
                        _ExeOrdersDT.Columns.Add(TransType, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(TransactTime, typeof(string));
                        _ExeOrdersDT.Columns.Add(TrdMatchID, typeof(string));
                        _ExeOrdersDT.Columns.Add(TrdSubType, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(TrdType, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(TradeStat, typeof(string));
                        _ExeOrdersDT.Columns.Add(AlloAccount, typeof(string));
                        _ExeOrdersDT.Columns.Add(UpdateFields, typeof(string)); 
                        _ExeOrdersDT.Columns.Add(CreateTime, typeof(string));
                        _ExeOrdersDT.Columns.Add(CreateTime_Sort, typeof(string));
                        _ExeOrdersDT.Columns.Add(UpdateTime, typeof(string));
                        _ExeOrdersDT.Columns.Add(TradeType, typeof(string));

                        _ExeOrdersDT.Columns.Add(CleanPrice, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(DirtyPrice, typeof(decimal));
                        _ExeOrdersDT.Columns.Add(AllocText, typeof(string));
                        _ExeOrdersDT.Columns.Add(Text, typeof(string));
                        _ExeOrdersDT.Columns.Add(StipulationValue, typeof(string));
                        _ExeOrdersDT.Columns.Add(Yield, typeof(string));
                        _ExeOrdersDT.Columns.Add(CounterPartyID, typeof(string));
                        _ExeOrdersDT.Columns.Add(CounterPartySubID, typeof(string));
                        _ExeOrdersDT.Columns.Add(BridgePaymentFixedRate, typeof(string));
                        _ExeOrdersDT.Columns.Add(PaymentFixedRate, typeof(string));
                        _ExeOrdersDT.Columns.Add(TransactionPriceType, typeof(string));
                        _ExeOrdersDT.Columns.Add(Remark, typeof(string));
                        _ExeOrdersDT.Columns.Add(EditRemark, typeof(string));
                        _ExeOrdersDT.Columns.Add(CloseBy, typeof(string));
                        _ExeOrdersDT.Columns.Add(EditBtnVisible, typeof(bool));
                        _ExeOrdersDT.Columns[EditBtnVisible].DefaultValue = true;//创建表格把了附一个默认值
                        _ExeOrdersDT.Columns.Add(SaveBtnVisible, typeof(bool));
                        _ExeOrdersDT.Columns[SaveBtnVisible].DefaultValue = false;
                        _ExeOrdersDT.Columns.Add(ChangeFlag, typeof(string));
                        _ExeOrdersDT.Columns.Add(ChangeMsg, typeof(string));
                        _ExeOrdersDT.Columns.Add(IsDelete, typeof(string));
                        _ExeOrdersDT.Columns.Add(ConfirmEnable, typeof(bool));
                        _ExeOrdersDT.Columns[ConfirmEnable].DefaultValue = false;
                        _ExeOrdersDT.Columns.Add(ConfirmID, typeof(string));
                        _ExeOrdersDT.Columns.Add(UserName, typeof(string));
                        _ExeOrdersDT.Columns.Add(PaymentAmount, typeof(string));
                        _ExeOrdersDT.Columns.Add(IsShow, typeof(string));
                        _ExeOrdersDT.Columns.Add(IsOwn, typeof(bool));

                        _ExeOrdersDT.PrimaryKey = new DataColumn[] { _ExeOrdersDT.Columns[SecondaryTradeID] };

                        _ExeOrdersDT.DefaultView.Sort = $"{CreateTime_Sort} desc";
                    }
                    return this._ExeOrdersDT;
                }
            }
        }

        //public void tempOnOrder(ExecuteOrder executerOrder)
        //{
        //    OnOrder(executerOrder);
        //}

        public void OnOrder(ExecuteOrder executerOrder, bool isSel)
        {
            try
            {
                lock (_lockObj)
                {
                    if (_ExeOrdersDT == null)
                        return;
                    DataRow dataRow = _ExeOrdersDT.Rows.Find(executerOrder.SecondaryTradeID);

                    if (dataRow == null)
                    {
                        dataRow = _ExeOrdersDT.NewRow();
                        dataRow[SecondaryTradeID] = executerOrder.SecondaryTradeID;

                        _ExeOrdersDT.Rows.InsertAt(dataRow, 0);
                        //_ExeOrdersDT.AcceptChanges();
                    }
                    //else
                    //{
                    //    if ("4".Equals(executerOrder.ordStatus))
                    //    {
                    //        this._ExeOrdersDT.Rows.Remove(dataRow);
                    //          dataRow.AcceptChanges();
                    //    }
                    //}

                    

                    dataRow[AllocQty] = CommonUtils.FormatNum0(executerOrder.AllocQty.ToString());
                    if (executerOrder.AvgPx.HasValue)
                        dataRow[AvgPx] = CommonUtils.FormatNum(executerOrder.AvgPx.ToString());
                    else if(executerOrder.StipulationValue.HasValue)
                        dataRow[AvgPx] = CommonUtils.FormatNum(executerOrder.StipulationValue.ToString());
                    dataRow[AvgPx2] = executerOrder.AvgPx.HasValue ? CommonUtils.FormatNum(executerOrder.AvgPx.ToString()) : "";
                    dataRow[BrokerExecID] = executerOrder.BrokerExecID;
                    dataRow[PartyID] = executerOrder.PartyID;
                    dataRow[PartyName] = executerOrder.PartyName;
                    dataRow[PartySubID] = executerOrder.PartySubID;
                    dataRow[PartySubName] = executerOrder.PartySubName;
                    dataRow[CounterPartyName] = executerOrder.CounterPartyName;
                    dataRow[CounterPartySubName] = executerOrder.CounterPartySubName;
                    dataRow[DeliveryType] = executerOrder.DeliveryType;
                    dataRow[IndividualAllocID] = string.IsNullOrWhiteSpace(executerOrder.IndividualAllocID) ? "" : executerOrder.IndividualAllocID;
                    dataRow[MarketIndicator] = executerOrder.MarketIndicator;
                    dataRow[MaturityDate] = parseCreateDate(executerOrder.MaturityDate);
                    dataRow[OnBehalfOfCompID] = executerOrder.OnBehalfOfCompID;// ComboSourceUtil.Instance.FindEnum(executerOrder.OnBehalfOfCompID, "venues");

                    dataRow[SecurityID] = executerOrder.SecurityID;
                    dataRow[SecurityType] = executerOrder.SecurityType;
                    dataRow[SendTime] = executerOrder.SendTime;
                    dataRow[SettlDate] = parseCreateDate(executerOrder.SettlDate);
                    dataRow[SettlType] = executerOrder.SettlType;// ComboSourceUtil.Instance.FindEnum(executerOrder.SettlType.ToString(), "settletype_myOrder");
                    dataRow[Side] = executerOrder.Side;
                    dataRow[Symbol] = executerOrder.Symbol;
                    dataRow[TradeDate] = !string.IsNullOrEmpty(executerOrder.TradeDate) && executerOrder.TradeDate.Length > 7 ? (executerOrder.TradeDate.Insert(4, "/").Insert(7, "/")) : executerOrder.TradeDate;
                    dataRow[TradeRequestID] = executerOrder.TradeRequestID;
                    dataRow[TradeID] = executerOrder.TradeID;
                    dataRow[TradeMatchTimestamp] = executerOrder.TradeMatchTimestamp;
                    dataRow[TransType] = executerOrder.TransType;
                    dataRow[TransactTime] = parseCreateDate(executerOrder.TransactTime);
                    dataRow[TrdMatchID] = executerOrder.TrdMatchID;
                    dataRow[TrdSubType] = executerOrder.TrdSubType;
                    dataRow[TrdType] = executerOrder.TrdType; //ComboSourceUtil.Instance.FindEnum(executerOrder.TrdType.ToString(), "transactionPriceType");
                    dataRow[UpdateFields] = executerOrder.UpdateFields;
                    dataRow[CreateTime] = executerOrder.CreateTime.Replace("-", "/");//parseUpdateDate(executerOrder.CreateTime);
                    dataRow[CreateTime_Sort] = executerOrder.CreateTime;
                    dataRow[UpdateTime] = parseUpdateDate(executerOrder.UpdateTime);
                    dataRow[TradeType] = executerOrder.TradeType;
                    dataRow[AlloAccount] = executerOrder.AlloAccount;


                    dataRow[CleanPrice] = CommonUtils.FormatNum(executerOrder.CleanPrice.ToString());
                    dataRow[DirtyPrice] = CommonUtils.FormatNum(executerOrder.DirtyPrice.ToString());

                    dataRow[AllocText] = SetAllocText(executerOrder);//string.Format("{0} {1}", executerOrder.IndividualAllocID, executerOrder.AllocText);
                    dataRow[Text] = executerOrder.Text;
                    dataRow[StipulationValue] = executerOrder.StipulationValue.HasValue ? executerOrder.StipulationValue.ToString() : "";
                    dataRow[Yield] = executerOrder.Yield.HasValue ? executerOrder.Yield.ToString() : "";
                    if (!string.IsNullOrEmpty(executerOrder.TransactionPriceType))
                        dataRow[TransactionPriceType] = ComboSourceUtil.Instance.FindEnum(executerOrder.TransactionPriceType.ToString(), "transactionPriceType");//executerOrder.TransactionPriceType;

                    dataRow[CounterPartyID] = executerOrder.CounterPartyID;
                    dataRow[CounterPartySubID] = executerOrder.CounterPartySubID;
                    dataRow[BridgePaymentFixedRate] = executerOrder.BridgePaymentFixedRate.HasValue ? executerOrder.BridgePaymentFixedRate.Value + "" : "";
                    dataRow[PaymentFixedRate] = executerOrder.PaymentFixedRate.HasValue ? executerOrder.PaymentFixedRate.Value + "" : "";
                    dataRow[PaymentAmount] = executerOrder.PaymentAmount.HasValue ? CommonUtils.FormatNum(executerOrder.PaymentAmount.ToString()) : "";
                    dataRow[Remark] = executerOrder.remark;
                    dataRow[EditRemark] = executerOrder.remark;
                    dataRow[CloseBy] = executerOrder.closeBy;
                    dataRow[UserName] = executerOrder.UserName;
                    dataRow[EditBtnVisible] = true;
                    dataRow[SaveBtnVisible] = false;
                    if (executerOrder.TradeStat != "0")
                        dataRow[ConfirmEnable] = false;
                    else
                        dataRow[ConfirmEnable] = true;
                    if (!string.IsNullOrEmpty(executerOrder.ordStatus) && "4".Equals(executerOrder.ordStatus))
                    {
                        dataRow[IsDelete] = "1";
                        dataRow[TradeStat] = "已删除";
                        dataRow[IsOwn] = false;
                        dataRow[ConfirmEnable] = false;
                        dataRow[ChangeFlag] = "0";
                    }
                    else
                    {
                        dataRow[IsDelete] = "0";
                        dataRow[TradeStat] = executerOrder.TradeStat;// ComboSourceUtil.Instance.FindEnum(executerOrder.TradeStat, "tradeStat");
                        if (LocalEnv.UserInfo.UserName.Equals(executerOrder.UserName))
                            dataRow[IsOwn] = true;
                        else
                        {
                            dataRow[ConfirmEnable] = false;
                            dataRow[IsOwn] = false;
                        }

                        //修改标记 已删除不展示；未删除展示
                        //if (isUpdate)
                        //{
                        StringBuilder sb = setChange(executerOrder.UpdateFields);
                        if (sb != null && sb.Length > 0)
                        {
                            sb.Append("已修改");
                            dataRow[ChangeFlag] = "1";
                            dataRow[ChangeMsg] = sb.ToString();
                        }
                        else
                        {
                            dataRow[ChangeFlag] = "0";
                            dataRow[ChangeMsg] = "";
                        }
                        //}
                    }
                   

                    
                    dataRow[ConfirmID] = executerOrder.ConfirmID;
                    dataRow[IsShow] = executerOrder.IsShow;
                    dataRow.AcceptChanges();
                    if (!isSel)
                    {
                        if (dataRow != null)
                            OnDetailOrder?.Invoke(dataRow);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("OnOrder", e);
            }
        }

        private bool checkVal(string oldVal, string newVal)
        {
            return oldVal == newVal;
        }
        public StringBuilder setChange(string UpdateFields)
        {
            if (string.IsNullOrEmpty(UpdateFields))//|| !UpdateFields.Contains(","))
                return null;
            StringBuilder sb = new StringBuilder();
            string[] str = UpdateFields.Split(',');
            //sb.Append("修改 ");
            foreach (string s in str)
            {
                switch (s)
                {
                    //case "TransType":
                    //    sb.Append("交易状态 ");
                    //    break;
                    case "SecurityID":
                        sb.Append("债券代码 ");
                        break;
                    case "AllocQty":
                        sb.Append("成交量 ");
                        break;
                    case "AvgPx":
                    case "StipulationValue":
                    case "CleanPrice":
                    case "DirtyPrice":
                        if (!sb.ToString().Contains("成交价"))
                            sb.Append("成交价 ");
                        break;
                    case "CounterPartyName":
                        sb.Append("对手方 ");
                        break;
                    case "CounterPartySubName":
                        sb.Append("对手方交易员 ");
                        break;
                    case "TrdSubType":
                        sb.Append("方向说明 ");
                        break;
                    case "TrdType":
                        sb.Append("报价方式 ");
                        break;
                    case "TradeType":
                        sb.Append("交易类型 ");
                        break;
                    //case "CleanPrice":
                    //    sb.Append("净价 "); 
                    //    break;
                    //case "DirtyPrice":
                    //    sb.Append("全价 ");
                    //    break;
                    case "TransactionPriceType":
                        sb.Append("价格备注 ");
                        break;
                    case "IndividualAllocID":
                    case "IndividualAllocID2":
                    case "IndividualAllocID3":
                    case "AllocText":
                    case "AllocText2":
                    case "AllocText3":
                    case "BridgePaymentAmount":
                    case "BridgePaymentFixedRate":
                    case "BridgePaymentUnitOfMeasure":
                    case "PaymentAmount":
                    case "PaymentFixedRate":
                    case "PaymentUnitOfMeasure":
                    case "BrokerPaymentAmount":
                    case "BrokerPaymentRate":
                    case "BrokerPaymentUnitOfMeasure":
                        if (!sb.ToString().Contains("经纪商备注"))
                            sb.Append("经纪商备注 ");
                        break;
                    //case "AvgPx":
                    //    sb.Append("到期收益率 ");
                    //    break;
                    //case "StipulationValue":
                    //    sb.Append("行权收益率 ");
                    //    break;
                    case "TradeDate":
                        sb.Append("交易日期 ");
                        break;
                    case "SettlType":
                        sb.Append("清算速度 ");
                        break;
                }
            }
            return sb;
        }
        //private void setChange(DataRow dataRow, ExecuteOrder executerOrder)
        //{

            //    StringBuilder sb = new StringBuilder();
            //    if (!checkVal(dataRow[SecurityID] + "", executerOrder.SecurityID + ""))
            //    {
            //        sb.Append("债券代码 ");
            //    }
            //    if (!checkVal(dataRow[AllocQty] + "", CommonUtils.FormatNum(executerOrder.AllocQty + "")))
            //    {
            //        sb.Append("券面总额 ");
            //    }
            //    if (!checkVal(dataRow[AvgPx] + "", CommonUtils.FormatNum(executerOrder.AvgPx + "")))
            //    {
            //        sb.Append("成交价 ");
            //    }
            //    if (!checkVal(dataRow[CounterPartyName] + "", executerOrder.CounterPartyName))
            //    {
            //        sb.Append("对手方 ");
            //    }
            //    if (!checkVal(dataRow[CounterPartySubName] + "", executerOrder.CounterPartySubName))
            //    {
            //        sb.Append("对手方交易员 ");
            //    }
            //    if (!checkVal(dataRow[TrdSubType] + "", executerOrder.TrdSubType + ""))
            //    {
            //        sb.Append("方向说明 ");
            //    }
            //    if (!checkVal(dataRow[TrdType] + "", executerOrder.TrdType + ""))
            //    {
            //        sb.Append("报价方式 ");
            //    }
            //    if (!checkVal(dataRow[TradeType] + "", executerOrder.TradeType + ""))
            //    {
            //        sb.Append("交易类型 ");
            //    }
            //    string clean = CommonUtils.FormatNum(executerOrder.CleanPrice + "");
            //    if (!checkVal(dataRow[CleanPrice] + "", clean))
            //    {
            //        sb.Append("净价 ");
            //    }
            //    string dirty = CommonUtils.FormatNum(executerOrder.DirtyPrice + "");
            //    if (!checkVal(dataRow[DirtyPrice] + "", dirty))
            //    {
            //        sb.Append("全价 ");
            //    }
            //    if (!checkVal(dataRow[AllocText] + "", string.Format("{0} {1}", executerOrder.IndividualAllocID, executerOrder.AllocText)))
            //    {
            //        sb.Append("经纪商备注 ");
            //    }
            //    if (!checkVal(dataRow[Yield] + "", executerOrder.Yield + ""))
            //    {
            //        sb.Append("到期收益率 ");
            //    }
            //    if (!checkVal(dataRow[StipulationValue] + "", executerOrder.StipulationValue + ""))
            //    {
            //        sb.Append("行权收益率 ");
            //    }
            //    string tradeDate = !string.IsNullOrEmpty(executerOrder.TradeDate) && executerOrder.TradeDate.Length > 7 ? (executerOrder.TradeDate.Insert(4, "/").Insert(7, "/")) : executerOrder.TradeDate;
            //    if (!checkVal(dataRow[TradeDate] + "", tradeDate))
            //    {
            //        sb.Append("交易日期 ");
            //    }
            //    if (!checkVal(dataRow[SettlType] + "", executerOrder.SettlType + ""))
            //    {
            //        sb.Append("清算速度 ");
            //    }

            //    if (sb.Length > 0)
            //    {
            //        sb.Append("已改变");

            //        dataRow[ChangeFlag] = "1";
            //        dataRow[ChangeMsg] = sb.ToString();
            //    }
            //}

        private string parseCreateDate(string time)
        {
            if (string.IsNullOrEmpty(time))
                return time;
            string result = string.Empty;
            string[] arr = time.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 0)
            {
                result = arr[0];
                result = result.Length > 7 ? (result.Insert(4, "/").Insert(7, "/")) : result;
                
            }
            return result;
        }

        public string parseUpdateDate(string time)
        {
            string result = string.Empty;
            string[] arr = time.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                result = arr[0];
                result = result.Length > 7 ? (result.Insert(4, "/").Insert(7, "/")) : result;
                result += " " + arr[1];
            }
            return result;
        }

        public string SetAllocText(ExecuteOrder executerOrder)
        {
            string temp = "";
            if (!string.IsNullOrWhiteSpace(executerOrder.IndividualAllocID) || !string.IsNullOrWhiteSpace(executerOrder.AllocText))
                temp += $"{executerOrder.IndividualAllocID} {executerOrder.AllocText};";
            if (!string.IsNullOrWhiteSpace(executerOrder.IndividualAllocID2) || !string.IsNullOrWhiteSpace(executerOrder.AllocText2))
                temp += $"{executerOrder.IndividualAllocID2} {executerOrder.AllocText2};";
            if (!string.IsNullOrWhiteSpace(executerOrder.IndividualAllocID3) || !string.IsNullOrWhiteSpace(executerOrder.AllocText3))
                temp += $"{executerOrder.IndividualAllocID3} {executerOrder.AllocText3};";
            //中介费、代付费是看费率字段   返点是看 amount
            if (executerOrder.BridgePaymentFixedRate.HasValue)
            {
                temp += $"代付费 {CommonUtils.FormatNum(executerOrder.BridgePaymentFixedRate.ToString())}厘;";//{ComboSourceUtil.Instance.FindEnum(executerOrder.BridgePaymentUnitOfMeasure, "unit")};";
            }
            if (executerOrder.PaymentAmount.HasValue)
            {
                temp += $"返费 {CommonUtils.FormatNum(executerOrder.PaymentAmount.ToString())}元;";
            }
            if (executerOrder.BrokerPaymentRate.HasValue)
            {
                temp += $"中介费 {CommonUtils.FormatNum(executerOrder.BrokerPaymentRate.ToString())}厘;";//{ComboSourceUtil.Instance.FindEnum(executerOrder.BrokerPaymentUnitOfMeasure, "unit")};";
            }
            if (!string.IsNullOrWhiteSpace(temp))
            {
                temp = temp.TrimEnd(';');
            }
            temp = temp.Replace(",", ";");
            return temp;
        }

        public string getSettleType(string trade, string settle)
        {
            if (!SettleTypeENUM.Contains(settle))
                return settle;
            string today = DateTime.Now.ToString("yyyy/MM/dd");
            if (trade.Length != 10)
                return settle;
            if (today.Equals(trade))
                return settle;
            trade = trade.Replace("/", ".").Substring(5, 5);
            settle = trade + settle;
            return settle;
        }


        public void clear()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (this._lockObj)
                {
                    if (_ExeOrdersDT != null)
                    {
                        _ExeOrdersDT.Rows.Clear();
                    }
                }
            }));
        }
    }
}
