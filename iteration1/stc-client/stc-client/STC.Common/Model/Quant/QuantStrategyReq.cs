using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    //增删策略
    public class QuantStrategyReq
    {
        //组编号
        public string groupid;
        //组名
        public string groupname;
        //1:新增,2:更新,3:删除
        public string action;
        //量化编号
        public string id;
        //量化CODE
        public string quantid;
        //策略名
        public string quantname;
        //策略说明
        public string remark;
        //开关
        public string enable;
        //策略模板类型 4=C++策略,1=Java,2=Python,3=jar
        public string type;
        //策略模板ID
        public string templetid;

        //标签
        public string lable;
        //标签拼音简称
        public string lablepinyin;
        //标签拼音全称
        public string lablepinyinfull;
        //stc用户
        public string username;
        //操作人
        public string closeby;

        //模板参数是否变更
        public string inf3;
    }
}
