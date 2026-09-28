using com.gateway.log;
using STC.Common.Utils;
using STC.DownloadCenter.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.DownloadCenter.Plugin.Utils
{
    public class DownloadSqliteManager
    {
        private const string dbFileName = "STC_Download_1.0.3.db";
        private const string dbFile = "./data/STC_Download_1.0.3.db";
        private string connectionString = string.Format("Data Source={0}", dbFile);

        private const string CreateTableSql = "create table if not exists DownloadInfoTable(ID varchar2(60) UNIQUE, BondID varchar2(32), BondName varchar2(32), " +
            "DataType varchar2(32), SettlType varchar2(45),DataSize varchar2(32),LastStatus varchar2(1000), FileName varchar2(512),StartDate varchar2(32), EndDate varchar2(32)," +
            "Frequency varchar2(32), Type varchar2(32)," +
            "CreateTime varchar2(32),UpdateTime varchar2(32), UserName varchar2(32))";

        public const string InsertDataSql = @"REPLACE INTO DownloadInfoTable (ID, BondID, BondName, DataType, SettlType, DataSize, LastStatus, FileName,StartDate, EndDate, CreateTime, UpdateTime, UserName, Frequency, Type) " +
            "VALUES(@ID,@BondID,@BondName,@DataType,@SettlType,@DataSize,@LastStatus,@FileName,@StartDate, @EndDate,@CreateTime,@UpdateTime, @UserName, @Frequency, @Type)";

        public const string SelectDataSql = @"Select ID, BondID, BondName, DataType, SettlType, DataSize, LastStatus, FileName,StartDate, EndDate, CreateTime, UpdateTime, Frequency, Type from DownloadInfoTable";

        public const string DeleteDataSql = @"DELETE FROM DownloadInfoTable where ID in ({0})";

        public const string UpdateDataSql = @"Update DownloadInfoTable SET DataSize = @DataSize, LastStatus = @LastStatus, FileName = @FileName,UpdateTime = @UpdateTime Where ID = @ID";


        private static DownloadSqliteManager sqliteHelper = new DownloadSqliteManager();
        public static DownloadSqliteManager Instance { get { return sqliteHelper; } }

        //private SQLiteConnection conn;

        /// <summary>
        /// 1:添加 2：删除
        /// </summary>
        private Action<string, List<DownloadItem>> _operationAct;
        public event Action<string, List<DownloadItem>> OperationAct
        {
            add
            {
                _operationAct += value;
            }
            remove
            {
                _operationAct -= value;
            }
        }

        public DownloadSqliteManager()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    ExecuteNonQuery(CreateTableSql);
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private int ExecuteNonQuery(string sql)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand(sql, conn);
                    int result = command.ExecuteNonQuery();
                    conn.Close();
                    conn.Dispose();
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return -999;
        }
        public bool DeleteBond(string ids)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    string[] idArray = ids.Split(CommonConstants.COMMA_CHAR);
                    SQLiteCommand cmd = new SQLiteCommand(string.Format(DeleteDataSql, ids), conn);
                    int result = cmd.ExecuteNonQuery();

                    conn.Close();
                    conn.Dispose();

                    return result == idArray.Length;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn("DeleteBond exception," + ex.Message);
            }
            return false;
        }

        public bool Insert(List<DownloadItem> list)
        {
            bool result = false;

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    using (SQLiteTransaction tran = conn.BeginTransaction())
                    {
                        try
                        {
                            int count = 0;
                            foreach (var po in list)
                            {
                                SQLiteCommand cmd = new SQLiteCommand(InsertDataSql, conn, tran);

                                cmd.Parameters.AddRange(new[]
                                {
                                new SQLiteParameter("@ID", po.ID),
                                new SQLiteParameter("@BondID",po.SecurityID),
                                new SQLiteParameter("@BondName",po.Symbol),
                                new SQLiteParameter("@DataType",po.DataType),
                                new SQLiteParameter("@SettlType",po.SettleType),
                                new SQLiteParameter("@DataSize",po.DataSize),
                                new SQLiteParameter("@LastStatus",po.LastStatus),
                                new SQLiteParameter("@FileName",po.FileName),
                                new SQLiteParameter("@StartDate",po.StartDate),
                                new SQLiteParameter("@EndDate",po.EndDate),
                                new SQLiteParameter("@CreateTime", po.CreateTime),
                                new SQLiteParameter("@UpdateTime",po.UpdateTime),
                                new SQLiteParameter("@UserName", po.UserName),
                                new SQLiteParameter("@Frequency", po.Frequency),
                                new SQLiteParameter("@Type", po.Type),
                            });
                                count += cmd.ExecuteNonQuery();
                            }

                            if (count == list.Count)
                            {
                                tran.Commit();

                                if (_operationAct != null)
                                {
                                    _operationAct.Invoke("1", list);
                                }

                                result = true;
                            }
                        }
                        catch(Exception ex)
                        {
                            LogHelper.Error(ex);

                            tran.Rollback();
                        }
                    }

                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (SQLiteException ex)
            {
                LogHelper.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return result;
        }

        public bool Update(DownloadItem po)
        {
            try
            {
                int result = 0;
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    SQLiteCommand cmd = new SQLiteCommand(UpdateDataSql, conn);

                    cmd.Parameters.AddRange(new[]
                    {
                        new SQLiteParameter("@ID", po.ID),
                        new SQLiteParameter("@DataSize",po.DataSize),
                        new SQLiteParameter("@LastStatus",po.LastStatus),
                        new SQLiteParameter("@FileName",po.FileName),
                        new SQLiteParameter("@UpdateTime",po.UpdateTime)
                    });

                    result = cmd.ExecuteNonQuery();

                    conn.Close();
                    conn.Dispose();
                    
                    return result == 1;
                }
            }
            catch (SQLiteException ex)
            {
                LogHelper.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return false;
        }

        public DataTable GetData()
        {
            DataSet ds = new DataSet();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    SQLiteDataAdapter adapter = new SQLiteDataAdapter($"{SelectDataSql} where UserName = '{LocalEnv.UserInfo.UserName}'", conn);

                    adapter.Fill(ds, "DownloadInfoTable");

                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn(ex);
            }

            return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
        }
    }
}
