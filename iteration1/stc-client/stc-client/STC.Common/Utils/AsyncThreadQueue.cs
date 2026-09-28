using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    /// <summary>
    /// 异步消息队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncThreadQueue<T> : IDisposable
    {
        protected Action<T> taskActioin;
        protected ConcurrentQueue<T> queue;
        protected Thread queueThread;
        private volatile bool runnableFlag;
        protected ReaderWriterLockSlim rwl;
        private volatile bool sleppFlag;
        private int millisecond = 10;

        public AsyncThreadQueue(Action<T> onDequeue)
        {
            this.queue = new ConcurrentQueue<T>();
            this.rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            this.runnableFlag = true;
            this.sleppFlag = true;
            this.taskActioin = onDequeue;
            this.queueThread = new Thread(new ThreadStart(this.DoWork));
            this.queueThread.IsBackground = true;
            this.runnableFlag = true;
            this.queueThread.Start();
        }

        public AsyncThreadQueue(Action<T> onDequeue, int millisecond)
        {
            this.queue = new ConcurrentQueue<T>();
            this.rwl = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            this.runnableFlag = true;
            this.sleppFlag = true;
            this.taskActioin = onDequeue;
            this.queueThread = new Thread(new ThreadStart(this.DoWork));
            this.queueThread.IsBackground = true;
            this.runnableFlag = true;
            this.millisecond = millisecond;
            this.queueThread.Start();
        }

        public void Dispose()
        {
            this.sleppFlag = false;
            this.runnableFlag = false;
        }

        private void DoWork()
        {
            while (this.runnableFlag)
            {
                try
                {
                    
                    if (!this.queue.IsEmpty)
                    {
                        T local;
                        this.rwl.EnterWriteLock();
                        bool flag = this.queue.TryDequeue(out local);
                        this.rwl.ExitWriteLock();
                        if (flag)
                        {
                            this.taskActioin(local);
                        }
                    }
                    else
                    {
                        this.sleppFlag = true;
                    }
                }
                catch (ApplicationException ex)
                {
                    // 如果线程具有读取器锁或没有锁，则调用 ReleaseWriterLock 将引发 ApplicationException 。
                }
                while (this.sleppFlag)
                {
                    Thread.Sleep(millisecond);
                }
            }
        }

        public void Enqueue(T data)
        {
            try
            {
                this.rwl.EnterWriteLock();
                this.queue.Enqueue(data);
            }
            finally
            {
                this.rwl.ExitWriteLock();
            }
            this.sleppFlag = false;
        }

        public Action<T> Dequeue
        {
            get
            {
                return this.taskActioin;
            }
        }

        /// <summary>
        /// 清空集合
        /// </summary>
        public void Clear()
        {
            try
            {
                this.rwl.EnterWriteLock();
                T result;
                while (queue.TryDequeue(out result)) ;
            }
            finally
            {
                this.rwl.ExitWriteLock();
            }
        }
    }
}
