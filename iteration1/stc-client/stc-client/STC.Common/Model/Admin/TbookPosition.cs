using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.Model.Admin
{
	/// <summary>
	/// 持仓表
	/// </summary>
	/// <remarks>
	/// 2024.05.13: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
	/// </remarks>
	[Serializable]
	public class TbookPosition
	{
		#region 字段

		private string _bookid = "";//组合id
		private string _securityid = "";//产品代码
		private string _symbol;//产品名称
		private string _symboltype;//产品类型
		private string _marketindicator;//市场
		private decimal? _netprice;//最新净价
		private decimal? _netavgprice;//净平均价格
		private decimal? _netavgyield;//净平均收益率
		private string _netpricesource;//净价来源 1-中债  2-成交
		private decimal? _longposition;//买持仓
		private decimal? _longpricetotal;//长价总计
		private decimal? _longavgprice;//买持仓均价
		private decimal? _shortposition;//卖持仓
		private decimal? _shortpricetotal;//短价总计
		private decimal? _shortavgprice;//卖持仓均价
		private decimal? _netposition;//净持仓
		private decimal _totalpl = 0M;//总盈亏
		private decimal? _closingpl;//平仓盈亏
		private decimal? _positionpl;//持仓盈亏
		private string _traderdate;//交易日期
		private string _createtime;//创建时间
		private string _updatetime;//更新时间
		private string _inf1;//扩展字段1
		private string _inf2;//扩展字段2
		private string _inf3;//扩展字段3
		private string _inf4;//扩展字段4
		private string _inf5;//扩展字段5
		private decimal? _todaylongposition;//今日持仓（多）
		private decimal? _todayshortposition;//今日持仓（空）
		private decimal? _ytdsettlementprice;//昨日结算价
		private decimal? _dailynetposition;//今日净持仓
		private decimal? _initialposition;//日间初始头寸
		private string _settldate = "";//交割日
		private string _settltype;//清算速度
		private decimal? _availableposition;//可用持仓
		private decimal? _cumavaliableposition;//累计可用持仓
		private decimal? _frozenamount;//冻结额度
		private string _longyieldtotal;//多头收益率总额
		private string _shortyieldtotal;//空头收益率总额
		private string _dayendposition;//日末持仓，当日持仓+日初持仓，T0累计
		#endregion

		//用于界面显示
		public string BookName { get; set; }
		public decimal LongPosition { get; set; }

		#region 属性

		/// <summary>
		/// 组合id
		/// </summary>
		public string Bookid
		{
			get { return _bookid; }
			set { _bookid = value; }
		}

		/// <summary>
		/// 产品代码
		/// </summary>
		public string Securityid
		{
			get { return _securityid; }
			set { _securityid = value; }
		}

		/// <summary>
		/// 产品名称
		/// </summary>
		public string Symbol
		{
			get { return _symbol; }
			set { _symbol = value; }
		}

		/// <summary>
		/// 产品类型
		/// </summary>
		public string Symboltype
		{
			get { return _symboltype; }
			set { _symboltype = value; }
		}

		/// <summary>
		/// 市场
		/// </summary>
		public string Marketindicator
		{
			get { return _marketindicator; }
			set { _marketindicator = value; }
		}

		/// <summary>
		/// 最新净价
		/// </summary>
		public decimal? Netprice
		{
			get { return _netprice; }
			set { _netprice = value; }
		}

		/// <summary>
		/// 净平均价格
		/// </summary>
		public decimal? Netavgprice
		{
			get { return _netavgprice; }
			set { _netavgprice = value; }
		}

		/// <summary>
		/// 净平均收益率
		/// </summary>
		public decimal? Netavgyield
		{
			get { return _netavgyield; }
			set { _netavgyield = value; }
		}

		/// <summary>
		/// 净价来源 1-中债  2-成交
		/// </summary>
		public string Netpricesource
		{
			get { return _netpricesource; }
			set { _netpricesource = value; }
		}

		/// <summary>
		/// 买持仓
		/// </summary>
		public decimal? Longposition
		{
			get { return _longposition; }
			set { _longposition = value; }
		}

		/// <summary>
		/// 长价总计
		/// </summary>
		public decimal? Longpricetotal
		{
			get { return _longpricetotal; }
			set { _longpricetotal = value; }
		}

		/// <summary>
		/// 买持仓均价
		/// </summary>
		public decimal? Longavgprice
		{
			get { return _longavgprice; }
			set { _longavgprice = value; }
		}

		/// <summary>
		/// 卖持仓
		/// </summary>
		public decimal? Shortposition
		{
			get { return _shortposition; }
			set { _shortposition = value; }
		}

		/// <summary>
		/// 短价总计
		/// </summary>
		public decimal? Shortpricetotal
		{
			get { return _shortpricetotal; }
			set { _shortpricetotal = value; }
		}

		/// <summary>
		/// 卖持仓均价
		/// </summary>
		public decimal? Shortavgprice
		{
			get { return _shortavgprice; }
			set { _shortavgprice = value; }
		}

		/// <summary>
		/// 净持仓
		/// </summary>
		public decimal? Netposition
		{
			get { return _netposition; }
			set { _netposition = value; }
		}

		/// <summary>
		/// 总盈亏
		/// </summary>
		public decimal Totalpl
		{
			get { return _totalpl; }
			set { _totalpl = value; }
		}

		/// <summary>
		/// 平仓盈亏
		/// </summary>
		public decimal? Closingpl
		{
			get { return _closingpl; }
			set { _closingpl = value; }
		}

		/// <summary>
		/// 持仓盈亏
		/// </summary>
		public decimal? Positionpl
		{
			get { return _positionpl; }
			set { _positionpl = value; }
		}

		/// <summary>
		/// 交易日期
		/// </summary>
		public string Traderdate
		{
			get { return _traderdate; }
			set { _traderdate = value; }
		}

		/// <summary>
		/// 创建时间
		/// </summary>
		public string Createtime
		{
			get { return _createtime; }
			set { _createtime = value; }
		}

		/// <summary>
		/// 更新时间
		/// </summary>
		public string Updatetime
		{
			get { return _updatetime; }
			set { _updatetime = value; }
		}

		/// <summary>
		/// 扩展字段1
		/// </summary>
		public string Inf1
		{
			get { return _inf1; }
			set { _inf1 = value; }
		}

		/// <summary>
		/// 扩展字段2
		/// </summary>
		public string Inf2
		{
			get { return _inf2; }
			set { _inf2 = value; }
		}

		/// <summary>
		/// 扩展字段3
		/// </summary>
		public string Inf3
		{
			get { return _inf3; }
			set { _inf3 = value; }
		}

		/// <summary>
		/// 扩展字段4
		/// </summary>
		public string Inf4
		{
			get { return _inf4; }
			set { _inf4 = value; }
		}

		/// <summary>
		/// 扩展字段5
		/// </summary>
		public string Inf5
		{
			get { return _inf5; }
			set { _inf5 = value; }
		}

		/// <summary>
		/// 今日持仓（多）
		/// </summary>
		public decimal? Todaylongposition
		{
			get { return _todaylongposition; }
			set { _todaylongposition = value; }
		}

		/// <summary>
		/// 今日持仓（空）
		/// </summary>
		public decimal? Todayshortposition
		{
			get { return _todayshortposition; }
			set { _todayshortposition = value; }
		}

		/// <summary>
		/// 昨日结算价
		/// </summary>
		public decimal? Ytdsettlementprice
		{
			get { return _ytdsettlementprice; }
			set { _ytdsettlementprice = value; }
		}

		/// <summary>
		/// 今日净持仓
		/// </summary>
		public decimal? Dailynetposition
		{
			get { return _dailynetposition; }
			set { _dailynetposition = value; }
		}

		/// <summary>
		/// 日间初始头寸
		/// </summary>
		public decimal? Initialposition
		{
			get { return _initialposition; }
			set { _initialposition = value; }
		}

		/// <summary>
		/// 交割日
		/// </summary>
		public string Settldate
		{
			get { return _settldate; }
			set { _settldate = value; }
		}

		/// <summary>
		/// 清算速度
		/// </summary>
		public string Settltype
		{
			get { return _settltype; }
			set { _settltype = value; }
		}

		/// <summary>
		/// 可用持仓
		/// </summary>
		public decimal? Availableposition
		{
			get { return _availableposition; }
			set { _availableposition = value; }
		}

		/// <summary>
		/// 累计可用持仓
		/// </summary>
		public decimal? Cumavaliableposition
		{
			get { return _cumavaliableposition; }
			set { _cumavaliableposition = value; }
		}

		/// <summary>
		/// 冻结额度
		/// </summary>
		public decimal? Frozenamount
		{
			get { return _frozenamount; }
			set { _frozenamount = value; }
		}

		/// <summary>
		/// 多头收益率总额
		/// </summary>
		public string Longyieldtotal
		{
			get { return _longyieldtotal; }
			set { _longyieldtotal = value; }
		}

		/// <summary>
		/// 空头收益率总额
		/// </summary>
		public string Shortyieldtotal
		{
			get { return _shortyieldtotal; }
			set { _shortyieldtotal = value; }
		}

		/// <summary>
		/// 日末持仓，当日持仓+日初持仓，T0累计
		/// </summary>
		public string Dayendposition
		{
			get { return _dayendposition; }
			set { _dayendposition = value; }
		}


		#endregion

	}
}