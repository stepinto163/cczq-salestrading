using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Component;
using STC.Common.Handler.PT;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.PT;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommonUtils = STC.Common.Utils.CommonUtils;

namespace STC.Broker.Plugin.View.History
{
    /// <summary>
    /// OrderSearchUI.xaml 的交互逻辑
    /// </summary>
    public partial class OrderSearchUI : UserControl
    {
        public OrderSearchUI()
        {
            InitializeComponent();
            this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            this.Loaded += OrderSearchUI_Loaded;
            this.Unloaded += OrderSearchUI_Unloaded;

        }

        private void OrderSearchUI_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveFieldsShowOrder();
        }

        bool isloaded = false;
        private void OrderSearchUI_Loaded(object sender, RoutedEventArgs e)
        {
            if (isloaded)
                return;
            isloaded = true;
            //waterMark.Init(LocalEnv.UserInfo.FullName);
            //waterMark.Height = 0;
            setFieldsShowOrder();

            dpFrom.SelectedDate = DateTimeEx.ServerTime.AddDays(-1);
            dpTo.SelectedDate = DateTimeEx.ServerTime.AddDays(-1);
            string today = DateTime.Today.ToString(DateFormat);
            oldRequest.createTimeEnd = newRequest.createTimeEnd = dpFrom.SelectedDate.HasValue ? dpFrom.SelectedDate.Value.ToString(DateFormat) : today;
            oldRequest.createTimeEnd = newRequest.createTimeEnd = dpTo.SelectedDate.HasValue ? dpTo.SelectedDate.Value.ToString(DateFormat) : today;
            pageControl.Page = 1;
            pageControl.Page = 1;
            Button_Click(null, null);
            Task.Factory.StartNew(() =>
            {
                StartAutoFreshTask();
            });
            dgRecords.Sorting += new DataGridSortingEventHandler(SortHandler);
            this.dgRecords.ColumnDisplayIndexChanged += dgRecords_ColumnDisplayIndexChanged;

        }

        private void SortHandler(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;
            string headerName = column.Header.ToString();
            if ("剩余期限".Equals(headerName))
            {
                newRequest.maturityOrder = newRequest.maturityOrder + "" == "A" ? "D" : "A";
                Button_Click1(null, null);
            }
            e.Handled = true;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            SaveFieldsShowOrder();
        }
        /// <summary>
        /// 设置字段顺序
        /// </summary>
        void setFieldsShowOrder()
        {
            try
            {
                string temp = IniFileHelper.ReadValue("OrderSearchUI", "DisplayOrder");
                if (!string.IsNullOrEmpty(temp))
                {
                    List<string> ordes = temp.ToString().Split(new char[] { '-' }).ToList();
                    for (int i = 0; i < dgRecords.Columns.Count; i++)
                    {
                        int seq;
                        if (i < ordes.Count && int.TryParse(ordes[i], out seq))
                            dgRecords.Columns[i].DisplayIndex = seq;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e.ToString());
            }

        }
        /// <summary>
        /// 保存字段顺序
        /// </summary>
        void SaveFieldsShowOrder()
        {
            StringBuilder cloumnOrderStr = new StringBuilder();
            foreach (var cloumn in dgRecords.Columns)
            {
                if (string.IsNullOrEmpty(cloumnOrderStr.ToString()))
                    cloumnOrderStr.Append(cloumn.DisplayIndex.ToString());
                else
                    cloumnOrderStr = cloumnOrderStr.Append("-" + cloumn.DisplayIndex);
            }
            IniFileHelper.WriteValue("OrderSearchUI", "DisplayOrder", cloumnOrderStr.ToString());
        }
        private const string DateFormat = "yyyy-MM-dd";
        private const int pageSize = 40;
        GrExecOrderQueryReq oldRequest = new GrExecOrderQueryReq() { pageSize = 40, pageNum = 0, ownOrder = "N", queryHistType = "2" };
        GrExecOrderQueryReq newRequest = new GrExecOrderQueryReq() { pageSize = 40, pageNum = 0, ownOrder = "N", queryHistType = "2" };

        private void setRequest()
        {
            if (newRequest.securityid + "" != oldRequest.securityid + "")
            {
                newRequest.pageNum = 0;
                oldRequest.securityid = newRequest.securityid;
            }

            if (newRequest.institutionid + "" != oldRequest.institutionid + "")
            {
                newRequest.pageNum = 0;
                oldRequest.institutionid = newRequest.institutionid;
            }
            if (newRequest.counterpartyInstitutionid + "" != oldRequest.counterpartyInstitutionid + "")
            {
                newRequest.pageNum = 0;
                oldRequest.counterpartyInstitutionid = newRequest.counterpartyInstitutionid;
            }
            bool isFocus = false;
            if (string.IsNullOrWhiteSpace(newRequest.institutionid) && !string.IsNullOrWhiteSpace(ctbBank.Text))
            {
                ctbBank.Text = string.Empty;
                ctbBank.Focus();
                isFocus = true;
            }
            if (newRequest.traderid + "" != oldRequest.traderid + "")
            {
                newRequest.pageNum = 0;
                oldRequest.traderid = newRequest.traderid;
            }
            if (newRequest.counterpartyTraderid + "" != oldRequest.counterpartyTraderid + "")
            {
                newRequest.pageNum = 0;
                oldRequest.counterpartyTraderid = newRequest.counterpartyTraderid;
            }
            if (!isFocus && string.IsNullOrWhiteSpace(newRequest.traderid) && !string.IsNullOrWhiteSpace(ctbBankAgent.Text))
            {
                ctbBankAgent.Text = string.Empty;
                ctbBankAgent.Focus();
            }
            if (newRequest.ownOrder != oldRequest.ownOrder)
            {
                newRequest.pageNum = 0;
                oldRequest.ownOrder = newRequest.ownOrder;
            }
            if (newRequest.createTimeBegin != oldRequest.createTimeBegin)
            {
                newRequest.pageNum = 0;
                oldRequest.createTimeBegin = newRequest.createTimeBegin;
            }
            if (newRequest.createTimeEnd != oldRequest.createTimeEnd)
            {
                newRequest.pageNum = 0;
                oldRequest.createTimeEnd = newRequest.createTimeEnd;
            }
            oldRequest.pageNum = newRequest.pageNum;
            pageControl.CurrentPageNum = (int)newRequest.pageNum;
        }
        /// <summary>
        /// 查询按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (!btnSelect.IsEnabled)
                    return;
                btnSelect.IsEnabled = false;
            }
            //ShowMask(true);
            List<TransOrderServerData> orders = new List<TransOrderServerData>();
            newRequest.maturityOrder = "";
            setRequest();
            ThreadPool.QueueUserWorkItem(a =>
            {
                try
                {
                    List<TransOrderServerData> orderDatas = new List<TransOrderServerData>();
                    GrExecOrdersReplyPo reply = null;
                    newRequest.queryHistType = "2";
                    reply = GRHandler.Instance.ExecOrderHistQuery(newRequest);
                    if (reply != null && reply.Datas != null && reply.Datas.Count > 0)
                    {
                        LogHelper.Info($"GetExecOrderHistData Result: Return {reply.Datas.Count} Records; TotalRecord {reply.totalSize} Records.");
                        //计算总页数
                        int pageNum = reply.totalSize % pageSize == 0 ? (reply.totalSize / (int)pageSize) : (reply.totalSize / (int)pageSize + 1);
                        TransOrderHelper.Instance.ConvertBaseDataToTransData(orderDatas, reply.Datas);

                        orders = orderDatas;

                        this.Dispatcher.Invoke(() =>
                        {
                            if (orders != null && orders.Any())
                            {
                                //waterMark.Height = orders.Count * 30;
                                pageControl.Page = newRequest.pageNum + 1;
                                pageControl.PageCount = pageNum;
                                pageControl.PageSize = pageSize;
                                pageControl.RecordCount = reply.totalSize;
                                dgRecords.ItemsSource = orders;
                                tbInfo.Visibility = Visibility.Collapsed;
                                dgRecords.Visibility = Visibility.Visible;
                                ShowMask(false);
                                btnSelect.IsEnabled = true;
                            }
                            else
                            {
                                //waterMark.Height = 0;
                                oldRequest.pageNum = 0;
                                pageControl.PageCount = 1;
                                pageControl.PageSize = pageSize;
                                pageControl.RecordCount = 0;
                                dgRecords.ItemsSource = orders;
                                tbInfo.Visibility = Visibility.Collapsed;
                                dgRecords.Visibility = Visibility.Visible;
                                ShowMask(false);
                                btnSelect.IsEnabled = true;
                            }
                        });

                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            //waterMark.Height = 0;
                            oldRequest.pageNum = 0;
                            pageControl.PageCount = 1;
                            dgRecords.ItemsSource = orders;
                            tbInfo.Visibility = Visibility.Collapsed;
                            dgRecords.Visibility = Visibility.Visible;
                            ShowMask(false);
                            btnSelect.IsEnabled = true;
                        });
                    }
                }
                catch (ArgumentException ex)
                {
                    LogHelper.Error(ex.Message, ex);
                }
                catch (OverflowException ex2)
                {
                    LogHelper.Error(ex2.Message, ex2);
                }
            });
        }

        /// <summary>
        /// 排序查询 ，目前只支持剩余期限
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            //ShowMask(true);
            List<TransOrderServerData> orders = new List<TransOrderServerData>();
            setRequest();
            ThreadPool.QueueUserWorkItem(a =>
            {
                try
                {
                    List<TransOrderServerData> orderDatas = new List<TransOrderServerData>();
                    var reply = GRHandler.Instance.ExecOrderHistQuery(newRequest);
                    if (reply != null && "0000".Equals(reply.Code, StringComparison.Ordinal) && reply.Datas != null)
                    {
                        //计算总页数
                        int pageNum = reply.totalSize % pageSize == 0 ? (reply.totalSize / (int)pageSize) : (reply.totalSize / (int)pageSize + 1);
                        TransOrderHelper.Instance.ConvertBaseDataToTransData(orderDatas, reply.Datas);
                        orders = orderDatas;
                        this.Dispatcher.Invoke(() =>
                        {
                            if (orders != null && orders.Any())
                            {
                                //waterMark.Height = orders.Count * 30;
                                //oldRequest.pageNum = 0;
                                pageControl.Page = newRequest.pageNum + 1;
                                pageControl.PageCount = pageNum;
                                pageControl.PageSize = pageSize;
                                pageControl.RecordCount = reply.totalSize;
                                dgRecords.ItemsSource = orders;
                                tbInfo.Visibility = Visibility.Collapsed;
                                dgRecords.Visibility = Visibility.Visible;
                                dgtcTimeToMaturity.SortDirection = newRequest.maturityOrder + "" == "A" ? ListSortDirection.Ascending : ListSortDirection.Descending;
                                //ShowMask(false);
                            }
                            else
                            {
                                //waterMark.Height = 0;
                                oldRequest.pageNum = 0;
                                pageControl.PageCount = 1;
                                pageControl.PageSize = pageSize;
                                dgRecords.ItemsSource = orders;
                                tbInfo.Visibility = Visibility.Collapsed;
                                dgRecords.Visibility = Visibility.Visible;
                                // ShowMask(false);
                            }
                        });

                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            // waterMark.Height = 0;
                            oldRequest.pageNum = 0;
                            pageControl.PageCount = 1;
                            pageControl.PageSize = pageSize;
                            dgRecords.ItemsSource = orders;
                            tbInfo.Visibility = Visibility.Collapsed;
                            dgRecords.Visibility = Visibility.Visible;
                            //ShowMask(false);
                        });
                    }
                }
                catch (ArgumentException ex)
                {
                    LogHelper.Error(ex.Message, ex);
                }
                catch (OverflowException ex2)
                {
                    LogHelper.Error(ex2.Message, ex2);
                }
            });
        }


        private HisReocrdWin hisWin;
        /// <summary>
        /// 订单的历史流水
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnCash_Click(object sender, RoutedEventArgs e)
        {
            var po = dgRecords.SelectedItem as TransOrderServerData;
            if (po != null)
            {
                GrExecOrderQueryReq request = new GrExecOrderQueryReq();
                request.execid = po.ExecID;
                request.pageNum = 0;
                request.pageSize = 10000;
                GrExecOrdersReplyPo reply = await GetExecOrderHistRecordData(request);

                if (reply != null && "0000".Equals(reply.Code) && reply.Datas != null && reply.Datas.Any())
                {
                    List<TransOrderServerData> orderDatas = new List<TransOrderServerData>();
                    TransOrderHelper.Instance.ConvertBaseDataToTransData(orderDatas, reply.Datas);
                    if (hisWin == null)
                    {
                        hisWin = new HisReocrdWin();//Windows窗体
                        hisWin.Closed += new EventHandler(delegate (object ob, EventArgs ev) { hisWin = null; });
                        hisWin.Owner = Window.GetWindow(this);
                        hisWin.Show();
                    }
                    hisWin.RefreshData(orderDatas);
                }
            }
        }


        public async Task<GrExecOrdersReplyPo> GetExecOrderHistRecordData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecOrderHistRecordQuery(req);
            });

            return reply;
        }

        private void spBrokerA_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel tb = e.OriginalSource as StackPanel;
            SetBroker(tb, true);
        }

        private void spBrokerB_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel tb = e.OriginalSource as StackPanel;
            SetBroker(tb, false);


        }
        /// <summary>
        /// 设置broker的显示效果
        /// </summary>
        /// <param name="tb"></param>
        /// <param name="isA"></param>
        private void SetBroker(StackPanel tb, bool isA)
        {
            if (tb != null)
            {
                ContentPresenter cp = tb.TemplatedParent as ContentPresenter;

                if (cp != null)
                {
                    OrderDetailPo po = cp.Content as OrderDetailPo;
                    if (po != null)
                    {
                        foreach (var a in tb.Children)
                        {
                            //image
                            Image image = a as Image;
                            if (image != null)
                            {
                                if ("E".Equals(po.OrdStatus) && !isA)
                                {
                                    if (po.IsPartlyConfirm)
                                    {
                                        image.Source = new BitmapImage(new Uri("../Images/trade_tag_partly_confirmed.png", UriKind.Relative));
                                    }
                                    else
                                    {
                                        image.Source = new BitmapImage(new Uri("../Images/trade_tag_confirmed.png", UriKind.Relative));
                                    }
                                }
                                else if ("F".Equals(po.OrdStatus) && !isA)
                                {
                                    image.Source = new BitmapImage(new Uri("../Images/trade_tag_denied.png", UriKind.Relative));
                                }
                                else if ("G".Equals(po.OrdStatus))
                                {
                                    if (isA)
                                    {
                                        image.Source = new BitmapImage(new Uri("../Images/trade_tag_confirmed.png", UriKind.Relative));
                                    }
                                    else
                                    {
                                        if (po.IsPartlyConfirm)
                                        {
                                            image.Source = new BitmapImage(new Uri("../Images/trade_tag_partly_confirmed.png", UriKind.Relative));
                                        }
                                        else
                                        {
                                            image.Source = new BitmapImage(new Uri("../Images/trade_tag_confirmed.png", UriKind.Relative));
                                        }
                                    }
                                }
                                else if ("H".Equals(po.OrdStatus))
                                {
                                    if (isA)
                                    {
                                        image.Source = new BitmapImage(new Uri("../Images/trade_tag_denied.png", UriKind.Relative));
                                    }
                                    else
                                    {
                                        if (po.IsPartlyConfirm)
                                        {
                                            image.Source = new BitmapImage(new Uri("../Images/trade_tag_partly_confirmed.png", UriKind.Relative));
                                        }
                                        else
                                        {
                                            image.Source = new BitmapImage(new Uri("../Images/trade_tag_confirmed.png", UriKind.Relative));
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 价格标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spTag_Loaded(object sender, RoutedEventArgs e)
        {
            var sp = e.OriginalSource as StackPanel;
            if (sp != null)
            {
                ContentPresenter cp = sp.TemplatedParent as ContentPresenter;

                if (cp != null)
                {
                    TransOrderServerData po = cp.Content as TransOrderServerData;
                    if (po != null)
                    {
                        foreach (var a in sp.Children)
                        {
                            var spanel = a as StackPanel;
                            if (spanel != null)
                            {
                                SetTag(po, spanel);
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 设置价格标签
        /// </summary>
        /// <param name="spTag"></param>
        public void SetTag(TransOrderServerData po, StackPanel spTag)
        {
            try
            {
                spTag.Children.Clear();
                if ("1".Equals(po.BargainFlag))
                {
                    Image star = (Image)spTag.FindResource("price_x_Image");
                    spTag.Children.Add(star);
                }
                else if ("2".Equals(po.BargainFlag))
                {
                    Image star = (Image)spTag.FindResource("price_xx_Image");
                    spTag.Children.Add(star);
                }
                if ("1".Equals(po.OCOFlag))
                {
                    Image oco = (Image)spTag.FindResource("price_oco_Image");
                    spTag.Children.Add(oco);
                }
                else if ("2".Equals(po.OCOFlag))
                {
                    Image oco = (Image)spTag.FindResource("price_package_Image");
                    spTag.Children.Add(oco);
                }

                if (po.ExchangeSignFlag == "1" && Constant.BrokerFlags.Contains(LocalEnv.ConfInfo.BrokerNameFlag))
                {
                    Image oco = (Image)spTag.FindResource("ExchangeBond_Image");
                    spTag.Children.Add(oco);

                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void dpFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
            {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value)
                {
                    dpTo.SelectedDate = dpFrom.SelectedDate.Value;
                }
            }
            string today = DateTime.Today.ToString(DateFormat);
            newRequest.createTimeBegin = dpFrom.SelectedDate.HasValue ? dpFrom.SelectedDate.Value.ToString(DateFormat) : today;
        }

        private void dpTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
            {
                if (dpTo.SelectedDate.Value < dpFrom.SelectedDate.Value)
                {
                    dpFrom.SelectedDate = dpTo.SelectedDate.Value;
                }
            }
            string today = DateTime.Today.ToString(DateFormat);
            newRequest.createTimeEnd = dpTo.SelectedDate.HasValue ? dpTo.SelectedDate.Value.ToString(DateFormat) : today;
        }


        public void ShowMask(bool flag)
        {
            positionloadMask.Dispatcher.Invoke(() =>
            {
                positionloadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }
        /// <summary>
        /// 价格展示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spPrice_Loaded(object sender, RoutedEventArgs e)
        {
            var sp = e.OriginalSource as StackPanel;
            if (sp != null)
            {
                ContentPresenter cp = sp.TemplatedParent as ContentPresenter;

                if (cp != null)
                {
                    OrderDetailPo po = cp.Content as OrderDetailPo;
                    if (po != null)
                    {
                        foreach (var a in sp.Children)
                        {
                            var tbPrice = a as TextBlock;
                            if (tbPrice != null && !string.IsNullOrEmpty(po.InternalFlag) && po.InternalFlag.Equals("2"))
                            {
                                tbPrice.Foreground = (Brush)FindResource("scbActualPrice");
                            }
                        }
                    }
                }

            }
        }
        /// <summary>
        /// 页面改变处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void PagingControl_CurrentPageChangeEvent(object sender, CurrentPageChangeEventArgs e)
        //{
        //    int current = e.CurrentPage;
        //    if (newRequest.pageNum != current)
        //    {
        //        newRequest.pageNum = current;
        //        if (string.IsNullOrEmpty(newRequest.maturityOrder))
        //        {
        //            Button_Click(null, null);
        //        }
        //        else
        //        {
        //            Button_Click1(null, null);
        //        }
        //    }
        //}

        #region 债券搜索
        private void ctbBond_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!tbBCodePop.IsOpen)
                dgCodeResult_MouseDoubleClick(null, null);
        }
        private void ctbBond_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                if (tbBCodePop.IsOpen)
                    dgCodeResult_MouseDoubleClick(null, null);
            }
        }
        /// <summary>
        /// 券码更改的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ctbBond_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppUtils.IsInnerInput = true;
            newRequest.securityid = string.Empty;
            if (!string.IsNullOrEmpty(ctbBond.Text))
            {
                string param = ctbBond.Text.Replace("'", "");
                tbBCodePop.IsOpen = true;
                List<SimpleBondsInfoPo> dataPos = STC.Common.Utils.CommonUtils.GetBondGridData(param, 20);
                if (dataPos != null && dataPos.Count > 0)
                {
                    newRequest.securityid = param;
                }
                dgCodeResult.DataContext = dataPos;
            }
            else
            {
                dgCodeResult.DataContext = null;
                newRequest.securityid = null;
                newRequest.pageNum = 0;
                tbBCodePop.IsOpen = false;
            }
        }
        private void dgCodeResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SimpleBondsInfoPo dv = dgCodeResult.SelectedItem as SimpleBondsInfoPo;
            if (dv != null && !string.IsNullOrEmpty(ctbBond.Text))
            {
                ctbBond.Text = dv.Short_Name;
                tbBCodePop.IsOpen = false;
                newRequest.securityid = dv.Bond_ID;
                AppUtils.IsInnerInput = false;
            }
        }
        private void tbBCodePop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ctbBond.Text))
            {
                tbBCodePop.IsOpen = false;
                AppUtils.IsInnerInput = false;
            }
        }

        private void ctbBond_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgCodeResult.SelectedIndex > 0)
                    dgCodeResult.SelectedIndex--;
                dgCodeResult.ScrollIntoView(dgCodeResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgCodeResult.SelectedIndex < dgCodeResult.Items.Count - 1)
                    dgCodeResult.SelectedIndex++;
                dgCodeResult.ScrollIntoView(dgCodeResult.SelectedItem);
            }
            else if (e.Key == Key.Enter)
            {
                txtb.Focus();
                if (dgCodeResult.SelectedItem != null)
                {
                    dgCodeResult_MouseDoubleClick(null, null);
                }
                else
                {
                    tbBCodePop.IsOpen = false;
                }
            }
        }
        #endregion


        #region 机构
        private void ctbBank_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!tbInstPop.IsOpen)
                dgInstResult_MouseDoubleClick(null, null);
        }

        private void ctbBank_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //e.Handled = false;
                CrmInst dv = dgInstResult.SelectedItem as CrmInst;
                if (dv != null && !string.IsNullOrWhiteSpace(ctbBank.Text))
                {
                    ctbBank.Text = dv.TipsShortnamelist;
                    tbInstPop.IsOpen = false;
                    newRequest.counterpartyInstitutionid = newRequest.institutionid = dv.idbkey;
                    txtb.Focus();
                }
                else
                {
                    newRequest.counterpartyInstitutionid = newRequest.institutionid = string.Empty;
                }
            }
        }

        //同同时支持交易员和机构搜索
        //20200615 V4.4.0  机构交易员分开搜索，互不影响
        private void ctbBank_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ctbBank.Text))
            {
                string param = ctbBank.Text.Replace("'", "");
                tbInstPop.IsOpen = true;
                List<CrmInst> dataGridDatas = STC.Broker.Plugin.Utils.CommonUtils.GetInstitutionsTipsNameInfo(param, 20);
                if (dataGridDatas != null && dataGridDatas.Count > 0)
                {
                    dgInstResult.DataContext = dataGridDatas;
                    dgInstResult.SelectedIndex = 0;
                }
                else
                {
                    dgInstResult.DataContext = null;
                    tbInstPop.IsOpen = false;
                    newRequest.counterpartyInstitutionid = newRequest.institutionid = null;
                }
            }
            else
            {
                dgInstResult.DataContext = null;
                tbInstPop.IsOpen = false;
                newRequest.counterpartyInstitutionid = newRequest.institutionid = null;
            }
        }


        private void tbInstPop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ctbBank.Text))
                tbInstPop.IsOpen = false;
        }

        private void dgInstResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CrmInst dv = dgInstResult.SelectedItem as CrmInst;
            if (dv != null && !string.IsNullOrWhiteSpace(ctbBank.Text))
            {
                ctbBank.Text = dv.TipsShortnamelist;
                tbInstPop.IsOpen = false;
                newRequest.counterpartyInstitutionid = newRequest.institutionid = dv.idbkey;
                txtb.Focus();
            }
            else
            {
                newRequest.counterpartyInstitutionid = newRequest.institutionid = string.Empty;
            }
        }

        private void ctbBank_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgInstResult.SelectedIndex > 0)
                    dgInstResult.SelectedIndex--;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgInstResult.SelectedIndex < dgInstResult.Items.Count - 1)
                    dgInstResult.SelectedIndex++;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
        }



        #endregion

        #region 交易员
        private void tbTraderPop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ctbBankAgent.Text))
                tbTraderPop.IsOpen = false;
        }
        private void ctbBankAgent_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!tbTraderPop.IsOpen)
                dgTraderResult_MouseDoubleClick(null, null);
        }

        private void ctbBankAgent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgTraderResult.SelectedIndex > 0)
                    dgTraderResult.SelectedIndex--;
                dgTraderResult.ScrollIntoView(dgTraderResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgTraderResult.SelectedIndex < dgTraderResult.Items.Count - 1)
                    dgTraderResult.SelectedIndex++;
                dgTraderResult.ScrollIntoView(dgTraderResult.SelectedItem);
            }
        }
        private void ctbBankAgent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = false;
                DataGridDataPo dv = dgTraderResult.SelectedItem as DataGridDataPo;
                if (dv != null && !string.IsNullOrWhiteSpace(ctbBankAgent.Text))
                {
                    ctbBankAgent.Text = dv.InstTraderName;
                    tbTraderPop.IsOpen = false;
                    newRequest.counterpartyTraderid = newRequest.traderid = dv.IdbKey;
                    txtb.Focus();
                }
                else
                {
                    newRequest.counterpartyTraderid = newRequest.traderid = string.Empty;
                }
                //dgTraderResult_MouseDoubleClick(null, null);
            }
        }
        private void ctbBankAgent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ctbBankAgent.Text))
            {
                if (dgTraderResult.SelectedItem != null)
                {
                    DataGridDataPo dv = dgTraderResult.SelectedItem as DataGridDataPo;
                    if (dv != null && !string.IsNullOrWhiteSpace(ctbBankAgent.Text))
                    {
                        if (ctbBankAgent.Text == dv.InstTraderName)
                        {
                            return;
                        }
                    }
                }
                List<DataGridDataPo> dataList = STC.Broker.Plugin.Utils.CommonUtils.GetContactByContactName(ctbBankAgent.Text, 100);
                if (dataList == null || dataList.Count <= 0)
                {
                    dataList = STC.Broker.Plugin.Utils.CommonUtils.GetContactByInstName(ctbBankAgent.Text, 100, string.Empty);
                }
                if (dataList.Count > 0)
                {
                    dgTraderResult.DataContext = dataList;
                    if (!tbTraderPop.IsOpen)
                        tbTraderPop.IsOpen = true;
                    dgTraderResult.SelectedIndex = 0;
                }
                else
                {
                    dgTraderResult.DataContext = null;
                    tbTraderPop.IsOpen = false;
                    newRequest.counterpartyTraderid = newRequest.traderid = null;
                }
            }
            else
            {
                dgTraderResult.DataContext = null;
                tbTraderPop.IsOpen = false;
                newRequest.counterpartyTraderid = newRequest.traderid = null;
            }
        }
        private void dgTraderResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridDataPo dv = dgTraderResult.SelectedItem as DataGridDataPo;
            if (dv != null && !string.IsNullOrWhiteSpace(ctbBankAgent.Text))
            {
                ctbBankAgent.Text = dv.InstTraderName;
                tbTraderPop.IsOpen = false;
                newRequest.counterpartyTraderid = newRequest.traderid = dv.IdbKey;
                txtb.Focus();
            }
            else
            {
                newRequest.counterpartyTraderid = newRequest.traderid = string.Empty;
            }
        }
        #endregion

        private void cbMyDeals_Click(object sender, RoutedEventArgs e)
        {
            newRequest.ownOrder = (bool)cbMyDeals.IsChecked ? "Y" : "N";
        }

        /// <summary>
        /// 当前控件所属tabitem是否被选中
        /// </summary>
        /// <returns></returns>
        public bool IsSelected()
        {
            bool flag;
            TabItem item = this.Parent as TabItem;
            if (item != null)
                flag = item.IsSelected;
            else
                flag = false;
            return flag;
        }
        private System.Threading.Timer timer = null;
        /// <summary>
        /// 自动刷新任务
        /// </summary>
        void StartAutoFreshTask()
        {
            //等待系统参数初始化完成
            while (!TransOrderHandler.Instance.BondInitCompleted)
            {
                Thread.Sleep(100);
            }
            int autoFreshTime = 0;
            int.TryParse(LocalEnv.ConfInfo.AutomaticQueryTime.ToString(), out autoFreshTime);
            if (autoFreshTime > 0)
            {
                timer = new System.Threading.Timer(new TimerCallback(freshData), null, 1000, autoFreshTime * 1000);
            }
        }

        void freshData(object obj)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!IsSelected())
                    return;
                if (string.IsNullOrEmpty(newRequest.maturityOrder))
                {
                    Button_Click(null, null);
                }
                else
                {
                    Button_Click1(null, null);
                }
            });
        }
        string getFieldByHeaderName(string headerName)
        {
            switch (headerName)
            {
                case "剩余期限":
                    return "timeToMaturity";
                default:
                    return "";
            }
        }
        string getHeaderNameByField(string field)
        {
            switch (field)
            {
                case "timeToMaturity":
                    return "剩余期限";
                default:
                    return "";
            }
        }

        private void EMSPageControl_PageChanged(object sender, Common.View.Component.PageChangedEventArgs e)
        {
            int current = e.Page;
            if (newRequest.pageNum != current - 1)
            {
                newRequest.pageNum = current - 1;
                if (string.IsNullOrEmpty(newRequest.maturityOrder))
                {
                    Button_Click(null, null);
                }
                else
                {
                    Button_Click1(null, null);
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UIElement uiElement = (UIElement)this.Content;
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(uiElement);
            if (adornerLayer != null)
            {
                adornerLayer.Add(new WatermarkAdorner(uiElement, LocalEnv.UserInfo.FullName));
            }
        }

        private void dgRecords_ColumnHeaderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            //// 获取被拖动的列标题控件
            //var columnHeader = (DataGridColumnHeader)sender;

            //// 获取被拖动的列索引
            //int draggedIndex = dgRecords.Columns.IndexOf((DataGridColumn)columnHeader.Tag);

            //// 获取目标位置的列索引（如果有）
            //DataGridColumn targetColumn = null;
            //if (e.OriginalSource is FrameworkElement element && element.Parent is DataGridColumnHeader header)
            //    targetColumn = (DataGridColumn)header.Tag;

            //int targetIndex = -1;
            //if (targetColumn != null)
            //    targetIndex = dgRecords.Columns.IndexOf(targetColumn);
            //SaveFieldsShowOrder();
        }

        private void dgRecords_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            SaveFieldsShowOrder();
        }
    }
}
