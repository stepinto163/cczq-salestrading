using STC.Common.Model.MM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.RFQ
{
    public class RfqUpdateEnableReq
    {
        /// <summary>
        /// 启动、停止时ID列表
        /// </summary>
        public List<string> ids;
        /// <summary>
        /// 0:查询,1:插入,2:更新,3:删除,4:导入,5:启动,6:停止
        /// </summary>
        public int action;

        public UpdateEnableModel strategy;
    }

    public class UpdateEnableModel
    {
        /// <summary>
        /// 策略分组ID
        /// </summary>
        public string groupid;

        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityid;

        /// <summary>
        /// 债券名称
        /// </summary>
        public string symbol;

        /// <summary>
        /// 清算速度
        /// </summary>
        public string settltype;

        /// <summary>
        /// 用户名称
        /// </summary>
        public string username;

        /// <summary>
        /// 开关标识
        /// </summary>
        public string enable;

        /// <summary>
        /// ip
        /// </summary>
        public string ip;

        /// <summary>
        /// remark
        /// </summary>
        public string remark;

        /// <summary>
        /// closeby
        /// </summary>
        public string closeby;

        /// <summary>
        /// 终端
        /// </summary>
        public string terminal;

        /// <summary>
        /// mac
        /// </summary>
        public string macid;

        /// <summary>
        /// harddiskid
        /// </summary>
        public string harddiskid;
    }
}
