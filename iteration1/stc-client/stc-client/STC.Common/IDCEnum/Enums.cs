using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// add 20200214 统一数据类型
/// </summary>
namespace STC.Common.IDCEnum
{
    /// <summary>
    /// 债券类别
    /// Goodstype
    /// 债券类型:1 信用,2 利率,11 ABS,12 NCD
    /// </summary>
    public enum EGoodsType
    {
        未知 = 0,
        /// <summary>
        /// bco
        /// </summary>
        信用 = 1,
        /// <summary>
        /// bnc
        /// </summary>
        利率 = 2,
        ABS = 11,
        NCD = 12
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
    /// O：新意向 E&I&G:确认 F&H：拒绝 C：撤销
    /// </summary>
    public enum EOrderStatus
    {
        //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
        //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
        已申请 = 'N',
        处理中 = '0',
        部分确认 = '1',
        完全确认 = '2',
        已撤销 = '4',
        失败 = '8',
        BrokerB确认 = 'E',
        BrokerB拒绝 = 'F',
        BrokerA确认 = 'G',
        BrokerA拒绝 = 'H',
        撤销 = 'C'
    }
    //if ("0".Equals(orderStatus))
    //{
    //    result = "新意向";
    //}
    //else if ("E".Equals(orderStatus))
    //{
    //    if (IsPartlyConfirm)
    //    {
    //        result = "部分确认";
    //    }
    //    else
    //    {
    //        result = "全部确认";
    //    }
    //}
    //else if ("F".Equals(orderStatus))
    //{
    //    result = "拒绝";
    //}
    //else if ("H".Equals(orderStatus))
    //{
    //    result = "拒绝";
    //}
    //else if ("C".Equals(orderStatus))
    //{
    //    result = "交易撤销";
    //}
    //else if ("I".Equals(orderStatus))
    //{
    //    result = "确认";
    //}
    ////客户端自定义状态显示
    //else if ("a".Equals(orderStatus))
    //    result = "修改债券";
    //else if ("b".Equals(orderStatus))
    //    result = "修改价格";
    //else if ("c".Equals(orderStatus))
    //    result = "修改数量";
    //else if ("d".Equals(orderStatus))
    //    result = "修改结算";
    //else if ("e".Equals(orderStatus))
    //    result = "修改交易员";
    //else if ("f".Equals(orderStatus))
    //    result = "修改Broker";
    //else if ("g".Equals(orderStatus))
    //    result = "修改发给对象";
    //else if ("h".Equals(orderStatus))
    //    result = "添加桥";
    //else if ("i".Equals(orderStatus))
    //    result = "修改桥要素";
    //else if ("j".Equals(orderStatus))
    //    result = "删除桥";
    //else if ("k".Equals(orderStatus))
    //    result = "修改价格/量";
    //else if ("l".Equals(orderStatus))
    //    result = "修改返点";
    //else if ("m".Equals(orderStatus))
    //    result = "DCS";
    //else if ("n".Equals(orderStatus))
    //    result = "修改桥状态";
    //else if ("G".Equals(orderStatus))
    //{
    //    //if ((IsChangeAmount || IsChangePrice) && string.IsNullOrEmpty(dcsflag))
    //    //{
    //    //    result = "修改";
    //    //}
    //    if (string.IsNullOrEmpty(brokerunread) || (!brokerunread.Contains(brokerIdA) && !brokerunread.Contains(brokerIdB)))
    //    {
    //        result = "确认";
    //    }
    //}
    /// <summary>
    /// 债券类别
    /// </summary>
    public enum EBondType
    {
        /// <summary>
        /// 利率
        /// </summary>
        BNC,
        /// <summary>
        /// NCD
        /// </summary>
        NCD,
        /// <summary>
        /// 信用
        /// </summary>
        BCO,
        /// <summary>
        /// ABS
        /// </summary>
        ABS,
    }
    /// <summary>
    /// 系统状态
    /// </summary>
    public enum ESystemStatus
    {
        开市 = 'O', 闭市 = 'C'
    }
    /// <summary>
    /// 状态
    /// </summary>
    public enum EMatchingStatus
    {
        预热 = 0, 进行 = 1, 失效 = 2, 删除 = 3, 匹配 = 4, 完成 = 5
    }
    /// <summary>
    /// 价格显示设置
    /// </summary>
    public enum EPriceShow
    {
        不显示 = 1, 显示2列 = 2, 显示4列 = 3
    }
    ///// <summary>
    ///// 报价方向
    ///// </summary>
    //public enum EDirection
    //{
    //    Offer = 0,
    //    Bid = 1
    //}
    ///// <summary>
    ///// 报价方向
    ///// </summary>
    public enum EQuoteSide
    {
        [Description("-1")]
        Ofr = -1,
        [Description("1")]
        Bid = 1,
    }
    ///// <summary>
    ///// 报价方向
    ///// </summary>
    public enum EBridgeType
    {
        [Description("B")]
        B = 'B',
        [Description("O")]
        O = 'O',
        [Description("B,O")]
        BO = B + O,
    }
    
    /// <summary>
    /// 点价是自动模式还是手动模式
    /// 在数据库idc_broker_client_default_rule表中手工修改
    /// </summary>
    public enum EManualMode
    {
        [Description("MR")]
        手动 = 0,
        [Description("TP")]
        自动TP = 1,
        [Description("PA")]
        自动PA = 2,
        [Description("MA")]
        线下 = 3,

    }

    public class SysConsts
    {
        /// <summary>
        /// 点价-GVN
        /// </summary>
        public static readonly string CLICKRICEGVN = "点价-GVN";
        public static readonly string CLICKRICETKN = "点价-TKN";

    }
    /// <summary>
    /// 结算方式
    /// </summary>
    public class SettlementTypeConsts
    {
        public const string TODAYADDZERO = "+0";
        public const string TOMORROWADDZERO = "明天+0";
        public const string TOMORROW0ADDONE = "明天+1";
        public const string TODAYADDONE = "+1";
    }
    /// <summary>
    /// 扩展枚举
    /// </summary>
    public static class EnumExt
    {
        private static ConcurrentDictionary<Enum, string> catchEnumDesc = new ConcurrentDictionary<Enum, string>();
        private static ConcurrentDictionary<Enum, string> catchEnumValue = new ConcurrentDictionary<Enum, string>();
        /////// <summary>
        /////// 获取枚举描述
        /////// </summary>
        /////// <param name="value"></param>
        /////// <returns></returns>
        ////public static string GetDescription(this Enum value)
        ////{
        ////    if (catchEnumDesc.ContainsKey(value))
        ////        return catchEnumDesc[value];
        ////    Type type = value.GetType();
        ////    string name = Enum.GetName(type, value);
        ////    if (name == null)
        ////    {
        ////        catchEnumDesc[value] = "";
        ////        return "";
        ////    }

        ////    FieldInfo field = type.GetField(name);
        ////    DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        ////    string v = attribute == null ? name : attribute.Description;
        ////    catchEnumDesc[value] = v;
        ////    return v;
        ////}
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
            string enumDesc = ExtensionHelper.GetOldDescription(value);
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
    }
}
