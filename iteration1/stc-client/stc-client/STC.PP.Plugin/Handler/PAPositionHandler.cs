using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Handler.PP;
using STC.Common.Model.Admin;
using STC.Common.Model.PP;
using STC.Common.Utils;
using STC.Common.ViewModel.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STC.PP.Plugin.Handler
{
    public class PAPositionHandler
    {
        #region 字段
        private static PAPositionHandler server = new PAPositionHandler();
        public static PAPositionHandler Instance { get { return server; } }

        /// <summary>
        /// 复制持仓参数设置 Key
        /// </summary>
        public const string PARAM_KEY_MSGALERTPARAM = "STC_PAPostion_CopyMsg_Setting";

        private List<MsgAlertModel> _pAPostionCopyParams = new List<MsgAlertModel>();

        /// <summary>
        /// 复制持仓参数
        /// </summary>
        public List<MsgAlertModel> PAPostionCopyParams
        {
            get
            {
                return _pAPostionCopyParams;
            }
            set
            { 
                _pAPostionCopyParams = value;
            }
        }


        private Dictionary<string, object> _dictTradeAccount = new Dictionary<string, object>();
        /// <summary>
        /// 交易账户 
        /// </summary>
        public Dictionary<string, object> DictTradeAccount
        {
            get { return _dictTradeAccount; }
            set
            {
                _dictTradeAccount = value;
            }
        }


        private Dictionary<string, object> _dictFundAccount = new Dictionary<string, object>();

        /// <summary>
        /// 投资经理 
        /// </summary>
        public Dictionary<string, object> DictFundAccount
        {
            get { return _dictFundAccount; }
            set
            {
                _dictFundAccount = value;
            }
        }

        #endregion

        public PAPositionHandler()
        {
        
        }

        public async void Init()
        {
           await QueryGlobalPara();
        }

        # region 查询复制持仓参数设置
        /// <summary>
        /// 查询复制持仓参数设置
        /// </summary>
        public async Task QueryGlobalPara(bool isdefault = false)
        {
            SystemParameterReq req = new SystemParameterReq();
            req.ParamKey = PARAM_KEY_MSGALERTPARAM;
            if (!isdefault)
                req.Username = LocalEnv.UserInfo.UserName;
            else
                req.Username = "*";

            TbookSystemParameterReplyPo reply = await GetReply(req);
            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                {
                    string s = reply.Datas.tbookSystemParameter.param_value;
                    if (JsonSplit.IsJson(s))
                    {
                        List<MsgAlertDataModel> lstMDM = JsonConvert.DeserializeObject<List<MsgAlertDataModel>>(s);

                        foreach (MsgAlertDataModel res in lstMDM)
                        {
                            if (res.subList != null)
                            {
                                if (res.code == "CopyPositionField")
                                {
                                    PAPostionCopyParams = res.subList;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _pAPostionCopyParams = new List<MsgAlertModel>();
            }
        }

        /// <summary>
        /// 异步获取系统参数回复
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TbookSystemParameterReplyPo> GetReply(SystemParameterReq req)
        {
            TbookSystemParameterReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = AdminHandler.Instance.querySystemParameter(req);
            });

            return reply;
        }
        #endregion

        #region 查询平安持仓

        /// <summary>
        /// 查询平安持仓
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<QueryBondPositionResult> GetQueryBondPosition(QueryBondPositionReq req)
        {
            QueryBondPositionResult reply = null;
            await Task.Run(() =>
            {
                reply = PPHandler.Instance.QueryBondPosition(req);
            });

            return reply;
        }

        #endregion

        #region 查询平安质押券 
        /// <summary>
        /// 质押券 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<QueryPledgePositionResult> GetQueryPledgeposition(QueryPledgePositionReq req)
        {
            QueryPledgePositionResult reply = null;
            await Task.Run(() =>
            {
                reply = PPHandler.Instance.QueryPledgePosition(req);
            });

            return reply;
        }
        #endregion

        #region 查询交易员下的 交易账户
        /// <summary>
        /// 查询交易账户
        /// </summary>
        /// <returns></returns>
        public async Task QueryTradingAccount()
        {
            //获取交易账户和投资经理
            QueryTradingAccountReq request = new QueryTradingAccountReq();
            request.userId = LocalEnv.UserInfo.UserId;
            QueryTradingAccountResult reply = await GetQueryTradingAccount(request);

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    _dictTradeAccount.Clear();
                    foreach (StcTradingAccountModel res in reply.Datas)
                    {
                        _dictTradeAccount.Add(res.accountNo, res.accountName);
                    }
                }
            }
        }

        /// <summary>
        /// 查询交易账户Task
        /// </summary>
        /// <returns></returns>
        public async Task<QueryTradingAccountResult> GetQueryTradingAccount(QueryTradingAccountReq request)
        {
            QueryTradingAccountResult reply = null;
            await Task.Run(() =>
            {
                reply = PPHandler.Instance.QueryTradingAccount(request);
            });

            return reply;
        }

        #endregion


        #region 查询投资经理
        /// <summary>
        /// 查询投资经理
        /// </summary>
        /// <returns></returns>
        public async Task QueryFundAccount(bool isFundAccount = false)
        {
            string roleid = null;   
            try
            {
                TRolesQuery rs = AdminHandler.Instance.rolesQuery();
                if (rs != null)
                {
                    if (rs.Datas != null && rs.Datas.Count > 0)
                    {
                        if (isFundAccount)
                        {
                            _dictFundAccount.Clear();
                            _dictFundAccount.Add(LocalEnv.UserInfo.UserName, LocalEnv.UserInfo.FullName);
                            return;
                        }
                        else
                            roleid = rs.Datas.Where(o => o.rolename.Contains("经理")).FirstOrDefault()?.id ?? "";
                    }
                }
            }
            catch { }



            QueryFundAccountReq request = new QueryFundAccountReq();
            request.roleid = roleid ?? LocalEnv.UserInfo.RoleId;
            //获取交易账户和投资经理
            STCUserReplyPo reply = await GetQueryFundAccount(request);
            if (reply != null)
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    _dictFundAccount.Clear();
                    foreach (StcTbookUsers res in reply.Datas)
                    {
                        _dictFundAccount.Add(res.Username, res.Name ?? res.Username);
                    }
                }
            }
        }

        public async Task<STCUserReplyPo> GetQueryFundAccount(QueryFundAccountReq request)
        {
            STCUserReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = PPHandler.Instance.QueryFundAccount(request);
            });

            return reply;
        }

        #endregion
    }
}
