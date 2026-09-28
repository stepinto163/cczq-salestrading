using com.gateway.log;
using BDB.Trade.Plugin.Component;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Trade;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Utils;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;
using STC.Common.View.Component;
using STC.Common.Handler.Table;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class AddBondViewModel : BaseViewModel
    {
        private FrameworkElement _ownUI = null;

        #region 关联属性

        private decimal _total;
        public decimal Total
        {
            get { return _total; }
            set { _total = value; this.OnPropertyChanged(); }
        }

        private string _security;
        public string Security
        {
            get { return _security; }
            set { _security = value; this.OnPropertyChanged(); }
        }

        private AddBondViewModel _selectedBond = null;
        public AddBondViewModel SelectedBond
        {
            get { return _selectedBond; }
            set { _selectedBond = value; }
        }

        public string _symbol = null;
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; this.OnPropertyChanged(); }
        }

        public string _remainingTime = null;
        public string RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; this.OnPropertyChanged(); }
        }

        private ObservableCollection<AddBondViewModel> _addBondList = new ObservableCollection<AddBondViewModel>();

        public ObservableCollection<AddBondViewModel> AddBondList {  get { return _addBondList; } }

        public ICommand DelteCommand { get { return new RelayCommand(OnDelteClick); } }
        

        public ICommand AddCommand { get { return new RelayCommand(OnAddClick); } }
        

        public ICommand OkCommand { get { return new RelayCommand(OnOkClick); } }

        public ICommand BatchCommand { get { return new RelayCommand(OnBatchClick); } }


        public Visibility _batchVisibility = Visibility.Collapsed;
        public Visibility BatchVisibility
        {
            get { return _batchVisibility; }
            set { _batchVisibility = value; this.OnPropertyChanged(); }
        }

        public Visibility _oneVisibility = Visibility.Visible;
        public Visibility OneVisibility
        {
            get { return _oneVisibility; }
            set { _oneVisibility = value; this.OnPropertyChanged(); }
        }

        public Visibility _replyVisibility = Visibility.Collapsed;
        public Visibility ReplyVisibility
        {
            get { return _replyVisibility; }
            set { _replyVisibility = value; this.OnPropertyChanged(); }
        }

        #endregion


        #region 批量关联属性

        private int _checkedCount = 0;

        public int CheckedCount
        {
            get
            {
                return _checkedCount;
            }

            set
            {
                if (_checkedCount != value)
                {
                    _checkedCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private int _sumCount = 0;

        public int SumCount
        {
            get
            {
                return _sumCount;
            }

            set
            {
                if (_sumCount != value)
                {
                    _sumCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public const string ISCHECK_FIELD = "IsCheck";
        public const string SECURITYID_FIELD = "SecurityID";
        public const string SYMBOL_FIELD = "Symbol";
        public const string REMAININGDATE_FIELD = "RemainingDate";
        

        private string[] splitStrArr = new string[] { "\n" };
        public void BindBonds(string importStr)
        {
            OneVisibility = Visibility.Visible;
            BatchVisibility = Visibility.Collapsed;

            importStr = importStr.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] bonds = importStr.Split(splitStrArr, StringSplitOptions.RemoveEmptyEntries);
            foreach (string bond in bonds)
            {
                try
                {
                    BondsInfoPo bondInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(bond);

                    String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                    if (today.CompareTo(bondInfoPo.Maturity_Date.ToString()) > 0)
                    {
                        continue;
                    }

                    AddBondViewModel newBondModle = new AddBondViewModel();
                    newBondModle.Security = bondInfoPo.Bond_ID;
                    newBondModle.Symbol = bondInfoPo.Short_Name;
                    newBondModle.RemainingTime = bondInfoPo.RemaingingTime;

                    Total++;

                    _addBondList.Add(newBondModle);

                }
                catch (Exception e)
                {
                    LogHelper.Error($"识别失败:{bond}", e);
                }
            }
        }

        private bool isStartRecog = false;
        public void RecognizeIntentionQuote(string content)
        {
            OneVisibility = Visibility.Visible;
            BatchVisibility = Visibility.Collapsed;

            isStartRecog = true;
            _addBondList.Clear();
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
                                        AddRecognize(obj);
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

        private AddBondViewModel AddRecognize(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            if (isStartRecog)
            {
                isStartRecog = false;
            }

            AddBondViewModel newBondModle = new AddBondViewModel();
            newBondModle.Security = obj.securityId;
            newBondModle.Symbol = obj.symbol;
            newBondModle.RemainingTime = obj.timeToMaturity;

            Total++;
            _addBondList.Add(newBondModle);
            return newBondModle;
        }

        private string groupID = string.Empty;

        public string GroupID
        {
            get
            {
                return this.groupID;
            }
            set
            {
                this.groupID = value;
                this.OnPropertyChanged();
            }
        }

        private string groupName = string.Empty;

        public string GroupName
        {
            get
            {
                return this.groupName;
            }
            set
            {
                this.groupName = value;
                this.OnPropertyChanged();
            }
        }


        private volatile int successCount = 0;

        public int SuccessCount
        {
            get
            {
                return successCount;
            }

            set
            {
                successCount = value;
                this.OnPropertyChanged();
            }
        }

        private volatile int failCount = 0;

        public int FailCount
        {
            get
            {
                return failCount;
            }

            set
            {
                failCount = value;
                this.OnPropertyChanged();
            }
        }



        private volatile string resultStr = string.Empty;

        public string ResultStr
        {
            get
            {
                return resultStr;
            }
            set
            {
                resultStr = value;
                this.OnPropertyChanged();
            }
        }

        private volatile bool btnOK3Enabled = false;

        public bool BtnOK3Enabled
        {
            get
            {
                return btnOK3Enabled;
            }
            set
            {
                btnOK3Enabled = value;
                this.OnPropertyChanged();
            }
        }

        private int totalCount = 0;
        private int cancleCount = 0;


        private volatile int callbackCount = 0;

        public int CallbackCount
        {
            get
            {
                return callbackCount;
            }

            set
            {
                callbackCount = value;
                this.OnPropertyChanged();
            }
        }

        private volatile bool _isCanceled = false;

        public void CancelOpt()
        {
            _isCanceled = true;
        }

        #endregion


        #region 发送成功关联属性

        private string _imageSource;
        public string ImageSource { get { return _imageSource; } set { _imageSource = value; this.OnPropertyChanged(); } }

        private string _resultTxt;
        public string ResultTxt { get { return _resultTxt; } set { _resultTxt = value; this.OnPropertyChanged(); } }

        private Brush _txtBrush;
        public Brush TxtBrush { get { return _txtBrush; } set { _txtBrush = value; this.OnPropertyChanged(); } }

        #endregion

        private string _groupId;
        public string GroupId { get { return _groupId; } set { _groupId = value; } }
        
        public void Init(FrameworkElement ownUI)
        {
            _ownUI = ownUI;
        }

        private void OnOkClick()
        {
            Button btnOk = _ownUI.FindName("btnSendOK") as Button;

            if (btnOk != null)
                btnOk.IsEnabled = false;

            List<HomeGroupSecurityidsUpdateReq> reqList = new List<HomeGroupSecurityidsUpdateReq>();
            foreach (AddBondViewModel model in AddBondList)
            {
                if (!string.IsNullOrEmpty(model.Security) && !string.IsNullOrEmpty(model.Symbol))
                {
                    HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                    request.securityid = model.Security;
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

        private void AddReplyTxt(HomeGroupSecurityidsUpdateReq req, bool isEnd, int failIndex, int successNum)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (ReplyVisibility == Visibility.Collapsed)
                {
                    ReplyVisibility = Visibility.Visible;
                    BatchVisibility = OneVisibility = Visibility.Collapsed;
                }
                //if (isEnd)
                {
                    ImageSource = "/STC.Common;component/Images/stclite-ok.png";
                    TxtBrush = _ownUI.FindResource("LogDefaultFontBrush") as Brush;
                }
                //else
                //{
                //    ImageSource = "/STC.Common;compone/STC.Common;component/Images/stclite-smile.png";
                //    TxtBrush = _ownUI.FindResource("BlueButton.Static.Border") as Brush;
                //}

                ResultTxt = string.Format(string.Format("新增成功{0}，失败{1}", successNum, failIndex));
                StackPanel sp = _ownUI.FindName("spTxt") as StackPanel;

                TextBlock tb = new TextBlock();
                tb.Text = string.Format("{0}  {1}  {2}", req.securityid, req.symbol, req.remainingTime);
                tb.Margin = new Thickness(0, 3, 0, 3);
                tb.Foreground = _ownUI.FindResource("TextBox.Static.Foreground") as Brush;

                tb.HorizontalAlignment = HorizontalAlignment.Center;

                sp.Children.Add(tb);
            }
            ));
        }

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
                            AddReplyTxt(req, index == total, failIndex, index);
                        else
                            failIndex++;

                        Thread.Sleep(200);
                    }

                    HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfoByCache(GroupId);
                    string jsonStr = JsonConvert.SerializeObject(homeGroupInfo);
                    homeGroupInfo = JsonConvert.DeserializeObject<HomeGroupInfo>(jsonStr);

                    HomePageDataHandler.Instance.RequestGroups(true);

                    //HomePageDataHandler.Instance.Notify(FavoriteTypeEnum.BatchImport, this.GroupId, string.Empty, string.Empty);

                    foreach (KeyValuePair<HomeGroupSecurityidsUpdateReq, PResult> pair in resultDic)
                    {
                        if (pair.Value != null && pair.Value.IsSuccess())
                        {
                            HomePageDataHandler.Instance.Notify(FavoriteTypeEnum.AddBondFromTable, this.GroupId, pair.Key.securityid, string.Empty);

                            break;
                        }
                    }

                    List<HomeGroupSecurityIDsInfo> idList = GetAddSecurity(HomePageDataHandler.Instance.GetHomeGroupInfoByCache(GroupId), homeGroupInfo);
                    
                    foreach (HomeGroupSecurityIDsInfo securityIDsInfo in idList)
                    {
                        QuoteConfigHandler.Instance.NotifyOnGroupTableUpdate(this.GroupId, securityIDsInfo.id, securityIDsInfo.securityid, FavoriteTypeEnum.AddBond);
                    }
                    

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Button btnOk = _ownUI.FindName("btnSendOK") as Button;

                        if (btnOk != null)
                            btnOk.IsEnabled = true;
                    }));
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


        private void OnDelteClick()
        {
            if (SelectedBond != null)
            {
                bool result = CustomMessageBox.Question(string.Format("是否确定删除该债券({0} {1})？", SelectedBond.Security, SelectedBond.Symbol), "删除债券");
                if (result == true)
                {
                    _addBondList.Remove(SelectedBond);

                    Total--;
                }
            }
        }

        private void OnAddClick()
        {
            AddBondViewModel newBondModle = new AddBondViewModel();
            _addBondList.Add(newBondModle);
        }

        public void AddSecurity(object bond)
        {
            SeBondPo seBondPo = bond as SeBondPo;
            if (seBondPo != null && !string.IsNullOrEmpty(seBondPo.BondCode))
            {
                AddBondViewModel newBondModle = new AddBondViewModel();
                newBondModle.Security = seBondPo.BondCode;
                newBondModle.Symbol = seBondPo.Sn_Prefix + seBondPo.Sn_KeyWord + seBondPo.Sn_Suffix;
                newBondModle.RemainingTime = seBondPo.LeftTenor;

                Total++;

                _addBondList.Add(newBondModle);
            }
        }

        private void OnBatchClick()
        {
            OneVisibility = Visibility.Collapsed;
            BatchVisibility = Visibility.Visible;

            //BatchAddBondsWindow batchAddBondsWind = new BatchAddBondsWindow(this.GroupId);
            //batchAddBondsWind.Owner = Application.Current.MainWindow;
            //batchAddBondsWind.Show();

            //if (batchAddBondsWind.Result)
            //{
            //    object bondData = batchAddBondsWind.GetData();
            //    if (bondData != null)
            //    {
            //        DataView dataView = bondData as DataView;

            //        foreach (DataRowView drv in dataView)
            //        {
            //            AddBondViewModel newBondModle = new AddBondViewModel();
            //            newBondModle.Security = CommonUtils.ConvertToString(drv[BatchAddBondsWindowViewModel.SECURITYID_FIELD]);
            //            newBondModle.Symbol = CommonUtils.ConvertToString(drv[BatchAddBondsWindowViewModel.SYMBOL_FIELD]);
            //            newBondModle.RemainingTime = CommonUtils.ConvertToString(drv[BatchAddBondsWindowViewModel.REMAININGDATE_FIELD]);
            //            _addBondList.Add(newBondModle);
            //        }

            //    }
            //}
        }
    }

    
}
