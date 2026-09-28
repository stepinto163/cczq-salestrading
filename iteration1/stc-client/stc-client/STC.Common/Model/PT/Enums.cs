using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
   
    /// <summary>
    /// 操作类型 ：交易处理后状态
    /// </summary>
    public enum ECommandType
    {
        默认 = 0,
        确认 = 1,
        修改价格 = 2,
        修改数量 = 3,
        修改债券代码 = 4,
        [Description("修改价格描述")]
        修改返点 = 5,//
        修改桥状态 = 6,//单击B按钮
        添加桥 = 7,
        修改加桥按钮B状态 = 8,
        修改加桥按钮O状态 = 9,
        删除桥 = 10,
        删除成交单 = 11,//修改delFlag字段
        [Description("修改ofr结算类型")]
        修改结算类型A = 12,
        [Description("修改bid结算类型")]
        修改结算类型B = 13,
        [Description("修改桥要素")]
        修改桥要素 = 14,
        [Description("修改发给")]
        修改Bid发给 = 15,
        [Description("修改发给")]
        修改Ofr发给 = 16,
        [Description("修改Broker")]
        修改BrokerA = 17,
        [Description("修改Broker")]
        修改BrokerB = 18,
        [Description("修改Trader")]
        修改TraderA = 19,
        [Description("修改Trader")]
        修改TraderB = 20,
        [Description("修改免佣")]
        修改BridgeA是否免佣金 = 21,
        [Description("修改免佣")]
        修改BridgeB是否免佣金 = 22,
        [Description("DCS操作")]
        DCS操作 = 23,
        [Description("确认")]
        A确认 = 24,
        [Description("拒绝")]
        A拒绝 = 25,
        [Description("全部确认")]
        B确认 = 26,
        [Description("部分确认")]
        B部分确认 = 27,
        [Description("拒绝")]
        B拒绝 = 28,
        [Description("新意向")]
        新意向 = 29,
        [Description("预约桥")]
        预约桥 = 30,
        [Description("换桥")]
        换桥 = 31,
        [Description("修改ofr和bid发给")]
        修改ofr和bid发给 = 32,
        [Description("换桥预约")]
        换桥预约 = 33,
        [Description("市场成交操作")]
        InternalTransaction = 34,
        [Description("取消不报市场")]
        NoInternalTransaction = 35,
        [Description("手动发送TraderA成交")]
        ManualSendDealA = 36,
        [Description("手动发送TraderB成交")]
        ManualSendDealB = 37,
        [Description("更新债券代码")]
        UpdateBondCode = 38,
        [Description("修改发给方向")]
        UpdateSendDirection = 39,
        [Description("修改发给渠道")]
        UpdateSendChannel = 40,
        [Description("修改发单人")]
        UpdateIssuer = 41,
        [Description("修改发给备注")]
        UpdateSendRemarks = 42,
        [Description("修改代付费")]
        UpdatePayFee = 43,
        [Description("取消代付费")]
        CancelPayFee = 44,
        [Description("开启发给标准化")]
        OpenSendStandardization = 45,
        [Description("修改ofr和bid结算类型")]
        修改结算类型A和B = 46,
        [Description("发送明细QQ消息A")]
        SendDetailQQMsgA = 47,
        [Description("发送明细QQ消息B")]
        SendDetailQQMsgB = 48,
        [Description("标记/取消标记明细")]
        UpdateDetailMark = 49
    }
    /// <summary>
    /// 桥的备注是否在明细中显示
    ///   在idc_broker_connect_execorders、idc_broker_connect_execorders_mvt增加两字段  bridgeremarkshow：
    ///   第一座桥备注是否显示，bridgeremarkshow2  第二座桥备注是否显示。其中码值：1 表示显示；0表示不显示。
    /// </summary>
    public enum ERemarkShow
    {
        不显示 = '0',
        显示 = '1',
    }


    /// <summary>
    /// 删除标志
    /// </summary>
    public enum EDelFlag
    {
        有效 = '0',
        删除 = '1',
    }
    /// <summary>
    /// 是否免佣
    /// </summary>
    public enum EBrokerage
    {
        不免佣 = '0',
        免佣 = '1',
    }
    /// <summary>
    /// 报价价格类型 20200415
    /// </summary>
    public enum EPriceType
    {
        净价 = 1,
        全价 = 2,
        收益率 = 3,
        利差 = 4
    }
    /// <summary>
    /// 通道 20200415
    /// </summary>
    [Obsolete("请使用ChannelEnum代替")]
    public enum EChannel
    {
        对话 = '1',
        请求 = '2',
        双边 = '3',
        xbond = '4',
        iDeal = '5',
        固收 = '6',
        竞价 = '7',
        大宗 = '8',
    }

    /// <summary>
    /// 预约桥操作类型: 1-预约桥，2-添加桥，3-拒绝，4-同意，5-修改桥要素，6-催ofr前台，7-催bid前台，8-标记成交，9-修改交易要素，10-取消成交
    /// 11-已发单，12-请求已回，13-推送SOR失败
    /// </summary>
    public enum EOperateType
    {
        [Description("未知")]
        未知 = 0,
        [Description("预约桥")]
        预约桥 = 1,
        [Description("添加桥")]
        添加桥 = 2,
        [Description("拒绝")]
        拒绝 = 3,
        [Description("同意")]
        同意 = 4,
        [Description("修改桥要素")]
        修改桥要素 = 5,
        [Description("催ofr前台")]
        催ofr前台 = 6,
        [Description("催bid前台")]
        催bid前台 = 7,
        [Description("标记成交")]
        标记成交 = 8,
        [Description("修改交易要素")]
        修改交易要素 = 9,
        [Description("取消成交")]
        取消成交 = 10,
        [Description("已发单")]
        已发单 = 11,
        [Description("请求已回")]
        请求已回 = 12,
        [Description("推送SOR失败")]
        推送SOR失败 = 13,
    }

    /// <summary> 
    /// 拆单桥状态
    /// 0-待发送，1-待回复，2-已确认，3-已取消，4-请求已回，5-已发单，6-已完成 
    /// </summary>
    public enum ECDBridgeState
    {
        [Description("未知")]
        未知 = 999,
        [Description("待回复")]
        待发送 = 0,
        [Description("待回复")]
        待回复 = 1,
        [Description("已确认")]
        已确认 = 2,
        [Description("已取消")]
        已取消 = 3,
        [Description("请求已回")]
        请求已回 = 4,
        [Description("已发单")]
        已发单 = 5,
        [Description("已成交")]
        已完成 = 6,
        [Description("已转指令")]
        Transferred = 7,
    }
    /// <summary> 
    /// 桥状态: 0-待发送，1-待回复，2-已确认，3-已取消，4-已成交
    /// </summary>
    public enum EBridgeState
    {
        [Description("未知")]
        未知 = 999,
        [Description("待发送")]
        待发送 = 0,
        [Description("待回复")]
        待回复 = 1,
        [Description("已确认")]
        已确认 = 2,
        [Description("已取消")]
        已取消 = 3,
        [Description("已成交")]
        已成交 = 4,
    }
    /// <summary> 
    /// 桥状态: 0-待发送，1-待回复，2-已确认，3-已取消，4-已成交
    /// </summary>
    public enum BridgeState
    {
        /// <summary>
        /// 0-待发送，1-待回复，2-已确认，3-已取消，4-请求已回，5-已发单，6-已完成
        /// </summary>
        [Description("待回复")]
        ToSent = 0,
        [Description("待回复")]
        ToAnswered = 1,
        [Description("已确认")]
        Confirmed = 2,
        [Description("已取消")]
        Cancelled = 3,
        [Description("请求已回")]
        RequestReturned = 4,
        [Description("已发单")]
        IssuedOrder = 5,
        [Description("已成交")]
        Finished = 6,
        [Description("已转指令")]
        Transferred = 7,
    }

    /// <summary>
    /// 桥更新是否已查看
    /// </summary>
    public enum EBridgeisChecked
    {
        [Description("未知")]
        未知 = 999,
        [Description("以查看")]
        以查看 = 0,
        [Description("未查看")]
        未查看 = 1,
    }

    public enum ChannelEnum
    {

        [Description("对话")]
        Dialogue = 1,
        [Description("请求")]
        Request = 2,
        [Description("双边")]
        Bilateral = 3,
        [Description("xbond")]
        xbond = 4,
        [Description("iDeal")]
        iDeal = 5,
        [Description("固收")]
        FixedIncome = 6,
        [Description("竞价")]
        Bidding = 7,
        [Description("大宗")]
        Bulk = 8,
        [Description("意向")]
        Intention = 9,
        [Description("匿名")]
        Anonymity = 10,
        [Description("xbargain")]
        Xbargain = 11
    }

    /// <summary>
    /// 扩展枚举
    /// </summary>
    public static class EnumExt
    {
        private static ConcurrentDictionary<Enum, string> catchEnumDesc = new ConcurrentDictionary<Enum, string>();
        private static ConcurrentDictionary<Enum, string> catchEnumValue = new ConcurrentDictionary<Enum, string>();
        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            if (catchEnumDesc.ContainsKey(value))
                return catchEnumDesc[value];
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name == null)
            {
                catchEnumDesc[value] = "";
                return "";
            }

            FieldInfo field = type.GetField(name);
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            string v = attribute == null ? name : attribute.Description;
            catchEnumDesc[value] = v;
            return v;
        }


        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescriptionN(this Enum value)
        {
            if (catchEnumDesc.ContainsKey(value))
                return catchEnumDesc[value];
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name == null)
            {
                catchEnumDesc[value] = "";
                return "";
            }

            FieldInfo field = type.GetField(name);
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            string v = attribute == null ? name : attribute.Description;
            catchEnumDesc[value] = v;
            return v;
        }
        /// <summary>
        /// 获取char值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToChar(this Enum value)
        {
            if (catchEnumValue.ContainsKey(value))
                return catchEnumValue[value];
            System.Reflection.FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            object o = fieldInfo.GetValue(null);
            //支持整数
            try
            {
                char v = (char)(int)o;
                catchEnumValue[value] = v.ToString();
                return v.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
            //支持byte
            //try
            //{
            //    char v = (char)(byte)o;
            //    return v.ToString();
            //}
            //catch (Exception ex)
            //{

            //}

        }
        /// <summary>
        /// 判断跟描述是否相等
        /// </summary>
        /// <param name="value"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static bool EqualsToDesc(this Enum value, string desc)
        {
            string enumDesc = GetDescription(value);
            if (enumDesc.Equals(desc))
                return true;
            return false;
        }
        /// <summary>
        /// 枚举的值一般是整数：把整数值转换为string 再比较
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <param name="v">要比较的字符串</param>
        /// <returns></returns>
        public static bool EqualsToValue(this Enum value, string v)
        {
            if (string.IsNullOrEmpty(v))
                return false;
            string enumValue = ToChar(value);
            return enumValue.Equals(v) ? true : false;
        }
        /// <summary>
        /// 判断enum的名称是否相同
        /// </summary>
        /// <param name="value">枚举值</param>
        /// <param name="name">枚举值变量名</param>
        /// <returns></returns>
        public static bool EqualsToName(this Enum value, string name)
        {
            if (value.ToString().Equals(name))
                return true;
            return false;
        }




        /// <summary>
        /// 解决是否包含userID的bug 2020417
        /// </summary>
       
    }
    public static class StringExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool ContainsUserId(this string self, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;
            string parent = string.Format(",{0},", self);
            string child = string.Format(",{0},", userId);
            if (parent.Contains(child))
                return true;
            return false;
        }
    }

}
