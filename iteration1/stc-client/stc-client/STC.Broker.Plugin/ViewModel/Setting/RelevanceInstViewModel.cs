using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.ViewModel.Setting
{
    public class RelevanceInstViewModel : BaseViewModel
    {

        private bool isSelect;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelect
        {
            get { return isSelect; }
            set
            {
                isSelect = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 关联列表ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 机构
        /// </summary>
        public string Inst { get; set; }
        /// <summary>
        /// 机构名称
        /// </summary>
        public string InstName { get; set; }
        /// <summary>
        /// 关联机构
        /// </summary>
        public string RelevanceInst { get; set; }
        /// <summary>
        /// 关联机构名称
        /// </summary>
        public string RelevanceInstName { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public string OperationTime { get; set; }
    }
}

