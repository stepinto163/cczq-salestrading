using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 部门表
    /// </summary>
    /// <remarks>
    /// 2025.02.05: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
    /// </remarks>
    [Serializable]
    public class TbookUserGroups
    {
        #region 字段

        private string _groupid = "";//部门id
        private string _groupname = "";//部门名
        private string _userids;//用户id列表
        private string _createtime;//创建时间
        private string _updatetime;//更新时间
        private string _inf1;//扩展字段1
        private string _inf2;//扩展字段2
        private string _inf3;//扩展字段3
        private string _inf4;//扩展字段4
        private string _inf5;//扩展字段5
        private string _action;
        private string _status;
        private string _mdgroupids;//可查看的行情部门字段
        private string _tdgroupids;//可撮合的交易部门字段

        #endregion

        #region 属性

        /// <summary>
        /// 部门id
        /// </summary>
        public string Groupid
        {
            get { return _groupid; }
            set { _groupid = value; }
        }

        /// <summary>
        /// 部门名
        /// </summary>
        public string Groupname
        {
            get { return _groupname; }
            set { _groupname = value; }
        }

        /// <summary>
        /// 用户id列表
        /// </summary>
        public string Userids
        {
            get { return _userids; }
            set { _userids = value; }
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

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public string Action
        {
            get { return _action; }
            set { _action = value; }
        }
        #endregion

        public string mdgroupids
        {
            get { return _mdgroupids; }
            set { _mdgroupids = value; }
        }

        public string tdgroupids
        {
            get { return _tdgroupids; }
            set { _tdgroupids = value; }
        }

    }
}