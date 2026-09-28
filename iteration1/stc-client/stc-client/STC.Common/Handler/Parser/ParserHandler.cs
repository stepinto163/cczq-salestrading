using netapi.Api;
using STC.Common.Api;
using STC.Common.Dispatcher;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.Table
{
    public class ParserHandler
    {
        private static ParserHandler instance = new ParserHandler();
     
        public static ParserHandler Instance { get { return instance; } }

        private ParserHandler()
        {

        }

        /// <summary>
        /// 报价识别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo RequestParserBond(STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo>(ApiConsts.ParserSvr, ApiConsts.Topic_ParserBond, request);
        }

        /// <summary>
        /// IRS识别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo RequestParserIRS(STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo>(ApiConsts.ParserSvr, ApiConsts.Topic_ParserIRS, request);
        }


        /// <summary>
        /// 模糊需求识别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public STC.Common.Model.Table.RecognizeVirtualPoModelReplyPo RequestParserVirtual(STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.RecognizeVirtualPoModelReplyPo>(ApiConsts.ParserSvr, ApiConsts.Topic_ParserVirtual, request);
        }

        /// <summary>
        /// 图片报价识别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo RequestParserImage(STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteRequest request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STC.Common.Model.Table.WorkBrench.RecognizeIntentionQuoteReplyPo>(ApiConsts.ParserSvr, ApiConsts.Topic_ParserImage, request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RecognizeDealReply RequestParserDeal(RecognizeDealReq request)
        {
            return ApiManager.Instance.GetDefaultApi().Request<RecognizeDealReply>(ApiConsts.ParserSvr, ApiConsts.Topic_ParserTrade, request);
        }

    }
}
