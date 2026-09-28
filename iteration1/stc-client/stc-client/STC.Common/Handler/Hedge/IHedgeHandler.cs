using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Hedge;
using STC.Common.Model.Position;
using System;
using System.Collections.Generic;

namespace STC.Common.Handler.Hedge
{
    public interface IHedgeHandler
    {
        /// <summary>
        /// 风险敞口计算器 RiskExposureCalclateResult
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        RiskExposureCalclateResult QueryRiskExposureCalclate(RiskExposureCalclateReq req);

        /// <summary>
        /// 查询模拟交易数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        PositionSimExecResult QueryPositionSimExec(PositionSimExecReq req);

        /// <summary>
        /// 模拟数据新增修改删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        PResult UpdPositionSimExec(PositionSimExecReq req);
    }
}