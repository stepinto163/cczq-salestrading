using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
	/// <summary>
	/// 国联-交易流水
	/// </summary>
	/// <remarks>
	/// 2024.06.27: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
	/// </remarks>
	[Serializable]
	public class TrptOtcTradeAll
	{
		#region 字段

		private int? _sysordid;//
		private int? _fstInstId;//
		private string _extordid;//外部成交编号
		private string _secuAccid;//内部证券
		private string _secuName;//证券名称
		private string _secuExtAccid;//外部证券账户
		private string _secuExtName;//外部证券名称
		private string _iCode;//债券代码
		private string _aType;//资产类型
		private string _mType;//市场
		private string _trdtype;//交易类型
		private decimal? _ordprice;//交易价格
		private decimal? _bndYtm;//到期收益率
		private decimal? _bndYtmOe;//行权收益率
		private decimal? _ordamount;//交易金额
		private decimal? _ordcount;//交易数量
		private string _orddate;//交易日
		private int? _setdays;//清算速度
		private string _setdate;//结算日
		private int? _partyid;//交易对手
		private string _partyname;//交易对手名称
		private decimal? _bndNetprice;//净价
		private string _pClass;//分类
		private string _updateTime;//交易更新时间
		private string _flag;//是否撤销
		private string _securityId;//债券ID

		private string _bookid;
		private string _bookname;
		

		#endregion

		#region 属性

		/// <summary>
		/// 
		/// </summary>
		public int? Sysordid
		{
			get { return _sysordid; }
			set { _sysordid = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int? Fst_Inst_Id
		{
			get { return _fstInstId; }
			set { _fstInstId = value; }
		}

		/// <summary>
		/// 外部成交编号
		/// </summary>
		public string Extordid
		{
			get { return _extordid; }
			set { _extordid = value; }
		}

		/// <summary>
		/// 内部证券
		/// </summary>
		public string Secu_Accid
		{
			get { return _secuAccid; }
			set { _secuAccid = value; }
		}

		/// <summary>
		/// 证券名称
		/// </summary>
		public string Secu_Name
		{
			get { return _secuName; }
			set { _secuName = value; }
		}

		/// <summary>
		/// 外部证券账户
		/// </summary>
		public string Secu_Ext_Accid
		{
			get { return _secuExtAccid; }
			set { _secuExtAccid = value; }
		}

		/// <summary>
		/// 外部证券名称
		/// </summary>
		public string Secu_Ext_Name
		{
			get { return _secuExtName; }
			set { _secuExtName = value; }
		}

		/// <summary>
		/// 债券代码
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
		/// 交易类型
		/// </summary>
		public string Trdtype
		{
			get { return _trdtype; }
			set { _trdtype = value; }
		}

		/// <summary>
		/// 交易价格
		/// </summary>
		public decimal? Ordprice
		{
			get { return _ordprice; }
			set { _ordprice = value; }
		}

		/// <summary>
		/// 到期收益率
		/// </summary>
		public decimal? Bnd_Ytm
		{
			get { return _bndYtm; }
			set { _bndYtm = value; }
		}

		/// <summary>
		/// 行权收益率
		/// </summary>
		public decimal? Bnd_Ytm_Oe
		{
			get { return _bndYtmOe; }
			set { _bndYtmOe = value; }
		}

		/// <summary>
		/// 交易金额
		/// </summary>
		public decimal? Ordamount
		{
			get { return _ordamount; }
			set { _ordamount = value; }
		}

		/// <summary>
		/// 交易数量
		/// </summary>
		public decimal? Ordcount
		{
			get { return _ordcount; }
			set { _ordcount = value; }
		}

		/// <summary>
		/// 交易日
		/// </summary>
		public string Orddate
		{
			get { return _orddate; }
			set { _orddate = value; }
		}

		/// <summary>
		/// 清算速度
		/// </summary>
		public int? Setdays
		{
			get { return _setdays; }
			set { _setdays = value; }
		}

		/// <summary>
		/// 结算日
		/// </summary>
		public string Setdate
		{
			get { return _setdate; }
			set { _setdate = value; }
		}

		/// <summary>
		/// 交易对手
		/// </summary>
		public int? Partyid
		{
			get { return _partyid; }
			set { _partyid = value; }
		}

		/// <summary>
		/// 交易对手名称
		/// </summary>
		public string Partyname
		{
			get { return _partyname; }
			set { _partyname = value; }
		}

		/// <summary>
		/// 净价
		/// </summary>
		public decimal? Bnd_Netprice
		{
			get { return _bndNetprice; }
			set { _bndNetprice = value; }
		}

		/// <summary>
		/// 分类
		/// </summary>
		public string P_class
		{
			get { return _pClass; }
			set { _pClass = value; }
		}

		/// <summary>
		/// 交易更新时间
		/// </summary>
		public string Update_Time
		{
			get { return _updateTime; }
			set { _updateTime = value; }
		}

		/// <summary>
		/// 是否撤销
		/// </summary>
		public string Flag
		{
			get { return _flag; }
			set { _flag = value; }
		}

		/// <summary>
		/// 债券ID
		/// </summary>
		public string Security_Id
		{
			get { return _securityId; }
			set { _securityId = value; }
		}


		public string bookid
		{
			get { return _bookid; }
			set { _bookid = value; }
		}
		public string bookname
		{
			get { return _bookname; }
			set { _bookname = value; }
		}


		#endregion

	}
}