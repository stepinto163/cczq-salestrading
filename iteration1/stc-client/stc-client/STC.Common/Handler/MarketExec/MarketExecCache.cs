using STC.Common.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Handler.MarketExec
{
    public class MarketExecCache
    {
        private object lockObj = new object();
        private ConcurrentDictionary<string, bool> loadDic = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, CapacityList<ExecutionReport>> sortDic = new ConcurrentDictionary<string, CapacityList<ExecutionReport>>();
        //每个债券保存的数据阀值量
        public int Limit = 120;

        private MarketExecCache() { }
        private static MarketExecCache server = new MarketExecCache();


        public static MarketExecCache Instance { get { return server; } }


        public void AddOrUpdateStatus(string securityId)
        {
            loadDic.AddOrUpdate(securityId, true, (key, value) => true);

        }

        public bool GetLoadStatus(string securityId)
        {
            if (loadDic.ContainsKey(securityId))
            {
                return loadDic[securityId];
            }

            return false;
        }


        public void OnClear()
        {
            sortDic.Clear();

            loadDic.Clear();
        }

        public void AddOrUpdate(ExecutionReport order)
        {
            lock (lockObj)
            {
                if (sortDic.ContainsKey(order.SecurityID))
                {
                    CapacityList<ExecutionReport> sortList = sortDic[order.SecurityID];
                    List<ExecutionReport> selectList = sortList.Where(delegate (ExecutionReport er)
                    {
                        return er.ExecID == order.ExecID;
                    }).ToList();

                    if ((selectList == null || selectList.Count <= 0) && order.OrdStatus != "4")
                    {
                        sortList.Add(order);

                    }
                    else if (selectList != null && selectList.Count > 0)
                    {
                        if (order.OrdStatus == "4")
                        {
                            ExecutionReport er = selectList[0];
                            selectList.RemoveAt(0);

                            sortList.Remove(er);
                        }
                        else
                        {
                            selectList[0] = order;
                        }
                    }
                }
                else
                {
                    if (order.OrdStatus != "4")
                    {
                        bool isFuture = CommonUtils.IsFutureBond(order.SecurityID);
                        CapacityList<ExecutionReport> newCapacityList = new CapacityList<ExecutionReport>(isFuture ? Limit : 100000, new ExecComparer());
                        newCapacityList.Add(order);

                        sortDic.AddOrUpdate(order.SecurityID, newCapacityList, (key, value) => newCapacityList);
                    }
                }
            }
        }

        public List<ExecutionReport> GetOwnExec()
        {
            List<ExecutionReport> result = new List<ExecutionReport>();

            lock (lockObj)
            {
                foreach (CapacityList<ExecutionReport> execOrderList in sortDic.Values)
                {
                    foreach (ExecutionReport er in execOrderList)
                    {
                        if (!er.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) && !er.VenueTypeGW.ToLower().Equals(ApiConsts.SDN.ToLower()))
                        {
                            result.Add(er);
                        }
                    }
                }
            }
            return result;
        }

        public List<ExecutionReport> GetMarketExec()
        {
            List<ExecutionReport> result = new List<ExecutionReport>();

            lock (lockObj)
            {
                foreach (CapacityList<ExecutionReport> execOrderList in sortDic.Values)
                {
                    foreach (ExecutionReport er in execOrderList)
                    {
                        if (er.VenueTypeGW.ToLower().Equals(ApiConsts.CDH.ToLower()) || er.VenueTypeGW.ToLower().Equals(ApiConsts.SDN.ToLower()))
                        {
                            result.Add(er);
                        }
                    }
                }
            }
            return result;
        }

        public List<ExecutionReport> GetMarketExecByCondition(string SecurityID, string venue)
        {
            List<ExecutionReport> result = new List<ExecutionReport>();
            lock (lockObj)
            {
                if (string.IsNullOrEmpty(venue) && !string.IsNullOrEmpty(SecurityID))
                {
                    if (sortDic.ContainsKey(SecurityID))
                    {
                        result = sortDic[SecurityID].ToList();
                    }
                }
                else if (string.IsNullOrEmpty(venue) && string.IsNullOrEmpty(SecurityID))
                {
                    foreach (CapacityList<ExecutionReport> execOrderList in sortDic.Values)
                    {
                        result.AddRange(execOrderList);
                    }
                }
                else if (!string.IsNullOrEmpty(venue) && string.IsNullOrEmpty(SecurityID))
                {
                    foreach (CapacityList<ExecutionReport> execOrderList in sortDic.Values)
                    {
                        foreach (ExecutionReport er in execOrderList)
                        {
                            if (er.VenueTypeGW.ToLower().Equals(venue.ToLower()))
                            {
                                result.Add(er);
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(venue) && !string.IsNullOrEmpty(SecurityID))
                {
                    foreach (CapacityList<ExecutionReport> execOrderList in sortDic.Values)
                    {
                        foreach (ExecutionReport er in execOrderList)
                        {
                            if (er.VenueTypeGW.ToLower().Equals(venue.ToLower()) && er.SecurityID.Equals(SecurityID))
                            {
                                result.Add(er);
                            }
                        }
                    }
                }

            }
            return result;
        }


        public List<ExecutionReport> GetmarketExecBySecurityID(string SecurityID)
        {
            lock (lockObj)
            {
                if (sortDic.ContainsKey(SecurityID))
                {
                    return sortDic[SecurityID].ToList();
                }
            }

            return null;
        }

        public ExecutionReport GetmarketExecByExecID(string execId)
        {
            lock (lockObj)
            {
                foreach (CapacityList<ExecutionReport> execOrderList in sortDic.Values)
                {
                    foreach (ExecutionReport er in execOrderList)
                    {
                        if (er.ExecID == execId)
                        {
                            return er;
                        }
                    }
                }
            }
            return null;
        }

    }

}
