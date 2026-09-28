using Ats.Foundation.Message;
using Ats.Foundation.Utils.Seqence;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Model;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Order;
using BDB.Trade.Plugin.View.UserSetting;
using BDB.Trade.Plugin.ViewModel.Adapter;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using STC.Common.Utils;
using STC.Common.ViewModel.UI;
using STC.Common.Handler.TBook;
using STC.Common.Model.Risk;
using STC.Common.Model.Fix.Message;
using STC.Common.View.UI;
using STC.Frame.UI;
using STC.Common.Model.Admin;
using STC.Common.Handler.Admin;

namespace BDB.Trade.Plugin.ViewModel.UserSetting
{
    public class NewUserSettingViewModel : BaseViewModel
    {
        public const string PARAM_KEY_BpWithCCDC = "STC_BP_WITH_CCDC";

        #region Stuctrue
        private NewUserSettingView myWindow;
        public ICommand OkCommand { get { return new RelayCommand(OnOKClick); } }
        /// <summary>
        /// 默认选择菜单
        /// </summary>
        private int settleBoxIndex = 4;
        public int SettleBoxIndex
        {
            get
            {
                return settleBoxIndex;
            }

            set
            {
                settleBoxIndex = value;

                if (settleBoxIndex==4) {
                    Border pageExcelLayout = myWindow.FindName("BorderExcelLayout") as Border;
                    pageExcelLayout.Visibility = Visibility.Collapsed;
                    Border pagePriceLayout = myWindow.FindName("BorderPriceLayout") as Border;
                    pagePriceLayout.Visibility = Visibility.Visible;

                    Border borderPythonLayout = myWindow.FindName("BorderPythonLayout") as Border;
                    borderPythonLayout.Visibility = Visibility.Collapsed;

                    Border borderMsgLayout = myWindow.FindName("BorderMsgLayout") as Border;
                    borderMsgLayout.Visibility = Visibility.Collapsed;

                    VisibilityRisk = Visibility.Collapsed;
                    VisibilityOK = Visibility.Visible;
                    VisibilityClose = Visibility.Visible;
                    VisibilityExcel = Visibility.Collapsed;
                    VisPriceLayout = Visibility.Visible;
                }
                else if (settleBoxIndex == 3)
                {
                    VisibilityRisk = Visibility.Visible;
                    VisibilityOK = Visibility.Visible;
                    VisibilityClose = Visibility.Visible;
                    VisibilityExcel = Visibility.Collapsed;
                }
                else if (settleBoxIndex == 5)
                {
                    Border pageExcelLayout = myWindow.FindName("BorderExcelLayout") as Border;
                    pageExcelLayout.Visibility = Visibility.Visible;
                    Border pagePriceLayout = myWindow.FindName("BorderPriceLayout") as Border;
                    pagePriceLayout.Visibility = Visibility.Collapsed;

                    Border borderPythonLayout = myWindow.FindName("BorderPythonLayout") as Border;
                    borderPythonLayout.Visibility = Visibility.Collapsed;

                    Border borderMsgLayout = myWindow.FindName("BorderMsgLayout") as Border;
                    borderMsgLayout.Visibility = Visibility.Collapsed;

                    VisibilityRisk = Visibility.Collapsed;
                    VisibilityOK = Visibility.Collapsed;
                    VisibilityClose = Visibility.Collapsed;
                    VisibilityExcel = Visibility.Collapsed;
                    VisPriceLayout = Visibility.Collapsed;
                }
                else if (settleBoxIndex == 6)
                {
                    Border pageExcelLayout = myWindow.FindName("BorderExcelLayout") as Border;
                    pageExcelLayout.Visibility = Visibility.Collapsed;
                    Border pagePriceLayout = myWindow.FindName("BorderPriceLayout") as Border;
                    pagePriceLayout.Visibility = Visibility.Collapsed;

                    Border borderPythonLayout = myWindow.FindName("BorderPythonLayout") as Border;
                    borderPythonLayout.Visibility = Visibility.Visible;

                    Border borderMsgLayout = myWindow.FindName("BorderMsgLayout") as Border;
                    borderMsgLayout.Visibility = Visibility.Collapsed;

                    VisibilityRisk = Visibility.Collapsed;
                    VisibilityOK = Visibility.Collapsed;
                    VisibilityClose = Visibility.Collapsed;
                    VisibilityExcel = Visibility.Collapsed;
                    VisPriceLayout = Visibility.Collapsed;
                }
                else if (SettleBoxIndex == 7)
                {
                    Border pageExcelLayout = myWindow.FindName("BorderExcelLayout") as Border;
                    pageExcelLayout.Visibility = Visibility.Collapsed;
                    Border pagePriceLayout = myWindow.FindName("BorderPriceLayout") as Border;
                    pagePriceLayout.Visibility = Visibility.Collapsed;

                    Border borderPythonLayout = myWindow.FindName("BorderPythonLayout") as Border;
                    borderPythonLayout.Visibility = Visibility.Collapsed;

                    Border borderMsgLayout = myWindow.FindName("BorderMsgLayout") as Border;
                    borderMsgLayout.Visibility = Visibility.Visible;

                    VisibilityRisk = Visibility.Collapsed;
                    VisibilityOK = Visibility.Visible;
                    VisibilityClose = Visibility.Visible;
                    VisibilityExcel = Visibility.Collapsed;
                    VisPriceLayout = Visibility.Collapsed;
                }

                this.OnPropertyChanged(nameof(SettleBoxIndex));
            }
        }

        /// <summary>
        /// 行情显示菜单
        /// </summary>
        private Visibility visPriceLayout = Visibility.Collapsed;
        public Visibility VisPriceLayout
        {
            get
            {
                return visPriceLayout;
            }

            set
            {
                visPriceLayout = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Excel菜单
        /// </summary>
        private Visibility visibilityExcel = Visibility.Collapsed;
        public Visibility VisibilityExcel
        {
            get
            {
                return visibilityExcel;
            }

            set
            {
                visibilityExcel = value;
                this.OnPropertyChanged(nameof(VisibilityExcel));
            }
        }

        /// <summary>
        /// 风控菜单
        /// </summary>
        private Visibility visibilityRisk = Visibility.Visible;
        public Visibility VisibilityRisk
        {
            get
            {
                return visibilityRisk;
            }

            set
            {
                visibilityRisk = value;
                this.OnPropertyChanged(nameof(VisibilityRisk));
            }
        }

        private Visibility visibilityOK = Visibility.Visible;
        public Visibility VisibilityOK
        {
            get
            {
                return visibilityOK;
            }

            set
            {
                visibilityOK = value;
                this.OnPropertyChanged(nameof(VisibilityOK));
            }
        }
        private Visibility visibilityClose = Visibility.Visible;
        public Visibility VisibilityClose
        {
            get
            {
                return visibilityClose;
            }

            set
            {
                visibilityClose = value;
                this.OnPropertyChanged(nameof(VisibilityClose));
            }
        }

        /// <summary>
        /// 账号
        /// </summary>
        private string _userName;
        public string userName
        {
            get
            {
                return _userName;
            }

            set
            {
                _userName = value;
                this.OnPropertyChanged(nameof(userName));
            }
        }

        #region  单笔报量
        /// <summary>
        /// 单笔报量 是否勾选 1-勾选  0-不勾选
        /// </summary>
        private int _qtyCheck = 0;
        public int qtyCheck
        {
            get
            {
                return _qtyCheck;
            }

            set
            {
                _qtyCheck = value;
                this.OnPropertyChanged(nameof(qtyCheck));
            }
        }

        /// <summary>
        /// 报量下限 1000
        /// </summary>
        private string _qtyMin = "";
        public string qtyMin
        {
            get
            {
                return _qtyMin;
            }

            set
            {
                _qtyMin = value;
                this.OnPropertyChanged(nameof(qtyMin));
                CheckQtyValid();
            }
        }

        public void CheckQtyValid()
        {
            decimal d = 0;

            bool res = true;

            if (!decimal.TryParse(qtyMin, out d))
            {
                res = false;
            }

            if (!decimal.TryParse(qtyMax, out d))
            {
                res = false;
            }

            if (!decimal.TryParse(qtyMin, out d))
            {
                res = false;
            }

            if (res && ((decimal.Parse(qtyMin) < 100) || (decimal.Parse(qtyMin) > 10000000)))
            {
                res = false;
            }

            if (res && ((decimal.Parse(qtyMax) < 100) || (decimal.Parse(qtyMax) > 10000000)))
            {
                res = false;
            }

            if (res && (decimal.Parse(qtyMin) > decimal.Parse(qtyMax)))
            {
                res = false;
            }

            if (res && (decimal.TryParse(qtyMin, out d) && (d % 100) > 0))
            {
                res = false;
            }

            if (res && (decimal.TryParse(qtyMax, out d) && (d % 100) > 0))
            {
                res = false;
            }

            SaveIsEnable = res;
        }

        /// <summary>
        /// 报量上限 10000000
        /// </summary>
        private string _qtyMax = "";
        public string qtyMax
        {
            get
            {
                return _qtyMax;
            }

            set
            {
                _qtyMax = value;
                this.OnPropertyChanged(nameof(qtyMax));
                CheckQtyValid();
            }
        }
        #endregion

        #region 中债估值
        /// <summary>
        /// 中债估值Bid偏离 是否勾选 1-勾选  0-不勾选
        /// </summary>
        private int _chinaBidCheck = 0;
        public int chinaBidCheck
        {
            get
            {
                return _chinaBidCheck;
            }

            set
            {
                _chinaBidCheck = value;
                this.OnPropertyChanged(nameof(chinaBidCheck));
            }
        }

        /// <summary>
        /// 中债估值Bid偏离
        /// </summary>
        private string _chinaBid = "";
        public string chinaBid
        {
            get
            {
                return _chinaBid;
            }

            set
            {
                _chinaBid = value;
                this.OnPropertyChanged(nameof(chinaBid));
            }
        }


        /// <summary>
        /// 中债估值ofr偏离 是否勾选 0-勾选  1-不勾选
        /// </summary>
        private int _chinaOfrCheck = 0;
        public int chinaOfrCheck
        {
            get
            {
                return _chinaOfrCheck;
            }

            set
            {
                _chinaOfrCheck = value;
                this.OnPropertyChanged(nameof(chinaOfrCheck));
            }
        }

        /// <summary>
        /// 中债估值ofr偏离
        /// </summary>
        private string _chinaOfr = "";
        public string chinaOfr
        {
            get
            {
                return _chinaOfr;
            }

            set
            {
                _chinaOfr = value;
                this.OnPropertyChanged(nameof(chinaOfr));
            }
        }
        #endregion

        #region 市场最优偏离
        /// <summary>
        /// 市场最优Bid偏离 是否勾选 1-勾选  0-不勾选
        /// </summary>
        private int _marketBidCheck = 0;
        public int marketBidCheck
        {
            get
            {
                return _marketBidCheck;
            }

            set
            {
                _marketBidCheck = value;
                this.OnPropertyChanged(nameof(marketBidCheck));
            }
        }

        /// <summary>
        /// 市场最优Bid偏离
        /// </summary>
        private string _marketBid = "";
        public string marketBid
        {
            get
            {
                return _marketBid;
            }

            set
            {
                _marketBid = value;
                this.OnPropertyChanged(nameof(marketBid));
            }
        }

        /// <summary>
        /// 市场最优ofr偏离 是否勾选 1-勾选  0-不勾选
        /// </summary>
        private int _marketOfrCheck = 0;
        public int marketOfrCheck
        {
            get
            {
                return _marketOfrCheck;
            }

            set
            {
                _marketOfrCheck = value;
                this.OnPropertyChanged(nameof(marketOfrCheck));
            }
        }

        /// <summary>
        /// 市场最优ofr偏离
        /// </summary>
        private string _marketOfr = "";
        public string marketOfr
        {
            get
            {
                return _marketOfr;
            }

            set
            {
                _marketOfr = value;
                this.OnPropertyChanged(nameof(marketOfr));
            }
        }

        #endregion

        #region 倒挂 
        /// <summary>
        /// 倒挂Bid 是否勾选 1-勾选  0-不勾选
        /// </summary>
        private int _dropAwayFromBidCheck = 0;
        public int dropAwayFromBidCheck
        {
            get
            {
                return _dropAwayFromBidCheck;
            }

            set
            {
                _dropAwayFromBidCheck = value;
                this.OnPropertyChanged(nameof(dropAwayFromBidCheck));
            }
        }

        /// <summary>
        /// 倒挂Bid
        /// </summary>
        private string _dropAwayFromBid = "";
        public string dropAwayFromBid
        {
            get
            {
                return _dropAwayFromBid;
            }

            set
            {
                _dropAwayFromBid = value;
                this.OnPropertyChanged(nameof(dropAwayFromBid));
            }
        }

        /// <summary>
        /// 倒挂ofr 是否勾选 1-勾选  0-不勾选
        /// </summary>
        private int _dropAwayFromOfrCheck = 0;
        public int dropAwayFromOfrCheck
        {
            get
            {
                return _dropAwayFromOfrCheck;
            }

            set
            {
                _dropAwayFromOfrCheck = value;
                this.OnPropertyChanged(nameof(dropAwayFromOfrCheck));
            }
        }

        /// <summary>
        /// 倒挂ofr
        /// </summary>
        private string _dropAwayFromOfr = "";
        public string dropAwayFromOfr
        {
            get
            {
                return _dropAwayFromOfr;
            }

            set
            {
                _dropAwayFromOfr = value;
                this.OnPropertyChanged(nameof(dropAwayFromOfr));
            }
        }
        #endregion

        #region 成交偏离
        /// <summary>
        /// 成交Bid偏离 是否勾选 1-勾选  0-不勾选
        /// </summary>
        private int _excutOrderBidCheck = 0;
        public int excutOrderBidCheck
        {
            get
            {
                return _excutOrderBidCheck;
            }

            set
            {
                _excutOrderBidCheck = value;
                this.OnPropertyChanged(nameof(excutOrderBidCheck));
            }
        }

        /// <summary>
        /// 成交bid偏离
        /// </summary>
        private string _excutOrderBid = "";
        public string excutOrderBid
        {
            get
            {
                return _excutOrderBid;
            }

            set
            {
                _excutOrderBid = value;
                this.OnPropertyChanged(nameof(excutOrderBid));
            }
        }

        /// <summary>
        /// 成交ofr偏离 是否勾选 1-勾选  0-不勾选
        /// </summary>
        private int _excutOrderOfrCheck = 0;
        public int excutOrderOfrCheck
        {
            get
            {
                return _excutOrderOfrCheck;
            }

            set
            {
                _excutOrderOfrCheck = value;
                this.OnPropertyChanged(nameof(excutOrderOfrCheck));
            }
        }

        /// <summary>
        /// 成交ofr偏离
        /// </summary>
        private string _excutOrderOfr = "";
        public string excutOrderOfr
        {
            get
            {
                return _excutOrderOfr;
            }

            set
            {
                _excutOrderOfr = value;
                this.OnPropertyChanged(nameof(excutOrderOfr));
            }
        }

        #endregion

        private bool _saveIsEnable = true;
        public bool SaveIsEnable
        {
            get
            {
                return _saveIsEnable;
            }

            set
            {
                _saveIsEnable = value;
                this.OnPropertyChanged(nameof(SaveIsEnable));
            }
        }

        private bool _isShowWarn = false;
        private MessageBoxGUI _warnBox;

        #endregion

        public NewUserSettingViewModel()
        {

        }

        #region Init
        public void Init(NewUserSettingView parentUI)
        {
            this.myWindow = parentUI;
            VisibilityRisk = Visibility.Visible;
            VisibilityOK = Visibility.Visible;
            visibilityClose = Visibility.Visible;
            // 查询风控设置
            //QueryRiskPara();
        }


        #endregion

        #region  查询风控设置
        /// <summary>
        /// 查询风控设置
        /// </summary>
        private void QueryRiskPara()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    // string userid = LocalEnv.UserInfo.UserName;
                    // userid = "idb_trader02";
                    RiskParaReplyPo result = TBookHandler.Instance.queryRiskPara(new QueryRiskParameter() {userName = LocalEnv.UserInfo.UserName });
                    if (result.IsSuccess())
                    {
                        // 如果已经设置了默认值
                        if (result.datas.defaultRiskParameter != null)
                        {
                            RefreshRiskParaUI(result);
                        }
                        else
                        {
                            // 如果没有自定义设置，则使用默认值
                            this.qtyCheck = 0;
                            this.qtyMin = "";
                            this.qtyMax = "";

                            this.chinaBidCheck = 0;
                            this.chinaBid = "";
                            this.chinaOfrCheck = 0;
                            this.chinaOfr = "";

                            this.marketBidCheck = 0;
                            this.marketBid = "";
                            this.marketOfrCheck = 0;
                            this.marketOfr = "";

                            this.dropAwayFromBidCheck = 0;
                            this.dropAwayFromBid = "";
                            this.dropAwayFromOfrCheck = 0;
                            this.dropAwayFromOfr = "";

                            this.excutOrderBidCheck = 0;
                            this.excutOrderBid = "";
                            this.excutOrderOfrCheck = 0;
                            this.excutOrderOfr = "";
                        }
                    }
                    else
                    {
                        // 如果失败，则写失败消息到日志
                        LogHelper.Info($"获取风控设置失败！Code:{result.Code} Msg:{result.Msg}");
                    }
                });
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        /// <param name="res"></param>
        public void RefreshRiskParaUI(RiskParaReplyPo res)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DefaultRiskParameter dp = res.datas.defaultRiskParameter;
                this.qtyCheck = int.Parse(dp.qtyCheck);
                this.qtyMin = dp.qtyMin;
                this.qtyMax = dp.qtyMax;

                this.chinaBidCheck = int.Parse(dp.chinaBidCheck);
                this.chinaBid = dp.chinaBid;
                this.chinaOfrCheck = int.Parse(dp.chinaOfrCheck);
                this.chinaOfr = dp.chinaOfr;

                this.marketBidCheck = int.Parse(dp.marketBidCheck);
                this.marketBid = dp.marketBid;
                this.marketOfrCheck = int.Parse(dp.marketOfrCheck);
                this.marketOfr = dp.marketOfr;

                this.dropAwayFromBidCheck = int.Parse(dp.dropAwayFromBidCheck);
                this.dropAwayFromBid = dp.dropAwayFromBid;
                this.dropAwayFromOfrCheck = int.Parse(dp.dropAwayFromOfrCheck);
                this.dropAwayFromOfr = dp.dropAwayFromOfr;

                this.excutOrderBidCheck = int.Parse(dp.excutOrderBidCheck);
                this.excutOrderBid = dp.excutOrderBid;
                this.excutOrderOfrCheck = int.Parse(dp.excutOrderOfrCheck);
                this.excutOrderOfr = dp.excutOrderOfr;
            });
        }

        #endregion


        #region  保存设置
        private void OnOKClick()
        {
            //if (!CheckParam())
            //{
            //    if (!_isShowWarn)
            //    {
            //        if (_warnBox == null)
            //            _warnBox = new MessageBoxGUI();
            //        else
            //            _warnBox.Activate();

            //        _warnBox.setWarnMsg("缺省报量必须是100的整数倍");
            //        _warnBox.Closed += WarnBox_Closed;
            //        _warnBox.ShowDialog();
            //        _isShowWarn = true;
            //    }
            //    return;
            //}

            this.SaveIsEnable = false;
            //UpdateRiskParaData();


            if (settleBoxIndex == 4)//行情卡片页显示设置
            {

                PageSetQuotation pageSetQuotation = myWindow.FindName("psPriceLayout") as PageSetQuotation;
                LayoutSettingPo currentSetting = pageSetQuotation.GetLayoutSetting();

                if (currentSetting.Row != LayoutSetttingCache.Instance.layoutSettingPo.Row
                    || currentSetting.Col != LayoutSetttingCache.Instance.layoutSettingPo.Col)
                {
                    QuoteConfigHandler.Instance.DeleteConfig();
                }

                LayoutSettingPo layoutSetting = currentSetting;
                LayoutSetttingCache.Instance.SetLayoutSetting(layoutSetting);

                LayoutSetttingCache.Instance.SaveModeSetting();

                CommonHandler.Instance.ResetNum();
                LogHelper.Warn("Reset SettingChange");
                STC.Common.Data.DesignHandler.Instance.SettingChange(layoutSetting);


                // 保存涨跌计算
                SystemGlobalParamReq req = new SystemGlobalParamReq();
                req.action = "2";
                req.list = new List<TTbookSystemParameter>();
                TTbookSystemParameter param = new TTbookSystemParameter();
                param.param_key = PARAM_KEY_BpWithCCDC;
                param.username = LocalEnv.UserInfo.UserName;
                string ccdcMode = pageSetQuotation.tbCCDC.IsChecked == true ? "1" : "0";
                param.param_value = ccdcMode;
                param.param_desc = "消息提醒设置";
                req.list.Add(param);

                PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);

                if (reply != null && reply.IsSuccess())
                {
                    TradingPlugin.BP_WITH_CCDC = ccdcMode;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        STC.Common.Data.DesignHandler.Instance.SettingChange(new STC.Common.Model.MM.NewSettingMsg() { key = "SettingBpMode", value = ccdcMode });
                    });
                }
            }

            if (settleBoxIndex == 5) //Excel
            {


                //ExcelView psExcelView = myWindow.FindName("psExcelLayout") as ExcelView;

            }
            if (settleBoxIndex == 7)
            {
                BDB.Common.View.UI.MsgAlertSettingView msgAlertSettingView = myWindow.FindName("msgAlertSettingView") as BDB.Common.View.UI.MsgAlertSettingView;
                if (msgAlertSettingView.DataContext != null && msgAlertSettingView.DataContext is MsgAlertSettingViewModel)
                {
                    MsgAlertSettingViewModel msgAlertSettingViewModel = msgAlertSettingView.DataContext as MsgAlertSettingViewModel;
                    msgAlertSettingViewModel.ConfirmCommand.Execute(null) ;
                }
            }    
        }

        /// <summary>
        /// 更新风控设置
        /// </summary>
        private void UpdateRiskParaData()
        {
            try
            {

                DefaultRiskParameter dp = new DefaultRiskParameter();
                dp.qtyCheck = this.qtyCheck.ToString();
                dp.qtyMin = this.qtyMin ;
                dp.qtyMax = this.qtyMax ;

                dp.chinaBidCheck = this.chinaBidCheck.ToString();
                dp.chinaBid = this.chinaBid;
                dp.chinaOfrCheck = this.chinaOfrCheck.ToString();
                dp.chinaOfr = this.chinaOfr;

                dp.marketBidCheck = this.marketBidCheck.ToString();
                dp.marketBid = this.marketBid;
                dp.marketOfrCheck = this.marketOfrCheck.ToString();
                dp.marketOfr = this.marketOfr;

                dp.dropAwayFromBidCheck = this.dropAwayFromBidCheck.ToString();
                dp.dropAwayFromBid = this.dropAwayFromBid;
                dp.dropAwayFromOfrCheck = this.dropAwayFromOfrCheck.ToString();
                dp.dropAwayFromOfr = this.dropAwayFromOfr;

                dp.excutOrderBidCheck = this.excutOrderBidCheck.ToString();
                dp.excutOrderBid = this.excutOrderBid;
                dp.excutOrderOfrCheck = this.excutOrderOfrCheck.ToString();
                dp.excutOrderOfr = this.excutOrderOfr;

                string userid = LocalEnv.UserInfo.UserName;
                // userid = "gao.chao";
                RiskParaReq req = new RiskParaReq();
                req.userName = userid;
                req.defaultRiskParameter = dp;

                PResult result = TBookHandler.Instance.updateRiskPara(req);
                if (result.IsSuccess())
                {
                    //SystemParamHandler.Instance.CopyParam(us);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        MessageBoxGUI22 msgBox = new MessageBoxGUI22();
                        msgBox.setWarnMsg("保存成功", true);
                        msgBox.ShowDialog();
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            finally
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.SaveIsEnable = true;
                }));
            }
        }

        private void WarnBox_Closed(object sender, EventArgs e)
        {
            MessageBoxGUI22 win = sender as MessageBoxGUI22;
            win.Closed -= WarnBox_Closed;
            _warnBox = null;
            _isShowWarn = false;
        }

        private bool CheckParam()
        {
            bool result = true;

            decimal d = 0;
            if (decimal.TryParse(qtyMin, out d) && (d % 100) > 0)
                result = false;

            if (result)
            {
                if (decimal.TryParse(qtyMax, out d) && (d % 100) > 0)
                    result = false;
            }

            return result;
        }
        #endregion
    }
}
