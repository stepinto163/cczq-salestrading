using com.gateway.connector.proto;
using com.gateway.log;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STC.Common.Dispatcher
{
    public class AsyncBase<T>
    {
        /// <summary>
        /// 安全线程队列
        /// </summary>
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //深度行情用
        private SemaphoreSlim semaphore = new SemaphoreSlim(0, int.MaxValue); // 计数信号量
        private ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private bool isStop = false;
        T proto;

        public void start()
        {
            lock (this)
            {
                isStop = false;
                //更新深度行情
                Task.Factory.StartNew(() => dealMsg(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
            }
        }

        private void dealMsg(CancellationToken cancellationToken)
        {
            while (!isStop)
            {
                try
                {
                    // 使用非阻塞轮询，减少线程切换开销
                    semaphore.Wait(cancellationToken); // 等待新数据到来

                    if (queue.TryDequeue(out proto))
                    {
                        process(proto);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }

        }
        public void dispose()
        {
            lock (this)
            {
                this.isStop = true;
            }
        }

        public virtual void process(T t)
        {
        }

        public void add(T t)
        {
            queue.Enqueue(t); // 将数据添加到队列中  
            semaphore.Release(); // 释放信号量，通知处理线程有新数据到来
            if (queue.Count > 100)
            {
                LogHelper.Warn("dispather[{0}] is critical[size:{1}]", this.GetType(), queue.Count);
            }
        }
    }
}