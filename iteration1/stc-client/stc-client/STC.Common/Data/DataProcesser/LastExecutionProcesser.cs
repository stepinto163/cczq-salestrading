using com.gateway.log;
using Newtonsoft.Json;
using Stc.Common.Model.EMS;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Handler.OHLC;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Chart;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.OHLC;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Common.DataProcesser
{
    /// <summary>
    /// 本地成交
    /// </summary>
    public class LastExecutionProcesser
    {
        
        private readonly Object m_OnLastExecutionEventLock = new Object();
        /// <summary>
        /// 订阅最新本地成交含市场成交
        /// </summary>
        private Action<ExecutionReport> _OnLastExecutions;
        /// <summary>
        /// 订阅最新本地成交
        /// </summary>
        public event Action<ExecutionReport> OnLastExecutions
        {
            add
            {
                lock (m_OnLastExecutionEventLock)
                {
                    _OnLastExecutions += value;
                }
            }
            remove
            {
                lock (m_OnLastExecutionEventLock)
                {
                    _OnLastExecutions -= value;
                }
            }
        }

        private static LastExecutionProcesser proc = new LastExecutionProcesser();

        public static LastExecutionProcesser Processer
        {
            get
            {
                return proc;
            }
        }

        private volatile bool isInit = false;


        /// <summary>
        /// 构造函数
        /// </summary>
        private LastExecutionProcesser()
        {
            ExecutionsHandler.Instance.OnClear += Instance_OnClear;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~LastExecutionProcesser()
        {
            Dispose();
        }


        /// <summary>
        /// 资源锁
        /// </summary>
        public object lockObj = new object();

        /// <summary>
        /// 获得本地成交 securityId （含市场成交内部成交）判断小红点的算法 todo...
        /// </summary>
        /// <param name="securityID">债券代码</param>
        /// <returns>市场成交数据</returns>
        ////public ObservableCollection<MarketExecutionsItem> GetDatasBySecurityID(string securityID)
        ////{
        ////    if (!string.IsNullOrEmpty(securityID))
        ////    {
        ////        List<MarketExecutionsItem> temp = STC.Common.Data.STCDbHandler.Instance.GetAllByKey(STC.Common.Data.STCDbHandler.DbMarketExecution, securityID);
        ////        List<MarketExecutionsItem> owntemp = OwnExecutions.Values.Where(p=>p.SecurityID == securityID).ToList();
        ////        if (temp == null)
        ////            temp = new List<MarketExecutionsItem>();
        ////        if (owntemp == null)
        ////            owntemp = new List<MarketExecutionsItem>();

        ////        List<MarketExecutionsItem> newunion = temp.Union(owntemp).ToList();
        ////        if (newunion.Count > 0)
        ////        {
        ////            return new ObservableCollection<MarketExecutionsItem>(newunion.OrderByDescending(p =>p.TimeStamp).ToList());
        ////        }
        ////    }
        ////    else
        ////    {
        ////        return new ObservableCollection<MarketExecutionsItem>();
        ////    }

        ////    return new ObservableCollection<MarketExecutionsItem>();
        ////}

        /// <summary>
        /// 内部成交
        /// </summary>
        private Dictionary<string, MarketExecutionsItem> OwnExecutions = new Dictionary<string, MarketExecutionsItem>();

        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        //待处理的内部成交
        private static BlockingCollection<Common.Model.Fix.Message.ExecutionReport> OwnExecutionsQueue = new BlockingCollection<Common.Model.Fix.Message.ExecutionReport>();

        /// <summary>
        /// 仅仅处理本地成交，不再处理市场成交
        /// </summary>
        public void Init()
        {
            if (isInit == false)
            {
                //市场成交
                //ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                
                //内部成交
                ExecutionsHandler.Instance.OnOwnExecutions += Instance_OnOwnExecutions;

                //待处理的内部成交
                Task.Factory.StartNew(() => DoProcOwnExecutions(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

                isInit = true;

                //处理最后成交数据
                Task.Factory.StartNew(() => DoProcGetLastExecutionQueue(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
            }
        }

        #region 订阅最后成交
        public ConcurrentDictionary<string, List<Action<ExecutionReport>>> DictLastExecutionCallBack = new ConcurrentDictionary<string, List<Action<ExecutionReport>>>();

        /// <summary>
        /// 订阅债券的最新成交
        /// </summary>
        /// <param name="securityId"></param>
        /// <param name="callBack"></param>
        public void SubscribeLastExecution(string securityId, Action<ExecutionReport> callBack)
        {
            // 获取回调函数的名称及所在类的全名，组成唯一标识
            string callbackIdentifier = $"{callBack.Method.DeclaringType.FullName}.{callBack.Method.Name}";

            // 如果 DictLastExecutionCallBack 中已经包含该 securityId
            if (DictLastExecutionCallBack.ContainsKey(securityId))
            {
                // 锁住该 securityId 对应的回调列表，以防止并发问题
                lock (DictLastExecutionCallBack[securityId])
                {
                    // 检查列表中是否已经包含相同的回调函数标识
                    var existingCallbacks = DictLastExecutionCallBack[securityId];
                    bool isCallbackExist = existingCallbacks.Any(cb => $"{cb.Method.DeclaringType.FullName}.{cb.Method.Name}" == callbackIdentifier);

                    // 如果不存在该回调，则添加
                    if (!isCallbackExist)
                    {
                        existingCallbacks.Add(callBack);
                    }
                }
            }
            else
            {
                // 如果不存在该 securityId，则创建一个新的 List 并添加回调
                var newCallbackList = new List<Action<ExecutionReport>> { callBack };

                // 将该 securityId 以及新的回调列表添加到字典中
                DictLastExecutionCallBack.TryAdd(securityId, newCallbackList);
            }

            ExecutionReport er = Common.DataProcesser.LastExecutionProcesser.Processer.GetExecutionReportDic(securityId);
            if (er != null)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var action in DictLastExecutionCallBack[securityId])
                    {
                        action?.Invoke(er);
                    }
                });
            }
            else
            {
                AddGetLastExecutionQueue(securityId);
            }

        }

        public void UnSubscribeLastExecution(string securityId, Action<ExecutionReport> callBack)
        {
            // 获取回调函数的名称及所在类的全名，组成唯一标识
            string callbackIdentifier = $"{callBack.Method.DeclaringType.FullName}.{callBack.Method.Name}";

            // 检查 DictLastExecutionCallBack 中是否存在该 securityId
            if (DictLastExecutionCallBack.ContainsKey(securityId))
            {
                // 锁住 securityId 对应的回调列表，以防止并发问题
                lock (DictLastExecutionCallBack[securityId])
                {
                    var existingCallbacks = DictLastExecutionCallBack[securityId];

                    // 找到并移除匹配的回调函数
                    existingCallbacks.RemoveAll(cb => $"{cb.Method.DeclaringType.FullName}.{cb.Method.Name}" == callbackIdentifier);

                    // 如果列表为空，移除该 securityId 的条目
                    if (existingCallbacks.Count == 0)
                    {
                        List<Action<ExecutionReport>> tmp = null;
                        DictLastExecutionCallBack.TryRemove(securityId, out tmp);
                    }
                }
            }
        }

        #endregion




        #region 获取或重新计算或等待计算最后成交的处理程序

        public ConcurrentDictionary<string, ExecutionReport> ProcedGetLastExecutionDic = new ConcurrentDictionary<string, ExecutionReport>();

        //待获取和计算的最新成交
        private static BlockingCollection<string> GetLastExecutionQueue = new BlockingCollection<string>();

        /// <summary>
        /// 待获取和计算的最新成交
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void DoProcGetLastExecutionQueue(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string securityid = GetLastExecutionQueue.Take(cancellationToken);

                    try
                    {
                        if (securityid != null)
                        {
                            if (!ProcedGetLastExecutionDic.ContainsKey(securityid))
                            {
                                List<ExecutionReport> lstlastOwnOrder = ExecutionsHandler.Instance.GetLastExecutionBySecurityID(securityid);
                                if (lstlastOwnOrder != null & lstlastOwnOrder.Count > 0)
                                {
                                    ProcedGetLastExecutionDic[securityid] = lstlastOwnOrder[0];
                                    Common.DataProcesser.LastExecutionProcesser.Processer.UpdateExecutionReportDic(securityid, lstlastOwnOrder[0]);
                                    Common.DataProcesser.LastExecutionProcesser.Processer.TriggerLastExectuions(lstlastOwnOrder[0]);
                                }
                            }
                            else
                            {
                                ExecutionReport prevMei = Common.DataProcesser.LastExecutionProcesser.Processer.GetExecutionReportDic(securityid);
                                Common.DataProcesser.LastExecutionProcesser.Processer.TriggerLastExectuions(prevMei);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }


        /// <summary>
        /// 添加待获取最后成交的队列
        /// </summary>
        /// <param name="sec"></param>
        public void AddGetLastExecutionQueue(string securityid)
        {
            GetLastExecutionQueue.Add(securityid);
        }

        #endregion


        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            ExecutionsHandler.Instance.OnOwnExecutions -= Instance_OnOwnExecutions;
            //ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;
            //TBookHandler.Instance.UnSubscribeBond(Instance_OnBondList);

            isInit = false;
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            this.Clear();

            isInit = false;
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void ResetClear()
        {
            LogHandler.Info("LastExecutionProcesser ResetClear.");
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Clear();
                this.Init();
            }));
        }

        /// <summary>
        /// 清零回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("LastExecutionProcesser Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        this.Clear();
                        this.Init();
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("LastExecutionProcesser Clear exception:" + e.Message);
                    }
                }));
            }
        }

        /// <summary>
        /// 收取本地内部成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnOwnExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            if (obj != null)
                OwnExecutionsQueue.Add(obj);
        }

        
        #region 更新处理内部成交
        /// <summary>
        /// 处理内部成交
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void DoProcOwnExecutions(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Common.Model.Fix.Message.ExecutionReport obj = OwnExecutionsQueue.Take(cancellationToken);

                    if (obj != null)
                    {
                        UpdateOwnExecutions(obj);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception on cancellation
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        /// <summary>
        /// 更新内部成交
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateOwnExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            try
            {
                lock (lockObj)
                {
                    if (obj == null || string.IsNullOrEmpty(obj.SecurityID))
                    {
                        LogHelper.Info("UpdateOwnExecutions obj == null || string.IsNullOrEmpty(obj.SecurityID)");
                        return;
                    }

                    if ("0".Equals(obj.TimeInForce) && "INNER".Equals(obj.VenueTypeGW))
                    {
                        return;
                    }

                    if (obj.OrdStatus == "4")
                        //更新最新成交字典，获取上一个有效的成交作为最后成交
                        Common.DataProcesser.LastExecutionProcesser.Processer.DeleteLastERAndGetPrevER(obj);
                    else
                        //更新最新成交字典
                        Common.DataProcesser.LastExecutionProcesser.Processer.UpdateLastExecutionProcesserLastExecution(obj);

                    MarketExecutionsItem result = null;
                    if (OwnExecutions.ContainsKey(obj.ExecID))
                    {
                        result = OwnExecutions[obj.ExecID];
                    }

                    if (result == null)
                    {
                        // 如果状态是4， 删除这笔单子。
                        if (obj.OrdStatus == "4")
                        {
                            return;
                        }

                        // 添加新数据
                        AddRecord(obj);
                    }
                    else
                    {
                        DateTime orgupdateTime = DateTimeEx.ConvertStringToDateTime(result.TimeStamp);
                        DateTime newupdateTime = DateTimeEx.ConvertStringToDateTime(obj.UpdateTime);

                        if (orgupdateTime >= newupdateTime)
                        {
                            return;
                        }

                        if (obj.OrdStatus == "4")
                        {
                            // 删除
                            DeleteRecord(obj);
                            return;
                        }

                        // 更新数据
                        UpdateRecord(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }

        /// <summary>
        /// 添加新记录
        /// </summary>
        /// <param name="obj"></param>
        private void AddRecord(Common.Model.Fix.Message.ExecutionReport obj)
        {
            MarketExecutionsItem marketExe = new MarketExecutionsItem();
            ConvertExecutionReportToMarketExecutionsItem(obj, ref marketExe);
            marketExe.Key = marketExe.ExecID;
            // 如果时间新
            OwnExecutions.Add(marketExe.ExecID , marketExe);
        }

        /// <summary>
        /// 删除execID的记录
        /// </summary>
        /// <param name="execID"></param>
        private void DeleteRecord(Common.Model.Fix.Message.ExecutionReport obj)
        {
            string execID = obj.ExecID;
            OwnExecutions.Remove(execID);
        }

        /// <summary>
        /// 删除execID的记录
        /// </summary>
        /// <param name="execID"></param>
        private void UpdateRecord(Common.Model.Fix.Message.ExecutionReport obj)
        {
            MarketExecutionsItem marketExe = new MarketExecutionsItem();
            ConvertExecutionReportToMarketExecutionsItem(obj, ref marketExe);
            marketExe.Key = marketExe.ExecID;
            OwnExecutions[obj.ExecID] = marketExe;
        }


        public void ConvertExecutionReportToMarketExecutionsItem(ExecutionReport obj, ref MarketExecutionsItem marketExe)
        {
            try
            {
                marketExe.ExecID = obj.ExecID;
                marketExe.Key = obj.ExecID;
                marketExe.OrderID = obj.OrderID;
                marketExe.Side = ConvertSide(obj.Side); // 方向
                marketExe.SecurityID = obj.SecurityID; // 代码
                marketExe.MarketIndicator = obj.MarketIndicator;
                if (obj.SecurityID.EndsWith(".SH") || obj.SecurityID.EndsWith(".SZ"))
                {
                    marketExe.ExecutePricePercent_Display = CommonUtils.FormatPriceN3(obj.NetPrice); //用于显示成交价
                    marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.NetPrice); // 成交价
                }
                else
                {
                    marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.Yield); //用于显示成交价
                    marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.Yield); // 成交价
                    marketExe.SettlType = obj.SettlType;
                }

                marketExe.SourceTrader = obj.Venues; // 来源
                marketExe.Time = obj.TransactTime;// 时间
                marketExe.SettlType = obj.SettlType; // 清算速度
                if ((obj.SettlType ?? "").Contains('.'))
                {
                    marketExe.SettlType_yuan = obj.SettlType;
                }
                else
                {
                    marketExe.SettlType_yuan = "";
                }

                marketExe.IsValid = obj.Vs == null || obj.Vs == "Y" || obj.Vs == "" ? "Y" : "N";
                marketExe.IsOwnInst = ("INNER".Equals(obj.VenueTypeGW) || "CDH".Equals(obj.VenueTypeGW) || "CMDS".Equals(obj.VenueTypeGW)
                || "SDN".Equals(obj.VenueTypeGW) || "SSE".Equals(obj.VenueTypeGW)
                || "SSF".Equals(obj.VenueTypeGW) || "SZE".Equals(obj.VenueTypeGW)) ? "N" : "Y";
                if ("Y".Equals(marketExe.IsOwnInst))
                {
                    marketExe.OwnInstTip = "本机构";
                }

                marketExe.Info1 = obj.Info1;
                marketExe.TimeStamp = obj.UpdateTime;
                marketExe.Info2 = obj.Info2;
                marketExe.Info10 = obj.Info10;

                marketExe.VenueTypeGW = obj.VenueTypeGW;


                if (obj.SecurityID.EndsWith(".SH") || obj.SecurityID.EndsWith(".SZ") || "5".Equals( obj.MarketIndicator?? "4"))
                    marketExe.NetPrice_Display = CommonUtils.FormatNum3(obj.NetPrice);
                else
                    marketExe.NetPrice_Display = CommonUtils.FormatPrice(obj.NetPrice);

                marketExe.NetPrice = CommonUtils.ConvertToDecimal(obj.NetPrice);
                marketExe.LastQty = string.IsNullOrEmpty(obj.LastQty) ? "--" : ("0".Equals(obj.LastQty) ? "--" : obj.LastQty);
                marketExe.DownUp = CommonConstants.NO_UP_DOWN;
                marketExe.Bp = CommonConstants.NO_UP_DOWN;
                marketExe.ShowStar = HomePageDataHandler.IsFavorite(obj.SecurityID) == "1" ? Visibility.Visible : Visibility.Hidden;//星星状态
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex);
            }
        }
        #endregion


        #region 更新原始最后成交

        /// <summary>
        /// 更新最新成交
        /// </summary>
        /// <param name="securityId"></param>
        /// <param name="marketExe"></param>
        public void UpdateExecutionReportDic(string securityId, ExecutionReport obj)
        {
            if (obj != null)
            {
                STC.Common.Data.STCDbHandler.Instance.Put(STC.Common.Data.STCDbHandler.DbLastExecution, securityId, obj);
            }
        }

        public void TriggerLastExectuions(ExecutionReport obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.SecurityID ?? ""))
            {
                //高线程优先级去处理数据
                Thread thread = new Thread(() =>
                {
                    _OnLastExecutions?.Invoke(obj);
                });
                thread.Priority = ThreadPriority.Normal;
                thread.Start();
            }
        }

        /// <summary>
        /// 获取最新成交,从缓存中获取
        /// </summary>
        /// <param name="securityId"></param>
        /// <returns></returns>
        public ExecutionReport GetExecutionReportDic(string securityId)
        {
            if (!string.IsNullOrEmpty(securityId))
            {
                string lastres = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbLastExecution, securityId);

                if (!string.IsNullOrEmpty(lastres))
                {
                    return CommonUtils.DeserializeObject<ExecutionReport>(lastres);
                }
                else
                {
                    //如果没有生成最新成交
                    if (!ProcedGetLastExecutionDic.ContainsKey(securityId))
                    {
                        List<ExecutionReport> lstlastOwnOrder = ExecutionsHandler.Instance.GetLastExecutionBySecurityID(securityId);
                        if (lstlastOwnOrder != null & lstlastOwnOrder.Count > 0)
                        {
                            lock (lockObj)
                            {
                               ProcedGetLastExecutionDic[securityId] = lstlastOwnOrder[0];
                            }

                            Common.DataProcesser.LastExecutionProcesser.Processer.UpdateExecutionReportDic(securityId, lstlastOwnOrder[0]);
                            Common.DataProcesser.LastExecutionProcesser.Processer.TriggerLastExectuions(lstlastOwnOrder[0]);
                            return lstlastOwnOrder[0];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 删除最新成交，获取上一个成交
        /// </summary>
        /// <param name="obj"></param>
        public void DeleteLastERAndGetPrevER(Common.Model.Fix.Message.ExecutionReport obj)
        {
            //首先从字典里删除，然后重新获取上一个有效的成交
            string securityId = obj.SecurityID;

            List<ExecutionReport> lstlastOwnOrder = ExecutionsHandler.Instance.GetLastExecutionBySecurityID(securityId);
            if (lstlastOwnOrder != null & lstlastOwnOrder.Count > 0)
            {
                ProcedGetLastExecutionDic[securityId] = lstlastOwnOrder[0];
                Common.DataProcesser.LastExecutionProcesser.Processer.UpdateExecutionReportDic(securityId, lstlastOwnOrder[0]);
                Common.DataProcesser.LastExecutionProcesser.Processer.TriggerLastExectuions(lstlastOwnOrder[0]);
            }
            else
            {
                //如果是没有记录，应该清空 todo...
                Common.DataProcesser.LastExecutionProcesser.Processer.TriggerLastExectuions(obj);
            }
        }

        /// <summary>
        /// 更新市场成交最新成交，不含内部成交
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="marketExe"></param>
        public void UpdateLastExecutionProcesserLastExecution(Common.Model.Fix.Message.ExecutionReport obj)
        {
            //内部成交
            ExecutionReport LastInnerExecution = null;
            List<ExecutionReport> owntemp = STC.Common.Data.ExecutionsHandler.Instance._CacheOwnExecutions.Values.Where(p => p.SecurityID == obj.SecurityID).OrderByDescending(p => p.TransactTime).ThenByDescending(p => p.UpdateTime).ToList();
            if (owntemp != null && owntemp.Count > 0)
            {
                LastInnerExecution = owntemp[0];
            }

            //如果没有内部成交
            if (LastInnerExecution == null)
            {
                //使用新的方法
                string src = STC.Common.Data.STCDbHandler.Instance.Get(STC.Common.Data.STCDbHandler.DbLastExecution, obj.SecurityID);
                if (!string.IsNullOrEmpty(src))
                {
                    //查找 "TimeStamp" 的位置
                    string[] parts = src.Split(new string[] { "\"TransactTime\":" }, StringSplitOptions.None);

                    if (parts.Length > 1)
                    {
                        string transactionTime = parts[1].Split(',')[0];
                        DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(transactionTime.Replace("\"", ""));
                        DateTime curUpdateTime = DateTimeEx.ConvertStringToDateTime(obj.TransactTime);
                        if (DateTime.Compare(orgdatetime, curUpdateTime) > 0)
                        {
                            TriggerLastExectuions(CommonUtils.DeserializeObject<ExecutionReport>(src));
                            return;
                        }

                        //更新最新成交
                        Common.DataProcesser.LastExecutionProcesser.Processer.UpdateExecutionReportDic(obj.SecurityID, obj);
                        TriggerLastExectuions(obj);
                    }
                }
                else
                {
                    //更新最新成交
                    Common.DataProcesser.LastExecutionProcesser.Processer.UpdateExecutionReportDic(obj.SecurityID, obj);
                    TriggerLastExectuions(obj);
                }
            }
            else
            {
                DateTime orgdatetime = DateTimeEx.ConvertStringToDateTime(LastInnerExecution.TransactTime); 
                DateTime curUpdateTime = DateTimeEx.ConvertStringToDateTime(obj.TransactTime);
                if (DateTime.Compare(orgdatetime, curUpdateTime) > 0)
                {
                    return;
                }
                else
                {
                    //更新最新成交
                    Common.DataProcesser.LastExecutionProcesser.Processer.UpdateExecutionReportDic(obj.SecurityID, obj);
                    TriggerLastExectuions(obj);
                }
            }
        }


        #endregion

        public string ConvertSide(string side)
        {
            if (side == "2")
            {
                return "Y";
            }
            else if (side == "1")
            {
                return "X";
            }

            return side;
        }

        #region 转发BDB成交

        private readonly Object m_OnLastExecutionEventLock_BDB = new Object();
        private Action<ExecutionReport> _OnLastExecutions_BDB;
        public event Action<ExecutionReport> OnLastExecutions_BDB
        {
            add
            {
                lock (m_OnLastExecutionEventLock_BDB)
                {
                    _OnLastExecutions_BDB += value;
                }
            }
            remove
            {
                lock (m_OnLastExecutionEventLock_BDB)
                {
                    _OnLastExecutions_BDB -= value;
                }
            }
        }


        public void TriggerLastExectuions_BDB(ExecutionReport er)
        {
            _OnLastExecutions_BDB?.Invoke(er);
        }

        public event Func<string, MarketExecutionsItem> OnGetLastExecutionsItem;

        public MarketExecutionsItem GetLastExecutionsItem_BDB(string securityID)
        {
            return OnGetLastExecutionsItem?.Invoke(securityID);
        }
        #endregion

    }
}
