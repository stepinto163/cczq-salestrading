using System;
using System.Collections.Generic;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 用户表
    /// </summary>
    /// <remarks>
    /// 2020.05.09: 创建. Csharp代码生成插件(V1.1.6825.19615(build:20180908)) 模板版本(v1.0.0.0)<br/>
    /// </remarks>
    [Serializable]
    public class StcTbookUsers
    {
        #region 字段

        private string _userid;//
        private string _username = "";//登录名
        private string _venueusersid;//对应渠道账户编号
        private string _name;//用户名
        private string _password;//密码
        private string _enable;//启用
        private string _remark;//备注
        private string _createtime;//创建时间
        private string _updatetime;//更新时间
        private string _biztypestr;//业务类型
        private string _biztypeid;//业务类型编号
        private string _roleid;//角色id
        private string _rolename;//角色名
        private string _mac;
        private string _mac1;
        private string _mac2;
        private string _saleflag;// 是否是销售 0-不是销售 1-是销售
        private List<TbookVenueUsers> _tbookVenueUsers;
        private bool _isHistory = false;
        private string isUsers;//空为用户，有值为组
        public string inf1;
        public string inf2;
        public string inf3;
        public string inf4;
        public string inf5;
        #endregion

        #region 属性

        /// <summary>
        /// 是否是历史数据
        /// </summary>
        public bool IsHistory
        {
            get { return _isHistory; }
            set { _isHistory = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Userid
        {
            get { return _userid; }
            set { _userid = value; }
        }

        /// <summary>
        /// 登录名
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// 对应渠道账户编号
        /// </summary>
        public string Venueusersid
        {
            get { return _venueusersid; }
            set { _venueusersid = value; }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// 启用
        /// </summary>
        public string Enable
        {
            get { return _enable; }
            set { _enable = value; }
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
        /// 业务类型
        /// </summary>
        public string Biztypestr
        {
            get { return _biztypestr; }
            set { _biztypestr = value; }
        }

        /// <summary>
        /// 业务类型编号
        /// </summary>
        public string Biztypeid
        {
            get { return _biztypeid; }
            set { _biztypeid = value; }
        }

        /// <summary>
        /// 角色id
        /// </summary>
        public string Roleid
        {
            get { return _roleid; }
            set { _roleid = value; }
        }

        /// <summary>
        /// 角色名
        /// </summary>
        public string Rolename
        {
            get { return _rolename; }
            set { _rolename = value; }
        }

        public List<TbookVenueUsers> TbookVenueUsers
        {
            get { return _tbookVenueUsers; }
            set { _tbookVenueUsers = value; }
        }
        public string Mac
        {
            get { return _mac; }
            set { _mac = value; }
        }
        public string Mac1
        {
            get { return _mac1; }
            set { _mac1 = value; }
        }
        public string Mac2
        {
            get { return _mac2; }
            set { _mac2 = value; }
        }

        public string Saleflag
        {
            get { return _saleflag; }
            set { _saleflag = value; }
        }

        public string Inf1
        {
            get
            {
                return inf1;
            }
            set
            {
                inf1 = value;
            }
        }

        public string Inf2
        {
            get
            {
                return inf2;
            }
            set
            {
                inf2 = value;
            }
        }

        public string Inf3
        {
            get
            {
                return inf3;
            }
            set
            {
                inf3 = value;
            }
        }

        public string Inf4
        {
            get
            {
                return inf4;
            }
            set
            {
                inf4 = value;
            }
        }

        public string Inf5
        {
            get
            {
                return inf5;
            }
            set
            {
                inf5 = value;
            }
        }
        #endregion


        public string IsUsers
        {
            get
            {
                return isUsers;
            }

            set
            {
                isUsers = value;
            }
        }

        private List<string> actionIds;

        public List<string> ActionIds
        {
            get { return actionIds; }
            set { actionIds = value; }
        }

    }

    public class TbookVenueUsers
    {
        public string id;
        public string partyid21;
        public string partyid6;
        public string partynamecnfullname;
        public string partynamecnshortname;
        public string partynameenfullname;
        public string partynameenshortname;
        public string partysubid;
        public string partysubname;
        public string updatetime;
        public string venues;
        public string venuetypegw;

    }
}
