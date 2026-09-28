using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.XBond.Plugin.ViewModel.Risk
{
    /// <summary>
    /// 风控指标明细类结构
    /// </summary>
    public class RiskLimitDetailViewModel : BaseViewModel
    {
        private string _riskTitle = string.Empty;
        public string RiskTitle { get { return _riskTitle; } set { _riskTitle = value; this.OnPropertyChanged(); } }


        private string _realtimeVal = string.Empty;
        public string RealtimeVal { get { return _realtimeVal; } set { _realtimeVal = value; this.OnPropertyChanged(); } }

        private Brush _realtimeValBrush;
        public Brush RealtimeValBrush { get { return _realtimeValBrush; } set { _realtimeValBrush = value; this.OnPropertyChanged(); } }


        private string _limitOne;
        public string LimitOne { get { return _limitOne; } set { _limitOne = value; this.OnPropertyChanged(); } }

        private string _limitTwo;
        public string LimitTwo { get { return _limitTwo; } set { _limitTwo = value; this.OnPropertyChanged(); } }

        private string _limit;
        public string Limit { get { return _limit; } set { _limit = value; this.OnPropertyChanged(); } }

        private string _source;
        public string Source { get { return _source; } set { _source = value; this.OnPropertyChanged(); } }


        private Brush _processBrush;
        public Brush ProcessBrush { get { return _processBrush; } set { _processBrush = value; this.OnPropertyChanged(); } }

        private Thickness _limitMargin;
        public Thickness LimitMargin { get { return _limitMargin; } set { _limitMargin = value; this.OnPropertyChanged(); } }


        private Brush _processBgBrush;
        public Brush ProcessBgBrush { get { return _processBgBrush; } set { _processBgBrush = value; this.OnPropertyChanged(); } }

        private Brush _cursorBrush;
        public Brush CursorBrush { get { return _cursorBrush; } set { _cursorBrush = value; this.OnPropertyChanged(); } }



        private string _processVal = string.Empty;
        public string ProcessVal { get { return _processVal; } set { _processVal = value; this.OnPropertyChanged(); } }
        
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

        
        

        private string _limitHeight;
        public string LimitHeight { get { return _limitHeight; } set { _limitHeight = value; this.OnPropertyChanged(); } }

        #endregion
    }
}
