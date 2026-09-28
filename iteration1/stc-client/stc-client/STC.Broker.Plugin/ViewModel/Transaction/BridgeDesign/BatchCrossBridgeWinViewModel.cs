using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using STC.Broker.Plugin.Model.Transaction.DesignModel;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Transaction.BridgeDesign;
using STC.Common.Model.PT;
using STC.Common.ViewModel;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.ViewModel.Transaction.BridgeDesign
{
    /// <summary>
    /// 一键过桥
    /// </summary>
    public class BatchCrossBridgeWinViewModel : ViewModelBase
    {
        private string batchCount = string.Empty;
        private FrameworkElement loadingUI = null;
        private Dictionary<int, ObservableCollection<BridgeOrderInfoViewModel>> orderDic = new Dictionary<int, ObservableCollection<BridgeOrderInfoViewModel>>();
        private Dictionary<string, BatchCrossBridgeModel> modelDic = new Dictionary<string, BatchCrossBridgeModel>();
        private BatchCrossBridgeWin mainWindow;
        private SysWarnGUI warn;

        public Dictionary<int, bool> checkAllCache = new Dictionary<int, bool>();
        public Dictionary<string, string> bridgeFeeCache = new Dictionary<string, string>();
        public Dictionary<string, List<FeeSettingInfo>> feeInstCache = new Dictionary<string, List<FeeSettingInfo>>();


        public BatchCrossBridgeWinViewModel(BatchCrossBridgeWin window, FrameworkElement loadingUI)
        {
            mainWindow = window;
            //this.loadingUI = loadingUI;

            warn = new SysWarnGUI();
            warn.Closed += Warn_Closed;
            InitData();
        }

        private void Warn_Closed(object sender, EventArgs e)
        {
            warn = null;
        }

        /// <summary>
        /// 点开窗口查询上次运行的结果
        /// </summary>
        /// <returns></returns>
        public async Task InitData()
        {
            await QueryLastRunResult();
        }

        /// <summary>
        /// 向后台查询上一次运行结果
        /// </summary>
        private async Task QueryLastRunResult(int bridgeIndex = -1)
        {
            List<BatchCrossBridgeModel> model = new List<BatchCrossBridgeModel>();
            //loadingUI.Visibility = Visibility.Visible;
            var response = new BatchCrossBridgeModel();//= await BrokerCEDAServer.Instance.QueryBatchCrossBridgeOrders();
            model.AddRange((IEnumerable<BatchCrossBridgeModel>)response);

            MatchOrderInfo(model);

            mainWindow.Dispatcher.Invoke(new Action(() =>
            {
                ConvertToViewModel(model);
                NavigateOrderBirdge(bridgeIndex);
                loadingUI.Visibility = Visibility.Collapsed;
            }));

        }

        /// <summary>
        /// 导航到有订单的桥下
        /// </summary>
        private void NavigateOrderBirdge(int bridgeIndex = -1)
        {
            BridgeOrderInfos.Clear();
            ObservableCollection<BridgeOrderInfoViewModel> values;
            if (orderDic.TryGetValue(bridgeIndex, out values))
            {
                foreach (var item in values)
                {
                    BridgeOrderInfos.Add(item);
                }
                mainWindow.lsBridge.SelectedIndex = bridgeIndex;
            }
            else if (bridgeIndex == -1)
            {
                foreach (var pair in orderDic)
                {
                    if (pair.Value.Count > 0)
                    {
                        foreach (var item in pair.Value)
                        {
                            BridgeOrderInfos.Add(item);
                        }
                        mainWindow.lsBridge.SelectedIndex = pair.Key;
                        break;
                    }
                }
                if (BridgeOrderInfos.Count == 0 && orderDic.Count > 0)
                {
                    mainWindow.lsBridge.SelectedIndex = 0;
                }
            }
            else
            {
                mainWindow.lsBridge.SelectedIndex = bridgeIndex;
            }

        }

        /// <summary>
        /// 获取执行订单列表
        /// </summary>
        private Dictionary<string, OrderPo> GetBridExecOrders(Dictionary<string, BridgeOrderModel> processDetail)
        {
            var result = new Dictionary<string, OrderPo>();
            if (processDetail != null && processDetail.Count > 0)
            {
                var temp = new Dictionary<string, OrderPo>();
                foreach (var pair in processDetail)
                {
                    temp[pair.Key] = new OrderPo();//OrderHandler.Instance.GetOrderByIdWithoutCopy(pair.Value.OrderId);
                }
                var sortedDic = temp.OrderBy(x => x.Value?.createTime);
                foreach (var pair in sortedDic)
                {
                    result[pair.Key] = pair.Value;
                }
            }
            return result;
        }


        /// <summary>
        /// 查找订单缓存，补齐订单信息
        /// </summary>
        private void MatchOrderInfo(List<BatchCrossBridgeModel> model)
        {
            modelDic.Clear();
            foreach (var item in model)
            {
                int showSeq = 0;
                List<string> noOrderList = new List<string>();

                var orderDic = GetBridExecOrders(item.ProcessDetails);
                foreach (var pair in orderDic)
                {
                    if (pair.Value != null)
                    {
                        var execOrder = pair.Value.execOrders.FirstOrDefault(x => x.ExecID == pair.Key);
                        if (execOrder != null)
                        {
                            var orderInfo = new CrossBridgeOrderInfo
                            {
                                BrokerAName = ConvertTextHelper.IsChinese(execOrder.PartySubName),
                                BrokerBName = ConvertTextHelper.IsChinese(execOrder.CounterPartySubName),
                                OrderId = item.ProcessDetails[pair.Key].OrderId,
                                OrderSeq = (int)execOrder.seq,
                                ExecuteOrderId = pair.Key,
                                OrderText = GetOrderText(item, execOrder, ++showSeq, item.ProcessDetails[pair.Key])
                            };
                            item.BridgeOrderInfo.Add(orderInfo);
                        }
                    }
                    else
                    {
                        noOrderList.Add(pair.Key);
                        LogHelper.Info($"找不到订单, Id={item.ProcessDetails[pair.Key].OrderId}; execId={pair.Key}");
                    }
                }
                foreach (var noOrderItme in noOrderList)
                {
                    item.ProcessDetails.Remove(noOrderItme);
                }
                modelDic[item.InstId + item.InstTraderId] = item;
            }
        }

        /// <summary>
        /// 获取订单显示文本
        /// </summary>
        /// <param name="insId"></param>
        /// <param name="traderId"></param>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        private string GetOrderText(BatchCrossBridgeModel bcModel, OrderDetailPo orderDetailPo, int showSeq, BridgeOrderModel bridgeOrderModel)
        {
            string result = string.Empty;
            var bridge = UserBridgeHandler.Instance.GetBridgeById(bcModel.InstId, bcModel.InstTraderId);
            if (bridge == null)
            {
                bridge = new UserBridge
                {
                    bankTraderid = string.Empty,
                    bankInstitutionid = string.Empty,
                };
                var bridgeName = $"{bcModel.InstName}({bcModel.InstTraderName})";
                LogHelper.Info($"订单 {orderDetailPo.seq} 找不到桥 {bridgeName}");
            }

            var bridgeOrderDetail = ObjConvertHelper.CopyToEx(orderDetailPo);
            bridgeOrderDetail.bridgeSeq = showSeq;

            var bridgeChannel = GetBridgeChannel(orderDetailPo.SecurityID, bridge.channel);

            bridgeOrderDetail.BridgeFee = bridgeOrderModel.OfrProxyFee;
            bridgeOrderDetail.BridgeFee2 = bridgeOrderModel.BidProxyFee;
            bridgeOrderDetail.bridgeOrder = new bridgeOrder { ofrDir = 2, bidDir = 2 };
            bridgeOrderDetail.BridgeSendTo = bridge.sendto;
            bridgeOrderDetail.BridgeSendTo2 = bridge.sendto;
            bridgeOrderDetail.BridgeChannel = bridgeChannel;
            bridgeOrderDetail.BridgeChannel2 = bridgeChannel;
            //bridgeOrderDetail.bridgeremark = bridge.remark;
            //bridgeOrderDetail.bridgeremark2 = bridge.remark;

            bridgeOrderDetail.OldOrderTxt = string.Format("{0}  {1}", bridgeOrderDetail.ShowSeq, BridgeHelper.CreateBridgeTxt(bridgeOrderDetail, "B,O", bridge));
            bridgeOrderDetail.OrderTxt = BridgeHelper.GetBridgeTxt(bridgeOrderDetail, bridge, bridgeOrderDetail.OldOrderTxt);
            result = bridgeOrderDetail.OrderTxt;

            return result;
        }

        /// <summary>
        /// 获取桥渠道
        /// </summary>
        /// <returns></returns>
        private static string GetBridgeChannel(string bondId, string birdgeChannel)
        {
            string result = birdgeChannel;
            if (!string.IsNullOrWhiteSpace(bondId))
            {
                if (bondId.EndsWith(".SH") || bondId.EndsWith(".SZ"))
                {
                    result = "6";
                }
            }
            return result;
        }

        /// <summary>
        /// 获取桥费用
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="remianTime"></param>
        /// <returns></returns>
        private string GetBirdgeFee(string instId, string remianTime)
        {
            string result = string.Empty;
            string cacheKey = $"{instId}_{remianTime}";
            string fee;
            List<FeeSettingInfo> paymentList;
            if (bridgeFeeCache.TryGetValue(cacheKey, out fee))
            {
                result = fee;
            }
            else if (feeInstCache.TryGetValue(instId, out paymentList))
            {
                foreach (var item in paymentList)
                {
                    if (CompareRemainTime(remianTime, item.termStart) && !CompareRemainTime(remianTime, item.termEnd))
                    {
                        result = $"{item.fee}厘";
                        bridgeFeeCache[cacheKey] = result;
                        break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(instId))
                {
                    try
                    {
                        //QueryFeeRequestPo request = new QueryFeeRequestPo();
                        //request.proxyInstId = instId;
                        //var payments = BrokerCEDAServer.Instance.QueryProxyFee(request);

                        //if (payments != null && payments.Data != null && payments.Data.Count > 0)
                        //{
                        //    feeInstCache[instId] = payments.Data;
                        //    foreach (var item in payments.Data)
                        //    {
                        //        if (CompareRemainTime(remianTime, item.termStart) && !CompareRemainTime(remianTime, item.termEnd))
                        //        {
                        //            result = $"{item.fee}厘";
                        //            bridgeFeeCache[cacheKey] = result;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("获取代付费用失败", e);
                    }

                }
            }
            return result;
        }

        /// <summary>
        /// 比较剩余期限和endtime
        /// </summary>
        /// <param name="remainValue"></param>
        /// <param name="compareValue"></param>
        /// <returns></returns>
        private bool CompareRemainTime(string remainValue, string compareValue)
        {
            if (string.IsNullOrEmpty(compareValue) || string.IsNullOrEmpty(remainValue))
            {
                return false;
            }

            var valueLast = remainValue.Last();
            var compLast = compareValue.Last();

            var valueTime = remainValue.Substring(0, remainValue.Length - 1);
            var compTime = compareValue.Substring(0, compareValue.Length - 1);

            bool result = true;
            if (valueLast.Equals(compLast))
            {
                result = double.Parse(valueTime) > double.Parse(compTime);
            }
            else if (compareValue == "0")
            {
                result = true;
            }
            else
            {
                switch (valueLast)
                {
                    case 'Y':
                        {
                            result = true;
                            break;
                        }
                    case 'M':
                        {
                            if (compLast.Equals('Y'))
                            {
                                result = false;
                            }
                            else
                            {
                                result = true;
                            }
                            break;
                        }
                    case 'D':
                        {
                            result = false;
                            break;
                        }
                }
            }
            return result;
        }


        /// <summary>
        /// Model to ViewModel
        /// </summary>
        /// <param name="models"></param>
        private int ConvertToViewModel(List<BatchCrossBridgeModel> models)
        {
            int index = 0;
            int count = 0;

            BridgeInfos.Clear();
            orderDic.Clear();
            BridgeOrderInfos.Clear();

            foreach (var model in models)
            {
                var usedLimitedShow = $"{model.UsedOrderQuota}/{model.BridgeOrderQuota}";
                var vm = new BridgeInfoViewModel
                {
                    InstitutionId = model.InstId,
                    TraderId = model.InstTraderId,
                    Item1 = $"{model.InstName}({model.InstTraderName})",
                    Item2 = usedLimitedShow,
                };
                BridgeInfos.Add(vm);

                int countShow = 0;
                if (model.BridgeOrderInfo != null && model.BridgeOrderInfo.Count > 0)
                {
                    var temp = new ObservableCollection<BridgeOrderInfoViewModel>();
                    foreach (var item in model.BridgeOrderInfo)
                    {
                        temp.Add(new BridgeOrderInfoViewModel
                        {
                            OrderSeq = item.OrderSeq.ToString(),
                            BrokerAName = item.BrokerAName,
                            BrokerBName = item.BrokerBName,
                            OrderText = item.OrderText,
                            ExecOrderId = item.ExecuteOrderId
                        });
                    }
                    countShow = temp.Count;
                    orderDic[index] = temp;
                }
                // 默认全选是false
                checkAllCache[index] = false;

                //vm.Item2 = countShow.ToString();
                count += model.ProcessDetails.Count;
                index++;
            }
            ObservableCollection<BridgeOrderInfoViewModel> showData;
            if (orderDic.Count > 0 && orderDic.TryGetValue(0, out showData))
            {
                foreach (var item in showData)
                {
                    BridgeOrderInfos.Add(item);
                }
            }
            BatchCount = $"{count}笔";
            return count;
        }

        /// <summary>
        /// ViewModel 转换成 Model
        /// </summary>
        /// <returns></returns>
        private void ConvertToModels()
        {
            foreach (var pair in orderDic)
            {
                var modelPair = modelDic.ElementAt(pair.Key);
                var notConfirmList = new List<string>();
                foreach (var item in pair.Value)
                {
                    if (!item.IsSelectedFlag)
                    {
                        notConfirmList.Add(item.ExecOrderId);
                    }
                }

                RemoveNotConfirmItem(notConfirmList, modelPair.Value.ProcessDetails);

                // 后端不需要桥订单详细信息
                modelPair.Value.BridgeOrderInfo.Clear();
            }
        }

        /// <summary>
        /// 确认订单时，需要移除没有勾选的订单
        /// </summary>
        /// <param name="notConfirmList"></param>
        /// <param name="orderDic"></param>
        private void RemoveNotConfirmItem(List<string> notConfirmList, Dictionary<string, BridgeOrderModel> orderIdDic)
        {
            foreach (var item in notConfirmList)
            {
                orderIdDic.Remove(item);
            }
        }

        public bool NeedCheckAll()
        {
            return !BridgeOrderInfos.Any(x => !x.IsSelectedFlag);
        }


        /// <summary>
        /// 当前批次总数
        /// </summary>
        public string BatchCount;
        //{
        //    get => batchCount;
        //    set => SetProperty(ref batchCount, value, "BatchCount");

        //}

        /// <summary>
        /// 桥列表
        /// </summary>
        public ObservableCollection<BridgeInfoViewModel> BridgeInfos { get; set; } = new ObservableCollection<BridgeInfoViewModel>();

        /// <summary>
        /// 桥订单列表
        /// </summary>
        public ObservableCollection<BridgeOrderInfoViewModel> BridgeOrderInfos { get; set; } = new ObservableCollection<BridgeOrderInfoViewModel>();

        /// <summary>
        /// 切换桥
        /// </summary>
        /// <param name="index"></param>
        public void ChangeBirdgeData(int index)
        {
            BridgeOrderInfos.Clear();
            ObservableCollection<BridgeOrderInfoViewModel> value;
            if (orderDic.TryGetValue(index, out value))
            {
                foreach (var item in value)
                {
                    BridgeOrderInfos.Add(item);
                }
            }
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="isSelected"></param>
        public void SetAllBridgeOrderIsSelected(int bridgeIndex, bool isSelected)
        {
            //foreach (var pair in orderDic)
            //{
            //    foreach (var item in pair.Value)
            //    { 
            //        item.IsSelectedFlag = isSelected;
            //    }
            //}
            mainWindow.Dispatcher.Invoke(() =>
            {
                foreach (var order in BridgeOrderInfos)
                {
                    order.IsSelectedFlag = isSelected;
                }
                checkAllCache[bridgeIndex] = isSelected;
            });
        }

        /// <summary>
        /// 运行一键过桥
        /// </summary>
        public async Task RunBatchCrossBridge()
        {
            // 加面罩
            // 查询后台
            loadingUI.Visibility = Visibility.Visible;
            //var response = await BrokerCEDAServer.Instance.RunBatchCrossBridgeOrders();

            //if ("0000".Equals(response.Code))
            //{
                //var tempData = response.Data ?? new List<BatchCrossBridgeModel>();
                //MatchOrderInfo(tempData);
                //var waitConfirmCount = ConvertToViewModel(tempData);
                NavigateOrderBirdge();

                if (warn != null)
                {
                    warn.Close();
                }

                warn = new SysWarnGUI();
                warn.Closed += Warn_Closed;
                //Window win = Application.Current.MainWindow;
                warn.Owner = mainWindow;
                warn.tbTitle.Text = "运行结果";
                warn.tbMsg.Text = "";//$"可加桥: {response.ExecutableOrderSize}笔, 待确认: {waitConfirmCount}笔, 剩余: {response.ExecutableOrderSize - waitConfirmCount}笔";
                warn.WindowStartupLocation = warn.Owner.WindowStartupLocation;
                warn.Show();
            //}
            // 其他人正在运行
            //else if ("8001".Equals(response.Code))
            //{
            //    if (warn != null)
            //    {
            //        warn.Close();
            //    }

            //    warn = new SysWarnGUI();
            //    warn.Closed += Warn_Closed;
            //    warn.Owner = mainWindow;
            //    warn.tbTitle.Text = "运行失败";
            //    warn.tbMsg.Text = "功能运行中，请稍后";
            //    warn.WindowStartupLocation = warn.Owner.WindowStartupLocation;
            //    warn.Show();
            //    LogHelper.Info($"其他人正在运行一键过桥");
            //}
            //else if ("8005".Equals(response.Code))
            //{
            //    if (warn != null)
            //    {
            //        warn.Close();
            //    }

            //    warn = new SysWarnGUI();
            //    warn.Closed += Warn_Closed;
            //    warn.Owner = mainWindow;
            //    warn.tbTitle.Text = "运行失败";
            //    warn.tbMsg.Text = "有桥专员正在进行加桥确认操作，请稍后重试！";
            //    warn.WindowStartupLocation = warn.Owner.WindowStartupLocation;
            //    warn.Show();
            //    LogHelper.Info($"有桥专员正在进行加桥确认操作，请稍后重试！");
            //}
            //else
            //{
            //    LogHelper.Error($"运行一键过桥失败");
            //}


            loadingUI.Visibility = Visibility.Collapsed;

            //await QueryLastRunResult();
        }

        /// <summary>
        /// 检查确认单的桥是否添加
        /// </summary>
        /// <returns></returns>
        private List<string> CheckBridgeExists(List<BatchCrossBridgeModel> seletedBridgeInfos)
        {
            List<string> bridges = new List<string>();
            foreach (var item in seletedBridgeInfos)
            {
                var bridge = UserBridgeHandler.Instance.GetBridgeById(item.InstId, item.InstTraderId);
                if (bridge == null)
                {
                    bridges.Add($"{item.InstName}({item.InstTraderName})");
                }
            }
            return bridges;
        }

        /// <summary>
        /// 获取选择的订单的信息，返回选择的 BatchCrossBridgeModel
        /// </summary>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public List<BatchCrossBridgeModel> GetSeletedBridgeOrder()
        {
            var bridgeInfo = new List<BatchCrossBridgeModel>();
            foreach (var pair in orderDic)
            {
                var modelPair = modelDic.ElementAt(pair.Key);
                bool hasSeleted = false;
                foreach (var item in pair.Value)
                {
                    if (item.IsSelectedFlag)
                    {
                        hasSeleted = true;
                        break;
                    }
                }
                if (hasSeleted)
                {
                    bridgeInfo.Add(new BatchCrossBridgeModel
                    {
                        InstId = modelPair.Value.InstId,
                        InstName = modelPair.Value.InstName,
                        InstTraderId = modelPair.Value.InstTraderId,
                        InstTraderName = modelPair.Value.InstTraderName
                    });
                }
            }
            return bridgeInfo;
        }

        /// <summary>
        /// 确认加桥订单
        /// </summary>
        public async Task ConfirmBrigeOrder(int bridgeIndex)
        {
            var orderBridges = GetSeletedBridgeOrder();
            if (orderBridges.Count == 0)
            {
                string errorMesssage = $"没有确认的订单";

                if (warn != null)
                {
                    warn.Close();
                }

                warn = new SysWarnGUI();
                //Window win = Application.Current.MainWindow;
                warn.Owner = mainWindow;
                warn.tbTitle.Text = "确认一键加桥";
                warn.tbMsg.Text = errorMesssage;
                warn.WindowStartupLocation = warn.Owner.WindowStartupLocation;
                warn.Show();
                return;
            }

            var noBriges = CheckBridgeExists(orderBridges);
            if (noBriges.Count > 0)
            {
                string errorMesssage = $"请先新建桥: {string.Join(",", noBriges)}";
                if (warn != null)
                {
                    warn.Close();
                }
                warn = new SysWarnGUI();
                warn.Owner = mainWindow;
                warn.tbTitle.Text = "缺桥提示";
                warn.tbMsg.Text = errorMesssage;
                warn.WindowStartupLocation = warn.Owner.WindowStartupLocation;
                warn.Show();
               // LogHelper.Info($"确认前缺桥提示， {MsgHelper.Serializer(noBriges)}");
                return;
            }

            ConvertToModels();
            List<BatchCrossBridgeModel> list = new List<BatchCrossBridgeModel>();
            foreach (var item in modelDic)
            {
                if (item.Value.ProcessDetails.Count > 0)
                {
                    list.Add(item.Value);
                }
            }

            if (list.Count > 0)
            {
                loadingUI.Visibility = Visibility.Visible;

                //var tuple = await BrokerCEDAServer.Instance.ConfirmBatchCrossBridgeOrders(list).ConfigureAwait(true);
                //if (!string.IsNullOrEmpty(tuple.Item2))
                //{
                //    string errorMesssage = "确认失败，功能重新被运行，数据已刷新，请重试";
                //    if (warn != null)
                //    {
                //        warn.Close();
                //    }
                //    warn = new SysWarnGUI();
                //    warn.Owner = mainWindow;
                //    warn.tbTitle.Text = "一键加桥失败";
                //    warn.tbMsg.Text = errorMesssage;
                //    warn.WindowStartupLocation = warn.Owner.WindowStartupLocation;
                //    warn.Show();
                //}
                //else if (tuple.Item1 != null && tuple.Item1.Count > 0)
                //{
                //    string errorMesssage = $"内码{string.Join(",", tuple.Item1)}的成交单添加桥失败";
                //    if (warn != null)
                //    {
                //        warn.Close();
                //    }

                //    warn = new SysWarnGUI();
                //    //Window win = Application.Current.MainWindow;
                //    warn.Owner = mainWindow;
                //    warn.tbTitle.Text = "一键加桥部分订单失败";
                //    warn.tbMsg.Text = errorMesssage;
                //    warn.WindowStartupLocation = warn.Owner.WindowStartupLocation;
                //    warn.Show();
                //}
                //else
                //{
                //    LogHelper.Debug("一键加桥成功");
                //}
                loadingUI.Visibility = Visibility.Collapsed;
            }
            QueryLastRunResult(bridgeIndex);
        }


    }

    /// <summary>
    /// 桥信息
    /// </summary>
    public class BridgeInfoViewModel : INotifyPropertyChanged
    {
        private string bridgeNameShow = string.Empty;

        private string orderCountShow = string.Empty;

        private string usedLimitedShow = string.Empty;


        /// <summary>
        /// 桥机构Id
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        /// 桥机构下交易员Id
        /// </summary>
        public string TraderId { get; set; }


        /// <summary>
        /// 桥名称展示
        /// </summary>
        public string Item1
        {
            get { return bridgeNameShow; }
            set
            {
                bridgeNameShow = value;
                OnPropertyChanged("Item1");
            }
        }

        /// <summary>
        /// 额度展示
        /// </summary>
        public string Item2
        {
            get { return orderCountShow; }
            set
            {
                orderCountShow = value;
                OnPropertyChanged("Item2");
            }
        }


        public string UsedLimitedShow;
        //{
        //    get => usedLimitedShow;
        //    set => SetProperty(ref usedLimitedShow, value, "UsedLimitedShow");
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// 桥订单信息
    /// </summary>
    public class BridgeOrderInfoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// 是否选中订单
        /// </summary>
        private bool isSelectedFlag;

        /// <summary>
        /// 订单列表
        /// </summary>
        private string orderSeq = string.Empty;

        /// <summary>
        /// 第一个broker 的名字
        /// </summary>
        private string brokerAName = string.Empty;

        /// <summary>
        /// 第二个broker 的名字
        /// </summary>
        private string brokerBName = string.Empty;

        /// <summary>
        /// 订单内容
        /// </summary>
        private string orderText = string.Empty;

        /// <summary>
        /// 执行订单Id
        /// </summary>
        public string ExecOrderId { get; set; }


        /// <summary>
        /// 是否选中订单
        /// </summary>
        public bool IsSelectedFlag
        {
            get { return isSelectedFlag; }
            set
            {
                isSelectedFlag = value;
                OnPropertyChanged("IsSelectedFlag");
            }
        }

        /// <summary>
        /// 订单列表
        /// </summary>
        public string OrderSeq
        {
            get { return orderSeq; }
            set
            {
                orderSeq = value;
                OnPropertyChanged("OrderSeq");
            }
        }

        /// <summary>
        /// 第一个broker 的名字
        /// </summary>
        public string BrokerAName
        {
            get { return brokerAName; }
            set
            {
                brokerAName = value;
                OnPropertyChanged("BrokerAName");
            }
        }

        /// <summary>
        /// 第二个broker 的名字
        /// </summary>
        public string BrokerBName
        {
            get { return brokerBName; }
            set
            {
                brokerBName = value;
                OnPropertyChanged("BrokerBName");
            }
        }

        /// <summary>
        /// 订单文本
        /// </summary>
        public string OrderText
        {
            get { return orderText; }
            set
            {
                orderText = value;
                OnPropertyChanged("OrderText");
            }
        }


    }
}
