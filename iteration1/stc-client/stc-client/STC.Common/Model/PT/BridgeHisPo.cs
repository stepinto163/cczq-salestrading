using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 桥操作历史数据
    /// {"afterState":2,"borderID":"b738341763357351938","operateMsg":"添加桥","operateType":2,"updateBy":"王兴鹏","updateTime":"2020-07-30 10:26:34"}
    /// </summary>
    public class BridgeHisPo
    {
        /// <summary>
        /// 桥订单号
        /// </summary>
        public string borderID { get; set; }
        /// <summary>
        /// 修改之前桥状态: 0-待发送，1-待回复，2-已确认，3-已拒绝
        /// </summary>
        public int beforeState;
        public EBridgeState beforeStateEnum
        {
            get { return (EBridgeState)beforeState; }
            set { beforeState = (int)value; }
        }
        /// <summary>
        /// 修改之后桥状态: 0-待发送，1-待回复，2-已确认，3-已拒绝，4-已成交
        /// </summary>
        public int afterState;
        public EBridgeState afterStateEnum
        {
            get { return (EBridgeState)afterState; }
            set { afterState = (int)value; }
        }
        /// <summary>
        /// 预约桥操作类型: 1-预约桥，2-添加桥，3-拒绝，4-同意，5-修改桥要素，6-催ofr前台，7-催bid前台，8-标记成交，9-修改交易要素，10-取消成交
        /// 11-已发单 12-请求已回 13-SOR推送失败
        /// </summary>
        public int operateType { get; set; }
        public EOperateType operateTypeEnum
        {
            get { return (EOperateType)operateType; }
            set { operateType = (int)value; }
        }
        /// <summary>
        /// 操作类型翻译，来自后台
        /// </summary>
        public string operateMsg { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string createBy { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        public string updateBy { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updateTime { get; set; }

        /// <summary>
        /// 修改之前拆单桥bid状态: 0-待发送，1-待回复，2-已确认，3-已取消，4-请求已回，5-已发单，6-已完成
        /// </summary>
        public int beforeBidState;
        public ECDBridgeState beforeBidStateEnum
        {
            get { return (ECDBridgeState)beforeBidState; }
            set { beforeBidState = (int)value; }
        }
        /// <summary>
        /// 修改之前拆单桥ofr状态: 0-待发送，1-待回复，2-已确认，3-已取消，4-请求已回，5-已发单，6-已完成
        /// </summary>
        public int beforeOfrState;
        public ECDBridgeState beforeOfrStateEnum
        {
            get { return (ECDBridgeState)beforeOfrState; }
            set { beforeOfrState = (int)value; }
        }
        /// <summary>
        /// 修改之后拆单桥bid状态: 0-待发送，1-待回复，2-已确认，3-已取消，4-请求已回，5-已发单，6-已完成
        /// </summary>
        public int afterBidState;
        public ECDBridgeState afterBidStateEnum
        {
            get { return (ECDBridgeState)afterBidState; }
            set { afterBidState = (int)value; }
        }
        /// <summary>
        /// 修改之后拆单桥ofr状态: 0-待发送，1-待回复，2-已确认，3-已取消，4-请求已回，5-已发单，6-已完成
        /// </summary>
        public int afterOfrState;
        public ECDBridgeState afterOfrStateEnum
        {
            get { return (ECDBridgeState)afterOfrState; }
            set { afterOfrState = (int)value; }
        }
        /// <summary>
        /// 变更状态
        /// </summary>
        public int stateChangeRange;
        public int StateChangeRange
        {
            get
            {
                return stateChangeRange;
            }
            set
            {
                stateChangeRange = value;
            }
        }


    }

    
}

