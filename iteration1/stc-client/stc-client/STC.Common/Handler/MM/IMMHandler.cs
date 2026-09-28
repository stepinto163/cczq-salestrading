using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Admin
{
    public interface IMMHandler
    {
        void UnSubscribeAll(bool isSubscribe);

        void UnsubscribeMMSysStatus(Action<MMSysStatus> onMsgCallBack, List<string> userNameList);

        void SubscribeMMSysStatus(Action<MMSysStatus> onMsgCallBack, List<string> userNameList);

        void UnSubscribeMM(Action<MM> onMsgCallBack);

        void SubscribeMM(Action<MM> onMsgCallBack);

        PResult mmGroupUpdate(MMGroupReq request);
        MMGroupReplyPo mmGroupQuery();
        PResult mmUpdate(MMReq request);

        PResult mmUpdateParams(MMOrderParamReq request);
        PResult mmUpdateSecurityCdc(MMOrderParamReq request);

        PResult mmUpdateEnable(MMUpdateEnableReq request);

        UserSettingReplyPo queryMMUserSetting();

        PResult updateMMUserSetting(UserSetting userSetting);

        PResult mmUpdateAllEnable(MMUpdateAllEnableReq request);

        PResult mmUpdateWidenSetting(List<MMUpdateWidenSettingReq> reqest);

        PResult mmCancelWidenSetting(MMCancelWidenSettingReq request);

        MMQueryWidenSettingReplyPo mmQueryWidenSetting();

        MMCheckSecurityUserReplyPo mmCheckSecurityUser(List<MMCheckSecurityUserReq> request);

        MMQuerySecurityCirulSizeReplyPo mmQueryBondBalance(string request);

        RiskQtyBySecurityReplyPo mmQueryRiskQty(string request);

        /// <summary>
        /// 双边剔除行情和恢复
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        PResult mmFilterQuote(MMFilterQuoteReq request);

        DealCloseEnableReply queryDealCloseEnableSetting();
        
        QueryMMSecurityidHistReplyPo QueryMMSecurityidHist(QueryMMSecurityidHistRequest request);

        PResult HandleUnifyGroup(string groupid, string enable);

        MMBondParamsSettingReplyPo mmQuerySingleBondSetting();
    }
}
