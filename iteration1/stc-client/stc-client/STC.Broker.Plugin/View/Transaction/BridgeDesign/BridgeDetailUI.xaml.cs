using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction.CrossBridge;

using STC.Frame.UI;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Broker.Plugin.Utils;
using STC.Common.Utils;
using STC.Common.Model.PT;
using STC.Common.Model.CRM;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    /// <summary>
    /// BridgeDetailUI.xaml 的交互逻辑
    /// 单桥要素修改
    /// </summary>
    public partial class BridgeDetailUI : UserControl
    {
        /// <summary>
        /// 桥的方向
        /// </summary>
        private bool isGvn;
        //获取订单对应方向的桥机构代码
        public string correspondingBridgeBankName;
        /// <summary>
        /// 当前桥
        /// </summary>
        public UserBridge CurrentBridge;

        /// <summary>
        /// 机构名
        /// </summary>
        private string _bankName = "";

        /// <summary>
        /// 机构交易员名
        /// </summary>
        private string _bankAgentName = "";

        /// <summary>
        /// 是否批量编辑
        /// </summary>
        public bool IsBatchEdit = false;
        /// <summary>
        /// 债券代码
        /// </summary>
        private string _goodsCode = "";

        /// <summary>
        /// 是否显示机构下拉列表
        /// </summary>
        private bool isShowInstList = true;

        /// <summary>
        /// 机构列表
        /// </summary>
        public ObservableCollection<BillingInstModel> InstList = new ObservableCollection<BillingInstModel>();

        BridgeOrderDetail _bridgeOrderDetail;

        public BridgeDetailUI()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 初始化控件
        /// </summary>
        /// <param name="orderDetail">成交单</param>
        /// <param name="isOfr">方向</param>
        /// <param name="userBridge">桥</param>
        public void Init(BridgeOrderDetail orderDetail, bool isOfr, UserBridge userBridge)
        {
            this.isGvn = isOfr;
            this.CurrentBridge = userBridge;
            this._goodsCode = orderDetail.SecurityID;
            _bridgeOrderDetail = orderDetail;
            if (userBridge != null)
            {
                //联系人机构
                tbBnakName.Text = userBridge.bankInstitutionname;
                //联系人
                tbBankAgentName.Text = userBridge.bankTradername;
                //计费
                tbFeeBankAgentName.Text = userBridge.feeInstitutionname;
                //联系方式
                tbContactType.Text = userBridge.contactType;
            }


            //对应方向的交易员
            if (isOfr)
            {
                //获取ofr方机构
                if (orderDetail.IsGVN)
                {
                    _bankName = orderDetail.InstAShow;
                    _bankAgentName = orderDetail.TraderName == null ? "" : orderDetail.TraderName;
                    //增加交易员显示 20200403 wym
                    string temp = string.Format("{0}({1})", orderDetail.InstAShow, orderDetail.TraderName);// orderDetail.TraderAInfo;
                    if (temp == "()")
                        tbInstTrader.Text = "";
                    else
                        tbInstTrader.Text = temp;
                }
                else
                {
                    _bankName = orderDetail.InstBShow;
                    _bankAgentName = orderDetail.CounterPartyTraderName == null ? "" : orderDetail.CounterPartyTraderName;
                    //增加交易员显示 20200403 wym
                    string temp = string.Format("{0}({1})", orderDetail.InstBShow, orderDetail.CounterPartyTraderName); //orderDetail.TraderBInfo;
                    if (temp == "()")
                        tbInstTrader.Text = "";
                    else
                        tbInstTrader.Text = temp;
                }

                tbSide.Text = "Ofr方";
                tbBnakName.Foreground = tbSide.Foreground = (Brush)FindResource("ColorBrush.Blue");
                if (ERemarkShow.不显示.EqualsToValue(orderDetail.BridgeRemarkShow))
                {
                    cbShowRemark.IsChecked = true;
                }
                //备注名称显示
                tbRemarksName.Text = "备注Ofr:";
            }
            else
            {
                //获取bid方机构
                if (orderDetail.IsGVN)
                {
                    _bankName = orderDetail.InstBShow;
                    _bankAgentName = orderDetail.CounterPartyTraderName == null ? "" : orderDetail.CounterPartyTraderName;
                    //增加交易员显示 20200403 wym
                    string temp = string.Format("{0}({1})", orderDetail.InstBShow, orderDetail.CounterPartyTraderName);//  orderDetail.TraderBInfo;
                    if (temp == "()")
                        tbInstTrader.Text = "";
                    else
                        tbInstTrader.Text = temp;
                }
                else
                {
                    _bankName = orderDetail.InstAShow;
                    _bankAgentName = orderDetail.TraderName == null ? "" : orderDetail.TraderName;
                    //增加交易员显示 20200403 wym
                    string temp = string.Format("{0}({1})", orderDetail.InstAShow, orderDetail.TraderName);//orderDetail.TraderAInfo;
                    if (temp == "()")
                        tbInstTrader.Text = "";
                    else
                        tbInstTrader.Text = temp;
                }

                tbSide.Text = "Bid方";
                tbBnakName.Foreground = tbSide.Foreground = (Brush)FindResource("ColorBrush.Yellow3");
                if (ERemarkShow.不显示.EqualsToValue(orderDetail.BridgeRemarkShow2))
                {
                    cbShowRemark.IsChecked = true;
                }
                //备注名称显示
                tbRemarksName.Text = "备注Bid:";
            }

            //获取订单对应方向的桥机构代码
            correspondingBridgeBankName = Utils.CommonUtils.GetInstName2(CurrentBridge.bankInstitutionid);
            //CRMService.Instance.GetInstName2(CurrentBridge.bankId);
            //发给
            tbSendto.Text = isOfr ? string.IsNullOrEmpty(orderDetail.BridgeSendTo) ? userBridge == null ? string.Empty : userBridge.sendto : orderDetail.BridgeSendTo
                : string.IsNullOrEmpty(orderDetail.BridgeSendTo2) ? userBridge == null ? string.Empty : userBridge.sendto : orderDetail.BridgeSendTo2;
            //渠道
            string channel = isOfr ? string.IsNullOrEmpty(orderDetail.BridgeChannel) ? userBridge == null ? string.Empty : userBridge.channel : orderDetail.BridgeChannel :
                string.IsNullOrEmpty(orderDetail.BridgeChannel2) ? userBridge == null ? string.Empty : userBridge.channel : orderDetail.BridgeChannel2;
            if (!string.IsNullOrEmpty(channel))
            {
                switch (channel)
                {
                    case "1":
                        lbTalk.IsSelected = true;
                        break;
                    case "2":
                        lbRequest.IsSelected = true;
                        break;
                    case "3":
                        lbDouble.IsSelected = true;
                        break;
                    case "4":
                        lbXBond.IsSelected = true;
                        break;
                    case "5":
                        lbIDeal.IsSelected = true;
                        break;
                    case "6":
                        lbFixed.IsSelected = true;
                        break;
                    case "7":
                        lbBidding.IsSelected = true;
                        break;
                    case "8":
                        lbBulk.IsSelected = true;
                        break;
                    case "9":
                        lbIntention.IsSelected = true;
                        break;
                    case "10":
                        lbAnonymity.IsSelected = true;
                        break;
                    case "11":
                        lbXbargain.IsSelected = true;
                        break;
                }
            }
            //费用
            tbCost.Text = isOfr ? string.IsNullOrEmpty(orderDetail.BridgeFee) ? string.Empty : orderDetail.BridgeFee.Substring(0, orderDetail.BridgeFee.Length - 1)
                : string.IsNullOrEmpty(orderDetail.BridgeFee2) ? string.Empty : orderDetail.BridgeFee2.Substring(0, orderDetail.BridgeFee2.Length - 1);

            //结算类型
            string settle = string.Empty;
            //特殊佣金
            if (isOfr)
            {
                if (orderDetail != null && orderDetail.bridgeOrder != null && !string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.OfrSpecialBridgeFee))
                {
                    this.tb_SpecialFee.Text = orderDetail.bridgeOrder.OfrSpecialBridgeFee;
                }
                else if (orderDetail != null && !string.IsNullOrWhiteSpace(orderDetail.ofrSpecialBridgeFee))
                {
                    this.tb_SpecialFee.Text = orderDetail.ofrSpecialBridgeFee;
                }
                else
                {
                    this.tb_SpecialFee.Text = "";
                }
                orderDetail.ofrSpecialBridgeFee = this.tb_SpecialFee.Text;
            }
            else
            {
                if (orderDetail != null && !string.IsNullOrWhiteSpace(orderDetail.BridgeInstitutionId2) && orderDetail.bridgeOrder2 != null && !string.IsNullOrWhiteSpace(orderDetail.bridgeOrder2.BidSpecialBridgeFee))
                {
                    this.tb_SpecialFee.Text = orderDetail.bridgeOrder2.BidSpecialBridgeFee;
                }
                else
                {
                    if (orderDetail != null && orderDetail.bridgeOrder != null && !string.IsNullOrWhiteSpace(orderDetail.bridgeOrder.BidSpecialBridgeFee))
                    {
                        this.tb_SpecialFee.Text = orderDetail.bridgeOrder.BidSpecialBridgeFee;
                    }
                    else if (orderDetail != null && !string.IsNullOrWhiteSpace(orderDetail.bidSpecialBridgeFee))
                    {
                        this.tb_SpecialFee.Text = orderDetail.bidSpecialBridgeFee;
                    }
                    else
                    {
                        this.tb_SpecialFee.Text = "";
                    }
                }
                orderDetail.bidSpecialBridgeFee = this.tb_SpecialFee.Text;
            }
            if (TransOrderHelper.Instance.IsDealHistDay(orderDetail.createTime))
                settle = TransOrderHelper.Instance.GetHistoryDisplaySettle(orderDetail.SettleDisplay, orderDetail.SettleDisplay2, orderDetail.remarks, orderDetail.dealdate, orderDetail.DealDate2, orderDetail.SettlType, orderDetail.SettlType2, orderDetail.SettlDate, orderDetail.SettlDate2);

            //OrderHelper.GetHistoryDisplaySettle(orderDetail);
            else
                settle = TransOrderHelper.Instance.GetDisplaySettle(orderDetail.SettleDisplay, orderDetail.SettleDisplay2, orderDetail.remarks, orderDetail.dealdate, orderDetail.SettlType, orderDetail.SettlType2);
                    //OrderHelper.GetDisplaySettle(orderDetail);

            string[] arr = settle.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 0)
            {
                if (isOfr)
                    settle = arr[0];
                else
                    settle = arr.Length == 1 ? arr[0] : arr.Length == 2 ? arr[1] : string.Empty;
            }
            tbSettle.Text = settle;//isOfr ? orderDetail.SettleDisplay2 : orderDetail.SettleDisplay;
            //备注
            tbRemark.Text = isOfr ? string.IsNullOrEmpty(orderDetail.BridgeRemark) ? string.Empty : orderDetail.BridgeRemark
                : string.IsNullOrEmpty(orderDetail.BridgeRemark2) ? string.Empty : orderDetail.BridgeRemark2;

            //标识方向设置
            SetFlagDir(orderDetail, isOfr);
            SetSendTo(userBridge, orderDetail, isOfr);

            //20220208  ----2.17.0----增加
            List<BillingInstModel> billingInsts = new List<BillingInstModel>();
            if (isOfr)
            {
                if (orderDetail != null && orderDetail.ofrIssueInfoReqList != null && orderDetail.ofrIssueInfoReqList.Count > 0)
                {
                    foreach (SendInstSevieceModel item in orderDetail.ofrIssueInfoReqList)
                    {
                        BillingInstModel billingInst = new BillingInstModel()
                        {
                            Shortnamecn = item.issueInstName,
                            Idbkey = item.issueInstId,
                            Quantity = item.qty.ToString(),
                            Createby = item.createby,
                        };
                        billingInsts.Add(billingInst);
                    }
                }
                else
                {
                    BillingInstModel billingInst = new BillingInstModel();
                    billingInsts.Add(billingInst);
                }
            }
            else
            {
                if (orderDetail != null && orderDetail.bidIssueInfoReqList != null && orderDetail.bidIssueInfoReqList.Count > 0)
                {
                    foreach (SendInstSevieceModel item in orderDetail.bidIssueInfoReqList)
                    {
                        BillingInstModel billingInst = new BillingInstModel()
                        {
                            Shortnamecn = item.issueInstName,
                            Idbkey = item.issueInstId,
                            Quantity = item.qty.ToString(),
                            Createby = item.createby,
                        };
                        billingInsts.Add(billingInst);
                    }
                }
                else
                {
                    BillingInstModel billingInst = new BillingInstModel();
                    billingInsts.Add(billingInst);
                }
            }
            InstList = new ObservableCollection<BillingInstModel>(billingInsts);
            if (InstList.Count >= 10)
            {
                foreach (BillingInstModel item in InstList)
                {
                    item.AddVisibility = Visibility.Collapsed;
                }
            }
            boxGroupList.DataContext = InstList;
        }
        /// <summary>
        /// 标识方向设置
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="isOfr"></param>
        private void SetFlagDir(BridgeOrderDetail orderDetail, bool isOfr)
        {
            if (isOfr)
            {
                if (orderDetail != null && orderDetail.bridgeOrder != null)
                {
                    //单桥
                    if (orderDetail.bridgeOrder2 == null)
                    {
                        orderDetail.OfrDir = orderDetail.bridgeOrder.ofrDir;
                        if (orderDetail.bridgeOrder.ofrDir == 1)
                        {
                            //箭头向下
                            btn_OfrFlag.Content = FindResource("DownImage");
                        }
                        else
                        {
                            //箭头向上
                            btn_OfrFlag.Content = FindResource("UpImage");
                        }
                    }
                    else
                    {
                        if (this.CurrentBridge.bankTraderid == orderDetail.bridgeOrder.instTradeId
                            && this.CurrentBridge.bankInstitutionid == orderDetail.bridgeOrder.instId)
                        {
                            orderDetail.OfrDir = orderDetail.bridgeOrder.ofrDir;
                            if (orderDetail.bridgeOrder.ofrDir == 1)
                            {
                                //箭头向下
                                btn_OfrFlag.Content = FindResource("DownImage");
                            }
                            else
                            {
                                //箭头向上
                                btn_OfrFlag.Content = FindResource("UpImage");
                            }
                        }
                        else if (this.CurrentBridge.bankTraderid == orderDetail.bridgeOrder2.instTradeId
                            && this.CurrentBridge.bankInstitutionid == orderDetail.bridgeOrder2.instId)
                        {
                            orderDetail.OfrDir = orderDetail.bridgeOrder2.ofrDir;
                            if (orderDetail.bridgeOrder2.ofrDir == 1)
                            {
                                //箭头向下
                                btn_OfrFlag.Content = FindResource("DownImage");
                            }
                            else
                            {
                                //箭头向上
                                btn_OfrFlag.Content = FindResource("UpImage");
                            }
                        }

                    }
                }
                //双桥
            }
            else
            {
                this.bod_Tile.Background = (Brush)FindResource("ColorBrush.Orange11");
                if (orderDetail != null && orderDetail.bridgeOrder != null)
                {
                    //单桥
                    if (orderDetail.bridgeOrder2 == null)
                    {
                        orderDetail.BidDir = orderDetail.bridgeOrder.bidDir;
                        if (orderDetail.bridgeOrder.bidDir == 1)
                        {
                            //箭头向下
                            btn_OfrFlag.Content = FindResource("DownImage");
                        }
                        else
                        {
                            //箭头向上
                            btn_OfrFlag.Content = FindResource("UpImage");
                        }
                    }
                    else
                    {
                        if (this.CurrentBridge.bankTraderid == orderDetail.bridgeOrder.instTradeId
                            && this.CurrentBridge.bankInstitutionid == orderDetail.bridgeOrder.instId)
                        {
                            orderDetail.BidDir = orderDetail.bridgeOrder.bidDir;
                            if (orderDetail.bridgeOrder.bidDir == 1)
                            {
                                //箭头向下
                                btn_OfrFlag.Content = FindResource("DownImage");
                            }
                            else
                            {
                                //箭头向上
                                btn_OfrFlag.Content = FindResource("UpImage");
                            }
                        }
                        else if (this.CurrentBridge.bankTraderid == orderDetail.bridgeOrder2.instTradeId
                            && this.CurrentBridge.bankInstitutionid == orderDetail.bridgeOrder2.instId)
                        {
                            orderDetail.BidDir2 = orderDetail.bridgeOrder2.bidDir;
                            if (orderDetail.bridgeOrder2.bidDir == 1)
                            {
                                //箭头向下
                                btn_OfrFlag.Content = FindResource("DownImage");
                            }
                            else
                            {
                                //箭头向上
                                btn_OfrFlag.Content = FindResource("UpImage");
                            }
                        }

                    }
                }
            }

            if (string.IsNullOrWhiteSpace(_bankName))
            {
                btn_OfrFlag.IsEnabled = false;
            }
            else
            {
                btn_OfrFlag.IsEnabled = true;
            }
        }


        /// <summary>
        /// 重新刷新方向
        /// </summary>
        /// <param name="dirFlag"></param>
        public void ReFlash(int dirFlag)
        {
            if (dirFlag == 1)
            {
                //箭头向下
                btn_OfrFlag.Content = FindResource("DownImage");
                //默认为桥的发给
                tbSendto.Text = "";
                tbSendto.IsEnabled = false;
            }
            else
            {
                //箭头向上
                btn_OfrFlag.Content = FindResource("UpImage");
                tbSendto.IsEnabled = true;
            }
        }


        private void SetSendTo(UserBridge userBridge, BridgeOrderDetail orderDetail, bool isOfr)
        {

            Image image = btn_OfrFlag.Content as Image;
            int dirFlag = 2;
            if (image != null && image.Name == "DownImage")
            {
                dirFlag = 1;
            }
            else
            {
                dirFlag = 2;
            }
            if (dirFlag == 2)
            {

                //发给
                tbSendto.Text = isOfr ? string.IsNullOrEmpty(orderDetail.BridgeSendTo) ? userBridge == null ? string.Empty : userBridge.sendto : orderDetail.BridgeSendTo
                    : string.IsNullOrEmpty(orderDetail.BridgeSendTo2) ? userBridge == null ? string.Empty : userBridge.sendto : orderDetail.BridgeSendTo2;

                //if (userBridge != null && !tbSendto.Text.Equals(userBridge.sendto))
                //{
                //    //lbBulk.IsEnabled = lbBidding.IsEnabled = lbFixed.IsEnabled = lbTalk.IsEnabled = lbRequest.IsEnabled = lbDouble.IsEnabled = lbXBond.IsEnabled = lbIDeal.IsEnabled = false;
                //    lbBulk.IsSelected = lbBidding.IsSelected = lbFixed.IsSelected = lbTalk.IsSelected = lbRequest.IsSelected = lbDouble.IsSelected = lbXBond.IsSelected = lbIDeal.IsSelected = false;
                //}
            }
            else
            {
                //发给
                tbSendto.Text = isOfr ? string.IsNullOrEmpty(orderDetail.BridgeSendTo) ? _bankAgentName == null ? string.Empty : _bankAgentName : orderDetail.BridgeSendTo
                    : string.IsNullOrEmpty(orderDetail.BridgeSendTo2) ? _bankAgentName == null ? string.Empty : _bankAgentName : orderDetail.BridgeSendTo2;
                //if (userBridge != null && !tbSendto.Text.Trim().Equals(_bankAgentName))
                //{
                //    //lbBulk.IsEnabled = lbBidding.IsEnabled = lbFixed.IsEnabled = lbTalk.IsEnabled = lbRequest.IsEnabled = lbDouble.IsEnabled = lbXBond.IsEnabled = lbIDeal.IsEnabled = false;
                //    lbBulk.IsSelected = lbBidding.IsSelected = lbFixed.IsSelected = lbTalk.IsSelected = lbRequest.IsSelected = lbDouble.IsSelected = lbXBond.IsSelected = lbIDeal.IsSelected = false;
                //}
            }

        }
        /// <summary>
        /// 修改成交单的桥要素
        /// </summary>
        /// <param name="orderDetail">被修改的成交单</param>
        /// <param name="isGvn"></param>
        public void GetData(BridgeOrderDetail orderDetail, bool isGvn)
        {
            if (isGvn)
            {
                //发给
                orderDetail.BridgeSendTo = tbSendto.Text;
                //渠道
                orderDetail.BridgeChannel = string.IsNullOrEmpty(GetChannelSelect()) ? orderDetail.BridgeChannel : GetChannelSelect();
                //费用
                if (!string.IsNullOrEmpty(tbCost.Text))
                    orderDetail.BridgeFee = tbCost.Text + "厘";
                else
                    orderDetail.BridgeFee = string.Empty;

                //特殊佣金
                if (!string.IsNullOrEmpty(tb_SpecialFee.Text.Trim()))
                {
                    orderDetail.ofrSpecialBridgeFee = tb_SpecialFee.Text.Trim();
                }
                else
                {
                    orderDetail.ofrSpecialBridgeFee = string.Empty;
                }
                //备注
                orderDetail.BridgeRemark = tbRemark.Text;
                orderDetail.BridgeRemarkShow = cbShowRemark.IsChecked == true ? ERemarkShow.不显示.ToChar() : ERemarkShow.显示.ToChar();


            }
            else
            {
                orderDetail.BridgeSendTo2 = tbSendto.Text;
                orderDetail.BridgeChannel2 = string.IsNullOrEmpty(GetChannelSelect()) ? orderDetail.BridgeChannel2 : GetChannelSelect();
                if (!string.IsNullOrEmpty(tbCost.Text))
                    orderDetail.BridgeFee2 = tbCost.Text + "厘";
                else
                    orderDetail.BridgeFee2 = string.Empty;


                //特殊佣金
                if (!string.IsNullOrEmpty(tb_SpecialFee.Text.Trim()))
                {
                    orderDetail.bidSpecialBridgeFee = tb_SpecialFee.Text.Trim();
                }
                else
                {
                    orderDetail.bidSpecialBridgeFee = string.Empty;
                }

                orderDetail.BridgeRemark2 = tbRemark.Text;
                orderDetail.BridgeRemarkShow2 = cbShowRemark.IsChecked == true ? ERemarkShow.不显示.ToChar() : ERemarkShow.显示.ToChar();
            }
            //获取订单对应方向的桥机构代码
            SetDirValue(orderDetail, isGvn);
            SetSentto(orderDetail, isGvn);
            //获取发单机构
            //GetSendOrderInst(orderDetail, isGvn);
        }

        /// <summary>
        /// 获取发单机构信息
        /// </summary>
        public bool GetSendOrderInst(BridgeOrderDetail orderDetail, bool isGvn)
        {
            bool isChecked = true;
            List<SendInstSevieceModel> sendInstSevieceModels = new List<SendInstSevieceModel>();
            decimal qty = 0;
            if (InstList != null && InstList.Count > 0)
            {
                foreach (BillingInstModel item in InstList)
                {
                    if (!string.IsNullOrWhiteSpace(item.Shortnamecn) && !string.IsNullOrWhiteSpace(item.Quantity))
                    {
                        decimal deQty;
                        if (decimal.TryParse(item.Quantity, out deQty))
                        {
                            if (deQty <= 0 || deQty > 2000000)
                            {
                                SysWarnGUI warn = new SysWarnGUI();
                                Window win = Window.GetWindow(this);
                                if (win != null)
                                    warn.Owner = win;
                                warn.tbTitle.Text = "提示";
                                warn.tbMsg.Text = "量填写有误";
                                warn.ShowDialog();
                                return false;
                            }
                            SendInstSevieceModel sendInstSeviece = new SendInstSevieceModel()
                            {
                                issueInstId = item.Idbkey,
                                issueInstName = item.Shortnamecn,
                                qty = Convert.ToDecimal(item.Quantity),
                                createby = LocalEnv.UserInfo.UserName,
                            };
                            qty += sendInstSeviece.qty;
                            sendInstSevieceModels.Add(sendInstSeviece);
                        }
                        else
                        {
                            SysWarnGUI warn = new SysWarnGUI();
                            Window win = Window.GetWindow(this);
                            if (win != null)
                                warn.Owner = win;
                            warn.tbTitle.Text = "提示";
                            warn.tbMsg.Text = "量填写有误";
                            warn.ShowDialog();
                            return false;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(item.Shortnamecn) && string.IsNullOrWhiteSpace(item.Quantity))
                        {
                            SysWarnGUI warn = new SysWarnGUI();
                            Window win = Window.GetWindow(this);
                            if (win != null)
                                warn.Owner = win;
                            warn.tbTitle.Text = "提示";
                            warn.tbMsg.Text = "量填写有误";
                            warn.ShowDialog();
                            return false;
                        }
                        else if (string.IsNullOrWhiteSpace(item.Shortnamecn) && !string.IsNullOrWhiteSpace(item.Quantity))
                        {
                            SysWarnGUI warn = new SysWarnGUI();
                            Window win = Window.GetWindow(this);
                            if (win != null)
                                warn.Owner = win;
                            warn.tbTitle.Text = "提示";
                            warn.tbMsg.Text = "机构填写有误";
                            warn.ShowDialog();
                            return false;
                        }
                    }
                }
            }
            if (isGvn)
            {
                orderDetail.ofrIssueInfoReqList = sendInstSevieceModels;
                if (orderDetail.ofrIssueInfoReqList.Count > 0)
                {
                    if (orderDetail.qty - qty != decimal.Zero)
                    {
                        SysWarnGUI warn = new SysWarnGUI();
                        Window win = Window.GetWindow(this);
                        if (win != null)
                            warn.Owner = win;
                        warn.tbTitle.Text = "提示";
                        warn.tbMsg.Text = "量填写有误";
                        warn.ShowDialog();
                        return false;
                    }
                }
            }
            else
            {
                orderDetail.bidIssueInfoReqList = sendInstSevieceModels;
                if (orderDetail.bidIssueInfoReqList.Count > 0)
                {
                    if (orderDetail.qty - qty != decimal.Zero)
                    {
                        SysWarnGUI warn = new SysWarnGUI();
                        Window win = Window.GetWindow(this);
                        if (win != null)
                            warn.Owner = win;
                        warn.tbTitle.Text = "提示";
                        warn.tbMsg.Text = "量填写有误";
                        warn.ShowDialog();
                        return false;
                    }
                }
            }
            return isChecked;
        }
        /// <summary>
        /// 获取标识方向值
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="isGvn"></param>
        private void SetDirValue(BridgeOrderDetail orderDetail, bool isGvn)
        {
            Image image = btn_OfrFlag.Content as Image;
            int dirFlag = 2;
            if (image != null && image.Name == "DownImage")
            {
                dirFlag = 1;
                ////获取订单对应方向的桥机构代码
                //correspondingBridgeBankName = _bankName;
            }
            else
            {
                dirFlag = 2;
            }
            if (isGvn)
            {
                if (orderDetail != null && orderDetail.bridgeOrder != null)
                {
                    orderDetail.OfrDir = dirFlag;
                }
            }
            else
            {
                if (orderDetail != null && orderDetail.bridgeOrder != null)
                {
                    //单桥

                    if (orderDetail.BridgeInstitutionId == orderDetail.BridgeInstitutionId2
                       && orderDetail.BridgeTraderId == orderDetail.BridgeTraderId)
                    {
                        //单桥
                        orderDetail.BidDir = dirFlag;
                    }
                    else
                    {
                        //双桥
                        orderDetail.BidDir2 = dirFlag;
                    }
                }
            }
        }
        /// <summary>
        /// 修改桥要素重置发给
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="isGvn"></param>
        private void SetSentto(BridgeOrderDetail orderDetail, bool isGvn)
        {
            //当订单的桥状态全部都是“已确认“的状态才会更新主界面发给；
            // 当订单有任意一座桥的桥状态不是“已确认“，则不更新主界面发给。
            if (!OrderHelper.BridgeIsAllConfirmed(orderDetail))
                return;
            string sendto = string.Empty;
            //当前填入发给与桥的默认发给一致

            //添加方向判别

            string sendtoStr = CurrentBridge.sendto;
            int flag = 2;

            Image image = btn_OfrFlag.Content as Image;
            if (image != null && image.Name == "DownImage")
            {
                sendtoStr = _bankAgentName;
                flag = 1;
            }
            else
            {
                sendtoStr = CurrentBridge.sendto;
                flag = 2;
            }
            if (CurrentBridge != null && tbSendto.Text.Equals(CurrentBridge.sendto))
            {
                ///如果用户没有选择渠道，则使用单子原来的渠道去生成发给 20200422
                if (!lbTalk.IsSelected && !lbRequest.IsSelected && !lbDouble.IsSelected && !lbXBond.IsSelected && !lbIDeal.IsSelected && !lbFixed.IsSelected && !lbBidding.IsSelected && !lbBulk.IsSelected && !lbIntention.IsSelected && !lbAnonymity.IsSelected && !lbXbargain.IsSelected)
                {
                    sendto = GetSendToFromOrder(orderDetail, isGvn);
                }
                else
                {
                    if (lbBulk.IsSelected || lbFixed.IsSelected || lbBidding.IsSelected)
                        sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(), correspondingBridgeBankName);
                    else if (lbTalk.IsSelected)
                        sendto = string.Format("发{0}", correspondingBridgeBankName);
                    else if (lbRequest.IsSelected)
                        sendto = string.Format("{0}发{1}", "【请求】", correspondingBridgeBankName);
                    else if (lbIntention.IsSelected || lbAnonymity.IsSelected)
                        sendto = string.Format("点{0},{1}", GetSelectChannelTxt(), correspondingBridgeBankName);
                    else
                        sendto = string.Format("{0}发{1}", GetSelectChannelTxt(), correspondingBridgeBankName);

                    if (!string.IsNullOrEmpty(tbSendto.Text) && !lbRequest.IsSelected)
                        sendto += " " + tbSendto.Text;
                    if (!string.IsNullOrEmpty(tbCost.Text))
                        sendto += ", " + tbCost.Text + "厘";
                    if (!string.IsNullOrEmpty(tbRemark.Text))
                        sendto += ", " + tbRemark.Text;
                }
            }
            else
            {
                if (CurrentBridge != null
                    && (tbSendto.Text.Equals(_bankAgentName) && !IsBatchEdit) || (IsBatchEdit && string.IsNullOrWhiteSpace(tbSendto.Text)))
                {
                    if (flag == 1)
                    {
                        if (isGvn)
                        {
                            orderDetail.BridgeSendTo = _bankAgentName;
                        }
                        else
                        {
                            orderDetail.BridgeSendTo2 = _bankAgentName;
                        }

                        //发给与默认不给不一致
                        sendto = "走{0}";
                        sendto = string.Format(sendto, correspondingBridgeBankName);
                        string sendto1 = string.Empty;

                        if (!lbTalk.IsSelected && !lbRequest.IsSelected && !lbDouble.IsSelected && !lbXBond.IsSelected && !lbIDeal.IsSelected && !lbFixed.IsSelected && !lbBidding.IsSelected && !lbBulk.IsSelected && !lbIntention.IsSelected && !lbAnonymity.IsSelected && !lbXbargain.IsSelected)
                        {
                            sendto1 = GetSendToFromOrder(orderDetail, isGvn);
                        }
                        else
                        {
                            if (lbBulk.IsSelected || lbFixed.IsSelected || lbBidding.IsSelected)
                                sendto1 = string.Format("{0}联系{1}", GetSelectChannelTxt(), _bankName);
                            else if (lbTalk.IsSelected)
                                sendto1 = string.Format("发{0}", _bankName);
                            else if (lbRequest.IsSelected)
                                sendto1 = string.Format("{0}发{1}", "【请求】", _bankName);
                            else if (lbIntention.IsSelected || lbAnonymity.IsSelected)
                                sendto1 = string.Format("点{0},{1}", GetSelectChannelTxt(), _bankName);
                            else
                                sendto1 = string.Format("{0}发{1}", GetSelectChannelTxt(), _bankName);

                            if (!string.IsNullOrEmpty(tbSendto.Text) && !lbRequest.IsSelected && !IsBatchEdit)
                            {
                                sendto1 += " " + tbSendto.Text;
                            }
                            else if (IsBatchEdit && string.IsNullOrEmpty(tbSendto.Text) && !lbRequest.IsSelected)
                            {
                                sendto1 += " " + _bankAgentName;
                            }
                            if (!string.IsNullOrEmpty(tbCost.Text))
                                sendto1 += ", " + tbCost.Text + "厘";
                            if (!string.IsNullOrEmpty(tbRemark.Text))
                                sendto1 += ", " + tbRemark.Text;
                        }

                        sendto = sendto + "," + sendto1;

                    }
                    else
                    {
                        //发给与默认不给不一致
                        sendto = "走{0}";
                        sendto = string.Format(sendto, correspondingBridgeBankName);

                        if (!string.IsNullOrEmpty(tbSendto.Text))
                            sendto += ", " + tbSendto.Text;
                        if (!string.IsNullOrEmpty(tbCost.Text))
                        {
                            if (string.IsNullOrEmpty(tbSendto.Text))
                                sendto += tbCost.Text;
                            else
                                sendto += ", " + tbCost.Text + "厘";
                        }
                        if (!string.IsNullOrEmpty(tbRemark.Text))
                        {
                            if (string.IsNullOrEmpty(tbSendto.Text) && string.IsNullOrEmpty(tbCost.Text))
                            {
                                sendto += tbRemark.Text;
                            }
                            else
                            {
                                sendto += ", " + tbRemark.Text;
                            }
                        }
                    }
                }
                else
                {
                    //发给与默认不给不一致
                    sendto = "走{0}";
                    sendto = string.Format(sendto, correspondingBridgeBankName);

                    if (!string.IsNullOrEmpty(tbSendto.Text))
                        sendto += ", " + tbSendto.Text;
                    if (!string.IsNullOrEmpty(tbCost.Text))
                    {
                        if (string.IsNullOrEmpty(tbSendto.Text))
                            sendto += tbCost.Text;
                        else
                            sendto += ", " + tbCost.Text + "厘";
                    }
                    if (!string.IsNullOrEmpty(tbRemark.Text))
                    {
                        if (string.IsNullOrEmpty(tbSendto.Text) && string.IsNullOrEmpty(tbCost.Text))
                        {
                            sendto += tbRemark.Text;
                        }
                        else
                        {
                            sendto += ", " + tbRemark.Text;
                        }
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(sendto))
            {
                sendto = sendto.Trim();
            }

            if (isGvn)
            {
                orderDetail.SendTo2 = sendto;
            }
            else
            {
                orderDetail.SendTo = sendto;
            }
        }

        /// <summary>
        /// 批量设置默认发给
        /// </summary>
        public void SetSentTo(BridgeOrderDetail orderDetail, bool isOfr)
        {

            //对应方向的交易员
            if (isOfr)
            {
                //获取ofr方机构
                if (orderDetail.IsGVN)
                {
                    _bankName = orderDetail.InstAShow;
                    _bankAgentName = orderDetail.TraderName == null ? "" : orderDetail.TraderName;

                }
                else
                {
                    _bankName = orderDetail.InstBShow;
                    _bankAgentName = orderDetail.CounterPartyTraderName == null ? "" : orderDetail.CounterPartyTraderName;
                }
            }
            else
            {
                //获取bid方机构
                if (orderDetail.IsGVN)
                {
                    _bankName = orderDetail.InstBShow;
                    _bankAgentName = orderDetail.CounterPartyTraderName == null ? "" : orderDetail.CounterPartyTraderName;
                }
                else
                {
                    _bankName = orderDetail.InstAShow;
                    _bankAgentName = orderDetail.TraderName == null ? "" : orderDetail.TraderName;
                }
            }
        }
        /// <summary>
        /// 如果用户没有选择渠道，则使用订单原有的渠道  20200421
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="isGvn"></param>
        private string GetSendToFromOrder(BridgeOrderDetail orderDetail, bool isGvn)
        {
            string sendto = string.Empty;
            string x = isGvn ? orderDetail.BridgeChannel : orderDetail.BridgeChannel2;
            if (x == "6" || x == "7" || x == "8")
                sendto = string.Format("{0}联系{1}", GetSelectChannelTxtByNo(x), correspondingBridgeBankName);
            else if (x == "1")
                sendto = string.Format("发{0}", correspondingBridgeBankName);
            else if (x == "2")
                sendto = string.Format("{0}发{1}", "【请求】", correspondingBridgeBankName);
            else if (x == "9" || x == "10")
                sendto = string.Format("点{0},{1}", GetSelectChannelTxtByNo(x), correspondingBridgeBankName);
            else
                sendto = string.Format("{0}发{1}", GetSelectChannelTxtByNo(x), correspondingBridgeBankName);

            if (!string.IsNullOrEmpty(tbSendto.Text) && x != "2")
                sendto += " " + tbSendto.Text;
            if (!string.IsNullOrEmpty(tbCost.Text))
                sendto += ", " + tbCost.Text + "厘";
            if (!string.IsNullOrEmpty(tbRemark.Text))
                sendto += ", " + tbRemark.Text;
            return sendto;
        }
        private string GetSelectChannelTxt()
        {
            if (lbTalk.IsSelected)
                return "对话";
            else if (lbRequest.IsSelected)
                return "请求";
            else if (lbDouble.IsSelected)
                return "双边";
            else if (lbXBond.IsSelected)
                return "xbond";
            else if (lbIDeal.IsSelected)
                return "iDeal";
            else if (lbFixed.IsSelected)
                return "固收";
            else if (lbBidding.IsSelected)
                return "竞价";
            else if (lbBulk.IsSelected)
                return "大宗";
            else if (lbIntention.IsSelected)
                return "意向";
            else if (lbAnonymity.IsSelected)
                return "匿名";
            else if (lbXbargain.IsSelected)
                return "xbargain";
            return string.Empty;
        }
        private string GetSelectChannelTxtByNo(string no)
        {
            if (no == "1")
                return "对话";
            else if (no == "2")
                return "请求";
            else if (no == "3")
                return "双边";
            else if (no == "4")
                return "xbond";
            else if (no == "5")
                return "iDeal";
            else if (no == "6")
                return "固收";
            else if (no == "7")
                return "竞价";
            else if (no == "8")
                return "大宗";
            else if (no == "9")
                return "意向";
            else if (no == "10")
                return "匿名";
            else if (no == "11")
                return "xbargain";
            return string.Empty;
        }
        /// <summary>
        /// 获取渠道
        /// </summary>
        /// <returns></returns>
        private string GetChannelSelect()
        {
            if (lbTalk.IsSelected)
                return "1";
            else if (lbRequest.IsSelected)
                return "2";
            else if (lbDouble.IsSelected)
                return "3";
            else if (lbXBond.IsSelected)
                return "4";
            else if (lbIDeal.IsSelected)
                return "5";
            else if (lbFixed.IsSelected)
                return "6";
            else if (lbBidding.IsSelected)
                return "7";
            else if (lbBulk.IsSelected)
                return "8";
            else if (lbIntention.IsSelected)
                return "9";
            else if (lbAnonymity.IsSelected)
                return "10";
            else if (lbXbargain.IsSelected)
                return "11";
            return string.Empty;
        }
        /// <summary>
        /// 选择渠道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //foreach (ListBoxItem item in lbChannel.Items)
            //{
            //    //单选
            //    if (!item.Equals(lbChannel.SelectedItem))
            //        item.IsSelected = false;
            //}
        }

        private void tbSendto_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Image image = btn_OfrFlag.Content as Image;
            //if (image != null && image.Name == "DownImage")
            //{
            //    if (CurrentBridge != null && !(_bankAgentName).Equals(tbSendto.Text.Trim()))
            //        lbBulk.IsSelected = lbBidding.IsSelected = lbFixed.IsSelected = lbTalk.IsSelected = lbRequest.IsSelected = lbDouble.IsSelected = lbXBond.IsSelected = lbIDeal.IsSelected = false;
            //}
            //else
            //{
            //    if (CurrentBridge != null && !(CurrentBridge.sendto + "").Equals(tbSendto.Text + ""))
            //        lbBulk.IsSelected = lbBidding.IsSelected = lbFixed.IsSelected = lbTalk.IsSelected = lbRequest.IsSelected = lbDouble.IsSelected = lbXBond.IsSelected = lbIDeal.IsSelected = false;
            //}
        }

        private void tbCost_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!e.Text.IsNumberOrDot())
            { 
                e.Handled = true;
                return;
            }
            string newTxt = tbCost.Text + e.Text;
            if (newTxt.Length > 20)
            {
                tbCost.Text = newTxt.Substring(0, 20);
            }
        }

        private void tbCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldTxt = tbCost.Text;
            double fee = 0.0;
            if (double.TryParse(oldTxt, out fee))
            {
                if (fee >= 0)
                {
                    tbCost.Text = oldTxt;
                }
                else
                {
                    tbCost.Text = "";
                }
            }
            else
            {
                tbCost.Text = "";
            }
        }

        private void tbRemark_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newTxt = tbRemark.Text;
            if (newTxt.Length > 30)
            {
                tbRemark.Text = newTxt.Substring(0, 30);
            }
        }
        /// <summary>
        /// 点击方向键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OfrFlag_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Image image = button.Content as Image;
                if (image == null)
                {
                    return;
                }
                if (image.Name == "UpImage")
                {
                    button.Content = FindResource("DownImage");
                    if (IsBatchEdit)
                    {
                        this.tbSendto.Text = "";
                        this.tbSendto.IsEnabled = false;
                    }
                    else
                    {
                        this.tbSendto.Text = _bankAgentName;
                        this.tbSendto.IsEnabled = true;
                    }
                }
                else
                {
                    button.Content = FindResource("UpImage");
                    this.tbSendto.Text = this.CurrentBridge.sendto;
                    this.tbSendto.IsEnabled = true;
                }
                string channel = CurrentBridge == null ? string.Empty : CurrentBridge.channel;

                //交易所债券渠道取固定值
                if (!string.IsNullOrWhiteSpace(this._goodsCode))
                {
                    if (_goodsCode.EndsWith(".SH")|| _goodsCode.EndsWith(".SZ"))
                        channel = "6";
                }
                if (!string.IsNullOrEmpty(channel))
                {
                    switch (channel)
                    {
                        case "1":
                            lbTalk.IsSelected = true;
                            break;
                        case "2":
                            lbRequest.IsSelected = true;
                            break;
                        case "3":
                            lbDouble.IsSelected = true;
                            break;
                        case "4":
                            lbXBond.IsSelected = true;
                            break;
                        case "5":
                            lbIDeal.IsSelected = true;
                            break;
                        case "6":
                            lbFixed.IsSelected = true;
                            break;
                        case "7":
                            lbBidding.IsSelected = true;
                            break;
                        case "8":
                            lbBulk.IsSelected = true;
                            break;
                        case "9":
                            lbIntention.IsSelected = true;
                            break;
                        case "10":
                            lbAnonymity.IsSelected = true;
                            break;
                        case "11":
                            lbXbargain.IsSelected = true;
                            break;
                        default:
                            lbRequest.IsSelected = true;
                            break;
                    }
                }
            }
            //Image image = FindResource("UpImage") as Image;
        }

        private void tb_SpecialFee_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void tb_SpecialFee_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldTxt = tb_SpecialFee.Text;
            double fee = 0.0;
            if (double.TryParse(oldTxt, out fee))
            {
                if (fee >= 0)
                {
                    tb_SpecialFee.Text = oldTxt;
                }
                else
                {
                    tb_SpecialFee.Text = "";
                }
            }
            else
            {
                tb_SpecialFee.Text = "";
            }
        }
        /// <summary>
        /// 移除当前行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void minusSign_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            if (image == null)
            {
                return;
            }
            BillingInstModel billing = image.DataContext as BillingInstModel;
            if (billing == null)
            {
                return;
            }
            if (InstList != null && InstList.Count > 0)
            {
                if (InstList.Count <= 1)
                {
                    int indext = InstList.IndexOf(billing);
                    if (indext == 0)
                    {
                        billing.Idbkey = "";
                        billing.instcode = "";
                        billing.Shortnamecn = "";
                        billing.Quantity = "";
                    }
                }
                else
                {
                    InstList.Remove(billing);
                }
            }
            if (InstList.Count < 10)
            {
                foreach (BillingInstModel item in InstList)
                {
                    item.AddVisibility = Visibility.Visible;
                }
            }
            double dbHeight = InstList.Count * 28;
            gd_main.RowDefinitions[13].Height = new GridLength(dbHeight);
        }

        /// <summary>
        /// 新增一行数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imTips_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BillingInstModel instModel = new BillingInstModel();
            Image image = sender as Image;
            if (image == null)
            {
                return;
            }
            BillingInstModel billing = image.DataContext as BillingInstModel;
            if (billing == null)
            {
                return;
            }
            int indext = InstList.IndexOf(billing);
            if (indext != -1)
            {
                InstList.Insert(indext + 1, instModel);
            }

            if (InstList.Count >= 10)
            {
                foreach (BillingInstModel item in InstList)
                {
                    item.AddVisibility = Visibility.Collapsed;
                }
            }
            double dbHeight = InstList.Count * 28;
            gd_main.RowDefinitions[13].Height = new GridLength(dbHeight);
        }

        private void popReciveName_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {

        }


        /// <summary>
        /// 选中机构数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgRecive_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CrmInst dataPo = dgRecive.SelectedItem as CrmInst;
                if (dataPo != null)
                {
                    pop_Organization.IsOpen = false;
                    visTextTextBox.Focus();

                    TextBox txt_Inst = pop_Organization.PlacementTarget as TextBox;
                    if (txt_Inst == null)
                    {
                        return;
                    }
                    BillingInstModel billingInstModel = txt_Inst.DataContext as BillingInstModel;
                    if (billingInstModel == null)
                    {
                        return;
                    }
                    billingInstModel.Shortnamecn = dataPo.shortnamecn;
                    billingInstModel.Idbkey = dataPo.idbkey;
                    billingInstModel.instcode = dataPo.instcode;
                    isShowInstList = false;
                    txt_Inst.Text = billingInstModel.Shortnamecn;
                    isShowInstList = true;
                    txt_Inst.Focus();
                    txt_Inst.SelectionStart = txt_Inst.Text.Length;
                }
                else {
                    //支持自定义
                    TextBox txt_Inst = sender as TextBox;
                    if (txt_Inst == null)
                    {
                        return;
                    }
                    BillingInstModel billingInstModel = txt_Inst.DataContext as BillingInstModel;
                    if (billingInstModel == null)
                    {
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(txt_Inst.Text.Trim()))
                    {
                        billingInstModel.Shortnamecn = txt_Inst.Text.Trim();
                    }
                    else {
                        billingInstModel.Shortnamecn ="";
                    }
                    billingInstModel.Shortnamecn = txt_Inst.Text.Trim();
                    billingInstModel.Idbkey = "";
                    billingInstModel.instcode ="";
                    pop_Organization.IsOpen = false;
                    visTextTextBox.Focus();
                    txt_Inst.Focus();
                    txt_Inst.SelectionStart = txt_Inst.Text.Length;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                pop_Organization.IsOpen = false;
            }
        }
        /// <summary>
        /// 搜索框变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_Organization_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isShowInstList)
            {
                return;
            }
            TextBox txt_Inst = sender as TextBox;
            if (txt_Inst == null)
            {
                return;
            }
            BillingInstModel billingInstModel = txt_Inst.DataContext as BillingInstModel;
            if (billingInstModel == null)
            {
                return;
            }

            string organizationKey = txt_Inst.Text.Trim();
            organizationKey = organizationKey.Replace("'", "").Replace("’", "");
            if (!string.IsNullOrEmpty(organizationKey))
            {
                List<CrmInst> instDT = Utils.CommonUtils.GetInstitutionsInfoPo(organizationKey, 50);
                //CRMService.Instance.GetInstByName(organizationKey, 50);
                if (instDT != null && instDT.Count > 0)
                {
                    pop_Organization.PlacementTarget = txt_Inst;
                    dgRecive.DataContext = instDT;
                    pop_Organization.IsOpen = true;
                    dgRecive.SelectedIndex =-1;
                }
                else
                {
                    dgRecive.DataContext = null;
                    pop_Organization.PlacementTarget = txt_Inst;
                    pop_Organization.IsOpen = false;
                    dgRecive.SelectedIndex = -1;
                }
            }
            else
            {
                pop_Organization.PlacementTarget = txt_Inst;
                dgRecive.DataContext = null;
                pop_Organization.IsOpen = false;
            }
        }

        private void tb_Organization_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                dgRecive_MouseUp(sender, null);
            }
        }

        private void tb_Organization_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgRecive.SelectedIndex > 0)
                {
                    dgRecive.SelectedIndex--;
                }
                else
                {
                    if (dgRecive.SelectedIndex == 0)
                    {
                        dgRecive.SelectedIndex = dgRecive.Items.Count - 1;
                    }
                }
                dgRecive.ScrollIntoView(dgRecive.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgRecive.SelectedIndex < dgRecive.Items.Count - 1)
                {
                    dgRecive.SelectedIndex++;
                }
                else
                {
                    if (dgRecive.SelectedIndex == dgRecive.Items.Count - 1)
                    {
                        dgRecive.SelectedIndex = 0;
                    }
                }
                dgRecive.ScrollIntoView(dgRecive.SelectedItem);
            }
        }

        private void tb_Organization_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            isShowInstList = true;
            //TextBox tb = sender as TextBox;

            //string text = tb.Text.Trim();

            //if (!string.IsNullOrWhiteSpace(text)) {
            //    if (text.Length>50) {
            //        e.Handled = true;
            //        return;
            //    }
            //}
            //int inputIndex = tb.SelectionStart;
            //if (text.Length + e.Text.Trim().Length <= 50)
            //{
            //    text = text.Insert(inputIndex, e.Text.Trim());
            //}
            //else {
            //    e.Handled = true;
            //    return;
            //}  
        }

        private void tb_Organization_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            isShowInstList = false;
        }

        private void tb_Quantity_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void tb_Quantity_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt_Inst = sender as TextBox;
            if (txt_Inst == null)
            {
                return;
            }
            decimal quanNum;
            if (decimal.TryParse(txt_Inst.Text.Trim(), out quanNum))
            {
                if (quanNum >0)
                {
                    txt_Inst.Text = (quanNum / 1000).ToString();
                }
               
                txt_Inst.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
            }
            else
            {
                txt_Inst.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
            }

        }

        private void tb_Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt_Inst = sender as TextBox;
            if (txt_Inst == null)
            {
                return;
            }
            decimal quanNum;
            if (decimal.TryParse(txt_Inst.Text.Trim(), out quanNum))
            {
                if (quanNum <= decimal.Zero)
                {
                    txt_Inst.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    return;
                }

                if (quanNum >2000)
                {
                    txt_Inst.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                    txt_Inst.Text = (quanNum * 1000).ToString("0.########");
                    return;
                }

                txt_Inst.Text = (quanNum * 1000).ToString("0.########");
               
                txt_Inst.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));

                decimal qty = 0;
                foreach (BillingInstModel item in InstList)
                {
                    qty += Convert.ToDecimal(item.Quantity);
                }
                if (_bridgeOrderDetail != null)
                {
                    if (_bridgeOrderDetail.qty-qty!=decimal.Zero)
                    {
                        for (int i = 0; i < this.boxGroupList.Items.Count; i++)
                        {
                            ListBoxItem itemtemp = this.boxGroupList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                            TextBox buttonobject = FindVisualChildByName<TextBox>(itemtemp, "tb_Quantity");
                            if (buttonobject == null) continue;
                            if (!string.IsNullOrEmpty(buttonobject.Text.Trim()))
                            {
                                buttonobject.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.boxGroupList.Items.Count; i++)
                        {
                            ListBoxItem itemtemp = this.boxGroupList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                            TextBox buttonobject = FindVisualChildByName<TextBox>(itemtemp, "tb_Quantity");
                            if (buttonobject == null) continue;
                            if (!string.IsNullOrEmpty(buttonobject.Text.Trim())) {
                                buttonobject.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#193D37"));
                            }
                        }
                    }
                }
            }
            else
            {
                txt_Inst.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF144B"));
            }
        }

        public T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i) as DependencyObject;
                    string controlName = child.GetValue(Control.NameProperty) as string;
                    if (controlName == name)
                    {
                        return child as T;
                    }
                    else
                    {
                        T result = FindVisualChildByName<T>(child, name);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }



        private void tb_Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Regex regex = new Regex("^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;

            int max = 2000;
            int min = 0;
            string text = tb.Text;

            if (text.Contains(".") && e.Text.Equals("."))
            {
                e.Handled = true;
                return;
            }
            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            decimal d = 0;
            if (decimal.TryParse(text, out d) && (d > max || d <min) && string.IsNullOrEmpty(tb.SelectedText))
            {
                e.Handled = true;
                return;
            }
            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 8 && string.IsNullOrEmpty(tb.SelectedText))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void tb_Quantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                visTextTextBox.Focus();
            }
        }
    }
}
