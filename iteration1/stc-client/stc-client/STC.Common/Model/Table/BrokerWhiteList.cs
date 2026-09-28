using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{

    public class BrokerWhiteList
    {

        public string id;

        /**
         * STC用户名
         */
        public string username;

        /**
         * STC用户ID
         */
        public string userId;

        /**
         * 经纪商ID（QQ号/微信号...）
         */
        public string brokerId;

        /**
         * 经纪商名称
         */
        public string brokerName;

        /**
         * 白名单类型
         */
        public string whiteListType;

        /**
         * 白名单状态 0-不生效 1-生效
         */
        public int whiteListEnable;

        /**
         * 更新方式 1-手动新增
         */
        public string updateType;

        /**
         * 创建人
         */
        public string updateBy;

        /**
         * 更新时间
         */
        public string updateTime;

        /**
         * 操作类型 0-查询 1-新增 2-修改 3-删除
         */
        public int action;

    }
}
