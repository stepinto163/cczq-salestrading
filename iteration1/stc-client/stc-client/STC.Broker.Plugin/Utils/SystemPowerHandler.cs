using STC.Broker.Plugin.Model;
using STC.Common.Handler.PT;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public class SystemPowerHandler
    {
        private static SystemPowerHandler instance = null;
        private static readonly object padlock = new object();


        private SystemPowerModel _adminPermissions = null;
        /// <summary>
        /// 管理员权限
        /// </summary>
        public SystemPowerModel AdminPermissions
        {
            get
            {
                return _adminPermissions;
            }
        }

        private List<SystemPowerModel> _assistantPermissions = null;
        /// <summary>
        /// 助理权限列表
        /// </summary>
        public List<SystemPowerModel> AssistantPermissions
        {
            get
            {
                return _assistantPermissions;
            }
        }

        private List<SystemPowerModel> _lookPermissions = null;
        /// <summary>
        /// 查看指定人成交 VIEW_APPOINTED_TRADE
        /// </summary>
        public List<SystemPowerModel> LookPermissions
        {
            get
            {
                return _lookPermissions;
            }
        }

        private List<SystemPowerModel> _bridgeAttachePermissions = null;
        /// <summary>
        /// 桥专员 BRIDGE_ATTACHE
        /// </summary>
        public List<SystemPowerModel> BridgeAttachePermissions
        {
            get
            {
                return _bridgeAttachePermissions;
            }
        }

        private bool _isBridgeRole = false;
        /// <summary>
        /// 桥专员 BRIDGE_ATTACHE
        /// </summary>
        public bool IsBridge
        {
            get
            {
                return _isBridgeRole;
            }
        }


        private List<SystemPowerModel> _otherBridgeAttachePermissions = null;
        /// <summary>
        /// 其他人过桥 BRIDGE_ATTACHE
        /// </summary>
        public List<SystemPowerModel> OtherBridgeAttachePermissions
        {
            get
            {
                return _otherBridgeAttachePermissions;
            }
        }


        private bool _isOrtherBridge = false;
        /// <summary>
        /// 其他人桥 BRIDGE_ATTACHE
        /// </summary>
        public bool IsOrtherBridge
        {
            get
            {
                return _isOrtherBridge;
            }
        }

        private List<PaymentInstDetail> _sendReceiptInstDisplay;
        private bool _isQueryPaymentInstDetail = false;
        /// <summary>
        /// 成交回执机构列表
        /// </summary>
        public List<PaymentInstDetail> SendReceiptInstDisplay
        {
            get
            {
                //if (!_isQueryPaymentInstDetail)
                //{
                //    GetSendReceiptInst();
                //}
                return _sendReceiptInstDisplay;
            }
        }

        public static SystemPowerHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        instance = new SystemPowerHandler();
                    }
                }
                return instance;
            }
        }

        private SystemPowerHandler()
        {
            QueryOrderInfo();
        }


        /// <summary>
        /// 查询订单
        /// </summary>
        private void QueryOrderInfo()
        {
            try
            {
                SystemPowerRequest systemPowerRequest = new SystemPowerRequest();
                systemPowerRequest.userId = LocalEnv.UserInfo.UserId;
                GetExecSystemPowerData(systemPowerRequest);
                GetSendReceiptInst();
            }
            catch (Exception ex)
            {
                com.gateway.log.LogHelper.Error(ex);
            }
            finally
            {
                //isHasGetData = true;
            }
        }


        public void Reflash()
        {
            QueryOrderInfo();
        }

        /// <summary>
        /// 查询权限数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private void GetExecSystemPowerData(SystemPowerRequest req)
        {
            SystemPowerReplay reply = null;
            reply = GRHandler.Instance.ExecSystemPowerQuery(req);
            if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
            {
                com.gateway.log.LogHelper.Info($"GetExecOrderHistData Result: Return {reply.Datas.Count} Records.");
                if (reply.Datas.TryGetValue("ASSISTANT", out _assistantPermissions))
                {

                }



                if (reply.Datas.TryGetValue("VIEW_APPOINTED_TRADE", out _lookPermissions))
                {

                }

                if (reply.Datas.TryGetValue("BRIDGE_ATTACHE", out _bridgeAttachePermissions))
                {
                }

                if (reply.Datas.ContainsKey("BRIDGE_ATTACHE"))
                {
                    _isBridgeRole = true;
                }
                else
                {
                    _isBridgeRole = false;
                }

                if (reply.Datas.TryGetValue("VIEW_OTHER_BRIDGE_TRADE", out _otherBridgeAttachePermissions))
                {

                }

                if (reply.Datas.ContainsKey("VIEW_OTHER_BRIDGE_TRADE"))
                {
                    _isOrtherBridge = true;
                }
                else
                {
                    _isOrtherBridge = false;
                }
            }
        }


        private void GetSendReceiptInst()
        {
            _sendReceiptInstDisplay = new List<PaymentInstDetail>();
            PaymentInstRequestPo request = new PaymentInstRequestPo();
            request.action = "0";
            request.businessType = "4";
            PaymentInstReplyPo paymentInstReplyPo = GRHandler.Instance.OperateInstProxySetting(request);
            if (paymentInstReplyPo != null && "0000".Equals(paymentInstReplyPo.Code) && paymentInstReplyPo.Datas != null && paymentInstReplyPo.Datas.Any())
            {
                _isQueryPaymentInstDetail = true;
                foreach (PaymentInstDetail item in paymentInstReplyPo.Datas)
                {
                    if (item.businessType == "4")
                    {
                        _sendReceiptInstDisplay.Add(item);
                    }
                }
            }
            else
            {
                _isQueryPaymentInstDetail = false;
            }
        }
        /// <summary>
        /// 当前用户是否为指定人助理
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool IsAssistant(string userid)
        {
            bool result = false;
            if (_assistantPermissions != null && _assistantPermissions.Count > 0)
            {
                if (_assistantPermissions.Any(_ => _.userId == userid))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 当前用户是否有指定人查看权限
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool IsLookPermissions(string userid)
        {
            bool result = false;
            if (_lookPermissions != null && _lookPermissions.Count > 0)
            {
                if (_lookPermissions.Any(_ => _.userId == userid))
                {
                    result = true;
                }
            }
            return result;
        }


        /// <summary>
        /// 当前用户能查看指定订单,包括自己
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool IsCanLookOrder(string userid, bool isOwer = true)
        {
            if (isOwer)
            {
                if (userid == LocalEnv.UserInfo.UserId)
                {
                    return true;
                }
            }

            if (IsLookPermissions(userid))
            {
                return true;
            }

            if (IsAssistant(userid))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 当前用户能编辑指定订单,包括自己
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool IsCanEditOrder(string userid)
        {
            if (string.IsNullOrWhiteSpace(userid))
            {
                return false;
            }
            if (userid == LocalEnv.UserInfo.UserId)
            {
                return true;
            }

            if (IsAssistant(userid))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 指定用户是否是桥专员
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool IsBridgeRole(string userid)
        {
            return _isBridgeRole;
        }

        public bool HasOtherPermision(TransOrderServerData order, UserBridge userBridge)
        {
            if (_otherBridgeAttachePermissions != null && _otherBridgeAttachePermissions.Count > 0)
            {
                if (order.BridgeOrderModel != null && userBridge.bankInstitutionid== order.BridgeOrderModel.instId&& userBridge.bankTraderid== order.BridgeOrderModel.instTradeId
                    && _otherBridgeAttachePermissions.Any(_=>_.userId== order.BridgeOrderModel.instTradeId))
                {
                    return true;
                }

                if (order.BridgeOrderModel2 != null && userBridge.bankInstitutionid == order.BridgeOrderModel2.instId && userBridge.bankTraderid == order.BridgeOrderModel2.instTradeId
                    && _otherBridgeAttachePermissions.Any(_ => _.userId == order.BridgeOrderModel2.instTradeId))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
