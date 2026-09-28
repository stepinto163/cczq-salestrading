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
    public class STCRocksDbHandler : ISTCDbHandler
    {
        private static string RootPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data");
        private static string DataPath = System.IO.Path.Combine(RootPath, $"{DateTime.Now.ToString("yyyyMMdd")}.{LocalEnv.UserInfo.UserName}");

        private static ConcurrentDictionary<string, RocksDb> htDB = new ConcurrentDictionary<string, RocksDb>();

        /// <summary>
        /// 系统在忙
        /// </summary>
        public static bool IsBbusy = false;

        public STCRocksDbHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
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

        public bool IsNewDay()
        {
            DataPath = System.IO.Path.Combine(RootPath, $"{DateTime.Now.ToString("yyyyMMdd")}.{LocalEnv.UserInfo.UserName}");
            return !System.IO.Directory.Exists(DataPath);
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
                    CloseAllDB();
                    System.IO.Directory.CreateDirectory(DataPath);
                    IsBbusy = false;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        public void Start()
        {
            try
            {
                CheckDirectionary();

                //创建
                GetOrCreateDb(STCDbHandler.DbMarketExecution);

                //创建
                GetOrCreateDb(STCDbHandler.DbCacheMarketExecutions);

                //最新成交
                GetOrCreateDb(STCDbHandler.DbLastExecution);
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
        private RocksDb GetOrCreateDb(string dbkey)
        {
            RocksDb _db = null;
            try
            {
                if (htDB.ContainsKey(dbkey))
                {
                    _db = htDB[dbkey];
                }
                else
                {
                    DbOptions options = new DbOptions().SetCreateIfMissing(true);
                    options.SetWriteBufferSize(64 * 1024 * 1024); //64M write_buffer_size 决定了 MemTable 的大小。MemTable 是 RocksDB 在内存中的结构，写入操作首先会写入 MemTable，当 MemTable 达到设定的大小时，会被 flush 到磁盘生成 SST 文件
                    options.SetMaxWriteBufferNumber(1000); //控制了同时存在的 MemTable 的最大数量。当写入操作超过最大数量时，RocksDB 会阻塞写操作，直到有 MemTable 被 flush 到磁盘。
                    options.SetDbWriteBufferSize(64 * 1024 * 1024); //128M  限制总的写缓冲区的内存使用量。它控制整个 RocksDB 实例中所有列族使用的总内存。如果总内存超过这个值，将会强制刷新到磁盘
                    //options.SetMaxBackgroundFlushes(100);
                    //options.SetUseDirectReads(true);

                    string tmppath = System.IO.Path.Combine(DataPath, dbkey);

                    _db = RocksDb.Open(options, tmppath);
                    htDB[dbkey] = _db;
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            return _db;
        }

        /// <summary>
        /// 增加记录 
        /// </summary>
        /// <param name="dbkey">BondDb,LastExecution,MarketExecution,CacheMarketExecutions</param>
        /// <param name="key"></param>
        /// <param name="txt"></param>
        public void Put(string dbkey, string key, object txt)
        {
            if (!string.IsNullOrEmpty(key))
            {
                RocksDb _db = GetOrCreateDb(dbkey);
                if (_db != null)
                {
                    if (txt.GetType() == typeof(string))
                        _db.Put(key, (string)txt);
                    else
                      _db.Put(key, CommonUtils.SerializeObject(txt));
                }
            }
        }

        /// <summary>
        /// 读取记录
        /// </summary>
        /// <param name="dbkey">BondDb,LastExecution,MarketExecution,CacheMarketExecutions</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string dbkey, string key)
        {
            RocksDb _db = GetOrCreateDb(dbkey);
            if (_db == null)
                return string.Empty;
            string res = string.Empty;
            try
            {
                res = _db?.Get(key) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }

            return res;
        }


        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="dbkey"></param>
        /// <param name="key"></param>
        public void Delete(string dbkey, string key)
        {
            try
            {
                RocksDb _db = GetOrCreateDb(dbkey);
                _db.Remove(key);
            }
            catch
            {
            }
        }


        /// <summary>
        /// 获取估算键的数量
        /// </summary>
        /// <param name="dbkey"></param>
        /// <returns></returns>
        public int GetRecordCount(string dbkey)
        {
            int count = 0;
            RocksDb _db = GetOrCreateDb(dbkey);

            string numKeysStr = _db.GetProperty("rocksdb.estimate-num-keys");

            // 将字符串转换为数值
            if (int.TryParse(numKeysStr, out count))
            {
            }

            return count;
        }

        #region 原始市场成交数据
        /// <summary>
        /// 读取原始市场成交记录
        /// </summary>
        /// <param name="dbkey"></param>
        /// <returns></returns>
        public List<ExecutionReport> GetAllCache(string dbkey, int num = -1)
        {
            RocksDb _db = GetOrCreateDb(dbkey);
            if (_db == null)
                return null;

            List<ExecutionReport> values = new List<ExecutionReport>();
            using (Iterator it = _db.NewIterator())
            {
                it.SeekToFirst();
                while (it.Valid() && it.StringValue().StartsWith("{"))
                {
                    values.Add(Common.Utils.CommonUtils.DeserializeObject<ExecutionReport>(it.StringValue()));

                    if (num > 0)
                    {
                        if (values.Count == num)
                            break;
                    }

                    it.Next();
                }
            }

            return values;
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
            RocksDb _db = GetOrCreateDb(dbkey);
            if (_db == null)
            {
                return new List<ExecutionReport>();
            }


            List<ExecutionReport> values = new List<ExecutionReport>();
            try
            {
                prefix = $"{prefix}#";
                ReadOptions readOptions = new ReadOptions();
                readOptions.SetPrefixSameAsStart(true);
                Iterator it = _db.NewIterator(null, readOptions);
                it.Seek(prefix);

                if (!orderasc)
                    it.SeekToLast();

                while (it.Valid())
                {
                    string key = it.StringKey();
                    if (key.StartsWith(prefix) && it.StringValue().StartsWith("{"))
                    {
                        values.Add(Common.Utils.CommonUtils.DeserializeObject<ExecutionReport>(it.StringValue()));
                    }

                    if (!orderasc)
                        it.Prev();
                    else
                        it.Next();

                    if (num > 0)
                    {
                        if (values.Count == num)
                            break;
                    }
                }

                it.Dispose();
            }
            catch { }

            return values;
        }

        #endregion

        #region 已处理的市场成交
        /// <summary>
        /// 读取所有已处理的市场成交记录
        /// </summary>
        /// <param name="dbkey"></param>
        /// <returns></returns>
        public List<MarketExecutionsItem> GetAll(string dbkey, int num = -1)
        {
            RocksDb _db = GetOrCreateDb(dbkey);
            List<MarketExecutionsItem> values = new List<MarketExecutionsItem>();
            using (Iterator it = _db.NewIterator())
            {
                it.SeekToFirst();
                while (it.Valid() && it.StringValue().StartsWith("{"))
                {
                    values.Add(Common.Utils.CommonUtils.DeserializeObject<MarketExecutionsItem>(it.StringValue()));

                    if (num > 0)
                    {
                        if (values.Count == num)
                            break;
                    }

                    it.Next();
                }
            }

            return values;
        }

        /// <summary>
        /// 根据key返回已处理好的市场成交
        /// </summary>
        /// <param name="dbkey"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public List<MarketExecutionsItem> GetAllByKey(string dbkey, string prefix)
        {
            RocksDb _db = GetOrCreateDb(dbkey);
            List<MarketExecutionsItem> values = new List<MarketExecutionsItem>();
            prefix = $"{prefix}#";
            ReadOptions readOptions = new ReadOptions();
            readOptions.SetPrefixSameAsStart(true);
            Iterator it = _db.NewIterator(null, readOptions);
            it.Seek(prefix);
            while (it.Valid())
            {
                string key = it.StringKey();
                if (key.StartsWith(prefix) && it.StringValue().StartsWith("{"))
                {
                    values.Add(Common.Utils.CommonUtils.DeserializeObject<MarketExecutionsItem>(it.StringValue()));
                }

                it.Next();
            }

            it.Dispose();

            return values;
        }

        public List<MarketExecutionsItem> GetPrevNumByKey(string dbkey, string prefix = null, int num = 200)
        {
            RocksDb _db = GetOrCreateDb(dbkey);
            List<MarketExecutionsItem> values = new List<MarketExecutionsItem>();
            prefix = $"{prefix}#";
            try
            {
                ReadOptions readOptions = new ReadOptions();
                readOptions.SetPrefixSameAsStart(true);
                Iterator it = _db.NewIterator(null, readOptions);

                if (prefix == null)
                {
                    it.SeekToFirst();
                }
                else
                {
                    it.Seek(prefix);
                }

                bool isFound = false;
                if (it.Valid())
                {
                    isFound = (it.StringKey().StartsWith(prefix) && it.StringValue().StartsWith("{"));
                }

                while (isFound && it.Valid() && it.StringValue().StartsWith("{"))
                {
                    values.Add(Common.Utils.CommonUtils.DeserializeObject<MarketExecutionsItem>(it.StringValue()));
                    it.Next();
                    if (values.Count == num)
                        break;
                }

                it.Dispose();
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }

            return values;
        }

        #endregion

        #region 关闭删除数据
        /// <summary>
        /// 关闭数据库
        /// </summary>
        /// <param name="dbkey"></param>
        private void CloseDB(string dbkey)
        {
            if (htDB.ContainsKey(dbkey))
                GetOrCreateDb(dbkey)?.Dispose();

            if (htDB.ContainsKey(dbkey))
            {
                RocksDb rocksDb_remove;
                htDB.TryRemove(dbkey, out rocksDb_remove);
            }
        }

        /// <summary>
        /// 关闭所有数据库
        /// </summary>
        private void CloseAllDB()
        {
            try
            {
                if (htDB != null)
                {
                    foreach (string dbkey in htDB.Keys)
                    {
                        CloseDB(dbkey);
                    }

                    htDB.Clear();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <param name="dbkey"></param>
        private void DeleteDb(string dbkey)
        {
            try
            {
                CloseDB(dbkey);
                STC.Common.Utils.FileHelper.DeleteDirectory(System.IO.Path.Combine(DataPath, dbkey));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public void Dispose()
        {
            CloseAllDB();
            //DeleteDb("STCDbHandler.MarketExecution");
            //DeleteDb("STCDbHandler.CacheMarketExecutions");
            //DeleteDb("STCDbHandler.LastExecution");
        }
        #endregion
    }
}
