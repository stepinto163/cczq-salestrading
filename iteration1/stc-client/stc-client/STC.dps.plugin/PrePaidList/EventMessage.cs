using com.gateway.connector;
using com.gateway.connector.proto;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Utils;
using STC.dps.plugin.Model.Fix.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.dps.plugin.PrePaidList
{
    public class EventMessage : IMessage
    {
        private AsyncThreadQueue<ExecutionReport> _ProcessEventThreadQueue;
        private ConcurrentDictionary<string, ExecutionReport> ht = new ConcurrentDictionary<string, ExecutionReport>();
        public EventMessage(Proto proto)
        {
            if (_ProcessEventThreadQueue == null)
            {
                _ProcessEventThreadQueue = new AsyncThreadQueue<ExecutionReport>(cacheMessage);
            }
        }
        public void onMessage(string topic, string t)
        {
            try
            {
                LogHelper.Info(".........topic:{0},{1}", topic, t);
                ExecutionReport order = JsonConvert.DeserializeObject<ExecutionReport>(t);
                if (this._ProcessEventThreadQueue != null)
                {
                    this._ProcessEventThreadQueue.Enqueue(order);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public void cacheMessage(ExecutionReport order)
        {
            try
            {

                lock (order)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        //EventUtils.Cache(order);
                        if (EventUtils.isDelete(order))
                        {
                            ExecutionReport value = null;
                            ht.TryRemove(order.OrderID, out value);
                            if (value != null)
                                EventUtils.instance.removeStrategyOrder(order);
                        }
                        else
                        {
                            if (ht.ContainsKey(order.OrderID))
                            {
                                if (order.InValidFlag != null && order.InValidFlag.Equals("1"))
                                {
                                    string trigEvent = order.TrigEvent;
                                    string cancEvent = order.CancEvent;

                                    ExecutionReport old = ht[order.OrderID];
                                    string oldTrigEvent = old.TrigEvent;
                                    string oldCancEvent = old.CancEvent;
                                    EventUtils.instance.updateStrategyOrder(oldTrigEvent, oldCancEvent, old);
                                }
                                ht.AddOrUpdate(order.OrderID, order, (key, value) => order);
                            }
                            else
                            {
                                EventUtils.instance.addStrategyOrder(order);
                                ht.AddOrUpdate(order.OrderID, order, (key, value) => order);
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

    }
}