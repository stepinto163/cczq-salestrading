using com.gateway.log;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.Utils.Events;
using STC.Common.ViewModel;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.ViewModel.TradeBookHandler;
using System;
using System.Threading;
using System.Windows;

namespace STC.XBond.Plugin
{
    /// <summary>
    /// TradeBook插件类
    /// </summary>
    public class TradeBookPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 2;

        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.TradeBookPlugin; }
        }

        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }


        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = false;
        public override bool IsAlone { get; set; }


        private bool isFullScreen = false;
        public override bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }

            set
            {
                isFullScreen = value;
            }
        }

        public override string PluginName
        {
            get { return "Tradebook插件"; }
        }

        public override string Description
        {
            get { return "Tradebook"; }
        }

        public override string Author
        {
            get { return "严晓丽"; }
        }

        #endregion

        #region load,unload
        NotifyCloseTabItem notifyCloseTabItem;

        private readonly string _ModelId = "4000";
        private readonly string _Code = "4002";

        private bool _isFirst = true;
        private bool _isTradeBookHeader = false;

        protected async override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            //无论是否有权限都要执行
            //第一次菜单加载
            if (!isPermission)
            {
                if (_isFirst)
                {
                    _isFirst = false;
                    TBookViewHandler.Instance.Init();
                }

                return;
            }

            if (_isTradeBookHeader == false)
            {
                _isTradeBookHeader = true;
                SystemParameterReq req = new SystemParameterReq();
                req.ParamKey = CommonUtils.PARAM_KEY_TRADEBOOKHEADER;
                req.Username = LocalEnv.UserInfo.UserName;
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                {
                    CommonUtils.TradeBookHeader = reply.Datas.tbookSystemParameter.param_value;
                }
            }

            //第一次菜单加载
            if (_isFirst)
            {
                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_tradebook.png", Description, PluginKey, "2");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_tradebook.png", Description, PluginKey, "2");
                }

                _isFirst = false;
                enabled = false;

                Application.Subscribe(Consts.Topic_RestartApp, this);
                TBookViewHandler.Instance.Init();
                return;
            }

            string headerTxt = this.Description;
            string id = string.Empty;
            if (PluginData != null)
            {
                id = PluginData.ToString();
                Common.Model.Base.BondsInfoPo bi = CommonUtils.GetBondsInfoPoBySecurityID(PluginData.ToString());
                if (bi != null)
                {
                    // 如果是信用债，显示债券名
                    if (CommonUtils.TradeBookHeader == "1")
                    {
                        headerTxt = bi.Short_Name;
                    }
                    // 其他
                    else
                    {
                        headerTxt = bi.Bond_ID;
                    }
                }
            }
            
            string tagHeader = headerTxt;
            if (TabPageHelper.IsExistTabItem(tagHeader))
            {
                PluginData = null;
                return;
            }


            EmsOrderProcesser.Processer.Init();
            LastExecutionProcesser.Processer.Init();

            LoadConfig.LoadBond = true;

            //TradeBookView view = new TradeBookView();
            //QuotationListViewModel viewModel = new QuotationListViewModel();
            //if (PluginData != null)
            //{
            //    viewModel.SecurityID = PluginData as string;
            //    BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(viewModel.SecurityID);
            //    if (bondsInfoPo != null)
            //    {
            //        viewModel.Symbol = bondsInfoPo.Short_Name;
            //    }
            //}


            //view.DataContext = viewModel;
            
            TradeBookView view = await TBookViewHandler.Instance.GetTradeBookView(id, false);

            if (notifyCloseTabItem == null)
            {
               notifyCloseTabItem = EventHub.GetEvent<NotifyCloseTabItem>();
               notifyCloseTabItem.Subscribe(NotifyCloseTabItem);
            }
            
            TabPageHelper.OpenTabPage(tagHeader, view, $"{PluginKey}-{id}");
            PluginData = null;
            Application.Subscribe("STC_Clear", this);

            base.load();

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    EmsOrdersHandler.Instance.init();
                    ExecutionsHandler.Instance.init();
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });

            this.Enabled = false;

            //// 系统备妥
            //Frame.Handler.DesignHandler.Instance.SystemIsReady(new object());
        }

        public override void BeforeTerminating()
        {
            //this.Application.Panel.Items.Remove(item3);
            //this.Application.Panel.Items.Remove(item2);
            //this.Application.Panel.Items.Remove(item);

            if (notifyCloseTabItem != null)
            {
                notifyCloseTabItem.UnSubscribe(NotifyCloseTabItem);
            }
        }

        public void SaveOpenedWindows()
        {
            foreach (var item in WindowManager.Instance._UniqueKeyMapWindowDic)
            {
                string key = item.Key;
                Window w = item.Value;

                if (key.Contains("QuotesMiniWindows"))
                {
                    w.Close();
                }
            }

            foreach (var item in WindowManager.Instance._UniqueKeyMapWindowDic)
            {
                string key = item.Key;
                Window w = item.Value;

                if (key.Contains("STC.XBond.Plugin"))
                {
                    w.Close();
                }
            }
        }


        private void NotifyCloseTabItem(int tabItemHashCode)
        {
            foreach (System.Windows.Controls.TabItem tabItem in Application.Panel.Items)
            {
                if (tabItem.GetHashCode() == tabItemHashCode)
                {
                    if (tabItem != null && tabItem.Content != null && tabItem.Content is TradeBookView)
                    {
                        LogHelper.Debug("QuotationList:" + tabItem.Header + " UserControl_Unloaded ");
                        TradeBookView tradeBookView = tabItem.Content as TradeBookView;
                        if (tradeBookView != null && tradeBookView.DataContext != null 
                            && tradeBookView.DataContext is QuotationListViewModel)
                        {
                            QuotationListViewModel viewModel = ((QuotationListViewModel)tradeBookView.DataContext);
                            //viewModel.Dispose();
                        }
                        
                    }
                }
            }
        }
        #endregion

        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

            if (!string.IsNullOrEmpty(name))
            {
                if (name.Equals(Consts.Topic_Clear))
                {

                }

                if (name.Equals(Consts.Topic_RestartApp))
                {
                    SaveOpenedWindows();
                }
            }
        }

        #endregion
    }
}
