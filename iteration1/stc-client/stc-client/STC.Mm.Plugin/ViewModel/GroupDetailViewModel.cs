using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Frame.Common;
using STC.Frame.Plugin;
using System.Data;
using System.Windows.Input;
using STC.Mm.Plugin.Utils;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using Newtonsoft.Json;
using STC.Common.Model.MM;
using STC.Common.Handler.Admin;
using STC.Mm.Plugin.View.Component;
using STC.Mm.Plugin.View;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using STC.Mm.Plugin.Validation;
using System.Windows.Data;
using STC.Common.View.UI;
using STC.Common.Utils;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.Utils.Data;

namespace STC.Mm.Plugin.ViewModel
{
    public class GroupDetailViewModel : ViewModelBase
    {
        public readonly Brush DefaultBorderBrush = (Brush)(new BrushConverter()).ConvertFromString("#3C3C3C");
        public readonly Brush RedBorderBrush = (Brush)(new BrushConverter()).ConvertFromString("red");

        private readonly SolidColorBrush NormalForeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        private readonly SolidColorBrush UpdateForeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9C152"));
        private readonly SolidColorBrush InvalidForeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));

        private readonly OrderParamProp originalData;

        private string bidBaseQtyLast = string.Empty;
        private string ofrBaseQtyLast = string.Empty;

        //新增债券模式
        private bool _IsAddBond = false;
        public bool IsAddBond
        {
            get { return _IsAddBond; }
            set
            {
                _IsAddBond = value;
                OnPropertyChanged();
            }
        }

        private OrderParamProp _Data;// = new OrderParamProp(null);
        public OrderParamProp Data
        {
            get { return this._Data; }

            set
            {
                this._Data = value;
                this.OnPropertyChanged();
            }
        }


        //自定义
        private Dictionary<string, string> _bidStrategyModeList = new Dictionary<string, string>();
        public Dictionary<string, string> bidStrategyModeList
        {
            get
            {
                if (_bidStrategyModeList.Count == 0)
                {
                    _bidStrategyModeList.Add("Defend", "防守");
                    _bidStrategyModeList.Add("Algorithm", "算法");
                }
                return _bidStrategyModeList;
            }
        }


        private MMOrderControlModel _SelectedItemRow;
        public MMOrderControlModel SelectedItemRow
        {
            get { return _SelectedItemRow; }
            set
            {
                _SelectedItemRow = value;
                OnPropertyChanged();
            }
        }

        private MMOrderParamReq _OrderParamReq = new MMOrderParamReq();
        public MMOrderParamReq OrderParamReq
        {
            get { return _OrderParamReq; }
            set
            {
                _OrderParamReq = value;
                this.OnPropertyChanged();
            }
        }

        private string _bidBasePriority1Text = "防守报价";
        public string BidBasePriority1Text
        {
            get { return _bidBasePriority1Text; }
            set
            {
                _bidBasePriority1Text = value;
                this.OnPropertyChanged();
            }
        }

        private string _bidBasePriority2Text = "算法报价";
        public string BidBasePriority2Text
        {
            get { return _bidBasePriority2Text; }
            set
            {
                _bidBasePriority2Text = value;
                this.OnPropertyChanged();
            }
        }

        private string _bidBasePriority1Value;
        public string BidBasePriority1Value
        {
            get { return _bidBasePriority1Value; }
            set
            {
                _bidBasePriority1Value = value;
                this.OnPropertyChanged();
            }
        }

        private string _bidBasePriority2Value;
        public string BidBasePriority2Value
        {
            get { return _bidBasePriority2Value; }
            set
            {
                _bidBasePriority2Value = value;
                this.OnPropertyChanged();
            }
        }

        private string _ofrBasePriority1Text = "防守报价";
        public string OfrBasePriority1Text
        {
            get { return _ofrBasePriority1Text; }
            set
            {
                _ofrBasePriority1Text = value;
                this.OnPropertyChanged();
            }
        }

        private string _ofrBasePriority2Text = "算法报价";
        public string OfrBasePriority2Text
        {
            get { return _ofrBasePriority2Text; }
            set
            {
                _ofrBasePriority2Text = value;
                this.OnPropertyChanged();
            }
        }

        private string _ofrBasePriority1Value;
        public string OfrBasePriority1Value
        {
            get { return _ofrBasePriority1Value; }
            set
            {
                _ofrBasePriority1Value = value;
                this.OnPropertyChanged();
            }
        }

        private string _ofrBasePriority2Value;
        public string OfrBasePriority2Value
        {
            get { return _ofrBasePriority2Value; }
            set
            {
                _ofrBasePriority2Value = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _BaseERReSendTypeList = new Dictionary<string, string>();
        public Dictionary<string, string> BaseERReSendTypeList
        {
            get { return this._BaseERReSendTypeList; }
            set
            {
                this._BaseERReSendTypeList = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _baseSettleTypeList = new Dictionary<string, string>();
        public Dictionary<string, string> BaseSettleTypeList
        {
            get { return this._baseSettleTypeList; }
            set
            {
                this._baseSettleTypeList = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _algoMidTypeList = new Dictionary<string, string>();
        public Dictionary<string, string> AlgoMidTypeList
        {
            get { return _algoMidTypeList; }
            set
            {
                _algoMidTypeList = value;
                this.OnPropertyChanged();
            }
        }
        private Dictionary<string, string> _DefenReferMarketTypeList = new Dictionary<string, string>();
        public Dictionary<string, string> DefenReferMarketTypeList
        {
            get { return _DefenReferMarketTypeList; }
            set
            {
                _DefenReferMarketTypeList = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _DefenReferLevelList = new Dictionary<string, string>();
        public Dictionary<string, string> DefenReferLevelList
        {
            get { return _DefenReferLevelList; }
            set
            {
                _DefenReferLevelList = value;
                this.OnPropertyChanged();
            }
        }

        private GroupDetailView _myWindow;
        public GroupDetailView MyWindow
        {
            get
            {
                return this._myWindow;
            }

            set
            {
                this._myWindow = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }

        /// <summary>
        /// Bid优先级设置-切换
        /// </summary>
        public ICommand BidBasePriorityShiftCommand { get { return new RelayCommand(this.DoBidBasePriorityShift); } }

        public ICommand OfrBasePriorityShiftCommand { get { return new RelayCommand(this.DoOfrBasePriorityShift); } }

        public ICommand ConfirmBtnCommand { get { return new RelayCommand(this.DoConfirmBtnCommand); } }

        public ICommand CancelBtnCommand { get { return new RelayCommand(this.DoCancelBtnCommand); } }

        private string _WinTitle;
        public string WinTitle
        {
            get
            {
                return this._WinTitle;
            }

            set
            {
                this._WinTitle = value;
                this.OnPropertyChanged();
            }
        }

        public bool NeedPrompt = true;

        public GroupDetailViewModel(OrderParamProp orderParams)
        {
            if (orderParams != null)
            {
                Data = orderParams.Clone() as OrderParamProp;
                originalData = orderParams.Clone() as OrderParamProp;

            }
            else
            {
                Data = new OrderParamProp(null);
                MMCommonUtils.OrderParamPropToDefault(Data);
            }
            Data2Request(Data, OrderParamReq);

            this.SetBasePriority();

            this.BaseERReSendTypeList.Add("1", "全量重发");
            this.BaseERReSendTypeList.Add("2", "余量重发");

            this.BaseSettleTypeList.Add("1", "T+0");
            this.BaseSettleTypeList.Add("2", "T+1");
            if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
            {
                this.AlgoMidTypeList.Add("1", "双边市场中间价");
            }
            this.AlgoMidTypeList.Add("2", "中债估值(推荐)");
            if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
            {
                this.AlgoMidTypeList.Add("3", "自定义%");
            }

            this.DefenReferMarketTypeList.Add("1", "双边");
            this.DefenReferMarketTypeList.Add("2", "X-Bond");
            this.DefenReferMarketTypeList.Add("4", "CFETS");
            this.DefenReferMarketTypeList.Add("3", "全市场");

            this.DefenReferLevelList.Add("1", "第一档");
            this.DefenReferLevelList.Add("2", "第二档");
            this.DefenReferLevelList.Add("3", "第三档");
            this.DefenReferLevelList.Add("4", "第四档");
            this.DefenReferLevelList.Add("5", "第五档");
        }

        //private void InitData()
        //{
        //    Data.bidBasePriority = "1,2";
        //    Data.ofrBasePriority = "1,2";
        //    Data.defaultManualSpread = "25";
        //    Data.bidDefenCalPrice = "5.125";
        //    Data.bidAlgoCalPrice = "6.34";
        //    Data.ofrDefenCalPrice = "5.589";
        //    Data.ofrAlgoCalPrice = "6.92";
        //    Data.bidQuoteModel = "1";
        //    Data.ofrQuoteModel = "1";
        //    Data.BaseERValue = "0";

        //    Data.bidBaseQty = "1000";
        //    Data.bidBaseReSendValue = "0";
        //    Data.bidBaseWidenValue = "0";

        //    Data.BaseERReSendType = "1";
        //    Data.baseSettleType = "1";

        //    Data.ofrBaseQty = "1000";
        //    Data.ofrBaseReSendValue = "0";
        //    Data.ofrBaseWidenValue = "0";

        //    Data.bidAlgoDefenValue = "0";
        //    Data.bidAlgoSafeValue = "0";
        //    Data.bidAlgoTrigValue = "0";
        //    Data.ofrAlgoDefenValue = "0";
        //    Data.ofrAlgoSafeValue = "0";
        //    Data.ofrAlgoTrigValue = "0";

        //    Data.algoEnable = "0";
        //    Data.algoBaseEnable = "0";
        //    Data.algoBaseSecurityid = string.Empty;
        //    Data.algoMidType = "1";
        //    Data.algoMidOffsetValue = "0";
        //    Data.algoValue = "0.01";
        //    Data.algoCDCEnable = "0";
        //    Data.bidAlgoCDCValue = "0.01";
        //    Data.ofrAlgoCDCValue = "0.01";

        //    Data.posLmtEnable = "0";
        //    Data.bidPosValue = "0";
        //    Data.ofrPosValue = "0";
        //    Data.stepLength = "0.25";
        //}

        private void SetBasePriority()
        {
            //1防守，2算法
            if (Data != null && !string.IsNullOrEmpty(Data.bidBasePriority))
            {
                string[] splitArr = Data.bidBasePriority.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr != null && splitArr.Length > 0)
                {
                    if (splitArr[0].Trim() == "1")
                    {
                        //防守优先
                        this.BidBasePriority1Text = "防守报价";
                        this.BidBasePriority2Text = "算法报价";

                        // TODO 这边需要切换相应的价格
                        this.BidBasePriority1Value = Data.bidDefenCalPrice;
                        this.BidBasePriority2Value = Data.bidAlgoCalPrice;
                    }
                    else
                    {
                        //算法优先
                        this.BidBasePriority1Text = "算法报价";
                        this.BidBasePriority2Text = "防守报价";

                        // TODO 这边需要切换相应的价格
                        this.BidBasePriority1Value = Data.bidAlgoCalPrice;
                        this.BidBasePriority2Value = Data.bidDefenCalPrice;
                    }
                }
            }

            if (Data != null && !string.IsNullOrEmpty(Data.ofrBasePriority))
            {
                string[] splitArr = Data.ofrBasePriority.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr != null && splitArr.Length > 0)
                {
                    if (splitArr[0].Trim() == "1")
                    {
                        //防守优先
                        this.OfrBasePriority1Text = "防守报价";
                        this.OfrBasePriority2Text = "算法报价";

                        // TODO 这边需要切换相应的价格
                        this.OfrBasePriority1Value = Data.ofrDefenCalPrice;
                        this.OfrBasePriority2Value = Data.ofrAlgoCalPrice;

                    }
                    else
                    {
                        //算法优先
                        this.OfrBasePriority1Text = "算法报价";
                        this.OfrBasePriority2Text = "防守报价";

                        // TODO 这边需要切换相应的价格
                        this.OfrBasePriority1Value = Data.ofrAlgoCalPrice;
                        this.OfrBasePriority2Value = Data.ofrDefenCalPrice;
                    }

                }
            }
        }

        private void DoBidBasePriorityShift()
        {
            if (!string.IsNullOrEmpty(this.Data.bidBasePriority))
            {
                string[] splitArr = Data.bidBasePriority.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr[0] == "1")
                {
                    StringBuilder b = new StringBuilder();
                    b.Append("2,1");
                    Data.bidBasePriority = b.ToString();
                }
                else
                {
                    StringBuilder b = new StringBuilder();
                    b.Append("1,2");
                    Data.bidBasePriority = b.ToString();
                }
                this.SetBasePriority();
            }
        }

        private void DoOfrBasePriorityShift()
        {
            if (!string.IsNullOrEmpty(this.Data.ofrBasePriority))
            {
                string[] splitArr = Data.ofrBasePriority.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr[0] == "1")
                {
                    StringBuilder b = new StringBuilder();
                    b.Append("2,1");
                    Data.ofrBasePriority = b.ToString();
                }
                else
                {
                    StringBuilder b = new StringBuilder();
                    b.Append("1,2");
                    Data.ofrBasePriority = b.ToString();
                }
                this.SetBasePriority();
            }
        }

        private void DoCancelBtnCommand()
        {
            MyWindow.Close();
        }

        public static MMOrderParamReq CheckSendFieldNull(MMOrderParamReq req)
        {
            MMOrderParamReq mq = req;
            if (string.IsNullOrEmpty(mq.circulSize)) { mq.circulSize = string.Empty; }
            //if (string.IsNullOrEmpty(mq.riskMinQtySize)) { mq.riskMinQtySize = string.Empty; }
            //if (string.IsNullOrEmpty(mq.riskMaxQtyRatio)) { mq.riskMaxQtyRatio = string.Empty; }
            if (string.IsNullOrEmpty(mq.securityid)) { mq.securityid = string.Empty; }
            if (string.IsNullOrEmpty(mq.symbol)) { mq.symbol = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidBasePriority)) { mq.bidBasePriority = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidDefenCalPrice)) { mq.bidDefenCalPrice = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidAlgoCalPrice)) { mq.bidAlgoCalPrice = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidQuoteModel)) { mq.bidQuoteModel = string.Empty; }
            if (string.IsNullOrEmpty(mq.defaultManualSpread)) { mq.defaultManualSpread = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidBaseQty)) { mq.bidBaseQty = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidBasePrice)) { mq.bidBasePrice = string.Empty; }

            if (string.IsNullOrEmpty(mq.bidBaseReSendValue)) { mq.bidBaseReSendValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidBaseWidenValue)) { mq.bidBaseWidenValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.BaseERValue)) { mq.BaseERValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.BaseERReSendType)) { mq.BaseERReSendType = string.Empty; }
            if (string.IsNullOrEmpty(mq.baseSettleType)) { mq.baseSettleType = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrBasePriority)) { mq.ofrBasePriority = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrDefenCalPrice)) { mq.ofrDefenCalPrice = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrAlgoCalPrice)) { mq.ofrAlgoCalPrice = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrQuoteModel)) { mq.ofrQuoteModel = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrBaseQty)) { mq.ofrBaseQty = string.Empty; }

            if (string.IsNullOrEmpty(mq.ofrBasePrice)) { mq.ofrBasePrice = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrBaseReSendValue)) { mq.ofrBaseReSendValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrBaseWidenValue)) { mq.ofrBaseWidenValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidAlgoDefenValue)) { mq.bidAlgoDefenValue = string.Empty; }

            if (string.IsNullOrEmpty(mq.algoSafeEnable)) { mq.algoSafeEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidAlgoSafeValue)) { mq.bidAlgoSafeValue = string.Empty; mq.ofrAlgoSafeValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.secondSafeEnable)) { mq.secondSafeEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.qdmespSafeEnable)) { mq.qdmespSafeEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.qdmespSafeLevel)) { mq.qdmespSafeLevel = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidQdmespSafeValue)) { mq.bidQdmespSafeValue = string.Empty; mq.ofrQdmespSafeValue = string.Empty; }

            if (string.IsNullOrEmpty(mq.bidAlgoTrigValue)) { mq.bidAlgoTrigValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrAlgoDefenValue)) { mq.ofrAlgoDefenValue = string.Empty; }
            //if (string.IsNullOrEmpty(mq.ofrAlgoSafeValue)) { mq.ofrAlgoSafeValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrAlgoTrigValue)) { mq.ofrAlgoTrigValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoEnable)) { mq.algoEnable = string.Empty; }

            if (string.IsNullOrEmpty(mq.algoBaseEnable)) { mq.algoBaseEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoBaseSecurityid)) { mq.algoBaseSecurityid = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoMidType)) { mq.algoMidType = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoMidDetail)) { mq.algoMidDetail = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoMidDetailDefined)) { mq.algoMidDetailDefined = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoMidOffsetValue)) { mq.algoMidOffsetValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoValue)) { mq.algoValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoCDCEnable)) { mq.algoCDCEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.algoCDCDetail)) { mq.algoCDCDetail = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidAlgoCDCValue)) { mq.bidAlgoCDCValue = string.Empty; }

            if (string.IsNullOrEmpty(mq.ofrAlgoCDCValue)) { mq.ofrAlgoCDCValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.posLmtEnable)) { mq.posLmtEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidPosValue)) { mq.bidPosValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrPosValue)) { mq.ofrPosValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.posFakEnable)) { mq.posFakEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.posFakSpread)) { mq.posFakSpread = string.Empty; }
            if (string.IsNullOrEmpty(mq.dealWidenLowerTime)) { mq.dealWidenLowerTime = string.Empty; }
            if (string.IsNullOrEmpty(mq.dealWidenUpperTime)) { mq.dealWidenUpperTime = string.Empty; }
            if (string.IsNullOrEmpty(mq.globalWallBid)) { mq.globalWallBid = string.Empty; }
            if (string.IsNullOrEmpty(mq.globalWallOfr)) { mq.globalWallOfr = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidStrategyType)) { mq.bidStrategyType = string.Empty; }

            if (string.IsNullOrEmpty(mq.ofrStrategyType)) { mq.ofrStrategyType = string.Empty; }
            if (string.IsNullOrEmpty(mq.apiBid)) { mq.apiBid = string.Empty; }
            if (string.IsNullOrEmpty(mq.apiOfr)) { mq.apiOfr = string.Empty; }
            if (string.IsNullOrEmpty(mq.apiBidQty)) { mq.apiBidQty = string.Empty; }
            if (string.IsNullOrEmpty(mq.apiOfrQty)) { mq.apiOfrQty = string.Empty; }
            if (string.IsNullOrEmpty(mq.apiLastTime)) { mq.apiLastTime = string.Empty; }
            if (string.IsNullOrEmpty(mq.manualBid)) { mq.manualBid = string.Empty; }
            if (string.IsNullOrEmpty(mq.manualOfr)) { mq.apiLastTime = string.Empty; }
            if (string.IsNullOrEmpty(mq.isQuoteSuccess)) { mq.isQuoteSuccess = string.Empty; }
            if (string.IsNullOrEmpty(mq.lastOptTime)) { mq.lastOptTime = string.Empty; }

            if (string.IsNullOrEmpty(mq.displayBidQty)) { mq.displayBidQty = string.Empty; }
            if (string.IsNullOrEmpty(mq.displayOfrQty)) { mq.displayOfrQty = string.Empty; }
            if (string.IsNullOrEmpty(mq.stepLength)) { mq.stepLength = string.Empty; }
            if (string.IsNullOrEmpty(mq.maxFloorEnable)) { mq.maxFloorEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.maxFloor)) { mq.maxFloor = string.Empty; }
            if (string.IsNullOrEmpty(mq.openQDM2Enable)) { mq.openQDM2Enable = string.Empty; }
            if (string.IsNullOrEmpty(mq.defenCDCEnable)) { mq.defenCDCEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidDefenCDCValue)) { mq.bidDefenCDCValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrDefenCDCValue)) { mq.ofrDefenCDCValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidDefenReferMarketType)) { mq.bidDefenReferMarketType = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidDefenReferLevel)) { mq.bidDefenReferLevel = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrDefenReferMarketType)) { mq.ofrDefenReferMarketType = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrDefenReferLevel)) { mq.ofrDefenReferLevel = string.Empty; }
            if (string.IsNullOrEmpty(mq.wallEnable)) { mq.wallEnable = string.Empty; }
            return mq;
        }

        private void DoConfirmBtnCommand()
        {
            Data.ofrQuoteModel = Data.bidQuoteModel;
            Data2Request(Data, OrderParamReq);

            Dictionary<string, string> errorList;
            errorList = CheckFieldIsEmpty();

            if (errorList.Count == 0)
            {
                if (!string.IsNullOrEmpty(this.WinTitle))
                {
                    string errorMsg = string.Empty;
                    //List<Inline> txtRun = new List<Inline>();
                    //Run run = null;

                    OrderParamReq = CheckSendFieldNull(OrderParamReq);

                    //当QuoteModel为手动时，需要将manual的值付给baseprice
                    //if (OrderParamReq.bidQuoteModel == "Manual") { OrderParamReq.bidBasePrice = OrderParamReq.manualBid; }
                    //if (OrderParamReq.ofrQuoteModel == "Manual") { OrderParamReq.ofrBasePrice = OrderParamReq.manualOfr; }

                    if (this.IsAddBond)
                    {
                        MyWindow.DialogResult = true;
                        MyWindow.Close();
                    }
                    else
                    {
                        
                    }
                }
            }
            else
            {
                MoveScrollPosition(errorList);

                return;
            }

            //MyWindow.DialogResult = true;
            //MyWindow.Close();
        }

        private void MoveScrollPosition(Dictionary<string, string> errorList)
        {
            if (errorList.ContainsKey("algoMidOffsetValue") || errorList.ContainsKey("algoValue") || errorList.ContainsKey("algoValue_Zero") ||
                errorList.ContainsKey("bidAlgoCDCValue") || errorList.ContainsKey("bidAlgoCDCValue_ZERO") ||
                errorList.ContainsKey("ofrAlgoCDCValue") || errorList.ContainsKey("ofrAlgoCDCValue_ZERO") ||
                errorList.ContainsKey("bidPosValue") || errorList.ContainsKey("ofrPosValue") ||
                errorList.ContainsKey("dealWidenLowerTime") || errorList.ContainsKey("dealWidenUpperTime") ||
                errorList.ContainsKey("dealWidenUpperTime_FIVE") || errorList.ContainsKey("dealWiden_ERROR") ||
                errorList.ContainsKey("bidBaseWidenValue") || errorList.ContainsKey("ofrBaseWidenValue") ||
                errorList.ContainsKey("posFakSpread"))
            {
                if (!errorList.ContainsKey("manualBid") && !errorList.ContainsKey("manualOfr") && !errorList.ContainsKey("stepLength") && !errorList.ContainsKey("bidBaseQty") && !errorList.ContainsKey("bidBaseQty_ZERO") &&
                !errorList.ContainsKey("ofrBaseQty") && !errorList.ContainsKey("ofrBaseQty_ZERO") && !errorList.ContainsKey("bidBaseReSendValue") && !errorList.ContainsKey("ofrBaseReSendValue") &&
                !errorList.ContainsKey("bidAlgoDefenValue") &&
                !errorList.ContainsKey("ofrAlgoDefenValue") &&
                !errorList.ContainsKey("bidAlgoSafeValue") && 
                !errorList.ContainsKey("bidQdmespSafeValue") && 
                !errorList.ContainsKey("ofrAlgoSafeValue") &&
                !errorList.ContainsKey("bidAlgoTrigValue") && !errorList.ContainsKey("ofrAlgoTrigValue") && !errorList.ContainsKey("algoBaseSecurityid_SAME") &&
                !errorList.ContainsKey("maxFloor") &&
                !errorList.ContainsKey("bidDefenCDCValue") && !errorList.ContainsKey("ofrDefenCDCValue"))
                {                //只有下半部分有错误
                    MyWindow.scroll.ScrollToVerticalOffset(500);
                }
            }
            else if (errorList.ContainsKey("manualBid") || errorList.ContainsKey("manualOfr") || !errorList.ContainsKey("stepLength") || errorList.ContainsKey("bidBaseQty") || errorList.ContainsKey("bidBaseQty_ZERO") ||
                errorList.ContainsKey("ofrBaseQty") || errorList.ContainsKey("ofrBaseQty_ZERO") || errorList.ContainsKey("bidBaseReSendValue") || errorList.ContainsKey("ofrBaseReSendValue") ||
                errorList.ContainsKey("bidAlgoDefenValue") ||
                errorList.ContainsKey("ofrAlgoDefenValue") || 
                errorList.ContainsKey("bidAlgoSafeValue") ||
                errorList.ContainsKey("bidQdmespSafeValue") ||
                errorList.ContainsKey("ofrAlgoSafeValue") ||
                errorList.ContainsKey("bidAlgoTrigValue") || errorList.ContainsKey("ofrAlgoTrigValue") || errorList.ContainsKey("algoBaseSecurityid_SAME") ||
                errorList.ContainsKey("maxFloor") ||
                errorList.ContainsKey("bidDefenCDCValue") || errorList.ContainsKey("ofrDefenCDCValue"))
            {
                if (!errorList.ContainsKey("algoMidOffsetValue") && !errorList.ContainsKey("algoValue") && !errorList.ContainsKey("algoValue_Zero") &&
                !errorList.ContainsKey("bidAlgoCDCValue") && !errorList.ContainsKey("bidAlgoCDCValue_ZERO") &&
                !errorList.ContainsKey("ofrAlgoCDCValue") && !errorList.ContainsKey("ofrAlgoCDCValue_ZERO") &&
                !errorList.ContainsKey("bidPosValue") && !errorList.ContainsKey("ofrPosValue") &&
                !errorList.ContainsKey("dealWidenLowerTime") && !errorList.ContainsKey("dealWidenUpperTime") &&
                !errorList.ContainsKey("bidBaseWidenValue") && !errorList.ContainsKey("ofrBaseWidenValue") &&
                !errorList.ContainsKey("dealWidenUpperTime_FIVE") && !errorList.ContainsKey("dealWiden_ERROR") &&
                !errorList.ContainsKey("posFakSpread"))
                {
                    MyWindow.scroll.ScrollToVerticalOffset(0);
                }
            }
        }

        public static void Data2Request(OrderParamProp data, MMOrderParamReq req)
        {
            if (data != null & req != null)
            {
                req.circulSize = data.circulSize;
                //req.riskMinQtySize = data.riskMinQtySize;
                //req.riskMaxQtyRatio = data.riskMaxQtyRatio;
                req.securityid = data.securityid;
                req.symbol = data.symbol;
                req.bidBasePriority = data.bidBasePriority;
                req.bidDefenCalPrice = data.bidDefenCalPrice;
                req.bidAlgoCalPrice = data.bidAlgoCalPrice;
                req.bidQuoteModel = data.bidQuoteModel;
                req.defaultManualSpread = data.defaultManualSpread;
                req.bidBaseQty = data.bidBaseQty;
                req.bidBasePrice = data.bidBasePrice;

                req.bidBaseReSendValue = data.bidBaseReSendValue;
                req.bidBaseWidenValue = data.bidBaseWidenValue;
                req.BaseERValue = data.BaseERValue;
                req.BaseERReSendType = data.BaseERReSendType;
                req.baseSettleType = data.baseSettleType;
                req.ofrBasePriority = data.ofrBasePriority;
                req.ofrDefenCalPrice = data.ofrDefenCalPrice;
                req.ofrAlgoCalPrice = data.ofrAlgoCalPrice;
                req.ofrQuoteModel = data.ofrQuoteModel;
                req.ofrBaseQty = data.ofrBaseQty;

                req.ofrBasePrice = data.ofrBasePrice;
                req.ofrBaseReSendValue = data.ofrBaseReSendValue;
                req.ofrBaseWidenValue = data.ofrBaseWidenValue;
                req.bidAlgoDefenValue = data.bidAlgoDefenValue;

                req.algoSafeEnable = data.algoSafeEnable;
                req.bidAlgoSafeValue = data.bidAlgoSafeValue;
                req.ofrAlgoSafeValue = data.bidAlgoSafeValue;
                req.secondSafeEnable = data.secondSafeEnable;
                req.qdmespSafeEnable = data.qdmespSafeEnable;
                req.qdmespSafeLevel = data.qdmespSafeLevel;
                req.bidQdmespSafeValue = data.bidQdmespSafeValue;
                req.ofrQdmespSafeValue = data.bidQdmespSafeValue;


                req.bidAlgoTrigValue = data.bidAlgoTrigValue;
                req.ofrAlgoDefenValue = data.ofrAlgoDefenValue;
                req.ofrAlgoTrigValue = data.ofrAlgoTrigValue;
                req.algoEnable = data.algoEnable;

                req.algoBaseEnable = data.algoBaseEnable;
                req.algoBaseSecurityid = data.algoBaseSecurityid;
                req.algoMidType = data.algoMidType;
                req.algoMidDetail = data.algoMidDetail;
                req.algoMidDetailDefined = data.algoMidDetailDefined;
                req.algoMidOffsetValue = data.algoMidOffsetValue;
                req.algoValue = data.algoValue;
                req.algoCDCEnable = data.algoCDCEnable;
                req.algoCDCDetail = data.algoCDCDetail;
                req.bidAlgoCDCValue = data.bidAlgoCDCValue;

                req.ofrAlgoCDCValue = data.ofrAlgoCDCValue;
                req.posLmtEnable = data.posLmtEnable;
                req.bidPosValue = data.bidPosValue;
                req.ofrPosValue = data.ofrPosValue;
                req.posFakEnable = data.posFakEnable;
                req.posFakSpread = data.posFakSpread;
                req.dealWidenLowerTime = data.dealWidenLowerTime;
                req.dealWidenUpperTime = data.dealWidenUpperTime;
                req.globalWallBid = data.globalWallBid;
                req.globalWallOfr = data.globalWallOfr;
                req.bidStrategyType = data.bidStrategyType;

                req.ofrStrategyType = data.ofrStrategyType;
                req.apiBid = data.apiBid;
                req.apiOfr = data.apiOfr;
                req.apiBidQty = data.apiBidQty;
                req.apiOfrQty = data.apiOfrQty;
                req.apiLastTime = data.apiLastTime;
                req.manualBid = data.manualBid;
                req.manualOfr = data.manualOfr;
                req.isQuoteSuccess = data.isQuoteSuccess;
                req.lastOptTime = data.lastOptTime;

                req.displayBidQty = data.displayBidQty;
                req.displayOfrQty = data.displayOfrQty;
                req.stepLength = data.stepLength;
                req.maxFloorEnable = data.maxFloorEnable;
                req.maxFloor = data.maxFloor;
                req.openQDM2Enable = data.openQDM2Enable;
                req.defenCDCEnable = data.defenCDCEnable;
                req.bidDefenCDCValue = data.bidDefenCDCValue;
                req.ofrDefenCDCValue = data.ofrDefenCDCValue;
                req.bidDefenReferMarketType = data.bidDefenReferMarketType;
                req.bidDefenReferLevel = data.bidDefenReferLevel;
                req.ofrDefenReferMarketType = data.ofrDefenReferMarketType;
                req.ofrDefenReferLevel = data.ofrDefenReferLevel;
                req.wallEnable = data.wallEnable;
                req.exeBondFollow = data.exeBondFollow;
            }
        }

        private bool CheckQtyIsThousand(string data)
        {
            bool bRet = false;

            decimal d;

            if (decimal.TryParse(data, out d))
            {
                if (d >= 1000 && d % 1000 == 0)
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        private Dictionary<string, string> CheckFieldIsEmpty()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            // ====== 必查项START ====================

            //if (string.IsNullOrEmpty(OrderParamReq.securityid))
            //{
            //    dic.Add("securityid", "债券代码为空");
            //}

            if (string.IsNullOrEmpty(OrderParamReq.bidQuoteModel))
            {
                OrderParamReq.ofrQuoteModel = OrderParamReq.bidQuoteModel;
                dic.Add("bidQuoteModel", "Bid方向报价模式为空");
            }
            //if (string.IsNullOrEmpty(OrderParamReq.ofrQuoteModel))
            //{
            //    dic.Add("ofrQuoteModel", "Ofr方向报价模式为空");
            //}


            if (string.IsNullOrEmpty(OrderParamReq.bidBaseReSendValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.bidBaseReSendValue, 1000, 0, 2))
            {
                dic.Add("bidBaseReSendValue", "Bid重发阈值为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrBaseReSendValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.ofrBaseReSendValue, 1000, 0, 2))
            {
                dic.Add("ofrBaseReSendValue", "Ofr重发阈值为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.bidBaseQty) || !MMCommonUtils.IsDecimal(OrderParamReq.bidBaseQty))
            {
                if (!"API".Equals(OrderParamReq.bidQuoteModel))
                {
                    dic.Add("bidBaseQty", "Bid单笔报量为空");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(OrderParamReq.bidBaseQty))
                {
                    //if (OrderParamReq.bidBaseQty.ConvertToDecimal().CompareTo(OrderParamReq.circulSize.ConvertToDecimal() * OrderParamReq.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
                    //{
                    //    if (!dic.ContainsKey("bidBaseQty_ZERO"))
                    //    {
                    //        dic.Add("bidBaseQty_ZERO", $"bid单笔报量需大于等于{Data.riskMinQtySize.ConvertToDecimal() / 1000}且小于等于{Data.circulSize.ConvertToDecimal() * Data.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10}");
                    //    }
                    //}

                    //if (OrderParamReq.bidBaseQty.ConvertToDecimal().CompareTo(OrderParamReq.riskMinQtySize.ConvertToDecimal() / 1000) < 0)
                    //{
                    //    if (!dic.ContainsKey("bidBaseQty_ZERO"))
                    //    {
                    //        dic.Add("bidBaseQty_ZERO", "bid单笔报量需大于等于" + Data.riskMinQtySize.ConvertToDecimal() / 1000);
                    //    }
                    //}
                }
            }

            if (string.IsNullOrEmpty(OrderParamReq.ofrBaseQty) || !MMCommonUtils.IsDecimal(OrderParamReq.ofrBaseQty))
            {
                if (!"API".Equals(OrderParamReq.ofrQuoteModel))
                {
                    dic.Add("ofrBaseQty", "Ofr单笔报量为空");
                }
                //MyWindow.WarnOfrBaseQty.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!string.IsNullOrEmpty(OrderParamReq.ofrBaseQty))
                {
                    //if (OrderParamReq.ofrBaseQty.ConvertToDecimal().CompareTo(OrderParamReq.circulSize.ConvertToDecimal() * OrderParamReq.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
                    //{
                    //    if (!dic.ContainsKey("ofrBaseQty_ZERO"))
                    //    {
                    //        dic.Add("ofrBaseQty_ZERO", $"ofr单笔报量需大于等于{Data.riskMinQtySize.ConvertToDecimal() / 1000}且小于等于{Data.circulSize.ConvertToDecimal() * Data.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10}");
                    //    }
                    //}

                    //if (OrderParamReq.ofrBaseQty.ConvertToDecimal().CompareTo(OrderParamReq.riskMinQtySize.ConvertToDecimal() / 1000) < 0)
                    //{
                    //    if (!dic.ContainsKey("ofrBaseQty_ZERO"))
                    //    {
                    //        dic.Add("ofrBaseQty_ZERO", "ofr单笔报量需大于等于" + Data.riskMinQtySize.ConvertToDecimal() / 1000);
                    //    }
                    //}

                }
            }

            /*if (string.IsNullOrEmpty(OrderParamReq.stepLength) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.stepLength, 99.99m, 0.01m, 2))
            {
                dic.Add("stepLength", "调整步长非法");
                //MyWindow.bidAlgoDefenValue_bd.BorderBrush = RedBorderBrush;
            }*/

            if (string.IsNullOrEmpty(OrderParamReq.baseSettleType) || !MMCommonUtils.IsDecimal(OrderParamReq.baseSettleType))
            {
                //清算速度已经选定默认值，不会为空
                dic.Add("baseSettleType", "清算速度为空");
            }

            // 成交报价设置检查
           /* if (string.IsNullOrEmpty(OrderParamReq.bidBaseWidenValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.bidBaseWidenValue, 1000, 0, 2))
            {
                dic.Add("bidBaseWidenValue", "Bid拉宽值为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrBaseWidenValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.ofrBaseWidenValue, 1000, 0, 2))
            {
                dic.Add("ofrBaseWidenValue", "Ofr拉宽值为空");
                //MyWindow.ofrBaseWidenValue_bd.BorderBrush = RedBorderBrush;
            }*/


            /*if (string.IsNullOrEmpty(OrderParamReq.BaseERReSendType) || !MMCommonUtils.IsDecimal(OrderParamReq.BaseERReSendType))
            {
                //拉宽类型已经选定默认值，不会为空
                dic.Add("BaseERReSendType", "成交拉宽类型为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.dealWidenLowerTime) || !MMCommonUtils.IsDecimal(OrderParamReq.dealWidenLowerTime))
            {
                dic.Add("dealWidenLowerTime", "成交拉宽时长下限为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.dealWidenUpperTime) || !MMCommonUtils.IsDecimal(OrderParamReq.dealWidenUpperTime))
            {
                dic.Add("dealWidenUpperTime", "成交拉宽时长上限为空");
            }*/
            if (!string.IsNullOrEmpty(OrderParamReq.dealWidenLowerTime) && !string.IsNullOrEmpty(OrderParamReq.dealWidenUpperTime) &&
                MMCommonUtils.IsDecimal(OrderParamReq.dealWidenLowerTime) && MMCommonUtils.IsDecimal(OrderParamReq.dealWidenUpperTime))
            {
                MyWindow.WarndealWiden.Visibility = Visibility.Collapsed;

                //if (decimal.Parse(OrderParamReq.dealWidenUpperTime) < 5)
                //{
                //    MyWindow.WarndealWiden.Text = "*上限应该大于5";
                //    MyWindow.WarndealWiden.Visibility = Visibility.Visible;
                //    dic.Add("dealWidenUpperTime_FIVE", "成交拉宽时长上限应该大于5");
                //}
                //else 
                if (decimal.Parse(OrderParamReq.dealWidenLowerTime) > decimal.Parse(OrderParamReq.dealWidenUpperTime))
                {
                    MyWindow.WarndealWiden.Text = "*下限应该小于上限";
                    MyWindow.WarndealWiden.Visibility = Visibility.Visible;
                    dic.Add("dealWiden_ERROR", "成交拉宽时长下限应该小于上限");
                }
            }

            // ====== 必查项END ====================

            // ========= 不做检查 START ==========
            //if (string.IsNullOrEmpty(OrderParamReq.bidBasePriority))
            //{
            //    dic.Add("bidBasePriority", "Bid报价优先级为空");
            //}

            if (string.IsNullOrEmpty(OrderParamReq.bidDefenCalPrice))
            {
                //bid防守报价 不做检查
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrDefenCalPrice))
            {
                //dic.Add("ofrDefenCalPrice", "Ofr防守报价为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.bidAlgoCalPrice))
            {
                //bid算法报价 不做检查
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrAlgoCalPrice))
            {
                //dic.Add("ofrAlgoCalPrice", "Ofr算法报价为空");
            }



            if (string.IsNullOrEmpty(OrderParamReq.defaultManualSpread))
            {
                //不做检查
                //dic.Add("defaultManualSpread","手动报价默认拉宽或者收窄点差为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.BaseERValue))
            {
                //日成交限额 不做检查
            }

            //if (string.IsNullOrEmpty(OrderParamReq.ofrBasePriority))
            //{
            //    dic.Add("ofrBasePriority", "Ofr价格优先级为空");
            //}

            // ========= 不做检查 END ==========

            //if (OrderParamReq.bidQuoteModel == "Manual" || OrderParamReq.ofrQuoteModel == "Manual")
            {
                //报价模式为人工检查 Manual(人工)
                //if (OrderParamReq.bidQuoteModel == "Manual")
                {
                    //if (string.IsNullOrEmpty(OrderParamReq.manualBid) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.manualBid, 9999, -99.9999m, 4))
                    //{
                    //    dic.Add("manualBid", "Bid单笔报价为空");
                    //}
                    //else if (MMCommonUtils.IsZero(OrderParamReq.bidBasePrice))
                    //{
                    //    dic.Add("bidBasePrice_ZERO", "Bid单笔报价为0");
                    //}
                }

                //if (OrderParamReq.ofrQuoteModel == "Manual")
                {
                    //if (string.IsNullOrEmpty(OrderParamReq.manualOfr) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.manualOfr, 9999, -99.9999m, 4))
                    //{
                    //    dic.Add("manualOfr", "Ofr单笔报价为空");
                    //}
                    //else if (MMCommonUtils.IsZero(OrderParamReq.ofrBasePrice))
                    //{
                    //    dic.Add("ofrBasePrice_ZERO", "Ofr单笔报价为0");
                    //}
                }
            }
            if (string.IsNullOrEmpty(OrderParamReq.bidDefenReferMarketType) || !MMCommonUtils.IsDecimal(OrderParamReq.bidDefenReferMarketType))
            {
                //参考中间价类型已经有默认值
                dic.Add("bidDefenReferMarketType", "Bid防守参考市场价为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.bidDefenReferLevel) || !MMCommonUtils.IsDecimal(OrderParamReq.bidDefenReferLevel))
            {
                //参考中间价类型已经有默认值
                dic.Add("bidDefenReferLevel", "Bid防守参考市场价档位为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrDefenReferMarketType) || !MMCommonUtils.IsDecimal(OrderParamReq.ofrDefenReferMarketType))
            {
                //参考中间价类型已经有默认值
                dic.Add("ofrDefenReferMarketType", "Ofr防守参考市场价为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrDefenReferLevel) || !MMCommonUtils.IsDecimal(OrderParamReq.ofrDefenReferLevel))
            {
                //参考中间价类型已经有默认值
                dic.Add("ofrDefenReferLevel", "Ofr防守参考市场价档位为空");
            }
            //if (OrderParamReq.bidQuoteModel == "Defend" || OrderParamReq.ofrQuoteModel == "Defend")
            {
                //报价模式为防守检查 Defend(防守) 

                //if (OrderParamReq.bidQuoteModel == "Defend")
                {
                    if (string.IsNullOrEmpty(OrderParamReq.bidAlgoDefenValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.bidAlgoDefenValue, 1000, 0, 2))
                    {
                        dic.Add("bidAlgoDefenValue", "Bid防守点为空");
                        //MyWindow.bidAlgoDefenValue_bd.BorderBrush = RedBorderBrush;
                    }

                    if (string.IsNullOrEmpty(OrderParamReq.bidAlgoSafeValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.bidAlgoSafeValue, 1000, -1000, 2))
                    {
                        dic.Add("bidAlgoSafeValue", "防守安全点为空");
                        //MyWindow.bidAlgoSafeValue_bd.BorderBrush = RedBorderBrush;
                    }
                    if (string.IsNullOrEmpty(OrderParamReq.bidQdmespSafeValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.bidQdmespSafeValue, 1000, -1000, 2))
                    {
                        dic.Add("bidQdmespSafeValue", "双边防守安全点为空");
                        //MyWindow.bidAlgoSafeValue_bd.BorderBrush = RedBorderBrush;
                    }

                    if (string.IsNullOrEmpty(OrderParamReq.bidAlgoTrigValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.bidAlgoTrigValue, 1000, 0, 2))
                    {
                        dic.Add("bidAlgoTrigValue", "Bid触墙拉宽点为空");
                        //MyWindow.bidAlgoTrigValue_bd.BorderBrush = RedBorderBrush;
                    }
                }

                //if (OrderParamReq.ofrQuoteModel == "Defend")
                {
                    if (string.IsNullOrEmpty(OrderParamReq.ofrAlgoDefenValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.ofrAlgoDefenValue, 1000, 0, 2))
                    {
                        dic.Add("ofrAlgoDefenValue", "Ofr防守点为空");
                        //MyWindow.ofrAlgoDefenValue_bd.BorderBrush = RedBorderBrush;
                    }

                    if (string.IsNullOrEmpty(OrderParamReq.ofrAlgoSafeValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.ofrAlgoSafeValue, 1000, -1000, 2))
                    {
                        dic.Add("ofrAlgoSafeValue", "Ofr安全点为空");
                        //MyWindow.ofrAlgoSafeValue_bd.BorderBrush = RedBorderBrush;
                    }
                    /*if (string.IsNullOrEmpty(OrderParamReq.ofrQdmespSafeValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.ofrQdmespSafeValue, 1000, 0, 2))
                    {
                        dic.Add("ofrQdmespSafeValue", "双边防守安全点为空");
                        //MyWindow.bidAlgoSafeValue_bd.BorderBrush = RedBorderBrush;
                    }*/

                    if (string.IsNullOrEmpty(OrderParamReq.ofrAlgoTrigValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.ofrAlgoTrigValue, 1000, 0, 2))
                    {
                        dic.Add("ofrAlgoTrigValue", "Ofr触墙拉宽点为空");
                        //MyWindow.ofrAlgoTrigValue_bd.BorderBrush = RedBorderBrush;
                    }
                }
            }

            //if (OrderParamReq.bidQuoteModel == "Algorithm" || OrderParamReq.ofrQuoteModel == "Algorithm")
            {
                //报价模式为算法检查 Algorithm(算法)

                if (string.IsNullOrEmpty(OrderParamReq.algoMidType) || !MMCommonUtils.IsDecimal(OrderParamReq.algoMidType))
                {
                    //参考中间价类型已经有默认值
                    dic.Add("algoMidType", "参考中间价类型为空");
                }
                else
                {
                    if (OrderParamReq.algoMidType == "3")
                    {
                        //自定义
                        if (string.IsNullOrEmpty(OrderParamReq.algoMidDetailDefined) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.algoMidDetailDefined, 1000, 0, 4))
                        {
                            //MyWindow.algoMidDetailDefined_bd.BorderBrush = RedBorderBrush;
                            if (!dic.ContainsKey("algoMidDetailDefined"))
                            {
                                dic.Add("algoMidDetailDefined", "参考中间价类型自定义值为空");
                            }
                        }
                        else
                        {
                            MyWindow.WarnAlgoMidDetailDefined.Visibility = Visibility.Collapsed;
                            //decimal d;
                            //if (!decimal.TryParse(OrderParamReq.algoMidDetailDefined, out d) || d == 0)
                            //{
                            //    MyWindow.WarnAlgoMidDetailDefined.Visibility = Visibility.Visible;
                            //    dic.Add("algoMidDetailDefined_ZERO", "参考中间价类型自定义值为0");
                            //}
                        }

                    }
                }

                if (string.IsNullOrEmpty(OrderParamReq.algoMidOffsetValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.algoMidOffsetValue, 1000, -1000, 2))
                {
                    dic.Add("algoMidOffsetValue", "算法偏离值为空");
                }

                if (string.IsNullOrEmpty(OrderParamReq.algoValue) || !MMCommonUtils.CheckDecimalValid(OrderParamReq.algoValue, 1000, 0.01m, 2))
                {
                    dic.Add("algoValue", "算法点差为空");
                }
                else
                {
                    decimal d;
                    if (!decimal.TryParse(OrderParamReq.algoValue, out d) || d == 0)
                    {
                        if (d <= 0)
                        {
                            MyWindow.WarnAlgoValue.Visibility = Visibility.Visible;
                            dic.Add("algoValue_Zero", "算法点差等于0,应该大于0");
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(OrderParamReq.posFakEnable))
            {
                //本方双边报价被点，若此时全市场存在比本方成交价更优行情，主动发起点价
                //不做检查
            }
            else
            {
                if ("1".Equals(OrderParamReq.posFakEnable))
                {
                    if (!MMCommonUtils.CheckDecimalValid(OrderParamReq.posFakSpread, 99.99m, 0, 2))
                    {
                        dic.Add("posFakSpread", "内部单点差非法");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(OrderParamReq.posFakSpread))
                    {
                        if (!string.IsNullOrEmpty(OrderParamReq.posFakSpread) && !MMCommonUtils.CheckDecimalValid(OrderParamReq.posFakSpread, 99.99m, 0, 2))
                        {
                            dic.Add("posFakSpread", "内部单点差非法");
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(OrderParamReq.maxFloorEnable))
            {
                //本方双边报价被点，若此时全市场存在比本方成交价更优行情，主动发起点价
                //不做检查
            }
            else
            {
                if ("1".Equals(OrderParamReq.maxFloorEnable))
                {
                    if (!MMCommonUtils.CheckDecimalValid(OrderParamReq.maxFloor, 999, 1, 0))
                    {
                        dic.Add("maxFloor", "冰山量非法");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(OrderParamReq.maxFloorEnable))
                    {
                        if (!string.IsNullOrEmpty(OrderParamReq.maxFloor) && !MMCommonUtils.CheckDecimalValid(OrderParamReq.maxFloor, 999, 1, 0))
                        {
                            dic.Add("maxFloor", "冰山量非法");
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(OrderParamReq.defenCDCEnable))
            {

            }
            else
            {
                if ("1".Equals(OrderParamReq.defenCDCEnable))
                {
                    if (!MMCommonUtils.CheckDecimalValid(OrderParamReq.bidDefenCDCValue, 1000, -1000, 0))
                    {
                        dic.Add("bidDefenCDCValue", "估值拉宽Bid非法");
                    }
                    if (!MMCommonUtils.CheckDecimalValid(OrderParamReq.ofrDefenCDCValue, 1000, -1000, 0))
                    {
                        dic.Add("ofrDefenCDCValue", "估值拉宽Ofr非法");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(OrderParamReq.defenCDCEnable))
                    {
                        if (!string.IsNullOrEmpty(OrderParamReq.bidDefenCDCValue) && !MMCommonUtils.CheckDecimalValid(OrderParamReq.bidDefenCDCValue, 1000, -1000, 0))
                        {
                            dic.Add("bidDefenCDCValue", "估值拉宽Bid非法");
                        }
                        if (!string.IsNullOrEmpty(OrderParamReq.ofrDefenCDCValue) && !MMCommonUtils.CheckDecimalValid(OrderParamReq.ofrDefenCDCValue, 1000, -1000, 0))
                        {
                            dic.Add("ofrDefenCDCValue", "估值拉宽Ofr非法");
                        }
                    }
                }
            }

            return dic;
        }

        private bool IsUpdateField(string fieldname)
        {
            bool bRet = false;

            switch (fieldname)
            {
                case "manualBid":
                    if (originalData.manualBid != Data.manualBid) { bRet = true; }
                    break;

                case "manualOfr":
                    if (originalData.manualOfr != Data.manualOfr) { bRet = true; }
                    break;

                case "stepLength":
                    if (originalData.stepLength != Data.stepLength) { bRet = true; }
                    break;

                case "bidBaseQty":
                    if (originalData.bidBaseQty != Data.bidBaseQty) { bRet = true; }
                    break;

                case "bidBaseReSendValue":
                    if (originalData.bidBaseReSendValue != Data.bidBaseReSendValue) { bRet = true; }
                    break;

                case "bidBaseWidenValue":
                    if (originalData.bidBaseWidenValue != Data.bidBaseWidenValue) { bRet = true; }
                    break;

                case "ofrBaseQty":
                    if (originalData.ofrBaseQty != Data.ofrBaseQty) { bRet = true; }
                    break;

                case "ofrBaseReSendValue":
                    if (originalData.ofrBaseReSendValue != Data.ofrBaseReSendValue) { bRet = true; }
                    break;

                case "ofrBaseWidenValue":
                    if (originalData.ofrBaseWidenValue != Data.ofrBaseWidenValue) { bRet = true; }
                    break;

                case "bidAlgoDefenValue":
                    if (originalData.bidAlgoDefenValue != Data.bidAlgoDefenValue) { bRet = true; }
                    break;

                case "bidAlgoSafeValue":
                    if (originalData.bidAlgoSafeValue != Data.bidAlgoSafeValue) { bRet = true; }
                    break;

                case "bidQdmespSafeValue":
                    if (originalData.bidQdmespSafeValue != Data.bidQdmespSafeValue) { bRet = true; }
                    break;

                case "bidAlgoTrigValue":
                    if (originalData.bidAlgoTrigValue != Data.bidAlgoTrigValue) { bRet = true; }
                    break;

                case "ofrAlgoDefenValue":
                    if (originalData.ofrAlgoDefenValue != Data.ofrAlgoDefenValue) { bRet = true; }
                    break;

                case "ofrAlgoSafeValue":
                    if (originalData.ofrAlgoSafeValue != Data.ofrAlgoSafeValue) { bRet = true; }
                    break;

                case "ofrAlgoTrigValue":
                    if (originalData.ofrAlgoTrigValue != Data.ofrAlgoTrigValue) { bRet = true; }
                    break;

                case "bond_txt":
                    if (originalData.algoBaseSecurityid != Data.algoBaseSecurityid) { bRet = true; }
                    break;

                case "algoMidDetailDefined":
                    if (originalData.algoMidDetailDefined != Data.algoMidDetailDefined) { bRet = true; }
                    break;

                case "algoMidOffsetValue":
                    if (originalData.algoMidOffsetValue != Data.algoMidOffsetValue) { bRet = true; }
                    break;

                case "algoValue":
                    if (originalData.algoValue != Data.algoValue) { bRet = true; }
                    break;

                case "bidAlgoCDCValue":
                    if (originalData.bidAlgoCDCValue != Data.bidAlgoCDCValue) { bRet = true; }
                    break;

                case "ofrAlgoCDCValue":
                    if (originalData.ofrAlgoCDCValue != Data.ofrAlgoCDCValue) { bRet = true; }
                    break;

                case "bidPosValue":
                    if (originalData.bidPosValue != Data.bidPosValue) { bRet = true; }
                    break;

                case "ofrPosValue":
                    if (originalData.ofrPosValue != Data.ofrPosValue) { bRet = true; }
                    break;
                case "bidDefenCDCValue":
                    if (originalData.bidDefenCDCValue != Data.bidDefenCDCValue) { bRet = true; }
                    break;
                case "ofrDefenCDCValue":
                    if (originalData.ofrDefenCDCValue != Data.ofrDefenCDCValue) { bRet = true; }
                    break;
            }

            return bRet;
        }

        public void ResetWindowBorderBursh(string name)
        {
            switch (name)
            {
                case "bidBaseQty_tb":
                    MyWindow.WarnBidBaseQty.Visibility = Visibility.Collapsed;
                    MyWindow.WarnBidBaseQty.Text = string.Empty;
                    MyWindow.bidBaseQty_tb.Foreground = NormalForeColor;
                    MyWindow.bidBaseQty_tb.IsReadOnly = false;
                    MyWindow.bidBaseQty_tb.IsEnabled = true;

                    if (IsUpdateField("bidBaseQty"))
                    {
                        MyWindow.bidBaseQty_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.bidBaseQty_tb.Foreground = NormalForeColor;
                    }

                    if (!string.IsNullOrEmpty(Data.bidBaseQty))
                    {
                        if (MMCommonUtils.IsZero(Data.bidBaseQty))
                        {
                            MyWindow.WarnBidBaseQty.Text = "*<Bid>设置需大于0";
                            MyWindow.WarnBidBaseQty.Visibility = Visibility.Visible;
                        }

                        //if (Data.bidBaseQty.ConvertToDecimal().CompareTo(Data.circulSize.ConvertToDecimal() * Data.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
                        //{
                        //    MyWindow.WarnBidBaseQty.Text = "*<Bid>设置超出风控设置范围";
                        //    MyWindow.WarnBidBaseQty.Visibility = Visibility.Visible;
                        //}
                    }

                    if (Data.bidQuoteModel == "API")
                    {
                        MyWindow.bidBaseQty_tb.Foreground = InvalidForeColor;
                        MyWindow.bidBaseQty_tb.IsReadOnly = true;
                        MyWindow.bidBaseQty_tb.IsEnabled = false;
                        Data.bidBaseQty = Data.apiBidQty;
                    }

                    break;

                case "ofrBaseQty_tb":
                    MyWindow.WarnOfrBaseQty.Visibility = Visibility.Collapsed;
                    MyWindow.WarnOfrBaseQty.Text = string.Empty;
                    MyWindow.ofrBaseQty_tb.Foreground = NormalForeColor;
                    MyWindow.ofrBaseQty_tb.IsReadOnly = false;
                    MyWindow.ofrBaseQty_tb.IsEnabled = true;

                    if (IsUpdateField("ofrBaseQty"))
                    {
                        MyWindow.ofrBaseQty_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrBaseQty_tb.Foreground = NormalForeColor;
                    }

                    if (!string.IsNullOrEmpty(Data.ofrBaseQty))
                    {
                        if (MMCommonUtils.IsZero(Data.ofrBaseQty))
                        {
                            MyWindow.WarnOfrBaseQty.Text = "*<Ofr>设置需大于0";
                            MyWindow.WarnOfrBaseQty.Visibility = Visibility.Visible;
                        }

                        //if (Data.ofrBaseQty.ConvertToDecimal().CompareTo(Data.circulSize.ConvertToDecimal() * Data.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
                        //{
                        //    MyWindow.WarnOfrBaseQty.Text = "*<Ofr>设置超出风控设置范围";
                        //    MyWindow.WarnOfrBaseQty.Visibility = Visibility.Visible;
                        //}
                    }

                    if (Data.ofrQuoteModel == "API")
                    {
                        MyWindow.ofrBaseQty_tb.Foreground = InvalidForeColor;
                        MyWindow.ofrBaseQty_tb.IsReadOnly = true;
                        MyWindow.ofrBaseQty_tb.IsEnabled = false;
                        Data.ofrBaseQty = Data.apiOfrQty;
                    }

                    break;

                case "bidBaseReSendValue_tb":
                    if (IsUpdateField("bidBaseReSendValue"))
                    {
                        MyWindow.bidBaseReSendValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.bidBaseReSendValue_tb.Foreground = NormalForeColor;
                    }

                    break;

                case "bidBaseWidenValue_tb":
                    if (IsUpdateField("bidBaseWidenValue"))
                    {
                        MyWindow.bidBaseWidenValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.bidBaseWidenValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "ofrBaseReSendValue_tb":
                    if (IsUpdateField("ofrBaseReSendValue"))
                    {
                        MyWindow.ofrBaseReSendValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrBaseReSendValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "ofrBaseWidenValue_tb":
                    if (IsUpdateField("ofrBaseWidenValue"))
                    {
                        MyWindow.ofrBaseWidenValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrBaseWidenValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "bidAlgoDefenValue_tb":
                    if (IsUpdateField("bidAlgoDefenValue"))
                    {
                        MyWindow.bidAlgoDefenValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.bidAlgoDefenValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "bidAlgoSafeValue_tb":
                    if (IsUpdateField("bidAlgoSafeValue"))
                    {
                        MyWindow.bidAlgoSafeValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.bidAlgoSafeValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "bidAlgoTrigValue_tb":
                    if (IsUpdateField("bidAlgoTrigValue"))
                    {
                        MyWindow.bidAlgoTrigValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.bidAlgoTrigValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "ofrAlgoDefenValue_tb":
                    if (IsUpdateField("ofrAlgoDefenValue"))
                    {
                        MyWindow.ofrAlgoDefenValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrAlgoDefenValue_tb.Foreground = NormalForeColor;
                    }
                    break;
                case "bidQdmespSafeValue_tb":
                    if (IsUpdateField("bidQdmespSafeValue"))
                    {
                        MyWindow.bidQdmespSafeValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.bidQdmespSafeValue_tb.Foreground = NormalForeColor;
                    }
                    break;
                /*case "ofrAlgoSafeValue_tb":
                    if (IsUpdateField("ofrAlgoSafeValue"))
                    {
                        MyWindow.ofrAlgoSafeValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrAlgoSafeValue_tb.Foreground = NormalForeColor;
                    }
                    break;*/

                case "ofrAlgoTrigValue_tb":
                    if (IsUpdateField("ofrAlgoTrigValue"))
                    {
                        MyWindow.ofrAlgoTrigValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrAlgoTrigValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "algoMidDetailDefined_tbox":
                    MyWindow.algoMidDetailDefined_tbox.BorderBrush = DefaultBorderBrush;
                    MyWindow.WarnAlgoMidDetailDefined.Visibility = Visibility.Collapsed;

                    if (Data.algoMidType == "3" && (Data.bidQuoteModel == "Algorithm" || Data.ofrQuoteModel == "Algorithm"))// && Data.algoEnable == "1"
                    {
                        if (string.IsNullOrEmpty(Data.algoMidDetailDefined))
                        {
                            //MyWindow.algoMidDetailDefined_tbox.BorderBrush = RedBorderBrush;
                            //MyWindow.algoMidDetailDefined_tbox.ToolTip = "不能为空";
                        }
                        else
                        {
                            //MyWindow.algoMidDetailDefined_tbox.BorderBrush = DefaultBorderBrush;
                            //MyWindow.algoMidDetailDefined_tbox.ToolTip = string.Empty;

                            //MyWindow.WarnAlgoMidDetailDefined.Visibility = Visibility.Collapsed;
                            //decimal d;
                            //if (!decimal.TryParse(Data.algoMidDetailDefined,out d) || d ==0)
                            //{
                            //    MyWindow.WarnAlgoMidDetailDefined.Visibility = Visibility.Visible;
                            //}
                        }
                    }

                    if (IsUpdateField("algoMidDetailDefined"))
                    {
                        MyWindow.algoMidDetailDefined_tbox.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.algoMidDetailDefined_tbox.Foreground = NormalForeColor;
                    }

                    break;
                case "algoMidOffsetValue_tb":
                case "algoValue_tb":
                    MyWindow.WarnAlgoValue.Visibility = Visibility.Collapsed;
                    MyWindow.algoMidOffsetValue_tb.BorderBrush = DefaultBorderBrush;
                    MyWindow.algoValue_tb.BorderBrush = DefaultBorderBrush;

                    if (!string.IsNullOrEmpty(Data.algoValue) && (Data.bidQuoteModel == "Algorithm" || Data.ofrQuoteModel == "Algorithm"))
                    {
                        decimal d;
                        if (!decimal.TryParse(Data.algoValue, out d) || d == 0)
                        {
                            if (d <= 0)
                            {
                                MyWindow.WarnAlgoValue.Visibility = Visibility.Visible;
                            }
                        }

                    }

                    if (IsUpdateField("algoMidOffsetValue"))
                    {
                        MyWindow.algoMidOffsetValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.algoMidOffsetValue_tb.Foreground = NormalForeColor;
                    }

                    if (IsUpdateField("algoValue"))
                    {
                        MyWindow.algoValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.algoValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "dealWidenUpperTime_tbx":
                    MyWindow.WarndealWiden.Visibility = Visibility.Collapsed;
                    if (!string.IsNullOrEmpty(Data.dealWidenLowerTime) && !string.IsNullOrEmpty(Data.dealWidenUpperTime))
                    {
                        //if (decimal.Parse(Data.dealWidenUpperTime) < 5)
                        //{
                        //    MyWindow.WarndealWiden.Text = "*上限应该大于5";
                        //    MyWindow.WarndealWiden.Visibility = Visibility.Visible;
                        //}
                        //else 
                        if (decimal.Parse(Data.dealWidenUpperTime) < decimal.Parse(Data.dealWidenLowerTime))
                        {
                            MyWindow.WarndealWiden.Text = "*下限应该小于上限";
                            MyWindow.WarndealWiden.Visibility = Visibility.Visible;
                        }
                    }
                    break;

                case "dealWidenLowerTime_tbx":
                    MyWindow.WarndealWiden.Visibility = Visibility.Collapsed;
                    if (!string.IsNullOrEmpty(Data.dealWidenUpperTime) && !string.IsNullOrEmpty(Data.dealWidenLowerTime))
                    {
                        if (decimal.Parse(Data.dealWidenLowerTime) > decimal.Parse(Data.dealWidenUpperTime))
                        {
                            MyWindow.WarndealWiden.Text = "*下限应该小于上限";
                            MyWindow.WarndealWiden.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case "posFakSpread_tb":
                    MyWindow.posFakSpread_tb.BorderBrush = DefaultBorderBrush;
                    if (Data.posFakEnable == "1")
                    {
                        if (string.IsNullOrEmpty(Data.posFakSpread))
                        {
                            MyWindow.posFakSpread_tb.BorderBrush = RedBorderBrush;
                        }
                        else
                        {
                            MyWindow.posFakSpread_tb.BorderBrush = DefaultBorderBrush;
                        }
                    }
                    else
                    {
                        MyWindow.posFakSpread_tb.BorderBrush = DefaultBorderBrush;
                    }
                    break;
                case "bidDefenCDCValue_tb":
                    if (IsUpdateField("bidDefenCDCValue"))
                    {
                        MyWindow.bidDefenCDCValue_tb.Foreground = UpdateForeColor;
                        MyWindow.bidDefenCDCValue_tb.BorderBrush = DefaultBorderBrush;
                    }
                    else
                    {
                        if ("1".Equals(Data.defenCDCEnable))
                        {
                            if (string.IsNullOrEmpty(Data.bidDefenCDCValue))
                            {
                                MyWindow.bidDefenCDCValue_tb.BorderBrush = RedBorderBrush;
                            }
                            else
                            {
                                MyWindow.bidDefenCDCValue_tb.BorderBrush = DefaultBorderBrush;
                            }
                        }
                        else
                        {
                            MyWindow.bidDefenCDCValue_tb.BorderBrush = DefaultBorderBrush;
                            MyWindow.bidDefenCDCValue_tb.Foreground = NormalForeColor;
                        }
                    }
                    break;
                case "ofrDefenCDCValue_tb":
                    if (IsUpdateField("ofrDefenCDCValue"))
                    {
                        MyWindow.ofrDefenCDCValue_tb.Foreground = UpdateForeColor;
                        MyWindow.ofrDefenCDCValue_tb.BorderBrush = DefaultBorderBrush;
                    }
                    else
                    {
                        if ("1".Equals(Data.defenCDCEnable))
                        {
                            if (string.IsNullOrEmpty(Data.ofrDefenCDCValue))
                            {
                                MyWindow.ofrDefenCDCValue_tb.BorderBrush = RedBorderBrush;
                            }
                            else
                            {
                                MyWindow.ofrDefenCDCValue_tb.BorderBrush = DefaultBorderBrush;
                            }
                        }
                        else
                        {
                            MyWindow.ofrDefenCDCValue_tb.BorderBrush = DefaultBorderBrush;
                            MyWindow.ofrDefenCDCValue_tb.Foreground = NormalForeColor;
                        }
                    }
                    break;
            }
        }

        public void UpdateSecurityCdc()
        {
            if (Data.algoBaseEnable == "1" && (Data.bidQuoteModel == "Algorithm" || Data.ofrQuoteModel == "Algorithm"))
            {
                if (!string.IsNullOrEmpty(Data.algoBaseSecurityid))
                {
                    MMOrderParamReq req = new MMOrderParamReq();
                    Data2Request(Data, req);

                    PResult result = STC.Common.Handler.Admin.MMHandler.Instance.mmUpdateSecurityCdc(req);

                    string errorMsg = string.Empty;
                    if (result != null && result.Code == "0000")
                    {
                    }
                    else
                    {
                        if (result != null && !string.IsNullOrEmpty(result.Msg))
                        {
                            errorMsg = result.Msg;
                        }
                        else
                        {
                            errorMsg = "发送基准券：服务器接收数据失败！";
                        }
                    }

                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.setWarnMsg(errorMsg, true);
                        msgBox.ShowDialog();
                    }
                }
            }
        }

        private void CloseClick()
        {
            MyWindow.Close();
        }
    }
}
