using BDB.Price.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.ViewModel.Data;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Utils;
using STC.Common.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;

namespace BDB.Price.Plugin.ViewModel.Trade
{
    public class RecognizeIntentionQuoteViewModel : ViewModelBase
    {

        //private string _countText;

        //public string CountText
        //{
        //    get { return _countText; }
        //    set 
        //    {
        //        _countText = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        // 是否选中全选多选框
        private bool _IsAllChecked = true;
        public bool IsAllChecked
        {
            get
            {
                return _IsAllChecked;
            }

            set
            {
                if (_IsAllChecked != value)
                {
                    _IsAllChecked = value;
                    this.OnPropertyChanged();
                    int i = 0;
                    foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
                    {
                        item.IsChecked = value;
                        if (value)
                        {
                            i++;
                        }
                    }
                    SelectedCount = i;
                    //CountText = $"共{dataCount},选中{selectedCount}";
                }
            }
        }

        private ObservableCollection<GroupItems> _groupDic = new ObservableCollection<GroupItems>();

        public ObservableCollection<GroupItems> GroupDic
        {
            get { return _groupDic; }
            set
            {
                _groupDic = value;
                this.OnPropertyChanged();
            }
        }


        private GroupItems _selectedGroup;

        public GroupItems SelectedGroup
        {
            get { return _selectedGroup; }
            set 
            {
                _selectedGroup = value;
                if (value != null)
                {
                    _groupId = value.GroupId;
                }
                this.OnPropertyChanged();
            }
        }


        private string _groupId;
        public string GroupId 
        { 
            get { return _groupId; } 
            set { 
                _groupId = value;
                this.OnPropertyChanged();

                if (GroupDic != null && GroupDic.Any(_ => _.GroupId == value))
                {
                    SelectedGroup = GroupDic.Single(_ => _.GroupId == value);
                }
            }
        }

        private object _LockObject = new object();
        private CollectionViewSource viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return viewSource; }
            set { viewSource = value; }
        }

        private CustomCollection<IntentionQuoteItem> intentionQuoteInfo = new CustomCollection<IntentionQuoteItem>();

        public CustomCollection<IntentionQuoteItem> IntentionQuoteInfo
        {
            get { return intentionQuoteInfo; }
            set { intentionQuoteInfo = value; }
        }

        private IntentionQuoteItem _selectedItem_Recognize;
        public IntentionQuoteItem SelectedItem_Recognize
        {
            get
            {
                return this._selectedItem_Recognize;
            }

            set
            {
                this._selectedItem_Recognize = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _ParserImageVisibility = Visibility.Collapsed;

        public Visibility ParserImageVisibility
        {
            get { return _ParserImageVisibility; }
            set
            {
                _ParserImageVisibility = value;
                this.OnPropertyChanged();
            }
        }

        public RecognizeIntentionQuoteViewModel()
        {
            if (LocalEnv.ConfInfo.IsParserImage == "1")
            {
                ParserImageVisibility = Visibility.Visible;
            }

            LoadGroupList();

            IntentionQuoteInfo.CollectionChanged += IntentionQuoteInfo_CollectionChanged;
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.LiveFilteringProperties.Add("Status");
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);
        }


        internal async void LoadGroupList(string groupname = null)
        {
            IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroupList = null;
            await Task.Run(() => {
                sortedGroupList = HomePageDataHandler.Instance.AsyncRequestGroups(true);
            });
            GroupDic.Clear();
            if (sortedGroupList != null)
            {
                foreach (KeyValuePair<string, HomeGroupInfo> pair in sortedGroupList)
                {
                    GroupItems gi = new GroupItems();
                    gi.GroupId = pair.Key;
                    gi.GroupName = pair.Value.groupname;
                    GroupDic.Add(gi);

                    if (!string.IsNullOrEmpty(groupname) && pair.Value.groupname == groupname)
                    {
                        SelectedGroup = gi;
                    }
                    else if (pair.Key == GroupId)
                    {
                        SelectedGroup = gi;
                    }
                }
            }
        }

        private int dataCount = 0;

        public int DataCount
        {
            get { return dataCount; }
            set { dataCount = value;
                this.OnPropertyChanged();
            }
        }

        private int selectedCount = 0;

        public int SelectedCount
        {
            get { return selectedCount; }
            set 
            {
                selectedCount = value;
                this.OnPropertyChanged();
            }
        }

        private void IntentionQuoteInfo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                SelectedCount++;
                DataCount++;
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                DataCount--;
            }
            //CountText = $"共{dataCount},选中{selectedCount}";
        }

        internal void ClickChkBox()
        {
            bool flag = true;
            int i = 0;
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                flag = flag && item.IsChecked;
                if (item.IsChecked)
                {
                    i++;
                }
            }
            _IsAllChecked = this.intentionQuoteInfo.Count > 0 && flag;
            this.OnPropertyChanged("IsAllChecked");

            SelectedCount = i;
            //CountText = $"共{dataCount},选中{selectedCount}";
        }

        internal void ClearList()
        {
            if (this.intentionQuoteInfo != null)
            {
                this.IntentionQuoteInfo.Clear();

                DataCount = 0;
                SelectedCount = 0;
                //CountText = $"共{dataCount},选中{selectedCount}";
            }
        }

        private bool isStartRecog = false;
        private IntentionQuoteItem AddRecognize(IntentionQuoteInfo obj)
        {
            if (this.IntentionQuoteInfo.Any(entry => entry.ClOrdID == obj.clordid))
            {
                return this.IntentionQuoteInfo.First(entry => entry.ClOrdID == obj.clordid);
            }

            if (isStartRecog)
            {
                isStartRecog = false;
            }


            IntentionQuoteItem item = new IntentionQuoteItem();

            item.IntentionId = obj.intentionId;
            item.Side = obj.side;
            item.SecurityId = obj.securityId;
            item.Symbol = obj.symbol;
            item.CleanPrice = obj.cleanPrice;
            item.FullPrice = obj.fullPrice;
            item.Price = obj.price;
            decimal qty = obj.qty.ConvertToDecimal() / 100;
            if (qty >= 1)
            {
                item.Qty = qty * 100;
            }
            //item.Qty = obj.qty;

            item.SettlType = obj.settlType;
            item.Text = obj.text;
            item.CreateTime = obj.createTime;
            item.UpdateTime = obj.updateTime;
            item.UserName = obj.userName;
            item.CloseBy = obj.closeBy;
            item.CloseByName = obj.closeByName;
            item.Status = obj.status;
            item.OrderID = obj.orderId;
            item.WholeFlag = obj.wholeFlag;
            item.ClOrdID = obj.clordid;
            item.TimeToMaturity = obj.timeToMaturity;
            item.TimeToMaturityToWorkDay = obj.timeToMaturityToWorkDay;
            item.TimeToMaturity_Convert = CommonUtils.ConvertRemainingTime(obj.timeToMaturity);

            try
            {
                //if (!_symbolCountDic.ContainsKey(er.securityid))
                //{
                //    _symbolCountDic[er.securityid] = null;
                //    LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{er.securityid}.");
                //    PriceHandler.Instance.RegisterCTPrice(er.securityid, UpdateCtPrice);
                //}

                CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(obj.securityId);
                if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                {
                    item.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                    item.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                    item.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                }
                else
                {
                    PriceHandler.Instance.RegisterCTPrice(obj.securityId, UpdateCtPrice);
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }

            item.PropertyChanged += Item_PropertyChanged;

            this.IntentionQuoteInfo.Add(item);

            return item;
        }

        private void UpdateCtPrice(MarketDataSnapshotFullRefresh price)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    CCDCValuaModel srcccdc = null;

                    try
                    {
                        srcccdc = CommonUtils.GetCCDCValua(price.SecurityID);
                        if (srcccdc == null)
                        {
                            srcccdc = CommonUtils.GenerateCCDCValue(price);

                            CommonUtils.SetCCDCValua(price.SecurityID, srcccdc);
                        }
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }

                    lock (_LockObject)
                    {
                        IEnumerable<IntentionQuoteItem> findDatas_best = IntentionQuoteInfo.Where(item => item.SecurityId == price.SecurityID);
                        foreach (IntentionQuoteItem findData in findDatas_best)
                        {
                            findData.CCDCValuation = srcccdc.CCDCValuation.ConvertToDecimal();
                            findData.CCDCValuation_Date = srcccdc.CCDCValuaDate;
                            findData.ShowLastWorkDay = srcccdc.ShowLastWorkDay;
                        }
                    }

                }));
            }
            PriceHandler.Instance.UnregisterCTPrice(price.SecurityID, UpdateCtPrice);
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }

        public void RecognizeIntentionQuote(string content)
        {
            // 测试数据
            //AddRecognize(new IntentionQuoteInfo { securityId = "180210.IB", settlType="2", orderId="1", clordid = "1", symbol = "18国开10", timeToMaturityToWorkDay = "休1", timeToMaturity ="10Y" });
            //AddRecognize(new IntentionQuoteInfo { securityId = "220002.IB", settlType = "2", orderId = "2", clordid = "2", symbol = "22附息国债02", timeToMaturityToWorkDay = "休1", timeToMaturity = "10Y" });
            //AddRecognize(new IntentionQuoteInfo { securityId = "220003.IB", settlType = "2", orderId = "3", clordid = "3", symbol = "22附息国债03", timeToMaturityToWorkDay = "休1", timeToMaturity = "10Y" });
            //AddRecognize(new IntentionQuoteInfo { securityId = "220004.IB", settlType = "2", orderId = "4", clordid = "4", symbol = "22附息国债04", timeToMaturityToWorkDay = "休1", timeToMaturity = "10Y" });
            //AddRecognize(new IntentionQuoteInfo { securityId = "220005.IB", settlType = "2", orderId = "5", clordid = "5", symbol = "22附息国债05", timeToMaturityToWorkDay = "休1", timeToMaturity = "10Y" });
            isStartRecog = true;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    string pattern = @"(\r\n)+";
                    string txtcont = System.Text.RegularExpressions.Regex.Replace(content, pattern, "\r\n");
                    RecognizeIntentionQuoteRequest request = new RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = txtcont;
                    ///获取订单 
                    // STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = TableOldHandler.Instance.RecognizeIntentionQuote(request);

                    RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserBond(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        else
                        {
                            if (reply.Datas != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                {
                                    int index = 0;
                                    foreach (IntentionQuoteInfo obj in reply.Datas)
                                    {
                                        IntentionQuoteItem item = AddRecognize(obj);
                                        item.No = ++index;
                                        await System.Windows.Threading.Dispatcher.Yield();
                                    }
                                }));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }


        public void RecognizeIntentionQuoteImage(string content)
        {
            isStartRecog = true;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    RecognizeIntentionQuoteRequest request = new RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = content;
                    ///获取订单 
                    // STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = TableOldHandler.Instance.RecognizeIntentionQuote(request);

                    RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserImage(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        else
                        {
                            if (reply.Datas != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                                {
                                    int index = 0;
                                    foreach (IntentionQuoteInfo obj in reply.Datas)
                                    {
                                        IntentionQuoteItem item = AddRecognize(obj);

                                        item.No = ++index;
                                        await System.Windows.Threading.Dispatcher.Yield();
                                    }
                                }));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }


        public void OnOkClick()
        {
            //Button btnOk = _ownUI.FindName("btnSendOK") as Button;

            //if (btnOk != null)
            //    btnOk.IsEnabled = false;

            List<HomeGroupSecurityidsUpdateReq> reqList = new List<HomeGroupSecurityidsUpdateReq>();
            foreach (IntentionQuoteItem model in intentionQuoteInfo)
            {
                if (model.IsChecked == false)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(model.SecurityId) && !string.IsNullOrEmpty(model.Symbol))
                {
                    HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                    request.securityid = model.SecurityId;
                    request.symbol = model.Symbol;
                    request.action = "1"; // 添加
                    request.remark = $"{request.securityid}  {request.symbol}  {model.RemainingTime}";
                    request.terminal = string.Format("BDBLlient_v{0}", LocalEnv.ServerInfo.Version);
                    request.remainingTime = model.RemainingTime;
                    reqList.Add(request);
                }
            }

            if (reqList.Count > 0)
            {
                ImportBonds(reqList);
            }
        }

        private ConcurrentDictionary<HomeGroupSecurityidsUpdateReq, PResult> resultDic = new ConcurrentDictionary<HomeGroupSecurityidsUpdateReq, PResult>();

        //private void AddReplyTxt(HomeGroupSecurityidsUpdateReq req, bool isEnd, int failIndex, int successNum)
        //{
        //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        if (ReplyVisibility == Visibility.Collapsed)
        //        {
        //            ReplyVisibility = Visibility.Visible;
        //            BatchVisibility = OneVisibility = Visibility.Collapsed;
        //        }
        //        //if (isEnd)
        //        {
        //            ImageSource = "/STC.Common;component/Images/stclite-ok.png";
        //            TxtBrush = _ownUI.FindResource("LogDefaultFontBrush") as Brush;
        //        }
        //        //else
        //        //{
        //        //    ImageSource = "/STC.Common;component/Images/stclite-smile.png";
        //        //    TxtBrush = _ownUI.FindResource("BlueButton.Static.Border") as Brush;
        //        //}

        //        ResultTxt = string.Format(string.Format("新增成功{0}，失败{1}", successNum, failIndex));
        //        StackPanel sp = _ownUI.FindName("spTxt") as StackPanel;

        //        TextBlock tb = new TextBlock();
        //        tb.Text = string.Format("{0}  {1}  {2}", req.securityid, req.symbol, req.remainingTime);
        //        tb.Margin = new Thickness(0, 3, 0, 3);
        //        tb.Foreground = _ownUI.FindResource("TextBox.Static.Foreground") as Brush;

        //        tb.HorizontalAlignment = HorizontalAlignment.Center;

        //        sp.Children.Add(tb);
        //    }
        //    ));
        //}

        public void ImportBonds(List<HomeGroupSecurityidsUpdateReq> reqList)
        {
            resultDic.Clear();

            ThreadPool.QueueUserWorkItem(o =>
            {

                try
                {
                    int index = 0;
                    int total = reqList.Count;
                    int failIndex = 0;
                    foreach (HomeGroupSecurityidsUpdateReq req in reqList)
                    {
                        index++;
                        req.groupid = GroupId;
                        PResult result = HomePageDataHandler.Instance.UpdateSecurityids(req);
                        resultDic[req] = result;

                        if (result != null && result.IsSuccess())
                        {
                            //AddReplyTxt(req, index == total, failIndex, index);
                        }
                        else
                            failIndex++;

                        Thread.Sleep(200);
                    }

                    HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfoByCache(GroupId);
                    string jsonStr = JsonConvert.SerializeObject(homeGroupInfo);
                    homeGroupInfo = JsonConvert.DeserializeObject<HomeGroupInfo>(jsonStr);

                    HomePageDataHandler.Instance.RequestGroups(true);

                    HomePageDataHandler.Instance.Notify(FavoriteTypeEnum.BatchImport, this.GroupId, string.Empty, string.Empty);

                    foreach (KeyValuePair<HomeGroupSecurityidsUpdateReq, PResult> pair in resultDic)
                    {
                        if (pair.Value != null && pair.Value.IsSuccess())
                        {
                            HomePageDataHandler.Instance.Notify(FavoriteTypeEnum.AddBondFromTable, this.GroupId, pair.Key.securityid, string.Empty);
                            //HomePageDataHandler.Instance.Notify(FavoriteTypeEnum.AddBond, this.GroupId, pair.Key.securityid, string.Empty);
                            break;
                        }
                    }

                    List<HomeGroupSecurityIDsInfo> idList = GetAddSecurity(HomePageDataHandler.Instance.GetHomeGroupInfoByCache(GroupId), homeGroupInfo);

                    foreach (HomeGroupSecurityIDsInfo securityIDsInfo in idList)
                    {
                        QuoteConfigHandler.Instance.NotifyOnGroupTableUpdate(this.GroupId, securityIDsInfo.id, securityIDsInfo.securityid, FavoriteTypeEnum.AddBond);
                    }


                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    //    Button btnOk = _ownUI.FindName("btnSendOK") as Button;

                    //    if (btnOk != null)
                    //        btnOk.IsEnabled = true;
                    //}));
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
            });
        }

        private List<HomeGroupSecurityIDsInfo> GetAddSecurity(HomeGroupInfo newHomeGroupInfo, HomeGroupInfo oldGroupInfo)
        {
            List<HomeGroupSecurityIDsInfo> idList = new List<HomeGroupSecurityIDsInfo>();
            //HomeGroupInfo newHo = HomePageDataHandler.Instance.GetHomeGroupInfoByCache(GroupId);
            if (newHomeGroupInfo != null && oldGroupInfo != null)
            {
                foreach (HomeGroupSecurityIDsInfo securityIDsInfo in newHomeGroupInfo.securityids)
                {
                    bool isExist = false;
                    foreach (HomeGroupSecurityIDsInfo oldSecurity in oldGroupInfo.securityids)
                    {
                        if (securityIDsInfo.id == oldSecurity.id)
                        {
                            isExist = true;
                            break;
                        }
                    }
                    if (!isExist)
                    {
                        idList.Add(securityIDsInfo);
                    }
                }
            }

            return idList;
        }

    }
}
