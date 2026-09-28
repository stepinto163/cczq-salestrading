using STC.Common.Model.Admin;
using STC.Common.Model.Quant;
using STC.Common.Utils;
using STC.quant.plugin.Command;
using STC.quant.plugin.Utils.Data;
using STC.quant.plugin.View.Component;
using System.Collections.Generic;
using System.Linq;
namespace STC.quant.plugin.Utils
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class QuantCommonUtils
    {
        //public static IForm Application;

        /// <summary>
        /// 弹窗命令处理类
        /// </summary>

        public static PopupWindowCommand PopupWindowCommand = new PopupWindowCommand();

        public static string groupID = "1";

        /// <summary>
        /// 获取全局策略信息
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="resultNum"></param>
        /// <returns></returns>
        public static List<QuantLable> GetTTbookQuantStrategys(string condition, int resultNum, string quantid = null)
        {
            List<QuantLable> result = new List<QuantLable>();
            List<TTbookQuantStrategys> list = QuantGroupHandler.Instance.GetAllLableByQuantID();
            if (list == null || list.Count == 0)
            {
                return result;
            }
            List<QuantLable> LableList = new List<QuantLable>();
            foreach (TTbookQuantStrategys g in list)
            {
                if (g.Lable == null || g.Lablepinyin == null)
                    continue;
                string[] keys = g.Lablepinyin.Split(new char[1] { ' ' });
                string[] values = g.Lable.Split(new char[1] { ' ' });
                string[] fulls = g.Lablepinyinfull.Split(new char[1] { ' ' });
                int i = 0;
                foreach (string s in values)
                {
                    QuantLable po = new QuantLable();
                    po.Lable = s;
                    po.Lablepinyin = keys[i];
                    po.Lablepinyinfull = fulls[i];
                    if (!LableList.Contains(po))
                        LableList.Add(po);
                    i++;
                }
            }
            lock (LableList)
            {
                List<QuantLable> v = new List<QuantLable>();
                if (quantid != null)
                {
                    v = (from d in LableList
                         where d.Lable.Contains(condition.ToLower())
                         || (d.Lablepinyin != null && d.Lablepinyin.Contains(condition.ToLower()))
                         || (d.Lablepinyinfull != null && d.Lablepinyinfull.Contains(condition.ToLower()))
                         select d).ToList();
                }
                else
                    v.AddRange(LableList);
                int i = 0;
                foreach (QuantLable kv in v)
                {
                    if (i < resultNum)
                        result.Add(kv);
                    else
                        break;
                    i++;
                }
            }
            return result;
        }

        public static string setGroupName(string Param_Value, Dictionary<string, HomeGroupInfo> dicGroup)
        {
            string value = "";
            if (Param_Value.Contains(","))
            {
                string[] str = Param_Value.Split(',');
                foreach (string s in str)
                {
                    if (dicGroup.ContainsKey(s))
                    {
                        if (!string.IsNullOrEmpty(value))
                            value += ",";
                        value += dicGroup[s].groupname;
                    }
                    else
                        LogHandler.Warn("setGroupName dicGroup not contains:" + s);
                }
            }
            else if (dicGroup.ContainsKey(Param_Value))
            {
                if (!string.IsNullOrEmpty(value))
                    value += ",";
                value += dicGroup[Param_Value].groupname;
            }
            else
                LogHandler.Warn("setGroupName dicGroup not contains:" + Param_Value);
            return value;
        }

        //public static string setGroupID(string Param_Value, Dictionary<string, HomeGroupInfo> dicGroup)
        //{
        //    string GroupID = Param_Value;
        //    bool isContain = false;
        //    string temp = Param_Value;

        //    string[] str = Param_Value.Split(',');
        //    foreach (KeyValuePair<string, HomeGroupInfo> pair in dicGroup)
        //    {
        //        if (Param_Value.Equals(pair.Value.groupname))
        //        {
        //            isContain = true;
        //            temp = temp.Replace( pair.Value.groupname, pair.Key);
        //        }
        //    }
        //    if (isContain)
        //        GroupID = temp;
        //    return GroupID;
        //}

    }
}
