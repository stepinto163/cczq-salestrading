using com.gateway.log;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.Position;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Position.Plugin.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static log4net.Appender.RollingFileAppender;


namespace STC.Position.Plugin.ViewModel
{
    public class ManualInputViewModel : ViewModelBase
    {
        public Window myWindow;
        public ICommand CloseWindowCommand { get { return new ComRelayCommand(OnCloseWindowCommand); } }
        public ICommand CancelWindowCommand { get { return new ComRelayCommand(CancelWindowCommandClick); } }
        public ICommand OkCommand { get { return new ComRelayCommand(OnOkCommandClick); } }

        public ICommand AddRecordCommand { get { return new ComRelayCommand(OnAddRecordCommandClick); } }
        public ICommand DelRecordCommand { get { return new ComRelayCommand(OnDelRecordCommandClick); } }

        public ICommand EditRecordCommand { get { return new ComRelayCommand(OnEditRecordCommandClick); } }
        public ICommand SaveRecordCommand { get { return new ComRelayCommand(OnSaveRecordCommandClick); } }

        private bool _isCanSave = true;
        public bool IsCanSave 
        {
            get 
            {
                return _isCanSave;
            }
            set 
            {
                _isCanSave = value;
                this.OnPropertyChanged(nameof(IsCanSave));
            }
        }

        bool _isEdit = false;
        public bool IsEdit
        {
            get
            {
                return _isEdit;
            }
            set 
            {
                _isEdit = value;
                IsCanSave = !value;
                this.OnPropertyChanged(nameof(IsEdit));
            }
        }

        public bool IsAdd = false;

        /// <summary>
        /// 选中的行数据
        /// </summary>
        private PositionManualPo _selectedItem;
        public PositionManualPo SelectedItem
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

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                this.OnPropertyChanged();
            }
        }


        private int _RecordCount = 0;
        public int RecordCount
        {
            get
            {
                return _RecordCount;
            }
            set
            {
                _RecordCount = value;
                this.OnPropertyChanged(nameof(RecordCount));
            }
        }
        private string _DefaultSecurityID;
        public string DefaultSecurityID
        {
            get
            {
                return _DefaultSecurityID;
            }
            set
            {
                _DefaultSecurityID = value;
                this.OnPropertyChanged(nameof(DefaultSecurityID));
            }
        }
        private object lockObj = new object();
        private ObservableCollection<PositionManualPo> positionManualPoList = new ObservableCollection<PositionManualPo>();
        public ObservableCollection<PositionManualPo> DataDV
        {
            get
            {
                return positionManualPoList;
            }
            set
            {
                positionManualPoList = value;
                this.OnPropertyChanged();
            }
        }

        public static List<KeyValuePair<string, string>> LstSide = new List<KeyValuePair<string, string>>();
        public static List<KeyValuePair<string, string>> LstSettleType = new List<KeyValuePair<string, string>>();
        public static List<KeyValuePair<string, string>> LstVenuetypegw = new List<KeyValuePair<string, string>>();
        public static List<KeyValuePair<string, string>> LstBookList = new List<KeyValuePair<string, string>>();
        public static List<KeyValuePair<string, string>> LstOpenClose = new List<KeyValuePair<string, string>>();
        private Dictionary<string, string> DictBookList = new Dictionary<string, string>();
        public static ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        public bool IsInit = false;

        private string iconUrl;
        /// <summary>
        /// 图标
        /// </summary>
        public string IconUrl
        {
            get
            {
                return iconUrl;
            }
            set
            {
                iconUrl = value;
                this.OnPropertyChanged(nameof(IconUrl));
            }
        }

        public ManualInputViewModel()
        {

        }

        public ManualInputViewModel(Window win)
        {
            this.myWindow = win;
            InitItemData();
        }

        private void InitItemData()
        {
            if (!IsInit)
            {
                LstOpenClose.Clear();
                LstOpenClose.Add(new KeyValuePair<string, string>("0", "开仓"));
                LstOpenClose.Add(new KeyValuePair<string, string>("1", "平仓"));
                LstOpenClose.Add(new KeyValuePair<string, string>("2", "平今"));
                LstOpenClose.Add(new KeyValuePair<string, string>("3", "平昨"));
                LstSide.Clear();
                LstSide.Add(new KeyValuePair<string, string>("1", "Bid"));
                LstSide.Add(new KeyValuePair<string, string>("2", "Ofr"));
                //LstSettleType = ComboSourceUtil.Instance.GetComboSource("settletype_myorder");
                LstSettleType.Clear();
                LstSettleType.Add(new KeyValuePair<string, string>("2", "T+1"));
                LstSettleType.Add(new KeyValuePair<string, string>("1", "T+0"));
                //LstVenuetypegw = ComboSourceUtil.Instance.GetComboSource("venues");
                LstVenuetypegw.Clear();
                LstVenuetypegw.Add(new KeyValuePair<string, string>("CTP", "期货"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("TPSC", "国利"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("CNEX", "国际"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("PTCN", "平安"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("CTTJ", "信唐"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("CCTB", "中诚"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("UEDA", "上田"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("INNER", "内部"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("ODM", "XBOND"));
                LstVenuetypegw.Add(new KeyValuePair<string, string>("QDMESP", "双边"));
                LstBookList.Clear();
                GetBookUser();
                MultiTreeComboxItemsSource.Clear();
                ObservableCollection<TBookBookTreeItemModel> BookList = AdminHandler.Instance.GetTCurrentBookCollection(LocalEnv.UserInfo.UserName);
                MultiTreeComboxItemsSource = CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(CommonUtils.SerializeObject(BookList));
                IsInit = true;

                //自动添加一条记录
                OnAddRecordCommandClick();
            }
        }
        public void GetBookUser()
        {
            QueryPositionBookReq req = new QueryPositionBookReq();
            req.username = LocalEnv.UserInfo.UserName;
            req.userid = LocalEnv.UserInfo.UserId;
            QueryPositionBookResult reply = PositionHandler.Instance.QueryPositionBook(req);
            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    foreach (PostionBookModel pbm in reply.Datas)
                    {
                        LstBookList.Add(new KeyValuePair<string, string>(pbm.id, $"{pbm.id}-{pbm.bookname}"));
                        DictBookList.Add(pbm.id, pbm.bookname);
                    }
                }
            }
        }

        #region 命令
        private void OnCloseWindowCommand()
        {
            myWindow.Close();
        }
        private void CancelWindowCommandClick()
        {
            myWindow.Close();
        }

        private void OnOkCommandClick()
        {
            if (!IsHaveRecord())
            {
                showMessage("请至少输入一笔订单！");
                return;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                IsCanSave = false;
                ((ManualInput)myWindow).ShowEMSMask(true);
                CommmitManualInput();
            }));
        }


        private void OnAddRecordCommandClick()
        {
            lock (DataDV)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        if (!IsEdit)
                        {
                            IsEdit = true;
                            IsAdd = true;
                            lock (DataDV)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    PositionManualPo row = new PositionManualPo();
                                    row.EditBtnVisible = "0";
                                    row.SaveBtnVisible = "1";
                                    row.SettleType = "2";
                                    row.IsAdded = "0"; 
                                    row.Side = "2";
                                    row.MarketIndicator = Consts.MARKETINDICATOR_BOND;
                                    row.IsExeBond = "0";//默认非含权债
                                    row.TradeDate = DateTime.Now.ToString("yyyy-MM-dd");
                                    row.CurrentDate = DateTime.Today;
                                    //row[BookId_FIELD] = LstBookList[0].Key;
                                    row.SideItem= LstSide;
                                    row.Venuetypegw = "INNER";
                                    if (!string.IsNullOrWhiteSpace(DefaultSecurityID)) 
                                    {
                                        row.SecurityId = DefaultSecurityID;
                                    }
                                    this.DataDV.Add(row);
                                    this.SelectedItem = this.DataDV?.LastOrDefault();
                                }));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                }));
            }
        }

        private void OnDelRecordCommandClick()
        {
            lock (DataDV)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        PositionManualPo row = SelectedItem;
                        lock (DataDV)
                        {
                            RecordCount = this.DataDV.Count();
                            IsEdit = false;
                            IsAdd = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                }));
            }
        }

        private void OnEditRecordCommandClick()
        {
            lock (DataDV)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        if (!IsEdit)
                        {
                            IsEdit = true;
                            IsAdd = false;
                            lock (DataDV)
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    PositionManualPo row = this.SelectedItem;
                                    row.EditBtnVisible = "0";
                                    row.SaveBtnVisible = "1";
                                    this.SelectedItem = this.DataDV[this.SelectedIndex];
                                }));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                }));
            }
        }

        Regex regex = new Regex("^[0-9.-]+$");
        private void OnSaveRecordCommandClick()
        {
            PositionManualPo row = this.SelectedItem;
            string securityid = row.SecurityId as string;
            if (string.IsNullOrWhiteSpace(securityid))
            {
                showMessage("代码不能为空！");
                return;
            }

            BondsInfoPo bondInfo = CommonUtils.GetBondsInfoPoBySecurityID(securityid);
            if (bondInfo == null)
            {
                showMessage("债券不存在或已过期！");
                return;
            }
            string side = row.Side as string;
            if (string.IsNullOrWhiteSpace(side))
            {
                showMessage("方向不能为空！");
                return;
            }

            string position = row.Position;
            if (string.IsNullOrWhiteSpace(position))
            {
                showMessage("成交量不能为空！");
                return;
            }
            else
            {
                if (!CommonUtils.IsNumeric(position))
                {
                    showMessage("成交量不合法!");
                    return;
                }

                decimal d = 0;
                if (!decimal.TryParse(position, out d)  || d < 0)
                {
                    showMessage("成交量不合法! 填写范围 >0 ");
                    return;
                }
            }


            string tradedate = row.TradeDate;
            if (string.IsNullOrWhiteSpace(tradedate))
            {
                showMessage("交易日期不能为空！");
                return;
            }
            else
            {
                string[] format = { "yyyy-MM-dd", "yyyy-mm-dd", "yyyy/MM/dd", "yyy-MM-dd", "yyyyMMdd", "M/d/yyyy" };
                DateTime date = DateTime.Now;
                tradedate = tradedate.Split(' ')[0];
                if (DateTime.TryParseExact(tradedate, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                {
                    if (!string.IsNullOrEmpty(bondInfo.Maturity_Date.ToString()))
                    {
                        DateTime bondsEndDate = DateTime.ParseExact(bondInfo.Maturity_Date.ToString().Substring(0, 8), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        if (bondsEndDate.CompareTo(date) < 0)
                        {
                            showMessage($"录入失败，录入成交日期 {date.ToString("yyyy-MM-dd") } 晚于债券到期日 { bondsEndDate.ToString("yyyy-MM-dd") } ！");
                            return;
                        }
                    }
                    row.TradeDate = date.ToString("yyyy-MM-dd");
                }
                else
                {

                    showMessage("交易日期不合法! 格式为 yyyy-MM-dd");
                    return;
                }
            }

            string bookid = row.BookId as string;
            if (string.IsNullOrWhiteSpace(bookid))
            {
                showMessage("组合不能为空！");
                return;
            }

            string settletype = row.SettleType as string;
            if (string.IsNullOrWhiteSpace(settletype))
            {
                showMessage("清算速度不能为空！");
                return;
            }

            string _price = row.Price;
            string _netprice = row.NetPrice;

            if (!string.IsNullOrWhiteSpace(_price))
            {
                if (!CommonUtils.IsNumeric(_price))
                {
                    showMessage("成交价(%)不合法!");
                    return;
                }
            }
            else 
            {
                showMessage("成交价(%)不能为空!");
                return;
            }

            if (!string.IsNullOrWhiteSpace(_netprice))
            {
                if (!CommonUtils.IsNumeric(_netprice))
                {
                    showMessage("成交价(净)不合法!");
                    return;
                }
            }
            else
            {
                showMessage("成交价(净)不能为空!");
                return;
            }
            {
                decimal d = 0;
                if (!string.IsNullOrWhiteSpace(_netprice))
                {

                    if (!(decimal.TryParse(_netprice, out d) || d <= 0))
                    {
                        showMessage("成交价(净)不合法! 填写范围 > 0");
                        return;
                    }

                }

                if (!string.IsNullOrWhiteSpace(_price))
                {
                    if (!(decimal.TryParse(_price, out d) || d < -100))
                    {
                        showMessage("成交价(%) 不合法! 填写范围 >-100");
                        return;
                    }
                    
                }
            }

            string venuetypegw = row.Venuetypegw as string;
            if (string.IsNullOrWhiteSpace(venuetypegw))
            {
                showMessage("渠道不能为空！");
                return;
            }
            row.NetPrice = _netprice;
            row.Price = _price;

            row.SaveBtnVisible = "0";
            row.EditBtnVisible = "1";

            if (string.IsNullOrWhiteSpace(row.IsAdded))
            {
                row.IsAdded = "0";
            }

            row.BookName = DictBookList.ContainsKey(bookid) ? DictBookList[bookid] : bookid;

            IsEdit = false;
            IsAdd = false;

            RecordCount = this.DataDV.Count();

        }
      
        #endregion

        #region 提交手工订单
        private bool IsHaveRecord()
        {
            return this.DataDV.Count() > 0;
        }

        private async void CommmitManualInput()
        {
            try
            {
                int cntSuccess = 0;
                int cntFail = 0;
                int cntRecord = 0;
                foreach (PositionManualPo dr in this.DataDV)
                {
                    if (dr.IsAdded == "1")
                    {
                        continue;
                    }

                    ExecutionReportReq er = new ExecutionReportReq();
                    er.MarketIndicator = dr.MarketIndicator;
                    er.BookID = dr.BookId;
                    er.ExecType = "2";
                    er.FirmFlag = "0";
                    er.Yield = dr.Price;
                    er.LastPx = dr.Price;
                    er.NetPrice = dr.NetPrice;
                    er.LastQty = dr.Position;
                    er.SecurityID = dr.SecurityId;
                    er.OrdRejReason = "";
                    er.OrdStatus = "2";
                    er.OrdType = "2";
                    er.CounterPartyName = dr.CounterPartyName;
                    er.CounterPartySubName = dr.CounterPartySubName;
                    er.SettlDate = "";
                    er.SettlType = ("2".Equals(er.MarketIndicator) || "5".Equals(er.MarketIndicator) || "1".Equals(dr.SettleType))?"+0":"+1";
                    er.Side = dr.Side;
                    //后台需要yyyyMMdd格式的日期 STCPROD-379
                    er.TradeDate = dr.TradeDate.Replace("-",string.Empty);
                    er.VenueTypeGW = "";
                    er.Venues = dr.Venuetypegw;
                    er.Info10 = "[持仓调整]" + dr.Remark;
                    er.MarketIndicator = dr.MarketIndicator;
                    if (Consts.MARKETINDICATOR_FUTURE.Equals(er.MarketIndicator))
                    {
                        er.OffsetFlag = dr.OpenClose;
                        er.LastPx = dr.NetPrice;
                        er.VenueTypeGW = "CTP";
                        er.SettlType = string.Empty;
                    }
                    
                    BondsInfoPo bondInfo = CommonUtils.GetBondsInfoPoBySecurityID(dr.SecurityId);
                    er.Symbol = bondInfo.Short_Name;
                    er.SecurityType = bondInfo.SECURITY_TYPE;
                    er.PartySubName = LocalEnv.UserInfo.FullName;
                    er.PartySubID = LocalEnv.UserInfo.UserId;

                    PResult reply = await CommmitManualInputOrder(er);

                    if (reply != null && reply.IsSuccess())
                    {
                        dr.Msg = "成功";
                        dr.IsAdded = "1";
                        cntSuccess++;
                    }
                    else
                    {
                        dr.IsAdded = "0";
                        if (reply != null)
                        {
                            dr.Msg = $"失败{reply.Msg}";
                        }
                        else
                        {
                            dr.Msg = $"失败";
                        }
                        cntFail++;
                    }

                    cntRecord++;
                }

                IsCanSave = true;

                ((ManualInput)myWindow).ShowEMSMask(false);
                if ((cntSuccess == cntRecord) && cntRecord > 0)
                {
                    showSuccessMessage($"成交录入全部成功!\r\n成功录入{cntSuccess} 条！");
                    OnCloseWindowCommand();
                }
                else
                {
                    if (cntRecord > 0)
                    {
                        CustomMessageBox.Error($"成交录入部分失败!\r\n成功录入{cntSuccess} 条!\r\n失败录入{cntFail} 条!", "提示", myWindow);
                    }
                    else
                    {
                        showMessage("请至少输入一笔订单！");
                        OnCloseWindowCommand();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                ((ManualInput)myWindow).ShowEMSMask(false);
                CustomMessageBox.Error($"手工录入订单失败因: {ex.Message}", "提示", myWindow);
            }
            finally
            {
                IsCanSave = true;
            }
        }

        public async Task<PResult> CommmitManualInputOrder(ExecutionReportReq req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.ManualInputOrder(req);
            });

            return reply;
        }
        /// <summary>
        /// 提示Warn消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();
                
                me.Title ="提示";
                me.setWarnMsg(message);
                me.Owner = this.myWindow;
                me.ShowDialog();
            }));
        }

        /// <summary>
        /// 提示成功消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showSuccessMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
               SuccessBoxView me = new SuccessBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = this.myWindow;
                me.ShowDialog();
            }));
        }
        #endregion
        public void setComboBox(string ItemKey)
        {
            if (SelectedItem == null)
                return;
            PositionManualPo Item = SelectedItem;
            if ("Z".Equals(ItemKey))
            {
                List<KeyValuePair<string, string>> ItemSource = new List<KeyValuePair<string, string>>();
                ItemSource.Add(new KeyValuePair<string, string>("1", "转入"));
                ItemSource.Add(new KeyValuePair<string, string>("2", "转出"));
                Item.SideItem = ItemSource;
            }
            else 
            {
                Item.SideItem = LstSide;
            }  
        }

    }
}
