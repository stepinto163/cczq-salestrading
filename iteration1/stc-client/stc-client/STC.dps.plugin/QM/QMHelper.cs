using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.dps.plugin.Common;
using STC.dps.plugin.Handler;
using STC.dps.plugin.QM.po;
using STC.dps.plugin.Utils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace STC.dps.plugin.QM
{
    public class QMHelper
    {
        private static QMHelper _qmHelper = new QMHelper();
        public static QMHelper Instance { get { return _qmHelper; } }
        public static ConcurrentDictionary<string, int> userDic = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, int> userKeyDic = new ConcurrentDictionary<string, int>();
        public static string UserName;
        public static BrokerAccount BrokerAccount;
        private AsyncThreadQueue<string> _ProcessMyOrdersThreadQueue;
        private string QM = "@idb_broker07:qa.sumscope.tptest.com";
        public QMHelper()
        {
            if (_ProcessMyOrdersThreadQueue == null)
            {
                _ProcessMyOrdersThreadQueue = new AsyncThreadQueue<string>(Publish);
            }
        }
        public void onPublish(string order)
        {
            try
            {
                if (_ProcessMyOrdersThreadQueue != null)
                {
                    _ProcessMyOrdersThreadQueue.Enqueue(order);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void Publish(string t)
        {
            ExecutionReport ex = JsonConvert.DeserializeObject<ExecutionReport>(t);
            if (Constants.VenueTypeGW.Equals(ex.VenueTypeGW))
            {
                if ("N".Equals(ex.ExecType))//新增/修改
                    AddOrUpdateOnQMEMS(ex);
                else if ("4".Equals(ex.ExecType))
                    CancelOnQMEMS(ex);//撤销
            }
        }

        public void CancelOnQMEMS(ExecutionReport po)
        {
            try
            {
                string key = po.SecurityID + po.Side + po.Venues;
                string SecurityID = po.SecurityID.Replace(".IB", "");
                string text = string.Format("refer {0} {1} ({2})", SecurityID, getSide(po.Side, po.TimeInForce), po.Info2);
                BrokerAccount broker = new BrokerAccount();//BaseHandler.Instance.IsContainBroker(po.Venues, po.SecurityType);
                broker.qm = QM;
                if (broker != null)
                    StartQM(broker, text);
                else
                    LogHelper.Warn("broker is not exit:" + po.Venues + po.SecurityType);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                OrdersHandler.Instance.OnMessage(po);
            }
        }

        public void AddOrUpdateOnQMEMS(ExecutionReport newOrder)
        {
            try
            {
                string text = "";
                string key = newOrder.SecurityID + newOrder.Side + newOrder.Venues;
                QMPo po = OrdersHandler.Instance.IsContainEMS(key, newOrder);
                string Ispoint = newOrder.TimeInForce == "0" ? "0" : "1";
                string Side = getSide(newOrder.Side, Ispoint);
                string SettlType = getSettlType(newOrder.SettlType);
                string SecurityID = newOrder.SecurityID.Replace(".IB", "");
                if ("0".Equals(Ispoint))
                {
                    switch (po.action)
                    {
                        case 0:
                            if ("".Equals(SettlType))
                                text = string.Format("{0} {1} {2} {3} ({4})", SecurityID, newOrder.Price, Side, getOrderQty(newOrder.OrderQty), newOrder.Info2);
                            else
                                text = string.Format("{0} {1} {2} {3} {4} ({5})", SecurityID, newOrder.Price, Side, getOrderQty(newOrder.OrderQty), SettlType, newOrder.Info2);
                            break;
                        case 1:
                            text = string.Format("{0} ", SecurityID);
                            text += setChangeEMS(newOrder, po, Side, SettlType);
                            break;
                    }
                }
                else
                {
                    text = string.Format("{0} {1} {2} {3} ({4})", SecurityID, Side, newOrder.Price, newOrder.OrderQty, newOrder.Info2);
                }
                BrokerAccount broker = new BrokerAccount();//BaseHandler.Instance.IsContainBroker(newOrder.Venues, newOrder.SecurityType);
                broker.qm = QM;
                if (broker != null)
                    StartQM(broker, text);
                else
                    LogHelper.Warn("broker is not exit:" + newOrder.Venues + newOrder.SecurityType);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                OrdersHandler.Instance.OnMessage(newOrder);
            }
        }


        private string setChangeEMS(ExecutionReport newV, QMPo oldPo, string Side, string SettlType)
        {
            ExecutionReport oldV = oldPo.ExecutionReport;
            StringBuilder sb = new StringBuilder();
            if (!checkValNumber(newV.Price, oldV.Price))
            {
                sb.Append(oldV.Price + " 改到 " + newV.Price);
                sb.Append(",");
            }
            else
                sb.Append(newV.Price + " " + Side + " ");
            if (!checkValNumber(newV.OrderQty, oldV.OrderQty))
            {
                sb.Append(getOrderQty(oldV.OrderQty) + " 改到 " + getOrderQty(newV.OrderQty));
                sb.Append(",");
            }
            else
                sb.Append(newV.OrderQty + " ");
            if (!checkVal(newV.SettlType, oldV.SettlType))
            {
                sb.Append(getSettlType(oldV.SettlType) + " 改到 " + SettlType);
                sb.Append(",");
            }
            else
                sb.Append(getSettlType(newV.SettlType) + " ");
            if (!checkVal(newV.Info2, oldV.Info2))
            {
                sb.Append("(" + oldV.Info2 + " 改到 " + newV.Info2 + ")");
            }
            else
                sb.Append("(" + newV.Info2 + ")");
            return sb.ToString();
        }

        private string getOrderQty(string OrderQty)
        {
            string qty = "";
            if (OrderQty.Length < 5)
                qty = OrderQty + "W";
            else
            {
                double s = CommonUtil.ConvertToDouble(OrderQty) / 10000;
                qty = s + "E";
            }
            return qty;
        }

        private bool checkValNumber(string oldVal, string newVal)
        {
            double oldV = CommonUtil.ConvertToDouble(oldVal);
            double newV = CommonUtil.ConvertToDouble(newVal);
            return oldV.Equals(newV);
        }

        private bool checkVal(string oldVal, string newVal)
        {
            // return oldVal == newVal;
            return oldVal.Equals(newVal);
        }

        public void StartQM(BrokerAccount broker, string content)
        {
            try
            {
                string[] arg = new string[1];
                //string exe = string.Format("{0}{1}", Environment.CurrentDirectory, "\\win-unpacked\\qmessages.exe");                                                                                                                                                                                      
                //string param = string.Format("qmId=@00c_10:qa.sumscope.cloud.com&msg=\"{0}\"&isSendDirectly=true", content);
                //DateTime begin = DateTime.Now;
                string param = string.Format("qmId={0}&msg=\"{1}\"&isSendDirectly=true&brokerName={2}", broker.qm, content, broker.userNm);
                LogHelper.Info("sendQm:" + content);
                ProcessStartInfo startInfo = new ProcessStartInfo(Utils.ProcessWatcher.QmProcess.MainModule.FileName);
                startInfo.CreateNoWindow = false;
                startInfo.Arguments = param;
                startInfo.UseShellExecute = true;

                Utils.ProcessWatcher.QmProcess.StartInfo = startInfo;
                Utils.ProcessWatcher.QmProcess.Start();
                //DateTime end = DateTime.Now;
                //TimeSpan ts = end - begin;
                // Console.WriteLine(i + "发送消息耗时:" + ts.TotalMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private string getSide(string side, string Ispoint)
        {
            string Side = "";
            switch (side)
            {
                case "1":
                    Side = "Bid";
                    if ("1".Equals(Ispoint))
                        Side = "GVN";
                    break;
                case "2":
                    Side = "Ofr";
                    if ("1".Equals(Ispoint))
                        Side = "TKN";
                    break;
                case "X":
                    Side = "GVN";
                    break;
                case "Y":
                    Side = "TKN";
                    break;
                case "Z":
                    Side = "TRD";
                    break;
            }
            return Side;
        }

        private string getSettlType(string ex)
        {
            string SettlType = "";
            switch (ex)
            {
                case "1":
                    SettlType = "T+0";
                    break;
                case "2":
                    SettlType = "T+1(今+1)";
                    break;
                case "3":
                    SettlType = "T+1(明+0)";
                    break;
                case "4":
                    SettlType = "明天+1";
                    break;
                case "2,3":
                    SettlType = "";//"T+1(不限)";
                    break;
            }
            return SettlType;
        }

    }
}