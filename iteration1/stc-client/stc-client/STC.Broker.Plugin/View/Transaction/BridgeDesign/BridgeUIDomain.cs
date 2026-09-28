
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using STC.Broker.Plugin.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    public class BridgeUIDomain
    {
        /// <summary>
        /// 更新桥的外观和数据
        /// </summary>
        /// <param name="filterOrder"></param>
        /// <param name="oldOrder"></param>
        /// <param name="newOrder"></param>
        /// <param name="Bridge"></param>
        public static void UpdateOrder(List<BridgeOrderDetail> filterOrder, BridgeOrderDetail oldOrder, OrderDetailPo newOrder, UserBridge Bridge)
        {
            string editImageSource = oldOrder.EditImageSource;
            string ofrImageSource = string.Empty;
            string bidImageSource = string.Empty;

            if (oldOrder.BridgeType.Equals("B,O") && !newOrder.BridgeType.Equals("B,O"))
            {
                if (newOrder.BridgeType.Equals("B"))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";

                }
                else if (newOrder.BridgeType.Equals("O"))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                oldOrder.OfrIsEnable = false;
                oldOrder.BidIsEnable = false;
            }
            else if ((oldOrder.BridgeType.Equals("B") || oldOrder.BridgeType.Equals("O")) && newOrder.BridgeType.Equals("B,O"))//双桥变单桥
            {
                ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";

                oldOrder.OfrIsEnable = true;
                oldOrder.BidIsEnable = true;
            }
            else if (oldOrder.BridgeType.Equals("B") && newOrder.BridgeType.Equals("B"))
            {
                if (IsBidBridge(oldOrder, Bridge))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";
                }
                else
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                oldOrder.OfrIsEnable = false;
                oldOrder.BidIsEnable = false;
            }
            else if (oldOrder.BridgeType.Equals("O") && newOrder.BridgeType.Equals("O"))
            {
                if (IsBidBridge(oldOrder, Bridge))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";

                }
                else
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                oldOrder.OfrIsEnable = false;
                oldOrder.BidIsEnable = false;
            }
            else if (oldOrder.BridgeType.Equals("B,O") && newOrder.BridgeType.Equals("B,O"))
            {
                if (string.IsNullOrEmpty(newOrder.BridgeStatus))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";

                    oldOrder.OfrIsEnable = true;
                    oldOrder.BidIsEnable = true;
                }
                else
                {
                    if (newOrder.BridgeStatus.Equals("B"))
                    {
                        ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                        oldOrder.OfrIsEnable = false;

                        bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_standby.png";
                        oldOrder.BidIsEnable = true;
                    }
                    else if (newOrder.BridgeStatus.Equals("O"))
                    {
                        ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_standby.png";
                        oldOrder.OfrIsEnable = true;

                        bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                        oldOrder.BidIsEnable = false;
                    }
                }
            }

            ObjConvertHelper.UpdateOrder(newOrder, oldOrder);
            oldOrder.isBidBridge = IsBidBridge(oldOrder, Bridge) ? "1" : "2";
            UpdateShowProperty(oldOrder, Bridge);
            editImageSource = GetEditImageSource(newOrder);


            oldOrder.EditImageSource = editImageSource;
            oldOrder.OfrImageSource = ofrImageSource;
            oldOrder.BidImageSource = bidImageSource;
            //SyncDetail(filterOrder, oldOrder, true, false, !OrderHelper.TodayReceipt(oldOrder));

        }

        public static void UpdateOrderEx(List<BridgeOrderDetail> filterOrder, BridgeOrderDetail oldOrder, TransOrderServerData newOrder, UserBridge Bridge)
        {
            string editImageSource = oldOrder.EditImageSource;
            string ofrImageSource = string.Empty;
            string bidImageSource = string.Empty;

            if (oldOrder.BridgeType.Equals("B,O") && !newOrder.BridgeType.Equals("B,O"))
            {
                if (newOrder.BridgeType.Equals("B"))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";

                }
                else if (newOrder.BridgeType.Equals("O"))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                oldOrder.OfrIsEnable = false;
                oldOrder.BidIsEnable = false;
            }
            else if ((oldOrder.BridgeType.Equals("B") || oldOrder.BridgeType.Equals("O")) && newOrder.BridgeType.Equals("B,O"))//双桥变单桥
            {
                ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";

                oldOrder.OfrIsEnable = true;
                oldOrder.BidIsEnable = true;
            }
            else if (oldOrder.BridgeType.Equals("B") && newOrder.BridgeType.Equals("B"))
            {
                if (IsBidBridge(oldOrder, Bridge))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";
                }
                else
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                oldOrder.OfrIsEnable = false;
                oldOrder.BidIsEnable = false;
            }
            else if (oldOrder.BridgeType.Equals("O") && newOrder.BridgeType.Equals("O"))
            {
                if (IsBidBridge(oldOrder, Bridge))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";

                }
                else
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                oldOrder.OfrIsEnable = false;
                oldOrder.BidIsEnable = false;
            }
            else if (oldOrder.BridgeType.Equals("B,O") && newOrder.BridgeType.Equals("B,O"))
            {
                if (string.IsNullOrEmpty(newOrder.BridgeStatus))
                {
                    ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";

                    oldOrder.OfrIsEnable = true;
                    oldOrder.BidIsEnable = true;
                }
                else
                {
                    if (newOrder.BridgeStatus.Equals("B"))
                    {
                        ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                        oldOrder.OfrIsEnable = false;

                        bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_standby.png";
                        oldOrder.BidIsEnable = true;
                    }
                    else if (newOrder.BridgeStatus.Equals("O"))
                    {
                        ofrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_standby.png";
                        oldOrder.OfrIsEnable = true;

                        bidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                        oldOrder.BidIsEnable = false;
                    }
                }
            }
            
          
            ObjConvertHelper.UpdateOrderEx(newOrder, oldOrder);
            oldOrder.isBidBridge = IsBidBridge(oldOrder, Bridge) ? "1" : "2";
            UpdateShowProperty(oldOrder, Bridge);
            editImageSource = GetEditImageSourceEx(newOrder);


            oldOrder.EditImageSource = editImageSource;
            oldOrder.OfrImageSource = ofrImageSource;
            oldOrder.BidImageSource = bidImageSource;
            //SyncDetail(filterOrder, oldOrder, true, false, !OrderHelper.TodayReceipt(oldOrder));

        }
        /// <summary>
        /// 判断订单是否属于桥
        /// </summary>
        /// <param name="order"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static bool OrderBelongtoBridge(OrderDetailPo order, UserBridge bridge)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(order.BridgeType))
            {
                return flag;
            }
            else if (order.BridgeType + "" == "B,O")
            {
                if (order.bridgeOrder != null && order.bridgeOrder.instId == bridge.bankInstitutionid && order.bridgeOrder.instTradeId == bridge.bankTraderid)
                {
                    flag = true;
                }
            }
            else
            {
                if (order.bridgeOrder != null && order.bridgeOrder.instId == bridge.bankInstitutionid && order.bridgeOrder.instTradeId == bridge.bankTraderid)
                {
                    flag = true;
                }
                if (order.bridgeOrder2 != null && order.bridgeOrder2.instId == bridge.bankInstitutionid && order.bridgeOrder2.instTradeId == bridge.bankTraderid)
                {
                    flag = true;
                }
            }
            return flag;
        }
        /// <summary>
        /// 获取指定桥的bridgeorder
        /// </summary>
        /// <param name="order"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static bridgeOrder GetBOrderByBirdge(OrderDetailPo order, UserBridge bridge)
        {
            bridgeOrder bridgeorder = null;
            if (order.BridgeType.Equals("B,O"))
            {
                bridgeorder = order.bridgeOrder;
            }
            else
            {
                if (order.bridgeOrder != null && order.bridgeOrder.instId == bridge.bankInstitutionid && order.bridgeOrder.instTradeId == bridge.bankTraderid)
                {
                    bridgeorder = order.bridgeOrder;
                }
                if (order.bridgeOrder2 != null && order.bridgeOrder2.instId == bridge.bankInstitutionid && order.bridgeOrder2.instTradeId == bridge.bankTraderid)
                {
                    bridgeorder = order.bridgeOrder2;
                }
            }
            return bridgeorder;
        }
        /// <summary>
        /// 根据桥获取桥订单的borderid；
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static string GetBOrderIdByBridge(BridgeOrderDetail orderDetail, UserBridge bridge)
        {
            string type = BridgeUIDomain.GetBridgeType(orderDetail);
            string borderID;
            if (type == "B,O")
            {
                borderID = orderDetail.BorderId;
            }
            else
            {
                if (orderDetail.bridgeOrder == null)
                {
                    borderID = "";
                }
                else
                {
                    if (orderDetail.bridgeOrder == null)
                    {
                        borderID = "";
                    }
                    else
                    {
                        if (orderDetail.bridgeOrder.instId == bridge.bankInstitutionid && orderDetail.bridgeOrder.instTradeId == bridge.bankTraderid)
                            borderID = orderDetail.BorderId;
                        else
                            borderID = orderDetail.BorderId2;
                    }
                }
            }
            return borderID;
        }

        
        /// <summary>
        /// 根据桥获取订单在某座桥下的的borderid；
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static string GetBOrderIdByBridge(OrderDetailPo orderDetail, UserBridge bridge)
        {
            string type = BridgeUIDomain.GetBridgeType(orderDetail, bridge);
            string borderID;
            if (type == "B,O")
            {
                borderID = orderDetail.BorderId;
            }
            else
            {
                if (orderDetail.bridgeOrder == null)
                {
                    borderID = "";
                }
                else
                {
                    if (orderDetail.bridgeOrder == null)
                    {
                        borderID = "";
                    }
                    else
                    {
                        if (orderDetail.bridgeOrder.instId == bridge.bankInstitutionid && orderDetail.bridgeOrder.instTradeId == bridge.bankTraderid)
                            borderID = orderDetail.BorderId;
                        else
                            borderID = orderDetail.BorderId2;
                    }
                }
            }
            return borderID;
        }

        public static string GetBOrderIdByBridgeEx(TransOrderServerData orderDetail, UserBridge bridge)
        {
            string type = BridgeUIDomain.GetBridgeTypeEx(orderDetail, bridge);
            string borderID;
            if (type == "B,O")
            {
                borderID = orderDetail.BorderId;
            }
            else
            {
                //if (orderDetail.bridgeOrder == null)
                //{
                //    borderID = "";
                //}
                //else
                //{
                //    if (orderDetail.bridgeOrder == null)
                //    {
                //        borderID = "";
                //    }
                //    else
                //    {
                if (orderDetail.BridgeInstitutionId == bridge.bankInstitutionid && orderDetail.BridgeTraderId == bridge.bankTraderid)
                    borderID = orderDetail.BorderId;
                else
                    borderID = orderDetail.BorderId2;
                //}
                //}
            }

            return borderID;
        }



        /// <summary>
        /// 获取桥订单的桥类型
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        public static string GetBridgeType(BridgeOrderDetail orderDetail)
        {
            string bridgeType = string.Empty;
            if (orderDetail.BridgeType.Equals("B,O"))
                bridgeType = "B,O";
            else
                bridgeType = orderDetail.isBidBridge.Equals("1") ? "B" : "O";

            return bridgeType;
        }

        /// <summary>
        /// 获取执行订单的在某座桥下的桥类型
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        public static string GetBridgeType(OrderDetailPo orderDetail, UserBridge bridge)
        {
            string bridgeType = string.Empty;
            if (orderDetail.BridgeType.Equals("B,O"))
                bridgeType = "B,O";
            else
            {
                if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                {
                    bridgeType = orderDetail.BridgeType2;
                }

                if (orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))
                {
                    bridgeType = orderDetail.BridgeType;
                }
            }
            return bridgeType;
        }


        /// <summary>
        /// 获取执行订单的在某座桥下的桥类型
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        public static string GetBridgeType2(TransOrderServerData orderDetail, UserBridge bridge)
        {
            string bridgeType = string.Empty;
            if (orderDetail.BridgeType.Equals("B,O"))
                bridgeType = "B,O";
            else
            {
                if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                {
                    bridgeType = orderDetail.BridgeType2;
                }

                if (orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))
                {
                    bridgeType = orderDetail.BridgeType;
                }
            }
            return bridgeType;
        }

        public static string GetBridgeTypeEx(TransOrderServerData orderDetail, UserBridge bridge)
        {
            string bridgeType = string.Empty;
            if (orderDetail.BridgeType.Equals("B,O"))
                bridgeType = "B,O";
            else
            {
                if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                {
                    bridgeType = orderDetail.BridgeType2;
                }

                if (orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))
                {
                    bridgeType = orderDetail.BridgeType;
                }
            }
            return bridgeType;
        }
        /// <summary>
        /// 加桥单的显示文本
        /// </summary>
        /// <param name="newBridgeOrder"></param>
        /// <param name="bridge"></param>
        public static void UpdateOrderTxt(BridgeOrderDetail newBridgeOrder, UserBridge bridge)
        {
            newBridgeOrder.OldOrderNoseqTxt = BridgeHelper.CreateBridgeTxt(newBridgeOrder, GetBridgeType(newBridgeOrder), bridge);
            newBridgeOrder.OldOrderTxt = string.Format("{0}  {1}", newBridgeOrder.ShowSeq, newBridgeOrder.OldOrderNoseqTxt);
            newBridgeOrder.OrderTxt = BridgeHelper.GetBridgeTxt(newBridgeOrder, bridge, newBridgeOrder.OldOrderTxt);
        }
        /// <summary>
        /// 设置过桥单的显示文本 
        /// </summary>
        /// <param name="newBridgeOrder"></param>
        /// <param name="bridge"></param>
        public static void UpdateShowProperty(BridgeOrderDetail newBridgeOrder, UserBridge bridge)
        {
            newBridgeOrder.OldOrderNoseqTxt = BridgeHelper.CreateBridgeTxt(newBridgeOrder, GetBridgeType(newBridgeOrder), bridge);
            newBridgeOrder.OldOrderTxt = string.Format("{0}  {1}", newBridgeOrder.ShowSeq,newBridgeOrder.OldOrderNoseqTxt);
            newBridgeOrder.OrderTxt = BridgeHelper.GetBridgeTxt(newBridgeOrder, bridge, newBridgeOrder.OldOrderTxt);
            ///桥状态 V2.5 20200629 wxp 
            newBridgeOrder.BridgeStateShow = BridgeHelper.getBridgeState(newBridgeOrder, bridge);
            newBridgeOrder.OfrBridgeStateShow = BridgeHelper.GetBridgeState(newBridgeOrder, bridge, 1);
            newBridgeOrder.BidBridgeStateShow = BridgeHelper.GetBridgeState(newBridgeOrder, bridge, 2);

            if (newBridgeOrder.OfrBridgeStateShow == Constant.BridgeStatusShow
                || newBridgeOrder.OfrBridgeStateShow == "已取消")
            {
                if (newBridgeOrder.OfrBridgeStateShow == "已取消")
                {
                    newBridgeOrder.OfrCancelStatusVisibility = Visibility.Visible;
                    newBridgeOrder.OfrContentStatusVisibility = Visibility.Collapsed;
                    if (newBridgeOrder.BridgeType.Equals("B,O"))
                    {
                        newBridgeOrder.OfrSendStatusreason = (newBridgeOrder.ofrIssueInfoReqList == null || newBridgeOrder.ofrIssueInfoReqList.Count == 0) ? BridgeHelper.GetBridgeRejectReason(newBridgeOrder, bridge, 1) : newBridgeOrder.ofrIssueInfoReqList[0].RejectReason;
                    }
                    else
                    {
                        if ((newBridgeOrder.BridgeType.Equals("B") && newBridgeOrder.BridgeType2.Equals("O"))
                  || (newBridgeOrder.BridgeType.Equals("O") && newBridgeOrder.BridgeType2.Equals("B")))
                        {
                            if (newBridgeOrder.BridgeTraderId2.Equals(bridge.bankTraderid) && newBridgeOrder.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                            {
                                newBridgeOrder.OfrSendStatusreason = BridgeHelper.GetBridgeRejectReason(newBridgeOrder, bridge, 1);


                            }
                            else
                            {
                                newBridgeOrder.OfrSendStatusreason = (newBridgeOrder.ofrIssueInfoReqList == null || newBridgeOrder.ofrIssueInfoReqList.Count == 0) ? BridgeHelper.GetBridgeRejectReason(newBridgeOrder, bridge, 1) : newBridgeOrder.ofrIssueInfoReqList[0].RejectReason;
                            }
                        }
                    }
                }
                else
                {
                    newBridgeOrder.OfrCancelStatusVisibility = Visibility.Collapsed;
                    newBridgeOrder.OfrContentStatusVisibility = Visibility.Visible;
                }

                if (newBridgeOrder.OfrBridgeStateShow == Constant.BridgeStatusShow)
                {
                    newBridgeOrder.OfrCancelStatusVisibility = Visibility.Collapsed;
                    newBridgeOrder.OfrContentStatusVisibility = Visibility.Visible;
                }
                else
                {
                    newBridgeOrder.OfrCancelStatusVisibility = Visibility.Visible;
                    newBridgeOrder.OfrContentStatusVisibility = Visibility.Collapsed;
                }

            }
            else
            {
                newBridgeOrder.OfrCancelStatusVisibility = Visibility.Collapsed;
                newBridgeOrder.OfrContentStatusVisibility = Visibility.Collapsed;
            }

            if (newBridgeOrder.OfrBridgeStateShow == "已成交")
            {
                newBridgeOrder.OfrToolTipVisibility = Visibility.Collapsed;
            }
            else
            {
                if (newBridgeOrder.OfrBridgeStateShow != "已取消"
                    && newBridgeOrder.OfrBridgeStateShow != Constant.BridgeStatusShow)
                {
                    newBridgeOrder.OfrToolTipVisibility = Visibility.Collapsed;
                }
                else
                {
                    newBridgeOrder.OfrToolTipVisibility = Visibility.Visible;
                }
            }


            if (newBridgeOrder.BidBridgeStateShow == Constant.BridgeStatusShow
                || newBridgeOrder.BidBridgeStateShow == "已取消")
            {
                if (newBridgeOrder.BidBridgeStateShow == "已取消")
                {
                    newBridgeOrder.BidCancelStatusVisibility = Visibility.Visible;
                    newBridgeOrder.BidContentStatusVisibility = Visibility.Collapsed;
                    if (newBridgeOrder.BridgeType.Equals("B,O"))
                    {
                        newBridgeOrder.BidSendStatusreason = (newBridgeOrder.bidIssueInfoReqList == null || newBridgeOrder.bidIssueInfoReqList.Count == 0) ? BridgeHelper.GetBridgeRejectReason(newBridgeOrder, bridge, 2) : newBridgeOrder.bidIssueInfoReqList[0].RejectReason;
                    }
                    else
                    {
                        if ((newBridgeOrder.BridgeType.Equals("B") && newBridgeOrder.BridgeType2.Equals("O"))
                                  || (newBridgeOrder.BridgeType.Equals("O") && newBridgeOrder.BridgeType2.Equals("B")))
                        {
                            if (newBridgeOrder.BridgeTraderId.Equals(bridge.bankTraderid) && newBridgeOrder.BridgeInstitutionId.Equals(bridge.bankInstitutionid))
                            {
                                newBridgeOrder.BidSendStatusreason = BridgeHelper.GetBridgeRejectReason(newBridgeOrder, bridge, 2);

                            }
                            else
                            {
                                newBridgeOrder.BidSendStatusreason = (newBridgeOrder.bidIssueInfoReqList == null || newBridgeOrder.bidIssueInfoReqList.Count == 0) ? BridgeHelper.GetBridgeRejectReason(newBridgeOrder, bridge, 2) : newBridgeOrder.bidIssueInfoReqList[0].RejectReason;
                            }
                        }
                    }
                }
                else
                {
                    newBridgeOrder.BidCancelStatusVisibility = Visibility.Collapsed;
                    newBridgeOrder.BidContentStatusVisibility = Visibility.Visible;
                }

                if (newBridgeOrder.BidBridgeStateShow == Constant.BridgeStatusShow)
                {
                    newBridgeOrder.BidCancelStatusVisibility = Visibility.Collapsed;
                    newBridgeOrder.BidContentStatusVisibility = Visibility.Visible;
                }
                else
                {
                    newBridgeOrder.BidCancelStatusVisibility = Visibility.Visible;
                    newBridgeOrder.BidContentStatusVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                newBridgeOrder.BidCancelStatusVisibility = Visibility.Collapsed;
                newBridgeOrder.BidContentStatusVisibility = Visibility.Collapsed;
            }
            if (newBridgeOrder.BidBridgeStateShow == "已成交")
            {
                newBridgeOrder.BidToolTipVisibility = Visibility.Collapsed;
            }
            else
            {
                if (newBridgeOrder.BidBridgeStateShow != "已取消"
                    && newBridgeOrder.BidBridgeStateShow != Constant.BridgeStatusShow)
                {
                    newBridgeOrder.BidToolTipVisibility = Visibility.Collapsed;
                }
                else
                {
                    newBridgeOrder.BidToolTipVisibility = Visibility.Visible;
                }
            }


            ///取消的单子不可编辑
            if (newBridgeOrder.OfrBridgeStateShow == "已取消"
                && newBridgeOrder.BidBridgeStateShow == "已取消")
            {
                newBridgeOrder.OfrIsEnable = false;
                newBridgeOrder.BidIsEnable = false;
                newBridgeOrder.EditIsEnable = false;
            }
            if (newBridgeOrder.IsGVN)
            {
                newBridgeOrder.ShowBrokerNameA = ConvertTextHelper.IsChinese(newBridgeOrder.PartySubName);
                newBridgeOrder.ShowBrokerNameB = ConvertTextHelper.IsChinese(newBridgeOrder.CounterPartySubName);
                newBridgeOrder.OfrInstName = newBridgeOrder.InstitutionName;
                newBridgeOrder.BidInstName = newBridgeOrder.CounterPartyInstitutionName;
            }
            else
            {
                newBridgeOrder.ShowBrokerNameA = ConvertTextHelper.IsChinese(newBridgeOrder.CounterPartySubName);
                newBridgeOrder.ShowBrokerNameB = ConvertTextHelper.IsChinese(newBridgeOrder.PartySubName);
                newBridgeOrder.OfrInstName = newBridgeOrder.CounterPartyInstitutionName;
                newBridgeOrder.BidInstName = newBridgeOrder.InstitutionName;
            }

            newBridgeOrder.OfrInstNamePY = ChinesePYHelper.GetFirstPY(newBridgeOrder.OfrInstName);
            newBridgeOrder.BidInstNamePY = ChinesePYHelper.GetFirstPY(newBridgeOrder.BidInstName);

            newBridgeOrder.maturityDays= IDCHelper.GetMaturityDays(newBridgeOrder.timeToMaturity);

        }
        /// <summary>
        /// 单桥也认为是bid桥
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static bool IsBidBridge(OrderDetailPo orderDetail, UserBridge bridge)
        {
            if ((orderDetail.BridgeType.Equals("B") && orderDetail.BridgeType2.Equals("O"))
                || (orderDetail.BridgeType.Equals("O") && orderDetail.BridgeType2.Equals("B")))
            {
                if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid)
                                && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                    return true;
                else
                    return false;
            }
            else
                return true;

        }
        public static bool IsBidBridgeEx(TransOrderServerData orderDetail, UserBridge bridge)
        {
            if ((orderDetail.BridgeType.Equals("B") && orderDetail.BridgeType2.Equals("O"))
                || (orderDetail.BridgeType.Equals("O") && orderDetail.BridgeType2.Equals("B")))
            {
                if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid)
                                && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                    return true;
                else
                    return false;
            }
            else
                return true;

        }
        /// <summary>
        /// 判断桥是否是成交单的bid桥
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static bool IsBidBridge(BridgeOrderDetail orderDetail, UserBridge bridge)
        {
            if ((orderDetail.BridgeType.Equals("B") && orderDetail.BridgeType2.Equals("O"))
                || (orderDetail.BridgeType.Equals("O") && orderDetail.BridgeType2.Equals("B")))
            {
                if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid)
                                && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        public static BridgeOrderDetail CreateBridgeOrderDetail(OrderDetailPo orderDetail, UserBridge bridge)
        {
            BridgeOrderDetail newBridgeOrder = ObjConvertHelper.CopyToEx(orderDetail);
            ///获取当前桥下的唯一订单ID
            newBridgeOrder.MasterID = BridgeUIDomain.GetBOrderIdByBridge(orderDetail, bridge);

            newBridgeOrder.isBidBridge = IsBidBridge(newBridgeOrder, bridge) ? "1" : "2";
            UpdateShowProperty(newBridgeOrder, bridge);

            //if (newBridgeOrder.IsGVN)
            //{
            //    newBridgeOrder.ShowBrokerNameA = ConvertTextHelper.IsChinese(newBridgeOrder.brokerNameA);
            //    newBridgeOrder.ShowBrokerNameB = ConvertTextHelper.IsChinese(newBridgeOrder.brokerNameB);
            //    newBridgeOrder.OfrInstName = newBridgeOrder.bankCodeA;
            //    newBridgeOrder.BidInstName = newBridgeOrder.bankCodeB;
            //}
            //else
            //{
            //    newBridgeOrder.ShowBrokerNameA = ConvertTextHelper.IsChinese(newBridgeOrder.brokerNameB);
            //    newBridgeOrder.ShowBrokerNameB = ConvertTextHelper.IsChinese(newBridgeOrder.brokerNameA);
            //    newBridgeOrder.OfrInstName = newBridgeOrder.bankCodeB;
            //    newBridgeOrder.BidInstName = newBridgeOrder.bankCodeA;
            //}

            //newBridgeOrder.OfrInstNamePY = ChinesePYHelper.GetFirstPY(newBridgeOrder.OfrInstName);
            //newBridgeOrder.BidInstNamePY = ChinesePYHelper.GetFirstPY(newBridgeOrder.BidInstName);


            SetImagePath(newBridgeOrder, orderDetail, bridge);

            if (OrderHelper.TodayReceipt(newBridgeOrder))
                newBridgeOrder.OrderGroup = 3;
            else
                newBridgeOrder.OrderGroup = 1;

            //设置桥方向
            SetBridgeDir(newBridgeOrder);
            return newBridgeOrder;
        }


        public static BridgeOrderDetail CreateBridgeOrderDetailEx(TransOrderServerData orderDetail, UserBridge bridge)
        {
            BridgeOrderDetail newBridgeOrder = ObjConvertHelper.CopyToEx2(orderDetail);
            ///获取当前桥下的唯一订单ID
            newBridgeOrder.MasterID = BridgeUIDomain.GetBOrderIdByBridgeEx(orderDetail, bridge);

            newBridgeOrder.isBidBridge = IsBidBridge(newBridgeOrder, bridge) ? "1" : "2";
            UpdateShowProperty(newBridgeOrder, bridge);

            //if (newBridgeOrder.IsGVN)
            //{
            //    newBridgeOrder.ShowBrokerNameA = ConvertTextHelper.IsChinese(newBridgeOrder.brokerNameA);
            //    newBridgeOrder.ShowBrokerNameB = ConvertTextHelper.IsChinese(newBridgeOrder.brokerNameB);
            //    newBridgeOrder.OfrInstName = newBridgeOrder.bankCodeA;
            //    newBridgeOrder.BidInstName = newBridgeOrder.bankCodeB;
            //}
            //else
            //{
            //    newBridgeOrder.ShowBrokerNameA = ConvertTextHelper.IsChinese(newBridgeOrder.brokerNameB);
            //    newBridgeOrder.ShowBrokerNameB = ConvertTextHelper.IsChinese(newBridgeOrder.brokerNameA);
            //    newBridgeOrder.OfrInstName = newBridgeOrder.bankCodeB;
            //    newBridgeOrder.BidInstName = newBridgeOrder.bankCodeA;
            //}

            //newBridgeOrder.OfrInstNamePY = ChinesePYHelper.GetFirstPY(newBridgeOrder.OfrInstName);
            //newBridgeOrder.BidInstNamePY = ChinesePYHelper.GetFirstPY(newBridgeOrder.BidInstName);


            SetImagePathEx(newBridgeOrder, orderDetail, bridge);

            if (OrderHelper.TodayReceipt(newBridgeOrder))
                newBridgeOrder.OrderGroup = 3;
            else
                newBridgeOrder.OrderGroup = 1;

            //设置桥方向
            SetBridgeDir(newBridgeOrder);
            return newBridgeOrder;
        }
        /// <summary>
        /// 设置桥之间方向
        /// </summary>
        /// <param name="bridgeOrderDetail"></param>
        private static void SetBridgeDir(BridgeOrderDetail bridgeOrderDetail)
        {
            if (bridgeOrderDetail == null)
            {
                return;
            }
            if (bridgeOrderDetail.BridgeInstitutionId2 == bridgeOrderDetail.BridgeInstitutionId
                && bridgeOrderDetail.BridgeInstitutionId == bridgeOrderDetail.BridgeInstitutionId2)
            {
                //单桥
                bridgeOrderDetail.Doublebridgedir = 0;
            }
            else
            {
                if (bridgeOrderDetail.bridgeOrder != null)
                {
                    if (bridgeOrderDetail.bridgeOrder.bidDir == 1)
                    {
                        bridgeOrderDetail.Doublebridgedir = 1;
                    }
                    else
                    {
                        bridgeOrderDetail.Doublebridgedir = 2;
                    }
                }
            }

        }

        public static void SetImagePath(BridgeOrderDetail newBridgeOrder, OrderDetailPo orderDetail, UserBridge bridge)
        {
            newBridgeOrder.DeleteImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_delete_normal.png";
            newBridgeOrder.EditImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_tag_normal.png";
            newBridgeOrder.DeleteIsEnable = true;
            newBridgeOrder.EditIsEnable = true;

            newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
            newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";

            if (orderDetail.BridgeType.Equals("B,O"))
            {
                if (string.IsNullOrEmpty(orderDetail.BridgeStatus))
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";

                    newBridgeOrder.OfrIsEnable = true;
                    newBridgeOrder.BidIsEnable = true;
                }
                else
                {
                    if (newBridgeOrder.BridgeStatus.Equals("B"))
                    {
                        newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                        newBridgeOrder.OfrIsEnable = false;

                        newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_standby.png";
                        newBridgeOrder.BidIsEnable = true;
                    }
                    else if (newBridgeOrder.BridgeStatus.Equals("O"))
                    {
                        newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_standby.png";
                        newBridgeOrder.OfrIsEnable = true;

                        newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                        newBridgeOrder.BidIsEnable = false;
                    }
                }

                newBridgeOrder.EditImageSource = GetEditImageSource(orderDetail);

            }
            else if (orderDetail.BridgeType.Equals("B") && orderDetail.BridgeType2.Equals("O"))
            {
                if (orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))//ofr方
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                else if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))//bid方
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";
                }

                newBridgeOrder.OfrIsEnable = false;
                newBridgeOrder.BidIsEnable = false;

                newBridgeOrder.EditImageSource = GetEditImageSource(orderDetail);
            }
            else if (orderDetail.BridgeType.Equals("O") && orderDetail.BridgeType2.Equals("B"))
            {
                if (orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))//ofr方
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                else if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))//bid方
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";
                }

                newBridgeOrder.OfrIsEnable = false;
                newBridgeOrder.BidIsEnable = false;

                newBridgeOrder.EditImageSource = GetEditImageSource(orderDetail);

            }
            ///取消的单子不可编辑
            if (newBridgeOrder.OfrBridgeStateShow == "已取消"
                && newBridgeOrder.BidBridgeStateShow == "已取消")
            {
                newBridgeOrder.OfrIsEnable = false;
                newBridgeOrder.BidIsEnable = false;
                newBridgeOrder.EditIsEnable = false;
            }
        }

        public static void SetImagePathEx(BridgeOrderDetail newBridgeOrder, TransOrderServerData orderDetail, UserBridge bridge)
        {
            newBridgeOrder.DeleteImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_delete_normal.png";
            newBridgeOrder.EditImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_tag_normal.png";
            newBridgeOrder.DeleteIsEnable = true;
            newBridgeOrder.EditIsEnable = true;

            newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
            newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";

            if (orderDetail.BridgeType.Equals("B,O"))
            {
                if (string.IsNullOrEmpty(orderDetail.BridgeStatus))
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";

                    newBridgeOrder.OfrIsEnable = true;
                    newBridgeOrder.BidIsEnable = true;
                }
                else
                {
                    if (newBridgeOrder.BridgeStatus.Equals("B"))
                    {
                        newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                        newBridgeOrder.OfrIsEnable = false;

                        newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_standby.png";
                        newBridgeOrder.BidIsEnable = true;
                    }
                    else if (newBridgeOrder.BridgeStatus.Equals("O"))
                    {
                        newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_standby.png";
                        newBridgeOrder.OfrIsEnable = true;

                        newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                        newBridgeOrder.BidIsEnable = false;
                    }
                }

                newBridgeOrder.EditImageSource = GetEditImageSourceEx(orderDetail);

            }
            else if (orderDetail.BridgeType.Equals("B") && orderDetail.BridgeType2.Equals("O"))
            {
                if (orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))//ofr方
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                else if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))//bid方
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";
                }

                newBridgeOrder.OfrIsEnable = false;
                newBridgeOrder.BidIsEnable = false;

                newBridgeOrder.EditImageSource = GetEditImageSourceEx(orderDetail);
            }
            else if (orderDetail.BridgeType.Equals("O") && orderDetail.BridgeType2.Equals("B"))
            {
                if (orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))//ofr方
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_selected.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
                }
                else if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))//bid方
                {
                    newBridgeOrder.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
                    newBridgeOrder.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_selected.png";
                }

                newBridgeOrder.OfrIsEnable = false;
                newBridgeOrder.BidIsEnable = false;

                newBridgeOrder.EditImageSource = GetEditImageSourceEx(orderDetail);

            }
            ///取消的单子不可编辑
            if (newBridgeOrder.OfrBridgeStateShow == "已取消"
                && newBridgeOrder.BidBridgeStateShow == "已取消")
            {
                newBridgeOrder.OfrIsEnable = false;
                newBridgeOrder.BidIsEnable = false;
                newBridgeOrder.EditIsEnable = false;
            }
        }

        public static string GetEditImageSource(OrderDetailPo orderDetail)
        {
            string result = "pack://application:,,,/STC.Frame;component/Images/icon_operation_tag_normal.png";

            UserBridge ofrBridge = UserBridgeHandler.Instance.GetBridgeById(orderDetail.BridgeInstitutionId, orderDetail.BridgeTraderId);
            UserBridge bidBridge = UserBridgeHandler.Instance.GetBridgeById(orderDetail.BridgeInstitutionId2, orderDetail.BridgeTraderId2);
            if (ofrBridge != null && bidBridge != null)
            {
                if (!(string.IsNullOrEmpty(orderDetail.BridgeFee) && string.IsNullOrEmpty(orderDetail.BridgeFee2)
                       && string.IsNullOrEmpty(orderDetail.BridgeRemark) && string.IsNullOrEmpty(orderDetail.BridgeRemark2)
                       && (orderDetail.BridgeChannel + "").Equals(ofrBridge.channel + "") && (orderDetail.BridgeChannel2 + "").Equals(bidBridge.channel + "")))
                {
                    result = "pack://application:,,,/STC.Frame;component/Images/icon_operation_tag_selected.png";
                }
            }
            return result;
        }

        public static string GetEditImageSourceEx(TransOrderServerData orderDetail)
        {
            string result = "pack://application:,,,/STC.Frame;component/Images/icon_operation_tag_normal.png";

            UserBridge ofrBridge = UserBridgeHandler.Instance.GetBridgeById(orderDetail.BridgeInstitutionId, orderDetail.BridgeTraderId);
            UserBridge bidBridge = UserBridgeHandler.Instance.GetBridgeById(orderDetail.BridgeInstitutionId2, orderDetail.BridgeTraderId2);
            if (ofrBridge != null && bidBridge != null)
            {
                if (!(string.IsNullOrEmpty(orderDetail.BridgeFee) && string.IsNullOrEmpty(orderDetail.BridgeFee2)
                       && string.IsNullOrEmpty(orderDetail.BridgeRemark) && string.IsNullOrEmpty(orderDetail.BridgeRemark2)
                       && (orderDetail.BridgeChannel + "").Equals(ofrBridge.channel + "") && (orderDetail.BridgeChannel2 + "").Equals(bidBridge.channel + "")))
                {
                    result = "pack://application:,,,/STC.Frame;component/Images/icon_operation_tag_selected.png";
                }
            }
            return result;
        }

        public BridgeOrderDetail CreateTodayBridgeOrderDetail(string TodayReceipt)
        {
            BridgeOrderDetail tem = new BridgeOrderDetail();
            tem.OrderTxt = TodayReceipt;
            tem.OrderGroup = 2;
            tem.createTime = DateTime.Now.ToString("yyyy-MM-dd");
            tem.OfrImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_ofr_normal.png";
            tem.BidImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_bid_normal.png";
            tem.DeleteImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_delete_normal.png";
            tem.EditImageSource = "pack://application:,,,/STC.Frame;component/Images/icon_operation_tag_normal.png";
            tem.Padding = "Padding-d-f";
            tem.Isliq = "Isliq-d-f";
            tem.Isbrokerunread = "Isbroker-d-f";
            tem.FontFamily = "Family-d-f";
            tem.FontSize = "Size-d-f";
            tem.IsShowColor = "IsShow-d-f";

            return tem;
        }
        /// <summary>
        /// 过滤出集合中的历史单或者今日单
        /// </summary>
        /// <param name="bridgeOrderList"></param>
        /// <param name="isNormal">true为历史单</param>
        /// <returns></returns>
        public static List<BridgeOrderDetail> FilterOrder(List<BridgeOrderDetail> bridgeOrderList, bool isNormal)
        {
            List<BridgeOrderDetail> filterOrder = bridgeOrderList.Where(delegate (BridgeOrderDetail orderDetail)
                {
                    if (orderDetail.OrderGroup != 2)
                        return isNormal ? !OrderHelper.TodayReceipt(orderDetail) : OrderHelper.TodayReceipt(orderDetail);
                    else
                        return false;
                }).ToList();

            return filterOrder;
        }


        /// <summary>
        /// 更新成交单的 桥的渠道 桥的发给 以及成交单发给
        /// </summary>
        /// <param name="bridge"></param>
        /// <param name="orderDetail"></param>
        public static void ResetBridgeInfo(UserBridge bridge, BridgeOrderDetail orderDetail)
        {
            if (orderDetail == null || string.IsNullOrWhiteSpace(orderDetail.ExecID))
            {
                return;
            }
            //UpdateOrderRequestPo result = new UpdateOrderRequestPo();
            //result.id = orderDetail.OrderID;
            //result.execOrders = new List<UpdateExecOrderPo>();
            ExecutionReport execOrderPo = new ExecutionReport();
            //result.execOrders.Add(execOrderPo);
            execOrderPo.ExecID = orderDetail.ExecID;

            execOrderPo.BridgeTraderId = orderDetail.BridgeTraderId;
            execOrderPo.BridgeTraderName = orderDetail.BridgeTraderName;
            execOrderPo.BridgeInstitutionFullName = orderDetail.bridgebankcode;
            execOrderPo.BridgeInstitutionName = orderDetail.BridgeInstitutionName;
            execOrderPo.BridgeInstitutionId = orderDetail.BridgeInstitutionId;
            execOrderPo.BridgeType = orderDetail.BridgeType;
            execOrderPo.BridgeFee = orderDetail.BridgeFee;
            execOrderPo.BridgeRemark = orderDetail.BridgeRemark;

            execOrderPo.BridgeTraderName2 = orderDetail.BridgeTraderName2;
            execOrderPo.BridgeInstitutionFullName2 = orderDetail.bridgebankcode2;
            execOrderPo.BridgeInstitutionName2 = orderDetail.BridgeInstitutionName2;
            execOrderPo.BridgeInstitutionId2 = orderDetail.BridgeInstitutionId2;
            execOrderPo.BridgeType2 = orderDetail.BridgeType2;
            execOrderPo.BridgeFee2 = orderDetail.BridgeFee2;
            execOrderPo.BridgeTraderId2 = orderDetail.BridgeTraderId2;
            execOrderPo.BridgeRemark2 = orderDetail.BridgeRemark2;


            //execOrderPo.bridgefee = execOrderPo.bridgefee2 = string.Empty;
            //execOrderPo.bridgeremark = execOrderPo.bridgeremark2 = string.Empty;

            //orderDetail.bridgefee = orderDetail.bridgefee2 = string.Empty;
            //orderDetail.bridgeremark = orderDetail.bridgeremark2 = string.Empty;

            if (orderDetail.BridgeType.Equals("B,O"))
            {
                //更新单桥

                execOrderPo.BridgeSendTo = execOrderPo.BridgeSendTo2 = bridge.sendto;
                if (OrderHelper.BridgeIsAllConfirmed(orderDetail))
                {
                    execOrderPo.SendTo2 = reSetSentto(orderDetail, bridge, true);
                    execOrderPo.SendTo = reSetSentto(orderDetail, bridge, false);
                }
                execOrderPo.BridgeChannel = execOrderPo.BridgeChannel2 = bridge.channel;
                execOrderPo.BridgeRemark = execOrderPo.BridgeRemark2 = bridge.remark;
                execOrderPo.Contacttype = execOrderPo.Contacttype2 = bridge.contactType;
            }
            else
            {
                //重置ofr桥
                if (orderDetail.BridgeTraderId.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId.Equals(bridge.bankInstitutionid))
                {
                    execOrderPo.BridgeSendTo = bridge.sendto;
                    if (OrderHelper.BridgeIsAllConfirmed(orderDetail))
                        execOrderPo.SendTo2 = reSetSentto(orderDetail, bridge, true);
                    execOrderPo.BridgeChannel = bridge.channel;
                    execOrderPo.BridgeRemark = bridge.remark;
                    execOrderPo.Contacttype = bridge.contactType;
                }
                //重置bid桥
                else if (orderDetail.BridgeTraderId2.Equals(bridge.bankTraderid) && orderDetail.BridgeInstitutionId2.Equals(bridge.bankInstitutionid))
                {
                    execOrderPo.BridgeSendTo2 = bridge.sendto;
                    if (OrderHelper.BridgeIsAllConfirmed(orderDetail))
                        execOrderPo.SendTo = reSetSentto(orderDetail, bridge, false);
                    execOrderPo.BridgeChannel2 = bridge.channel;
                    execOrderPo.BridgeRemark2 = bridge.remark;
                    execOrderPo.Contacttype2 = bridge.contactType;
                }
            }

            //操作类型
            execOrderPo.CommandType = (int)ECommandType.修改桥要素 + "";
            ///borderID传给后台
            execOrderPo.OperateBorderID = BridgeUIDomain.GetBOrderIdByBridge(orderDetail, bridge);

            List<ExecutionReport> list = new List<ExecutionReport>();
            list.Add(execOrderPo);
            OrderHelper.ChangeOrderEx(list);
        }

        /// <summary>
        /// 编辑桥的时候重置单子，保留订单原本的费用信息
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="bridge"></param>
        /// <param name="isGvn"></param>
        /// <returns></returns>
        private static string reSetSentto(BridgeOrderDetail orderDetail, UserBridge bridge, bool isGvn)
        {
            string sendto = string.Empty;
            //当订单的桥状态全部都是“已确认“的状态才会更新主界面发给；
            // 当订单有任意一座桥的桥状态不是“已确认“，则不更新主界面发给。
            if (!OrderHelper.BridgeIsAllConfirmed(orderDetail))
                return sendto;
            if (bridge != null)
            {
                if (!string.IsNullOrEmpty(bridge.channel))
                {
                    //桥的机构
                    string bankInstitutionname = bridge.bankInstitutionname + "";
                    //桥的渠道
                    string channel = bridge.channel + "";
                    //桥的发给
                    string bridgeSendto = bridge.sendto + "";
                    //单子的费用
                    string bridgeFee = isGvn ? orderDetail.BridgeFee : orderDetail.BridgeFee2;
                    ////单子的备注
                    //string bridgeRemark = isGvn ? orderDetail.bridgeremark : orderDetail.bridgeremark2;
                    //桥的备注  根据桥的备注重新设置发给
                    string bridgeRemark = bridge.remark;
                    if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                        sendto = string.Format("{0}联系{1} ", GetSelectChannelTxt(channel), bankInstitutionname);
                    else if (channel.Equals("1"))
                        sendto = string.Format("发{0}", bankInstitutionname);
                    else if (channel.Equals("2"))
                        sendto = string.Format("{0}发{1}", "【请求】", bankInstitutionname);
                    else if (channel.Equals("9") || channel.Equals("10"))
                        sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                    else
                    {
                        sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                    }
                    //如果渠道不是请求，则拼上桥的发给。
                    if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                        sendto += " " + bridgeSendto;

                    //保留费用
                    if (!string.IsNullOrEmpty(bridgeFee))
                        sendto += ", " + bridgeFee;
                    //保留备注
                    if (!string.IsNullOrEmpty(bridgeRemark))
                        sendto += ", " + bridgeRemark;
                }
            }
            return sendto;
        }
        /// <summary>
        /// 换桥的时候重置单子，保留订单原本的费用备注等信息
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="bridge"></param>
        /// <param name="isGvn"></param>
        /// <returns></returns>
        private static string changeSentto(BridgeOrderDetail orderDetail, UserBridge bridge, bool isGvn, int bridgeFlage)
        {
            string sendto = string.Empty;
            //当订单的桥状态全部都是“已确认“的状态才会更新主界面发给；
            // 当订单有任意一座桥的桥状态不是“已确认“，则不更新主界面发给。
            if (!OrderHelper.BridgeIsAllConfirmed(orderDetail))
                return sendto;
            if (bridge != null)
            {
                if (!string.IsNullOrEmpty(bridge.channel))
                {
                    //桥的机构
                    //string bankInstitutionname = bridge.bankInstitutionname + "";
                    string bankInstitutionname = CommonUtils.GetInstName2(bridge.bankInstitutionid, "");// CRMService.Instance.GetInstName2(bridge.bankId, orderDetail.goodstype);
                    //桥的渠道
                    string channel = isGvn ? orderDetail.BridgeChannel : orderDetail.BridgeChannel2;
                    //桥的发给
                    string bridgeSendto = bridge.sendto + "";

                    //桥的发给根据方向确定
                    string tradebankInstitutionname = string.Empty;


                    int dirFlag = 2;

                    if (bridgeFlage == 1)
                    {
                        if (isGvn)
                        {
                            dirFlag = orderDetail.bridgeOrder.ofrDir;
                        }
                        else
                        {
                            dirFlag = orderDetail.bridgeOrder.bidDir;
                        }
                    }
                    else
                    {
                        if (isGvn)
                        {
                            dirFlag = orderDetail.bridgeOrder.ofrDir;
                        }
                        else
                        {
                            dirFlag = orderDetail.bridgeOrder2.bidDir;
                        }
                    }

                    if (dirFlag == 1)
                    {
                        if (isGvn)
                        {
                            if (orderDetail.IsGVN)
                            {
                                tradebankInstitutionname = orderDetail.InstAShow;
                                bridgeSendto = orderDetail.TraderName == null ? "" : orderDetail.TraderName;
                            }
                            else
                            {
                                tradebankInstitutionname = orderDetail.InstBShow;
                                bridgeSendto = orderDetail.CounterPartyTraderName == null ? "" : orderDetail.CounterPartyTraderName;
                            }
                        }
                        else
                        {
                            if (orderDetail.IsGVN)
                            {
                                tradebankInstitutionname = orderDetail.InstBShow;
                                bridgeSendto = orderDetail.CounterPartyTraderName == null ? "" : orderDetail.CounterPartyTraderName;
                            }
                            else
                            {
                                tradebankInstitutionname = orderDetail.InstAShow;
                                bridgeSendto = orderDetail.TraderName == null ? "" : orderDetail.TraderName;
                            }
                        }
                    }
                    else
                    {
                        bridgeSendto = bridge.sendto;
                    }

                    //单子的费用
                    string bridgeFee = isGvn ? orderDetail.BridgeFee : orderDetail.BridgeFee2;
                    //单子的备注
                    string bridgeRemark = isGvn ? orderDetail.BridgeRemark : orderDetail.BridgeRemark2;


                    string sendto1 = "走" + bankInstitutionname;

                    if (dirFlag == 1)
                    {
                        //桥的备注  根据桥的备注重新设置发给
                        if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                            sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(channel), tradebankInstitutionname);
                        else if (channel.Equals("1"))
                            sendto = string.Format("发{0}", tradebankInstitutionname);
                        else if (channel.Equals("2"))
                            sendto = string.Format("{0}发{1}", "【请求】", tradebankInstitutionname);
                        else if (channel.Equals("9") || channel.Equals("10"))
                            sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), tradebankInstitutionname);
                        else
                        {
                            sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), tradebankInstitutionname);
                        }
                        //如果渠道不是请求，则拼上桥的发给。
                        if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                            sendto += " " + bridgeSendto;

                        //保留费用
                        if (!string.IsNullOrEmpty(bridgeFee))
                            sendto += ", " + bridgeFee;
                        //保留备注
                        if (!string.IsNullOrEmpty(bridgeRemark))
                            sendto += ", " + bridgeRemark;

                        sendto = sendto1 + "," + sendto;
                    }
                    else
                    {
                        //桥的备注  根据桥的备注重新设置发给
                        //string bridgeRemark = bridge.remark;
                        if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                            sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                        else if (channel.Equals("1"))
                            sendto = string.Format("发{0}", bankInstitutionname);
                        else if (channel.Equals("2"))
                            sendto = string.Format("{0}发{1}", "【请求】", bankInstitutionname);
                        else if (channel.Equals("9") || channel.Equals("10"))
                            sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                        else
                        {
                            sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                        }
                        //如果渠道不是请求，则拼上桥的发给。
                        if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                            sendto += " " + bridgeSendto;

                        //保留费用
                        if (!string.IsNullOrEmpty(bridgeFee))
                            sendto += ", " + bridgeFee;
                        //保留备注
                        if (!string.IsNullOrEmpty(bridgeRemark))
                            sendto += ", " + bridgeRemark;
                    }

                }
            }
            if (!string.IsNullOrWhiteSpace(sendto))
            {
                sendto = sendto.Trim();
            }
            return sendto;
        }


        private static void GetBridgeSentTo(BridgeOrderDetail orderDetail, UserBridge bridge, bool isGvn, out string sentTo)
        {

            sentTo = string.Empty;
            if (orderDetail.bridgeOrder != null)
            {
                if (orderDetail.bridgeOrder.ofrDir == 1)
                {
                    if (orderDetail.IsGVN)
                    {
                        orderDetail.BridgeSendTo = orderDetail.TraderName == null ? "" : orderDetail.TraderName;
                    }
                    else
                    {
                        orderDetail.BridgeSendTo = orderDetail.CounterPartyTraderName == null ? "" : orderDetail.CounterPartyTraderName;
                    }
                }
                else
                {
                    orderDetail.BridgeSendTo = orderDetail.SendTo;
                }

                //bid
                if (orderDetail.bridgeOrder.bidDir == 1)
                {
                    if (orderDetail.IsGVN)
                    {
                        orderDetail.BridgeSendTo2 = orderDetail.CounterPartyTraderName == null ? "" : orderDetail.CounterPartyTraderName;
                    }
                    else
                    {
                        orderDetail.BridgeSendTo2 = orderDetail.TraderName == null ? "" : orderDetail.TraderName;
                    }
                }
                else
                {
                    orderDetail.BridgeSendTo2 = orderDetail.SendTo;
                }
            }
        }
        static private string GetSelectChannelTxtByNo(string no)
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
        /// 获取单子的默认渠道
        /// 上交所债券（代码以.SH结尾的），默认渠道为固收，深交所债券（代码以.SZ结尾的），默认渠道为大宗。
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="Bridge"></param>
        /// <returns></returns>
        public static string GetDefaultChannel(OrderDetailPo orderDetailPo, UserBridge Bridge)
        {
            string channel = Bridge == null ? string.Empty : Bridge.channel;
            if (orderDetailPo.SecurityID.EndsWith(".SH")|| orderDetailPo.SecurityID.EndsWith(".SZ"))
                channel = "6";
            return channel;
        }

        public static string GetDefaultChannelEx(TransOrderServerData orderDetailPo, UserBridge Bridge)
        {
            string channel = Bridge == null ? string.Empty : Bridge.channel;
            if (orderDetailPo.SecurityID.EndsWith(".SH") || orderDetailPo.SecurityID.EndsWith(".SZ"))
                channel = "6";
            return channel;
        }

        public static string GetDefaultChannel(TransOrderServerData orderDetailPo, UserBridge Bridge)
        {
            string channel = Bridge == null ? string.Empty : Bridge.channel;
            if (orderDetailPo.SecurityID.EndsWith(".SH") || orderDetailPo.SecurityID.EndsWith(".SZ"))
                channel = "6";
            return channel;
        }

        /// <summary>
        /// 换桥
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="Bridge">原来剩下的桥</param>
        /// <param name="changeBridge">换的新桥</param>
        /// <param name="isReserve"></param>
        public static UpdateOrderRequestPo ChangeBridge(BridgeOrderDetail orderDetailPo, UserBridge Bridge, UserBridge changeBridge, bool isReserve = false)
        {
            UserBridge secondBridgeTemp = UserBridge.Copy(changeBridge);
            secondBridgeTemp.channel = GetDefaultChannel(orderDetailPo, changeBridge);
            UpdateOrderRequestPo result = new UpdateOrderRequestPo();
            result.id = orderDetailPo.OrderID;
            result.execOrders = new List<UpdateExecOrderPo>();
            UpdateExecOrderPo execOrderPo = new UpdateExecOrderPo();
            result.execOrders.Add(execOrderPo);
            execOrderPo.execId = orderDetailPo.ExecID;
            //操作类型
            execOrderPo.commandType = isReserve ? (int)ECommandType.换桥预约 : (int)ECommandType.换桥;
            execOrderPo.operateBorderID = orderDetailPo.MasterID;
            if (orderDetailPo.BridgeType == "B,O")
            {
                //桥
                execOrderPo.bridgebankid = execOrderPo.bridgebankid2 = changeBridge.bankInstitutionid;
                execOrderPo.bridgebankcodeshort = execOrderPo.bridgebankcodeshort2 = changeBridge.bankInstitutionname;
                execOrderPo.bridgebankagentid = execOrderPo.bridgebankagentid2 = changeBridge.bankTraderid;
                execOrderPo.bridgebankagentname = execOrderPo.bridgebankagentname2 = changeBridge.bankTradername;
                execOrderPo.contactType = execOrderPo.contactType2 = changeBridge.contactType;
                //计费
                execOrderPo.feeInstId = execOrderPo.feeInstId2 = changeBridge.feeTraderid;
                execOrderPo.feeInstName = execOrderPo.feeInstName2 = changeBridge.feeTradername;
                execOrderPo.feeInstTradeId = execOrderPo.feeInstTradeId2 = changeBridge.feeInstitutionid;
                execOrderPo.feeInstTradeName = execOrderPo.feeInstTradeName2 = changeBridge.feeInstitutionname;
                //桥发给
                execOrderPo.bridgesendto = execOrderPo.bridgesendto2 = changeBridge.sendto;

                //桥发给根据方向确定
                if (orderDetailPo.bridgeOrder != null)
                {
                    if (orderDetailPo.bridgeOrder.ofrDir == 1)
                    {
                        if (orderDetailPo.IsGVN)
                        {
                            execOrderPo.bridgesendto = orderDetailPo.TraderName == null ? "" : orderDetailPo.TraderName;
                        }
                        else
                        {
                            execOrderPo.bridgesendto = orderDetailPo.CounterPartyTraderName == null ? "" : orderDetailPo.CounterPartyTraderName;
                        }
                    }
                    else
                    {
                        execOrderPo.bridgesendto = changeBridge.sendto;
                    }

                    //bid
                    if (orderDetailPo.bridgeOrder.bidDir == 1)
                    {
                        if (orderDetailPo.IsGVN)
                        {
                            execOrderPo.bridgesendto2 = orderDetailPo.CounterPartyTraderName == null ? "" : orderDetailPo.CounterPartyTraderName;
                        }
                        else
                        {
                            execOrderPo.bridgesendto2 = orderDetailPo.TraderName == null ? "" : orderDetailPo.TraderName;
                        }
                    }
                    else
                    {
                        execOrderPo.bridgesendto2 = changeBridge.sendto;
                    }
                }


                //外部发给
                if (OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                {
                    execOrderPo.sendto2 = changeSentto(orderDetailPo, changeBridge, true, 1);
                    execOrderPo.sendto = changeSentto(orderDetailPo, changeBridge, false, 1);
                }
            }
            else
            {
                UserBridge BridgeTemp = UserBridge.Copy(Bridge);
                BridgeTemp.channel = GetDefaultChannel(orderDetailPo, Bridge);
                if (orderDetailPo.isBidBridge.Equals("1"))
                {
                    //bid桥
                    execOrderPo.bridgebankid2 = changeBridge.bankInstitutionid;
                    execOrderPo.bridgebankcodeshort2 = changeBridge.bankInstitutionname;
                    execOrderPo.bridgebankagentid2 = changeBridge.bankTraderid;
                    execOrderPo.bridgebankagentname2 = changeBridge.bankTradername;
                    execOrderPo.contactType2 = changeBridge.contactType;
                    //计费
                    execOrderPo.feeInstId2 = changeBridge.feeTraderid;
                    execOrderPo.feeInstName2 = changeBridge.feeTradername;
                    execOrderPo.feeInstTradeId2 = changeBridge.feeInstitutionid;
                    execOrderPo.feeInstTradeName2 = changeBridge.feeInstitutionname;

                    execOrderPo.bridgesendto2 = changeBridge.sendto;

                    //桥发给根据方向确定
                    if (orderDetailPo.bridgeOrder2 != null)
                    {
                        //bid
                        if (orderDetailPo.bridgeOrder2.bidDir == 1)
                        {
                            if (orderDetailPo.IsGVN)
                            {
                                execOrderPo.bridgesendto2 = orderDetailPo.CounterPartyTraderName == null ? "" : orderDetailPo.CounterPartyTraderName;
                            }
                            else
                            {
                                execOrderPo.bridgesendto2 = orderDetailPo.TraderName == null ? "" : orderDetailPo.TraderName;
                            }
                        }
                        else
                        {
                            execOrderPo.bridgesendto2 = changeBridge.sendto;
                        }
                    }

                    //双桥发给
                    if (orderDetailPo.Doublebridgedir == 1)
                    {
                        execOrderPo.doublebridgesendto = changeBridge.sendto;
                    }
                    else if (orderDetailPo.Doublebridgedir == 2)
                    {
                        execOrderPo.doublebridgesendto = Bridge.sendto;
                    }

                    if (OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                    {
                        execOrderPo.sendto = changeSentto(orderDetailPo, secondBridgeTemp, false, 2);
                        execOrderPo.sendto2 = changeSentto(orderDetailPo, BridgeTemp, true, 2);
                    }
                }
                else
                {

                    //ofr桥
                    execOrderPo.bridgebankid = changeBridge.bankInstitutionid;
                    execOrderPo.bridgebankcodeshort = changeBridge.bankInstitutionname;
                    execOrderPo.bridgebankagentid = changeBridge.bankTraderid;
                    execOrderPo.bridgebankagentname = changeBridge.bankTradername;
                    execOrderPo.contactType = changeBridge.contactType;
                    //计费
                    execOrderPo.feeInstId = changeBridge.feeTraderid;
                    execOrderPo.feeInstName = changeBridge.feeTradername;
                    execOrderPo.feeInstTradeId = changeBridge.feeInstitutionid;
                    execOrderPo.feeInstTradeName = changeBridge.feeInstitutionname;

                    execOrderPo.bridgesendto = changeBridge.sendto;
                    //桥发给根据方向确定
                    if (orderDetailPo.bridgeOrder != null)
                    {
                        if (orderDetailPo.bridgeOrder.ofrDir == 1)
                        {
                            if (orderDetailPo.IsGVN)
                            {
                                execOrderPo.bridgesendto = orderDetailPo.TraderName == null ? "" : orderDetailPo.TraderName;
                            }
                            else
                            {
                                execOrderPo.bridgesendto = orderDetailPo.CounterPartyTraderName == null ? "" : orderDetailPo.CounterPartyTraderName;
                            }
                        }
                        else
                        {
                            execOrderPo.bridgesendto = changeBridge.sendto;
                        }
                    }

                    //双桥发给
                    if (orderDetailPo.Doublebridgedir == 1)
                    {
                        execOrderPo.doublebridgesendto = changeBridge.sendto;
                    }
                    else if (orderDetailPo.Doublebridgedir == 2)
                    {
                        execOrderPo.doublebridgesendto = Bridge.sendto;
                    }

                    if (OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                    {
                        execOrderPo.sendto = changeSentto(orderDetailPo, BridgeTemp, false, 2);
                        execOrderPo.sendto2 = changeSentto(orderDetailPo, secondBridgeTemp, true, 2);
                    }
                }
            }
            return result;
        }

        public static TransOrderServerData ChangeBridgeEx(BridgeOrderDetail orderDetailPo, UserBridge Bridge, UserBridge changeBridge, bool isReserve = false)
        {
            UserBridge secondBridgeTemp = UserBridge.Copy(changeBridge);
            secondBridgeTemp.channel = GetDefaultChannel(orderDetailPo, changeBridge);
            //UpdateOrderRequestPo result = new UpdateOrderRequestPo();
            //result.id = orderDetailPo.OrderID;
            //result.execOrders = new List<UpdateExecOrderPo>();
            //UpdateExecOrderPo execOrderPo = new UpdateExecOrderPo();
            //result.execOrders.Add(execOrderPo);
            TransOrderServerData execOrderPo = new TransOrderServerData();
            execOrderPo.ExecID = orderDetailPo.ExecID;
            //操作类型
            execOrderPo.CommandType = (isReserve ? (int)ECommandType.换桥预约 : (int)ECommandType.换桥) + "";
            execOrderPo.OperateBorderID = orderDetailPo.MasterID;
            if (orderDetailPo.BridgeType == "B,O")
            {
                //桥
                execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId2 = changeBridge.bankInstitutionid;
                execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName2 = changeBridge.bankInstitutionname;
                execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId2 = changeBridge.bankTraderid;
                execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName2 = changeBridge.bankTradername;
                execOrderPo.Contacttype = execOrderPo.Contacttype2 = changeBridge.contactType;
                //计费
                execOrderPo.FeeInstitutionid = execOrderPo.FeeInstitutionid2 = changeBridge.feeTraderid;
                execOrderPo.FeeInstitutionname = execOrderPo.FeeInstitutionname2 = changeBridge.feeTradername;
                execOrderPo.FeeTraderid = execOrderPo.FeeTraderid2 = changeBridge.feeInstitutionid;
                execOrderPo.FeeTradername = execOrderPo.FeeTradername2 = changeBridge.feeInstitutionname;
                //桥发给
                execOrderPo.BridgeSendTo = execOrderPo.BridgeSendTo2 = changeBridge.sendto;

                //桥发给根据方向确定
                if (orderDetailPo.bridgeOrder != null)
                {
                    if (orderDetailPo.bridgeOrder.ofrDir == 1)
                    {
                        if (orderDetailPo.IsGVN)
                        {
                            execOrderPo.BridgeSendTo = orderDetailPo.TraderName == null ? "" : orderDetailPo.TraderName;
                        }
                        else
                        {
                            execOrderPo.BridgeSendTo = orderDetailPo.CounterPartyTraderName == null ? "" : orderDetailPo.CounterPartyTraderName;
                        }
                    }
                    else
                    {
                        execOrderPo.BridgeSendTo = changeBridge.sendto;
                    }

                    //bid
                    if (orderDetailPo.bridgeOrder.bidDir == 1)
                    {
                        if (orderDetailPo.IsGVN)
                        {
                            execOrderPo.BridgeSendTo2 = orderDetailPo.CounterPartyTraderName == null ? "" : orderDetailPo.CounterPartyTraderName;
                        }
                        else
                        {
                            execOrderPo.BridgeSendTo2 = orderDetailPo.TraderName == null ? "" : orderDetailPo.TraderName;
                        }
                    }
                    else
                    {
                        execOrderPo.BridgeSendTo2 = changeBridge.sendto;
                    }
                }


                //外部发给
                if (OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                {
                    execOrderPo.SendTo2 = changeSentto(orderDetailPo, changeBridge, true, 1);
                    execOrderPo.SendTo = changeSentto(orderDetailPo, changeBridge, false, 1);
                }
            }
            else
            {
                UserBridge BridgeTemp = UserBridge.Copy(Bridge);
                BridgeTemp.channel = GetDefaultChannel(orderDetailPo, Bridge);
                if (orderDetailPo.isBidBridge.Equals("1"))
                {
                    //bid桥
                    execOrderPo.BridgeInstitutionId2 = changeBridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName2 = changeBridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId2 = changeBridge.bankTraderid;
                    execOrderPo.BridgeTraderName2 = changeBridge.bankTradername;
                    execOrderPo.Contacttype2 = changeBridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid2 = changeBridge.feeTraderid;
                    execOrderPo.FeeInstitutionname2 = changeBridge.feeTradername;
                    execOrderPo.FeeTraderid2 = changeBridge.feeInstitutionid;
                    execOrderPo.FeeTradername2 = changeBridge.feeInstitutionname;

                    execOrderPo.BridgeSendTo2 = changeBridge.sendto;

                    //桥发给根据方向确定
                    if (orderDetailPo.bridgeOrder2 != null)
                    {
                        //bid
                        if (orderDetailPo.bridgeOrder2.bidDir == 1)
                        {
                            if (orderDetailPo.IsGVN)
                            {
                                execOrderPo.BridgeSendTo2 = orderDetailPo.CounterPartyTraderName == null ? "" : orderDetailPo.CounterPartyTraderName;
                            }
                            else
                            {
                                execOrderPo.BridgeSendTo2 = orderDetailPo.TraderName == null ? "" : orderDetailPo.TraderName;
                            }
                        }
                        else
                        {
                            execOrderPo.BridgeSendTo2 = changeBridge.sendto;
                        }
                    }

                    //双桥发给
                    if (orderDetailPo.Doublebridgedir == 1)
                    {
                        execOrderPo.DoubleBridgeSendTo = changeBridge.sendto;
                    }
                    else if (orderDetailPo.Doublebridgedir == 2)
                    {
                        execOrderPo.DoubleBridgeSendTo = Bridge.sendto;
                    }

                    if (OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                    {
                        execOrderPo.SendTo = changeSentto(orderDetailPo, secondBridgeTemp, false, 2);
                        execOrderPo.SendTo2 = changeSentto(orderDetailPo, BridgeTemp, true, 2);
                    }
                }
                else
                {

                    //ofr桥
                    execOrderPo.BridgeInstitutionId = changeBridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName = changeBridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId = changeBridge.bankTraderid;
                    execOrderPo.BridgeTraderName = changeBridge.bankTradername;
                    execOrderPo.Contacttype = changeBridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid = changeBridge.feeTraderid;
                    execOrderPo.FeeInstitutionname = changeBridge.feeTradername;
                    execOrderPo.FeeTraderid = changeBridge.feeInstitutionid;
                    execOrderPo.FeeTradername = changeBridge.feeInstitutionname;

                    execOrderPo.BridgeSendTo = changeBridge.sendto;
                    //桥发给根据方向确定
                    if (orderDetailPo.bridgeOrder != null)
                    {
                        if (orderDetailPo.bridgeOrder.ofrDir == 1)
                        {
                            if (orderDetailPo.IsGVN)
                            {
                                execOrderPo.BridgeSendTo = orderDetailPo.TraderName == null ? "" : orderDetailPo.TraderName;
                            }
                            else
                            {
                                execOrderPo.BridgeSendTo = orderDetailPo.CounterPartyTraderName == null ? "" : orderDetailPo.CounterPartyTraderName;
                            }
                        }
                        else
                        {
                            execOrderPo.BridgeSendTo = changeBridge.sendto;
                        }
                    }

                    //双桥发给
                    if (orderDetailPo.Doublebridgedir == 1)
                    {
                        execOrderPo.DoubleBridgeSendTo = changeBridge.sendto;
                    }
                    else if (orderDetailPo.Doublebridgedir == 2)
                    {
                        execOrderPo.DoubleBridgeSendTo = Bridge.sendto;
                    }

                    if (OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                    {
                        execOrderPo.SendTo = changeSentto(orderDetailPo, BridgeTemp, false, 2);
                        execOrderPo.SendTo2 = changeSentto(orderDetailPo, secondBridgeTemp, true, 2);
                    }
                }
            }
            return execOrderPo;
        }

        /// <summary>
        /// 1、双桥 在加桥单上再加桥操作
        /// 2、单桥 添加单桥
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="isDouble"></param>
        /// <param name="Bridge"></param>
        /// <param name="secondBridge"></param>
        /// <param name="isOfr"></param>
        public static TransOrderServerData SetBridge2(TransOrderServerData orderDetailPo, bool isDouble, UserBridge Bridge, UserBridge secondBridge, bool isOfr)
        {
            UserBridge BridgeTemp = UserBridge.Copy(Bridge);


            TransOrderServerData execOrderPo = new TransOrderServerData();
            BridgeTemp.channel = GetDefaultChannelEx(orderDetailPo, Bridge);
            execOrderPo.ExecID = orderDetailPo.ExecID;
            //操作类型operateBorderID
            execOrderPo.CommandType = (int)ECommandType.添加桥 + "";
            execOrderPo.BridgeType = execOrderPo.BridgeType2 = "B,O";
            execOrderPo.BridgeFlag = "2";
            execOrderPo.BridgeStatus = string.Empty;
            //桥
            execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId2 = Bridge.bankInstitutionid;
            execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName2 = Bridge.bankInstitutionname;
            execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId2 = Bridge.bankTraderid;
            execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName2 = Bridge.bankTradername;
            execOrderPo.Contacttype = execOrderPo.Contacttype2 = Bridge.contactType;
            //计费
            execOrderPo.FeeInstitutionid = execOrderPo.FeeInstitutionid2 = Bridge.feeTraderid;
            execOrderPo.FeeInstitutionname = execOrderPo.FeeInstitutionname2 = Bridge.feeTradername;
            execOrderPo.FeeTraderid = execOrderPo.FeeTraderid2 = Bridge.feeInstitutionid;
            execOrderPo.FeeTradername = execOrderPo.FeeTradername2 = Bridge.feeInstitutionname;
            //桥发给
            execOrderPo.BridgeSendTo = execOrderPo.BridgeSendTo2 = Bridge.sendto;
            //桥渠道
            execOrderPo.BridgeChannel = execOrderPo.BridgeChannel2 = GetDefaultChannelEx(orderDetailPo, Bridge);
            ////桥费用
            execOrderPo.BridgeFee = orderDetailPo.BridgeFee + "";
            execOrderPo.BridgeFee2 = orderDetailPo.BridgeFee2 + "";
            ////特殊佣金
            execOrderPo.OFRSpecialBridgeFee = orderDetailPo.OFRSpecialBridgeFee + "";
            execOrderPo.BIDSpecialBridgeFee = orderDetailPo.BIDSpecialBridgeFee + "";
            ////桥备注
            if (orderDetailPo.BridgeFlag == "2" && !isDouble)
            {
                execOrderPo.BridgeRemark = orderDetailPo.SendTo2 + "";
                execOrderPo.BridgeRemark2 = orderDetailPo.SendTo + "";
            }

            //重置方向
            execOrderPo.OfrDir = 2 + "";
            execOrderPo.BidDir = 2 + "";
            execOrderPo.BidDir2 = 2 + "";
            execOrderPo.Doublebridgedir = 1 + "";
            //外部发给
            if (!isDouble)
                execOrderPo.SendTo2 = execOrderPo.SendTo = SetSenttoEx(orderDetailPo.SecurityType, BridgeTemp);
            //Ofr
            //费用
            if (!string.IsNullOrEmpty(execOrderPo.SendTo2))
            {
                if (!string.IsNullOrEmpty(execOrderPo.BridgeFee))
                {
                    execOrderPo.SendTo2 += "," + execOrderPo.BridgeFee;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(execOrderPo.BridgeFee))
                {
                    execOrderPo.SendTo2 += execOrderPo.BridgeFee;
                }
            }

            if (!string.IsNullOrEmpty(execOrderPo.SendTo2))
            {
                if (!string.IsNullOrEmpty(execOrderPo.BridgeRemark))
                {
                    execOrderPo.SendTo2 += "," + execOrderPo.BridgeRemark;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(execOrderPo.BridgeRemark))
                {
                    execOrderPo.SendTo2 += execOrderPo.BridgeRemark;
                }
            }

            //bid
            //费用
            if (!string.IsNullOrEmpty(execOrderPo.SendTo))
            {
                if (!string.IsNullOrEmpty(execOrderPo.BridgeFee2))
                {
                    execOrderPo.SendTo += "," + execOrderPo.BridgeFee2;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(execOrderPo.BridgeFee2))
                {
                    execOrderPo.SendTo += execOrderPo.BridgeFee2;
                }
            }

            //备注
            if (!string.IsNullOrEmpty(execOrderPo.SendTo))
            {
                if (!string.IsNullOrEmpty(execOrderPo.BridgeRemark2))
                {
                    execOrderPo.SendTo += "," + execOrderPo.BridgeRemark2;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(execOrderPo.BridgeRemark2))
                {
                    execOrderPo.SendTo += execOrderPo.BridgeRemark2;
                }
            }


            if (isDouble)
            {
                UserBridge secondBridgeTemp = UserBridge.Copy(secondBridge);
                secondBridgeTemp.channel = GetDefaultChannelEx(orderDetailPo, secondBridge);
                if (isOfr)
                {
                    //第二座桥是OFR桥
                    execOrderPo.BridgeType = "B";
                    execOrderPo.BridgeType2 = "O";

                    //bid桥
                    execOrderPo.BridgeInstitutionId2 = Bridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName2 = Bridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId2 = Bridge.bankTraderid;
                    execOrderPo.BridgeTraderName2 = Bridge.bankTradername;
                    execOrderPo.Contacttype2 = Bridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid2 = Bridge.feeTraderid;
                    execOrderPo.FeeInstitutionname2 = Bridge.feeTradername;
                    execOrderPo.FeeTraderid2 = Bridge.feeInstitutionid;
                    execOrderPo.FeeTradername2 = Bridge.feeInstitutionname;

                    execOrderPo.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
                    execOrderPo.BridgeChannel2 = orderDetailPo.BridgeChannel2;

                    //ofr桥
                    execOrderPo.BridgeInstitutionId = secondBridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName = secondBridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId = secondBridge.bankTraderid;
                    execOrderPo.BridgeTraderName = secondBridge.bankTradername;
                    execOrderPo.Contacttype = secondBridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid = secondBridge.feeTraderid;
                    execOrderPo.FeeInstitutionname = secondBridge.feeTradername;
                    execOrderPo.FeeTraderid = secondBridge.feeInstitutionid;
                    execOrderPo.FeeTradername = secondBridge.feeInstitutionname;


                    if (execOrderPo.BridgeOrderModel.ofrDir == 1)
                        execOrderPo.BridgeSendTo = orderDetailPo.BridgeSendTo;
                    else
                        execOrderPo.BridgeSendTo = secondBridge.sendto;
                    execOrderPo.BridgeChannel = orderDetailPo.BridgeChannel;

                    execOrderPo.BridgeRemark = orderDetailPo.BridgeRemark;
                    execOrderPo.BridgeRemark2 = orderDetailPo.BridgeRemark2;
                    execOrderPo.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
                    execOrderPo.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;

                    execOrderPo.OfrDir = orderDetailPo.BridgeOrderModel.ofrDir + "";
                    execOrderPo.BidDir = orderDetailPo.BridgeOrderModel.bidDir + "";
                    execOrderPo.BidDir2 = orderDetailPo.BridgeOrderModel.bidDir + "";

                    if (OrderHelper.BridgeIsAllConfirmedEx(execOrderPo))
                    {
                        BridgeTemp.channel = execOrderPo.BridgeChannel2;
                        bool isSameSendtoWithDefault = execOrderPo.BridgeSendTo2 == BridgeTemp.sendto;
                        bool isSameSendtoWithTrader = execOrderPo.BridgeSendTo2 == orderDetailPo.BidbankAgentName;
                        BridgeTemp.sendto = execOrderPo.BridgeSendTo2;
                        execOrderPo.SendTo = SetSenttoNew(orderDetailPo, BridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, orderDetailPo.BridgeOrderModel.bidDir, orderDetailPo.BidInst);

                        //备注
                        string bridgeremark = execOrderPo.BridgeRemark2;
                        //if (orderDetailPo.bridgeOrder != null && !string.IsNullOrEmpty(orderDetailPo.bridgeOrder.bidOriginalBridgeRemark))
                        //{
                        //bridgeremark = orderDetailPo.bridgeOrder.bidOriginalBridgeRemark;
                        //}

                        if (!string.IsNullOrEmpty(execOrderPo.SendTo))
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                            {
                                execOrderPo.SendTo += ", " + orderDetailPo.BridgeFee2;
                            }

                            if (!string.IsNullOrEmpty(bridgeremark))
                            {
                                execOrderPo.SendTo += ", " + bridgeremark;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                            {
                                execOrderPo.SendTo += orderDetailPo.BridgeFee2;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark))
                            {
                                execOrderPo.SendTo += bridgeremark;
                            }
                        }
                        secondBridgeTemp.channel = orderDetailPo.BridgeChannel;
                        isSameSendtoWithDefault = execOrderPo.BridgeSendTo == secondBridgeTemp.sendto;
                        isSameSendtoWithTrader = execOrderPo.BridgeSendTo == orderDetailPo.OfrbankAgentName;
                        secondBridgeTemp.sendto = execOrderPo.BridgeSendTo;
                        execOrderPo.SendTo2 = SetSenttoNew(orderDetailPo, secondBridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, orderDetailPo.BridgeOrderModel.ofrDir, orderDetailPo.OfrInst);

                        string bridgeremark2 = execOrderPo.BridgeRemark;
                        //if (orderDetailPo.bridgeOrder != null && !string.IsNullOrEmpty(orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark))
                        //{
                        //    bridgeremark2 = orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark;
                        //}

                        if (!string.IsNullOrEmpty(execOrderPo.SendTo2))
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                            {
                                execOrderPo.SendTo2 += ", " + orderDetailPo.BridgeFee;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark2))
                            {
                                execOrderPo.SendTo2 += ", " + bridgeremark2;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                            {
                                execOrderPo.SendTo2 += orderDetailPo.BridgeFee;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark2))
                            {
                                execOrderPo.SendTo2 += bridgeremark2;
                            }
                        }
                    }
                }
                else
                {
                    //第二座桥是Bid桥
                    execOrderPo.BridgeType = "O";
                    execOrderPo.BridgeType2 = "B";

                    //ofr桥
                    execOrderPo.BridgeInstitutionId = Bridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName = Bridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId = Bridge.bankTraderid;
                    execOrderPo.BridgeTraderName = Bridge.bankTradername;
                    execOrderPo.Contacttype = Bridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid = Bridge.feeTraderid;
                    execOrderPo.FeeInstitutionname = Bridge.feeTradername;
                    execOrderPo.FeeTraderid = Bridge.feeInstitutionid;
                    execOrderPo.FeeTradername = Bridge.feeInstitutionname;

                    execOrderPo.BridgeSendTo = orderDetailPo.BridgeSendTo;
                    execOrderPo.BridgeChannel = orderDetailPo.BridgeChannel;

                    //bid桥
                    execOrderPo.BridgeInstitutionId2 = secondBridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName2 = secondBridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId2 = secondBridge.bankTraderid;
                    execOrderPo.BridgeTraderName2 = secondBridge.bankTradername;
                    execOrderPo.Contacttype2 = secondBridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid2 = secondBridge.feeTraderid;
                    execOrderPo.FeeInstitutionname2 = secondBridge.feeTradername;
                    execOrderPo.FeeTraderid2 = secondBridge.feeInstitutionid;
                    execOrderPo.FeeTradername2 = secondBridge.feeInstitutionname;

                    if (orderDetailPo.BridgeOrderModel.bidDir == 1)
                        execOrderPo.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
                    else
                        execOrderPo.BridgeSendTo2 = secondBridge.sendto;
                    execOrderPo.BridgeChannel2 = orderDetailPo.BridgeChannel2;

                    execOrderPo.BridgeRemark = orderDetailPo.BridgeRemark;
                    execOrderPo.BridgeRemark2 = orderDetailPo.BridgeRemark2;
                    execOrderPo.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
                    execOrderPo.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;

                    execOrderPo.OfrDir = orderDetailPo.BridgeOrderModel.ofrDir + "";
                    execOrderPo.BidDir = orderDetailPo.BridgeOrderModel.bidDir + "";
                    execOrderPo.BidDir2 = orderDetailPo.BridgeOrderModel.bidDir + "";

                    if (OrderHelper.BridgeIsAllConfirmedEx(execOrderPo))
                    {
                        secondBridgeTemp.channel = execOrderPo.BridgeChannel2;
                        bool isSameSendtoWithDefault = execOrderPo.BridgeSendTo2 == secondBridgeTemp.sendto;
                        bool isSameSendtoWithTrader = execOrderPo.BridgeSendTo2 == orderDetailPo.BidbankAgentName;
                        secondBridgeTemp.sendto = execOrderPo.BridgeSendTo2;
                        execOrderPo.SendTo = SetSenttoNew(orderDetailPo, secondBridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, orderDetailPo.BridgeOrderModel.bidDir, orderDetailPo.BidInst);

                        string bridgeremark = execOrderPo.BridgeRemark2;
                        //if (orderDetailPo.bridgeOrder != null && !string.IsNullOrEmpty(orderDetailPo.bridgeOrder.bidOriginalBridgeRemark))
                        //{
                        //    bridgeremark = orderDetailPo.bridgeOrder.bidOriginalBridgeRemark;
                        //}

                        //备注
                        if (!string.IsNullOrEmpty(execOrderPo.SendTo))
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                            {
                                execOrderPo.SendTo += ", " + orderDetailPo.BridgeFee2;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark))
                            {
                                execOrderPo.SendTo += ", " + bridgeremark;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                            {
                                execOrderPo.SendTo += orderDetailPo.BridgeFee2;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark))
                            {
                                execOrderPo.SendTo += bridgeremark;
                            }
                        }

                        BridgeTemp.channel = orderDetailPo.BridgeChannel;
                        isSameSendtoWithDefault = execOrderPo.BridgeSendTo == BridgeTemp.sendto;
                        isSameSendtoWithTrader = execOrderPo.BridgeSendTo == orderDetailPo.OfrbankAgentName;
                        BridgeTemp.sendto = execOrderPo.BridgeSendTo;
                        execOrderPo.SendTo2 = SetSenttoNew(orderDetailPo, BridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, orderDetailPo.BridgeOrderModel.ofrDir, orderDetailPo.OfrInst);
                        //重新生成发给

                        string bridgeremark2 = execOrderPo.BridgeRemark;
                        //if (orderDetailPo.bridgeOrder != null && !string.IsNullOrEmpty(orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark))
                        //{
                        //    bridgeremark2 = orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark;
                        //}


                        if (!string.IsNullOrEmpty(execOrderPo.SendTo2))
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                            {
                                execOrderPo.SendTo2 += ", " + orderDetailPo.BridgeFee;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark2))
                            {
                                execOrderPo.SendTo2 += ", " + bridgeremark2;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                            {
                                execOrderPo.SendTo2 += orderDetailPo.BridgeFee;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark2))
                            {
                                execOrderPo.SendTo2 += bridgeremark2;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(orderDetailPo.DoubleBridgeBidSettltype) && string.IsNullOrEmpty(orderDetailPo.DoubleBridgeOffSettltype)
                    && string.IsNullOrEmpty(orderDetailPo.DoubleBridgeChannel))
                {
                    execOrderPo.DoubleBridgeChannel = GetDefaultChannelEx(orderDetailPo, Bridge);
                    SetDefaultSettle(orderDetailPo, execOrderPo);
                }
            }
            return execOrderPo;
        }



        public static List<ExecutionReport> SetBridge3(OrderDetailPo orderDetailPo, bool isDouble, UserBridge Bridge, UserBridge secondBridge, bool isOfr)
        {
            UserBridge BridgeTemp = UserBridge.Copy(Bridge);
            BridgeTemp.channel = GetDefaultChannel(orderDetailPo, Bridge);

            //UpdateOrderRequestPo result = new UpdateOrderRequestPo();
            //result.id = orderDetailPo.orderId;
            //result.execOrders = new List<UpdateExecOrderPo>();
            TransOrderServerData execOrderPo = new TransOrderServerData();
            //result.execOrders.Add(execOrderPo);
            execOrderPo.ExecID = orderDetailPo.ExecID;
            //操作类型operateBorderIDs
            execOrderPo.CommandType = (int)ECommandType.添加桥 + "";
            execOrderPo.BridgeType = execOrderPo.BridgeType2 = "B,O";
            execOrderPo.BridgeFlag = "2";
            execOrderPo.BridgeStatus = string.Empty;
            //桥
            execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId2 = Bridge.bankInstitutionid;
            execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName2 = Bridge.bankInstitutionname;
            execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId2 = Bridge.bankTraderid;
            execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName2 = Bridge.bankTradername;
            execOrderPo.Contacttype = execOrderPo.Contacttype2 = Bridge.contactType;
            //计费
            execOrderPo.FeeInstitutionid = execOrderPo.FeeInstitutionid2 = Bridge.feeTraderid;
            execOrderPo.FeeInstitutionname = execOrderPo.FeeInstitutionname2 = Bridge.feeTradername;
            execOrderPo.FeeTraderid = execOrderPo.FeeTraderid2 = Bridge.feeInstitutionid;
            execOrderPo.FeeTradername = execOrderPo.FeeTradername2 = Bridge.feeInstitutionname;
            //桥发给
            execOrderPo.BridgeSendTo = execOrderPo.BridgeSendTo2 = Bridge.sendto;
            //桥渠道
            execOrderPo.BridgeChannel = execOrderPo.BridgeChannel2 = GetDefaultChannel(orderDetailPo, Bridge);
            //桥费用
            execOrderPo.BridgeFee = orderDetailPo.BridgeFee + "";
            execOrderPo.BridgeFee2 = orderDetailPo.BridgeFee2 + "";
            //特殊佣金
            execOrderPo.OFRSpecialBridgeFee = orderDetailPo.ofrSpecialBridgeFee + "";
            execOrderPo.BIDSpecialBridgeFee = orderDetailPo.bidSpecialBridgeFee + "";
            //桥备注
            if (orderDetailPo.BridgeFlag == "2" && !isDouble)
            {
                execOrderPo.BridgeRemark = orderDetailPo.SendTo2 + "";
                execOrderPo.BridgeRemark2 = orderDetailPo.SendTo + "";
            }

            //重置方向
            execOrderPo.OfrDir = "2";
            execOrderPo.BidDir = "2";
            execOrderPo.BidDir2 = "2";
            execOrderPo.Doublebridgedir = 1 + "";
            //外部发给
            if (!isDouble)
                execOrderPo.SendTo2 = execOrderPo.SendTo = SetSenttoEx(orderDetailPo.goodstype, BridgeTemp);
            //Ofr
            //费用
            if (!string.IsNullOrEmpty(execOrderPo.SendTo2))
            {
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                {
                    execOrderPo.SendTo2 += "," + orderDetailPo.BridgeFee;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                {
                    execOrderPo.SendTo2 += orderDetailPo.BridgeFee;
                }
            }

            if (!string.IsNullOrEmpty(execOrderPo.SendTo2))
            {
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeRemark))
                {
                    execOrderPo.SendTo2 += "," + orderDetailPo.BridgeRemark;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeRemark))
                {
                    execOrderPo.SendTo2 += orderDetailPo.BridgeRemark;
                }
            }

            //bid
            //费用
            if (!string.IsNullOrEmpty(execOrderPo.SendTo))
            {
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                {
                    execOrderPo.SendTo += "," + orderDetailPo.BridgeFee2;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                {
                    execOrderPo.SendTo += orderDetailPo.BridgeFee2;
                }
            }

            //备注
            if (!string.IsNullOrEmpty(execOrderPo.SendTo))
            {
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeRemark2))
                {
                    execOrderPo.SendTo += "," + orderDetailPo.BridgeRemark2;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeRemark2))
                {
                    execOrderPo.SendTo += orderDetailPo.BridgeRemark2;
                }
            }


            if (isDouble)
            {
                UserBridge secondBridgeTemp = UserBridge.Copy(secondBridge);
                secondBridgeTemp.channel = GetDefaultChannel(orderDetailPo, secondBridge);
                if (isOfr)
                {
                    //第二座桥是OFR桥
                    execOrderPo.BridgeType = "B";
                    execOrderPo.BridgeType2 = "O";

                    //bid桥
                    execOrderPo.BridgeInstitutionId2 = Bridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName2 = Bridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId2 = Bridge.bankTraderid;
                    execOrderPo.BridgeTraderName2 = Bridge.bankTradername;
                    execOrderPo.Contacttype2 = Bridge.contactType;

                    //计费
                    execOrderPo.FeeInstitutionid2 = Bridge.feeTraderid;
                    execOrderPo.FeeInstitutionname2 = Bridge.feeTradername;
                    execOrderPo.FeeTraderid2 = Bridge.feeInstitutionid;
                    execOrderPo.FeeTradername2 = Bridge.feeInstitutionname;

                    execOrderPo.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
                    execOrderPo.BridgeChannel2 = orderDetailPo.BridgeChannel2;

                    //ofr桥
                    execOrderPo.BridgeInstitutionId = secondBridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName = secondBridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId = secondBridge.bankTraderid;
                    execOrderPo.BridgeTraderName = secondBridge.bankTradername;
                    execOrderPo.Contacttype2 = secondBridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid = secondBridge.feeTraderid;
                    execOrderPo.FeeInstitutionname = secondBridge.feeTradername;
                    execOrderPo.FeeTraderid = secondBridge.feeInstitutionid;
                    execOrderPo.FeeTradername = secondBridge.feeInstitutionname;

                    if (orderDetailPo.bridgeOrder.ofrDir == 1)
                        execOrderPo.BridgeSendTo = orderDetailPo.BridgeSendTo;
                    else
                        execOrderPo.BridgeSendTo = secondBridge.sendto;
                    execOrderPo.BridgeChannel = orderDetailPo.BridgeChannel;

                    execOrderPo.BridgeRemark = orderDetailPo.BridgeRemark;
                    execOrderPo.BridgeRemark2 = orderDetailPo.BridgeRemark2;
                    execOrderPo.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
                    execOrderPo.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;

                    execOrderPo.OfrDir = orderDetailPo.bridgeOrder.ofrDir + "";
                    execOrderPo.BidDir = orderDetailPo.bridgeOrder.bidDir + "";
                    execOrderPo.BidDir2 = orderDetailPo.bridgeOrder.bidDir + "";

                    if (OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                    {
                        BridgeTemp.channel = orderDetailPo.BridgeChannel2;
                        bool isSameSendtoWithDefault = execOrderPo.BridgeSendTo2 == BridgeTemp.sendto;
                        bool isSameSendtoWithTrader = execOrderPo.BridgeSendTo2 == orderDetailPo.BidbankAgentName;
                        BridgeTemp.sendto = execOrderPo.BridgeSendTo2;
                        execOrderPo.SendTo = SetSenttoNew(orderDetailPo, BridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, orderDetailPo.bridgeOrder.bidDir, orderDetailPo.BidInst);

                        //备注
                        string bridgeremark = execOrderPo.BridgeRemark2;
                        //if (orderDetailPo.bridgeOrder != null && !string.IsNullOrEmpty(orderDetailPo.bridgeOrder.bidOriginalBridgeRemark))
                        //{
                        //bridgeremark = orderDetailPo.bridgeOrder.bidOriginalBridgeRemark;
                        //}

                        if (!string.IsNullOrEmpty(execOrderPo.SendTo))
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                            {
                                execOrderPo.SendTo += ", " + orderDetailPo.BridgeFee2;
                            }

                            if (!string.IsNullOrEmpty(bridgeremark))
                            {
                                execOrderPo.SendTo += ", " + bridgeremark;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                            {
                                execOrderPo.SendTo += orderDetailPo.BridgeFee2;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark))
                            {
                                execOrderPo.SendTo += bridgeremark;
                            }
                        }
                        secondBridgeTemp.channel = orderDetailPo.BridgeChannel;
                        isSameSendtoWithDefault = execOrderPo.BridgeSendTo == secondBridgeTemp.sendto;
                        isSameSendtoWithTrader = execOrderPo.BridgeSendTo == orderDetailPo.OfrbankAgentName;
                        secondBridgeTemp.sendto = execOrderPo.BridgeSendTo;
                        execOrderPo.SendTo2 = SetSenttoNew(orderDetailPo, secondBridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, orderDetailPo.bridgeOrder.ofrDir, orderDetailPo.OfrInst);

                        string bridgeremark2 = execOrderPo.BridgeRemark;
                        //if (orderDetailPo.bridgeOrder != null && !string.IsNullOrEmpty(orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark))
                        //{
                        //    bridgeremark2 = orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark;
                        //}

                        if (!string.IsNullOrEmpty(execOrderPo.SendTo2))
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                            {
                                execOrderPo.SendTo2 += ", " + orderDetailPo.BridgeFee;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark2))
                            {
                                execOrderPo.SendTo2 += ", " + bridgeremark2;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                            {
                                execOrderPo.SendTo2 += orderDetailPo.BridgeFee;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark2))
                            {
                                execOrderPo.SendTo2 += bridgeremark2;
                            }
                        }
                    }
                }
                else
                {
                    //第二座桥是Bid桥
                    execOrderPo.BridgeType = "O";
                    execOrderPo.BridgeType2 = "B";

                    //ofr桥
                    execOrderPo.BridgeInstitutionId = Bridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName = Bridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId = Bridge.bankTraderid;
                    execOrderPo.BridgeTraderName = Bridge.bankTradername;
                    execOrderPo.Contacttype = Bridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid =  Bridge.feeTraderid;
                    execOrderPo.FeeInstitutionname =  Bridge.feeTradername;
                    execOrderPo.FeeTraderid =  Bridge.feeInstitutionid;
                    execOrderPo.FeeTradername =  Bridge.feeInstitutionname;

                    execOrderPo.BridgeSendTo = orderDetailPo.BridgeSendTo;
                    execOrderPo.BridgeChannel = orderDetailPo.BridgeChannel;

                    //bid桥
                    execOrderPo.BridgeInstitutionId2 = secondBridge.bankInstitutionid;
                    execOrderPo.BridgeInstitutionName2 = secondBridge.bankInstitutionname;
                    execOrderPo.BridgeTraderId2 = secondBridge.bankTraderid;
                    execOrderPo.BridgeTraderName2 = secondBridge.bankTradername;
                    execOrderPo.Contacttype2 = secondBridge.contactType;
                    //计费
                    execOrderPo.FeeInstitutionid2 = secondBridge.feeTraderid;
                    execOrderPo.FeeInstitutionname2 = secondBridge.feeTradername;
                    execOrderPo.FeeTraderid2 = secondBridge.feeInstitutionid;
                    execOrderPo.FeeTradername2 = secondBridge.feeInstitutionname;

                    if (orderDetailPo.bridgeOrder.bidDir == 1)
                        execOrderPo.BridgeSendTo2 = orderDetailPo.BridgeSendTo2;
                    else
                        execOrderPo.BridgeSendTo2 = secondBridge.sendto;
                    execOrderPo.BridgeChannel2 = orderDetailPo.BridgeChannel2;

                    execOrderPo.BridgeRemark = orderDetailPo.BridgeRemark;
                    execOrderPo.BridgeRemark2 = orderDetailPo.BridgeRemark2;
                    execOrderPo.BridgeRemarkShow = orderDetailPo.BridgeRemarkShow;
                    execOrderPo.BridgeRemarkShow2 = orderDetailPo.BridgeRemarkShow2;

                    execOrderPo.OfrDir = orderDetailPo.bridgeOrder.ofrDir + "" ;
                    execOrderPo.BidDir = orderDetailPo.bridgeOrder.bidDir + "";
                    execOrderPo.BidDir2 = orderDetailPo.bridgeOrder.bidDir + "";

                    if (OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                    {
                        secondBridgeTemp.channel = orderDetailPo.BridgeChannel2;
                        bool isSameSendtoWithDefault = execOrderPo.BridgeSendTo2 == secondBridgeTemp.sendto;
                        bool isSameSendtoWithTrader = execOrderPo.BridgeSendTo2 == orderDetailPo.BidbankAgentName;
                        secondBridgeTemp.sendto = execOrderPo.BridgeSendTo2;
                        execOrderPo.SendTo = SetSenttoNew(orderDetailPo, secondBridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, orderDetailPo.bridgeOrder.bidDir, orderDetailPo.BidInst);

                        string bridgeremark = execOrderPo.BridgeRemark2;
                        //if (orderDetailPo.bridgeOrder != null && !string.IsNullOrEmpty(orderDetailPo.bridgeOrder.bidOriginalBridgeRemark))
                        //{
                        //    bridgeremark = orderDetailPo.bridgeOrder.bidOriginalBridgeRemark;
                        //}

                        //备注
                        if (!string.IsNullOrEmpty(execOrderPo.SendTo))
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                            {
                                execOrderPo.SendTo += ", " + orderDetailPo.BridgeFee2;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark))
                            {
                                execOrderPo.SendTo += ", " + bridgeremark;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee2))
                            {
                                execOrderPo.SendTo += orderDetailPo.BridgeFee2;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark))
                            {
                                execOrderPo.SendTo += bridgeremark;
                            }
                        }

                        BridgeTemp.channel = orderDetailPo.BridgeChannel;
                        isSameSendtoWithDefault = execOrderPo.BridgeSendTo == BridgeTemp.sendto;
                        isSameSendtoWithTrader = execOrderPo.BridgeSendTo == orderDetailPo.OfrbankAgentName;
                        BridgeTemp.sendto = execOrderPo.BridgeSendTo;
                        execOrderPo.SendTo2 = SetSenttoNew(orderDetailPo, BridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, orderDetailPo.bridgeOrder.ofrDir, orderDetailPo.OfrInst);
                        //重新生成发给

                        string bridgeremark2 = execOrderPo.BridgeRemark;
                        //if (orderDetailPo.bridgeOrder != null && !string.IsNullOrEmpty(orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark))
                        //{
                        //    bridgeremark2 = orderDetailPo.bridgeOrder.ofrOriginalBridgeRemark;
                        //}


                        if (!string.IsNullOrEmpty(execOrderPo.SendTo2))
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                            {
                                execOrderPo.SendTo2 += ", " + orderDetailPo.BridgeFee;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark2))
                            {
                                execOrderPo.SendTo2 += ", " + bridgeremark2;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(orderDetailPo.BridgeFee))
                            {
                                execOrderPo.SendTo2 += orderDetailPo.BridgeFee;
                            }
                            if (!string.IsNullOrEmpty(bridgeremark2))
                            {
                                execOrderPo.SendTo2 += bridgeremark2;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(execOrderPo.DoubleBridgeBidSettltype) && string.IsNullOrEmpty(execOrderPo.DoubleBridgeOffSettltype)
                   && string.IsNullOrEmpty(execOrderPo.DoubleBridgeChannel))
                {
                    execOrderPo.DoubleBridgeChannel = GetDefaultChannel(orderDetailPo, Bridge);
                    //SetDefaultSettleEx(execOrderPo);
                    SetDefaultSettle(orderDetailPo, execOrderPo);
                }
            }

            List<ExecutionReport> list = new List<ExecutionReport>();
            list.Add(execOrderPo);
            return list;
        }

        public static void SetBridgeWithChangeOrder(OrderDetailPo orderDetailPo, bool isDouble, UserBridge Bridge, UserBridge secondBridge, bool isOfr)
        {
            OrderHelper.ChangeOrderEx(SetBridge3(orderDetailPo, isDouble, Bridge, secondBridge, isOfr));
        }

        /// <summary>
        /// 清空发给
        /// </summary>
        /// <param name="orderDetailPo"></param>
        public static void clearSendTo(BridgeOrderDetail orderDetailPo)
        {
            UpdateOrderRequestPo result = new UpdateOrderRequestPo();
            result.id = orderDetailPo.OrderID;
            result.execOrders = new List<UpdateExecOrderPo>();
            UpdateExecOrderPo execOrderPo = new UpdateExecOrderPo();
            result.execOrders.Add(execOrderPo);
            execOrderPo.execId = orderDetailPo.ExecID;

            execOrderPo.sendto2 = execOrderPo.sendto = string.Empty;
            execOrderPo.commandType = (int)ECommandType.修改ofr和bid发给;
            OrderHelper.ChangeOrder(result);
        }
        /// <summary>
        /// 重置发给编辑桥重置单子
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="Bridge"></param>
        public static void reSetSendTo(BridgeOrderDetail orderDetailPo, UserBridge Bridge)
        {
            if (!OrderHelper.BridgeIsAllConfirmed(orderDetailPo))
                return;
            UserBridge BridgeTemp = UserBridge.Copy(Bridge);
            BridgeTemp.channel = GetDefaultChannel(orderDetailPo, Bridge);

            UpdateOrderRequestPo result = new UpdateOrderRequestPo();
            result.id = orderDetailPo.OrderID;
            result.execOrders = new List<UpdateExecOrderPo>();
            UpdateExecOrderPo execOrderPo = new UpdateExecOrderPo();
            result.execOrders.Add(execOrderPo);
            execOrderPo.execId = orderDetailPo.ExecID;

            if (orderDetailPo.BridgeType.Equals("B,O"))
            {
                execOrderPo.sendto2 = execOrderPo.sendto = SetSentto(orderDetailPo, BridgeTemp);
            }
            else
            {
                if (orderDetailPo.isBidBridge == "2")
                {
                    UserBridge bridgeTwo = UserBridgeHandler.Instance.GetBridgeById(orderDetailPo.BridgeInstitutionId2, orderDetailPo.BridgeTraderId2);
                    UserBridge secondBridgeTemp = null;
                    if (bridgeTwo != null)
                    {
                        secondBridgeTemp = UserBridge.Copy(bridgeTwo);
                        secondBridgeTemp.channel = GetDefaultChannel(orderDetailPo, bridgeTwo);
                    }
                    execOrderPo.sendto = SetSentto(orderDetailPo, BridgeTemp);
                    execOrderPo.sendto2 = SetSentto(orderDetailPo, secondBridgeTemp);
                }
                else
                {
                    UserBridge bridgeTwo = UserBridgeHandler.Instance.GetBridgeById(orderDetailPo.BridgeInstitutionId, orderDetailPo.BridgeTraderId);
                    UserBridge secondBridgeTemp = null;
                    if (bridgeTwo != null)
                    {
                        secondBridgeTemp = UserBridge.Copy(bridgeTwo);
                        secondBridgeTemp.channel = GetDefaultChannel(orderDetailPo, bridgeTwo);
                    }
                    execOrderPo.sendto = SetSentto(orderDetailPo, secondBridgeTemp);
                    execOrderPo.sendto2 = SetSentto(orderDetailPo, BridgeTemp);
                }
            }
            execOrderPo.commandType = (int)ECommandType.修改ofr和bid发给;
            OrderHelper.ChangeOrder(result);
        }
        /// <summary>
        /// 预订桥
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <param name="isDouble"></param>
        /// <param name="Bridge"></param>
        /// <param name="secondBridge"></param>
        /// <param name="isOfr"></param>
        public static void ReserveBridge(OrderDetailPo orderDetailPo, bool isDouble, UserBridge Bridge, UserBridge secondBridge, bool isOfr)
        {
            UserBridge BridgeTemp = UserBridge.Copy(Bridge);
            BridgeTemp.channel = GetDefaultChannel(orderDetailPo, Bridge);

            UpdateOrderRequestPo result = new UpdateOrderRequestPo();
            result.id = orderDetailPo.OrderID;
            result.execOrders = new List<UpdateExecOrderPo>();
            UpdateExecOrderPo execOrderPo = new UpdateExecOrderPo();
            result.execOrders.Add(execOrderPo);
            execOrderPo.execId = orderDetailPo.ExecID;
            //操作类型
            execOrderPo.commandType = (int)ECommandType.预约桥;
            execOrderPo.bridgeType = execOrderPo.bridgeType2 = "B,O";
            execOrderPo.bridgeflag = "2";
            execOrderPo.bridgeStatus = string.Empty;
            //桥
            execOrderPo.bridgebankid = execOrderPo.bridgebankid2 = Bridge.bankInstitutionid;
            execOrderPo.bridgebankcodeshort = execOrderPo.bridgebankcodeshort2 = Bridge.bankInstitutionname;
            execOrderPo.bridgebankagentid = execOrderPo.bridgebankagentid2 = Bridge.bankTraderid;
            execOrderPo.bridgebankagentname = execOrderPo.bridgebankagentname2 = Bridge.bankTradername;
            execOrderPo.contactType = execOrderPo.contactType2 = Bridge.contactType;
            //计费
            execOrderPo.feeInstId = execOrderPo.feeInstId2 = Bridge.feeTraderid;
            execOrderPo.feeInstName = execOrderPo.feeInstName2 = Bridge.feeTradername;
            execOrderPo.feeInstTradeId = execOrderPo.feeInstTradeId2 = Bridge.feeInstitutionid;
            execOrderPo.feeInstTradeName = execOrderPo.feeInstTradeName2 = Bridge.feeInstitutionname;

            //发给，渠道，费用，备注
            execOrderPo.bridgesendto = execOrderPo.bridgesendto2 = Bridge.sendto;
            execOrderPo.bridgechannel = execOrderPo.bridgechannel2 = GetDefaultChannel(orderDetailPo, Bridge);

            execOrderPo.bridgefee = orderDetailPo.BridgeFee + "";
            execOrderPo.bridgefee2 = orderDetailPo.BridgeFee2 + "";

            execOrderPo.bridgeremark = orderDetailPo.BridgeRemark + "";
            execOrderPo.bridgeremark2 = orderDetailPo.BridgeRemark2 + "";

            if (isDouble)
            {
                UserBridge secondBridgeTemp = UserBridge.Copy(secondBridge);
                secondBridgeTemp.channel = GetDefaultChannel(orderDetailPo, secondBridge);
                if (isOfr)
                {
                    execOrderPo.bridgeType = "B";
                    execOrderPo.bridgeType2 = "O";
                    //桥
                    execOrderPo.bridgebankid2 = Bridge.bankInstitutionid;
                    execOrderPo.bridgebankcodeshort2 = Bridge.bankInstitutionname;
                    execOrderPo.bridgebankagentid2 = Bridge.bankTraderid;
                    execOrderPo.bridgebankagentname2 = Bridge.bankTradername;
                    execOrderPo.contactType2 = Bridge.contactType;
                    //计费
                    execOrderPo.feeInstId2 = Bridge.feeTraderid;
                    execOrderPo.feeInstName2 = Bridge.feeTradername;
                    execOrderPo.feeInstTradeId2 = Bridge.feeInstitutionid;
                    execOrderPo.feeInstTradeName2 = Bridge.feeInstitutionname;

                    execOrderPo.bridgesendto2 = Bridge.sendto;
                    execOrderPo.bridgechannel2 = GetDefaultChannel(orderDetailPo, Bridge);
                    //桥
                    execOrderPo.bridgebankid = secondBridge.bankInstitutionid;
                    execOrderPo.bridgebankcodeshort = secondBridge.bankInstitutionname;
                    execOrderPo.bridgebankagentid = secondBridge.bankTraderid;
                    execOrderPo.bridgebankagentname = secondBridge.bankTradername;
                    execOrderPo.contactType = Bridge.contactType;
                    //计费
                    execOrderPo.feeInstId = secondBridge.feeTraderid;
                    execOrderPo.feeInstName = secondBridge.feeTradername;
                    execOrderPo.feeInstTradeId = secondBridge.feeInstitutionid;
                    execOrderPo.feeInstTradeName = secondBridge.feeInstitutionname;

                    execOrderPo.bridgesendto = secondBridge.sendto;
                    execOrderPo.bridgechannel = GetDefaultChannel(orderDetailPo, secondBridge);

                }
                else
                {
                    execOrderPo.bridgeType = "O";
                    execOrderPo.bridgeType2 = "B";
                    //桥
                    execOrderPo.bridgebankid = Bridge.bankInstitutionid;
                    execOrderPo.bridgebankcodeshort = Bridge.bankInstitutionname;
                    execOrderPo.bridgebankagentid = Bridge.bankTraderid;
                    execOrderPo.bridgebankagentname = Bridge.bankTradername;
                    execOrderPo.contactType = Bridge.contactType;
                    //计费
                    execOrderPo.feeInstId = Bridge.feeTraderid;
                    execOrderPo.feeInstName = Bridge.feeTradername;
                    execOrderPo.feeInstTradeId = Bridge.feeInstitutionid;
                    execOrderPo.feeInstTradeName = Bridge.feeInstitutionname;

                    execOrderPo.bridgesendto = Bridge.sendto;
                    execOrderPo.bridgechannel = GetDefaultChannel(orderDetailPo, Bridge);
                    //桥
                    execOrderPo.bridgebankid2 = secondBridge.bankInstitutionid;
                    execOrderPo.bridgebankcodeshort2 = secondBridge.bankInstitutionname;
                    execOrderPo.bridgebankagentid2 = secondBridge.bankTraderid;
                    execOrderPo.bridgebankagentname2 = secondBridge.bankTradername;
                    execOrderPo.contactType2 = Bridge.contactType;
                    //计费
                    execOrderPo.feeInstId2 = secondBridge.feeTraderid;
                    execOrderPo.feeInstName2 = secondBridge.feeTradername;
                    execOrderPo.feeInstTradeId2 = secondBridge.feeInstitutionid;
                    execOrderPo.feeInstTradeName2 = secondBridge.feeInstitutionname;

                    execOrderPo.bridgesendto2 = secondBridge.sendto;
                    execOrderPo.bridgechannel2 = GetDefaultChannel(orderDetailPo, secondBridge);
                }

                execOrderPo.bridgefee = execOrderPo.bridgefee2 = string.Empty;
                execOrderPo.bridgeremark = execOrderPo.bridgeremark2 = string.Empty;

                if (string.IsNullOrEmpty(orderDetailPo.DoubleBridgeBidSettltype) && string.IsNullOrEmpty(orderDetailPo.DoubleBridgeOffSettltype)
                    && string.IsNullOrEmpty(orderDetailPo.DoubleBridgeChannel))
                {
                    execOrderPo.doublebridgechannel = GetDefaultChannel(orderDetailPo, Bridge);
                    SetDefaultSettle(orderDetailPo, execOrderPo);
                }
            }

            OrderHelper.ChangeOrder(result);
        }
        /// <summary>
        /// 删除桥。
        /// </summary>
        /// <param name="deleteOrderDetail">主订单或者桥订单，主订单删双桥，桥订单删单桥</param>
        /// <param name="isFromDetail">是否从成交明细删除</param>
        /// <param name="isBid"></param>
        /// <param name="defaultBridge">默认的桥。获取其渠道和发给</param>
        public static void DeleteBridge(OrderDetailPo deleteOrderDetail, bool isFromDetail, bool isBid, UserBridge defaultBridge)
        {
            //UpdateOrderRequestPo request = new UpdateOrderRequestPo();
            //request.id = deleteOrderDetail.OrderID;
            //request.execOrders = new List<UpdateExecOrderPo>();
            TransOrderServerData execOrderPo = new TransOrderServerData();
            //request.execOrders.Add(execOrderPo);
            execOrderPo.ExecID = deleteOrderDetail.ExecID;
            //操作类型
            execOrderPo.CommandType = (int)ECommandType.删除桥 + "";
            //borderID传给后台 
            //传到后台，则只删MasterID对应的桥，不传，则两座桥都删掉
            //if (deleteOrderDetail is BridgeOrderDetail)
            //    execOrderPo.operateBorderID = ((BridgeOrderDetail)deleteOrderDetail)?.MasterID;
            execOrderPo.OperateBorderID = (deleteOrderDetail as BridgeOrderDetail)?.MasterID;
            execOrderPo.DoubleBridgeSettleSide = string.Empty;
            //删除双桥
            if (string.IsNullOrWhiteSpace(deleteOrderDetail.BridgeType) || deleteOrderDetail.BridgeType.Equals("B,O") || isFromDetail)
            {
                //桥
                execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId2 = string.Empty;
                execOrderPo.BridgeInstitutionFullName = execOrderPo.BridgeInstitutionFullName2 = string.Empty;
                execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName2 = string.Empty;
                execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId2 = string.Empty;
                execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName2 = string.Empty;
                //计费
                execOrderPo.FeeInstitutionid = execOrderPo.FeeInstitutionid2 = string.Empty;
                execOrderPo.FeeInstitutionname = execOrderPo.FeeInstitutionname2 = string.Empty;
                execOrderPo.FeeTraderid = execOrderPo.FeeTraderid2 = string.Empty;
                execOrderPo.FeeTradername = execOrderPo.FeeTradername2 = string.Empty;
                //桥 费用 渠道 备注 发给          
                execOrderPo.BridgeFee = execOrderPo.BridgeFee2 = string.Empty;
                execOrderPo.BridgeChannel = execOrderPo.BridgeChannel2 = string.Empty;
                execOrderPo.BridgeRemark = execOrderPo.BridgeRemark2 = string.Empty;
                execOrderPo.BridgeRemarkShow = execOrderPo.BridgeRemarkShow2 = "1";
                execOrderPo.BridgeSendTo = execOrderPo.BridgeSendTo2 = string.Empty;
                execOrderPo.Contacttype = execOrderPo.Contacttype2 = string.Empty;
                //主发给
                execOrderPo.SendTo = execOrderPo.SendTo2 = string.Empty;
                //桥类型
                execOrderPo.BridgeType = execOrderPo.BridgeType2 = string.Empty;

                //删除交易要素备注
                execOrderPo.CombinedOrderRemark = string.Empty;
                execOrderPo.CombinedOrderRemark2 = string.Empty;
                //删除双桥信息
                execOrderPo.DoubleBridgeBidSettltype = execOrderPo.DoubleBridgeOffSettltype = string.Empty;
                execOrderPo.DoubleBridgeChannel = execOrderPo.DoubleBridgeFee = execOrderPo.DoubleBridgeRemark = execOrderPo.DoubleBridgeSendTo = string.Empty;
                if (isFromDetail)
                    execOrderPo.BridgeFlag = "1";
            }
            else
            {
                //桥备注 ，费用 ，渠道
                //execOrderPo.bridgeremark = execOrderPo.bridgeremark2 = string.Empty;
                //execOrderPo.bridgeremarkshow = execOrderPo.bridgeremarkshow2 = "1";
                //execOrderPo.bridgefee = execOrderPo.bridgefee2 = string.Empty;
                //execOrderPo.bridgechannel = execOrderPo.bridgechannel2 = GetDefaultChannel(deleteOrderDetail, defaultBridge);
                //删除交易要素备注
                //execOrderPo.combinedOrderRemark = string.Empty;
                //execOrderPo.combinedOrderRemark2 = string.Empty;
                //桥发给
                //execOrderPo.bridgesendto = execOrderPo.bridgesendto2 = defaultBridge == null ? string.Empty : defaultBridge.sendto;
                ///联系方式
                //execOrderPo.contactType = execOrderPo.contactType = defaultBridge == null ? string.Empty : defaultBridge.contactType;

                if (isBid)
                {
                    execOrderPo.BIDSpecialBridgeFee = deleteOrderDetail.bridgeOrder2.BidSpecialBridgeFee;
                    execOrderPo.OFRSpecialBridgeFee = deleteOrderDetail.bridgeOrder.OfrSpecialBridgeFee;
                    execOrderPo.BridgeSendTo2 = deleteOrderDetail.bridgeOrder2.bidDir == 1 ? deleteOrderDetail.BidbankAgentName : defaultBridge.sendto;
                    execOrderPo.BridgeSendTo = deleteOrderDetail.BridgeSendTo;
                    ///删除bid桥，则保留ofr桥信息
                    execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId2 = deleteOrderDetail.BridgeInstitutionId;
                    execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName2 = deleteOrderDetail.BridgeInstitutionName;
                    execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId2 = deleteOrderDetail.BridgeTraderId;
                    execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName2 = deleteOrderDetail.BridgeTraderName;
                    execOrderPo.BridgeInstitutionFullName = execOrderPo.BridgeInstitutionFullName2 = deleteOrderDetail.bridgebankcode;
                    //计费
                    execOrderPo.FeeInstitutionid = execOrderPo.FeeInstitutionid2 = deleteOrderDetail.bridgeOrder?.feeInstId;
                    execOrderPo.FeeInstitutionname = execOrderPo.FeeInstitutionname2 = deleteOrderDetail.bridgeOrder?.feeInstName;
                    execOrderPo.FeeTraderid = execOrderPo.FeeTraderid2 = deleteOrderDetail.bridgeOrder?.feeInstTradeId;
                    execOrderPo.FeeTradername = execOrderPo.FeeTradername2 = deleteOrderDetail.bridgeOrder?.feeInstTradeName;
                }
                else
                {
                    execOrderPo.BIDSpecialBridgeFee = deleteOrderDetail.bridgeOrder.BidSpecialBridgeFee;
                    execOrderPo.OFRSpecialBridgeFee = deleteOrderDetail.bridgeOrder2.OfrSpecialBridgeFee;
                    execOrderPo.BridgeSendTo = deleteOrderDetail.bridgeOrder.ofrDir == 1 ? deleteOrderDetail.OfrbankAgentName : defaultBridge.sendto;
                    execOrderPo.BridgeSendTo2 = deleteOrderDetail.BridgeSendTo2;
                    ///删除ofr桥，则保留bid桥信息
                    execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId2 = deleteOrderDetail.BridgeInstitutionId2;
                    execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName2 = deleteOrderDetail.BridgeInstitutionName2;
                    execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId2 = deleteOrderDetail.BridgeTraderId2;
                    execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName2 = deleteOrderDetail.BridgeTraderName2;
                    execOrderPo.BridgeInstitutionFullName = execOrderPo.BridgeInstitutionFullName2 = deleteOrderDetail.bridgebankcode2;

                    //计费
                    execOrderPo.FeeInstitutionid = execOrderPo.FeeInstitutionid2 = deleteOrderDetail.bridgeOrder2?.feeInstId;
                    execOrderPo.FeeInstitutionname = execOrderPo.FeeInstitutionname2 = deleteOrderDetail.bridgeOrder2?.feeInstName;
                    execOrderPo.FeeTraderid = execOrderPo.FeeTraderid2 = deleteOrderDetail.bridgeOrder2?.feeInstTradeId;
                    execOrderPo.FeeTradername = execOrderPo.FeeTradername2 = deleteOrderDetail.bridgeOrder2?.feeInstTradeName;
                }

                //双桥结算和渠道
                execOrderPo.DoubleBridgeBidSettltype = execOrderPo.DoubleBridgeOffSettltype = string.Empty;
                execOrderPo.DoubleBridgeChannel = execOrderPo.DoubleBridgeFee = execOrderPo.DoubleBridgeRemark = execOrderPo.DoubleBridgeSendTo = string.Empty;
                //添加双桥渠道默认显示 20200318 
                //2个桥删掉一个再加桥的时候，双桥渠道不为空，会影响双桥的默认结算，结果为空 20200429
                //execOrderPo.doublebridgechannel = GetDefaultChannel(deleteOrderDetail, defaultBridge);
                //双桥删桥操作，设置成交单子的默认发给 20200320

                //重置主发给
                UserBridge BridgeTemp = defaultBridge == null ? new UserBridge() : UserBridge.Copy(defaultBridge);
                BridgeTemp.channel = GetDefaultChannel(deleteOrderDetail, defaultBridge);
                if (OrderHelper.BridgeIsAllConfirmed(deleteOrderDetail))
                {
                    string BridgeTempSendto = BridgeTemp.sendto;
                    BridgeTemp.channel = deleteOrderDetail.BridgeChannel;
                    bool isSameSendtoWithDefault = execOrderPo.BridgeSendTo == BridgeTempSendto;
                    bool isSameSendtoWithTrader = execOrderPo.BridgeSendTo == deleteOrderDetail.OfrbankAgentName;
                    BridgeTemp.sendto = execOrderPo.BridgeSendTo;
                    execOrderPo.SendTo2 = SetSenttoNew(deleteOrderDetail, BridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, deleteOrderDetail.bridgeOrder.ofrDir, deleteOrderDetail.OfrInst);
                    if (!string.IsNullOrWhiteSpace(deleteOrderDetail.BridgeFee))
                    {
                        execOrderPo.SendTo2 += ", " + deleteOrderDetail.BridgeFee;
                    }
                    if (!string.IsNullOrWhiteSpace(deleteOrderDetail.BridgeRemark))
                    {
                        execOrderPo.SendTo2 += ", " + deleteOrderDetail.BridgeRemark;
                    }

                    BridgeTemp.channel = deleteOrderDetail.BridgeChannel2;
                    isSameSendtoWithDefault = execOrderPo.BridgeSendTo2 == BridgeTempSendto;
                    isSameSendtoWithTrader = execOrderPo.BridgeSendTo2 == deleteOrderDetail.BidbankAgentName;
                    BridgeTemp.sendto = execOrderPo.BridgeSendTo2;
                    execOrderPo.SendTo = SetSenttoNew(deleteOrderDetail, BridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, deleteOrderDetail.bridgeOrder2.bidDir, deleteOrderDetail.BidInst);
                    if (!string.IsNullOrWhiteSpace(deleteOrderDetail.BridgeFee2))
                    {
                        execOrderPo.SendTo += ", " + deleteOrderDetail.BridgeFee2;
                    }
                    if (!string.IsNullOrWhiteSpace(deleteOrderDetail.BridgeRemark2))
                    {
                        execOrderPo.SendTo += ", " + deleteOrderDetail.BridgeRemark2;
                    }
                }

                execOrderPo.BridgeType2 = execOrderPo.BridgeType = "B,O";

                //重置方向
                //execOrderPo.ofrDir = 2;
                //execOrderPo.bidDir = 2;
            }

            List<ExecutionReport> list = new List<ExecutionReport>();
            list.Add(execOrderPo);
            OrderHelper.ChangeOrderEx(list);
        }
        /// <summary>
        /// 设置双桥的默认结算方式
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="execOrderPo"></param>
        private static void SetDefaultSettle(OrderDetailPo orderDetail, UpdateExecOrderPo execOrderPo)
        {
            string defaultOfrSettle = string.Empty;
            string defaultBidSettle = string.Empty;
            if (orderDetail.SettlType == orderDetail.SettlType2)
            {
                defaultBidSettle = defaultOfrSettle = orderDetail.SettlType;
            }
            else
            {
                ///双桥错期默认选择ofr方
                defaultOfrSettle = orderDetail.SettlType2 + "/" + orderDetail.SettlType;
                defaultBidSettle = orderDetail.SettlType;
            }

            execOrderPo.doublebridgebidsettlementtype = defaultBidSettle;
            execOrderPo.doublebridgeoffersettlementtype = defaultOfrSettle;
        }

        private static void SetDefaultSettle(OrderDetailPo orderDetail, TransOrderServerData execOrderPo)
        {
            string defaultOfrSettle = string.Empty;
            string defaultBidSettle = string.Empty;
            if (orderDetail.SettlType == orderDetail.SettlType2)
            {
                defaultBidSettle = defaultOfrSettle = GetSettle(orderDetail.SettlType);
            }
            else
            {
                ///双桥错期默认选择ofr方
                defaultOfrSettle = GetSettle(orderDetail.SettlType2) + "/" + GetSettle(orderDetail.SettlType);
                defaultBidSettle = GetSettle(orderDetail.SettlType);
            }

            execOrderPo.DoubleBridgeBidSettltype = defaultBidSettle;
            execOrderPo.DoubleBridgeOffSettltype = defaultOfrSettle;
        }


        private static string GetSettle(string settleStr)
        {
            string settleStrOfr = settleStr;
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
            return settleStrOfr;
        }


        private static void SetDefaultSettleEx(TransOrderServerData execOrderPo)
        {
            string defaultOfrSettle = string.Empty;
            string defaultBidSettle = string.Empty;
            if (execOrderPo.SettlType == execOrderPo.SettlType2)
            {
                defaultBidSettle = defaultOfrSettle = execOrderPo.SettlType;
            }
            else
            {
                ///双桥错期默认选择ofr方
                defaultOfrSettle = execOrderPo.SettlType2 + "/" + execOrderPo.SettlType;
                defaultBidSettle = execOrderPo.SettlType;
            }

            execOrderPo.DoubleBridgeBidSettltype = defaultBidSettle;
            execOrderPo.DoubleBridgeOffSettltype = defaultOfrSettle;
        }
        private static void SetDefaultSettle(TransOrderServerData orderDetail, TransOrderServerData execOrderPo)
        {
            string defaultOfrSettle = string.Empty;
            string defaultBidSettle = string.Empty;
            if (orderDetail.SettlType == orderDetail.SettlType2)
            {
                defaultBidSettle = defaultOfrSettle = orderDetail.SettlType;
            }
            else
            {
                ///双桥错期默认选择ofr方
                defaultOfrSettle = orderDetail.SettlType2 + "/" + orderDetail.SettlType;
                defaultBidSettle = orderDetail.SettlType;
            }

            execOrderPo.DoubleBridgeBidSettltype = defaultBidSettle;
            execOrderPo.DoubleBridgeOffSettltype = defaultOfrSettle;
        }

        private static string GetSelectChannelTxt(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return string.Empty;
            ChannelEnum channelEumu;
            var isSuccess = Enum.TryParse(channel, out channelEumu);
            return isSuccess ? channelEumu.GetDescription() : string.Empty;

            //byte b = (byte)channel[0];//Jenkins上编译报错 vs2015编译此处也会报错 20200426 wym  要验证
            //EChannel enumChannel;
            //if (Enum.TryParse(b.ToString(), out enumChannel))
            //    return enumChannel.ToString();
            //if (channel.Equals("1"))
            //    return "对话";
            //else if (channel.Equals("2"))
            //    return "请求";
            //else if (channel.Equals("3"))
            //    return "双边";
            //else if (channel.Equals("4"))
            //    return "xbond";
            //else if (channel.Equals("5"))
            //    return "iDeal";
            //else if (channel.Equals("6"))
            //    return "固收";
            //else if (channel.Equals("7"))
            //    return "竞价";
            //else if (channel.Equals("8"))
            //    return "大宗";

            return string.Empty;
        }

        /// <summary>
        /// 加桥的时候用，初始化外部发给
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="bridge"></param>
        /// <returns></returns>
        public static string SetSentto(OrderDetailPo orderDetail, UserBridge userBridge)
        {
            string sendto = string.Empty;
            if (userBridge != null)
            {
                if (!string.IsNullOrEmpty(userBridge.channel))
                {

                    //桥的机构
                    //string bankInstitutionname = userBridge.bankInstitutionname + "";
                    string bankInstitutionname = CommonUtils.GetInstName2(userBridge.bankInstitutionid, "");//CRMService.Instance.GetInstName2(userBridge.bankId, orderDetail.goodstype);
                    //桥的渠道
                    string channel = userBridge.channel + "";
                    //桥的发给
                    string bridgeSendto = userBridge.sendto + "";

                    if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                        sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                    else if (channel.Equals("1"))
                        sendto = string.Format("发{0}", bankInstitutionname);
                    else if (channel.Equals("2"))
                        sendto = string.Format("{0}发{1}", "【请求】", bankInstitutionname);
                    else if (channel.Equals("9") || channel.Equals("10"))
                        sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                    else
                    {
                        sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                    }
                    if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                        sendto += " " + bridgeSendto;
                }
            }
            return sendto;
        }

        public static string SetSenttoEx(string goodstype, UserBridge userBridge)
        {
            string sendto = string.Empty;
            if (userBridge != null)
            {
                if (!string.IsNullOrEmpty(userBridge.channel))
                {

                    //桥的机构
                    //string bankInstitutionname = userBridge.bankInstitutionname + "";
                    string bankInstitutionname = CommonUtils.GetInstName2(userBridge.bankInstitutionid, goodstype);//CRMService.Instance.GetInstName2(userBridge.bankId, orderDetail.goodstype);
                    //桥的渠道
                    string channel = userBridge.channel + "";
                    //桥的发给
                    string bridgeSendto = userBridge.sendto + "";

                    if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                        sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                    else if (channel.Equals("1"))
                        sendto = string.Format("发{0}", bankInstitutionname);
                    else if (channel.Equals("2"))
                        sendto = string.Format("{0}发{1}", "【请求】", bankInstitutionname);
                    else if (channel.Equals("9") || channel.Equals("10"))
                        sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                    else
                    {
                        sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                    }
                    if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                        sendto += " " + bridgeSendto;
                }
            }
            return sendto;
        }

        /// <summary>
        /// 设置发给（包含此逻辑：填的发给和桥的默认发给是否一致）
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="userBridge"></param>
        /// <param name="isSameSendtoWithDefault">填的发给和桥的默认发给是否一致</param>
        /// <param name="isSameSendtoWithTrader">填的发给和交易员是否一致</param>
        /// <param name="bridgeFlag">桥方向 1：向下 其他：向上</param>
        /// <param name="traderInstName">交易员所在的机构</param>
        /// <returns></returns>
        public static string SetSenttoNew(OrderDetailPo orderDetail, UserBridge userBridge, bool isSameSendtoWithDefault, bool isSameSendtoWithTrader, int bridgeFlag, string traderInstName)
        {
            string sendto = string.Empty;
            if (userBridge == null) return sendto;
            if (string.IsNullOrWhiteSpace(userBridge.channel)) return sendto;

            //桥的机构
            string bridgeInstName = CommonUtils.GetInstName2(userBridge.bankInstitutionid, orderDetail.goodstype);
            //CRMService.Instance.GetInstName2(userBridge.bankId, orderDetail.goodstype);
            //发给框里的内容始终与桥默认的发给进行比较是否相同
            //相同：
            if (isSameSendtoWithDefault)
            {
                string bankName = bridgeInstName;
                //桥的渠道
                string channel = userBridge.channel + "";
                //桥的发给
                string bridgeSendto = userBridge.sendto + "";

                if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                    sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(channel), bankName);
                else if (channel.Equals("1"))
                    sendto = string.Format("发{0}", bankName);
                else if (channel.Equals("2"))
                    sendto = string.Format("{0}发{1}", "【请求】", bankName);
                else if (channel.Equals("9") || channel.Equals("10"))
                    sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), bankName);
                else
                {
                    sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), bankName);
                }
                if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                    sendto += " " + bridgeSendto;

            }
            else if (isSameSendtoWithTrader && bridgeFlag == 1) //发给框里的内容再与交易员比较是否相同，若相同 且 桥方向指向交易员（箭头向下）则：
            {
                string bankName = traderInstName;
                //桥的渠道
                string channel = userBridge.channel + "";
                //桥的发给
                string bridgeSendto = userBridge.sendto + "";

                if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                    sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(channel), bankName);
                else if (channel.Equals("1"))
                    sendto = string.Format("发{0}", bankName);
                else if (channel.Equals("2"))
                    sendto = string.Format("{0}发{1}", "【请求】", bankName);
                else if (channel.Equals("9") || channel.Equals("10"))
                    sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), bankName);
                else
                {
                    sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), bankName);
                }
                if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                    sendto += " " + bridgeSendto;
                sendto = $"走{bridgeInstName},{sendto}";
            }
            else //其他所有情况
            {
                sendto = $"走{bridgeInstName}";
                if (!string.IsNullOrWhiteSpace(userBridge.sendto))
                {
                    sendto += ", " + userBridge.sendto;
                }
            }
            return sendto;
        }

        public static string SetSenttoNew(TransOrderServerData orderDetail, UserBridge userBridge,bool isSameSendtoWithDefault, bool isSameSendtoWithTrader, int bridgeFlag,string traderInstName)
        {
            string sendto = string.Empty;
            if (userBridge == null) return sendto;
            if (string.IsNullOrWhiteSpace(userBridge.channel)) return sendto;

            //桥的机构
            string bridgeInstName = CommonUtils.GetInstName2(userBridge.bankInstitutionid, orderDetail.SecurityType);//CRMService.Instance.GetInstName2(userBridge.bankId, orderDetail.goodstype);
            //发给框里的内容始终与桥默认的发给进行比较是否相同
            //相同：
            if (isSameSendtoWithDefault)
            {
                string bankInstitutionname = bridgeInstName;
                //桥的渠道
                string channel = userBridge.channel + "";
                //桥的发给
                string bridgeSendto = userBridge.sendto + "";

                if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                    sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                else if (channel.Equals("1"))
                    sendto = string.Format("发{0}", bankInstitutionname);
                else if (channel.Equals("2"))
                    sendto = string.Format("{0}发{1}", "【请求】", bankInstitutionname);
                else if (channel.Equals("9") || channel.Equals("10"))
                    sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                else
                {
                    sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                }
                if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                    sendto += " " + bridgeSendto;

            }
            else if (isSameSendtoWithTrader && bridgeFlag == 1) //发给框里的内容再与交易员比较是否相同，若相同 且 桥方向指向交易员（箭头向下）则：
            {
                string bankInstitutionname = traderInstName;
                //桥的渠道
                string channel = userBridge.channel + "";
                //桥的发给
                string bridgeSendto = userBridge.sendto + "";

                if (channel.Equals("6") || channel.Equals("7") || channel.Equals("8"))
                    sendto = string.Format("{0}联系{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                else if (channel.Equals("1"))
                    sendto = string.Format("发{0}", bankInstitutionname);
                else if (channel.Equals("2"))
                    sendto = string.Format("{0}发{1}", "【请求】", bankInstitutionname);
                else if (channel.Equals("9") || channel.Equals("10"))
                    sendto = string.Format("点{0},{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                else
                {
                    sendto = string.Format("{0}发{1}", GetSelectChannelTxt(channel), bankInstitutionname);
                }
                if (!channel.Equals("2") && !string.IsNullOrEmpty(bridgeSendto))
                    sendto += " " + bridgeSendto;
                sendto = $"走{bridgeInstName},{sendto}";
            }
            else //其他所有情况
            {
                sendto = $"走{bridgeInstName}";
                if (!string.IsNullOrWhiteSpace(userBridge.sendto))
                {
                    sendto += ", " + userBridge.sendto;
                }
            }
            return sendto;
        }


        public static void UpdateBridgeStatus(BridgeOrderDetail bod, ECommandType commandType, string status, UserBridge bridge)
        {

            //UpdateOrderRequestPo request = new UpdateOrderRequestPo();
            //request.id = bod.OrderID;
            //request.execOrders = new List<UpdateExecOrderPo>();
            TransOrderServerData execOrderPo = new TransOrderServerData();
            //request.execOrders.Add(execOrderPo);
            execOrderPo.ExecID = bod.ExecID;

            execOrderPo.BridgeStatus = status;

            execOrderPo.BridgeFee = bod.BridgeFee;
            execOrderPo.BridgeChannel = bod.BridgeChannel;
            execOrderPo.BridgeRemark = bod.BridgeRemark;
            execOrderPo.BridgeSendTo = bod.BridgeSendTo;
            execOrderPo.BridgeInstitutionId = bod.BridgeInstitutionId;
            execOrderPo.BridgeInstitutionName = bod.BridgeInstitutionName;
            execOrderPo.BridgeTraderId = bod.BridgeTraderId;
            execOrderPo.BridgeTraderName = bod.BridgeTraderName;
            execOrderPo.BridgeInstitutionFullName = bod.bridgebankcode;
            execOrderPo.BridgeType = "B,O";

            execOrderPo.BridgeType2 = "B,O";
            execOrderPo.BridgeInstitutionId2 = bod.BridgeInstitutionId;
            execOrderPo.BridgeInstitutionName2 = bod.BridgeInstitutionName;
            execOrderPo.BridgeTraderId2 = bod.BridgeTraderId;
            execOrderPo.BridgeTraderName2 = bod.BridgeTraderName;
            execOrderPo.BridgeInstitutionFullName2 = bod.bridgebankcode;
            ///操作类型
            execOrderPo.CommandType = (int)commandType + "";
            execOrderPo.BridgeFlag = "2";
            ///borderID传给后台
            execOrderPo.OperateBorderID = GetBOrderIdByBridge(bod, bridge);
            List<ExecutionReport> list = new List<ExecutionReport>();
            list.Add(execOrderPo);
            OrderHelper.ChangeOrderEx(list);
        }

        public static OrderDetailPo ConvertOrderDetail(TransOrderServerData o)
        {
            OrderDetailPo po = new OrderDetailPo();
            po.seq = decimal.Parse(o.Sequence);
            po.ExecID = o.ExecID;
            po.OrderID = o.OrderID;
            po.dealdate = o.TradeTime;
            po.delflag = o.DelFlag;
            po.OrdStatus = o.OrdStatus;
            po.Side = o.Side;
            po.SecurityID = o.SecurityID;
            po.goodsShortName = o.Symbol;
            po.goodstype = o.SecurityType;
            po.BondSubtype = o.BondSubtype;
            decimal price;
            if (decimal.TryParse(o.Yield, out price))
                po.price = price;
            decimal qty;
            if (decimal.TryParse(o.OrderQty, out qty))
                po.qty = qty;
            //po.ListedMarket = o.m
            po.SettlDate = o.SettlDate;
            po.SettlDate2 = o.SettlDate2;
            po.SettlType = o.SettlType;
            po.SettlType2 = o.SettlType2;
            po.remarks = o.Remark;

            po.InstitutionName = o.InstitutionName;
            po.TraderName = o.TraderName;
            po.PartySubID = o.PartySubID;
            po.PartySubName = o.PartySubName;

            po.CounterPartyInstitutionName = o.CounterPartyInstitutionName;//o.Counterpartyname;
            po.CounterPartyTraderName = o.CounterPartyTraderName;//o.Counterpartysubname;
            po.CounterPartySubName = o.CounterPartySubName;
            po.CounterPartySubID = o.CounterPartySubID;
            po.dealdate = o.TransactTime;
            po.DealDate2 = o.TransactTime;

            po.SendTo = o.SendTo;
            po.SendTo2 = o.SendTo2;
            po.DoubleBridgeBidSettltype = o.DoubleBridgeBidSettltype;
            po.DoubleBridgeOffSettltype = o.BridgeRemarkShow2;
            po.DoubleBridgeChannel = o.BridgeRemarkShow2;

            po.DoubleBridgeChannel = o.BridgeRemarkShow2;
            po.DoubleBridgeChannel = o.BridgeRemarkShow2;

            po.BorderId = o.BorderId;
            po.BorderId2 = o.BorderId2;
            po.BridgeCloseById = o.BridgeCloseById;
            po.BridgeCreateTime = o.BridgeCreateTime;
            po.BridgeCreateTime2 = o.BridgeCreateTime2;
            po.BridgeFee = o.BridgeFee;
            po.BridgeFee2 = o.BridgeFee2;
            po.BridgeFlag = o.BridgeFlag;
            po.BridgeInstitutionId = o.BridgeInstitutionId;
            po.BridgeInstitutionId2 = o.BridgeInstitutionId2;
            po.BridgeInstitutionName = o.BridgeInstitutionName;
            po.BridgeInstitutionName2 = o.BridgeInstitutionName2;
            po.BridgeRemark = o.BridgeRemark;
            po.BridgeRemark2 = o.BridgeRemark2;
            po.BridgeRemarkShow = o.BridgeRemarkShow;
            po.BridgeRemarkShow2 = o.BridgeRemarkShow2;
            po.BridgeSendTo = o.BridgeSendTo;
            po.BridgeSendTo2 = o.BridgeSendTo2;
            po.BridgeStatus = o.BridgeStatus;

            po.BridgeTraderId = o.BridgeTraderId;
            po.BridgeTraderId2 = o.BridgeTraderId2;
            po.BridgeTraderName = o.BridgeTraderName;
            po.BridgeTraderName2 = o.BridgeTraderName2;
            po.BridgeType = o.BridgeType;
            po.BridgeType2 = o.BridgeType2;
            po.BridgeUpdateTime = o.BridgeUpdateTime;
            po.BridgeUpdateTime2 = o.BridgeUpdateTime2;
            po.brokeragea = o.BrokerageA;
            po.brokerageb = o.BrokerageB;


            po.DoubleBridgeBidSettltype = o.DoubleBridgeBidSettltype;
            po.DoubleBridgeChannel = o.DoubleBridgeChannel;

            po.DoubleBridgeFee = o.DoubleBridgeFee;
            po.DoubleBridgeOffSettltype = o.DoubleBridgeOffSettltype;
            po.DoubleBridgeRemark = o.DoubleBridgeRemark;
            po.DoubleBridgeSendTo = o.DoubleBridgeSendTo;
            po.doubleBridgeSettleSide = o.DoubleBridgeSettleSide;

            po.ExchangeSignFlag = o.ExchangeSignFlag;
            return po;
        }



        public static TransOrderServerData ConvertTransOrderServerData(OrderDetailPo o)
        {
            TransOrderServerData po = new TransOrderServerData();
            po.Sequence = o.seq + "";
            po.ExecID = o.ExecID;
            po.OrderID = o.OrderID;
            po.TradeTime  = o.dealdate;
            //po.delflag = o.Delflag;
            po.OrdStatus = o.OrdStatus;
            po.Side = o.Side;
            po.SecurityID = o.SecurityID;
            po.Symbol  = o.goodsShortName;
            po.SecurityType  = o.goodstype;
            po.BondSubtype = o.BondSubtype;

            po.Price = o.price + "";

            po.OrderQty = o.qty + "";
            //po.ListedMarket = o.m
            po.SettlDate = o.SettlDate;
            po.SettlDate2 = o.SettlDate2;
            po.SettlType = o.SettlType;
            po.SettlType2 = o.SettlType2;
            po.Remark  = o.remarks;

            po.InstitutionName = o.InstitutionName;
            po.TraderName = o.TraderName;
            po.PartySubID = o.PartySubID;
            po.PartySubName = o.PartySubName;

            po.CounterPartyInstitutionName = o.CounterPartyInstitutionName;//o.Counterpartyname;
            po.CounterPartyTraderName = o.CounterPartyTraderName;//o.Counterpartysubname;
            po.CounterPartySubName = o.CounterPartySubName;
            po.CounterPartySubID = o.CounterPartySubID;
            po.TransactTime  = o.dealdate;
            po.TransactTime  = o.DealDate2;

            po.SendTo = o.SendTo;
            po.SendTo2 = o.SendTo2;
            po.DoubleBridgeBidSettltype = o.DoubleBridgeBidSettltype;
            po.DoubleBridgeOffSettltype = o.BridgeRemarkShow2;
            po.DoubleBridgeChannel = o.BridgeRemarkShow2;

            po.DoubleBridgeChannel = o.BridgeRemarkShow2;
            po.DoubleBridgeChannel = o.BridgeRemarkShow2;

            po.BorderId = o.BorderId;
            po.BorderId2 = o.BorderId2;
            po.BridgeCloseById = o.BridgeCloseById;
            po.BridgeCreateTime = o.BridgeCreateTime;
            po.BridgeCreateTime2 = o.BridgeCreateTime2;
            po.BridgeFee = o.BridgeFee;
            po.BridgeFee2 = o.BridgeFee2;
            po.BridgeFlag = o.BridgeFlag;
            po.BridgeInstitutionId = o.BridgeInstitutionId;
            po.BridgeInstitutionId2 = o.BridgeInstitutionId2;
            po.BridgeInstitutionName = o.BridgeInstitutionName;
            po.BridgeInstitutionName2 = o.BridgeInstitutionName2;
            po.BridgeRemark = o.BridgeRemark;
            po.BridgeRemark2 = o.BridgeRemark2;
            po.BridgeRemarkShow = o.BridgeRemarkShow;
            po.BridgeRemarkShow2 = o.BridgeRemarkShow2;
            po.BridgeSendTo = o.BridgeSendTo;
            po.BridgeSendTo2 = o.BridgeSendTo2;
            po.BridgeStatus = o.BridgeStatus;

            po.BridgeTraderId = o.BridgeTraderId;
            po.BridgeTraderId2 = o.BridgeTraderId2;
            po.BridgeTraderName = o.BridgeTraderName;
            po.BridgeTraderName2 = o.BridgeTraderName2;
            po.BridgeType = o.BridgeType;
            po.BridgeType2 = o.BridgeType2;
            po.BridgeUpdateTime = o.BridgeUpdateTime;
            po.BridgeUpdateTime2 = o.BridgeUpdateTime2;
            po.BrokerageA  = o.brokeragea;
            po.BrokerageB  = o.brokerageb;


            po.DoubleBridgeBidSettltype = o.DoubleBridgeBidSettltype;
            po.DoubleBridgeChannel = o.DoubleBridgeChannel;

            po.DoubleBridgeFee = o.DoubleBridgeFee;
            po.DoubleBridgeOffSettltype = o.DoubleBridgeOffSettltype;
            po.DoubleBridgeRemark = o.DoubleBridgeRemark;
            po.DoubleBridgeSendTo = o.DoubleBridgeSendTo;
            po.DoubleBridgeSettleSide  = o.doubleBridgeSettleSide;

            po.ExchangeSignFlag = o.ExchangeSignFlag;
            return po;
        }

    }
}
