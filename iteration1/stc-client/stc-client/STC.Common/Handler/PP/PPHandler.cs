using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Position;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PP;
using STC.Common.Utils;
using STC.Common.View.Component;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Instrumentation;
using System.Threading.Tasks;

namespace STC.Common.Handler.PP
{
    public class PPHandler : IPPHandler
    {
        private static IPPHandler instance = new PPHandler();
        public static IPPHandler Instance { get { return instance; } }

        public PPHandler()
        {
        }

        /// <summary>
        /// 查询债券持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public QueryBondPositionResult QueryBondPosition(QueryBondPositionReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryBondPositionResult>(ApiConsts.OptimSvr, ApiConsts.Topic_OptimSvr_QueryBondPosition, req);
        }

        /// <summary>
        /// 查询质押券持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public QueryPledgePositionResult QueryPledgePosition(QueryPledgePositionReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryPledgePositionResult>(ApiConsts.OptimSvr, ApiConsts.Topic_OptimSvr__QueryPledgePosition, req);
        }

        /// <summary>
        /// 查询交易账户
        /// </summary>
        /// <returns></returns>
        public QueryTradingAccountResult QueryTradingAccount(QueryTradingAccountReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryTradingAccountResult>("PaylBaseSvr", "stc.basepayl.queryTradingAccount", req);
        }


        /// <summary>
        /// 查询投资经理
        /// </summary>
        /// <returns></returns>
        public STCUserReplyPo QueryFundAccount(QueryFundAccountReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<STCUserReplyPo>(ApiConsts.AdminSvr, ApiConsts.Topic_stc_userQuery, req);//ClientApi.Instance.stcUserQuery();
        }

        /// <summary>
        /// 查询绩效经理
        /// </summary>
        /// <returns></returns>
        public QueryFundAccountResult QueryPerformanceAccount(QueryFundAccountReq req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QueryFundAccountResult>(ApiConsts.OptimSvr, ApiConsts.Topic_OptimSvr_queryPortfolio, req);//ClientApi.Instance.stcUserQuery();
        }

        #region 公共函数
        /// <summary>
        /// 获取请求标签
        /// </summary>
        /// <param name="_desSource">选择标签描述，注意：期限 如果是自定义期限范围必须是 CUSTTERM,需要修改 StartTermDY EndTermDY</param>
        /// <param name="_focusTagDic">已选标签</param>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetReqTags(ObservableCollection<DescriptionItem> _desSource, Dictionary<string, List<string>> _focusTagDic)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            Dictionary<string, string> dict = _desSource.ToDictionary(item => item.Key, item => item.Title);
            return _focusTagDic?.Where(kvp => dict.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, List<string>>();
        }

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
        public Dictionary<string, string> GetReqExtParams(ObservableCollection<DescriptionItem> _desSource, Dictionary<string, List<string>> _focusTagDic, string _startTermDY, string _endTermDY, string _startBidBp, string _endBidBp, string _startOfrBp, string _endOfrBp, string _startCoupon, string _endCoupon, string _startPrice, string _endPrice, string _startChinaBondValue, string _endChinaBondValue)
        {
            /// 期限        TERM: TermDaysRange => StartTermDY,EndTermDY
            /// bid-中债    BidBp:BidBpRange
            /// 中债-ofr    OfrBp:OfrBpRange
            /// 票面:       Coupon：CouponRateCurrentRange
            /// 报价：      Price：YieldRange
            /// 中债估值：  ChinaBondValue：ChinaBondValuationRange
            Dictionary<string, string> filtered = new Dictionary<string, string>();

            Dictionary<string, string> dict = _desSource.ToDictionary(item => item.Key, item => item.Title);
            if (_focusTagDic == null)
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> filtered2 = _desSource.Where(kvp => !_focusTagDic.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Title);
            if (filtered2 == null || filtered2.Count == 0)
            {
                return new Dictionary<string, string>();
            }
            else
            {
                List<string> keys = new List<string>(filtered2.Keys);
                string newkey = string.Empty;
                foreach (string item in keys)
                {
                    switch (item)
                    {
                        case "CUSTTERM":
                            // 期限
                            newkey = "TermDaysRange";
                            var startTerm = ProcessTerm(_startTermDY);
                            var endTerm = ProcessTerm(_endTermDY);

                            if (!string.IsNullOrEmpty(startTerm) || !string.IsNullOrEmpty(endTerm))
                                filtered[newkey] = FormatRange(startTerm, endTerm);
                            else
                                filtered.Remove(newkey);
                            break;

                        case "BidBp":
                            // bid-中债
                            newkey = "BidBpRange";
                            if (!string.IsNullOrEmpty(_startBidBp) || !string.IsNullOrEmpty(_endBidBp))
                                filtered[newkey] = FormatRange(_startBidBp, _endBidBp);
                            else
                                filtered.Remove(newkey);
                            break;

                        case "OfrBp":
                            // 中债-ofr
                            newkey = "OfrBpRange";
                            if (!string.IsNullOrEmpty(_startOfrBp) || !string.IsNullOrEmpty(_endOfrBp))
                                filtered[newkey] = FormatRange(_startOfrBp, _endOfrBp);
                            else
                                filtered.Remove(newkey);
                            break;

                        case "Coupon":
                            // 票面
                            newkey = "CouponRateCurrentRange";
                            if (!string.IsNullOrEmpty(_startCoupon) || !string.IsNullOrEmpty(_endCoupon))
                                filtered[newkey] = FormatRange(_startCoupon, _endCoupon);
                            else
                                filtered.Remove(newkey);
                            break;

                        case "Price":
                            // 报价
                            newkey = "YieldRange";
                            if (!string.IsNullOrEmpty(_startPrice) || !string.IsNullOrEmpty(_endPrice))
                                filtered[newkey] = FormatRange(_startPrice, _endPrice);
                            else
                                filtered.Remove(newkey);
                            break;

                        case "ChinaBondValue":
                            // 中债估值
                            newkey = "ChinaBondValuationRange";
                            if (!string.IsNullOrEmpty(_startChinaBondValue) || !string.IsNullOrEmpty(_endChinaBondValue))
                                filtered[newkey] = FormatRange(_startChinaBondValue, _endChinaBondValue);
                            else
                                filtered.Remove(newkey);
                            break;
                    }
                }
            }

            return filtered;
        }

        private string FormatRange(string start, string end)
        {
            return $"{(string.IsNullOrEmpty(start) ? "" : start)},{(string.IsNullOrEmpty(end) ? "" : end)}";
        }


        private string ProcessTerm(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            input = input.Trim().ToUpper();

            string result = "";

            if (input.EndsWith("D"))
            {
                // 只允许前面是整数
                string numPart = input.Substring(0, input.Length - 1);
                int days;
                if (int.TryParse(numPart, out days))
                {
                    result = days.ToString();
                }
                else
                {
                    return "";
                }
            }
            else if (input.EndsWith("Y"))
            {
                string numPart = input.Substring(0, input.Length - 1);
                double years;
                if (double.TryParse(numPart, out years))
                {
                    result = ((int)(years * 365)).ToString();
                }
                else
                {
                    return "";
                }
            }
            else
            {
                // 无单位时，只接受整数
                int number;
                if (int.TryParse(input, out number))
                {
                    result = number.ToString();
                }
                else
                {
                    return "";
                }
            }

            // 最终确认 result 是整数
            int temp;
            return int.TryParse(result, out temp) ? result : "";
        }



        public CheckRiskReplyPo CheckRisk(NewOrderSingle req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<CheckRiskReplyPo>(ApiConsts.OptimSvr, ApiConsts.Topic_OptimSvr_checkRisk, req);
        }

        public BatchCheckRiskReplyPo BatchCheckRisk(List<NewOrderSingle> req)
        {
            return ApiManager.Instance.GetDefaultApi().Request<BatchCheckRiskReplyPo>(ApiConsts.OptimSvr, ApiConsts.Topic_OptimSvr_batchcheckRisk, req);
        }

        // 转换方法
        public ObservableCollection<TreeNodeModel> ConvertToTree(List<TTbookBook> books)
        {
            var idSet = new HashSet<string>(books.Select(b => b.Id));
            var validBooks = books
                .Where(b => string.IsNullOrEmpty(b.Parentnode) || idSet.Contains(b.Parentnode))
                .ToList();

            // 先把 List<TTbookBook> 转成 Dictionary，方便查找父节点
            Dictionary<string, TreeNodeModel> dict = new Dictionary<string, TreeNodeModel>();
            foreach (TTbookBook itemModel in validBooks)
            {
                if (!dict.ContainsKey(itemModel.Id))
                {
                    dict[itemModel.Id] = new TreeNodeModel
                    {
                        Id = itemModel.Id.Trim(),
                        Parentnode = itemModel.Parentnode,
                        Name = itemModel.Bookname.Trim(),
                        UserId = itemModel.Userid.Trim(),
                        UserName = itemModel.UserName,
                        BookType = itemModel.Booktype,
                        Remark = itemModel.Remark,
                        ProductId = itemModel.Inf3??"".Trim()
                    };
                }
            }

            ObservableCollection<TreeNodeModel> rootNodes = new ObservableCollection<TreeNodeModel>();

            foreach (TreeNodeModel node in dict.Values)
            {
                if (dict.ContainsKey(node.Parentnode))
                {
                    // 找到父节点，加入父节点的 Children
                    dict[node.Parentnode].Nodes.Add(node);
                }
                else
                {
                    // 没有父节点的，认为是根节点
                    rootNodes.Add(node);
                }
            }

            return rootNodes;
        }
        #endregion

        #region 交易账户数据源
        /// <summary>
        /// 树数据源
        /// </summary>
        private ObservableCollection<TreeNodeModel> DictTradeMultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        private ObservableCollection<TreeNodeModel> DictFundAccounteMultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        private BookBookReplyPo AllBookBookReplyPo = null;
        /// <summary>
        /// 获取交易员 交易账户
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<TreeNodeModel> GetTradeTBookGroup()
        {
            if (DictTradeMultiTreeComboxItemsSource.Count == 0)
            {
                if (AllBookBookReplyPo == null  || AllBookBookReplyPo?.Datas.Count == 0)
                {
                    AllBookBookReplyPo = STC.Common.Handler.Admin.AdminHandler.Instance.QueryBook();
                }

                if (AllBookBookReplyPo?.Datas != null)
                    DictTradeMultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.ConvertToTree(AllBookBookReplyPo?.Datas) ?? new ObservableCollection<TreeNodeModel>();
            }

            return DictTradeMultiTreeComboxItemsSource;
        }

        /// <summary>
        /// 获取投资经理使用的 交易账户，兼容STC
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<TreeNodeModel> GetFundAccountTBookGroup()
        {
            if (DictFundAccounteMultiTreeComboxItemsSource.Count == 0)
            {
                if (AllBookBookReplyPo == null || AllBookBookReplyPo?.Datas.Count == 0)
                {
                    AllBookBookReplyPo = STC.Common.Handler.Admin.AdminHandler.Instance.QueryBook();
                }

                //转换为全部的数结构
                if (DictTradeMultiTreeComboxItemsSource.Count == 0)
                {
                    DictTradeMultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.ConvertToTree(AllBookBookReplyPo?.Datas) ?? new ObservableCollection<TreeNodeModel>();
                }

                //判断投资经理是否有交易账户

                DictFundAccounteMultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.ConvertTreeItemModelToTree(DictTradeMultiTreeComboxItemsSource.ToList(), true);

            }

            return DictFundAccounteMultiTreeComboxItemsSource;
        }


        public ObservableCollection<TreeNodeModel> ConvertTreeItemModelToTree(List<TreeNodeModel> books, bool isInvestor)
        {
            var result = new ObservableCollection<TreeNodeModel>();

            foreach (var node in books)
            {
                var filtered = FilterNodeRecursive(node, LocalEnv.UserInfo.UserName);
                if (filtered != null)
                    result.Add(filtered);
            }

            return result;
        }


        /// <summary>
        /// 递归过滤节点：
        /// 只要子树任何节点 UserName 包含 username，就保留该节点
        /// 否则返回 null
        /// </summary>
        private TreeNodeModel FilterNodeRecursive(TreeNodeModel node, string username)
        {
            // 克隆当前节点（不带子节点）
            TreeNodeModel clone = new TreeNodeModel
            {
                Id = node.Id,
                UserId = node.UserId,
                UserName = node.UserName,
                Parentnode = node.Parentnode,
                Name = node.Name,
                BookType = node.BookType,
                ProductId = node.ProductId,
                Remark = node.Remark,
                Nodes = new ObservableCollection<TreeNodeModel>()
            };

            bool matched = node.UserName != null && node.UserName.Contains(username);

            // 递归过滤子节点
            foreach (var child in node.Nodes)
            {
                var filteredChild = FilterNodeRecursive(child, username);
                if (filteredChild != null)
                {
                    clone.Nodes.Add(filteredChild);
                    matched = true; // 子节点匹配 → 父节点也需保留
                }
            }

            // 如果该节点和所有子节点都不匹配 → 删除
            if (!matched)
                return null;

            return clone;
        }

        #endregion

        #region 查询绩效经理
        private Dictionary<string, object> _dictFundAccount = new Dictionary<string, object>();
        /// <summary>
        /// 绩效经理 
        /// </summary>
        public Dictionary<string, object> DictFundAccount
        {
            get { return _dictFundAccount; }
            set
            {
                _dictFundAccount = value;
            }
        }
        /// <summary>
        /// 查询绩效经理
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, object>>  QueryFundAccount(bool isFundAccount = false)
        {
            ////string roleid = null;
            ////try
            ////{
            ////    TRolesQuery rs = AdminHandler.Instance.rolesQuery();
            ////    if (rs != null)
            ////    {
            ////        if (rs.Datas != null && rs.Datas.Count > 0)
            ////        {
            ////            if (isFundAccount)
            ////            {
            ////                _dictFundAccount.Clear();
            ////                _dictFundAccount.Add(LocalEnv.UserInfo.UserName, LocalEnv.UserInfo.FullName);
            ////                return _dictFundAccount;
            ////            }
            ////            else
            ////                roleid = rs.Datas.Where(o => o.rolename.Contains("经理")).FirstOrDefault()?.id ?? "";
            ////        }
            ////    }
            ////}
            ////catch { }

            DictFundAccount.Clear();

            QueryFundAccountReq request = new QueryFundAccountReq();
            //request.id = roleid ?? LocalEnv.UserInfo.RoleId;
            //获取查询绩效经理
            QueryFundAccountResult reply = await GetQueryFundAccount(request);
            if (reply != null)
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    _dictFundAccount.Clear();
                    foreach (FundAccountModel res in reply.Datas)
                    {
                        _dictFundAccount.Add(res.um, res.name);
                    }
                }
            }

            return DictFundAccount;
        }

        private async Task<QueryFundAccountResult> GetQueryFundAccount(QueryFundAccountReq request)
        {
            QueryFundAccountResult reply = null;
            await Task.Run(() =>
            {
                reply = QueryPerformanceAccount(request);
            });

            return reply;
        }

        #endregion

    }
}
