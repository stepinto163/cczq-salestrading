using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PP;
using STC.Common.View.Component;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace STC.Common.Handler.PP
{
    public interface IPPHandler
    {
        /// <summary>
        /// 查询债券持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        QueryBondPositionResult QueryBondPosition(QueryBondPositionReq req);

        /// <summary>
        /// 查询投资经理
        /// </summary>
        /// <returns></returns>
        STCUserReplyPo QueryFundAccount(QueryFundAccountReq req);

        /// <summary>
        /// 查询绩效经理
        /// </summary>
        /// <returns></returns>
        QueryFundAccountResult QueryPerformanceAccount(QueryFundAccountReq req);

        /// <summary>
        /// 查询质押券持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        QueryPledgePositionResult QueryPledgePosition(QueryPledgePositionReq req);

        /// <summary>
        /// 查询交易账户
        /// </summary>
        /// <returns></returns>        
        QueryTradingAccountResult QueryTradingAccount(QueryTradingAccountReq req);

        /// <summary>
        /// 获取请求标签
        /// </summary>
        /// <param name="_desSource">选择标签描述，注意：期限 如果是自定义期限范围必须是 CUSTTERM,需要修改 StartTermDY EndTermDY</param>
        /// <param name="_focusTagDic">已选标签</param>
        /// <returns></returns>
        Dictionary<string, List<string>> GetReqTags(ObservableCollection<DescriptionItem> _desSource, Dictionary<string, List<string>> _focusTagDic);



        /// <summary>
        /// 获取请求扩展参数
        /// </summary>
        /// <param name="_desSource">标签描述 如果是自定义期限范围必须是 CUSTTERM</param>
        /// <param name="_focusTagDic">已选标签</param>
        /// <param name="_startTermDY">期限开始</param>
        /// <param name="_endTermDY">期限结束</param>
        /// <param name="_startBidBp">bid-中债开始</param>
        /// <param name="_endBidBp">bid-中债结束</param>
        /// <param name="_startOfrBp">中债-ofr开始</param>
        /// <param name="_endOfrBp">中债-ofr结束</param>
        /// <param name="_startCoupon">票面开始</param>
        /// <param name="_endCoupon">票面结束</param>
        /// <param name="_startPrice">报价开始</param>
        /// <param name="_endPrice">报价结束</param>
        /// <param name="_startChinaBondValue">中债估值开始</param>
        /// <param name="_endChinaBondValue">中债估值结束</param>
        /// <returns></returns>
        Dictionary<string, string> GetReqExtParams(ObservableCollection<DescriptionItem> _desSource, Dictionary<string, List<string>> _focusTagDic, string _startTermDY, string _endTermDY, string _startBidBp, string _endBidBp, string _startOfrBp, string _endOfrBp, string _startCoupon, string _endCoupon, string _startPrice, string _endPrice, string _startChinaBondValue, string _endChinaBondValue);

        CheckRiskReplyPo CheckRisk(NewOrderSingle req);

        BatchCheckRiskReplyPo BatchCheckRisk(List<NewOrderSingle> req);

        ObservableCollection<TreeNodeModel> ConvertTreeItemModelToTree(List<TreeNodeModel> books, bool isInvestor);

        ObservableCollection<TreeNodeModel> ConvertToTree(List<TTbookBook> books);

        /// <summary>
        /// 获取交易员 交易账户
        /// </summary>
        /// <returns></returns>
        ObservableCollection<TreeNodeModel> GetTradeTBookGroup();

        /// <summary>
        /// 获取投资经理使用的 交易账户，兼容STC
        /// </summary>
        /// <returns></returns>
        ObservableCollection<TreeNodeModel> GetFundAccountTBookGroup();

        /// <summary>
        /// 查询绩效经理
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, object>> QueryFundAccount(bool isFundAccount = false);
    }
}