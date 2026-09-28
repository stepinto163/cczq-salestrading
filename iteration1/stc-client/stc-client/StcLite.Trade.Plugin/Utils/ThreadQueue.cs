using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Utils
{
    public class ThreadQueue<T> : IDisposable
    {
        protected Action<T> taskActioin;
        protected Queue<T> queue;
        protected Thread queueThread;
        private volatile bool runnableFlag;
        protected ReaderWriterLock rwl;
        private volatile bool sleppFlag;

        public ThreadQueue(Action<T> onDequeue)
        {
            this.queue = new Queue<T>();
            this.rwl = new ReaderWriterLock();
            this.runnableFlag = true;
            this.sleppFlag = true;
            this.taskActioin = onDequeue;
            this.queueThread = new Thread(new ThreadStart(this.DoWork));
            this.queueThread.IsBackground = true;
            this.runnableFlag = true;
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
                    T local = default(T);
                    try
                    {
                        this.rwl.AcquireWriterLock(-1);
                        local = this.queue.Dequeue();
                        this.rwl.ReleaseWriterLock();
                        this.taskActioin(local);
                    }
                    catch (InvalidOperationException)
                    {
                        try
                        {
                            this.rwl.ReleaseWriterLock();
                        }
                        finally
                        {
                            this.sleppFlag = true;
                        }
                    }
                }
                catch (ApplicationException ex)
                {
                    // 如果线程具有读取器锁或没有锁，则调用 ReleaseWriterLock 将引发 ApplicationException 。
                }
                while (this.sleppFlag)
                {
                    Thread.Sleep(10);
                }
            }
        }

        public void Enqueue(T data)
        {
            try
            {
                this.rwl.AcquireWriterLock(-1);
                this.queue.Enqueue(data);
            }
            finally
            {
                this.rwl.ReleaseWriterLock();
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
                this.rwl.AcquireWriterLock(-1);
                this.queue.Clear();
            }
            finally
            {
                this.rwl.ReleaseWriterLock();
            }
        }
    }
}
