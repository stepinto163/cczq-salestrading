using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    /// <summary>
    /// 量化策略
    /// </summary>
    /// <remarks>
    /// 2021.10.18: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
    /// </remarks>
    [Serializable]
    public  class TTbookQuantStrategys
    {
		#region 字段

		private string _id = "";//数据标识
		private string _groupid = "";//量化分组id
		private string _quantid = "";//量化编号（code）
		private string _quantname = "";//策略名
		private string _username;//stc用户
		private string _type;//类型：1 模版
		private string _enable;//开关标记1开关，0关闭
		private string _terminal;//终端
		private string _ip;//服务器ip
		private string _closeby;//操作人
		private string _remark;//说明
		private string _action;//
		private string _createtime;//创建时间
		private string _updatetime;//更新时间
		private string _inf1;//扩展字段1
		private string _inf2;//扩展字段2
		private string _inf3;//扩展字段3
		private string _inf4;//扩展字段4
		private string _inf5;//扩展字段5
        private string _lable;//标签
        private string _lablepinyin;//标签拼音简称
        private string _lablepinyinfull;// 标签拼音全称

        #endregion

        #region 属性

        /// <summary>
        /// 数据标识
        /// </summary>
        public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>
		/// 量化分组id
		/// </summary>
		public string Groupid
		{
			get { return _groupid; }
			set { _groupid = value; }
		}

		/// <summary>
		/// 量化编号（code）
		/// </summary>
		public string Quantid
		{
			get { return _quantid; }
			set { _quantid = value; }
		}

		/// <summary>
		/// 策略名
		/// </summary>
		public string Quantname
		{
			get { return _quantname; }
			set { _quantname = value; }
		}

		/// <summary>
		/// stc用户
		/// </summary>
		public string Username
		{
			get { return _username; }
			set { _username = value; }
		}

		/// <summary>
		/// 类型：1 模版
		/// </summary>
		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}

		/// <summary>
		/// 开关标记1开关，0关闭
		/// </summary>
		public string Enable
		{
			get { return _enable; }
			set { _enable = value; }
		}

		/// <summary>
		/// 终端
		/// </summary>
		public string Terminal
		{
			get { return _terminal; }
			set { _terminal = value; }
		}

		/// <summary>
		/// 服务器ip
		/// </summary>
		public string Ip
		{
			get { return _ip; }
			set { _ip = value; }
		}

		/// <summary>
		/// 操作人
		/// </summary>
		public string Closeby
		{
			get { return _closeby; }
			set { _closeby = value; }
		}

		/// <summary>
		/// 说明
		/// </summary>
		public string Remark
		{
			get { return _remark; }
			set { _remark = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Action
		{
			get { return _action; }
			set { _action = value; }
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
		/// 标签
		/// </summary>
		public string Lable
        {
            get { return _lable; }
            set { _lable = value; }
        }
     

        /// <summary>
		/// 标签拼音简称
		/// </summary>
		public string Lablepinyin
        {
            get { return _lablepinyin; }
            set { _lablepinyin = value; }
        }
       
        /// <summary>
        /// 标签拼音全称
        /// </summary>
        public string Lablepinyinfull
        {
            get { return _lablepinyinfull; }
            set { _lablepinyinfull = value; }
        }
       

        #endregion

    }
}