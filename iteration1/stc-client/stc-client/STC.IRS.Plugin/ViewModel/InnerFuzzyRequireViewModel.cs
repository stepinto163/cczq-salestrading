using Ats.Foundation.Message;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Common.ViewModel.Order.InnerView;
using STC.IRS.Plugin.View;
using STC.IRS.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static STC.Common.View.Component.AutoCompleteComboBox;
using OrderCancelRequest = STC.Common.Model.TBook.OrderCancelRequest;

namespace STC.IRS.Plugin.ViewModel
{
    public class InnerFuzzyRequireViewModel : ViewModelBase
    {
        #region 构造
        internal bool isInit = false;
        private string DEFAULT_FOCUSTAGS = string.Empty;
        private InnerFuzzyRequireView myWindow;

        private string _focusTags;

        public string FocusTags
        {
            get { return _focusTags; }
            set
            {
                if (_focusTags != value)
                {
                    _focusTags = value;

                    try
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            FocusTagDic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
                        }
                        else
                        {
                            FocusTagDic = new Dictionary<string, List<string>>();
                        }
                    }
                    catch (Exception)
                    {
                        FocusTagDic = new Dictionary<string, List<string>>();
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        private Dictionary<string, List<string>> _focusTagDic;
        /// <summary>
        /// 筛选字典
        /// </summary>
        public Dictionary<string, List<string>> FocusTagDic
        {
            get
            {
                return _focusTagDic;
            }
            set
            {
                _focusTagDic = value;
                if (value != null)
                {
                    TagConditionViewModel tagconditionviewmodel = (myWindow as InnerFuzzyRequireView).TagConditionView.DataContext as TagConditionViewModel;
                    if (value.ContainsKey("SIDE"))
                    {
                        List<string> lst = value["SIDE"];
                        if (lst.Count <= 1)
                        {
                            tagconditionviewmodel.Side = lst[0] == "0001" ? "0" : "1";
                        }
                        else
                        {
                            tagconditionviewmodel.Side = "";
                        }
                    }
                    else
                    {
                        tagconditionviewmodel.Side = "";
                    }

                    Side = tagconditionviewmodel.Side == "0" ? "1" : "2";
                }

                this.OnPropertyChanged();
            }
        }

        private string _startPrice;

        public string StartPrice
        {
            get { return _startPrice; }
            set
            {
                _startPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _endPrice;

        public string EndPrice
        {
            get { return _endPrice; }
            set
            {
                _endPrice = value;
                this.OnPropertyChanged();

            }
        }

        private bool _isPrice;

        public bool IsPrice
        {
            get { return _isPrice; }
            set
            {
                _isPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string bondTxt = string.Empty;
        public string BondTxt
        {
            get { return bondTxt; }
            set { bondTxt = value; this.OnPropertyChanged(); }
        }

        private string side = string.Empty;
        public string Side
        {
            get { return side; }
            set { side = value; this.OnPropertyChanged(); }
        }

        private CrmInst selectClientTypeItem;

        public CrmInst SelectClientTypeItem
        {
            get { return selectClientTypeItem; }
            set
            {
                if (selectClientTypeItem != value)
                {
                    selectClientTypeItem = value;
                    if (selectClientTypeItem != null)
                        ClientTypeEnum = selectClientTypeItem.shortnamecn;
                    this.OnPropertyChanged();
                }
            }
        }

        private string clientTypeEnum;
        public string ClientTypeEnum
        {
            get { return clientTypeEnum; }
            set { clientTypeEnum = value; this.OnPropertyChanged(); }
        }


        private bool checkVIP;
        public bool CheckVIP
        {
            get { return checkVIP; }
            set { checkVIP = value; this.OnPropertyChanged(); }
        }

        private CrmInst _selectedInstitution_Edit;

        public CrmInst SelectedInstintuion_Edit
        {
            get { return _selectedInstitution_Edit; }
            set
            {
                if (_selectedInstitution_Edit != value)
                {
                    _selectedInstitution_Edit = value;

                    if (value != null)
                    {
                        List<CrmInst> result = GetCrmInst();
                        if (this._selectedInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedInstitution_Edit.insttypevalue))
                        {
                            result = result.Where(item => item.shortnamecn == this._selectedInstitution_Edit.insttypevalue).ToList();
                            if (result != null && result.Count > 0)
                            {
                                this.SelectClientTypeItem = result.First();
                            }

                        }
                    }

                    this.OnPropertyChanged();
                }
            }
        }


        private CrmContact _selectedTrader_Edit;

        public CrmContact SelectedTrader_Edit
        {
            get { return _selectedTrader_Edit; }
            set
            {

                if (_selectedTrader_Edit != value)
                {
                    // 只有crmContact中才有机构信息，可以联动显示
                    if (value != null)
                    {
                        if (SelectedInstintuion_Edit == null || SelectedInstintuion_Edit.idbkey != value.instid)
                        {
                            List<string> instList = new List<string>();
                            instList.Add(value.instid);
                            List<CrmInst> results = CommonUtils.GetInstitutionsInfoPo(instList);
                            if (results != null && results.Count > 0)
                            {
                                SelectedInstintuion_Edit = results[0];
                                InstitutionName_Edit = results[0].shortnamecn;
                            }
                        }

                        if (SelectedInstintuion_Edit != null && SelectedInstintuion_Edit.idbkey == value.instid)
                        {
                            SelectedInstintuion_Edit = SelectedInstintuion_Edit;
                            InstitutionName_Edit = InstitutionName_Edit;
                        }
                    }

                    _selectedTrader_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderName_Edit;

        public string TraderName_Edit
        {
            get { return _traderName_Edit; }
            set
            {
                _traderName_Edit = value;
                this.OnPropertyChanged();

            }
        }

        private string _institutionName_Edit;
        public string InstitutionName_Edit
        {
            get { return _institutionName_Edit; }
            set
            {
                _institutionName_Edit = value;
                this.OnPropertyChanged();

            }
        }

        private BondsInfoPo BondsInfoPo;
        private NewOrderSingle OldOrder;
        public bool IsUpdatePrice = false;

        /// <summary>
        /// 是否显示  Saler 区域
        /// </summary>
        private Visibility isshowsaler = Visibility.Hidden;
        public Visibility IsShowSaler
        {
            get { return isshowsaler; }
            set
            {
                isshowsaler = value;
                this.OnPropertyChanged();
            }
        }

        #region Command
        public ICommand CloseCommand { get; set; }

        //发布
        public ICommand PublishCommand { get; set; }

        //发布并继续
        public ICommand PublishAndContinueCommand { get; set; }
        #endregion

        #endregion
        public InnerFuzzyRequireViewModel()
        {
            this.CloseCommand = new ComRelayCommand(OnCloseCommand);
            this.PublishCommand = new ComRelayCommand(OnPublishCommand);
            this.PublishAndContinueCommand = new ComRelayCommand(OnPublishAndContinueCommand);
            Init();
        }

        public InnerFuzzyRequireViewModel(InnerFuzzyRequireView my, string info = "")
        {
            myWindow = my;
            this.CloseCommand = new ComRelayCommand(OnCloseCommand);
            this.PublishCommand = new ComRelayCommand(OnPublishCommand);
            this.PublishAndContinueCommand = new ComRelayCommand(OnPublishAndContinueCommand);
            Init(info);
        }

        private void Init(string info = "")
        {
            BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID("99999");
            if (bondsInfoPo != null)
            {
                this.BondsInfoPo = bondsInfoPo;
            }

            if (STC.Common.Data.IntentionIRSHandler.Instance.isSALER())
            {
                IsShowSaler = Visibility.Visible;
            }

            if (string.IsNullOrEmpty(info))
            {
                string tmp = Clipboard.GetText().Trim();
                this.BondTxt = tmp;
            }
            else
            {
                if (JsonSplit.IsJson(info))
                {
                    PriceMatchParam pm = JsonConvert.DeserializeObject<PriceMatchParam>(info);
                    OldOrder = pm.OldOrder;
                    IsUpdatePrice = pm.UpdataPrice;
                    //UpdataPrice
                    TagConditionViewModel tagconditionviewmodel = (myWindow as InnerFuzzyRequireView).TagConditionView.DataContext as TagConditionViewModel;
                    OrderTimeModel otm = JsonConvert.DeserializeObject<OrderTimeModel>(pm.Info1);
                    //恢复模糊识别文本
                    this.BondTxt = otm.Text;
                    //恢复时间设置
                    tagconditionviewmodel.ParseValidTimeFormat(otm.ValidTimeType);
                    //恢复撤单条件
                    tagconditionviewmodel.ParsePriceCondition(otm);
                    //恢复订阅
                    tagconditionviewmodel.ParseTagData(otm.IF_SUB_BOND, pm.Side == "1" ? "0" : "1");
                    //恢复订阅过滤区
                    if ((!string.IsNullOrEmpty(otm.FocusTags)) && JsonSplit.IsJson(otm.FocusTags))
                    {
                        FocusTags = otm.FocusTags;
                    }

                    //恢复客户，客户名，VIP，客户类型
                    ParseInstitutionAndTrader(pm);
                    ParseVipOrgtypeData(pm.Info1);
                }
            }
        }

        #region  UI设置
        /// <summary>
        /// 设置Vip和客户类型
        /// </summary>
        /// <param name="info2"></param>
        public void ParseVipOrgtypeData(string info2)
        {
            if (!string.IsNullOrEmpty(info2))
            {
                if (JsonSplit.IsJson(info2))
                {
                    try
                    {
                        TagData tagData = JsonConvert.DeserializeObject<TagData>(info2);
                        string code = tagData.ORG_TYPE;
                        if (!string.IsNullOrEmpty(code))
                        {
                            List<CrmInst> lstInst = GetCrmInst();
                            SelectClientTypeItem = lstInst.FindLast(o => o.instcode == code);

                        }

                        if (!string.IsNullOrEmpty(tagData.VIP))
                        {
                            CheckVIP = "0001".Equals(tagData.VIP);
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 设置机构名和客户名
        /// </summary>
        /// <param name="pm"></param>
        public void ParseInstitutionAndTrader(PriceMatchParam pm)
        {
            if (!string.IsNullOrEmpty(pm.InstitutionID))
            {
                List<CrmInst> crmInsts = CommonUtils.GetInstitutionsIDInfoPo(pm.InstitutionID);
                if (crmInsts != null && crmInsts.Count > 0)
                {
                    SelectedInstintuion_Edit = crmInsts[0];
                    InstitutionName_Edit = SelectedInstintuion_Edit.shortnamecn;
                }
            }

            if (!string.IsNullOrEmpty(pm.TraderID))
            {
                List<CrmContact> crmContacts = CommonUtils.GetContactInfoPo(pm.TraderID, pm.InstitutionID);
                if (crmContacts != null && crmContacts.Count > 0)
                {
                    SelectedTrader_Edit = crmContacts[0];
                    TraderName_Edit = SelectedTrader_Edit.namecn;
                }
            }
        }

        /// <summary>
        /// 获取机构
        /// </summary>
        /// <returns></returns>
        private List<CrmInst> GetCrmInst()
        {
            List<CrmInst> result = new List<CrmInst>();
            Dictionary<string, STC.Common.Model.Table.QuoteTag> dic = BaseHandler.Instance.GetQuoteTagDic();

            if (dic != null && dic.ContainsKey("ORG_TYPE") && dic["ORG_TYPE"] != null
                && dic["ORG_TYPE"].childTags != null && dic["ORG_TYPE"].childTags.Count > 0)
            {
                foreach (STC.Common.Model.Table.ChildTag ct in dic["ORG_TYPE"].childTags)
                {
                    CrmInst bankCrm = new CrmInst();
                    bankCrm.shortnamecn = ct.childTagName;
                    bankCrm.instcode = ct.childTagValue;
                    result.Add(bankCrm);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取VIP标志和客户类型
        /// </summary>
        /// <returns></returns>
        public TagData GetTagData()
        {
            TagData tagData = new TagData();

            if (CheckVIP)
            {
                tagData.VIP = "0001";
            }
            else
            {
                tagData.VIP = "0010";
            }

            tagData.ORG_TYPE = selectClientTypeItem != null ? selectClientTypeItem.instcode : string.Empty;


            return tagData;
        }
        #endregion

        #region Command
        /// <summary>
        /// 取消关闭
        /// </summary>
        private void OnCloseCommand()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (myWindow != null)
                    myWindow.Close();
            }));
        }

        /// <summary>
        /// 发布
        /// </summary>
        private async void OnPublishCommand()
        {
            bool isok = false;
            isok = await PublishOrder();
            if (isok)
            {
                // 关闭
                OnCloseCommand();
            }
        }

        /// <summary>
        /// 报价前检查参数设置
        /// </summary>
        /// <returns></returns>
        private bool CheckParam()
        {
            if (myWindow == null)
                return false;

            bool result = true;
            TagConditionViewModel tagconditionviewmodel = (myWindow as InnerFuzzyRequireView).TagConditionView.DataContext as TagConditionViewModel;
            BaseReplyPo replyPo = tagconditionviewmodel.GetErrorText();
            if (replyPo.Code == "10000")
                return true;

            if (string.IsNullOrEmpty(BondTxt))
            {
                CustomMessageBox.Error($"发布需求失败, 原因:\r\n必须输入识别文本");
                return false;
            }

            List<TPlaceOrderResult> orderRisks = null;
            try
            {
                orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                StringBuilder sb = new StringBuilder();
                foreach (TPlaceOrderResult por in orderRisks)
                {
                    sb.AppendLine(por.Reason);
                }

                CustomMessageBox.Error($"发布需求失败, 原因:\r\n{sb.ToString()}");
                return false;
            }
            catch (Exception ex)
            { }

           

            return result;
        }

        /// <summary>
        /// 批量发布
        /// </summary>
        /// <param name="tmp"></param>
        /// <returns></returns>
        private async Task<StringBuilder> SubPublishOrders(List<NewOrderSingle> reqlst)
        {
            StringBuilder error = new StringBuilder();

            await Task.Run(() =>
            {
                if (reqlst != null)
                {
                    int count = reqlst.Count;
                    Task[] lstTask = new Task[count];
                    ConcurrentDictionary<string, RecognizeIntentionQuoteReplyPo> dic = new ConcurrentDictionary<string, RecognizeIntentionQuoteReplyPo>();
                    for (int i = 0; i < count; i++)
                    {
                        NewOrderSingle sendPricePo = reqlst[i];
                        lstTask[i] = new Task(() =>
                        {
                            sendPricePo.HandlInst = "2";
                            RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                            if (result != null && !string.IsNullOrEmpty(result.Msg))
                            {
                                dic[sendPricePo.ClOrdID] = result;
                            }
                            else
                            {
                                RecognizeIntentionQuoteReplyPo res = new RecognizeIntentionQuoteReplyPo();
                                res.Code = "-1";
                                res.Msg = "服务器接收数据失败！";
                                dic[sendPricePo.ClOrdID] = res;
                            }
                        });

                        lstTask[i].Start();
                    }

                    Task.WaitAll(lstTask);

                    foreach (KeyValuePair<string, RecognizeIntentionQuoteReplyPo> pair in dic)
                    {
                        string orderID = pair.Key;
                        RecognizeIntentionQuoteReplyPo reply = pair.Value;
                        if (reply != null)
                        {
                            if (reply.IsSuccess() == false)
                            {
                                error.AppendLine($"订单号 {orderID}:{reply.Msg}");
                            }
                        }
                    }
                }
            });


            return error;
        }


        /// <summary>
        /// 发布并继续
        /// </summary>
        private async void OnPublishAndContinueCommand()
        {
            bool isok = false;
            isok = await PublishOrder();
            if (isok )
                ResetUi();
        }


        /// <summary>
        /// 发布需求
        /// </summary>
        private async Task<bool> PublishOrder()
        {
            if (!CheckParam())
            {
                return false;
            }

            //撤单
            if (IsUpdatePrice)
            {
                if (OldOrder != null)
                {
                    OrderCancelRequest cancelPo = new OrderCancelRequest();
                    cancelPo.AlgoName = OldOrder.AlgoName;
                    cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                    cancelPo.OrderID = OldOrder.StrategyOrderId;
                    TBookCancelReplyPo reply = await CancelOrder(cancelPo);
                    if (reply != null)
                    {
                        if (reply.Code == "0000")
                        {
                        }
                        else
                        {
                            CustomMessageBox.Info(string.Format($"删除订单失败, 原因:{reply.Msg}."), "删除订单");
                            return false;
                        }
                    }
                }
            }
            //发送新订单
            NewOrderSingle req = GetNewOrderSingle();
            List<NewOrderSingle> reqlst = new List<NewOrderSingle>();
            reqlst.Add(req);

            //bool res = CustomMessageBox.Question($"是否发布需求？", "信息");
            bool res = true;
            if (ApiManager.Instance.GetDefaultApi().EnabledMsgLog == "1")
            {
                LogHandler.Info("点击发布需求按钮");
            }

            if (res == true)
            {
                StringBuilder error = null;
                error = await SubPublishOrders(reqlst);
                if (error != null && error.Length > 0)
                {
                    CustomMessageBox.Error($"发布需求失败, 原因:\r\n{error.ToString()}");
                    return false;
                }
            }

            return true;
        }

        #endregion


        #region UI搜索


        private void ResetUi()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

                this.BondTxt = string.Empty;
                TagConditionViewModel tagconditionviewmodel = (myWindow as InnerFuzzyRequireView).TagConditionView.DataContext as TagConditionViewModel;
                //恢复默认设置
                tagconditionviewmodel.InitViewModel();

                SelectedInstintuion_Edit = new CrmInst();
                InstitutionName_Edit = string.Empty;

                SelectedTrader_Edit = new CrmContact();
                TraderName_Edit = string.Empty;
                FocusTags = string.Empty;
                Side = string.Empty;
                CheckVIP = false;
                SelectClientTypeItem = new CrmInst();
                //恢复订阅过滤区
                (myWindow as InnerFuzzyRequireView).contentTxt.Focus();
            });
        }

        internal void FilterInstitution(AutoCompleteArgs args)
        {
            string currentInstId = this.SelectedTrader_Edit == null ? string.Empty : this.SelectedTrader_Edit.instid;

            List<string> instIdList = null;
            if (!string.IsNullOrEmpty(currentInstId))
            {
                if (instIdList == null)
                    instIdList = new List<string>();
                if (!instIdList.Contains(currentInstId))
                {
                    instIdList.Add(currentInstId);
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

                        List<CrmInst> selectDatas = CommonUtils.GetInstitutionsInfoPo(str, 2000, instIdList);
                        result.AddRange(selectDatas);
                    }
                }
                else
                {
                    result = CommonUtils.GetInstitutionsInfoPo(args.Pattern.Trim(), 2000, instIdList);
                }
            }
            else
            {
                List<CrmInst> myInstList = Common.Utils.CommonUtils.GetMyInstList();
                if ((instIdList == null || instIdList.Count == 0) && myInstList.Count > 0)
                {
                    result.AddRange(myInstList);
                }
                else
                {
                    result.AddRange(CommonUtils.GetInstitutionsInfoPo(instIdList));
                }
            }

            // 排序
            if (result != null)
            {
                result = result.OrderBy(item => item, new InstitutionComparer(args.Pattern)).ToList();
            }

            args.DataSource = result;
        }

        internal void FilterTraderLocal(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedInstitution_Edit == null ? string.Empty : this._selectedInstitution_Edit.idbkey;

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = CommonUtils.GetContactInfoPo(str, currentInstID);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = CommonUtils.GetContactInfoPo(args.Pattern.Trim(), currentInstID);
            }
            else
            {
                List<CrmContact> myTraderList = Common.Utils.CommonUtils.GetMyContract();
                if (string.IsNullOrEmpty(currentInstID) && myTraderList.Count > 0)
                {
                    result = myTraderList;
                }
                else
                {
                    result.AddRange(CommonUtils.GetContactInfoPo(null, currentInstID));
                }
            }

            // 排序
            if (result != null)
            {
                result = result.OrderBy(item => item, new TraderComparer(args.Pattern)).ToList();
            }

            args.DataSource = result;
        }

        internal void FilterClientType(AutoCompleteArgs args)
        {
            List<CrmInst> result = GetCrmInst();
            if (this._selectedInstitution_Edit != null && !string.IsNullOrEmpty(this._selectedInstitution_Edit.insttypevalue))
            {
                result = result.Where(item => item.shortnamecn == this._selectedInstitution_Edit.insttypevalue).ToList();
            }
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                result = result.Where(item => item.shortnamecn.Contains(args.Pattern)).ToList();
            }

            args.DataSource = result;
        }
        #endregion

        #region 发送和撤销订单

        /// <summary>
        /// 生成订单报文
        /// </summary>
        /// <returns></returns>
        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle sendPricePo = new NewOrderSingle();

            sendPricePo.AlgoName = "CROSS";
            sendPricePo.VenueTypeGW = "INNER";
            sendPricePo.Venues = "MANUAL";

            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            sendPricePo.SecurityID = BondsInfoPo == null ? "99999" : BondsInfoPo.Bond_ID;
            string symbol = string.IsNullOrEmpty(BondTxt) ? "虚拟债券" : BondTxt;
            if (symbol.Length > 40)
            {
                symbol = symbol.Substring(0, 40) + "...";
            }

            sendPricePo.Symbol = symbol;
            sendPricePo.FirmFlag = "0";
            sendPricePo.HandlInst = "1";
            sendPricePo.WholeFlag = "0";
            sendPricePo.Side = Side;
            sendPricePo.MarketIndicator = "2";
            sendPricePo.OrdType = "2";
            sendPricePo.SettlType = "2,3";
            sendPricePo.TimeInForce = "0";

            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查

            
            
            //识别
            //if (IntentionIRSHandler.Instance.isSALER())
            {
                sendPricePo.InstitutionID = SelectedInstintuion_Edit == null ? string.Empty : SelectedInstintuion_Edit.idbkey;
                //sendPricePo.InstitutionName = SelectedInstintuion_Edit == null ? string.Empty : SelectedInstintuion_Edit.shortnamecn;
                sendPricePo.InstitutionName = InstitutionName_Edit;
                sendPricePo.TraderID = SelectedTrader_Edit == null ? string.Empty : SelectedTrader_Edit.idbkey;
                //sendPricePo.TraderName = SelectedTrader_Edit == null ? string.Empty : SelectedTrader_Edit.namecn;
                sendPricePo.TraderName = TraderName_Edit;
            }

            sendPricePo.Text = BondTxt;

            TagConditionViewModel tagconditionviewmodel = (myWindow as InnerFuzzyRequireView).TagConditionView.DataContext as TagConditionViewModel;

            //撤单时间
            string info1 = tagconditionviewmodel.GetValidTimeType();

            //撤单条件
            PriceCondition priceCondition = tagconditionviewmodel.GetPriceCondition();
            if (priceCondition != null)
            {
                sendPricePo.PriceCondition = JsonConvert.SerializeObject(priceCondition);
            }
            //标签
            TagData td = tagconditionviewmodel.GetTagData();
            sendPricePo.IfSubBond = td.IF_SUB_BOND;

            TagData clientTagData = GetTagData();
            sendPricePo.OrgType = clientTagData.ORG_TYPE;
            sendPricePo.VipCode = clientTagData.VIP;

            //默认 "0|" : 时长 "1|{min}:{sec}" : 时点  "2|{hour}:{min}:{sec}" 无 "3|"
            string validtimetype = tagconditionviewmodel.GetValidTimeType();
            //yyyy-MM-dd HH:mm:ss
            string validuntiltime = tagconditionviewmodel.GetValidUntilTime();
            OrderTimeModel otm = new OrderTimeModel() {ValidTimeType = info1, ValidUntilTime = validuntiltime, IF_SUB_BOND = sendPricePo.IfSubBond ?? string.Empty, VIP = sendPricePo.VipCode ?? string.Empty, ORG_TYPE = sendPricePo.OrgType ?? string.Empty, MorethanMarketBPCheck = priceCondition.MorethanMarketBPCheck ?? string.Empty, MorethanMarketBP = priceCondition.MorethanMarketBP ?? string.Empty, FocusTags = FocusTags, Text= BondTxt, QuoteType = "virtual", IsFromIRSorInner = "irs"};
            sendPricePo.Info1 = JsonConvert.SerializeObject(otm);
            sendPricePo.ValidUntilTime = validuntiltime;

            sendPricePo.Info2 = FocusTags;

            sendPricePo.Terminal = string.Format("STClient_{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.UserName = LocalEnv.UserInfo.UserName;
            sendPricePo.Name = LocalEnv.UserInfo.UserName;
            sendPricePo.QuoteType = "virtual";
            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

            return sendPricePo;
        }

        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="cancelPo"></param>
        /// <returns></returns>
        public async Task<TBookCancelReplyPo> CancelOrder(STC.Common.Model.TBook.OrderCancelRequest cancelPo)
        {
            TBookCancelReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = OrdersHandler.Instance.CancelOrder(cancelPo);

            });
            return reply;
        }

        #endregion

    }
}
