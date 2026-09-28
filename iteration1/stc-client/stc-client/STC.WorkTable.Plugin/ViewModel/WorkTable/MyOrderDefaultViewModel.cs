using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Handler.Base;
using STC.Common.Handler.OwnExec;
using STC.Common.Handler.TBook;
using STC.Common.Model.Base;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Frame.UI;
using STC.Frame.Utils;
using System.Data;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using STC.Common.Model;
using Ats.Foundation.Message;
using STC.Common.Model.Fix.Message;
using System.Windows.Controls;
using STC.Common.ViewModel;
using com.gateway.log;
using STC.WorkTable.Plugin.ViewModel.Data;
using STC.Common.View.Component;
using STC.Common.Model.Table;
using STC.WorkTable.Plugin.Utils.Data;
using STC.WorkTable.Plugin.Utils;
using STC.WorkTable.Plugin.View.WorkTable;
using STC.WorkTable.Plugin.View;
using WtUtils = STC.WorkTable.Plugin.Utils.CommonUtils;
using System.IO;
using STC.Frame.Utils.ExcelHelper;
using STC.Frame.CustomUI.Component;
using static STC.WorkTable.Plugin.View.Component.AutoCompleteComboBox;
using STC.Common.Data;
using STC.WorkTable.Plugin.Utils.Data.CRM.Model.CRM;
using STC.Common.DataProcesser;

namespace STC.WorkTable.Plugin.ViewModel.WorkTable
{
    public enum TBookStatusEnum
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Working,

        /// <summary>
        /// 已处理
        /// </summary>
        Worked,

        /// <summary>
        /// 已忽略
        /// </summary>
        Ignore,
    }

    public class MyOrderDefaultViewModel : ViewModelBase
    {
        private TBookStatusEnum _tbookStatusEnum;
        public TBookStatusEnum OrderStatusEnum
        {
            get
            {
                return this._tbookStatusEnum;
            }

            set
            {
                this._tbookStatusEnum = value;

                switch (value)
                {
                    case TBookStatusEnum.Working:
                        WorkingDataGridVisibility = Visibility.Visible;
                        WorkedDataGridVisibility = Visibility.Hidden;
                        IgnoreDataGridVisibility = Visibility.Hidden;
                        //this.workingViewSource.View.Filter = x => pStatus0((IntentionQuoteItem)x);
                        break;
                    case TBookStatusEnum.Worked:
                        WorkingDataGridVisibility = Visibility.Hidden;
                        WorkedDataGridVisibility = Visibility.Visible;
                        IgnoreDataGridVisibility = Visibility.Hidden;
                        //this.workingViewSource.View.Filter = x => pStatus1_2((IntentionQuoteItem)x);
                        break;
                    case TBookStatusEnum.Ignore:
                        WorkingDataGridVisibility = Visibility.Hidden;
                        WorkedDataGridVisibility = Visibility.Hidden;
                        IgnoreDataGridVisibility = Visibility.Visible;
                        //this.workingViewSource.View.Filter = x => pStatus3((IntentionQuoteItem)x);
                        break;
                }

                this.OnPropertyChanged();
            }
        }

        private Visibility _WorkingDataGridVisibility = Visibility.Visible;

        public Visibility WorkingDataGridVisibility
        {
            get
            {
                return _WorkingDataGridVisibility;
            }
            set
            {
                _WorkingDataGridVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _WorkedDataGridVisibility = Visibility.Hidden;

        public Visibility WorkedDataGridVisibility
        {
            get
            {
                return _WorkedDataGridVisibility;
            }
            set
            {
                _WorkedDataGridVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _IgnoreDataGridVisibility = Visibility.Hidden;

        public Visibility IgnoreDataGridVisibility
        {
            get
            {
                return _IgnoreDataGridVisibility;
            }
            set
            {
                _IgnoreDataGridVisibility = value;
                this.OnPropertyChanged();
            }
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

        private bool _QuickConfirmEnable = true;
        public bool QuickConfirmEnable
        {
            get
            {
                return _QuickConfirmEnable;
            }
            set
            {
                _QuickConfirmEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _TGEnable = true;
        public bool TGEnable
        {
            get
            {
                return _TGEnable;
            }
            set
            {
                _TGEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IgnoreEnable = true;
        public bool IgnoreEnable
        {
            get
            {
                return _IgnoreEnable;
            }
            set
            {
                _IgnoreEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool _RecoverEnable = true;
        public bool RecoverEnable
        {
            get
            {
                return _RecoverEnable;
            }
            set
            {
                _RecoverEnable = value;
                this.OnPropertyChanged();
            }
        }


        private IntentionQuoteItem _selectedItem_Working;
        public IntentionQuoteItem SelectedItem_Working
        {
            get
            {
                return this._selectedItem_Working;
            }

            set
            {
                this._selectedItem_Working = value;
                this.OnPropertyChanged();
            }
        }

        private IntentionQuoteItem _selectedItem_Worked;
        public IntentionQuoteItem SelectedItem_Worked
        {
            get
            {
                return this._selectedItem_Worked;
            }

            set
            {
                this._selectedItem_Worked = value;
                this.OnPropertyChanged();
            }
        }

        private IntentionQuoteItem _selectedItem_Ignore;
        public IntentionQuoteItem SelectedItem_Ignore
        {
            get
            {
                return this._selectedItem_Ignore;
            }

            set
            {
                this._selectedItem_Ignore = value;
                this.OnPropertyChanged();
            }
        }

        //// 机构号
        //private ObservableCollection<KeyValueData> _bankList = new ObservableCollection<KeyValueData>();
        //public ObservableCollection<KeyValueData> BankList
        //{
        //    get
        //    {
        //        if (this._bankList.Count == 0)
        //        {
        //            this._bankList.Add(new KeyValueData() { Key = "BOC", Value = "中国银行" });
        //            this._bankList.Add(new KeyValueData() { Key = "CCB", Value = "建设银行" });
        //            this._bankList.Add(new KeyValueData() { Key = "ICBC", Value = "工商银行" });
        //            this._bankList.Add(new KeyValueData() { Key = "ABOC", Value = "农业银行" });
        //            this._bankList.Add(new KeyValueData() { Key = "JTYH", Value = "交通银行" });
        //        }

        //        return this._bankList;
        //    }

        //    set
        //    {
        //        this._bankList = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private object _bankListLock = new object();
        //private CollectionViewSource _BankListViewSource = new CollectionViewSource();
        //public CollectionViewSource BankListViewSource
        //{
        //    get { return _BankListViewSource; }
        //    set { _BankListViewSource = value; }
        //}

        //// 交易员
        //private ObservableCollection<KeyValueData> _userList = new ObservableCollection<KeyValueData>();
        //public ObservableCollection<KeyValueData> UserList
        //{
        //    get
        //    {
        //        if (this._userList.Count == 0)
        //        {
        //            this._userList.Add(new KeyValueData() { Key = "001", Value = "李客户" });
        //            this._userList.Add(new KeyValueData() { Key = "002", Value = "刘客户" });
        //            this._userList.Add(new KeyValueData() { Key = "003", Value = "张客户" });
        //            this._userList.Add(new KeyValueData() { Key = "004", Value = "王客户" });
        //            this._userList.Add(new KeyValueData() { Key = "005", Value = "沈客户" });
        //        }
        //        return this._userList;
        //    }
        //    set
        //    {
        //        this._userList = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private object _userListLock = new object();
        //private CollectionViewSource _userListViewSource = new CollectionViewSource();
        //public CollectionViewSource UserListViewSource
        //{
        //    get { return _userListViewSource; }
        //    set { _userListViewSource = value; }
        //}

        ////经纪商
        //private ObservableCollection<KeyValueData> _brokerList = new ObservableCollection<KeyValueData>();
        //public ObservableCollection<KeyValueData> BrokerList
        //{
        //    get
        //    {
        //        if (this._brokerList.Count == 0)
        //        {
        //            this._brokerList.Add(new KeyValueData() { Key = "001", Value = "张岱君" });
        //            this._brokerList.Add(new KeyValueData() { Key = "002", Value = "王阳明" });
        //            this._brokerList.Add(new KeyValueData() { Key = "003", Value = "林炜凯" });
        //            this._brokerList.Add(new KeyValueData() { Key = "004", Value = "李国思" });
        //            this._brokerList.Add(new KeyValueData() { Key = "005", Value = "刘方舟" });
        //        }
        //        return this._brokerList;
        //    }

        //    set
        //    {
        //        this._brokerList = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private object _brokerListLock = new object();
        //private CollectionViewSource _brokerListViewSource = new CollectionViewSource();
        //public CollectionViewSource BrokerListViewSource
        //{
        //    get { return _brokerListViewSource; }
        //    set { _brokerListViewSource = value; }
        //}

        public ICommand ConfirmCommand { get; set; }

        public ICommand QuickConfirmCommand { get; set; }

        public ICommand TGCommand { get; set; }

        public ICommand IgnoreCommand { get; set; }

        public ICommand ReorderCommand { get; set; }

        public ICommand QuoteIdentifyCommand { get; set; }

        public ICommand AllConfirmCommand { get; set; }

        public ICommand AllQuickConfirmCommand { get; set; }

        public ICommand AllTGCommand { get; set; }

        public ICommand AllIgnoreCommand { get; set; }

        public ICommand AllRecoverCommand { get; set; }

        /// <summary>
        /// 编辑行
        /// </summary>
        public ICommand EditRowCommand { get; set; }

        /// <summary>
        /// 确认行
        /// </summary>
        public ICommand SaveRowCommand { get; set; }

        /// <summary>
        // 取消行
        /// </summary>
        public ICommand CancelRowCommand { get; set; }

        /// <summary>
        /// 导出
        /// </summary>
        public ICommand ExportCommand { get; set; }

        /// <summary>
        /// 导入
        /// </summary>
        public ICommand ImportCommand { get; set; }


        private bool _ExportEnabled = true;

        public bool ExportEnabled
        {
            get
            {
                return _ExportEnabled;
            }
            set
            {
                this._ExportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private bool _ImportEnabled = true;

        public bool ImportEnabled
        {
            get
            {
                return _ImportEnabled;
            }
            set
            {
                this._ImportEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private object _workingLock = new object();
        private CollectionViewSource workingViewSource = new CollectionViewSource();
        public CollectionViewSource WorkingViewSource
        {
            get { return workingViewSource; }
            set { workingViewSource = value; }
        }

        private object _workedLock = new object();
        private CollectionViewSource workedViewSource = new CollectionViewSource();
        public CollectionViewSource WorkedViewSource
        {
            get { return workedViewSource; }
            set { workedViewSource = value; }
        }

        private object _ignoreLock = new object();
        private CollectionViewSource ignoreViewSource = new CollectionViewSource();
        public CollectionViewSource IgnoreViewSource
        {
            get { return ignoreViewSource; }
            set { ignoreViewSource = value; }
        }

        private CustomCollection<IntentionQuoteItem> intentionQuoteInfo = new CustomCollection<IntentionQuoteItem>();

        public CustomCollection<IntentionQuoteItem> IntentionQuoteInfo
        {
            get { return intentionQuoteInfo; }
            set { intentionQuoteInfo = value; }
        }

        private Predicate<IntentionQuoteItem> pStatus0 { get; } = new Predicate<IntentionQuoteItem>(x => (x.Status == "0" && (x.UserName == null || x.UserName == LocalEnv.UserInfo.UserName)));
        private Predicate<IntentionQuoteItem> pStatus1_2 { get; } = new Predicate<IntentionQuoteItem>(x => (x.Status == "1" || x.Status == "2") && x.UserName == LocalEnv.UserInfo.UserName);
        private Predicate<IntentionQuoteItem> pStatus3{ get; } = new Predicate<IntentionQuoteItem>(x => x.Status == "3" && x.UserName == LocalEnv.UserInfo.UserName);

        private const string filterProperty = nameof(IntentionQuoteItem.Status);

        //AsyncThreadQueue<ConfirmIntentionQuoteReplyPo> replyUpdateThread;

        private Dictionary<string, string> _sideDic = new Dictionary<string, string>();
        public Dictionary<string, string> SideDic
        {
            get
            {
                if(_sideDic.Count == 0)
                {
                    _sideDic["1"] = "Bid";
                    _sideDic["2"] = "Ofr";
                }
                return _sideDic;
            }
        }

        private int _TotalCount4Working = 0;
        public int TotalCount4Working
        {
            get
            {
                return _TotalCount4Working;
            }
            set
            {
                _TotalCount4Working = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4BidWorking = 0;

        public int TotalCount4BidWorking
        {
            get
            {
                return _TotalCount4BidWorking;
            }
            set
            {
                _TotalCount4BidWorking = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4OfrWorking = 0;

        public int TotalCount4OfrWorking
        {
            get
            {
                return _TotalCount4OfrWorking;
            }
            set
            {
                _TotalCount4OfrWorking = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4Worked = 0;
        public int TotalCount4Worked
        {
            get
            {
                return _TotalCount4Worked;
            }
            set
            {
                _TotalCount4Worked = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4BidWorked = 0;

        public int TotalCount4BidWorked
        {
            get
            {
                return _TotalCount4BidWorked;
            }
            set
            {
                _TotalCount4BidWorked = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4OfrWorked = 0;

        public int TotalCount4OfrWorked
        {
            get
            {
                return _TotalCount4OfrWorked;
            }
            set
            {
                _TotalCount4OfrWorked = value;
                this.OnPropertyChanged();
            }
        }


        private int _TotalCount4Ignore = 0;
        public int TotalCount4Ignore
        {
            get
            {
                return _TotalCount4Ignore;
            }
            set
            {
                _TotalCount4Ignore = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4BidIgnore = 0;

        public int TotalCount4BidIgnore
        {
            get
            {
                return _TotalCount4BidIgnore;
            }
            set
            {
                _TotalCount4BidIgnore = value;
                this.OnPropertyChanged();
            }
        }

        private int _TotalCount4OfrIgnore = 0;

        public int TotalCount4OfrIgnore
        {
            get
            {
                return _TotalCount4OfrIgnore;
            }
            set
            {
                _TotalCount4OfrIgnore = value;
                this.OnPropertyChanged();
            }
        }

        public MyOrderDefaultViewModel()
        {
            try
            {
                //for (int i = 0; i < 4; i++)
                //{
                //    IntentionQuoteItem item = new IntentionQuoteItem();
                //    item.IntentionId = i.ConvertToString();
                //    item.InstitutionId = "BOC";
                //    item.InstitutionName = "中国银行";
                //    item.TraderId = "001";
                //    item.TraderName = "李客户";
                //    item.QqNumber = "123456";
                //    item.Side = "1";
                //    item.SecurityId = "180210.IB";
                //    item.Symbol = "18国开10";
                //    item.CleanPrice = 101.6m;
                //    item.FullPrice = 3.186m;
                //    item.Price = 3.186m;
                //    item.Qty = 2000;
                //    item.SettlType = "1";
                //    string tradeDate = CommonUtils.CalTradeDate("1");
                //    item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
                //    DateTime settlDateTime;
                //    int selectIndex;
                //    CommonUtils.SetSettleType("1", tradeDate, out settlDateTime, out selectIndex);
                //    item.edit_settletype = selectIndex;
                //    item.BrokerId = "001";
                //    item.BrokerName = "张岱君";
                //    item.Text = "bid 3.186 AAA 180210 18国开10 2000";
                //    item.Status = i + "";
                //    item.CreateTime = "2021-12-12 12:12:12";
                //    item.UpdateTime = "2021-12-12 12:12:12";
                //    item.CloseBy = "yxl";


                //    this.IntentionQuoteInfo.Add(item);
                //}
                BaseHandler.Instance.Init(Instance_OnClear);

                DesignHandler.Instance.OnHotKey += Instance_OnHotKey;

                //if (replyUpdateThread == null)
                //{
                //    replyUpdateThread = new AsyncThreadQueue<ConfirmIntentionQuoteReplyPo>(UpdateConfirmIntentionQuoteRequest);
                //}

                Thread refreshCheckBoxThread = new Thread(RefreshCheckBox);
                refreshCheckBoxThread.IsBackground = true;
                refreshCheckBoxThread.Start();


                IntentionHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;

                //this.BankListViewSource.IsLiveFilteringRequested = true;
                //this.BankListViewSource.IsLiveSortingRequested = true;
                //this.BankListViewSource.Source = BankList;
                //BindingOperations.EnableCollectionSynchronization(this.BankListViewSource.View, _bankListLock);

                //this.UserListViewSource.IsLiveFilteringRequested = true;
                //this.UserListViewSource.IsLiveSortingRequested = true;
                //this.UserListViewSource.Source = UserList;
                //BindingOperations.EnableCollectionSynchronization(this.UserListViewSource.View, _userListLock);

                //this.BrokerListViewSource.IsLiveFilteringRequested = true;
                //this.BrokerListViewSource.IsLiveSortingRequested = true;
                //this.BrokerListViewSource.Source = BrokerList;
                //BindingOperations.EnableCollectionSynchronization(this.BrokerListViewSource.View, _brokerListLock);


                this.WorkingViewSource.IsLiveFilteringRequested = true;
                this.WorkingViewSource.IsLiveSortingRequested = true;
                this.WorkingViewSource.LiveFilteringProperties.Add("Status");
                this.WorkingViewSource.Source = IntentionQuoteInfo;
                this.WorkingViewSource.View.CollectionChanged += WorkingView_CollectionChanged;
                this.WorkingViewSource.View.Filter = x => pStatus0((IntentionQuoteItem)x);
                BindingOperations.EnableCollectionSynchronization(this.WorkingViewSource.View, _workingLock);


                this.WorkedViewSource.IsLiveFilteringRequested = true;
                this.WorkedViewSource.IsLiveSortingRequested = true;
                this.WorkedViewSource.LiveFilteringProperties.Add("Status");
                this.WorkedViewSource.Source = IntentionQuoteInfo;
                this.WorkedViewSource.View.CollectionChanged += WorkedView_CollectionChanged;
                this.WorkedViewSource.View.Filter = x => pStatus1_2((IntentionQuoteItem)x);
                BindingOperations.EnableCollectionSynchronization(this.WorkedViewSource.View, _workedLock);


                this.IgnoreViewSource.IsLiveFilteringRequested = true;
                this.IgnoreViewSource.IsLiveSortingRequested = true;
                this.IgnoreViewSource.LiveFilteringProperties.Add("Status");
                this.IgnoreViewSource.Source = IntentionQuoteInfo;
                this.IgnoreViewSource.View.CollectionChanged += IgnoreView_CollectionChanged;
                this.IgnoreViewSource.View.Filter = x => pStatus3((IntentionQuoteItem)x);
                BindingOperations.EnableCollectionSynchronization(this.IgnoreViewSource.View, _ignoreLock);

                this.OrderStatusEnum = TBookStatusEnum.Working;


                this.ConfirmCommand = new ComRelayCommand(this.DoConfirmCommand);
                this.QuickConfirmCommand = new ComRelayCommand(this.DoQuickConfirmCommand);
                this.TGCommand = new ComRelayCommand(this.DoTGCommand);
                this.IgnoreCommand = new ComRelayCommand(this.DoIgnoreCommand);
                this.ReorderCommand = new ComRelayCommand(this.DoReorderCommand);

                this.QuoteIdentifyCommand = new ComRelayCommand(this.DoQuoteIdentifyCommand);
                this.AllConfirmCommand = new ComRelayCommand(this.DoAllConfirmCommand);
                this.AllQuickConfirmCommand = new ComRelayCommand(this.DoAllQuickConfirmCommand);
                this.AllTGCommand = new ComRelayCommand(this.DoAllTGCommand);
                this.AllIgnoreCommand = new ComRelayCommand(this.DoAllIgnoreCommand);
                this.AllRecoverCommand = new ComRelayCommand(this.DoAllRecoverCommand);

                EditRowCommand = new ComRelayCommand<Button>(EditRow);
                SaveRowCommand = new ComRelayCommand<Button>(SaveRow);
                CancelRowCommand = new ComRelayCommand<Button>(CancelRow);

                ExportCommand = new ComRelayCommand(ExportCSV);
                ImportCommand = new ComRelayCommand(ImportCSV);

                isAdjustAllChkStatus = true;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            } 
        }

        public void ImportCSV()
        {

            try
            {
                this.ImportEnabled = false;
                // 1.弹框
                //创建一个保存文件式的对话框
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();                              
                //设置这个对话框的起始保存路径
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.RestoreDirectory = true;
                //设置保存的文件的类型，注意过滤器的语法
                ofd.Filter = "(文本文件*.csv)|*.csv";
                ofd.DefaultExt = ".csv";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ofd.FileName.EndsWith(".csv", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持csv";
                        prompt.Show();
                        return;
                    }

                    // 3.导入操作
                    StringBuilder sb = new StringBuilder();

                    DataTable dt = CSVHelper.Instance.ReadCSV(ofd.FileName);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            StringBuilder lineSB = new StringBuilder();
                            foreach (DataColumn dc in dt.Columns)
                            {
                                string value = dr[dc.ColumnName].ConvertToString();
                                value = value.Trim('\"');
                                
                                if (!string.IsNullOrEmpty(value))
                                {
                                    if (lineSB.Length > 0)
                                    {
                                        lineSB.Append(" ");
                                    }
                                    lineSB.Append(value);
                                }
                            }
                            sb.AppendLine(lineSB.ToString());
                        }
                    }
                    else
                    {
                        promptTxt = "无数据或文件有误或文件被占用";
                    }
                    

                    if (sb.Length > 0)
                    {
                        IntentionHandler.Instance.RecognizeIntentionQuote(sb.ToString());
                    }
                    

                    //if (errorList != null && errorList.Count > 0)
                    //{
                    //    StringBuilder errors = new StringBuilder();
                    //    foreach (string error in errorList)
                    //    {
                    //        if (errors.Length > 0)
                    //        {
                    //            errors.AppendLine();
                    //        }
                    //        errors.Append(error);
                    //    }
                    //    promptTxt = errors.ToString();
                    //}
                    //else
                    //{
                    //    promptTxt = "导入成功";
                    //}
                }
                else
                {
                    promptTxt = "取消导入";
                }

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导入";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导入";
                prompt.PromptContext = e.Message;
                prompt.Show();
                
                LogHelper.Error(e);
            }
            finally
            {
                this.ImportEnabled = true;
            }
        }

        private async void ExportCSV()
        {
            int totalSize = 0;
            int successCount = 0;
            int failCount = 0;

            this.ExportEnabled = false;
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            try
            {
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = DateTimeEx.ServerTime.ToString("yyyyMMddHHmmss") + "_待处理";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    using (StreamWriter writer = CSVHelper.Instance.createStream(fileName))
                    {
                        // 输出表头
                        writer.WriteLine(GetHeaderLine(mainExportDic));

                        
                        await Task.Run(() =>
                        {
                            foreach (IntentionQuoteItem item in IntentionQuoteInfo)
                            {
                                if (item.Status == "0" && (item.UserName == null || item.UserName == LocalEnv.UserInfo.UserName))
                                {
                                    StringBuilder lineSB = new StringBuilder();
                                    try
                                    {
                                        foreach (KeyValuePair<string, string> pair in mainExportDic)
                                        {
                                            string field = pair.Key;

                                            object fieldValue = typeof(IntentionQuoteItem).GetProperty(field).GetValue(item);

                                            string value = Common.Utils.CommonUtils.ConvertToString(fieldValue);

                                            if (field.Equals("Side", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "side");
                                            }
                                            else if(field.Equals("SettlType", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = ComboSourceUtil.Instance.FindEnum(value, "settletype");
                                            }
                                            else if (field.Equals("Price", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = value.ConvertToDecimal().ToString("0.00#");
                                            }
                                            else if (field.Equals("Qty", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = value.ConvertToDecimal().ToString("0");
                                            }
                                            if (lineSB.Length > 0)
                                            {
                                                lineSB.Append(",");
                                            }

                                            lineSB.Append($"\"{value}\"");
                                        }
                                        writer.WriteLine(lineSB);
                                        successCount++;
                                    }
                                    catch (Exception e)
                                    {
                                        LogHelper.Error(e);
                                        failCount++;
                                    }
                                    totalSize++;
                                }
                               
                            }
                        });
                        // 释放资源
                        writer.Flush();
                        writer.Close();
                    }
                    Common.View.Component.CustomMessageBox.Info($"《{saveFileDialog.FileName}》已保存。总条数：{totalSize}   成功：{successCount}  失败：{failCount}");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                this.ExportEnabled = true;
            }


        }


        private string GetHeaderLine(List<KeyValuePair<string, string>> list)
        {
            StringBuilder headerSb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in list)
            {
                if (headerSb.Length > 0)
                {
                    headerSb.Append(",");
                }
                headerSb.Append(pair.Value);
            }

            return headerSb.ToString();
        }

        List<KeyValuePair<string, string>> mainExportDic = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("InstitutionName", "客户机构"),
            new KeyValuePair<string, string>("TraderName","客户名"),
            new KeyValuePair<string, string>("Side","方向" ),
            new KeyValuePair<string, string>("SecurityId","代码" ),
            new KeyValuePair<string, string>("Symbol","简称" ),
            new KeyValuePair<string, string>("Price","报价" ),
            new KeyValuePair<string, string>("Qty","报量" ),
            new KeyValuePair<string, string>("SettlType","清算速度" ),
        };

        private void WorkingView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
           if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Working_Calculate();
            }
        }

        private void WorkedView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                 || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                 || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Worked_Calculate();
            }
        }

        private void IgnoreView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                 || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                 || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Ignore_Calculate();
            }
        }

        /// <summary>
        /// 计算待处理数量
        /// </summary>
        private void Working_Calculate()
        {
            int bidCount = 0;
            int ofrCount = 0;
            int totalCount = 0;

            lock (_workingLock)
            {
                foreach(IntentionQuoteItem item in this.WorkingViewSource.View)
                {
                    totalCount++;
                    if (item.Side == "1")
                    {
                        bidCount++;
                    }
                    else if (item.Side == "2")
                    {
                        ofrCount++;
                    }
                }
            }

            this.TotalCount4Working = totalCount;
            this.TotalCount4BidWorking = bidCount;
            this.TotalCount4OfrWorking = ofrCount;
        }

        /// <summary>
        /// 计算待处理数量
        /// </summary>
        private void Worked_Calculate()
        {
            int bidCount = 0;
            int ofrCount = 0;
            int totalCount = 0;

            lock (_workedLock)
            {
                foreach (IntentionQuoteItem item in this.WorkedViewSource.View)
                {
                    totalCount++;
                    if (item.Side == "1")
                    {
                        bidCount++;
                    }
                    else if (item.Side == "2")
                    {
                        ofrCount++;
                    }
                }
            }

            this.TotalCount4Worked = totalCount;
            this.TotalCount4BidWorked = bidCount;
            this.TotalCount4OfrWorked = ofrCount;
        }

        /// <summary>
        /// 计算待处理数量
        /// </summary>
        private void Ignore_Calculate()
        {
            int bidCount = 0;
            int ofrCount = 0;
            int totalCount = 0;

            lock (_ignoreLock)
            {
                foreach (IntentionQuoteItem item in this.IgnoreViewSource.View)
                {
                    totalCount++;
                    if (item.Side == "1")
                    {
                        bidCount++;
                    }
                    else if (item.Side == "2")
                    {
                        ofrCount++;
                    }
                }
            }

            this.TotalCount4Ignore = totalCount;
            this.TotalCount4BidIgnore = bidCount;
            this.TotalCount4OfrIgnore = ofrCount;
        }
        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => {
  
                        if (this.IntentionQuoteInfo != null)
                        {
                            this.IntentionQuoteInfo.Clear();
                        }
                        this.TotalCount4Working = 0;
                        this.TotalCount4BidWorking = 0;
                        this.TotalCount4OfrWorking = 0;
                        this.TotalCount4Worked = 0;
                        this.TotalCount4BidWorked = 0;
                        this.TotalCount4OfrWorked = 0;
                        this.TotalCount4Ignore = 0;
                        this.TotalCount4BidIgnore = 0;
                        this.TotalCount4OfrIgnore = 0;
                        IntentionHandler.Instance.OnIntentionQuotes -= Instance_OnIntentionQuotes;
                        IntentionHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;
                    }));
                }
            }
        }

        private void Instance_OnHotKey(EHotKeySetting obj)
        {
            if (this.OrderStatusEnum == TBookStatusEnum.Working)
            {
                if (obj == EHotKeySetting.需求工作台普通确认)
                {
                    DoAllConfirmCommand();
                }
                else if  (obj == EHotKeySetting.需求工作台闪电确认)
                {
                    DoAllQuickConfirmCommand();
                }
                else if (obj == EHotKeySetting.需求工作台TG)
                {
                    DoAllTGCommand();
                }
                else if (obj == EHotKeySetting.需求工作台忽略)
                {
                    DoAllIgnoreCommand();
                }
            }
        }


        private void UpdateConfirmIntentionQuoteRequest(ConfirmIntentionQuoteReplyPo reply)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
               var findList = IntentionQuoteInfo.Where(x => x.IntentionId == reply.intentionId);
                if (findList != null && findList.Count() > 0)
                {
                    IntentionQuoteItem item = findList.First();
                    item.Status = reply.status;

                    isAdjustAllChkStatus = true;

                    if (reply.status == "3")
                    {
                        isAdjustAllChkStatus_Ignore = true;
                    }
                    item.OrderID = reply.orderId;
                }
            }));
        }

        private void Instance_OnIntentionQuotes(IntentionQuoteInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                IntentionQuoteItem item = null;
                var findList = IntentionQuoteInfo.Where(x => x.IntentionId == obj.intentionId);
                if (findList != null && findList.Count() > 0)
                {
                    item = findList.First();
                    if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName
                        && obj.status != "0")
                    {
                        this.IntentionQuoteInfo.Remove(item);
                        return;
                    }

                    isAdjustAllChkStatus = true;

 					if ((obj.status == "3" && item.Status == "0")
                         ||(obj.status == "0" && item.Status == "3"))
                    {
                        isAdjustAllChkStatus_Ignore = true;
                    }

                    item.Status = obj.status;

                    if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    {
                        if (string.IsNullOrEmpty(obj.orderId) == false)
                        {

                            // 更新我的订单/我的撤单
                            MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                               );
                            // 更新我的成交
                            MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                                );
                        }
                    }
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.Side = obj.side;
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    item.SettlType = obj.settlType;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    item.CloseBy = obj.closeBy;
                    item.OrderID = obj.orderId;
                    item.QqNumber = obj.qqNumber;

                }
                else
                {
                    item = new IntentionQuoteItem();
                    if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    {
                        if (string.IsNullOrEmpty(obj.orderId) == false)
                        {

                            // 更新我的订单/我的撤单
                            MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                               );
                            // 更新我的成交
                            MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                                );
                        }
                    }
                    item.IntentionId = obj.intentionId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.QqNumber = obj.qqNumber;
                    item.Side = obj.side;
                    item.SecurityId = obj.securityId;
                    item.Symbol = obj.symbol;
                    item.NetPrice = obj.netPrice;
                    item.FullPrice = obj.fullPrice;
                    item.Price = obj.price;
                    item.Qty = obj.qty;

                    item.SettlType = obj.settlType;
                    //string tradeDate = CommonUtils.CalTradeDate(obj.settlType);
                    //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
                    //DateTime settlDateTime;
                    //int selectIndex;
                    //CommonUtils.SetSettleType(obj.settlType, tradeDate, out settlDateTime, out selectIndex);
                    //item.edit_settletype = selectIndex;

                    //item.BrokerId = obj.brokerId;
                    //item.BrokerName = obj.brokerName;
                    item.Text = obj.text;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    item.CloseBy = obj.closeBy;
                    item.Status = obj.status;


                    isAdjustAllChkStatus = true;
                    
                    if (obj.status == "3")
                    {
                        isAdjustAllChkStatus_Ignore = true;
                    }
                    item.OrderID = obj.orderId;

                   
                    this.IntentionQuoteInfo.Add(item);
                }
            }));
        }

        /// <summary>
        /// 普通确认
        /// </summary>
        private void DoConfirmCommand()
        {
            if (this.SelectedItem_Working != null)
            {
                if (!Common.View.Component.CustomMessageBox.Question("是否执行普通确认操作？", "普通确认"))
                {
                    return;
                }

                CheckandTips("1");
            }
        }

        private void DoConfirmOne(IntentionQuoteItem item)
        {
            item.IsConfirmed = true;
            string intentionId = item.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.crossVenues = "2";
            request.actionType = "1";// 普通确认
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
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
                finally
                {
                    item.IsConfirmed = false;
                }
            });
        }

        private void DoQuickConfirmCommand()
        {
            if (this.SelectedItem_Working != null)
            {
                if (!Common.View.Component.CustomMessageBox.Question("是否执行闪电确认操作？", "闪电确认"))
                {
                    return;
                }

                CheckandTips("2");
            }
        }

        private void DoQuickConfirmOne(IntentionQuoteItem item)
        {
            item.IsConfirmed = true;
            string intentionId = item.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.crossVenues = "2";
            request.actionType = "2";// 闪电确认
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
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
                            //CustomMessageBox.Info($"共闪电确认{totalCount}条, 其中失败{failCount}条", "闪电确认结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共闪电确认{totalCount}条, 全部成功", "闪电确认结果");
                        //}
                    }));

                }
                catch (Exception e)
                {
                    LogHelper.Error("DoQuickConfirmCommand", e);
                }
                finally
                {
                    item.IsConfirmed = false;
                }
            });
        }

        private void DoTGCommand()
        {
            if (this.SelectedItem_Working != null)
            {
                if (!Common.View.Component.CustomMessageBox.Question("是否执行T/G操作？", "T/G"))
                {
                    return;
                }
                CheckandTips("3");
            }
        }

        private void DoTGOne(IntentionQuoteItem item)
        {
            item.IsConfirmed = true;
            string intentionId = item.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.crossVenues = "2";
            request.actionType = "3";// T/G确认
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
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
                            //CustomMessageBox.Info($"共T/G确认{totalCount}条, 其中失败{failCount}条", "T/G确认结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共T/G确认{totalCount}条, 全部成功", "T/G确认结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoQuickConfirmCommand", e);
                }
                finally
                {
                    item.IsConfirmed = false;
                }
            });
        }

        private void DoIgnoreCommand()
        {
            if (this.SelectedItem_Working != null)
            {
                if (!Common.View.Component.CustomMessageBox.Question("是否执行忽略操作？", "忽略"))
                {
                    return;
                }

                DoIgnoreOne(this.SelectedItem_Working);
            }
        }

        private void DoIgnoreOne(IntentionQuoteItem item)
        {
            string intentionId = item.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.actionType = "4";// 忽略
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
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
                            //CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoQuickConfirmCommand", e);
                }
            });
        }

        private void DoReorderCommand()
        {
            if (this.SelectedItem_Ignore != null)
            {
                if (!Common.View.Component.CustomMessageBox.Question("是否执行恢复操作？", "恢复"))
                {
                    return;
                }

                DoReorderOne(this.SelectedItem_Ignore);
            }
        }

        private void DoReorderOne(IntentionQuoteItem item)
        {
            string intentionId = item.IntentionId;
            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
            request.intentionId = intentionId;
            request.actionType = "5";// 恢复
            request.userName = LocalEnv.UserInfo.UserName;
            request.clOrdId = DataTimeSequence.Instance.GetId();
            item.Request = request;

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    orderDic[item.IntentionId] = item;

                    GetErrorInfo(item.Request,
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
                            //CustomMessageBox.Info($"共恢复{totalCount}条, 其中失败{failCount}条", "恢复结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共恢复{totalCount}条, 全部成功", "恢复结果");
                        //}
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("DoReorderCommand", e);
                }
            });
        }

        private void DoQuoteIdentifyCommand()
        {
            if (!WindowManager.Instance.IsExist(WindowKeyHelper.RECOGENIZEQUOTE))
            {
                RecognizeIntentionQuoteWin win = new RecognizeIntentionQuoteWin();
                WindowManager.Instance.OpenOnlyOneWindow(WindowKeyHelper.RECOGENIZEQUOTE, win);
            }
        }

        private ConfirmIntentionQuoteReplyPo CheckParam(IntentionQuoteItem item)
        {
            ConfirmIntentionQuoteReplyPo reply = new ConfirmIntentionQuoteReplyPo();
            reply.intentionId = item.IntentionId;
            reply.orderId = item.OrderID;
            reply.securityId = item.SecurityId;
            reply.status = item.Status;
            reply.Code = "0000";
            decimal d = 0;
            string errorMsg = string.Empty;
            if (item.Price == null || item.Price == 0)
                errorMsg = "未填写价格或价格为0";
            else if (item.Qty == null || item.Qty == 0)
                errorMsg = "未填写量或量为0";
            else if (item.Qty % 100 > 0)
                errorMsg = string.Format("量必须是100的整数倍，当前输入值：{0}", item.Qty.Value.ToString("0"));
            else if (string.IsNullOrEmpty(item.SettlType))
                errorMsg = "清算速度未填写";
            else if (string.IsNullOrEmpty(item.Side))
                errorMsg = "方向未填写";
            else if (decimal.TryParse(item.Qty.ToString(), out d) && (d % 100) > 0)
                errorMsg = string.Format("量必须是100的整数倍，当前输入值：{0}", item.Qty);

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

        private void GetLocalCheckErrorInfo(IntentionQuoteItem item,
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

        private void GetErrorInfo(ConfirmIntentionQuoteRequest request,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                ConfirmIntentionQuoteReplyPo replyPo = IntentionHandler.Instance.ConfirmIntentionQuote(request);
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

        private void SetDoAllBtnEnable(bool isEnable)
        {
            this.ConfirmEnable = isEnable;
            this.QuickConfirmEnable = isEnable;
            this.TGEnable = isEnable;
        }
        internal void DoAllConfirmCommand()
        {
            if (this._hasSelected == false)
            {
                Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量普通确认");
                return;
            }

            if (!Common.View.Component.CustomMessageBox.Question("是否执行批量普通确认操作？","批量普通确认"))
            {
                return;
            }    

            SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                foreach (IntentionQuoteItem item in this.WorkingViewSource.View)
                {
                    if (item.IsChecked == false) continue;
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.IntentionId] = item;
                }

                //if (exceptionReplyDic_local.Count > 1)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.IntentionId))
                        {
                            continue;
                        }
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
                            request.intentionId = item.IntentionId;
                            request.crossVenues = "2";
                            request.actionType = "1";// 普通确认
                            request.userName = LocalEnv.UserInfo.UserName;
                            request.clOrdId = DataTimeSequence.Instance.GetId();
                            orderDic[item.IntentionId].Request = request;

                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                    }

                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
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
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllConfirmCommand1", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllQuickConfirmCommand()
        {
            if (this._hasSelected == false)
            {
                Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量闪电确认");
                return;
            }

            if (!Common.View.Component.CustomMessageBox.Question("是否执行批量闪电确认操作？", "批量闪电确认"))
            {
                return;
            }

            SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                foreach (IntentionQuoteItem item in this.WorkingViewSource.View)
                {
                    if (item.IsChecked == false) continue;
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.IntentionId] = item;
                }
                //if (exceptionReplyDic_local.Count > 0)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.IntentionId))
                        {
                            continue;
                        }
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
                            request.intentionId = item.IntentionId;
                            request.crossVenues = "2";
                            request.actionType = "2";// 闪电下单
                            request.userName = LocalEnv.UserInfo.UserName;
                            request.clOrdId = DataTimeSequence.Instance.GetId();
                            orderDic[item.IntentionId].Request = request;

                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                    }

                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共闪电确认{totalCount}条, 其中失败{failCount}条", "闪电确认结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共闪电确认{totalCount}条, 全部成功", "闪电确认结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllQuickConfirmCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllTGCommand()
        {
            if (this._hasSelected == false)
            {
                Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量T/G");
                return;
            }

            if (!Common.View.Component.CustomMessageBox.Question("是否执行批量T/G操作？", "批量T/G"))
            {
                return;
            }

            SetDoAllBtnEnable(false);
            try
            {
                int totalCount_local = 0;
                int failCount_local = 0;
                Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                foreach (IntentionQuoteItem item in this.WorkingViewSource.View)
                {
                    if (item.IsChecked == false) continue;
                    GetLocalCheckErrorInfo(item, ref failCount_local, ref totalCount_local, exceptionReplyDic);
                    orderDic[item.IntentionId] = item;
                }

                //if (exceptionReplyDic_local.Count > 0)
                //{
                //    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                //                    warnReplyDic_local, prohibitReplyDic_local, failCount_local, exceptionReplyDic_local);
                //    batchResultWindow.Owner = Application.Current.MainWindow;
                //    batchResultWindow.ShowDialog();
                //    return;
                //}

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (exceptionReplyDic.ContainsKey(item.IntentionId))
                        {
                            continue;
                        }
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
                            request.intentionId = item.IntentionId;
                            request.crossVenues = "2";
                            request.actionType = "3";// TG确认
                            request.userName = LocalEnv.UserInfo.UserName;
                            request.clOrdId = DataTimeSequence.Instance.GetId();
                            item.Request = request;

                            GetErrorInfo(request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                    }


                    while ((totalCount + failCount_local) != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0 || failCount_local > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount + failCount_local, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                            //CustomMessageBox.Info($"共T/G确认{totalCount}条, 其中失败{failCount}条", "T/G确认结果");
                        }
                        //else
                        //{
                        //    CustomMessageBox.Info($"共T/G确认{totalCount}条, 全部成功", "T/G确认结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllTGCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        private void DoAllIgnoreCommand()
        {
            if (this._hasSelected == false)
            {
                Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量忽略");
                return;
            }

            if (!Common.View.Component.CustomMessageBox.Question("是否执行批量忽略操作？", "批量忽略"))
            {
                return;
            }


            this.IgnoreEnable = false;
            try
            {
                Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                foreach (IntentionQuoteItem item in this.WorkingViewSource.View)
                {
                    if (item.IsChecked == false) continue;
                    ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
                    request.intentionId = item.IntentionId;
                    request.actionType = "4";// 忽略
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.clOrdId = DataTimeSequence.Instance.GetId();
                    item.Request = request;
                    orderDic[item.IntentionId] = item;
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                    }

                    while (totalCount != orderDic.Count)
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
                            //Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 其中失败{failCount}条", "忽略结果");
                        }
                        //else
                        //{
                        //   Common.View.Component.CustomMessageBox.Info($"共忽略{totalCount}条, 全部成功", "忽略结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllIgnoreCommand", e);
            }
            finally
            {
                this.IgnoreEnable = true;
            }
        }

        private void DoAllRecoverCommand()
        {
            if (this._hasSelected_Ignore == false)
            {
                Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量恢复");
                return;
            }

            if (!Common.View.Component.CustomMessageBox.Question("是否执行批量恢复操作？", "批量恢复"))
            {
                return;
            }

            this.RecoverEnable = false;
            try
            {
                Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
                foreach (IntentionQuoteItem item in this.IgnoreViewSource.View)
                {
                    if (item.IsChecked == false) continue;
                    ConfirmIntentionQuoteRequest request = new ConfirmIntentionQuoteRequest();
                    request.intentionId = item.IntentionId;
                    request.actionType = "5";// 恢复
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.clOrdId = DataTimeSequence.Instance.GetId();
                    item.Request = request;
                    orderDic[item.IntentionId] = item;
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                        {
                            GetErrorInfo(item.Request,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                        });
                    }

                    while (totalCount != orderDic.Count)
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
                            //Common.View.Component.CustomMessageBox.Info($"共恢复{totalCount}条, 其中失败{failCount}条", "恢复结果");
                        }
                        //else
                        //{
                        //    Common.View.Component.CustomMessageBox.Info($"共恢复{totalCount}条, 全部成功", "恢复结果");
                        //}
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllRecoverCommand", e);
            }
            finally
            {
                this.RecoverEnable = true;
            }
            
        }

        private void RefreshCheckBox()
        {
            try
            {
                while (true)
                {
                    if (isAdjustAllChkStatus_Ignore)
                    {
                        isAdjustAllChkStatus_Ignore = false;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.AdjustAllChkStatus_Ignore();
                        }));
                    }

                    if (isAdjustAllChkStatus)
                    {
                        isAdjustAllChkStatus = false;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            this.AdjustAllChkStatus();
                        }));
                    }
                    Thread.Sleep(500);
                }
            }
            catch (ThreadAbortException)
            {
                // ignore.
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        #region 待处理
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

            lock (_workingLock)
            {
                foreach (IntentionQuoteItem item in this.workingViewSource.View)
                {

                    item.IsChecked = isAllChecked;
                }
            }

            
            if (this.workingViewSource.View.IsEmpty == true)
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

            lock (_workingLock)
            {
                this._hasSelected = false;
                foreach (IntentionQuoteItem item in this.workingViewSource.View)
                {
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
                IsAllChecked = isCheckAll && this.workingViewSource.View.IsEmpty == false;
            }

        }

        public void CheckBoxSingleClick()
        {
            isAdjustAllChkStatus = true;
        }
        #endregion

        #region 已忽略
        private bool _hasSelected_Ignore = false;
        // 是否选中全选多选框
        private bool _IsAllChecked_Ignore = false;
        public bool IsAllChecked_Ignore
        {
            get
            {
                return _IsAllChecked_Ignore;
            }

            set
            {
                if (_IsAllChecked_Ignore != value)
                {
                    _IsAllChecked_Ignore = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private volatile bool isAdjustAllChkStatus_Ignore = false;
        public void CheckBoxAllValueChange_Ignore(bool isAllChecked)
        {

            lock (_ignoreLock)
            {
                foreach (IntentionQuoteItem item in this.IgnoreViewSource.View)
                {

                    item.IsChecked = isAllChecked;
                }
            }

            if (this.IgnoreViewSource.View.IsEmpty == true)
            {
                IsAllChecked_Ignore = false;
                _hasSelected_Ignore = false;
            }
            else
            {
                IsAllChecked_Ignore = isAllChecked;
                _hasSelected_Ignore = isAllChecked;
            }
        }

        public void AdjustAllChkStatus_Ignore()
        {
            bool isCheckAll = true;

            lock (_workingLock)
            {
                foreach (IntentionQuoteItem item in this.IgnoreViewSource.View)
                {
                    if (item.IsChecked == true)
                    {
                        _hasSelected_Ignore = true;
                    }
                    if (item.IsChecked == false)
                    {
                        isCheckAll = false;
                    }
                    if (_hasSelected_Ignore == true && isCheckAll == false)
                    {
                        break;
                    }
                }
                IsAllChecked_Ignore = isCheckAll && this.IgnoreViewSource.View.IsEmpty == false;
            }

        }

        public void CheckBoxSingleClick_Ignore()
        {
            isAdjustAllChkStatus_Ignore = true;
        }
        #endregion

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
            string currentTraderID = string.Empty;
            if (this.OrderStatusEnum == TBookStatusEnum.Working)
            {
                if (this.SelectedItem_Working != null)
                {
                    currentTraderID = this.SelectedItem_Working.TraderId;
                }
            }
            else if (this.OrderStatusEnum == TBookStatusEnum.Worked)
            {
                if (this.SelectedItem_Worked != null)
                {
                    currentTraderID = this.SelectedItem_Worked.TraderId;
                }
            }

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

            //UserListViewSource.View.Filter = x =>
            //{
            //    if (string.IsNullOrEmpty(args.Pattern))
            //        return true;
            //    else
            //        return ((KeyValueData)x).Key.Contains(args.Pattern) || ((KeyValueData)x).Value.Contains(args.Pattern);
            //};
            //args.DataSource = UserListViewSource.View;
            string currentInstID = string.Empty;
            if (this.OrderStatusEnum == TBookStatusEnum.Working)
            {
                if (this.SelectedItem_Working != null)
                {
                    currentInstID = this.SelectedItem_Working.InstitutionId;
                }
            }
            else if (this.OrderStatusEnum == TBookStatusEnum.Worked)
            {
                if (this.SelectedItem_Worked != null)
                {
                    currentInstID = this.SelectedItem_Worked.InstitutionId;
                }
            }
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


        internal void FilterBroker(AutoCompleteArgs args)
        {
            //BrokerListViewSource.View.Filter = x =>
            //{
            //    if (string.IsNullOrEmpty(args.Pattern))
            //        return true;
            //    else
            //        return ((KeyValueData)x).Key.Contains(args.Pattern) || ((KeyValueData)x).Value.Contains(args.Pattern);
            //};
            //args.DataSource = BrokerListViewSource.View;
        }

        private string _selectFilterType;

        public string SelectFilterType
        {
            get
            {
                return _selectFilterType;
            }
            set
            {
                _selectFilterType = value;
            }
        }


        private string _FilterText;

        public string FilterText
        {
            get { return _FilterText; }
            set 
            { 
                _FilterText = value;
            }
        }

        internal void FilterData()
        {
            if (string.IsNullOrEmpty(this.FilterText))
            {
                this.workingViewSource.View.Filter = x => pStatus0((IntentionQuoteItem)x);
                this.WorkedViewSource.View.Filter = x => pStatus1_2((IntentionQuoteItem)x);
                this.IgnoreViewSource.View.Filter = x => pStatus3((IntentionQuoteItem)x);
            }
            else
            {
                this.workingViewSource.View.Filter = x => {
                    IntentionQuoteItem item = (IntentionQuoteItem)x;
                    bool flag = true;
                    if (item == null)
                        return flag;
                    if (this.SelectFilterType == "SecurityID")
                    {
                        flag = !string.IsNullOrEmpty(item.SecurityId) && item.SecurityId.IndexOf(this.FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    else if (this.SelectFilterType == "InstitutionName")
                    {
                        flag = !string.IsNullOrEmpty(item.InstitutionName) && item.InstitutionName.Contains(this.FilterText);
                    }
                    else if (this.SelectFilterType == "TradeName")
                    {
                        flag = !string.IsNullOrEmpty(item.TraderName) && item.TraderName.Contains(this.FilterText);
                    }
                    return item.Status == "0" && (item.UserName == null || item.UserName == LocalEnv.UserInfo.UserName) && flag;
                };
                this.WorkedViewSource.View.Filter = x => {
                    IntentionQuoteItem item = (IntentionQuoteItem)x;
                    bool flag = true;
                    if (item == null)
                        return flag;
                    if (this.SelectFilterType == "SecurityID")
                    {
                        flag = !string.IsNullOrEmpty(item.SecurityId) && item.SecurityId.IndexOf(this.FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    else if (this.SelectFilterType == "InstitutionName")
                    {
                        flag = !string.IsNullOrEmpty(item.InstitutionName) && item.InstitutionName.Contains(this.FilterText);
                    }
                    else if (this.SelectFilterType == "TradeName")
                    {
                        flag = !string.IsNullOrEmpty(item.TraderName) && item.TraderName.Contains(this.FilterText);
                    }
                    return ((item.Status == "1" || item.Status == "2") && item.UserName == LocalEnv.UserInfo.UserName) && flag;
                };
                this.IgnoreViewSource.View.Filter = x => {
                    IntentionQuoteItem item = (IntentionQuoteItem)x;
                    bool flag = true;
                    if (item == null)
                        return flag;
                    if (this.SelectFilterType == "SecurityID")
                    {
                        flag = !string.IsNullOrEmpty(item.SecurityId) && item.SecurityId.IndexOf(this.FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    else if (this.SelectFilterType == "InstitutionName")
                    {
                        flag = !string.IsNullOrEmpty(item.InstitutionName) && item.InstitutionName.Contains(this.FilterText);
                    }
                    else if (this.SelectFilterType == "TradeName")
                    {
                        flag = !string.IsNullOrEmpty(item.TraderName) && item.TraderName.Contains(this.FilterText);
                    }
                    return item.Status == "3" && item.UserName == LocalEnv.UserInfo.UserName && flag;
                };
            }
        }


        /// <summary>
        /// 编辑记录
        /// </summary>
        /// <param name="editBtn"></param>
        private void EditRow(Button editBtn)
        {
            if (editBtn != null && editBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = editBtn.DataContext as IntentionQuoteItem;
                EditRow(item);
            }
        }

        internal void EditRow(IntentionQuoteItem item)
        {
            item.IsEdit = "0";
            item.InstitutionName_Edit = item.InstitutionName;
            //item.InstitutionName_Text = item.InstitutionName;
            item.TraderName_Edit = item.TraderName;
            //item.TraderName_Text = item.TraderName;
            item.Side_Edit = item.Side;
            item.Price_Edit = item.Price;
            item.Qty_Edit = item.Qty;
            item.SettlType_Edit = item.SettlType;
            //string tradeDate = CommonUtils.CalTradeDate(item.SettlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(item.SettlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;
        }

        private static string COLUMNNAME_INSTITUTION = "客户机构";
        private static string COLUMNNAME_TRADER = "客户名";
        private static string COLUMNNAME_SIDE = "方向";
        private static string COLUMNNAME_PRICE = "报价";
        private static string COLUMNNAME_QTY = "报量";
        private static string COLUMNNAME_SETTLETYPE = "清算速度";

        /// <summary>
        /// 保存记录修改
        /// </summary>
        /// <param name="saveBtn"></param>
        private void SaveRow(Button saveBtn)
        {
            if (saveBtn != null && saveBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = saveBtn.DataContext as IntentionQuoteItem;
                SaveRow(item);
            }
        }

        internal async void SaveRow(IntentionQuoteItem item, string columnHeader = null)
        {
            item.IsEdit = "2";
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

            // 客户机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            {
                if (item.SelectedInstintuion_Edit != null)
                {
                    institutionName = item.SelectedInstintuion_Edit.shortnamecn;
                    institutionId = item.SelectedInstintuion_Edit.idbkey;
                }
                else
                {
                    institutionId = item.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                    institutionName = item.InstitutionName_Edit == item.InstitutionName ? item.InstitutionName : string.Empty;
                }
                if (institutionId == item.InstitutionId && institutionName == item.InstitutionName)
                {
                    return;
                }

                //// 联动检查客户名
                //if (!string.IsNullOrEmpty(institutionId) 
                //        && !string.IsNullOrEmpty(item.TraderId))
                //{
                //    List<CrmContact> CrmContactList = CommonUtils.GetContactidInfoPo(item.InstitutionId);
                //    if (!CrmContactList.Any(m=>m.ContactId == item.TraderId))
                //    {
                //        traderName = string.Empty;
                //        traderId = string.Empty;
                //    }
                //}
            }

            // 客户名
            if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            {
                if (item.SelectedTrader_Edit != null)
                {
                    traderName = item.SelectedTrader_Edit.namecn;
                    traderId = item.SelectedTrader_Edit.idbkey;
                    //item.QqNumber = item.SelectedTrader_Edit.QQ;
                }
                else
                {
                    traderName = item.TraderName_Edit == item.TraderName ? item.TraderName : string.Empty;
                    traderId = item.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                }
                if(traderId == item.TraderId && traderName == item.TraderName)
                {
                    return;
                }
            }

            // 方向
            if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
            {
                side = item.Side_Edit;
                if (side == item.Side)
                    return;
            }
            // 价格
            if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            {
                price = item.Price_Edit;
                if (price == item.Price)
                    return;
            }
            // 数量
            if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            {
                qty = item.Qty_Edit;
                if (qty == item.Qty)
                    return;
            }
            // 清算速度
            if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
            {
                //DateTime createtime = CommonUtils.ConvertToDateTime(item.CreateTime);
                //settlType = CommonUtils.IsSpotSettle(createtime, item.edit_settltype_date, item.edit_settletype);
                //if (string.IsNullOrEmpty(settlType))
                //{
                //    settlType = item.edit_settltype_date.ToString("MM.dd") + (item.edit_settletype == 0 ? "+0" : "+1");
                //}
                settlType = item.SettlType_Edit;
                if (settlType == item.SettlType)
                    return;
            }

            // 新更新
            UpdateIntentionQuoteRequest request = new UpdateIntentionQuoteRequest();
            request.intentionId = item.IntentionId;
            request.institutionId = institutionId;
            request.institutionName = institutionName;
            request.traderId = traderId;
            request.traderName = traderName;
            request.side = side;
            request.price = price;
            request.qty = qty;
            request.settlType = settlType;
            bool isSuccess = await IntentionHandler.Instance.UpdateIntentionQuote(request);
            //if (isSuccess)
            //{
            //    // 客户机构
            //    if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            //    {
            //        item.InstitutionName = institutionName;
            //        item.InstitutionId = institutionId;
            //    }
            //    // 客户名
            //    if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            //    {
            //        item.TraderName = traderName;
            //        item.TraderId = traderId;
            //    }
            //    // 方向
            //    if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
            //        item.Side = side;
            //    // 价格
            //    if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            //        item.Price = price;
            //    // 数量
            //    if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            //        item.Qty = qty;
            //    // 清算速度
            //    if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
            //        item.SettlType = settlType;

            //    if (string.IsNullOrEmpty(item.OrderID) == false)
            //    {
            //        // 更新我的订单/我的撤单
            //        MyOrderProcesser.Processer.UpdateCustomInfo(item.IntentionId,
            //           item.InstitutionId, item.InstitutionName,
            //           item.TraderId, item.TraderName
            //           );
            //        // 更新我的成交
            //        MyExecutionProcesser.Processer.UpdateCustomInfo(item.IntentionId,
            //            item.InstitutionId, item.InstitutionName,
            //            item.TraderId, item.TraderName
            //            );
            //    }
            //}
        }

        /// <summary>
        /// 取消编辑修改
        /// </summary>
        /// <param name="cancelBtn"></param>
        private void CancelRow(Button cancelBtn)
        {
            if (cancelBtn != null && cancelBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = cancelBtn.DataContext as IntentionQuoteItem;
                item.IsEdit = "2";
            }
        }

        private void CheckandTips(string type)
        {
            Dictionary<string, TradeRequestData> orderDic = new Dictionary<string, TradeRequestData>();
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic_local = new Dictionary<string, List<TPlaceOrderResult>>();
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic_local = new Dictionary<string, List<TPlaceOrderResult>>();
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic_local = new Dictionary<string, List<TPlaceOrderResult>>();
            BaseReplyPo replyPo = CheckParam(this.SelectedItem_Working);
            if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
            {


                if (replyPo != null)
                {
                    if (replyPo.Code == "10001")
                    {
                        exceptionReplyDic_local[this.SelectedItem_Working.IntentionId] = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);

                        orderDic[this.SelectedItem_Working.IntentionId] = this.SelectedItem_Working;
                        BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                               warnReplyDic_local, prohibitReplyDic_local, 1, exceptionReplyDic_local);
                        batchResultWindow.Owner = Application.Current.MainWindow;
                        batchResultWindow.ShowDialog();
                        return;

                    }
                }
                
            }
            else
            {

                switch (type)
                {
                    case "1"://普通确认
                        DoConfirmOne(this.SelectedItem_Working);
                        break;
                    case "2"://闪电确认
                        DoQuickConfirmOne(this.SelectedItem_Working);
                        break;
                    case "3"://t/g确认
                        DoTGOne(this.SelectedItem_Working);
                        break;
                    case "4":
                        break;
                    case "5":
                        break;
                }

            }
        }
    }
}
