using com.gateway.log;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.SqlLite
{
    public class TagsSqliteManager
    {
        private object _tagsLock = new object();
        private object _cacheLock = new object();
        private volatile bool _isClosed = false;  // 标记连接是否已关闭

        private const string dbFileName = @"data\Tags_0.0.0.db";
        private string dbFile = "data/Tags_0.0.0.db";

        // 内存缓存
        private List<ChildTag> _issuerCache = null;
        private List<ChildTag> _guarantorCache = null;
        private DateTime _issuerCacheTime = DateTime.MinValue;
        private DateTime _guarantorCacheTime = DateTime.MinValue;
        private const int CacheExpirationMinutes = 120;  // 2 小时过期

        #region Contact 发行人
        private const string CreateIssuers = "create table if not exists Issuer(childTag varchar2(60), childTagName varchar2(60), childTagValue varchar2(64) UNIQUE, firstletter varchar2(32),pinyin varchar2(512), num integer)";
        public const string UpdateIssuerData = @"REPLACE INTO Issuer(childTag, childTagName, childTagValue, firstletter, pinyin, num) VALUES(@childTag,@childTagName,@childTagValue,@firstletter,@pinyin, @num)";
        public const string GetIssuerMaxDateSql = @"SELECT childTagValue FROM Issuer ORDER BY num DESC LIMIT 1";
        public const string GetAllIssuersSql = @"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer";
        public const string DeleteIssuerSql = @"DELETE FROM Issuer";
        public const string DeleteIssuerByIDSql = @"DELETE FROM Issuer where childTagValue = @childTagValue";
		public const string GetIssuersSql = @"SELECT COUNT(*) FROM Issuer";
        #endregion

        #region Inst 担保人
        private const string CreateInsts = "create table if not exists Guarantor(childTag varchar2(60), childTagName varchar2(60), childTagValue varchar2(64) UNIQUE, firstletter varchar2(32),pinyin varchar2(512), num integer)";
        public const string UpdateInstData = @"REPLACE INTO Guarantor(childTag, childTagName, childTagValue, firstletter, pinyin, num) VALUES(@childTag,@childTagName,@childTagValue,@firstletter,@pinyin, @num)";
        public const string GetInstMaxDateSql = @"SELECT childTagValue FROM guarantor ORDER BY num DESC LIMIT 1";
        public const string GetAllGuarantorsSql = @"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Guarantor";
        public const string DeleteCrmGuarantorSql = @"DELETE FROM Guarantor";
        public const string DeleteCrmInstByIDSql = @"DELETE FROM Guarantor where childTagValue = @childTagValue";
        public const string GetGuarantorsSql = @"SELECT COUNT(*) FROM Guarantor";
        #endregion

        private static TagsSqliteManager sqliteHelper = new TagsSqliteManager();
        public static TagsSqliteManager Instance { get { return sqliteHelper; } }

        private SQLiteConnection conn;

        ~TagsSqliteManager()
        {
            this.Close();
        }

        private TagsSqliteManager()
        {
            try
            {
                //LogHandler.Info($"删除Tags");
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
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("DeleteContact: Database connection is closed, operation skipped");
                    return;
                }

                lock (_tagsLock)
                {
                    // 再次检查（双重检查）
                    if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                    {
                        //LogHelper.Warn("DeleteContact: Database connection is closed, operation skipped");
                        return;
                    }

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
                //LogHelper.Warn("DeleteContact exception: " + ex.Message);
            }
        }

        public ChildTag GetIssuerMaxDate()
        {
            ChildTag po = null;
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("GetIssuerMaxDate: Database connection is closed");
                    return po;
                }

                using (SQLiteCommand cmd = new SQLiteCommand(GetIssuerMaxDateSql, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.HasRows && reader.Read())
                        {
                            po = new ChildTag();
                            po.childTagValue = GetString(reader, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Warn("GetIssuerMaxDate exception: " + ex.Message);
            }

            return po;
        }
        public void InsertIssuer(List<ChildTag> list)
        {
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("InsertIssuer: Database connection is closed, operation skipped");
                    return;
                }

                lock (_tagsLock)
                {
                    // 再次检查（双重检查）
                    if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                    {
                        //LogHelper.Warn("InsertIssuer: Database connection is closed, operation skipped");
                        return;
                    }

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
                LogHelper.Error("InsertIssuer exception: " + ex.Message);
            }
        }

        public List<ChildTag> GetAllIssuers(string sql)
        {
            List<ChildTag> list = new List<ChildTag>();
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("GetAllIssuers: Database connection is closed, returning empty list");
                    return list;
                }

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
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
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Warn("GetAllIssuers exception: " + ex.Message);
            }

            return list;
        }

        public int GetIssuersCount()
        {
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("GetIssuersCount: Database connection is closed, returning 0");
                    return 0;
                }

                lock (_tagsLock)
                {
                    using (var command = new SQLiteCommand(GetIssuersSql, conn))
                    {
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            int count = Convert.ToInt32(result);
                            return count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("GetIssuersCount exception: " + ex.Message);
            }

            return 0;
        }

        /// <summary>
        /// 获取所有发行人数据列表（用于定期检查更新，每隔 2 小时）
        /// 采用内存缓存策略，第一次读表，之后直接返回缓存数据，2 小时后过期
        /// </summary>
        /// <returns>发行人数据列表</returns>
        public List<ChildTag> GetAllIssuerList()
        {
            lock (_cacheLock)
            {
                // 检查缓存是否有效（未过期）
                if (_issuerCache != null && DateTime.Now - _issuerCacheTime < TimeSpan.FromMinutes(CacheExpirationMinutes))
                {
                    //LogHelper.Debug("GetAllIssuerList: 返回缓存数据");
                    return _issuerCache;
                }

                // 缓存过期或不存在，重新读表
                //LogHelper.Info("GetAllIssuerList: 重新读取数据库");
                _issuerCache = GetAllIssuers(GetAllIssuersSql);
                _issuerCacheTime = DateTime.Now;
                return _issuerCache;
            }
        }

        /// <summary>
        /// 清空发行人缓存，用于数据更新后立即生效
        /// </summary>
        public void ClearIssuerCache()
        {
            lock (_cacheLock)
            {
                _issuerCache = null;
                _issuerCacheTime = DateTime.MinValue;
                //LogHelper.Debug("ClearIssuerCache: 发行人缓存已清空");
            }
        }
        #endregion

        #region Guarantor 担保人

        public void DeleteGuarantor()
        {
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("DeleteGuarantor: Database connection is closed, operation skipped");
                    return;
                }

                lock (_tagsLock)
                {
                    // 再次检查（双重检查）
                    if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                    {
                        //LogHelper.Warn("DeleteGuarantor: Database connection is closed, operation skipped");
                        return;
                    }

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
                //LogHelper.Warn("DeleteGuarantor exception: " + ex.Message);
            }
        }

        public ChildTag GetGuarantorMaxDate()
        {
            ChildTag po = null;
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("GetGuarantorMaxDate: Database connection is closed");
                    return po;
                }

                using (SQLiteCommand cmd = new SQLiteCommand(GetInstMaxDateSql, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.HasRows && reader.Read())
                        {
                            po = new ChildTag();
                            po.childTagValue = GetString(reader, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Warn("GetGuarantorMaxDate exception: " + ex.Message);
            }

            return po;
        }
        public void InsertGuarantor(List<ChildTag> list)
        {
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("InsertGuarantor: Database connection is closed, operation skipped");
                    return;
                }

                lock (_tagsLock)
                {
                    // 再次检查（双重检查）
                    if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                    {
                        //LogHelper.Warn("InsertGuarantor: Database connection is closed, operation skipped");
                        return;
                    }

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
                LogHelper.Error("InsertGuarantor exception: " + ex.Message);
            }
        }

        public List<ChildTag> GetAllGuarantors(string sql)
        {
            List<ChildTag> list = new List<ChildTag>();
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("GetAllGuarantors: Database connection is closed, returning empty list");
                    return list;
                }

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
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
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Warn("GetAllGuarantors exception: " + ex.Message);
            }

            return list;
        }

        public int GetGuarantorsCount()
        {
            try
            {
                // 检查连接是否已关闭
                if (_isClosed || conn == null || conn.State == ConnectionState.Closed)
                {
                    //LogHelper.Warn("GetGuarantorsCount: Database connection is closed, returning 0");
                    return 0;
                }

                lock (_tagsLock)
                {
                    using (var command = new SQLiteCommand(GetGuarantorsSql, conn))
                    {
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            int count = Convert.ToInt32(result);
                            return count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("GetGuarantorsCount exception: " + ex.Message);
            }

            return 0;
        }

        /// <summary>
        /// 获取所有担保人数据列表（用于定期检查更新，每隔 2 小时）
        /// 采用内存缓存策略，第一次读表，之后直接返回缓存数据，2 小时后过期
        /// </summary>
        /// <returns>担保人数据列表</returns>
        public List<ChildTag> GetAllGuarantorList()
        {
            lock (_cacheLock)
            {
                // 检查缓存是否有效（未过期）
                if (_guarantorCache != null && DateTime.Now - _guarantorCacheTime < TimeSpan.FromMinutes(CacheExpirationMinutes))
                {
                    //LogHelper.Debug("GetAllGuarantorList: 返回缓存数据");
                    return _guarantorCache;
                }

                // 缓存过期或不存在，重新读表
                //LogHelper.Info("GetAllGuarantorList: 重新读取数据库");
                _guarantorCache = GetAllGuarantors(GetAllGuarantorsSql);
                _guarantorCacheTime = DateTime.Now;
                return _guarantorCache;
            }
        }

        /// <summary>
        /// 清空担保人缓存，用于数据更新后立即生效
        /// </summary>
        public void ClearGuarantorCache()
        {
            lock (_cacheLock)
            {
                _guarantorCache = null;
                _guarantorCacheTime = DateTime.MinValue;
                //LogHelper.Debug("ClearGuarantorCache: 担保人缓存已清空");
            }
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
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    return command.ExecuteNonQuery();
                }
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

        /// <summary>
        /// 识别和筛选发行人信息
        /// </summary>
        /// <param name="contentText">输入的文本内容</param>
        /// <param name="isFuzzy">是否使用模糊搜索</param>
        /// <param name="failedItems">输出：未能识别的项目列表</param>
        /// <returns>识别成功的发行人集合</returns>
        public CustomCollection<IssueRecogizeItem> RecognizeIssuerItems(
            string contentText,              // 输入文本
            bool isFuzzy,                     // 模糊搜索模式
            out List<string> failedItems      // 输出：未识别项目
)
        {
            failedItems = new List<string>();
            var result = new CustomCollection<IssueRecogizeItem>();

            try
            {
                if (string.IsNullOrEmpty(contentText))
                    return result;

                // 定义分割字符
                char[] splitCharArr_Fuzzy = new char[] { '\n', '\r', ' ', '　', ',', '，', '、', ' ' };
                char[] splitCharArr = new char[] { '\n', '\r' };
                char[] splitArr = isFuzzy ? splitCharArr_Fuzzy : splitCharArr;

                // 清理文本
                string text = contentText.TrimEnd(splitArr);
                text = text.TrimEnd(splitArr);

                // 移除 "bid" 和 "ofr" 关键词
                text = System.Text.RegularExpressions.Regex.Replace(text, $"\\b{System.Text.RegularExpressions.Regex.Escape("bid")}\\b", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                text = System.Text.RegularExpressions.Regex.Replace(text, $"\\b{System.Text.RegularExpressions.Regex.Escape("ofr")}\\b", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // 移除前缀
                text = text.TrimStart(new char[] { '收' });
                text = text.TrimStart(new char[] { '出' });

                // 分割文本
                string[] lsstring = text.Split(splitArr, StringSplitOptions.RemoveEmptyEntries);

                if (lsstring.Length > 3000)
                {
                    LogHelper.Error("RecognizeIssuerItems: 最大识别个数3000条,请检查");
                    return result;
                }

                // 查询数据库
                List<ChildTag> queryList = new List<ChildTag>();
                List<string> sourcelist = new List<string>();

                if (isFuzzy)
                {
                    // 模糊搜索模式
                    string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where ";
                    StringBuilder sb = new StringBuilder();
                    int i = 0;

                    foreach (string findStr in lsstring)
                    {
                        StringBuilder patternSB = new StringBuilder();
                        string searchStr = findStr.Trim();
                        char[] charArr = searchStr.ToCharArray();

                        foreach (char a in charArr)
                        {
                            if (patternSB.Length > 0)
                                patternSB.Append("%");
                            patternSB.Append(a);
                        }

                        if (sb.Length > 0)
                            sb.Append(" or ");

                        sb.Append($"pinyin like '%{patternSB}%' or firstletter like '%{searchStr}%' or childTagName like '%{patternSB}%'");

                        i++;
                        if (i % 200 == 0)
                        {
                            if (sb.Length > 0)
                            {
                                queryList.AddRange(GetAllIssuers(sql + sb.ToString()));
                                sb.Clear();
                            }
                        }
                    }

                    if (sb.Length > 0)
                    {
                        queryList.AddRange(GetAllIssuers(sql + sb.ToString()));
                        sb.Clear();
                    }
                }
                else
                {
                    // 精确搜索模式
                    string sql = $"SELECT childTag,childTagName,childTagValue,firstletter,pinyin FROM Issuer where ";
                    StringBuilder sb = new StringBuilder();
                    int i = 0;

                    foreach (string findStr in lsstring)
                    {
                        string searchStr = findStr.Trim();

                        if (sb.Length > 0)
                            sb.Append(" or ");

                        sb.Append($"pinyin = '{searchStr}' or firstletter = '{searchStr}' or childTagName = '{searchStr}'");
                        sourcelist.Add(searchStr);

                        i++;
                        if (i % 200 == 0)
                        {
                            if (sb.Length > 0)
                            {
                                List<ChildTag> dbList = GetAllIssuers(sql + sb.ToString());
                                queryList.AddRange(dbList);
                                sb.Clear();

                                // 移除已找到的项目
                                foreach (ChildTag ct in dbList)
                                {
                                    sourcelist.Remove(ct.childTagName);
                                }
                            }
                        }
                    }

                    if (sb.Length > 0)
                    {
                        List<ChildTag> dbList = GetAllIssuers(sql + sb.ToString());
                        queryList.AddRange(dbList);
                        sb.Clear();

                        // 移除已找到的项目
                        foreach (ChildTag ct in dbList)
                        {
                            sourcelist.Remove(ct.childTagName);
                        }
                    }

                    failedItems = sourcelist;
                }

                // 转换为 IssueRecogizeItem 集合
                foreach (ChildTag ct in queryList)
                {
                    try
                    {
                        var node = new Node(ct.childTagName, ct.childTagValue, ct.pinyin, ct.firstletter);
                        var item = new IssueRecogizeItem
                        {
                            NodeInfo = node,
                            IsChecked = !isFuzzy  // 模糊模式下默认不选中，精确模式下默认选中
                        };
                        result.Add(item);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"Failed to create IssueRecogizeItem: {ex.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"RecognizeIssuerItems exception: {ex.Message}");
                return result;
            }
        }

        public void Close()
        {
            try
            {
                //LogHelper.Info("TagsSqliteManager.Close() called");
                
                // 1. 设置关闭标志 - 之后所有数据库操作都会被拒绝
                _isClosed = true;
                
                // 2. 检查连接状态，避免已关闭的连接继续使用
                if (conn != null)
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        try
                        {
                            conn.Close();
                            //LogHelper.Info("Database connection closed successfully");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error("Error closing database connection: " + ex.Message);
                        }
                    }
                    
                    try
                    {
                        conn.Dispose();
                        //LogHelper.Info("Database connection disposed");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("Error disposing database connection: " + ex.Message);
                    }
                    finally
                    {
                        conn = null;
                    }
                }
                
                //LogHelper.Info("TagsSqliteManager.Close() completed");
            }
            catch (Exception ex)
            {
                LogHelper.Error("Fatal error in TagsSqliteManager.Close(): " + ex.Message);
            }
        }
    }
}

