using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    /// <summary>
    /// 量化分组
    /// </summary>
    /// <remarks>
    /// 2021.10.14: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
    /// </remarks>
    public class TTbookQuantGroups
    {
        #region 字段

        private string _id = "";//id
        private string _groupname = "";//分组名字
        private int? _ordersn;//顺序
        private string _username;//stc用户
        private string _terminal;//终端
        private string _ip;//服务器ip
        private string _closeby;//操作人
        private string _remark;//备注
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
        /// id
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// 分组名字
        /// </summary>
        public string Groupname
        {
            get { return _groupname; }
            set { _groupname = value; }
        }

        /// <summary>
        /// 顺序
        /// </summary>
        public int? Ordersn
        {
            get { return _ordersn; }
            set { _ordersn = value; }
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
        /// 备注
        /// </summary>
        public string Remark
        {
            get { return _remark; }
            set { _remark = value; }
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