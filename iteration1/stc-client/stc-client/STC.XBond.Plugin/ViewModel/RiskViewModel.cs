using STC.Common.Handler.Risk;
using STC.Common.Model.Risk;
using STC.Common.Utils;
using STC.Frame.Utils;
using STC.XBond.Plugin.View;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel
{
    public class RiskViewModel : ViewModelBase
    {
        private readonly string DEFAULTTXT = "--";
        private readonly string HUNDRED = "100";
        private FrameworkElement _ownUI = null;
        #region Tag
        
        private Dictionary<string, string> _riskDic = new Dictionary<string, string>();

        private readonly string BNC_LONGPOSITION = "BNCLONGPOSTION";
        private readonly string BNC_SHORTPOSITION = "BNCSHORTPOTION";
        private readonly string BNC_NETPOSITION = "BNCNETPOSITION";
        private readonly string BNC_LONG_SHORT_POSITION = "BNCLONGSHORTPOSITION";
        private readonly string BNC_RISK_DV01 = "BNCRISKDV01";
        private readonly string BNC_BOND_DV01 = "BNCBONDDV01";

        private readonly string BCO_SCOPE = "BCOSCOPE";
        private readonly string BCO_BOND_DV01 = "BCOBONDDV01";

        private readonly string BNC_LONGPOSITION_CN = "多头合计";
        private readonly string BNC_SHORTPOSITION_CN = "空头合计";
        private readonly string BNC_NETPOSITION_CN = "净仓位合计";
        private readonly string BNC_LONG_SHORT_POSITION_CN = "多空仓位合计";
        private readonly string BNC_RISK_CN = "风险敞口DV01";
        private readonly string BNC_BOND_DV01_CN = "债券DV01";

        private readonly string BCO_SCOPE_CN = "信用债规模";
        private readonly string BCO_BOND_DV01_CN = "信用债DV01";

        private readonly string SETTLE = "结算";
        private readonly string NETPOSITION = "净头寸";
        #endregion

        #region 界面数据源

        public RiskViewModel()
        {
            _riskDic.Add(BNC_LONGPOSITION, BNC_LONGPOSITION_CN);
            _riskDic.Add(BNC_SHORTPOSITION, BNC_SHORTPOSITION_CN);
            _riskDic.Add(BNC_NETPOSITION, BNC_NETPOSITION_CN);
            _riskDic.Add(BNC_LONG_SHORT_POSITION, BNC_LONG_SHORT_POSITION_CN);
            _riskDic.Add(BNC_RISK_DV01, BNC_RISK_CN);
            _riskDic.Add(BNC_BOND_DV01, BNC_BOND_DV01_CN);
            _riskDic.Add(BCO_SCOPE, BCO_SCOPE_CN);
            _riskDic.Add(BCO_BOND_DV01, BCO_BOND_DV01_CN);
        }

        private Dictionary<string, RiskData> _riskDataDic = new Dictionary<string, RiskData>();
        public Dictionary<string, RiskData> RiskDataDic
        {
            get { return _riskDataDic; }
        }

        private AsyncThreadQueue<RiskIndicator> _riskDataTask = null;
        private object _lockObj = new object();
        #endregion

        public void Init(FrameworkElement riskView)
        {
            _ownUI = riskView;
            if (_riskDataTask == null)
                _riskDataTask = new AsyncThreadQueue<RiskIndicator>(Process);
            else
                _riskDataTask.Clear();

            int index = 0;
            StackPanel spBnc = (StackPanel)riskView.FindName("spBncRisk");
            StackPanel spBco = (StackPanel)riskView.FindName("spBcoRisk");
            foreach (KeyValuePair<string, string> kv in _riskDic)
            {
                RiskData riskData = new RiskData();
                riskData.HeadTitle = kv.Value;
                riskData.RiskTag = kv.Key;

                RiskDetailView riskDetailView = new RiskDetailView();

                RiskDetailData settleRisk = new RiskDetailData();
                settleRisk.BottomTitle = SETTLE;
                settleRisk.Total = DEFAULTTXT;
                settleRisk.Num = DEFAULTTXT;
                settleRisk.NumBrush = (Brush)riskView.FindResource("ColorBrush.Gray14");

                riskData.SettleRisk = settleRisk;

                riskDetailView.DataContext = riskData;

                _riskDataDic.Add(kv.Key, riskData);

                if (index < 6)
                {
                    RiskDetailData netRisk = new RiskDetailData();
                    netRisk.Total = DEFAULTTXT;
                    netRisk.BottomTitle = NETPOSITION;
                    netRisk.Num = DEFAULTTXT;
                    netRisk.ProcessBrush = netRisk.NumBrush = (Brush)riskView.FindResource("ColorBrush.Red7");
                    //netRisk.ProcessVal = "30";

                    //settleRisk.ProcessVal = "50";
                    settleRisk.ProcessBrush = settleRisk.NumBrush = (Brush)riskView.FindResource("ColorBrush.Green14");

                    riskData.NetPositionRisk = netRisk;

                    riskData.BcoColWidth = string.Empty;
                    riskData.BcoVisibility = Visibility.Visible;
                    riskData.BncVisibility = Visibility.Visible;

                    settleRisk.LimitMargin = netRisk.LimitMargin = new Thickness(37, 0, 0, 3);

                    if (index < 4)
                    {
                        netRisk.ProcessVisibility = settleRisk.ProcessVisibility = Visibility.Visible;
                        netRisk.LimitVisibility = settleRisk.LimitVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        netRisk.ProcessVisibility = settleRisk.ProcessVisibility = Visibility.Collapsed;
                        netRisk.LimitVisibility = settleRisk.LimitVisibility = Visibility.Visible;
                    }

                    if (index == 5)
                        riskDetailView.Margin = new Thickness(5, 5, 5, 5);
                    else if (index == 0)
                        riskDetailView.Margin = new Thickness(0, 5, 0, 5);
                    else
                        riskDetailView.Margin = new Thickness(5, 5, 0, 5);

                    spBnc.Children.Add(riskDetailView);
                }
                else
                {
                    settleRisk.ProcessVisibility = Visibility.Visible;
                    settleRisk.LimitVisibility = Visibility.Collapsed;

                    if (index == 7)
                        riskDetailView.Margin = new Thickness(5, 5, 5, 5);
                    else 
                        riskDetailView.Margin = new Thickness(0, 5, 0, 5);

                    settleRisk.NumBrush = settleRisk.ProcessBrush = (Brush)riskView.FindResource("ColorBrush.Green14");

                    //settleRisk.ProcessVal = "50";
                    riskData.BcoColWidth = "0";
                    riskData.BcoVisibility = Visibility.Collapsed;
                    riskData.BncVisibility = Visibility.Visible;
                    spBco.Children.Add(riskDetailView);
                }

                index++;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                RiskHandler.Instance.Init(OnRiskData);
            });
        }

        public void UnLoad()
        {
            _riskDataTask.Dispose();
            RiskHandler.Instance.UnSubscribe(OnRiskData);
        }

        #region 实时数据处理

        private void OnRiskData(List<RiskIndicator> riskIndicator)
        {
            foreach (RiskIndicator ri in riskIndicator)
            {
                if (LocalEnv.UserInfo.UserName.Equals(ri.user_id) || LocalEnv.UserInfo.GroupIds.Equals(ri.user_id))
                    _riskDataTask.Enqueue(ri);
            }
        }

        private string FormatTxt = "/{0}";
        private string FormatLimit = "/{0},{1}";
        private void Process(RiskIndicator riskIndicator)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (_lockObj)
                {
                    foreach (KeyValuePair<string, string> kv in _riskDic)
                    {
                        try
                        {
                            RiskData riskData = _riskDataDic[kv.Key];
                            switch (kv.Key)
                            {
                                case "BNCLONGPOSTION"://多头合计
                                    riskData.SettleRisk.Total = string.Format(FormatTxt, riskIndicator.long_pos_limit_settl);
                                    riskData.NetPositionRisk.Total = string.Format(FormatTxt, riskIndicator.long_pos_limit);

                                    riskData.SettleRisk.Num = riskIndicator.long_pos_realTime_settl;
                                    riskData.NetPositionRisk.Num = riskIndicator.long_pos_realTime;

                                    riskData.SettleRisk.ProcessVal = ComputeProcess(riskIndicator.long_pos_limit_settl, riskIndicator.long_pos_realTime_settl);
                                    riskData.NetPositionRisk.ProcessVal = ComputeProcess(riskIndicator.long_pos_limit, riskIndicator.long_pos_realTime);

                                    break;
                                case "BNCSHORTPOTION"://空头合计
                                    riskData.SettleRisk.Total = string.Format(FormatTxt, riskIndicator.short_pos_limit_settl);
                                    riskData.NetPositionRisk.Total = string.Format(FormatTxt, riskIndicator.short_pos_limit);

                                    riskData.SettleRisk.Num = riskIndicator.short_pos_realTime_settl;
                                    riskData.NetPositionRisk.Num = riskIndicator.short_pos_realTime;

                                    riskData.SettleRisk.ProcessVal = ComputeProcess(riskIndicator.short_pos_limit_settl, riskIndicator.short_pos_realTime_settl);
                                    riskData.NetPositionRisk.ProcessVal = ComputeProcess(riskIndicator.short_pos_limit, riskIndicator.short_pos_realTime);

                                    break;

                                case "BNCNETPOSITION"://净仓位合计
                                    riskData.SettleRisk.Total = DEFAULTTXT;
                                    riskData.NetPositionRisk.Total = DEFAULTTXT;

                                    riskData.SettleRisk.Num = riskIndicator.pos_realTime_settl;
                                    riskData.NetPositionRisk.Num = riskIndicator.pos_realTime;

                                    riskData.SettleRisk.ProcessVal = HUNDRED;//ComputeProcess(riskData.SettleRisk.Total, riskIndicator.pos_realTime_settl);
                                    riskData.NetPositionRisk.ProcessVal = HUNDRED;//ComputeProcess(riskData.NetPositionRisk.Total, riskIndicator.pos_realTime);

                                    break;

                                case "BNCLONGSHORTPOSITION"://多空仓位合计
                                    riskData.SettleRisk.Total = DEFAULTTXT;
                                    riskData.NetPositionRisk.Total = DEFAULTTXT;

                                    riskData.SettleRisk.Num = riskIndicator.ls_pos_realTime_settl;
                                    riskData.NetPositionRisk.Num = riskIndicator.ls_pos_realTime;

                                    riskData.SettleRisk.ProcessVal = HUNDRED;//ComputeProcess(riskData.SettleRisk.Total, riskIndicator.ls_pos_realTime_settl);
                                    riskData.NetPositionRisk.ProcessVal = HUNDRED;//ComputeProcess(riskData.NetPositionRisk.Total, riskIndicator.ls_pos_realTime);

                                    break;


                                case "BNCRISKDV01"://风险敞口DV01
                                    riskData.SettleRisk.Total = string.Format(FormatLimit, riskIndicator.min_re_dv01_settl, riskIndicator.max_re_dv01_settl);
                                    riskData.NetPositionRisk.Total = string.Format(FormatLimit, riskIndicator.min_re_dv01, riskIndicator.max_re_dv01);

                                    riskData.SettleRisk.LimitMargin = ComputeLimit(riskIndicator.re_dv01_settl_realTime, riskIndicator.max_re_dv01_settl,
                                        riskIndicator.min_re_dv01_settl, riskData.SettleRisk.LimitHeight);
                                    riskData.NetPositionRisk.LimitMargin = ComputeLimit(riskIndicator.re_dv01_realTime, riskIndicator.max_re_dv01,
                                        riskIndicator.min_re_dv01, riskData.NetPositionRisk.LimitHeight);

                                    riskData.SettleRisk.Num = riskIndicator.re_dv01_settl_realTime;
                                    riskData.NetPositionRisk.Num = riskIndicator.re_dv01_realTime;

                                    riskData.SettleRisk.LimitOne = riskIndicator.min_re_dv01_settl;
                                    riskData.SettleRisk.LimitTwo = riskIndicator.max_re_dv01_settl;

                                    riskData.NetPositionRisk.LimitOne = riskIndicator.min_re_dv01;
                                    riskData.NetPositionRisk.LimitTwo = riskIndicator.max_re_dv01;

                                    break;
                                case "BNCBONDDV01"://债券DV01
                                    riskData.SettleRisk.Total = string.Format(FormatLimit, riskIndicator.min_dv01_settl, riskIndicator.max_dv01_settl);
                                    riskData.NetPositionRisk.Total = string.Format(FormatLimit, riskIndicator.min_dv01, riskIndicator.max_dv01);

                                    riskData.SettleRisk.Num = riskIndicator.dv01_settl_realTime;
                                    riskData.NetPositionRisk.Num = riskIndicator.dv01_realTime;

                                    riskData.SettleRisk.LimitMargin = ComputeLimit(riskIndicator.dv01_settl_realTime, riskIndicator.max_dv01_settl,
                                        riskIndicator.min_dv01_settl, riskData.SettleRisk.LimitHeight);
                                    riskData.NetPositionRisk.LimitMargin = ComputeLimit(riskIndicator.dv01_realTime, riskIndicator.max_dv01,
                                        riskIndicator.min_dv01, riskData.NetPositionRisk.LimitHeight);

                                    riskData.SettleRisk.LimitOne = riskIndicator.min_dv01_settl;
                                    riskData.SettleRisk.LimitTwo = riskIndicator.max_dv01_settl;

                                    riskData.NetPositionRisk.LimitOne = riskIndicator.min_dv01;
                                    riskData.NetPositionRisk.LimitTwo = riskIndicator.max_dv01;

                                    break;

                                case "BCOSCOPE"://信用债规模

                                    riskData.SettleRisk.Total = string.IsNullOrEmpty(riskIndicator.credit_limit)? DEFAULTTXT :string.Format(FormatTxt, riskIndicator.credit_limit);
                                    riskData.SettleRisk.Num = riskIndicator.credit_realTime;

                                    riskData.SettleRisk.ProcessVal = ComputeProcess(riskIndicator.credit_limit, riskIndicator.credit_realTime);

                                    break;
                                case "BCOBONDDV01"://信用债DV01

                                    riskData.SettleRisk.Num = riskIndicator.credit_dv01_realTime;
                                    riskData.SettleRisk.Total = string.IsNullOrEmpty(riskIndicator.credit_dv01) ? DEFAULTTXT : string.Format(FormatTxt, riskIndicator.credit_dv01);

                                    riskData.SettleRisk.ProcessVal = HUNDRED;//ComputeProcess(riskIndicator.credit_dv01, riskIndicator.credit_dv01_realTime);

                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            LogHelper.Error(e);
                        }
                    }
                }
            }));
        }

        private string ComputeProcess(string total, string realtime)
        {
            decimal d = 0;
            decimal totalD = 0;
            decimal realtimeD = 0;

            //Border limitBorder = (Border)_ownUI.FindName("bdLimit");
            //decimal limit = (decimal)limitBorder.ActualHeight;

            if (decimal.TryParse(total, out totalD) && decimal.TryParse(realtime, out realtimeD))
                d = realtimeD / totalD * 100;//* limit;//100;


            return d.ToString();
        }
        
        private Thickness ComputeLimit(string limit, string max, string min, string actualHeight)
        {
            Thickness result = new Thickness(37, 0, 0, 0);

            decimal limitD = 0;
            decimal maxD = 0;
            decimal minD = 0;
            decimal actualHeightD = 0;

            if (decimal.TryParse(limit, out limitD) && decimal.TryParse(max, out maxD) && decimal.TryParse(min, out minD)
                && decimal.TryParse(actualHeight, out actualHeightD))
            {
                decimal d = limitD / maxD * actualHeightD;// 106;
                
                result.Bottom = (double)(d + 3);
            }

            return result;
        }

        #endregion

    }

    public class RiskData : ViewModelBase
    {
        private object _riskTag;
        public object RiskTag { get { return _riskTag; } set { _riskTag = value; this.OnPropertyChanged(); } }


        private string _headTitle;
        public string HeadTitle { get { return _headTitle; } set { _headTitle = value; this.OnPropertyChanged(); } }

        private Visibility _bncVisibility;
        public Visibility BncVisibility { get { return _bncVisibility; } set { _bncVisibility = value; this.OnPropertyChanged(); } }

        private Visibility _bcoVisibility;
        public Visibility BcoVisibility { get { return _bcoVisibility; } set { _bcoVisibility = value; this.OnPropertyChanged(); } }

        private string _bcoColWidth;
        public string BcoColWidth { get { return _bcoColWidth; } set { _bcoColWidth = value; this.OnPropertyChanged(); } }

        public RiskDetailData SettleRisk { get; set; }
        public RiskDetailData NetPositionRisk { get; set; }

       
    }


    public class RiskDetailData : ViewModelBase
    {
        private string _processVal = string.Empty;
        public string ProcessVal { get { return _processVal; } set { _processVal = value; this.OnPropertyChanged(); } }

        private Brush _processBrush;
        public Brush ProcessBrush { get { return _processBrush; } set { _processBrush = value; this.OnPropertyChanged(); } }

        private string _bottomTitle;
        public string BottomTitle { get { return _bottomTitle; } set { _bottomTitle = value; this.OnPropertyChanged(); } }

        private string _num;
        public string Num { get { return _num; } set { _num = value; this.OnPropertyChanged(); } }

        private Brush _numBrush;
        public Brush NumBrush { get { return _numBrush; } set { _numBrush = value; this.OnPropertyChanged(); } }
        
        private string _total;
        public string Total { get { return _total; } set { _total = value; this.OnPropertyChanged(); } }

        private Visibility _processVisibility;
        public Visibility ProcessVisibility { get { return _processVisibility; } set { _processVisibility = value; this.OnPropertyChanged(); } }

        #region 范围显示属性

        private Visibility _limitVisibility;
        public Visibility LimitVisibility { get { return _limitVisibility; } set { _limitVisibility = value; this.OnPropertyChanged(); } }

        private Brush _topLimitBgBrush;
        public Brush TopLimitBgBrush { get { return _topLimitBgBrush; } set { _topLimitBgBrush = value; this.OnPropertyChanged(); } }

        private Brush _limitBgBrush;
        public Brush LimitBgBrush { get { return _limitBgBrush; } set { _limitBgBrush = value; this.OnPropertyChanged(); } }

        private Brush _bottomLimitBgBrush;
        public Brush BottomLimitBgBrush { get { return _bottomLimitBgBrush; } set { _bottomLimitBgBrush = value; this.OnPropertyChanged(); } }

        private Thickness _limitMargin;
        public Thickness LimitMargin { get { return _limitMargin; } set { _limitMargin = value; this.OnPropertyChanged(); } }

        private string _limitOne;
        public string LimitOne { get { return _limitOne; } set { _limitOne = value; this.OnPropertyChanged(); } }

        private string _limitTwo;
        public string LimitTwo { get { return _limitTwo; } set { _limitTwo = value; this.OnPropertyChanged(); } }

        private string _limitHeight;
        public string LimitHeight { get { return _limitHeight; } set { _limitHeight = value; this.OnPropertyChanged(); } }
        
        #endregion
    }
}
