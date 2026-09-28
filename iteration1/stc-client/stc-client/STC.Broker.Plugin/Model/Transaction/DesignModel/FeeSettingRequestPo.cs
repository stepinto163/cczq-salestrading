using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.DesignModel
{
    public class FeeSettingRequestPo
    {
        /// <summary>
        /// //操作类型
        /// </summary>
        public string feeOperateType { get; set; } = "2";

        /// <summary>
        /// 代付机构id
        /// </summary>
        public string proxyInstId { get; set; }
        /// <summary>
        /// 代付机构名称
        /// </summary>
        public string proxyInstName { get; set; }
        /// <summary>
        /// 代付机构id
        /// </summary>
        public List<FeeSettingInfo> datas { get; set; }
    }

    public class FeeSettingInfo : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// //费用id
        /// </summary>
        //public string feeId { get; set; }
        /// <summary>
        /// 期限开始
        /// </summary>
        public string termStart { get; set; }
        /// <summary>
        ///  期限结束
        /// </summary>
        public string termEnd { get; set; }
        /// <summary>
        /// 费用
        /// </summary>
        public string fee { get; set; }
        /// <summary>
        /// issuer_code
        /// </summary>
        public string createby { get; set; }
        /// <summary>
        /// citycode
        /// </summary>
        public string updateby { get; set; }
        /// <summary>
        /// 是否空数据
        /// </summary>
        public bool IsEmptyData { get; set; } = false;

        /// <summary>
        /// issuer_code
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// citycode
        /// </summary>
        public string updatetime { get; set; }
        /// <summary>
        /// citycode
        /// </summary>
        public string KeyId { get; set; }
        /// <summary>
        /// OrderDateTime
        /// </summary>
        public DateTime OrderDateTime { get; set; }

        /// <summary>
        /// OrderEndDateTime
        /// </summary>
        public DateTime OrderEndDateTime { get; set; }


        /// <summary>
        /// 删除是否可用
        /// </summary>
        private bool isDel = true;
        /// <summary>
        /// 删除是否可用
        /// </summary>
        public bool IsDel
        {
            get
            {
                return isDel;
            }
            set
            {
                if (isDel != value)
                {
                    isDel = value;
                    NotifyPropertyChanged("IsDel");
                }
            }
        }


    }
}
