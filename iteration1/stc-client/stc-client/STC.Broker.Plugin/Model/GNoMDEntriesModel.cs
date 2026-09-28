using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Models
{
    public class GNoMDEntriesModel : BaseViewModel
    {
        #region 定义
        private string _Key = "";
        public string Key { get { return _Key; } set { _Key = value; this.OnPropertyChanged(); } }

        private string _SecurityID = "";

        public string SecurityID { get { return _SecurityID; } set { _SecurityID = value; this.OnPropertyChanged(); } }


        private decimal? _TotalQty;
        public decimal? TotalQty { get { return _TotalQty; } set { _TotalQty = value; this.OnPropertyChanged(); } }

        private string _Time = "";

        public string Time { get { return _Time; } set { _Time = value; this.OnPropertyChanged(); } }


        private string _Source = "";
        public string Source { get { return _Source; } set { _Source = value; this.OnPropertyChanged(); } }

        private decimal? _Qty;
        public decimal? Qty { get { return _Qty; } set { _Qty = value; this.OnPropertyChanged(); } }


        private string _EntryPx = "";
        /// <summary>
        /// 用户输入的原始价
        /// </summary>
        public string EntryPx { get { return _EntryPx; } set { _EntryPx = value; this.OnPropertyChanged(); } }

        private object _NetPrice;
        /// <summary>
        /// 净价
        /// </summary>
        public object NetPrice { get { return _NetPrice; } set { _NetPrice = value; this.OnPropertyChanged(); } }


        private string _Yield = "";
        /// <summary>
        /// 收益率
        /// </summary>
        public string Yield { get { return _Yield; } set { _Yield = value; this.OnPropertyChanged(); } }



        private string _SettlType = "";
        /// <summary>
        /// 清算速度
        /// </summary>
        public string SettlType { get { return _SettlType; } set { _SettlType = value; this.OnPropertyChanged(); } }


        private string _SettlType_yuan = "";
        /// <summary>
        /// 远期清算速度
        /// </summary>

        public string SettlType_yuan { get { return _SettlType_yuan; } set { _SettlType_yuan = value; this.OnPropertyChanged(); } }

        private string _Side = "";
        /// <summary>
        /// 方向
        /// </summary>
        public string Side { get { return _Side; } set { _Side = value; this.OnPropertyChanged(); } }

        private bool _IsBest = false;
        /// <summary>
        /// 最优价
        /// </summary>
        public bool IsBest { get { return _IsBest; } set { _IsBest = value; this.OnPropertyChanged(); } }

        private string _FlagType = "";
        /// <summary>
        /// 角标，重新计算
        /// </summary>
        public string FlagType { get { return _FlagType; } set { _FlagType = value; this.OnPropertyChanged(); } }


        private string _DeleteFlag = "0";
        /// <summary>
        ///  1要删除 0 新纪录
        /// </summary>
        public string DeleteFlag { get { return _DeleteFlag; } set { _DeleteFlag = value; this.OnPropertyChanged(); } }

        private string _FirmFlag = "";
        /// <summary>
        ///  意向型， 重新计算
        /// </summary>
        public string FirmFlag { get { return _FirmFlag; } set { _FirmFlag = value; this.OnPropertyChanged(); } }

        private string _VenueTypeGW = "";
        /// <summary>
        /// 
        /// </summary>
        /// 
        public string VenueTypeGW { get { return _VenueTypeGW; } set { _VenueTypeGW = value; this.OnPropertyChanged(); } }


        private string _Text = "";
        /// <summary>
        /// 角标内容
        /// </summary>

        public string Text { get { return _Text; } set { _Text = value; this.OnPropertyChanged(); } }

        private int _MDPriceLevel = 0;
        /// <summary>
        /// 优先顺序
        /// </summary>
        public int MDPriceLevel { get { return _MDPriceLevel; } set { _MDPriceLevel = value; this.OnPropertyChanged(); } }

        private string _PartySubName = "";
        /// <summary>
        /// 本方交易员名
        /// </summary>
        public string PartySubName { get { return _PartySubName; } set { _PartySubName = value; this.OnPropertyChanged(); } }


        private string _BargainFlag = "";
        /// <summary>
        /// 询问标记
        /// </summary>
        public string BargainFlag { get { return _BargainFlag; } set { _BargainFlag = value; this.OnPropertyChanged(); } }

        private string _HidePxFlag = "";
        /// <summary>
        /// 是否隐藏价格，0：不隐藏 1：隐藏(此字段只有主表使用)
        /// </summary>
        public string HidePxFlag { get { return _HidePxFlag; } set { _HidePxFlag = value; this.OnPropertyChanged(); } }

        private string _HideNetPriceFlag = "";
        /// <summary>
        /// 是否隐藏净价，0：不隐藏 1：隐藏(此字段只有主表使用)
        /// </summary>

        public string HideNetPriceFlag { get { return _HideNetPriceFlag; } set { _HideNetPriceFlag = value; this.OnPropertyChanged(); } }

        private string _WholeFlag = "";
        /// <summary>
        /// 是否是整量
        /// </summary>
        public string WholeFlag { get { return _WholeFlag; } set { _WholeFlag = value; this.OnPropertyChanged(); } }

        private string _InstitutionName = "";
        /// <summary>
        /// 机构名称
        /// </summary>
        public string InstitutionName { get { return _InstitutionName; } set { _InstitutionName = value; this.OnPropertyChanged(); } }


        private string _ShowTrader = "";
        /// <summary>
        /// 是否显示机构名称图标
        /// </summary>
        public string ShowTrader { get { return _ShowTrader; } set { _ShowTrader = value; this.OnPropertyChanged(); } }

        private bool _IsMe = false;
        /// <summary>
        /// 是否是我的
        /// </summary>
        public bool IsMe { get { return _IsMe; } set { _IsMe = value; this.OnPropertyChanged(); } }

        private string _TraderName = "";
        /// <summary>
        /// 交易员名称
        /// </summary>
        public string TraderName { get { return _TraderName; } set { _TraderName = value; this.OnPropertyChanged(); } }

        private string _TransactTime = "";
        /// <summary>
        /// 报价时间 20210714 16:56:20.103
        /// </summary>
        public string TransactTime { get { return _TransactTime; } set { _TransactTime = value; this.OnPropertyChanged(); } }

        private string _DeliveryType = "";
        /// <summary>
        /// 
        /// </summary>
        public string DeliveryType { get { return _DeliveryType; } set { _DeliveryType = value; this.OnPropertyChanged(); } }

        private string _ClearingMethod = "";
        /// <summary>
        /// 
        /// </summary>
        public string ClearingMethod { get { return _ClearingMethod; } set { _ClearingMethod = value; this.OnPropertyChanged(); } }

        private string _QuoteType = "";
        /// <summary>
        /// 
        /// </summary>
        public string QuoteType { get { return _QuoteType; } set { _QuoteType = value; this.OnPropertyChanged(); } }

        private string _CustomerSource = "";
        /// <summary>
        /// 自定义来源
        /// </summary>
        public string CustomerSource { get { return _CustomerSource; } set { _CustomerSource = value; this.OnPropertyChanged(); } }

        private string _PartyName = "";
        /// <summary>
        /// 机构名称
        /// </summary>
        public string PartyName { get { return _PartyName; } set { _PartyName = value; this.OnPropertyChanged(); } }

        private string _PartyID = "";
        /// <summary>
        /// 机构名称
        /// </summary>
       public string PartyID { get { return _PartyID; } set { _PartyID = value; this.OnPropertyChanged(); } }

        private bool _IsPermission = false;
        /// <summary>
        /// 是否有权限
        /// </summary>
        public bool IsPermission { get { return _IsPermission; } set { _IsPermission = value; this.OnPropertyChanged(); } }

        private bool _IsMore = false;
        /// <summary>
        /// 是否显示多结算速度
        /// </summary>
        public bool IsMore { get { return _IsMore; } set { _IsMore = value; this.OnPropertyChanged(); } }

        private string _QuoteStatus = "";
        /// <summary>
        /// 报价状态
        /// </summary>
        public string QuoteStatus { get { return _QuoteStatus; } set { _QuoteStatus = value; this.OnPropertyChanged(); } }

        private string _VenueSort = "";
        /// <summary>
        /// 个人设置自定义Venue顺序
        /// </summary>
        public string VenueSort { get { return _VenueSort; } set { _VenueSort = value; this.OnPropertyChanged(); } }

        private string _IsInner = "";
        /// <summary>
        /// 是否是内部
        /// </summary>
        public string IsInner { get { return _IsInner; } set { _IsInner = value; this.OnPropertyChanged(); } }

        private string _MarketIndicator = "";
        /// <summary>
        /// 是否是内部 5期货 4债券
        /// </summary>
        public string MarketIndicator { get { return _MarketIndicator; } set { _MarketIndicator = value; this.OnPropertyChanged(); } }

        private string _MinChange = "";
        /// <summary>
        /// 价格最小变动单位
        /// </summary>

        public string MinChange { get { return _MinChange; } set { _MinChange = value; this.OnPropertyChanged(); } }

        private string _CalFlag = "";
        /// <summary>
        /// 试算标记 2-收益率反算净价  4-净价反算收益率  3-全价反算收益率和净价 5-利差反算收益率和净价 0-原始 
        /// </summary>
        public string CalFlag { get { return _CalFlag; } set { _CalFlag = value; this.OnPropertyChanged(); } }

        private string _PriceType = "";
        /// <summary>
        /// 类型  0-意向；1-净价；2-全价；3-收益率；4-利差；8-经纪商未指定 如果没有输入则为 3-收益率
        /// </summary>
        public string PriceType { get { return _PriceType; } set { _PriceType = value; this.OnPropertyChanged(); } }


        private string _CalFlagYieldTip = "";
        /// <summary>
        /// ToolTip
        /// </summary>
       public string CalFlagYieldTip { get { return _CalFlagYieldTip; } set { _CalFlagYieldTip = value; this.OnPropertyChanged(); } }

        private string _CalFlagNetPriceTip = "";
        /// <summary>
        /// ToolTip
        /// </summary>
        public string CalFlagNetPriceTip { get { return _CalFlagNetPriceTip; } set { _CalFlagNetPriceTip = value; this.OnPropertyChanged(); } }

        private string _IsIncomplete = "";
        /// <summary>
        /// 是否是缺损报价
        /// </summary>
        public string IsIncomplete { get { return _IsIncomplete; } set { _IsIncomplete = value; this.OnPropertyChanged(); } }

        private string _IsInnerInComplete = "";
        /// <summary>
        /// 是否是内部无价无量订单
        /// </summary>
        public string IsInnerInComplete { get { return _IsInnerInComplete; } set { _IsInnerInComplete = value; this.OnPropertyChanged(); } }
        private string _ExerciseFlag = "";
        /// <summary>
        /// 0=行权收益率,1或空=到期收益率
        /// </summary>
        public string ExerciseFlag { get { return _ExerciseFlag; } set { _ExerciseFlag = value; this.OnPropertyChanged(); } }

        #endregion
    }
}
