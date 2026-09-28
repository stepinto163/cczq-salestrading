using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    /// <summary>
    /// Datas为空表示没有任务
    /// </summary>
    public class QueryProcessingRecord : ViewModelBase
    {

        private string _id;
        public string id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 批处理起始时间 yyyy-MM-dd
        /// </summary>
        private string _startDate;
        public string startDate
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

        /// <summary>
        /// 批处理结束时间  yyyy-MM-dd
        /// </summary>
        private string _endDate;
        public string endDate
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

        /// <summary>
        /// 批处理状态  0-未执行 1-执行中 2-执行成功 3-执行失败
        /// </summary>
        private string _status;
        public string status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// 失败原因
        /// </summary>
        private string _comment;
        public string comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// 失败原因描述
        /// </summary>
        private string _info1;
        public string info1
        {
            get
            {
                return _info1;
            }
            set
            {
                _info1 = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 批处理执行时间 yyyy-MM-dd HH:mm:ss.SSS
        /// </summary>
        private string _actionTime;
        public string actionTime
        {
            get
            {
                return _actionTime;
            }
            set
            {
                _actionTime = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 批处理结束时间 yyyy-MM-dd HH:mm:ss.SSS
        /// </summary>
        private string _endTime;
        public string endTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                _endTime = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 批处理执行人
        /// </summary>
        private string _actionUser;
        public string actionUser
        {
            get
            {
                return _actionUser;
            }
            set
            {
                _actionUser = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 批处理执行人姓名
        /// </summary>
        private string _actionUserName;
        public string actionUserName
        {
            get
            {
                return _actionUserName;
            }
            set
            {
                _actionUserName = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 批处理创建时间 yyyy-MM-dd HH:mm:ss.SSS
        /// </summary>
        private string _createTime;
        public string createTime
        {
            get
            {
                return _createTime;
            }
            set
            {
                _createTime = value;
                this.OnPropertyChanged();
            }
        }

    }
}
