using Ats.Foundation.Message;
using STC.Common.Api;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.Frame.CustomUI
{
    /// <summary>
    /// 对象转换器
    /// </summary>
    public class ObjectConvert
    {
        public static void UpdateMsg(MsgPo oldMsg, MsgPo newPo)
        {
            oldMsg.BorderName = newPo.BorderName;
            oldMsg.CloseBy = newPo.CloseBy;
            oldMsg.CloseByBrush = newPo.CloseByBrush;
            oldMsg.CloseByHeight = newPo.CloseByHeight;
            oldMsg.CloseByVisibility = newPo.CloseByVisibility;
            oldMsg.Content = newPo.Content;
            oldMsg.ContentBrush = newPo.ContentBrush;
            oldMsg.ContentType = newPo.ContentType;
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
            oldMsg.readFlag = newPo.readFlag;
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
        }

        public static SystemMessageReq MsgPoToSystemMessageReq(MsgPo msgPo, string msgType)
        {
            SystemMessageReq req = new SystemMessageReq();
            req.id = msgPo.Id;
            req.type = msgType;
            req.readflag = LocalEnv.UserInfo.UserId;
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
            content.title = !string.IsNullOrEmpty(msgPo.ContentType) ? msgPo.ContentType.Replace("[", "").Replace("]", "") : msgPo.ContentType;
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

            return req;
        }

        public static MsgPo SystemMsgToShowMsg(TTbookSystemMessage sysMsg, UserControl uc)
        {
            MsgPo result = null;
            try
            {
                TMessageContent detail = MsgHelper.Deserialize<TMessageContent>(sysMsg.content, Ats.Foundation.Message.Serializer.MsgSerializerType.Json);
                result = new MsgPo();
                result.Level = detail.type;
                result.Id = sysMsg.id;//sysMsg.id;
                result.ContentType = !string.IsNullOrEmpty(detail.title)?string.Format("[{0}]", detail.title):string.Empty;
               
                result.counterpartyid = detail.counterpartyid;
                result.counterpartyname = detail.counterpartyname;
                result.counterpartysubid = detail.counterpartysubid;
                result.counterpartysubname = detail.counterpartysubname;
                result.readFlag = sysMsg.readflag;

                result.Content = detail.content;
                if (!string.IsNullOrEmpty(detail.counterpartyname) && !string.IsNullOrEmpty(detail.counterpartysubname))
                    result.CloseBy = string.Format("{0}({1})", detail.counterpartyname, detail.counterpartysubname);
                else if (string.IsNullOrEmpty(detail.counterpartyname) && string.IsNullOrEmpty(detail.counterpartysubname))
                    result.CloseBy = string.Empty;
                else
                    result.CloseBy = string.IsNullOrEmpty(detail.counterpartyname) ? detail.counterpartysubname : detail.counterpartyname;


                result.ValueTime = detail.validtime;//"有效时间 3分20秒";
                if (!string.IsNullOrEmpty(sysMsg.createtime))
                    result.UpdateTime = sysMsg.createtime.Substring(11, 8); //"10:32:21";
                result.createtime = sysMsg.createtime;

                if ((detail.status + "").Equals(AdminConsts.Admin_Status_One))
                {
                    result.Status = AdminConsts.Admin_Status_Normal;
                    result.ShowStatusBG = (Brush)uc.FindResource("ColorBrush.Green14");
                    //result.StatusVisibility = Visibility.Visible;
                    result.DealVisibility = Visibility.Visible;
                }
                else if ((detail.status + "").Equals(AdminConsts.Admin_Status_Two))
                {
                    result.Status = AdminConsts.Admin_Status_Outtime;
                    result.DealVisibility = Visibility.Collapsed;
                    //result.StatusVisibility = Visibility.Collapsed;
                    result.ValueTimeVisibility = Visibility.Collapsed;
                }


                if (result.Level.Equals(AdminConsts.Admin_Critical))
                {
                    result.Level = "1";
                    result.ShowBG = (Brush)uc.FindResource("ColorBrush.Red8");
                    result.ValueTimeVisibility = Visibility.Visible;
                    result.ShowHeight = 96;
                    result.StatusWidth = 40;
                    result.ShowBorderBrush = (Brush)uc.FindResource("ColorBrush.Red7");


                }
                else if (result.Level.Equals(AdminConsts.Admin_Normal))
                {
                    result.Level = "3";
                    result.ShowBG = (Brush)uc.FindResource("ColorBrush.Gray13");

                    result.ShowHeight = 78;
                    result.StatusWidth = 40;
                    //result.CloseByHeight = 0;
                    //result.CloseByVisibility = Visibility.Collapsed;

                    result.ShowBorderBrush = (Brush)uc.FindResource("ColorBrush.Gray13");
                }
                else if (result.Level.Equals(AdminConsts.Admin_Warn))
                {
                    result.Level = "2";
                    result.ShowBG = (Brush)uc.FindResource("ColorBrush.Gray13");

                    result.DealVisibility = Visibility.Collapsed;
                    //result.StatusVisibility = Visibility.Collapsed;
                    //result.ValueTimeVisibility = Visibility.Collapsed;
                    result.ShowHeight = 96;
                    result.StatusWidth = 42;
                    //result.CloseByHeight = 0;
                    //result.CloseByVisibility = Visibility.Collapsed;

                    result.ShowBorderBrush = (Brush)uc.FindResource("ColorBrush.Orange5");
                }

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

                if (!string.IsNullOrEmpty(sysMsg.readflag) && sysMsg.readflag.Contains(LocalEnv.UserInfo.UserId))
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
                    result.DealVisibility = Visibility.Collapsed;

                result.MsgType = sysMsg.type;

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
                msgPo.ShowPopBG = (Brush)uc.FindResource("ColorBrush.Transparent");
                msgPo.ValueTimeVisibility = Visibility.Visible;
                msgPo.ShowPopHeight = 88;

                msgPo.ContentTypeBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.ContentBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.CloseByBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.UpdateTimeBrush = (Brush)uc.FindResource("ColorBrush.Gray14");
                msgPo.StatusBrush = (Brush)uc.FindResource("ColorBrush.Gray14");
                msgPo.ValueTimeBrush = (Brush)uc.FindResource("ColorBrush.Gray14");
            }
            else if (msgPo.Level.Equals("3"))
            {
                msgPo.ShowPopBG = (Brush)uc.FindResource("ColorBrush.Transparent");

                msgPo.ShowPopHeight = 75;
                msgPo.CloseByHeight = 0;
                msgPo.CloseByVisibility = Visibility.Collapsed;

                msgPo.ContentTypeBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.ContentBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.CloseByBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.UpdateTimeBrush = (Brush)uc.FindResource("ColorBrush.Gray14");
                msgPo.StatusBrush = (Brush)uc.FindResource("ColorBrush.Gray14");
                msgPo.ValueTimeBrush = (Brush)uc.FindResource("ColorBrush.Gray14");
            }
            else if (msgPo.Level.Equals("2"))
            {
                msgPo.ShowPopBG = (Brush)uc.FindResource("ColorBrush.Transparent");

                msgPo.DealVisibility = Visibility.Collapsed;
                msgPo.StatusVisibility = Visibility.Collapsed;
                msgPo.ValueTimeVisibility = Visibility.Collapsed;
                msgPo.ShowPopHeight = 88;

                msgPo.ContentTypeBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.ContentBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.CloseByBrush = (Brush)uc.FindResource("ColorBrush.While");
                msgPo.UpdateTimeBrush = (Brush)uc.FindResource("ColorBrush.Gray14");
                msgPo.StatusBrush = (Brush)uc.FindResource("ColorBrush.Gray14");
                msgPo.ValueTimeBrush = (Brush)uc.FindResource("ColorBrush.Gray14");

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

        }
    }
}
