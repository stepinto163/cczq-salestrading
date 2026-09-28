using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.Handler.FinQ;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.Table;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Common.ViewModel.WorkTable;
using STC.WorkBench.Plugin.View;
using STC.WorkBench.Plugin.View.WorkTable;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static STC.Common.View.Component.AutoCompleteComboBox;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class PublishQuoteViewModel : ViewModelBase
    {
        #region 报价标准
        private ObservableCollection<KeyValuePair<string, string>> _priceType_empty_Dic = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> PriceTypeEmptyDic
        {
            get
            {
                if (_priceType_empty_Dic.Count == 0)
                {
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("1", "净价"));//["1"] = "净价";
                    //_priceType_empty_Dic.Add(new KeyValuePair<string, string>("2", "全价"));//["2"] = "全价";
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("3", "到期"));//["3"] = "到期收益率";
                    _priceType_empty_Dic.Add(new KeyValuePair<string, string>("7", "行权"));//["7"] = "行权收益率";
                }
                return _priceType_empty_Dic;
            }
        }
        #endregion
        //private string _currentId;

        //public string CurrentId
        //{
        //    get
        //    { return _currentId; }
        //    set
        //    {
        //        _currentId = value;
        //    }
        //}

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

        private Visibility _BtnVisibility = Visibility.Visible;

        public Visibility BtnVisibility
        {
            get { return _BtnVisibility; }
            set
            {
                _BtnVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private IntentionMarketItem _currentMarketItem;

        public IntentionMarketItem CurrentMarketItem
        {
            get
            {
                return _currentMarketItem;
            }
            set
            {
                _currentMarketItem = value;

                if (value != null)
                {
                    BtnVisibility = Visibility.Collapsed;
                    IsToEnabled = false;
                }
                else
                {
                    BtnVisibility = Visibility.Visible;
                    IsToEnabled = true;
                }
            }
        }

        private bool _isEnabled = true;

        public bool IsToEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private string _Key;

        private IntentionQuoteItem _currentItem;

        public IntentionQuoteItem CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                _currentItem = value;

                _Key = value.IntentionId;
                if(LocalEnv.ConfInfo.IsPAYL)
                {
                    _Key = value.GroupKey;
                }
                
                if (value != null && value.SignDic1 != null && value.SignDic1.ContainsKey(Consts.BOND_SUB_TYPE))
                {
                    string bondSubType_tag = value.SignDic1[Consts.BOND_SUB_TYPE];
                    string securityType = CommonUtils.GetQQSecurityType(bondSubType_tag, value.BondSubType);
                    SecurityType = securityType;
                }

                this.OnPropertyChanged();
            }
        }

        private string _securityType;

        private string SecurityType
        {
            get
            {
                return _securityType;
            }
            set
            {
                _securityType = value;
                this.OnPropertyChanged();
            }
        }

        private IntentionMarketItem _selectedItem;

        public IntentionMarketItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                this.OnPropertyChanged();
            }
        }

        internal object _dataLock = new object();
        private CollectionViewSource _viewSource = new CollectionViewSource();

        public CollectionViewSource ViewSource
        {
            get { return _viewSource; }
            set { _viewSource = value; }
        }

        private CustomCollection<IntentionMarketItem> _datas = new CustomCollection<IntentionMarketItem>();
        public CustomCollection<IntentionMarketItem> Datas
        {
            get { return _datas; }
            set { _datas = value; }
        }

        #region 勾选
        private bool _hasSelected = false;
        // 是否选中全选多选框
        private bool _IsAllChecked = false;
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
                }
            }
        }

        private volatile bool isAdjustAllChkStatus = false;
        public void CheckBoxAllValueChange(bool isAllChecked)
        {
            int workingDataCount = 0;
            lock (_dataLock)
            {
                foreach (IntentionMarketItem item in this.Datas)
                {
                    item.IsChecked = isAllChecked;
                    workingDataCount++;
                }
            }


            if (workingDataCount == 0)
            {
                IsAllChecked = false;
                _hasSelected = false;
            }
            else
            {
                IsAllChecked = isAllChecked;
                _hasSelected = isAllChecked;
            }
        }

        public void AdjustAllChkStatus()
        {
            bool isCheckAll = true;

            int workingDataCount = 0;
            lock (_dataLock)
            {
                this._hasSelected = false;
                foreach (IntentionMarketItem item in this.Datas)
                {
                    workingDataCount++;
                    if (item.IsChecked == true)
                    {
                        this._hasSelected = true;
                    }
                    if (item.IsChecked == false)
                    {
                        isCheckAll = false;
                    }
                    if (this._hasSelected == true && isCheckAll == false)
                    {
                        break;
                    }
                }
                IsAllChecked = isCheckAll && workingDataCount != 0;
            }

        }

        public void CheckBoxSingleClick()
        {
            bool flag = true;
            int i = 0;
            lock (_dataLock)
            {
                foreach (IntentionMarketItem item in this.Datas)
                {
                    flag = flag && item.IsChecked;
                    if (item.IsChecked)
                    {
                        i++;
                    }
                }
                _IsAllChecked = this.Datas.Count > 0 && flag;
            }
            this.OnPropertyChanged("IsAllChecked");
        }
        #endregion

        private string senderNumber = string.Empty;

        public ICommand AddQuoteCommand { get; set; }

        public ICommand DelQuoteCommand { get; set; }

        public ICommand SendQuoteCommand { get; set; }

        public PublishQuoteViewModel()
        {
            string qqNumber = CommonUtils.GetQQNumber(LocalEnv.UserInfo.UserName);
            senderNumber = qqNumber; //"3003945772";// "3003945735";

            AddQuoteCommand =  new ComRelayCommand(this.DoAddQuoteCommand);
            DelQuoteCommand = new ComRelayCommand<Button>(this.DoDelQuoteCommand);
            SendQuoteCommand = new ComRelayCommand<Button>(this.DoSendQuoteCommand);

            
        }

        public void Clear()
        {
            lock (_dataLock)
            {
                if (Datas != null)
                    Datas.Clear();
            }
        }

        private void Instance_OnHistoryMarketQuotes(List<TableIntentionInquiry> datas)
        {
            if (datas == null)
                return;

            int batchSize = 500;
            var syncLock = new object(); // 新增集合访问锁

            Task.Run(() =>
            {
                try
                {
                    // 获取数据快照
                    List<TableIntentionInquiry> snapshot;
                    lock (datas)
                    {
                        snapshot = datas.ToList(); // 线程安全拷贝
                    }

                    for (int batchIndex = 0; batchIndex < snapshot.Count; batchIndex += batchSize)
                    {
                        var currentBatch = snapshot.Skip(batchIndex).Take(batchSize).ToList();

                        // 异步更新UI（非阻塞）
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var data in currentBatch)
                            {
                                //if (startDate_intention.CompareTo(data.updateTime) <= 0 && endDate_intention.CompareTo(data.updateTime) >= 0)
                                //{
                                OnMarketQuotes(data);
                                //}
                            }
                        }, System.Windows.Threading.DispatcherPriority.Send); // 调整优先级

                        // 优化CPU释放策略
                        if (batchIndex % (batchSize * 2) == 0 && batchIndex != 0)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    // 处理窗口关闭时的正常终止
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex); // UI线程记录日志
                }
            });
        }

        private void OnMarketQuotes(TableIntentionInquiry obj)
        {
            if (obj.intentionId != _Key)
            {
                return;
            }

            if (obj.sendType == 3)
            {
                return;
            }

            lock (_dataLock)
            {
                string preName = string.Empty;
                string preCode = string.Empty;
                ToOption toOption = null;
                if (JsonSplit.IsJson(obj.info5))
                {
                    toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);
                    int lineIndex = toOption.DispayName.IndexOf("-");
                    if (lineIndex > -1)
                    {
                        preName = toOption.DispayName.Substring(0, lineIndex);
                        preCode = ComboSourceUtil.Instance.FindEnum(preName, "venues_cn_to_en");
                    }
                }
                IntentionMarketItem item = Datas.FirstOrDefault(entry => entry.Id == obj.id 
                || (!string.IsNullOrEmpty(entry.SourceTrader) && !string.IsNullOrEmpty(preName)
                && entry.OptionType == "sendIntentBrokers" && ComboSourceUtil.Instance.FindEnum(entry.SourceTrader, "venuesxb") == preName));

                if (item != null)
                {
                    if ((CurrentMarketItem == null && obj.status != "1") || (CurrentMarketItem != null && CurrentMarketItem.Id != obj.id))
                    {
                        if (item.OptionType != "sendIntentBrokers")
                        {
                            Datas.Remove(item);
                        }
                        return;
                    }

                    item.Id = obj.id;
                    item.QuoteId = obj.quoteId;

                    if (toOption != null)
                    {
                        item.Receiver_Edit = toOption;
                        item.ReceiverNumber = toOption.DispayName;
                        item.ReceiverNumber_Edit = toOption.DispayName;
                        item.OptionType = toOption.OptionType;
                        item.SourceTrader = preCode;
                    }
                    if (CurrentMarketItem != null)
                    {
                        item.IsChecked = true;
                    }
                    item.IntentionId = obj.intentionId;
                    item.SecurityId = obj.securityId;
                    item.SendType = obj.sendType;

                    if (obj.sendType == 3)
                    {
                        item.Side = obj.side == "1" ? "3" : "4";
                    }
                    else
                    {
                        item.Side = obj.side;
                    }
                    item.Price = obj.price;
                    //item.Price_Edit = obj.price;
                    item.Qty = obj.qty;
                    //item.Qty_Edit = obj.qty;
                    item.Name = obj.name;
                    item.UserName = obj.userName;
                    item.UserId = obj.userId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.SettlType = obj.settlType;
                    item.Status = obj.status;
                    item.Action = obj.action;
                    item.PFlag = item.PFlag_Edit = "0";
                    if (obj.bargainFlag == "1")
                    {
                        item.PFlag = item.PFlag_Edit = "1";
                    }
                    else if (obj.quoteDividedFlag == "Y")
                    {
                        item.PFlag = item.PFlag_Edit = "2";
                    }
                    item.OCOFlag = item.OCOFlag_Edit = obj.ocoFlag;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    item.SenderNumber = obj.senderNumber;
                    //item.ReceiverNumber = obj.receiverNumber;
                    item.Remark = obj.info1;
                    item.Remark_Edit = obj.info1;
                    item.ClOrdID = obj.clOrdID;
                    item.ExecOrderId = obj.execOrderId;
                    item.Venue = obj.venue;
                    item.VenueTypeGW = obj.venueTypeGW;
                    if (JsonSplit.IsJson(obj.info3))
                    {
                        InquiryInfo3 ii3 = JsonConvert.DeserializeObject<InquiryInfo3>(obj.info3);
                        if (ii3 != null)
                        {
                            item.PriceType = ii3.PriceType;
                        }
                    }
                    item.GroupKey =  $"{this.CurrentItem.QuoteType}_{this.CurrentItem.Trader_Buyer}_{this.CurrentItem.SecurityId}_{this.CurrentItem.Side}";
                }
                else
                {
                    if ((CurrentMarketItem == null && obj.status != "1") || (CurrentMarketItem != null && CurrentMarketItem.Id != obj.id))
                    {
                        return;
                    }

                    item = new IntentionMarketItem();
                    item.Id = obj.id;
                    item.QuoteId = obj.quoteId;

                    if (toOption != null)
                    {
                        item.Receiver_Edit = toOption;
                        item.ReceiverNumber = toOption.DispayName;
                        item.ReceiverNumber_Edit = toOption.DispayName;
                        item.OptionType = toOption.OptionType;
                        item.SourceTrader = preCode;
                    }

                    if (CurrentMarketItem != null)
                    {
                        item.IsChecked = true;
                    }
                    item.IntentionId = obj.intentionId;
                    item.SecurityId = obj.securityId;
                    item.SendType = obj.sendType;
                    if (obj.sendType == 3)
                    {
                        item.Side = obj.side == "1" ? "3" : "4";
                    }
                    else
                    {
                        item.Side = obj.side;
                    }

                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    if (JsonSplit.IsJson(obj.info3))
                    {
                        InquiryInfo3 ii3 = JsonConvert.DeserializeObject<InquiryInfo3>(obj.info3);
                        if (ii3 != null)
                        {
                            item.PriceType = ii3.PriceType;
                        }
                    }

                    if (CurrentItem.Type == "INTENTION_GROUP")
                    {
                        IEnumerable<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> list = IntentionOldHandler.Instance.GetSameGroupDataList(CurrentItem.QuoteType, CurrentItem.Trader_Buyer, CurrentItem.SecurityId, CurrentItem.Side);
                        if (list != null && list.Count() > 0)
                        {
                            decimal? price = null;
                            decimal? qty = null;
                            if (CurrentItem.Side == "1")
                            {
                                price = list.Min(e => e.price);
                            }
                            else if (CurrentItem.Side == "2")
                            {
                                price = list.Max(e => e.price);
                            }
                            if (list.Any(x => x.price != price && x.status == CurrentItem.Status))
                            {
                                qty = list.First(x => x.price == price).qty;
                            }
                            else
                            {
                               qty = list.Sum(e => e.qty);
                            }
                             
                           
                            item.Price_Edit = price;
                            item.Qty_Edit = qty;
                        }
                    }
                    else
                    {
                        item.Price_Edit = CurrentItem.Price;
                        item.Qty_Edit = CurrentItem.Qty;
                    }


                    item.Name = obj.name;
                    item.UserName = obj.userName;
                    item.UserId = obj.userId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.SettlType = obj.settlType;
                    item.Status = obj.status;
                    item.Action = obj.action;
                    item.PFlag = item.PFlag_Edit = "0";
                    if (obj.bargainFlag == "1")
                    {
                        item.PFlag = item.PFlag_Edit = "1";
                    }
                    else if (obj.quoteDividedFlag == "Y")
                    {
                        item.PFlag = item.PFlag_Edit = "2";
                    }
                    item.OCOFlag = item.OCOFlag_Edit = obj.ocoFlag;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    item.SenderNumber = obj.senderNumber;
                    //item.ReceiverNumber = obj.receiverNumber;
                    item.Remark = obj.info1;
                    item.Remark_Edit = obj.info1;
                    item.ClOrdID = obj.clOrdID;
                    item.ExecOrderId = obj.execOrderId;
                    item.Venue = obj.venue;
                    item.VenueTypeGW = obj.venueTypeGW;
                    item.GroupKey = $"{this.CurrentItem.QuoteType}_{this.CurrentItem.Trader_Buyer}_{this.CurrentItem.SecurityId}_{this.CurrentItem.Side}";
                    Datas.Add(item);
                }
            }
        }

        private void DoAddQuoteCommand()
        {
            IntentionMarketItem newItem = new IntentionMarketItem();
            newItem.IntentionId = _Key;
            newItem.Status = string.Empty;
            newItem.Side = this.CurrentItem.Side;
            newItem.SecurityId = this.CurrentItem.SecurityId;
            if (CurrentItem.Type == "INTENTION_GROUP")
            {
                IEnumerable<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> list = IntentionOldHandler.Instance.GetSameGroupDataList(CurrentItem.QuoteType, CurrentItem.Trader_Buyer, CurrentItem.SecurityId, CurrentItem.Side);
                if (list != null)
                {
                    decimal? price = null;
                    decimal? qty = null;
                    if (CurrentItem.Side == "1")
                    {
                        price = list.Min(e => e.price);
                    }
                    else if (CurrentItem.Side == "2")
                    {
                        price = list.Max(e => e.price);
                    }
                    if (list.Any(x => x.price != price && x.status == CurrentItem.Status))
                    {
                        qty = list.First(x => x.price == price).qty;
                    }
                    else
                    {
                        qty = list.Sum(e => e.qty);
                    }
                    newItem.Price = newItem.Price_Edit = price;
                    newItem.Qty = newItem.Qty_Edit = qty;
                    newItem.PriceType = newItem.PriceType_Edit = string.IsNullOrEmpty(CurrentItem.PriceType) ? "3" : CurrentItem.PriceType;
                }
            }
            else
            {
                newItem.Price = newItem.Price_Edit = CurrentItem.Price;
                newItem.Qty = newItem.Qty_Edit = CurrentItem.Qty;
                newItem.PriceType = newItem.PriceType_Edit = string.IsNullOrEmpty(CurrentItem.PriceType) ? "3" : CurrentItem.PriceType;
            }
            //newItem.SettlType = CurrentItem.SettlType;

            lock (_dataLock)
            {
                Datas.Add(newItem);
            }

            IsAllChecked = false;
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
                    
                    CheckBoxSingleClick();
                }
            }
        }

        private void DoEditQuoteCommand()
        {
            
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

        private void DoSendQuoteCommand(Button btn)
        {
            List<IntentionMarketItem> dataList = null;
            lock(_dataLock)
            {
                if (Datas != null)
                {
                    dataList = Datas.ToList<IntentionMarketItem>();
                }
            }

            if (dataList == null || !dataList.Any(entry => entry.IsChecked))
            {
                CustomMessageBox.Info($"请勾选要发送的记录", "发送", _owner);
                return;
            }

            if (dataList.Any(entry => entry.IsChecked && entry.Receiver_Edit == null && entry.ReceiverNumber != "内部"))
            {
                CustomMessageBox.Info($"请选择To值", "发送", _owner);
                return;
            }

            //// 检查报量是否为空
            //if (LocalEnv.ConfInfo.IsPAYL == false && dataList.Any(entry => entry.IsChecked && !entry.Qty_Edit.HasValue && entry.OptionType != "sendIntentGroups" && entry.OptionType != "sendIntentQQGroups"))
            //{
            //    CustomMessageBox.Info($"量不可为空", "发送", _owner);
            //    return;
            //}

            SetDoAllBtnEnable(false);

            //System.Threading.ThreadPool.QueueUserWorkItem(o =>
            //{
            //    // 发送内部
            //    if (dataList != null && dataList.Count > 0)
            //    {
            //        IntentionMarketItem innerItem = dataList[0];
            //        if (innerItem.OptionType == "Inner" && innerItem.IsChecked)
            //        {
            //            if (this.CurrentItem.Status == "0")
            //            {
            //                // 更新元素
            //                STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request = GetIntentionOrder(innerItem);
            //                PResult result = TableOldHandler.Instance.UpdateIntentionQuote(request);
            //                string errMsg = result == null ? $"意向id:[{request.intentionId}]内部单发送失败" : (!result.IsSuccess() ? result.Msg : string.Empty);
            //                if (string.IsNullOrEmpty(errMsg))
            //                {
            //                    // 发布
            //                    DoConfirmOne(CurrentItem);
            //                }
            //                else
            //                {
            //                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //                    {
            //                        MessageBoxGUI msgBox = new MessageBoxGUI();
            //                        msgBox.setWarnMsg(errMsg);
            //                        //msgBox.Owner = Window.GetWindow(btn);
            //                        msgBox.ShowDialog();

            //                    }));
            //                }
            //            }
            //            else if (this.CurrentItem.Status == "1")
            //            {
            //                // 更新元素
            //                NewOrderSingle sendPricePo = GetNewOrderSingle(innerItem);
            //                //if (sendPricePo.Price != inneritem.)
            //                sendPricePo.HandlInst = "2";
            //                RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
            //                string errMsg = result == null ? $"意向id:[{sendPricePo.IntentionID}]内部单发送失败" : (!result.IsSuccess() ? result.Msg : string.Empty);
            //                if (!string.IsNullOrEmpty(errMsg))
            //                {
            //                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //                    {
            //                        MessageBoxGUI msgBox = new MessageBoxGUI();
            //                        msgBox.setWarnMsg(errMsg);
            //                        //msgBox.Owner = Window.GetWindow(btn);
            //                        msgBox.ShowDialog();

            //                    }));
            //                }
            //            }
            //        }
            //    }
            //});

            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
            {
                //如果是待处理，要变成处理中
                if (_currentItem.Status == "80")
                {
                    DoConfirmOne(_currentItem);
                }
            }

            //发送询价
            Sub_DoSendQuoteCommand();

            Window win = Window.GetWindow(btn);
            if (win != null)
            {
                win.Close();
            }
        }

        /// <summary>
        /// 生成订单报文, 这里应该拿改后的值
        /// ， 界面上控制哪些可以修改， 哪些不能修改
        /// 撤单条件，订阅 是否传值， 
        /// </summary>
        /// <returns></returns>
        private STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest GetIntentionOrder(IntentionMarketItem innerItem)
        {
            STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest();
            request.intentionId = _Key;
            request.institutionId = this.CurrentItem.InstitutionId;
            request.institutionName = this.CurrentItem.InstitutionName;
            request.traderId = this.CurrentItem.TraderId;
            request.traderName = this.CurrentItem.TraderName;
            request.side = this.CurrentItem.Side;

            if (innerItem.Price_Edit.HasValue)
                request.price = innerItem.Price_Edit.Value;

            if (innerItem.Qty_Edit.HasValue)
                request.qty = innerItem.Qty_Edit.Value;

            //if (sendPriceViewModel.SettleBoxIndex == 1)
            //{
            //    request.settlType = sendPriceViewModel.SendPriceItem.SettleType;
            //}
            //else
            //    request.settlType = GetSetttle();

            request.settlType = innerItem.SettlType_Edit;

            request.wholeFlag = this.CurrentItem.WholeFlag;
            request.userName = this.CurrentItem.UserName;
            request.closeBy = LocalEnv.UserInfo.UserName;
            request.closeByName = LocalEnv.UserInfo.FullName;

            request.hurryFlag = innerItem.HurryFlag_Edit;
            request.suggestFlag = innerItem.SuggestFlag_Edit;
            request.fireFlag = innerItem.FireFlag_Edit;
            request.floorPrice = innerItem.FloorPrice_Edit;
            request.OcoFlag = innerItem.OCOFlag_Edit;
            request.publicRemarks = this.CurrentItem.PublicRemarks;
            request.personalRemarks = this.CurrentItem.PersonalRemarks;
            request.customerPrice = this.CurrentItem.CustomerPrice;

            decimal? bp = null;
            if (innerItem.Price_Edit.HasValue && this.CurrentItem.CustomerPrice.HasValue)
            {
                if (this.CurrentItem.Side == "1")
                {
                    bp = (innerItem.Price_Edit - this.CurrentItem.CustomerPrice) * 100;
                }
                else
                {
                    bp = (this.CurrentItem.CustomerPrice - innerItem.Price_Edit) * 100;
                }
            }
            request.customerSpread = bp;

            TagData info2tag = null;
            if (this.CurrentItem != null && JsonSplit.IsJson(this.CurrentItem.Info2))
            {
                info2tag = JsonConvert.DeserializeObject<TagData>(this.CurrentItem.Info2);
            }
            else
            {
                info2tag = new TagData();
            }
           
            info2tag.HURRY_FLAG = innerItem.HurryFlag_Edit;
            info2tag.SUGGEST_FLAG = innerItem.SuggestFlag_Edit;
            info2tag.FIRE_FLAG = innerItem.FireFlag_Edit;
            info2tag.FLOOR_PRICE = innerItem.FloorPrice_Edit;
            info2tag.CUSTOMER_SPREAD = bp.HasValue ? bp.ToString() : string.Empty;
            info2tag.OCO_FLAG = innerItem.OCOFlag_Edit;

            request.info2 = JsonConvert.SerializeObject(info2tag);
            request.VipCode = info2tag.VIP;
            request.OrgType = info2tag.ORG_TYPE;
            request.IfSubBond = info2tag.IF_SUB_BOND;
            request.channel = (this.CurrentItem == null || string.IsNullOrEmpty(this.CurrentItem.Channel)) ? "CLIENT" : this.CurrentItem.Channel;
            if (this.CurrentItem != null && !string.IsNullOrEmpty(this.CurrentItem.QuoteType))
            {
                request.QuoteType = this.CurrentItem.QuoteType;
            }
            return request;
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

        /// <summary>
        /// 生成订单报文
        /// </summary>
        /// <returns></returns>
        private NewOrderSingle GetNewOrderSingle(IntentionMarketItem innerItem)
        {
            NewOrderSingle sendPricePo = new NewOrderSingle();

            sendPricePo.AlgoName = "CROSS";
            sendPricePo.VenueTypeGW = "INNER";
            sendPricePo.Venues = "MANUAL";

            sendPricePo.ClOrdID = DataTimeSequence.Instance.GetId();
            sendPricePo.Info1 = this.CurrentItem.Info1;
            sendPricePo.Info2 = this.CurrentItem.Info2;
            sendPricePo.IntentionID = _Key;

            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(this.CurrentItem.SecurityId);
            if (bondsInfoPo != null)
                sendPricePo.MarketIndicator = bondsInfoPo.MarketIndicator;
            else
                sendPricePo.MarketIndicator = "4";

            sendPricePo.OrdType = "2";

            if (innerItem.Qty_Edit.HasValue)
                sendPricePo.OrderQty = innerItem.Qty_Edit.Value.ToString("0");

            if (innerItem.Price_Edit.HasValue)
                sendPricePo.Price = innerItem.Price_Edit.Value.ToString("0.00##");
            sendPricePo.PriceType = "3"; //收益率

            sendPricePo.SecurityID = this.CurrentItem.SecurityId;

            sendPricePo.RiskManualCk = string.Empty;//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
            //if (sendPriceViewModel.SettleBoxIndex == 1)
            //{
            //    sendPricePo.SettlType = sendPriceViewModel.SendPriceItem.SettleType;
            //}
            //else
            //    sendPricePo.SettlType = GetSetttle();
            sendPricePo.SettlType = innerItem.SettlType_Edit;

            sendPricePo.Side = this.CurrentItem.Side;

            sendPricePo.Terminal = string.Format("STClient_{0}", LocalEnv.ServerInfo.Version);
            sendPricePo.TimeInForce = "0";
            //选中：0(nonfirm)，未选中：1(firm)
            sendPricePo.FirmFlag = this.CurrentItem.FirmFlag;
            sendPricePo.WholeFlag = this.CurrentItem.WholeFlag;
            sendPricePo.QuantID = "";
            decimal d = 0;


            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();

            sendPricePo.InstitutionID = this.CurrentItem.InstitutionId;
            //sendPricePo.InstitutionName = requireParseViewModel.SelectedInstintuion_Edit == null ? string.Empty : requireParseViewModel.SelectedInstintuion_Edit.shortnamecn;
            sendPricePo.InstitutionName = this.CurrentItem.InstitutionName;
            sendPricePo.TraderID = this.CurrentItem.TraderId;
            //sendPricePo.TraderName = requireParseViewModel.SelectedTrader_Edit == null ? string.Empty : requireParseViewModel.SelectedTrader_Edit.namecn;
            sendPricePo.TraderName = this.CurrentItem.TraderName;


            sendPricePo.Text = this.CurrentItem.Text;

            sendPricePo.PriceType = this.CurrentItem.PriceType;
           

            sendPricePo.UserName = this.CurrentItem.UserName;
            sendPricePo.Name = LocalEnv.UserInfo.UserName; //放当前操作员


            decimal? bp = null;
            if (innerItem.Price_Edit.HasValue && this.CurrentItem.CustomerPrice.HasValue)
            {
                if (this.CurrentItem.Side == "1")
                {
                    bp = (innerItem.Price_Edit - this.CurrentItem.CustomerPrice) * 100;
                }
                else
                {
                    bp = (this.CurrentItem.CustomerPrice - innerItem.Price_Edit) * 100;
                }
            }

            sendPricePo.Info1 = this.CurrentItem.Info1;
            OrderTimeModel otm = null;
            if (this.CurrentItem != null && JsonSplit.IsJson(this.CurrentItem.Info1))
            {
                otm = JsonConvert.DeserializeObject<OrderTimeModel>(this.CurrentItem.Info1);
            }
            else
            {
                otm = new OrderTimeModel();
            }

            //标签
            sendPricePo.ValidUntilTime = otm.ValidUntilTime;
            sendPricePo.IfSubBond = otm.IF_SUB_BOND;
            sendPricePo.OrgType = otm.ORG_TYPE;
            sendPricePo.VipCode = otm.VIP;
            sendPricePo.OCOFlag = innerItem.OCOFlag_Edit;

            TagData info2tag = null;
            if (this.CurrentItem != null && JsonSplit.IsJson(this.CurrentItem.Info2))
            {
                info2tag = JsonConvert.DeserializeObject<TagData>(this.CurrentItem.Info2);
            }
            else
            {
                info2tag = new TagData();
            }

            info2tag.HURRY_FLAG = innerItem.HurryFlag_Edit;
            info2tag.SUGGEST_FLAG = innerItem.SuggestFlag_Edit;
            info2tag.FIRE_FLAG = innerItem.FireFlag_Edit;
            info2tag.FLOOR_PRICE = innerItem.FloorPrice_Edit;
            info2tag.CUSTOMER_SPREAD = bp.HasValue ? bp.ToString() : string.Empty;
            info2tag.OCO_FLAG = innerItem.OCOFlag_Edit;

            sendPricePo.Info2 = JsonConvert.SerializeObject(info2tag);



            sendPricePo.StrategyOrderId = this.CurrentItem.OrderID;

            sendPricePo.Symbol = this.CurrentItem.Symbol;

            if (LocalEnv.ConfInfo.IsAccountUser == "1")
            {
                sendPricePo.AccountID = this.CurrentItem.AccountID;
                sendPricePo.AccountName = this.CurrentItem.AccountName;
            }

            sendPricePo.Channel = (this.CurrentItem == null || string.IsNullOrEmpty(this.CurrentItem.Channel)) ? "CLIENT" : this.CurrentItem.Channel;
            if (this.CurrentItem != null && !string.IsNullOrEmpty(this.CurrentItem.QuoteType))
            {
                sendPricePo.QuoteType = this.CurrentItem.QuoteType;
            }

            //sendPricePo.HurryFlag = tagconditionviewmodel.HurryFlag;
            //sendPricePo.HurryFlag = tagconditionviewmodel.HurryFlag;
            //sendPricePo.SuggestFlag = tagconditionviewmodel.SuggestFlag;
            //sendPricePo.FireFlag = tagconditionviewmodel.FireFlag;
            //sendPricePo.FloorPrice = tagconditionviewmodel.FloorPrice;
            //sendPricePo.OCOFlag = tagconditionviewmodel.OcoFlag;
            //sendPricePo.PublicRemarks = tagconditionviewmodel.PublicRemarks;
            //sendPricePo.PersonalRemarks = tagconditionviewmodel.PersonalRemarks;
            //request.customerPrice = info.customerPrice.Value;
            //request.customerSpread = info.customerSpread.Value;

            //string s = JsonConvert.SerializeObject(sendPricePo);
            return sendPricePo;
        }

        private void Sub_DoSendQuoteCommand()
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

                int totalCount_local = 0;
                int failCount_local = 0;
                //Dictionary<string, IntentionMarketItem> orderDic = new Dictionary<string, IntentionMarketItem>();
                List<TableIntentionInquiry> refList = new List<TableIntentionInquiry>();
                List<TableIntentionInquiry> inquiryList = new List<TableIntentionInquiry>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                //List<string> existToList = new List<string>();
                foreach (IntentionMarketItem item in dataList)
                {
                    if (item.IsChecked == false || item.OptionType == "Inner") continue;
                    //GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    //if (exceptionReplyDic.ContainsKey(item.IntentionId))
                    //{
                    //    continue;
                    //}
                    //if (item.OptionType == "sendIntentBrokers")
                    //{
                    //    if (existToList.Contains(item.ReceiverNumber_Edit))
                    //    {
                    //        continue;
                    //    }
                    //}
                    //else
                    //{
                    //    if (existToList.Contains(item.ReceiverNumber_Edit))
                    //    {
                    //        continue;
                    //    }
                    //}
                    List<TableIntentionInquiry> refInquiryList = IntentionOldHandler.Instance.GetSameToInquiry(_Key, item.ReceiverNumber_Edit, item.Id);
                    bool isRef = false;
                    if (refInquiryList != null)
                    {
                        foreach (TableIntentionInquiry refInquiry in refInquiryList)
                        {
                            if (refInquiry != null)
                            {
                                string displayName = string.Empty;
                                if (JsonSplit.IsJson(refInquiry.info5))
                                {
                                    ToOption toOption = JsonConvert.DeserializeObject<ToOption>(refInquiry.info5);
                                    if (toOption != null)
                                    {
                                        displayName = toOption.DispayName;
                                    }
                                }
                                else
                                {
                                    displayName = refInquiry.receiverName;
                                }
                                if (refInquiry.id != item.Id || displayName != item.ReceiverNumber_Edit)
                                {
                                    refInquiry.biztypeid = LocalEnv.UserInfo.Termids;
                                    if (!refList.Any(x => x.id == refInquiry.id))
                                    {
                                        refInquiry.action = "5";
                                        refList.Add(refInquiry);
                                    }
                                    isRef = true;
                                }
                            }
                        }
                    }

                    //if (item.OptionType == "sendIntentBrokers")
                    //{
                    //    existToList.Add(item.ReceiverNumber);
                    //}
                    //else
                    //{

                    //}
                    TableIntentionInquiry data = new TableIntentionInquiry();
                    if (!isRef)
                    {
                        if (item != null && item.Status == "1")
                        {
                            data.quoteId = item.QuoteId;
                            data.id = item.Id;
                            data.createTime = item.CreateTime;
                        }
                    }

                    data.biztypeid = LocalEnv.UserInfo.Termids;
                    data.securityId = this.CurrentItem.SecurityId;
                    data.institutionId = string.Empty;//????
                    data.institutionName = string.Empty;//????
                    data.traderId = string.Empty;//????
                    data.traderName = string.Empty;//????
                    data.intentionId = _Key;
                    data.inquiryKey = _Key;
                    string side = this.CurrentItem.Side;
                    if (this.CurrentItem.Side == "3")
                    {
                        side = "1";
                    }
                    else if (this.CurrentItem.Side == "4")
                    {
                        side = "2";
                    }
                    data.side = side;
                    data.price = item.Price_Edit?.ToString("0.####").ConvertToDecimal();
                    data.qty = item.Qty_Edit?.ToString("0").ConvertToDecimal();
                    data.settlType = item.SettlType_Edit;
                    string groupKey = $"{this.CurrentItem.QuoteType}_{this.CurrentItem.Trader_Buyer}_{this.CurrentItem.SecurityId}_{this.CurrentItem.Side}";
                    data.strategyOrderId = groupKey;
                    if (item.Receiver_Edit != null && item.Receiver_Edit.OptionType == "sendIntentBrokers")
                    {
                        data.receiverNumber = item.Receiver_Edit.Broker.qq; //"3004330557";// todo: 
                        data.receiverName = item.Receiver_Edit.Broker.name;
                        data.info5 = JsonConvert.SerializeObject(item.Receiver_Edit);
                        data.sendType = 0;
                        data.venueTypeGW = "SOR";
                        data.venue = item.SourceTrader;
                    }
                    else if (item.Receiver_Edit != null && item.Receiver_Edit.OptionType == "sendIntentContacts")
                    {
                        data.receiverNumber = item.Receiver_Edit.Sender.qq;
                        data.receiverName = item.Receiver_Edit.Sender.name;
                        data.batchReceiverNumberMap = new Dictionary<string, string>();
                        data.batchReceiverNumberMap[item.Receiver_Edit.Sender.qq] = item.Receiver_Edit.Sender.name;
                        data.info5 = JsonConvert.SerializeObject(item.Receiver_Edit);
                        data.sendType = 0;
                        data.venueTypeGW = "SOR";
                        data.venue = "QQ";
                    }
                    else if (item.Receiver_Edit != null && item.Receiver_Edit.OptionType == "sendIntentCounterparties")
                    {
                        data.receiverNumber = item.Receiver_Edit.Sender.qq;
                        data.receiverName = item.Receiver_Edit.Sender.name;
                        data.batchReceiverNumberMap = new Dictionary<string, string>();
                        data.batchReceiverNumberMap[item.Receiver_Edit.Sender.qq] = item.Receiver_Edit.Sender.name;
                        data.info5 = JsonConvert.SerializeObject(item.Receiver_Edit);
                        data.sendType = 0;
                        data.venueTypeGW = "SOR";
                        data.venue = "QQ";
                    }
                    else if (item.Receiver_Edit != null && item.Receiver_Edit.OptionType == "sendIntentGroups")
                    {
                        if (item.Receiver_Edit.Groups != null
                                        && item.Receiver_Edit.Groups.members != null
                                        && item.Receiver_Edit.Groups.members.Count > 0)
                        {
                            data.batchReceiverNumberMap = new Dictionary<string, string>();
                            foreach (UserConfig uc in item.Receiver_Edit.Groups.members)
                            {
                                data.batchReceiverNumberMap[uc.qq] = uc.name;
                            }
                        }
                        data.info5 = JsonConvert.SerializeObject(item.Receiver_Edit);
                        data.sendType = 2;
                        string price = data.price.HasValue ? data.price?.ToString("0.00##") : string.Empty;
                        string qty = data.qty.HasValue ? data.qty?.ToString("0") + "w" : string.Empty;
                        data.text = $"{CurrentItem.RemainingTime} {CurrentItem.Symbol} {CurrentItem.SecurityId} {ComboSourceUtil.Instance.FindEnum(CurrentItem.Side, "side")} {price} {qty}";
                        data.venueTypeGW = "SOR";
                        data.venue = "QQ";
                    }
                    else if (item.Receiver_Edit != null && item.Receiver_Edit.OptionType == "sendIntentQQGroups")
                    {
                        data.receiverGroupOpenId = item.Receiver_Edit.QQGroups.groupOpenID;
                        data.receiverGroupName = item.Receiver_Edit.QQGroups.groupName;
                        data.info5 = JsonConvert.SerializeObject(item.Receiver_Edit);
                        data.sendType = 1;
                        string price = data.price.HasValue ? data.price?.ToString("0.00##") : string.Empty;
                        string qty = data.qty.HasValue ? data.qty?.ToString("0") + "w" : string.Empty;
                        data.text = $"{CurrentItem.RemainingTime} {CurrentItem.Symbol} {CurrentItem.SecurityId} {ComboSourceUtil.Instance.FindEnum(CurrentItem.Side, "side")} {price} {qty}";
                        data.venueTypeGW = "SOR";
                        data.venue = "GQQ";
                    }
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

                    inquiryList.Add(data);
                    //orderDic[item.IntentionId] = item;
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                    IntentionInquiryRequest request;
                    if (refList != null && refList.Count > 0)
                    {
                        request = new IntentionInquiryRequest();
                        request.intentionId = _Key;
                        request.inquiryKey = _Key;
                        request.userName = LocalEnv.UserInfo.UserName;
                        request.biztypeid = LocalEnv.UserInfo.Termids;
                        request.action = "3";
                        request.dataList = refList;

                        GetErrorInfo_Inquiry(request,
                                ref failCount,
                                ref totalCount,
                                warnReplyDic,
                                prohibitReplyDic,
                                exceptionReplyDic);

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (failCount > 0 || failCount_local > 0)
                            {
                                if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                                {
                                    Dictionary<string, IntentionQuoteItem> tempDic = new Dictionary<string, IntentionQuoteItem>();
                                    foreach (TableIntentionInquiry inquiry in inquiryList)
                                    {
                                        tempDic[Guid.NewGuid().ToString()] = null;
                                    }
                                    BatchResultWindow batchResultWindow = new BatchResultWindow(tempDic,
                                        warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
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
                    }
                    

                    if (failCount > 0 || failCount_local > 0)
                    {
                        return;
                    }

                    totalCount = 0;
                    failCount = 0;
                    warnReplyDic.Clear();
                    prohibitReplyDic.Clear();

                    request = new IntentionInquiryRequest();
                    request.intentionId = _Key;
                    request.inquiryKey = _Key;
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
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                Dictionary<string, IntentionQuoteItem> tempDic = new Dictionary<string, IntentionQuoteItem>();
                                foreach (TableIntentionInquiry inquiry in inquiryList)
                                {
                                    tempDic[Guid.NewGuid().ToString()] = null;
                                }
                                BatchResultWindow batchResultWindow = new BatchResultWindow(tempDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
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
            }
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

        private void GetLocalCheckErrorInfo(IntentionMarketItem item,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                BaseReplyPo replyPo = CheckParam(item);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10001")
                        {
                            exceptionReplyDic[item.IntentionId] = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
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
                                prohibitReplyDic[request.intentionId] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.intentionId] = orderRisks;
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
                                result.SecurityId = this.CurrentItem.SecurityId;
                                result.Side = this.CurrentItem.Side;
                                //result.Price = ;
                                //result.Qty = error.qty;
                                exceptionResult.Add(result);
                                string key = string.IsNullOrEmpty(_Key) ? Guid.NewGuid().ToString() : _Key;
                                exceptionReplyDic[key] = exceptionResult;
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

        PublishQuoteView _owner;

        public void InitData(PublishQuoteView owner)
        {
            _owner = owner;

            if(CurrentItem != null && !string.IsNullOrEmpty(CurrentItem.SecurityId))
            {
                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(CurrentItem.SecurityId);
                if (bondsInfoPo != null)
                {
                    // 含权
                    if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                    ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                    (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                    {

                        if (PriceTypeEmptyDic.Count == 2)
                        {
                            PriceTypeEmptyDic.Add(new KeyValuePair<string, string>("7", "行权"));
                        }
                        //this.PriceType = "7";
                    }
                    else
                    {
                        //this.PriceType = "3";
                        if (PriceTypeEmptyDic.Count == 3)
                        {
                            PriceTypeEmptyDic.RemoveAt(2);
                        }
                    }
                }
            }

            ThreadPoolHelper.InvokeThreadPool(o => {
                QueryFQUserConfig();
                Query();
                //IntentionOldHandler.Instance.OnHistoryMarketQuotes -= Instance_OnHistoryMarketQuotes;
                //IntentionOldHandler.Instance.OnHistoryMarketQuotes += Instance_OnHistoryMarketQuotes;
            });
        }

        private void Query()
        {
            //Application.Current.Dispatcher.Invoke((Action)(() => { 

            //    lock(_dataLock)
            //    {
            //        Datas.Clear();
            //        if (this.CurrentMarketItem == null || (this.CurrentMarketItem != null && this.CurrentMarketItem.ReceiverNumber == "内部"))
            //        {
            //            // 内部
            //            IntentionMarketItem newItem = new IntentionMarketItem();
            //            newItem.IsChecked = (this.CurrentMarketItem != null && this.CurrentMarketItem.ReceiverNumber == "内部") ? true : false;
            //            newItem.OptionType = "Inner";
            //            newItem.ReceiverNumber = newItem.ReceiverNumber_Edit = "内部";
            //            newItem.Status = "0";
            //            newItem.Side = this.CurrentItem.Side;
            //            newItem.SecurityId = this.CurrentItem.SecurityId;
            //            newItem.Price = newItem.Price_Edit = CurrentItem.Price;
            //            newItem.Qty = newItem.Qty_Edit = CurrentItem.Qty;
            //            newItem.SettlType = CurrentItem.SettlType;
            //            newItem.HurryFlag = newItem.HurryFlag_Edit = CurrentItem.HurryFlag;
            //            newItem.SuggestFlag = newItem.SuggestFlag_Edit = CurrentItem.SuggestFlag;
            //            newItem.FireFlag = newItem.FireFlag_Edit = CurrentItem.FireFlag;
            //            newItem.FloorPrice = newItem.FloorPrice_Edit = CurrentItem.FloorPrice;
            //            newItem.OCOFlag = newItem.OCOFlag_Edit = CurrentItem.OCOFlag;
            //            Datas.Add(newItem);
            //        }
            //    }
            //}));

            //string date = DateTimeEx.ServerTime.ToString("yyyy-MM-dd");
            //IntentionOldHandler.Instance.RequestMarketQuotes_IntentionId(this.CurrentItem.IntentionId, date, date);
            List<TableIntentionInquiry> resultList = IntentionOldHandler.Instance.GetInquiries(_Key, this.CurrentMarketItem?.Id);
            Instance_OnHistoryMarketQuotes(resultList);
        }



        /// <summary>
        /// 缓存userconfig
        /// </summary>
        Dictionary<string, SendIntentBrokers> _UserConfigs = new Dictionary<string, SendIntentBrokers>();

        /// <summary>
        /// to下拉框中内容
        /// </summary>
        List<ToOption> ToOptionList = new List<ToOption>();
        ConcurrentDictionary<string, List<ToOption>> broker_ToOption_Dic = new ConcurrentDictionary<string, List<ToOption>>();

        private void QueryFQUserConfig()
        {
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
                        broker_ToOption_Dic.Clear();
                        if (reply.data.sendIntentBrokers != null && reply.data.sendIntentBrokers.Count > 0)
                        {
                            int index = 0;// 如果有内部，从1开始
                            foreach (var item in reply.data.sendIntentBrokers)
                            {
                                if (item.Value == null || item.Value.Count == 0) 
                                    continue;

                                string brokerName = GetBrokerName(item.Key);
                                if (!broker_ToOption_Dic.ContainsKey(brokerName))
                                {
                                    broker_ToOption_Dic[brokerName] = new List<ToOption>();
                                }

                                foreach(SendIntentBrokers brokerInfo in item.Value)
                                {
                                    if (brokerInfo.securityType == this.SecurityType)
                                    {
                                        ToOption to = new ToOption();
                                        to.OptionType = "sendIntentBrokers";
                                        to.DispayName = $"{ComboSourceUtil.Instance.FindEnum(brokerName, "venuesxb")}-{brokerInfo.name}";
                                        to.Broker = brokerInfo;
                                        broker_ToOption_Dic[brokerName].Add(to);
                                    }
                                }

                                if (CurrentMarketItem == null)
                                {
                                    lock (_dataLock)
                                    {
                                        IntentionMarketItem brokerItem = Datas.FirstOrDefault(entry => entry.SourceTrader == brokerName);

                                        if (brokerItem == null)
                                        {
                                            brokerItem = new IntentionMarketItem();
                                            if (broker_ToOption_Dic.ContainsKey(brokerName) && broker_ToOption_Dic[brokerName].Count > 0)
                                            {
                                                brokerItem.Receiver_Edit = broker_ToOption_Dic[brokerName][0];
                                                brokerItem.ReceiverNumber_Edit = broker_ToOption_Dic[brokerName][0].DispayName;
                                            }
                                            brokerItem.SourceTrader = brokerName;
                                            brokerItem.IntentionId = CurrentItem.IntentionId;
                                            brokerItem.Side = this.CurrentItem.Side;
                                            if (CurrentItem.Type == "INTENTION_GROUP")
                                            {
                                                IEnumerable<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> list = IntentionOldHandler.Instance.GetSameGroupDataList(CurrentItem.QuoteType, CurrentItem.Trader_Buyer, CurrentItem.SecurityId, CurrentItem.Side);
                                                if (list != null)
                                                {
                                                    decimal? price = null;
                                                    decimal? qty = null;
                                                    if (CurrentItem.Side == "1")
                                                    {
                                                        price = list.Min(e => e.price);
                                                    }
                                                    else if (CurrentItem.Side == "2")
                                                    {
                                                        price = list.Max(e => e.price);
                                                    }
                                                    if (list.Any(x => x.price != price && x.status == CurrentItem.Status))
                                                    {
                                                        qty = list.First(x => x.price == price).qty;
                                                    }
                                                    else
                                                    {
                                                        qty = list.Sum(e => e.qty);
                                                    }
                                                    brokerItem.Price = brokerItem.Price_Edit = price;
                                                    brokerItem.Qty = brokerItem.Qty_Edit = qty;
                                                    brokerItem.PriceType = brokerItem.PriceType_Edit = string.IsNullOrEmpty(CurrentItem.PriceType) ? "3" : CurrentItem.PriceType;
                                                }
                                            }
                                            else
                                            {
                                                brokerItem.Price = brokerItem.Price_Edit = CurrentItem.Price;
                                                brokerItem.Qty = brokerItem.Qty_Edit = CurrentItem.Qty;
                                                brokerItem.PriceType = brokerItem.PriceType_Edit = string.IsNullOrEmpty(CurrentItem.PriceType) ? "3" : CurrentItem.PriceType;
                                            }
                                           
                                            //brokerItem.SettlType = CurrentItem.SettlType;
                                            brokerItem.OptionType = "sendIntentBrokers";
                                            Datas.Insert(index, brokerItem);
                                        }
                                        else
                                        {
                                            Datas.Remove(brokerItem);
                                            Datas.Insert(index, brokerItem);
                                        }
                                    }
                                    index++;
                                }
                                
                                //int index = 0;// 如果有内部，从1开始
                                //foreach(SendIntentBrokers broker in item.Value)
                                //{
                                //    if (broker.securityType == this.SecurityType)
                                //    {
                                //        ToOption to = new ToOption();
                                //        to.OptionType = "sendIntentBrokers";
                                //        to.DispayName = broker.name;
                                //        to.Broker = broker;
                                //        //ToOptionList.Add(to);

                                //        if (CurrentMarketItem == null)
                                //        {
                                //            lock(_dataLock)
                                //            {
                                //                IntentionMarketItem brokerItem = Datas.FirstOrDefault(entry =>
                                //                    entry.Receiver_Edit != null && entry.Receiver_Edit.Broker != null && to != null && to.Broker != null
                                //                    && entry.OptionType == "sendIntentBrokers" && entry.Receiver_Edit.Broker.qq == to.Broker.qq);

                                //                if (brokerItem == null)
                                //                {
                                //                    brokerItem = new IntentionMarketItem();
                                //                    brokerItem.IntentionId = CurrentItem.IntentionId;
                                //                    brokerItem.Receiver_Edit = to;
                                //                    brokerItem.ReceiverNumber = to.DispayName;
                                //                    brokerItem.ReceiverNumber_Edit = to.DispayName;
                                //                    brokerItem.Side = this.CurrentItem.Side;
                                //                    brokerItem.Price = brokerItem.Price_Edit = CurrentItem.Price;
                                //                    brokerItem.Qty = brokerItem.Qty_Edit = CurrentItem.Qty;
                                //                    brokerItem.SettlType = CurrentItem.SettlType;
                                //                    brokerItem.OptionType = "sendIntentBrokers";
                                //                    Datas.Insert(index, brokerItem);
                                //                }
                                //                else
                                //                {
                                //                    Datas.Remove(brokerItem);
                                //                    Datas.Insert(index, brokerItem);
                                //                }
                                //            }
                                //            index++;
                                //        }
                                //    }
                                //}
                            }
                        }

                        // 常用联系人
                        if (reply.data.sendIntentContacts != null && reply.data.sendIntentContacts.Count > 0)
                        {
                            foreach(UserConfig item in reply.data.sendIntentContacts)
                            {
                                ToOption to = new ToOption();
                                to.OptionType = "sendIntentContacts";
                                to.DispayName = $"常用联系人-{item.name}";
                                to.Sender = item;
                                ToOptionList.Add(to);
                            }
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
                                ToOptionList.Add(to);
                            }
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
                                ToOptionList.Add(to);
                            }
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
                                ToOptionList.Add(to);
                            }
                        }
                    }
                }
            }));
        }

        private string GetBrokerName(string key)
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

        public void FilterToQQ(AutoCompleteArgs args)
        {
            List<IntentionMarketItem> dataList = null;
            lock (_dataLock)
            {
                dataList = Datas.ToList();
            }
            List<ToOption> result = null;
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                result = ToOptionList.Where(item => item.DispayName.Contains(args.Pattern) && (dataList == null || dataList != null && !dataList.Any(a => a.ReceiverNumber_Edit == item.DispayName))).ToList();
            }
            else
            {
                result = ToOptionList.Where(item => dataList == null || dataList != null && !dataList.Any(a => a.ReceiverNumber_Edit == item.DispayName)).ToList();
            }

            args.DataSource = result;
        }

        public void FilterBroker(AutoCompleteArgs args, string sourceTrader)
        {

            List<ToOption> sourceList;
            if (broker_ToOption_Dic.ContainsKey(sourceTrader))
            {
                sourceList = broker_ToOption_Dic[sourceTrader];
            }
            else
            {
                sourceList = new List<ToOption>();
            }
            List<ToOption> result = null;
            if (!string.IsNullOrEmpty(args.Pattern))
            {
                result = sourceList.Where(item => item.DispayName.Contains(args.Pattern)).ToList();
            }
            else
            {
                result = sourceList;
            }

            args.DataSource = result;
        }
    }
}
