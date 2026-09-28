using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public static class CommonEnum
    {
        // 桥订单复制类型
        public enum BrigeCopyType
        {
            // 正常复制
            Normal,

            // 不带单号复制
            WithoutSeq,

            // 特殊内码复制
            SpecialSeq,

            //不带broker复制
            NoBroker
        }


        public enum ParamterType
        {
            // 带简称
            WithShortName,

            // 地方债带简称
            WithShortNameLGB,

            /// <summary>
            /// 带剩余量信息
            /// </summary>
            WithResidualQuantity,

            /// <summary>
            /// qq发送带要素变化
            /// </summary>
            WithQQElementChange,

            /// <summary>
            /// 通过助理发送qq
            /// </summary>
            WithAssistantQQSendMsg,
            /// <summary>
            /// 带私募债类型
            /// </summary>
            WithpPivatebondtype,
        }
    }
}
