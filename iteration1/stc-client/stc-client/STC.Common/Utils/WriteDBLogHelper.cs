using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class WriteDBLogHelper
    {

        private static WriteDBLogHelper _instance = new WriteDBLogHelper();

        public static WriteDBLogHelper Instance
        {
            get
            {
                return _instance;
            }
        }

        //private List<DBLogRequest> _DBLogRequestList = new List<DBLogRequest>();

        //public List<DBLogRequest> DBLogRequestList
        //{
        //    get
        //    {
        //        return _DBLogRequestList;
        //    }
        //}

        private ConcurrentDictionary<string, List<object>> _logDic = new ConcurrentDictionary<string, List<object>>();

        private Thread writeDBLogThread;

        private WriteDBLogHelper()
        {
            
        }

        public void Init()
        {
            if (writeDBLogThread == null && LocalEnv.ConfInfo.IsWriteDBLog)
            {
                writeDBLogThread = new Thread(DoWork);
                writeDBLogThread.IsBackground = true;
                writeDBLogThread.Start();
            }
        }

        public void ExitApp()
        {
            if (LocalEnv.ConfInfo.IsWriteDBLog)
            {
                if (writeDBLogThread != null)
                {
                    writeDBLogThread.Abort();
                }
                if (_logDic.Count > 0)
                {
                    SendDB(_logDic);
                }
            }
        }

        public void GenRecord(string moduleName, string compId, string compName)
        {
            if (!LocalEnv.ConfInfo.IsWriteDBLog)
            {
                return;
            }

            string module = string.Empty;
            if (moduleName == "personal")
            {
                module = "需求工作台";
            }
            else if (moduleName == "inner")
            {
                module = "内部看板";
            }
            else if (moduleName == "position")
            {
                module = "持仓管理";
            }
            var content = new { time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), controlid = $"{moduleName}-{compId}", name = $"{module}-{compId}-{compName}" };

            if (!_logDic.ContainsKey(moduleName))
            {
                _logDic[moduleName] = new List<object>();
            }

            //_logDic[moduleName].Add(JsonConvert.SerializeObject(content));
            _logDic[moduleName].Add(content);
        }


        //public DBLogRequest GenRecord(string moduleName, string compId, string compName)
        //{
        //    if (!LocalEnv.ConfInfo.IsWriteDBLog)
        //    {
        //        return null;
        //    }
        //    DBLogRequest req = new DBLogRequest();
        //    req.closeby = LocalEnv.UserInfo.UserName;

        //    string module = string.Empty;
        //    if (moduleName == "personal")
        //    {
        //        module = "需求工作台";
        //    }
        //    else if (moduleName == "inner")
        //    {
        //        module = "内部看板";
        //    }

        //    var content = new { time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), controlid = $"{moduleName}-{compId}", name = $"{module}-{compId}-{compName}" };
        //    req.content = JsonConvert.SerializeObject(content);
        //    req.id = string.Empty;
        //    req.ip = EngineHelper.GetIp();
        //    req.mac = EngineHelper.GetMacByNetworkInterface();
        //    req.status = "Click";
        //    req.type = "TableSvr";
        //    return req;
        //}

        //public void AddRecord(DBLogRequest req)
        //{
        //    if (req != null && LocalEnv.ConfInfo.IsWriteDBLog)
        //    {
        //        lock (DBLogRequestList)
        //        {
        //            DBLogRequestList.Add(req);
        //        }
        //    }
        //}
        
        private void DoWork()
        {
            while(true)
            {
                SendDB(_logDic);

                Thread.Sleep(1000 * 60 * 5);
            }
        }

        private void SendDB(ConcurrentDictionary<string, List<object>> dic)
        {
            if (dic != null && dic.Count > 0)
            {
                foreach (string key in dic.Keys)
                {
                    if (dic[key] == null && dic[key].Count == 0)
                    {
                        List<object> removeContentList;
                        dic.TryRemove(key, out removeContentList);
                        continue;
                    }

                    DBLogRequest req = new DBLogRequest();
                    req.closeby = LocalEnv.UserInfo.UserName;

                    
                    req.content = JsonConvert.SerializeObject(dic[key]);
                    req.rolename = LocalEnv.UserInfo.RoleName;
                    req.id = string.Empty;
                    req.ip = EngineHelper.GetIp();
                    req.mac = EngineHelper.GetMacByNetworkInterface();
                    req.status = "Click";
                    req.type = key;

                    PResult result = AdminHandler.Instance.WriteDBLog(req);
                    if (result != null && result.IsSuccess())
                    {
                        List<object> removeContentList;
                        dic.TryRemove(key, out removeContentList);
                    }
                }
            }
        }
    }
}
