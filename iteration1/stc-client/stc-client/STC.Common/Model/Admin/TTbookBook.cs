using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
	/// <summary>
	/// 组合表
	/// </summary>
	/// <remarks>
	/// 2024.04.22: 创建. Csharp代码生成插件(V1.1.7796.27373(build:20210506)) 模板版本(v1.0.0.0)<br/>
	/// </remarks>
	[Serializable]
	public class TTbookBook
	{
		#region 字段

		private string _id = "";//组合id
		private string _bookname = "";//组合名称
		private string _remark;//描述
		private string _closeby;//操作人
		private string _createtime;//创建时间
		private string _updatetime;//更新时间
		private string _booktype;//A = 组合
		private string _parentnode;//父级节点编号
		private string _status;//状态：N：正常;D：删除
		private string _external;//
		private string _inf1;//扩展字段1
		private string _inf2;//扩展字段2
		private string _inf3;//扩展字段3
		private string _inf4;//扩展字段4
		private string _inf5;//扩展字段5

		private string _userid;//用户
		private string _userName;
		private string _action;
		#endregion

		#region 属性

		/// <summary>
		/// 组合id
		/// </summary>
		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>
		/// 组合名称
		/// </summary>
		public string Bookname
		{
			get { return _bookname; }
			set { _bookname = value; }
		}

		/// <summary>
		/// 描述
		/// </summary>
		public string Remark
		{
			get { return _remark; }
			set { _remark = value; }
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
		/// A = 组合
		/// </summary>
		public string Booktype
		{
			get { return _booktype; }
			set { _booktype = value; }
		}

		/// <summary>
		/// 父级节点编号
		/// </summary>
		public string Parentnode
		{
			get { return _parentnode; }
			set { _parentnode = value; }
		}

		/// <summary>
		/// 状态：N：正常;D：删除
		/// </summary>
		public string Status
		{
			get { return _status; }
			set { _status = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string External
		{
			get { return _external; }
			set { _external = value; }
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

		public string Userid
		{
			get { return _userid; }
			set { _userid = value; }
		}

		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		public string Action
		{
			get { return _action; }
			set { _action = value; }
		}

		#endregion

	}
}