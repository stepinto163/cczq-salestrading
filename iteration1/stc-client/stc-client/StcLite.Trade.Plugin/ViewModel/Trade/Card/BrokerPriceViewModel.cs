using com.gateway.log;
using STC.Common.Handler.QQ;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.QQ;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BDB.Trade.Plugin.ViewModel.Trade.Card
{
    public class BrokerPriceViewModel : BaseViewModel
    {
        #region 关联属性

        private int _brokerBlockWidth = 28;
        public int BrokerBlockWidth
        {
            get { return _brokerBlockWidth; }
            set { _brokerBlockWidth = value; this.OnPropertyChanged(); }
        }

        private Brush _brokerBlockBG;
        public Brush BrokerBlockBG
        {
            get { return _brokerBlockBG; }
            set { _brokerBlockBG = value; this.OnPropertyChanged(); }
        }

        private Brush _brokerBlockFG;
        public Brush BrokerBlockFG
        {
            get { return _brokerBlockFG; }
            set { _brokerBlockFG = value; this.OnPropertyChanged(); }
        }

        private Brush _brokerBlockHoverBG;
        public Brush BrokerBlockHoverBG
        {
            get { return _brokerBlockHoverBG; }
            set { _brokerBlockHoverBG = value; this.OnPropertyChanged(); }
        }


        private string _brokerCN = string.Empty;
        public string BrokerCN
        {
            get { return _brokerCN; }
            set { _brokerCN = value; this.OnPropertyChanged(); }
        }

        private int _brokerIndex;
        public int BrokerIndex
        {
            get { return _brokerIndex; }
            set { _brokerIndex = value; this.OnPropertyChanged(); }
        }

        private string _security;
        public string Security
        {
            get { return _security; }
            set { _security = value; this.OnPropertyChanged(); }
        }


        private string _showSettleType;
        public string ShowSettleType
        {
            get { return _showSettleType; }
            set { _showSettleType = value; this.OnPropertyChanged(); }
        }

        private Visibility _headVisibility = Visibility.Collapsed;
        public Visibility HeadVisibility
        {
            get { return _headVisibility; }
            set { _headVisibility = value; this.OnPropertyChanged(); }
        }

        private int _settleColWidth = 0;
        public int SettleColWidth { get { return _settleColWidth; } set { _settleColWidth = value; this.OnPropertyChanged(); } }

        private string _settleType;
        public string SettleType { get { return _settleType; } set { _settleType = value; this.OnPropertyChanged(); } }

        private string _bidSettleType;
        public string BidSettleType { get { return _bidSettleType; } set { _bidSettleType = value; this.OnPropertyChanged(); } }

        private string _ofrSettleType;
        public string OfrSettleType { get { return _ofrSettleType; } set { _ofrSettleType = value; this.OnPropertyChanged(); } }

        private string _settleFwdType;
        public string SettleFwdType { get { return _settleFwdType; } set { _settleFwdType = value; this.OnPropertyChanged(); } }

        
        private string _bidpricetype;
        public string BidPriceType { get { return _bidpricetype; } set { _bidpricetype = value; this.OnPropertyChanged(); } }

        private string _ofrpricetype;
        public string OfrPriceType { get { return _ofrpricetype; } set { _ofrpricetype = value; this.OnPropertyChanged(); } }

        private Brush _bidBgBrush;
        public Brush BidBgBrush
        {
            get { return _bidBgBrush; }
            set { _bidBgBrush = value; this.OnPropertyChanged(); }
        }

        private bool _isBidPermission = false;
        public bool IsBidPermission
        {
            get { return _isBidPermission; }
            set { _isBidPermission = value; this.OnPropertyChanged(); }
        }
        
        private string _bid = CommonConstants.NO_PRICE;
        public string Bid { get { return _bid; } set { _bid = value; this.OnPropertyChanged(); } }


        private string _bidAmount = CommonConstants.NO_PRICE;
        public string BidAmount { get { return _bidAmount; } set { _bidAmount = value; this.OnPropertyChanged(); } }


        private Brush _ofrBgBrush;
        public Brush OfrBgBrush
        {
            get { return _ofrBgBrush; }
            set { _ofrBgBrush = value; this.OnPropertyChanged(); }
        }

        private bool _isOfrPermission = false;
        public bool IsOfrPermission
        {
            get { return _isOfrPermission; }
            set { _isOfrPermission = value; this.OnPropertyChanged(); }
        }

        private string _ofr = CommonConstants.NO_PRICE; 
        public string Ofr { get { return _ofr; } set { _ofr = value; this.OnPropertyChanged(); } }


        private string _ofrAmount = CommonConstants.NO_PRICE;
        public string OfrAmount { get { return _ofrAmount; } set { _ofrAmount = value; this.OnPropertyChanged(); } }

        private Visibility _bidCornerMarket = Visibility.Collapsed;
        public Visibility BidCornerMarket
        {
            get { return _bidCornerMarket; }
            set { _bidCornerMarket = value; this.OnPropertyChanged(); }
        }

        private string _bidCornerMarketText;
        public string BidCornerMarketText { get { return _bidCornerMarketText; } set { _bidCornerMarketText = value; this.OnPropertyChanged(); } }

        private Visibility _bidLineVisibility;

        public Visibility BidLineVisibility { get { return _bidLineVisibility; } set { _bidLineVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _bidStarVisibility;

        public Visibility BidStarVisibility { get { return _bidStarVisibility; } set { _bidStarVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _bidPStarVisibility;

        public Visibility BidPStarVisibility { get { return _bidPStarVisibility; } set { _bidPStarVisibility = value; this.OnPropertyChanged(); } }

        private decimal _bidT1Qty;

        public decimal BidT1Qty
        {
            get { return _bidT1Qty; }
            set { 
                _bidT1Qty = value; 
                this.OnPropertyChanged(); }
        }

        private Visibility _ofrCornerMarket = Visibility.Collapsed;
        public Visibility OfrCornerMarket
        {
            get { return _ofrCornerMarket; }
            set { _ofrCornerMarket = value; this.OnPropertyChanged(); }
        }

        private string _ofrCornerMarketText;
        public string OfrCornerMarketText { get { return _ofrCornerMarketText; } set { _ofrCornerMarketText = value; this.OnPropertyChanged(); } }

        private Visibility _ofrLineVisibility;

        public Visibility OfrLineVisibility { get { return _ofrLineVisibility; } set { _ofrLineVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _ofrStarVisibility;

        public Visibility OfrStarVisibility { get { return _ofrStarVisibility; } set { _ofrStarVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _ofrPStarVisibility;

        public Visibility OfrPStarVisibility { get { return _ofrPStarVisibility; } set { _ofrPStarVisibility = value; this.OnPropertyChanged(); } }

        private decimal _ofrT1Qty;

        public decimal OfrT1Qty
        {
            get { return _ofrT1Qty; }
            set
            {
                _ofrT1Qty = value;
                this.OnPropertyChanged();
            }
        }

        private string _updateTime;
        public string UpdateTime { get { return _updateTime; } set { _updateTime = value; this.OnPropertyChanged(); } }


        private string _bidBargainFlag;
        public string BidBargainFlag { get { return _bidBargainFlag; } set { _bidBargainFlag = value; this.OnPropertyChanged(); } }

        private string _ofrBargainFlag;
        public string OfrBargainFlag { get { return _ofrBargainFlag; } set { _ofrBargainFlag = value; this.OnPropertyChanged(); } }
        


        private Visibility _bidSettleVisibility = Visibility.Collapsed;
        public Visibility BidSettleVisibility
        {
            get { return _bidSettleVisibility; }
            set { _bidSettleVisibility = value; this.OnPropertyChanged(); }
        }

        private string _bidSettleTip;
        public string BidSettleTip { get { return _bidSettleTip; } set { _bidSettleTip = value; this.OnPropertyChanged(); } }

        private Visibility _ofrSettleVisibility = Visibility.Collapsed;
        public Visibility OfrSettleVisibility
        {
            get { return _ofrSettleVisibility; }
            set { _ofrSettleVisibility = value; this.OnPropertyChanged(); }
        }

        private string _ofrSettleTip;
        public string OfrSettleTip { get { return _ofrSettleTip; } set { _ofrSettleTip = value; this.OnPropertyChanged(); } }

        private Visibility _bidPCornerMarket = Visibility.Collapsed;
        public Visibility BidPCornerMarket
        {
            get { return _bidPCornerMarket; }
            set { 
                _bidPCornerMarket = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _ofrPCornerMarket = Visibility.Collapsed;
        public Visibility OfrPCornerMarket
        {
            get { return _ofrPCornerMarket; }
            set { 
                _ofrPCornerMarket = value;
                this.OnPropertyChanged(); 
            }
        }

        private string _BidMDEntryID;
        public string BidMDEntryID
        {
            get { return _BidMDEntryID; }
            set { _BidMDEntryID = value; this.OnPropertyChanged(); }
        }

        private string _OfrMDEntryID;
        public string OfrMDEntryID
        {
            get { return _OfrMDEntryID; }
            set { _OfrMDEntryID = value; this.OnPropertyChanged(); }
        }

        public List<GNoMDEntries> BidMD = new List<GNoMDEntries>();
        public List<GNoMDEntries> OfrMD = new List<GNoMDEntries>();

        private PriceModel _priceModel;

        public PriceModel PriceModel { get { return _priceModel; } set { _priceModel = value; } }
        #endregion

        public void OnClear(string emptyStr)
        {
            Bid = Ofr = BidAmount = OfrAmount = emptyStr;// CommonConstants.NO_PRICE;
            BidBargainFlag = OfrBargainFlag = BidCornerMarketText = OfrCornerMarketText = string.Empty;
            BidCornerMarket = OfrCornerMarket = BidPCornerMarket = OfrPCornerMarket = BidStarVisibility = BidPStarVisibility = BidLineVisibility = OfrStarVisibility = OfrPStarVisibility = OfrLineVisibility = Visibility.Collapsed;
            OfrBgBrush = BidBgBrush = new SolidColorBrush(Colors.Transparent);
            IsBidPermission = IsOfrPermission = false;
            BidSettleType = OfrSettleType = string.Empty;
            BidPriceType = OfrPriceType = string.Empty;
            BidMDEntryID = string.Empty;
            OfrMDEntryID = string.Empty;
            BidSettleVisibility = OfrSettleVisibility = Visibility.Collapsed;
            BidMD.Clear();
            OfrMD.Clear();
            if (string.IsNullOrEmpty(emptyStr))
            {
                BrokerBlockWidth = 0;
            }
            else
            {
                BrokerBlockWidth = 26;
            }
        }


        public void Clear(string emptyStr)
        {
            HeadVisibility = Visibility.Collapsed;

            OnClear(emptyStr);

            SettleColWidth = 0;

            if (string.IsNullOrEmpty(emptyStr))
            {
                BrokerBlockWidth = 0;
            }
            else
            {
                BrokerBlockWidth = 26;
            }
        }

        public void ShowSettle(string settle)
        {
            HeadVisibility = Visibility.Visible;
            ShowSettleType = settle;

            SettleColWidth = 30;

            BrokerBlockWidth = 0;
        }


        #region 复制或发送QQ

        public void CopyPointOrderData(string side, string type = null)
        {
            List<dynamic> paramList = new List<dynamic>();
            dynamic d = new System.Dynamic.ExpandoObject();
            d.side = side;
            d.type = type;
            d.isNeedSource = HeadVisibility == Visibility.Collapsed;
            d.isBestInvoke = false;
            d.html = string.Empty;
            d.plainTxt = string.Empty;
            paramList.Add(d);
            Sub_CopyPointOrderData(paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="side"></param>
        /// <param name="type">1:join 2:报对价</param>
        public void Sub_CopyPointOrderData(List<dynamic> paramList)
        {
            if (paramList.Count == 0 || string.IsNullOrEmpty(this.PriceModel.Security) || this.PriceModel.Security.ToLower() == "tradebook")
                return;

            dynamic condition = paramList[0];


            string html;
            string plainTxt;
            string html_onlyCopy;
            string plainTxt_onlyCopy;

            string html_double;
            string plainTxt_double;
            GetDoubleText(out html_double, out plainTxt_double, condition.isNeedSource);

            BrokerPriceViewModel brokerPriceViewModel = this;

            string s = string.Empty;
            string qty = string.Empty;
            string px = string.Empty;

            string securityId = brokerPriceViewModel.Security;
            string symbolName = string.Empty;
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            if (bondsInfo != null)
            {
                symbolName = bondsInfo.Short_Name;
            }

            if (condition.side == "1")//GVN
            {
                s = "GVN";

                if (brokerPriceViewModel.BidAmount != "- -" && brokerPriceViewModel.BidAmount != "***")
                {
                    if (brokerPriceViewModel.BidT1Qty > 0)
                    {
                        qty = brokerPriceViewModel.BidT1Qty.ToString();
                    }
                }
                if (brokerPriceViewModel.Bid != "- -" && brokerPriceViewModel.Bid != "***")
                {
                    px = brokerPriceViewModel.Bid;
                }
            }
            else if (condition.side == "2")//TKN
            {
                s = "TKN";

                if (brokerPriceViewModel.OfrAmount != "- -" && brokerPriceViewModel.OfrAmount != "***")
                {
                    if (brokerPriceViewModel.OfrT1Qty > 0)
                    {
                        qty = brokerPriceViewModel.OfrT1Qty.ToString();
                    }
                }

                if (brokerPriceViewModel.Ofr != "- -" && brokerPriceViewModel.Ofr != "***")
                {
                    px = brokerPriceViewModel.Ofr;
                }
            }

            //html.Append($"<p><span style=\"color: {color}\">{s}</span>&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{qty}&#9;{settle}</p>");

            //html.Append($"<p>{s}&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{settle}&#9;{qty}</p>");

            //plainTxt.Append($"{s} {securityId} {symbolName} {px} {settle} {qty}");

            //if (settle.Length == 0)
            //{
            //string htmlSettle = string.Empty;
            //string txtSettle = string.Empty;
            //if (settle.Length > 0)
            //{
            //    htmlSettle = $"&#9;{settle}";
            //    txtSettle = $" {settle}";
            //}

            string htmlPx = string.Empty;
            string txtPx = string.Empty;
            if (px.Length > 0)
            {
                htmlPx = $"&#9;{px}";
                txtPx = $" {px}";
            }

            string htmlQty = string.Empty;
            string txtQty = string.Empty;
            if (qty.Length > 0)
            {
                htmlQty = $"&#9;{qty}w";
                txtQty = $" {qty}w";
            }



            //}
            //else
            //{
            //    string bidQty = brokerPriceViewModel.BidCornerMarketText.Length > 0 ? brokerPriceViewModel.BidCornerMarketText : brokerPriceViewModel.BidAmount;
            //    string bidPx = brokerPriceViewModel.Bid;
            //    string bidSettle = brokerPriceViewModel.BidSettleTip;

            //    string ofrQty = brokerPriceViewModel.OfrCornerMarketText.Length > 0 ? brokerPriceViewModel.OfrCornerMarketText : brokerPriceViewModel.OfrAmount;
            //    string ofrPx = brokerPriceViewModel.Ofr;
            //    string ofrSettle = brokerPriceViewModel.OfrSettleTip;


            //    string v = brokerPriceViewModel.BrokerCN;//index venues_cn
            //    string t = string.Empty;

            //    // todo 时间？？？

            //    //html.Append($"<p>{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQty}/{ofrQty}&#9;{v}&#9;{t}</p>");

            //    //plainTxt.Append($"{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQty}/{ofrQty}    {v}    {t}");

            //    html.Append($"<p>[{securityId}&#9;{bidPx}/{ofrPx}&#9;{bidQty}/{ofrQty}&#9;{v}],    {s} {securityId} {px}</p>");

            //    plainTxt.Append($"[{securityId}    {bidPx}/{ofrPx}    {bidQty}/{ofrQty}    {v}],    {s} {securityId} {px}");
            //}
            html = $"<p>[{html_double}],<\br>{s}&#9;{securityId}&#9;{symbolName}{htmlPx}{htmlQty}</p>";
            plainTxt = $"[{plainTxt_double}],\r\n{s} {securityId} {symbolName}{txtPx}{txtQty}";
            html_onlyCopy = $"<p>{s}&#9;{securityId}&#9;{symbolName}{htmlPx}{htmlQty}</p>";
            plainTxt_onlyCopy = $"{s} {securityId} {symbolName}{txtPx}{txtQty}";

            condition.html = html;
            condition.plainTxt = plainTxt;
            condition.html_onlyCopy = html_onlyCopy;
            condition.plainTxt_onlyCopy = plainTxt_onlyCopy;
            condition.Qty = qty;

            if (condition.isBestInvoke == false)
            {
                //复制
                ClipboardHelper.CopyToClipboard(html_onlyCopy, plainTxt_onlyCopy);
                if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                {
                    SendHttpQQ(condition.side, condition.type, plainTxt.ToString(), string.Empty, brokerPriceViewModel);
                    //await ResulfApiHelper.ManualSendMessageToQQRequest(qq, plainTxt.ToString());
                }
                else
                {
                    CommonUtils.Prompt2Second("复制", "复制成功");
                }
            }
        }

        public async Task<STC.Common.Model.QQ.DataMessage> GenDataMessage(string side, string type)
        {
            STC.Common.Model.QQ.DataMessage dm = new STC.Common.Model.QQ.DataMessage();
            try
            {
                string[,] vol_delivery = null;

                string[,] opp_vol_delivery = null;
                List<List<string>> vol_delivery_qb = null;
                string opp_price = string.Empty;
                string opp_price_type = string.Empty;
                string opp_yield_type = string.Empty;


                string side_txt = side == "1" ? "bid" : "ofr";
                string reserve_side = side == "1" ? "ofr" : "bid";
                string source = ComboSourceUtil.Instance.FindEnum(this.BrokerCN, "venues_cn_tonumber");
                string join = string.Empty;
                string marketText;
                string settleType;

                if (type == "1")
                {
                    join = $"join_{side_txt}";
                }
                else if (type == "2")
                {
                    join = $"join_{reserve_side}";
                }

                string px;
                string qty;

                string securityId = this.Security;
                string symbolName = string.Empty;
                string bond_type = string.Empty;
                BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
                if (bondsInfo != null)
                {
                    symbolName = bondsInfo.Short_Name;
                    bond_type = bondsInfo.SECURITY_TYPE;

                }

                if (side == "1")
                {
                    marketText = this.BidCornerMarketText;
                    settleType = this.BidSettleType;
                    px = this.Bid;
                    qty = this.BidAmount;
                    opp_vol_delivery = STC.Common.Utils.FQHelper.GetVolDelivery(this.OfrCornerMarketText, this.OfrSettleType, this.OfrAmount);
                    opp_price = this.Ofr;
                    opp_price_type = OfrPriceType;
                }
                else
                {
                    marketText = this.OfrCornerMarketText;
                    settleType = this.OfrSettleType;
                    px = this.Ofr;
                    qty = this.OfrAmount;
                    opp_vol_delivery = STC.Common.Utils.FQHelper.GetVolDelivery(this.BidCornerMarketText, this.BidSettleType, this.BidAmount);
                    opp_price = this.Bid;
                    opp_price_type = BidPriceType;
                }
                string price_type = string.Empty;
                string yield_type = string.Empty;
                if (side == "1")
                {
                    //bid
                    string[,] bidvol_delivery = new string[BidMD.Count, 2];
                    for (int i = 0; i < BidMD.Count; i++)
                    {
                        GNoMDEntries item = BidMD[i];
                        decimal itemqty = CommonUtils.ConvertToDecimal(item.MDEntrySize);
                        string itemqtyamount = itemqty == 0 ? CommonConstants.NO_PRICE : itemqty.ToString();
                        string itemsettl = item.SettlType;
                        bidvol_delivery[i, 0] = itemqtyamount;
                        bidvol_delivery[i, 1] = ResulfApiHelper.ConvertSettleType(itemsettl);
                        price_type = string.IsNullOrEmpty(price_type) ? item.PriceType ?? "3" : price_type;
                        yield_type = string.IsNullOrEmpty(yield_type) ? item.ExerciseFlag ?? "1" : yield_type; // 0=行权收益率,1或空=到期收益率
                    }

                    vol_delivery = new string[bidvol_delivery.GetLength(0), bidvol_delivery.GetLength(1)];
                    Array.Copy(bidvol_delivery, vol_delivery, bidvol_delivery.Length);

                    vol_delivery_qb = STC.Common.Utils.FQHelper.GetQBVolDeliveryFromMD(BidMD);
                }
                else
                {
                    //ofr
                    string[,] ofrvol_delivery = new string[OfrMD.Count, 2];
                    for (int i = 0; i < OfrMD.Count; i++)
                    {
                        GNoMDEntries item = OfrMD[i];
                        decimal itemqty = CommonUtils.ConvertToDecimal(item.MDEntrySize);
                        string itemqtyamount = itemqty == 0 ? CommonConstants.NO_PRICE : itemqty.ToString();
                        string itemsettl = item.SettlType;
                        ofrvol_delivery[i, 0] = itemqtyamount;
                        ofrvol_delivery[i, 1] = ResulfApiHelper.ConvertSettleType(itemsettl);
                        price_type = string.IsNullOrEmpty(price_type) ? item.PriceType ?? "3" : price_type;
                        yield_type = string.IsNullOrEmpty(yield_type) ? item.ExerciseFlag ?? "1" : yield_type; // 0=行权收益率,1或空=到期收益率
                    }
                    vol_delivery = new string[ofrvol_delivery.GetLength(0), ofrvol_delivery.GetLength(1)];
                    Array.Copy(ofrvol_delivery, vol_delivery, ofrvol_delivery.Length);
                    vol_delivery_qb = STC.Common.Utils.FQHelper.GetQBVolDeliveryFromMD(OfrMD);
                }

                dm.id = Guid.NewGuid().ToString("N");
                dm.comp_id = source;
                dm.code = securityId;
                dm.name = symbolName;
                dm.side = side_txt;
                dm.join = join;
                dm.price = (px == CommonConstants.NO_PRICE || px == CommonConstants.NO_BROKER) ? string.Empty : px;
                dm.vol_delivery = vol_delivery;
                dm.vol_delivery_qb = vol_delivery_qb;
                dm.bond_type = bond_type;
                dm.price_type = price_type;
                dm.opp_vol_delivery = opp_vol_delivery;
                dm.opp_price = (opp_price == CommonConstants.NO_PRICE || opp_price == CommonConstants.NO_BROKER) ? string.Empty : opp_price;
                dm.opp_price_type = opp_price_type;
                dm.yield_type = dm.opp_yield_type = yield_type;
                dm.ext = BidMDEntryID + "_" + OfrMDEntryID;
                dm.val_yield = _priceModel.CDCValue;
            }
            catch (Exception ex )
            {
                LogHelper.Error(ex.Message);
            }

            return dm;
        }

        private async void SendHttpQQ(string side, string type, string content, string qq, BrokerPriceViewModel brokerPriceViewModel)
        {
            SendFinQFriendReq qqsmm = new SendFinQFriendReq();
            qqsmm.content = content;
            qqsmm.data = await GenDataMessage(side, type);
            qqsmm.fromQB = LocalEnv.UserInfo.UserName;
            qqsmm.openChat = true;
            qqsmm.fromQBAccountID = LocalEnv.UserInfo.UserId;
            //如果使用WebFinQ发送，则调用WebFinQ接口，否则调用QQ接口
            if (LocalEnv.ConfInfo.IsForceUseWebFinQ == "1")
            {
                qqsmm.UpdateTime = brokerPriceViewModel.UpdateTime;
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
            //string msg = ResulfApiHelper.GenMsg(content, securityId, symbolName, side_txt, join, px, vol_delivery, qq);
            //LogHelper.Debug(msg);
            //await ResulfApiHelper.SendMessageToQQ(msg);
        }

        public void CopyOrderData(string side = null, bool isCtrl = false)
        {
            List<dynamic> paramList = new List<dynamic>();
            dynamic d = new System.Dynamic.ExpandoObject();
            d.side = side;
            d.isCtrl = isCtrl;
            d.isNeedSource = HeadVisibility == Visibility.Collapsed;
            d.isBestInvoke = false;
            d.html = string.Empty;
            d.plainTxt = string.Empty;
            paramList.Add(d);
            Sub_CopyOrderData(paramList);
        }

        public async void Sub_CopyOrderData(List<dynamic> paramList)
        {
            if (paramList.Count == 0 || string.IsNullOrEmpty(this.PriceModel.Security) || this.PriceModel.Security.ToLower() == "tradebook")
                return;

            dynamic condition = paramList[0];

            string html;
            string plainTxt;
            string html_double;
            string plainTxt_double;

            GetDoubleText(out html_double, out plainTxt_double, condition.isNeedSource);
            BrokerPriceViewModel brokerPriceViewModel = this;
            string securityId = brokerPriceViewModel.Security;
            string symbolName = string.Empty;
            BondsInfoPo bondsInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            if (bondsInfo != null)
            {
                symbolName = bondsInfo.Short_Name;
            }

            string info;
            if (condition.side == "1")
            {
                string s = "GVN";
                string p = (brokerPriceViewModel.Bid == CommonConstants.NO_PRICE || brokerPriceViewModel.Bid == CommonConstants.NO_BROKER) ? string.Empty : brokerPriceViewModel.Bid;
                info = $",    {s} {securityId} {p}";

                html = $"<p>[{html_double}]{info}</p>";

                plainTxt = $"[{plainTxt_double}]{info}";
            }
            else if (condition.side == "2")
            {
                string s = "TKN";
                string p = (brokerPriceViewModel.Ofr == CommonConstants.NO_PRICE || brokerPriceViewModel.Ofr == CommonConstants.NO_BROKER) ? string.Empty : brokerPriceViewModel.Ofr;
                info = $",    {s} {securityId} {brokerPriceViewModel.Ofr}";

                html = $"<p>[{html_double}]{info}</p>";

                plainTxt = $"[{plainTxt_double}]{info}";
            }
            else
            {
                html = $"<p>{html_double}</p>";

                plainTxt = $"{plainTxt_double}";
            }

            condition.html = html;
            condition.plainTxt = plainTxt;

            if (condition.isBestInvoke == false)
            {
                //复制
                ClipboardHelper.CopyToClipboard(html, plainTxt);

                if (condition.isCtrl)
                {
                    //    if (plainTxt.Length > 0)
                    //    {
                    //        string qq = ResulfApiHelper.GetQQByBrokerCN(brokerPriceViewModel.BrokerCN);

                    //        if (!string.IsNullOrEmpty(qq))
                    //        {
                    //            //await ResulfApiHelper.SendMessageToQQ(plainTxt.ToString());
                    //            await ResulfApiHelper.ManualSendMessageToQQRequest(qq, plainTxt.ToString());
                    //        }
                    //    }

                    if (LocalEnv.UserInfo.QeubeeFingPermissions == "1")
                    {
                        SendHttpQQ(condition.side, null, plainTxt, string.Empty, brokerPriceViewModel);
                    }
                }
            }
        }

        private void GetDoubleText(out string html, out string plainTxt, bool isNeedSource = false)
        {
            BrokerPriceViewModel brokerPriceViewModel = this;

            string securityId = brokerPriceViewModel.Security;
            string symbolName = string.Empty;//brokerPriceViewModel.SymbolName;
            string remainingTime = string.Empty;//brokerPriceViewModel.RemainingTime;

            string cdcValue = string.Empty;

            if (this.PriceModel != null)
            {
                symbolName = brokerPriceViewModel.PriceModel.SymbolName == null ? string.Empty : brokerPriceViewModel.PriceModel.SymbolName;
                remainingTime = brokerPriceViewModel.PriceModel.RemainingTime == null ? string.Empty : brokerPriceViewModel.PriceModel.RemainingTime;

                cdcValue = CommonConstants.CDCValueTxt + brokerPriceViewModel.PriceModel.CCDCValuation;
            }

            string bidQty = brokerPriceViewModel.BidCornerMarketText.Length > 0 ? brokerPriceViewModel.BidCornerMarketText : brokerPriceViewModel.BidAmount;
            string bidPx = brokerPriceViewModel.Bid;
            string bidSettle = brokerPriceViewModel.BidSettleTip;

            string ofrQty = brokerPriceViewModel.OfrCornerMarketText.Length > 0 ? brokerPriceViewModel.OfrCornerMarketText : brokerPriceViewModel.OfrAmount;
            string ofrPx = brokerPriceViewModel.Ofr;
            string ofrSettle = brokerPriceViewModel.OfrSettleTip;



            string htmlv = string.Empty;
            string txtv = string.Empty;
            if (isNeedSource)
            {
                htmlv = $"{brokerPriceViewModel.BrokerCN}&#9;";//index venues_cn
                txtv = $"{brokerPriceViewModel.BrokerCN}    ";
            }

            string t = string.Empty;

            // todo 时间？？？

            //html.Append($"<p>{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQty}/{ofrQty}&#9;{v}&#9;{t}</p>");

            //plainTxt.Append($"{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQty}/{ofrQty}    {v}    {t}");

            string htmlRemainingTime = string.Empty;
            string txtRemainingTime = string.Empty;
            if (!string.IsNullOrEmpty(remainingTime) && remainingTime.Length > 0)
            {
                htmlRemainingTime = $"{remainingTime}&#9;";
                txtRemainingTime = $"{remainingTime}    ";
            }

            string htmlSymbolName = string.Empty;
            string txtSymbolName = string.Empty;
            if (symbolName.Length > 0)
            {
                htmlSymbolName = $"{symbolName}&#9;";
                txtSymbolName = $"{symbolName}    ";
            }

            html = $"{htmlRemainingTime}{securityId}&#9;{htmlSymbolName}{bidPx}/{ofrPx}&#9;{bidQty}/{ofrQty}&#9;{htmlv}{cdcValue}";
            plainTxt = $"{txtRemainingTime}{securityId}    {txtSymbolName}{bidPx}/{ofrPx}    {bidQty}/{ofrQty}    {txtv}{cdcValue}";
        }

        #endregion
    }
}
