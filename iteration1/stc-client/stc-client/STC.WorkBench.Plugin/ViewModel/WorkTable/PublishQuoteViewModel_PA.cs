using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Data;
using STC.Common.Handler.FinQ;
using STC.Common.Handler.PT;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Drp;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Common.ViewModel.WorkTable;
using STC.WorkBench.Plugin.View;
using STC.WorkBench.Plugin.View.WorkTable;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml.Serialization;
using static STC.Common.View.Component.AutoCompleteComboBox;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class PublishQuoteViewModel_PA : ViewModelBase
    {
        public ICommand DelQuoteCommand { get; set; }

        public ICommand SendQuoteCommand { get; set; }




        #region 清算速度
        private ObservableCollection<string> _SettlTypeList = new ObservableCollection<string>();

        public ObservableCollection<string> SettlTypeList
        {
            get
            {
                if (_SettlTypeList.Count == 0)
                {
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        _SettlTypeList.Add("不限");
                        _SettlTypeList.Add("T+1");
                        _SettlTypeList.Add("明+0");
                    }
                    else
                    {
                        _SettlTypeList.Add("+1");
                        _SettlTypeList.Add("今+1");
                        _SettlTypeList.Add("明+0");
                    }
                }
                return _SettlTypeList;
            }
        }
        #endregion

        #region 表格数据源
        internal object _dataLock = new object();
        //private CollectionViewSource _viewSource = new CollectionViewSource();

        //public CollectionViewSource ViewSource
        //{
        //    get { return _viewSource; }
        //    set { _viewSource = value; }
        //}

        private ObservableCollection<IntentionMarketItem> _datas = new ObservableCollection<IntentionMarketItem>();
        public ObservableCollection<IntentionMarketItem> Datas
        {
            get { return _datas; }
            set { _datas = value; }
        }
        #endregion

        #region To相关内容

        private List<string> bncList = new List<string>();
        private List<string> bcoList = new List<string>();
        private List<string> localList = new List<string>();
        private List<string> ncdList = new List<string>();
        private List<string> counterPartiesList = new List<string>();
        private List<string> contactsList = new List<string>();
        private List<string> qqGroupsList = new List<string>();

        private ObservableCollection<GroupsData> _customGroups = new ObservableCollection<GroupsData>();

        public ObservableCollection<GroupsData> CustomGroups
        {
            get { return _customGroups; }
            set { _customGroups = value; }
        }

        #endregion

        #region 经纪商显示参数

        // 利率
        private bool _isEnabled_BNC = false;
        public bool IsEnabled_BNC
        {
            get { return _isEnabled_BNC; }
            set
            {
                _isEnabled_BNC = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _visibility_BNC = Visibility.Visible;

        public Visibility Visibility_BNC
        {
            get { return _visibility_BNC; }
            set
            {
                _visibility_BNC = value;
                this.OnPropertyChanged();
            }
        }

        // 地方债
        private bool _isEnabled_LOCAL = false;
        public bool IsEnabled_LOCAL
        {
            get { return _isEnabled_LOCAL; }
            set
            {
                _isEnabled_LOCAL = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _visibility_LOCAL = Visibility.Visible;

        public Visibility Visibility_LOCAL
        {
            get { return _visibility_LOCAL; }
            set
            {
                _visibility_LOCAL = value;
                this.OnPropertyChanged();
            }
        }

        // 信用
        private bool _isEnabled_BCO = false;
        public bool IsEnabled_BCO
        {
            get { return _isEnabled_BCO; }
            set
            {
                _isEnabled_BCO = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _visibility_BCO = Visibility.Visible;

        public Visibility Visibility_BCO
        {
            get { return _visibility_BCO; }
            set
            {
                _visibility_BCO = value;
                this.OnPropertyChanged();
            }
        }

        // NCD
        private bool _isEnabled_NCD = false;
        public bool IsEnabled_NCD
        {
            get { return _isEnabled_NCD; }
            set
            {
                _isEnabled_NCD = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _visibility_NCD = Visibility.Visible;

        public Visibility Visibility_NCD
        {
            get { return _visibility_NCD; }
            set
            {
                _visibility_NCD = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        List<string> securityTypeList = new List<string>();

        public PublishQuoteViewModel_PA()
        {
            string qqNumber = CommonUtils.GetQQNumber(LocalEnv.UserInfo.UserName);
            senderNumber = qqNumber; //"3003945772";// "3003945735";

            DelQuoteCommand = new ComRelayCommand<Button>(this.DoDelQuoteCommand);
            SendQuoteCommand = new ComRelayCommand<Button>(this.DoSendQuoteCommand);
        }

        PublishQuoteView_PA _owner;
        public void Init(IList SelectedItems, PublishQuoteView_PA view)
        {
            try
            {
                _owner = view;
                List<string> glist = new List<string>();
                foreach (IntentionQuoteItem intentionItem in SelectedItems)
                {
                    if (intentionItem.Status != "80" && intentionItem.Status != "81" && intentionItem.Status != "82")
                    {
                        continue;
                    }

                    if (glist.Contains(intentionItem.GroupKey)) continue;
                    glist.Add(intentionItem.GroupKey);

                    IntentionMarketItem item = new IntentionMarketItem();
                    item.RemainingTime = intentionItem.RemainingTime;
                    item.RemaingingTime_convert = intentionItem.RemaingingTime_convert;
                    item.SecurityId = intentionItem.SecurityId;
                    item.Symbol = intentionItem.Symbol;
                    item.Side = intentionItem.Side;
                    if (intentionItem.Type == "INTENTION_GROUP" || intentionItem.Type == "INTENTION_DETAIL")
                    {
                        IEnumerable<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> list = IntentionOldHandler.Instance.GetSameGroupDataList(intentionItem.QuoteType, intentionItem.Trader_Buyer, intentionItem.SecurityId, intentionItem.Side);
                        if (list != null)
                        {
                            list = list.Where(x => x.status == "81");
                        }

                        STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo tmpInfo = null;
                        List<decimal?> pricelist = new List<decimal?>();

                        if (list != null)
                        {
                            int count_Yield = 0;
                            int count_cleanPrice = 0;
                            foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo sub_info in list)
                            {
                                if (sub_info.priceType == "1")
                                {
                                    count_cleanPrice++;
                                }
                                else if (sub_info.priceType == "2")
                                {

                                }
                                else
                                {
                                    count_Yield++;
                                }
                            }

                            foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo sub_info in list)
                            {
                                if (count_Yield == 0)
                                {
                                    //if (!sub_info.cleanPrice.HasValue)
                                    //    continue;

                                    if (!pricelist.Contains(sub_info.cleanPrice))
                                        pricelist.Add(sub_info.cleanPrice);
                                }
                                else
                                {
                                    if (count_cleanPrice > 0)
                                    {
                                        if (sub_info.priceType == "1")
                                        {
                                            continue;
                                        }
                                    }
                                    //if (!sub_info.price.HasValue)
                                    //    continue;

                                    if (!pricelist.Contains(sub_info.price))
                                        pricelist.Add(sub_info.price);
                                }


                                if (tmpInfo == null)
                                {
                                    tmpInfo = sub_info;
                                }

                                if (tmpInfo != null)
                                {
                                    if (intentionItem.Side == "1")
                                    {
                                        if (count_Yield == 0)
                                        {
                                            decimal tmpCleanPrice = tmpInfo.cleanPrice.HasValue ? tmpInfo.cleanPrice.Value : decimal.MinValue;
                                            decimal cleanPrice = sub_info.cleanPrice.HasValue ? sub_info.cleanPrice.Value : decimal.MinValue;

                                            if (tmpCleanPrice < cleanPrice)
                                            {
                                                tmpInfo = sub_info;
                                            }
                                        }
                                        else
                                        {
                                            decimal tmpYield = tmpInfo.price.HasValue ? tmpInfo.price.Value : decimal.MaxValue;
                                            decimal yield = sub_info.price.HasValue ? sub_info.price.Value : decimal.MaxValue;

                                            if (tmpYield > yield)
                                            {
                                                tmpInfo = sub_info;
                                            }
                                        }
                                    }
                                    else if (intentionItem.Side == "2")
                                    {
                                        if (count_Yield == 0)
                                        {
                                            decimal tmpCleanPrice = tmpInfo.cleanPrice.HasValue ? tmpInfo.cleanPrice.Value : decimal.MaxValue;
                                            decimal cleanPrice = sub_info.cleanPrice.HasValue ? sub_info.cleanPrice.Value : decimal.MaxValue;

                                            if (tmpCleanPrice > cleanPrice)
                                            {
                                                tmpInfo = sub_info;
                                            }
                                        }
                                        else
                                        {
                                            decimal tmpYield = tmpInfo.price.HasValue ? tmpInfo.price.Value : decimal.MinValue;
                                            decimal yield = sub_info.price.HasValue ? sub_info.price.Value : decimal.MinValue;

                                            if (tmpYield < yield)
                                            {
                                                tmpInfo = sub_info;
                                            }
                                        }
                                    }
                                }
                            }

                            string priceType = string.Empty;
                            decimal? price = null;
                            decimal? qty = null;

                            if (pricelist.Count == 1)
                            {
                                price = pricelist[0];
                                if (count_Yield > 0)
                                {
                                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo e in list)
                                    {
                                        if (e.priceType != "1" && e.priceType != "2")
                                        {
                                            decimal matchQty = 0;
                                            if (JsonSplit.IsJson(e.info2))
                                            {
                                                JObject jsonObj = JObject.Parse(e.info2);
                                                if (jsonObj.ContainsKey("MATCHED_QUANTITY"))
                                                {
                                                    matchQty = jsonObj["MATCHED_QUANTITY"].ConvertToDecimal();
                                                }
                                            }
                                            if (!qty.HasValue)
                                            {
                                                qty = e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                            }
                                            else
                                            {
                                                qty += e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                            }
                                        }

                                    }
                                    //qty = list.Sum(e => (e.priceType != "1" && e.priceType != "2") ? e.qty : 0);
                                }
                                else if (count_cleanPrice > 0)
                                {
                                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo e in list)
                                    {
                                        if (e.cleanPrice == price)
                                        {
                                            decimal matchQty = 0;
                                            if (JsonSplit.IsJson(e.info2))
                                            {
                                                JObject jsonObj = JObject.Parse(e.info2);
                                                if (jsonObj.ContainsKey("MATCHED_QUANTITY"))
                                                {
                                                    matchQty = jsonObj["MATCHED_QUANTITY"].ConvertToDecimal();
                                                }
                                            }
                                            if (!qty.HasValue)
                                            {
                                                qty = e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                            }
                                            else
                                            {
                                                qty += e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                            }
                                        }
                                    }
                                    //qty = list.Sum(e => e.cleanPrice == price ? e.qty : 0);
                                }
                            }
                            else
                            {
                                if (tmpInfo != null)
                                {
                                    if (tmpInfo.priceType == "1")
                                    {
                                        price = tmpInfo.cleanPrice;
                                        foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo e in list)
                                        {
                                            if (e.cleanPrice == price)
                                            {
                                                decimal matchQty = 0;
                                                if (JsonSplit.IsJson(e.info2))
                                                {
                                                    JObject jsonObj = JObject.Parse(e.info2);
                                                    if (jsonObj.ContainsKey("MATCHED_QUANTITY"))
                                                    {
                                                        matchQty = jsonObj["MATCHED_QUANTITY"].ConvertToDecimal();
                                                    }
                                                }
                                                if (!qty.HasValue)
                                                {
                                                    qty = e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                                }
                                                else
                                                {
                                                    qty += e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                                }
                                            }
                                        }
                                        //qty = list.Sum(e => e.cleanPrice == price ? e.qty : 0);
                                    }
                                    else if (tmpInfo.priceType == "2")
                                    {
                                        price = tmpInfo.fullPrice;
                                        foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo e in list)
                                        {
                                            if (e.fullPrice == price)
                                            {
                                                decimal matchQty = 0;
                                                if (JsonSplit.IsJson(e.info2))
                                                {
                                                    JObject jsonObj = JObject.Parse(e.info2);
                                                    if (jsonObj.ContainsKey("MATCHED_QUANTITY"))
                                                    {
                                                        matchQty = jsonObj["MATCHED_QUANTITY"].ConvertToDecimal();
                                                    }
                                                }
                                                if (!qty.HasValue)
                                                {
                                                    qty = e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                                }
                                                else
                                                {
                                                    qty += e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                                }
                                            }
                                        }
                                        //qty = list.Sum(e => e.fullPrice == price ? e.qty : 0);
                                    }
                                    else
                                    {
                                        price = tmpInfo.price;
                                        foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo e in list)
                                        {
                                            if (e.price == price)
                                            {
                                                decimal matchQty = 0;
                                                if (JsonSplit.IsJson(e.info2))
                                                {
                                                    JObject jsonObj = JObject.Parse(e.info2);
                                                    if (jsonObj.ContainsKey("MATCHED_QUANTITY"))
                                                    {
                                                        matchQty = jsonObj["MATCHED_QUANTITY"].ConvertToDecimal();
                                                    }
                                                }
                                                if (!qty.HasValue)
                                                {
                                                    qty = e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                                }
                                                else
                                                {
                                                    qty += e.qty.HasValue ? e.qty.Value - matchQty : 0 - matchQty;
                                                }
                                            }
                                        }
                                        //qty = list.Sum(e => e.price == price ? e.qty : 0);
                                    }

                                    priceType = tmpInfo.priceType;
                                }
                            }

                            item.Price = item.Price_Edit = price;
                            item.Qty = item.Qty_Edit = qty;
                            BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(item.SecurityId);
                            if (bondsInfoPo != null)
                            {
                                // 含权
                                if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                                ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                                (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                                {
                                    if (item.PriceTypeDic.Count == 2)
                                    {
                                        item.PriceTypeDic.Add(new KeyValuePair<string, string>("7", "行权"));
                                    }
                                    item.PriceType = item.PriceType_Edit = string.IsNullOrEmpty(intentionItem.PriceType) ? "7" : intentionItem.PriceType;
                                }
                                else
                                {
                                    item.PriceType = item.PriceType_Edit = string.IsNullOrEmpty(intentionItem.PriceType) ? "3" : intentionItem.PriceType;
                                    if (item.PriceTypeDic.Count == 3)
                                    {
                                        item.PriceTypeDic.RemoveAt(2);
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(priceType))
                            {
                                item.PriceType = item.PriceType_Edit = priceType;
                            }
                            item.Trader_Buyer = intentionItem.Trader_Buyer;
                            item.TraderName_Buyer = intentionItem.TraderName_Buyer;
                            item.GroupKey = item.IntentionId = intentionItem.GroupKey;
                        }
                    }
                    else
                    {
                        item.intentionItem = intentionItem;
                        item.Qty = item.Qty_Edit = intentionItem.Qty - intentionItem.MatchedQty;
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(item.SecurityId);
                        if (bondsInfoPo != null)
                        {
                            // 含权
                            if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                            {
                                if (item.PriceTypeDic.Count == 2)
                                {
                                    item.PriceTypeDic.Add(new KeyValuePair<string, string>("7", "行权"));
                                }
                                item.PriceType = item.PriceType_Edit = string.IsNullOrEmpty(intentionItem.PriceType) ? "7" : intentionItem.PriceType;
                            }
                            else
                            {
                                item.PriceType = item.PriceType_Edit = string.IsNullOrEmpty(intentionItem.PriceType) ? "3" : intentionItem.PriceType;
                                if (item.PriceTypeDic.Count == 3)
                                {
                                    item.PriceTypeDic.RemoveAt(2);
                                }
                            }
                        }
                        item.PriceType = item.PriceType_Edit = string.IsNullOrEmpty(intentionItem.PriceType) ? "3" : intentionItem.PriceType;
                        if (item.PriceType == "1")
                        {
                            item.Price = item.Price_Edit = intentionItem.CleanPrice;
                        }
                        else if (item.PriceType == "2")
                        {
                            item.Price = item.Price_Edit = intentionItem.FullPrice;
                        }
                        else
                        {
                            item.Price = item.Price_Edit = intentionItem.Price;
                        }
                        item.Trader_Buyer = intentionItem.Trader_Buyer;
                        item.TraderName_Buyer = intentionItem.TraderName_Buyer;
                        item.GroupKey = item.IntentionId = intentionItem.GroupKey;
                    }
                    //item.Price_Edit = intentionItem.Price;
                    //item.PriceType_Edit = intentionItem.PriceType;
                    //item.Qty_Edit = intentionItem.Qty;
                    //item.SettlType_Edit = intentionItem.SettlType;
                    //item.HurryFlag_Edit = intentionItem.HurryFlag;
                    //item.SuggestFlag_Edit = intentionItem.SuggestFlag;
                    //item.FireFlag_Edit = intentionItem.FireFlag;
                    //item.FloorPrice_Edit = intentionItem.FloorPrice;
                    //item.PFlag_Edit = intentionItem.PFlag;
                    //item.OCOFlag_Edit = intentionItem.OCOFlag;
                    //item.Remark_Edit = intentionItem.Remark;
                    lock (_dataLock)
                    {
                        Datas.Add(item);
                    }

                    if (intentionItem.SignDic1 != null && intentionItem.SignDic1.ContainsKey(Consts.BOND_SUB_TYPE))
                    {
                        string bondSubType_tag = intentionItem.SignDic1[Consts.BOND_SUB_TYPE];
                        string bondType = CommonUtils.GetQQSecurityType(bondSubType_tag, intentionItem.BondSubType);
                        if (!securityTypeList.Contains(bondType))
                        {
                            securityTypeList.Add(bondType);
                        }
                        item.BondType = bondType;
                    }
                }

                // 1:信用;2:利率债(其他);3：NCD;21：地方债
                if (securityTypeList.Count == 1)
                {
                    IsEnabled_BNC = true;
                    IsEnabled_LOCAL = true;
                    IsEnabled_BCO = true;
                    IsEnabled_NCD = true;
                    if (securityTypeList[0] == "1")
                    {
                        Visibility_BNC = Visibility.Hidden;
                        Visibility_LOCAL = Visibility.Hidden;
                        Visibility_BCO = Visibility.Visible;
                        Visibility_NCD = Visibility.Hidden;
                    }
                    else if (securityTypeList[0] == "2")
                    {
                        Visibility_BNC = Visibility.Visible;
                        Visibility_LOCAL = Visibility.Hidden;
                        Visibility_BCO = Visibility.Hidden;
                        Visibility_NCD = Visibility.Hidden;
                    }
                    else if (securityTypeList[0] == "3")
                    {
                        Visibility_BNC = Visibility.Hidden;
                        Visibility_LOCAL = Visibility.Hidden;
                        Visibility_BCO = Visibility.Hidden;
                        Visibility_NCD = Visibility.Visible;
                    }
                    else if (securityTypeList[0] == "21")
                    {
                        Visibility_BNC = Visibility.Hidden;
                        Visibility_LOCAL = Visibility.Visible;
                        Visibility_BCO = Visibility.Hidden;
                        Visibility_NCD = Visibility.Hidden;
                    }
                    else
                    {
                        Visibility_BNC = Visibility.Hidden;
                        Visibility_LOCAL = Visibility.Hidden;
                        Visibility_BCO = Visibility.Hidden;
                        Visibility_NCD = Visibility.Hidden;
                    }
                }
                else
                {
                    IsEnabled_BNC = false;
                    IsEnabled_LOCAL = false;
                    IsEnabled_BCO = false;
                    IsEnabled_NCD = false;
                    Visibility_BNC = Visibility.Visible;
                    Visibility_LOCAL = Visibility.Visible;
                    Visibility_BCO = Visibility.Visible;
                    Visibility_NCD = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }

            QueryFQUserConfig();
        }

        public void InitRenew(IntentionMarketItem inquiryItem, PublishQuoteView_PA view)
        {
            try
            {
                _owner = view;
                List<string> glist = new List<string>();


                IntentionMarketItem item = new IntentionMarketItem();
                item.RemainingTime = inquiryItem.RemainingTime;
                item.RemaingingTime_convert = inquiryItem.RemaingingTime_convert;
                item.SecurityId = inquiryItem.SecurityId;
                item.Symbol = inquiryItem.Symbol;
                item.Side = inquiryItem.Side;
                item.BondType = inquiryItem.BondType;

                item.intentionItem = inquiryItem.intentionItem;
                item.Qty = item.Qty_Edit = inquiryItem.Qty;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(item.SecurityId);
                if (bondsInfoPo != null)
                {
                    // 含权
                    if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                    ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                    (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                    {
                        if (item.PriceTypeDic.Count == 2)
                        {
                            item.PriceTypeDic.Add(new KeyValuePair<string, string>("7", "行权"));
                        }
                        item.PriceType = item.PriceType_Edit = string.IsNullOrEmpty(inquiryItem.PriceType) ? "7" : inquiryItem.PriceType;
                    }
                    else
                    {
                        item.PriceType = item.PriceType_Edit = string.IsNullOrEmpty(inquiryItem.PriceType) ? "3" : inquiryItem.PriceType;
                        if (item.PriceTypeDic.Count == 3)
                        {
                            item.PriceTypeDic.RemoveAt(2);
                        }
                    }
                }
                item.PriceType = item.PriceType_Edit = string.IsNullOrEmpty(inquiryItem.PriceType) ? "3" : inquiryItem.PriceType;
                item.Price = item.Price_Edit = inquiryItem.Price;
                item.Trader_Buyer = inquiryItem.Trader_Buyer;
                item.TraderName_Buyer = inquiryItem.TraderName_Buyer;
                item.GroupKey = item.IntentionId = inquiryItem.GroupKey;

                lock (_dataLock)
                {
                    Datas.Add(item);
                }

                if (!securityTypeList.Contains(item.BondType))
                {
                    securityTypeList.Add(item.BondType);
                }

                // 1:信用;2:利率债(其他);3：NCD;21：地方债
                if (securityTypeList.Count == 1)
                {
                    IsEnabled_BNC = true;
                    IsEnabled_LOCAL = true;
                    IsEnabled_BCO = true;
                    IsEnabled_NCD = true;
                    if (securityTypeList[0] == "1")
                    {
                        Visibility_BNC = Visibility.Hidden;
                        Visibility_LOCAL = Visibility.Hidden;
                        Visibility_BCO = Visibility.Visible;
                        Visibility_NCD = Visibility.Hidden;
                    }
                    else if (securityTypeList[0] == "2")
                    {
                        Visibility_BNC = Visibility.Visible;
                        Visibility_LOCAL = Visibility.Hidden;
                        Visibility_BCO = Visibility.Hidden;
                        Visibility_NCD = Visibility.Hidden;
                    }
                    else if (securityTypeList[0] == "3")
                    {
                        Visibility_BNC = Visibility.Hidden;
                        Visibility_LOCAL = Visibility.Hidden;
                        Visibility_BCO = Visibility.Hidden;
                        Visibility_NCD = Visibility.Visible;
                    }
                    else if (securityTypeList[0] == "21")
                    {
                        Visibility_BNC = Visibility.Hidden;
                        Visibility_LOCAL = Visibility.Visible;
                        Visibility_BCO = Visibility.Hidden;
                        Visibility_NCD = Visibility.Hidden;
                    }
                    else
                    {
                        Visibility_BNC = Visibility.Hidden;
                        Visibility_LOCAL = Visibility.Hidden;
                        Visibility_BCO = Visibility.Hidden;
                        Visibility_NCD = Visibility.Hidden;
                    }
                }
                else
                {
                    IsEnabled_BNC = false;
                    IsEnabled_LOCAL = false;
                    IsEnabled_BCO = false;
                    IsEnabled_NCD = false;
                    Visibility_BNC = Visibility.Visible;
                    Visibility_LOCAL = Visibility.Visible;
                    Visibility_BCO = Visibility.Visible;
                    Visibility_NCD = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }

            QueryFQUserConfig();
        }

        private string senderNumber = string.Empty;
        //ConcurrentDictionary<string, List<ToOption>> broker_ToOption_Dic = new ConcurrentDictionary<string, List<ToOption>>();
        //List<ToOption> ToOptionList_Contacts = new List<ToOption>();
        //List<ToOption> ToOptionList_Counterparties = new List<ToOption>();
        //private Dictionary<string, ToOption> ToOptionList_QQGroups = new Dictionary<string, ToOption>();
        private Dictionary<string, object> DS_Broker_BNC = new Dictionary<string, object>();
        private Dictionary<string, object> DS_Broker_LOCAL = new Dictionary<string, object>();
        private Dictionary<string, object> DS_Broker_BCO = new Dictionary<string, object>();
        private Dictionary<string, object> DS_Broker_NCD = new Dictionary<string, object>();
        private Dictionary<string, object> DS_Contacts = new Dictionary<string, object>();
        private Dictionary<string, object> DS_Counterparties = new Dictionary<string, object>();
        private Dictionary<string, object> DS_QQGroups = new Dictionary<string, object>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, ToOption>> ToOptionDic = new ConcurrentDictionary<string, ConcurrentDictionary<string, ToOption>>();

        private string GetHistorySelectedValues(string type)
        {
            string bnc = IniFileHelper.ReadValue($"Inquiry_{LocalEnv.UserInfo.UserName}", type);
            StringBuilder sb = new StringBuilder();
            if (JsonSplit.IsJson(bnc))
            {
                List<string> bncArr = JsonConvert.DeserializeObject<List<string>>(bnc);

                if (bncArr != null && bncArr.Count > 0)
                {
                    foreach (string qq in bncArr)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(qq);
                    }
                }
            }
            return sb.ToString();
        }

        internal void QueryFQUserConfig()
        {
            Task.Run(() =>
            {
                try
                {
                    string bnc = GetHistorySelectedValues("bnc");
                    string bco = GetHistorySelectedValues("bco");
                    string local = GetHistorySelectedValues("local");
                    string ncd = GetHistorySelectedValues("ncd");
                    string counterParties = GetHistorySelectedValues("counterParties");
                    string contacts = GetHistorySelectedValues("contacts");
                    string qqGroups = GetHistorySelectedValues("qqGroups");
                    List<string> customGroupList = JsonConvert.DeserializeObject<List<string>>(IniFileHelper.ReadValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "group"));


                    GetUserConfigFinqRequest req = new GetUserConfigFinqRequest();
                    req.Username = LocalEnv.UserInfo.UserName;
                    //string qqNumber = CommonUtils.GetQQNumber(LocalEnv.UserInfo.UserName);
                    //req.sender = "3003945772";// qqNumber;
                    req.sender = senderNumber;
                    //req.sender = "3003945772";
                    GetUserConfigFinqReplyPo reply = FinQHandler.Instance.GetUserConfig(req);
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {

                        if (reply != null && reply.IsSuccess())
                        {
                            if (reply != null)
                            {
                                // broker;
                                if (reply.data.sendIntentBrokers != null && reply.data.sendIntentBrokers.Count > 0)
                                {
                                    foreach (var item in reply.data.sendIntentBrokers)
                                    {
                                        if (item.Value == null || item.Value.Count == 0)
                                            continue;

                                        string brokerCode = GetBrokerCode(item.Key);

                                        foreach (SendIntentBrokers brokerInfo in item.Value)
                                        {
                                            if (securityTypeList.Count == 1 && brokerInfo.securityType == securityTypeList[0])
                                            {
                                                ToOption to = new ToOption();
                                                to.OptionType = "sendIntentBrokers";
                                                to.DispayName = $"{ComboSourceUtil.Instance.FindEnum(brokerCode, "venuesxb")}-{brokerInfo.name}";
                                                to.Broker = brokerInfo;
                                                to.BrokerCode = brokerCode;

                                                if (!ToOptionDic.ContainsKey(brokerInfo.securityType))
                                                {
                                                    ToOptionDic[brokerInfo.securityType] = new ConcurrentDictionary<string, ToOption>();
                                                }
                                                ToOptionDic[brokerInfo.securityType][brokerInfo.qq] = to;
                                                if (brokerInfo.securityType == "1")//信用
                                                {
                                                    DS_Broker_BCO[brokerInfo.qq] = brokerInfo.name;
                                                }
                                                else if (brokerInfo.securityType == "2")//利率
                                                {
                                                    DS_Broker_BNC[brokerInfo.qq] = brokerInfo.name;
                                                }
                                                else if (brokerInfo.securityType == "3")//ncd
                                                {
                                                    DS_Broker_NCD[brokerInfo.qq] = brokerInfo.name;
                                                }
                                                else if (brokerInfo.securityType == "21") //local
                                                {
                                                    DS_Broker_LOCAL[brokerInfo.qq] = brokerInfo.name;
                                                }
                                            }

                                        }
                                    }
                                    _owner.broker_bnc_box.ItemsSource = DS_Broker_BNC;
                                    _owner.broker_bnc_box.SelectedValues = bnc.ToString();
                                    _owner.broker_bco_box.ItemsSource = DS_Broker_BCO;
                                    _owner.broker_bco_box.SelectedValues = bco.ToString();
                                    _owner.broker_ncd_box.ItemsSource = DS_Broker_NCD;
                                    _owner.broker_ncd_box.SelectedValues = ncd.ToString();
                                    _owner.broker_local_box.ItemsSource = DS_Broker_LOCAL;
                                    _owner.broker_local_box.SelectedValues = local.ToString();
                                }

                                // 常用联系人
                                if (reply.data.sendIntentContacts != null && reply.data.sendIntentContacts.Count > 0)
                                {
                                    foreach (UserConfig item in reply.data.sendIntentContacts)
                                    {
                                        ToOption to = new ToOption();
                                        to.OptionType = "sendIntentContacts";
                                        to.DispayName = $"常用联系人-{item.name}";
                                        to.Sender = item;
                                        if (!ToOptionDic.ContainsKey(to.OptionType))
                                        {
                                            ToOptionDic[to.OptionType] = new ConcurrentDictionary<string, ToOption>();
                                        }
                                        ToOptionDic[to.OptionType][item.qq] = to;
                                        DS_Contacts[item.qq] = item.name;
                                    }
                                    _owner.contacts_box.ItemsSource = DS_Contacts;
                                    _owner.contacts_box.SelectedValues = contacts;
                                }

                                // 对手方
                                if (reply.data.sendIntentCounterparties != null && reply.data.sendIntentCounterparties.Count > 0)
                                {
                                    foreach (UserConfig item in reply.data.sendIntentCounterparties)
                                    {
                                        ToOption to = new ToOption();
                                        to.OptionType = "sendIntentCounterparties";
                                        to.DispayName = $"对手方-{item.name}";
                                        to.Sender = item;
                                        if (!ToOptionDic.ContainsKey(to.OptionType))
                                        {
                                            ToOptionDic[to.OptionType] = new ConcurrentDictionary<string, ToOption>();
                                        }
                                        ToOptionDic[to.OptionType][item.qq] = to;
                                        DS_Counterparties[item.qq] = item.name;
                                    }
                                    _owner.counterparties_box.ItemsSource = DS_Counterparties;
                                    _owner.counterparties_box.SelectedValues = counterParties;
                                }

                                // QO群组
                                if (reply.data.sendIntentQQGroups != null && reply.data.sendIntentQQGroups.Count > 0)
                                {
                                    foreach (QQGroupConfig item in reply.data.sendIntentQQGroups)
                                    {
                                        ToOption to = new ToOption();
                                        to.OptionType = "sendIntentQQGroups";
                                        to.DispayName = $"QO群组-{item.groupName}";
                                        to.QQGroups = item;
                                        if (!ToOptionDic.ContainsKey(to.OptionType))
                                        {
                                            ToOptionDic[to.OptionType] = new ConcurrentDictionary<string, ToOption>();
                                        }
                                        ToOptionDic[to.OptionType][item.groupOpenID] = to;
                                        DS_QQGroups[item.groupOpenID] = item.groupName;
                                    }
                                    _owner.group_box.ItemsSource = DS_QQGroups;
                                    _owner.group_box.SelectedValues = qqGroups;
                                }

                                // 自定义分组
                                if (reply.data.sendIntentGroups != null && reply.data.sendIntentGroups.Count > 0)
                                {
                                    foreach (SendIntentGroups item in reply.data.sendIntentGroups)
                                    {
                                        ToOption to = new ToOption();
                                        to.OptionType = "sendIntentGroups";
                                        to.DispayName = $"自定义分组-{item.groupName}";
                                        to.Groups = item;
                                        //CustomGroups.Add(to);

                                        GroupsData gd = new GroupsData();
                                        gd.GroupName = item.groupName;
                                        gd.GroupsToOption = to;
                                        if (!ToOptionDic.ContainsKey(to.OptionType))
                                        {
                                            ToOptionDic[to.OptionType] = new ConcurrentDictionary<string, ToOption>();
                                        }
                                        ToOptionDic[to.OptionType][item.groupName] = to;
                                        if (customGroupList != null)
                                        {
                                            gd.IsChecked = customGroupList.Contains(gd.GroupName);
                                        }
                                        CustomGroups.Add(gd);
                                    }
                                }
                            }
                        }
                    }));
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            });
        }

        private string GetBrokerCode(string key)
        {
            if (key == "1")
            {
                return "TPSC";// 国利
            }
            else if (key == "2")
            {
                return "CNEX";// 国际
            }
            else if (key == "3")
            {
                return "CCTB";//中诚
            }
            else if (key == "4")
            {
                return "PTCN";//平安
            }
            else if (key == "5")
            {
                return "CTTJ";//信唐
            }
            else if (key == "6")
            {
                return "UEDA"; //上田
            }
            return key;
        }

        private bool _ConfirmEnable = true;
        public bool ConfirmEnable
        {
            get
            {
                return _ConfirmEnable;
            }
            set
            {
                _ConfirmEnable = value;
                this.OnPropertyChanged();
            }
        }

        private void SetDoAllBtnEnable(bool isEnable)
        {
            this.ConfirmEnable = isEnable;
        }

        private void DoDelQuoteCommand(Button btn)
        {
            if (btn != null && btn.DataContext is IntentionMarketItem)
            {
                IntentionMarketItem currentItem = btn.DataContext as IntentionMarketItem;
                if (currentItem != null)
                {
                    lock (_dataLock)
                    {
                        Datas.Remove(currentItem);
                    }
                }
            }
        }

        private void DoSendQuoteCommand(Button btn)
        {
            Window owner = Window.GetWindow(btn);

            List<IntentionMarketItem> dataList = null;
            lock (_dataLock)
            {
                if (Datas != null)
                {
                    dataList = Datas.ToList<IntentionMarketItem>();
                }
            }

            if (dataList == null || dataList.Count == 0)
            {
                CustomMessageBox.Info($"无要发送的记录", "发送", owner);
                return;
            }

            if (!(bncList.Count == 0 || bcoList.Count == 0 || localList.Count == 0 || ncdList.Count == 0
                || counterPartiesList.Count == 0 || contactsList.Count == 0
                || qqGroupsList.Count == 0 || CustomGroups.Any(x => x.IsChecked) == true))
            {
                CustomMessageBox.Info($"请选择To值", "发送", owner);
                return;
            }

            //// 检查报量是否为空
            //if (LocalEnv.ConfInfo.IsPAYL == false && dataList.Any(entry => entry.IsChecked && !entry.Qty_Edit.HasValue && entry.OptionType != "sendIntentGroups" && entry.OptionType != "sendIntentQQGroups"))
            //{
            //    CustomMessageBox.Info($"量不可为空", "发送", _owner);
            //    return;
            //}

            SetDoAllBtnEnable(false);

            foreach (IntentionMarketItem item in Datas)
            {
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    //如果是待处理，要变成处理中
                    if (item.Status == "80")
                    {
                        DoConfirmOne(item.intentionItem);
                    }
                }
            }

            //发送询价
            Sub_DoBatchSendQuoteCommand();

            if (owner != null)
            {
                owner.Close();
            }
        }


        private void DoConfirmOne(IntentionQuoteItem item)
        {
            item.IsConfirmed = true;
            string intentionId = item.IntentionId;
            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.corssVenues = "2";
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.actionType = "81";// NF发布确认
            }
            else
            {
                request.actionType = "1";// NF发布确认
            }
            request.userName = item.BrokerId;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            request.accountID = item.AccountID;
            request.accountName = item.AccountName;
            request.ifSubBond = item.ifSubBond;
            request.vipCode = item.vipCode;
            request.orgType = item.orgType;
            request.validUntilTime = item.validUntilTime;
            request.inf1 = item.Info1;
            request.info2 = item.Info2;
            request.channel = item.Channel;
            request.priceType = item.PriceType;
            request.updateTime = item.UpdateTime;
            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                request.quoteType = "BUYER_INTENTION";
            }
            else
            {
                request.quoteType = "INTENTION";
            }
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[intentionId] = item;

                    GetErrorInfo_Inner(item.Request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    while (totalCount != 1)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoConfirmCommand", e);
                }
            });
        }

        private void GetErrorInfo_Inner(STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteReplyPo replyPo = IntentionOldHandler.Instance.ConfirmIntentionQuote(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                            foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                            {
                                if (placeOrderResult.Code == "10002")
                                {
                                    isProhibitory = true;
                                    break;
                                }
                            }

                            if (isProhibitory)
                            {
                                prohibitReplyDic[request.intentionId] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.intentionId] = orderRisks;
                            }
                        }
                        else
                        {
                            List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                            TPlaceOrderResult result = new TPlaceOrderResult();
                            result.Code = "10003";
                            result.DoubleWarn = false;
                            result.Reason = replyPo.Msg;
                            exceptionResult.Add(result);
                            exceptionReplyDic[request.intentionId] = exceptionResult;
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        // broker
        private Dictionary<string, ToOption> CollectReceiverNumberDic()
        {
            Dictionary<string, ToOption> receiverNumberDic = new Dictionary<string, ToOption>();

            // broker 信用
            foreach (string qq in bcoList)
            {
                if (ToOptionDic.ContainsKey("1") && ToOptionDic["1"].ContainsKey(qq))
                    receiverNumberDic[qq] = ToOptionDic["1"][qq];
            }

            // broker 利率
            foreach (string qq in bncList)
            {
                if (ToOptionDic.ContainsKey("2") && ToOptionDic["2"].ContainsKey(qq))
                    receiverNumberDic[qq] = ToOptionDic["2"][qq];
            }

            // broker NCD
            foreach (string qq in ncdList)
            {
                if (ToOptionDic.ContainsKey("3") && ToOptionDic["3"].ContainsKey(qq))
                    receiverNumberDic[qq] = ToOptionDic["3"][qq];
            }

            // broker 地方债
            foreach (string qq in localList)
            {
                if (ToOptionDic.ContainsKey("21") && ToOptionDic["21"].ContainsKey(qq))
                    receiverNumberDic[qq] = ToOptionDic["21"][qq];
            }

            return receiverNumberDic;
        }

        // 常用联系人 对手方
        private Dictionary<string, ToOption> CollectCounterPartiesDic()
        {
            Dictionary<string, ToOption> receiverNumberDic = new Dictionary<string, ToOption>();
            foreach (string qq in counterPartiesList)
            {
                if (ToOptionDic.ContainsKey("sendIntentCounterparties") && ToOptionDic["sendIntentCounterparties"].ContainsKey(qq))
                    receiverNumberDic[qq] = ToOptionDic["sendIntentCounterparties"][qq];
            }
            return receiverNumberDic;
        }

        // 常用联系人 同事
        private Dictionary<string, ToOption> CollectContactsDic()
        {
            Dictionary<string, ToOption> receiverNumberDic = new Dictionary<string, ToOption>();
            foreach (string qq in contactsList)
            {
                if (ToOptionDic.ContainsKey("sendIntentContacts") && ToOptionDic["sendIntentContacts"].ContainsKey(qq))
                    receiverNumberDic[qq] = ToOptionDic["sendIntentContacts"][qq];
            }
            return receiverNumberDic;
        }

        // 常用组
        private Dictionary<string, ToOption> CollectGroupsDic()
        {
            Dictionary<string, ToOption> receiverNumberDic = new Dictionary<string, ToOption>();
            foreach (string qq in qqGroupsList)
            {
                if (ToOptionDic.ContainsKey("sendIntentQQGroups") && ToOptionDic["sendIntentQQGroups"].ContainsKey(qq))
                    receiverNumberDic[qq] = ToOptionDic["sendIntentQQGroups"][qq];
            }
            return receiverNumberDic;
        }

        private void Sub_DoBatchSendQuoteCommand()
        {
            try
            {
                List<IntentionMarketItem> dataList = null;
                lock (_dataLock)
                {
                    if (Datas != null)
                    {
                        dataList = Datas.ToList<IntentionMarketItem>();
                    }
                }

                if (dataList == null || dataList.Count == 0)
                {
                    return;
                }
               
                List<TableIntentionInquiry> refList = new List<TableIntentionInquiry>();
                List<TableIntentionInquiry> inquiryList = new List<TableIntentionInquiry>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
               
                //添加下新增数据
                // 经纪人
                Dictionary<string, ToOption> receiverNumberDic = CollectReceiverNumberDic();
                if (receiverNumberDic != null && receiverNumberDic.Count > 0)
                {
                    foreach (ToOption to in receiverNumberDic.Values)
                    {
                        foreach (IntentionMarketItem item in dataList)
                        {
                            TableIntentionInquiry data = GenTableIntentionInquiry(item);
                            data.receiverNumber = to.Broker.qq; //"3004330557";// todo: 
                            data.receiverName = to.Broker.name;
                            to.TraderBuyer = item.Trader_Buyer;
                            to.TraderName_Buyer = item.TraderName_Buyer;
                            to.Symbol = item.Symbol;
                            to.RemainingTime = item.RemainingTime;
                            to.BondType = item.BondType;
                            data.info5 = JsonConvert.SerializeObject(to);
                            data.sendType = 411;
                            data.venueTypeGW = "SOR";
                            data.venue = to.BrokerCode;
                            inquiryList.Add(data);
                        }
                    }
                }

                // 对手方
                Dictionary<string, ToOption> counterPartiesDic = CollectCounterPartiesDic();
                if (counterPartiesDic != null && counterPartiesDic.Count > 0)
                {
                    foreach (ToOption to in counterPartiesDic.Values)
                    {
                        foreach (IntentionMarketItem item in dataList)
                        {
                            TableIntentionInquiry data = GenTableIntentionInquiry(item);
                            data.receiverNumber = to.Sender.qq;
                            data.receiverName = to.Sender.name;
                            data.batchReceiverNumberMap = new Dictionary<string, string>();
                            data.batchReceiverNumberMap[to.Sender.qq] = to.Sender.name;
                            to.TraderBuyer = item.Trader_Buyer;
                            to.TraderName_Buyer = item.TraderName_Buyer;
                            to.Symbol = item.Symbol;
                            to.RemainingTime = item.RemainingTime;
                            to.BondType = item.BondType;
                            data.info5 = JsonConvert.SerializeObject(to);
                            data.sendType = 412;
                            data.venueTypeGW = "SOR";
                            data.venue = "QQ";
                            inquiryList.Add(data);
                        }
                    }
                }

                // 同事
                Dictionary<string, ToOption> contactsDic = CollectContactsDic();
                if (contactsDic != null && contactsDic.Count > 0)
                {
                    foreach (ToOption to in contactsDic.Values)
                    {
                        foreach (IntentionMarketItem item in dataList)
                        {
                            TableIntentionInquiry data = GenTableIntentionInquiry(item);
                            data.receiverNumber = to.Sender.qq;
                            data.receiverName = to.Sender.name;
                            data.batchReceiverNumberMap = new Dictionary<string, string>();
                            data.batchReceiverNumberMap[to.Sender.qq] = to.Sender.name;
                            to.TraderBuyer = item.Trader_Buyer;
                            to.TraderName_Buyer = item.TraderName_Buyer;
                            to.Symbol = item.Symbol;
                            to.RemainingTime = item.RemainingTime;
                            to.BondType = item.BondType;
                            data.info5 = JsonConvert.SerializeObject(to);
                            data.sendType = 413;
                            data.venueTypeGW = "SOR";
                            data.venue = "QQ";
                            inquiryList.Add(data);
                        }
                    }
                }

                // 常用组
                Dictionary<string, ToOption> groupsDic = CollectGroupsDic();
                if (groupsDic != null && groupsDic.Count > 0)
                {
                    foreach (ToOption to in groupsDic.Values)
                    {
                        foreach (IntentionMarketItem item in dataList)
                        {
                            TableIntentionInquiry data = GenTableIntentionInquiry(item);
                            data.receiverGroupOpenId = to.QQGroups.groupOpenID;
                            data.receiverGroupName = to.QQGroups.groupName;
                            to.TraderBuyer = item.Trader_Buyer;
                            to.TraderName_Buyer = item.TraderName_Buyer;
                            to.Symbol = item.Symbol;
                            to.RemainingTime = item.RemainingTime;
                            to.BondType = item.BondType;
                            data.info5 = JsonConvert.SerializeObject(to);
                            data.sendType = 42;
                            string price = data.price.HasValue ? data.price?.ToString("0.00##") : string.Empty;
                            string qty = data.qty.HasValue ? data.qty?.ToString("0") + "w" : string.Empty;
                            data.text = $"{item.RemainingTime} {item.Symbol} {item.SecurityId} {ComboSourceUtil.Instance.FindEnum(item.Side, "side")} {price} {qty}";
                            data.venueTypeGW = "SOR";
                            data.venue = "GQQ";
                            inquiryList.Add(data);
                        }
                    }
                }

                // 自定义组
                if (CustomGroups != null && CustomGroups.Count > 0)
                {
                    foreach (GroupsData gd in CustomGroups)
                    {
                        if (gd.IsChecked)
                        {
                            foreach (IntentionMarketItem item in dataList)
                            {
                                TableIntentionInquiry data = GenTableIntentionInquiry(item);
                                if (gd.GroupsToOption != null && gd.GroupsToOption.Groups != null
                                             && gd.GroupsToOption.Groups.members != null
                                             && gd.GroupsToOption.Groups.members.Count > 0)
                                {
                                    data.batchReceiverNumberMap = new Dictionary<string, string>();
                                    foreach (UserConfig uc in gd.GroupsToOption.Groups.members)
                                    {
                                        data.batchReceiverNumberMap[uc.qq] = uc.name;
                                    }
                                }
                                gd.GroupsToOption.TraderBuyer = item.Trader_Buyer;
                                gd.GroupsToOption.TraderName_Buyer = item.TraderName_Buyer;
                                gd.GroupsToOption.Symbol = item.Symbol;
                                gd.GroupsToOption.RemainingTime = item.RemainingTime;
                                gd.GroupsToOption.BondType = item.BondType;
                                data.info5 = JsonConvert.SerializeObject(gd.GroupsToOption);
                                data.sendType = 43;
                                string price = data.price.HasValue ? data.price?.ToString("0.00##") : string.Empty;
                                string qty = data.qty.HasValue ? data.qty?.ToString("0") + "w" : string.Empty;
                                data.text = $"{item.RemainingTime} {item.Symbol} {item.SecurityId} {ComboSourceUtil.Instance.FindEnum(item.Side, "side")} {price} {qty}";
                                data.venueTypeGW = "SOR";
                                data.venue = "QQ";
                                inquiryList.Add(data);
                            }
                        }
                    }
                }

                foreach(TableIntentionInquiry data in inquiryList)
                {
                    // 找相同有效的询价数据, 更新
                    if (JsonSplit.IsJson(data.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(data.info5);
                        TableIntentionInquiry findInquriy = IntentionOldHandler.Instance.GetSameToInquiry(data.intentionId, toOption);
                        if (findInquriy != null)
                        {
                            data.quoteId = findInquriy.quoteId; // 更新传quoteid
                            data.id = findInquriy.id;
                            data.createTime = findInquriy.createTime;
                        }
                    }
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    IntentionInquiryRequest request;
                    totalCount = 0;
                    failCount = 0;
                    warnReplyDic.Clear();
                    prohibitReplyDic.Clear();

                    request = new IntentionInquiryRequest();
                    //request.intentionId = _Key;
                    //request.inquiryKey = _Key;
                    request.inquiryKey = DataTimeSequence.Instance.GetId();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.biztypeid = LocalEnv.UserInfo.Termids;
                    request.action = "1";
                    request.dataList = inquiryList;
                    //TableOldHandler.Instance.IntentionInquiry(request);

                    //orderDic[item.IntentionId].Request = request;
                    //RemenberInstAndTrade(item);

                    GetErrorInfo_Inquiry(request,
                            ref failCount,
                            ref totalCount,
                            warnReplyDic,
                            prohibitReplyDic,
                            exceptionReplyDic);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                Dictionary<string, IntentionQuoteItem> tempDic = new Dictionary<string, IntentionQuoteItem>();
                                foreach (TableIntentionInquiry inquiry in inquiryList)
                                {
                                    tempDic[Guid.NewGuid().ToString()] = null;
                                }
                                BatchResultWindow batchResultWindow = new BatchResultWindow(tempDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                //batchResultWindow.Owner = _owner;
                                batchResultWindow.ShowDialog();
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                            //}
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                        //}
                    }));

                    //System.Threading.ThreadPool.QueueUserWorkItem(x =>
                    //{
                    //    Thread.Sleep(2000);
                    //    Query();
                    //});
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoSendQuoteCommand1", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
                
                IniFileHelper.WriteValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "bnc", JsonConvert.SerializeObject(bncList));
                IniFileHelper.WriteValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "bco", JsonConvert.SerializeObject(bcoList));
                IniFileHelper.WriteValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "local", JsonConvert.SerializeObject(localList));
                IniFileHelper.WriteValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "ncd", JsonConvert.SerializeObject(ncdList));
                IniFileHelper.WriteValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "counterParties", JsonConvert.SerializeObject(counterPartiesList));
                IniFileHelper.WriteValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "contacts", JsonConvert.SerializeObject(contactsList));
                IniFileHelper.WriteValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "qqGroups", JsonConvert.SerializeObject(qqGroupsList));
                List<string> grouplist = new List<string>();
                foreach(GroupsData gd in CustomGroups)
                {
                    if (gd.IsChecked)
                    {
                        grouplist.Add(gd.GroupName);
                    }
                }
                IniFileHelper.WriteValue($"Inquiry_{LocalEnv.UserInfo.UserName}", "group", JsonConvert.SerializeObject(grouplist));
            }
        }

        private TableIntentionInquiry GenTableIntentionInquiry(IntentionMarketItem item)
        {
            TableIntentionInquiry data = new TableIntentionInquiry();

            
            //if (!isRef) // 修改
            //{
            //    if (item != null && item.Status == "1")
            //    {
            //        data.quoteId = item.QuoteId;
            //        data.id = item.Id;
            //        data.createTime = item.CreateTime;
            //    }
            //}
            data.biztypeid = LocalEnv.UserInfo.Termids;
            data.securityId = item.SecurityId;
            data.institutionId = string.Empty;//????
            data.institutionName = string.Empty;//????
            data.traderId = string.Empty;//????
            data.traderName = string.Empty;//????
            data.intentionId = item.GroupKey;
            data.inquiryKey = item.GroupKey;
            string side = item.Side;
            if (item.Side == "3")
            {
                side = "1";
            }
            else if (item.Side == "4")
            {
                side = "2";
            }
            data.side = side;
            data.price = item.Price_Edit?.ToString("0.####").ConvertToDecimal();
            data.qty = item.Qty_Edit?.ToString("0").ConvertToDecimal();
            data.settlType = item.SettlType_Edit;
            data.strategyOrderId = item.GroupKey;
            

            data.senderNumber = senderNumber;
            //TagsInfo tagsInfo = new TagsInfo();
            //tagsInfo.P_FLAG = item.IsP_Edit;
            //tagsInfo.PS_FLAG = item.IsPS_Edit;
            //tagsInfo.OCO_FLAG = item.OCOFlag_Edit;
            //data.tags = JsonConvert.SerializeObject(tagsInfo);
            data.bargainFlag = "0";
            data.quoteDividedFlag = "N";
            if (item.PFlag_Edit == "1")
            {
                data.bargainFlag = "1";
            }
            else if (item.PFlag_Edit == "2")
            {
                data.quoteDividedFlag = "Y";
            }
            data.ocoFlag = item.OCOFlag_Edit;
            data.userName = LocalEnv.UserInfo.UserName;
            data.info1 = item.Remark_Edit;
            InquiryInfo3 info3 = new InquiryInfo3();
            info3.PriceType = item.PriceType_Edit;
            data.info3 = JsonConvert.SerializeObject(info3);
            return data;
        }

        private ConfirmIntentionQuoteReplyPo CheckParam(IntentionMarketItem item)
        {
            ConfirmIntentionQuoteReplyPo reply = new ConfirmIntentionQuoteReplyPo();
            reply.intentionId = item.IntentionId;
            reply.orderId = item.Id;
            reply.securityId = item.SecurityId;
            reply.status = item.Status;
            reply.Code = "0000";
            decimal d = 0;
            string errorMsg = string.Empty;
            //if (item.Price == null || item.Price == 0)
            //    errorMsg = "未填写价格或价格为0";
            //else if (item.Qty == null || item.Qty == 0)
            //    errorMsg = "未填写量或量为0";
            //else if (item.Qty % 100 > 0)
            //    errorMsg = string.Format("量必须是100的整数倍，当前输入值：{0}", item.Qty.Value.ToString("0"));
            //else 
            if (string.IsNullOrEmpty(item.SettlType_Edit))
                errorMsg = "清算速度未填写";
            else if (string.IsNullOrEmpty(item.Side))
                errorMsg = "方向未填写";
            //else if (item.Qty != null && item.Qty > 0 && decimal.TryParse(item.Qty.ToString(), out d) && (d % 100) > 0)
            //    errorMsg = string.Format("量必须是100的整数倍，当前输入值：{0}", item.Qty);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                reply.Code = "10001";

                List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();
                TPlaceOrderResult por = new TPlaceOrderResult();
                por.Code = "10003";
                por.DoubleWarn = false;
                por.Reason = errorMsg;
                result.Add(por);

                reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);
            }
            return reply;
        }

        private void GetErrorInfo_Inquiry(IntentionInquiryRequest request,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    request.quoteType = "BUYER_INTENTION";
                }
                else
                {
                    request.quoteType = "INTENTION";
                }

                if (request.dataList != null)
                {
                    foreach (TableIntentionInquiry inquiryItem in request.dataList)
                    {
                        if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                        {
                            inquiryItem.quoteType = "BUYER_INTENTION";
                        }
                        else
                        {
                            inquiryItem.quoteType = "INTENTION";
                        }
                    }
                }

                IntentionInquiryPo replyPo = TableOldHandler.Instance.IntentionInquiry(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                            foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                            {
                                if (placeOrderResult.Code == "10002")
                                {
                                    isProhibitory = true;
                                    break;
                                }
                            }

                            if (isProhibitory)
                            {
                                prohibitReplyDic[request.inquiryKey] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.inquiryKey] = orderRisks;
                            }
                        }
                        else
                        {
                            if (replyPo.Datas != null && replyPo.Datas.Count > 0)
                            {
                                foreach (TableIntentionInquiry error in replyPo.Datas)
                                {
                                    if (!string.IsNullOrEmpty(error.pubFQMsg))
                                    {
                                        List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                                        TPlaceOrderResult result = new TPlaceOrderResult();
                                        result.Code = "10003";
                                        result.DoubleWarn = false;
                                        result.Reason = error.pubFQMsg;
                                        result.SecurityId = error.securityId;
                                        result.Side = error.side;
                                        result.Price = error.price;
                                        result.Qty = error.qty;
                                        exceptionResult.Add(result);
                                        string key = string.IsNullOrEmpty(error.id) ? Guid.NewGuid().ToString() : error.id;
                                        exceptionReplyDic[key] = exceptionResult;
                                    }
                                }
                            }
                            else
                            {
                                List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                                TPlaceOrderResult result = new TPlaceOrderResult();
                                result.Code = "10003";
                                result.DoubleWarn = false;
                                result.Reason = string.IsNullOrEmpty(replyPo.Msg) ? "发送失败" : replyPo.Msg;
                                //result.SecurityId = request.securityId;
                                //result.Side = marketItem.Side;
                                //result.Price = ;
                                //result.Qty = error.qty;
                                exceptionResult.Add(result);
                                exceptionReplyDic[request.inquiryKey] = exceptionResult;
                            }
                        }
                    }
                }
                else
                {
                    bool hasError = false;
                    if (replyPo.Datas != null && replyPo.Datas.Count > 0)
                    {
                        foreach (TableIntentionInquiry error in replyPo.Datas)
                        {
                            if (!string.IsNullOrEmpty(error.pubFQMsg))
                            {
                                List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                                TPlaceOrderResult result = new TPlaceOrderResult();
                                result.Code = "10003";
                                result.DoubleWarn = false;
                                result.Reason = error.pubFQMsg;
                                result.SecurityId = error.securityId;
                                result.Side = error.side;
                                result.Price = error.price;
                                result.Qty = error.qty;
                                exceptionResult.Add(result);
                                string key = string.IsNullOrEmpty(error.id) ? Guid.NewGuid().ToString() : error.id;
                                exceptionReplyDic[key] = exceptionResult;
                                hasError = true;
                            }
                        }
                    }

                    if (hasError)
                    {
                        failCount++;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        internal void SetBcoList(string selectedValues)
        {
            bcoList.Clear();
            if (!string.IsNullOrEmpty(selectedValues))
            { 
                string[] arr = selectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    bcoList.Add(item);
                }
            }
        }

        internal void SetBncList(string selectedValues)
        {
            bncList.Clear();
            if (!string.IsNullOrEmpty(selectedValues))
            {
                string[] arr = selectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    bncList.Add(item);
                }
            }
        }

        internal void SetNcdList(string selectedValues)
        {
            ncdList.Clear();
            if (!string.IsNullOrEmpty(selectedValues))
            {
                string[] arr = selectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    ncdList.Add(item);
                }
            }
        }
        internal void SetLocalList(string selectedValues)
        {
            localList.Clear();
            if (!string.IsNullOrEmpty(selectedValues))
            {
                string[] arr = selectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    localList.Add(item);
                }
            }
        }

        internal void SetCounterPartiesList(string selectedValues)
        {
            counterPartiesList.Clear();
            if (!string.IsNullOrEmpty(selectedValues))
            {
                string[] arr = selectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    
                    counterPartiesList.Add(item);
                }
            }
        }

        internal void SetContactsList(string selectedValues)
        {
            contactsList.Clear();
            if (!string.IsNullOrEmpty(selectedValues))
            {
                string[] arr = selectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    contactsList.Add(item);
                }
            }
        }

        internal void SetQQGroupList(string selectedValues)
        {
            qqGroupsList.Clear();
            if (!string.IsNullOrEmpty(selectedValues))
            {
                string[] arr = selectedValues.Split(Consts.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    qqGroupsList.Add(item);
                }
            }
        }
    }
}
