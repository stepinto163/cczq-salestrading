using STC.Common.Handler.Drp;
using STC.Common.Model.Drp;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System;

namespace STC.Common.ViewModel.Drp.Component
{
    public class DRPOrderLogViewModel : ViewModelBase
    {
        #region 界面属性
        private object _lockObj = new object();
        private Window _myWindow;

        private object Updatelock = new object();

        private ObservableCollection<DRPOrderLogModel> _gridDatas;

        /// <summary>
        /// 回价列表
        /// </summary>
        public ObservableCollection<DRPOrderLogModel> GridDatas
        {
            get
            {
                if (_gridDatas == null)
                    _gridDatas = new ObservableCollection<DRPOrderLogModel>();

                return _gridDatas;
            }

            set
            {
                if (value != null)
                {
                    _gridDatas = value;
                    this.OnPropertyChanged(nameof(GridDatas));
                }
            }
        }

        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }

        private DRPOrderLogModel selectDrp;
        public DRPOrderLogModel SelectDrp
        {
            get { return selectDrp; }
            set
            {
                if (value != null)
                {
                    selectDrp = value;
                    this.OnPropertyChanged(nameof(SelectDrp));
                }
            }
        }

        #endregion

        #region 构造函数
        public DRPOrderLogViewModel()
        {
            this.Mv.Source = GridDatas;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
        }

        #endregion

        /// <summary>
        /// 根据参数查询历史回价订单信息
        /// </summary>
        /// <param name="orderid">订单id</param>
        /// <param name="queryType">查询类型(1-报单方，2-询价、点价方)</param>
        public void Init(string orderid, string queryType)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                ThreadPoolHelper.InvokeThreadPool((o) =>
                {
                    bool NeedPrompt = false;
                    string errorMsg = string.Empty;
                    DRPQueryOrderLogReq req = new DRPQueryOrderLogReq();
                    req.orderid = orderid;
                    req.queryType = queryType;

                    DRPQueryOrderLogResult result = DrpHandler.Instance.QueryDqOrderLog(req);
                    if (result != null && result.Code == "0000")
                    {
                        UpdateData(result.Datas);
                        NeedPrompt = false;
                    }
                    else
                    {
                        NeedPrompt = true;
                        if (result != null && !string.IsNullOrEmpty(result.Msg))
                        {
                            errorMsg = result.Msg;
                        }
                        else
                        {
                            errorMsg = "服务器接收数据失败！";
                        }
                    }
                });

                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
            }));
        }

        private void UpdateData(List<DRPOrderLogModel> datas)
        {
            lock (Updatelock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {

                    foreach (DRPOrderLogModel pv in datas)
                    {
                        DRPOrderLogModel rf = GridDatas.FirstOrDefault(o => o.ID == pv.ID);
                        string tmpprice = string.IsNullOrEmpty(pv.bargainprice) ? "--" : pv.bargainprice;
                        pv.ShowPrice = tmpprice == "--" ? "--" : CommonUtils.FormatNum(tmpprice);
                        //1 BID BUY   2 Ofr SELL
                        //rf.ShowSide = (drd.order.side.ToUpper().Equals("BUY") || drd.order.side.Equals("1")) ? "1" : "2";
                        pv.ShowAmount = string.IsNullOrEmpty(pv.qty) ? "--" : pv.qty;

                        //if (!string.IsNullOrEmpty(pv.MsgCode))
                        //{
                        //    switch (pv.MsgCode)
                        //    {
                        //        case "11016": //回价成交
                        //            pv.CMsgCode = pv.orderstatus == "Partially_Filled" ? "部分成交" : "全部成交";
                        //            break;
                        //        case "11011": //议价拒绝
                        //            pv.CMsgCode = $"价格讨论";
                        //            break;
                        //        case "10010":
                        //        case "11012":
                        //        case "11013":
                        //        case "11015":
                        //            pv.CMsgCode = ComboSourceUtil.Instance.FindEnum(pv.MsgCode, "dqmsgcode");
                        //            break;
                        //        default:
                        //            pv.CMsgCode = "";
                        //            break;
                        //    }
                        //}
                        //else
                        //{
                            //操作类型（1-议价，2-成交，3-拒绝（忽略），4-撤单(询价方)，5-撤单(报价方)，6-撤单(超时)，7-报价
                            //N:已申请,0:处理中，1,2:已确认(其中1为部分确认,2完全确认)，4:已撤销，8:失败
                            //E(Broker B确认),F(Broker B撤销),G（Broker A确认），H（Broker A撤销）
                        if (pv.actiontype == "2")
                        {
                            pv.CMsgCode = pv.orderstatus == "Partially_Filled" ? "部分成交" : "全部成交";
                        }
                        else
                        {
                            pv.CMsgCode = ComboSourceUtil.Instance.FindEnum(pv.actiontype, "actiontype");
                        }
                        //}

                        if (rf != null)
                        {
                            rf = pv;
                        }
                        else
                        {
                            GridDatas.Add(pv);
                        }
                    }

                    this.Mv.View.Refresh();

                });
            }
        }

        public void Clear()
        {

        }
    }

}
