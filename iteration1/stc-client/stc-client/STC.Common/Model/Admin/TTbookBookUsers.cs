using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
	/// <summary>
	/// 组合用户表
	/// </summary>
	/// <remarks>
	/// 2024.04.29: 创建. Csharp代码生成插件(V1.1.7796.27373(build:20210506)) 模板版本(v1.0.0.0)<br/>
	/// </remarks>
	[Serializable]
	public class TTbookBookUsers
    {
		#region 字段

		private string _bookid = "";//组合id
		private string _userid = "";//用户id
		private string _username = "";//用户登录名
		private string _createtime;//创建时间
		private string _updatetime;//更新时间
		private string _inf1;//扩展字段1
		private string _inf2;//扩展字段2
		private string _inf3;//扩展字段3
		private string _inf4;//扩展字段4
		private string _inf5;//扩展字段5

		#endregion

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
		/// 用户id
		/// </summary>
		public string Userid
		{
			get { return _userid; }
			set { _userid = value; }
		}

		/// <summary>
		/// 用户登录名
		/// </summary>
		public string Username
		{
			get { return _username; }
			set { _username = value; }
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


		#endregion

	}
}
