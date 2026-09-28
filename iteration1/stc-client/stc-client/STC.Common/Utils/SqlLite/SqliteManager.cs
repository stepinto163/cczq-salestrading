using com.gateway.log;
using common.netapi.Utils;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Common.Utils.SqlLite
{
    public class SqliteManager
    {
        public static readonly string Version = "v1";
        public static readonly string LastVersion = "";
        //public static readonly string DBFile = "./data/StcLite_{0}_{1}.db";
        public static readonly string DBFile = "./data/StcLite_user_config.db";

        public static void Initialize()
        {
            try
            {
                string folder = FileUtil.BaseDir + "./data";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                //string id = LocalEnv.ServerInfo.Host + LocalEnv.ServerInfo.Port;
                //id = Regex.Replace(id, "\\W", "");

                //string[] lasts = LastVersion.Split(new char[] { ',' });
                //foreach (string item in lasts)
                //{
                //    string lastDbFile = FileUtil.BaseDir + string.Format(DBFile, id, item);
                //    if (File.Exists(lastDbFile))
                //    {
                //        File.Delete(lastDbFile);
                //    }
                //}
                string dbFile = FileUtil.BaseDir + DBFile;//string.Format(DBFile, id, Version);
                //if (File.Exists(dbFile))
                //{
                //    File.Delete(dbFile);
                //}
                string connectionString = string.Format("Data Source={0}", dbFile);
                SQLiteConnection conn = new SQLiteConnection(connectionString);
                conn.Open();
                //ExecuteNonQuery("create table if not exists DealtPo (bondCode varchar2(100),dealtype varchar2(100),shortName varchar2(100),dealprice varchar2(100),UpdateDateTime varchar2(100),InnerUpdateTime varchar2(100),InnerLeftTenor varchar2(100),InnerIssuerRatingCurrent varchar2(100))");
                //ExecuteNonQuery(CreateBondsInfo);
                SqlliteUtils.Connection = conn;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        } 
    }
}
