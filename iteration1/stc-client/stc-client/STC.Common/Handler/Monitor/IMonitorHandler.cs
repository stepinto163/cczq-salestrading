using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Monitor;
using STC.Common.Model.Risk;
using System;
using System.Collections.Generic;

namespace STC.Common.Handler.Monitor
{
    public interface IMonitorHandler
    {
        PResult SetMonitorSvrinfo(MonitorInfoModel req);

        MonitorInfoDetailResult QueryMonitorSvrinfo(MonitorInfoReq req);

        void SubscribeMonitorInfo(Action<Dictionary<string, monitormodel>> callBack);

        void UnSubscribeMonitorInfo(Action<Dictionary<string, monitormodel>> callBack);

        void SubscribeMonitorMsginfo(Action<Dictionary<string, Dictionary<string, monitormodel>>> callBack);

        void UnSubscribeMonitorMsginfo(Action<Dictionary<string, Dictionary<string, monitormodel>>> callBack);

        LastZookeeperResult GetLastZookeeper(MonitorInfoReq req);

        void ProcessMonitorInfo(NetMessage netMsg);
        void ProcessMonitorMsgInfo(NetMessage netMsg);

    }
}