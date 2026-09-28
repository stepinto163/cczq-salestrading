using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Frame.Common;
using STC.WorkTable.Plugin.InnerOrders.ViewModel.Data;
using STC.WorkTable.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.WorkTable.Plugin.InnerOrders.ViewModel
{
    public class InnerBoardViewModel : ViewModelBase
    {
        private string _SearchText;

        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                this.OnPropertyChanged();
                this.ViewSource.View.Refresh();
            }
        }

        private bool _autoEnable = true;

        public bool AutoEnable
        {
            get { return _autoEnable; }
            set 
            { 
                _autoEnable = value;
                this.OnPropertyChanged();

                if (auteRefreshTimer != null)
                {
                    auteRefreshTimer.Enabled = _autoEnable;
                }
            }
        }

        private InnerOrderData _selectedItem_MyInnerOrders;

        public InnerOrderData SelectedItem_MyInnerOrders
        {
            get
            {
                return _selectedItem_MyInnerOrders;
            }
            set
            {
                _selectedItem_MyInnerOrders = value;
                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<FilterListBoxItemData> _interestRateDebtList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "利率债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "国债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "央票", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "地方政府债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "金融债", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> InterestRateDebtList { get { return _interestRateDebtList; } }

        private ObservableCollection<FilterListBoxItemData> _creditBondList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "信用债", itemIsSelected = false },
            new FilterListBoxItemData() {itemText = "短融", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "中票", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "企业债", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "公司债", itemIsSelected = false, Itemmargin="0 0 50 0"},
            new FilterListBoxItemData() {itemText = "PPN", itemIsSelected = false, Itemmargin="50 0 0 0"},
            new FilterListBoxItemData() {itemText = "NCD", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "ABS", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "CRM", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> CreditBondList { get { return _creditBondList; } }

        private ObservableCollection<FilterListBoxItemData> _remaingingTimeList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "0~1Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "1~3Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "3~5Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "5~7Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "7~10Y", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = ">10Y", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RemaingingTimeList { get { return _remaingingTimeList; } }

        private ObservableCollection<FilterListBoxItemData> _ratingList = new ObservableCollection<FilterListBoxItemData>()
        {
            new FilterListBoxItemData() {itemText = "全部", itemIsSelected = true },
            new FilterListBoxItemData() {itemText = "AAA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AAA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "AA-", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "A+", itemIsSelected = false},
            new FilterListBoxItemData() {itemText = "其他", itemIsSelected = false},
        };
        public ObservableCollection<FilterListBoxItemData> RatingList { get { return _ratingList; } }

        //private ObservableCollection<FilterListBoxItemData> _roleList = new ObservableCollection<FilterListBoxItemData>();

        //public ObservableCollection<FilterListBoxItemData> RoleList { get { return _roleList; } }

        

        private object _dataLock = new object();
        private CollectionViewSource _viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private CustomCollection<InnerOrderData> _innerOrders = new CustomCollection<InnerOrderData>();

        public CustomCollection<InnerOrderData> InnerOrders
        {
            get { return _innerOrders; }
            set { _innerOrders = value; }
        }

        public ICommand RefreshCommand { get; set; }

        private Timer auteRefreshTimer;

        private readonly string _ModelId = "5000";
        private readonly string _Code = "5002";
        private bool isPermission = false; //查看机构内部权限

        public InnerBoardViewModel()
        {
            BaseHandler.Instance.SubscribeLoadDataStatus(onloadData);

            BaseHandler.Instance.Init(Instance_OnClear);

            _lastUpdateTime = $"{DateTimeEx.ServerTime.ToString(dateDateFormatPattern)} 00:00:00.000";

            isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);

            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.Source = InnerOrders;
            SetFilterDatas();
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _dataLock);

            RefreshCommand = new ComRelayCommand(InitEmsOrder);

            InitEmsOrder();
        }

        private void onloadData(bool obj)
        {
            var itemList = InnerOrders.Where(item => !string.IsNullOrEmpty(item.RemaingingTime));
            foreach(InnerOrderData data in itemList)
            {
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(data.SecurityID);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    data.RemainingTime = bondsInfoPo.RemaingingTime;
                    data.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    data.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    data.Issuer = bondsInfoPo.issuer;
                    data.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    data.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                InitEmsOrder();
            }
        }

        private void AuteRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RefreshEmsOrder();
        }

       

        private void InitEmsOrder()
        {
            InnerOrders.Clear();
            _lastUpdateTime = string.Empty;

            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                QueryEmsOrderReq request = new QueryEmsOrderReq();
                request.dealTimeBegin = $"{DateTimeEx.ServerTime.ToString(dateDateFormatPattern)} 00:00:00.000";
                if (!isPermission)
                    request.userName = LocalEnv.UserInfo.UserName;
                QueryEmsOrderReplyPo reply = TableHandler.Instance.QueryEmsOrder(request);
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (reply != null && reply.Datas.Count > 0)
                        {
                            foreach (EmsOrderReport er in reply.Datas)
                            {
                                UpdateInnerOrder(er);
                            }
                        }

                        if (auteRefreshTimer == null)
                        {
                            auteRefreshTimer = new Timer();
                            auteRefreshTimer.Elapsed += AuteRefreshTimer_Elapsed;
                            auteRefreshTimer.Interval = 5000;
                            auteRefreshTimer.Enabled = AutoEnable;
                        }
                    }));
                }
            });
        }

        private string _lastUpdateTime = string.Empty;
        private const string dateDateFormatPattern = "yyyy-MM-dd";
        private const string dateTimeFormatPattern = "yyyy-MM-dd HH:mm:ss.fff";
        private void RefreshEmsOrder()
        {
            ThreadPoolHelper.InvokeThreadPool((o) => {
                QueryEmsOrderReq request = new QueryEmsOrderReq();
                request.dealTimeBegin = string.IsNullOrEmpty(_lastUpdateTime) ? $"{DateTimeEx.ServerTime.ToString(dateDateFormatPattern)} 00:00:00.000" : _lastUpdateTime;
                if (!isPermission)
                    request.userName = LocalEnv.UserInfo.UserName;
                QueryEmsOrderReplyPo reply = TableHandler.Instance.QueryEmsOrder(request);
                if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
                {
                    if (System.Windows.Application.Current != null)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            foreach (EmsOrderReport er in reply.Datas)
                            {
                                UpdateInnerOrder(er);
                            }
                        }));
                    }
                }
            });
        }

        private async void UpdateInnerOrder(EmsOrderReport er)
        {
            if (_lastUpdateTime.CompareTo(er.updatetime) < 0)
            {
                _lastUpdateTime = er.updatetime;
            }

            InnerOrderData findData;
            IEnumerable<InnerOrderData> findDatas = InnerOrders.Where(item => item.OrderID == er.orderid);

            if (findDatas != null && findDatas.Count() > 0)
            {
                findData = findDatas.First();

                //if (!(er.exectype == "N" || er.exectype == "0" || er.exectype == "1" || er.exectype == "S" || er.exectype == "6"))
                if (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C")
                {
                    this.InnerOrders.Remove(findData);
                    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                    return;
                }
                findData.OrderID = er.orderid;
                findData.SecurityID = er.securityid;
                findData.Symbol = er.symbol;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    findData.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    findData.Issuer = bondsInfoPo.issuer;
                    findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
                findData.Side = er.side;
                findData.FirmFlag = er.firmflag;
                //findData.RemaingingTime = 
                findData.SettleType = er.settltype;
                findData.RoleId = er.roleid;
                findData.RoleName = er.rolename;
                findData.Price = er.price;
                findData.Qty = er.orderqty?.ConvertToLong();
                findData.PartySubName = er.partysubname;
                findData.UpdateTime = er.updatetime.Length > 10 ? er.updatetime.Substring(10) : er.updatetime;
            }
            else
            {
                if (er.exectype == "2" || er.exectype == "4" || er.exectype == "8" || er.exectype == "C")
                {
                    return;
                }

                findData = new InnerOrderData();
                findData.OrderID = er.orderid;
                findData.SecurityID = er.securityid;
                findData.Symbol = er.symbol;
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(er.securityid);
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    findData.RemainingTime = bondsInfoPo.RemaingingTime;
                    findData.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    findData.Issuer = bondsInfoPo.issuer;
                    findData.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                }
                findData.Side = er.side;
                findData.FirmFlag = er.firmflag;
                //findData.RemaingingTime = 
                findData.SettleType = er.settltype;
                findData.RoleId = er.roleid;
                findData.RoleName = er.rolename;
                findData.Price = er.price;
                findData.Qty = er.orderqty?.ConvertToLong();
                findData.PartySubName = er.partysubname;
                findData.UpdateTime = er.updatetime.Length > 10 ? er.updatetime.Substring(10) : er.updatetime;
                InnerOrders.Insert(0, findData);
            }
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
        }

        private List<string> firmFlagFilterList = new List<string>();
        private List<string> settleTypeFilterList = new List<string>();
        private List<string> sideFilterList = new List<string>();
        private List<string> roleFilterList = new List<string>();

        internal void FilterData(Dictionary<string, IList> filterDic)
        {
            firmFlagFilterList.Clear();
            if (filterDic != null && filterDic.ContainsKey("FirmFlag"))
            {
                IList firmTypeList = filterDic["FirmFlag"];
                if (firmTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem firmType in firmTypeList)
                    {
                        string firmContent = firmType.Content.ToString();
                        string firmflag = string.Empty;
                        if (firmContent == "firm")
                        {
                            firmflag = "1";
                        }
                        else if (firmContent == "nonfirm")
                        {
                            firmflag = "0";
                        }
                        if (!firmFlagFilterList.Contains(firmflag))
                        {
                            firmFlagFilterList.Add(firmflag);
                        }
                        
                    }
                }
            }

            settleTypeFilterList.Clear();
            if (filterDic != null && filterDic.ContainsKey("SettlType"))
            {

                IList settleTypeList = filterDic["SettlType"];
                if (settleTypeList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem settleTypeItem in settleTypeList)
                    {
                        string settleTypeContent = settleTypeItem.Content.ToString();
                        string settleTypeflag = string.Empty;
                        if (settleTypeContent == "T+0")
                        {
                            settleTypeflag = "1";
                        }
                        else if (settleTypeContent == "T+1")
                        {
                            if (!settleTypeFilterList.Contains("2"))
                            {
                                settleTypeFilterList.Add("2");
                            }
                            if (!settleTypeFilterList.Contains("3"))
                            {
                                settleTypeFilterList.Add("3");
                            }
                        }
                        else if (settleTypeContent == "远期")
                        {
                            settleTypeflag = "4";
                        }
                        if (!settleTypeFilterList.Contains(settleTypeflag))
                        {
                            settleTypeFilterList.Add(settleTypeflag);
                        }

                    }
                }
            }

            sideFilterList.Clear();
            if (filterDic != null && filterDic.ContainsKey("Side"))
            {
                IList sideList = filterDic["Side"];
                if (sideList.Count > 0)
                {
                    foreach (System.Windows.Controls.ListBoxItem sideItem in sideList)
                    {
                        string sideContent = sideItem.Content.ToString();
                        string side = string.Empty;
                        if (sideContent == "Bid")
                        {
                            side = "1";
                        }
                        else if (sideContent == "Ofr")
                        {
                            side = "2";
                        }
                        if (!sideFilterList.Contains(side))
                        {
                            sideFilterList.Add(side);
                        }

                    }
                }
            }

            roleFilterList.Clear();
            if (filterDic != null && filterDic.ContainsKey("Role"))
            {
                IList roleList = filterDic["Role"];
                if (roleList.Count > 0)
                {
                    foreach (string roleItem in roleList)
                    {

                        if (roleItem == "All")
                        {
                            break;
                        }

                        if (!roleFilterList.Contains(roleItem))
                        {
                            roleFilterList.Add(roleItem);
                        }

                    }
                }
            }

            this.ViewSource.View.Refresh();
        }

        string interest = string.Empty;
        List<string> bondCategoryConditions_Interest = new List<string>();
        /// <summary>
        /// 生成债券类型过滤条件
        /// 利率债 BNC
        /// </summary>
        public void FilterBondType(System.Collections.IList selectedItems)
        {
            interest = string.Empty;
            bondCategoryConditions_Interest.Clear();
            if (InterestRateDebtList != null)
            {
                foreach (FilterListBoxItemData lbi in InterestRateDebtList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "利率债")
                    {
                        bondCategoryConditions_Interest.Clear();
                        interest = "1";
                        break;
                    }
                    bondCategoryConditions_Interest.Add(lbi.itemText);
                }
            }
            this.ViewSource.View.Refresh();
        }

        string credit = string.Empty;
        List<string> bondCategoryConditions_Credit = new List<string>();
        /// <summary>
        /// 生成信用债过滤条件
        /// 不是利率债BNC的都归为信用债（信用债:BCO，ABS，NCD）
        /// </summary>
        public void FilterCreditBondType()
        {
            credit = string.Empty;
            bondCategoryConditions_Credit.Clear();
            if (CreditBondList != null)
            {
                foreach (FilterListBoxItemData lbi in CreditBondList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == "信用债")
                    {
                        bondCategoryConditions_Credit.Clear();
                        credit = "1";
                        break;
                    }

                    if (lbi.itemText.ToString() == "其他")
                    {
                        bondCategoryConditions_Credit.Add("");
                    }
                    else
                    {
                        bondCategoryConditions_Credit.Add(lbi.itemText);
                    }
                }
            }
            this.ViewSource.View.Refresh();
        }

        List<int[]> RemainingTimeConditions = new List<int[]>();
        /// <summary>
        /// 生成剩余时间过滤条件
        /// </summary>
        public void FilterRemainingTime()
        {
            RemainingTimeConditions.Clear();
            if (RemaingingTimeList != null)
            {
                foreach (FilterListBoxItemData lbi in RemaingingTimeList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        RemainingTimeConditions.Clear();
                        break;
                    }


                    if (lbi.itemText == "0~1Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 0, 1 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= 0 and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 1})");
                    }
                    else if (lbi.itemText == "1~3Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 1, 3 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 1} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 3})");
                    }
                    else if (lbi.itemText == "3~5Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 3, 5 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 3} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 5})");
                    }
                    else if (lbi.itemText == "5~7Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 5, 7 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 5} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 7})");
                    }
                    else if (lbi.itemText == "7~10Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 7, 10 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 7} and {MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} < {365 * 10})");
                    }
                    else if (lbi.itemText == ">10Y")
                    {
                        RemainingTimeConditions.Add(new int[] { 10 });
                        //filterRemainingTime.Append($"({MMListBoxViewModel.REMAINGING_TIME_CONVERT_FIELD} >= {365 * 10})");
                    }
                }
            }
            this.ViewSource.View.Refresh();
        }

        List<string> rateOptions = new List<string>() { "AAA+", "AAA", "AA+", "AA", "AA-", "A+" };
        List<string> ratingConditions = new List<string>();
        /// <summary>
        /// 主体评级过滤条件
        /// </summary>
        public void FilterRating()
        {
            ratingConditions.Clear();
            if (RatingList != null)
            {
                StringBuilder inFilterSB = new StringBuilder();
                string notInFilter = string.Empty;
                foreach (FilterListBoxItemData lbi in RatingList)
                {
                    if (lbi.itemIsSelected == false)
                    {
                        continue;
                    }

                    if (lbi.itemText == CommonConstants.ALL)
                    {
                        ratingConditions.Clear();
                        break;
                    }

                    ratingConditions.Add(lbi.itemText);
                    //switch (lbi.itemText)
                    //{
                    //    case "AAA+":
                    //    case "AAA":
                    //    case "AA+":
                    //    case "AA":
                    //    case "AA-":
                    //    case "A+":
                    //        AddListFilter(inFilterSB, $"'{lbi.itemText}'");
                    //        break;
                    //    case "其他":
                    //        notInFilter = $"{MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} not in ('AAA+','AAA','AA+','AA','AA-','A+') or {MMListBoxViewModel.ISSUER_RATING_CURRENT_NPY_FIELD} is null";
                    //        break;
                    //}
                }
            }
            this.ViewSource.View.Refresh();
        }

        private void SetFilterDatas()
        {
            this.ViewSource.View.Filter = x => {
                InnerOrderData data = x as InnerOrderData;

                if (string.IsNullOrEmpty(this.SearchText) == false && !data.SecurityID.Contains(this.SearchText) && !data.Symbol.Contains(this.SearchText) && !data.RoleName.Contains(this.SearchText) && !data.PartySubName.Contains(this.SearchText))
                    return false;

                if (firmFlagFilterList != null && firmFlagFilterList.Count > 0 && !firmFlagFilterList.Contains(data.FirmFlag))
                    return false;

                if (settleTypeFilterList != null && settleTypeFilterList.Count > 0 && !(settleTypeFilterList.Contains(data.SettleType) || (settleTypeFilterList.Contains("4") && data.SettleType.Contains("+"))))
                    return false;

                if (sideFilterList != null && sideFilterList.Count > 0 && !sideFilterList.Contains(data.Side))
                    return false;

                if (roleFilterList != null && roleFilterList.Count > 0 && !roleFilterList.Contains(data.RoleId))
                    return false;

                if (!((interest == "1" && data.SecurityType == "BNC")
                   || (bondCategoryConditions_Interest.Count > 0 && bondCategoryConditions_Interest.Contains(data.BondCategory) && data.SecurityType == "BNC")
                   || (credit == "1" && data.SecurityType != "BNC")
                   || (bondCategoryConditions_Credit.Count > 0 && bondCategoryConditions_Credit.Contains(data.BondCategory) && data.SecurityType != "BNC")
                   || (bondCategoryConditions_Interest.Count == 0 && bondCategoryConditions_Credit.Count == 0 && string.IsNullOrEmpty(interest) && string.IsNullOrEmpty(credit))))
                    return false;

                if (RemainingTimeConditions.Count > 0)
                {
                    bool flag = false;
                    foreach (int[] range in RemainingTimeConditions)
                    {
                        if (range.Length == 2)
                        {
                            if (data.RemaingingTime_convert >= range[0] && data.RemaingingTime_convert < range[1])
                            {
                                flag = true;
                                break;
                            }
                        }
                        else if (range.Length == 1)
                        {
                            if (data.RemaingingTime_convert >= range[0])
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                        return false;
                }
                if (ratingConditions.Count > 0)
                {
                    bool flag = false;
                    if (ratingConditions.Contains(data.IssuerRatingCurrentNPY))
                    {
                        flag = true;
                    }
                    else if (ratingConditions.Contains("其他") && (string.IsNullOrEmpty(data.IssuerRatingCurrentNPY) || !rateOptions.Contains(data.IssuerRatingCurrentNPY)))
                    {
                        flag = true;
                    }
                    if (!flag)
                        return false;
                }

                return true;
            };
        }


        /// <summary>
        /// 打开tradebook界面
        /// </summary>
        public void OpenTradeBook()
        {
            if (this.SelectedItem_MyInnerOrders != null)
            {
                // TradeBookView view = new TradeBookView();
                // QuotationListViewModel viewModel = new QuotationListViewModel();
                // viewModel.SecurityID = CommonUtils.ConvertToString(this.SelectedItem[SECURITY_ID_FIELD]);
                // viewModel.Symbol = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);
                // viewModel.FullName = CommonUtils.ConvertToString(this.SelectedItem[SYMBOL_FIELD]);//TODO： 这个还没有
                // view.DataContext = viewModel;

                //TabPageHelper.OpenTabPage(viewModel.SecurityID, view, -1);

                // 使用插件形式打开深度行情页面
                string securityID = CommonUtils.ConvertToString(this.SelectedItem_MyInnerOrders.SecurityID);
                int tradeBookPluginKey = (int)PluginKeyEnum.TradeBookPlugin;

                IPlugin plugin = AppUtils.Application.PluginManager.GetPlugin(tradeBookPluginKey);
                if (plugin != null)
                {
                    plugin.PluginData = securityID;
                    AppUtils.Application.PluginManager.EnablePlugin(tradeBookPluginKey);
                    plugin.PluginData = null;
                    // pluginKey是22，表示TradeBook插件， 可以多次打开
                    // 到load方法中判断 是否需要新建页面
                    if (plugin.PluginKey == tradeBookPluginKey)
                    {
                        plugin.Enabled = false;
                    }
                }
            }
        }
    }
}
