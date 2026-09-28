using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.DownloadCenter.Plugin.ViewModel.Data
{
    public class DownloadItem : BaseViewModel
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set {
                if (IsChecked != value)
                {
                    _isChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _id;

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }


        private string _securityId;

        public string SecurityID
        {
            get { return _securityId; }
            set { _securityId = value; }
        }

        private string _symbol;

        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        private string _dataType;

        public string DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        private string _settleType;

        public string SettleType
        {
            get { return _settleType; }
            set { _settleType = value;
                this.OnPropertyChanged();
            }
        }

        private string _dataSize;

        public string DataSize
        {
            get { return _dataSize; }
            set { 
                if (_dataSize != value)
                {
                    _dataSize = value;
                    DataSize_Sort = value.ConvertToLong();
                    this.OnPropertyChanged();
                }
            }
        }

        private long _dataSize_Sort;

        public long DataSize_Sort
        {
            get { return _dataSize_Sort; }
            set
            {
                if (_dataSize_Sort != value)
                {
                    _dataSize_Sort = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _lastStatus;

        public string LastStatus
        {
            get { return _lastStatus; }
            set { _lastStatus = value;
                this.OnPropertyChanged();
            }
        }

        private string _fileName;

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                this.OnPropertyChanged();
            }
        }

        private string _startDate;

        public string StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                _startDate = value;
                this.OnPropertyChanged();
            }
        }

        private string _endDate;

        public string EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
                this.OnPropertyChanged();
            }
        }

        private string _updateTime;

        public string UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value;
                this.OnPropertyChanged();
            }
        }

        private string _createTime;

        public string CreateTime
        {
            get { return _createTime; }
            set { _createTime = value; }
        }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        private bool _isDownloading = false;

        public bool IsDownloading
        {
            get { return _isDownloading; }
            set { 
                _isDownloading = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// k线频率
        /// </summary>
        private string _frequency;

        public string Frequency
        {
            get { return _frequency; }
            set { 
                _frequency = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 行情类型
        /// </summary>
        private string _type;

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                this.OnPropertyChanged();
            }
        }
    }
}
