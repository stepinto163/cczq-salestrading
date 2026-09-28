using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
	/// <summary>
	/// ttrd_acc_balance_secu
	/// </summary>
	/// <remarks>
	/// 2024.05.13: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
	/// </remarks>
	[Serializable]
	public class TtrdAccBalanceSecu
	{
		//#region 字段

		//private string _begDate;//有效日期
		//private string _iCode;//金融工具代码
		//private string _aType;//资产类型
		//private string _mType;//市场类型
		//private string _accId;//二级证券账户
		//private string _accName;//证券账户名称
		//private decimal? _fullPriceCost;//全价成本(万元)
		//private decimal? _netPriceCost;//净价成本(万元)
		//private decimal? _realizedProfitLoss;//实现损益(万元)
		//private decimal? _floatingProfitLoss;//浮动盈亏(万元)
		//private string _securityId;//(转换字段)证券ID
		//private decimal? _mzAmount;//自有面额(万元)

		//#endregion

		//#region 属性

		///// <summary>
		///// 有效日期
		///// </summary>
		//public string beg_date
		//{
		//	get { return _begDate; }
		//	set { _begDate = value; }
		//}

		///// <summary>
		///// 金融工具代码
		///// </summary>
		//public string i_code
		//{
		//	get { return _iCode; }
		//	set { _iCode = value; }
		//}

		///// <summary>
		///// 资产类型
		///// </summary>
		//public string a_type
		//{
		//	get { return _aType; }
		//	set { _aType = value; }
		//}

		///// <summary>
		///// 市场类型
		///// </summary>
		//public string m_type
		//{
		//	get { return _mType; }
		//	set { _mType = value; }
		//}

		///// <summary>
		///// 二级证券账户
		///// </summary>
		//public string acc_id
		//{
		//	get { return _accId; }
		//	set { _accId = value; }
		//}

		///// <summary>
		///// 证券账户名称
		///// </summary>
		//public string acc_name
		//{
		//	get { return _accName; }
		//	set { _accName = value; }
		//}

		///// <summary>
		///// 全价成本(万元)
		///// </summary>
		//public decimal? full_price_cost
		//{
		//	get { return _fullPriceCost; }
		//	set { _fullPriceCost = value; }
		//}

		///// <summary>
		///// 净价成本(万元)
		///// </summary>
		//public decimal? net_price_cost
		//{
		//	get { return _netPriceCost; }
		//	set { _netPriceCost = value; }
		//}

		///// <summary>
		///// 实现损益(万元)
		///// </summary>
		//public decimal? realized_profit_loss
		//{
		//	get { return _realizedProfitLoss; }
		//	set { _realizedProfitLoss = value; }
		//}

		///// <summary>
		///// 浮动盈亏(万元)
		///// </summary>
		//public decimal? floating_profit_loss
		//{
		//	get { return _floatingProfitLoss; }
		//	set { _floatingProfitLoss = value; }
		//}

		///// <summary>
		///// (转换字段)证券ID
		///// </summary>
		//public string security_id
		//{
		//	get { return _securityId; }
		//	set { _securityId = value; }
		//}

		///// <summary>
		///// 自有面额(万元)
		///// </summary>
		//public decimal? mz_amount
		//{
		//	get { return _mzAmount; }
		//	set { _mzAmount = value; }
		//}



		//#endregion


		#region 字段

		private string _begDate;//持仓日期
		private string _secuAccid;//内部证券账户
		private string _accname;//内部证券账户名称
		private string _iCode;//资产代码
		private string _aType;//资产类型
		private string _mType;//市场
		private decimal? _secuAmount;//持仓量
		private decimal? _secuAmountT1;//持仓量(T+1)
		private decimal? _secuCost;//全价成本（元）
		private decimal? _secuCostNetprice;//净价成本（元）
		private decimal? _secuActualPl;//已实现损益（元）
		private decimal? _chgFv;//公允价值变动（元）
		private string _updateTime;//更新时间
		private string _securityId;//债券ID，T2409.IB

		private decimal? _sumamount;

		#endregion

		#region 属性

		/// <summary>
		/// 持仓日期
		/// </summary>
		public string Beg_Date
		{
			get { return _begDate; }
			set { _begDate = value; }
		}

		/// <summary>
		/// 内部证券账户
		/// </summary>
		public string Secu_Accid
		{
			get { return _secuAccid; }
			set { _secuAccid = value; }
		}

		/// <summary>
		/// 内部证券账户名称
		/// </summary>
		public string Accname
		{
			get { return _accname; }
			set { _accname = value; }
		}

		/// <summary>
		/// 资产代码
		/// </summary>
		public string I_code
		{
			get { return _iCode; }
			set { _iCode = value; }
		}

		/// <summary>
		/// 资产类型
		/// </summary>
		public string A_type
		{
			get { return _aType; }
			set { _aType = value; }
		}

		/// <summary>
		/// 市场
		/// </summary>
		public string M_type
		{
			get { return _mType; }
			set { _mType = value; }
		}

		/// <summary>
		/// 持仓量
		/// </summary>
		public decimal? Secu_Amount
		{
			get { return _secuAmount; }
			set { _secuAmount = value; }
		}

		/// <summary>
		/// 持仓量(T+1)
		/// </summary>
		public decimal? Secu_AmountT1
		{
			get { return _secuAmountT1; }
			set { _secuAmountT1 = value; }
		}

		/// <summary>
		/// 全价成本（元）
		/// </summary>
		public decimal? Secu_Cost
		{
			get { return _secuCost; }
			set { _secuCost = value; }
		}

		/// <summary>
		/// 净价成本（元）
		/// </summary>
		public decimal? Secu_Cost_Netprice
		{
			get { return _secuCostNetprice; }
			set { _secuCostNetprice = value; }
		}

		/// <summary>
		/// 已实现损益（元）
		/// </summary>
		public decimal? Secu_Actual_Pl
		{
			get { return _secuActualPl; }
			set { _secuActualPl = value; }
		}

		/// <summary>
		/// 公允价值变动（元）
		/// </summary>
		public decimal? Chg_Fv
		{
			get { return _chgFv; }
			set { _chgFv = value; }
		}

		/// <summary>
		/// 更新时间
		/// </summary>
		public string Update_Time
		{
			get { return _updateTime; }
			set { _updateTime = value; }
		}

		/// <summary>
		/// 债券ID，T2409.IB
		/// </summary>
		public string Security_Id
		{
			get { return _securityId; }
			set { _securityId = value; }
		}

		public decimal? Sum_amount
		{
			get { return _sumamount; }
			set { _sumamount = value; }
		}


		#endregion

	}
}
