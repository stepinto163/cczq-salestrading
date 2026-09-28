using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.DownloadCenter.Plugin.Validation;
using STC.DownloadCenter.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.DownloadCenter.Plugin.ViewModel
{
    public class AddTaskViewModel : BaseViewModel
    {
        private ObservableCollection<DataTypeItem> _dataTypeList = new ObservableCollection<DataTypeItem>();

        public ObservableCollection<DataTypeItem> DataTypeList
        {
            get
            {
                if (_dataTypeList.Count == 0)
                {
                    _dataTypeList.Add(new DataTypeItem() { DataTypeCode = "1", DataTypeName = "分渠道最优"});
                    _dataTypeList.Add(new DataTypeItem() { DataTypeCode = "2", DataTypeName = "多档深度"});
                    _dataTypeList.Add(new DataTypeItem() { DataTypeCode = "3", DataTypeName = "XBOND深度"});
                    _dataTypeList.Add(new DataTypeItem() { DataTypeCode = "4", DataTypeName = "CFETS双边深度"});
                    _dataTypeList.Add(new DataTypeItem() { DataTypeCode = "5", DataTypeName = "经纪商成交"});
                    _dataTypeList.Add(new DataTypeItem() { DataTypeCode = "6", DataTypeName = "期货高频快照"});
                    _dataTypeList.Add(new DataTypeItem() { DataTypeCode = "7", DataTypeName = "历史K线" });
                    _dataTypeList.Add(new DataTypeItem() { DataTypeCode = "8", DataTypeName = "原始行情" });
                }

                return _dataTypeList;
            }
            set
            {
                _dataTypeList = value;
                this.OnPropertyChanged();
            }
        }

        private BondsInfoPo _currentBondInfo;

        private object _dataLock = new object();
        private CollectionViewSource _dataTypeViewSource = new CollectionViewSource();
        public CollectionViewSource DataTypeViewSource
        {
            get { return _dataTypeViewSource; }
            set { _dataTypeViewSource = value; }
        }

        private DataTypeItem _dataType_Edit;

        public DataTypeItem DataType_Edit
        {
            get
            {
                if (_dataType_Edit == null)
                    _dataType_Edit = DataTypeList.First();

                return _dataType_Edit;
            }
            set
            {
                _dataType_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _securityid_Edit;
        [CustomValidation(typeof(ValidationExtensionRules), "NotEmpty")]
        public string Securityid_Edit
        {
            get { return _securityid_Edit; }
            set
            {
                _securityid_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _EditSettleTypeMode = "1";
        public string EditSettleTypeMode
        {
            get { return _EditSettleTypeMode; }
            set
            {
                _EditSettleTypeMode = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _settleTypeVisibility = Visibility.Visible;

        public Visibility SettleTypeVisibility
        {
            get
            {
                return _settleTypeVisibility;
            }
            set
            {
                _settleTypeVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _settleType = new Dictionary<string, string>();
        public Dictionary<string, string> SettleTypeDic
        {
            get
            {
                if (_settleType.Count == 0)
                {
                    _settleType["1"] = "T+0";
                    _settleType["2"] = "T+1";
                }
                return _settleType;
            }
        }

        private Visibility _textVisibility = Visibility.Collapsed;

        public Visibility TextVisibility
        {
            get
            {
                return _textVisibility;
            }
            set
            {
                _textVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _textDic = new Dictionary<string, string>();
        public Dictionary<string, string> TextDic
        {
            get
            {
                if (_textDic.Count == 0)
                {
                    _textDic["1M"] = "1M";
                    _textDic["5M"] = "5M";
                    _textDic["15M"] = "15M";
                    _textDic["30M"] = "30M";
                    _textDic["1H"] = "1H";
                    _textDic["1D"] = "1D";
                       
                }
                return _textDic;
            }
        }

        private string _editTextMode = "1M";
        public string EditTextMode
        {
            get { return _editTextMode; }
            set
            {
                _editTextMode = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _typeVisibility = Visibility.Collapsed;

        public Visibility TypeVisibility
        {
            get
            {
                return _typeVisibility;
            }
            set
            {
                _typeVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _typeDic = new Dictionary<string, string>();
        public Dictionary<string, string> TypeDic
        {
            get
            {
                if (_typeDic.Count == 0)
                {
                    _typeDic["CTNP"] = "CTNP";
                    _typeDic["TBNP"] = "TBNP";
                    _typeDic["CT"] = "CT";

                }
                return _typeDic;
            }
        }

        private string _EditTypeMode = "CTNP";
        public string EditTypeMode
        {
            get { return _EditTypeMode; }
            set
            {
                _EditTypeMode = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime? _startDate;

        public DateTime? StartDate
        {
            get
            {
                if (!_startDate.HasValue)
                {
                    DateTime? endDt = EndDate?.AddDays(-5);
                    _startDate = new DateTime(endDt.Value.Year, endDt.Value.Month, endDt.Value.Day, 0, 0, 0);
                }

                return _startDate;
            }
            set
            {
                _startDate = value;
                this.OnPropertyChanged("StartDate");
                this.OnPropertyChanged("StartHour");
                this.OnPropertyChanged("StartMinute");
                this.OnPropertyChanged("StartSecond");
                this.OnPropertyChanged("EndDate");
                this.OnPropertyChanged("EndHour");
                this.OnPropertyChanged("EndMinute");
                this.OnPropertyChanged("EndSecond");
            }
        }

        private DateTime? _endDate;

        public DateTime? EndDate
        {
            get
            {
                if (!_endDate.HasValue)
                {
                    _endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                }
                return _endDate;
            }
            set
            {
                _endDate = value;
                this.OnPropertyChanged("StartDate");
                this.OnPropertyChanged("StartHour");
                this.OnPropertyChanged("StartMinute");
                this.OnPropertyChanged("StartSecond");
                this.OnPropertyChanged("EndDate");
                this.OnPropertyChanged("EndHour");
                this.OnPropertyChanged("EndMinute");
                this.OnPropertyChanged("EndSecond");

            }
        }

        private string _StartHour = "00";
        public string StartHour
        {
            get { return _StartHour; }
            set {
                _StartHour = value;
                this.OnPropertyChanged("StartDate");
                this.OnPropertyChanged("StartHour");
                this.OnPropertyChanged("StartMinute");
                this.OnPropertyChanged("StartSecond");
                this.OnPropertyChanged("EndDate");
                this.OnPropertyChanged("EndHour");
                this.OnPropertyChanged("EndMinute");
                this.OnPropertyChanged("EndSecond");
            }
        }

        private string _StartMinute = "00";
        public string StartMinute
        {
            get { return _StartMinute; }
            set
            {
                _StartMinute = value;
                this.OnPropertyChanged("StartDate");
                this.OnPropertyChanged("StartHour");
                this.OnPropertyChanged("StartMinute");
                this.OnPropertyChanged("StartSecond");
                this.OnPropertyChanged("EndDate");
                this.OnPropertyChanged("EndHour");
                this.OnPropertyChanged("EndMinute");
                this.OnPropertyChanged("EndSecond");
            }
        }

        private string _StartSecond = "00";
        public string StartSecond
        {
            get { return _StartSecond; }
            set
            {
                _StartSecond = value;
                this.OnPropertyChanged("StartDate");
                this.OnPropertyChanged("StartHour");
                this.OnPropertyChanged("StartMinute");
                this.OnPropertyChanged("StartSecond");
                this.OnPropertyChanged("EndDate");
                this.OnPropertyChanged("EndHour");
                this.OnPropertyChanged("EndMinute");
                this.OnPropertyChanged("EndSecond");
            }
        }

        private string _EndHour = "23";
        public string EndHour
        {
            get { return _EndHour; }
            set
            {
                _EndHour = value;
                this.OnPropertyChanged("StartDate");
                this.OnPropertyChanged("StartHour");
                this.OnPropertyChanged("StartMinute");
                this.OnPropertyChanged("StartSecond");
                this.OnPropertyChanged("EndDate");
                this.OnPropertyChanged("EndHour");
                this.OnPropertyChanged("EndMinute");
                this.OnPropertyChanged("EndSecond");
            }
        }

        private string _EndMinute = "59";
        public string EndMinute
        {
            get { return _EndMinute; }
            set
            {
                _EndMinute = value;
                this.OnPropertyChanged("StartDate");
                this.OnPropertyChanged("StartHour");
                this.OnPropertyChanged("StartMinute");
                this.OnPropertyChanged("StartSecond");
                this.OnPropertyChanged("EndDate");
                this.OnPropertyChanged("EndHour");
                this.OnPropertyChanged("EndMinute");
                this.OnPropertyChanged("EndSecond");
            }
        }

        private string _EndSecond = "59";
        public string EndSecond
        {
            get { return _EndSecond; }
            set
            {
                _EndSecond = value;
                this.OnPropertyChanged("StartDate");
                this.OnPropertyChanged("StartHour");
                this.OnPropertyChanged("StartMinute");
                this.OnPropertyChanged("StartSecond");
                this.OnPropertyChanged("EndDate");
                this.OnPropertyChanged("EndHour");
                this.OnPropertyChanged("EndMinute");
                this.OnPropertyChanged("EndSecond");
            }
        }


        public AddTaskViewModel()
        {
            this.DataTypeViewSource.Source = this.DataTypeList;
            this.DataTypeViewSource.View.Filter = x =>
            {
                DataTypeItem data = x as DataTypeItem;
                if (_currentBondInfo != null && _currentBondInfo.SECURITY_TYPE.StartsWith("T"))
                {
                    if (data.DataTypeCode == "6"
                        || data.DataTypeCode == "7"
                        || data.DataTypeCode == "8")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (data.DataTypeCode == "6")
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            };
            this.DataTypeViewSource.View.Refresh();
        }

        internal void SetBondInfo(SimpleBondsInfoPo bi)
        {
            if (bi != null)
                _currentBondInfo = CommonUtils.GetBondsInfoPoBySecurityID(bi.Bond_ID);
        }

        internal string GetSymbol()
        {
            return _currentBondInfo == null ? string.Empty : _currentBondInfo.Short_Name;
        }

        internal void Filter()
        {
            this.DataTypeViewSource.View.Refresh();

            if ((_currentBondInfo != null && _currentBondInfo.SECURITY_TYPE.StartsWith("T")) || DataType_Edit.DataTypeCode == "5")
            {
                SettleTypeVisibility = Visibility.Collapsed;
            }
            else
            {
                if (DataType_Edit.DataTypeCode == "7"
                    || DataType_Edit.DataTypeCode == "8")
                {
                    SettleTypeVisibility = Visibility.Collapsed;
                }
                else
                {
                    SettleTypeVisibility = Visibility.Visible;
                }
            }

            if (DataType_Edit.DataTypeCode == "7")
            {
                TextVisibility = Visibility.Visible;
            }
            else
            {
                TextVisibility = Visibility.Collapsed;
            }

            if (DataType_Edit.DataTypeCode == "8")
            {
                TypeVisibility = Visibility.Visible;
            }
            else
            {
                TypeVisibility = Visibility.Collapsed;
            }
        }
    }
}
