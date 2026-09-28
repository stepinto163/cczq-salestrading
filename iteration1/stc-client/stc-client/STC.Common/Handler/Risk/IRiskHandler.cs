using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Risk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Risk
{
    public interface IRiskHandler
    {
        void UnSubscribeAll(bool isSubscribe);
        void Init(Action<List<RiskIndicator>> onRiskCallBack);
        void UnSubscribe(Action<List<RiskIndicator>> onRiskCallBack);
        RiskHistoryReplyPo queryRisk(RiskHistoryReq req);

        void UnSubscribe(Action<List<RiskIndicator>> onRiskCallBack, string topic);

        void Subscribe(Action<List<RiskIndicator>> onRiskCallBack, string topic);

        /// <summary>
        /// 新增/修改 特殊审批黑名单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        PResult SetRisFilter(SetRisFilterReq req);

        /// <summary>
        /// 查询当前用户的设置的特殊审批黑名单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        RisBlackResult QueryRisFilter(QueryRisFilterReq req);

        /// <summary>
        /// 下载查询所有黑名单
        /// </summary>
        /// <returns></returns>
        RisBlackResult QueryRisBlack();

        void ProcessRisk(NetMessage netMsg);

        BatchCheckRiskReplyPo BatchCheckRisk(List<NewOrderSingle> req);
    }
}
