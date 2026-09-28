using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Transaction
{
    /// <summary>
    /// ExecUpdateWin.xaml 的交互逻辑
    /// </summary>
    public partial class ExecUpdateWin : Window
    {
        public ExecutionReport OldOrderDetail;
        public ExecutionReport NewOrderDetail;

        public ExecUpdateWin()
        {
            InitializeComponent();
            this.MouseDown += ExecUpdateWin_MouseDown;
            this.Unloaded += ExecUpdateWin_Unloaded;
            //this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        private void ExecUpdateWin_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MouseDown -= ExecUpdateWin_MouseDown;
            this.Unloaded -= ExecUpdateWin_Unloaded;
        }

        private void ExecUpdateWin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void RefreshForm()
        {
            SolidColorBrush bgYellow = (SolidColorBrush)FindResource("ColorBrush.Yellow2");
            SolidColorBrush bgBlack = (SolidColorBrush)FindResource("ColorBrush.Black2");

            if (OldOrderDetail != null)
            {
                if ((!string.IsNullOrEmpty(OldOrderDetail.BridgeFlag) && OldOrderDetail.BridgeFlag.Equals("1"))
                        || string.IsNullOrEmpty(OldOrderDetail.BridgeFlag))
                {
                    tbSend.Visibility = Visibility.Collapsed;
                    tbSendTo.Visibility = Visibility.Collapsed;
                    tbSendOfr.Visibility = Visibility.Collapsed;
                    tbSendToOfr.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbSend.Text = "Bid发给";
                    tbSendOfr.Visibility = Visibility.Visible;
                    tbSendToOfr.Visibility = Visibility.Visible;
                    tbSend.Visibility = Visibility.Visible;
                    tbSendTo.Visibility = Visibility.Visible;
                }
                tbSendTo.Text = OldOrderDetail.SendTo;
                tbSendToOfr.Text = OldOrderDetail.SendTo2;
                

                tbCode.Text = TransOrderHelper.Instance.GetBondCodeShow(OldOrderDetail);//OldOrderDetail.goodsCode;
                tbPrice.Text = OldOrderDetail.Price;
                tbQty.Text = OldOrderDetail.LastQty + "";
                tbSettle.Text = TransOrderHelper.Instance.IsDealHistDay(OldOrderDetail) ? TransOrderHelper.Instance.GetHistoryDisplaySettle(OldOrderDetail) : TransOrderHelper.Instance.GetDisplaySettle(OldOrderDetail);
               
                                                                                                                                                                              //若用户输入纯数字，则认为是返点信息，在明细窗口、过桥窗口、操作记录中展示时，均带有"F“开头。
                                                                                                                                                                              //2.2版本需求
                if (!string.IsNullOrEmpty(OldOrderDetail.FanDian))
                {
                    decimal d = 0;
                    if (decimal.TryParse(OldOrderDetail.FanDian, out d))
                        tbPrice.Text += !string.IsNullOrEmpty(OldOrderDetail.FanDian) ? "F" + OldOrderDetail.FanDian : string.Empty;
                    else
                        tbPrice.Text += OldOrderDetail.FanDian;
                }

                tbBrokerA.Text = OldOrderDetail.PartySubName;

                string sideDisplay = "";
                if (!string.IsNullOrWhiteSpace(OldOrderDetail.OnlineA) && !string.IsNullOrWhiteSpace(OldOrderDetail.OnlineB))
                {
                    if (OldOrderDetail.OnlineA.Equals("3") && OldOrderDetail.OnlineB.Equals("3"))
                    {
                        sideDisplay = OldOrderDetail.Side == "2" ? "出给" : "买入";
                    }
                    else
                    {
                        sideDisplay = OldOrderDetail.Side == "2" ? "GVN" : "TKN";
                    }
                }
                else
                {
                    sideDisplay = OldOrderDetail.Side == "2" ? "GVN" : "TKN";
                }
                tbSize.Text = sideDisplay;
                tbBrokerB.Text = OldOrderDetail.CounterPartySubName;

                tbTraderB.Text = TransOrderHelper.Instance.GetTraderBInfo(OldOrderDetail);
                //显示traderA信息 20200421
                if (!string.IsNullOrEmpty(TransOrderHelper.Instance.GetTraderAInfo(OldOrderDetail)))
                    tbTraderA.Text = TransOrderHelper.Instance.GetTraderAInfo(OldOrderDetail);
                else
                    tbTraderA.Text = string.Empty;

                SetSendInfo(OldOrderDetail,false);
            }

            if (NewOrderDetail != null)
            {
                tbOrderId.Text = string.Format(tbOrderId.Text, NewOrderDetail.Sequence);

                if ((!string.IsNullOrEmpty(NewOrderDetail.BridgeFlag) && NewOrderDetail.BridgeFlag.Equals("1"))
                       || string.IsNullOrEmpty(NewOrderDetail.BridgeFlag))
                {
                    tbSend2.Visibility = Visibility.Collapsed;
                    tbSendTo2.Visibility = Visibility.Collapsed;
                    tbSend2Ofr.Visibility = Visibility.Collapsed;
                    tbSendTo2Ofr.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbSend2.Text = "Bid发给";
                    tbSend2Ofr.Visibility = Visibility.Visible;
                    tbSendTo2Ofr.Visibility = Visibility.Visible;
                    tbSend2.Visibility = Visibility.Visible;
                    tbSendTo2.Visibility = Visibility.Visible;
                }
                tbSendTo2.Text = NewOrderDetail.SendTo;
                tbSendTo2Ofr.Text = NewOrderDetail.SendTo2;

                tbCodeNew.Text = TransOrderHelper.Instance.GetBondCodeShow(NewOrderDetail);//NewOrderDetail.goodsCode;
                tbPriceNew.Text = NewOrderDetail.Price;
                tbQtyNew.Text = NewOrderDetail.LastQty + "";
                //若用户输入纯数字，则认为是返点信息，在明细窗口、过桥窗口、操作记录中展示时，均带有"F“开头。
                if (!string.IsNullOrEmpty(NewOrderDetail.FanDian))
                {
                    decimal d = 0;
                    if (decimal.TryParse(NewOrderDetail.FanDian, out d))
                        tbPriceNew.Text += !string.IsNullOrEmpty(NewOrderDetail.FanDian) ? "F" + NewOrderDetail.FanDian : string.Empty;
                    else
                        tbPriceNew.Text += NewOrderDetail.FanDian;
                }

                tbSettleNew.Text = TransOrderHelper.Instance.IsDealHistDay(NewOrderDetail) ? TransOrderHelper.Instance.GetHistoryDisplaySettle(NewOrderDetail) : TransOrderHelper.Instance.GetDisplaySettle(NewOrderDetail);


                if (!string.IsNullOrEmpty(TransOrderHelper.Instance.GetTraderAInfo(NewOrderDetail)))
                    tbTraderANew.Text = TransOrderHelper.Instance.GetTraderAInfo(NewOrderDetail);
                else
                    tbTraderANew.Text = string.Empty;

                tbBrokerANew.Text = NewOrderDetail.PartySubName;
                string sideDisplay = "";
                if (!string.IsNullOrWhiteSpace(NewOrderDetail.OnlineA) && !string.IsNullOrWhiteSpace(NewOrderDetail.OnlineB))
                {
                    if (NewOrderDetail.OnlineA.Equals("3") && NewOrderDetail.OnlineB.Equals("3"))
                    {
                        sideDisplay = NewOrderDetail.Side == "2" ? "出给" : "买入";
                    }
                    else
                    {
                        sideDisplay = NewOrderDetail.Side == "2" ? "GVN" : "TKN";
                    }
                }
                else
                {
                    sideDisplay = NewOrderDetail.Side == "2" ? "GVN" : "TKN";
                }
                tbSizeNew.Text = sideDisplay;

                tbTraderBNew.Text = TransOrderHelper.Instance.GetTraderBInfo(NewOrderDetail);
                tbBrokerBNew.Text = NewOrderDetail.CounterPartySubName;

                SetSendInfo(NewOrderDetail, true);
            }

            //我是最后操作人
            if (LocalEnv.UserInfo.IsMyself(NewOrderDetail.ParamCloseById))
            {
                //我同时是brokerA和brokerB
                if (LocalEnv.UserInfo.IsMyself(NewOrderDetail.PartySubID) && LocalEnv.UserInfo.IsMyself(NewOrderDetail.CounterPartySubID))
                    btnKnow.Visibility = Visibility.Visible;
                else
                {
                    btnKnow.Visibility = Visibility.Collapsed;
                }
            }
            else
                btnKnow.Visibility = Visibility.Visible;

            if (OldOrderDetail != null && NewOrderDetail != null)
            {
                if (!IDCHelper.CheckStrEqual(tbSendTo.Text, tbSendTo2.Text))// && !(!string.IsNullOrEmpty(NewOrderDetail.bridgeflag) && NewOrderDetail.bridgeflag.Equals("2")))
                {
                    tbSendTo2.Background = bgYellow;
                    tbSendTo2.Foreground = bgBlack;
                }

                if (!tbSendToOfr.Text.Equals(tbSendTo2Ofr.Text))
                {
                    tbSendTo2Ofr.Background = bgYellow;
                    tbSendTo2Ofr.Foreground = bgBlack;
                }
                //}

                if (!IDCHelper.CheckStrEqual(OldOrderDetail.SecurityID, NewOrderDetail.SecurityID))
                {
                    tbCodeNew.Background = bgYellow;
                    tbCodeNew.Foreground = bgBlack;
                }


                if (!IDCHelper.CheckStrEqual(tbPrice.Text, tbPriceNew.Text))
                {
                    tbPriceNew.Background = bgYellow;
                    tbPriceNew.Foreground = bgBlack;
                }
                if (OldOrderDetail.LastQty != NewOrderDetail.LastQty)
                {
                    tbQtyNew.Background = bgYellow;
                    tbQtyNew.Foreground = bgBlack;
                }
                if (!IDCHelper.CheckStrEqual(tbSettle.Text, tbSettleNew.Text))
                {
                    tbSettleNew.Background = bgYellow;
                    tbSettleNew.Foreground = bgBlack;
                }
                if (!IDCHelper.CheckStrEqual(TransOrderHelper.Instance.GetTraderAInfo(OldOrderDetail), TransOrderHelper.Instance.GetTraderAInfo(NewOrderDetail)))
                {
                    tbTraderANew.Background = bgYellow;
                    tbTraderANew.Foreground = bgBlack;
                }
                if (!IDCHelper.CheckStrEqual(OldOrderDetail.PartySubName, NewOrderDetail.PartySubName))
                {
                    tbBrokerANew.Background = bgYellow;
                    tbBrokerANew.Foreground = bgBlack;
                }
                if (!IDCHelper.CheckStrEqual(TransOrderHelper.Instance.GetTraderBInfo(OldOrderDetail), TransOrderHelper.Instance.GetTraderBInfo(NewOrderDetail)))
                {
                    tbTraderBNew.Background = bgYellow;
                    tbTraderBNew.Foreground = bgBlack;
                }
                if (!IDCHelper.CheckStrEqual(OldOrderDetail.CounterPartySubName, NewOrderDetail.CounterPartySubName))
                {
                    tbBrokerBNew.Background = bgYellow;
                    tbBrokerBNew.Foreground = bgBlack;
                }


                if (!txt_Dir.Text.Equals(txt_DirN.Text))
                {
                    txt_DirN.Background = bgYellow;
                    txt_DirN.Foreground = bgBlack;
                }

                if (!txt_Chnenl.Text.Equals(txt_ChnenlN.Text))
                {
                    txt_ChnenlN.Background = bgYellow;
                    txt_ChnenlN.Foreground = bgBlack;
                }

                if (!txt_SendInst.Text.Equals(txt_SendInstN.Text))
                {
                    txt_SendInstN.Background = bgYellow;
                    txt_SendInstN.Foreground = bgBlack;
                }

                if (!txt_Reamks.Text.Equals(txt_ReamksN.Text))
                {
                    txt_ReamksN.Background = bgYellow;
                    txt_ReamksN.Foreground = bgBlack;
                }

                if (!txt_payFee.Text.Equals(txt_payFeeN.Text))
                {
                    txt_payFeeN.Background = bgYellow;
                    txt_payFeeN.Foreground = bgBlack;
                }

                if (!txt_Fee.Text.Equals(txt_FeeN.Text))
                {
                    txt_FeeN.Background = bgYellow;
                    txt_FeeN.Foreground = bgBlack;
                }

            }
        }


        /// <summary>
        /// 设置发给标准化信息
        /// </summary>
        private void SetSendInfo(ExecutionReport orderDetail ,bool isNewInfo) {
            if ((!string.IsNullOrEmpty(orderDetail.BridgeFlag) && orderDetail.BridgeFlag.Equals("1"))
                        || string.IsNullOrEmpty(orderDetail.BridgeFlag))
            {
                if (isNewInfo)
                {
                    sp_sendNew.Visibility = Visibility.Visible;

                    if (orderDetail.IssueDir == "1")
                    {
                        txt_DirN.Text = "ofr发bid";
                    }
                    else if (orderDetail.IssueDir == "2")
                    {
                        txt_DirN.Text = "bid发ofr";
                    }
                    else {
                        txt_DirN.Text = " ";
                    }

                    txt_ChnenlN.Text = GetIssueChannelShow(orderDetail);

                    txt_SendInstN.Text = GetIssueTraderShow(orderDetail);

                    txt_ReamksN.Text = orderDetail.IssueRemark;
                    txt_payFeeN.Text = GetIssuePayFeeShow(orderDetail);
                    txt_FeeN.Text = orderDetail.ProxyFee + "厘";
                }
                else {
                    sp_sendOld.Visibility = Visibility.Visible;
                    if (orderDetail.IssueDir == "1")
                    {
                        txt_Dir.Text = "ofr发bid";
                    }
                    else if (orderDetail.IssueDir == "2")
                    {
                        txt_Dir.Text = "bid发ofr";
                    }
                     else
                    {
                        txt_Dir.Text = " ";
                    }
                    txt_Chnenl.Text = GetIssueChannelShow(orderDetail);

                    txt_SendInst.Text = GetIssueTraderShow(orderDetail);

                    txt_Reamks.Text = orderDetail.IssueRemark;
                    txt_payFee.Text = GetIssuePayFeeShow(orderDetail);
                    txt_Fee.Text = orderDetail.ProxyFee + "厘";
                }
            }
            else {
                if (isNewInfo)
                {
                    sp_sendNew.Visibility = Visibility.Collapsed;
                    txt_payFeeN.Text = GetIssuePayFeeShow(orderDetail);
                    txt_FeeN.Text = orderDetail.ProxyFee + "厘";
                }
                else
                {
                    sp_sendOld.Visibility = Visibility.Collapsed;
                    txt_payFee.Text = GetIssuePayFeeShow(orderDetail);
                    txt_Fee.Text = orderDetail.ProxyFee + "厘";
                }
            }
        }



        private string GetIssueTraderShow(ExecutionReport orderDetailPo)
        {
            string idesc = "";

            string issueInstName = orderDetailPo.IssueInstNameShort;
            if (string.IsNullOrWhiteSpace(issueInstName))
            {
                issueInstName = orderDetailPo.IssueInstName;
            }

            if (!string.IsNullOrWhiteSpace(orderDetailPo.SecurityID))
            {
                if (string.IsNullOrWhiteSpace(orderDetailPo.IssueChannel))
                {
                    return idesc;
                }
                ChannelEnum channelEnum = (ChannelEnum)System.Enum.Parse(typeof(ChannelEnum), orderDetailPo.IssueChannel, true);
                switch (channelEnum)
                {
                    case ChannelEnum.Request:
                        idesc = orderDetailPo.IssueDesc + " " + issueInstName;
                        break;
                    case ChannelEnum.FixedIncome:
                    case ChannelEnum.Bulk:
                    case ChannelEnum.Bidding:
                    case ChannelEnum.Dialogue:
                    case ChannelEnum.Bilateral:
                    case ChannelEnum.xbond:
                    case ChannelEnum.iDeal:
                    case ChannelEnum.Xbargain:
                        if (!string.IsNullOrWhiteSpace(orderDetailPo.IssueTraderName))
                        {
                            idesc = orderDetailPo.IssueDesc + issueInstName + "(" + orderDetailPo.IssueTraderName + ")";
                           
                        }
                        else
                        {
                            idesc = orderDetailPo.IssueDesc + issueInstName;
                           
                        }
                        break;
                    case ChannelEnum.Intention:
                    case ChannelEnum.Anonymity:
                        if (!string.IsNullOrWhiteSpace(orderDetailPo.IssueTraderName))
                        {
                            idesc = issueInstName + "(" + orderDetailPo.IssueTraderName + ")";
                        }
                        else
                        {
                            idesc = issueInstName;
                        }
                        break;
                }
            }
            else
            {
                idesc = orderDetailPo.IssueDesc + issueInstName;
                //idesc = issueInstName;
            }

            return idesc;
        }

        private string GetIssueChannelShow(ExecutionReport orderDetailPo)
        {
            if (string.IsNullOrWhiteSpace(orderDetailPo.IssueChannel))
            {
                return "";
            }

            string strChannel = "";
            foreach (ComboxBaseModel item in EnumHelper.Instance.ChannelComboxModel)
            {
                if (item.ValueMemberPath == orderDetailPo.IssueChannel)
                {
                    strChannel = item.DisplayMemberPath;
                    return strChannel;
                }
            }
            return "";
        }




        private string GetIssuePayFeeShow(ExecutionReport orderDetailPo)
        {
            string instName = "";
            //查询代付信息
            if (orderDetailPo.ProxyFlag == "1")
            {
                if (!string.IsNullOrWhiteSpace(orderDetailPo.ProxyInstId) && orderDetailPo.ProxyInstId == orderDetailPo.InstitutionID)
                {
                    if (!string.IsNullOrWhiteSpace(orderDetailPo.InstitutionShowName))
                    {
                        instName = "帮" + orderDetailPo.InstitutionShowName + "代付";
                    }
                    else
                    {
                        instName = "帮" + orderDetailPo.InstitutionName + "代付";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(orderDetailPo.ProxyInstId) && orderDetailPo.ProxyInstId == orderDetailPo.CounterPartyInstitutionId)
                {
                    if (!string.IsNullOrWhiteSpace(orderDetailPo.CounterPartyInstitutionShowName))
                    {
                        instName = "帮" + orderDetailPo.CounterPartyInstitutionShowName + "代付";
                    }
                    else
                    {
                        instName = "帮" + orderDetailPo.CounterPartyInstitutionName + "代付";
                    }
                }
            }
            return instName;
        }

        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //if (OnBtnClick != null)
            //    OnBtnClick(OldOrderDetail == null ? string.Empty : OldOrderDetail.execId, false);
            this.Close();
        }

        public event Action<ExecutionReport, bool> OnBtnClick;

        private void btnKnow_Click(object sender, RoutedEventArgs e)
        {
            if (OnBtnClick != null)
                OnBtnClick(OldOrderDetail, true);
            this.Close();
        }
        /// <summary>
        /// 右击窗口任务栏图标-点击删除，无法触发OnBtnClick事件，故使用OnClosed事件触发。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (OnBtnClick != null)
                OnBtnClick(OldOrderDetail, false);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (btnKnow.Visibility == Visibility.Visible)
                    btnKnow_Click(null, null);
            }
        }
    }
}
