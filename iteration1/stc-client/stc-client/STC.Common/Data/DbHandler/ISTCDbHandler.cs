using RocksDbSharp;
using STC.Common.Model.Fix.Message;
using STC.Common.ViewModel.Data;
using System.Collections.Generic;

namespace STC.Common.Data
{
    public interface ISTCDbHandler
    {
        void Start();
        void Dispose();
        bool IsNewDay();
        string Get(string dbkey, string key);
        void Put(string dbkey, string key, object txt);
        void Delete(string dbkey, string key);
        //List<MarketExecutionsItem> GetAll(string dbkey, int num = -1);
        //List<MarketExecutionsItem> GetAllByKey(string dbkey, string prefix);
        List<ExecutionReport> GetAllCache(string dbkey, int num = -1);
        List<ExecutionReport> GetAllCacheByKey(string dbkey, string prefix, bool orderasc = true, int num = -1);
        //List<MarketExecutionsItem> GetPrevNumByKey(string dbkey, string prefix = null, int num = 200);
        int GetRecordCount(string dbkey);
    }
}