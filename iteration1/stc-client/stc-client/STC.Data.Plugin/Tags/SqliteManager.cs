using com.gateway.log;
using STC.Common.Model.Table;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Data.Plugin.Tags
{
    class SqliteManager
    {
        private object _tagsLock = new object();

        private const string dbFileName = @"data\Tags_0.0.0.db";
        private string dbFile = "data/Tags_0.0.0.db";

        #region Contact 发行人
        private const string CreateIssuers = "create table if not exists Issuer(childTag varchar2(60), childTagName varchar2(60), childTagValue varchar2(64) UNIQUE, firstletter varchar2(32),pinyin varchar2(512), num integer)";
        public const string UpdateIssuerData = @"REPLACE INTO Issuer(childTag, childTagName, childTagValue, firstletter, pinyin, num) VALUES(@childTag,@childTagName,@childTagValue,@firstletter,@pinyin, @num)";
        public const string GetIssuerMaxDateSql = @"SELECT childTagValue FROM Issuer ORDER BY num DESC LIMIT 1";
        public const string GetAllIssuersSql = @"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer";
        public const string DeleteIssuerSql = @"DELETE FROM Issuer";
        public const string DeleteIssuerByIDSql = @"DELETE FROM Issuer where childTagValue = @childTagValue";
        #endregion

        #region Inst 担保人
        private const string CreateInsts = "create table if not exists Guarantor(childTag varchar2(60), childTagName varchar2(60), childTagValue varchar2(64) UNIQUE, firstletter varchar2(32),pinyin varchar2(512), num integer)";
        public const string UpdateInstData = @"REPLACE INTO Guarantor(childTag, childTagName, childTagValue, firstletter, pinyin, num) VALUES(@childTag,@childTagName,@childTagValue,@firstletter,@pinyin, @num)";
        public const string GetInstMaxDateSql = @"SELECT childTagValue FROM guarantor ORDER BY num DESC LIMIT 1";
        public const string GetAllGuarantorsSql = @"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor";
        public const string DeleteCrmGuarantorSql = @"DELETE FROM Guarantor";
        public const string DeleteCrmInstByIDSql = @"DELETE FROM Guarantor where childTagValue = @childTagValue";
        #endregion

        private static SqliteManager sqliteHelper = new SqliteManager();
        public static SqliteManager Instance { get { return sqliteHelper; } }

        private SQLiteConnection conn;

        ~SqliteManager()
        {
            this.Close();
        }

        private SqliteManager()
        {
            try
            {
                LogHandler.Info($"删除Tags");
                if (FileHelper.FileIsExists(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\TagsTemp.db")))
                {
                    FileInfo[] files = new DirectoryInfo(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data")).GetFiles("*.db");
                    if (files != null)
                    {
                        foreach (FileInfo fi in files)
                        {
                            try
                            {
                                if (fi.Name == "TagsTemp.db" || fi.Name == Path.GetFileName(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, dbFileName)))
                                {
                                    File.Delete(fi.FullName);
                                    LogHandler.Info($"删除 {fi.FullName}");
                                }
                            }
                            catch (Exception ex1)
                            {
                                LogHandler.Error(ex1);
                            }
                        }
                    }
                }

                if (conn == null || conn.State == ConnectionState.Closed)
                {
                    if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.DataPath))
                        dbFile = Path.Combine(LocalEnv.ConfInfo.DataPath, dbFileName);

                    string connectionString = string.Format("Data Source={0}", dbFile);
                    conn = new SQLiteConnection(connectionString);
                    //conn.SetPassword("aP@ssw0rd");
                    conn.Open();
                    ExecuteNonQuery(CreateInsts);
                    ExecuteNonQuery(CreateIssuers);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        #region Contact

        public void DeleteContact()
        {
            try
            {
                lock (_tagsLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        SQLiteCommand cmd = new SQLiteCommand(DeleteIssuerSql, conn);
                        int result = cmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("DeleteContact exception," + ex.Message);
            }
        }

        public ChildTag GetIssuerMaxDate()
        {
            ChildTag po = null;
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetIssuerMaxDateSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    po = new ChildTag();
                    po.childTagValue = GetString(reader, 0);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("can't get GetIssuerMaxDate," + ex.Message);
            }

            return po;
        }
        public void InsertIssuer(List<ChildTag> list)
        {
            try
            {
                lock (_tagsLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        foreach (var po in list)
                        {

                            SQLiteCommand cmd = new SQLiteCommand(UpdateIssuerData, conn, tran);
                            cmd.Parameters.AddRange(new[]
                            {
                                new SQLiteParameter("@childTag",po.childTag),
                                new SQLiteParameter("@childTagName",po.childTagName),
                                new SQLiteParameter("@childTagValue",po.childTagValue),
                                new SQLiteParameter("@firstletter",po.firstletter),
                                new SQLiteParameter("@pinyin",po.pinyin),
                                new SQLiteParameter("@num",ToDecimalism(po.childTagValue)),
                            });
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

        public List<ChildTag> GetAllIssuers()
        {
            List<ChildTag> list = new List<ChildTag>();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetAllIssuersSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    ChildTag po = new ChildTag();
                    po.childTag = GetString(reader, 0);
                    po.childTagName = GetString(reader, 1);
                    po.childTagValue = GetString(reader, 2);
                    po.firstletter = GetString(reader, 3);
                    po.pinyin = GetString(reader, 4);
                    list.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return list;
        }
        #endregion

        #region Guarantor 担保人

        public void DeleteGuarantor()
        {
            try
            {
                lock(_tagsLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        SQLiteCommand cmd = new SQLiteCommand(DeleteCrmGuarantorSql, conn);
                        int result = cmd.ExecuteNonQuery();
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("DeleteGuarantor exception," + ex.Message);
            }
        }

        public ChildTag GetGuarantorMaxDate()
        {
            ChildTag po = null;
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetInstMaxDateSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    po = new ChildTag();
                    po.childTagValue = GetString(reader, 0);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("can't get GetGuarantorMaxDate," + ex.Message);
            }

            return po;
        }
        public void InsertGuarantor(List<ChildTag> list)
        {
            try
            {
                lock (_tagsLock)
                {
                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        foreach (var po in list)
                        {

                            SQLiteCommand cmd = new SQLiteCommand(UpdateInstData, conn, tran);
                            cmd.Parameters.AddRange(new[]
                            {
                            new SQLiteParameter("@childTag",po.childTag),
                            new SQLiteParameter("@childTagName",po.childTagName),
                            new SQLiteParameter("@childTagValue",po.childTagValue),
                            new SQLiteParameter("@firstletter",po.firstletter),
                            new SQLiteParameter("@pinyin",po.pinyin),
                            new SQLiteParameter("@num",ToDecimalism(po.childTagValue)),
                        });
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public List<ChildTag> GetAllGuarantors()
        {
            List<ChildTag> list = new List<ChildTag>();
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(GetAllGuarantorsSql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows && reader.Read())
                {
                    ChildTag po = new ChildTag();
                    po.childTag = GetString(reader, 0);
                    po.childTagName = GetString(reader, 1);
                    po.childTagValue = GetString(reader, 2);
                    po.firstletter = GetString(reader, 3);
                    po.pinyin = GetString(reader, 4);
                    list.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return list;
        }
        #endregion

        private string GetString(SQLiteDataReader reader, int i)
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

        private int ExecuteNonQuery(string sql)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return -999;
        }

        public long ToDecimalism(string x)
        {
            if (string.IsNullOrEmpty(x))
                return 0;

            long y = 0;
            char[] arr = x.Reverse().ToArray();
            for (int i = 0; i < x.Length; i++)
            {
                y += arr[i].ConvertToLong() * (long)Math.Pow(2, i);
            }
            return y;
        }

        public void Close()
        {
            if (conn != null && conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn?.Dispose();
            conn = null;
        }
    }
}
