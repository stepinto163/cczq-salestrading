using System;
namespace STC.Common.Model.ODM
{
	/// <summary>
	/// stc_tbook_md_securityids
	/// </summary>
	/// <remarks>
	/// 2020.04.26: 创建. Csharp代码生成插件(V1.1.6825.19615(build:20180908)) 模板版本(v1.0.0.0)<br/>
	/// </remarks>
	[Serializable]
	public class StcTbookMdSecurityids 
	{
		#region 字段

		private string _securityid = "";//
		private string _venuetypegw = "";//
		private string _settltype;//
		private string _createby;//
		private string _createtime;//
        private string _symbol = "";

		#endregion

		#region 属性

		/// <summary>
		/// 
		/// </summary>
		public string Securityid
		{
			get { return _securityid; }
			set
            {
                _securityid = value;                
            }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Venuetypegw
		{
			get { return _venuetypegw; }
			set { _venuetypegw = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Settltype
		{
			get { return _settltype; }
			set { _settltype = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Createby
		{
			get { return _createby; }
			set { _createby = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string Createtime
		{
			get { return _createtime; }
			set { _createtime = value; }
		}

        public string Symbol
        {
            get
            {
                return _symbol;
            }

            set
            {
                _symbol = value;
            }
        }


        #endregion

    }
}
