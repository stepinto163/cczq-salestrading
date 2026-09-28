using STC.Common.Model.Base;
using STC.Common.Model.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Base
{
    public interface IBaseHandler
    {
        void Init(Action<ClearPo> onOrderCallBack);

        void UnSubscribeAll(bool isSubscribe);

        void UnSubscribe(Action<ClearPo> onOrderCallBack);

        void Reset();
        TPartydealerInfo searchqueryPartyDealerInfoForBase(PartydealerReq request);
        void SetLoadDataStatus(bool loadData);
        void SubscribeLoadDataStatus(Action<bool> onLoadData);

        void UnSubscribeLoadDataStatus(Action<bool> onLoadData);

        BondsSearchReplyPo SearchBonds(BondsSearchRequestPo request);

        InstSearchReplyPo SearchInst(InstSearchParamPo request);

        TBondValuationReplyPo SearchChianSecurity(TBondValuationReq request);
        bool IsLoad();
        void PublishAppMsg(object obj);
        void SubscribeAppMsg(Action<object> onmsg);
        void UnSubscribeAppMsg(Action<object> onmsg);

        void SubscribeProctMarketExecutionsStatus(Action<bool> onProcDataSuccess);
        void SetProctMarketExecutionsStatus(bool proctmarketexecutions);

        void SubscribeLoadtMarketExecutionsStatus(Action<bool> onLoadDataSuccess);
        void SetLoadtMarketExecutionsStatus(bool loadtmarketexecutions);

        QueryQuoteTagReplyPo QueryQuoteTag();

        QueryTagsCountReplyPo QueryTagsCount(QueryTagsCountRequest req);

        QueryTagsLoadReplyPo QueryTagsLoad(QueryTagsLoadRequest req);

        void SetLoadDataStatus_Issuer(bool loadData);
        void SubscribeLoadData_Issuer(Action<bool> onLoadData);
		void SetSyncIdcStatus(bool status);

        void UnSubscribeLoadData_Issuer(Action<bool> onLoadData);
		bool GetSyncIdcStatus();
        void SetLoadDataStatus_Guarantor(bool loadData);

        void SubscribeLoadData_Guarantor(Action<bool> onLoadData);

        void UnSubscribeLoadData_Guarantor(Action<bool> onLoadData);

        void QueryTags(bool isCache = true);

        Dictionary<string, QuoteTag> GetQuoteTagDic();

        bool FinishLoadTags();

        Dictionary<string, Dictionary<string, string>> TagsDic { get; }

        void InitSubscribe();

        void SetloginOk(bool loadData);
        void SubscribeLoginOk(Action<bool> onLoadData);
    }
}
