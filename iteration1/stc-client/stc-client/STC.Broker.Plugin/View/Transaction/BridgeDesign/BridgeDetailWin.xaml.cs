
using STC.Broker.Plugin.Model.Transaction.CrossBridge;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Broker.Plugin.Utils;
using com.gateway.log;
using STC.Common.Model.PT;
using STC.Common.Model.Fix.Message;
using Newtonsoft.Json;
using STC.Broker.Plugin.Model;
using Ats.Foundation.Message;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign {
    /// <summary>
    /// BridgeDetailWin.xaml 的交互逻辑
    /// 修改加桥成交单的桥要素
    /// </summary>
    public partial class BridgeDetailWin : Window {
        private BridgeOrderDetail orderDetail = null;
        private List<BridgeOrderDetail> orderDetails = new List<BridgeOrderDetail>();
        public event Action<BridgeOrderDetail> OnClose;
        public event Action<List<BridgeOrderDetail>> OnCloses;
        /// <summary>
        /// 当前修改的单子所在桥
        /// </summary>
        UserBridge Bridge;

        /// <summary>
        /// ofr桥
        /// </summary>
        private UserBridge _ofrBridge;
        /// <summary>
        /// bid桥
        /// </summary>
        private UserBridge _bidBridge;
        public BridgeDetailWin() {
            InitializeComponent();

            this.Unloaded += BridgeDetailWin_Unloaded;
            //this.Icon = LocalEnv.ServerInfo.AppImage;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="orderDetail">桥下成交单</param>
        /// <param name="ofrBridge">ofr桥</param>
        /// <param name="bidBridge">bid桥</param>
        public void Init(BridgeOrderDetail orderDetail, UserBridge ofrBridge, UserBridge bidBridge, UserBridge bridge) {
            Bridge = bridge;
            this.orderDetail = orderDetail;
            _ofrBridge = ofrBridge;
            _bidBridge = bidBridge;

            //ofr桥，bid桥的方向键是否显示
            SetBridgeDir(ofrBridge, bidBridge, orderDetail);
            bdBid.Init(orderDetail, false, bidBridge);
            bdOfr.Init(orderDetail, true, ofrBridge);
            if (orderDetail.BridgeType.Equals("O") || orderDetail.BridgeType.Equals("B")) {
                //有双桥合单备注
                if (ofrBridge != null && bridge.bankInstitutionid == ofrBridge.bankInstitutionid && bridge.bankTraderid == ofrBridge.bankTraderid) {
                    //当前桥为ofr桥
                    this.tbAllRemarks.Text = orderDetail.bridgeOrder == null ? "" : orderDetail.bridgeOrder.combinedOrderRemark;
                }
                else if (bidBridge != null && bridge.bankInstitutionid == bidBridge.bankInstitutionid && bridge.bankTraderid == bidBridge.bankTraderid) {
                    //当前桥为bid桥
                    this.tbAllRemarks.Text = orderDetail.bridgeOrder2 == null ? "" : orderDetail.bridgeOrder2.combinedOrderRemark;
                }
                tbSendto.Text = orderDetail.DoubleBridgeSendTo;
                tbRemark.Text = orderDetail.DoubleBridgeRemark;
                tbCost.Text = string.IsNullOrEmpty(orderDetail.DoubleBridgeFee) ? string.Empty
                    : orderDetail.DoubleBridgeFee.Substring(0, orderDetail.DoubleBridgeFee.Length - 1);

                if (!string.IsNullOrEmpty(orderDetail.DoubleBridgeChannel)) {
                    switch (orderDetail.DoubleBridgeChannel) {
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
                            lbTalk.IsSelected = true;
                            break;
                    }
                }
                else
                    lbTalk.IsSelected = true;

                if (orderDetail.SettlType == orderDetail.SettlType2) {
                    ///隐藏双桥错期选择项
                    spDoubleBirdgeCross.Visibility = Visibility.Collapsed;
                }
                else {
                    if (orderDetail.doubleBridgeSettleSide + "" == "B")
                        lbCrossBid.IsSelected = true;
                    else
                        lbCrossOfr.IsSelected = true;
                }
            }
            else {
                // 单桥时合单备注
                this.tbAllRemarks.Text = orderDetail.bridgeOrder == null ? "" : orderDetail.bridgeOrder.combinedOrderRemark;
                ///隐藏双桥数据
                gdDoubleBridgeInfo.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 设置桥方向键
        /// </summary>
        /// <param name="ofrBridge"></param>
        /// <param name="bidBridge"></param>
        private void SetBridgeDir(UserBridge ofrBridge, UserBridge bidBridge, BridgeOrderDetail orderDetail) {
            if (ofrBridge != null && bidBridge != null) {
                if (ofrBridge.bankInstitutionid == bidBridge.bankInstitutionid
                    && ofrBridge.bankTraderid == bidBridge.bankTraderid) {
                    //同一座桥
                    btn_Flag.Visibility = Visibility.Collapsed;
                    gdBrid.ColumnDefinitions[1].Width = new GridLength(0);
                }
                else {
                    //双桥
                    if (orderDetail.bridgeOrder != null && orderDetail.bridgeOrder2 != null) {
                        if ((ofrBridge.bankTraderid == orderDetail.bridgeOrder.instTradeId
                            && ofrBridge.bankInstitutionid == orderDetail.bridgeOrder.instId)) {
                            if (orderDetail.bridgeOrder.bidDir == 2) {
                                //箭头向右
                                btn_Flag.Content = FindResource("LeftImage");
                                //tbSendto.Text = ofrBridge.sendto;
                            }
                            else {
                                //箭头向左
                                btn_Flag.Content = FindResource("RightImage");
                                //tbSendto.Text = bidBridge.sendto;
                            }
                        }
                        else if ((ofrBridge.bankTraderid == orderDetail.bridgeOrder2.instTradeId
                           && ofrBridge.bankInstitutionid == orderDetail.bridgeOrder2.instId)) {
                            if (orderDetail.bridgeOrder2.bidDir == 2) {
                                //箭头向右
                                btn_Flag.Content = FindResource("LeftImage");
                                //tbSendto.Text = ofrBridge.sendto;
                            }
                            else {
                                //箭头向左
                                btn_Flag.Content = FindResource("RightImage");
                                //tbSendto.Text = bidBridge.sendto;
                            }
                        }
                    }
                    btn_Flag.Visibility = Visibility.Visible;
                    gdBrid.ColumnDefinitions[1].Width = new GridLength(26);
                }
            }
            else {
                btn_Flag.Visibility = Visibility.Collapsed;
                gdBrid.ColumnDefinitions[1].Width = new GridLength(0);
            }
        }
        /// <summary>
        /// 批量修改桥要素窗口初始化
        /// </summary>
        public void Init(List<BridgeOrderDetail> orderDetails, UserBridge bridge) {

            //批量修改
            bdOfr.IsBatchEdit = true;
            bdBid.IsBatchEdit = true;

            Bridge = bridge;
            this.orderDetails = orderDetails;
            bdOfr.tbSide.Text = "Ofr方";
            bdBid.tbSide.Text = "Bid方";
            bdBid.CurrentBridge = bdOfr.CurrentBridge = bridge;
            bdBid.tbBnakName.Text = bdOfr.tbBnakName.Text = bridge.bankInstitutionname;
            bdBid.tbBankAgentName.Text = bdOfr.tbBankAgentName.Text = bridge.bankTradername;
            bdBid.correspondingBridgeBankName = bdOfr.correspondingBridgeBankName = Utils.CommonUtils.GetInstName2(bridge.bankInstitutionid); ;//CRMService.Instance.GetInstName2(bridge.bankId);
            bdBid.tbInstTrader.Visibility = bdOfr.tbInstTrader.Visibility = Visibility.Collapsed;
            //修改备注显示
            this.bdOfr.tbRemarksName.Text = "备注Ofr:";
            this.bdBid.tbRemarksName.Text = "备注Bid:";
            //修改标题颜色
            this.bdOfr.tbBnakName.Foreground = this.bdOfr.tbSide.Foreground = (Brush)FindResource("ColorBrush.Blue");
            //默认为桥的发给
            bdBid.tbSendto.Text = bdOfr.tbSendto.Text = bridge.sendto;
            //默认为桥的渠道
            string channel = bridge.channel;
            if (!string.IsNullOrEmpty(channel)) {
                switch (channel) {
                    case "1":
                        bdBid.lbTalk.IsSelected = bdOfr.lbTalk.IsSelected = true;
                        break;
                    case "2":
                        bdBid.lbRequest.IsSelected = bdOfr.lbRequest.IsSelected = true;
                        break;
                    case "3":
                        bdBid.lbDouble.IsSelected = bdOfr.lbDouble.IsSelected = true;
                        break;
                    case "4":
                        bdBid.lbXBond.IsSelected = bdOfr.lbXBond.IsSelected = true;
                        break;
                    case "5":
                        bdBid.lbIDeal.IsSelected = bdOfr.lbIDeal.IsSelected = true;
                        break;
                    case "6":
                        bdBid.lbFixed.IsSelected = bdOfr.lbFixed.IsSelected = true;
                        break;
                    case "7":
                        bdBid.lbBidding.IsSelected = bdOfr.lbBidding.IsSelected = true;
                        break;
                    case "8":
                        bdBid.lbBulk.IsSelected = bdOfr.lbBulk.IsSelected = true;
                        break;
                    case "9":
                        bdBid.lbIntention.IsSelected = bdOfr.lbIntention.IsSelected = true;
                        break;
                    case "10":
                        bdBid.lbAnonymity.IsSelected = bdOfr.lbAnonymity.IsSelected = true;
                        break;
                    case "11":
                        bdBid.lbXbargain.IsSelected = bdOfr.lbXbargain.IsSelected = true;
                        break;
                }
            }
            ///隐藏双桥数据
            gdDoubleBridgeInfo.Visibility = Visibility.Collapsed;
            gdDoubleBridgeInfo.Height = 0;
            ///桥子界面 ,总备注
            spRemark.Visibility = Visibility.Collapsed;
            //隐藏“隐藏备注”单选框
            bdBid.tbShowRemark.Visibility = bdOfr.tbShowRemark.Visibility = Visibility.Collapsed;

            bdBid.bod_Tile.Background = (Brush)FindResource("ColorBrush.Orange11");
            bdBid.gd_main.RowDefinitions[12].Height = new GridLength(0);
            bdOfr.gd_main.RowDefinitions[12].Height = new GridLength(0);

            //批量编辑隐藏发单机构
            bdBid.gd_main.RowDefinitions[13].Height = new GridLength(0);
            bdOfr.gd_main.RowDefinitions[13].Height = new GridLength(0);
            bdBid.tb_BillingInst.Visibility = Visibility.Collapsed;
            bdOfr.tb_BillingInst.Visibility = Visibility.Collapsed;
            bdBid.boxGroupList.Visibility = Visibility.Collapsed;
            bdOfr.boxGroupList.Visibility = Visibility.Collapsed;

            gDetail.RowDefinitions[2].Height = new GridLength(0);
            SetBatchDir(orderDetails, bridge);
            gdBrid.Height = 370;
        }

        /// <summary>
        /// 批量设置方向
        /// </summary>
        private void SetBatchDir(List<BridgeOrderDetail> orderDetails, UserBridge bridge) {

            bool is0frDir = true;
            bool isBidDir = true;
            //中间方向不显示
            btn_Flag.Visibility = Visibility.Collapsed;
            gdBrid.ColumnDefinitions[1].Width = new GridLength(0);


            if (orderDetails == null || orderDetails.Count == 0) {
                bdOfr.btn_OfrFlag.Visibility = Visibility.Collapsed;
                bdBid.btn_OfrFlag.Visibility = Visibility.Collapsed;
                return;
            }
            int ofrDir = 2;
            int bidDir = 2;
            if (orderDetails[0].bridgeOrder != null
                           && orderDetails[0].bridgeOrder.instId == orderDetails[0].BridgeInstitutionId
                           && orderDetails[0].bridgeOrder.instTradeId == orderDetails[0].BridgeTraderId) {
                ofrDir = orderDetails[0].bridgeOrder.ofrDir;
            }
            else if (orderDetails[0].bridgeOrder2 != null
                           && orderDetails[0].bridgeOrder2.instId == orderDetails[0].BridgeInstitutionId
                           && orderDetails[0].bridgeOrder2.instTradeId == orderDetails[0].BridgeTraderId) {
                ofrDir = orderDetails[0].bridgeOrder2.ofrDir;
            }

            if (orderDetails[0].bridgeOrder != null
                          && orderDetails[0].bridgeOrder.instId == orderDetails[0].BridgeInstitutionId2
                          && orderDetails[0].bridgeOrder.instTradeId == orderDetails[0].BridgeTraderId2) {
                bidDir = orderDetails[0].bridgeOrder.bidDir;
            }
            else if (orderDetails[0].bridgeOrder2 != null
                           && orderDetails[0].bridgeOrder2.instId == orderDetails[0].BridgeInstitutionId2
                           && orderDetails[0].bridgeOrder2.instTradeId == orderDetails[0].BridgeTraderId2) {
                bidDir = orderDetails[0].bridgeOrder2.bidDir;
            }
            foreach (BridgeOrderDetail item in orderDetails) {
                //ofr方向
                if (item.BridgeTraderId == bridge.bankTraderid
                    && item.BridgeInstitutionId == bridge.bankInstitutionid) {
                    if (is0frDir) {
                        if (item.bridgeOrder != null
                            && item.bridgeOrder.instId == item.BridgeInstitutionId
                            && item.bridgeOrder.instTradeId == item.BridgeTraderId) {
                            if (item.bridgeOrder.ofrDir != ofrDir) {
                                is0frDir = false;
                            }
                        }
                        else if (item.bridgeOrder2 != null
                          && item.bridgeOrder2.instId == item.BridgeInstitutionId
                          && item.bridgeOrder2.instTradeId == item.BridgeTraderId) {
                            if (item.bridgeOrder2.ofrDir != ofrDir) {
                                is0frDir = false;
                            }
                        }
                    }
                }
                //bid 方向
                if (item.BridgeTraderId2 == bridge.bankTraderid
                   && item.BridgeInstitutionId2 == bridge.bankInstitutionid) {
                    if (isBidDir) {
                        if (item.bridgeOrder != null
                            && item.bridgeOrder.instId == item.BridgeInstitutionId2
                            && item.bridgeOrder.instTradeId == item.BridgeTraderId2) {
                            if (item.bridgeOrder.bidDir != bidDir) {
                                isBidDir = false;
                            }
                        }
                        else if (item.bridgeOrder2 != null
                          && item.bridgeOrder2.instId == item.BridgeInstitutionId2
                          && item.bridgeOrder2.instTradeId == item.BridgeTraderId2) {
                            if (item.bridgeOrder2.bidDir != bidDir) {
                                isBidDir = false;
                            }
                        }
                    }
                }
            }

            if (is0frDir) {
                bdOfr.btn_OfrFlag.Visibility = Visibility.Visible;
                bdOfr.ReFlash(ofrDir);
            }
            else {
                bdOfr.btn_OfrFlag.Visibility = Visibility.Collapsed;
            }
            if (isBidDir) {
                bdBid.btn_OfrFlag.Visibility = Visibility.Visible;
                bdBid.ReFlash(bidDir);
            }
            else {
                bdBid.btn_OfrFlag.Visibility = Visibility.Collapsed;
            }
        }

        private void BridgeDetailWin_Unloaded(object sender, RoutedEventArgs e) {
            this.MouseDown -= Window_MouseDown;
            this.Closed -= Window_Closed;
        }
        /// <summary>
        /// 桥信息是否变更
        /// </summary>
        private bool bridgeInfoChanged(BridgeOrderDetail orderDetailTemp) {
            //单桥要素修改
           
            bdBid.GetData(orderDetailTemp, false);
            bdOfr.GetData(orderDetailTemp, true);
            //双桥要素修改
            orderDetailTemp.DoubleBridgeChannel = GetChannelSelect();

            if (!string.IsNullOrEmpty(tbCost.Text))
                orderDetailTemp.DoubleBridgeFee = tbCost.Text + "厘";
            else
                orderDetailTemp.DoubleBridgeFee = string.Empty;

            //合单备注
            if (orderDetail.BridgeType.Equals("O") || orderDetail.BridgeType.Equals("B")) {

                if (this.Bridge.bankInstitutionid == orderDetail.BridgeInstitutionId
                && this.Bridge.bankTraderid == orderDetail.BridgeTraderId) {
                    orderDetailTemp.combinedOrderRemark = this.tbAllRemarks.Text.Trim();
                }
                else if (this.Bridge.bankInstitutionid == orderDetail.BridgeInstitutionId2
                   && this.Bridge.bankTraderid == orderDetail.BridgeTraderId2) {
                    orderDetailTemp.combinedOrderRemark2 = this.tbAllRemarks.Text.Trim();
                }
            }
            else if (orderDetail.BridgeType.Equals("B,O")) {
                orderDetailTemp.combinedOrderRemark = this.tbAllRemarks.Text.Trim();
                orderDetailTemp.combinedOrderRemark2 = this.tbAllRemarks.Text.Trim();
            }

            //桥方向标识修改
            if (!(orderDetail.BridgeInstitutionId == orderDetail.BridgeInstitutionId2
               && orderDetail.BridgeTraderId == orderDetail.BridgeTraderId)) {
                //双桥发给
                orderDetailTemp.DoubleBridgeSendTo = tbSendto.Text;
                //双桥备注
                orderDetailTemp.DoubleBridgeRemark = tbRemark.Text;
                // 双桥bid清算类型
                orderDetailTemp.DoubleBridgeOffSettltype = GetSettle(true);
                //双桥bid清算类型
                orderDetailTemp.DoubleBridgeBidSettltype = GetSettle(false);
                //双桥错期
                orderDetailTemp.doubleBridgeSettleSide = lbCrossOfr.IsSelected ? "O" : "B";
            }
            //比较
            return !iseQue(orderDetailTemp.BridgeSendTo, orderDetail.BridgeSendTo)
               || !iseQue(orderDetailTemp.BridgeSendTo2, orderDetail.BridgeSendTo2)
               || !iseQue(orderDetailTemp.BridgeChannel, orderDetail.BridgeChannel)
               || !iseQue(orderDetailTemp.BridgeChannel2, orderDetail.BridgeChannel2)
               || !iseQue(orderDetailTemp.BridgeFee, orderDetail.BridgeFee)
               || !iseQue(orderDetailTemp.BridgeFee2, orderDetail.BridgeFee2)
               || !iseQue(orderDetailTemp.BridgeRemark, orderDetail.BridgeRemark)
               || !iseQue(orderDetailTemp.BridgeRemark2, orderDetail.BridgeRemark2)
               || !iseQue(orderDetailTemp.DoubleBridgeBidSettltype, orderDetail.DoubleBridgeBidSettltype)
               || !iseQue(orderDetailTemp.DoubleBridgeOffSettltype, orderDetail.DoubleBridgeOffSettltype)
               || !iseQue(orderDetailTemp.DoubleBridgeChannel, orderDetail.DoubleBridgeChannel)
               || !iseQue(orderDetailTemp.DoubleBridgeFee, orderDetail.DoubleBridgeFee)
               || !iseQue(orderDetailTemp.DoubleBridgeRemark, orderDetail.DoubleBridgeRemark)
               || !iseQue(orderDetailTemp.DoubleBridgeSendTo, orderDetail.DoubleBridgeSendTo)
               || !iseQue(orderDetailTemp.BridgeRemarkShow, orderDetail.BridgeRemarkShow)
               || !iseQue(orderDetailTemp.BridgeRemarkShow2, orderDetail.BridgeRemarkShow2)
               || !iseQue(orderDetailTemp.combinedOrderRemark, GetBridgeRemarkInfo(orderDetail, true))
               || !iseQue(orderDetailTemp.combinedOrderRemark2, GetBridgeRemarkInfo(orderDetail, false))
               || !iseQue(orderDetailTemp.doubleBridgeSettleSide, orderDetail.doubleBridgeSettleSide)
               || !iseQue(orderDetailTemp.ofrSpecialBridgeFee, orderDetail.ofrSpecialBridgeFee)
               || !iseQue(orderDetailTemp.bidSpecialBridgeFee, orderDetail.bidSpecialBridgeFee)
               ||!IsCheckSendOrderInst(orderDetailTemp.ofrIssueInfoReqList, orderDetail.ofrIssueInfoReqList)
               ||!IsCheckSendOrderInst(orderDetailTemp.bidIssueInfoReqList, orderDetail.bidIssueInfoReqList)
                ;
        }


        /// <summary>
        /// 比较发单机构是否发生变化
        /// </summary>
        /// <returns></returns>
        private bool IsCheckSendOrderInst(List<SendInstSevieceModel> oldIssueInfoBOList, List<SendInstSevieceModel> newIssueInfoBOList) {
            bool isSame = true;

            if (oldIssueInfoBOList != null && oldIssueInfoBOList.Count > 0)
            {
                if (newIssueInfoBOList == null)
                {
                    return false;
                }
                else
                {
                    if (newIssueInfoBOList.Count != oldIssueInfoBOList.Count)
                    {
                        return false;
                    }
                    else
                    {
                        for (int i = 0; i < oldIssueInfoBOList.Count; i++)
                        {
                            if (oldIssueInfoBOList[i].issueInstName.Trim() != newIssueInfoBOList[i].issueInstName.Trim()
                                || oldIssueInfoBOList[i].qty != newIssueInfoBOList[i].qty)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            else {
                if (newIssueInfoBOList != null && newIssueInfoBOList.Count > 0) {
                    return false;
                }
            }
            return isSame;
        }

        /// <summary>
        /// 合单备注信息修改
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="isofr">是否ofr方向</param>
        /// <returns></returns>
        private string GetBridgeRemarkInfo(BridgeOrderDetail orderDetail,bool isofr) {

            string strRemarkInfo = string.Empty;

            if (orderDetail.BridgeType.Equals("O") || orderDetail.BridgeType.Equals("B")) {

                if (isofr) {
                    strRemarkInfo = orderDetail.bridgeOrder == null ? "" : orderDetail.bridgeOrder.combinedOrderRemark;
                }
                else {
                    strRemarkInfo = orderDetail.bridgeOrder2 == null ? "" : orderDetail.bridgeOrder2.combinedOrderRemark;
                }
            }
            else if (orderDetail.BridgeType.Equals("B,O")) {
                strRemarkInfo = orderDetail.bridgeOrder == null ? "" : orderDetail.bridgeOrder.combinedOrderRemark;
            }
            return strRemarkInfo;
        }
        private bool iseQue(string oldStr, string newStr) {
            if (string.IsNullOrWhiteSpace(oldStr)) {
                if (string.IsNullOrWhiteSpace(newStr)) {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                if (string.IsNullOrWhiteSpace(newStr)) {
                    return false;
                }
                else {
                    if (oldStr == newStr) {
                        return true;
                    }
                    return false;
                }
            }
        }
        /// <summary>
        /// 桥信息是否变更
        /// </summary>
        private bool bridgeInfoChangedByBatch(BridgeOrderDetail oldOrder, BridgeOrderDetail newOrder, bool isGvn) {
            if (isGvn) {
                return oldOrder.BridgeSendTo != newOrder.BridgeSendTo
                    || oldOrder.BridgeChannel != newOrder.BridgeChannel
                    || oldOrder.BridgeFee != newOrder.BridgeFee
                    || oldOrder.BridgeRemark != newOrder.BridgeRemark;
            }
            else {
                return oldOrder.BridgeSendTo2 != newOrder.BridgeSendTo2
                    || oldOrder.BridgeChannel2 != newOrder.BridgeChannel2
                    || oldOrder.BridgeFee2 != newOrder.BridgeFee2
                    || oldOrder.BridgeRemark2 != newOrder.BridgeRemark2;
            }
        }
        /// <summary>
        /// 发给是否变更
        /// </summary>
        private bool sendToChangedByBatch(BridgeOrderDetail oldOrder, BridgeOrderDetail newOrder, bool isGvn) {
            if (isGvn) {
                return oldOrder.SendTo2 != newOrder.SendTo2;
            }
            else {
                return oldOrder.SendTo != newOrder.SendTo;
            }
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (orderDetail != null)
            {
                // 先获取老的发单机构信息，用于对比新旧桥要素
                BridgeOrderDetail oldOrderDetail = new BridgeOrderDetail();
                oldOrderDetail.ofrIssueInfoReqList = orderDetail.ofrIssueInfoReqList;
                oldOrderDetail.bidIssueInfoReqList = orderDetail.bidIssueInfoReqList;

                if (!bdBid.GetSendOrderInst(orderDetail, false))
                {
                    return;
                }

                if (!bdOfr.GetSendOrderInst(orderDetail, true))
                {
                    return;
                }
                //单桥由4个要素可以修改 双桥有6个可以修改 逐一对比，没有发生变化，则不向后台提交流水更新操作。
                if (!bridgeInfoChanged(oldOrderDetail))
                {
                    btnClose_Click(null, null);
                    return;
                }


                bdBid.GetData(orderDetail, false);
                bdOfr.GetData(orderDetail, true);


                TransOrderServerData execOrderPo = new TransOrderServerData();
                //发单机构
                BridgeOrderReq req = new BridgeOrderReq();
                req.ofrInstExecOrderReqList.AddRange(orderDetail.ofrIssueInfoReqList);
                req.bidInstExecOrderReqList.AddRange(orderDetail.bidIssueInfoReqList);
                execOrderPo.InstExecOrderReqListExt = MsgHelper.Serializer(req);

                execOrderPo.ExecID = orderDetail.ExecID;
                //单桥要素修改ofr
                execOrderPo.BridgeTraderId = orderDetail.BridgeTraderId;
                execOrderPo.BridgeTraderName = orderDetail.BridgeTraderName;
                execOrderPo.BridgeInstitutionFullName = orderDetail.bridgebankcode;
                execOrderPo.BridgeInstitutionName = orderDetail.BridgeInstitutionName;
                execOrderPo.BridgeInstitutionId = orderDetail.BridgeInstitutionId;
                execOrderPo.BridgeChannel = orderDetail.BridgeChannel;
                execOrderPo.BridgeType = orderDetail.BridgeType;
                execOrderPo.BridgeSendTo = orderDetail.BridgeSendTo;
                execOrderPo.BridgeFee = orderDetail.BridgeFee;
                execOrderPo.OFRSpecialBridgeFee = orderDetail.ofrSpecialBridgeFee;
                execOrderPo.BridgeRemark = orderDetail.BridgeRemark;
                execOrderPo.BridgeRemarkShow = orderDetail.BridgeRemarkShow;


                if (orderDetail.BridgeType.Equals("B,O"))
                {
                    execOrderPo.BridgeType2 = orderDetail.BridgeType;
                    execOrderPo.SendTo = orderDetail.SendTo;
                    execOrderPo.SendTo2 = orderDetail.SendTo2;
                }
                else
                {
                    execOrderPo.SendTo = orderDetail.SendTo;
                    execOrderPo.SendTo2 = orderDetail.SendTo2;
                }
                //单桥要素修改bid
                execOrderPo.BridgeTraderName2 = orderDetail.BridgeTraderName2;
                execOrderPo.BridgeInstitutionFullName2 = orderDetail.bridgebankcode2;
                execOrderPo.BridgeInstitutionName2 = orderDetail.BridgeInstitutionName2;
                execOrderPo.BridgeInstitutionId2 = orderDetail.BridgeInstitutionId2;
                execOrderPo.BridgeType2 = orderDetail.BridgeType2;
                execOrderPo.BridgeChannel2 = orderDetail.BridgeChannel2;
                execOrderPo.BridgeFee2 = orderDetail.BridgeFee2;
                execOrderPo.BIDSpecialBridgeFee = orderDetail.bidSpecialBridgeFee;
                execOrderPo.BridgeTraderId2 = orderDetail.BridgeTraderId2;
                execOrderPo.BridgeRemark2 = orderDetail.BridgeRemark2;
                execOrderPo.BridgeSendTo2 = orderDetail.BridgeSendTo2;
                execOrderPo.BridgeRemarkShow2 = orderDetail.BridgeRemarkShow2;

                //20220214 ----2.17.0-----
                execOrderPo.OfrIssueInfoReqList = orderDetail.ofrIssueInfoReqList;
                execOrderPo.BidIssueInfoReqList = orderDetail.bidIssueInfoReqList;


                if (!(orderDetail.BridgeInstitutionId == orderDetail.BridgeInstitutionId2
              && orderDetail.BridgeTraderId == orderDetail.BridgeTraderId))
                {
                    //双桥要素修改
                    execOrderPo.DoubleBridgeChannel = GetChannelSelect();

                    if (!string.IsNullOrEmpty(tbCost.Text))
                        execOrderPo.DoubleBridgeFee = tbCost.Text + "厘";
                    else
                        execOrderPo.DoubleBridgeFee = string.Empty;

                    execOrderPo.DoubleBridgeSendTo = tbSendto.Text;
                    execOrderPo.DoubleBridgeRemark = tbRemark.Text;
                    execOrderPo.DoubleBridgeOffSettltype = GetSettle(true);
                    execOrderPo.DoubleBridgeBidSettltype = GetSettle(false);
                    execOrderPo.DoubleBridgeSettleSide = lbCrossOfr.IsSelected ? "O" : "B";
                }
                //交易要素备注
                //合单备注
                if (orderDetail.BridgeType.Equals("O") || orderDetail.BridgeType.Equals("B"))
                {

                    if (this.Bridge.bankInstitutionid == orderDetail.BridgeInstitutionId && this.Bridge.bankTraderid == orderDetail.BridgeTraderId)
                    {
                        execOrderPo.CombinedOrderRemark = this.tbAllRemarks.Text.Trim();
                    }
                    else
                    {
                        execOrderPo.CombinedOrderRemark = orderDetail.bridgeOrder == null ? "" : orderDetail.bridgeOrder.combinedOrderRemark;
                    }
                    if (this.Bridge.bankInstitutionid == orderDetail.BridgeInstitutionId2 && this.Bridge.bankTraderid == orderDetail.BridgeTraderId2)
                    {
                        execOrderPo.CombinedOrderRemark2 = this.tbAllRemarks.Text.Trim();
                    }
                    else
                    {
                        execOrderPo.CombinedOrderRemark2 = orderDetail.bridgeOrder2 == null ? "" : orderDetail.bridgeOrder2.combinedOrderRemark;
                    }
                }
                else if (orderDetail.BridgeType.Equals("B,O"))
                {
                    execOrderPo.CombinedOrderRemark = this.tbAllRemarks.Text.Trim();
                    execOrderPo.CombinedOrderRemark2 = this.tbAllRemarks.Text.Trim();
                }

                //桥方向标识修改
                if (orderDetail.BridgeInstitutionId == orderDetail.BridgeInstitutionId2
                   && orderDetail.BridgeTraderId == orderDetail.BridgeTraderId)
                {
                    //单桥
                    execOrderPo.OfrDir = orderDetail.OfrDir + "";
                    execOrderPo.BidDir = orderDetail.BidDir + "";
                }
                else
                {
                    //双桥
                    execOrderPo.OfrDir = orderDetail.OfrDir + "";
                    execOrderPo.BidDir2 = orderDetail.BidDir2 + "";
                    if (string.IsNullOrEmpty(execOrderPo.BidDir))
                        execOrderPo.BidDir = "0";
                    Image image = btn_Flag.Content as Image;
                    if (image == null)
                    {
                        orderDetail.Doublebridgedir = 1;
                    }
                    if (image.Name == "LeftImage")
                    {
                        if (orderDetail != null)
                        {
                            orderDetail.Doublebridgedir = 2;
                        }
                    }
                    else
                    {
                        orderDetail.Doublebridgedir = 1;
                        tbSendto.Text = _ofrBridge.sendto;
                    }
                     execOrderPo.Doublebridgedir = orderDetail.Doublebridgedir + "";
                }




                //操作类型
                execOrderPo.CommandType = (int)ECommandType.修改桥要素 + "";
                ///borderID传给后台
                execOrderPo.OperateBorderID = BridgeUIDomain.GetBOrderIdByBridge(orderDetail, Bridge);
                //更新成交单状态
                List<ExecutionReport> list = new List<ExecutionReport>();
                list.Add(execOrderPo);
                OrderHelper.ChangeOrderEx(list);

                btnClose_Click(null, null);

            }
            //批量修改桥要素，目前支持批量修改支持的内容包括：发给，渠道，费用，备注
            if (orderDetails.Any())
            {
                //先发送修改发给，后发送修改桥要素，防止覆盖掉后台的发给重置
                sendToChangeByBatch();
                Thread.Sleep(100);
                bridgeInfoChangeByBatch();
                // OrderHelper.ChangeOrderEx(list);
                btnClose_Click(null, null);
            }
        }

        /// <summary>
        /// 批量修改桥要素
        /// </summary>
        private void bridgeInfoChangeByBatch()
        {
            List<ExecutionReport> list = new List<ExecutionReport>();
            foreach (var orderdetail in orderDetails)
            {
                //UpdateOrderRequestPo result = new UpdateOrderRequestPo();
                //result.id = orderdetail.OrderID;
                //result.execOrders = new List<UpdateExecOrderPo>();
                ExecutionReport execOrderPo = new ExecutionReport();
                //result.execOrders.Add(execOrderPo);
                execOrderPo.ExecID = orderdetail.ExecID;
                BridgeOrderDetail orderDetailTemp = ObjConvertHelper.CopyBridgeOrderDetailo(orderdetail);

                // 双桥并且ofr方是当前桥
                if (orderdetail.BridgeInstitutionId == bdOfr.CurrentBridge.bankInstitutionid && orderdetail.BridgeInstitutionId2 != bdBid.CurrentBridge.bankInstitutionid)
                {
                    bdOfr.SetSentTo(orderDetailTemp, true);
                    bdOfr.GetData(orderDetailTemp, true);
                    //如果单子没有修改，跳过
                    if (!bridgeInfoChangedByBatch(orderdetail, orderDetailTemp, true))
                        continue;
                    execOrderPo.SendTo2 = orderDetailTemp.SendTo2;

                    execOrderPo.BridgeSendTo = orderDetailTemp.BridgeSendTo;
                    execOrderPo.BridgeChannel = orderDetailTemp.BridgeChannel;
                    execOrderPo.BridgeFee = orderDetailTemp.BridgeFee;
                    execOrderPo.OFRSpecialBridgeFee = orderDetailTemp.ofrSpecialBridgeFee;
                    execOrderPo.BridgeRemark = orderDetailTemp.BridgeRemark;
                    //批量修改桥要素，并修改明细是否显示 20200426 wxp
                    //execOrderPo.bridgeremarkshow = orderDetailTemp.bridgeremarkshow;
                    execOrderPo.OfrDir = orderDetailTemp.OfrDir + "";
                    execOrderPo.BidDir = orderDetailTemp.BidDir + "";
                    execOrderPo.BidDir2 = orderDetailTemp.BidDir2 + "";
                    execOrderPo.CommandType = (int)ECommandType.修改桥要素 + "";
                }
                // 双桥并且bid方是当前的桥
                else if (orderdetail.BridgeInstitutionId != bdOfr.CurrentBridge.bankInstitutionid && orderdetail.BridgeInstitutionId2 == bdBid.CurrentBridge.bankInstitutionid)
                {
                    bdBid.SetSentTo(orderDetailTemp, false);
                    bdBid.GetData(orderDetailTemp, false);
                    if (!bridgeInfoChangedByBatch(orderdetail, orderDetailTemp, false))
                        continue;
                    execOrderPo.SendTo = orderDetailTemp.SendTo;

                    execOrderPo.BridgeSendTo2 = orderDetailTemp.BridgeSendTo2;
                    execOrderPo.BridgeChannel2 = orderDetailTemp.BridgeChannel2;
                    execOrderPo.BridgeFee2 = orderDetailTemp.BridgeFee2;
                    execOrderPo.BIDSpecialBridgeFee = orderDetailTemp.bidSpecialBridgeFee;
                    execOrderPo.BridgeRemark2 = orderDetailTemp.BridgeRemark2;
                    //execOrderPo.bridgeremarkshow2 = orderDetailTemp.bridgeremarkshow2;
                    execOrderPo.OfrDir = orderDetailTemp.OfrDir + "";
                    execOrderPo.BidDir = orderDetailTemp.BidDir + "";
                    execOrderPo.BidDir2 = orderDetailTemp.BidDir2 + "";
                    execOrderPo.CommandType = (int)ECommandType.修改桥要素 + "";
                }
                // 单桥
                else if (orderdetail.BridgeInstitutionId == bdOfr.CurrentBridge.bankInstitutionid && orderdetail.BridgeInstitutionId2 == bdBid.CurrentBridge.bankInstitutionid)
                {
                    bdOfr.SetSentTo(orderDetailTemp, true);
                    bdOfr.GetData(orderDetailTemp, true);
                    bdBid.SetSentTo(orderDetailTemp, false);
                    bdBid.GetData(orderDetailTemp, false);
                    if (!bridgeInfoChangedByBatch(orderdetail, orderDetailTemp, false) && !bridgeInfoChangedByBatch(orderdetail, orderDetailTemp, true))
                        continue;
                    execOrderPo.SendTo2 = orderDetailTemp.SendTo2;
                    execOrderPo.SendTo = orderDetailTemp.SendTo;

                    execOrderPo.BridgeSendTo = orderDetailTemp.BridgeSendTo;
                    execOrderPo.BridgeChannel = orderDetailTemp.BridgeChannel;
                    execOrderPo.BridgeFee = orderDetailTemp.BridgeFee;
                    execOrderPo.BridgeRemark = orderDetailTemp.BridgeRemark;

                    execOrderPo.OFRSpecialBridgeFee = orderDetailTemp.ofrSpecialBridgeFee;
                    execOrderPo.BIDSpecialBridgeFee = orderDetailTemp.bidSpecialBridgeFee;

                    execOrderPo.BridgeSendTo2 = orderDetailTemp.BridgeSendTo2;
                    execOrderPo.BridgeChannel2 = orderDetailTemp.BridgeChannel2;
                    execOrderPo.BridgeFee2 = orderDetailTemp.BridgeFee2;
                    execOrderPo.BridgeRemark2 = orderDetailTemp.BridgeRemark2;
                    execOrderPo.OfrDir = orderDetailTemp.OfrDir + "";
                    execOrderPo.BidDir = orderDetailTemp.BidDir + "";
                    execOrderPo.BidDir2 = orderDetailTemp.BidDir2 + "";

                    execOrderPo.CommandType = (int)ECommandType.修改桥要素 + "";
                }
                else
                {
                    LogHelper.Debug("批量修改桥信息，找不到对应的桥。ofr桥：{0}，bid桥：{1}，单子：{2}", bdOfr.CurrentBridge.bankInstitutionid, bdBid.CurrentBridge.bankInstitutionid, orderdetail.ExecID);
                    continue;
                }
                ///borderID传给后台
                execOrderPo.OperateBorderID = BridgeUIDomain.GetBOrderIdByBridge(orderdetail, Bridge);
                list.Add(execOrderPo);
            }
            OrderHelper.ChangeOrderEx(list);
        }
        /// <summary>
        /// 批量修改发给
        /// </summary>
        private void sendToChangeByBatch() {
            List<ExecutionReport> list = new List<ExecutionReport>();
            foreach (var orderdetail in orderDetails) {
                //UpdateOrderRequestPo result = new UpdateOrderRequestPo();
                //result.id = orderdetail.OrderID;
                //result.execOrders = new List<UpdateExecOrderPo>();
                ExecutionReport execOrderPo = new ExecutionReport();
                //result.execOrders.Add(execOrderPo);
                execOrderPo.ExecID = orderdetail.ExecID;

                // BridgeOrderDetail orderDetailTemp = new BridgeOrderDetail();
                BridgeOrderDetail orderDetailTemp = ObjConvertHelper.CopyBridgeOrderDetailo(orderdetail);
                if (orderdetail.BridgeInstitutionId == bdOfr.CurrentBridge.bankInstitutionid && orderdetail.BridgeInstitutionId2 != bdBid.CurrentBridge.bankInstitutionid) {
                    bdOfr.SetSentTo(orderDetailTemp, true);
                    bdOfr.GetData(orderDetailTemp, true);
                    //如果单子没有修改，跳过
                    if (!sendToChangedByBatch(orderdetail, orderDetailTemp, true))
                        continue;
                    execOrderPo.SendTo2 = orderDetailTemp.SendTo2;
                    //execOrderPo.bridgesendto = orderDetailTemp.bridgesendto;
                    //批量修改桥要素，并修改明细是否显示 20200426 wxp
                    //execOrderPo.bridgeremarkshow = orderDetailTemp.bridgeremarkshow;
                    execOrderPo.CommandType = (int)ECommandType.修改Ofr发给 + "";
                }
                else if (orderdetail.BridgeInstitutionId != bdOfr.CurrentBridge.bankInstitutionid && orderdetail.BridgeInstitutionId2 == bdBid.CurrentBridge.bankInstitutionid) {
                    bdBid.SetSentTo(orderDetailTemp, false);
                    bdBid.GetData(orderDetailTemp, false);
                    if (!sendToChangedByBatch(orderdetail, orderDetailTemp, false))
                        continue;
                    execOrderPo.SendTo = orderDetailTemp.SendTo;
                    //execOrderPo.bridgesendto2 = orderDetailTemp.bridgesendto2;
                    //execOrderPo.bridgeremarkshow2 = orderDetailTemp.bridgeremarkshow2;
                    execOrderPo.CommandType = (int)ECommandType.修改Bid发给 + "";
                }
                else if (orderdetail.BridgeInstitutionId == bdOfr.CurrentBridge.bankInstitutionid && orderdetail.BridgeInstitutionId2 == bdBid.CurrentBridge.bankInstitutionid) {
                    bdOfr.SetSentTo(orderDetailTemp, true);
                    bdOfr.GetData(orderDetailTemp, true);
                    bdBid.SetSentTo(orderDetailTemp, false);
                    bdBid.GetData(orderDetailTemp, false);
                    if (!sendToChangedByBatch(orderdetail, orderDetailTemp, true) && !sendToChangedByBatch(orderdetail, orderDetailTemp, false))
                        continue;
                    execOrderPo.SendTo2 = orderDetailTemp.SendTo2;
                    execOrderPo.SendTo = orderDetailTemp.SendTo;
                    //execOrderPo.bridgesendto= orderDetailTemp.bridgesendto;
                    //execOrderPo.bridgesendto2 = orderDetailTemp.bridgesendto2;
                    execOrderPo.CommandType = (int)ECommandType.修改ofr和bid发给 + "";
                }
                else {
                    LogHelper.Debug("批量修改桥发给，找不到对应的桥。ofr桥：{0}，bid桥：{1}，单子：{2}", bdOfr.CurrentBridge.bankInstitutionid, bdBid.CurrentBridge.bankInstitutionid, orderdetail.ExecID);
                    continue;
                }
                ///borderID传给后台
                execOrderPo.OperateBorderID = BridgeUIDomain.GetBOrderIdByBridge(orderdetail, Bridge);
                list.Add(execOrderPo);
            }
            OrderHelper.ChangeOrderEx(list);
        }
        private string GetSettle(bool isOfr) {
            string settleStrOfr = orderDetail.SettlType2;
            string settleStrBid = orderDetail.SettlType;
            switch (settleStrOfr)
            {
                case "1":
                    settleStrOfr = "+0";
                    break;
                case "2":
                    settleStrOfr = "+1";
                    break;
                case "3":
                    settleStrOfr = "明天+0";
                    break;
            }

            switch (settleStrBid)
            {
                case "1":
                    settleStrBid = "+0";
                    break;
                case "2":
                    settleStrBid = "+1";
                    break;
                case "3":
                    settleStrBid = "明天+0";
                    break;
            }


            if (isOfr) {
                if (orderDetail.SettlType == orderDetail.SettlType2) {
                    return settleStrOfr;
                }
                else {
                    if (lbCrossOfr.IsSelected)
                        return settleStrBid + "/" + settleStrOfr;
                    else
                        return settleStrBid;
                }
            }
            else {
                if (orderDetail.SettlType == orderDetail.SettlType2) {
                    return settleStrBid;
                }
                else {
                    if (lbCrossOfr.IsSelected)
                        return settleStrOfr;
                    else
                        return settleStrBid + "/" + settleStrOfr;
                }
            }
        }

        private string GetChannelSelect() {
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

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            btnClose_Click(null, null);
        }

        private void Window_Closed(object sender, EventArgs e) {
            btnClose_Click(null, null);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            OnClose?.Invoke(orderDetail);
            OnCloses?.Invoke(orderDetails);
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                this.btnConfirm_Click(null, null);
        }



        private void lbChannel_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (ListBoxItem item in lbChannel.Items) {
                if (!item.Equals(lbChannel.SelectedItem))
                    item.IsSelected = false;
            }
        }

        private void tbAllRemarks_TextChanged(object sender, TextChangedEventArgs e) {
            string newTxt = tbAllRemarks.Text;
            if (newTxt.Length > 30) {
                e.Handled = true;
                tbAllRemarks.Text = newTxt.Substring(0, 30);
            }
        }

        private void btn_Flag_Click(object sender, RoutedEventArgs e) {

            Button button = sender as Button;
            if (button != null) {
                Image image = button.Content as Image;
                if (image == null) {
                    return;
                }
                if (image.Name == "LeftImage") {
                    button.Content = FindResource("RightImage");
                    if (orderDetail != null) {
                        orderDetail.Doublebridgedir = 1;
                        tbSendto.Text = _bidBridge.sendto;
                    }
                }
                else {
                    button.Content = FindResource("LeftImage");
                    orderDetail.Doublebridgedir = 2;
                    tbSendto.Text = _ofrBridge.sendto;
                }
            }
        }
    }
}
