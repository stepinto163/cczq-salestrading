using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Model.MMshe;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Mmshe.Plugin.Command;
using STC.Mmshe.Plugin.Utils;
using STC.Mmshe.Plugin.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Mmshe.Plugin.ViewModel
{
    public class BatchDetailViewModel : ViewModelBase
    {
        private BatchDetailView _myWindow;
        public BatchDetailView MyWindow
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
        private readonly OrderParamProp originalData;
        public readonly Brush DefaultBorderBrush = (Brush)(new BrushConverter()).ConvertFromString("#3C3C3C");
        public readonly Brush RedBorderBrush = (Brush)(new BrushConverter()).ConvertFromString("red");

        private readonly SolidColorBrush NormalForeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        private readonly SolidColorBrush UpdateForeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9C152"));
        private readonly SolidColorBrush InvalidForeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
        private OrderParamProp _Data;
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
                    _bidStrategyModeList.Add("Defend", "跟随");
                    //_bidStrategyModeList.Add("Algorithm", "算法");
                }
                return _bidStrategyModeList;
            }
        }
        //自定义
        private Dictionary<string, string> _ofrStrategyModeList = new Dictionary<string, string>();
        public Dictionary<string, string> ofrStrategyModeList
        {
            get
            {
                if (_ofrStrategyModeList.Count == 0)
                {
                    _ofrStrategyModeList.Add("Defend", "跟随");
                    //_ofrStrategyModeList.Add("Algorithm", "算法");
                }
                return _ofrStrategyModeList;
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
        public List<DataRowView> updateList = new List<DataRowView>();
        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }
        public ICommand ConfirmBtnCommand { get { return new RelayCommand(this.DoConfirmBtnCommand); } }

        public ICommand CancelBtnCommand { get { return new RelayCommand(this.DoCancelBtnCommand); } }
        public ICommand RestBtnCommand { get { return new RelayCommand(this.DoRestBtnCommand); } }

        private void DoRestBtnCommand()
        {
            Data = originalData.Clone() as OrderParamProp;
        }

        public BatchDetailViewModel(OrderParamProp orderParams)
        {
            if (orderParams != null)
            {
                Data = orderParams.Clone() as OrderParamProp;
                originalData = orderParams.Clone() as OrderParamProp;

            }
            else
            {
                Data = new OrderParamProp(null);
                MMsheCommonUtils.OrderParamPropToDefault(Data);
            }

            this.BaseERReSendTypeList.Add("1", "全量重发");
            this.BaseERReSendTypeList.Add("2", "余量重发");

            this.BaseSettleTypeList.Add("1", "T+0");
            this.BaseSettleTypeList.Add("2", "T+1");
            //if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
            //{
            //    this.AlgoMidTypeList.Add("1", "双边市场中间价");
            //}
            this.AlgoMidTypeList.Add("2", "中债估值(推荐)");
            //if ("1".Equals(LocalEnv.ConfInfo.openMMDHideSet))
            //{
            //    this.AlgoMidTypeList.Add("3", "自定义%");
            //}

            /*this.DefenReferMarketTypeList.Add("1", "双边");
            this.DefenReferMarketTypeList.Add("2", "X-Bond");
            this.DefenReferMarketTypeList.Add("4", "CFETS");
            this.DefenReferMarketTypeList.Add("3", "全市场");*/
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
        private void CloseClick()
        {
            MyWindow.Close();
        }
        private void DoCancelBtnCommand()
        {
            MyWindow.Close();
        }
        public bool NeedPrompt = true;
        private void DoConfirmBtnCommand()
        {
            Dictionary<string, string> errorList;
            errorList = CheckFieldIsEmpty();
            int sucessCount = 0;
            int failCount = 0;
            if (errorList.Count == 0)
            {
                ThreadPoolHelper.InvokeThreadPool((o) =>
                {
                    foreach (DataRowView drv in updateList)
                    {
                        MMsheOrderParamReq req = convertDataRequest(Data, drv);

                        if (!string.IsNullOrEmpty(this.WinTitle))
                        {
                            PResult result = STC.Common.Handler.Admin.MMsheHandler.Instance.mmUpdateParams(req);
                            if (result != null && result.Code == "0000")
                            {
                                sucessCount++;
                            }
                            else
                            {
                                failCount++;
                            }
                        }
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (failCount > 0)
                        {
                            if (NeedPrompt)
                            {
                                MessageBoxGUI msgBox = new MessageBoxGUI();
                                msgBox.setWarnMsg(string.Format("更新成功{0}，失败{1}", sucessCount, failCount), true);
                                msgBox.ShowDialog();
                            }
                        }
                        MyWindow.Close();
                    });
                });
            }
            else
            {
                MoveScrollPosition(errorList);
                return;
            }
        }

        private MMsheOrderParamReq convertDataRequest(OrderParamProp data, DataRowView drv)
        {
            MMsheOrderParamReq req = new MMsheOrderParamReq();
            if (data != null)
            {
                req.securityid = drv[MmsheListBoxViewModel.SECURITY_ID_FIELD] +"";
                req.symbol = drv[MmsheListBoxViewModel.SYMBOL_FIELD] + "";
                OrderParamProp param = drv[MmsheListBoxViewModel.PARAMS_FIELD] as OrderParamProp;
                req.bidQuoteModel = "多".Equals(data.bidQuoteModel)? param.bidQuoteModel: data.bidQuoteModel;
                req.bidBaseQty = "多".Equals(data.bidBaseQty) ? param.bidBaseQty : data.bidBaseQty;
                req.bidBaseReSendValue = "多".Equals(data.bidBaseReSendValue) ? param.bidBaseReSendValue : data.bidBaseReSendValue;
                req.bidBaseWidenValue = "多".Equals(data.bidBaseWidenValue) ? param.bidBaseWidenValue : data.bidBaseWidenValue;
                req.BaseERReSendType = "多".Equals(data.BaseERReSendType) ? param.BaseERReSendType : data.BaseERReSendType;
                req.baseSettleType = "多".Equals(data.baseSettleType) ? param.baseSettleType : data.baseSettleType;
                req.ofrQuoteModel = "多".Equals(data.ofrQuoteModel) ? param.ofrQuoteModel : data.ofrQuoteModel;
                req.ofrBaseQty = "多".Equals(data.ofrBaseQty) ? param.ofrBaseQty : data.ofrBaseQty;
                req.ofrBaseReSendValue = "多".Equals(data.ofrBaseReSendValue) ? param.ofrBaseReSendValue : data.ofrBaseReSendValue;
                //req.ofrBaseWidenValue = "多".Equals(data.ofrBaseWidenValue) ? param.ofrBaseWidenValue : data.ofrBaseWidenValue;
                req.bidAlgoDefenValue = "多".Equals(data.bidAlgoDefenValue) ? param.bidAlgoDefenValue : data.bidAlgoDefenValue;
                //req.bidAlgoSafeValue = "多".Equals(data.bidAlgoSafeValue) ? param.bidAlgoSafeValue : data.bidAlgoSafeValue;
                //req.bidAlgoTrigValue = "多".Equals(data.bidAlgoTrigValue) ? param.bidAlgoTrigValue : data.bidAlgoTrigValue;
                req.ofrAlgoDefenValue = "多".Equals(data.ofrAlgoDefenValue) ? param.ofrAlgoDefenValue : data.ofrAlgoDefenValue;
                //req.ofrAlgoTrigValue = "多".Equals(data.ofrAlgoTrigValue) ? param.ofrAlgoTrigValue : data.ofrAlgoTrigValue;
                //req.ofrAlgoSafeValue = "多".Equals(data.ofrAlgoSafeValue) ? param.ofrAlgoSafeValue : data.ofrAlgoSafeValue;
                //req.algoBaseEnable = "多".Equals(data.algoBaseEnable) ? param.algoBaseEnable : data.algoBaseEnable;
                //req.algoBaseSecurityid = "多".Equals(data.algoBaseSecurityid) ? param.algoBaseSecurityid : data.algoBaseSecurityid;
                //req.algoMidType = "多".Equals(data.algoMidType) ? param.algoMidType : data.algoMidType;
                //req.algoMidDetailDefined = "多".Equals(data.algoMidDetailDefined) ? param.algoMidDetailDefined : data.algoMidDetailDefined;
                //req.algoMidOffsetValue = "多".Equals(data.algoMidOffsetValue) ? param.algoMidOffsetValue : data.algoMidOffsetValue;
                //req.algoValue = "多".Equals(data.algoValue) ? param.algoValue : data.algoValue;
                //req.posFakEnable = "多".Equals(data.posFakEnable) ? param.posFakEnable : data.posFakEnable;
                //req.posFakSpread = "多".Equals(data.posFakSpread) ? param.posFakSpread : data.posFakSpread;
                //req.dealWidenLowerTime = "多".Equals(data.dealWidenLowerTime) ? param.dealWidenLowerTime : data.dealWidenLowerTime;
                //req.dealWidenUpperTime = "多".Equals(data.dealWidenUpperTime) ? param.dealWidenUpperTime : data.dealWidenUpperTime;
                ////req.maxFloorEnable = "多".Equals(data.maxFloorEnable) ? param.maxFloorEnable : data.maxFloorEnable;
                //req.maxFloor = "多".Equals(data.maxFloor) ? param.maxFloor : data.maxFloor;
                //req.openQDM2Enable = "多".Equals(data.openQDM2Enable) ? param.openQDM2Enable : data.openQDM2Enable;
                req.defenCDCEnable = "多".Equals(data.defenCDCEnable) ? param.defenCDCEnable : data.defenCDCEnable;
                req.bidDefenCDCValue = "多".Equals(data.bidDefenCDCValue) ? param.bidDefenCDCValue : data.bidDefenCDCValue;
                req.ofrDefenCDCValue = "多".Equals(data.ofrDefenCDCValue) ? param.ofrDefenCDCValue : data.ofrDefenCDCValue;
                req.bidDefenReferMarketType = "多".Equals(data.bidDefenReferMarketType) ? param.bidDefenReferMarketType : data.bidDefenReferMarketType;
                req.bidDefenReferLevel = "多".Equals(data.bidDefenReferLevel) ? param.bidDefenReferLevel : data.bidDefenReferLevel;
                req.ofrDefenReferMarketType = "多".Equals(data.ofrDefenReferMarketType) ? param.ofrDefenReferMarketType : data.ofrDefenReferMarketType;
                req.ofrDefenReferLevel = "多".Equals(data.ofrDefenReferLevel) ? param.ofrDefenReferLevel : data.ofrDefenReferLevel;
                req.wallEnable = "多".Equals(data.wallEnable) ? param.wallEnable : data.wallEnable;
                req.manualBid = param.manualBid;
                req.manualOfr = param.manualOfr;
                req.stepLength = param.stepLength;
                //req.exeManualBid = param.exeManualBid;
                //req.exeManualOfr = param.exeManualOfr;
                //req.exeBondPre = param.exeBondPre;
                //req.exeBondFollow = param.exeBondFollow;
                req.dealClose = "多".Equals(data.dealClose) ? param.dealClose : data.dealClose;
                req.venues = req.venuetypegw = data.Venues;
            }
            return req;
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
        private Dictionary<string, string> CheckFieldIsEmpty()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(Data.bidQuoteModel))
            {
                dic.Add("bidQuoteModel", "Bid方向报价模式为空");
            }
            if (string.IsNullOrEmpty(Data.ofrQuoteModel))
            {
                dic.Add("bidQuoteModel", "Bid方向报价模式为空");
            }
            if (string.IsNullOrEmpty(Data.bidBaseReSendValue) || (!"多".Equals(Data.bidBaseReSendValue) && !MMsheCommonUtils.CheckDecimalValid(Data.bidBaseReSendValue, 1000, 0, 2)))
            {
                dic.Add("bidBaseReSendValue", "Bid重发阈值为空");
            }
            if (string.IsNullOrEmpty(Data.ofrBaseReSendValue) || (!"多".Equals(Data.ofrBaseReSendValue) && !MMsheCommonUtils.CheckDecimalValid(Data.ofrBaseReSendValue, 1000, 0, 2)))
            {
                dic.Add("ofrBaseReSendValue", "Ofr重发阈值为空");
            }

            if (string.IsNullOrEmpty(Data.bidBaseQty) || (!"多".Equals(Data.bidBaseQty) && !MMsheCommonUtils.IsDecimal(Data.bidBaseQty)))
            {
                if (!"API".Equals(Data.bidQuoteModel))
                {
                    dic.Add("bidBaseQty", "Bid单笔报量为空");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Data.bidBaseQty) && !"多".Equals(Data.bidBaseQty) && !MMsheCommonUtils.CheckDecimalValid(Data.bidBaseQty, 1000000, 10, 0))
                {
                    dic.Add("bidBaseQty", $"bid单笔报量需大于等于10且小于等于1000000");
                }
            }

            if (string.IsNullOrEmpty(Data.ofrBaseQty) || (!"多".Equals(Data.ofrBaseQty) && !MMsheCommonUtils.IsDecimal(Data.ofrBaseQty)))
            {
                if (!"API".Equals(Data.ofrQuoteModel))
                {
                    dic.Add("ofrBaseQty", "Ofr单笔报量为空");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Data.ofrBaseQty) && !"多".Equals(Data.ofrBaseQty) && !MMsheCommonUtils.CheckDecimalValid(Data.ofrBaseQty, 1000000, 10, 0))
                {
                    dic.Add("ofrBaseQty", $"ofr单笔报量需大于等于10且小于等于1000000");
                }
            }
            if (string.IsNullOrEmpty(Data.baseSettleType) || (!"多".Equals(Data.baseSettleType) && !MMsheCommonUtils.IsDecimal(Data.baseSettleType)))
            {
                dic.Add("baseSettleType", "清算速度为空");
            }

            if (string.IsNullOrEmpty(Data.bidDefenReferMarketType) || (!"多".Equals(Data.bidDefenReferMarketType) && !MMsheCommonUtils.IsDecimal(Data.bidDefenReferMarketType)))
            {
                //参考中间价类型已经有默认值
                dic.Add("bidDefenReferMarketType", "Bid跟随参考市场价为空");
            }
            if (string.IsNullOrEmpty(Data.bidDefenReferLevel) || (!"多".Equals(Data.bidDefenReferLevel) && !MMsheCommonUtils.IsDecimal(Data.bidDefenReferLevel)))
            {
                //参考中间价类型已经有默认值
                dic.Add("bidDefenReferLevel", "Bid跟随参考市场价档位为空");
            }
            if (string.IsNullOrEmpty(Data.ofrDefenReferMarketType) || (!"多".Equals(Data.ofrDefenReferMarketType) && !MMsheCommonUtils.IsDecimal(Data.ofrDefenReferMarketType)))
            {
                //参考中间价类型已经有默认值
                dic.Add("ofrDefenReferMarketType", "Ofr跟随参考市场价为空");
            }
            if (string.IsNullOrEmpty(Data.ofrDefenReferLevel) || (!"多".Equals(Data.ofrDefenReferLevel) && !MMsheCommonUtils.IsDecimal(Data.ofrDefenReferLevel)))
            {
                //参考中间价类型已经有默认值
                dic.Add("ofrDefenReferLevel", "Ofr跟随参考市场价档位为空");
            }
            {
                //报价模式为防守检查 Defend(防守) 

                {
                    if (string.IsNullOrEmpty(Data.bidAlgoDefenValue) || (!"多".Equals(Data.bidAlgoDefenValue) && !MMsheCommonUtils.CheckDecimalValid(Data.bidAlgoDefenValue, 100, -100, 3)))
                    {
                        dic.Add("bidAlgoDefenValue", "Bid跟随点为空");
                    }

                }
                {
                    if (string.IsNullOrEmpty(Data.ofrAlgoDefenValue) || (!"多".Equals(Data.ofrAlgoDefenValue) && !MMsheCommonUtils.CheckDecimalValid(Data.ofrAlgoDefenValue, 100, -100, 3)))
                    {
                        dic.Add("ofrAlgoDefenValue", "Ofr跟随点为空");
                    }

                }
            }
           
            if (string.IsNullOrEmpty(Data.defenCDCEnable))
            {

            }
            else
            {
                if ("1".Equals(Data.defenCDCEnable))
                {
                    if ((!"多".Equals(Data.bidDefenCDCValue) && !MMsheCommonUtils.CheckDecimalValid(Data.bidDefenCDCValue, 1000, 0, 2)))
                    {
                        dic.Add("bidDefenCDCValue", "估值拉宽Bid非法");
                    }
                    if ((!"多".Equals(Data.ofrDefenCDCValue) && !MMsheCommonUtils.CheckDecimalValid(Data.ofrDefenCDCValue, 1000, 0, 2)))
                    {
                        dic.Add("ofrDefenCDCValue", "估值拉宽Ofr非法");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Data.defenCDCEnable))
                    {
                        if (!string.IsNullOrEmpty(Data.bidDefenCDCValue) && (!"多".Equals(Data.bidDefenCDCValue) && !MMsheCommonUtils.CheckDecimalValid(Data.bidDefenCDCValue, 1000, 0, 2)))
                        {
                            dic.Add("bidDefenCDCValue", "估值拉宽Bid非法");
                        }
                        if (!string.IsNullOrEmpty(Data.ofrDefenCDCValue) && (!"多".Equals(Data.ofrDefenCDCValue) && !MMsheCommonUtils.CheckDecimalValid(Data.ofrDefenCDCValue, 1000, 0, 2)))
                        {
                            dic.Add("ofrDefenCDCValue", "估值拉宽Ofr非法");
                        }
                    }
                }
            }

            return dic;
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

                    if (!string.IsNullOrEmpty(Data.bidBaseQty) && !"多".Equals(Data.bidBaseQty))
                    {
                        if (MMsheCommonUtils.IsZero(Data.bidBaseQty))
                        {
                            MyWindow.WarnBidBaseQty.Text = "*<Bid>设置需大于0";
                            MyWindow.WarnBidBaseQty.Visibility = Visibility.Visible;
                        }
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

                    if (!string.IsNullOrEmpty(Data.ofrBaseQty) && !"多".Equals(Data.ofrBaseQty))
                    {
                        if (MMsheCommonUtils.IsZero(Data.ofrBaseQty))
                        {
                            MyWindow.WarnOfrBaseQty.Text = "*<Ofr>设置需大于0";
                            MyWindow.WarnOfrBaseQty.Visibility = Visibility.Visible;
                        }
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
                    //if (IsUpdateField("bidBaseWidenValue"))
                    //{
                    //    MyWindow.bidBaseWidenValue_tb.Foreground = UpdateForeColor;
                    //}
                    //else
                    //{
                    //    MyWindow.bidBaseWidenValue_tb.Foreground = NormalForeColor;
                    //}
                    //break;

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
                    //if (IsUpdateField("ofrBaseWidenValue"))
                    //{
                    //    MyWindow.ofrBaseWidenValue_tb.Foreground = UpdateForeColor;
                    //}
                    //else
                    //{
                    //    MyWindow.ofrBaseWidenValue_tb.Foreground = NormalForeColor;
                    //}
                    //break;

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
                    //MyWindow.WarndealWiden.Visibility = Visibility.Collapsed;
                    //if (!string.IsNullOrEmpty(Data.dealWidenLowerTime) && !string.IsNullOrEmpty(Data.dealWidenUpperTime))
                    //{
                    //    if (decimal.Parse(Data.dealWidenUpperTime) < decimal.Parse(Data.dealWidenLowerTime))
                    //    {
                    //        MyWindow.WarndealWiden.Text = "*下限应该小于上限";
                    //        MyWindow.WarndealWiden.Visibility = Visibility.Visible;
                    //    }
                    //}
                    //break;

                case "dealWidenLowerTime_tbx":
                    //MyWindow.WarndealWiden.Visibility = Visibility.Collapsed;
                    //if (!string.IsNullOrEmpty(Data.dealWidenUpperTime) && !string.IsNullOrEmpty(Data.dealWidenLowerTime))
                    //{
                    //    if (decimal.Parse(Data.dealWidenLowerTime) > decimal.Parse(Data.dealWidenUpperTime))
                    //    {
                    //        MyWindow.WarndealWiden.Text = "*下限应该小于上限";
                    //        MyWindow.WarndealWiden.Visibility = Visibility.Visible;
                    //    }
                    //}
                    //break;
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
            }

            return bRet;
        }
        public void UpdateSecurityCdc()
        {
            if (Data.algoBaseEnable == "1" && (Data.bidQuoteModel == "Algorithm" || Data.ofrQuoteModel == "Algorithm"))
            {
                if (!string.IsNullOrEmpty(Data.algoBaseSecurityid))
                {
                    DataRowView drv = updateList.Where(a => a[MmsheListBoxViewModel.SECURITY_ID_FIELD] == Data.algoBaseSecurityid).FirstOrDefault();
                    MMsheOrderParamReq req = convertDataRequest(Data, drv);
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
    }
}
