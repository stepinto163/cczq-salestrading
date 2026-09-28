using com.gateway.log;
using RocksDbSharp;
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
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Data
{
    public class STCDbHandler : ISTCDbHandler
    {
        /// <summary>
        /// MarketExecution 已处理市场成交
        /// </summary>
        public const string DbMarketExecution = "MarketExecution";

        /// <summary>
        /// 市场成交原始ER报文
        /// </summary>
        public const string DbCacheMarketExecutions = "CacheMarketExecutions";

        /// <summary>
        /// 最后成交
        /// </summary>
        public const string DbLastExecution = "LastExecution";

        /// <summary>
        /// Key Value
        /// </summary>
        public const string DbKeyValue = "KeyValue";

        /// <summary>
        /// 系统在忙
        /// </summary>
        public static bool IsBbusy = false;

        private ISTCDbHandler dbHandler;
        public static STCDbHandler Instance = new STCDbHandler();
        /// <summary>
        /// 采用数据库类型 RocksDb,Sqlite,LiteDb
        /// </summary>
        public string dbType = LocalEnv.ConfInfo.LocalTableType ?? "SQLITE";

        public STCDbHandler()
        {
            // 根据传入的 dbType 实例化不同的数据库处理程序
            // RocksDb,Sqlite,SQLITE
            switch (dbType)
            {
                case "RocksDb":
                    dbHandler = new STCRocksDbHandler();
                    break;
                case "Sqlite":
                    dbHandler = new STCSqliteHandler();
                    break;
                case "SQLITE":
                    dbHandler = new STCSqliteHandler();
                    break;
                default:
                    dbHandler = new STCRocksDbHandler();
                    break;
            }
        }

        public void Put(string dbkey, string key, object txt)
        {
            dbHandler.Put(dbkey, key, txt);
        }

        public void Delete(string dbkey, string key)
        {
            dbHandler.Delete(dbkey, key);
        }

        public string Get(string dbkey, string key)
        {
            return dbHandler.Get(dbkey, key);
        }

        //public List<MarketExecutionsItem> GetAll(string dbkey, int num = -1)
        //{
        //    return dbHandler.GetAll(dbkey, num);
        //}

        //public List<MarketExecutionsItem> GetAllByKey(string dbkey, string prefix)
        //{
        //    return dbHandler.GetAllByKey(dbkey, prefix);
        //}

        public List<ExecutionReport> GetAllCache(string dbkey, int num = -1)
        {
            return dbHandler.GetAllCache(dbkey, num);
        }

        public List<ExecutionReport> GetAllCacheByKey(string dbkey, string prefix, bool orderasc = true, int num = -1)
        {
            return dbHandler.GetAllCacheByKey(dbkey, prefix, orderasc, num);
        }

        //public List<MarketExecutionsItem> GetPrevNumByKey(string dbkey, string prefix = null, int num = 200)
        //{
        //    return dbHandler.GetPrevNumByKey(dbkey, prefix, num);
        //}

        public int GetRecordCount(string dbkey)
        {
            return dbHandler.GetRecordCount(dbkey);
        }

        public bool IsNewDay()
        {
            return dbHandler.IsNewDay();
        }



        public void Start()
        {
            dbHandler.Start();
        }

        public void Dispose()
        {
            dbHandler.Dispose();
        }
    }
}
