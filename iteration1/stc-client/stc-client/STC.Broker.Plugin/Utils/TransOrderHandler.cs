using com.gateway.log;
using Newtonsoft.Json;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Common.Handler.Base;
using STC.Common.Handler.Drp;
using STC.Common.Handler.PT;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.Utils
{
    public class TransOrderHandler
    {
        private static TransOrderHandler instance = null;
        private static readonly object padlock = new object();

        /// <summary>
        /// 订单刷新
        /// </summary>
        public event Action<ExecutionReport> OnUpdateOrder;
        public event Action<TransOrderServerData> OnUpdateOrder2;

        public event Action<TransOrderServerData> OnOtherBridgeOrderUpdateOrder;
        /// <summary>
        /// 桥状态更新时刷新订单
        /// </summary>
        public event Action<TransOrderServerData> UpdateOrderOnBridgeStatusChanged;

        /// <summary>
        /// 凌晨刷新界面
        /// </summary>
        public event Action OnClearOrder;


        //private ConcurrentDictionary<string, ExecutionReport> dic = new ConcurrentDictionary<string, ExecutionReport>();

        private ConcurrentDictionary<string, TransOrderServerData> dicTransOrder = new ConcurrentDictionary<string, TransOrderServerData>();

        private ConcurrentDictionary<string, List<TransOrderServerData>> dicBlocknumOrder = new ConcurrentDictionary<string, List<TransOrderServerData>>();

        private ConcurrentDictionary<string, TransOrderServerData> dicOtherTransOrder = new ConcurrentDictionary<string, TransOrderServerData>();

        //public ConcurrentDictionary<string, ExecutionReport> DicOrder
        //{
        //    get
        //    {
        //        return dic;
        //    }
        //}

        /// <summary>
        /// 债券拉取完成标志
        /// </summary>
        public bool BondInitCompleted = false;

        bool isHasGetData = false;
        SystemPowerHandler _systemPower = null;

        private Thread RefreshGrOrderThread = null;
        private TransOrderHandler()
        {
            _systemPower = SystemPowerHandler.Instance;
            isHasGetData = false;
            //查询数据
            QueryOrderInfo();
            BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);
            //补录债券信息
            FillBondInfo();
            BaseHandler.Instance.Init(OnClear);

            if (RefreshGrOrderThread == null)
            {
                RefreshGrOrderThread = new Thread(RefreshGrOrder);
                RefreshGrOrderThread.IsBackground = true;
                RefreshGrOrderThread.Start();
            }

        }



        private void OnClear(ClearPo clearPo)
        {
            LogHelper.Info("TransOrderHandler Clear Action");
            if (clearPo != null && clearPo.IsClear == "Y")
            {
                isHasGetData = false;
                //查询数据
                QueryOrderInfo();
                //补录债券信息
                FillBondInfo();
                _systemPower.Reflash();
                OnClearOrder?.Invoke();
            }
        }

        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                BondInitCompleted = obj;
            }
        }

        public static TransOrderHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        instance = new TransOrderHandler();
                    }
                }
                return instance;
            }
        }

        private object _queueLock = new object();
        public void UpdateOrder(TransOrderServerData er)
        {
            if (er == null)
            {
                return;
            }
            SupplementBondInfo(er);
            //dic.AddOrUpdate(er.ExecID, er, (key, value) => er);
            //if (!string.IsNullOrEmpty(er.DelFlag) && er.DelFlag == "1")
            //{
            //    ExecutionReport tem = null;
            //    dic.TryRemove(er.ExecID, out tem);
            //}
            lock (_queueLock)
            {
                dicTransOrder.AddOrUpdate(er.ExecID, er, (key, value) => er);
                if (!string.IsNullOrEmpty(er.DelFlag) && er.DelFlag == "1")
                {
                    TransOrderServerData tem = null;
                    dicTransOrder.TryRemove(er.ExecID, out tem);
                }
                TransOrderServerData trans = new TransOrderServerData();
                trans.CopyParentData(er);
                if (IsCanSeeOrder(er))
                {
                    //dic.AddOrUpdate(er.ExecID, er, (key, value) => er);
                    dicTransOrder.AddOrUpdate(trans.ExecID, trans, (key, value) => trans);
                }

                if (!string.IsNullOrWhiteSpace(trans.BlockNum))
                {
                    List<TransOrderServerData> transOrder = null;
                    if (dicBlocknumOrder.Count > 0)
                    {
                        if (dicBlocknumOrder.TryGetValue(trans.BlockNum, out transOrder))
                        {
                            if (transOrder != null && transOrder.Count > 0)
                            {
                                if (transOrder.Any(_ => _.ExecID == trans.ExecID && !string.IsNullOrEmpty(trans.DelFlag) && trans.DelFlag == "1"))
                                {
                                    transOrder.Remove(transOrder.Where(_ => _.ExecID == trans.ExecID).FirstOrDefault());
                                }
                            }
                            if (transOrder != null && transOrder.Count > 0)
                            {
                                dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                {
                                    UpdateOrderCache(trans, value);
                                    return value;
                                });
                            }
                            else
                            {
                                dicBlocknumOrder.TryRemove(trans.BlockNum, out transOrder);
                            }

                        }
                        else
                        {
                            transOrder = new List<TransOrderServerData>();
                            if (!(!string.IsNullOrEmpty(er.DelFlag) && er.DelFlag == "1"))
                            {
                                transOrder.Add(trans);
                            }
                            if (transOrder.Count > 0)
                            {
                                dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                {
                                    UpdateOrderCache(trans, value);
                                    return value;
                                });
                            }
                        }
                    }
                    else
                    {
                        transOrder = new List<TransOrderServerData>();
                        if (!(!string.IsNullOrEmpty(er.DelFlag) && er.DelFlag == "1"))
                        {
                            transOrder.Add(trans);
                        }

                        if (transOrder.Count > 0)
                        {
                            dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                            {
                                UpdateOrderCache(trans, value);
                                return value;
                            });
                        }
                    }
                }
                OnUpdateOrder?.Invoke(er);
                OnUpdateOrder2?.Invoke(er);
            }
        }

        //public ConcurrentDictionary<string, ExecutionReport> GetAllOrder()
        //{
        //    if (!isHasGetData)
        //    {
        //        while (true)
        //        {
        //            if (!isHasGetData)
        //            {
        //                Thread.Sleep(200);
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return dic;
        //}

        internal object _dataLock = new object();
        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="isall">是否包含其他人过桥成交</param>
        /// <returns></returns>
        public ConcurrentDictionary<string, TransOrderServerData> GetAllOrder(bool isall = true)
        {
            if (!isHasGetData)
            {
                while (true)
                {
                    if (!isHasGetData)
                    {
                        Thread.Sleep(200);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (isall)
            {
                ConcurrentDictionary<string, TransOrderServerData> dic = new ConcurrentDictionary<string, TransOrderServerData>();
                lock (_dataLock)
                {
                    foreach (var item in dicTransOrder)
                    {
                        dic.AddOrUpdate(item.Value.ExecID, item.Value, (key, value) => item.Value);
                    }

                    foreach (var item in dicOtherTransOrder)
                    {
                        dic.AddOrUpdate(item.Value.ExecID, item.Value, (key, value) => item.Value);
                    }

                    return dic;
                }
            }
            else
            {
                return dicTransOrder;
            }

        }

        /// <summary>
        /// 根据execID 获取指定订单
        /// </summary>
        /// <param name="execID"></param>
        /// <returns></returns>
        public TransOrderServerData GetTransOrderByExecID(string execID)
        {
            TransOrderServerData valuePairs = null;
            if (dicTransOrder != null && dicTransOrder.TryGetValue(execID, out valuePairs))
            {
                return valuePairs;
            }
            return valuePairs;
        }


        /// <summary>
        /// 根据execID 获取指定订单
        /// </summary>
        /// <param name="execID"></param>
        /// <returns></returns>
        public List<TransOrderServerData> GetTransOrderByBlocknum(string blocknum)
        {
            List<TransOrderServerData> valuePairs = null;
            if (dicBlocknumOrder != null && dicBlocknumOrder.TryGetValue(blocknum, out valuePairs))
            {
                return valuePairs;
            }
            return valuePairs;
        }

        /// <summary>
        /// 查询订单数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public void GetExecOrderHistData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = null;
            reply = GRHandler.Instance.ExecOrderHistQuery(req);
            if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
            {
                com.gateway.log.LogHelper.Info($"GetExecOrderHistData Result: Return {reply.Datas.Count} Records; TotalRecord {reply.totalSize} Records.");

                if (SystemPowerHandler.Instance.IsBridgeRole(LocalEnv.UserInfo.UserId))
                {
                    foreach (var item in reply.Datas)
                    {
                        //dic.AddOrUpdate(item.ExecID, item, (key, value) => item);
                        TransOrderServerData trans = new TransOrderServerData();
                        trans.CopyParentData(item);
                        dicTransOrder.AddOrUpdate(trans.ExecID, trans, (key, value) => trans);


                        if (string.IsNullOrWhiteSpace(trans.BlockNum))
                        {
                            continue;
                        }

                        List<TransOrderServerData> transOrder = null;
                        if (dicBlocknumOrder.Count > 0)
                        {
                            if (dicBlocknumOrder.TryGetValue(trans.BlockNum, out transOrder))
                            {
                                dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                {
                                    UpdateOrderCache(trans, value);
                                    return value;
                                });
                            }
                            else
                            {
                                transOrder = new List<TransOrderServerData>();
                                dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                {
                                    UpdateOrderCache(trans, value);
                                    return value;
                                });
                            }
                        }
                        else
                        {
                            transOrder = new List<TransOrderServerData>();
                            dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                            {
                                UpdateOrderCache(trans, value);
                                return value;
                            });
                        }
                    }
                }
                else
                {
                    foreach (var item in reply.Datas)
                    {
                        if (IsCanSeeOrder(item))
                        {
                            //dic.AddOrUpdate(item.ExecID, item, (key, value) => item);
                            TransOrderServerData trans = new TransOrderServerData();
                            trans.CopyParentData(item);
                            dicTransOrder.AddOrUpdate(trans.ExecID, trans, (key, value) => trans);

                            if (string.IsNullOrWhiteSpace(trans.BlockNum))
                            {
                                continue;
                            }
                            List<TransOrderServerData> transOrder = null;
                            if (dicBlocknumOrder.Count > 0)
                            {
                                if (dicBlocknumOrder.TryGetValue(trans.BlockNum, out transOrder))
                                {
                                    dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                    {
                                        UpdateOrderCache(trans, value);
                                        return value;
                                    });
                                }
                                else
                                {
                                    transOrder = new List<TransOrderServerData>();
                                    dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                    {
                                        UpdateOrderCache(trans, value);
                                        return value;
                                    });
                                }
                            }
                            else
                            {
                                transOrder = new List<TransOrderServerData>();
                                dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                {
                                    UpdateOrderCache(trans, value);
                                    return value;
                                });
                            }
                        }
                    }
                }

            }
        }


        public void GetExecOrderData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = GRHandler.Instance.ExecOrderQuery(req);
            if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
            {
                com.gateway.log.LogHelper.Info($"GetExecOrderData Result: Return {reply.Datas.Count} Records; TotalRecord {reply.totalSize} Records.");


                if (SystemPowerHandler.Instance.IsBridgeRole(LocalEnv.UserInfo.UserId))
                {
                    foreach (var item in reply.Datas)
                    {
                        //dic.AddOrUpdate(item.ExecID, item, (key, value) => item);
                        TransOrderServerData trans = new TransOrderServerData();
                        trans.CopyParentData(item);
                        dicTransOrder.AddOrUpdate(trans.ExecID, trans, (key, value) => trans);


                        if (string.IsNullOrWhiteSpace(trans.BlockNum))
                        {
                            continue;
                        }
                        List<TransOrderServerData> transOrder = null;
                        if (dicBlocknumOrder.Count > 0)
                        {
                            if (dicBlocknumOrder.TryGetValue(trans.BlockNum, out transOrder))
                            {
                                dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                {
                                    UpdateOrderCache(trans, value);
                                    return value;
                                });
                            }
                            else
                            {
                                transOrder = new List<TransOrderServerData>();
                                transOrder.Add(trans);
                                dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                {
                                    UpdateOrderCache(trans, value);
                                    return value;
                                });
                            }
                        }
                        else
                        {
                            transOrder = new List<TransOrderServerData>();
                            transOrder.Add(trans);
                            dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                            {
                                UpdateOrderCache(trans, value);
                                return value;
                            });
                        }
                    }
                }
                else
                {
                    foreach (var item in reply.Datas)
                    {
                        if (IsCanSeeOrder(item))
                        {
                            //dic.AddOrUpdate(item.ExecID, item, (key, value) => item);
                            TransOrderServerData trans = new TransOrderServerData();
                            trans.CopyParentData(item);
                            dicTransOrder.AddOrUpdate(trans.ExecID, trans, (key, value) => trans);


                            if (string.IsNullOrWhiteSpace(trans.BlockNum))
                            {
                                continue;
                            }
                            List<TransOrderServerData> transOrder = null;
                            if (dicBlocknumOrder.Count > 0)
                            {
                                if (dicBlocknumOrder.TryGetValue(trans.BlockNum, out transOrder))
                                {
                                    dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                    {
                                        UpdateOrderCache(trans, value);
                                        return value;
                                    });
                                }
                                else
                                {
                                    transOrder = new List<TransOrderServerData>();
                                    transOrder.Add(trans);
                                    dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                    {
                                        UpdateOrderCache(trans, value);
                                        return value;
                                    });
                                }
                            }
                            else
                            {
                                transOrder = new List<TransOrderServerData>();
                                transOrder.Add(trans);
                                dicBlocknumOrder.AddOrUpdate(trans.BlockNum, transOrder, (key, value) =>
                                {
                                    UpdateOrderCache(trans, value);
                                    return value;
                                });
                            }
                        }
                    }
                }
            }
        }

        public void GetExecOtherBridgeOrderData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = GRHandler.Instance.ExecOtherOrderQuery(req);
            if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
            {
                com.gateway.log.LogHelper.Info($"ExecOtherOrderQuery Result: Return {reply.Datas.Count} Records; TotalRecord {reply.totalSize} Records.");


                if (SystemPowerHandler.Instance.IsBridgeRole(LocalEnv.UserInfo.UserId))
                {
                    foreach (var item in reply.Datas)
                    {
                        TransOrderServerData trans = new TransOrderServerData();
                        trans.CopyParentData(item);
                        dicTransOrder.AddOrUpdate(trans.ExecID, trans, (key, value) => trans);
                    }
                }
                else
                {
                    foreach (var item in reply.Datas)
                    {
                        TransOrderServerData trans = new TransOrderServerData();
                        trans.CopyParentData(item);
                        dicOtherTransOrder.AddOrUpdate(trans.ExecID, trans, (key, value) => trans);
                        if ((!string.IsNullOrEmpty(item.DelFlag) && item.DelFlag == "1")
                    || ((!string.IsNullOrEmpty(item.BridgeFlag) && item.BridgeFlag.Equals("1"))
                       || string.IsNullOrEmpty(item.BridgeFlag)))
                        {
                            TransOrderServerData keyValues = null;
                            dicOtherTransOrder.TryRemove(item.ExecID, out keyValues);
                        }
                        //{

                        //}
                    }
                }
            }

            //查询后订阅
            GRHandler.Instance.SubscribeOtherGrExec(OnSubscribeGrExec);
        }

        private ConcurrentQueue<ExecutionReport> GrERQueue = new ConcurrentQueue<ExecutionReport>();
        private object _grLock = new object();
        private object _grqueueLock = new object();
        private AutoResetEvent _grautoResetEvent = new AutoResetEvent(false);
        public bool StopRefreshER = false;
        /// <summary>
        /// 更新回调订单
        /// </summary>
        /// <param name="er"></param>
        private void OnSubscribeGrExecOrder(List<ExecutionReport> lster)
        {
            if (lster == null)
                return;

            try
            {
                lock (_grLock)
                {
                    foreach (ExecutionReport er in lster)
                    {
                        GrERQueue.Enqueue(er);
                        _grautoResetEvent.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        private void RefreshGrOrder()
        {
            while (true)
            {
                try
                {
                    if (StopRefreshER)
                    {
                        break;
                    }

                    ExecutionReport er = null;

                    lock (_grqueueLock)
                    {
                        if (GrERQueue.TryDequeue(out er))
                        {
                            procGrOrder(er);

                            while (GrERQueue.Count > 0)
                            {
                                er = null;
                                GrERQueue.TryDequeue(out er);
                                procGrOrder(er);
                            }
                        }
                    }

                    if (GrERQueue.Count == 0)
                        _grautoResetEvent.WaitOne();
                }
                catch (ThreadAbortException ex)
                {
                    LogHelper.Debug("RefreshGrOrder Normal Abort:", ex);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("RefreshGrOrder exception:", ex);
                }
            }
        }


        /// <summary>
        /// 更新回调订单
        /// </summary>
        /// <param name="er"></param>
        private void procGrOrder(ExecutionReport er)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                TransOrderServerData transOrder = new TransOrderServerData();
                transOrder.CopyParentData(er);
                UpdateOrder(transOrder);
                //UpdateInnerOrder(transOrder);
            });
        }

        /// <summary>
        /// 更新回调
        /// </summary>
        /// <param name="er"></param>
        private void OnSubscribeGrExec(List<ExecutionReport> lster)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                if (lster != null && lster.Count > 0)
                {
                    if (System.Windows.Application.Current != null)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            foreach (ExecutionReport er in lster)
                            {
                                TransOrderServerData transOrder = new TransOrderServerData();
                                transOrder.CopyParentData(er);
                                TransOrderServerData trans = new TransOrderServerData();
                                trans.CopyParentData(er);
                                if (string.IsNullOrWhiteSpace(trans.BridgeOrder) && string.IsNullOrWhiteSpace(trans.BridgeOrder2))
                                {
                                    TransOrderServerData keyValues = null;
                                    dicOtherTransOrder.TryRemove(trans.ExecID, out keyValues);
                                }
                                else
                                {
                                    dicOtherTransOrder.AddOrUpdate(trans.ExecID, trans, (key, value) => trans);
                                    OnOtherBridgeOrderUpdateOrder?.Invoke(trans);
                                }
                            }
                        }));
                    }
                }
            });
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        public async void QueryOrderInfo()
        {
            try
            {
                await Task.Run(() =>
                 {
                     GrExecOrderQueryReq request = new GrExecOrderQueryReq();
                     request.createTimeEnd = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                     request.tradedateBegin = DateTime.Now.ToString("yyyyMMdd");
                     request.pageNum = 0;
                     request.ownOrder = "Y";
                     request.pageSize = 10000;
                     request.queryHistType = "1";
                     GetExecOrderHistData(request);
                     request = new GrExecOrderQueryReq();
                     request.pageNum = 0;
                     request.pageSize = 10000;
                     GetExecOrderData(request);
                     GRHandler.Instance.SubscribeGrExec(OnSubscribeGrExecOrder);
                     if (_systemPower.IsOrtherBridge)
                     {
                         request = new GrExecOrderQueryReq();
                         GetExecOtherBridgeOrderData(request);
                     }
                     isHasGetData = true;
                 });
            }
            catch (Exception ex)
            {
                com.gateway.log.LogHelper.Error(ex);
            }
            finally
            {
                isHasGetData = true;
            }
        }

        /// <summary>
        /// 当数据库拉取完成的时候，补充订单的债券信息
        /// </summary>
        public void FillBondInfo()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                //Thread.Sleep(2000);
                //等待债券拉取完成标志
                while (!BondInitCompleted)
                {
                    Thread.Sleep(100);
                }
                foreach (var order in dicTransOrder.Values.ToList())
                {
                    SupplementBondInfo(order);
                }
            });
        }

        public void FillBondInfo(IEnumerable<TransOrderServerData> orders)
        {
            //等待债券拉取完成标志
            while (!BondInitCompleted)
            {
                Thread.Sleep(100);
            }
            foreach (var order in orders)
            {
                SupplementBondInfo(order);
            }
        }

        /// <summary>
        /// 往成交单中补充债券信息
        /// </summary>
        /// <param name="po"></param>
        public void SupplementBondInfo(TransOrderServerData execOrder)
        {
            try
            {
                string bondId = execOrder.SecurityID;
                //BondsInfoPo bond = CRMDataHandler.Instance.GetBondsInfoPoOne(bondId);
                BondsInfoPo bond = Utils.CommonUtils.GetBondsInfoByCode(bondId);
                if (bond != null)
                {
                    execOrder.SecurityType = bond.SECURITY_TYPE;
                    execOrder.BondSubtype = bond.Bond_Subtype;
                    execOrder.ListedMarket = bond.listed_Market;
                    execOrder.IssueAmount = bond.Issue_Amount;
                    execOrder.IssuerRatingCurrent = bond.Issuer_Rating_Current;
                    execOrder.IssuerCode = bond.Issuer_Code;
                    execOrder.EntCor = bond.Ent_Cor;
                    execOrder.FRNIndexID = bond.FRNIndexID;
                }
                // ReSetBridgeStatus(execOrder);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Process Order error," + ex.Message, ex);
            }
        }

        /// <summary>
        /// 往成交单中补充债券信息
        /// </summary>
        /// <param name="po"></param>
        public void SupplementBondInfo(CopyDetailPo execOrder)
        {
            try
            {
                string bondId = execOrder.goodsCode;
                //BondsInfoPo bond = CRMDataHandler.Instance.GetBondsInfoPoOne(bondId);
                BondsInfoPo bond = Utils.CommonUtils.GetBondsInfoByCode(bondId);
                if (bond != null)
                {
                    execOrder.goodstype = bond.SECURITY_TYPE;
                    execOrder.BondSubtype = bond.Bond_Subtype;
                    execOrder.ListedMarket = bond.listed_Market;
                    execOrder.IssueAmount = bond.Issue_Amount;
                    execOrder.IssuerRatingCurrent = bond.Issuer_Rating_Current;
                    execOrder.IssuerCode = bond.Issuer_Code;
                    execOrder.EntCor = bond.Ent_Cor;
                    execOrder.FRNIndexID = bond.FRNIndexID;
                }
                // ReSetBridgeStatus(execOrder);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Process Order error," + ex.Message, ex);
            }
        }

        /// <summary>
        /// 是否有权限查看
        /// </summary>
        /// <returns></returns>
        public bool IsCanSeeOrder(ExecutionReport item)
        {
            bool isresult = true;
            if (!SystemPowerHandler.Instance.IsBridgeRole(LocalEnv.UserInfo.UserId))
            {
                if ((!string.IsNullOrEmpty(item.DelFlag) && item.DelFlag == "1")
                      ||
                      (!SystemPowerHandler.Instance.IsCanLookOrder(item.PartySubID)
                      && !SystemPowerHandler.Instance.IsCanLookOrder(item.CounterPartySubID)))
                {
                    isresult = false;
                }
            }
            else
            {
                if ((!string.IsNullOrEmpty(item.DelFlag) && item.DelFlag == "1")
                    || ((!string.IsNullOrEmpty(item.BridgeFlag) && item.BridgeFlag.Equals("1"))
                       || string.IsNullOrEmpty(item.BridgeFlag))
                       && (!SystemPowerHandler.Instance.IsCanLookOrder(item.PartySubID)
                      && !SystemPowerHandler.Instance.IsCanLookOrder(item.CounterPartySubID)))
                {
                    isresult = false;
                }
            }
            return isresult;
        }


        private void UpdateOrderCache(TransOrderServerData oldOrder, List<TransOrderServerData> newOrders)
        {
            ExecutionReport er = oldOrder.Clone();
            TransOrderServerData trans = new TransOrderServerData();
            trans.CopyParentData(er);
            if (newOrders == null)
            {
                newOrders = new List<TransOrderServerData>();
                newOrders.Add(trans);
            }
            else
            {
                if (newOrders.Count >= 1)
                {
                    if (newOrders.Any(_ => _.ExecID == oldOrder.ExecID))
                    {
                        int indexof = newOrders.IndexOf(newOrders.Where(_ => _.ExecID == oldOrder.ExecID).FirstOrDefault());
                        newOrders[indexof] = trans;
                    }
                    else
                    {
                        newOrders.Add(trans);
                    }
                }
                else
                {
                    newOrders.Add(trans);
                }
            }
        }


        /// <summary>
        /// 发送qq
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<PResult> SendQQMsg(SendQQMsgReq req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.sendQQMsg(req);
                if (reply != null && reply.Code != "0000")
                {
                    ShowMessage(reply.Msg);
                }
            });

            return reply;
        }

        private void ShowMessage(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.ShowDialog();

            }));
        }
        /// <summary>
        /// Broker发送催单消息
        /// </summary>
        /// <param name="order"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<bool> SendQQMsgToBroker(TransOrderServerData order, string text, string sign)
        {
            bool result = false;
            var tuple = GetQQSendTo(order);
            if (tuple != null && !string.IsNullOrEmpty(tuple.Item2))
            {
                //发送给TraderA的QQ
                SendQQMsgReq request = new SendQQMsgReq();
                request.counterPartySubId = tuple.Item2;
                request.partySubId = tuple.Item1;
                request.message = text;

                // sendType = 2 标识催单
                request.sendType = "2";

                var replay = await SendQQMsg(request);

                if (replay.IsSuccess())
                {
                    LogHelper.Info($"{sign}催单QQ成功，消息：\r\n{text}");
                    result = true;
                }
                else
                {
                    LogHelper.Info($"{sign}催单QQ失败，消息：\r\n{text}");
                }

                if (!replay.IsSuccess())
                {
                    LogHelper.Info($"订单无法发送催单消息");
                }

            }
            else
            {
                LogHelper.Info($"当前订单无法催单，内码:{order.Sequence}; id:{order.ExecID}");
            }
            return result;
        }

        /// <summary>
        /// Item1 = from; Item2 = to
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Tuple<string, string> GetQQSendTo(TransOrderServerData order)
        {
            Tuple<string, string> result;

            // 无桥
            if (order.BridgeOrderModel == null && order.BridgeOrderModel2 == null)
            {
                result = GetSendFromTo(order.PartySubID, order.CounterPartySubID);
            }
            // 单桥
            else if (order.BridgeOrderModel != null && order.BridgeOrderModel2 == null && !string.IsNullOrEmpty(order.BridgeOrderModel.reserveBrokerId))
            {
                result = GetSendFromTo(order.PartySubID, order.CounterPartySubID, order.BridgeOrderModel.reserveBrokerId);
            }
            // 单桥
            else if (order.BridgeOrderModel2 != null && order.BridgeOrderModel == null && !string.IsNullOrEmpty(order.BridgeOrderModel2.reserveBrokerId))
            {
                result = GetSendFromTo(order.PartySubID, order.CounterPartySubID, order.BridgeOrderModel2.reserveBrokerId);
            }
            // 双桥
            else
            {
                DateTime time1 = DateTime.Now;
                DateTime time2 = DateTime.Now;
                DateTime.TryParse(order.BridgeOrderModel.createTime, out time1);
                DateTime.TryParse(order.BridgeOrderModel2.createTime, out time2);

                if (time1 <= time2)
                {
                    result = GetSendFromTo(order.PartySubID, order.CounterPartySubID, order.BridgeOrderModel.reserveBrokerId);
                }
                else
                {
                    result = GetSendFromTo(order.PartySubID, order.CounterPartySubID, order.BridgeOrderModel2.reserveBrokerId);
                }
            }
            return result;
        }


        /// <summary>
        /// Item1 = from; Item2 = to
        /// </summary>
        /// <param name="brokerIdA"></param>
        /// <param name="brokerIdB"></param>
        /// <param name="bridgeBrokerId"></param>
        /// <returns></returns>
        private Tuple<string, string> GetSendFromTo(string brokerIdA, string brokerIdB, string bridgeBrokerId = null)
        {
            Tuple<string, string> result = null;
            if (string.IsNullOrEmpty(bridgeBrokerId))
            {
                // 保证两方 broker 不是同一个人
                if (!string.IsNullOrEmpty(brokerIdA) && !string.IsNullOrEmpty(brokerIdB) && brokerIdA != brokerIdB)
                {

                    if (LocalEnv.UserInfo.IsMyself(brokerIdA))
                    {
                        result = Tuple.Create(brokerIdA, brokerIdB);
                    }
                    else if (LocalEnv.UserInfo.IsMyself(brokerIdB))
                    {
                        result = Tuple.Create(brokerIdB, brokerIdA);
                    }
                }
            }
            else
            {
                // 保证 发送方和接收方不是同一个人
                if (LocalEnv.UserInfo.IsMyself(brokerIdA) && brokerIdA != bridgeBrokerId)
                {
                    result = Tuple.Create(brokerIdA, bridgeBrokerId);
                }
                // 保证 发送方和接收方不是同一个人
                else if (LocalEnv.UserInfo.IsMyself(brokerIdB) && brokerIdB != bridgeBrokerId)
                {
                    result = Tuple.Create(brokerIdB, bridgeBrokerId);
                }
            }
            return result;
        }

        public void BridgeStateProcessOrder(BridgeReserveState obj)
        {

            TransOrderServerData newpo = GetTransOrderByExecID(obj.execId);
            TransOrderServerData execOrder = new TransOrderServerData();
            execOrder.CopyParentData(newpo);
            if (obj.execId == execOrder.ExecID)
            {
                if (obj.stateType == 1)
                {
                    if (execOrder.BridgeOrderModel != null && execOrder.BridgeOrderModel.borderID == obj.borderId)
                    {
                        execOrder.BridgeOrderModel.bridgeStateEnum = obj.bridgeStateEnum;
                        execOrder.BridgeOrderModel.OfrbridgeStateEnum = obj.OfrbridgeStateEnum;
                        execOrder.BridgeOrderModel.BidbridgeStateEnum = obj.BidbridgeStateEnum;
                    }
                    if (execOrder.BridgeOrderModel2 != null && execOrder.BridgeOrderModel2.borderID == obj.borderId)
                    {
                        execOrder.BridgeOrderModel2.bridgeStateEnum = obj.bridgeStateEnum;
                        execOrder.BridgeOrderModel2.OfrbridgeStateEnum = obj.OfrbridgeStateEnum;
                        execOrder.BridgeOrderModel2.BidbridgeStateEnum = obj.BidbridgeStateEnum;
                    }
                }
                else if (obj.stateType == 2)
                {
                    if (obj.sid == 1)
                    {
                        if (execOrder.OfrIssueInfoReqList != null && execOrder.OfrIssueInfoReqList.Count > 0)
                        {
                            foreach (SendInstSevieceModel item in execOrder.OfrIssueInfoReqList)
                            {
                                if (!string.IsNullOrWhiteSpace(obj.issueInstId))
                                {
                                    if (item.issueInstId == obj.issueInstId)
                                    {
                                        item.IssueInstState = obj.issueInstState;
                                        item.RejectReason = obj.rejectReason;
                                    }
                                }
                                else
                                {
                                    if (item.issueInstName == obj.issueInstName)
                                    {
                                        item.IssueInstState = obj.issueInstState;
                                        item.RejectReason = obj.rejectReason;
                                    }
                                }

                            }
                        }
                    }
                    else if (obj.sid == 2)
                    {
                        if (execOrder.BidIssueInfoReqList != null && execOrder.BidIssueInfoReqList.Count > 0)
                        {
                            foreach (SendInstSevieceModel item in execOrder.BidIssueInfoReqList)
                            {
                                if (!string.IsNullOrWhiteSpace(obj.issueInstId))
                                {
                                    if (item.issueInstId == obj.issueInstId)
                                    {
                                        item.IssueInstState = obj.issueInstState;
                                        item.RejectReason = obj.rejectReason;
                                    }
                                }
                                else
                                {
                                    if (item.issueInstName == obj.issueInstName)
                                    {
                                        item.IssueInstState = obj.issueInstState;
                                        item.RejectReason = obj.rejectReason;
                                    }
                                }

                            }
                        }
                    }
                }
                //桥状态更新通知
                ReSetBridgeStatus(execOrder);

                //UpdateOrder?.Invoke(newpo);
                UpdateOrderOnBridgeStatusChanged?.Invoke(execOrder);
            }
        }


        /// <summary>
        /// 重置订单状态
        /// </summary>
        /// <param name="execOrder"></param>
        private void ReSetBridgeStatus(TransOrderServerData execOrder)
        {
            //ofr方向
            if (execOrder.OfrIssueInfoReqList != null && execOrder.OfrIssueInfoReqList.Count > 0)
            {
                List<int> statusValue = new List<int>();
                foreach (SendInstSevieceModel item in execOrder.OfrIssueInfoReqList)
                {
                    if (!statusValue.Contains(item.IssueInstState))
                    {
                        statusValue.Add(item.IssueInstState);
                    }
                }
                if (statusValue.Count == 1)
                {
                    if (statusValue[0] == 3 || statusValue[0] == 6)
                    {
                        if (execOrder.BridgeType.Equals("B,O"))
                        {
                            if (execOrder.BridgeOrderModel != null)
                            {
                                execOrder.BridgeOrderModel.ofrState = statusValue[0];
                            }
                        }
                        else
                        {
                            if (execOrder.BridgeType.Equals("B") || execOrder.BridgeType.Equals("O"))
                            {
                                if (execOrder.BridgeOrderModel != null)
                                {
                                    if (execOrder.BridgeTraderId == execOrder.BridgeOrderModel.instTradeId
                                        && execOrder.BridgeInstitutionId == execOrder.BridgeOrderModel.instId)
                                    {
                                        execOrder.BridgeOrderModel.ofrState = statusValue[0];
                                    }
                                    else
                                    {
                                        if (execOrder.BridgeOrderModel2 != null)
                                        {
                                            if (execOrder.BridgeTraderId == execOrder.BridgeOrderModel2.instTradeId
                                                && execOrder.BridgeInstitutionId == execOrder.BridgeOrderModel2.instId)
                                            {
                                                execOrder.BridgeOrderModel2.ofrState = statusValue[0];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

            //bid方向
            if (execOrder.BidIssueInfoReqList != null && execOrder.BidIssueInfoReqList.Count > 0)
            {
                List<int> statusValue = new List<int>();
                foreach (SendInstSevieceModel item in execOrder.BidIssueInfoReqList)
                {
                    if (!statusValue.Contains(item.IssueInstState))
                    {
                        statusValue.Add(item.IssueInstState);
                    }
                }

                if (statusValue.Count == 1)
                {
                    if (statusValue[0] == 3 || statusValue[0] == 6)
                    {
                        if (execOrder.BridgeType.Equals("B,O"))
                        {
                            if (execOrder.BridgeOrderModel != null)
                            {
                                execOrder.BridgeOrderModel.bidState = statusValue[0];
                            }
                        }
                        else
                        {
                            if (execOrder.BridgeType.Equals("B") || execOrder.BridgeType.Equals("O"))
                            {
                                if (execOrder.BridgeOrderModel != null)
                                {
                                    if (execOrder.BridgeTraderId2 == execOrder.BridgeOrderModel.instTradeId
                                        && execOrder.BridgeInstitutionId2 == execOrder.BridgeOrderModel.instId)
                                    {
                                        execOrder.BridgeOrderModel.bidState = statusValue[0];
                                    }
                                    else
                                    {
                                        if (execOrder.BridgeOrderModel2 != null)
                                        {
                                            if (execOrder.BridgeTraderId2 == execOrder.BridgeOrderModel2.instTradeId
                                                && execOrder.BridgeInstitutionId2 == execOrder.BridgeOrderModel2.instId)
                                            {
                                                execOrder.BridgeOrderModel2.bidState = statusValue[0];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

        }

        public bool ExistInOtherOrders(string id)
        {
            TransOrderServerData valuePairs = null;
            return dicOtherTransOrder.TryGetValue(id, out valuePairs);
        }

        public List<TransOrderServerData> GetOtherDetailPoById(string execID)
        {
            List<TransOrderServerData> list = new List<TransOrderServerData>();
            TransOrderServerData valuePairs = null;
            if (dicOtherTransOrder != null && dicOtherTransOrder.TryGetValue(execID, out valuePairs))
            {
                list.Add(valuePairs);
            }
            return list;
        }


        public bridgeOrder GetBridgeOrder(string bridgeOrder)
        {
            if (!string.IsNullOrWhiteSpace(bridgeOrder))
            {
                return JsonConvert.DeserializeObject<bridgeOrder>(bridgeOrder);
            }
            else
            {
                return null;
            }
        }

    }

}
