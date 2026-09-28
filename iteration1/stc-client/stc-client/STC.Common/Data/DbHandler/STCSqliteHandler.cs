using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using com.gateway.log;
using System.IO;

namespace STC.Common.Data
{
    public class STCSqliteHandler : ISTCDbHandler
    {
        #region

        private const string CreateExecutionReportSql = @"CREATE TABLE IF NOT EXISTS ExecutionReport(
                Key VARCHAR(100) UNIQUE,
                ExecId VARCHAR(45),
                StrategyOrderId VARCHAR(45),
                OrderID VARCHAR(45),
                QuoteID VARCHAR(45),
                VenueTypeGW VARCHAR(45),
                Venues VARCHAR(45),
                SecurityID VARCHAR(45),
                Symbol VARCHAR(100),
                ValidUntilTime VARCHAR(45),
                MarketIndicator VARCHAR(45),
                PartySubId VARCHAR(45),
                UserName VARCHAR(45),
                Price DECIMAL(35,8),
                Qty DECIMAL(50,8),
                LastQty DECIMAL(50,8),
                ExecType VARCHAR(45),
                OrderStatus VARCHAR(45),
                OrderSn VARCHAR(45),
                Yield VARCHAR(45),
                NetPrice DECIMAL(50,8),
                Remark VARCHAR(1000),
                Rejecter VARCHAR(45),
                RejecterCode VARCHAR(45),
                UpdateTime VARCHAR(45),
                Info1 TEXT,
                Info2 TEXT,
                Info3 TEXT,
                Info4 TEXT,
                Info5 TEXT,
                Info10 TEXT,
                CounterpartyId VARCHAR(45),
                CounterpartyName VARCHAR(500),
                CounterpartySubId VARCHAR(45),
                CounterpartySubName VARCHAR(500),
                CounterpartyCode VARCHAR(45),
                PartyId VARCHAR(45),
                UserId VARCHAR(45),
                FirmFlag VARCHAR(45),
                WholeFlag VARCHAR(45),
                Name VARCHAR(45),
                QuantID VARCHAR(45),
                PartyName VARCHAR(45),
                PartySubName VARCHAR(45),
                PartyCode VARCHAR(45),
                SettlType VARCHAR(45),
                Side VARCHAR(45),
                TimeInForce VARCHAR(45),
                OrdType VARCHAR(45),
                ClOrdID VARCHAR(45),
                DirtyPrice DECIMAL(50,8),
                TransactTime VARCHAR(45),
                BlockNum VARCHAR(45),
                TradeDate VARCHAR(45),
                SettlDate VARCHAR(45),
                CounterpartyOrdId VARCHAR(45),
                InstitutionId VARCHAR(45),
                InstitutionName VARCHAR(3000),
                TraderId VARCHAR(45),
                TraderName VARCHAR(3000),
                CounterpartyFullName VARCHAR(45),
                StrategyOrderVenues VARCHAR(45),
                CounterpartyVenues VARCHAR(45),
                DelFlag char (1),
                SendingTime VARCHAR(45),
                Vs VARCHAR(45),
                BookID VARCHAR(45),
                IntentionId VARCHAR(45),
                OffsetFlag VARCHAR(45),
                HedgeFlag VARCHAR(45),
                PriceType VARCHAR(45),
                AccountId VARCHAR(45),
                AccountName VARCHAR(45),
                CounterAccountId VARCHAR(45),
                CounterAccountName VARCHAR(45),
                BargainFlag VARCHAR(45),
                ExchangeSignFlag VARCHAR(45),
                VipCode VARCHAR(45),
                CompanyId VARCHAR(45),
                YieldToExecution VARCHAR(45),
                LastPx VARCHAR(45)
            );";

        private const string CreateMarketExecutionsItemSql = @"CREATE TABLE IF NOT EXISTS MarketExecutionsItem(
                Key VARCHAR(100) UNIQUE,
                ExecId VARCHAR(45),
                StrategyOrderId VARCHAR(45),
                OrderID VARCHAR(45),
                QuoteID VARCHAR(45),
                VenueExecId VARCHAR(45),
                VenueId VARCHAR(45),
                VenueTypeGW VARCHAR(45),
                Venues VARCHAR(45),
                SecurityID VARCHAR(45),
                Symbol VARCHAR(100),
                SymbolType VARCHAR(45),
                ValidUntilTime VARCHAR(45),
                MarketIndicator VARCHAR(45),
                PartySubId VARCHAR(45),
                UserName VARCHAR(45),
                Price DECIMAL(35,8),
                Qty DECIMAL(50,8),
                ExecType VARCHAR(45),
                OrderStatus VARCHAR(45),
                OrderSn VARCHAR(45),
                Yield VARCHAR(45),
                NetPrice DECIMAL(50,8),
                Remark VARCHAR(1000),
                Rejecter VARCHAR(45),
                RejecterCode VARCHAR(45),
                CreateTime VARCHAR(45),
                UpdateTime VARCHAR(45),
                Info1 TEXT,
                Info2 TEXT,
                Info3 TEXT,
                Info4 TEXT,
                Info5 TEXT,
                Info10 TEXT,
                CounterpartyId VARCHAR(45),
                CounterpartyName VARCHAR(500),
                CounterpartySubId VARCHAR(45),
                CounterpartySubName VARCHAR(500),
                CounterpartyCode VARCHAR(45),
                PartyId VARCHAR(45),
                UserId VARCHAR(45),
                FirmFlag VARCHAR(45),
                WholeFlag VARCHAR(45),
                Name VARCHAR(45),
                QuantID VARCHAR(45),
                PartyName VARCHAR(45),
                PartySubName VARCHAR(45),
                PartyCode VARCHAR(45),
                SettlType VARCHAR(45),
                Side VARCHAR(45),
                TimeInForce VARCHAR(45),
                OrdType VARCHAR(45),
                ClOrdID VARCHAR(45),
                DirtyPrice DECIMAL(50,8),
                TransactTime VARCHAR(45),
                BlockNum VARCHAR(45),
                TradeDate VARCHAR(45),
                SettlDate VARCHAR(45),
                CounterpartyOrdId VARCHAR(45),
                InstitutionId VARCHAR(45),
                InstitutionName VARCHAR(3000),
                TraderId VARCHAR(45),
                TraderName VARCHAR(3000),
                CounterpartyFullName VARCHAR(45),
                StrategyOrderVenues VARCHAR(45),
                CounterpartyVenues VARCHAR(45),
                DelFlag char (1),
                SendingTime VARCHAR(45),
                Vs VARCHAR(45),
                BookID VARCHAR(45),
                PartySubId2 VARCHAR(45),
                IntentionId VARCHAR(45),
                OffsetFlag VARCHAR(45),
                HedgeFlag VARCHAR(45),
                PriceType VARCHAR(45),
                AccountId VARCHAR(45),
                AccountName VARCHAR(45),
                CounterAccountId VARCHAR(45),
                CounterAccountName VARCHAR(45),
                BargainFlag VARCHAR(45),
                ExchangeSignFlag VARCHAR(45),
                VipCode VARCHAR(45),
                CompanyId VARCHAR(45),
                YieldToExecution DECIMAL(35,8),
                LastPx VARCHAR(45)
            );";

        private const string UpdateExecutionReportSql = @"REPLACE INTO ExecutionReport (
                Key,
                ExecId,
                StrategyOrderId,
                OrderID,
                QuoteID,
                VenueTypeGW,
                Venues,
                SecurityID,
                Symbol,
                ValidUntilTime,
                MarketIndicator,
                PartySubId,
                UserName,
                Price,
                LastQty,
                Qty,
                ExecType,
                OrderStatus,
                OrderSn,
                Yield,
                NetPrice,
                Remark,
                Rejecter,
                RejecterCode,
                UpdateTime,
                Info1,
                Info2,
                Info3,
                Info4,
                Info5,
                Info10,
                CounterpartyId,
                CounterpartyName,
                CounterpartySubId,
                CounterpartySubName,
                CounterpartyCode,
                PartyId,
                UserId,
                FirmFlag,
                WholeFlag,
                Name,
                QuantID,
                PartyName,
                PartySubName,
                PartyCode,
                SettlType,
                Side,
                TimeInForce,
                OrdType,
                ClOrdID,
                DirtyPrice,
                TransactTime,
                BlockNum,
                TradeDate,
                SettlDate,
                CounterpartyOrdId,
                InstitutionId,
                InstitutionName,
                TraderId,
                TraderName,
                CounterpartyFullName,
                StrategyOrderVenues,
                CounterpartyVenues,
                DelFlag,
                SendingTime,
                Vs,
                BookID,
                IntentionId,
                OffsetFlag,
                HedgeFlag,
                PriceType,
                AccountId,
                AccountName,
                CounterAccountId,
                CounterAccountName,
                BargainFlag,
                ExchangeSignFlag,
                VipCode,
                CompanyId,
                YieldToExecution,
                LastPx
            ) VALUES (
                @key,
                @execid,
                @strategyorderid,
                @orderid,
                @quoteid,
                @venuetypegw,
                @venues,
                @securityid,
                @symbol,
                @validuntiltime,
                @marketindicator,
                @partysubid,
                @username,
                @price,
                @lastqty,
                @qty,
                @exectype,
                @orderstatus,
                @ordersn,
                @yield,
                @netprice,
                @remark,
                @rejecter,
                @rejectercode,
                @updatetime,
                @info1,
                @info2,
                @info3,
                @info4,
                @info5,
                @info10,
                @counterpartyid,
                @counterpartyname,
                @counterpartysubid,
                @counterpartysubname,
                @counterpartycode,
                @partyid,
                @userid,
                @firmflag,
                @wholeflag,
                @name,
                @quantid,
                @partyname,
                @partysubname,
                @partycode,
                @settltype,
                @side,
                @timeinforce,
                @ordtype,
                @clordid,
                @dirtyprice,
                @transacttime,
                @blocknum,
                @tradedate,
                @settldate,
                @counterpartyordid,
                @institutionid,
                @institutionname,
                @traderid,
                @tradername,
                @counterpartyfullname,
                @strategyordervenues,
                @counterpartyvenues,
                @delflag,
                @sendingtime,
                @vs,
                @bookid,
                @intentionid,
                @offsetflag,
                @hedgeflag,
                @pricetype,
                @accountid,
                @accountname,
                @counteraccountid,
                @counteraccountname,
                @bargainflag,
                @exchangesignflag,
                @vipcode,
                @companyid,
                @yieldtoexecution,
                @lastpx
            );";

        private const string QueryExecutionReportSql = @"select * from ExecutionReport where Key = @key order by UpdateTime";


        private const string CreateKeyValueSql = @"CREATE TABLE IF NOT EXISTS KeyValue(
                Key VARCHAR(100) UNIQUE,
                Remark VARCHAR(1000)
            );";


        private const string UpdateKeyValueSql = @"REPLACE INTO KeyValue (
                Key,
                Remark
                ) VALUES (
                @key,
                @remark
            );";

        private const string QueryKeyValueSql = "SELECT * FROM KeyValue where Key = @key";


        private const string DeleteExecutionReportSql = @"DELETE FROM ExecutionReport WHERE Key = @key";

        private const string DeleteLastExecutionReportSql = @"DELETE FROM ExecutionReport WHERE Key = @key";

        private const string DeleteMarketExecutionsItemSql = @"DELETE FROM MarketExecutionsItem WHERE Key = @key";

        #endregion

        private static string RootPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data");
        private static string DataPath = System.IO.Path.Combine(RootPath, $"{DateTime.Now.ToString("yyyyMMdd")}.{LocalEnv.UserInfo.UserName}");

        private static ConcurrentDictionary<string, SQLiteConnection> htDB = new ConcurrentDictionary<string, SQLiteConnection>();
        private volatile bool _isClosed = false;  // 关闭标志

        private Dictionary<string, string> KeyList = new Dictionary<string, string>();
        /// <summary>
        /// 系统在忙
        /// </summary>
        public static bool IsBbusy = false;

        public STCSqliteHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
            KeyList.Add("LastUpdateTime", "LastUpdateTime");
        }

        public void Start()
        {
            try
            {
                CheckDirectionary();

                //创建  MarketExecutionsItem 已处理市场成交
                //GetOrCreateDb(STCDbHandler.DbMarketExecution);

                //创建 市场成交原始ER报文  ExecutionReport
                GetOrCreateDb(STCDbHandler.DbCacheMarketExecutions);

                //最后成交  ExecutionReport
                GetOrCreateDb(STCDbHandler.DbLastExecution);

                //KeyValue
                GetOrCreateDb(STCDbHandler.DbKeyValue);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }


        /// <summary>
        /// BondDb,LastExecution,MarketExecution,CacheMarketExecutions
        /// </summary>
        /// <param name="dbkey"></param>
        /// <returns></returns>
        private SQLiteConnection GetOrCreateDb(string dbkey)
        {
            SQLiteConnection conn = null;
            try
            {
                if (htDB.ContainsKey(dbkey))
                {
                    conn = htDB[dbkey];
                    if (conn == null || conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                }
                else
                {
                    string dbFile = string.Empty;
                    if (!string.IsNullOrEmpty(DataPath))
                    {
                        if (LocalEnv.ConfInfo.LocalTableType == "Sqlite")
                            dbFile = Path.Combine(DataPath, $"{dbkey}.db");
                        else
                            dbFile = $":memory:";
                    }
                    
                    //如果在内存中使用
                    bool isExistDbFile = false;

                    if (!dbFile.Contains(":memory:"))
                    {
                        isExistDbFile = File.Exists(dbFile);
                    }
                    
                    string connectionString = string.Empty;

                    if (dbFile.Contains(":memory:"))
                        connectionString = string.Format("Data Source={0};Version=3;mode=memory;Cache=Shared;", dbFile);
                    else
                        connectionString = string.Format("Data Source={0};Version=3;", dbFile);

                    conn = new SQLiteConnection(connectionString);
                    conn.Open();

                    if (!isExistDbFile)
                    {
                        switch (dbkey)
                        {
                            case STCDbHandler.DbCacheMarketExecutions:
                                //市场成交原始ER报文  ExecutionReport
                                ExecuteNonQuery(CreateExecutionReportSql, conn);
                                break;
                            case STCDbHandler.DbLastExecution:
                                //最后成交  ExecutionReport
                                ExecuteNonQuery(CreateExecutionReportSql, conn);
                                break;
                            //case STCDbHandler.DbMarketExecution:
                            //    //MarketExecutionsItem 已处理市场成交
                            //    ExecuteNonQuery(CreateMarketExecutionsItemSql, conn);
                            //    break;
                            case STCDbHandler.DbKeyValue:
                                //KeyValue
                                ExecuteNonQuery(CreateKeyValueSql, conn);
                                break;
                        }
                    }

                    htDB[dbkey] = conn;
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error("GetOrCreateDb exception for key " + dbkey + ": " + ex.Message);
            }

            return conn;
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        private void CheckDirectionary()
        {
            try
            {
                if (IsNewDay())
                {
                    //如果是新的一天，则关闭数据库
                    Dispose();
                    if (LocalEnv.ConfInfo.LocalTableType != "SQLITE")
                        System.IO.Directory.CreateDirectory(DataPath);
                    IsBbusy = false;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public void Dispose()
        {
            try
            {
                //LogHelper.Info("STCSqliteHandler.Dispose() called");
                _isClosed = true;  // 设置关闭标志
                
                foreach (string dbkey in htDB.Keys.ToList())
                {
                    CloseDB(dbkey);
                }
                
                htDB.Clear();
                //LogHelper.Info("STCSqliteHandler.Dispose() completed");
            }
            catch (Exception ex)
            {
                LogHelper.Error("Error in Dispose(): " + ex.Message);
            }
        }

        /// <summary>
        /// 关闭数据库
        /// </summary>
        /// <param name="dbkey"></param>
        private void CloseDB(string dbkey)
        {
            try
            {
                SQLiteConnection conn;
                htDB.TryGetValue(dbkey, out conn);

                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                conn?.Dispose();
                conn = null;

                htDB.TryRemove(dbkey, out conn);
            }
            catch
            { 
            
            }
        }

        /// <summary>
        /// 重启消息处理程序
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                IsBbusy = true;

                if (IsNewDay())
                {
                    CheckDirectionary();
                }
                else
                {
                    IsBbusy = false;
                }
            }
        }
        /// <summary>
        /// 获取估算键的数量
        /// </summary>
        /// <param name="dbkey"></param>
        /// <returns></returns>
        public int GetRecordCount(string dbkey)
        {
            int count = -1;
            try
            {
                //如果是字典
                SQLiteConnection _db = GetOrCreateDb(dbkey);

                string tableName = string.Empty;
                switch (dbkey)
                {
                    case STCDbHandler.DbCacheMarketExecutions:
                        //市场成交原始ER报文  ExecutionReport
                        tableName = "ExecutionReport";
                        break;
                    case STCDbHandler.DbLastExecution:
                        //最后成交  ExecutionReport
                        tableName = "ExecutionReport";
                        break;
                    //case STCDbHandler.DbMarketExecution:
                    //    //MarketExecutionsItem 已处理市场成交
                    //    tableName = "MarketExecutionsItem";
                    //    break;
                    case STCDbHandler.DbKeyValue:
                        tableName = "KeyValue";
                        break;
                }

                string query = string.Format("SELECT COUNT(*) FROM {0}", tableName);

                using (var command = new SQLiteCommand(query, _db))
                {
                    try
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                    catch (SQLiteException e)
                    {
                        LogHelper.Error("GetRecordCount SQLiteException: " + e.Message);
                        count = -1;
                    }
                }
            }
            catch
            {
                return 0;
            }

            return count;
        }


        public bool IsNewDay()
        {
            DataPath = System.IO.Path.Combine(RootPath, $"{DateTime.Now.ToString("yyyyMMdd")}.{LocalEnv.UserInfo.UserName}");
            return !System.IO.Directory.Exists(DataPath);
        }

        public void Put(string dbkey, string key, object txt)
        {
            switch (dbkey)
            {
                case STCDbHandler.DbCacheMarketExecutions:
                    //市场成交原始ER报文  ExecutionReport
                    if (txt.GetType() == typeof(ExecutionReport))
                    {
                        ExecutionReport er = txt as ExecutionReport;
                        UpdExecutionReport(dbkey, key, er);
                    }
                    else
                    {
                        if (!KeyList.ContainsKey(key))
                        {
                            KeyList.Add(key, key);
                        }

                        UpdKeyValue(STCDbHandler.DbKeyValue, key, txt as string);
                    }

                    break;
                case STCDbHandler.DbLastExecution:
                    //最后成交  ExecutionReport
                    if (txt.GetType() == typeof(ExecutionReport))
                    {
                        ExecutionReport le = txt as ExecutionReport;
                        UpdExecutionReport(dbkey, key, le);
                    }
                    else
                    {
                        //UpdKeyValue(STCDbHandler.DbKeyValue, key, txt as string);
                    }

                    break;
                //case STCDbHandler.DbMarketExecution:

                //    //MarketExecutionsItem 已处理市场成交
                //    if (txt.GetType() == typeof(MarketExecutionsItem))
                //    {
                //        MarketExecutionsItem me = txt as MarketExecutionsItem;
                //    }

                //    break;
            }
        }


        private void UpdKeyValue(string dbkey, string key, string obj)
        {
            try
            {
                if (_isClosed)
                {
                    //LogHelper.Warn("UpdKeyValue: Database handler is closed, operation skipped");
                    return;
                }

                if (obj != null)
                {
                    SQLiteConnection _db = GetOrCreateDb(dbkey);
                    if (_db == null || _db.State == ConnectionState.Closed)
                    {
                        //LogHelper.Warn("UpdKeyValue: Database connection is invalid, operation skipped");
                        return;
                    }

                    using (SQLiteCommand cmd = new SQLiteCommand(UpdateKeyValueSql, _db))
                    {
                        cmd.Parameters.AddRange(new[]
                        {
                            new SQLiteParameter("@key", key),
                            new SQLiteParameter("@remark", obj),
                        });

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SQLiteException ex)
            {
                LogHelper.Error("UpdKeyValue SQLiteException: " + ex.Message);
            }
            catch (Exception ex)
            {
                LogHelper.Error("UpdKeyValue exception: " + ex.Message);
            }
        }

        private void UpdExecutionReport(string dbkey, string key, ExecutionReport dr)
        {
            try
            {
                if (dr != null)
                {
                    SQLiteConnection _db = GetOrCreateDb(dbkey);
                    SQLiteCommand cmd = new SQLiteCommand(UpdateExecutionReportSql, _db);
                    cmd.Parameters.AddRange(new[]
                    {
                        new SQLiteParameter("@key", key),
                        new SQLiteParameter("@execid", dr.ExecID),
                        new SQLiteParameter("@strategyorderid", dr.StrategyOrderId),
                        new SQLiteParameter("@orderid", dr.OrderID),
                        new SQLiteParameter("@quoteid", dr.QuoteID),
                        new SQLiteParameter("@venuetypegw", dr.VenueTypeGW),
                        new SQLiteParameter("@venues", dr.Venues),
                        new SQLiteParameter("@securityid", dr.SecurityID),
                        new SQLiteParameter("@symbol", dr.Symbol),
                        new SQLiteParameter("@validuntiltime", dr.ValidUntilTime),
                        new SQLiteParameter("@marketindicator", dr.MarketIndicator),
                        new SQLiteParameter("@partysubid", dr.PartySubID),
                        new SQLiteParameter("@username", dr.UserName),
                        new SQLiteParameter("@price", dr.Price),
                        new SQLiteParameter("@qty", dr.OrderQty),
                        new SQLiteParameter("@LastQty", dr.LastQty),
                        new SQLiteParameter("@exectype", dr.ExecType),
                        new SQLiteParameter("@orderstatus", dr.OrdStatus),
                        new SQLiteParameter("@ordersn", dr.OrderSN),
                        new SQLiteParameter("@yield", dr.Yield),
                        new SQLiteParameter("@netprice", dr.NetPrice),
                        new SQLiteParameter("@remark", dr.Remark),
                        new SQLiteParameter("@rejecter", dr.Rejecter),
                        new SQLiteParameter("@rejectercode", dr.RejectText),
                        new SQLiteParameter("@updatetime", dr.UpdateTime),
                        new SQLiteParameter("@info1", dr.Info1),
                        new SQLiteParameter("@info2", dr.Info2),
                        new SQLiteParameter("@info3", dr.Info3),
                        new SQLiteParameter("@info4", dr.Info4),
                        new SQLiteParameter("@info5", dr.Info5),
                        new SQLiteParameter("@info10", dr.Info10),
                        new SQLiteParameter("@counterpartyid", dr.CounterPartyID),
                        new SQLiteParameter("@counterpartyname", dr.CounterPartyName),
                        new SQLiteParameter("@counterpartysubid", dr.CounterPartySubID),
                        new SQLiteParameter("@counterpartysubname", dr.CounterPartySubName),
                        new SQLiteParameter("@counterpartycode", dr.CounterPartyCode),
                        new SQLiteParameter("@partyid", dr.PartyID),
                        new SQLiteParameter("@userid", dr.UserID),
                        new SQLiteParameter("@firmflag", dr.FirmFlag),
                        new SQLiteParameter("@wholeflag", dr.WholeFlag),
                        new SQLiteParameter("@name", dr.Name),
                        new SQLiteParameter("@quantid", dr.QuantID),
                        new SQLiteParameter("@partyname", dr.PartyName),
                        new SQLiteParameter("@partysubname", dr.PartySubName),
                        new SQLiteParameter("@partycode", dr.PartyCode),
                        new SQLiteParameter("@settltype", dr.SettlType),
                        new SQLiteParameter("@side", dr.Side),
                        new SQLiteParameter("@timeinforce", dr.TimeInForce),
                        new SQLiteParameter("@ordtype", dr.OrdType),
                        new SQLiteParameter("@clordid", dr.ClOrdID),
                        new SQLiteParameter("@dirtyprice", dr.DirtyPrice),
                        new SQLiteParameter("@transacttime", dr.TransactTime),
                        new SQLiteParameter("@blocknum", dr.BlockNum),
                        new SQLiteParameter("@tradedate", dr.TradeDate),
                        new SQLiteParameter("@settldate", dr.SettlDate),
                        new SQLiteParameter("@counterpartyordid", dr.CounterPartyOrdID),
                        new SQLiteParameter("@institutionid", dr.InstitutionID),
                        new SQLiteParameter("@institutionname", dr.InstitutionName),
                        new SQLiteParameter("@traderid", dr.TraderID),
                        new SQLiteParameter("@tradername", dr.TraderName),
                        new SQLiteParameter("@counterpartyfullname", dr.CounterPartyFullName),
                        new SQLiteParameter("@strategyordervenues", dr.StrategyOrderVenues),
                        new SQLiteParameter("@counterpartyvenues", dr.CounterPartyVenues),
                        new SQLiteParameter("@delflag", dr.DelFlag),
                        new SQLiteParameter("@sendingtime", dr.SendingTime),
                        new SQLiteParameter("@vs", dr.Vs),
                        new SQLiteParameter("@bookid", dr.BookID),
                        new SQLiteParameter("@intentionid", dr.IntentionID),
                        new SQLiteParameter("@offsetflag", dr.OffsetFlag),
                        new SQLiteParameter("@hedgeflag", dr.HedgeFlag),
                        new SQLiteParameter("@pricetype", dr.PriceType),
                        new SQLiteParameter("@accountid", dr.AccountID),
                        new SQLiteParameter("@accountname", dr.AccountName),
                        new SQLiteParameter("@counteraccountid", dr.CounterAccountID),
                        new SQLiteParameter("@counteraccountname", dr.CounterAccountName),
                        new SQLiteParameter("@bargainflag", dr.BargainFlag),
                        new SQLiteParameter("@exchangesignflag", dr.ExchangeSignFlag),
                        new SQLiteParameter("@vipcode", dr.VipCode),
                        new SQLiteParameter("@companyid", dr.CompanyId),
                        new SQLiteParameter("@yieldtoexecution", dr.YieldToExecution),
                        new SQLiteParameter("@lastpx",dr.LastPx)
                    });

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SQLiteException ex)
            {
                LogHelper.Error("UpdExecutionReport SQLiteException: " + ex.Message);
            }
            catch (Exception ex)
            {
                LogHelper.Error("UpdExecutionReport exception: " + ex.Message);
            }
        }

        public string Get(string dbkey, string key)
        {
            string res = string.Empty;
            try
            {
                if (KeyList.ContainsKey(key))
                {
                    return QueryKeyValue(key);
                }
                else
                {
                    switch (dbkey)
                    {
                        case STCDbHandler.DbCacheMarketExecutions:
                            //市场成交原始ER报文  ExecutionReport
                            return QueryExecutionReport(dbkey, key);
                            //QueryExecutionReportSql
                            break;
                        case STCDbHandler.DbLastExecution:
                            //最后成交  ExecutionReport
                            return QueryExecutionReport(dbkey, key);
                            break;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                LogHelper.Error("SQLiteException: " + ex.Message);
            }
            catch (Exception ex)
            {
                LogHelper.Error("查询数据发生异常: " + ex.Message);
            }

            return res;
        }

        private string QueryKeyValue(string key)
        {
            string res = string.Empty;

            try
            {
                //如果是字典
                SQLiteConnection _db = GetOrCreateDb(STCDbHandler.DbKeyValue);
                using (var command = new SQLiteCommand(QueryKeyValueSql, _db))
                {
                    command.Parameters.AddWithValue("@key", key);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            res = reader["Remark"].ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }

            return res;
        }

        private string QueryExecutionReport(string dbkey, string key)
        {
            string res = string.Empty;
            try
            {
                //如果是字典
                SQLiteConnection _db = GetOrCreateDb(dbkey);
                using (var command = new SQLiteCommand(QueryExecutionReportSql, _db))
                {
                    command.Parameters.AddWithValue("@key", key);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ExecutionReport
                            {
                                ExecID = reader["ExecId"].ToString(),
                                StrategyOrderId = reader["StrategyOrderId"].ToString(),
                                OrderID = reader["OrderID"].ToString(),
                                QuoteID = reader["QuoteID"].ToString(),
                                VenueTypeGW = reader["VenueTypeGW"].ToString(),
                                Venues = reader["Venues"].ToString(),
                                SecurityID = reader["SecurityID"].ToString(),
                                Symbol = reader["Symbol"].ToString(),
                                ValidUntilTime = reader["ValidUntilTime"].ToString(),
                                MarketIndicator = reader["MarketIndicator"].ToString(),
                                PartySubID = reader["PartySubId"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                Price = reader["Price"].ToString(),
                                LastQty = reader["LastQty"].ToString(),
                                OrderQty = reader["Qty"].ToString(),
                                ExecType = reader["ExecType"].ToString(),
                                OrdStatus = reader["OrderStatus"].ToString(),
                                OrderSN = reader["OrderSn"].ToString(),
                                Yield = reader["Yield"].ToString(),
                                NetPrice = reader["NetPrice"].ToString(),
                                Remark = reader["Remark"].ToString(),
                                Rejecter = reader["Rejecter"].ToString(),
                                RejectText = reader["RejecterCode"].ToString(),
                                TransactTime = reader["TransactTime"].ToString(),
                                UpdateTime = reader["UpdateTime"].ToString(),
                                Info1 = reader["Info1"].ToString(),
                                Info2 = reader["Info2"].ToString(),
                                Info3 = reader["Info3"].ToString(),
                                Info4 = reader["Info4"].ToString(),
                                Info5 = reader["Info5"].ToString(),
                                Info10 = reader["Info10"].ToString(),
                                CounterPartyID = reader["CounterpartyId"].ToString(),
                                CounterPartyName = reader["CounterpartyName"].ToString(),
                                CounterPartySubID = reader["CounterpartySubId"].ToString(),
                                CounterPartySubName = reader["CounterpartySubName"].ToString(),
                                CounterPartyCode = reader["CounterpartyCode"].ToString(),
                                PartyID = reader["PartyId"].ToString(),
                                UserID = reader["UserId"].ToString(),
                                FirmFlag = reader["FirmFlag"].ToString(),
                                WholeFlag = reader["WholeFlag"].ToString(),
                                Name = reader["Name"].ToString(),
                                QuantID = reader["QuantID"].ToString(),
                                PartyName = reader["PartyName"].ToString(),
                                PartySubName = reader["PartySubName"].ToString(),
                                PartyCode = reader["PartyCode"].ToString(),
                                SettlType = reader["SettlType"].ToString(),
                                Side = reader["Side"].ToString(),
                                TimeInForce = reader["TimeInForce"].ToString(),
                                OrdType = reader["OrdType"].ToString(),
                                ClOrdID = reader["ClOrdID"].ToString(),
                                DirtyPrice = reader["DirtyPrice"].ToString(),
                                BlockNum = reader["BlockNum"].ToString(),
                                TradeDate = reader["TradeDate"].ToString(),
                                SettlDate = reader["SettlDate"].ToString(),
                                CounterPartyOrdID = reader["CounterpartyOrdId"].ToString(),
                                InstitutionID = reader["InstitutionId"].ToString(),
                                InstitutionName = reader["InstitutionName"].ToString(),
                                TraderID = reader["TraderId"].ToString(),
                                TraderName = reader["TraderName"].ToString(),
                                CounterPartyFullName = reader["CounterpartyFullName"].ToString(),
                                StrategyOrderVenues = reader["StrategyOrderVenues"].ToString(),
                                CounterPartyVenues = reader["CounterpartyVenues"].ToString(),
                                DelFlag = reader["DelFlag"].ToString(),
                                SendingTime = reader["SendingTime"].ToString(),
                                Vs = reader["Vs"].ToString(),
                                BookID = reader["BookID"].ToString(),
                                IntentionID = reader["IntentionId"].ToString(),
                                OffsetFlag = reader["OffsetFlag"].ToString(),
                                HedgeFlag = reader["HedgeFlag"].ToString(),
                                PriceType = reader["PriceType"].ToString(),
                                AccountID = reader["AccountId"].ToString(),
                                AccountName = reader["AccountName"].ToString(),
                                CounterAccountID = reader["CounterAccountId"].ToString(),
                                CounterAccountName = reader["CounterAccountName"].ToString(),
                                BargainFlag = reader["BargainFlag"].ToString(),
                                ExchangeSignFlag = reader["ExchangeSignFlag"].ToString(),
                                VipCode = reader["VipCode"].ToString(),
                                CompanyId = reader["CompanyId"].ToString(),
                                YieldToExecution = NormalizeYieldToExecution(reader["YieldToExecution"]),
                                LastPx = reader["LastPx"].ToString()
                            };

                            res = CommonUtils.SerializeObject(item);
                            break;
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }

            return res;
        }

        /// <summary>
        /// 规整 ExecutionReport.YieldToExecution（行权收益率）的读取值。
        /// YieldToExecution 列为 DECIMAL，后台 null 或无值会被存/读成数值 0；
        /// 业务上 0 表示“无行权收益率”，统一规整为空串，避免上层把 "0" 当成有效值显示。
        /// </summary>
        private static string NormalizeYieldToExecution(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            decimal dec;
            if (decimal.TryParse(str, out dec) && dec == 0)
                return string.Empty;

            return str;
        }

        public void Delete(string dbkey, string key)
        {
            try
            {
                //如果是字典
                SQLiteConnection _db = GetOrCreateDb(dbkey);

                switch (dbkey)
                {
                    case STCDbHandler.DbCacheMarketExecutions:
                        //市场成交原始ER报文  ExecutionReport
                        using (var cmd = new SQLiteCommand(DeleteExecutionReportSql, _db))
                        {
                            cmd.Parameters.AddWithValue("@key", key);
                            cmd.ExecuteNonQuery();
                        }

                        break;

                    case STCDbHandler.DbMarketExecution:
                        //todo...
                        //已处理市场成交 MarketExecutionsItem
                        ////using (var cmd = new SQLiteCommand(DeleteMarketExecutionsItemSql, _db))
                        ////{
                        ////    cmd.Parameters.AddWithValue("@key", key);
                        ////    cmd.ExecuteNonQuery();
                        ////}
                        
                        break;

                    case STCDbHandler.DbLastExecution:
                        //最后成交  ExecutionReport
                        using (var cmd = new SQLiteCommand(DeleteLastExecutionReportSql, _db))
                        {
                            cmd.Parameters.AddWithValue("@key", key);
                            cmd.ExecuteNonQuery();
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("SQLite Delete Exception: " + ex.Message);
            }
        }


        public List<ExecutionReport> GetAllCache(string dbkey, int num = -1)
        {
            SQLiteConnection _db = GetOrCreateDb(dbkey);
            var items = new List<ExecutionReport>();

            // 根据 num 来判断 SQL 查询的不同
            string query;
            if (num == -1)
            {
                query = "SELECT * FROM ExecutionReport";
            }
            else
            {
                query = "SELECT * FROM ExecutionReport ORDER BY CreateTime DESC LIMIT @num";
            }

            using (var command = new SQLiteCommand(query, _db))
            {
                if (num > 0)
                {
                    command.Parameters.AddWithValue("@num", num);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ExecutionReport
                        {
                            ExecID = reader["ExecId"].ToString(),
                            StrategyOrderId = reader["StrategyOrderId"].ToString(),
                            OrderID = reader["OrderID"].ToString(),
                            QuoteID = reader["QuoteID"].ToString(),
                            VenueTypeGW = reader["VenueTypeGW"].ToString(),
                            Venues = reader["Venues"].ToString(),
                            SecurityID = reader["SecurityID"].ToString(),
                            Symbol = reader["Symbol"].ToString(),
                            ValidUntilTime = reader["ValidUntilTime"].ToString(),
                            MarketIndicator = reader["MarketIndicator"].ToString(),
                            PartySubID = reader["PartySubId"].ToString(),
                            UserName = reader["UserName"].ToString(),
                            Price = reader["Price"].ToString(),
                            LastQty = reader["LastQty"].ToString(),
                            OrderQty = reader["Qty"].ToString(),
                            ExecType = reader["ExecType"].ToString(),
                            OrdStatus = reader["OrderStatus"].ToString(),
                            OrderSN = reader["OrderSn"].ToString(),
                            Yield = reader["Yield"].ToString(),
                            NetPrice = reader["NetPrice"].ToString(),
                            Remark = reader["Remark"].ToString(),
                            Rejecter = reader["Rejecter"].ToString(),
                            RejectText = reader["RejecterCode"].ToString(),
                            TransactTime = reader["TransactTime"].ToString(),
                            UpdateTime = reader["UpdateTime"].ToString(),
                            Info1 = reader["Info1"].ToString(),
                            Info2 = reader["Info2"].ToString(),
                            Info3 = reader["Info3"].ToString(),
                            Info4 = reader["Info4"].ToString(),
                            Info5 = reader["Info5"].ToString(),
                            Info10 = reader["Info10"].ToString(),
                            CounterPartyID = reader["CounterpartyId"].ToString(),
                            CounterPartyName = reader["CounterpartyName"].ToString(),
                            CounterPartySubID = reader["CounterpartySubId"].ToString(),
                            CounterPartySubName = reader["CounterpartySubName"].ToString(),
                            CounterPartyCode = reader["CounterpartyCode"].ToString(),
                            PartyID = reader["PartyId"].ToString(),
                            UserID = reader["UserId"].ToString(),
                            FirmFlag = reader["FirmFlag"].ToString(),
                            WholeFlag = reader["WholeFlag"].ToString(),
                            Name = reader["Name"].ToString(),
                            QuantID = reader["QuantID"].ToString(),
                            PartyName = reader["PartyName"].ToString(),
                            PartySubName = reader["PartySubName"].ToString(),
                            PartyCode = reader["PartyCode"].ToString(),
                            SettlType = reader["SettlType"].ToString(),
                            Side = reader["Side"].ToString(),
                            TimeInForce = reader["TimeInForce"].ToString(),
                            OrdType = reader["OrdType"].ToString(),
                            ClOrdID = reader["ClOrdID"].ToString(),
                            DirtyPrice = reader["DirtyPrice"].ToString(),
                            BlockNum = reader["BlockNum"].ToString(),
                            TradeDate = reader["TradeDate"].ToString(),
                            SettlDate = reader["SettlDate"].ToString(),
                            CounterPartyOrdID = reader["CounterpartyOrdId"].ToString(),
                            InstitutionID = reader["InstitutionId"].ToString(),
                            InstitutionName = reader["InstitutionName"].ToString(),
                            TraderID = reader["TraderId"].ToString(),
                            TraderName = reader["TraderName"].ToString(),
                            CounterPartyFullName = reader["CounterpartyFullName"].ToString(),
                            StrategyOrderVenues = reader["StrategyOrderVenues"].ToString(),
                            CounterPartyVenues = reader["CounterpartyVenues"].ToString(),
                            DelFlag = reader["DelFlag"].ToString(),
                            SendingTime = reader["SendingTime"].ToString(),
                            Vs = reader["Vs"].ToString(),
                            BookID = reader["BookID"].ToString(),
                            IntentionID = reader["IntentionId"].ToString(),
                            OffsetFlag = reader["OffsetFlag"].ToString(),
                            HedgeFlag = reader["HedgeFlag"].ToString(),
                            PriceType = reader["PriceType"].ToString(),
                            AccountID = reader["AccountId"].ToString(),
                            AccountName = reader["AccountName"].ToString(),
                            CounterAccountID = reader["CounterAccountId"].ToString(),
                            CounterAccountName = reader["CounterAccountName"].ToString(),
                            BargainFlag = reader["BargainFlag"].ToString(),
                            ExchangeSignFlag = reader["ExchangeSignFlag"].ToString(),
                            VipCode = reader["VipCode"].ToString(),
                            CompanyId = reader["CompanyId"].ToString(),
                            YieldToExecution = NormalizeYieldToExecution(reader["YieldToExecution"]),
                            LastPx = reader["LastPx"].ToString()
                        };

                        items.Add(item);
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// 根据key返回原始市场成交
        /// </summary>
        /// <param name="dbkey"></param>
        /// <param name="prefix"></param>
        /// <param name="orderasc">升序 true, 降序 false</param>
        /// <param name="num">全部 -1 </param>
        /// <returns></returns>
        /// <example>List<ExecutionReport> tmpER = STC.Common.Data.STCRocksDbHandler.Instance.GetAllCacheByKey(STC.Common.Data.STCRocksDbHandler.DbCacheMarketExecutions, _searchText, false, 20);</example>

        public List<ExecutionReport> GetAllCacheByKey(string dbkey, string prefix, bool orderasc = true, int num = -1)
        {
            SQLiteConnection _db = GetOrCreateDb(dbkey);
            var items = new List<ExecutionReport>();

            // 根据 num 来判断 SQL 查询的不同
            string query;
            if (num == -1)
            {
                if (orderasc)
                    query = @"SELECT * FROM ExecutionReport where SecurityID = @sec ORDER BY UpdateTime";
                else
                    query = @"SELECT * FROM ExecutionReport where SecurityID = @sec ORDER BY UpdateTime DESC";
            }
            else
            {
                if (orderasc)
                    query = @"SELECT * FROM ExecutionReport where SecurityID = @sec ORDER BY UpdateTime LIMIT @num";
                else
                    query = @"SELECT * FROM ExecutionReport where SecurityID = @sec ORDER BY UpdateTime DESC LIMIT @num";
            }

            using (var command = new SQLiteCommand(query, _db))
            {
                command.Parameters.AddWithValue("@sec", prefix);

                if (num > 0)
                {
                    command.Parameters.AddWithValue("@num", num);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ExecutionReport
                        {
                            ExecID = reader["ExecId"].ToString(),
                            StrategyOrderId = reader["StrategyOrderId"].ToString(),
                            OrderID = reader["OrderID"].ToString(),
                            QuoteID = reader["QuoteID"].ToString(),
                            VenueTypeGW = reader["VenueTypeGW"].ToString(),
                            Venues = reader["Venues"].ToString(),
                            SecurityID = reader["SecurityID"].ToString(),
                            Symbol = reader["Symbol"].ToString(),
                            ValidUntilTime = reader["ValidUntilTime"].ToString(),
                            MarketIndicator = reader["MarketIndicator"].ToString(),
                            PartySubID = reader["PartySubId"].ToString(),
                            UserName = reader["UserName"].ToString(),
                            Price = reader["Price"].ToString(),
                            LastQty = reader["LastQty"].ToString(),
                            OrderQty = reader["Qty"].ToString(),
                            ExecType = reader["ExecType"].ToString(),
                            OrdStatus = reader["OrderStatus"].ToString(),
                            OrderSN = reader["OrderSn"].ToString(),
                            Yield = reader["Yield"].ToString(),
                            NetPrice = reader["NetPrice"].ToString(),
                            Remark = reader["Remark"].ToString(),
                            Rejecter = reader["Rejecter"].ToString(),
                            RejectText = reader["RejecterCode"].ToString(),
                            TransactTime = reader["TransactTime"].ToString(),
                            UpdateTime = reader["UpdateTime"].ToString(),
                            Info1 = reader["Info1"].ToString(),
                            Info2 = reader["Info2"].ToString(),
                            Info3 = reader["Info3"].ToString(),
                            Info4 = reader["Info4"].ToString(),
                            Info5 = reader["Info5"].ToString(),
                            Info10 = reader["Info10"].ToString(),
                            CounterPartyID = reader["CounterpartyId"].ToString(),
                            CounterPartyName = reader["CounterpartyName"].ToString(),
                            CounterPartySubID = reader["CounterpartySubId"].ToString(),
                            CounterPartySubName = reader["CounterpartySubName"].ToString(),
                            CounterPartyCode = reader["CounterpartyCode"].ToString(),
                            PartyID = reader["PartyId"].ToString(),
                            UserID = reader["UserId"].ToString(),
                            FirmFlag = reader["FirmFlag"].ToString(),
                            WholeFlag = reader["WholeFlag"].ToString(),
                            Name = reader["Name"].ToString(),
                            QuantID = reader["QuantID"].ToString(),
                            PartyName = reader["PartyName"].ToString(),
                            PartySubName = reader["PartySubName"].ToString(),
                            PartyCode = reader["PartyCode"].ToString(),
                            SettlType = reader["SettlType"].ToString(),
                            Side = reader["Side"].ToString(),
                            TimeInForce = reader["TimeInForce"].ToString(),
                            OrdType = reader["OrdType"].ToString(),
                            ClOrdID = reader["ClOrdID"].ToString(),
                            DirtyPrice = reader["DirtyPrice"].ToString(),
                            BlockNum = reader["BlockNum"].ToString(),
                            TradeDate = reader["TradeDate"].ToString(),
                            SettlDate = reader["SettlDate"].ToString(),
                            CounterPartyOrdID = reader["CounterpartyOrdId"].ToString(),
                            InstitutionID = reader["InstitutionId"].ToString(),
                            InstitutionName = reader["InstitutionName"].ToString(),
                            TraderID = reader["TraderId"].ToString(),
                            TraderName = reader["TraderName"].ToString(),
                            CounterPartyFullName = reader["CounterpartyFullName"].ToString(),
                            StrategyOrderVenues = reader["StrategyOrderVenues"].ToString(),
                            CounterPartyVenues = reader["CounterpartyVenues"].ToString(),
                            DelFlag = reader["DelFlag"].ToString(),
                            SendingTime = reader["SendingTime"].ToString(),
                            Vs = reader["Vs"].ToString(),
                            BookID = reader["BookID"].ToString(),
                            IntentionID = reader["IntentionId"].ToString(),
                            OffsetFlag = reader["OffsetFlag"].ToString(),
                            HedgeFlag = reader["HedgeFlag"].ToString(),
                            PriceType = reader["PriceType"].ToString(),
                            AccountID = reader["AccountId"].ToString(),
                            AccountName = reader["AccountName"].ToString(),
                            CounterAccountID = reader["CounterAccountId"].ToString(),
                            CounterAccountName = reader["CounterAccountName"].ToString(),
                            BargainFlag = reader["BargainFlag"].ToString(),
                            ExchangeSignFlag = reader["ExchangeSignFlag"].ToString(),
                            VipCode = reader["VipCode"].ToString(),
                            CompanyId = reader["CompanyId"].ToString(),
                            YieldToExecution = NormalizeYieldToExecution(reader["YieldToExecution"]),
                            LastPx = reader["LastPx"].ToString()
                        };

                        items.Add(item);
                    }
                }
            }

            return items;
        }


        #region function
        private decimal? GetDecimal(SQLiteDataReader reader, int i)
        {
            if (reader.IsDBNull(i))
            {
                return null;
            }
            else
            {
                return reader.GetDecimal(i);
            }
        }

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

        private int ExecuteNonQuery(string sql, SQLiteConnection conn)
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
        #endregion
    }
}
