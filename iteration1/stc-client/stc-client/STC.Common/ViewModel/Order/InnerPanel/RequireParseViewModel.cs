using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.CRM;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static STC.Common.View.Component.AutoCompleteComboBox;

namespace STC.Common.ViewModel.Order.InnerPanel
{
    public class RequireParseViewModel : BaseViewModel
    {
        #region 构造
        private string securityID = string.Empty;
        public string SecurityID
        {
            get { return securityID; }
            set { securityID = value; this.OnPropertyChanged(); }
        }

        private string bondTxt = string.Empty;
        public string BondTxt
        {
            get { return bondTxt; }
            set { bondTxt = value; this.OnPropertyChanged(); }
        }

        private string priceType = string.Empty;

        public string PriceType
        {
            get { return priceType;  }
            set {
                priceType = value;
                this.OnPropertyChanged();
            }
        }

        private string clientInsName;
        public string ClientInsName
        {
            get { return clientInsName; }
            set { clientInsName = value; this.OnPropertyChanged(); }
        }


        private string clientName;
        public string ClientName
        {
            get { return clientName; }
            set { clientName = value; this.OnPropertyChanged(); }
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

        private bool checkVIP;
        public bool CheckVIP
        {
            get { return checkVIP; }
            set { checkVIP = value; this.OnPropertyChanged(); }
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

        private StcTbookUsers selectInvestorItem;

        public StcTbookUsers SelectInvestorItem
        {
            get { return selectInvestorItem; }
            set
            {
                selectInvestorItem = value;
                this.OnPropertyChanged();
            }
        }

        private string selectInvestorText;

        public string SelectInvestorText
        {
            get { return selectInvestorText; }
            set
            {
                selectInvestorText = value;
                this.OnPropertyChanged();
            }
        }

        private StcTbookUsers selectTraderItem;

        public StcTbookUsers SelectTraderItem
        {
            get { return selectTraderItem; }
            set
            {
                selectTraderItem = value;
                this.OnPropertyChanged();
            }
        }

        private string selectTraderText;

        public string SelectTraderText
        {
            get { return selectTraderText;  }
            set
            {
                selectTraderText = value;
                this.OnPropertyChanged();
            }
        }


        private bool isenableparse = true;
        public bool IsEnableParse
        {
            get { return isenableparse; }
            set { isenableparse = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0xED, 0x6D, 0x00));
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                    this.OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }


        private bool isirs = false;
        /// <summary>
        /// 是否是IRS识别
        /// </summary>
        public bool IsIRS
        {
            get { return isirs; }
            set { isirs = value; this.OnPropertyChanged(); }
        }

        #region Broker
        private StcTbookUsers _selectedBroker_Edit;
        /// <summary>
        /// 选择的Broker
        /// </summary>
        public StcTbookUsers SelectedBroker_Edit
        {
            get { return _selectedBroker_Edit; }
            set
            {
                if (_selectedBroker_Edit != value)
                {
                    _selectedBroker_Edit = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _brokerName_Edit;


        /// <summary>
        /// 显示的BrokerName
        /// </summary>
        public string BrokerName_Edit
        {
            get { return _brokerName_Edit; }
            set
            {
                _brokerName_Edit = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        public event Action<List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>> OnParse;
        public event Action<object> BeginParse;
        #endregion

        public RequireParseViewModel()
        {
            SetBrokerName(LocalEnv.UserInfo.UserName);
        }

        private char[] enterSplitChar = new char[] { '\n','\r' };

        public void RecognizeIntentionQuote(string content)
        {
            ThreadPoolHelper.InvokeThreadPool(o => {
                IsEnableParse = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        BeginParse?.Invoke(new object());
                        STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                        request.userName = LocalEnv.UserInfo.UserName;
                        request.content = content;

                        STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = null;
                        ///获取订单 
                        if (IsIRS)
                        {
                            string[] lsstring = content.Split(enterSplitChar, StringSplitOptions.RemoveEmptyEntries);
                            
                            if(lsstring != null && lsstring.Length > 0)
                            {
                                List<string> ls = new List<string>();
                                ls.Add(lsstring[0]);
                                request.contentList = ls;
                                reply = ParserHandler.Instance.RequestParserIRS(request);
                            }
                        }
                        else
                            reply = ParserHandler.Instance.RequestParserBond(request);
                        

                        if (reply == null)
                        {
                            IsEnableParse = true;
                            CommonUtils.Prompt2Second("警告", $"报价识别失败");
                        }
                        else
                        {
                            if (!reply.IsSuccess())
                            {
                                IsEnableParse = true;
                                CommonUtils.Prompt2Second("警告", reply.Msg);
                            }
                            else if (reply.IsSuccess())
                            {
                                IsEnableParse = true;
                                SetParseData(reply);
                                OnParse?.Invoke(reply.Datas);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        IsEnableParse = true;
                        LogHelper.Error("RequestOrders", e);
                    }
                });
            });
        }

        private void SetParseData(RecognizeIntentionQuoteReplyPo replyPo)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (replyPo != null && replyPo.Datas != null && replyPo.Datas.Count > 0)
                {
                    IntentionQuoteInfo intentionQuoteInfo = replyPo.Datas[0];

                    PriceType = intentionQuoteInfo.priceType;

                    List<CrmInst> crmInsts = null;
                    if (!string.IsNullOrEmpty(intentionQuoteInfo.parseCompanyIdbkey))
                        crmInsts = CommonUtils.GetInstitutionsIDInfoPo(intentionQuoteInfo.parseCompanyIdbkey);


                    List<CrmContact> crmContacts = null;
                    if (!string.IsNullOrEmpty(intentionQuoteInfo.parseCompanyIdbkey) && !string.IsNullOrEmpty(intentionQuoteInfo.parseTraderIdbkey))
                        crmContacts = CommonUtils.GetContactInfoPo(intentionQuoteInfo.parseTraderIdbkey, intentionQuoteInfo.parseCompanyIdbkey);



                    if (crmInsts != null && crmInsts.Count > 0)
                    {
                        SelectedInstintuion_Edit = crmInsts[0];

                        InstitutionName_Edit = SelectedInstintuion_Edit.shortnamecn;
                    }

                    if (crmContacts != null && crmContacts.Count > 0)
                    {
                        SelectedTrader_Edit = crmContacts[0];

                        TraderName_Edit = SelectedTrader_Edit.namecn;
                    }

                }
            }));
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

        /// <summary>
        /// 过滤交易员
        /// </summary>
        /// <param name="args"></param>
        internal void FilterBrokerType(AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            STCUserReplyPo result = AdminHandler.Instance.GetCachestcUserQuery();

            if (!string.IsNullOrEmpty(args.Pattern))
            {
                list = result.Datas.Where(d => d.ActionIds.Contains("600102") 
                && ((d.Username != null && d.Username.Contains(args.Pattern)) || (d.Name != null && d.Name.Contains(args.Pattern)))).ToList();//多条件筛选，新功能增加
            }
            else
            {
                list = result.Datas.Where(d => d.ActionIds.Contains("600102")).ToList();
            }

            args.DataSource = list;
        }

        /// <summary>
        /// 设置BrokerName
        /// </summary>
        /// <param name="brokername"></param>
        public void SetBrokerName(string brokername = "")
        {
            if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
            {
                string username = string.IsNullOrEmpty(brokername) ? LocalEnv.UserInfo.UserName : brokername;
                
                if (AdminHandler.Instance.GetCachestcUserQuery().Datas.Any(item => item.Username == username))
                {
                    StcTbookUsers currentUser = AdminHandler.Instance.GetCachestcUserQuery().Datas.First(item => item.Username == username);
                    this.SelectedBroker_Edit = currentUser;
                    this.BrokerName_Edit = currentUser.Name;
                }
            }
        }

        /// <summary>
        /// 获取BrokerName
        /// </summary>
        /// <returns></returns>
        public string GetBrokerName()
        {
            if (SelectedBroker_Edit != null)
                return SelectedBroker_Edit.Username;
            else
                return string.Empty;
        }

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

        /// <summary>
        /// 设置Vip和客户类型
        /// </summary>
        /// <param name="info2"></param>
        public void SetTagData(string info2)
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
        public void SetInstitutionAndTrader(PriceMatchParam pm)
        {
            if (!string.IsNullOrEmpty(pm.InstitutionID))
            {
                List<CrmInst> crmInsts = CommonUtils.GetInstitutionsIDInfoPo(pm.InstitutionID);
                if (crmInsts != null && crmInsts.Count > 0)
                {
                    SelectedInstintuion_Edit = crmInsts[0];
                    InstitutionName_Edit = SelectedInstintuion_Edit.shortnamecn;
                }
                else
                {
                    SelectedInstintuion_Edit = new CrmInst();
                    SelectedInstintuion_Edit.idbkey = pm.InstitutionID;
                    SelectedInstintuion_Edit.shortnamecn = pm.InstitutionName;
                    InstitutionName_Edit = pm.InstitutionName;
                }
            }
            else
            {
                InstitutionName_Edit = pm.InstitutionName;
            }

            if (!string.IsNullOrEmpty(pm.TraderID))
            {
                List<CrmContact> crmContacts = CommonUtils.GetContactInfoPo(pm.TraderID, pm.InstitutionID);
                if (crmContacts != null && crmContacts.Count > 0)
                {
                    SelectedTrader_Edit = crmContacts[0];
                    TraderName_Edit = SelectedTrader_Edit.namecn;
                }
                else
                {
                    SelectedTrader_Edit = new CrmContact();
                    SelectedTrader_Edit.idbkey = pm.TraderID;
                    SelectedTrader_Edit.namecn = pm.TraderName;
                    TraderName_Edit = pm.TraderName;
                }
            }
            else
            {
                TraderName_Edit = pm.TraderName;
            }

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                StcTbookUsers trader = null;
                if (!string.IsNullOrEmpty(pm.buyerTraderId))
                {
                    trader = CommonUtils.GetTraders().FirstOrDefault(e => e.Username == pm.buyerTraderId);
                }

                if (trader != null)
                {
                    SelectTraderItem = trader;
                    SelectTraderText = trader.Name;
                }
                else
                {
                    if (string.IsNullOrEmpty(pm.buyerTraderName) && !CommonUtils.IsInvestor())
                    {
                        SelectTraderItem = CommonUtils.GetTraders().FirstOrDefault(e => e.Username == LocalEnv.UserInfo.UserName);
                        SelectTraderText = LocalEnv.UserInfo.FullName;
                    }
                    else
                    {
                        if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsI2T && string.IsNullOrEmpty(pm.buyerTraderName) && !string.IsNullOrEmpty(pm.Symbol) && CommonUtils.IsInvestor())
                        {
                            string type = CommonUtils.ConvertToPAType(pm.Symbol);


                            List<string> traderList =  AdminHandler.Instance.GetTraderList(LocalEnv.UserInfo.UserId, type);
                            if (traderList != null)
                            {
                                List<StcTbookUsers> traders = Common.Utils.CommonUtils.GetTraders();
                                List<StcTbookUsers> list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                                if (list != null && list.Count > 0)
                                {
                                    StcTbookUsers stcTbookUsers = list[0];
                                    SelectTraderItem = stcTbookUsers;
                                    SelectTraderText = stcTbookUsers.Name;
                                }
                            }
                        }
                        else
                        {
                            SelectTraderItem = null;
                            SelectTraderText = pm.buyerTraderName;
                        }
                    }
                }


                StcTbookUsers investor = null;
                if (!string.IsNullOrEmpty(pm.managerId))
                {
                    investor = CommonUtils.GetInvestors().FirstOrDefault(e => e.Username == pm.managerId);
                }
                if (investor != null)
                {
                    SelectInvestorItem = investor;
                    SelectInvestorText = investor.Name;
                }
                else
                {
                    if (string.IsNullOrEmpty(pm.managerName) && CommonUtils.IsInvestor())
                    {
                        SelectInvestorItem = CommonUtils.GetInvestors().FirstOrDefault(e => e.Username == LocalEnv.UserInfo.UserName);
                        SelectInvestorText = LocalEnv.UserInfo.FullName;
                    }
                    else
                    {
                        SelectInvestorItem = null;
                        SelectInvestorText = pm.managerName;
                    }
                }
            }
        }

        private List<CrmInst> GetCrmInst()
        {
            List<CrmInst> result = new List<CrmInst>();
            Dictionary<string, Model.Table.QuoteTag> dic = BaseHandler.Instance.GetQuoteTagDic();

            if (dic != null && dic.ContainsKey("ORG_TYPE") && dic["ORG_TYPE"] != null
                && dic["ORG_TYPE"].childTags != null && dic["ORG_TYPE"].childTags.Count > 0)
            {
                foreach (Model.Table.ChildTag ct in dic["ORG_TYPE"].childTags)
                {
                    CrmInst bankCrm = new CrmInst();
                    bankCrm.shortnamecn = ct.childTagName;
                    bankCrm.instcode = ct.childTagValue;
                    result.Add(bankCrm);
                }
            }
            
            return result;
        }
        
    }
}
