using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Handler.Cal;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Hedge;
using STC.Common.Model.Position;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Handler.Hedge
{

    public class HedgeHandler : IHedgeHandler
    {
        private static IHedgeHandler instance = new HedgeHandler();
        public static IHedgeHandler Instance { get { return instance; } }

        public Dictionary<string, List<string>> DictCallBack = new Dictionary<string, List<string>>();

        private HedgeHandler()
        {

        }

        /// <summary>
        /// 风险敞口计算器  RiskExposureCalclateResult
        /// </summary>
        /// <returns></returns>
        public RiskExposureCalclateResult QueryRiskExposureCalclate(RiskExposureCalclateReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<RiskExposureCalclateResult>(ApiConsts.HedgeSvr, ApiConsts.Topic_stc_RiskExposureCalculate, req);
        }

        /// <summary>
        /// 查询持仓模拟
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PositionSimExecResult QueryPositionSimExec(PositionSimExecReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PositionSimExecResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_PositionSimExec, req);
        }

        /// <summary>
        /// 模拟数据新增修改删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public PResult UpdPositionSimExec(PositionSimExecReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<PResult>(ApiConsts.PositionSvr, ApiConsts.Topic_stc_PositionSimExec, req);
        }
        
    }
}
