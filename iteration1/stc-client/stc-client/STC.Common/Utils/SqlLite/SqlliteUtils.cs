using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.SqlLite
{
    public class SqlliteUtils
    {
        public static SQLiteConnection Connection { get; set; }

        public static void CreateTable<T>(List<string> pkCol)
        {
            string sql = SqlCreator.TableSql<T>(pkCol);
            Console.WriteLine("CreateTable: {0},sql={1}", typeof(T).Name, sql);
            using (SQLiteCommand cmd = new SQLiteCommand(sql, Connection))
            {
                //cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(string sql, object[] args)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sql, Connection))
            {
                if (args != null && args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        SQLiteParameter sqlliteParam = new SQLiteParameter();
                        cmd.Parameters.Add(sqlliteParam);
                        sqlliteParam.Value = args[i];
                    }
                }
                
                cmd.ExecuteNonQuery();
                
            }
        }


        public static List<T> Query<T>(string sql,object[] args)
        {
            List<T> list = new List<T>();
            using (SQLiteCommand cmd = new SQLiteCommand(sql, Connection))
            {
                if (args != null && args.Length > 0)
                {
                    for(int i = 0; i < args.Length; i++)
                    {
                        SQLiteParameter sqlliteParam = new SQLiteParameter();
                        cmd.Parameters.Add(sqlliteParam);
                        sqlliteParam.Value = args[i];
                    }
                }
                SQLiteDataReader reader = cmd.ExecuteReader();
                PropertyInfo[] props = typeof(T).GetProperties();
                Dictionary<string, PropertyInfo> dic = new Dictionary<string, PropertyInfo>();
                foreach(PropertyInfo item in props)
                {
                    dic[item.Name] = item;
                }
                while (reader.Read())
                {
                    T t = GetObject<T>(reader, dic);
                    list.Add(t);
                }
            }
            return list;
        }

        private static T GetObject<T>(SQLiteDataReader reader, Dictionary<string, PropertyInfo> dic)
        {
            T t = Activator.CreateInstance<T>();

            for(int i = 0; i < reader.FieldCount; i++)
            {
                if (!reader.IsDBNull(i))
                {
                    string fieldName = reader.GetName(i);
                    if (dic.ContainsKey(fieldName))
                    {
                        PropertyInfo property = dic[fieldName];
                        object val = reader.GetValue(i);
                        if (val != null)
                        {
                            property.SetValue(t, val);
                        }
                    }
                }
            }
            return t;
        }

        public static void Insert<T>(T t)
        {
            InsertReplace<T>(t, "insert");
        }

        public static void InsertList<T>(List<T> list)
        {
            InsertReplaceList<T>(list, "insert");
        }

        public static void Replace<T>(T t)
        {
            InsertReplace<T>(t, "replace");
        }

        public static void ReplaceList<T>(List<T> list)
        {
            InsertReplaceList<T>(list, "replace");
        }

        public static void InsertReplace<T>(T t, string keyWord)
        {
            SqlObject sqlObj = SqlCreator.InsertReplaceSql<T>(keyWord);
            //Console.WriteLine("{2}: {0},sql={1}", typeof(T).Name, sqlObj.Sql, keyWord);
            using (SQLiteCommand cmd = new SQLiteCommand(sqlObj.Sql, Connection))
            {
                //cmd.CommandText = sql;
                SetArgs(cmd, sqlObj, t);
                cmd.ExecuteNonQuery();
            }
        }

        public static void InsertReplaceList<T>(List<T> list, string keyWord)
        {
            SqlObject sqlObj = SqlCreator.InsertReplaceSql<T>(keyWord);
            //Console.WriteLine("{3}List: {0},count={2},sql={1}", typeof(T).Name, sqlObj.Sql, list.Count, keyWord);
            using (SQLiteTransaction transaction = Connection.BeginTransaction())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlObj.Sql, Connection))
                {
                    //cmd.CommandText = sql;
                    //foreach (T t in list)
                    //{
                    ExecuteList(cmd, sqlObj, list);
                    //    cmd.ExecuteNonQuery();
                    //}
                    transaction.Commit();
                }
            }
        }

        private static void ExecuteList<T>(SQLiteCommand cmd, SqlObject sqlObj, List<T> list)
        {
            for (int i = 0; i < sqlObj.Properties.Length; i++)
            {
                PropertyInfo property = sqlObj.Properties[i];
                SQLiteParameter sqlliteParam = new SQLiteParameter();
                cmd.Parameters.Add(sqlliteParam);
                //object val = property.GetValue(obj);
                //sqlliteParam.Value = val;
            }
            foreach (T obj in list)
            {
                for (int i = 0; i < sqlObj.Properties.Length; i++)
                {
                    PropertyInfo property = sqlObj.Properties[i];
                    SQLiteParameter sqlliteParam = cmd.Parameters[i];
                    object val = property.GetValue(obj);
                    sqlliteParam.Value = val;
                }
                cmd.ExecuteNonQuery();
            }
        }

        private static void SetArgs(SQLiteCommand cmd, SqlObject sqlObj, object obj)
        {
            for (int i = 0; i < sqlObj.Properties.Length; i++)
            {
                PropertyInfo property = sqlObj.Properties[i];
                SQLiteParameter sqlliteParam = new SQLiteParameter();
                cmd.Parameters.Add(sqlliteParam);
                object val = property.GetValue(obj);
                sqlliteParam.Value = val;
                //Console.WriteLine("SetArgs: {0}={1}", property.Name, val);
            }
        }
    }
}
