using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Position.Plugin.Model
{
    public class HistoryOrderData : ViewModelBase
    {
        /// <summary>
        /// 代码
        /// </summary>
        public string SecurityID { get; set; }

        /// <summary>
        /// 简称
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// 成交价(%)
        /// </summary>
        public decimal LastPx { get; set; }

        /// <summary>
        /// 成交净价
        /// </summary>
        public string NetPrice { get; set; }


        /// <summary>
        /// 成交净价(排序用 )
        /// </summary>
        public decimal NetPrice1 { get; set; }

        /// <summary>
        /// 报量(万元)
        /// </summary>
        public decimal Qty { get; set; }


        /// <summary>
        /// 成交量（万元）
        /// </summary>
        public decimal LastQty { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public string Veneues { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public string VenuesSelectDisplay { get; set; }

        /// <summary>
        /// 清算速度
        /// </summary>
        public string SettlType { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        public string TradeDate { get; set; }

        /// <summary>
        /// 成交时间
        /// </summary>
        public string ExecutionTime { get; set; }

        /// <summary>
        /// 对手方
        /// </summary>
        public string CounterPartyName { get; set; }

        /// <summary>
        /// 对手交易员
        /// </summary>
        public string CounterPartySubName { get; set; }


        /// <summary>
        /// 同步状态
        /// </summary>
        public string SyncStatus { get; set; }

        /// <summary>
        /// STC成交单号
        /// </summary>
        public string ExecID { get; set; }

        /// <summary>
        /// 外部成交单号
        /// </summary>
        public string ClOrdID { get; set; }

        /// <summary>
        /// STC订单号
        /// </summary>
        public string StrategyOrderId { get; set; }

        /// <summary>
        /// 对手订单号
        /// </summary>
        public string RefOrderID { get; set; }

        /// <summary>
        /// 组合ID
        /// </summary>
        private string bookid = string.Empty;
        public string BookID
        {
            get
            {
                return bookid;
            }
            set
            {
                bookid = value;
                this.OnPropertyChanged(nameof(BookID));
            }
        }


        /// <summary>
        /// 组合
        /// </summary>
        public string BookName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 数据类型 1 EMS 2 Manual人工
        /// </summary>
        /// 
        private string datatype = string.Empty;
        public string DataType
        {
            get
            {
                return datatype;
            }
            set
            {
                datatype = value;
                this.OnPropertyChanged(nameof(DataType));
            }
        }

        /// <summary>
        /// 删除按钮是否可见
        /// </summary>
        private string delbtnvisible = "0";
        public string DelBtnVisible
        {
            get
            {
                return delbtnvisible;
            }
            set
            {
                delbtnvisible = value;
                this.OnPropertyChanged(nameof(DelBtnVisible));
            }
        }


        /// <summary>
        /// 已关联按钮是否可见
        /// </summary>
        private string relatedbuttonvisible = "0";
        public string RelatedButtonVisible
        {
            get
            {
                return relatedbuttonvisible;
            }
            set
            {
                relatedbuttonvisible = value;
                this.OnPropertyChanged(nameof(RelatedButtonVisible));
            }
        }


        /// <summary>
        /// 未关联按钮是否可见
        /// </summary>
        private string notrelatedbuttonvisible = "0";
        public string NotRelatedButtonVisible
        {
            get
            {
                return notrelatedbuttonvisible;
            }
            set
            {
                notrelatedbuttonvisible = value;
                this.OnPropertyChanged(nameof(NotRelatedButtonVisible));
            }
        }
        public string Venueexecid { get; set; }
    }
}
