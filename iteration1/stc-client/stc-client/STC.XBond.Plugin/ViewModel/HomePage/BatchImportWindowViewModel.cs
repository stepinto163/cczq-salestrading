using com.gateway.log;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table.WorkBrench;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel.HomePage
{
    public class BatchImportWindowViewModel : ViewModelBase
    {
        private ICommand _top10Command;

        public ICommand Top10Command
        {
            get { return _top10Command; }
            set { _top10Command = value; }
        }

        private ICommand _top50Command;

        public ICommand Top50Command
        {
            get { return _top50Command; }
            set { _top50Command = value; }
        }

        private string _groupID;

        public string GroupID
        {
            get { return _groupID; }
            set 
            { 
                _groupID = value;
            
                if (!string.IsNullOrEmpty(_groupID))
                {
                    this.SelectedGroupName_Edit = HomePageDataHandler.GetGroupNameByGroupID(_groupID);
                    IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> dic = HomePageDataHandler.RequestGroups(false);
                    this.SelectedGroupItem_Edit = HomePageDataHandler.GetHomeGroupInfo(dic, _groupID);
                }
            }
        }

        /// <summary>
        /// 0:新建组  1：已有组
        /// </summary>
        private string _selectedGroupType = "0";

        public string SelectedGroupType
        {
            get { return _selectedGroupType; }
            set 
            {
                _selectedGroupType = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否关注
        /// </summary>
        private bool _isFollow;

        public bool IsFollow
        {
            get { return _isFollow; }
            set {
                _isFollow = value;
                this.OnPropertyChanged();
            }
        }

        private HomeGroupInfo _selectedGroupItem_Edit;

        public HomeGroupInfo SelectedGroupItem_Edit
        {
            get { return _selectedGroupItem_Edit; }
            set 
            { 
                _selectedGroupItem_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _selectedGroupName_Edit;

        public string SelectedGroupName_Edit
        {
            get { return _selectedGroupName_Edit; }
            set { _selectedGroupName_Edit = value;
                this.OnPropertyChanged();
            }
        }

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

        public BatchImportWindowViewModel()
        {
            IntentionQuoteInfo.CollectionChanged += IntentionQuoteInfo_CollectionChanged;
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.LiveFilteringProperties.Add("Status");
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);

            Top10Command = new ComRelayCommand(OnTop10Click);
            Top50Command = new ComRelayCommand(OnTop50Click);
        }

        private void OnTop50Click()
        {
            int i = 1;
            foreach(IntentionQuoteItem item in IntentionQuoteInfo)
            {
                if (i <= 50)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
                i++;
            }
            if (i > 52)
            {
                _IsAllChecked = false;
            }
            else
            {
                _IsAllChecked = true;
            }
            this.OnPropertyChanged("IsAllChecked");
        }

        private void OnTop10Click()
        {
            int i = 1;
            foreach (IntentionQuoteItem item in IntentionQuoteInfo)
            {
                if (i <= 10)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
                i++;
            }
            if (i > 12)
            {
                _IsAllChecked = false;
            }
            else
            {
                _IsAllChecked = true;
            }
            this.OnPropertyChanged("IsAllChecked");
        }

        private int dataCount = 0;

        public int DataCount
        {
            get { return dataCount; }
            set
            {
                dataCount = value;
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
                    item.OrderCount = ccdcValueModel.OrderCount;
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

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
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
                            findData.OrderCount = srcccdc.OrderCount;
                        }
                    }

                }));
            }
            PriceHandler.Instance.UnregisterCTPrice(price.SecurityID, UpdateCtPrice);
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

        #region 描述控件

        private ObservableCollection<DescriptionItem> _desSource = new ObservableCollection<DescriptionItem>();
        public ObservableCollection<DescriptionItem> DesSource
        {
            get { return _desSource; }
            set { _desSource = value; this.OnPropertyChanged(); }
        }

        public void SetDesSource(string key, string value)
        {
            //if (key == Utils.Constants.BONDTYPE)
            //    return;//key = Utils.Constants.BOND_SUB_TYPE;

            if (string.IsNullOrEmpty(value))
            {
                if (DesSource.Any(item => item.Key == key))
                {
                    DescriptionItem removeItem = DesSource.First(item => item.Key == key);
                    DesSource.Remove(removeItem);
                }
            }
            else
            {
                if (DesSource.Any(item => item.Key == key))
                {
                    DescriptionItem updateItem = DesSource.First(item => item.Key == key);
                    updateItem.Title = value;
                }
                else
                {
                    DesSource.Add(new DescriptionItem(value, key));
                }
            }
        }
        #endregion

 

    }
}
