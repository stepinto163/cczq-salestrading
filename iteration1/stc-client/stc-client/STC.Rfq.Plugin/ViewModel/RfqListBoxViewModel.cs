using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using System.Data;
using System.Windows.Input;
using STC.Rfq.Plugin.Utils;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using STC.Rfq.Plugin.View.HomePage;
using STC.Rfq.Plugin.View.Component;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using System.Collections.Concurrent;
using System.Threading;
using STC.Rfq.Plugin.View;
using System.Windows.Data;
using Ats.Foundation.Message;
using Newtonsoft.Json;
using com.gateway.log;
using System.Windows.Documents;
using System.Windows.Media;
using STC.Common.Handler.Ans;
using STC.Common.Model.Ans;
using STC.Rfq.Plugin.Utils.Data;
using STC.Rfq.Plugin.Command;
using STC.Rfq.Plugin.ViewModel;
using STC.Common.ViewModel.Data;
using STC.Common.DataProcesser;
using STC.Common.View.UI;
using STC.Frame.CustomUI;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.Common.Model.RFQ;
using STC.Common.Handler.RFQ;
using System.Windows.Markup;
using STC.Common.Api;
using System.Collections.ObjectModel;
using STC.Rfq.Plugin.Model;
using System.ComponentModel;
using STC.Common.Data;
using System.IO;

namespace STC.Rfq.Plugin.ViewModel
{
    /// <summary>
    /// RFQ数据内容
    /// </summary>
    public class RfqListBoxViewModel : ViewModelBase
    {
        public ConcurrentDictionary<string, string> mainWinList = new ConcurrentDictionary<string, string>();

        public string groupID = string.Empty;

        public ICommand AddBondCommand { get; set; }

        public ICommand ExportCommand { get; set; }

        public ICommand ImportCommand { get; set; }

        public ICommand ToDelCommand { get; set; }

        public ICommand ConfigCommand { get; set; }

        public ICommand EnableCommand { get; set; }
        /// <summary>
        /// 更新锁
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// 字典锁
        /// </summary>
        private object LockDict = new object();
        private object queueLock = new object();
        /// <summary>
        /// Rfq策略列表
        /// </summary>
        private ObservableCollection<RfqstgViewDataModel> fullrfqorder = new ObservableCollection<RfqstgViewDataModel>();
        public ObservableCollection<RfqstgViewDataModel> FullRfqOrder
        {
            get
            {
                return fullrfqorder;
            }

            set
            {
                fullrfqorder = value;
                this.OnPropertyChanged(nameof(FullRfqOrder));
            }
        }


        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }

        private string _searchText = string.Empty;
        /// <summary>
        /// 筛选
        /// </summary>
        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();
                    this.Mv.View.Refresh();
                }
            }
        }

        private RfqstgViewDataModel _SelectedStg;
        public RfqstgViewDataModel SelectedStg
        {
            get
            {
                return _SelectedStg;
            }

            set
            {
                if (_SelectedStg != value)
                {
                    _SelectedStg = value;
                    this.OnPropertyChanged(nameof(SelectedStg));
                }
            }
        }

        /// <summary>
        /// 是否已经订阅
        /// </summary>
        private bool IsSubscribe = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RfqListBoxViewModel(string grpID)
        {
            this.AddBondCommand = new RelayCommand(this.AddBond);
            this.ConfigCommand = new DelegateCommand(new Action<object>(OnConfigCommand));
            this.ImportCommand = new RelayCommand(this.ImportExcel);
            this.ExportCommand = new RelayCommand(this.ExportExcel);
            this.EnableCommand = new DelegateCommand(new Action<object>(OnEnableCommand));

            //初始化视图
            InitView();

            this.Init(grpID);
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        private void InitView()
        {
            this.Mv.Source = FullRfqOrder;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            //排序
            this.Mv.SortDescriptions.Add(new SortDescription("createtime", ListSortDirection.Descending));
            this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();
        }

        private void Mv_Filter(object sender, FilterEventArgs e)
        {
            RfqstgViewDataModel pbd = e.Item as RfqstgViewDataModel;
            //搜索框关键字搜索
            if (SearchText != null && this.SearchText.Length > 0)
            {
                if (pbd.securityid.Contains(SearchText) || pbd.symbol.Contains(SearchText))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = false;
                }
            }
            return;
        }

        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init(this.groupID);
                }));
            }
        }

        ~RfqListBoxViewModel()
        {
            LogHandler.Info("rfqListBox Dispose");
            this.Dispose();
        }

        new public void Dispose()
        {
            Clear();
            base.Dispose();
        }


        public void Clear()
        {

            BaseHandler.Instance.UnSubscribeLoadDataStatus(OnLoadDataStatus);

            RfqHandler.Instance.UnSubscribeRfq(Instance_OnGroup);

            RfqHandler.Instance.UnSubscribeRfqOrder(OnRfqOrderCallBack);

            IsSubscribe = false;

            BaseHandler.Instance.UnSubscribe(Instance_OnClear);
        }


        private void Init(string groupID)
        {
            this.groupID = groupID;

            FullRfqOrder.Clear();

            BaseHandler.Instance.Init(Instance_OnClear);
            BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);
        }


        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                RfqstgViewReq req = new RfqstgViewReq();
                req.username = LocalEnv.UserInfo.UserName;
                req.groupid = this.groupID;

                GetData(req);
            }
        }
        public async void GetData(RfqstgViewReq req)
        {
            RfqstgViewResult reply = null;
            await Task.Run(() =>
            {
                reply = RfqHandler.Instance.RfqStgViewQuery(req);

                if (reply != null && reply.Code == "0000")
                {
                    OnRfqstgViewCallBack(reply.Datas);
                }
                else
                {
                    if (reply != null)
                    {
                        com.gateway.log.LogHelper.Error($"RfqQueryOrder exception.{reply.Msg}");
                    }
                }

                // 订阅Rfq
                Subscribe();
            });
        }

        #region Command

        /// <summary>
        /// 添加债券
        /// </summary>
        public void AddBond()
        {
            try
            {
                AddBondWindow addBondWind = new AddBondWindow(this.groupID);
                addBondWind.Owner = Application.Current.MainWindow;
                if (addBondWind.ShowDialog() == false)
                {
                    return;
                }

                OrderDetailView odView = new OrderDetailView();
                odView.Owner = Application.Current.MainWindow;
                OrderParamProp orderParam = new OrderParamProp();
                orderParam = RfqCommonUtils.OrderParamPropToDefault(orderParam);
                orderParam.securityid = addBondWind.securityID;
                orderParam.symbol = addBondWind.symbol;
                orderParam.groupid = addBondWind.Groupid;


                OrderDetailViewModel odVM = new OrderDetailViewModel(orderParam);
                odVM.NeedPrompt = false;
                odVM.IsAddBond = true;

                odVM.MyWindow = odView;
                odView.DataContext = odVM;

                if (odView.ShowDialog() == true)
                {

                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }
        /// <summary>
        /// 删除行
        /// </summary>
        public async void DelBond()
        {
            string securityID = SelectedStg.securityid;
            string settlType = SelectedStg.settltype;
            string id = SelectedStg.id;

            RfqstgActReq req = new RfqstgActReq();
            req.action = 3;// 删除
            req.ids = new List<string>();
            RfqstgActStrategyReq request = new RfqstgActStrategyReq();
            request.id = id;
            request.securityid = securityID;
            request.groupid = groupID;
            request.settltype = settlType;
            request.username = LocalEnv.UserInfo.UserName; //用户名
            request.ip = EngineHelper.GetIp();
            request.terminal = LocalEnv.ServerInfo.Version; // 终端
            request.closeby = LocalEnv.UserInfo.UserName; //用户名
            request.remark = "";
            req.strategy = request;


            PResult pResult = await Utils.Data.RfqGroupHandler.Instance.AsyncUpdateSecurityids(req);
            if (pResult != null && pResult.Code == "0000")
            {
                Prompt2Second($"{securityID}删除成功", "删除债券");

                if (WindowManager.Instance.IsExist($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}"))
                {
                    WindowManager.Instance.CloseWindow($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}");
                }

                RfqCommonUtils.ClearSection(securityID);
            }
            else
            {
                CustomMessageBox.Warning(pResult == null ? "删除失败" : pResult.Msg, "删除债券");
            }
        }


        /// <summary>
        /// 修改策略参数
        /// </summary>
        /// <param name="param"></param>
        private void OnConfigCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                RfqstgViewDataModel rf = FullRfqOrder.FirstOrDefault(o => o.id == (string)inparam);
                if (rf != null)
                {
                    string securityID = rf.securityid;
                    OrderParamProp param = JsonConvert.DeserializeObject<OrderParamProp>(rf.inf1.ToString());

                    if (param != null)
                    {
                        param.id = rf.id.ToString();
                        param.enable = CommonUtils.ConvertToString(string.IsNullOrEmpty(rf.enable) ? "0" : rf.enable.ToString());
                        //修改显示参数 防守报价
                        param.bidDefenCalPrice = rf.defBid;
                        param.ofrDefenCalPrice = rf.defOfr;
                        // 墙设置 显示用的
                        param.defWallPriceBid = rf.defWallPriceBid;
                        param.defWallPriceOfr = rf.defWallPriceOfr;
                        // 算法报价显示 - BID价   algoBidPrice
                        // 算法报价显示 - OFR价   algoOfrPrice
                        param.bidAlgoCalPrice = rf.algoBidPrice;
                        param.ofrAlgoCalPrice = rf.algoOfrPrice;

                        param.securityid = rf.securityid;

                        if (!string.IsNullOrEmpty(param.securityid))
                        {
                            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(param.securityid);
                            String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                            if (bondsInfoPo == null)
                            {
                                return;
                            }

                            if (today.CompareTo(bondsInfoPo.Maturity_Date.ToString()) > 0)
                            {
                                return;
                            }

                            if (string.IsNullOrEmpty(param.symbol))
                            {
                                param.symbol = bondsInfoPo.Short_Name;
                            }

                            if (param.algoBaseEnable != "1")
                            {
                                
                                if (RfqCdccHandler.Instance.CcdcDic.ContainsKey(param.securityid))
                                {
                                    if (!string.IsNullOrEmpty(RfqCdccHandler.Instance.CcdcDic[param.securityid].CCDCValuation))
                                    {
                                        DateTime cdate = DateTime.ParseExact(RfqCdccHandler.Instance.CcdcDic[param.securityid].CCDCValuaDate.Replace("-", "").Replace("/", ""), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                        param.algoCDCDetail = $"当前估值({cdate.Month}月{cdate.Day}日):{RfqCdccHandler.Instance.CcdcDic[param.securityid].CCDCValuation}";
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(param.algoBaseSecurityId))
                                {
                                    if (RfqCdccHandler.Instance.CcdcDic.ContainsKey(param.algoBaseSecurityId))
                                    {
                                        CCDCValuaModel srcccdc = RfqCdccHandler.Instance.CcdcDic[param.algoBaseSecurityId];
                                        if (srcccdc != null)
                                        {
                                            DateTime cdate = DateTime.ParseExact(srcccdc.CCDCValuaDate.Replace("-", "").Replace("/", ""), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                            param.algoCDCDetail = $"当前估值{srcccdc.ShowLastWorkDay}({cdate.Month}月{cdate.Day}日):{srcccdc.CCDCValuation}";
                                        }
                                    }
                                }
                            }

                            OrderDetailView odView = new OrderDetailView();

                            OrderDetailViewModel odVM;
                            odVM = new OrderDetailViewModel(param);
                            ////odVM.SelectedItemRow = vm;
                            odVM.MyWindow = odView;

                            odView.DataContext = odVM;
                            odView.ShowDialog();
                        }
                    }

                    await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                }
            }));
        }

        /// <summary>
        /// 启动停止策略
        /// </summary>
        /// <param name="param"></param>
        public void OnEnableCommand(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                RfqstgViewDataModel m2 = FullRfqOrder.FirstOrDefault(o => o.id == (string)param);
                if (m2 != null)
                {
                    string securityID = CommonUtils.ConvertToString(m2.securityid);
                    string settltype = CommonUtils.ConvertToString(m2.settltype);
                    string enable = CommonUtils.ConvertToString(m2.enable);
                    RfqUpdateEnableReq req = new RfqUpdateEnableReq();
                    UpdateEnableModel strategy = new UpdateEnableModel();
                    strategy.securityid = securityID;
                    strategy.symbol = CommonUtils.ConvertToString(m2.symbol);
                    strategy.settltype = settltype;
                    strategy.enable = enable == "1" ? "0" : "1";
                    strategy.ip = EngineHelper.GetIp();
                    strategy.terminal = LocalEnv.ServerInfo.Version; // 终端
                    strategy.username = LocalEnv.UserInfo.UserName; //用户名
                    strategy.closeby = LocalEnv.UserInfo.UserName; //用户名
                    strategy.groupid = groupID;
                    strategy.macid = EngineHelper.GetMacByNetworkInterface();
                    strategy.harddiskid = EngineHelper.Getharddiskid2();
                    req.action = strategy.enable == "1" ? 5 : 6; //5:启动,6:停止
                    req.ids = new List<string>();
                    req.ids.Add(CommonUtils.ConvertToString(m2.id));
                    req.strategy = strategy;

                    PResult result = null;
                    await Task.Run(() =>
                    {
                        result = RfqHandler.Instance.RfqUpdateEnable(req);

                        string promptTitle;

                        if ("1" == strategy.enable)
                        {
                            promptTitle = $"债券策略[{strategy.securityid}]报价开启";
                        }
                        else
                        {
                            promptTitle = $"债券策略[{strategy.securityid}]报价关闭";
                        }

                        if (result == null)
                        {
                            Prompt2Second(promptTitle, "失败");
                        }
                        else
                        {
                            if (result != null && !result.IsSuccess())
                            {
                                Prompt2Second(promptTitle, result.Msg);
                            }
                        }
                    });
                }
            }));
        }

        #endregion

        /// <summary>
        /// 订阅Rfq订单
        /// </summary>
        private void Subscribe()
        {
            if (!IsSubscribe)
            {
                Task.Run(() => {
                    RfqHandler.Instance.SubscribeRfq(Instance_OnGroup);
                    RfqHandler.Instance.SubscribeRfqOrder(OnRfqOrderCallBack);
                    IsSubscribe = true;
                });
            }
        }


        /// <summary>
        /// 订阅回调
        /// </summary>
        /// <param name="riskIndicator"></param>
        private void OnRfqOrderCallBack(QueryRfqOrderResult riskIndicator)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (lockObj)
                {
                    List<ExecutionReport> selfData = new List<ExecutionReport>(riskIndicator.Datas);
                    if (selfData.Count == 0)
                    {
                        return;
                    }

                    foreach (ExecutionReport pbm in selfData)
                    {
                        if (pbm.UserName == LocalEnv.UserInfo.UserName)
                        {
                            if (pbm.QuoteStatus == "33" || pbm.QuoteStatus == "34" || pbm.QuoteStatus == "35" || pbm.QuoteStatus == "40" || pbm.QuoteStatus == "44")
                            {
                                lock (lockObj)
                                {
                                    RfqstgViewDataModel m2 = FullRfqOrder.FirstOrDefault(o => o.securityid == pbm.SecurityID && o.settltype == pbm.SettlType);
                                    if (m2 != null)
                                    {
                                        if (pbm.Side == "1")
                                        {
                                            //BId
                                            string myLstBidPrice = CommonUtils.ConvertToString(pbm.Price);
                                            m2.MyBid = CommonUtils.ConvertToDecimal(myLstBidPrice);

                                            string myLstBidQty = CommonUtils.ConvertToString(pbm.OrderQty);
                                            m2.MyBidQty = CommonUtils.ConvertToDecimal(myLstBidQty) / 1000;

                                            m2.updatetime = CommonUtils.ConvertToDateTime(pbm.UpdateTime).ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                        else if (pbm.Side == "2")
                                        {
                                            //Ofr
                                            string myLstOfrPrice = CommonUtils.ConvertToString(pbm.Price);
                                            m2.MyOfr = CommonUtils.ConvertToDecimal(myLstOfrPrice);

                                            string myLstOfrQty = CommonUtils.ConvertToString(pbm.OrderQty);
                                            m2.MyOfrQty = CommonUtils.ConvertToDecimal(myLstOfrQty) / 1000;

                                            m2.updatetime = CommonUtils.ConvertToDateTime(pbm.UpdateTime).ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }));

        }

        /// <summary>
        /// Rfq策略列表组变化回调
        /// </summary>
        /// <param name="mm"></param>
        public void Instance_OnGroup(RfqstgViewModel mm)
        {
            if (mm.groupid == this.groupID)
            {
                List<RfqstgViewModel> rf = new List<RfqstgViewModel>();
                rf.Add(mm);
                OnRfqstgViewCallBack(rf);
            }
        }

        /// <summary>
        /// Rfq策略列表处理
        /// </summary>
        private void OnRfqstgViewCallBack(List<RfqstgViewModel> mmList)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    lock (lockObj)
                    {
                        foreach (RfqstgViewModel pbm in mmList)
                        {
                            if (pbm.groupid == this.groupID)
                            {
                                RfqstgViewDataModel m2 = FullRfqOrder.FirstOrDefault(o => o.id == pbm.id);
                                if (pbm.action == "3")
                                {
                                    if (m2 != null)
                                    {
                                        FullRfqOrder.Remove(m2);
                                        return;
                                    }
                                }
                                else
                                {
                                    OrderParamProp param = JsonConvert.DeserializeObject<OrderParamProp>(pbm.inf1);

                                    if (m2 != null)
                                    {
                                        m2.enable = pbm.enable;
                                        m2.updatetime = pbm.updatetime;

                                        m2.settltype = pbm.settltype;
                                        m2.validSide = pbm.validSide;
                                        m2.actCancel = pbm.actCancel;
                                        m2.algoBaseEnable = pbm.algoBaseEnable;
                                        m2.algoBaseSecurityId = pbm.algoBaseSecurityId;
                                        m2.algoBidPrice = param.algoBidPrice;
                                        m2.algoDeviation = param.algoDeviation;
                                        m2.algoMidDetail = param.algoMidDetail;
                                        m2.algoMidPrice = param.algoMidPrice;
                                        m2.algoMidPriceType = param.algoMidPriceType;
                                        m2.algoOfrPrice = param.algoOfrPrice;
                                        m2.algoSpread = param.algoSpread;
                                        m2.apiBid = param.apiBid;
                                        m2.apiBidQty = param.apiBidQty;
                                        m2.apiOfr = param.apiOfr;
                                        m2.apiOfrQty = param.apiOfrQty;
                                        m2.apiStatus = param.apiStatus;
                                        m2.autoReply = param.autoReply;
                                        m2.bidAlgoCalPrice = param.bidAlgoCalPrice;
                                        m2.bidDefenCalPrice = param.bidDefenCalPrice;
                                        m2.closeby = pbm.closeby;
                                        m2.updatetime = pbm.updatetime;
                                        m2.validDateBid = string.IsNullOrEmpty(pbm.validDateBid) ? "" : pbm.validDateBid;
                                        m2.validDateOfr = string.IsNullOrEmpty(pbm.validDateOfr) ? "" : pbm.validDateOfr;
                                        m2.strategyTypeBid = param.strategyTypeBid;
                                        m2.strategyTypeOfr = param.strategyTypeOfr;
                                        m2.resendThresholdBid = param.resendThresholdBid;
                                        m2.resendThresholdOfr = param.resendThresholdOfr;

                                        m2.validSide = pbm.validSide;
                                        m2.inf1 = pbm.inf1;
                                        m2.inf2 = pbm.inf2;

                                        if (param.algoBaseEnable == "1")
                                        {
                                            if (!string.IsNullOrEmpty(param.algoBaseSecurityId))
                                            {
                                                RfqCdccHandler.Instance.SubscribeCCDC(param.algoBaseSecurityId, this.groupID, OnCCDCEvent);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        RfqstgViewDataModel dr = Newtonsoft.Json.JsonConvert.DeserializeObject<RfqstgViewDataModel>(Newtonsoft.Json.JsonConvert.SerializeObject(pbm));
                                        dr.updatetime = pbm.updatetime;
                                        dr.algoBidPrice = param.algoBidPrice;
                                        dr.algoDeviation = param.algoDeviation;
                                        dr.algoMidDetail = param.algoMidDetail;
                                        dr.algoMidPrice = param.algoMidPrice;
                                        dr.algoMidPriceType = param.algoMidPriceType;
                                        dr.algoOfrPrice = param.algoOfrPrice;
                                        dr.algoSpread = param.algoSpread;
                                        dr.apiBid = param.apiBid;
                                        dr.apiBidQty = param.apiBidQty;
                                        dr.apiOfr = param.apiOfr;
                                        dr.apiOfrQty = param.apiOfrQty;
                                        dr.apiStatus = param.apiStatus;
                                        dr.autoReply = param.autoReply;
                                        dr.bidAlgoCalPrice = param.bidAlgoCalPrice;
                                        dr.bidDefenCalPrice = param.bidDefenCalPrice;

                                        dr.validDateBid = string.IsNullOrEmpty(param.validDateBid)? "" : param.validDateBid;
                                        dr.validDateOfr = string.IsNullOrEmpty(param.validDateOfr)? "" : param.validDateOfr;
                                        dr.strategyTypeBid = param.strategyTypeBid;
                                        dr.strategyTypeOfr = param.strategyTypeOfr;
                                        dr.resendThresholdBid = param.resendThresholdBid;
                                        dr.resendThresholdOfr = param.resendThresholdOfr;

                                        BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(param.securityid);
                                        if (bondsInfoPo != null)
                                        {
                                            dr.symbol = bondsInfoPo.Short_Name; // 代码名称
                                            if (bondsInfoPo.MarketIndicator == "4")
                                            {
                                                String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                                                if (today.CompareTo(bondsInfoPo.Maturity_Date.ToString()) > 0)
                                                {
                                                    dr.RemainingTime = "不可用";//剩余时间
                                                }
                                                else
                                                {
                                                    dr.RemainingTime = bondsInfoPo.RemaingingTime;//剩余时间
                                                }

                                                dr.RemaingingTime_convert = ConvertRemainingTime(bondsInfoPo.RemaingingTime); //剩余时间转换成时间段
                                                dr.BondType = CommonUtils.GetBondCategoryName(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor); // 债券类型 
                                            }
                                            else
                                            {
                                                dr.RemainingTime = "不可用";//剩余时间
                                                dr.BondType = string.Empty;
                                            }

                                            dr.Publisher = bondsInfoPo.CN_Short_Name;//发行人
                                            dr.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY; // 评级
                                            dr.BondSubType = bondsInfoPo.Bond_Subtype; //债券子类
                                            dr.GoodsType = bondsInfoPo.SECURITY_TYPE;  //债券大类
                                        }
                                        else
                                        {
                                            dr.symbol = string.Empty; // 代码名称
                                            dr.RemainingTime = "不可用";//剩余时间
                                            dr.RemaingingTime_convert = 0;
                                            dr.BondType = string.Empty;
                                            dr.Publisher = string.Empty;//发行人
                                            dr.IssuerRatingCurrentNPY = string.Empty; // 评级
                                            dr.BondSubType = string.Empty; //债券子类
                                            dr.GoodsType = string.Empty; //债券大类
                                        }

                                        if (RfqCdccHandler.Instance.CcdcDic.ContainsKey(param.securityid))
                                        {
                                            if (!string.IsNullOrEmpty(RfqCdccHandler.Instance.CcdcDic[param.securityid].CCDCValuation))
                                            {
                                                dr.Valuation = RfqCdccHandler.Instance.CcdcDic[param.securityid].CCDCValuation;
                                            }
                                        }

                                        this.FullRfqOrder.Add(dr);
                                        

                                        RfqCdccHandler.Instance.SubscribeCCDC(param.securityid, this.groupID, OnCCDCEvent);

                                        if (param.algoBaseEnable == "1")
                                        {
                                            if (!string.IsNullOrEmpty(param.algoBaseSecurityId))
                                            {
                                                RfqCdccHandler.Instance.SubscribeCCDC(param.algoBaseSecurityId, this.groupID, OnCCDCEvent);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        private void OnCCDCEvent(CCDCValuaModel obj)
        {
            string secid = obj.CCDCValuationShow;
            if (!string.IsNullOrEmpty(obj.CCDCValuation))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    List<RfqstgViewDataModel> lstdr = FullRfqOrder.ToList().FindAll(o => o.securityid == secid);
                    foreach (RfqstgViewDataModel dr in lstdr)
                    {
                        if (dr.Valuation == null)
                        {
                            RfqstgViewDataModel m2 = FullRfqOrder.FirstOrDefault(o => o.id == dr.id);
                            m2.Valuation = obj.CCDCValuation;
                            m2.CCDCValuaDate = obj.CCDCValuaDate;
                        }
                    }
                }));
            }
        }

     
        #region 导入导出

        #region 导入Excel
        /// <summary>
        /// 导入Excel
        /// </summary>
        private async void ImportExcel()
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
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导入";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }

                    // 2.导入操作
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
                        promptTxt = $"导入成功!";
                    }
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
                    //CustomMessageBox.Info(promptTxt, "批量导入");
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导入";
                prompt.PromptContext = e.Message;
                prompt.Show();
                //CustomMessageBox.Info(promptTxt, "批量导入");

                LogHelper.Error(e);
            }
        }


        private async Task<List<string>> Sub_ImportExcel(string filePath)
        {
            List<string> errorlist = new List<string>();
            await Task.Run(() =>
            {
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);

                    if (isFileLock)
                    {
                        errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                        return;
                    }

                    Dictionary<int, string> dic = helper.ReturnSheetList();
                    if (dic.Count > 20)
                    {
                        errorlist.Add("分组总数应不超过20个");
                        return;
                    }

                    foreach (KeyValuePair<int, string> pair in dic)
                    {
                        DataTable importDT = helper.ExcelToDataTable(pair.Key);
                        int validCount = 0;

                        if (importDT != null && importDT.Rows.Count > 0)
                        {
                            List<RfqExcelDataModel> LstAllPreImport = JsonConvert.DeserializeObject<List<RfqExcelDataModel>>(JsonConvert.SerializeObject(importDT));
                            List<RfqstgViewModel> LstFullStg = new List<RfqstgViewModel>();
                            foreach (List<RfqstgViewModel> x in RfqGroupHandler.Instance._CacheMMListDic.Values)
                            {
                                LstFullStg.AddRange(x);
                            }

                            //组合inf2
                            Dictionary<string, RfqstgViewModel> DictFullStg = new Dictionary<string, RfqstgViewModel>();
                            foreach (RfqstgViewModel rs in LstFullStg)
                            {
                                rs.inf2 = $"{rs.securityid}#{rs.settltype}";
                                DictFullStg.Add(rs.inf2, rs);
                            }
                            //准备合并的
                            List<RfqstgViewModel> LstPreIns = JsonConvert.DeserializeObject<List<RfqstgViewModel>>(JsonConvert.SerializeObject(LstAllPreImport));
                            List<RfqstgViewModel> LstIns = new List<RfqstgViewModel>();
                            foreach (RfqstgViewModel rs in LstPreIns)
                            {
                                string key = $"{rs.securityid}#{rs.settltype}";
                                if (!DictFullStg.ContainsKey(key))
                                {
                                    LstIns.Add(rs);
                                }
                            }

                            if (LstIns.Count == 0)
                            {
                                errorlist.Add("无可导入债券！");
                                return;
                            }
                            else
                            {
                                validCount = LstIns.Count;

                                string promptTxt = string.Empty;

                                List<string> errorList = Sub_ImportStg(LstIns);

                            }
                        }

                        break;
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

            });

            return errorlist;
        }

        private List<string> Sub_ImportStg(List<RfqstgViewModel> stglst)
        {
            List<string> errorlist = new List<string>();
            string errorMsg = string.Empty;
            foreach (RfqstgViewModel rs in stglst)
            {
                RfqstgActReq req = new RfqstgActReq();
                req.action = 1;
                req.ids = new List<string>();
                OrderParamProp Data = JsonConvert.DeserializeObject<OrderParamProp>(JsonConvert.SerializeObject(rs));
                RfqstgActStrategyReq rfv = JsonConvert.DeserializeObject<RfqstgActStrategyReq>(JsonConvert.SerializeObject(Data));
                rfv.groupid = this.groupID;
                //如果BID方向有效日期，OFR方向有效日期 为 null ，则改为 "";
                rfv.validDateBid = string.IsNullOrEmpty(rfv.validDateBid) ? "" : rfv.validDateBid;
                rfv.validDateOfr = string.IsNullOrEmpty(rfv.validDateOfr) ? "" : rfv.validDateOfr;
                rfv.inf1 = JsonConvert.SerializeObject(Data);
                rfv.username = LocalEnv.UserInfo.UserName; //用户名
                rfv.ip = EngineHelper.GetIp();
                rfv.terminal = LocalEnv.ServerInfo.Version; // 终端
                rfv.closeby = LocalEnv.UserInfo.UserName; //用户名
                rfv.remark = "";
                req.strategy = rfv;

                PResult result = RfqHandler.Instance.RfqUpdatestgAct(req);
                if (result != null && result.Code == "0000")
                {

                }
                else
                {
                    if (result != null && !string.IsNullOrEmpty(result.Msg))
                    {
                        errorlist.Add($"{rs.securityid} 导入失败! 原因：{result.Msg}");
                    }
                    else
                    {
                        errorlist.Add($"{rs.securityid} 导入失败! 原因：服务器接收数据失败！");
                    }
                }
            }

            return errorlist;
        }

        #endregion


        #region 导出Excel
        /// <summary>
        /// 导出Excel
        /// </summary>
        private async void ExportExcel()
        {
            try
            {
                //创建一个保存文件式的对话框
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
                sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.RestoreDirectory = true;
                sfd.FileName = $"RFQ策略导出_{DateTime.Now.ToString("yyyy_MM_dd")}";
                //设置保存的文件的类型，注意过滤器的语法
                sfd.Filter = "Excel Documents|*.xls;*.xlsx;";
                sfd.DefaultExt = ".xlsx";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                string promptTxt = string.Empty;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!sfd.FileName.EndsWith(".xls", true, System.Globalization.CultureInfo.CurrentCulture)
                        && !sfd.FileName.EndsWith(".xlsx", true, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Prompt2Seconds prompt = new Prompt2Seconds();
                        prompt.Owner = Application.Current.MainWindow;
                        prompt.PromptTitle = "批量导出";
                        prompt.PromptContext = "当前只支持xls文件和xlsx文件";
                        prompt.Show();
                        return;
                    }


                    List<string> errorList = await Sub_ExportExcel(sfd.FileName);

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
                        promptTxt = "导出成功";
                    }
                }
                else
                {
                    promptTxt = "取消导出";
                }

                if (string.IsNullOrEmpty(promptTxt) == false)
                {
                    Prompt2Seconds prompt = new Prompt2Seconds();
                    prompt.Owner = Application.Current.MainWindow;
                    prompt.PromptTitle = "批量导出";
                    prompt.PromptContext = promptTxt;
                    prompt.Show();
                    //CustomMessageBox.Info(promptTxt, "批量导入");
                }
            }
            catch (Exception e)
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.Owner = Application.Current.MainWindow;
                prompt.PromptTitle = "批量导出";
                prompt.PromptContext = e.Message;
                prompt.Show();
                //CustomMessageBox.Info(promptTxt, "批量导入");

                LogHelper.Error(e);
            }
        }

        private async Task<List<string>> Sub_ExportExcel(string filePath)
        {
            List<string> errorlist = new List<string>();
            await Task.Run(() =>
            {
                STC.Frame.Utils.ExcelHelper.ExcelHelper helper = null;
                try
                {
                    // 1 检查文件是否被占用
                    if (File.Exists(filePath))
                    {
                        bool isFileLock = STC.Frame.Utils.ExcelHelper.ExcelHelper.CheckFileIsLock(filePath);
                        if (isFileLock)
                        {
                            errorlist.Add($"{filePath} 被占用，请关闭后再试。");
                            return errorlist;
                        }

                        File.Delete(filePath);
                    }


                    helper = new STC.Frame.Utils.ExcelHelper.ExcelHelper(filePath);
                    if (RfqGroupHandler.Instance.CacheSortedGroups != null)
                    {
                        Dictionary<string, DataTable> dataDic = new Dictionary<string, DataTable>();
                        foreach (RfqGroupInfo item in RfqGroupHandler.Instance.CacheSortedGroups)
                        {
                            if (item.id == this.groupID)
                            {
                                DataTable dt = GenerateExcelDataTable(item.id);
                                dataDic[item.groupname] = dt;
                            }
                        }

                        Dictionary<string, int> result = helper.DataTableToExcel(dataDic, true);
                        if (result == null) errorlist.Add("无数据导出");
                    }
                    else
                    {
                        errorlist.Add("无数据导出");
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

            return errorlist;
        }


        private DataTable GenerateExcelDataTable(string groupid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("代码");
            dt.Columns.Add("简称");
            dt.Columns.Add("清算速度");
            dt.Columns.Add("有效方向");

            dt.Columns.Add("Bid策略类型");
            dt.Columns.Add("Bid预设回量");
            dt.Columns.Add("Bid有效日期");
            dt.Columns.Add("Bid有效时长");
            dt.Columns.Add("Bid重发阈值");
            dt.Columns.Add("Bid回量上限");
            dt.Columns.Add("Bid手动报价");
            dt.Columns.Add("Bid防守点");
            dt.Columns.Add("Bid全市场最优");
            dt.Columns.Add("Bid触墙拉宽");

            dt.Columns.Add("Ofr策略类型");
            dt.Columns.Add("Ofr预设回量");
            dt.Columns.Add("Ofr有效日期");
            dt.Columns.Add("Ofr有效时长");
            dt.Columns.Add("Ofr重发阈值");
            dt.Columns.Add("Ofr回量上限");
            dt.Columns.Add("Ofr手动报价");
            dt.Columns.Add("Ofr防守点");
            dt.Columns.Add("Ofr全市场最优");
            dt.Columns.Add("Ofr触墙拉宽");

            dt.Columns.Add("参考中间价");
            dt.Columns.Add("自定义"); 
            dt.Columns.Add("是否使用基准券");
            dt.Columns.Add("基准券");

            dt.Columns.Add("算法偏离");
            dt.Columns.Add("算法点差");

            dt.Columns.Add("成交达成后不再自动回复");

            foreach (RfqstgViewDataModel row in FullRfqOrder)
            {
                OrderParamProp param = JsonConvert.DeserializeObject<OrderParamProp>(row.inf1.ToString());
                DataRow newDR = dt.NewRow();
                newDR["代码"] = param.securityid;
                newDR["简称"] = param.symbol;
                newDR["清算速度"] = RfqGroupHandler.Instance.BaseSettleTypeList.ContainsKey(param.settltype) ? RfqGroupHandler.Instance.BaseSettleTypeList[param.settltype] : "2";
                newDR["有效方向"] = RfqGroupHandler.Instance.ValidSideList.ContainsKey(param.validSide) ? RfqGroupHandler.Instance.ValidSideList[param.validSide] : "All";
                newDR["Bid策略类型"] = RfqGroupHandler.Instance.StrategyModeList.ContainsKey(param.strategyTypeBid) ? RfqGroupHandler.Instance.StrategyModeList[param.strategyTypeBid] : "Manual";
                newDR["Bid预设回量"] = param.quantityBid;
                newDR["Bid有效日期"] = param.validDateBid;
                newDR["Bid有效时长"] = param.quoteValidUntilTimeBid;
                newDR["Bid重发阈值"] = param.resendThresholdBid;
                newDR["Bid回量上限"] = param.maxQtyBid;
                newDR["Bid手动报价"] = param.manualBid;
                newDR["Bid防守点"] = param.defBid;
                newDR["Bid全市场最优"] = param.defMaxBid;
                newDR["Bid触墙拉宽"] = param.defWallBid;

                newDR["Ofr策略类型"] = RfqGroupHandler.Instance.StrategyModeList.ContainsKey(param.strategyTypeOfr) ? RfqGroupHandler.Instance.StrategyModeList[param.strategyTypeOfr] : "Manual";
                newDR["Ofr预设回量"] = param.quantityOfr;
                newDR["Ofr有效日期"] = param.validDateOfr;
                newDR["Ofr有效时长"] = param.quoteValidUntilTimeOfr;
                newDR["Ofr重发阈值"] = param.resendThresholdOfr;
                newDR["Ofr回量上限"] = param.maxQtyOfr;
                newDR["Ofr手动报价"] = param.manualOfr;
                newDR["Ofr防守点"] = param.defOfr;
                newDR["Ofr全市场最优"] = param.defMaxOfr;
                newDR["Ofr触墙拉宽"] = param.defWallOfr;

                // algoMidType 1 全市场中间价，2：中债估值，3：自定义 ""为null  algoMidPrice
                newDR["参考中间价"] = string.IsNullOrEmpty(param.algoMidPriceType) ? null : RfqGroupHandler.Instance.AlgoMidTypeList.ContainsKey(param.algoMidPriceType) ? RfqGroupHandler.Instance.AlgoMidTypeList[param.algoMidPriceType] : null;

                newDR["自定义"] = param.algoMidPrice;
                //是否使用基准债券0：否、1：是
                newDR["是否使用基准券"] = string.IsNullOrEmpty(param.algoBaseEnable) ? "0" : param.algoBaseEnable;
                newDR["基准券"] = param.algoBaseSecurityId;
                newDR["算法偏离"] = param.algoDeviation;
                newDR["算法点差"] = param.algoSpread;
                //0：不回复，1：回复
                newDR["成交达成后不再自动回复"] = param.autoReply;
                dt.Rows.Add(newDR);
            }

            return dt;
        }
        #endregion

        #endregion

        #region Tools

        // <summary>
        /// 把基础剩余时间转换成时间段
        /// 如：2Y+5D -> 2 * 365
        /// </summary>
        /// <param name="remainingTime">基础剩余时间</param>
        /// <returns>筛选时间段</returns>
        private int ConvertRemainingTime(string remainingTime)
        {
            string remainingTimeLevel = string.Empty;
            int convertTime = 0;
            if (string.IsNullOrEmpty(remainingTime) == false)
            {
                string[] splitArr = remainingTime.Split(CommonConstants.PLUS_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr != null && splitArr.Length > 0)
                {
                    for (int i = 0; i < splitArr.Length; i++)
                    {
                        string data;
                        data = splitArr[i].Trim();
                        if (data.EndsWith("Y"))
                        {
                            string yearStr = data.Replace("Y", "");
                            decimal year = CommonUtils.ConvertToDecimal(yearStr);
                            convertTime = convertTime + (int)(365 * year);
                        }
                        else if (data.EndsWith("D"))
                        {
                            string dayStr = data.Replace("D", "");
                            int day = CommonUtils.ConvertToInt(dayStr);
                            convertTime = convertTime + day;
                        }
                    }
                }
            }

            return convertTime;
        }

        #endregion

        #region  UI 
        private void Prompt2Second(string title, string text)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Sub_Prompt2Second(title, text);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Sub_Prompt2Second(title, text);
                }));
            }
        }

        private void Sub_Prompt2Second(string title, string text)
        {
            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.PromptTitle = title;
            prompt.PromptContext = text;
            prompt.Owner = Application.Current.MainWindow;
            prompt.Show();
        }
        #endregion
    }
}
