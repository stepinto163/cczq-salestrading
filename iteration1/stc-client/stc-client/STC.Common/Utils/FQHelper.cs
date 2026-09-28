using com.gateway.log;
using STC.Common.Handler.QQ;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class FQHelper
    {

        /// <summary>
        /// 点击成交价
        /// </summary>
        /// <param name="side"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="securityId"></param>
        /// <param name="symbolName"></param>
        /// <param name="qq"></param>
        /// <param name="comp_id"></param>
        /// <param name="exePrice"></param>
        /// <param name="settlType"></param>
        public static void SendHttpQQ(string side, string type, string content, string securityId, string symbolName, string qq, string comp_id, string exePrice, string settlType,string val_yield,string lastupdatetime)
        {
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            string[,] vol_delivery = null;
            string side_txt = side == "1" ? "bid" : "ofr";
            string reserve_side = side == "1" ? "ofr" : "bid";
            string bond_type = string.Empty;
            if (bondsInfo != null)
            {
                bond_type = bondsInfo.SECURITY_TYPE;
            }

            string join = string.Empty;
            if (type == "1")
            {
                join = $"join_{side_txt}";
            }
            else if (type == "2")
            {
                join = $"join_{reserve_side}";
            }


            //Task.Run(() =>
            //{
                SendFinQFriendReq qqsmm = new SendFinQFriendReq();
                qqsmm.content = content;
                qqsmm.dataType = 2;
                qqsmm.data = new STC.Common.Model.QQ.DataMessage();

                qqsmm.data.id = Guid.NewGuid().ToString("N");
                qqsmm.data.comp_id = comp_id;
                qqsmm.data.code = securityId;
                qqsmm.data.bond_type = bond_type;
                qqsmm.data.name = symbolName;
                qqsmm.data.side = side_txt;
                qqsmm.data.join = join;
                qqsmm.data.price = (exePrice == CommonConstants.NO_PRICE || exePrice == CommonConstants.NO_BROKER) ? string.Empty : exePrice;
                qqsmm.data.vol_delivery = vol_delivery;
                qqsmm.data.price_type = "3";
                qqsmm.data.settl_type = ResulfApiHelper.ConvertSettleType(settlType);
                qqsmm.data.val_yield = val_yield;
                qqsmm.fromQB = LocalEnv.UserInfo.UserName;
                qqsmm.openChat = true;
                qqsmm.fromQBAccountID = LocalEnv.UserInfo.UserId;


                //如果使用WebFinQ发送，则调用WebFinQ接口，否则调用QQ接口
                if (LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
                {
                    qqsmm.UpdateTime = DateTimeEx.ConvertStringToDateTime(lastupdatetime).ToLongTimeString();
                    AppUtils.Application.PluginManager.Application.BroadCast("ConvertSendFinQFriendReqToWebFinQ", qqsmm);
                }
                else
                {
                    PResult result = QQHandler.Instance.SendQQ(qqsmm);
                    if (result != null && result.IsSuccess())
                    {
                        CommonUtils.Prompt2Second("金融Q", "已成功推送至金融Q");
                    }
                    else
                    {
                        CommonUtils.Prompt2Second("金融Q", (result == null || string.IsNullOrEmpty(result.Msg)) ? "发送金融Q失败" : result.Msg);
                    }
                }
            //});

            //string msg = ResulfApiHelper.GenMsg(content, securityId, symbolName, side_txt, join, px, vol_delivery, qq);
            //LogHelper.Debug(msg);
            //await ResulfApiHelper.SendMessageToQQ(msg);
        }

        /// <summary>
        /// 生成FQ单渠道数据
        /// </summary>
        /// <param name="side"></param>
        /// <param name="type"></param>
        /// <param name="sourceNumber"></param>
        /// <param name="securityId"></param>
        /// <param name="marketText"></param>
        /// <param name="settleType"></param>
        /// <param name="px"></param>
        /// <param name="qty"></param>
        /// <returns></returns>
        public static STC.Common.Model.QQ.DataMessage GenDataMessage(string side, string type, string sourceNumber, string securityId, string marketText,
            string settleType, string px, string qty)
        {
            string[,] vol_delivery = null;
            string side_txt = side == "1" ? "bid" : "ofr";
            string reserve_side = side == "1" ? "ofr" : "bid";
            //string source = ComboSourceUtil.Instance.FindEnum(this.BrokerCN, "venues_cn_tonumber");
            string source = sourceNumber;
            string join = string.Empty;
            settleType = settleType.Replace("[", string.Empty).Replace("]", string.Empty);

            if (type == "1")
            {
                join = $"join_{side_txt}";
            }
            else if (type == "2")
            {
                join = $"join_{reserve_side}";
            }

           

            //string securityId = this.Security;
            string symbolName = string.Empty;
            string bond_type = string.Empty;
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            if (bondsInfo != null)
            {
                symbolName = bondsInfo.Short_Name;
                bond_type = bondsInfo.SECURITY_TYPE;
            }

            //if (side == "1")
            //{
            //    marketText = this.BidCornerMarketText;
            //    settleType = this.BidSettleType;
            //    px = this.Bid;
            //    qty = this.BidAmount;
            //}
            //else
            //{
            //    marketText = this.OfrCornerMarketText;
            //    settleType = this.OfrSettleType;
            //    px = this.Ofr;
            //    qty = this.OfrAmount;
            //}
            if (!string.IsNullOrEmpty(marketText))
            {
                string output = Regex.Replace(marketText, @"\(.*?\)", string.Empty);
                output = output.Replace("[", string.Empty).Replace("]", string.Empty);
                string[] splitArr = output.Split(CommonConstants.PLUS_CHAR);
                string[] splitArr1 = settleType.Split(CommonConstants.COLON_CHAR);
                if (splitArr.Length > 0 && splitArr.Length == splitArr1.Length)
                {
                    vol_delivery = new string[splitArr.Length, 2];
                    for (int i = 0; i < splitArr.Length; i++)
                    {
                        vol_delivery[i, 0] = splitArr[i];
                        vol_delivery[i, 1] = ResulfApiHelper.ConvertSettleType(splitArr1[i]);
                    }
                }
            }
            else
            {
                vol_delivery = new string[1, 2];
                vol_delivery[0, 0] = qty;
                vol_delivery[0, 1] = ResulfApiHelper.ConvertSettleType(settleType);
            }

            STC.Common.Model.QQ.DataMessage dm = new STC.Common.Model.QQ.DataMessage();
            dm.id = Guid.NewGuid().ToString("N");
            dm.comp_id = source;
            dm.code = securityId;
            dm.name = symbolName;
            dm.side = side_txt;
            dm.join = join;
            dm.price = (px == CommonConstants.NO_PRICE || px == CommonConstants.NO_BROKER) ? string.Empty : px;
            dm.vol_delivery = vol_delivery;
            dm.bond_type = bond_type;
            dm.price_type = "3";
            return dm;
        }


        /// <summary>
        /// 最优
        /// </summary>
        /// <param name="content"></param>
        /// <param name="data"></param>
        public static void SendHttpQQ_Best(string content, List<STC.Common.Model.QQ.DataMessage> data)
        {
            SendFinQFriendReq_Best qqsmm = new SendFinQFriendReq_Best();
            qqsmm.content = content;
            qqsmm.fromQB = LocalEnv.UserInfo.UserName;
            qqsmm.data = data;
            qqsmm.openChat = true;
            qqsmm.dataType = 1;
            qqsmm.fromQBAccountID = LocalEnv.UserInfo.UserId;

            PResult result = QQHandler.Instance.SendQQ_Best(qqsmm);
            if (result != null && result.IsSuccess())
            {
                CommonUtils.Prompt2Second("金融Q", "已成功推送至金融Q");
            }
            else
            {
                CommonUtils.Prompt2Second("金融Q", (result == null || string.IsNullOrEmpty(result.Msg)) ? "发送金融Q失败" : result.Msg);
            }
        }
        
        /// <summary>
        /// 单渠道
        /// </summary>
        /// <param name="side"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="sourceNumber"></param>
        /// <param name="securityId"></param>
        /// <param name="marketText"></param>
        /// <param name="settleType"></param>
        /// <param name="px"></param>
        /// <param name="qty"></param>
        public static void SendHttpQQ_Broker(string side, string type, string content, string sourceNumber, string securityId, string marketText,
            string settleType, string px, string qty)
        {
            SendFinQFriendReq qqsmm = new SendFinQFriendReq();
            qqsmm.content = content;
            qqsmm.data = GenDataMessage(side, type, sourceNumber, securityId, marketText, settleType, px, qty);
            qqsmm.fromQB = LocalEnv.UserInfo.UserName;
            qqsmm.openChat = true;
            qqsmm.fromQBAccountID = LocalEnv.UserInfo.UserId;

            PResult result = QQHandler.Instance.SendQQ(qqsmm);
            if (result != null && result.IsSuccess())
            {
                CommonUtils.Prompt2Second("金融Q", "已成功推送至金融Q");
            }
            else
            {
                CommonUtils.Prompt2Second("金融Q", (result == null || string.IsNullOrEmpty(result.Msg)) ? "发送金融Q失败" : result.Msg);
            }
        }

        /// <summary>
        /// 获取 vol_delivery
        /// </summary>
        /// <param name="marketText"></param>
        /// <param name="settleType"></param>
        /// <param name="qty"></param>
        /// <returns></returns>
        public static string[,] GetVolDelivery(string marketText, string settleType, string qty)
        {
            string[,] vol_delivery = null;
            if (!string.IsNullOrEmpty(marketText))
            {
                string output = Regex.Replace(marketText, @"\(.*?\)", string.Empty);
                string[] splitArr = output.Split(CommonConstants.PLUS_CHAR);
                string[] splitArr1 = settleType.Split(CommonConstants.COLON_CHAR);
                if (splitArr.Length > 0 && splitArr.Length == splitArr1.Length)
                {
                    vol_delivery = new string[splitArr.Length, 2];
                    for (int i = 0; i < splitArr.Length; i++)
                    {
                        vol_delivery[i, 0] = splitArr[i];
                        vol_delivery[i, 1] = ResulfApiHelper.ConvertSettleType(splitArr1[i]);
                    }
                }
            }
            else
            {
                vol_delivery = new string[1, 2];
                vol_delivery[0, 0] = qty;
                vol_delivery[0, 1] = ResulfApiHelper.ConvertSettleType(settleType);
            }

            return vol_delivery ?? new string[0, 0];
        }


        /// <summary>
        /// 获取 QB vol_delivery
        /// </summary>
        /// <param name="marketText"></param>
        /// <param name="settleType"></param>
        /// <param name="qty"></param>
        /// <returns></returns>
        public static List<List<string>> GetQBVolDelivery(string marketText, string settleType, string qty)
        {
            List<List<string>> list = new List<List<string>>();

            string[,] vol_delivery = null;
            try
            {
                if (!string.IsNullOrEmpty(marketText))
                {
                    string output = Regex.Replace(marketText, @"\(.*?\)", string.Empty);
                    string[] splitArr = output.Split('+');
                    string[] splitArr1 = settleType.Split(':');
                    
                    if (splitArr.Length > 0 && splitArr.Length == splitArr1.Length)
                    {
                        vol_delivery = new string[splitArr.Length, 2];
                        for (int i = 0; i < splitArr.Length; i++)
                        {
                            vol_delivery[i, 0] = splitArr[i];
                            vol_delivery[i, 1] = ResulfApiHelper.ConvertSettleType(splitArr1[i]);
                            if (!string.IsNullOrWhiteSpace(vol_delivery[i, 1]))
                            {
                                if (int.Parse(vol_delivery[i, 1]) >= 4)
                                {
                                    List<string> sublist = new List<string>() { vol_delivery[i, 0], vol_delivery[i, 1], splitArr1[i] };
                                    list.Add(sublist);
                                }
                            }
                            else
                            {
                                List<string> sublist = new List<string>() { vol_delivery[i, 0], vol_delivery[i, 1] };
                                list.Add(sublist);
                            }
                        }
                    }
                }
                else
                {
                    
                    vol_delivery = new string[1, 2];
                    vol_delivery[0, 0] = qty;
                    vol_delivery[0, 1] = ResulfApiHelper.ConvertSettleType(settleType);
                    if (!string.IsNullOrWhiteSpace(vol_delivery[0, 1]))
                    {
                        if (int.Parse(vol_delivery[0, 1]) >= 4)
                        {
                            List<string> sublist = new List<string>() { vol_delivery[0, 0], vol_delivery[0, 1], settleType };
                            list.Add(sublist);
                        }
                    }
                    else
                    {
                        List<string> sublist = new List<string>() { vol_delivery[0, 0], vol_delivery[0, 1] };
                        list.Add(sublist);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }

            return list;
        }

        /// <summary>
        /// 获取 QB vol_delivery
        /// </summary>
        /// <param name="listMD"></param>
        /// <returns></returns>
        public static List<List<string>> GetQBVolDeliveryFromMD(List<GNoMDEntries> listMD)
        {
            List<List<string>> list = new List<List<string>>();
            try
            {
                for (int i = 0; i < listMD.Count; i++)
                {
                    GNoMDEntries item = listMD[i];
                    decimal itemqty = CommonUtils.ConvertToDecimal(item.MDEntrySize);
                    string itemqtyamount = itemqty == 0 ? CommonConstants.NO_PRICE : itemqty.ToString();
                    string itemsettl = item.SettlType;
                    string itemsettl_convet = ResulfApiHelper.ConvertSettleType(itemsettl);
                    if (!string.IsNullOrWhiteSpace(itemsettl_convet))
                    {
                        if (int.Parse(itemsettl_convet) >= 4)
                        {
                            List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet, itemsettl };
                            list.Add(sublist);
                        }
                        else
                        {
                            List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet };
                            list.Add(sublist);
                        }
                    }
                    else
                    {
                        List<string> sublist = new List<string>() { itemqtyamount, itemsettl_convet };
                        list.Add(sublist);
                    }
                }
            }
            catch (Exception  ex)
            {
                LogHelper.Error(ex.Message);
            }

            return list;
        }

    }
}

