using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.XBond.Plugin.ViewModel.Risk
{
    /// <summary>
    /// 风控指标主结构
    /// </summary>
    public class RiskLimitViewModel : BaseViewModel
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

        public RiskLimitDetailViewModel SettleRisk { get; set; }
        public RiskLimitDetailViewModel NetPositionRisk { get; set; }
    }
}
