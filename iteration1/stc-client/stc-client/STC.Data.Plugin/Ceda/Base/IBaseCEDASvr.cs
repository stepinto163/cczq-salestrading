using STC.Common.Model.Base;
using STC.Data.Plugin.Model.Base;
using System;

namespace STC.Data.Plugin.Ceda.Base
{
    public interface IBaseCEDASvr
    {
        event Action<ClearPo> OnClear;
        string Topic_Clear { get; }
        void SubScribe(string[] topics);
        void SubScribeWithImage(string[] topics);
        void UnSubScribePricing(string[] topics);

        BondsSearchReplyPo SearchBonds(BondsSearchRequestPo request);

        ValuationReplyPo Valuation(ValuationRequestPo request);
    }
}
