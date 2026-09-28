/*************************************************************************************
 *
 * 文 件 名： EventAggregator.cs
 * 描    述： 消息总线封装类，基于 CommunityToolkit.Mvvm 的 WeakReferenceMessenger
 *            实现轻量级事件聚合器，供各插件业务解耦复用。
 *            与同目录 EventType&lt;T&gt;/EventHub 的区别：本类使用弱引用 + recipient 维度
 *            退订，不会因漏退订导致订阅者被永久强引用（内存泄漏）。
 * ======================================
 * 历史更新记录
 * 修改时间：         修改人：
 * 修改内容：
 * ======================================
 *************************************************************************************/

using System;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;

namespace STC.Common.Utils.Events
{
    /// <summary>
    /// 基于 WeakReferenceMessenger 的轻量级事件总线封装。
    /// 消息类型建议放在各业务模块的 Messages 契约目录下集中定义（纯 POCO，无基类要求）。
    /// </summary>
    public static class EventAggregator
    {
        #region Fields

        /// <summary>独立实例，不与 Toolkit 的 Default 实例混用，避免与宿主/其他库的消息域互相干扰。</summary>
        private static readonly WeakReferenceMessenger Messenger = new WeakReferenceMessenger();

        #endregion

        #region Publish

        /// <summary>
        /// 发布一条消息。所有订阅了该消息类型的接收者都会收到通知。
        /// 注意：派发是同步的，且在发布线程上执行。
        /// </summary>
        /// <typeparam name="TMessage">要发布的消息类型。</typeparam>
        /// <param name="message">要发送的消息实例。</param>
        public static void Publish<TMessage>(TMessage message) where TMessage : class
        {
            Messenger.Send(message);
        }

        /// <summary>
        /// 发布一条携带 token 的消息。仅订阅了相同 token 的接收者会收到通知。
        /// </summary>
        public static void Publish<TMessage, TToken>(TMessage message, TToken token)
            where TMessage : class
            where TToken : IEquatable<TToken>
        {
            Messenger.Send(message, token);
        }

        /// <summary>
        /// 在 UI 线程上发布消息。行情推送等后台线程可直接调用：
        /// 已处于 UI 线程时同步派发，否则经 Dispatcher 异步投递到 UI 线程。
        /// </summary>
        public static void PublishOnUi<TMessage>(TMessage message) where TMessage : class
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                Messenger.Send(message);
                return;
            }

            dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Messenger.Send(message)));
        }

        #endregion

        #region Subscribe

        /// <summary>
        /// 订阅一条消息。订阅使用弱引用记录：订阅者被 GC 回收后订阅自动失效，
        /// 但仍建议在 ViewModel 释放时显式取消订阅，避免回调在回收前仍被触发。
        /// </summary>
        /// <typeparam name="TMessage">要订阅的消息类型。</typeparam>
        /// <param name="recipient">订阅者实例（通常是 'this'），用于取消订阅时识别。</param>
        /// <param name="action">收到消息后要执行的回调方法。</param>
        public static void Subscribe<TMessage>(object recipient, Action<TMessage> action) where TMessage : class
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));
            if (action == null) throw new ArgumentNullException(nameof(action));
            Messenger.Register<TMessage>(recipient, (r, m) => action(m));
        }

        /// <summary>
        /// 订阅指定 token 的消息。仅接收使用相同 token 发送的消息。
        /// token 类型须可比较（IEquatable&lt;TToken&gt;），常用 string/枚举/值类型。
        /// </summary>
        /// <typeparam name="TMessage">要订阅的消息类型。</typeparam>
        /// <typeparam name="TToken">令牌类型。</typeparam>
        /// <param name="recipient">订阅者实例。</param>
        /// <param name="token">用于精确接收消息的令牌。</param>
        /// <param name="action">收到消息后要执行的回调方法。</param>
        public static void Subscribe<TMessage, TToken>(object recipient, TToken token, Action<TMessage> action)
            where TMessage : class
            where TToken : IEquatable<TToken>
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (action == null) throw new ArgumentNullException(nameof(action));
            Messenger.Register<TMessage, TToken>(recipient, token, (r, m) => action(m));
        }

        /// <summary>
        /// 注册实现了 IRecipient&lt;TMessage&gt; 的订阅者（ObservableRecipient 派生类常用），
        /// 由订阅者自身按消息类型分发处理。退订使用 <see cref="Unsubscribe(object)"/>。
        /// </summary>
        /// <param name="recipient">订阅者实例。</param>
        public static void RegisterAll(object recipient)
        {
            Messenger.RegisterAll(recipient);
        }

        #endregion

        #region Unsubscribe

        /// <summary>
        /// 取消订阅者对全部消息类型的订阅。
        /// </summary>
        /// <param name="recipient">要取消订阅的订阅者实例。</param>
        public static void Unsubscribe(object recipient)
        {
            Messenger.UnregisterAll(recipient);
        }

        /// <summary>
        /// 取消订阅者对特定类型消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">要取消订阅的消息类型。</typeparam>
        /// <param name="recipient">要取消订阅的订阅者实例。</param>
        public static void Unsubscribe<TMessage>(object recipient) where TMessage : class
        {
            Messenger.Unregister<TMessage>(recipient);
        }

        /// <summary>
        /// 取消订阅者对特定令牌消息的订阅。
        /// </summary>
        /// <typeparam name="TMessage">要取消订阅的消息类型。</typeparam>
        /// <typeparam name="TToken">令牌类型。</typeparam>
        /// <param name="recipient">要取消订阅的订阅者实例。</param>
        /// <param name="token">与订阅时使用的令牌一致。</param>
        public static void Unsubscribe<TMessage, TToken>(object recipient, TToken token)
            where TMessage : class
            where TToken : IEquatable<TToken>
        {
            Messenger.Unregister<TMessage, TToken>(recipient, token);
        }

        #endregion
    }
}
