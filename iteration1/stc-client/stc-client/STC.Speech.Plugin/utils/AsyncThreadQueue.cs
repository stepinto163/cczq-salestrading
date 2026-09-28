using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Speech.Plugin
{
    public class AsyncThreadQueue<T> : IDisposable
    {

        protected Action<T> taskActioin;
        protected ConcurrentQueue<T> queue;
        protected Thread queueThread;
        private bool runnableFlag = true;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        public AsyncThreadQueue(Action<T> onDequeue)
        {
            this.queue = new ConcurrentQueue<T>();
            this.taskActioin = onDequeue;
            this.queueThread = new Thread(new ThreadStart(this.DoWork));
            this.queueThread.IsBackground = true;
            this.queueThread.Start();
        }

        public int count()
        {
            return this.queue.Count;
        }

        private void DoWork()
        {   
            while (this.runnableFlag)
            {
             
                try
                {
                    T local = default(T);
                    if (queue.TryDequeue(out local))
                    {
                        taskActioin(local);
                       /* ClsMCI cls = new ClsMCI();
                        cls.FileName = local.ToString();
                        cls.play();*/
                    }  
                    else
                        _autoResetEvent.WaitOne();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
              
            }
        }

     

        public void Enqueue(T data)
        {
            queue.Enqueue(data);

            _autoResetEvent.Set();
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
            while (queue.Count > 0)
            {
                try
                {
                    T local = default(T);
                    queue.TryDequeue(out local);
                }
                catch (Exception e)
                {
                }
            }
        }


        public void Dispose()
        {
            this.runnableFlag = false;
        }
















        //protected Action<T> taskActioin;
        //protected Queue<T> queue;
        //protected Thread queueThread;
        //private volatile bool runnableFlag;
        //protected ReaderWriterLock rwl;
        //private volatile bool sleppFlag;

        //public AsyncThreadQueue(Action<T> onDequeue)
        //{
        //    this.queue = new Queue<T>();
        //    this.rwl = new ReaderWriterLock();
        //    this.runnableFlag = true;
        //    this.sleppFlag = true;
        //    this.taskActioin = onDequeue;
        //    this.queueThread = new Thread(new ThreadStart(this.DoWork));
        //    this.queueThread.IsBackground = true;
        //    this.runnableFlag = true;
        //    this.queueThread.Start();
        //}

        //public void Dispose()
        //{
        //    this.sleppFlag = false;
        //    this.runnableFlag = false;
        //}

        //private void DoWork()
        //{
        //    while (this.runnableFlag)
        //    {
        //        T local = default(T);
        //        try
        //        {
        //            this.rwl.AcquireWriterLock(-1);
        //            local = this.queue.Dequeue();
        //            this.rwl.ReleaseWriterLock();
        //            this.taskActioin(local);
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            try
        //            {
        //                this.rwl.ReleaseWriterLock();
        //            }
        //            finally
        //            {
        //                this.sleppFlag = true;
        //            }
        //        }
        //        while (this.sleppFlag)
        //        {
        //            Thread.Sleep(10);
        //        }
        //    }
        //}

        //public void Enqueue(T data)
        //{
        //    try
        //    {
        //        this.rwl.AcquireWriterLock(-1);
        //        this.queue.Enqueue(data);
        //    }
        //    finally
        //    {
        //        this.rwl.ReleaseWriterLock();
        //    }
        //    this.sleppFlag = false;
        //}

        //public Action<T> Dequeue
        //{
        //    get
        //    {
        //        return this.taskActioin;
        //    }
        //}

        ///// <summary>
        ///// 清空集合
        ///// </summary>
        //public void Clear()
        //{
        //    try
        //    {
        //        this.rwl.AcquireWriterLock(-1);
        //        this.queue.Clear();
        //    }
        //    finally
        //    {
        //        this.rwl.ReleaseWriterLock();
        //    }
        //}
    }
}
