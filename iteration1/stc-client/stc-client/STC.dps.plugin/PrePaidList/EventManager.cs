using com.gateway.connector.proto;
using com.gateway.connector.tcp;
using com.gateway.log;
using STC.Common.Utils;
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
	public class EventManager
	{
		private static EventManager server = new EventManager();

		//private static readonly ReaderWriterLockSlim LockSlim = new ReaderWriterLockSlim();
		static ReaderWriterLock LockSlim = new ReaderWriterLock();//读写锁，允许多个线程同时读，只允许一个线程写
		
		public static EventManager instance { get { return server; } }
		public string seesionid;
		public Proto msg;
		public GateWayApi GateWayApi { get; set; }

		private AsyncThreadQueue<STC.Common.Handler.PrePaidList.Event> _ProcessEventThreadQueue;
		public EventManager()
		{
			
			if (_ProcessEventThreadQueue == null)
			{
				_ProcessEventThreadQueue = new AsyncThreadQueue<STC.Common.Handler.PrePaidList.Event>(publish);
			}
		}
		
			~EventManager()
		{
			Dispose();
		}

		private ConcurrentDictionary<string, List<IEventListener>> eventHm = new ConcurrentDictionary<string, List<IEventListener>>();

		public void addEventListener(String eventname, IEventListener eventListener)
		{
			try
			{
				//LockSlim.EnterWriteLock();
				//获取一个 witerLock
				//LockSlim.AcquireWriterLock(TimeSpan.FromSeconds(100));
				lock (string.Intern(eventListener.getOrderId()))
				{
					if (!string.IsNullOrWhiteSpace(eventname) && eventListener != null)
					{
						List<IEventListener> cas = eventHm.ContainsKey(eventname) ? eventHm[eventname] : null;


						if (cas == null)
						{
							cas = new List<IEventListener>();
							eventHm.AddOrUpdate(eventname, cas, (key, value) => cas);
						}
						cas.Add(eventListener);
						LogHelper.Info("addEventListener:{0},{1}", eventname, eventListener.getOrderId());
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Error(ex);
			}
			finally
			{
				//LockSlim.ExitWriteLock();
				//判断当前线程是否持有 writerlock
				//if (LockSlim.IsWriterLockHeld)
				//{
				//	//释放writer lock
				//	LockSlim.ReleaseWriterLock();
				//}
			}
		}

		public void removeEventListener(string eventname, IEventListener eventListener)
		{
			try
			{
				//LockSlim.EnterWriteLock();
				//LockSlim.AcquireWriterLock(TimeSpan.FromSeconds(100));
				lock (string.Intern(eventListener.getOrderId()))
				{
					if (!string.IsNullOrWhiteSpace(eventname) && eventListener != null)
					{
						List<IEventListener> cas = eventHm.ContainsKey(eventname) ? eventHm[eventname] : null;//getEventHm(eventname);
						if (cas != null)
						{
							cas.Remove(eventListener);
							LogHelper.Info("removeEventListener:{0},{1}", eventname, eventListener.getOrderId());
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Error(ex);
			}
			finally
			{
				//LockSlim.ExitWriteLock();
				//判断当前线程是否持有 writerlock
				//if (LockSlim.IsWriterLockHeld)
				//{
				//	//释放writer lock
				//	LockSlim.ReleaseWriterLock();
				//}
			}
		}

		public List<IEventListener> getEventHm(string eventname)
		{
			List<IEventListener> cas = null;
			try
			{//获取一个readlock
				//LockSlim.AcquireReaderLock(TimeSpan.FromSeconds(30));
				//LockSlim.EnterReadLock();
				cas = eventHm.ContainsKey(eventname) ? eventHm[eventname] : null;
			}
			catch (Exception ex)
			{
				LogHelper.Error(ex);
			}
			finally
			{
				//LockSlim.ExitReadLock();
				//判断当前线程是否持有reader lock
				//if (LockSlim.IsReaderLockHeld)
				//{
				//	//释放readlock
				//	LockSlim.ReleaseReaderLock();
				//}
			}
			return cas;
		}

		public void pubEvent(STC.Common.Handler.PrePaidList.Event events)
		{
			try
			{
				if (this._ProcessEventThreadQueue != null)
				{
					this._ProcessEventThreadQueue.Enqueue(events);
				}

			}
			catch (Exception ex)
			{
				LogHelper.Error(ex);
			}
		}

		public void publish(STC.Common.Handler.PrePaidList.Event events)
		{
			try
			{
				lock (eventHm)
				{
					LogHelper.Info("pubEvent:{0},{1}", events.name, events);
					List<IEventListener> cas = getEventHm(events.name);
					List<IEventListener> newcas = new List<IEventListener>();
					if (cas != null)
					{
						foreach (IEventListener iEventListener in cas)
						{
							newcas.Add(iEventListener);
						}

						if (newcas != null)
						{
							ThreadPool.QueueUserWorkItem(o =>
							{
							//for (int i = 0; i < cas.Count; i++)
							//{
							foreach (IEventListener iEventListener in newcas)
								{
								//IEventListener iEventListener = cas[i];
								LogHelper.Info("onEvent:{0},{1}", events.name, iEventListener.getOrderId());
									iEventListener.onEvent(events);
								}
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Error(ex);
			}
		}

		public void Dispose()
		{
			if (_ProcessEventThreadQueue != null)
			{
				_ProcessEventThreadQueue.Clear();
				_ProcessEventThreadQueue.Dispose();
				_ProcessEventThreadQueue = null;
			}
		}
	}
}
