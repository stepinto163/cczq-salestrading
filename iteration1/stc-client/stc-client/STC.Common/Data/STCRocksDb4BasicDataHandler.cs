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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Data
{
    public class STCRocksDb4BasicDataHandler
    {
        private static STCRocksDb4BasicDataHandler _Handler = new STCRocksDb4BasicDataHandler();
        public static STCRocksDb4BasicDataHandler Instance { get { return _Handler; } }

        public string path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data");

        private static ConcurrentDictionary<string, RocksDb> htDB = new ConcurrentDictionary<string, RocksDb>();

        private string DirectoryName = "BasicData_1.0.0";

        /// <summary>
        /// 基础数据(全)
        /// </summary>
        public static string DbBondInfo = "BondInfo";

        /// <summary>
        /// 基础数据(简)
        /// </summary>
        public static string DbSimpleBondInfo = "SimpleBondInfo";

        /// <summary>
        /// 基础数据（最大时间）
        /// </summary>
        public static string DbBaseMaxTime = "BaseMaxTime";

        public STCRocksDb4BasicDataHandler()
        {
            if (FileHelper.FileIsExists(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\STCTemp.db")))
            {
                LogHandler.Info($"删除RockBase");
                STC.Common.Utils.FileHelper.DeleteDirectory(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, "data\\STCTemp.db"));

                if (Directory.Exists(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, $"data\\{DirectoryName}.{LocalEnv.UserInfo.UserName}")))
                {
                    STC.Common.Utils.FileHelper.DeleteDirectory(System.IO.Path.Combine(LocalEnv.ConfInfo.DataPath, $"data\\{DirectoryName}.{LocalEnv.UserInfo.UserName}"));
                }
            }

            BaseHandler.Instance.Init(Instance_OnClear);
            CheckDirectionary();
        }

        /// <summary>
        /// 重启消息处理程序
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data");
                CheckDirectionary();
            }
        }

        private void CheckDirectionary()
        {
            try
            {
                //string datepath = $"20241010.{LocalEnv.UserInfo.UserName}";
                path = System.IO.Path.Combine(path, $"{DirectoryName}.{LocalEnv.UserInfo.UserName}");

                //如果是新的一天，则关闭数据库
                if (!System.IO.Directory.Exists(path))
                {
                    CloseAllDB();

                    System.IO.Directory.CreateDirectory(path);
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
                //基础数据
                GetOrCreateDb(DbBondInfo);
                GetOrCreateDb(DbSimpleBondInfo);
                GetOrCreateDb(DbBaseMaxTime);
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
        public RocksDb GetOrCreateDb(string dbkey)
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

                    string tmppath = System.IO.Path.Combine(path, dbkey);

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
        public void Put(string dbkey, string key, string txt)
        {
            if (!string.IsNullOrEmpty(key))
            {
                RocksDb _db = GetOrCreateDb(dbkey);
                if (_db != null)
                    _db.Put(key, txt);
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

        #region 获取所有基础数据
        public List<SimpleBondsInfoPo> GetAllSimpleBondsInfo(int num = -1)
        {
            RocksDb _db = GetOrCreateDb(DbSimpleBondInfo);
            if (_db == null)
                return null;

            List<SimpleBondsInfoPo> values = new List<SimpleBondsInfoPo>();
            using (Iterator it = _db.NewIterator())
            {
                it.SeekToFirst();
                while (it.Valid() && it.StringValue().StartsWith("{"))
                {
                    values.Add(Common.Utils.CommonUtils.DeserializeObject<SimpleBondsInfoPo>(it.StringValue()));

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

        public List<BondsInfoPo> GetAllBondsInfo(int num = -1)
        {
            RocksDb _db = GetOrCreateDb(DbBondInfo);
            if (_db == null)
                return null;

            List<BondsInfoPo> values = new List<BondsInfoPo>();
            using (Iterator it = _db.NewIterator())
            {
                it.SeekToFirst();
                while (it.Valid() && it.StringValue().StartsWith("{"))
                {
                    values.Add(Common.Utils.CommonUtils.DeserializeObject<BondsInfoPo>(it.StringValue()));

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

        #endregion

        #region 关闭删除数据
        /// <summary>
        /// 关闭数据库
        /// </summary>
        /// <param name="dbkey"></param>
        public void CloseDB(string dbkey)
        {
            if (htDB.ContainsKey(dbkey))
            {
                RocksDb rocksDb_remove;
                htDB.TryRemove(dbkey, out rocksDb_remove);
                rocksDb_remove?.Dispose();
            }
        }

        /// <summary>
        /// 关闭所有数据库
        /// </summary>
        public void CloseAllDB()
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
        public void DeleteDb(string dbkey)
        {
            try
            {
                CloseDB(dbkey);
                STC.Common.Utils.FileHelper.DeleteDirectory(System.IO.Path.Combine(path, dbkey));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public void Dispose()
        {
            CloseAllDB();
            //DeleteDb("MarketExecution");
            //DeleteDb("CacheMarketExecutions");
            //DeleteDb("LastExecution");
        }
        #endregion
    }
}

