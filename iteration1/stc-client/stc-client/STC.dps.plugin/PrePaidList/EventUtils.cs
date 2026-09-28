using com.gateway.log;
using STC.dps.plugin.Model.Fix.Message;
using STC.dps.plugin.PrePaidList.Event;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.dps.plugin.PrePaidList
{
	/// <summary>
	/// 信号工具类
	/// </summary>
	public class EventUtils
	{
		private static EventUtils server = new EventUtils();

		//private readonly object eventlock = new object();
		public static EventUtils instance { get { return server; } }
		//订单

		//public static readonly object htlock = new object();
		//public static readonly object eventlock = new object();
		//public static readonly object lisentenlock = new object();
		public void addStrategyOrder(ExecutionReport so)
		{
			lock (so)
			{
				string trigEvent = so.TrigEvent;
				string cancEvent = so.CancEvent;
				string userName = so.UserName;
				if (!string.IsNullOrWhiteSpace(trigEvent))
				{
					TrigEventListener tel = (TrigEventListener)so.getTrigEventListener();
					if (tel == null)
					{
						tel = new TrigEventListener(so);
					}
					addOrUpdateEvent(userName, "", trigEvent, tel);
				}
				if (!string.IsNullOrWhiteSpace(cancEvent))
				{
					CancEventListener tel = (CancEventListener)so.getCancEventListener();
					if (tel == null)
					{
						tel = new CancEventListener(so);
					}
					addOrUpdateEvent(userName, "", cancEvent, tel);
				}
			}
		}

		public void updateStrategyOrder(string oldTrigEvent, string oldCancEvent, ExecutionReport so)
		{
			lock (so)
			{
				string trigEvent = so.TrigEvent;
				string cancEvent = so.CancEvent;
				string userName = so.UserName;
				if (!string.IsNullOrWhiteSpace(trigEvent))
				{
					TrigEventListener tel = (TrigEventListener)so.getTrigEventListener();
					if (tel == null)
					{
						tel = new TrigEventListener(so);
					}
					addOrUpdateEvent(userName, oldTrigEvent, trigEvent, tel);
				}
				if (!string.IsNullOrWhiteSpace(cancEvent))
				{
					CancEventListener tel = (CancEventListener)so.getCancEventListener();
					if (tel == null)
					{
						tel = new CancEventListener(so);
					}
					addOrUpdateEvent(userName, oldCancEvent, cancEvent, tel);
				}
			}
		}

		public void removeStrategyOrder(ExecutionReport so)
		{
			lock (so)
			{
				string trigEvent = so.TrigEvent;
				string cancEvent = so.CancEvent;
				string userName = so.UserName;
				if (!string.IsNullOrWhiteSpace(trigEvent))
				{
					TrigEventListener tel = (TrigEventListener)so.getTrigEventListener();
					if (tel != null)
						removeEvent(userName, trigEvent, tel);
				}
				if (!string.IsNullOrWhiteSpace(cancEvent))
				{
					TrigEventListener tel = (TrigEventListener)so.getCancEventListener();
					if (tel != null)
						removeEvent(userName, cancEvent, tel);
				}
			}
		}

		public void addOrUpdateEvent(string userName, string oldEvent, string newEvent,
			IEventListener eventListener)
		{
			lock (eventListener.getOrderId())
			{
				if (!string.IsNullOrWhiteSpace(oldEvent))
				{
					List<string> eventLt = StringToListStr(oldEvent);
					foreach (string eventStr in eventLt)
					{
						string eventName = getEventName(userName, oldEvent);
						EventManager.instance.removeEventListener(eventName, eventListener);
					}
				}
				if (!string.IsNullOrWhiteSpace(newEvent))
				{
					List<string> eventLt = StringToListStr(newEvent);
					foreach (string eventStr in eventLt)
					{
						string eventName = getEventName(userName, newEvent);
						EventManager.instance.addEventListener(eventName, eventListener);
					}
				}
            }
        }

		public  void removeEvent(string userName, string oldEvent, IEventListener eventListener)
		{
            lock (eventListener.getOrderId())
            {
                if (!string.IsNullOrWhiteSpace(oldEvent))
                {
                    List<string> eventLt = StringToListStr(oldEvent);
                    foreach (string eventStr in eventLt)
                    {
                        string eventName = getEventName(userName, eventStr);
                        EventManager.instance.removeEventListener(eventName, eventListener);
                    }
                }
            }
        }

		public static string getEventName(string userName, string eventName)
		{
			//return string.Format("C.{0}.{1}", userName, eventName);
			return string.Format("C.{0}", eventName);
		}

		public static void onPublishOrder(STC.Common.Handler.PrePaidList.Event events)
		{
			EventManager.instance.pubEvent(events);
		}

		public static List<string> StringToListStr(string str)
		{
			List<string> lt = new List<string>();
			if (str != null)
			{
				string[] strs = str.Split(',');
				string[] var3 = strs;
				int var4 = strs.Length;

				for (int var5 = 0; var5 < var4; ++var5)
				{
					string s = var3[var5];
					if (!string.IsNullOrEmpty(str))
					{
						lt.Add(s);
					}
				}
			}
			return lt;
		}

		public static bool isDelete(ExecutionReport order)
		{
			bool flag = false;
			if (order.OrdStatus.Equals("4") || order.OrdStatus.Equals("2") || order.OrdStatus.Equals("8") || order.OrdStatus.Equals("C"))
			{
				flag = true;
			}

			return flag;
		}

	}
}
