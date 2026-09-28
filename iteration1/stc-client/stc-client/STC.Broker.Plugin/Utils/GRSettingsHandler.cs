using Newtonsoft.Json;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Handler.Admin;
using STC.Common.Handler.PT;
using STC.Common.Model.Admin;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace STC.Broker.Plugin.Utils
{
    public class GRSettingsHandler
    {
        public const string PARAM_KEY_USER_GR_SETTINGS = "USER_GR_SETTINGS";

        public static SystemParameter GlobSystemParameter = new SystemParameter();


        public static PowerSettingModel PowerSettingModel = new PowerSettingModel();

        private static GRSettingsHandler systemParamHandler = new GRSettingsHandler();
        public static GRSettingsHandler Instance { get { return systemParamHandler; } }

        public static bool IsInit = false;

        private GRSettingsHandler()
        {
            SystemParameterReq req = new SystemParameterReq();
            req.ParamKey = PARAM_KEY_USER_GR_SETTINGS;
            req.Username = LocalEnv.UserInfo.UserName;
            TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
            if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
            {
                GlobSystemParameter = JsonConvert.DeserializeObject<SystemParameter>(reply.Datas.tbookSystemParameter.param_value);
                LocalEnv.GrGlobSystemParameter = JsonConvert.DeserializeObject<GrSystemParameter>(reply.Datas.tbookSystemParameter.param_value);
            }

            loadUserRoleSettings();
        }

        public SystemParameter GetSystemParameter()
        {
            return GlobSystemParameter;
        }

        /// <summary>
        /// 只传修改的参数
        /// </summary>
        /// <param name="request"></param>
        public void UpdateSystemParameter(SystemParameter request)
        {
            if (request.FormatInst != null)
                GlobSystemParameter.FormatInst = request.FormatInst;
            if (request.HotKey != null)
            {
                GlobSystemParameter.HotKey = request.HotKey;
                
            }
            //if (request.OpenCloses != null)
            //    GlobSystemParameter.OpenCloses = request.OpenCloses;
            //if (request.PriceCheck != null)
            //    GlobSystemParameter.PriceCheck = request.PriceCheck;
            //if (request.PriceShow != null)
            //    GlobSystemParameter.PriceShow = request.PriceShow;
            //if (request.QueryOrders != null)
            //    GlobSystemParameter.QueryOrders = request.QueryOrders;
            //if (request.UpdateOrders != null)
            //    GlobSystemParameter.UpdateOrders = request.UpdateOrders;
            //if (request.Settings != null)
            //    GlobSystemParameter.Settings = request.Settings;
            //if (request.onlyQueryOwnHistoryOrder != null)
            //    GlobSystemParameter.onlyQueryOwnHistoryOrder = request.onlyQueryOwnHistoryOrder;
            //if (request.Users != null)
            //    GlobSystemParameter.Users = request.Settings;
            if (request.formatShortNameCheck != null)
                GlobSystemParameter.formatShortNameCheck = request.formatShortNameCheck;
            if (request.formatTenorCheck != null)
                GlobSystemParameter.formatTenorCheck = request.formatTenorCheck;
            if (!string.IsNullOrWhiteSpace(request.formatDebtTypeCheck))
            {
                GlobSystemParameter.formatDebtTypeCheck = request.formatDebtTypeCheck;
            }
            if (!string.IsNullOrWhiteSpace(request.formatPPNTypeCheck))
            {
                GlobSystemParameter.formatPPNTypeCheck = request.formatPPNTypeCheck;
            }
            if (request.ClientAppSettings.Count > 0)
            {
                foreach (var pair in request.ClientAppSettings)
                {
                    GlobSystemParameter.ClientAppSettings[pair.Key] = pair.Value;
                }
            }
            if (request.CanOpenSet != null)
                GlobSystemParameter.CanOpenSet = request.CanOpenSet;
            if (request.CanCloseSet != null)
                GlobSystemParameter.CanCloseSet = request.CanCloseSet;

            // CopyInfoJson
            SystemGlobalParamReq req = new SystemGlobalParamReq();
            req.action = "2";
            req.list = new List<TTbookSystemParameter>();
            TTbookSystemParameter param = new TTbookSystemParameter();
            param.param_key = PARAM_KEY_USER_GR_SETTINGS;
            param.username = LocalEnv.UserInfo.UserName;
            param.param_value = JsonConvert.SerializeObject(GlobSystemParameter);
            param.param_desc = "GR配置";
            req.list.Add(param);

            AdminHandler.Instance.updateSystemGlobalPara(req);
            
            //同步更新全局参数
            LocalEnv.GrGlobSystemParameter = JsonConvert.DeserializeObject<GrSystemParameter>(param.param_value);
        }

        public static readonly string OffLineTag = "OFFLINEENTRY";
        public static readonly string PricingTabTag = "PRICINGTABENTRY";

        public static readonly HashSet<string> OffLineKeys = new HashSet<string> { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" };
        public static readonly HashSet<string> PricingTabKeys = new HashSet<string> { "LeftShift", "RightShift", "LeftCtrl", "RightCtrl", "Tab", "Space" };

        /// <summary>
        /// 获取指定系统快捷键
        /// </summary>
        /// <returns></returns>
        public static string GetSysHotKey(string tag)
        {
            string hotKey = string.Empty;
            if (!string.IsNullOrWhiteSpace(GRSettingsHandler.GlobSystemParameter.HotKey))
            {
                string[] hotKeyList = GRSettingsHandler.GlobSystemParameter.HotKey.Split(',');
                if (hotKeyList.Length == 2 && hotKeyList[0] == LocalEnv.UserInfo.UserName)
                {
                    string[] hotKeyValues = hotKeyList[1].Split('|');
                    foreach (string hotKeyValue in hotKeyValues)
                    {
                        string[] hotKeys = hotKeyValue.Split(':');
                        if (hotKeys.Length == 2)
                        {
                            if (tag == hotKeys[0])
                            {
                                hotKey = hotKeys[1];
                            }
                        }
                    }
                }
            }

            return hotKey;
        }


        public static bool AppSettingIsTrue(string name)
        {
            string valueStr;
            bool appSetting;
            return GlobSystemParameter.ClientAppSettings.TryGetValue(name, out valueStr)
            && bool.TryParse(valueStr, out appSetting)
            && appSetting;
        }

        private string MODULEID_GR_MANAGEMENT_SETTING = "5009";
        private string spSend = "500901";//发单
        private string spPayment = "500902";//代付
        private string spOutSend = "500903";//外发
        private string spRelevance = "500904";//关联方
        private string MarketSetup = "500905";//开闭市

        public bool CanSend()
        {
           return AdminHandler.Instance.CheckPermission(MODULEID_GR_MANAGEMENT_SETTING, spSend);
        }

        public bool CanPayment()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_MANAGEMENT_SETTING, spPayment);
        }

        public bool CanOutSend()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_MANAGEMENT_SETTING, spOutSend);
        }

        public bool CanRelevance()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_MANAGEMENT_SETTING, spRelevance);
        }

        public bool CanMarketSetup()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_MANAGEMENT_SETTING, MarketSetup);
        }

        private string MODULEID_GR_PERMISSION_SETTING = "5010";
        private string OpenClose = "501001";//开闭市
        private string BridgeExe = "501002";//桥专员
        private string OtherBridge = "501003";//查看他人过桥成交
        private string PersonExe = "501004";//查看指定人成交
        private string UpdatePersonExe = "501005";//助理权限
        private string OwnHistory = "501006";//不可查看他人历史成交

        public bool CanOpenClose()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_PERMISSION_SETTING, OpenClose);
        }

        public bool CanBridgeExe()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_PERMISSION_SETTING, BridgeExe);
        }

        public bool CanOtherBridge()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_PERMISSION_SETTING, OtherBridge);
        }

        public bool CanPersonExe()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_PERMISSION_SETTING, PersonExe);
        }

        public bool CanUpdatePersonExe()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_PERMISSION_SETTING, UpdatePersonExe);
        }
        public bool CanOwnHistory()
        {
            return AdminHandler.Instance.CheckPermission(MODULEID_GR_PERMISSION_SETTING, OwnHistory);
        }

        #region 权限设置接口

        public void loadUserRoleSettings(string roleType = null)
        {
            // 权限设置接口
            UserRoleSettingRequest request = new UserRoleSettingRequest();
            if (!string.IsNullOrEmpty(roleType))
            {
                request.roleType = roleType;
            }
            request.action = "0";
            UserRoleSettingReply response = GRHandler.Instance.OperateUserRoleSetting(request);
            if (response != null && response.IsSuccess() && response.Datas != null)
            {
                if (PowerSettingModel.OpenCloses == null)
                    PowerSettingModel.OpenCloses = new List<UserRoleSettingDetail>();
                else
                {
                    if (string.IsNullOrEmpty(roleType) || roleType == UserRoleEnum.OPEN_CLOSE_MARKET.ToString())
                    {
                        PowerSettingModel.OpenCloses.Clear();
                    }
                }

                if (PowerSettingModel.BridgePermissions == null)
                    PowerSettingModel.BridgePermissions = new List<UserRoleSettingDetail>();
                else
                {
                    if (string.IsNullOrEmpty(roleType) || roleType == UserRoleEnum.BRIDGE_ATTACHE.ToString())
                    {
                        PowerSettingModel.BridgePermissions.Clear();
                    }
                }

                if (PowerSettingModel.OtherBridgePermissions == null)
                    PowerSettingModel.OtherBridgePermissions = new List<UserRoleSettingDetail>();
                else
                {
                    if (string.IsNullOrEmpty(roleType) || roleType == UserRoleEnum.VIEW_OTHER_BRIDGE_TRADE.ToString())
                    {
                        PowerSettingModel.OtherBridgePermissions.Clear();
                    }
                }

                if (PowerSettingModel.QueryOrders == null)
                    PowerSettingModel.QueryOrders = new List<UserRoleSettingDetail>();
                else
                {
                    if (string.IsNullOrEmpty(roleType) || roleType == UserRoleEnum.VIEW_APPOINTED_TRADE.ToString())
                    {
                        PowerSettingModel.QueryOrders.Clear();
                    }
                }


                if (PowerSettingModel.UpdateOrders == null)
                    PowerSettingModel.UpdateOrders = new List<UserRoleSettingDetail>();
                else
                {
                    if (string.IsNullOrEmpty(roleType) || roleType == UserRoleEnum.ASSISTANT.ToString())
                    {
                        PowerSettingModel.UpdateOrders.Clear();
                    }
                }

                if (PowerSettingModel.onlyQueryOwnHistoryOrder == null)
                    PowerSettingModel.onlyQueryOwnHistoryOrder = new List<UserRoleSettingDetail>();
                else
                {
                    if (string.IsNullOrEmpty(roleType) || roleType == UserRoleEnum.NOT_VIEW_OTHER_HIST_TRADE.ToString())
                    {
                        PowerSettingModel.onlyQueryOwnHistoryOrder.Clear();
                    }
                }

                foreach (UserRoleSettingDetail detail in response.Datas)
                {
                    // 开闭市设置
                    if (detail.roleType == UserRoleEnum.OPEN_CLOSE_MARKET.ToString())
                    {
                        PowerSettingModel.OpenCloses.Add(detail);
                    }
                    // 桥专员权限
                    else if (detail.roleType == UserRoleEnum.BRIDGE_ATTACHE.ToString())
                    {
                        PowerSettingModel.BridgePermissions.Add(detail);
                    }
                    // 查看他人过桥成交
                    else if (detail.roleType == UserRoleEnum.VIEW_OTHER_BRIDGE_TRADE.ToString())
                    {
                        PowerSettingModel.OtherBridgePermissions.Add(detail);
                    }
                    // 查看指定成交
                    else if (detail.roleType == UserRoleEnum.VIEW_APPOINTED_TRADE.ToString())
                    {
                        PowerSettingModel.QueryOrders.Add(detail);
                    }
                    // 助理
                    else if (detail.roleType == UserRoleEnum.ASSISTANT.ToString())
                    {
                        PowerSettingModel.UpdateOrders.Add(detail);
                    }
                    // 不可查看他人历史成交
                    else if (detail.roleType == UserRoleEnum.NOT_VIEW_OTHER_HIST_TRADE.ToString())
                    {
                        PowerSettingModel.onlyQueryOwnHistoryOrder.Add(detail);
                    }
                }
            }
        }

        public async Task<UserRoleSettingReply> UpdateUserRoleSettings(UserRoleSettingRequest request)
        {
            UserRoleSettingReply reply = null;
            await Task.Run(() => {
                reply = GRHandler.Instance.OperateUserRoleSetting(request);
                //Thread.Sleep(500);
            });
            return reply;
        }

        #endregion


        /// <summary>
        /// 当前登录账号是否有查看其他人桥订单权限
        /// </summary>
        /// <returns></returns>
        public static bool HasPermissonForOtherBridgeOrder(string userName = null)
        {
            if (string.IsNullOrEmpty(userName))
            {
                userName = LocalEnv.UserInfo.UserName;
            }
            return SystemPowerHandler.Instance.IsOrtherBridge && LocalEnv.ConfInfo.ShowLookupOtherSetting;
        }

        public static bool IsShowTenor()
        {
            return "true".Equals(GlobSystemParameter.formatTenorCheck);
        }
        public static bool IsShowShortName()
        {
            return "true".Equals(GlobSystemParameter.formatShortNameCheck);
        }
    }
}
