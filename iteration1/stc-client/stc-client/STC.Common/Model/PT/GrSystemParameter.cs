using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    public class GrSystemParameter
    {
        /// <summary>
        /// 客户端设置
        /// </summary>
        public Dictionary<string, string> ClientAppSettings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 格式化机构设置 机构合并展示
        /// 示例:[{"name":"工商银行","userName":"2a4622f4346a647001346a7c172a00cb(contact表中的instCode"}
        /// </summary>
        public List<UserRoleSettingDetail> FormatInst;

        /// <summary>
        /// 是否显示简称:true/false
        /// </summary>
        public string formatShortNameCheck;

        /// <summary>
        /// 是否显示期限:true/false
        /// </summary>
        public string formatTenorCheck;

        /// <summary>
        /// 带地方债类型:true/false
        /// </summary>
        public string formatDebtTypeCheck { get; set; }

        /// <summary>
        /// 带私募债类型:true/false
        /// </summary>
        public string formatPPNTypeCheck { get; set; }
        ///// <summary>
        ///// 二次价格确认:true/false
        ///// </summary>
        //public string PriceCheck;

        ///// <summary>
        ///// 客户端点价分自动模式和手动模式，在后台中配置
        ///// 可选值：
        ///// TP PA 自动
        ///// MR 手动
        ///// MA 线下
        ///// 数据来源：Topic(idc.broker.systemparas.)
        ///// </summary>
        //public string ClientDefaultRule;

        ///// <summary>
        ///// 价格显示设置:
        /////1:不显示
        /////2:显示2列
        /////3:显示4列
        ///// </summary>
        //public string PriceShow;

        /// <summary>
        /// 快捷键设置
        /// autotest
        /// </summary>
        public string HotKey;

        /// <summary>
        /// 开启开市设置
        /// </summary>
        public string CanOpenSet;

        /// <summary>
        /// 开启闭市设置
        /// </summary>
        public string CanCloseSet;

        /// <summary>
        /// 开市时间设置
        /// autotest
        /// </summary>
        public string AutoOpenInterval { get; set; }

        /// <summary>
        /// 闭市时间设置
        /// autotest
        /// </summary>
        public string AutoCloseInterval { get; set; }

        //private OrderFlow orderFlowPo;
        //public OrderFlow OrderFlowPo
        //{
        //    get
        //    {
        //        try
        //        {
        //            if (!string.IsNullOrEmpty(OrderFlow))
        //                orderFlowPo = MsgHelper.Deserialize<OrderFlow>(OrderFlow);
        //        }
        //        catch (Exception e)
        //        {

        //        }

        //        return orderFlowPo;
        //    }
        //}

        //public string OrderFlow;

        ///// <summary>
        ///// 用户桥设置 
        ///// 20201011 弃用 王兴鹏
        ///// </summary>
        //public List<UserBridge> Bridge;

        /// <summary>
        /// 查看指定人成交权限
        /// 可以查看其他人成交记录的人员
        /// </summary>
        public List<UserRoleSettingDetail> QueryOrders;

        /// <summary>
        /// 查看指定人成交权限
        /// 可以查看其他人成交记录的人员
        /// </summary>
        public List<UserRoleSettingDetail> UpdateOrders;

        ///// <summary>
        ///// 指定人集合
        ///// 当前用户可以查看其成交记录的人员
        ///// </summary>
        //public List<AccountInfo> Users;

        ///// <summary>
        ///// 设置权限
        ///// </summary>
        //public List<AccountInfo> Settings;

        /// <summary>
        /// 开/闭市权限
        /// </summary>
        public List<UserRoleSettingDetail> OpenCloses;

        /// <summary>
        /// 不可查看他人历史成交
        /// </summary>
        public List<UserRoleSettingDetail> onlyQueryOwnHistoryOrder;

        //public static List<UserBridge> CopyBridge(SystemParameter sp)
        //{
        //    if (sp != null && sp.Bridge != null)
        //    {
        //        UserBridge[] arr = new UserBridge[sp.Bridge.Count];
        //        Array.Copy(sp.Bridge.ToArray(), arr, sp.Bridge.Count);

        //        return arr.ToList();
        //    }

        //    return null;
        //}
        /// <summary>
        /// //桥权限
        /// </summary>
        public List<UserRoleSettingDetail> BridgePermissions;
        /// <summary>
        /// 其他人桥权限
        /// </summary>
        public List<UserRoleSettingDetail> OtherBridgePermissions;
        ///// <summary>
        ///// //是否有桥权限标记：true,false
        ///// </summary>
        //public string IsBridge;
        ///// <summary>
        ///// 公司代码
        ///// PATR - 平安
        ///// </summary>
        //public string CompanyCode;
        ///// <summary>
        ///// 历史记录自动刷新时间
        ///// </summary>
        //public string RefreshTime;

        ///// <summary>
        ///// 成交回执机构列表
        ///// </summary>
        //public string SendReceiptInst { get; set; }

        ///// <summary>
        ///// QM成交回执机构列表
        ///// </summary>
        //public string sendExternalInst { get; set; }

        ///// <summary>
        ///// 单条发送qq消息是否隐藏交易对手方
        ///// </summary>
        public bool HideCounterpartySend { get; set; } = true;


        ///// <summary>
        ///// 是否展示真实方向,true  代表要展示真实方向 false就按原来逻辑展示
        ///// </summary>
        public bool showSourceDealSide { get; set; } = false;

        ///// <summary>
        ///// 1-国利；2-国际；3-bgc；4-平安；5-信唐；6-上田
        ///// </summary>
        //public string companyid { get; set; }

        ///// <summary>
        ///// 成交回执机构列表
        ///// </summary>
        //public List<ReceiptInst> SendReceiptInstDisplay
        //{
        //    get
        //    {
        //        List<ReceiptInst> receipts = new List<ReceiptInst>();
        //        if (!string.IsNullOrWhiteSpace(SendReceiptInst))
        //        {
        //            receipts = MsgHelper.Deserialize<List<ReceiptInst>>(SendReceiptInst, MsgSerializerType.Json);
        //        }
        //        return receipts;
        //    }
        //}

        ///// <summary>
        ///// QM成交回执机构列表
        ///// </summary>
        //public List<ReceiptInst> SendExternalInstDisplay
        //{
        //    get
        //    {
        //        List<ReceiptInst> receipts = new List<ReceiptInst>();
        //        if (!string.IsNullOrWhiteSpace(sendExternalInst))
        //        {
        //            if (!sendExternalInst.ToLower().Contains("true"))
        //            {
        //                receipts = MsgHelper.Deserialize<List<ReceiptInst>>(sendExternalInst, MsgSerializerType.Json);
        //            }
        //        }
        //        return receipts;
        //    }
        //}
        ///// <summary>
        ///// QM成交回执所有
        ///// </summary>
        //public bool IsAllSendExternalInst
        //{
        //    get
        //    {
        //        bool receipts = false;
        //        if (!string.IsNullOrWhiteSpace(sendExternalInst))
        //        {
        //            if (sendExternalInst.ToLower().Contains("true"))
        //            {
        //                receipts = true;
        //            }
        //        }
        //        return receipts;
        //    }
        //}
    }

    public class OrderFlow
    {
        /// <summary>
        /// 新意向，brokerA是否弹框 
        /// </summary>
        public string brokerAShowBox_BrokerAFilledNew;
        /// <summary>
        /// brokerB是否弹框，当A全确认
        /// </summary>
        public string brokerBShowBox_BrokerAFiledConfirm;
        /// <summary>
        /// brokerB是否弹框，当B部分确认
        /// </summary>
        public string brokerBShowBox_BrokerBPartiallyFiledConfirm;
        /// <summary>
        /// brokerA是否自动确认，当B全部确认时
        /// </summary>
        public string brokerAAutoConfirm_BrokerBFiledConfirm;
        /// <summary>
        /// brokerA是否自动确认，当B部分确认时
        /// </summary>
        public string brokerAAutoConfirm_BrokerBPartiallyFiledConfirm;
    }

    //public class AccountInfo
    //{
    //    /// <summary>
    //    /// 指定人名字
    //    /// </summary>
    //    public string Name;

    //    /// <summary>
    //    /// 指定人帐号
    //    /// </summary>
    //    public string userName;


    //    public List<AccountInfo> Users;
    //}

   
    /// <summary>
    /// 回执机构列表
    /// </summary>
    public class ReceiptInst
    {

        /// <summary>
        /// 成交回执机构id
        /// </summary>
        public string instid { get; set; }

        /// <summary>
        /// 成交回执机构名称
        /// </summary>
        public string instname { get; set; }
    }
}
