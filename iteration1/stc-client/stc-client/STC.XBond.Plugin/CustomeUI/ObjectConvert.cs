using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Drp;
using STC.Common.Utils;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.XBond.Plugin.CustomUI
{
    /// <summary>
    /// 对象转换器
    /// </summary>
    public class ObjectConvert
    {
        public static void UpdatePriceValue(GridData oldData, GridData newData)
        {
            oldData.Amount = newData.Amount;
            oldData.FirmType = newData.FirmType;
            oldData.Id = newData.Id;
            oldData.Price = newData.Price;
            oldData.Sort = newData.Sort;
            oldData.Source = newData.Source;
            oldData.PriceBrush = newData.PriceBrush;
            oldData.PartySubID = newData.PartySubID;
        }

        public static GridData PriceValueToGridData(PriceValue pv)
        {
            GridData result = new GridData();

            result.Amount = ((int)pv.Qty).ToString();
            result.FirmType = pv.IsFirmFlag ? "firm" : "nonfirm";
            result.Id = pv.Id;
            result.Price = pv.Value.ToString("N4");
            result.Source = GenerateSourceByVenues(pv.Venue, pv.CounterPartySubName);
            string showName = ComboSourceUtil.Instance.FindEnum(pv.Venue, "venues");
            if (!string.IsNullOrEmpty(showName))
                result.Source = showName;

            if (result.Source == "内部")
            {
                result.Source = pv.PartySubName;
            }
            result.PartySubID = pv.PartySubID;

            result.Sort = pv.SN.ToString();

            result.Settle = ParseSettle(pv.SettlType);
            result.DealSettle = ParseSettle(pv.SettlType);

            
            result.Symbol = pv.Securityid;
            result.Side = pv.Side;
            result.WholeFlag = pv.WholeFlag;
            result.VenueTypeGW = pv.VenueGW;
            result.Venue = pv.Venue;
            result.PointAmount = result.Amount;

            result.TrsactTime = string.Format("{0} {1}", pv.Date, pv.Time);
            result.QuoteType = pv.QuoteTransType;
            result.ClearingMethod = pv.ClearingMethod;
            result.DeliveryType = pv.DeliveryType;

            if (!string.IsNullOrEmpty(pv.WholeFlag) && pv.WholeFlag.Equals("1"))
                result.WholeVisibility = Visibility.Visible;

            return result;
        }


        
        private static string ParseSettle(string settle)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(settle))
            {
                string[] arr = settle.Split(new char[] { ',' });
                StringBuilder sb = new StringBuilder();
                if (arr != null && arr.Length > 0)
                {
                    foreach (string str in arr)
                    {
                        string tem = CommonMapUtil.FindEnum(str, "settletype");
                        sb.Append(tem + ",");
                    }
                }

                string parseStr = sb.ToString();

                result = parseStr.Length > 0 ? parseStr.Substring(0, parseStr.Length - 1) : parseStr;

            }

            return result;
        }

        private static string GenerateSourceByVenues(string venues, string partSubName)
        {
            if (string.IsNullOrEmpty(venues) == false)
            {
                if (venues.ToUpper() == "IDB")
                {
                    return $"销售-{partSubName}";
                }
                else if (venues.ToUpper() == "INNER"
                    || venues.ToUpper() == "QUANT"
                    || venues.ToUpper() == "MANUAL")
                {
                    return $"交易员-{partSubName}";
                }
            }
            return venues;
        }


        public static void UpdateMsg(MsgPo oldMsg, MsgPo newPo)
        {
            oldMsg.BorderName = newPo.BorderName;
            oldMsg.CloseBy = newPo.CloseBy;
            oldMsg.CloseByBrush = newPo.CloseByBrush;
            oldMsg.CloseByHeight = newPo.CloseByHeight;
            oldMsg.CloseByVisibility = newPo.CloseByVisibility;
            oldMsg.Content = newPo.Content;
            oldMsg.ContentBrush = newPo.ContentBrush;
            oldMsg.HeaderTitle = newPo.HeaderTitle;
            oldMsg.ContentTypeBrush = newPo.ContentTypeBrush;
            oldMsg.counterpartyid = newPo.counterpartyid;
            oldMsg.counterpartyname = newPo.counterpartyname;
            oldMsg.counterpartysubid = newPo.counterpartysubid;
            oldMsg.counterpartysubname = newPo.counterpartysubname;
            oldMsg.createtime = newPo.createtime;
            oldMsg.DealVisibility = newPo.DealVisibility;
            oldMsg.Id = newPo.Id;
            oldMsg.KnowVisibility = newPo.KnowVisibility;
            oldMsg.Level = newPo.Level;
            oldMsg.ReadFlag = newPo.ReadFlag;
            oldMsg.ShowBG = newPo.ShowBG;
            oldMsg.ShowBorderBrush = newPo.ShowBorderBrush;
            oldMsg.ShowBorderThickness = newPo.ShowBorderThickness;
            oldMsg.ShowHeight = newPo.ShowHeight;
            oldMsg.ShowPopBG = newPo.ShowPopBG;
            oldMsg.ShowPopHeight = newPo.ShowPopHeight;
            oldMsg.ShowStatusBG = newPo.ShowStatusBG;
            oldMsg.Status = newPo.Status;
            oldMsg.StatusBrush = newPo.StatusBrush;
            oldMsg.StatusVisibility = newPo.StatusVisibility;
            oldMsg.StatusWidth = newPo.StatusWidth;
            oldMsg.Storyboard = newPo.Storyboard;
            oldMsg.transacttime = newPo.transacttime;
            oldMsg.UpdateTime = newPo.UpdateTime;
            oldMsg.UpdateTimeBrush = newPo.UpdateTimeBrush;
            oldMsg.ValueTime = newPo.ValueTime;
            oldMsg.ValueTimeBrush = newPo.ValueTimeBrush;
            oldMsg.ValueTimeVisibility = newPo.ValueTimeVisibility;
            oldMsg.DelFlag = newPo.DelFlag;
            oldMsg.MessageContent = newPo.MessageContent;
            oldMsg.Detail = newPo.Detail;
            oldMsg.BtnDealShowTxt = newPo.BtnDealShowTxt;

            oldMsg.ContentType = newPo.ContentType;
            oldMsg.TimeOutTxtVisibility = newPo.TimeOutTxtVisibility;
            oldMsg.UpdateTimeVisibility = newPo.UpdateTimeVisibility;

            oldMsg.PopRefuseVisibility = newPo.PopRefuseVisibility;

            oldMsg.SignalDetail = newPo.SignalDetail;
            oldMsg.ReadMsgVisible = newPo.ReadMsgVisible;
            oldMsg.InnerColorAnimation = newPo.InnerColorAnimation;
            oldMsg.groupname = newPo.groupname;
        }

        public static SystemMessageReq MsgPoToSystemMessageReq(MsgPo msgPo, string msgType)
        {
            SystemMessageReq req = new SystemMessageReq();
            req.id = msgPo.Id;
            req.type = msgType;
            req.readflag = "1";// LocalEnv.UserInfo.UserId;
            req.username = LocalEnv.UserInfo.UserName;
            TMessageContent content = new TMessageContent();
            //content.type = "N";
            if (msgPo.Level.Equals("1"))
                content.type = "N";
            else if (msgPo.Level.Equals("2"))
                content.type = "N";
            else if (msgPo.Level.Equals("3"))
                content.type = "A";

            //if (msgPo.ContentType.Equals(AdminConsts.Admin_ExecutionReport))
            //    content.type = AdminConsts.Admin_ER;
            //else if (msgPo.ContentType.Equals(AdminConsts.Admin_RFQ_Txt))
            //    content.type = AdminConsts.Admin_RFQ;
            
            content.content = msgPo.Content;
            content.title = !string.IsNullOrEmpty(msgPo.HeaderTitle) ? msgPo.HeaderTitle.Replace("[", "").Replace("]", "") : msgPo.HeaderTitle;
            content.validtime = msgPo.ValueTime;
            content.counterpartyid = msgPo.counterpartyid;
            content.counterpartyname = msgPo.counterpartyname;
            content.counterpartysubid = msgPo.counterpartysubid;
            content.counterpartysubname = msgPo.counterpartysubname;
            if (!string.IsNullOrEmpty(msgPo.Status))
            {
                if (msgPo.Status.Equals(AdminConsts.Admin_Status_Normal))
                    content.status = AdminConsts.Admin_Status_One;
                else if (msgPo.Status.Equals(AdminConsts.Admin_Status_Outtime))
                    content.status = AdminConsts.Admin_Status_Two;
            }
            req.content = MsgHelper.Serializer(content);
            req.id = msgPo.Id;
            req.closeby = LocalEnv.UserInfo.UserId;
            req.createtime = msgPo.createtime;
            req.test = string.Empty;

            if (msgType.Equals("2"))
                req.delFlag = "1";
            
            return req;
        }

        public static MsgPo SystemMsgToShowMsg(TTbookSystemMessage sysMsg, FrameworkElement uc)
        {
            MsgPo result = null;
            try
            {
                if (string.IsNullOrEmpty(sysMsg.content))
                {
                    return result;
                }

                if (!JsonSplit.IsJson(sysMsg.content))
                {
                    return result;
                }

                //TMessageContent detail = MsgHelper.Deserialize<TMessageContent>(sysMsg.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                TMessageContent detail = JsonConvert.DeserializeObject<TMessageContent>(sysMsg.content);
                if (sysMsg.type.Equals("2"))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(detail.detail))
                        {
                            PriceMatchDetail priceMatchDetail = MsgHelper.Deserialize<PriceMatchDetail>(detail.detail);
                            if ((priceMatchDetail == null || priceMatchDetail.Prices == null) && detail.contentType.Equals(AdminConsts.PriceMatch))
                                return result;
                            else if (priceMatchDetail != null && priceMatchDetail.Prices != null
                                && (priceMatchDetail.Prices == null || priceMatchDetail.Prices.Count == 0)
                                && (!string.IsNullOrEmpty(detail.contentType) && detail.contentType.Equals(AdminConsts.PriceMatch)))
                            {
                                sysMsg.delflag = "1";//前台主动过滤匹配0条报价的消息
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }

                    ////增加消息复制
                    //if (!string.IsNullOrEmpty(sysMsg.detail))
                    //{
                    //    try
                    //    {
                    //        SystemMessageDetail systemMessageDetail = MsgHelper.Deserialize<SystemMessageDetail>(sysMsg.detail, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                    //        if (!string.IsNullOrEmpty(systemMessageDetail.EventName) && systemMessageDetail.EventName.ToUpper() == ("clipboard").ToUpper())
                    //            Clipboard.SetDataObject(systemMessageDetail.Data, true);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //    }
                    //}

                }
                result = new MsgPo();
                result.username = sysMsg.username;
                result.MessageContent = detail;
                result.Level = detail.type;
                result.Id = sysMsg.id;//sysMsg.id;
                result.HeaderTitle = !string.IsNullOrEmpty(detail.title)?string.Format("[{0}]", detail.title):string.Empty;
                result.ContentType = detail.contentType;
                result.counterpartyid = detail.counterpartyid;
                result.counterpartyname = detail.counterpartyname;
                result.counterpartysubid = detail.counterpartysubid;
                result.counterpartysubname = detail.counterpartysubname;
                result.ReadFlag = sysMsg.readflag;

                result.Venue = detail.venue;
                result.Content = detail.content;
                result.IsShow = detail.show ?? true;
                result.NeedSend = detail.needSend ?? true;
                
                if (!string.IsNullOrEmpty(result.ReadFlag))
                {
                    string[] arr = result.ReadFlag.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Contains(LocalEnv.UserInfo.UserName))
                    {
                        result.ReadMsgVisible = Visibility.Collapsed;
                        result.IsShow = false;
                    }
                    else
                    {
                        result.ReadMsgVisible = Visibility.Visible;
                        result.IsShow = true;
                    }
                }
                else
                {
                    result.ReadMsgVisible = Visibility.Visible;
                }

                //如果消息提醒不是RFQ则走原来的流程，修改CloseBy   result.ContentType.Equals("DQB") || result.ContentType.Equals("DQ")
                if (sysMsg.closeby != "RFQSvr" && sysMsg.closeby != "RFQMSvr" )
                {
                    if (!string.IsNullOrEmpty(detail.counterpartyname) && !string.IsNullOrEmpty(detail.counterpartysubname))
                        result.CloseBy = string.Format("{0}({1})", detail.counterpartyname, detail.counterpartysubname);
                    else if (string.IsNullOrEmpty(detail.counterpartyname) && string.IsNullOrEmpty(detail.counterpartysubname))
                        result.CloseBy = string.Empty;
                    else
                        result.CloseBy = string.IsNullOrEmpty(detail.counterpartyname) ? detail.counterpartysubname : detail.counterpartyname;
                }

                result.ValueTime = detail.validtime;//"有效时间 3分20秒";
                if (!string.IsNullOrEmpty(sysMsg.createtime))
                    result.UpdateTime = sysMsg.createtime.Substring(11, 8); //"10:32:21";
                result.createtime = sysMsg.createtime;

                if (result.Level != null)
                {
                    if (result.Level.Equals(AdminConsts.Admin_Critical))
                    {
                        result.Level = "1";

                        {
                            result.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red8");
                            result.ValueTimeVisibility = Visibility.Visible;
                            result.ShowHeight = 96;
                            result.StatusWidth = 40;
                            result.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red7");
                        }
                    }
                    else if (result.Level.Equals(AdminConsts.Admin_Normal))
                    {
                        result.Level = "3";
                        result.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray13");

                        result.ShowHeight = 78;
                        result.StatusWidth = 40;

                        result.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray13");
                    }
                    else if (result.Level.Equals(AdminConsts.Admin_Warn))
                    {
                        result.Level = "2";
                        result.ShowBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray13");

                        if (sysMsg.type.Equals("1"))
                            result.DealVisibility = Visibility.Collapsed;

                        result.ShowHeight = 96;
                        result.StatusWidth = 42;
                        result.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");
                    }


                    if (string.IsNullOrEmpty(result.ValueTime))
                    {
                        result.ValueTimeVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        result.ValueTimeVisibility = Visibility.Visible;

                    }

                    if (!string.IsNullOrEmpty(sysMsg.readflag) && sysMsg.readflag.Equals("1"))
                    {
                        result.KnowVisibility = Visibility.Collapsed;
                    }

                    result.CloseByHeight = 0;
                    result.CloseByVisibility = Visibility.Collapsed;
                    result.ShowHeight = 78;

                    if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("1"))
                    {
                        result.DealVisibility = Visibility.Collapsed;
                        result.BtnShowTxt = "清除";
                    }
                    else if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("2"))
                    {
                        result.BtnDealShowTxt = "处理";
                        result.BtnShowTxt = "忽略";
                        result.KnowVisibility = Visibility.Visible;
                        if (detail.content.EndsWith("中断") || detail.content.EndsWith("恢复"))
                        {
                            result.DealVisibility = result.PopDealVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            result.DealVisibility = result.PopDealVisibility = Visibility.Visible;
                        }

                        if (result.ContentType.Equals(AdminConsts.Admin_ER) || result.ContentType.Equals(AdminConsts.Admin_EV))
                        {
                            if ((!string.IsNullOrEmpty(detail.venue) && AdminHandler.Instance.CheckPermission("1001", string.Empty) && result.Level.Equals("2"))
                                || (result.ContentType.Equals(AdminConsts.Admin_EV)))
                                result.BtnDealShowTxt = "处理";
                            else
                                result.BtnDealShowTxt = "查看";


                        }
                    }
                    if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("2") && !string.IsNullOrEmpty(detail.detail))
                    {
                        result.Status = "";
                        result.StatusWidth = 50;
                        result.ShowStatusBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");
                        result.StatusVisibility = Visibility.Visible;

                        if (result.ContentType.Equals(AdminConsts.PriceMatch))
                            result.Status = "请确认";

                    }
                    else if (sysMsg.type.Equals("1"))
                    {
                        if (result.Level.Equals("1"))
                        {
                            result.Status = "";
                            result.StatusWidth = 50;
                            result.ShowStatusBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red7");
                            result.StatusVisibility = Visibility.Visible;
                        }
                        else if (result.Level.Equals("2"))
                        {
                            result.Status = "";
                            result.StatusWidth = 80;
                            result.ShowStatusBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");
                            result.StatusVisibility = Visibility.Visible;
                        }
                        else if (result.Level.Equals("3"))
                        {
                            result.StatusVisibility = Visibility.Collapsed;
                        }

                        if (detail.title.Equals("连接中断"))
                            result.Status = "";
                        else if (detail.title.Equals("下单失败") || detail.title.Equals("点价单失败"))
                            result.Status = "请知悉";
                        else if (detail.title.Equals("撤单失败"))
                            result.Status = "";
                        else if (detail.title.Equals("订单超时"))
                            result.Status = "";

                    }

                    if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("2") && result.Level.Equals("3"))
                    {
                        result.StatusVisibility = Visibility.Collapsed;
                    }

                    result.MsgType = sysMsg.type;

                    ////Nonfirm点价待确认，则修改过期时间为接收到的时间+IocTimeOut
                    if (result.ContentType != null)
                    {
                        if (result.ContentType.Equals(AdminConsts.Admin_IT))
                        {
                            PointPriceDetailModel ppdm = Newtonsoft.Json.JsonConvert.DeserializeObject<PointPriceDetailModel>(detail.detail);
                            ppdm.iocOrder.validuntitime = STC.Common.Utils.DateTimeEx.GetNow().AddSeconds(int.Parse(ppdm.iocOrder.iocTimeOut)).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            result.Detail = JsonConvert.SerializeObject(ppdm);
                        }
                        else
                        {
                            result.Detail = detail.detail;
                        }
                    }

                    
                    result.DelFlag = sysMsg.delflag;

                    result.SignalDetail = sysMsg.detail;

                    result.groupname = detail.groupname;

                    if (result.ContentType != null)
                    {
                        if (result.ContentType.Equals(AdminConsts.Admin_IT))
                        {
                            //Nonfirm点价待确认
                            result.TimeOutTxtVisibility = Visibility.Visible;
                            result.UpdateTimeVisibility = Visibility.Collapsed;

                            result.PopDealVisibility = Visibility.Visible;
                            result.PopRefuseVisibility = Visibility.Visible;
                            result.KnowVisibility = Visibility.Collapsed;

                            result.ShowBorderBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Red16");//ColorBrush.Red7");

                        }
                        else if (result.ContentType.Equals(AdminConsts.Admin_ST))
                        {
                            //如果是RFQ撤单，则隐藏去处理按钮
                            if (sysMsg.closeby == "RFQSvr" || sysMsg.closeby == "RFQMSvr")
                            {
                                result.PopDealVisibility = Visibility.Collapsed;
                                result.DealVisibility = Visibility.Collapsed;
                            }
                        }
                        else if (result.ContentType.Equals("DQB") || result.ContentType.Equals("DQ"))
                        {
                            result.BtnShowTxt = "清除";
                            result.HeaderTitle = !string.IsNullOrEmpty(detail.title) ? string.Format("{0}", detail.title) : string.Empty;
                            //定向回价
                            result.KnowVisibility = Visibility.Visible;
                            result.LookVisibility = Visibility.Collapsed;
                            result.PopQuickDealVisibility = Visibility.Collapsed;
                            result.PopDealVisibility = Visibility.Collapsed;
                            result.DealVisibility = Visibility.Visible;
                            result.BtnDealShowTxt = "回价";
                            //新增参数
                            result.TbContentVisibility = Visibility.Collapsed;
                            result.DQVisibility = Visibility.Visible;
                            result.UpdateTimeVisibility = Visibility.Visible;

                            DRPDetail dr = Newtonsoft.Json.JsonConvert.DeserializeObject<DRPDetail>(result.Detail);

                            //原始报价信息
                            //result.DQSecurityID = dr.order.symbol;
                            if (dr.order.symbolType.ToUpper() == "BCO")
                            {
                                result.DQSecurityID = dr.order.securityid;
                            }
                            else
                            {
                                result.DQSecurityID = dr.order.symbol;
                            }
                            result.DQSide = (dr.order.side.ToUpper().Equals("BUY") || dr.order.side.Equals("1")) ? "1" : "2";
                            result.DQShowSide = result.DQSide == "1" ? "Bid" : "Ofr";
                            string tmpprice = dr.order.price.ToString() == "--" ? "--" : CommonUtils.FormatNum(dr.order.price.ToString());
                            if ((CommonUtils.ConvertToDecimal(tmpprice).ToString("F4") == "-1.0000") || (CommonUtils.ConvertToDecimal(tmpprice).ToString("F4") == "999.0000"))
                            {
                                result.DQShowPrice = "--";
                            }
                            else
                            {
                                result.DQShowPrice = tmpprice.ToString() == "--" ? "--" : CommonUtils.FormatNum(tmpprice.ToString());
                            }
                            result.DQAmount = dr.order.qty.ToString() == "0" ? "--" : (dr.order.qty - dr.order.cumQty).ToString();
                            
                            if (dr.order.marketIndicator.ToString().Trim() == "2")
                                result.DQFirmType = "";
                            else
                                result.DQFirmType = dr.order.firmFlag == "1" ? $"Firm" : $"NF";

                            result.DQName = dr.order.name;
                            result.DQForegroundBrush = result.DQSide == "1" ? (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange7") : (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
                            //建议： dqOrderInfo2  //1 - 议价，2 - 成交，3 - 拒绝  4-撤销  5 - 报价方撤单
                            if (dr.iocOrder.dqOrderInfo2 != null)
                            {
                                if (dr.iocOrder.dqOrderInfo2.bargainYield != null)
                                    result.BargainYield = dr.iocOrder.dqOrderInfo2.bargainYield.ToString() == "--" ? "--" : CommonUtils.FormatNum(dr.iocOrder.dqOrderInfo2.bargainYield.ToString());
                                else
                                    result.BargainYield = "--";
                                if (dr.iocOrder.dqOrderInfo2.bargainQty != null)
                                    result.BargainQty = dr.iocOrder.dqOrderInfo2.bargainQty.ToString() == "0" ? "--" : dr.iocOrder.dqOrderInfo2.bargainQty.ToString();
                                else
                                    result.BargainQty = "--";
                            }

                            //我的
                            result.IocDQSide = (dr.iocOrder.side.ToUpper().Equals("BUY") || dr.iocOrder.side.Equals("1")) ? "1" : "2";
                            result.IocDQShowSide = result.IocDQSide == "1" ? "Bid" : "Ofr";
                            result.IocDQShowPrice = dr.iocOrder.price.ToString() == "--" ? "--" : CommonUtils.FormatNum(dr.iocOrder.price.ToString());
                            result.IocDQAmount = dr.iocOrder.qty.ToString() == "0" ? "--" : dr.iocOrder.qty.ToString();
                            result.IocDQForegroundBrush = result.IocDQSide == "1" ? (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange7") : (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8");
                            result.IocDQName = dr.iocOrder.name;
                            //UI显示控制
                            result.HeaderTitle = result.ContentType.Equals("DQ") ? "定向回价[收到回价]" : "定向回价[价格讨论]";
                            result.DQBVisibility = Visibility.Visible;
                            //1 - 议价，2 - 成交，3 - 拒绝  4-撤销 5 - 报价方撤单 6超时
                            if (dr.iocOrder.dqOrderInfo2 != null)
                            {
                                if (dr.iocOrder.dqOrderInfo2.actionType == "3")
                                {
                                    result.HeaderTitle = "定向回价[已忽略]";
                                    result.DQBVisibility = Visibility.Collapsed;
                                }
                                if (dr.iocOrder.dqOrderInfo2.actionType == "4")
                                {
                                    result.HeaderTitle = "定向回价[已撤销]";
                                    result.DQBVisibility = Visibility.Collapsed;
                                    result.DQVisibility = Visibility.Collapsed;
                                    result.TbContentVisibility = Visibility.Visible;
                                    result.BtnDealShowTxt = "查看";
                                }
                                if (dr.iocOrder.dqOrderInfo2.actionType == "5")
                                {
                                    result.HeaderTitle = "定向回价[报价已撤]";
                                    result.DQBVisibility = Visibility.Collapsed;
                                    result.DQVisibility = Visibility.Collapsed;
                                    result.TbContentVisibility = Visibility.Visible;
                                    result.BtnDealShowTxt = "查看";
                                }
                            }
                            if (dr.iocOrder.rejCode == "10010")
                            {
                                result.HeaderTitle = "定向回价[报价已撤]";
                                result.DQBVisibility = Visibility.Collapsed;
                                result.DQVisibility = Visibility.Collapsed;
                                result.TbContentVisibility = Visibility.Visible;
                                result.BtnDealShowTxt = "查看";
                            }
                            if (dr.iocOrder.rejCode == "11015")
                            {
                                result.HeaderTitle = "定向回价[已超时]";
                                result.DQBVisibility = Visibility.Collapsed;
                                result.BtnDealShowTxt = "回价";
                            }
                            if (dr.iocOrder.rejCode == "11013")
                            {
                                result.HeaderTitle = "定向回价[已撤销]";
                                result.DQBVisibility = Visibility.Collapsed;
                                result.DQVisibility = Visibility.Collapsed;
                                result.TbContentVisibility = Visibility.Visible;
                                result.BtnDealShowTxt = "查看";
                            }

                            //修改Grid的Content;
                            //if (result.IocDQName == LocalEnv.UserInfo.UserName)
                            //{
                            //    result.IocDQName = "我的";
                            //}

                            result.Content = $"{result.DQName}：{result.DQSecurityID} {result.DQShowSide} {result.DQShowPrice} {result.DQAmount}万 {result.DQFirmType}";
                            result.Content += $"\r\n{result.IocDQName}：{result.IocDQShowSide} {result.IocDQShowPrice} {result.IocDQAmount}万";
                            if (result.DQBVisibility == Visibility.Visible && result.ContentType.Equals("DQB"))
                            {
                                result.Content += $"\r\n{result.DQName}：建议 {result.DQShowSide} {result.BargainYield} {result.BargainQty}万";
                            }
                        }
                        else if (result.ContentType.Equals("Quant_Focus_Tag") || result.ContentType.Equals("TableSvr_FOCUS_TAG")
                            || result.ContentType.EndsWith("TableSvr_FOCUS_TAG_RW"))
                        {
                            if (result.ContentType == "TableSvr_FOCUS_TAG" || result.ContentType == "TableSvr_FOCUS_TAG_RW")
                            {
                                result.CopyVisibility = Visibility.Visible;
                                result.SendVisibility = LocalEnv.ConfInfo.IsFQ && AdminHandler.Instance.CheckPermisionById(CommonConstants.MODELID_5000, CommonConstants.CODE_5002) ? Visibility.Visible : Visibility.Collapsed;
                            }

                            result.LookVisibility = Visibility.Collapsed;
                            result.Content = result.Content.Replace("BEST_PRICE", "\uD83D\uDC4D");
                        }
                        else if (result.ContentType.ToUpper().Equals(AdminConsts.Admin_MM))
                        {
                            result.BtnShowTxt = "清除";
                            result.HeaderTitle = !string.IsNullOrEmpty(detail.title) ? string.Format("{0}", detail.title) : string.Empty;
                            result.KnowVisibility = Visibility.Visible;
                            result.LookVisibility = Visibility.Collapsed;
                            result.PopQuickDealVisibility = Visibility.Collapsed;
                            result.PopDealVisibility = Visibility.Collapsed;
                            result.DealVisibility = Visibility.Visible;
                            result.BtnDealShowTxt = "处理";
                            MMBreakAlarmModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<MMBreakAlarmModel>(result.Detail);
                            if (model != null)
                            {
                                if (model.passiveCount > 0)
                                {
                                    result.Content += $"\r\n被动关闭债券：{model.passiveCount} 只";
                                }
                                if (model.dangerCount > 0)
                                {
                                    result.Content += $"\r\n断报券危险：{model.dangerCount} 只";
                                }
                                if ("2".Equals(model.status))
                                {
                                    result.Content += $"\r\n做市状态：需关注";
                                }
                            }
                        }
                        else if (result.ContentType.Equals("TableSvr_ORDER_UNREF_Msg"))
                        {
                            //TableSvr Unref 消息提醒  //1 C 紧急  //2 A 警告  //3 N普通
                            if (result.Level == "3")
                            {
                                result.PopDealVisibility = Visibility.Collapsed;
                                result.DealVisibility = Visibility.Collapsed;
                            }
                            else
                            {
                                result.DealVisibility = Visibility.Visible;
                                result.BtnDealShowTxt = "查看";
                            }

                        }
                        else if (result.ContentType.Equals("BDB"))
                        {
                            result.BDBVisiblitity = Visibility.Visible;
                            result.KnowVisibility = Visibility.Collapsed;
                            result.PopDealVisibility = Visibility.Collapsed;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return result;
        }
        public static MsgPo HisSystemMsgToShowMsg(TTbookSystemMessage sysMsg)
        {
            MsgPo result = null;
            try
            {
                TMessageContent detail = MsgHelper.Deserialize<TMessageContent>(sysMsg.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                result = new MsgPo();
                result.MessageContent = detail;
                result.Level = detail.type;
                result.Id = sysMsg.id;//sysMsg.id;
                result.HeaderTitle = !string.IsNullOrEmpty(detail.title) ? string.Format("[{0}]", detail.title) : string.Empty;
                result.ContentType = detail.contentType;
                result.counterpartyid = detail.counterpartyid;
                result.counterpartyname = detail.counterpartyname;
                result.counterpartysubid = detail.counterpartysubid;
                result.counterpartysubname = detail.counterpartysubname;
                
                result.ReadFlag = sysMsg.readflag;

                result.Venue = detail.venue;
                result.Presult = detail.presult;
                result.Ptime = detail.ptime;
                result.Content = detail.content;
                if (!string.IsNullOrEmpty(detail.counterpartyname) && !string.IsNullOrEmpty(detail.counterpartysubname))
                    result.CloseBy = string.Format("{0}({1})", detail.counterpartyname, detail.counterpartysubname);
                else if (string.IsNullOrEmpty(detail.counterpartyname) && string.IsNullOrEmpty(detail.counterpartysubname))
                    result.CloseBy = string.Empty;
                else
                    result.CloseBy = string.IsNullOrEmpty(detail.counterpartyname) ? detail.counterpartysubname : detail.counterpartyname;


                result.ValueTime = detail.validtime;//"有效时间 3分20秒";
                if (!string.IsNullOrEmpty(sysMsg.updatetime))
                    result.UpdateTime = sysMsg.updatetime; //"10:32:21";
                result.createtime = sysMsg.createtime;
                if (string.IsNullOrEmpty(result.Status) || (detail.status + "").Equals(AdminConsts.Admin_Status_Two))
                {
                    result.StatusVisibility = Visibility.Collapsed;
                }
                else
                {
                    result.StatusVisibility = Visibility.Visible;
                }

                if (string.IsNullOrEmpty(result.ValueTime))
                {
                    result.ValueTimeVisibility = Visibility.Collapsed;
                }
                else
                {
                    result.ValueTimeVisibility = Visibility.Visible;

                }

                if (!string.IsNullOrEmpty(sysMsg.readflag) && sysMsg.readflag.Equals("1"))
                {
                    result.KnowVisibility = Visibility.Collapsed;
                }

                if (string.IsNullOrEmpty(result.CloseBy))
                {
                    result.CloseByHeight = 0;
                    result.CloseByVisibility = Visibility.Collapsed;
                    result.ShowHeight = 78;
                }
                else
                {
                    result.ShowHeight = 96;
                }

                if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("1"))
                {
                    result.DealVisibility = Visibility.Collapsed;
                    result.BtnShowTxt = "清除";
                }
                else if (!string.IsNullOrEmpty(sysMsg.type) && sysMsg.type.Equals("2"))
                {
                    result.BtnDealShowTxt = "处理";
                    result.BtnShowTxt = "忽略";
                    result.KnowVisibility = Visibility.Visible;
                    //result.DealVisibility = string.IsNullOrEmpty(detail.detail) ? Visibility.Collapsed : Visibility.Visible;
                    if (detail.content.EndsWith("中断") || detail.content.EndsWith("恢复"))
                    {
                        result.DealVisibility = result.PopDealVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        result.DealVisibility = result.PopDealVisibility = Visibility.Visible;
                    }
                    
                    if (result.ContentType.Equals(AdminConsts.Admin_ER))
                    {
                        if (!string.IsNullOrEmpty(detail.venue) && detail.venue.Equals("IDB"))// && detail.type.Equals("2"))
                            result.BtnDealShowTxt = "处理";
                        else
                            result.BtnDealShowTxt = "查看";


                    }
                }              

                result.MsgType = sysMsg.type;

                result.Detail = detail.detail;
                result.DelFlag = sysMsg.delflag;
                TMessageDetail detail1 = null;
                if (detail.detail!= null)
                    detail1 = MsgHelper.Deserialize<TMessageDetail>(detail.detail, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                TMessageOrder order = null;
                //if (detail1.order != null)
                //    order = MsgHelper.Deserialize<TMessageOrder>(detail1.order, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                result.source = STC.Common.Utils.CommonUtils.IsBDBAppName()? "BDB" : "STC";

                if (result.ContentType != null && !result.ContentType.Equals(""))
                {
                    if (result.ContentType.Equals("ER") && detail1 != null)
                    {
                        result.securityid = detail1.securityid;
                        result.strategyOrderId = detail1.strategyOrderId;
                    }
                    else if (result.ContentType.Equals("PM") && detail1.order != null)
                    {
                        result.securityid = detail1.order.symbol;
                        result.strategyOrderId = detail1.order.orderID;
                    }
                    else if (result.ContentType.Equals("IT") && detail1.order != null)
                    {
                        result.securityid = detail1.order.symbol;
                        result.strategyOrderId = detail1.order.orderID;
                    }
                    else if (result.ContentType.Equals("BDB"))
                    {
                        result.KnowVisibility = Visibility.Collapsed;
                    }
                    else if ((result.ContentType.Equals("TableSvr_FOCUS_TAG") || result.ContentType.Equals("TableSvr_FOCUS_TAG_RW")) 
                        && detail1 != null)
                    {
                        result.securityid = detail1.securityid;
                        result.strategyOrderId = detail1.strategyOrderId;
                    }
                    else if (result.ContentType.Equals("EV") && detail1 != null)
                    {
                        result.securityid = detail1.securityid;
                    }
                }
                //if (string.IsNullOrEmpty(result.ContentType))
                //    result.ContentType = AdminConsts.Admin_ExecutionReport;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return result;
        }
        public static void ChangeStyle(MsgPo msgPo, UserControl uc)
        {
            if (msgPo.Level.Equals("1"))
            {
                msgPo.ShowPopBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Transparent");
                msgPo.ValueTimeVisibility = Visibility.Visible;
                msgPo.ShowPopHeight = 88;

                msgPo.ContentTypeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.ContentBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.CloseByBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.UpdateTimeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");
                msgPo.StatusBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");
                msgPo.ValueTimeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");
            }
            else if (msgPo.Level.Equals("3"))
            {
                msgPo.ShowPopBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Transparent");

                msgPo.ShowPopHeight = 75;
                msgPo.CloseByHeight = 0;
                msgPo.CloseByVisibility = Visibility.Collapsed;

                msgPo.ContentTypeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.ContentBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.CloseByBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.UpdateTimeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");
                msgPo.StatusBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");
                msgPo.ValueTimeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");
            }
            else if (msgPo.Level.Equals("2"))
            {
                msgPo.ShowPopBG = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Transparent");

                msgPo.DealVisibility = Visibility.Collapsed;
                msgPo.StatusVisibility = Visibility.Collapsed;
                msgPo.ValueTimeVisibility = Visibility.Collapsed;
                msgPo.ShowPopHeight = 88;

                msgPo.ContentTypeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.ContentBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.CloseByBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.While");
                msgPo.UpdateTimeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");
                msgPo.StatusBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");
                msgPo.ValueTimeBrush = (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Gray14");

            }

            if (string.IsNullOrEmpty(msgPo.CloseBy))
            {
                msgPo.CloseByHeight = 0;
                msgPo.CloseByVisibility = Visibility.Collapsed;
                msgPo.ShowPopHeight = 75;
            }

            if (string.IsNullOrEmpty(msgPo.Status))
            {
                msgPo.StatusVisibility = Visibility.Collapsed;
            }
            if (string.IsNullOrEmpty(msgPo.ValueTime))
            {
                msgPo.ValueTimeVisibility = Visibility.Collapsed;
            }

            msgPo.KnowVisibility = Visibility.Visible;

        }

        public static MsgPo CopyMsgPo(MsgPo msgPo)
        {
            MsgPo result = new MsgPo();
            result.BorderName = msgPo.BorderName;
            result.BtnDealShowTxt = msgPo.BtnDealShowTxt;
            result.BtnShowTxt = msgPo.BtnShowTxt;
            result.CloseBy = msgPo.CloseBy;
            result.CloseByBrush = msgPo.CloseByBrush;
            result.CloseByHeight = msgPo.CloseByHeight;
            result.CloseByVisibility = msgPo.CloseByVisibility;
            result.Content = msgPo.Content;
            result.ContentBrush = msgPo.ContentBrush;
            result.ContentType = msgPo.ContentType;
            result.ContentTypeBrush = msgPo.ContentTypeBrush;
            result.counterpartyid = msgPo.counterpartyid;
            result.counterpartyname = msgPo.counterpartyname;
            result.counterpartysubid = msgPo.counterpartysubid;
            result.counterpartysubname = msgPo.counterpartysubname;
            result.createtime = msgPo.createtime;
            result.DealVisibility = msgPo.DealVisibility;
            result.DelFlag = msgPo.DelFlag;
            result.Detail = msgPo.Detail;
            result.HeaderTitle = msgPo.HeaderTitle;
            result.Id = msgPo.Id;
            result.KnowVisibility = msgPo.KnowVisibility;
            result.Level = msgPo.Level;
            result.MessageContent = msgPo.MessageContent;
            result.MsgType = msgPo.MsgType;
            result.ShowBorderBrush = msgPo.ShowBorderBrush;
            result.ShowBG = msgPo.ShowBG;
            result.ShowBorderThickness = msgPo.ShowBorderThickness;
            result.ShowHeight = msgPo.ShowHeight;
            result.ShowPopBG = msgPo.ShowPopBG;
            result.ShowPopHeight = msgPo.ShowPopHeight;
            result.ShowStatusBG = msgPo.ShowStatusBG;
            result.Status = msgPo.Status;
            result.StatusBrush = msgPo.StatusBrush;
            result.StatusVisibility = msgPo.StatusVisibility;
            result.StatusWidth = msgPo.StatusWidth;
            result.Storyboard = msgPo.Storyboard;
            result.UpdateTime = msgPo.UpdateTime;
            result.UpdateTimeBrush = msgPo.UpdateTimeBrush;
            result.ValueTime = msgPo.ValueTime;
            result.ValueTimeBrush = msgPo.ValueTimeBrush;
            result.ValueTimeVisibility = msgPo.ValueTimeVisibility;
            result.Venue = msgPo.Venue;

            result.PopDealVisibility = msgPo.PopDealVisibility;
            result.PopRefuseVisibility = msgPo.PopRefuseVisibility;
            result.KnowVisibility = msgPo.KnowVisibility;
            result.TimeOutTxtVisibility = msgPo.TimeOutTxtVisibility;
            result.UpdateTimeVisibility = msgPo.UpdateTimeVisibility;
            result.username = msgPo.username;
            result.ReadFlag = msgPo.ReadFlag;

            result.SignalDetail = msgPo.SignalDetail;
            result.ReadMsgVisible = msgPo.ReadMsgVisible;
            result.InnerColorAnimation = msgPo.InnerColorAnimation;
            result.groupname = msgPo.groupname;
            return result;
        }
    }
}
