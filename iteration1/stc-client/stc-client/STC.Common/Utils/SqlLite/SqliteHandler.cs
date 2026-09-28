using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Utils.SqlLite
{
    public abstract class SqliteHandler
    {        
        public abstract string ConnStr { get; }
        private SQLiteConnection conn;

        private string GetString(SQLiteDataReader reader,int i)
        {
            if (reader.IsDBNull(i))
            {
                return null;
            }
            else
            {
                return reader.GetString(i);
            }
        }

        protected SQLiteConnection GetConn()
        {
            InitConn();
            return conn;
        }

        protected void InitConn()
        {
            if (conn == null)
                conn = new SQLiteConnection(ConnStr);
        }

        protected void Close()
        {
            if (conn != null)
                conn.Close();
        }

        protected int ExecuteNonQuery(string sql, params object[] paramList)
        {
            SQLiteCommand cmd = null;
            try
            {
                InitConn();
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                cmd = new SQLiteCommand(sql, conn);
                if (paramList != null)
                {
                    AttachParameters(cmd, sql, paramList);
                }
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                if (cmd != null)
                    cmd.Dispose();
                if (conn != null)
                    conn.Close();   
            }
            return -999;
        }

        private DataTable ExecuteDataTable(string sql, object[] paramList)
        {
            DataTable ds = new DataTable();
            SQLiteCommand cmd = null;
            SQLiteDataAdapter da = null;
            try
            {
                InitConn();
                cmd = new SQLiteCommand(sql, conn);
                if (paramList != null)
                {
                    AttachParameters(cmd, sql, paramList);
                }               
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                da = new SQLiteDataAdapter(cmd);
                da.Fill(ds);
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
            finally
            {
                if (da != null)
                    da.Dispose();
                if (cmd != null)
                    cmd.Dispose();
                if (conn != null)
                    conn.Close();
            }
           
            return ds;
        }

        public List<T> ExecuteList<T>(string sql, object[] paramList) where T : new()
        {          
            DataTable ds = ExecuteDataTable(sql, paramList);
            return CreateListFromTable<T>(ds);            
        }

        private List<T> CreateListFromTable<T>(DataTable tbl) where T : new()
        {
            List<T> lt = new List<T>();
            foreach(DataRow r in tbl.Rows)
            {
                lt.Add(CreateItemFromRow<T>(r));
            }
            return lt;
        }

        private T CreateItemFromRow<T>(DataRow row) where T : new()
        {
            T item = new T();
            foreach (DataColumn c in row.Table.Columns)
            {
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
            return item;
        }

        private static SQLiteParameterCollection AttachParameters(SQLiteCommand cmd, string sql, params object[] paramList)
        {
            if (paramList == null || paramList.Length == 0) return null;
            SQLiteParameterCollection coll = cmd.Parameters;
            string parmString = sql.Substring(sql.IndexOf("@"));            
            parmString = parmString.Replace(",", " ,");            
            string pattern = @"(@)\S*(.*?)\b";
            Regex ex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection mc = ex.Matches(parmString);
            string[] paramNames = new string[mc.Count];
            int i = 0;
            foreach (Match m in mc)
            {
                paramNames[i] = m.Value;
                i++;
            }
           
            int j = 0;
            Type t = null;
            foreach (object o in paramList)
            {
                t = o.GetType();
                SQLiteParameter parm = new SQLiteParameter();
                switch (t.ToString())
                {
                    case ("DBNull"):
                    case ("Char"):
                    case ("SByte"):
                    case ("UInt16"):
                    case ("UInt32"):
                    case ("UInt64"):
                        throw new SystemException("Invalid data type");
                    case ("System.String"):
                        parm.DbType = DbType.String;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (string)paramList[j];
                        coll.Add(parm);
                        break;
                    case ("System.Byte[]"):
                        parm.DbType = DbType.Binary;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (byte[])paramList[j];
                        coll.Add(parm);
                        break;
                    case ("System.Int32"):
                        parm.DbType = DbType.Int32;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (int)paramList[j];
                        coll.Add(parm);
                        break;
                    case ("System.Boolean"):
                        parm.DbType = DbType.Boolean;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (bool)paramList[j];
                        coll.Add(parm);
                        break;
                    case ("System.DateTime"):
                        parm.DbType = DbType.DateTime;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDateTime(paramList[j]);
                        coll.Add(parm);
                        break;
                    case ("System.Double"):
                        parm.DbType = DbType.Double;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDouble(paramList[j]);
                        coll.Add(parm);
                        break;
                    case ("System.Decimal"):
                        parm.DbType = DbType.Decimal;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDecimal(paramList[j]);
                        break;
                    case ("System.Guid"):
                        parm.DbType = DbType.Guid;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (System.Guid)(paramList[j]);
                        break;
                    case ("System.Object"):
                        parm.DbType = DbType.Object;
                        parm.ParameterName = paramNames[j];
                        parm.Value = paramList[j];
                        coll.Add(parm);
                        break;
                    default:
                        throw new SystemException("Value is of unknown data type");
                } 
                j++;
            }
            return coll;
        }


    }
}
