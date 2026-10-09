using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace STC.SalesTrading.Plugin.Ficc
{
    /// <summary>
    /// 复用 STC 宿主已登录的 IApi，封装 DPS 请求、订阅、日志和 UI 上下文回调。
    /// 插件不重新登录，也不主动断开宿主共享连接；释放时仅清理本适配器持有的订阅。
    /// </summary>
    public sealed class StcFiccTransport : IFiccTransport, IDisposable
    {
        private readonly IApi api;
        private readonly SynchronizationContext context;
        private readonly Dictionary<string, TopicSubscription> subscriptions = new Dictionary<string, TopicSubscription>();
        private bool disposed;

        public event Action ConnectionChanged;
        public event Action<string> MessageLogged;
        public bool LogHeartbeat { get; set; }

        public StcFiccTransport(IApi authenticatedApi, SynchronizationContext uiContext)
        {
            api = authenticatedApi ?? throw new ArgumentNullException(nameof(authenticatedApi));
            context = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
            api.SubScribeConStatus(OnConnectionStatus);
        }

        public async Task<JObject> RequestAsync(string service, string topic, JObject request)
        {
            ThrowIfDisposed();
            JObject body = (JObject)request.DeepClone();
            Log("REQ", topic, body);
            try
            {
                JObject reply = await Task.Run(() => api.Request<JObject>(service, topic, body, 5000));
                if (reply == null)
                {
                    throw new InvalidOperationException("服务未返回应答：" + topic);
                }
                Log("REP", topic, reply);
                return reply;
            }
            catch (Exception error)
            {
                LogHelper.Error(error);
                throw;
            }
        }

        public IDisposable Subscribe(string service, string topic, Action<JObject> receive, bool withImage = false)
        {
            ThrowIfDisposed();
            if (!subscriptions.TryGetValue(topic, out TopicSubscription subscription))
            {
                subscription = new TopicSubscription(service, topic);
                subscriptions.Add(topic, subscription);
                subscription.Receivers.Add(receive);
                try
                {
                    // One SDK subscription per topic; GWApi otherwise ignores subsequent callbacks.
                    if (withImage)
                    {
                        api.SubScribeWithImage(message => Receive(topic, message), new[] { topic }, service, "");
                    }
                    else
                    {
                        api.SubScribe(message => Receive(topic, message), new[] { topic }, service, "");
                    }
                    Log("SUB", topic, new JObject { ["service"] = service });
                }
                catch
                {
                    subscriptions.Remove(topic);
                    throw;
                }
            }
            else
            {
                if (subscription.Service != service)
                {
                    throw new InvalidOperationException("同一主题不能绑定不同服务。");
                }
                subscription.Receivers.Add(receive);
            }
            return new SubscriptionLease(() => Remove(topic, receive));
        }

        private void Receive(string topic, object message)
        {
            try
            {
                NetMessage envelope = message as NetMessage;
                if (envelope == null)
                {
                    throw new InvalidOperationException("无法识别的 STC 推送封装。");
                }
                JObject body = JObject.Parse(envelope.Data);
                if (LogHeartbeat || (string)body["kind"] != "HEARTBEAT")
                {
                    Log("PUSH", topic, body);
                }
                context.Post(_ =>
                {
                    if (disposed || !subscriptions.TryGetValue(topic, out TopicSubscription subscription))
                    {
                        return;
                    }
                    foreach (Action<JObject> receiver in subscription.Receivers.ToArray())
                    {
                        try
                        {
                            receiver((JObject)body.DeepClone());
                        }
                        catch (Exception error)
                        {
                            LogHelper.Error(error);
                        }
                    }
                }, null);
            }
            catch (Exception error)
            {
                LogHelper.Error(error);
            }
        }

        private void Remove(string topic, Action<JObject> receiver)
        {
            if (!subscriptions.TryGetValue(topic, out TopicSubscription subscription))
            {
                return;
            }
            subscription.Receivers.Remove(receiver);
            if (subscription.Receivers.Count == 0)
            {
                api.UnSubScribe(subscription.Service, new[] { topic });
                subscriptions.Remove(topic);
                Log("UNSUB", topic, new JObject());
            }
        }

        private void OnConnectionStatus(object status)
        {
            context.Post(_ =>
            {
                if (!disposed)
                {
                    ConnectionChanged?.Invoke();
                }
            }, null);
        }

        private void Log(string direction, string topic, JObject body)
        {
            string line = direction + " " + topic + " " + body.ToString(Formatting.None);
            LogHelper.Info(line);
            context.Post(_ => MessageLogged?.Invoke(line), null);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(StcFiccTransport));
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            api.UnSubScribeConStatus(OnConnectionStatus);
            foreach (TopicSubscription subscription in subscriptions.Values.ToArray())
            {
                try
                {
                    api.UnSubScribe(subscription.Service, new[] { subscription.Topic });
                }
                catch (Exception error)
                {
                    LogHelper.Error(error);
                }
            }
            subscriptions.Clear();
        }

        private sealed class TopicSubscription
        {
            internal readonly string Service;
            internal readonly string Topic;
            internal readonly List<Action<JObject>> Receivers = new List<Action<JObject>>();

            internal TopicSubscription(string service, string topic)
            {
                Service = service;
                Topic = topic;
            }
        }

        private sealed class SubscriptionLease : IDisposable
        {
            private Action release;

            internal SubscriptionLease(Action release)
            {
                this.release = release;
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref release, null)?.Invoke();
            }
        }
    }
}
