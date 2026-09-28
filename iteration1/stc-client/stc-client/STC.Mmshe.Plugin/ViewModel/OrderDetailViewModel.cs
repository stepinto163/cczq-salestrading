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
using STC.Mmshe.Plugin.Utils;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using Newtonsoft.Json;
using STC.Common.Model.MMshe;
using STC.Common.Handler.Admin;
using STC.Mmshe.Plugin.View.Component;
using STC.Mmshe.Plugin.View;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using STC.Mmshe.Plugin.Validation;
using System.Windows.Data;
using STC.Common.View.UI;
using STC.Common.Utils;
using STC.Mmshe.Plugin.Command;
using STC.Common.Model.MM;
using System.Threading;
using com.gateway.log;
using STC.Common.Handler.Cal;
using STC.Common.Model.Cal;

namespace STC.Mmshe.Plugin.ViewModel
{
    public class OrderDetailViewModel : ViewModelBase
    {

        private readonly string InputSecurityID;

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

        private MMsheOrderParamReq _OrderParamReq = new MMsheOrderParamReq();
        public MMsheOrderParamReq OrderParamReq
        {
            get { return _OrderParamReq; }
            set
            {
                _OrderParamReq = value;
                this.OnPropertyChanged();
            }
        }

        private string _bidBasePriority1Text = "跟随报价";
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

        private string _ofrBasePriority1Text = "跟随报价";
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

        private Dictionary<string, string> _exeBondPreList = new Dictionary<string, string>();
        public Dictionary<string, string> ExeBondPreList
        {
            get { return this._exeBondPreList; }
            set
            {
                this._exeBondPreList = value;
                this.OnPropertyChanged();
            }
        }

        private OrderDetailView _myWindow;
        public OrderDetailView MyWindow
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
        public ICommand InterestDefaultCommand { get { return new RelayCommand(this.DoInterestDefaultCommand); } }
        private void showMessageBoxGUI(string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBoxGUI msgBox = new MessageBoxGUI();
                msgBox.setWarnMsg(msg, true);
                msgBox.ShowDialog();
            });
        }
        private void copySettingToData(MMBondParamsSetting paramsSetting)
        {
            if (!string.IsNullOrEmpty(paramsSetting.algoMidType))
            {
                Data.algoMidType = paramsSetting.algoMidType;
            }
            if (!string.IsNullOrEmpty(paramsSetting.algoValue))
            {
                Data.algoValue = paramsSetting.algoValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.baseSettleType))
            {
                Data.baseSettleType = paramsSetting.baseSettleType;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidAlgoDefenValue))
            {
                Data.bidAlgoDefenValue = paramsSetting.bidAlgoDefenValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidAlgoSafeValue))
            {
                Data.bidAlgoSafeValue = paramsSetting.bidAlgoSafeValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidAlgoTrigValue))
            {
                Data.bidAlgoTrigValue = paramsSetting.bidAlgoTrigValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidBaseQty))
            {
                Data.bidBaseQty = paramsSetting.bidBaseQty;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidBaseReSendValue))
            {
                Data.bidBaseReSendValue = paramsSetting.bidBaseReSendValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidBaseWidenValue))
            {
                Data.bidBaseWidenValue = paramsSetting.bidBaseWidenValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidDefenCDCValue))
            {
                Data.bidDefenCDCValue = paramsSetting.bidDefenCDCValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidDefenReferLevel))
            {
                Data.bidDefenReferLevel = paramsSetting.bidDefenReferLevel;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidDefenReferMarketType))
            {
                Data.bidDefenReferMarketType = paramsSetting.bidDefenReferMarketType;
            }
            if (!string.IsNullOrEmpty(paramsSetting.bidQuoteModel))
            {
                Data.bidQuoteModel = paramsSetting.bidQuoteModel;
            }
            if (!string.IsNullOrEmpty(paramsSetting.dealWidenLowerTime))
            {
                Data.dealWidenLowerTime = paramsSetting.dealWidenLowerTime;
            }
            if (!string.IsNullOrEmpty(paramsSetting.dealWidenUpperTime))
            {
                Data.dealWidenUpperTime = paramsSetting.dealWidenUpperTime;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoDefenValue))
            {
                Data.ofrAlgoDefenValue = paramsSetting.ofrAlgoDefenValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoSafeValue))
            {
                Data.ofrAlgoSafeValue = paramsSetting.ofrAlgoSafeValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrAlgoTrigValue))
            {
                Data.ofrAlgoTrigValue = paramsSetting.ofrAlgoTrigValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrBaseQty))
            {
                Data.ofrBaseQty = paramsSetting.ofrBaseQty;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrBaseReSendValue))
            {
                Data.ofrBaseReSendValue = paramsSetting.ofrBaseReSendValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrBaseWidenValue))
            {
                Data.ofrBaseWidenValue = paramsSetting.ofrBaseWidenValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrDefenCDCValue))
            {
                Data.ofrDefenCDCValue = paramsSetting.ofrDefenCDCValue;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrDefenReferLevel))
            {
                Data.ofrDefenReferLevel = paramsSetting.ofrDefenReferLevel;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrDefenReferMarketType))
            {
                Data.ofrDefenReferMarketType = paramsSetting.ofrDefenReferMarketType;
            }
            if (!string.IsNullOrEmpty(paramsSetting.ofrQuoteModel))
            {
                Data.ofrQuoteModel = paramsSetting.ofrQuoteModel;
            }
        }
        private void DoInterestDefaultCommand()
        {
            MMBondParamsSetting paramsSetting = MMsheCommonUtils.getBondParamsSetting("InterestDefault");
            if (paramsSetting != null)
            {
                copySettingToData(paramsSetting);
            }
            else
            {
                showMessageBoxGUI("利率债默认参数为空");
            }
        }

        public ICommand InterestActiveCommand { get { return new RelayCommand(this.DoInterestActiveCommand); } }

        private void DoInterestActiveCommand()
        {
            MMBondParamsSetting paramsSetting = MMsheCommonUtils.getBondParamsSetting("InterestActive");
            if (paramsSetting != null)
            {
                copySettingToData(paramsSetting);
            }
            else
            {
                showMessageBoxGUI("利率债活跃债参数为空");
            }
        }

        public ICommand InterestBidCommand { get { return new RelayCommand(this.DoInterestBidCommand); } }

        private void DoInterestBidCommand()
        {
            MMBondParamsSetting paramsSetting = MMsheCommonUtils.getBondParamsSetting("InterestBid");
            if (paramsSetting != null)
            {
                copySettingToData(paramsSetting);
            }
            else
            {
                showMessageBoxGUI("利率债买入激进参数为空");
            }
        }

        public ICommand InterestOfrCommand { get { return new RelayCommand(this.DoInterestOfrCommand); } }

        private void DoInterestOfrCommand()
        {
            MMBondParamsSetting paramsSetting = MMsheCommonUtils.getBondParamsSetting("InterestOfr");
            if (paramsSetting != null)
            {
                copySettingToData(paramsSetting);
            }
            else
            {
                showMessageBoxGUI("利率债卖出激进参数为空");
            }
        }

        public ICommand CreditDefaultCommand { get { return new RelayCommand(this.DoCreditDefaultCommand); } }

        private void DoCreditDefaultCommand()
        {
            MMBondParamsSetting paramsSetting = MMsheCommonUtils.getBondParamsSetting("CreditDefault");
            if (paramsSetting != null)
            {
                copySettingToData(paramsSetting);
            }
            else
            {
                showMessageBoxGUI("信用债默认参数为空");
            }
        }

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

        public OrderDetailViewModel(OrderParamProp orderParams)
        {
            if (orderParams != null)
            {
                InputSecurityID = !string.IsNullOrEmpty(orderParams.securityid) ? orderParams.securityid : string.Empty;
                string symbol = !string.IsNullOrEmpty(orderParams.symbol) ? orderParams.symbol : string.Empty;

                this.WinTitle = InputSecurityID + " " + symbol;

                Data = orderParams.Clone() as OrderParamProp;
                originalData = orderParams.Clone() as OrderParamProp;

            }
            else
            {
                Data = new OrderParamProp(null);
                InitData();

            }

            this.SetBasePriority();
            this.ExeBondPreList.Add("0", "行权");
            this.ExeBondPreList.Add("1", "到期");

            this.BaseERReSendTypeList.Add("1", "全量重发");

            this.BaseERReSendTypeList.Add("2", "余量重发");

            this.BaseSettleTypeList.Add("1", "T+0");
            this.BaseSettleTypeList.Add("2", "T+1");
            //if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
            //{
            //    this.AlgoMidTypeList.Add("1", "双边市场中间价");
            //}

            this.AlgoMidTypeList.Add("2", "中债估值");
            //if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
            //{
            //    this.AlgoMidTypeList.Add("3", "自定义%");
            //}

            //this.DefenReferMarketTypeList.Add("1", "双边");
            //this.DefenReferMarketTypeList.Add("2", "X-Bond");
            //this.DefenReferMarketTypeList.Add("4", "CFETS");
            //this.DefenReferMarketTypeList.Add("3", "全市场");
            if (MMsheConstants.SSE_VENUE.Equals(Data.Venuetypegw))
            {
                this.DefenReferMarketTypeList.Add("5", "上新债");
            }
            else if (MMsheConstants.SSF_VENUE.Equals(Data.Venuetypegw))
            {
                this.DefenReferMarketTypeList.Add("6", "上固收");
            }
            else if (MMsheConstants.SZF_VENUE.Equals(Data.Venuetypegw))
            {
                this.DefenReferMarketTypeList.Add("7", "深点击");
            }
            else if (MMsheConstants.SZE_VENUE.Equals(Data.Venuetypegw))
            {
                this.DefenReferMarketTypeList.Add("8", "深匹配");
            }
            this.DefenReferLevelList.Add("1", "第一档");
            this.DefenReferLevelList.Add("2", "第二档");
            this.DefenReferLevelList.Add("3", "第三档");
            this.DefenReferLevelList.Add("4", "第四档");
            this.DefenReferLevelList.Add("5", "第五档");

        }

        private void InitData()
        {
            Data.bidBasePriority = "1,2";
            Data.ofrBasePriority = "1,2";
            Data.bidQuoteModel = "1";
            Data.ofrQuoteModel = "1";
            Data.BaseERValue = "0";

            Data.bidBaseQty = "10";
            Data.bidBaseReSendValue = "0";
            Data.bidBaseWidenValue = "0";

            Data.BaseERReSendType = "1";
            Data.baseSettleType = "1";

            Data.ofrBaseQty = "10";
            Data.ofrBaseReSendValue = "0";
            Data.ofrBaseWidenValue = "0";

            Data.bidAlgoDefenValue = "0.001";
            Data.bidAlgoSafeValue = "0";
            Data.bidAlgoTrigValue = "0";
            Data.ofrAlgoDefenValue = "0.001";
            Data.ofrAlgoSafeValue = "0";
            Data.ofrAlgoTrigValue = "0";

            Data.algoEnable = "0";
            Data.algoBaseEnable = "0";
            Data.algoBaseSecurityid = string.Empty;
            Data.algoMidType = "1";
            Data.algoMidOffsetValue = "0";
            Data.algoValue = "0.01";
            Data.algoCDCEnable = "0";
            Data.bidAlgoCDCValue = "0.01";
            Data.ofrAlgoCDCValue = "0.01";

            Data.posLmtEnable = "0";
            Data.bidPosValue = "0";
            Data.ofrPosValue = "0";
            Data.stepLength = "0.001";
            Data.bidDefenReferMarketType = "1";
            Data.bidDefenReferLevel = "1";
            Data.ofrDefenReferMarketType = "1";
            Data.ofrDefenReferLevel = "1";
            Data.exeBondPre = "0";
            //Data.exeBondFollow = "1";
            Data.dealClose = "0";
        }

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
                        this.BidBasePriority1Text = "跟随报价";
                        this.BidBasePriority2Text = "算法报价";

                        // TODO 这边需要切换相应的价格
                        this.BidBasePriority1Value = Data.bidDefenCalPrice;
                        this.BidBasePriority2Value = Data.bidAlgoCalPrice;
                    }
                    else
                    {
                        //算法优先
                        this.BidBasePriority1Text = "算法报价";
                        this.BidBasePriority2Text = "跟随报价";

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
                        //跟随优先
                        this.OfrBasePriority1Text = "跟随报价";
                        this.OfrBasePriority2Text = "算法报价";

                        // TODO 这边需要切换相应的价格
                        this.OfrBasePriority1Value = Data.ofrDefenCalPrice;
                        this.OfrBasePriority2Value = Data.ofrAlgoCalPrice;

                    }
                    else
                    {
                        //算法优先
                        this.OfrBasePriority1Text = "算法报价";
                        this.OfrBasePriority2Text = "跟随报价";

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

        public static MMsheOrderParamReq CheckSendFieldNull(MMsheOrderParamReq req)
        {
            MMsheOrderParamReq mq = req;
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
            if (string.IsNullOrEmpty(mq.bidAlgoSafeValue)) { mq.bidAlgoSafeValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidAlgoTrigValue)) { mq.bidAlgoTrigValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrAlgoDefenValue)) { mq.ofrAlgoDefenValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrAlgoSafeValue)) { mq.ofrAlgoSafeValue = string.Empty; }
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
            //if (string.IsNullOrEmpty(mq.maxFloorEnable)) { mq.maxFloorEnable = string.Empty; }
            //if (string.IsNullOrEmpty(mq.maxFloor)) { mq.maxFloor = string.Empty; }
            if (string.IsNullOrEmpty(mq.openQDM2Enable)) { mq.openQDM2Enable = string.Empty; }
            if (string.IsNullOrEmpty(mq.defenCDCEnable)) { mq.defenCDCEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidDefenCDCValue)) { mq.bidDefenCDCValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrDefenCDCValue)) { mq.ofrDefenCDCValue = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidDefenReferMarketType)) { mq.bidDefenReferMarketType = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidDefenReferLevel)) { mq.bidDefenReferLevel = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrDefenReferMarketType)) { mq.ofrDefenReferMarketType = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrDefenReferLevel)) { mq.ofrDefenReferLevel = string.Empty; }
            if (string.IsNullOrEmpty(mq.wallEnable)) { mq.wallEnable = string.Empty; }
            if (string.IsNullOrEmpty(mq.dealClose)) { mq.dealClose = string.Empty; }
            if (string.IsNullOrEmpty(mq.bidCDCDefenNetPrice)) { mq.bidCDCDefenNetPrice = string.Empty; }
            if (string.IsNullOrEmpty(mq.ofrCDCDefenNetPrice)) { mq.ofrCDCDefenNetPrice = string.Empty; }
            return mq;
        }

        private void DoConfirmBtnCommand()
        {
            if (InputSecurityID != null && InputSecurityID.Length > 0)
                Data.securityid = InputSecurityID;
            Data2Request(Data, OrderParamReq);

            Dictionary<string, string> errorList;
            errorList = CheckFieldIsEmpty();

            if (errorList.Count == 0)
            {
                if (InputSecurityID != null && InputSecurityID.Length > 0)
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
                        ThreadPoolHelper.InvokeThreadPool((o) =>
                        {
                            PResult result = STC.Common.Handler.Admin.MMsheHandler.Instance.mmUpdateParams(OrderParamReq);
                            if (result != null && result.Code == "0000")
                            {
                                errorMsg = "更新[" + InputSecurityID + "]参数成功！";
                                NeedPrompt = false;
                            }
                            else
                            {
                                NeedPrompt = true;
                                if (result != null && !string.IsNullOrEmpty(result.Msg))
                                {
                                    errorMsg = result.Msg;
                                }
                                else
                                {
                                    errorMsg = "服务器接收数据失败！";
                                }
                            }

                            //run = new Run(errorMsg);
                            //txtRun.Add(run);
                            //CustomMessageBox.ShowDialog(
                            //    result != null && result.Code == "0000" ? CustomMessageBox.EnumNotifyType.Info : CustomMessageBox.EnumNotifyType.Error,
                            //    txtRun, null, MyWindow);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (NeedPrompt)
                                {
                                    MessageBoxGUI msgBox = new MessageBoxGUI();
                                    msgBox.setWarnMsg(errorMsg, true);
                                    msgBox.ShowDialog();

                                    if (result == null)
                                    {
                                        return;
                                    }

                                    if (result.Code != "0000")
                                    {
                                        return;
                                    }
                                }

                                if (errorMsg.Equals("更新[" + InputSecurityID + "]参数成功！"))
                                {
                                    //MyWindow.DialogResult = true;
                                    MyWindow.Close();
                                }
                            });
                        });
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
                !errorList.ContainsKey("ofrAlgoDefenValue") && !errorList.ContainsKey("bidAlgoSafeValue") && !errorList.ContainsKey("ofrAlgoSafeValue") &&
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
                errorList.ContainsKey("ofrAlgoDefenValue") || errorList.ContainsKey("bidAlgoSafeValue") || errorList.ContainsKey("ofrAlgoSafeValue") ||
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

        public static void Data2Request(OrderParamProp data, MMsheOrderParamReq req)
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
                req.bidAlgoSafeValue = data.bidAlgoSafeValue;
                req.bidAlgoTrigValue = data.bidAlgoTrigValue;
                req.ofrAlgoDefenValue = data.ofrAlgoDefenValue;
                req.ofrAlgoSafeValue = data.ofrAlgoSafeValue;
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
                //req.maxFloorEnable = data.maxFloorEnable;
                //req.maxFloor = data.maxFloor;
                req.openQDM2Enable = data.openQDM2Enable;
                req.defenCDCEnable = data.defenCDCEnable;
                req.bidDefenCDCValue = data.bidDefenCDCValue;
                req.ofrDefenCDCValue = data.ofrDefenCDCValue;
                req.bidDefenReferMarketType = data.bidDefenReferMarketType;
                req.bidDefenReferLevel = data.bidDefenReferLevel;
                req.ofrDefenReferMarketType = data.ofrDefenReferMarketType;
                req.ofrDefenReferLevel = data.ofrDefenReferLevel;
                req.wallEnable = data.wallEnable;
                req.exeManualBid = data.exeManualBid;
                req.exeManualOfr = data.exeManualOfr;
                req.extAlgoMidDetailDefined = data.extAlgoMidDetailDefined;
                //req.exeBondPre = data.exeBondPre;
                //req.exeBondFollow = data.exeBondFollow;
                req.venues = data.Venues;
                req.venuetypegw = data.Venuetypegw;
                req.dealClose = data.dealClose;
                req.bidCDCDefenNetPrice = data.bidCDCDefenNetPrice;
                req.ofrCDCDefenNetPrice = data.ofrCDCDefenNetPrice;
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

            if (string.IsNullOrEmpty(OrderParamReq.securityid))
            {
                dic.Add("securityid", "债券代码为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.bidQuoteModel))
            {
                dic.Add("bidQuoteModel", "Bid方向报价模式为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrQuoteModel))
            {
                dic.Add("ofrQuoteModel", "Ofr方向报价模式为空");
            }


            if (string.IsNullOrEmpty(OrderParamReq.bidBaseReSendValue) || !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.bidBaseReSendValue, 1000, 0, 3))
            {
                dic.Add("bidBaseReSendValue", "Bid重发阈值为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrBaseReSendValue) || !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.ofrBaseReSendValue, 1000, 0, 3))
            {
                dic.Add("ofrBaseReSendValue", "Ofr重发阈值为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.bidBaseQty) || !MMsheCommonUtils.IsDecimal(OrderParamReq.bidBaseQty))
            {
                if (!"API".Equals(OrderParamReq.bidQuoteModel))
                {
                    dic.Add("bidBaseQty", "Bid单笔报量为空");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(OrderParamReq.bidBaseQty) && !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.bidBaseQty, 1000000, 10, 0))
                {
                    dic.Add("bidBaseQty_ZERO", $"bid单笔报量需大于等于10且小于等于1000000");
                }
            }

            if (string.IsNullOrEmpty(OrderParamReq.ofrBaseQty) || !MMsheCommonUtils.IsDecimal(OrderParamReq.ofrBaseQty))
            {
                if (!"API".Equals(OrderParamReq.ofrQuoteModel))
                {
                    dic.Add("ofrBaseQty", "Ofr单笔报量为空");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(OrderParamReq.ofrBaseQty) && !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.ofrBaseQty, 1000000, 10, 0))
                {
                    dic.Add("ofrBaseQty_ZERO", $"ofr单笔报量需大于等于10且小于等于1000000");
                }
            }

            if (string.IsNullOrEmpty(OrderParamReq.stepLength) || !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.stepLength, 100m, 0.001m, 3))
            {
                dic.Add("stepLength", "调整步长非法");
            }

            if (string.IsNullOrEmpty(OrderParamReq.baseSettleType) || !MMsheCommonUtils.IsDecimal(OrderParamReq.baseSettleType))
            {
                //清算速度已经选定默认值，不会为空
                dic.Add("baseSettleType", "清算速度为空");
            }

            {
                {
                    if (string.IsNullOrEmpty(OrderParamReq.manualBid) || !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.manualBid, 9999, -99.999m, 3))
                    {
                        dic.Add("manualBid", "Bid单笔报价为空");
                    }
                }

                {
                    if (string.IsNullOrEmpty(OrderParamReq.manualOfr) || !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.manualOfr, 9999, -99.999m, 3))
                    {
                        dic.Add("manualOfr", "Ofr单笔报价为空");
                    }
                }
            }
            if (string.IsNullOrEmpty(OrderParamReq.bidDefenReferMarketType) || !MMsheCommonUtils.IsDecimal(OrderParamReq.bidDefenReferMarketType))
            {
                //参考中间价类型已经有默认值
                dic.Add("bidDefenReferMarketType", "Bid跟随参考市场价为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.bidDefenReferLevel) || !MMsheCommonUtils.IsDecimal(OrderParamReq.bidDefenReferLevel))
            {
                //参考中间价类型已经有默认值
                dic.Add("bidDefenReferLevel", "Bid跟随参考市场价档位为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.ofrDefenReferMarketType) || !MMsheCommonUtils.IsDecimal(OrderParamReq.ofrDefenReferMarketType))
            {
                //参考中间价类型已经有默认值
                dic.Add("ofrDefenReferMarketType", "Ofr跟随参考市场价为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.ofrDefenReferLevel) || !MMsheCommonUtils.IsDecimal(OrderParamReq.ofrDefenReferLevel))
            {
                //参考中间价类型已经有默认值
                dic.Add("ofrDefenReferLevel", "Ofr跟随参考市场价档位为空");
            }
            {
                {
                    if (string.IsNullOrEmpty(OrderParamReq.bidAlgoDefenValue) || !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.bidAlgoDefenValue, 100, -100, 3))
                    {
                        dic.Add("bidAlgoDefenValue", "Bid跟随点为空");
                    }
                }

                {
                    if (string.IsNullOrEmpty(OrderParamReq.ofrAlgoDefenValue) || !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.ofrAlgoDefenValue, 100, -100, 3))
                    {
                        dic.Add("ofrAlgoDefenValue", "Ofr跟随点为空");
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
                    if (!MMsheCommonUtils.CheckDecimalValid(OrderParamReq.bidDefenCDCValue, 1000, 0, 2))
                    {
                        dic.Add("bidDefenCDCValue", "估值拉宽Bid非法");
                    }
                    if (!MMsheCommonUtils.CheckDecimalValid(OrderParamReq.ofrDefenCDCValue, 1000, 0, 2))
                    {
                        dic.Add("ofrDefenCDCValue", "估值拉宽Ofr非法");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(OrderParamReq.defenCDCEnable))
                    {
                        if (!string.IsNullOrEmpty(OrderParamReq.bidDefenCDCValue) && !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.bidDefenCDCValue, 1000, 0, 2))
                        {
                            dic.Add("bidDefenCDCValue", "估值拉宽Bid非法");
                        }
                        if (!string.IsNullOrEmpty(OrderParamReq.ofrDefenCDCValue) && !MMsheCommonUtils.CheckDecimalValid(OrderParamReq.ofrDefenCDCValue, 1000, 0, 2))
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
                case "exeManualBid":
                    if (originalData.exeManualBid != Data.exeManualBid) { bRet = true; }
                    break;
                case "exeManualOfr":
                    if (originalData.exeManualOfr != Data.exeManualOfr) { bRet = true; }
                    break;
            }

            return bRet;
        }
        public void setExeBondPre(string exeBondPre)
        {
            Data.exeBondPre = exeBondPre;
        }
        public void ResetWindowBorderBursh(string name)
        {
            switch (name)
            {
                case "manualBid_tb":
                    MyWindow.WarnmanualBid.Visibility = Visibility.Collapsed;

                    //if (MMsheCommonUtils.IsZero(Data.bidBasePrice) && !string.IsNullOrEmpty(Data.bidBasePrice))
                    //{
                    //    MyWindow.WarnbidBasePrice.Visibility = Visibility.Visible;
                    //}

                    if (IsUpdateField("manualBid"))
                    {
                        MyWindow.manualBid_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.manualBid_tb.Foreground = NormalForeColor;
                    }
                    /*if (!Data.IswallEnable)
                    {
                        Data.exeBondPre = "1";
                        CalBond(Data.securityid, "4", Data.baseSettleType, Data.manualBid, "1");
                    }*/
                    break;
                case "manualOfr_tb":
                    MyWindow.WarnmanualOfr.Visibility = Visibility.Collapsed;

                    //if (MMsheCommonUtils.IsZero(Data.ofrBasePrice) && !string.IsNullOrEmpty(Data.ofrBasePrice))
                    //{
                    //    MyWindow.WarnofrBasePrice.Visibility = Visibility.Visible;
                    //}

                    if (IsUpdateField("manualOfr"))
                    {
                        MyWindow.manualOfr_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.manualOfr_tb.Foreground = NormalForeColor;
                    }
                    /*if (!Data.IswallEnable)
                    {
                        Data.exeBondPre = "1";
                        CalBond(Data.securityid, "4", Data.baseSettleType, Data.manualOfr, "2");
                    }*/
                    break;

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
                        if (MMsheCommonUtils.IsZero(Data.bidBaseQty))
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
                        if (MMsheCommonUtils.IsZero(Data.ofrBaseQty))
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
                case "stepLength_tb":
                    if (IsUpdateField("stepLength"))
                    {
                        MyWindow.stepLength_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.stepLength_tb.Foreground = NormalForeColor;
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

                /*case "bidBaseWidenValue_tb":
                    if (IsUpdateField("bidBaseWidenValue"))
                    {
                        MyWindow.bidBaseWidenValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.bidBaseWidenValue_tb.Foreground = NormalForeColor;
                    }
                    break;*/

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

                /*case "ofrBaseWidenValue_tb":
                    if (IsUpdateField("ofrBaseWidenValue"))
                    {
                        MyWindow.ofrBaseWidenValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrBaseWidenValue_tb.Foreground = NormalForeColor;
                    }
                    break;*/

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

                /*case "bidAlgoSafeValue_tb":
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
                    break;*/

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

                /*case "ofrAlgoSafeValue_tb":
                    if (IsUpdateField("ofrAlgoSafeValue"))
                    {
                        MyWindow.ofrAlgoSafeValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrAlgoSafeValue_tb.Foreground = NormalForeColor;
                    }
                    break;

                case "ofrAlgoTrigValue_tb":
                    if (IsUpdateField("ofrAlgoTrigValue"))
                    {
                        MyWindow.ofrAlgoTrigValue_tb.Foreground = UpdateForeColor;
                    }
                    else
                    {
                        MyWindow.ofrAlgoTrigValue_tb.Foreground = NormalForeColor;
                    }
                    break;*/

                case "bond_txt":
                    MyWindow.bond_txt.BorderBrush = DefaultBorderBrush;
                    MyWindow.WarnalgoBaseSecurityid.Text = string.Empty;
                    MyWindow.WarnalgoBaseSecurityid.Visibility = Visibility.Collapsed;
                    if (Data.algoBaseEnable == "1") //&& (Data.bidQuoteModel == "Algorithm" || Data.ofrQuoteModel == "Algorithm")
                    {
                        if (string.IsNullOrEmpty(Data.algoBaseSecurityid))
                        {
                            //MyWindow.bond_txt.BorderBrush = RedBorderBrush;
                        }
                        else if (Data.algoBaseSecurityid == originalData.securityid)
                        {
                            MyWindow.WarnalgoBaseSecurityid.Text = "*基准券不能与当前券相同";
                            MyWindow.WarnalgoBaseSecurityid.Visibility = Visibility.Visible;
                        }

                        if (IsUpdateField("bond_txt"))
                        {
                            MyWindow.bond_txt.TextForeground = UpdateForeColor;
                        }
                        else
                        {
                            MyWindow.bond_txt.TextForeground = NormalForeColor;
                        }
                    }
                    else
                    {
                        MyWindow.bond_txt.TextForeground = InvalidForeColor;
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
                    //else
                    //{
                    //    MyWindow.algoMidOffsetValue_tb.BorderBrush = DefaultBorderBrush;
                    //    MyWindow.algoMidOffsetValue_tb.Foreground = InvalidForeColor;

                    //    MyWindow.algoValue_tb.BorderBrush = DefaultBorderBrush;
                    //    MyWindow.algoValue_tb.Foreground = InvalidForeColor;

                    //    //if (string.IsNullOrEmpty(Data.algoMidOffsetValue))
                    //    //{
                    //    //    Data.algoMidOffsetValue = originalData.algoMidOffsetValue;
                    //    //}

                    //    //if (string.IsNullOrEmpty(Data.algoValue))
                    //    //{
                    //    //    Data.algoValue = originalData.algoValue;
                    //    //}
                    //}
                    break;
                //case "bidAlgoCDCValue_tb":
                //case "ofrAlgoCDCValue_tb":
                //    MyWindow.WarnBidAlgoCDCValue.Visibility = Visibility.Collapsed;
                //    MyWindow.WarnOfrAlgoCDCValue.Visibility = Visibility.Collapsed;

                //    if (Data.algoCDCEnable == "1")
                //    {
                //        decimal bid;
                //        decimal ofr;

                //        if (string.IsNullOrEmpty(Data.bidAlgoCDCValue))
                //        {
                //            //MyWindow.bidAlgoCDCValue_tb.BorderBrush = RedBorderBrush;
                //            //MyWindow.bidAlgoCDCValue_tb.ToolTip = "不能为空";
                //        }
                //        else
                //        {
                //            //MyWindow.bidAlgoCDCValue_tb.BorderBrush = DefaultBorderBrush;
                //            //MyWindow.bidAlgoCDCValue_tb.ToolTip = string.Empty;

                //            if (!decimal.TryParse(Data.bidAlgoCDCValue, out bid) || bid == 0)
                //            {
                //                MyWindow.WarnBidAlgoCDCValue.Visibility = Visibility.Visible;
                //            }

                //            if (IsUpdateField("bidAlgoCDCValue"))
                //            {
                //                MyWindow.bidAlgoCDCValue_tb.Foreground = UpdateForeColor;
                //            }
                //            else
                //            {
                //                MyWindow.bidAlgoCDCValue_tb.Foreground = NormalForeColor;
                //            }
                //        }

                //        if (string.IsNullOrEmpty(Data.ofrAlgoCDCValue))
                //        {
                //            //MyWindow.ofrAlgoCDCValue_tb.BorderBrush = RedBorderBrush;
                //            //MyWindow.ofrAlgoCDCValue_tb.ToolTip = "不能为空";
                //        }
                //        else
                //        {
                //            //MyWindow.ofrAlgoCDCValue_tb.BorderBrush = DefaultBorderBrush;
                //            //MyWindow.ofrAlgoCDCValue_tb.ToolTip = string.Empty;

                //            if (!decimal.TryParse(Data.ofrAlgoCDCValue, out ofr) || ofr == 0)
                //            {
                //                MyWindow.WarnOfrAlgoCDCValue.Visibility = Visibility.Visible;
                //            }

                //            if (IsUpdateField("ofrAlgoCDCValue"))
                //            {
                //                MyWindow.ofrAlgoCDCValue_tb.Foreground = UpdateForeColor;
                //            }
                //            else
                //            {
                //                MyWindow.ofrAlgoCDCValue_tb.Foreground = NormalForeColor;
                //            }
                //        }                      
                //    }
                //    else
                //    {
                //        MyWindow.bidAlgoCDCValue_tb.BorderBrush = DefaultBorderBrush;
                //        MyWindow.bidAlgoCDCValue_tb.Foreground = InvalidForeColor;

                //        MyWindow.ofrAlgoCDCValue_tb.BorderBrush = DefaultBorderBrush;
                //        MyWindow.ofrAlgoCDCValue_tb.Foreground = InvalidForeColor;

                //        if (string.IsNullOrEmpty(Data.bidAlgoCDCValue))
                //        {
                //            Data.bidAlgoCDCValue = originalData.bidAlgoCDCValue;
                //        }

                //        if (string.IsNullOrEmpty(Data.ofrAlgoCDCValue))
                //        {
                //            Data.ofrAlgoCDCValue = originalData.ofrAlgoCDCValue;
                //        }
                //    }
                //    break;

                //case "bidPosValue_tb":
                //case "ofrPosValue_tb":
                //    if (Data.posLmtEnable == "1")
                //    {
                //        if (string.IsNullOrEmpty(Data.bidPosValue))
                //        {
                //            //MyWindow.bidPosValue_tb.BorderBrush = RedBorderBrush;
                //            //MyWindow.bidPosValue_tb.ToolTip = "不能为空";
                //        }
                //        else
                //        {
                //            //MyWindow.bidPosValue_tb.BorderBrush = DefaultBorderBrush;
                //            //MyWindow.bidPosValue_tb.ToolTip = string.Empty;

                //            if (IsUpdateField("bidPosValue"))
                //            {
                //                MyWindow.bidPosValue_tb.Foreground = UpdateForeColor;
                //            }
                //            else
                //            { MyWindow.bidPosValue_tb.Foreground = NormalForeColor; }
                //        }

                //        if (string.IsNullOrEmpty(Data.ofrPosValue))
                //        {
                //            //MyWindow.ofrPosValue_tb.BorderBrush = RedBorderBrush;
                //            //MyWindow.ofrPosValue_tb.ToolTip = "不能为空";
                //        }
                //        else
                //        {
                //            //MyWindow.ofrPosValue_tb.BorderBrush = DefaultBorderBrush;
                //            //MyWindow.ofrPosValue_tb.ToolTip = string.Empty;

                //            if (IsUpdateField("ofrPosValue")) { MyWindow.ofrPosValue_tb.Foreground = UpdateForeColor; }
                //            else { MyWindow.ofrPosValue_tb.Foreground = NormalForeColor; }
                //        }
                //    }
                //    else
                //    {
                //        MyWindow.bidPosValue_tb.BorderBrush = DefaultBorderBrush;
                //        MyWindow.bidPosValue_tb.Foreground = InvalidForeColor ;

                //        MyWindow.ofrPosValue_tb.BorderBrush = DefaultBorderBrush;
                //        MyWindow.ofrPosValue_tb.Foreground = InvalidForeColor;

                //        if (string.IsNullOrEmpty(Data.bidPosValue))
                //        {
                //            Data.bidPosValue = originalData.bidPosValue;
                //        }

                //        if (string.IsNullOrEmpty(Data.ofrPosValue))
                //        {
                //            Data.ofrPosValue = originalData.ofrPosValue;
                //        }
                //    }

                //    break;

                /*case "dealWidenUpperTime_tbx":
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
                    break;*/
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
                //case "manualBid_tb_exe":
                //    MyWindow.manualBid_tb_exe.BorderBrush = DefaultBorderBrush;
                //    if (IsUpdateField("exeManualBid"))
                //    {
                //        MyWindow.manualBid_tb_exe.Foreground = UpdateForeColor;
                //    }
                //    else
                //    {
                //        MyWindow.manualBid_tb_exe.Foreground = NormalForeColor;
                //    }
                    /*if (!Data.IswallEnable)
                    {
                        Data.exeBondPre = "0";
                        CalBond(Data.securityid, "7", Data.baseSettleType, Data.exeManualBid, "1");
                    }*/
                    break;
                //case "manualOfr_tb_exe":
                //    MyWindow.manualOfr_tb_exe.BorderBrush = DefaultBorderBrush;
                //    if (IsUpdateField("exeManualOfr"))
                //    {
                //        MyWindow.manualOfr_tb_exe.Foreground = UpdateForeColor;
                //    }
                //    else
                //    {
                //        MyWindow.manualOfr_tb_exe.Foreground = NormalForeColor;
                //    }

                    /*if (!Data.IswallEnable)
                    {
                        Data.exeBondPre = "0";
                        CalBond(Data.securityid, "7", Data.baseSettleType, Data.exeManualOfr, "2");
                    }*/
                    break;
                    //case "maxFloor_tb":
                    //    MyWindow.maxFloor_tb.BorderBrush = DefaultBorderBrush;
                    //    if (Data.maxFloorEnable == "1")
                    //    {
                    //        if (string.IsNullOrEmpty(Data.maxFloor))
                    //        {
                    //            MyWindow.maxFloor_tb.BorderBrush = RedBorderBrush;
                    //        }
                    //        else
                    //        {
                    //            MyWindow.maxFloor_tb.BorderBrush = DefaultBorderBrush;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        MyWindow.maxFloor_tb.BorderBrush = DefaultBorderBrush;
                    //    }
                    //    break;

            }
        }
        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        private void CalBond(string securityId, string calType, string settleType, string val, string side)
        {
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d))
                return;

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalReq requst = new CalReq();
                    requst.SecurityID = securityId;
                    requst.CalType = calType;//"4";
                    requst.SettlType = settleType;// "1";
                    requst.Value = decimal.Divide(CommonUtils.ConvertToDecimal(val), 100).ToString();// customPrice;
                    if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                        requst.CalPath = LocalEnv.ConfInfo.CalType;

                    CalResult result = CalHandler.Instance.CalBond(requst);
                    if (result != null && result.Data != null && result.IsSuccess())
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {

                            if (calType.Equals("4"))
                            {
                                if (side.Equals("1"))
                                {
                                    Data.exeManualBid = decimal.Multiply(result.Data.YieldToExecution, 100).ToString("F4");
                                }
                                else
                                {
                                    Data.exeManualOfr = decimal.Multiply(result.Data.YieldToExecution, 100).ToString("F4");
                                }
                            }
                            else if (calType.Equals("7"))//d
                            {
                                if (side.Equals("1"))
                                {
                                    Data.manualBid = decimal.Multiply(result.Data.Yield, 100).ToString("F4");
                                }
                                else
                                {
                                    Data.manualOfr = decimal.Multiply(result.Data.Yield, 100).ToString("F4");
                                }
                            }
                        }));
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
                finally
                {
                }
            });
        }
        public void UpdateSecurityCdc()
        {
            if (Data.algoBaseEnable == "1" && (Data.bidQuoteModel == "Algorithm" || Data.ofrQuoteModel == "Algorithm"))
            {
                if (!string.IsNullOrEmpty(Data.algoBaseSecurityid))
                {
                    MMsheOrderParamReq req = new MMsheOrderParamReq();
                    Data2Request(Data, req);

                    PResult result = STC.Common.Handler.Admin.MMsheHandler.Instance.mmUpdateSecurityCdc(req);

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
