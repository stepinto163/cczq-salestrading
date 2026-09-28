
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Broker.Plugin.Model.Transaction;
using com.gateway.log;
using STC.Common.Handler.PT;
using STC.Common.Model.PT;

namespace STC.Broker.Plugin.View.Transaction.BridgeDesign
{
    public class UserBridgeHandler
    {
        public readonly List<UserBridge> Bridges = new List<UserBridge>();
        private static UserBridgeHandler userBridgeHandler = new UserBridgeHandler();
        public static UserBridgeHandler Instance { get { return userBridgeHandler; } }

        public static bool IsInit = false;

        private UserBridgeHandler()
        { }

        public void InitBridges()
        {
            UserBridgeChangeRequestPo req = new UserBridgeChangeRequestPo();
            req.bridgeChangeType = "3";
            //获取当前用户所有的桥
            UserBridgeChangeReplyPo msg = GRHandler.Instance.UserBridgeExec(req);
            List<UserBridge> BridgeReviews = new List<UserBridge>();
            if (msg != null && msg.Code == "0000" && msg.Datas != null && msg.Datas.Count > 0)
            {
                foreach (var item in msg.Datas)
                {
                    addBridge(item);
                }
            }
        }

        /// <summary>
        /// 是否包含某个桥
        /// </summary>
        /// <param name="userBridge"></param>
        /// <returns></returns>
        public bool ContainBridge(UserBridge userBridge)
        {
            if (userBridge == null)
                return false;
            string bankId = userBridge.bankInstitutionid;
            string bankAgentId = userBridge.bankTraderid;
            if (GetBridgeById(bankId, bankAgentId) != null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取请求
        /// </summary>
        /// <param name="userBridge"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private UserBridgeChangeRequestPo getRequst(UserBridge userBridge, string type)
        {
            UserBridgeChangeRequestPo request = new UserBridgeChangeRequestPo();
            ///操作类型
            request.bridgeChangeType = type;
            //id
            request.bridgeId = userBridge.bridgeId;
            if (type != "2")
            {
                ///broker
                request.partySubid = userBridge.partySubid;
                request.partySubname = userBridge.partySubname;
                ///机构+交易员
                request.bankInstitutionid = userBridge.bankInstitutionid;
                request.bankInstitutionname = userBridge.bankInstitutionname;
                request.bankTraderid = userBridge.bankTraderid;
                request.bankTradername = userBridge.bankTradername;
                ///计费
                request.feeTraderid = userBridge.feeTraderid;
                request.feeTradername = userBridge.feeTradername;
                request.feeInstitutionid = userBridge.feeInstitutionid;
                request.feeInstitutionname = userBridge.feeInstitutionname;
                ///联系方式，发给，渠道，备注
                request.contactType = userBridge.contactType;
                request.sendto = userBridge.sendto;
                request.channel = userBridge.channel;
                request.remark = userBridge.remark;
            }
            return request;
        }

        /// <summary>
        /// 添加桥，外部调用
        /// </summary>
        /// <param name="userBridge"></param>
        public bool AddBridge(UserBridge userBridge)
        {
            if (!ContainBridge(userBridge))
            {
                UserBridgeChangeRequestPo request = getRequst(userBridge, "0");
                ///添加桥请求
                UserBridgeChangeReplyPo msg = GRHandler.Instance.UserBridgeExec(request);
                //UserBridgeChangeReplyPo msg = BrokerCEDAServer.Instance.userBridgeChange(request);
                if (msg != null && msg.Code == "0000")
                {
                    userBridge.bridgeId = msg.Datas[0].bridgeId;//msg.bridgeId;
                    return addBridge(userBridge);
                }
            }
            return false;
        }
        /// <summary>
        /// 删除桥，外部调用
        /// </summary>
        /// <param name="userBridge"></param>
        public bool DelBridge(UserBridge userBridge)
        {
            if (ContainBridge(userBridge))
            {
                UserBridgeChangeRequestPo request = getRequst(userBridge, "2");
                ///删除桥请求
                UserBridgeChangeReplyPo msg = GRHandler.Instance.UserBridgeExec(request);
                //BaseDataReplyPo msg = BrokerCEDAServer.Instance.userBridgeChange(request);
                if (msg != null && msg.Code == "0000")
                {
                    return delBridge(userBridge);
                }

            }
            return false;
        }

        /// <summary>
        /// 修改桥，外部调用
        /// </summary>
        /// <param name="userBridge"></param>
        public bool UpdateBridge(UserBridge userBridge)
        {
            if (ContainBridge(userBridge))
            {
                UserBridgeChangeRequestPo request = getRequst(userBridge, "1");
                ///修改桥请求
                //BaseDataReplyPo msg = BrokerCEDAServer.Instance.userBridgeChange(request);
                UserBridgeChangeReplyPo msg = GRHandler.Instance.UserBridgeExec(request);
                if (msg != null && msg.Code == "0000")
                {
                    return updateBridge(userBridge);
                }
            }
            return false;
        }
        /// <summary>
        /// 添加桥
        /// </summary>
        /// <param name="userBridge"></param>
        private bool addBridge(UserBridge userBridge)
        {
            if (Bridges.Count > 0)
            {
                List<UserBridge> filterBridges = Bridges.Where(delegate (UserBridge bridge)
                {
                    if (userBridge.bankTraderid + "" == (bridge.bankTraderid) && userBridge.bankInstitutionid + "" == (bridge.bankInstitutionid))
                        return true;
                    else
                        return false;
                }).ToList();
                if (filterBridges == null || filterBridges.Count <= 0)
                {
                    Bridges.Add(userBridge);
                    LogHelper.Info(string.Format("{0}（{1}） 添加桥成功！", userBridge.bankInstitutionname, userBridge.bankTradername));
                    return true;
                }
                return false;
            }
            else
            {
                Bridges.Add(userBridge);
                LogHelper.Info(string.Format("{0}（{1}） 添加桥成功！", userBridge.bankInstitutionname, userBridge.bankTradername));
                return true;
            }
        }
        /// <summary>
        /// 删除桥
        /// </summary>
        /// <param name="userBridge"></param>
        private bool delBridge(UserBridge userBridge)
        {

            List<UserBridge> filterBridges = Bridges.Where(delegate (UserBridge bridge)
            {
                if (userBridge.bankTraderid + "" == bridge.bankTraderid + "" && userBridge.bankInstitutionid + "" == bridge.bankInstitutionid + "")
                    return true;
                else
                    return false;
            }).ToList();

            if (filterBridges != null)
            {
                foreach (var item in filterBridges)
                {
                    Bridges.Remove(item);
                    LogHelper.Info(string.Format("{0}（{1}） 删除桥成功！", item.bankInstitutionname, item.bankTradername));
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 更新桥
        /// </summary>
        /// <param name="userBridge"></param>
        private bool updateBridge(UserBridge userBridge)
        {
            UserBridge bridge = GetBridgeById(userBridge.bankInstitutionid, userBridge.bankTraderid);
            if (bridge != null)
            {
                //bridge.feeBankAgentId = userBridge.feeBankAgentId;
                //bridge.feeBankAgentName = userBridge.feeBankAgentName;

                bridge.contactType = userBridge.contactType;
                bridge.sendto = userBridge.sendto;
                bridge.channel = userBridge.channel;
                bridge.remark = userBridge.remark;
                LogHelper.Info(string.Format("{0}（{1}） 更新桥成功！", userBridge.bankInstitutionname, userBridge.bankTradername));
                return true;
            }
            return false;
        }
        /// <summary>
        /// 从系统桥中获取指定机构和交易员的桥
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="traderId"></param>
        /// <returns></returns>
        public UserBridge GetBridgeById(string bankId, string bankAgentId)
        {
            if (Bridges != null && Bridges.Count > 0)
            {
                List<UserBridge> filterBridges = Bridges.Where(delegate (UserBridge bridge)
                {
                    if (bridge.bankTraderid + "" == bankAgentId + "" && bridge.bankInstitutionid + "" == bankId + "")
                        return true;
                    else
                        return false;
                }).ToList();

                if (filterBridges != null && filterBridges.Count > 0)
                {
                    //LogHelper.Debug($"GetBridgeById== bankId={bankId};bankAgentId={bankAgentId}; 找到{filterBridges.Count}个桥");
                    return filterBridges[0];
                }
                return null;
            }
            else
                return null;
        }
    }
}

