using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Speech.utils
{
    /// <summary>
    /// 异步队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
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


        /// <summary>
        /// 添加数据到队列
        /// </summary>
        /// <param name="data"></param>
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
    }
}
