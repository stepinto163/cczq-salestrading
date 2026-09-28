using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel.Data
{
    /// <summary>
    /// 日志提醒
    /// </summary>
    public class BackLogItem : ViewModelBase
    {
        private string _LogType = string.Empty;

        public string LogType
        {
            get
            {
                return this._LogType;
            }

            set
            {
                if (this._LogType != value)
                {
                    this._LogType = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Message;

        public string Message
        {
            get
            {
                return this._Message;
            }

            set
            {
                if (this._Message != value)
                {
                    this._Message = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _Status = string.Empty;

        public string Status
        {
            get
            {
                return this._Status;
            }

            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _StatusDescritption = string.Empty;

        public string StatusDescritption
        {
            get
            {
                return this._StatusDescritption;
            }

            set
            {
                if (this._StatusDescritption != value)
                {
                    this._StatusDescritption = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _LogTime = string.Empty;

        public string LogTime
        {
            get
            {
                return this._LogTime;
            }

            set
            {
                if (this._LogTime != value)
                {
                    this._LogTime = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}