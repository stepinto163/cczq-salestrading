using STC.Common.Model.Fix.Message;
using STC.Common.Model.QTR;
using System;
using System.Collections.Generic;

namespace STC.Common.Handler.QTRS
{
    public interface IQtrsHandler
    {
        void Init();

        void InitTestData();

        void Test();
        /// <summary>
        /// Qtr 成交
        /// </summary>
        /// <param name="callBack"></param>
        void SubscribeQtrTrade(Action<List<QtrTrade>> callBack);

        void UnSubscribeQtrTrade(Action<List<QtrTrade>> callBack);

        /// <summary>
        /// Qtr 订单
        /// </summary>
        /// <param name="callBack"></param>
        void SubscribeQtrOrder(Action<List<QtrOrder>> callBack);

        void UnSubscribeQtrOrder(Action<List<QtrOrder>> callBack);

        /// <summary>
        /// Qtr 连接状态
        /// </summary>
        /// <param name="callBack"></param>
        void SubscribeQtrStatusMsg(Action<SessionStatus> callBack);

        void UnSubscribeQtrStatusMsg(Action<SessionStatus> callBack);

        /// <summary>
        /// Qtr 行情 
        /// </summary>
        /// <param name="callBack"></param>
        void SubscribeQtrQuoteMsg(Action<QtrQuote> callBack);
        void UnSubscribeQtrQuoteMsg(Action<QtrQuote> callBack);

    }
}