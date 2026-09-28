using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.ViewModel.Setting
{
    public class OutboundManagementModel : BaseViewModel
    {
        public long Id { get; set; } 

        private string organizationId;
        /// <summary>
        /// 机构id
        /// </summary>
        public string OrganizationId
        {
            get { return organizationId; }
            set
            {
                if (organizationId != value)
                {
                    organizationId = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string organizationShortName;
        /// <summary>
        /// 机构简称称
        /// </summary>
        public string OrganizationShortName
        {
            get { return organizationShortName; }
            set
            {
                if (organizationShortName != value)
                {
                    organizationShortName = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string organizationFullName;
        /// <summary>
        /// 机构全称
        /// </summary>
        public string OrganizationFullName
        {
            get { return organizationFullName; }
            set
            {
                if (organizationFullName != value)
                {
                    organizationFullName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string city;
        /// <summary>
        /// 城市
        /// </summary>
        public string City
        {
            get { return city; }
            set
            {
                if (city != value)
                {
                    city = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 操作员姓名
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 操作时间,格式 yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string OperateTime { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool isChecked = false;

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }
    }
}

