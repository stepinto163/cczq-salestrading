using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Model;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.WorkTable.plugin.Validation;
using STC.WorkTable.Plugin.Utils.Data.CRM.Model.CRM;
using STC.WorkTable.Plugin.View;
using STC.WorkTable.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static STC.WorkTable.Plugin.View.Component.AutoCompleteComboBox;
using WtUtils = STC.WorkTable.Plugin.Utils.CommonUtils;

namespace STC.WorkTable.Plugin.ViewModel
{
    public class ConfirmIntentionWindowViewModel : BaseViewModel
    {
        private TradeRequestData _Orignal;
        private TradeRequestData _EditData;

        public TradeRequestData Orignal
        {
            get
            {
                return _Orignal;
            }
            set
            {
                if (value != null)
                {
                    Side_Edit = value.Side;
                    InstitutionName_Edit = value.InstitutionName;
                    TraderName_Edit = value.TraderName;
                    Securityid_Edit = value.SecurityId;
                    if (value.Price.HasValue)
                        Price_Edit = value.Price;
                    if (value.Qty.HasValue)
                        DisplayQty_Edit = value.Qty / 1000;

                    string settlType = CommonUtils.ConvertToString(value.SettlType);
                    string tradeDate = string.IsNullOrEmpty(value.CreateTime) ? DateTime.Now.ToString("yyyyMMdd") : CommonUtils.ConvertToString(value.CreateTime);

                    DateTime settlDateTime;
                    int selectIndex;
                    CommonUtils.SetSettleType(settlType, tradeDate, out settlDateTime, out selectIndex);
                    EditSettleTypeDate = settlDateTime;
                    EditSettleTypeMode = selectIndex == 0 ? "1" : "2";
                }
                _Orignal = _EditData = value;
                IntentionHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;
                this.OnPropertyChanged();
            }
        }

        private string _side_Edit = "1";


        public string Side_Edit
        {
            get { return _side_Edit; }
            set
            {
                _side_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private CrmInst _selectedInstitution_Edit;

        public CrmInst SelectedInstintuion_Edit
        {
            get { return _selectedInstitution_Edit; }
            set
            {
                _selectedInstitution_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionName_Edit;

        [CustomValidation(typeof(ValidationExtensionRules), "NotEmpty")]
        public string InstitutionName_Edit
        {
            get { return _institutionName_Edit; }
            set
            {
                _institutionName_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private CrmContact _selectedTrader_Edit;

        public CrmContact SelectedTrader_Edit
        {
            get { return _selectedTrader_Edit; }
            set
            {
                _selectedTrader_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderName_Edit;

        [CustomValidation(typeof(ValidationExtensionRules), "NotEmpty")]
        public string TraderName_Edit
        {
            get { return _traderName_Edit; }
            set
            {
                _traderName_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _securityid_Edit;
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmpty")]
        public string Securityid_Edit
        {
            get { return _securityid_Edit; }
            set
            {
                _securityid_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _price_Edit;
        [CustomValidation(typeof(ValidationExtensionRules), "NotNull")]
        public decimal? Price_Edit
        {
            get { return _price_Edit; }
            set
            {
                _price_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private decimal? _displayQty_Edit;
        [CustomValidation(typeof(ValidationExtensionRules), "NotNull")]
        public decimal? DisplayQty_Edit
        {
            get
            {
                return _displayQty_Edit;
            }
            set
            {
                _displayQty_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime _EditSettleTypeDate = DateTime.Now;
        public DateTime EditSettleTypeDate
        {
            get
            {
                return _EditSettleTypeDate;
            }
            set
            {
                _EditSettleTypeDate = value;
                this.OnPropertyChanged();
            }
        }

        private string _EditSettleTypeMode = "1";
        public string EditSettleTypeMode
        {
            get { return _EditSettleTypeMode; }
            set
            {
                _EditSettleTypeMode = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _sideDic = new Dictionary<string, string>();
        public Dictionary<string, string> SideDic
        {
            get
            {
                if (_sideDic.Count == 0)
                {
                    _sideDic["1"] = "Bid";
                    _sideDic["2"] = "Ofr";
                }
                return _sideDic;
            }
        }

        private Dictionary<string, string> _settleType = new Dictionary<string, string>();
        public Dictionary<string, string> SettleTypeDic
        {
            get
            {
                if (_settleType.Count == 0)
                {
                    _settleType["1"] = "T+0";
                    _settleType["2"] = "T+1";
                }
                return _settleType;
            }
        }

        private object _replaceLock = new object();
        private CollectionViewSource replaceViewSource = new CollectionViewSource();
        public CollectionViewSource ReplacedViewSource
        {
            get { return replaceViewSource; }
            set { replaceViewSource = value; }
        }

        private CustomCollection<TradeRequestData> tradeRequestInfo = new CustomCollection<TradeRequestData>();

        public CustomCollection<TradeRequestData> TradeRequestInfo
        {
            get { return tradeRequestInfo; }
            set { tradeRequestInfo = value; }
        }

        private TradeRequestData _SelectedReplaceItem;

        public TradeRequestData SelectedReplaceItem
        {
            get { return _SelectedReplaceItem; }
            set { _SelectedReplaceItem = value; }
        }


        public ConfirmIntentionWindowViewModel()
        {
            TradeRequestData firstData = new TradeRequestData();
            firstData.IntentionId = string.Empty;
            TradeRequestInfo.Add(firstData);

            this.ReplacedViewSource.IsLiveFilteringRequested = true;
            this.ReplacedViewSource.IsLiveSortingRequested = true;
            this.ReplacedViewSource.Source = TradeRequestInfo;
            BindingOperations.EnableCollectionSynchronization(this.ReplacedViewSource.View, _replaceLock);
        }

        private void Instance_OnIntentionQuotes(IntentionQuoteInfo obj)
        {
            if (obj.securityId == this.Orignal.SecurityId
                && obj.institutionName == this.Orignal.InstitutionName
                && obj.traderName == this.Orignal.TraderName
                && obj.side == this.Orignal.Side)
            {

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    TradeRequestData item = null;
                    var findList = TradeRequestInfo.Where(x => x.IntentionId == obj.intentionId);
                    if (findList != null && findList.Count() > 0)
                    {


                        item = findList.First();
                        if (obj.status == "0" || obj.status == "1" || obj.status == "3" || obj.bookQty > 0)
                        {
                            TradeRequestInfo.Remove(item);
                            return;
                        }

                        item.Status = obj.status;
                        item.InstitutionId = obj.institutionId;
                        item.InstitutionName = obj.institutionName;
                        item.TraderId = obj.traderId;
                        item.TraderName = obj.traderName;
                        item.Side = obj.side;
                        item.Price = obj.price;
                        item.Qty = obj.qty;
                        item.DisplayQty = (long)obj.qty / 1000;
                        item.SettlType = obj.settlType;
                        item.UpdateTime = obj.updateTime;
                        item.UserName = obj.userName;
                        double completeRate = 0;
                        double.TryParse(obj.completeRate == null ? "0" : obj.completeRate.ToString(), out completeRate);
                        item.CompleteRate = completeRate;
                        item.BookQty = obj.bookQty;
                        item.NikeName = obj.nickName;

                    }
                    else
                    {
                        if (obj.status == "0" || obj.status == "1" || obj.status == "3" || obj.bookQty > 0)
                        {
                            return;
                        }

                        item = new TradeRequestData();

                        item.IntentionId = obj.intentionId;
                        item.InstitutionId = obj.institutionId;
                        item.InstitutionName = obj.institutionName;
                        item.TraderId = obj.traderId;
                        item.TraderName = obj.traderName;
                        item.Side = obj.side;
                        item.SecurityId = obj.securityId;
                        item.Symbol = obj.symbol;
                        item.Price = obj.price;
                        item.Qty = obj.qty;
                        item.DisplayQty = (long)obj.qty / 1000;
                        item.SettlType = obj.settlType;
                        item.Text = obj.text;
                        item.CreateTime = obj.createTime;
                        item.UpdateTime = obj.updateTime;
                        item.UserName = obj.userName;
                        item.Status = obj.status;
                        double completeRate = 0;
                        double.TryParse(obj.completeRate == null ? "0" : obj.completeRate.ToString(), out completeRate);
                        item.CompleteRate = completeRate;
                        item.BookQty = obj.bookQty;
                        item.NikeName = obj.nickName;

                        this.TradeRequestInfo.Add(item);

                    }

                }));
            }
        }

        internal void FilterInstitution(AutoCompleteArgs args)
        {

            //BankListViewSource.View.Filter = x =>
            //{
            //    if (string.IsNullOrEmpty(args.Pattern))
            //        return true;
            //    else
            //        return ((KeyValueData)x).Key.Contains(args.Pattern) || ((KeyValueData)x).Value.Contains(args.Pattern);
            //};
            //args.DataSource = BankListViewSource.View;
            string currentTraderID = this.SelectedTrader_Edit == null ? string.Empty : this.SelectedTrader_Edit.idbkey;

            List<string> instIdList = null;
            if (!string.IsNullOrEmpty(currentTraderID))
            {
                List<CrmContact> crList = WtUtils.GetContacts(null, currentTraderID);

                if (crList != null && crList.Count > 0)
                {
                    instIdList = new List<string>();
                    foreach (CrmContact crItem in crList)
                    {
                        if (!instIdList.Contains(crItem.instid))
                        {
                            instIdList.Add(crItem.instid);
                        }
                    }
                }
            }

            List<CrmInst> result = new List<CrmInst>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {

                        List<CrmInst> selectDatas = WtUtils.GetInstitutionsInfoPo(str, 20, instIdList);
                        result.AddRange(selectDatas);
                    }
                }
                else
                {
                    result = WtUtils.GetInstitutionsInfoPo(args.Pattern.Trim(), 20, instIdList);
                }
            }
            else
            {
                result.AddRange(WtUtils.GetInstitutionsInfoPo(instIdList));
            }
            args.DataSource = result;
        }

        internal void FilterTrader(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedInstitution_Edit == null ? string.Empty : this._selectedInstitution_Edit.idbkey;

            List<string> instList = new List<string>();
            if (!string.IsNullOrEmpty(currentInstID))
            {
                instList.Add(currentInstID);
            }

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = WtUtils.GetContactInfoPo(str, instList);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = WtUtils.GetContactInfoPo(args.Pattern.Trim(), instList);
            }
            else
            {
                result.AddRange(WtUtils.GetContacts(currentInstID));
            }
            args.DataSource = result;
        }

        internal async void Confirm()
        {
            UpdateIntentionQuoteRequest updateRequest = new UpdateIntentionQuoteRequest();
            updateRequest.userName = LocalEnv.UserInfo.UserName;
            updateRequest.nickName = LocalEnv.UserInfo.FullName;
            updateRequest.intentionId = Orignal.IntentionId;
            //updateRequest.replaceId = Orignal.IntentionId;
            //updateRequest.institutionId = institutionId;
            //updateRequest.institutionName = institutionName;
            //updateRequest.traderId = traderId;
            //updateRequest.traderName = traderName;
            //updateRequest.securityId = securityId;
            //updateRequest.symbol = symbol;
            //updateRequest.side = side;
            //updateRequest.price = price;
            //updateRequest.qty = qty;
            //updateRequest.settlType = settlType;
            updateRequest.status = "1";

            bool isSuccess = await IntentionHandler.Instance.UpdateIntentionQuote(updateRequest);

            if (isSuccess == false)
            {
                return;
            }

            DoConfirmCommand();
        }

        /// <summary>
        /// 普通确认
        /// </summary>
        private void DoConfirmCommand()
        {
            // 判断是否更新
            // 价格
            decimal? price = null;
            // 报量
            decimal? qty = null;
            // 清算速度
            string settlType = null;
            // 交易员编号
            string traderId = null;
            // 交易员名称
            string traderName = null;
            // 经纪商编号
            string brokerId = null;
            // 经纪商名称
            string brokerName = null;
            // 机构编号
            string institutionId = null;
            // 机构名称
            string institutionName = null;
            // 方向
            string side = null;
            // 债券代码
            string securityId = null;
            // 债券简称
            string symbol = null;

            // 客户机构
            if (SelectedInstintuion_Edit != null)
            {
                institutionId = SelectedInstintuion_Edit.idbkey;
                institutionName = SelectedInstintuion_Edit.shortnamecn;
            }
            else
            {
                institutionId = string.Empty;
                institutionName = InstitutionName_Edit;
            }

            // 客户名
            if (SelectedTrader_Edit != null)
            {
                traderId = SelectedTrader_Edit.idbkey;
                traderName = SelectedTrader_Edit.namecn;
                //item.QqNumber = item.SelectedTrader_Edit.QQ;
            }
            else
            {
                traderName = string.Empty;
                traderId = TraderName_Edit;
            }

            // 债券代码
            securityId = Securityid_Edit;
            symbol = CommonUtils.GetBondsInfoPoBySecurityID(securityId).Short_Name;
            // 方向
            side = Side_Edit;
            // 价格
            price = Price_Edit;
            // 数量
            qty = DisplayQty_Edit * 1000;
            // 清算速度
            DateTime createtime = CommonUtils.ConvertToDateTime(Orignal.CreateTime);
            string settlTypeEdit = CommonUtils.IsSpotSettle(createtime, EditSettleTypeDate, EditSettleTypeMode == "1" ? 0 : 1);
            if (string.IsNullOrEmpty(settlTypeEdit))
            {
                settlTypeEdit = EditSettleTypeDate.ToString("MM.dd") + (EditSettleTypeMode == "1" ? "+0" : "+1");
            }
            settlType = settlTypeEdit;


            string intentionId = this.Orignal.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.crossVenues = "2";
            request.actionType = "1";// 普通确认
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.venueTypeGw = "INNER";
            request.venues = "MANUAL";
            request.orderType = "2"; //1:市价单; 2:限价单
            request.timeInForce = "0";// 0:DAY; 1:GTC; 3:IOC; 4:FOK; A:GFD
            request.firmFlag = "0";

            
            request.institutionId = institutionId;
            request.institutionName = institutionName;
            request.traderId = traderId;
            request.traderName = traderName;
            request.securityId = securityId;
            request.side = side;
            request.price = price;
            request.qty = qty;
            request.settlType = settlType;

            IntentionOrder io = new IntentionOrder();
            io.request = request;
            io.detailIntentionItem = new TradeRequestData();
            io.detailIntentionItem.SecurityId = this.Securityid_Edit;
            io.detailIntentionItem.Side = this.Side_Edit;
            io.detailIntentionItem.Price = this.Price_Edit;
            io.detailIntentionItem.Qty = this.DisplayQty_Edit * 1000;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    ConfirmIntentionQuoteReplyPo replyPo = IntentionHandler.Instance.ConfirmIntentionQuote(request);
                    if (replyPo == null)
                    {
                        WtUtils.Prompt2Second("警告", $"意向id:[{intentionId}]普通确认失败");
                    }
                    else
                    {
                        if (!replyPo.IsSuccess())
                        {
                            if (replyPo.Code == "10000")
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    SendPriceIntentionWarnView warnView = new SendPriceIntentionWarnView(replyPo, io);
                                    warnView.ShowDialog();
                                }));

                            }
                            else
                            {
                                WtUtils.Prompt2Second("警告", replyPo.Msg);
                            }
                        }
                        else
                        {
                            //replyUpdateThread.Enqueue(replyPo);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoConfirmCommand", e);
                }
            });
        }


        /// <summary>
        /// 替换操作
        /// </summary>
        internal void DoReplaceCommand()
        {
            if (this.SelectedReplaceItem.BookQty > 0)
            {
                WtUtils.Prompt2Second("警告", "替换订单已成交");
                return;
            }
            // 判断是否更新
            // 价格
            decimal? price = null;
            // 报量
            decimal? qty = null;
            // 清算速度
            string settlType = null;

            // 价格
            price = Price_Edit;
            // 数量
            qty = DisplayQty_Edit;

            // 清算速度
            DateTime createtime = CommonUtils.ConvertToDateTime(Orignal.CreateTime);
            string settlTypeEdit = CommonUtils.IsSpotSettle(createtime, EditSettleTypeDate, EditSettleTypeMode == "1" ? 0 : 1);
            if (string.IsNullOrEmpty(settlTypeEdit))
            {
                settlTypeEdit = EditSettleTypeDate.ToString("MM.dd") + (EditSettleTypeMode == "1" ? "+0" : "+1");
            }
            settlType = settlTypeEdit;

            // 新更新
            string intentionId = this.Orignal.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.crossVenues = "2";
            request.actionType = "1";// 普通确认
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.venueTypeGw = "INNER";
            request.venues = "MANUAL";
            request.orderType = "2"; //1:市价单; 2:限价单
            request.timeInForce = "0";// 0:DAY; 1:GTC; 3:IOC; 4:FOK; A:GFD
            request.firmFlag = "0";

            request.replaceId = this.SelectedReplaceItem.IntentionId;
            request.price = price;
            request.qty = qty;
            request.settlType = settlType;
            IntentionOrder io = new IntentionOrder();
            io.request = request;
            io.detailIntentionItem = new TradeRequestData();
            io.detailIntentionItem.SecurityId = this.Securityid_Edit;
            io.detailIntentionItem.Side = this.Side_Edit;
            io.detailIntentionItem.Price = this.Price_Edit;
            io.detailIntentionItem.Qty = this.DisplayQty_Edit * 1000;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    ConfirmIntentionQuoteReplyPo replyPo = IntentionHandler.Instance.ConfirmIntentionQuote(request);
                    if (replyPo == null)
                    {
                        WtUtils.Prompt2Second("警告", $"意向id:[{intentionId}]替换[{this.SelectedReplaceItem.IntentionId}]失败");
                    }
                    else
                    {
                        if (!replyPo.IsSuccess())
                        {
                            if (replyPo.Code == "10000")
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    SendPriceIntentionWarnView warnView = new SendPriceIntentionWarnView(replyPo, io);
                                    warnView.ShowDialog();
                                }));

                            }
                            else
                            {
                                WtUtils.Prompt2Second("警告", replyPo.Msg);
                            }
                        }
                        else
                        {
                            //replyUpdateThread.Enqueue(replyPo);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoReplaceCommand", e);
                }
            });

        }
    }
}
