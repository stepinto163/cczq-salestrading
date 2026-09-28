using com.gateway.log;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Risk;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Risk;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel.Risk
{
    /// <summary>
    /// 风控指标RskIndexView的数据内容
    /// </summary>
    public class RiskIndexViewModel : BaseViewModel
    {
        private string FormatTxt = "/{0}";
        private string FormatLimit = "/{0},{1}";
        private readonly string DEFAULTTXT = "--";
        private readonly string HUNDRED = "100";
        private FrameworkElement _ownUI = null;
        private readonly string BOOK_SOURCE = "内部系统";

        private AsyncThreadQueue<RiskIndicator> _riskDataTask = null;
        private object _lockObj = new object();
        private string _currentTopic = string.Empty;
        private bool _isInit = true;

        #region 属性

        private List<StcTbookUsers> _userDatas;
        public List<StcTbookUsers> UserDatas
        {
            get
            {
                //if (_userDatas == null || _userDatas.Count == 0)
                //    _userDatas = AdminHandler.Instance.GetStcTbookUsersByUser(LocalEnv.UserInfo.UserId);

                if (_userDatas == null)
                    _userDatas = new List<StcTbookUsers>();

                return _userDatas;
            }
        }
        

        private StcTbookUsers _selectedUser;
        public StcTbookUsers SelectedUser { get { return _selectedUser; } set { _selectedUser = value; } }
        
        private RiskLimitViewModel _bncRiskLimitDV01 = new RiskLimitViewModel();
        private RiskLimitViewModel _bncRiskLimitScope = new RiskLimitViewModel();

        public RiskLimitViewModel BncRiskLimitDV01 { get { return _bncRiskLimitDV01; } }
        public RiskLimitViewModel BncRiskLimitScope { get { return _bncRiskLimitScope; } }

        private string _source;
        public string Source { get { return _source; } set { _source = value; this.OnPropertyChanged(); } }

        private string _longPosLimit;//利率债-多头合计净头寸
        public string LongPosLimit { get { return _longPosLimit; } set { _longPosLimit = value; this.OnPropertyChanged(); } }

        private string _longPosLimitSettl;////利率债-多头合计结算头寸
        public string LongPosLimitSettl { get { return _longPosLimitSettl; } set { _longPosLimitSettl = value; this.OnPropertyChanged(); } }

        private string _shortPosLimit;//利率债-空头合计净头寸
        public string ShortPosLimit { get { return _shortPosLimit; } set { _shortPosLimit = value; this.OnPropertyChanged(); } }

        private string _shortPosLimitSettl;//利率债-空头合计结算头寸
        public string ShortPosLimitSettl { get { return _shortPosLimitSettl; } set { _shortPosLimitSettl = value; this.OnPropertyChanged(); } }

        private string _maxReDv01Settl;//利率债-风险敞口结算头寸DV01最大值
        public string MaxReDv01Settl { get { return _maxReDv01Settl; } set { _maxReDv01Settl = value; this.OnPropertyChanged(); } }


        private string _minReDv01Settl;//利率债-风险敞口结算头寸DV01最小值
        public string MinReDv01Settl { get { return _minReDv01Settl; } set { _minReDv01Settl = value; this.OnPropertyChanged(); } }

        private string _maxReDv01;//利率债-风险敞口净头寸DV01最大值用户
        public string MaxReDv01 { get { return _maxReDv01; } set { _maxReDv01 = value; this.OnPropertyChanged(); } }


        private string _minReDv01;//利率债-风险敞口净头寸DV01最小值
        public string MinReDv01 { get { return _minReDv01; } set { _minReDv01 = value; this.OnPropertyChanged(); } }

        private string _maxDv01Settl;//利率债-债券结算头寸DV01最大值
        public string MaxDv01Settl { get { return _maxDv01Settl; } set { _maxDv01Settl = value; this.OnPropertyChanged(); } }

        private string _minDv01Settl;//利率债-债券结算头寸DV01最小值
        public string MinDv01Settl { get { return _minDv01Settl; } set { _minDv01Settl = value; this.OnPropertyChanged(); } }


        private string _maxDv01;//利率债-债券净头寸DV01最大值
        public string MaxDv01 { get { return _maxDv01; } set { _maxDv01 = value; this.OnPropertyChanged(); } }

        private string _minDv01;//利率债-债券净头寸DV01最小值
        public string MinDv01 { get { return _minDv01; } set { _minDv01 = value; this.OnPropertyChanged(); } }

        private string _creditDv01;//信用债dv01结算
        public string CreditDv01 { get { return _creditDv01; } set { _creditDv01 = value; this.OnPropertyChanged(); } }

        private string _creditLimit;//信用债规模结算
        public string CreditLimit { get { return _creditLimit; } set { _creditLimit = value; this.OnPropertyChanged(); } }

        private string _irsLimit;//irs名义本金限额
        public string IrsLimit { get { return _irsLimit; } set { _irsLimit = value; this.OnPropertyChanged(); } }

        private string _bondFutsPosLimit;//国债期货限额
        public string BondFutsPosLimit { get { return _bondFutsPosLimit; } set { _bondFutsPosLimit = value; this.OnPropertyChanged(); } }


        private string _totalPosLimit;//总限额
        public string TotalPosLimit { get { return _totalPosLimit; } set { _totalPosLimit = value; this.OnPropertyChanged(); } }


        public string _longPosRealTime;//利率债-多头合计净头寸实时值
        public string LongPosRealTime { get { return _longPosRealTime; } set { _longPosRealTime = value; this.OnPropertyChanged(); } }

        public string _longPosRealTimeSettl;//利率债-多头合计结算头寸实时值
        public string LongPosRealTimeSettl { get { return _longPosRealTimeSettl; } set { _longPosRealTimeSettl = value; this.OnPropertyChanged(); } }

        public string _shortPosRealTime;//利率债-空头合计净头寸实时值
        public string ShortPosRealTime { get { return _shortPosRealTime; } set { _shortPosRealTime = value; this.OnPropertyChanged(); } }

        public string _shortPosRealTimeSettl;//利率债-空头合计结算头寸实时值
        public string ShortPosRealTimeSettl { get { return _shortPosRealTimeSettl; } set { _shortPosRealTimeSettl = value; this.OnPropertyChanged(); } }

        public string _posRealTime;//利率债-净仓位合计净头寸实时值
        public string PosRealTime { get { return _posRealTime; } set { _posRealTime = value; this.OnPropertyChanged(); } }

        public string _posRealTimeSettl;//利率债-净仓位合计结算头寸实时值
        public string PosRealTimeSettl { get { return _posRealTimeSettl; } set { _posRealTimeSettl = value; this.OnPropertyChanged(); } }

        public string _lsPosRealTime;//利率债-多空仓位合计净头寸实时值
        public string LsPosRealTime { get { return _lsPosRealTime; } set { _lsPosRealTime = value; this.OnPropertyChanged(); } }

        public string _lsPosRealTimeSettl;//利率债-多空仓位合计结算头寸实时值
        public string LsPosRealTimeSettl { get { return _lsPosRealTimeSettl; } set { _lsPosRealTimeSettl = value; this.OnPropertyChanged(); } }

        public string _reDv01SettlRealTime;//利率债-风险敞口结算头寸DV01实时值
        public string ReDv01SettlRealTime { get { return _reDv01SettlRealTime; } set { _reDv01SettlRealTime = value; this.OnPropertyChanged(); } }

        public string _reDv01RealTime;//利率债-风险敞口净头寸DV01实时值
        public string ReDv01RealTime { get { return _reDv01RealTime; } set { _reDv01RealTime = value; this.OnPropertyChanged(); } }

        public string _dv01SettlRealTime;//利率债-债券结算头寸DV01实时值
        public string Dv01SettlRealTime { get { return _dv01SettlRealTime; } set { _dv01SettlRealTime = value; this.OnPropertyChanged(); } }

        public string _dv01RealTime;//利率债-债券净头寸DV01实时值
        public string Dv01RealTime { get { return _dv01RealTime; } set { _dv01RealTime = value; this.OnPropertyChanged(); } }

        public string _creditDv01RealTime;//信用债dv01实时值
        public string CreditDv01RealTime { get { return _creditDv01RealTime; } set { _creditDv01RealTime = value; this.OnPropertyChanged(); } }

        public string _creditRealTime;//信用债规模实时值
        public string CreditRealTime { get { return _creditRealTime; } set { _creditRealTime = value; this.OnPropertyChanged(); } }


        #endregion

        public void Clear()
        {
            if (_riskDataTask != null)
            {
                _riskDataTask.Clear();
                _riskDataTask.Dispose();
            }

            RiskHandler.Instance.UnSubscribe(OnRiskData);
        }

        public void InitData()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    AdminHandler.Instance.stcUserAndGroupsQuery();

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ComboBox comboBox = _ownUI.FindName("cbUser") as ComboBox;
                        if (comboBox != null)
                        {
                            if (UserDatas != null)
                                _userDatas.Clear();

                            _userDatas = AdminHandler.Instance.GetStcTbookUsersByUser(LocalEnv.UserInfo.UserId);

                            comboBox.ItemsSource = null;
                            comboBox.ItemsSource = UserDatas;

                            if (UserDatas != null && UserDatas.Count > 0)
                            {
                                comboBox.SelectedIndex = 0;
                            }

                            SubscribeRisk();
                        }
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("RiskIndexViewModel InitData", e);
                }
            });
        }

        public void Init(FrameworkElement riskView)
        {
            InitData();

            _ownUI = riskView;
            if (_riskDataTask == null)
                _riskDataTask = new AsyncThreadQueue<RiskIndicator>(Process);
            else
                _riskDataTask.Clear();

            this.Source = BOOK_SOURCE;
            _bncRiskLimitDV01.HeadTitle = "风险敞口DV01";
            _bncRiskLimitScope.HeadTitle = "债券规模";

            //风险敞口DV01
            if (_bncRiskLimitDV01.SettleRisk == null)
            {
                
                _bncRiskLimitDV01.SettleRisk = new RiskLimitDetailViewModel();
                _bncRiskLimitDV01.SettleRisk.RiskTitle = "结算";
                _bncRiskLimitDV01.SettleRisk.Source = BOOK_SOURCE;

                _bncRiskLimitDV01.SettleRisk.ProcessBgBrush = (Brush)_ownUI.FindResource("ColorBrush.Gray13");
                _bncRiskLimitDV01.SettleRisk.CursorBrush = (Brush)_ownUI.FindResource("ColorBrush.Green14");
            }
            if (_bncRiskLimitDV01.NetPositionRisk == null)
            {
                
                _bncRiskLimitDV01.NetPositionRisk = new RiskLimitDetailViewModel();
                _bncRiskLimitDV01.NetPositionRisk.RiskTitle = "净头寸";
                _bncRiskLimitDV01.NetPositionRisk.Source = BOOK_SOURCE;

                _bncRiskLimitDV01.NetPositionRisk.ProcessBgBrush = (Brush)_ownUI.FindResource("ColorBrush.Gray13");
                _bncRiskLimitDV01.NetPositionRisk.CursorBrush = (Brush)_ownUI.FindResource("ColorBrush.Green14");
            }

            //债券DV01 //债券规模
            if (_bncRiskLimitScope.SettleRisk == null)
            {
                _bncRiskLimitScope.SettleRisk = new RiskLimitDetailViewModel();
                _bncRiskLimitScope.SettleRisk.RiskTitle = "结算";
                _bncRiskLimitScope.SettleRisk.Source = BOOK_SOURCE;

                _bncRiskLimitScope.SettleRisk.ProcessBgBrush = (Brush)_ownUI.FindResource("ColorBrush.Gray13");
                _bncRiskLimitScope.SettleRisk.CursorBrush = (Brush)_ownUI.FindResource("ColorBrush.Green14");
            }
            if (_bncRiskLimitScope.NetPositionRisk == null)
            {
                _bncRiskLimitScope.NetPositionRisk = new RiskLimitDetailViewModel();
                _bncRiskLimitScope.NetPositionRisk.RiskTitle = "净头寸";
                _bncRiskLimitScope.NetPositionRisk.Source = BOOK_SOURCE;

                _bncRiskLimitScope.NetPositionRisk.ProcessBgBrush = (Brush)_ownUI.FindResource("ColorBrush.Gray13");
                _bncRiskLimitScope.NetPositionRisk.CursorBrush = (Brush)_ownUI.FindResource("ColorBrush.Green14");
            }

            //if (UserDatas != null && UserDatas.Count > 0)
            //{
            //    ComboBox comboBox = _ownUI.FindName("cbUser") as ComboBox;
            //    comboBox.SelectedIndex = 0;
            //}
            //SubscribeRisk();
        }

        private void SubscribeRisk()
        {
            string topic = string.Empty;
            if (UserDatas != null && UserDatas.Count > 0 && SelectedUser != null)
            {
                if (!string.IsNullOrEmpty(SelectedUser.IsUsers))
                    topic = string.Format("{0}G{1}", ApiConsts.Topic_stc_risk_prefix, SelectedUser.Name);
                else
                    topic = string.Format("{0}{1}", ApiConsts.Topic_stc_risk_prefix, LocalEnv.UserInfo.UserName);
            }
            else
            {
                topic = string.Format("{0}{1}", ApiConsts.Topic_stc_risk_prefix, LocalEnv.UserInfo.UserName);
            }
            _currentTopic = topic;
            ThreadPool.QueueUserWorkItem(o =>
            {
                RiskHandler.Instance.Subscribe(OnRiskData, topic);

                _isInit = false;
            });
        }

        public void ChangeSubscribe()
        {
            if (_isInit)
                return;

            RiskHandler.Instance.UnSubscribe(OnRiskData, _currentTopic);

            //Process(new RiskIndicator());
            _riskDataTask.Enqueue(new RiskIndicator());

            SubscribeRisk();
        }

        #region 实时数据处理

        private void OnRiskData(List<RiskIndicator> riskIndicator)
        {
            LogHelper.Info("OnRiskData:" + riskIndicator == null ? riskIndicator.Count : 0);
            foreach (RiskIndicator ri in riskIndicator)
            {
                LogHelper.Info(string.Format("user_id:{0}, selecteduser:{1}", ri.user_id, (SelectedUser != null ? ("G" + SelectedUser.Name) : string.Empty)));
                if (LocalEnv.UserInfo.UserName.Equals(ri.user_id) || (SelectedUser != null && ("G" + SelectedUser.Name).Equals(ri.user_id)))
                    _riskDataTask.Enqueue(ri);
                LogHelper.Info("Refresh OnRiskData end.");
            }
        }

        private void Process(RiskIndicator riskIndicator)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (_lockObj)
                {
                    //利率债-多头合计结算头寸实时值
                    LongPosRealTimeSettl = CommonUtils.formatString(riskIndicator.long_pos_realTime_settl, 4);
                    //利率债-多头合计净头寸实时值
                    LongPosRealTime = CommonUtils.formatString(riskIndicator.long_pos_realTime, 4);


                    //利率债-空头合计结算头寸实时值
                    ShortPosRealTimeSettl = CommonUtils.formatString(riskIndicator.short_pos_realTime_settl, 4);
                    //利率债-空头合计净头寸实时值
                    ShortPosRealTime = CommonUtils.formatString(riskIndicator.short_pos_realTime, 4);

                    
                    //利率债 - 净仓位合计结算头寸实时值
                    PosRealTimeSettl = CommonUtils.formatString(riskIndicator.pos_realTime_settl, 4);
                    //利率债-净仓位合计净头寸实时值
                    PosRealTime = CommonUtils.formatString(riskIndicator.pos_realTime, 4);

                    //利率债-多空仓位合计净头寸实时值
                    LsPosRealTimeSettl = CommonUtils.formatString(riskIndicator.ls_pos_realTime_settl, 4);
                    //利率债-多空仓位合计净头寸实时值
                    LsPosRealTime = CommonUtils.formatString(riskIndicator.ls_pos_realTime, 4);

                    //信用债规模实时值
                    CreditRealTime = CommonUtils.formatString(riskIndicator.credit_realTime, 4);
                    //信用债dv01实时值
                    CreditDv01RealTime = CommonUtils.formatString(riskIndicator.credit_dv01_realTime, 4);



                    //风险敞口DV01
                    _bncRiskLimitDV01.SettleRisk.LimitOne = riskIndicator.min_dv01_settl;
                    _bncRiskLimitDV01.SettleRisk.LimitTwo = riskIndicator.max_dv01_settl;
                    _bncRiskLimitDV01.SettleRisk.RealtimeVal = riskIndicator.dv01_settl_realTime;
                    _bncRiskLimitDV01.SettleRisk.LimitMargin = ComputeLimit(_bncRiskLimitDV01.SettleRisk, riskIndicator.dv01_settl_realTime, riskIndicator.max_dv01_settl, riskIndicator.min_dv01_settl, "180");

                    //_bncRiskLimitDV01.SettleRisk.LimitOne = riskIndicator.min_re_dv01_settl;
                    //_bncRiskLimitDV01.SettleRisk.LimitTwo = riskIndicator.max_re_dv01_settl;
                    //_bncRiskLimitDV01.SettleRisk.RealtimeVal = riskIndicator.re_dv01_settl_realTime;
                    //_bncRiskLimitDV01.SettleRisk.LimitMargin = ComputeLimit(_bncRiskLimitDV01.SettleRisk, riskIndicator.re_dv01_settl_realTime, riskIndicator.max_re_dv01_settl, riskIndicator.min_re_dv01_settl, "180");

                    if (string.IsNullOrEmpty(riskIndicator.min_dv01_settl) || string.IsNullOrEmpty(riskIndicator.max_dv01_settl))
                        _bncRiskLimitDV01.SettleRisk.Limit = DEFAULTTXT;
                    else
                        _bncRiskLimitDV01.SettleRisk.Limit = string.Format("[{0}, {1}]", riskIndicator.min_dv01_settl, riskIndicator.max_dv01_settl);


                    _bncRiskLimitDV01.NetPositionRisk.LimitOne = riskIndicator.min_dv01;
                    _bncRiskLimitDV01.NetPositionRisk.LimitTwo = riskIndicator.max_dv01;
                    _bncRiskLimitDV01.NetPositionRisk.RealtimeVal = riskIndicator.dv01_realTime;
                    _bncRiskLimitDV01.NetPositionRisk.LimitMargin = ComputeLimit(_bncRiskLimitDV01.NetPositionRisk, riskIndicator.dv01_realTime, riskIndicator.max_dv01, riskIndicator.min_dv01, "180");

                    if (string.IsNullOrEmpty(riskIndicator.min_dv01) || string.IsNullOrEmpty(riskIndicator.max_dv01))
                        _bncRiskLimitDV01.NetPositionRisk.Limit = DEFAULTTXT;
                    else
                        _bncRiskLimitDV01.NetPositionRisk.Limit = string.Format("[{0}, {1}]", riskIndicator.min_dv01, riskIndicator.max_dv01);


                    //债券DV01 //债券规模
                    _bncRiskLimitScope.SettleRisk.LimitOne = riskIndicator.short_pos_limit_settl;
                    _bncRiskLimitScope.SettleRisk.LimitTwo = riskIndicator.long_pos_limit_settl;
                    _bncRiskLimitScope.SettleRisk.RealtimeVal = riskIndicator.bond_size_settl_realTime;
                    _bncRiskLimitScope.SettleRisk.LimitMargin = ComputeLimit(_bncRiskLimitScope.SettleRisk, riskIndicator.bond_size_settl_realTime, riskIndicator.long_pos_limit_settl, riskIndicator.short_pos_limit_settl, "180");

                    //_bncRiskLimitScope.SettleRisk.LimitOne = riskIndicator.min_dv01_settl;
                    //_bncRiskLimitScope.SettleRisk.LimitTwo = riskIndicator.max_dv01_settl;
                    //_bncRiskLimitScope.SettleRisk.RealtimeVal = riskIndicator.dv01_settl_realTime;
                    //_bncRiskLimitScope.SettleRisk.LimitMargin = ComputeLimit(_bncRiskLimitScope.SettleRisk, riskIndicator.dv01_settl_realTime, riskIndicator.max_dv01_settl, riskIndicator.min_dv01_settl, "180");

                    if (string.IsNullOrEmpty(riskIndicator.short_pos_limit_settl) || string.IsNullOrEmpty(riskIndicator.long_pos_limit_settl))
                        _bncRiskLimitScope.SettleRisk.Limit = DEFAULTTXT;
                    else
                        _bncRiskLimitScope.SettleRisk.Limit = string.Format("[{0}, {1}]", riskIndicator.short_pos_limit_settl, riskIndicator.long_pos_limit_settl);

                    _bncRiskLimitScope.NetPositionRisk.LimitOne = riskIndicator.short_pos_limit;
                    _bncRiskLimitScope.NetPositionRisk.LimitTwo = riskIndicator.long_pos_limit;
                    _bncRiskLimitScope.NetPositionRisk.RealtimeVal = riskIndicator.bond_size_realTime;
                    _bncRiskLimitScope.NetPositionRisk.LimitMargin = ComputeLimit(_bncRiskLimitScope.NetPositionRisk, riskIndicator.bond_size_realTime, riskIndicator.long_pos_limit, riskIndicator.short_pos_limit, "180");

                    //_bncRiskLimitScope.NetPositionRisk.LimitOne = riskIndicator.min_dv01;
                    //_bncRiskLimitScope.NetPositionRisk.LimitTwo = riskIndicator.max_dv01;
                    //_bncRiskLimitScope.NetPositionRisk.RealtimeVal = riskIndicator.dv01_realTime;
                    //_bncRiskLimitScope.NetPositionRisk.LimitMargin = ComputeLimit(_bncRiskLimitScope.NetPositionRisk, riskIndicator.dv01_realTime, riskIndicator.max_dv01, riskIndicator.min_dv01, "180");

                    if (string.IsNullOrEmpty(riskIndicator.short_pos_limit) || string.IsNullOrEmpty(riskIndicator.long_pos_limit))
                        _bncRiskLimitScope.NetPositionRisk.Limit = DEFAULTTXT;
                    else
                        _bncRiskLimitScope.NetPositionRisk.Limit = string.Format("[{0}, {1}]", riskIndicator.short_pos_limit, riskIndicator.long_pos_limit);

                }
            }));
        }

        private Thickness ComputeLimit(RiskLimitDetailViewModel riskLimitDetailViewModel, string limit, string max, string min, string actualHeight)
        {
            Thickness result = new Thickness(0, 0, 0, 0);

            decimal limitD = 0;
            decimal maxD = 0;
            decimal minD = 0;
            decimal actualHeightD = 0;

            riskLimitDetailViewModel.ProcessBgBrush = (Brush)_ownUI.FindResource("ColorBrush.Gray13");

            if (decimal.TryParse(limit, out limitD) && decimal.TryParse(max, out maxD) && decimal.TryParse(min, out minD)
                && decimal.TryParse(actualHeight, out actualHeightD))
            {
                decimal d = (limitD - minD) / (maxD - minD) * actualHeightD;// 106;

                if (limitD < minD)
                    result.Left = 0;
                else
                    result.Left = d > actualHeightD ? (double)actualHeightD - 30 : (double)d;

                if (limitD != 0 && (limitD > maxD || limitD < minD))
                {
                    riskLimitDetailViewModel.ProcessBgBrush = (Brush)_ownUI.FindResource("ColorBrush.Green16");
                    riskLimitDetailViewModel.CursorBrush = (Brush)_ownUI.FindResource("ColorBrush.Green17");
                }
                else
                {
                    riskLimitDetailViewModel.CursorBrush = (Brush)_ownUI.FindResource("ColorBrush.Green14");
                }
            }
            else
            {
                riskLimitDetailViewModel.ProcessBgBrush = (Brush)_ownUI.FindResource("ColorBrush.Gray13");
                riskLimitDetailViewModel.CursorBrush = (Brush)_ownUI.FindResource("ColorBrush.Green14");
            }

            return result;
        }

        #endregion
    }
}
