using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 风控参数全量查询 接口请求参数
    /// </summary>
    public class RiskParaReq : BaseReplyPo
    {
        public string userName;

        public List<AppointRiskParameter> appointRiskParameterList;

        public List<PartyPositionRiskParameter> partyPositionRiskParameters;

        public DefaultRiskParameter defaultRiskParameter;
    }
}
