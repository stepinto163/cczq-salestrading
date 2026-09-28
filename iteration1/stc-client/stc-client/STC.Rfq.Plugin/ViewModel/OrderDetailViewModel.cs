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
using STC.Rfq.Plugin.Utils;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Rfq.Plugin.View.Component;
using STC.Rfq.Plugin.View;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using STC.Rfq.Plugin.Validation;
using System.Windows.Data;
using STC.Common.View.UI;
using STC.Common.Utils;
using STC.Rfq.Plugin.Command;
using STC.Common.Model.RFQ;
using STC.Common.Handler.RFQ;
using Microsoft.Expression.Shapes;
using STC.Common.View.Component;
using STC.Rfq.Plugin.Utils.Data;

namespace STC.Rfq.Plugin.ViewModel
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

        private OrderParamProp _Data = new OrderParamProp();
        public OrderParamProp Data
        {
            get { return this._Data; }

            set
            {
                this._Data = value;
                this.OnPropertyChanged();
            }
        }

        private RfqstgViewModel _OrderParamReq = new RfqstgViewModel();
        /// <summary>
        /// 请求
        /// </summary>
        public RfqstgViewModel OrderParamReq
        {
            get { return _OrderParamReq; }
            set
            {
                _OrderParamReq = value;
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


        private Dictionary<string, string> _validSideList = new Dictionary<string, string>();
        public Dictionary<string, string> ValidSideList
        {
            get { return _validSideList; }
            set
            {
                _validSideList = value;
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

        /// <summary>
        /// 有效时长列表
        /// </summary>
        private Dictionary<string, string> _baseValidUntilTime = new Dictionary<string, string>();
        public Dictionary<string, string> BaseValidUntilTime
        {
            get { return this._baseValidUntilTime; }
            set
            {
                this._baseValidUntilTime = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }

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

        public OrderDetailViewModel(OrderParamProp orderParams)
        {
            if (orderParams != null)
            {
                InputSecurityID = !string.IsNullOrEmpty(orderParams.securityid) ? orderParams.securityid : string.Empty;
                string symbol = !string.IsNullOrEmpty(orderParams.symbol) ? orderParams.symbol : string.Empty;

                this.WinTitle = InputSecurityID + " " + symbol;

                Data = orderParams;
                originalData = orderParams;
            }
            else
            {
                return;
            }

            this.BaseSettleTypeList = RfqGroupHandler.Instance.BaseSettleTypeList;
            this.AlgoMidTypeList = RfqGroupHandler.Instance.AlgoMidTypeList;
            this.ValidSideList = RfqGroupHandler.Instance.ValidSideList;
            this.BaseValidUntilTime = RfqGroupHandler.Instance.BaseValidUntilTime;
        }

        private void DoCancelBtnCommand()
        {
            MyWindow.Close();
        }

        /// <summary>
        /// 修改或新增策略参数
        /// </summary>
        private void DoConfirmBtnCommand()
        {
            if (InputSecurityID != null && InputSecurityID.Length > 0)
                Data.securityid = InputSecurityID;

            Dictionary<string, string> errorList;
            errorList = CheckFieldIsEmpty(Data);

            if (errorList.Count == 0)
            {
                if (InputSecurityID != null && InputSecurityID.Length > 0)
                {
                    string errorMsg = string.Empty;
                    //OrderParamReq = CheckSendFieldNull(Data);
                    RfqstgActReq req = new RfqstgActReq();
                    req.action = this.IsAddBond ? 1 : 2;
                    req.ids = new List<string>();
                    RfqstgActStrategyReq rfv = JsonConvert.DeserializeObject<RfqstgActStrategyReq>(JsonConvert.SerializeObject(Data));
                    //如果BID方向有效日期，OFR方向有效日期 为 null ，则改为 "";
                    rfv.validDateBid = string.IsNullOrEmpty(rfv.validDateBid) ? "" : rfv.validDateBid;
                    rfv.validDateOfr = string.IsNullOrEmpty(rfv.validDateOfr) ? "" : rfv.validDateOfr;
                    rfv.inf1 = JsonConvert.SerializeObject(Data);
                    rfv.username = LocalEnv.UserInfo.UserName; //用户名
                    rfv.ip = EngineHelper.GetIp();
                    rfv.terminal = LocalEnv.ServerInfo.Version; // 终端
                    rfv.closeby = LocalEnv.UserInfo.UserName; //用户名
                    rfv.remark = "";
                    req.strategy = rfv;

                    //if (this.IsAddBond)
                    //{
                    //MyWindow.DialogResult = true;
                    //MyWindow.Close();
                    //}
                    { 
                        ThreadPoolHelper.InvokeThreadPool((o) =>
                        {
                            PResult result = RfqHandler.Instance.RfqUpdatestgAct(req);
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

                                    CustomMessageBox.Info(errorMsg, "信息", MyWindow);
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
                CustomMessageBox.Info(string.Join("\r\n", errorList), "信息", MyWindow);
                return;
            }

            //MyWindow.DialogResult = true;
            //MyWindow.Close();
        }

        private bool CheckQtyIsThousand(string data)
        {
            bool bRet = false;

            decimal d;

            if (decimal.TryParse(data,out d))
            {
                if (d >= 1000 && d % 1000 ==0)
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        private Dictionary<string,string> CheckFieldIsEmpty(OrderParamProp OrderParamReq)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            // ====== 必查项START ====================

            if (string.IsNullOrEmpty(OrderParamReq.securityid))
            {
                dic.Add("securityid","债券代码为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.strategyTypeBid))
            {
                dic.Add("strategyTypeBid", "Bid方向报价模式为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.strategyTypeOfr))
            {
                dic.Add("strategyTypeOfr", "Ofr方向报价模式为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.resendThresholdBid) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.resendThresholdBid, 1000,0,4))
            {
                dic.Add("resendThresholdBid", "Bid重发阈值为空");
            }
            if (string.IsNullOrEmpty(OrderParamReq.resendThresholdOfr) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.resendThresholdOfr, 1000,0,4))
            {
                dic.Add("resendThresholdOfr", "Ofr重发阈值为空");
            }

            if (string.IsNullOrEmpty(OrderParamReq.quantityBid) || !RfqCommonUtils.IsDecimal(OrderParamReq.quantityBid))
            {
                dic.Add("quantityBid", "Bid单笔报量为空");
            }
            else
            {
                if (!string.IsNullOrEmpty(OrderParamReq.maxQtyBid))
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

            if (string.IsNullOrEmpty(OrderParamReq.quantityOfr) || !RfqCommonUtils.IsDecimal(OrderParamReq.quantityOfr))
            {
                dic.Add("quantityOfr", "Ofr单笔报量为空");
                //MyWindow.WarnOfrBaseQty.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!string.IsNullOrEmpty(OrderParamReq.maxQtyBid))
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

            if (string.IsNullOrEmpty(OrderParamReq.settltype) || !RfqCommonUtils.IsDecimal(OrderParamReq.settltype))
            {
                //清算速度已经选定默认值，不会为空
                dic.Add("baseSettleType", "清算速度为空");
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
                    if (string.IsNullOrEmpty(OrderParamReq.manualBid) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.manualBid, 9999, -99.9999m, 4))
                    {
                        dic.Add("manualBid", "Bid单笔报价为空");
                    }
                    //else if (MMCommonUtils.IsZero(OrderParamReq.bidBasePrice))
                    //{
                    //    dic.Add("bidBasePrice_ZERO", "Bid单笔报价为0");
                    //}
                }

                //if (OrderParamReq.ofrQuoteModel == "Manual")
                {
                    if (string.IsNullOrEmpty(OrderParamReq.manualOfr) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.manualOfr,9999,-99.9999m,4))
                    {
                        dic.Add("manualOfr", "Ofr单笔报价为空");
                    }
                    //else if (MMCommonUtils.IsZero(OrderParamReq.ofrBasePrice))
                    //{
                    //    dic.Add("ofrBasePrice_ZERO", "Ofr单笔报价为0");
                    //}
                }
            }


            if (string.IsNullOrEmpty(OrderParamReq.algoSpread) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.algoSpread, 1000, 0.01m, 2))
            {
                dic.Add("algoValue", "算法点差为空");
            }
            else
            {
                decimal d;
                if (!decimal.TryParse(OrderParamReq.algoSpread, out d) || d == 0)
                {
                    if (d <= 0)
                    {
                        dic.Add("algoSpread", "算法点差必须大于0");
                    }
                }
            }

            //if (OrderParamReq.bidQuoteModel == "Defend" || OrderParamReq.ofrQuoteModel == "Defend")
            {
                //报价模式为防守检查 Defend(防守) 

                //if (OrderParamReq.bidQuoteModel == "Defend")
                //{
                //    if (string.IsNullOrEmpty(OrderParamReq.bidAlgoDefenValue) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.bidAlgoDefenValue,1000,0,2))
                //    {
                //        dic.Add("bidAlgoDefenValue", "Bid防守点为空");
                //        //MyWindow.bidAlgoDefenValue_bd.BorderBrush = RedBorderBrush;
                //    }

                //    if (string.IsNullOrEmpty(OrderParamReq.bidAlgoSafeValue) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.bidAlgoSafeValue,1000,0,2))
                //    {
                //        dic.Add("bidAlgoSafeValue", "Bid安全点为空");
                //        //MyWindow.bidAlgoSafeValue_bd.BorderBrush = RedBorderBrush;
                //    }

                //    if (string.IsNullOrEmpty(OrderParamReq.bidAlgoTrigValue) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.bidAlgoTrigValue,1000,0,2))
                //    {
                //        dic.Add("bidAlgoTrigValue", "Bid触墙拉宽点为空");
                //        //MyWindow.bidAlgoTrigValue_bd.BorderBrush = RedBorderBrush;
                //    }
                //}

                //if (OrderParamReq.ofrQuoteModel == "Defend")
                //{
                //    if (string.IsNullOrEmpty(OrderParamReq.ofrAlgoDefenValue) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.ofrAlgoDefenValue,1000,0,2))
                //    {
                //        dic.Add("ofrAlgoDefenValue", "Ofr防守点为空");
                //        //MyWindow.ofrAlgoDefenValue_bd.BorderBrush = RedBorderBrush;
                //    }

                //    if (string.IsNullOrEmpty(OrderParamReq.ofrAlgoSafeValue) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.ofrAlgoSafeValue,1000,0,2))
                //    {
                //        dic.Add("ofrAlgoSafeValue", "Ofr安全点为空");
                //        //MyWindow.ofrAlgoSafeValue_bd.BorderBrush = RedBorderBrush;
                //    }

                //    if (string.IsNullOrEmpty(OrderParamReq.ofrAlgoTrigValue) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.ofrAlgoTrigValue,1000,0,2))
                //    {
                //        dic.Add("ofrAlgoTrigValue", "Ofr触墙拉宽点为空");
                //        //MyWindow.ofrAlgoTrigValue_bd.BorderBrush = RedBorderBrush;
                //    }
                //}
            }

            //if (OrderParamReq.bidQuoteModel == "Algorithm" || OrderParamReq.ofrQuoteModel == "Algorithm")
            //{
            //    //报价模式为算法检查 Algorithm(算法)

            //    if (string.IsNullOrEmpty(OrderParamReq.algoMidType) || !RfqCommonUtils.IsDecimal(OrderParamReq.algoMidType))
            //    {
            //        //参考中间价类型已经有默认值
            //        dic.Add("algoMidType", "参考中间价类型为空");
            //    }
            //    else
            //    {
            //        if (OrderParamReq.algoMidType == "3")
            //        {
            //            //自定义
            //            if (string.IsNullOrEmpty(OrderParamReq.algoMidDetailDefined) || !RfqCommonUtils.CheckDecimalValid(OrderParamReq.algoMidDetailDefined,1000,0,4))
            //            {
            //                //MyWindow.algoMidDetailDefined_bd.BorderBrush = RedBorderBrush;
            //                if (!dic.ContainsKey("algoMidDetailDefined"))
            //                {
            //                    dic.Add("algoMidDetailDefined", "参考中间价类型自定义值为空");
            //                }
            //            }
            //            else
            //            {
            //                MyWindow.WarnAlgoMidDetailDefined.Visibility = Visibility.Collapsed;
            //                //decimal d;
            //                //if (!decimal.TryParse(OrderParamReq.algoMidDetailDefined, out d) || d == 0)
            //                //{
            //                //    MyWindow.WarnAlgoMidDetailDefined.Visibility = Visibility.Visible;
            //                //    dic.Add("algoMidDetailDefined_ZERO", "参考中间价类型自定义值为0");
            //                //}
            //            }

            //        }
            //    }

            
            //}

            //if (!string.IsNullOrEmpty(OrderParamReq.algoBaseEnable) || !RfqCommonUtils.IsDecimal(OrderParamReq.algoBaseEnable))
            //{
            //    //使用基准债券0：否、1：是
            //    if (OrderParamReq.algoBaseEnable == "1")
            //    {
            //        if (string.IsNullOrEmpty(OrderParamReq.algoBaseSecurityid))
            //        {
            //            if (!dic.ContainsKey("algoBaseSecurityid"))
            //            {
            //                dic.Add("algoBaseSecurityid", "启用基准债券值为空");
            //            }
            //            MyWindow.bond_txt.BorderBrush = RedBorderBrush;
            //        }
            //        else if (OrderParamReq.algoBaseSecurityid == originalData.securityid)
            //        {
            //            if (!dic.ContainsKey("algoBaseSecurityid_SAME"))
            //            {
            //                dic.Add("algoBaseSecurityid_SAME", "基准债券与当前券相同");
            //            }
            //        }
            //        else if (OrderParamReq.algoBaseSecurityid != MyWindow.bond_txt.SearchTermTextBox.Text)
            //        {

            //        }
            //    }
            //}

            //if (string.IsNullOrEmpty(OrderParamReq.algoBaseSecurityid))
            //{
            //    //基准债券值是否为空
            //    //MyWindow.bond_txt.BorderBrush = DefaultBorderBrush;

            //    if (Data.algoBaseEnable == "1")
            //    {
            //        if (string.IsNullOrEmpty(OrderParamReq.algoBaseSecurityid))
            //        {
            //            MyWindow.bond_txt.BorderBrush = RedBorderBrush;
            //            if (!dic.ContainsKey("algoBaseSecurityid"))
            //            {
            //                dic.Add("algoBaseSecurityid", "基准债券值为空");
            //            }
            //        }
            //        else if (OrderParamReq.algoBaseSecurityid == originalData.securityid)
            //        {
            //            if (!dic.ContainsKey("algoBaseSecurityid_SAME"))
            //            {
            //                dic.Add("algoBaseSecurityid_SAME", "基准债券与当前券相同");
            //            }
            //        }
            //    }
            //}

            //if (OrderParamReq.bidQuoteModel == "API" || OrderParamReq.ofrQuoteModel == "API")
            //{
            //    //报价模式为接口检查 API("接口")
            //    // 暂不做特殊的检查
            //}

            //if (string.IsNullOrEmpty(OrderParamReq.posFakEnable))
            //{
            //    //本方双边报价被点，若此时全市场存在比本方成交价更优行情，主动发起点价
            //    //不做检查
            //}
            //else 
            //{
            //    if ("1".Equals(OrderParamReq.posFakEnable))
            //    {
            //        if (!RfqCommonUtils.CheckDecimalValid(OrderParamReq.posFakSpread, 99.99m, 0, 2))
            //        {
            //            dic.Add("posFakSpread", "内部单点差非法");
            //        }
            //    }
            //    else
            //    {
            //        if (!string.IsNullOrEmpty(OrderParamReq.posFakSpread)) 
            //        {
            //            if (!string.IsNullOrEmpty(OrderParamReq.posFakSpread) && !RfqCommonUtils.CheckDecimalValid(OrderParamReq.posFakSpread, 99.99m, 0, 2))
            //            {
            //                dic.Add("posFakSpread", "内部单点差非法");
            //            }
            //        }
            //    }
            //}
            //if (string.IsNullOrEmpty(OrderParamReq.maxFloorEnable))
            //{
            //    //本方双边报价被点，若此时全市场存在比本方成交价更优行情，主动发起点价
            //    //不做检查
            //}
            //else
            //{
            //    if ("1".Equals(OrderParamReq.maxFloorEnable))
            //    {
            //        if (!RfqCommonUtils.CheckDecimalValid(OrderParamReq.maxFloor, 999, 1, 0))
            //        {
            //            dic.Add("maxFloor", "冰山量非法");
            //        }
            //    }
            //    else
            //    {
            //        if (!string.IsNullOrEmpty(OrderParamReq.maxFloorEnable))
            //        {
            //            if (!string.IsNullOrEmpty(OrderParamReq.maxFloor)&&!RfqCommonUtils.CheckDecimalValid(OrderParamReq.maxFloor, 999, 1, 0))
            //            {
            //                dic.Add("maxFloor", "冰山量非法");
            //            }
            //        }
            //    }
            //}

            //if (string.IsNullOrEmpty(OrderParamReq.defenCDCEnable))
            //{
                
            //}
            //else
            //{
            //    if ("1".Equals(OrderParamReq.defenCDCEnable))
            //    {
            //        if (!RfqCommonUtils.CheckDecimalValid(OrderParamReq.bidDefenCDCValue, 1000, -1000, 0))
            //        {
            //            dic.Add("bidDefenCDCValue", "估值拉宽Bid非法");
            //        }
            //        if (!RfqCommonUtils.CheckDecimalValid(OrderParamReq.ofrDefenCDCValue, 1000, -1000, 0))
            //        {
            //            dic.Add("ofrDefenCDCValue", "估值拉宽Ofr非法");
            //        }
            //    }
            //    else
            //    {
            //        if (!string.IsNullOrEmpty(OrderParamReq.defenCDCEnable))
            //        {
            //            if (!string.IsNullOrEmpty(OrderParamReq.bidDefenCDCValue) && !RfqCommonUtils.CheckDecimalValid(OrderParamReq.bidDefenCDCValue, 1000, -1000, 0))
            //            {
            //                dic.Add("bidDefenCDCValue", "估值拉宽Bid非法");
            //            }
            //            if (!string.IsNullOrEmpty(OrderParamReq.ofrDefenCDCValue) && !RfqCommonUtils.CheckDecimalValid(OrderParamReq.ofrDefenCDCValue, 1000, -1000, 0))
            //            {
            //                dic.Add("ofrDefenCDCValue", "估值拉宽Ofr非法");
            //            }
            //        }
            //    }
            //}

            return dic;
        }

        /// <summary>
        /// 修改基准券
        /// </summary>
        public void UpdateSecurityCdc()
        {
            //if (Data.algoBaseEnable == "1" && (Data.bidQuoteModel == "Algorithm" || Data.ofrQuoteModel == "Algorithm"))
            //{
            //    if (!string.IsNullOrEmpty(Data.algoBaseSecurityid))
            //    {
            //        RfqstgViewModel req = new RfqstgViewModel();
            //        Data2Request(Data, req);

            //        PResult result = RfqHandler.Instance.RfqUpdateSecurityCdc(req);

            //        string errorMsg = string.Empty;
            //        if (result != null && result.Code == "0000")
            //        {
            //        }
            //        else
            //        {
            //            if (result != null && !string.IsNullOrEmpty(result.Msg))
            //            {
            //                errorMsg = result.Msg;
            //            }
            //            else
            //            {
            //                errorMsg = "发送基准券：服务器接收数据失败！";
            //            }
            //        }

            //        if (!string.IsNullOrEmpty(errorMsg))
            //        {
            //            MessageBoxGUI msgBox = new MessageBoxGUI();
            //            msgBox.setWarnMsg(errorMsg, true);
            //            msgBox.ShowDialog();
            //        }                   
            //    }
            //}
        }

        private void CloseClick()
        {
            MyWindow.Close();
        }
    }
}
