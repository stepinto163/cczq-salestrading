using STC.Broker.Plugin.Command;
using STC.Broker.Plugin.View.Component;
using STC.Broker.Plugin.View.History;
using STC.Broker.Plugin.View.Transaction;
using STC.Broker.Plugin.View.Transaction.BridgeDesign;
using STC.Broker.Plugin.ViewModel.Transaction;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Frame.Common;
using STC.Frame.Plugin;
using STC.Frame.Utils.MultiUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.Broker.Plugin
{
    public class STCTransactionPlugin : AbstractPlugin
    {
        #region 插件信息

        private int _tag = 16;


        public override int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override int PluginKey
        {
            get { return (int)PluginKeyEnum.GRTransaction; }
        }

        public override string PluginName
        {
            get { return "交易处理插件"; }
        }

        public override string Description
        {
            get { return "交易处理"; }
        }

        public override string Author
        {
            get { return "chenhu"; }
        }

        protected new bool isMain = false;
        public override bool IsMain { get; set; }

        protected new bool isAlone = true;
        public override bool IsAlone { get; set; }


        protected new bool enabled = true;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        private bool isFullScreen = true;
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

        #endregion

        #region load,unload
        private bool _isFirst = true;
        //待定 
        private readonly string _ModelId = "4000";
        private readonly string _Code = "4022";

        private CopyDetailUI copyUI = null;

        private BridgeWinNew bridgeWin = null;

        protected override void load()
        {
            bool isPermission = AdminHandler.Instance.CheckPermisionById(_ModelId, _Code);
            if (!isPermission)
                return;
            //第一次菜单加载
            if (_isFirst)
            {
                _isFirst = false;

                if (LocalEnv.DictSkin.Contains(LocalEnv.ConfInfo.DefaultSkinStyle))
                {
                    AppUtils.Application.MenuPanel.AddMenu($"pack://application:,,,/STC.Common;component/Images/{LocalEnv.ConfInfo.DefaultSkinStyle}/stc_image_menulist_neibukanban.png", Description, PluginKey, "16");
                }
                else
                {
                    AppUtils.Application.MenuPanel.AddMenu("pack://application:,,,/STC.Common;component/Images/DefaultSkin/stc_image_menulist_neibukanban.png", Description, PluginKey, "16");
                }

                enabled = false;

                //订阅App消息
                Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
                return;
            }


            TabPageHelper.OpenTabPage(this.Description, new TransactionPanel(), PluginKey);

            if (isLoad)
            {
                return;
            }

            this.broadcast("loadCRM", string.Empty);
            Application.Subscribe("IDC_Clear", this);

            base.load();
            //isLoad = true;
        }
        public override void BeforeTerminating()
        {
            //////this.Application.Panel.Items.Remove(item2);
            //////this.Application.Panel.Items.Remove(item);
        }

        #endregion

        #region onNotify
        public override void onNotify(string name, object o)
        {
            base.onNotify(name, o);

        }

        int bridgeInt = 0;
        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private async void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "TriggerGrCommand" && !string.IsNullOrEmpty(obj.Info1))
            {
                switch (obj.Info1)
                {
                    case "DetailCommand":
                        //明细
                        if (copyUI == null)
                        {
                            await MultDispaterHelper.InvokeByModelName("OrderDetail", () =>
                            {
                                copyUI = new CopyDetailUI();
                                MultBorderWin copyUIBoardWin = new MultBorderWin();
                                copyUIBoardWin.SetTitleAndKey("DetailCommand", "明细", "Transaction");
                                copyUIBoardWin.SetCommandParameter($"DetailCommand");
                                copyUIBoardWin.SetMainControl(copyUI);
                                copyUIBoardWin.Width = 1084;
                                copyUIBoardWin.Height = 800;
                                copyUIBoardWin.Owner = null;
                                copyUIBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                WindowManager.Instance.OpenWindow($"Transaction-DetailCommand", copyUIBoardWin);
                            });

                        }
                        else
                        {
                            // 隐藏期间，有数据更新需要刷新界面
                            //if (!copyUI.IsVisible)
                            //{
                                await MultDispaterHelper.InvokeByModelName("OrderDetail", () =>
                                {
                                    copyUI.refresh();
                                    MultBorderWin copyUIBoardWin = new MultBorderWin();
                                    copyUIBoardWin.SetTitleAndKey("DetailCommand", "明细", "Transaction");
                                    copyUIBoardWin.SetCommandParameter($"DetailCommand");
                                    copyUIBoardWin.SetMainControl(copyUI);
                                    copyUIBoardWin.Width = 1084;
                                    copyUIBoardWin.Height = 800;
                                    copyUIBoardWin.Owner = null;
                                    copyUIBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                    WindowManager.Instance.OpenWindow($"Transaction-DetailCommand", copyUIBoardWin);
                                });
                            //}
                        }

                        break;
                    case "BridgeCommand":
                        //过桥
                        if (bridgeWin == null)
                        {
                            bridgeInt++;
                            if (bridgeInt > 1)
                            {
                                break;
                            }
                            await MultDispaterHelper.InvokeByModelName("OrderBridge", () =>
                            {
                                bridgeWin = new BridgeWinNew();
                                MultBorderWin copyUIBoardWin = new MultBorderWin();
                                //copyUIBoardWin.SetTitleAndKey(PopupTypeEnum.BridgeWin.ToString(), "过桥", "Transaction");
                                //copyUIBoardWin.SetCommandParameter($"{((int)PopupTypeEnum.CopyDetailUI).ToString()}");
                                copyUIBoardWin.SetTitleAndKey("BridgeCommand", "过桥", "Transaction");
                                copyUIBoardWin.SetCommandParameter($"BridgeCommand");
                                copyUIBoardWin.SetMainControl(bridgeWin);
                                copyUIBoardWin.OnCloseClick += bridgeWin.BridgeWinNew_Closed;
                                //copyUIBoardWin.Width = 1200;
                                //copyUIBoardWin.Height = 500;
                                //copyUIBoardWin.OnCloseClick += bridgeWin.BridgeWinNew_Closed;
                                copyUIBoardWin.Width = 1128;
                                copyUIBoardWin.Height = 800;
                                copyUIBoardWin.Owner = null;

                                copyUIBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                WindowManager.Instance.OpenWindow($"Transaction-BridgeCommand", copyUIBoardWin);
                            });
                        }
                        else
                        {
                            await MultDispaterHelper.InvokeByModelName("OrderBridge", () =>
                            {
                                bridgeWin.ReFlash();
                                MultBorderWin copyUIBoardWin = new MultBorderWin();
                                //copyUIBoardWin.SetTitleAndKey(PopupTypeEnum.BridgeWin.ToString(), "过桥", "Transaction");
                                //copyUIBoardWin.SetCommandParameter($"{((int)PopupTypeEnum.CopyDetailUI).ToString()}");
                                copyUIBoardWin.SetTitleAndKey("BridgeCommand", "过桥", "Transaction");
                                copyUIBoardWin.SetCommandParameter($"BridgeCommand");
                                copyUIBoardWin.SetMainControl(bridgeWin);
                                copyUIBoardWin.OnCloseClick += bridgeWin.BridgeWinNew_Closed;
                                //copyUIBoardWin.Width = 1200;
                                //copyUIBoardWin.Height = 500;
                                //copyUIBoardWin.OnCloseClick += bridgeWin.BridgeWinNew_Closed;
                                copyUIBoardWin.Width = 1128;
                                copyUIBoardWin.Height = 800;
                                copyUIBoardWin.Owner = null;

                                copyUIBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                WindowManager.Instance.OpenWindow($"Transaction-BridgeCommand", copyUIBoardWin);
                            });
                        }
                        break;
                    case "DeleteCommand":
                        //删除
                        await Task.Factory.StartNew(() =>
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(async () =>
                            {
                                DeleteOrderWin delorderWin = new DeleteOrderWin();
                                BorderWin copyUIBoardWin = new BorderWin();
                                copyUIBoardWin.SetTitleAndKey("DeleteCommand", "已删除成交单", "Transaction");
                                copyUIBoardWin.SetCommandParameter($"DeleteCommand");
                                copyUIBoardWin.SetMainControl(delorderWin);
                                copyUIBoardWin.Width = 1180;
                                copyUIBoardWin.Height = 600;
                                copyUIBoardWin.Owner = null;
                                copyUIBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                WindowManager.Instance.OpenWindow($"Transaction-DeleteCommand", copyUIBoardWin);
                            }));
                        });

                        break;
                    case "TransactionPanel":
                        //交易处理 弹出窗口
                        await Task.Factory.StartNew(() =>
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(async () =>
                            {
                                BorderWin innerBoardWin = new BorderWin();
                                innerBoardWin.SetTitleAndKey(PopupTypeEnum.TransactionPanel.ToString(), "交易处理", "Transaction");
                                innerBoardWin.SetCommandParameter($"{((int)PopupTypeEnum.TransactionPanel).ToString()}");

                                TransactionPanel transactionPanel = new TransactionPanel();
                                (transactionPanel.FindName("popupBtn") as Button).Visibility = Visibility.Collapsed;
                                //transactionPanel.DataContext = new TransactionPanelViewModel(transactionPanel);
                                (transactionPanel.DataContext as TransactionPanelViewModel).SubscribeAppDetailMsg();
                                //innerBoardView.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                                innerBoardWin.SetMainControl(transactionPanel);
                                innerBoardWin.Width = 1560;
                                innerBoardWin.Height = 500;
                                innerBoardWin.Owner = null;
                                innerBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                WindowManager.Instance.OpenWindow($"Transaction-{PopupTypeEnum.TransactionPanel.ToString()}", innerBoardWin);
                                innerBoardWin.SizeChanged += InnerBoardWin_SizeChanged;
                            }));
                        });
                        break;
                    case "UnderLineExecWin":
                        //线下成交
                        await Task.Factory.StartNew(() =>
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(async () =>
                            {
                                BorderWin copyUIBoardWin = new BorderWin();
                                View.Transaction.UnderLineExec.UnderLineExecWin ui = new View.Transaction.UnderLineExec.UnderLineExecWin();
                                copyUIBoardWin.SetTitleAndKey("UnderLineExecWin", "线下成交", "Transaction");
                                copyUIBoardWin.SetMainControl(ui);
                                copyUIBoardWin.Width = 490;
                                copyUIBoardWin.Height = 440;
                                copyUIBoardWin.ResizeMode = ResizeMode.NoResize;
                                copyUIBoardWin.Owner = null;
                                copyUIBoardWin.SetMinVisible(false);
                                copyUIBoardWin.SetMaxVisible(false);
                                copyUIBoardWin.RemoveWindowsMouseDoubleClickEvent();
                                copyUIBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                WindowManager.Instance.OpenWindow($"Transaction-UnderLineExecWin", copyUIBoardWin);
                            }));
                        });
                        break;
                }
            }
        }

        /// <summary>
        /// 大小变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void InnerBoardWin_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            BorderWin innerBoardWin = sender as BorderWin;
            if (innerBoardWin != null)
            {
                TransactionPanel transactionPanel = innerBoardWin.GetMainControl() as TransactionPanel;
                if (transactionPanel != null)
                {

                    transactionPanel.Width = innerBoardWin.ActualWidth - 10;
                }
            }
        }

        #endregion
    }
}
