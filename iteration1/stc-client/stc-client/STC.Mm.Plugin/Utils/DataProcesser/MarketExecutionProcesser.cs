using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Handler.TBook;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Mm.Plugin.Utils.DataProcesser
{
    /// <summary>
    /// 市场成交处理器
    /// </summary>
    public class MarketExecutionProcesser
    {
        /// <summary>
        /// MarketExecutionProcesser实例
        /// </summary>
        private static MarketExecutionProcesser proc = new MarketExecutionProcesser();

        public static MarketExecutionProcesser Processer
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
        private MarketExecutionProcesser()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~MarketExecutionProcesser()
        {
            Dispose();
        }

        private object lockObj = new object();

        /// <summary>
        /// 我的成交数据源
        /// </summary>
        private ObservableCollection<MarketExecutionsItem> _myMarketExes = new ObservableCollection<MarketExecutionsItem>();
        public ObservableCollection<MarketExecutionsItem> MyMarketExes
        {
            get
            {
                lock (this.lockObj)
                {
                    BindingOperations.EnableCollectionSynchronization(_myMarketExes, lockObj);
                    return this._myMarketExes;
                }
            }
        }

        private const string SECURITYID_FIELD = "SecurityID";
        private const string TRANSACTIONTIME_FIELD = "TransactionTime";
        private const string EXEID_FIELD = "ExeID";
       
        private DataView _PrimaryKeyDV;
        private DataView PrimaryKeyDV
        {
            get
            {
                if (_PrimaryKeyDV == null)
                {
                    _PrimaryKeyDV = new DataTable().DefaultView;
                    _PrimaryKeyDV.Table.Columns.Add($"{SECURITYID_FIELD}");
                    _PrimaryKeyDV.Table.Columns.Add($"{TRANSACTIONTIME_FIELD}");
                    _PrimaryKeyDV.Table.Columns.Add($"{EXEID_FIELD}");

                    _PrimaryKeyDV.Table.PrimaryKey = new DataColumn[] { _PrimaryKeyDV.Table.Columns[$"{ EXEID_FIELD}"] };
                    _PrimaryKeyDV.Sort = $"{TRANSACTIONTIME_FIELD} DESC";
                    //_PrimaryKeyDV.Sort = $"{EXEID_FIELD},{TRANSACTIONTIME_FIELD}";
                }
                return _PrimaryKeyDV;
            }
        }

        private ConcurrentDictionary<string, int> countDic = new ConcurrentDictionary<string, int>();

        private AsyncThreadQueue<Common.Model.Fix.Message.ExecutionReport> _ProcessMarketExecutioinsThreadQueue;

        private List<Bond> _CacheBondList = new List<Bond>();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (isInit == false)
            {
                BindingOperations.EnableCollectionSynchronization(_myMarketExes, lockObj);

                if (_ProcessMarketExecutioinsThreadQueue == null)
                {
                    _ProcessMarketExecutioinsThreadQueue = new AsyncThreadQueue<Common.Model.Fix.Message.ExecutionReport>(UpdateMarketExecutions);
                }

                //#region  创建测试数据
                //this.GenerateTestData();
                //return;
                //#endregion


                #region 收MarketExec消息
                ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                #endregion

                #region 接收债券列表
                // todo首页
                //TBookHandler.Instance.Init(null, Instance_OnBondList);
                #endregion

                isInit = true;
            }
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Clear()
        {
            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;
            //TBookHandler.Instance.UnSubscribeBond(Instance_OnBondList);


            lock (this.lockObj)
            {
                if (countDic != null)
                {
                    countDic.Clear();
                }

                if (_PrimaryKeyDV != null)
                {
                    _PrimaryKeyDV.Table.Rows.Clear();
                }

                if (_myMarketExes != null)
                {
                    foreach (MarketExecutionsItem item in _myMarketExes)
                    {
                        item.Dispose();
                    }
                    _myMarketExes.Clear();
                }
            }
            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                _ProcessMarketExecutioinsThreadQueue.Clear();
            }

            isInit = false;
        }

        /// <summary>
        /// 释放数据
        /// </summary>
        public void Dispose()
        {
            this.Clear();

            lock (this.lockObj)
            {
                if (countDic != null)
                {
                    countDic.Clear();
                    countDic = null;
                }

                if (_PrimaryKeyDV != null)
                {
                    _PrimaryKeyDV.Table.Clear();
                    _PrimaryKeyDV.Table.Dispose();
                    _PrimaryKeyDV.Dispose();
                    _PrimaryKeyDV = null;
                }

                if (_myMarketExes != null)
                {
                    foreach (MarketExecutionsItem item in _myMarketExes)
                    {
                        item.Dispose();
                    }
                    _myMarketExes.Clear();
                    _myMarketExes = null;
                }
            }
            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                _ProcessMarketExecutioinsThreadQueue.Clear();
                _ProcessMarketExecutioinsThreadQueue.Dispose();
                _ProcessMarketExecutioinsThreadQueue = null;
            }

            isInit = false;
        }

        /// <summary>
        /// 清零数据回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(Common.Model.Base.ClearPo obj)
        {
            LogHandler.Info("MarketExecutionProcesser Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init();
                }));
            }
        }

        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            //UpdateMarketExecutions(obj);
            if (_ProcessMarketExecutioinsThreadQueue != null)
            {
                this._ProcessMarketExecutioinsThreadQueue.Enqueue(obj);
            }
        }

        /// <summary>
        /// 收取Bond更新数据
        /// </summary>
        /// <param name="bondList"></param>
        private void Instance_OnBondList(List<Bond> bondList)
        {
            lock (_CacheBondList)
            {
                foreach (Bond bond in bondList)
                {
                    _CacheBondList.RemoveAll(orderItem => (orderItem.securityid == bond.securityid
                                                           && orderItem.settltype == bond.settltype
                                                           && orderItem.venues == bond.venues
                                                           && orderItem.venuetypegw == bond.venuetypegw));
                    if (bond.action != "0")
                    {
                        _CacheBondList.Add(bond);
                    }
                }
            }

            lock (this.lockObj)
            {
                foreach (MarketExecutionsItem item in this._myMarketExes)
                {
                    UpdateStarStatus(item);
                }
            }
        }

        /// <summary>
        /// 更新收藏标记
        /// </summary>
        /// <param name="item"></param>
        private void UpdateStarStatus(MarketExecutionsItem item)
        {
            string securityID = item.SecurityID;
            lock (_CacheBondList)
            {
                //item.ShowStar = _CacheBondList.Any(p => p.securityid == securityID) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
               
            }
        }

        /// <summary>
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMarketExecutions(Common.Model.Fix.Message.ExecutionReport obj)
        {
            lock (lockObj)
            {
                DataRow findDr = this.PrimaryKeyDV.Table.Rows.Find(obj.ExecID);
                if (findDr == null)
                {
                    // 如果状态是4， 删除这笔单子。
                    if (obj.OrdStatus == "4")
                    {
                        return;
                    }

                    // 计数
                    if (this.countDic.ContainsKey(obj.SecurityID) == false)
                    {
                        this.countDic[obj.SecurityID] = 0;
                    }

                    if (this.countDic[obj.SecurityID] == 200)
                    {
                        // 删除老数据
                        this.PrimaryKeyDV.RowFilter = $"{SECURITYID_FIELD} = '{obj.SecurityID}'";

                        string oldExecID = CommonUtils.ConvertToString(this.PrimaryKeyDV[199][EXEID_FIELD]); //this.PrimaryKeyDV.Table.Rows[199][EXEID_FIELD]);

                        string oldTime = CommonUtils.ConvertToString(this.PrimaryKeyDV[199][TRANSACTIONTIME_FIELD]);
                        if (oldTime.CompareTo(obj.TransactTime) <= 0)
                        {

                            this.PrimaryKeyDV.Table.Rows.Remove(this.PrimaryKeyDV[199].Row);

                            DeleteRecord(oldExecID);
                        }
                        else
                            return;
                    }
                    else
                    {
                        this.countDic[obj.SecurityID]++;
                    }

                    // 存储基础数据
                    DataRow newDr = this.PrimaryKeyDV.Table.NewRow();
                    newDr[EXEID_FIELD] = obj.ExecID;
                    newDr[TRANSACTIONTIME_FIELD] = obj.TransactTime;
                    newDr[SECURITYID_FIELD] = obj.SecurityID;
                    this.PrimaryKeyDV.Table.Rows.Add(newDr);
                    newDr.AcceptChanges();

                    // 添加新数据
                    AddRecord(obj);
                }
                else
                {
                    string transactionTime = CommonUtils.ConvertToString(findDr[TRANSACTIONTIME_FIELD]);
                    if (transactionTime.CompareTo(obj.TransactTime) > 0)
                    {
                        // 如果时间早， 就不更新了。
                        return;
                    }

                    // 如果状态是4， 删除这笔单子。
                    if (obj.OrdStatus == "4")
                    {
                        this.countDic[obj.SecurityID]--;

                        this.PrimaryKeyDV.Table.Rows.Remove(findDr);

                        // 删除
                        DeleteRecord(obj.ExecID);

                        return;
                    }

                    // 更新数据
                    UpdateRecord(obj);
                }
            }
        }

        /// <summary>
        /// 删除execID的记录
        /// </summary>
        /// <param name="execID"></param>
        private void DeleteRecord(string execID)
        {
            MarketExecutionsItem marketExe = null;
            var result = _myMarketExes.Where(orderItem => orderItem.Key == execID);
            if (result != null)
            {
                foreach (MarketExecutionsItem item in result)
                {
                    marketExe = item;
                    break;
                }
            }
            if (marketExe != null)
            {
                _myMarketExes.Remove(marketExe);
            }
        }

        /// <summary>
        /// 更新记录内容
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateRecord(Common.Model.Fix.Message.ExecutionReport obj)
        {
            MarketExecutionsItem marketExe = null;
            var result = _myMarketExes.Where(orderItem => orderItem.Key == obj.ExecID);
            if (result != null)
            {
                foreach (MarketExecutionsItem item in result)
                {
                    marketExe = item;
                    break;
                }
            }
            if (marketExe != null)
            {
                marketExe.Key = obj.ExecID;
                marketExe.Side = obj.Side; // 方向
                marketExe.SecurityID = obj.SecurityID; // 代码
                marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.LastPx); //用于显示成交价
                marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.LastPx); // 成交价
                marketExe.SourceTrader = obj.Venues; // 来源
                marketExe.Time = obj.TransactTime;// 时间

                UpdateStarStatus(marketExe);//星星状态
            }
        }

        /// <summary>
        /// 添加新记录
        /// </summary>
        /// <param name="obj"></param>
        private void AddRecord(Common.Model.Fix.Message.ExecutionReport obj)
        {
            MarketExecutionsItem marketExe = new MarketExecutionsItem();
            marketExe.Key = obj.ExecID;
            marketExe.Side = obj.Side; // 方向
            marketExe.SecurityID = obj.SecurityID; // 代码
            marketExe.ExecutePricePercent_Display = CommonUtils.FormatPrice(obj.LastPx); //用于显示成交价
            marketExe.ExecutePricePercent = CommonUtils.ConvertToDecimal(obj.LastPx); // 成交价
            marketExe.SourceTrader = obj.Venues; // 来源
            marketExe.Time = obj.TransactTime;// 时间
            _myMarketExes.Add(marketExe);

            UpdateStarStatus(marketExe);//星星状态
        }

        /// <summary>
        /// 生成测试数据
        /// </summary>
        private void GenerateTestData()
        {
            // -------- 创建测试数据 -------------

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                // 债券列表
                Bond bond1 = new Bond();
                bond1.closeby = "kong2";
                bond1.createtime = DateTime.Now.ToString("hh:mm:ss");
                bond1.enable = "0";
                bond1.ip = "172.16.97.119";
                bond1.remark = "手动关闭";
                bond1.securityid = "140215.IB";
                bond1.settltype = "2";
                bond1.symbol = "140215.IB";
                bond1.terminal = "";
                bond1.updatetime = DateTime.Now.ToString("hh:mm:ss");
                bond1.username = "";
                bond1.venues = "ODM";
                bond1.venuetypegw = "ODM";
                List<Bond> list = new List<Bond>();
                list.Add(bond1);
                Instance_OnBondList(list);

                int i = 0;
                Common.Model.Fix.Message.ExecutionReport er = new Common.Model.Fix.Message.ExecutionReport();
                er.ExecID = string.Format($"e{i}"); // 成交编号
                er.ExecType = "F"; // 执行状态 ; F = 正常 ; 5 = 修改; 4 = 撤销
                er.Info1 = DateTime.Now.ToString("HH:mm:ss");
                er.LastPx = "107.512";  //成交价
                er.LastQty = "0.00000000"; // 成交量
                er.OrdRejReason = "";
                er.OrdStatus = "2"; // 订单状态； 0 = 有效（正常）；2 = 成交；4 = 撤销；I = 冻结；21 = 过期
                er.OrderID = string.Format($"emse{i}"); // 订单编号
                er.PartyName = "";
                er.PartySubID = ""; // 本方交易员id
                er.SecurityID = "140215.IB"; // 代码
                er.Side = "Z";    // 这个里是x, y, z 是什么意思, 是否是分别代表GVN，TKN，Refer
                er.StandardHeader.MsgType = "8";
                er.VenueTypeGW = "CDH";
                er.Venues = "CTTJ";
                UpdateMarketExecutions(er);

                i++;
                Common.Model.Fix.Message.ExecutionReport er1 = new Common.Model.Fix.Message.ExecutionReport();
                er1.ExecID = string.Format($"e{i}"); // 成交编号
                er1.ExecType = "F"; // 执行状态 ; F = 正常 ; 5 = 修改; 4 = 撤销
                er1.Info1 = DateTime.Now.ToString("HH:mm:ss");
                er1.LastPx = "100.9956";  //成交价
                er1.LastQty = "0.00000000"; // 成交量
                er1.OrdRejReason = "";
                er1.OrdStatus = "2"; // 订单状态； 0 = 有效（正常）；2 = 成交；4 = 撤销；I = 冻结；21 = 过期
                er1.OrderID = string.Format($"emse{i}"); // 订单编号
                er1.PartyName = "";
                er1.PartySubID = ""; // 本方交易员id
                er1.SecurityID = "190305.IB"; // 代码
                er1.Side = "Y";    // 这个里是x, y, z 是什么意思
                er1.StandardHeader.MsgType = "8";
                er1.VenueTypeGW = "CDH";
                er1.Venues = "PTCN";
                UpdateMarketExecutions(er1);

                i++;
                Common.Model.Fix.Message.ExecutionReport er2 = new Common.Model.Fix.Message.ExecutionReport();
                er2.ExecID = string.Format($"e{i}"); // 成交编号
                er2.ExecType = "F"; // 执行状态 ; F = 正常 ; 5 = 修改; 4 = 撤销
                er2.Info1 = "09:00:00";
                er2.LastPx = "101.1401";  //成交价
                er2.LastQty = "0.00000000"; // 成交量
                er2.OrdRejReason = "";
                er2.OrdStatus = "2"; // 订单状态； 0 = 有效（正常）；2 = 成交；4 = 撤销；I = 冻结；21 = 过期
                er2.OrderID = string.Format($"emse{i}"); // 订单编号
                er2.PartyName = "";
                er2.PartySubID = ""; // 本方交易员id
                er2.SecurityID = "190215.IB"; // 代码
                er2.Side = "X";    // 这个里是x, y, z 是什么意思
                er2.StandardHeader.MsgType = "8";
                er2.VenueTypeGW = "CDH";
                er2.Venues = "CCTB";
                UpdateMarketExecutions(er2);

                for (int j = 5; j < 1000; j++)
                {
                    Common.Model.Fix.Message.ExecutionReport erx = new Common.Model.Fix.Message.ExecutionReport();
                    erx.ExecID = string.Format($"e{j}"); // 成交编号
                    erx.ExecType = "F"; // 执行状态 ; F = 正常 ; 5 = 修改; 4 = 撤销
                    erx.Info1 = "09:00:00";
                    erx.LastPx = "101.1401";  //成交价
                    erx.LastQty = "0.00000000"; // 成交量
                    erx.OrdRejReason = "";
                    erx.OrdStatus = "2"; // 订单状态； 0 = 有效（正常）；2 = 成交；4 = 撤销；I = 冻结；21 = 过期
                    erx.OrderID = string.Format($"emse{i}"); // 订单编号
                    erx.PartyName = "";
                    erx.PartySubID = ""; // 本方交易员id
                    erx.SecurityID = "190215.IB"; // 代码
                    erx.Side = "X";    // 这个里是x, y, z 是什么意思
                    erx.StandardHeader.MsgType = "8";
                    erx.VenueTypeGW = "CDH";
                    erx.Venues = "CCTB";
                    erx.TransactTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    UpdateMarketExecutions(erx);
                }
            });
            //-------------------------------------
        }
    }
}
