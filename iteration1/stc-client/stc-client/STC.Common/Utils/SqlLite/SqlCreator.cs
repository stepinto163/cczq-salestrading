using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.SqlLite
{
    public class SqlCreator
    {
        private static Dictionary<Type, Dictionary<string, string>> dicCreate = new Dictionary<Type, Dictionary<string, string>>();

        //public static void SetCreateDic(Type type, Dictionary<string, string> dic)
        //{
        //    dicCreate[type] = dic;
        //}

        public static void SpecCreate<T>(string fieldName,string typeName)
        {
            Type type = typeof(T);
            if (!dicCreate.ContainsKey(type))
            {
                dicCreate[type] = new Dictionary<string, string>();
            }
            dicCreate[type][fieldName] = typeName;
        }

        private static string GetSpecCreate<T>(string fieldName)
        {
            Type type = typeof(T);
            if (dicCreate.ContainsKey(type))
            {
                var dic = dicCreate[type];
                if (dic.ContainsKey(fieldName))
                {
                    return dic[fieldName];
                }
            }
            return null;
        }

        public static string TableSql<T>(List<string> primaryKeyList)
        {
            Type type = typeof(T);
            PropertyInfo[] props = type.GetProperties();

            StringBuilder sbPrimaryKey = new StringBuilder();
            foreach (string str in primaryKeyList)
            {
                sbPrimaryKey.Append(str + ",");
            }

            string psStr = sbPrimaryKey.ToString();
            if (psStr.Length > 0)
            {
                psStr = psStr.Substring(0, psStr.Length - 1);

                psStr = ", primary key (" + psStr + ")";
            }
            
            string sql = "create table if not exists {0} ({1} {2})";
            StringBuilder sb = new StringBuilder();
            //foreach(PropertyInfo item in props)
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo item = props[i];
                if (i > 0)
                {
                    sb.Append(",");
                }
                string fieldName = item.Name;
                string typeName = GetSpecCreate<T>(fieldName);
                if (string.IsNullOrEmpty(typeName)) {
                    //Console.WriteLine("TableSql: {0},{1}")
                    if (item.PropertyType == typeof(string))
                    {
                        typeName = "varchar2(100)";
                    }
                    else if (item.PropertyType == typeof(int))
                    {
                        typeName = "int";
                    }
                    else if (item.PropertyType == typeof(long))
                    {
                        typeName = "int64";
                    }
                    else if (item.PropertyType == typeof(decimal))
                    {
                        typeName = "decimal";
                    }else if (item.PropertyType == typeof(DateTime))
                    {
                        typeName = "DATETIME";
                    }
                    else
                    {
                        typeName = "varchar2(100)";
                    }
            }
                sb.Append(string.Format("{0} {1}", fieldName, typeName));
            }
            string args = sb.ToString();
            string tbName = type.Name;
            return string.Format(sql, tbName, args, psStr);
        }

        public static SqlObject InsertSql<T>()
        {
            return InsertReplaceSql<T>("insert");
        }

        public static SqlObject ReplaceSql<T>()
        {
            return InsertReplaceSql<T>("replace");
        }

        public static SqlObject InsertReplaceSql<T>(string keyWord)
        {
            Type type = typeof(T);
            PropertyInfo[] props = type.GetProperties();

            string sql = "{3} into {0}({1}) values({2})";
            StringBuilder sbField = new StringBuilder();
            StringBuilder sbVal = new StringBuilder();
            //foreach(PropertyInfo item in props)
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo item = props[i];
                if (i > 0)
                {
                    sbField.Append(",");
                    sbVal.Append(",");
                }
                string fieldName = item.Name;
                sbField.Append(fieldName);
                sbVal.Append("?");
            }
            string tbName = type.Name;
            SqlObject obj = new SqlObject();
            obj.Sql = string.Format(sql, tbName, sbField.ToString(), sbVal.ToString(), keyWord);
            obj.Properties = props;
            return obj;
        }
    }

    public class SqlObject
    {
        public string Sql { get; set; }
        public PropertyInfo[] Properties { get; set; }
    }
}
