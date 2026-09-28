using STC.Common.Model.Fix.Message;
using STC.Common.Model.MMshe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Admin
{
    public interface IMMsheHandler
    {
        void UnSubscribeAll(bool isSubscribe);

        void UnsubscribeMMSysStatus(Action<MMsheSysStatus> onMsgCallBack, List<string> userNameList);

        void SubscribeMMSysStatus(Action<MMsheSysStatus> onMsgCallBack, List<string> userNameList);

        void UnSubscribeMM(Action<MMshe> onMsgCallBack);

        void SubscribeMM(Action<MMshe> onMsgCallBack);

        PResult mmGroupUpdate(MMsheGroupReq request);
        MMsheGroupReplyPo mmGroupQuery();
        PResult mmUpdate(MMsheReq request);

        PResult mmUpdateParams(MMsheOrderParamReq request);
        PResult mmUpdateSecurityCdc(MMsheOrderParamReq request);

        PResult mmUpdateEnable(MMsheUpdateEnableReq request);

        UserSettingReplyPo queryMMUserSetting();

        PResult updateMMUserSetting(MMsheUserSetting userSetting);

        PResult mmUpdateAllEnable(MMsheUpdateAllEnableReq request);

        PResult mmUpdateWidenSetting(List<MMsheUpdateWidenSettingReq> reqest);

        PResult mmCancelWidenSetting(MMsheCancelWidenSettingReq request);

        MMsheQueryWidenSettingReplyPo mmQueryWidenSetting();

        MMsheCheckSecurityUserReplyPo mmCheckSecurityUser(List<MMsheCheckSecurityUserReq> request);

        MMsheQuerySecurityCirulSizeReplyPo mmQueryBondBalance(string request);

        RiskQtyBySecurityReplyPo mmQueryRiskQty(string request);

        /// <summary>
        /// 双边剔除行情和恢复
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        PResult mmFilterQuote(MMsheFilterQuoteReq request);

        PResult HandleUnifyGroup(MMsheUnifyGroupPo unifyGroupPo);
        STC.Common.Model.MM.MMBondParamsSettingReplyPo mmQuerySingleBondSetting();
    }
}
