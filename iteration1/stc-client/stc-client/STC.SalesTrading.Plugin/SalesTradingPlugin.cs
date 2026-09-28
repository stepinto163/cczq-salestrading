using com.gateway.log;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.SalesTrading.Plugin.Shell;
using STC.SalesTrading.Plugin.Shell.View;
using STC.SalesTrading.Plugin.Shared.Messages;
using System;
using System.Configuration;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin
{
    /// <summary>
    /// 销交平台插件（单例插件：点击菜单开 tab，关闭 tab 释放 VM）。
    /// </summary>
    public class SalesTradingPlugin : AbstractPlugin
    {
        /// <summary>日志前缀,便于在宿主共享日志中检索本插件记录。</summary>
        private const string LogPrefix = "[销交平台] ";

        #region 插件信息

        private int _tag = 21;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.SalesTradingPlugin; }
        }

        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        protected new bool isMain = false;
        public override bool IsMain
        {
            get { return isMain; }
            set { isMain = value; }
        }

        // 单例插件:必须把属性接回字段。若用自动属性,IsAlone 恒为 false,
        // 宿主 LogoButton 会在打开 tab 后立刻把 Enabled 打回 false,
        // 导致 EnablePlugin 的门重新打开、二次点击再进 load(),可开出重复 tab。
        protected new bool isAlone = true;
        public override bool IsAlone
        {
            get { return isAlone; }
            set { isAlone = value; }
        }

        private bool isFullScreen = true;
        public override bool IsFullScreen
        {
            get { return isFullScreen; }
            set { isFullScreen = value; }
        }

        public override string PluginName
        {
            get { return "销交平台插件"; }
        }

        public override string Description
        {
            get { return "销交平台"; }
        }

        public override string Author
        {
            get { return ""; }
        }

        #endregion

        #region load,unload

        private bool _isFirst = true;

        /// <summary>tab 关闭事件句柄。缓存以避免重复订阅(EventType.Subscribe 无去重)。</summary>
        private NotifyCloseTabItem _notifyCloseTabItem;

        protected override void load()
        {
            //第一次菜单加载
            if (_isFirst)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_jiaoyibao.png", Description, PluginKey, "17");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_jiaoyibao.png", Description, PluginKey, "17");
                }

                _isFirst = false;
                enabled = false;
                Application.Subscribe(Consts.Topic_RestartApp, this);
                LogHelper.Info(LogPrefix + "菜单注册完成, PluginKey=" + PluginKey);
                if (netapi.Api.LocalIntegrationProfile.Current.Enabled)
                {
                    load();
                }
                return;
            }

            // 已打开则仅激活,不重复构造 VM。
            // OpenTabPage 命中同名 tab 时直接 return 并丢弃传入的 View(TabPageHelper),
            // 若无此守卫,每次重入都会白建一整棵 MainViewModel 对象图。
            if (TabPageHelper.IsExistTabItem(this.Description))
                return;

            var mainView = new MainView { DataContext = new MainViewModel() };
            TabPageHelper.OpenTabPage(this.Description, mainView, PluginKey);
            InstallTraceLogger();

            Application.Subscribe(Consts.Topic_Clear, this);

            // tab 关闭时释放 VM。EventType<T>.Subscribe 无去重且全仓库无人 UnSubscribe,
            // 必须靠此空引用判断保证只订阅一次,否则回调会被永久重复触发。
            if (_notifyCloseTabItem == null)
            {
                _notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
                _notifyCloseTabItem.Subscribe(OnCloseTabItem);
            }

            LogHelper.Info(LogPrefix + "页面已打开, PluginKey=" + PluginKey);
            base.load();
        }

        /// <summary>
        /// tab 关闭回调。宿主在 Window1.OnTabItemClose 中广播被关闭 TabItem 的 HashCode,
        /// 这里按 HashCode 定位自己的页面并释放 ViewModel 持有的资源(服务/消息订阅)。
        /// </summary>
        private void OnCloseTabItem(int tabItemHashCode)
        {
            try
            {
                if (Application?.Panel?.Items == null) return;
                foreach (TabItem tabItem in Application.Panel.Items)
                {
                    if (tabItem == null || tabItem.GetHashCode() != tabItemHashCode) continue;
                    if (!(tabItem.Content is MainView view)) continue;

                    (view.DataContext as IDisposable)?.Dispose();
                    LogHelper.Debug(LogPrefix + "页面已关闭并释放: {0}", tabItem.Header);
                    break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(LogPrefix + "tab 关闭释放失败", ex);
            }
        }

        public override void BeforeTerminating()
        {
        }

        #endregion

        #region 诊断日志(捕获被 WPF 静默吞掉的 XAML/资源异常,定位"页面打不开")

        private static bool _traceInstalled;

        /// <summary>
        /// 注册 FirstChanceException 钩子,把 XAML 解析/资源缺失类异常经 LogHelper 落盘。
        /// 这是全 AppDomain 钩子,会拦到宿主与兄弟插件的所有异常,故默认关闭。
        /// 排查时在 STC.exe.config 加 EnableSalesTradingTrace=1 打开。
        /// </summary>
        private static void InstallTraceLogger()
        {
            if (_traceInstalled) return;

            try
            {
                if (ConfigurationManager.AppSettings["EnableSalesTradingTrace"] != "1")
                    return;

                _traceInstalled = true;
                LogHelper.Info(LogPrefix + "XAML 诊断钩子已启用(EnableSalesTradingTrace=1)");

                AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
                {
                    var ex = e.Exception;
                    var t = ex?.GetType();
                    if (t?.FullName == null) return;

                    bool want = t == typeof(System.Windows.Markup.XamlParseException)
                                || t == typeof(System.Windows.ResourceReferenceKeyNotFoundException)
                                || (t == typeof(InvalidOperationException) && ex.Message != null
                                    && (ex.Message.Contains("Resource") || ex.Message.Contains("元素")))
                                || t.FullName.Contains("XamlException");
                    if (!want) return;

                    try
                    {
                        LogHelper.Error(LogPrefix + "[XAML诊断] " + t.FullName, ex);
                    }
                    catch { }
                };
            }
            catch (Exception ex)
            {
                // 显式 (object) 消歧:Warn(object, Exception) 与 Warn(string, params object[]) 二义
                LogHelper.Warn((object)(LogPrefix + "XAML 诊断钩子注册失败"), ex);
            }
        }

        #endregion

        #region onNotify

        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

            if (string.IsNullOrEmpty(name)) return;

            LogHelper.Debug(LogPrefix + "收到广播: {0}", name);

            if (name.Equals(Consts.Topic_RestartApp))
            {
                // 退出前持久化 UI 状态(列宽/列序、过滤器激活项、子tab 选中项)。P4 落地。
            }
            else if (name.Equals(Consts.Topic_Clear))
            {
                // 登出复位:广播数据源重置消息,各模块 VM 订阅后清空自身缓存(集合实例保持不变,绑定不断)。
                EventAggregator.Publish(new DataSourceResetMessage());
            }
        }

        #endregion
    }
}
