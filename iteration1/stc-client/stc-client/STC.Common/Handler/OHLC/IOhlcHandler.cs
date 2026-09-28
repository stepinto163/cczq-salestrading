using netapi.Api;
using STC.Common.Model.OHLC;
using System;
using System.Collections.Generic;

namespace STC.Common.Handler.OHLC
{
    public interface IOhlcHandler
    {
        void Init(List<string> subsecurityidlist);
        void SubscribeOhlc(string securityID, Action<OhlcMsg> callBack);
        void UnSubscribeOhlc(string securityID, Action<OhlcMsg> callBack);
        void SubscribeKLine(string msgType, string securityId, string timeType, Action<OhlcPo> klineCallBack);

        void UnSubscribeKLine(string msgType, string securityId, string timeType, Action<OhlcPo> klineCallBack);

        void ProcessOHLC(NetMessage netMsg);
        void ProcessKlineMsg(NetMessage netMsg);

        void UnSubscribeKline1M(string securityID, Action<OhlcMsg> callBack);
    }
}