using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Common.ViewModel.MyTrading;
using STC.Frame.Utils.ExcelHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Frame.ViewModel
{
    public class RecognizeBondViewModel : ViewModelBase
    {
        private HomeGroupInfo _SelectedAddHomeGroup;
        public HomeGroupInfo SelectedAddHomeGroup
        {
            get { return _SelectedAddHomeGroup; }
            set
            {
                _SelectedAddHomeGroup = value;

                if (_SelectedAddHomeGroup != null && CommonConstants.HOME_Type.Equals(_SelectedAddHomeGroup.inf1))
                {
                    isHomeGroup = true;
                }
                else
                {
                    isHomeGroup = false;
                }
                this.OnPropertyChanged();
            }
        }
        private HomeGroupInfo _SelectedHomeGroup;
        public HomeGroupInfo SelectedHomeGroup
        {
            get { return _SelectedHomeGroup; }
            set
            {
                _SelectedHomeGroup = value;
                this.OnPropertyChanged();
            }
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
                    HomeGroupInfo obj = AddHomeGroupDic.Where(O => O.id.Equals(_groupID)).FirstOrDefault();
                    SelectedAddHomeGroup = obj;
                }
            }
        }

        public bool isHomeGroup = false;

        private ObservableCollection<HomeGroupInfo> _homeGroupDic = new ObservableCollection<HomeGroupInfo>();

        public ObservableCollection<HomeGroupInfo> HomeGroupDic
        {
            get { return _homeGroupDic; }
            set
            {
                _homeGroupDic = value;
                this.OnPropertyChanged();
            }
        }
        private ObservableCollection<HomeGroupInfo> _AddHomeGroupDic = new ObservableCollection<HomeGroupInfo>();

        public ObservableCollection<HomeGroupInfo> AddHomeGroupDic
        {
            get { return _AddHomeGroupDic; }
            set
            {
                _AddHomeGroupDic = value;
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
            set { intentionQuoteInfo = value;
                this.OnPropertyChanged();
            }
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

        internal void UnregisterEvent()
        {
            IntentionOldHandler.Instance.OnRecognition -= Instance_OnRecoginition;
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
        private bool _isHomeChecked = false;

        public bool IsHomeChecked
        {
            get { return _isHomeChecked; }
            set
            {
                _isHomeChecked = value;
                this.OnPropertyChanged();
            }
        }
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
        private ICommand _ImportCommand;

        public ICommand ImportCommand
        {
            get { return _ImportCommand; }
            set { _ImportCommand = value; }
        }
        public RecognizeBondViewModel()
        {
            if (LocalEnv.ConfInfo.IsParserImage == "1")
            {
                ParserImageVisibility = Visibility.Visible;
            }
            IntentionQuoteInfo.CollectionChanged += IntentionQuoteInfo_CollectionChanged;
            IntentionOldHandler.Instance.OnRecognition += Instance_OnRecoginition;
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.LiveFilteringProperties.Add("Status");
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);
            Top10Command = new ComRelayCommand(OnTop10Click);
            Top50Command = new ComRelayCommand(OnTop50Click);
            ImportCommand = new ComRelayCommand(ImportClick);
            HomeGroupDic.Clear();
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                if ("*".Equals(AdminHandler.Instance.getSecurityGroupUser()))
                {
                    request.username = "";
                }
                else 
                {
                    request.username = LocalEnv.UserInfo.UserName;
                }
                HomeGroupSecurityidsReplyPo replyPo = AdminHandler.Instance.homeGroupSecurityidsQuery(request);
                Application.Current.Dispatcher.Invoke((Action)(() => {
                    foreach (KeyValuePair<string, HomeGroupInfo> pair in AdminHandler.dicGroup)
                    {
                        HomeGroupDic.Add(pair.Value);
                    }
                }));
            });
        }

        private void OnTop50Click()
        {
            int i = 1;
            foreach (IntentionQuoteItem item in IntentionQuoteInfo)
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
        }
        internal void ClearList()
        {
            if (this.intentionQuoteInfo != null)
            {
                this.IntentionQuoteInfo.Clear();

                DataCount = 0;
                SelectedCount = 0;
            }
        }
        internal void ClickOppositeCheckAll(bool flag)
        {
            foreach (IntentionQuoteItem item in this.intentionQuoteInfo)
            {
                item.IsOpposite = flag;
            }
        }
        private void Instance_OnRecoginition(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                AddRecognize(obj);

            }));
        }

        private bool isStartRecog = false;
        private IntentionQuoteItem AddRecognize(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            if (this.IntentionQuoteInfo.Any(entry => entry.SecurityId == obj.securityId))
            {
                return this.IntentionQuoteInfo.First(entry => entry.SecurityId == obj.securityId);
            }

            if (isStartRecog)
            {
                isStartRecog = false;
            }

            IntentionQuoteItem item = new IntentionQuoteItem();
            item.IntentionId = obj.intentionId;
            item.InstitutionId = obj.institutionId;
            item.InstitutionName = obj.institutionName;
            item.TraderId = obj.traderId;
            item.TraderName = obj.traderName;
            item.QqNumber = obj.qqNumber;
            item.Side = obj.side;
            item.SecurityId = obj.securityId;
            item.Symbol = obj.symbol;
            item.CleanPrice = obj.cleanPrice;
            item.FullPrice = obj.fullPrice;
            item.Price = obj.price;
            //item.ClOrdID = obj.clordid;
            decimal qty = obj.qty.ConvertToDecimal() / 100;
            if (qty >= 1)
            {
                item.Qty = qty * 100;
            }

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
            item.IsChecked = true;
            //if (IsAllChecked)
            //{
            //    item.IsChecked = true;
            //}
            try
            {
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
            item.PriceType = obj.priceType;

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
                            findData.OrderCount = srcccdc.OrderCount;
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
            isStartRecog = true;
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    string pattern = @"(\r\n)+";
                    string txtcont = System.Text.RegularExpressions.Regex.Replace(content, pattern, "\r\n");
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = txtcont;
                    STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo reply = ParserHandler.Instance.RequestParserBond(request);

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
                                    foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in reply.Datas)
                                    {
                                        IntentionQuoteItem item = AddRecognize(obj);
                                        //item.No = ++index;
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
        public void BindBonds(string importStr)
        {
            List<string> bonds = new List<string>();
            if (IsHomeChecked)
            {
                // 根据选中HomeGroup
                //IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> dic = (IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>>)SecurityGroupManagerViewModel.dicGroup;
                HomeGroupInfo homeGroupInfo = null;
                if (SelectedHomeGroup != null && AdminHandler.dicGroup.TryGetValue(this.SelectedHomeGroup.id, out homeGroupInfo))
                {
                    //HomeGroupInfo homeGroupInfo = HomePageDataHandler.GetHomeGroupInfo(dic, this.SelectedHomeGroup.id);
                    if (homeGroupInfo != null && homeGroupInfo.securityids != null && homeGroupInfo.securityids.Count > 0)
                    {
                        foreach (HomeGroupSecurityIDsInfo homeSecurtity in homeGroupInfo.securityids)
                        {
                            bonds.Add(homeSecurtity.securityid);
                        }
                    }
                }
            }
            else
            {
                return;
            }
            List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo> list = new List<STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo>();
            List<string> errorList = new List<string>();
            string ResultStr ="";
            foreach (string bond in bonds)
            {
                try
                {
                    string newBond = CommonUtils.AdjustSecurityID(bond);

                    BondsInfoPo bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(newBond.ToUpper());
                    //如果合约代码用的是小写
                    if (bondInfoPo == null)
                    {
                        bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(newBond.ToLower());
                    }

                    String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                    if (bondInfoPo == null)
                    {
                        ResultStr += $"债券{bond}无效" + "\r\n";
                        errorList.Add(string.Format("{0}债券不存在", bond));
                        break;
                    }
                    if (this.IntentionQuoteInfo.Any(entry => entry.SecurityId == bondInfoPo.Bond_ID))
                    {
                        continue;
                    }
                    STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj = new STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo();
                    obj.securityId = bondInfoPo.Bond_ID;
                    obj.symbol = bondInfoPo.Short_Name;
                    obj.timeToMaturity = bondInfoPo.RemaingingTime;
                    list.Add(obj);
                }
                catch (Exception e)
                {
                    LogHelper.Error($"识别失败:{bond}", e);
                    errorList.Add(string.Format("{0}识别失败", bond));
                    break;
                }
            }
            //if (errorList.Count > 0)
            //{
            //    //CustomMessageBox.Warning(errorList.ToString(), "从已有关注组中导入债券");
            //    Prompt2Seconds prompt = new Prompt2Seconds();
            //    prompt.PromptTitle = "批量导入";
            //    prompt.Owner = Application.Current.MainWindow;
            //    prompt.PromptContext = ResultStr;
            //    prompt.Show();
            //}
            //else
            //{
                int index = 0;
                foreach (STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj in list)
                {
                    IntentionQuoteItem item = AddRecognize(obj);
                    item.No = ++index;
                }
            //}
        }
        internal void BatchAddBond()
        {
            
        }

        public List<HomeGroupSecurityidsUpdateReq> getUpdateList()
        {
            List<HomeGroupSecurityidsUpdateReq> reqList = new List<HomeGroupSecurityidsUpdateReq>();
            if (ViewSource.View != null)
            {
                foreach (IntentionQuoteItem mm in ViewSource.View)
                {
                    HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                    request.securityid = mm.SecurityId;
                    request.symbol = mm.Symbol;
                    request.action = "1"; // 添加
                    request.remark = $"{mm.SecurityId}  {mm.Symbol}  {mm.RemainingTime}";
                    request.terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
                    request.type = isHomeGroup ? CommonConstants.HOME_Type : CommonConstants.QUANT_Type;
                    request.groupid = SelectedAddHomeGroup.id;
                    request.username = SelectedAddHomeGroup.username;
                    reqList.Add(request);
                }
            }
            return reqList;
        }

        private async void ImportClick()
        {
            try
            {
                // 1.弹框
                //创建一个保存文件式的对话框
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();                              //设置这个对话框的起始保存路径
                ofd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.RestoreDirectory = true;
                //设置保存的文件的类型，注意过滤器的语法
                ofd.Filter = "Excel Documents|*.xls;*.xlsx";
                ofd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!ofd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !ofd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        CommonUtils.Prompt2Second("批量导入", "当前只支持xls文件和xlsx文件");
                        return;
                    }

                    // 3.导入操作
                    List<string> errorList = await Sub_ImportExcel(ofd.FileName);


                    if (errorList != null && errorList.Count > 0)
                    {
                        StringBuilder errors = new StringBuilder();
                        foreach (string error in errorList)
                        {
                            if (errors.Length > 0)
                            {
                                errors.AppendLine();
                            }
                            errors.Append(error);
                        }
                        promptTxt = errors.ToString();
                    }
                    else
                    {
                        promptTxt = "导入成功";
                    }
                }
                else
                {
                    promptTxt = "取消导入";
                }
                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    CommonUtils.Prompt2Second("批量导入", promptTxt);
                }
            }
            catch (Exception e)
            {
                CommonUtils.Prompt2Second("批量导入", e.Message);
                LogHelper.Error(e);
            }
        }

        private Task<List<string>> Sub_ImportExcel(string filePath)
        {
            return Task<List<string>>.Run(async () =>
            {
                List<string> errorlist = new List<string>();
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                    if (isFileLock)
                    {
                        errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                        return errorlist;
                    }
                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    Dictionary<int, string> dic = helper.ReturnSheetList();
                    StringBuilder str = new StringBuilder();
                    foreach (KeyValuePair<int, string> pair in dic)
                    {
                        DataTable importDT = helper.ExcelToDataTable(pair.Key);
                        if (importDT != null && importDT.Rows.Count > 0)
                        {
                            foreach (DataRow dr in importDT.Rows)
                            {
                                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj = new STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo();
                                obj.securityId = CommonUtils.ConvertToString(dr[ExcelExportUtils.Excel_SecurityIdField]);
                                obj.symbol = CommonUtils.ConvertToString(dr[ExcelExportUtils.Excel_SymbolField]);
                                AddRecognize(obj);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    errorlist.Add(e.Message);
                    LogHelper.Error(e);
                }
                finally
                {
                    if (helper != null)
                        helper.Dispose();
                }
                return errorlist;
            });
        }
    }
}
