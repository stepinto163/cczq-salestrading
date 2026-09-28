using STC.Common.Handler.Ptr;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Execute;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Execute.Plugin.Handler.Process;
using STC.Execute.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ComboSourceUtil = STC.Execute.Plugin.Utils.ComboSourceUtil;
using CommonUtils = STC.Execute.Plugin.Utils.CommonUtils;

namespace STC.Execute.Plugin.ViewModel.Execute
{
    public class ExecuteOrderDetailViewModel : BaseViewModel
    {
        private object _lockObj = new object();
        public ICommand ConfirmCommand { get; set; }

        #region 字段

        private bool _confirmEnable = false;
        public bool ConfirmEnable { get { return _confirmEnable; } set { _confirmEnable = value; this.OnPropertyChanged(); } }
        private bool _isOwn = false;
        public bool IsOwn { get { return _isOwn; } set { _isOwn = value; this.OnPropertyChanged(); } }

        private string _allocQty;
        public string AllocQty { get { return _allocQty; } set { _allocQty = value; this.OnPropertyChanged(); } }
        private string _avgPx;
        public string AvgPx { get { return _avgPx; } set { _avgPx = value; this.OnPropertyChanged(); } }
        private string _avgPx2;
        public string AvgPx2 { get { return _avgPx2; } set { _avgPx2 = value; this.OnPropertyChanged(); } }
        private string _brokerExecID;
        public string BrokerExecID { get { return _brokerExecID; } set { _brokerExecID = value; this.OnPropertyChanged(); } }
        private string _partyID;
        public string PartyID { get { return _partyID; } set { _partyID = value; this.OnPropertyChanged(); } }
        private string _partyName;
        public string PartyName { get { return _partyName; } set { _partyName = value; this.OnPropertyChanged(); } }
        private string _partySubID;
        public string PartySubID { get { return _partySubID; } set { _partySubID = value; this.OnPropertyChanged(); } }
        private string _partySubName;
        public string PartySubName { get { return _partySubName; } set { _partySubName = value; this.OnPropertyChanged(); } }
        private string _counterPartyName;
        public string CounterPartyName { get { return _counterPartyName; } set { _counterPartyName = value; this.OnPropertyChanged(); } }
        private string _counterPartySubName;
        public string CounterPartySubName { get { return _counterPartySubName; } set { _counterPartySubName = value; this.OnPropertyChanged(); } }
        private string _deliveryType;
        public string DeliveryType { get { return _deliveryType; } set { _deliveryType = value; this.OnPropertyChanged(); } }
        private string _individualAllocID;
        public string IndividualAllocID { get { return _individualAllocID; } set { _individualAllocID = value; this.OnPropertyChanged(); } }
        private string _marketIndicator;
        public string MarketIndicator { get { return _marketIndicator; } set { _marketIndicator = value; this.OnPropertyChanged(); } }
        private string _maturityDate;
        public string MaturityDate { get { return _maturityDate; } set { _maturityDate = value; this.OnPropertyChanged(); } }
        private string _onBehalfOfCompID;
        public string OnBehalfOfCompID { get { return _onBehalfOfCompID; } set { _onBehalfOfCompID = value; this.OnPropertyChanged(); } }
        private string _secondaryTradeID;
        public string SecondaryTradeID { get { return _secondaryTradeID; } set { _secondaryTradeID = value; this.OnPropertyChanged(); } }
        private string _securityID;
        public string SecurityID { get { return _securityID; } set { _securityID = value; this.OnPropertyChanged(); } }
        private string _securityType;
        public string SecurityType { get { return _securityType; } set { _securityType = value; this.OnPropertyChanged(); } }
        private string _sendTime;
        public string SendTime { get { return _sendTime; } set { _sendTime = value; this.OnPropertyChanged(); } }
        private string _settlDate;
        public string SettlDate { get { return _settlDate; } set { _settlDate = value; this.OnPropertyChanged(); } }
        private string _settlType;
        public string SettlType { get { return _settlType; } set { _settlType = value; this.OnPropertyChanged(); } }
        private string _side;
        public string Side { get { return _side; } set { _side = value; this.OnPropertyChanged(); } }
        private string _symbol;
        public string Symbol { get { return _symbol; } set { _symbol = value; this.OnPropertyChanged(); } }
        private string _tradeDate;
        public string TradeDate { get { return _tradeDate; } set { _tradeDate = value; this.OnPropertyChanged(); } }
        private string _tradeRequestID;
        public string TradeRequestID { get { return _tradeRequestID; } set { _tradeRequestID = value; this.OnPropertyChanged(); } }
        private string _tradeID;
        public string TradeID { get { return _tradeID; } set { _tradeID = value; this.OnPropertyChanged(); } }
        private string _tradeMatchTimestamp;
        public string TradeMatchTimestamp { get { return _tradeMatchTimestamp; } set { _tradeMatchTimestamp = value; this.OnPropertyChanged(); } }
        private string _transType;
        public string TransType { get { return _transType; } set { _transType = value; this.OnPropertyChanged(); } }
        private string _transasctTime;
        public string TransactTime { get { return _transasctTime; } set { _transasctTime = value; this.OnPropertyChanged(); } }
        private string _trdMatchID;
        public string TrdMatchID { get { return _trdMatchID; } set { _trdMatchID = value; this.OnPropertyChanged(); } }
        private string _trdSubType;
        public string TrdSubType { get { return _trdSubType; } set { _trdSubType = value; this.OnPropertyChanged(); } }
        private string _trdType;
        public string TrdType { get { return _trdType; } set { _trdType = value; this.OnPropertyChanged(); } }
        private string _updateFields;
        public string UpdateFields { get { return _updateFields; } set { _updateFields = value; this.OnPropertyChanged(); } }
        private string _updateTime;
        public string UpdateTime { get { return _updateTime; } set { _updateTime = value; this.OnPropertyChanged(); } }
        private string _createTime;
        public string CreateTime { get { return _createTime; } set { _createTime = value; this.OnPropertyChanged(); } }        private string _tradeType;
        public string TradeType { get { return _tradeType; } set { _tradeType = value; this.OnPropertyChanged(); } }
        private string _allocText;
        public string AllocText { get { return _allocText; } set { _allocText = value; this.OnPropertyChanged(); } }

        private string _dirtyPrice;
        public string DirtyPrice { get { return _dirtyPrice; } set { _dirtyPrice = value; this.OnPropertyChanged(); } }
        private string _cleanPrice;
        public string CleanPrice { get { return _cleanPrice; } set { _cleanPrice = value; this.OnPropertyChanged(); } }
        private string _Text;
        public string Text { get { return _Text; } set { _Text = value; this.OnPropertyChanged(); } }
        private string _StipulationValue;
        public string StipulationValue { get { return _StipulationValue; } set { _StipulationValue = value; this.OnPropertyChanged(); } }
        private string _Yield;
        public string Yield { get { return _Yield; } set { _Yield = value; this.OnPropertyChanged(); } }
        private string _CounterPartyID;
        public string CounterPartyID { get { return _CounterPartyID; } set { _CounterPartyID = value; this.OnPropertyChanged(); } }
        private string _CounterPartySubID;
        public string CounterPartySubID { get { return _CounterPartySubID; } set { _CounterPartySubID = value; this.OnPropertyChanged(); } }
        private string _BridgePaymentFixedRate;
        public string BridgePaymentFixedRate { get { return _BridgePaymentFixedRate; } set { _BridgePaymentFixedRate = value; this.OnPropertyChanged(); } }
        private string _PaymentFixedRate;
        public string PaymentFixedRate { get { return _PaymentFixedRate; } set { _PaymentFixedRate = value; this.OnPropertyChanged(); } }
        private string _Remark;
        public string Remark { get { return _Remark; } set { _Remark = value; this.OnPropertyChanged(); } }
        private string _EditRemark;
        public string EditRemark { get { return _EditRemark; } set { _EditRemark = value; this.OnPropertyChanged(); } }
        private string _CloseBy;
        public string CloseBy { get { return _CloseBy; } set { _CloseBy = value; this.OnPropertyChanged(); } }
        private bool _EditBtnVisible;
        public bool EditBtnVisible { get { return _EditBtnVisible; } set { _EditBtnVisible = value; this.OnPropertyChanged(); } }
        private bool _SaveBtnVisible;
        public bool SaveBtnVisible { get { return _SaveBtnVisible; } set { _SaveBtnVisible = value; this.OnPropertyChanged(); } }
        private string _confirmID;
        public string ConfirmID { get { return _confirmID; } set { _confirmID = value; this.OnPropertyChanged(); } }
        private string _paymentAmount;
        public string PaymentAmount { get { return _paymentAmount; } set { _paymentAmount = value; this.OnPropertyChanged(); } }
        private string _transactionPriceType;
        public string TransactionPriceType { get { return _transactionPriceType; } set { _transactionPriceType = value; this.OnPropertyChanged(); } }
        


        public ICommand EditRemarkCommand { get; set; }
        public ICommand SaveRemarkCommand { get; set; }
        public ICommand CancelRemarkCommand { get; set; }
        public ExecuteOrderDetailViewModel()
        {
            EditRemarkCommand = new RelayCommand(EditRemarkClick);
            SaveRemarkCommand = new RelayCommand(SaveRemarkClick);
            CancelRemarkCommand = new RelayCommand(CancelRemarkClick);
            ConfirmCommand = new RelayCommand(this.ConfirmClick);
            ExecOrderProcesser.Instance.OnDetailOrder += Instance_OnDetailOrder;   
    }

        public void setStatus(bool status)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                EditBtnVisible = status;
                SaveBtnVisible = status?false:true;
            }));
        }
        public void EditRemarkClick()
        {
            setStatus(false);
        }

        public void SaveRemarkClick()
        {
            if (string.IsNullOrEmpty(SecondaryTradeID))
                return;
            ThreadPool.QueueUserWorkItem(o =>
            {
                UpdateRemarkRequest req = new UpdateRemarkRequest();
                req.remark = EditRemark;
                req.secondaryTradeID = SecondaryTradeID;
                PResult result = PtrHandler.Instance.updateTradeRemark(req);
                if (result != null && result.Code.Equals("0000"))
                {
                    Remark = EditRemark;
                    setStatus(true);
                }
            });
        }
        
        public void CancelRemarkClick()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                EditRemark = Remark;
                setStatus(true);
            }));
        }
        private bool checkData()
        {
            bool status = false;
            return status;
        }

        #endregion

        public void ClearUI()
        {
            lock (_lockObj)
            {
                AllocQty = string.Empty;
                AvgPx = string.Empty;
                AvgPx2 = string.Empty;
                BrokerExecID = string.Empty;
                PartyID = string.Empty;
                PartyName = string.Empty;
                PartySubID = string.Empty;
                PartySubName = string.Empty;
                CounterPartyName = string.Empty;
                CounterPartySubName = string.Empty;
                DeliveryType = string.Empty;
                IndividualAllocID = string.Empty;
                MarketIndicator = string.Empty;
                MaturityDate = string.Empty;
                OnBehalfOfCompID = string.Empty;
                SecondaryTradeID = string.Empty;
                SecurityID = string.Empty;
                SecurityType = string.Empty;
                SendTime = string.Empty;
                SettlDate = string.Empty;
                SettlType = string.Empty;
                Side = string.Empty;
                Symbol = string.Empty;
                TradeDate = string.Empty;
                TradeRequestID = string.Empty;
                TradeID = string.Empty;
                TradeMatchTimestamp = string.Empty;
                TransType = string.Empty;
                TransactTime = string.Empty;
                TrdMatchID = string.Empty;
                TrdSubType = string.Empty;
                TrdType = string.Empty;
                UpdateFields = string.Empty;
                UpdateTime = string.Empty;
                CreateTime = string.Empty;
                TradeType = string.Empty;
                AllocText = string.Empty;
                DirtyPrice = string.Empty;
                CleanPrice = string.Empty;
                Text = string.Empty;
                StipulationValue = string.Empty;
                CounterPartyID = string.Empty;
                CounterPartySubID = string.Empty;
                BridgePaymentFixedRate = string.Empty;
                PaymentFixedRate = string.Empty;
                Remark = string.Empty;
                EditRemark = string.Empty;
                CloseBy = string.Empty;
                EditBtnVisible = true;
                SaveBtnVisible = false;
                ConfirmID = string.Empty;
                PaymentAmount = string.Empty;
                IsOwn = false;
                TransactionPriceType = string.Empty;
            }
        }

        public void OnOrder(DataRow dataRowView)
        {
            try
            {
                lock (_lockObj)
                {
                    if (dataRowView != null)
                    {
                        AllocQty = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.AllocQty].ToString());
                        AvgPx = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.AvgPx].ToString());
                        AvgPx2 = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.AvgPx2].ToString());
                        BrokerExecID = dataRowView[ExecOrderProcesser.BrokerExecID].ToString();
                        PartyID = dataRowView[ExecOrderProcesser.PartyID].ToString();
                        PartyName = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.PartyName].ToString());
                        PartySubID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.PartySubID].ToString());
                        PartySubName = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.PartySubName].ToString());
                        CounterPartyName = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.CounterPartyName].ToString());
                        CounterPartySubName = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.CounterPartySubName].ToString());
                        DeliveryType = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.DeliveryType].ToString());
                        IndividualAllocID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.IndividualAllocID].ToString());
                        MarketIndicator = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.MarketIndicator].ToString());
                        MaturityDate = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.MaturityDate].ToString());
                        OnBehalfOfCompID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.OnBehalfOfCompID].ToString());
                        SecondaryTradeID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.SecondaryTradeID].ToString());
                        SecurityID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.SecurityID].ToString());
                        SecurityType = ComboSourceUtil.Instance.FindEnum(dataRowView[ExecOrderProcesser.SecurityType].ToString(), "securitytype");
                        SendTime = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.SendTime].ToString());
                        SettlDate = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.SettlDate].ToString());
                        SettlType = ComboSourceUtil.Instance.FindEnum(dataRowView[ExecOrderProcesser.SettlType].ToString(), "settletype_execute");
                        Side = ComboSourceUtil.Instance.FindEnum(dataRowView[ExecOrderProcesser.Side].ToString(), "side");
                        Symbol = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.Symbol].ToString());
                        TradeDate = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.TradeDate].ToString());
                        TradeRequestID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.TradeRequestID].ToString());
                        TradeID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.TradeID].ToString());
                        TradeMatchTimestamp = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.TradeMatchTimestamp].ToString());
                        TransType = dataRowView[ExecOrderProcesser.TransType].ToString();
                        TransactTime = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.TransactTime].ToString());
                        TrdMatchID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.TrdMatchID].ToString());
                        TrdSubType = dataRowView[ExecOrderProcesser.TrdSubType].ToString();
                        TrdType = ComboSourceUtil.Instance.FindEnum(dataRowView[ExecOrderProcesser.TrdType].ToString(), "trdType");
                        UpdateFields = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.UpdateFields].ToString());
                        CreateTime = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.CreateTime].ToString());
                        UpdateTime = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.UpdateTime].ToString());
                        TradeType = ComboSourceUtil.Instance.FindEnum(CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.TradeType].ToString()), "exeType");

                        AllocText = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.AllocText].ToString());
                        DirtyPrice = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.DirtyPrice].ToString());
                        CleanPrice = dataRowView[ExecOrderProcesser.CleanPrice].ToString();
                        Text = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.Text].ToString());
                        StipulationValue = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.StipulationValue].ToString());
                        Yield = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.Yield].ToString());
                        CounterPartyID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.CounterPartyID].ToString());
                        CounterPartySubID = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.CounterPartySubID].ToString());
                        BridgePaymentFixedRate = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.BridgePaymentFixedRate].ToString());
                        PaymentFixedRate = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.PaymentFixedRate].ToString());
                        Remark = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.Remark].ToString());
                        EditRemark = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.EditRemark].ToString());
                        CloseBy = CommonUtils.ConvertToString(dataRowView[ExecOrderProcesser.CloseBy].ToString());
                        EditBtnVisible = (bool)dataRowView[ExecOrderProcesser.EditBtnVisible];
                        SaveBtnVisible = (bool)dataRowView[ExecOrderProcesser.SaveBtnVisible];
                        ConfirmEnable = (bool)dataRowView[ExecOrderProcesser.ConfirmEnable];
                        IsOwn = (bool)dataRowView[ExecOrderProcesser.IsOwn];
                        ConfirmID = dataRowView[ExecOrderProcesser.ConfirmID] + "";
                        PaymentAmount = dataRowView[ExecOrderProcesser.PaymentAmount] + "";
                        TransactionPriceType = dataRowView[ExecOrderProcesser.TransactionPriceType] + "";
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void Instance_OnDetailOrder(DataRow dataRow)
        {
            string id = dataRow[ExecOrderProcesser.SecondaryTradeID] + "";
            if (string.IsNullOrEmpty(id))
                return;
            if (id.Equals(this.SecondaryTradeID))
            {
                DataTable dt = new DataTable();

                OnOrder(dataRow);
            }
        }

        public void ConfirmClick()
        {
            ExecuteConfirmRequest req = new ExecuteConfirmRequest();
            req.secondaryTradeID = this.SecondaryTradeID;
            req.confirmID = this.ConfirmID;
            PResult result= PtrHandler.Instance.updateTradeConfirm(req);
        }

        /// <summary>
        /// 清零回调方法
        /// </summary>
        /// <param name="obj"></param>
        public void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("ExecuteOrderViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                ClearUI();
            }
        }
    }
}
