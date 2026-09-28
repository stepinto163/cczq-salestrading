using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Common.Handler.Base;
using STC.Common.Handler.Cal;
using STC.Common.Handler.PT;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using CommonUtils = STC.Common.Utils.CommonUtils;

namespace STC.Broker.Plugin.View.Transaction.UnderLineExec
{
    /// <summary>
    /// UnderLineExecWin.xaml 的交互逻辑
    /// 线下录入窗口
    /// </summary>
    public partial class UnderLineExecWin : UserControl
    {
        private readonly string UnderlineExec = "UnderlineExec";
        private readonly string Order = "Order";
        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        private readonly string FormatItem = "FormatItem";
        private Dictionary<int, OrderDetailPo> orderDic = new Dictionary<int, OrderDetailPo>();
        private Dictionary<string, TBondValuationPo> valuationDic = new Dictionary<string, TBondValuationPo>();
        //记录当前输入内容
        private Dictionary<int, string> dictextContext = new Dictionary<int, string>();
        /// <summary>
        /// 当前选中的格式：利率台格式和信用台格式
        /// </summary>
        private int CurrentFromat = 1;
        private bool IsInit = true;
        public UnderLineExecWin()
        {
            InitializeComponent();
            this.Loaded += UnderLineExecWin_Loaded;
            //this.Closed += UnderLineExecWin_Closed;
            //this.Icon = LocalEnv.ServerInfo.AppImage;
        }

        //延迟加载textChange
        DispatcherTimer timer = new DispatcherTimer();
        private bool TimerElapsed = false;
        private bool KeysPressed = false;

        public void UnderLineExecWin_Closed(object sender, EventArgs e)
        {
            //this.Closed -= UnderLineExecWin_Closed;
            this.btnClose_Click(null, null);
        }

        private void UnderLineExecWin_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //V2.5 取消利率台格式，仅保留信用台格式，筛选格式的下拉框取消sssssss
                //comboBoxServer.Items.Add("利率台格式");
                //comboBoxServer.Items.Add("信用台格式");
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += new EventHandler(DelayTimer_Elapsed);

                this.Loaded -= UnderLineExecWin_Loaded;
                this.tbOrder.TextChanged -= tbOrder_TextChanged;
                //this._loading.Visibility = Visibility.Visible;
                string formatItem = IniFileHelper.ReadValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), FormatItem);
                ////if (string.IsNullOrEmpty(formatItem))
                ////{
                ////    CurrentFromat = 1;
                ////    comboBoxServer.SelectedIndex = 0;
                ////}
                ////else
                ////{
                ////    comboBoxServer.Text = formatItem;
                ////    CurrentFromat = comboBoxServer.Items.IndexOf(formatItem) + 1;
                ////}

                string historyOrder = IniFileHelper.ReadValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), Order);
                // 如果从微信发过来的的文本无法识别换行符替换为当前换行符
                //tbOrder.Text = tbOrder.Text.Replace("\n", System.Environment.NewLine);
                tbOrder.Text = historyOrder.Replace("||", System.Environment.NewLine);


                //string height = IniFileHelper.ReadValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), WinHeight);
                //string width = IniFileHelper.ReadValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), WinWidth);

                //double d = 0;
                //if (double.TryParse(height, out d))
                //    this.Height = d;
                //if (double.TryParse(width, out d))
                //    this.Width = d;

                //if (LocalEnv.ConfInfo.ExecPosition)
                //{
                //    string positionX = IniFileHelper.ReadValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), PositionX);
                //    string positionY = IniFileHelper.ReadValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), PositionY);
                //    double dY = 0;
                //    if (double.TryParse(positionX, out d) && double.TryParse(positionY, out dY))
                //    {
                //        bool isContain = false;
                //        foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                //        {
                //            if (sc.WorkingArea.Contains(new System.Drawing.Point((int)d, (int)dY)))
                //            {
                //                isContain = true;
                //                break;
                //            }
                //        }

                //        if (isContain)
                //        {
                //            this.Top = dY;
                //            this.Left = d;
                //        }
                //    }
                //}
                ////tbOrder.Text = tbOrder.Text.Replace(System.Environment.NewLine, "\n");
                string[] lines = tbOrder.Text.Split(new string[] { System.Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(o => !string.IsNullOrWhiteSpace(o)).ToArray();
                if (lines != null && lines.Length > 0)
                {
                    //加入字典表
                    int i = 1;
                    foreach (string item in lines)
                    {
                        dictextContext.Add(i, item.Trim());
                        i++;
                    }
                }

                if (lines != null && lines.Length > 0)
                {
                    Task.Factory.StartNew(() => { InitOrder(lines); });
                }
                else
                {
                    //this._loading.Visibility = Visibility.Collapsed;
                }

                //InitOrder();
                this.tbOrder.TextChanged += tbOrder_TextChanged;

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                IsInit = false;
            }
        }


        private void DelayTimer_Elapsed(object sender, EventArgs e)
        {

            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            TimerElapsed = true;
            UpdateUnderLineOrder();

        }
        private void PlaceExecOrder()
        {
            int insertCount = 0;
            if (dictextContext != null && dictextContext.Count > 0)
            {
                //打印线下录入日志
                LogHelper.Info("--------------线下录入原文本开始--------------");
                LogHelper.Info(tbOrder.Text);
                LogHelper.Info("--------------线下录入原文本结束--------------");
                foreach (OrderDetailPo orderDetail in orderDic.Values)
                {
                    insertCount++;
                    if (string.IsNullOrWhiteSpace(orderDetail.SecurityID))
                    {
                        continue;
                    }
                    if (insertCount > dictextContext.Count)
                    {
                        break;
                    }
                    //PlaceOrderRequestPo po = new PlaceOrderRequestPo();
                    ExecutionReport po = new ExecutionReport();                  
                    po.InstitutionID = orderDetail.InstitutionID;
                    po.InstitutionName = orderDetail.InstitutionName;
                    po.PartyName = orderDetail.PartyName;
                    po.PartySubID = orderDetail.PartySubID;
                    po.PartySubName = orderDetail.PartySubName;
                    po.TraderID = orderDetail.TraderID;
                    po.TraderName = orderDetail.TraderName;            

                    po.CounterPartyInstitutionId = orderDetail.CounterPartyInstitutionId; 
                    po.CounterPartyInstitutionName = orderDetail.CounterPartyInstitutionName;
                    po.CounterPartyName = orderDetail.CounterPartyName;
                    po.CounterPartySubID = orderDetail.CounterPartySubID;
                    po.CounterPartySubName = orderDetail.CounterPartySubName;
                    po.CounterPartyTraderId = orderDetail.CounterPartyTraderId;
                    po.CounterPartyTraderName = orderDetail.CounterPartyTraderName;
                    //po.rule = "MA";

                    po.TimeToMaturity = orderDetail.timeToMaturity;


                    po.Symbol= orderDetail.goodsShortName;

                    //po.clientOrderId = IDC.Frame.Utils.DataTimeSequence.Instance.GetId();
                    po.SecurityID = orderDetail.SecurityID;
                    //po.remark = po.goodsCode.EndsWith(".SH") || po.goodsCode.EndsWith(".SZ") ? "交易所" : "";
                    po.OrdType = "2";
                    po.OrderQty= po.LastQty = orderDetail.qty == null ? "" : orderDetail.qty.Value + "";
                    //po.qty = orderDetail.qty;
                    po.Price = orderDetail.price == null ? "" : orderDetail.price.Value + "";
                    po.Side = orderDetail.IsGVN ? "2" : "1";//"1" : "2";
                    po.SettlType = TransOrderHelper.Instance.GetSettleValue(orderDetail.SettlType);
                    //orderDetail.settlementType;
                    po.SettlType2 = TransOrderHelper.Instance.GetSettleValue(orderDetail.SettlType2);
                    //orderDetail.settlementtype2;
                    //po.PartyName = orderDetail.OfrInst;
                    //po.PartySubName = orderDetail.OfrBroker;
                    //po.CounterPartySubName = orderDetail.BidBroker;
                    po.OrdStatus = "2";
                    // po.terminal = string.Format("BorkerClient_v{0}", ReflectionHelper.GetFrameVersion(this));
                    //po.tif = "3";
                    po.FirmFlag = intentionCheckBox.IsChecked.HasValue && intentionCheckBox.IsChecked.Value ? "0" : "1";//"2" : "1";
                    po.OnlineA = po.OnlineB = "3";//1-线下, 2-线上,3-线下录入
                    po.HandlInst = "2"; //2明盘 1是暗盘
                    if (!string.IsNullOrEmpty(orderDetail.SettlType) || !string.IsNullOrEmpty(orderDetail.SettlType2))
                    {
                        CalSettleDateRequestPo request = new CalSettleDateRequestPo();
                        request.SettlementType = orderDetail.SettlType;

                        CalSettleDateRequestPo request2 = new CalSettleDateRequestPo();
                        request2.SettlementType = orderDetail.SettlType2;
                        CalDate(orderDetail, CalSettleDate(request), CalSettleDate(request2));
                    }
                    po.TradeDate = orderDetail.dealdate;
                    po.TradeDate2 = orderDetail.DealDate2;
                    po.SettlDate = orderDetail.SettlDate;
                    po.SettlDate2 = orderDetail.SettlDate2;
                    //po.cou
                    sendOrder(po);
                }
            }
        }

        private void sendOrder(ExecutionReport po)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                var reply = GRHandler.Instance.ExecOrderSave(po);

            });
        }


        //private bool IsShowWarn = false;
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Empty;
            bool isCheck = false;
            int i = 0;
            int k = 0;
            if (dictextContext != null && dictextContext.Count > 0)
            {
                foreach (OrderDetailPo orderDetail in orderDic.Values)
                {
                    i++;
                    if (i > dictextContext.Count)
                    {
                        break;
                    }
                    if (orderDetail.qty == null || orderDetail.qty <= 0)
                    {
                        k++;
                        msg += $"      第{i}条数量为空\r\n";
                        if (k >= 5)
                        {
                            break;
                        }
                    }
                }
            }

            if (k > 0)
            {
                SysWarnGUI warn = new SysWarnGUI();
                Window win = Window.GetWindow(this);
                if (win != null)
                    warn.Owner = win;
                warn.tbTitle.Text = "提示";
                warn.tbMsg.Text = msg;
                warn.ShowDialog();
            }
            else
            {
                PlaceExecOrder();

                tbOrder.Text = string.Empty;
                this.spExec.Children.Clear();

                orderDic.Clear();

                IniFileHelper.WriteValue(UnderlineExec, Order, tbOrder.Text);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
            //tbOrder.Text = tbOrder.Text.Replace(System.Environment.NewLine, "\n");
            //防止再次出发textchanged事件
            string textContent = tbOrder.Text.Replace(System.Environment.NewLine, "||");
            IniFileHelper.WriteValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), Order, textContent);

            //IniFileHelper.WriteValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), WinHeight, this.Height + "");
            //IniFileHelper.WriteValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), WinWidth, this.Width + "");

            IniFileHelper.WriteValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), FormatItem, "信用台格式");

            //if (LocalEnv.ConfInfo.ExecPosition)
            //{
            //    IniFileHelper.WriteValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), PositionX, this.Left + "");
            //    IniFileHelper.WriteValue(string.Format("{0}_{1}", UnderlineExec, LocalEnv.UserInfo.UserId), PositionY, this.Top + "");
            //}

            //if (this.Owner != null)
            //    this.Owner.Focus();
        }



        //private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    this.DragMove();
        //}

       
        private Separator GetSeparatorByName(int indexControl)
        {
            Separator result = null;
            int index = 0;
            foreach (UIElement ui in spExec.Children)
            {
                Separator execUI = ui as Separator;
                if (execUI != null)
                {
                    index++;
                    if (indexControl == index)
                    {
                        result = execUI;
                        break;
                    }
                }
            }
            return result;
        }

        private UnderLineExecUI GetUnderLineExecUIByControlIndex(int controlIndex)
        {
            UnderLineExecUI result = null;
            int index = 0;
            foreach (UIElement ui in spExec.Children)
            {
                UnderLineExecUI execUI = ui as UnderLineExecUI;
                if (execUI != null)
                {
                    index++;
                    if (controlIndex == index)
                    {
                        result = execUI;
                        break;
                    }
                }
            }
            return result;
        }

        private int ComputeRowIndex()
        {
            int rowIndex = 0;

            string txt = tbOrder.Text;
            int inputIndex = tbOrder.SelectionStart;

            string[] lines = tbOrder.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines == null || lines.Length == 1)
                rowIndex = 1;
            else
            {
                int total = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    total += lines[i].Length + "\n".Length;
                    if (total >= inputIndex || ((total + "\n".Length) == inputIndex))
                    {
                        rowIndex = i + 1;
                        break;
                    }
                }
            }

            return rowIndex;
        }

        private string GetInputeLine(int index)
        {
            string lineText = "";
            string[] lines = tbOrder.Text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() > index)
            {
                lineText = lines[index];
            }
            return lineText;
        }

        #region 重写逻辑

        private void InitOrder(string[] lines)
        {
            //string[] lines = tbOrder.Text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string tem = lines[i].Replace("出给", " ");
                    string[] line = tem.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    int inputLineIndex = i + 1;
                    LoadUI(inputLineIndex, line, lines.Length);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                this.Dispatcher.Invoke(() =>
                {
                    //this._loading.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void LoadUI(int inputLineIndex, string[] line, int totalCount)
        {
            OrderDetailPo changeOrder = null;
            if (orderDic.ContainsKey(inputLineIndex))
                changeOrder = orderDic[inputLineIndex];
            else
            {
                changeOrder = new OrderDetailPo();
                changeOrder.Side = "2";
                changeOrder.SettlType = "明天+0";
                orderDic.Add(inputLineIndex, changeOrder);
            }
            //信用台格式解析
            Analysis(inputLineIndex, changeOrder, line);
            //推送UI线程
            this.Dispatcher.Invoke(() =>
            {
                UnderLineExecUI execUI = new UnderLineExecUI();
                execUI.Index = inputLineIndex;
                if (spExec.Children.Count > 0)
                {
                    Separator separator = new Separator();
                    separator.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    separator.Opacity = 0.3;
                    separator.Margin = new Thickness(0, 1, 0, 1);
                    spExec.Children.Add(separator);

                    spExec.Children.Add(execUI);
                }
                else
                {
                    spExec.Children.Add(execUI);
                }
                execUI.orderDetailPo = changeOrder;
                execUI.RefreshUI();
                if (inputLineIndex >= totalCount)
                {
                    //this._loading.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void RefreshUI(int inputLineIndex, string[] line, int totalCount)
        {
            OrderDetailPo changeOrder = null;
            if (orderDic.ContainsKey(inputLineIndex))
                changeOrder = orderDic[inputLineIndex];
            else
            {
                changeOrder = new OrderDetailPo();
                changeOrder.Side = "2";
                //信用台模式下的默认结算方式为明天 + 0，默认的量为3000W
                changeOrder.SettlType = "明天+0";
                orderDic.Add(inputLineIndex, changeOrder);
            }
            //信用台格式解析
            Analysis(inputLineIndex, changeOrder, line);
            //如果债券代码未识别显示
            //if (!string.IsNullOrWhiteSpace(changeOrder.goodsCode))
            //{
            this.Dispatcher.Invoke(() =>
            {
                UnderLineExecUI execUI = GetUnderLineExecUIByControlIndex(inputLineIndex);
                if (execUI == null)
                {
                    execUI = new UnderLineExecUI();
                    execUI.Index = inputLineIndex;
                    if (spExec.Children.Count > 0)
                    {
                        Separator separator = new Separator();
                        separator.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                        separator.Opacity = 0.3;
                        separator.Margin = new Thickness(0, 1, 0, 1);
                        separator.Name = "line" + inputLineIndex.ToString();
                        spExec.Children.Add(separator);

                        spExec.Children.Add(execUI);
                    }
                    else
                        spExec.Children.Add(execUI);
                }
                else
                    execUI.Index = inputLineIndex;
                execUI.orderDetailPo = changeOrder;
                execUI.RefreshUI();
                if (inputLineIndex >= totalCount)
                {
                    //this._loading.Visibility = Visibility.Collapsed;
                }
            });
            //}

        }

        private void tbOrder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInit && !inputFlag)
                return;
            if (!timer.IsEnabled)
            {
                //开始计时
                timer.Start();
            }
            else
            {
                //计时器正在运行重新计时
                timer.Stop();
                timer.Start();
            }
        }

        /// <summary>
        /// 更新线下单子
        /// </summary>
        private void UpdateUnderLineOrder()
        {
            //将要更新的数据放入数据字典中
            Dictionary<int, string> dicUpdateContent = new Dictionary<int, string>();
            //是否清空
            bool isClear = false;
            bool isDelete = false;
            int oldLineCount = 0;
            try
            {
                if (string.IsNullOrEmpty(tbOrder.Text.Trim()))
                {
                    orderDic.Clear();
                    spExec.Children.Clear();
                    dicUpdateContent.Clear();
                    dictextContext.Clear();
                    isClear = true;
                }
                else
                {
                    isClear = false;
                    //tbOrder.Text = tbOrder.Text.Replace(System.Environment.NewLine,"\n");
                    string[] lines = tbOrder.Text.Split(new string[] { System.Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(o => !string.IsNullOrWhiteSpace(o)).ToArray();
                    oldLineCount = lines.Length;
                    if (lines != null && oldLineCount > 0)
                    {
                        //新增或者更新
                        if (lines.Length >= dictextContext.Count)
                        {
                            if (dictextContext.Count > 0)
                            {
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    if (i < dictextContext.Count)
                                    {
                                        if (lines[i].Trim() != dictextContext[i + 1])
                                        {
                                            //需要更新
                                            dicUpdateContent.Add(i + 1, lines[i]);
                                        }
                                    }
                                    else
                                    {
                                        //需要更新
                                        dicUpdateContent.Add(i + 1, lines[i]);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    //全部要更新
                                    dicUpdateContent.Add(i + 1, lines[i]);
                                }
                            }
                            isDelete = false;
                        }
                        else
                        {
                            isDelete = true;
                            //有删除行
                            for (int i = 0; i < lines.Length; i++)
                            {
                                //全部要更新
                                dicUpdateContent.Add(i + 1, lines[i]);
                                //spExec.Children.Clear();
                            }
                        }
                        //比较完成以后将dictextContext从新赋值
                        //清空以前数据
                        dictextContext.Clear();
                        if (lines != null && lines.Length > 0)
                        {
                            //加入字典表
                            int i = 1;
                            foreach (string item in lines)
                            {
                                dictextContext.Add(i, item.Trim());
                                //生成对象
                                OrderDetailPo changeOrder = null;
                                changeOrder = new OrderDetailPo();
                                changeOrder.Side = "2";
                                //信用台模式下的默认结算方式为明天 + 0，默认的量为3000W
                                changeOrder.SettlType = "明天+0";
                                i++;
                            }
                        }
                    }
                }
                int count = oldLineCount;
                bool hasdelete = isDelete;
                Dictionary<int, string> dicUpdate = new Dictionary<int, string>();
                foreach (var item in dicUpdateContent)
                {
                    dicUpdate.Add(item.Key, item.Value);
                }
               UpdateUnderLineShow(dicUpdate, hasdelete, count, isClear);
                //Task.Factory.StartNew(() => UpdateUnderLineShow(dicUpdate, hasdelete, count, isClear));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }


        private void UpdateUnderLineShow(Dictionary<int, string> dicUpdateContent, bool isDelete, int oldLineCount, bool isClear)
        {
            try
            {
                if (isClear)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        orderDic.Clear();
                        spExec.Children.Clear();
                        dicUpdateContent.Clear();
                        dictextContext.Clear();
                    });
                }
                else
                {
                    if (dicUpdateContent.Count > 0)
                    {
                        string[] linesContent = dicUpdateContent.Values.ToArray();
                        foreach (var item in dicUpdateContent)
                        {
                            string content = item.Value;
                            byte[] space = new byte[] { 0xc2, 0xa0 };
                            String UTFSpace = Encoding.UTF8.GetString(space);
                            content = item.Value.Replace(UTFSpace, " ");

                            string[] linesContext = content.Split(new string[] { " ", " " }, StringSplitOptions.RemoveEmptyEntries);
                            RefreshUI(item.Key, linesContext, dicUpdateContent.Count);
                            if (!isDelete)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    double verticalOffset = 0;//父边框高度
                                                              //计算滚动条高度
                                    UnderLineExecUI execUI = GetUnderLineExecUIByControlIndex(item.Key);
                                    if (execUI != null)
                                    {
                                        //获取偏移量
                                        var vector = VisualTreeHelper.GetOffset(execUI);
                                        verticalOffset = vector.Y;
                                        if (item.Key > 3 && verticalOffset == 0)
                                        {
                                            verticalOffset = 63 * (item.Key - 1);
                                        }

                                        //偏移量比例计算
                                        if (this.scrList.ScrollableHeight > 0)
                                        {
                                            verticalOffset = Convert.ToDouble(item.Key - 1) * this.scrList.ScrollableHeight / Convert.ToDouble(oldLineCount - 1);
                                            this.scrList.ScrollToVerticalOffset(verticalOffset);
                                        }
                                    }
                                });
                            }
                        }
                    }
                    //有删除时移除多余的线下解析
                    if (isDelete)
                    {
                        List<Separator> separators = new List<Separator>();
                        this.Dispatcher.Invoke(() =>
                        {
                            if (oldLineCount > 0)
                            {
                                int spExecCount = 0;
                                foreach (var item in spExec.Children)
                                {
                                    UnderLineExecUI execUI = item as UnderLineExecUI;
                                    if (execUI != null)
                                    {
                                        spExecCount++;
                                    }
                                }
                                for (int k = oldLineCount + 1; k <= spExecCount; k++)
                                {
                                    UnderLineExecUI execUI = GetUnderLineExecUIByControlIndex(oldLineCount + 1);
                                    if (execUI != null)
                                    {
                                        spExec.Children.Remove(execUI);
                                    }
                                    Separator separator = GetSeparatorByName(oldLineCount);
                                    if (separator != null)
                                    {
                                        spExec.Children.Remove(separator);
                                    }
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                this.Dispatcher.Invoke(() =>
                {
                    //this._loading.Visibility = Visibility.Collapsed;
                });
            }
        }



        private BondsInfoPo GetBestBond(string goodscode, int resultNum)
        {
            BondsInfoPo bestBond = null;
            SimpleBondsInfoPo tempBestBond = null;
            List<SimpleBondsInfoPo> dtBond = STC.Common.Utils.CommonUtils.GetBondGridData(goodscode, 20);//CRMService.Instance.GetBondGridData(goodscode, 20);
            if (dtBond.Count > 0)
            {
                int bestLength = 100;
                foreach (SimpleBondsInfoPo bg in dtBond)
                {
                    if (bg.Bond_ID.Length < bestLength)
                    {
                        bestLength = bg.Bond_ID.Length;
                        tempBestBond = bg;
                    }
                }
            }
            if (tempBestBond != null)
            {
                bestBond = CommonUtils.GetBondsInfoPoBySecurityID(tempBestBond.Bond_ID);
            }
            return bestBond;
        }

        private BondsInfoPo GetBestBondByFlag(string goodscode, int resultNum, out int flag)
        {
            BondsInfoPo bestBond = null;
            SimpleBondsInfoPo tempBestBond = null;
            List<SimpleBondsInfoPo> dtBond = STC.Common.Utils.CommonUtils.GetBondGridData(goodscode, 20);
            //CRMService.Instance.GetBondGridDataByFlag(goodscode, 20, out flag);
            flag = 3;
            if (dtBond.Count > 0)
            {
                if (flag == 1)
                {
                    BondsByID = dtBond;

                }
                else if (flag == 2)
                {
                    BondsByName = dtBond;
                }
                int bestLength = 100;
                foreach (SimpleBondsInfoPo bg in dtBond)
                {
                    if (bg.Bond_ID.Length < bestLength)
                    {
                        bestLength = bg.Bond_ID.Length;
                        tempBestBond = bg;
                    }
                }
            }
            if (tempBestBond != null)
            {
                bestBond = CommonUtils.GetBondsInfoPoBySecurityID(tempBestBond.Bond_ID);
            }
            return bestBond;
        }
        /// <summary>
        /// 赋值Trader
        /// </summary>
        /// <param name="changeOrder"></param>
        /// <param name="lineData"></param>
        /// <param name="isA"></param>
        private void setTraderVal(OrderDetailPo changeOrder, string lineData, bool isA)
        {
            DataTable temDt = new DataTable();//CRMService.Instance.GetContactByName(lineData, 10);//CRMDataHandler.Instance.SearchContact(lineData, 10);
            if (temDt.Rows.Count > 0)
            {
                foreach (DataRow dr in temDt.Rows)
                {
                    string instid = dr["instid"] + "";

                    if (isA)
                    {
                        if (!string.IsNullOrEmpty(changeOrder.InstitutionID) && instid.Equals(changeOrder.InstitutionID))
                        {
                            changeOrder.TraderName = (dr["namecn"] + "").Trim();
                            changeOrder.TraderID = dr["idbkey"] + "";
                            break;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(changeOrder.CounterPartyInstitutionId) && instid.Equals(changeOrder.CounterPartyInstitutionId))
                        {
                            changeOrder.CounterPartyTraderName = (dr["namecn"] + "").Trim();
                            changeOrder.CounterPartyTraderId = dr["idbkey"] + "";
                            break;
                        }
                    }
                }
                //string instid = temDt.Rows[0]["instid"] + "";

                //if (isA)
                //{
                //    if (!string.IsNullOrEmpty(changeOrder.bankIdA) && instid.Equals(changeOrder.bankIdA))
                //    {
                //        changeOrder.bankAgentNameA = (temDt.Rows[0]["namecn"] + "").Trim();
                //        changeOrder.bankagentIdA = temDt.Rows[0]["idbkey"] + "";
                //    }
                //}
                //else
                //{
                //    if (!string.IsNullOrEmpty(changeOrder.bankIdB) && instid.Equals(changeOrder.bankIdB))
                //    {
                //        changeOrder.bankAgentNameB = (temDt.Rows[0]["namecn"] + "").Trim();
                //        changeOrder.bankAgentIdB = temDt.Rows[0]["idbkey"] + "";
                //    }
                //}
            }
            else
            {
                if (isA)
                    changeOrder.TraderName = changeOrder.TraderID = string.Empty;
                else
                    changeOrder.CounterPartyTraderName = changeOrder.CounterPartyTraderId = string.Empty;
            }
        }

        /// <summary>
        /// 赋值Broker
        /// </summary>
        /// <param name="changeOrder"></param>
        /// <param name="lineData"></param>
        /// <param name="isA"></param>
        private void setBrokerVal(OrderDetailPo changeOrder, string lineData, bool isA)
        {
            BondsInfoPo bestBond = new BondsInfoPo();//CRMService.Instance.GetBestBond(changeOrder.goodsCode, 20);
            DataTable temDt = new DataTable();//CRMService.Instance.GetBrokerByGoodstype(lineData, null, 1, bestBond == null ? string.Empty : bestBond.Goodstype);//CRMDataHandler.Instance.SearchBroker3(lineData, 1);
            if (temDt.Rows.Count > 0)
            {
                if (isA)
                {
                    changeOrder.PartySubID = temDt.Rows[0]["Idb_Key"] + "";
                    changeOrder.PartySubName = temDt.Rows[0]["AccountName"] + "";
                }
                else
                {
                    changeOrder.CounterPartySubID = temDt.Rows[0]["Idb_Key"] + "";
                    changeOrder.CounterPartySubName = temDt.Rows[0]["AccountName"] + "";
                }
            }
            else
            {
                if (isA)
                    changeOrder.PartySubID = changeOrder.PartySubName = string.Empty;
                else
                    changeOrder.CounterPartySubID = changeOrder.CounterPartySubName = string.Empty;
            }
        }

       

        private bool IsContain(string goodstype)
        {
            List<string> termidsList = LocalEnv.UserInfo.Termids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (termidsList != null && termidsList.Count > 0)
                return termidsList.Contains(goodstype);

            return false;
        }


        private string GetInstName(CrmInst dr, string goodcode)
        {
            string result = string.Empty;

            if (dr == null)
                return result;

            BondsInfoPo bg = GetBestBond(goodcode, 20);
            string goodstype = bg.SECURITY_TYPE;//Goodstype
            if (dr == null)
                return result;
            result = Utils.CommonUtils.GetInstName(dr, goodstype);
            //if (!string.IsNullOrEmpty(goodstype))
            //{
            //    if (goodstype == "2")
            //        result = dr.bncshortnamelist+ "";
            //    else if (goodstype == "1" || goodstype == "11")
            //        result = dr.bcoshortnamelist + "";
            //    else if (goodstype == "12")
            //        result = dr.ncdshortnamelist + "";
            //}
            //else
            //{
            //    if (IsContain("2"))
            //    {
            //        result = dr.bncshortnamelist + "";
            //        if (string.IsNullOrEmpty(result))
            //        {
            //            if (IsContain("1") || IsContain("11"))
            //                result = dr.bcoshortnamelist + "";
            //        }

            //        if (string.IsNullOrEmpty(result) && IsContain("12"))
            //            result = dr.ncdshortnamelist + "";

            //    }
            //    else if (IsContain("1") || IsContain("11"))
            //    {
            //        result = dr.bcoshortnamelist + "";
            //        if (string.IsNullOrEmpty(result) && IsContain("12"))
            //            result = dr.ncdshortnamelist + "";
            //    }
            //    else if (IsContain("12"))
            //        result = dr.ncdshortnamelist + "";

            //}


            //if (string.IsNullOrEmpty(result))
            //    result = dr.shortnamecn + "";

            //if (result.Contains(","))
            //{
            //    string[] arr = result.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //    if (arr != null && arr.Length > 0)
            //        result = arr[0];
            //}

            return result;
        }

        private void setPositionVal(OrderDetailPo changeOrder, string lineData, bool isA)
        {
            DataTable dtBrokerB = new DataTable();//CRMService.Instance.GetBroker(lineData, null, 1);
            DataTable dtTraderB = new DataTable();//CRMService.Instance.GetContactByName(lineData, 1);
            if ((dtBrokerB.Rows.Count > 0 && dtTraderB.Rows.Count > 0)
                || (dtBrokerB.Rows.Count > 0 && dtTraderB.Rows.Count <= 0))
            {
                if (isA)
                {
                    changeOrder.PartySubID = dtBrokerB.Rows[0]["Idb_Key"] + "";
                    changeOrder.PartySubName = dtBrokerB.Rows[0]["AccountName"] + "";

                    changeOrder.TraderID = changeOrder.TraderName = string.Empty;
                }
                else
                {
                    changeOrder.CounterPartySubID = dtBrokerB.Rows[0]["Idb_Key"] + "";
                    changeOrder.CounterPartySubName = dtBrokerB.Rows[0]["AccountName"] + "";

                    changeOrder.CounterPartyTraderId = changeOrder.CounterPartyTraderName = string.Empty;
                }
            }
            else if (dtBrokerB.Rows.Count <= 0 && dtTraderB.Rows.Count > 0)
            {
                if (isA)
                {
                    changeOrder.TraderName = (dtTraderB.Rows[0]["namecn"] + "").Trim();
                    changeOrder.TraderID = dtTraderB.Rows[0]["idbkey"] + "";

                    changeOrder.PartySubID = changeOrder.PartySubName = string.Empty;
                }
                else
                {
                    changeOrder.CounterPartyTraderName = (dtTraderB.Rows[0]["namecn"] + "").Trim();
                    changeOrder.CounterPartyTraderId = dtTraderB.Rows[0]["idbkey"] + "";

                    changeOrder.CounterPartySubID = changeOrder.CounterPartySubName = string.Empty;
                }
            }
            else
            {
                if (isA)
                {
                    changeOrder.TraderID = changeOrder.TraderName = string.Empty;
                    changeOrder.PartySubID = changeOrder.PartySubName = string.Empty;
                }
                else
                {
                    changeOrder.CounterPartyTraderId = changeOrder.CounterPartyTraderName = string.Empty;
                    changeOrder.CounterPartySubID = changeOrder.CounterPartySubName = string.Empty;
                }
            }
        }

        #endregion

        private bool inputFlag = true;
        private void tbOrder_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            inputFlag = true;
        }

        private void tbOrder_PreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
        {
            inputFlag = false;
        }

        private void tbOrder_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Back || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                inputFlag = true;
        }

        //代码识别的债券列表
        List<SimpleBondsInfoPo> BondsByID = new List<SimpleBondsInfoPo>();
        //名称识别的债券列表
        List<SimpleBondsInfoPo> BondsByName = new List<SimpleBondsInfoPo>();
        private void Analysis(int index, OrderDetailPo changeOrder, string[] lineData)
        {
            if ((string.IsNullOrEmpty(changeOrder.PartySubID) && string.IsNullOrEmpty(changeOrder.CounterPartySubID))
                || (string.IsNullOrEmpty(changeOrder.PartySubID) && !string.IsNullOrEmpty(changeOrder.CounterPartySubID)))
            {
                changeOrder.PartySubID = LocalEnv.UserInfo.UserId;
                changeOrder.PartySubName = LocalEnv.UserInfo.FullName;
            }

            decimal d = 0;
            //债券代码
            bool isSetCode = false;
            //价格
            bool isSetPrice = false;
            //数量
            bool isSetQty = false;
            //特别标识数量
            bool isQty = false;
            //结算方式
            bool isSetSettle = false;
            //ofr方的交易员
            bool isOfr = false;
            //bidbroker
            bool isBid = false;
            //ofr方的交易员列表
            //DataTable temDt = null;
            List<CrmContact> temDt = null;
            int lineIndex = 0;
            int lineNameIndex = 0;
            //数量标识位
            int indexQty = -1;
            //债券条件识别标志
            int flag = 0;
            //记录识别债券代码所用字符长度
            int strLength = 0;
            //债券代码
            bool isSetCodebyID = false;
            BondsByID.Clear();
            BondsByName.Clear();
            foreach (string str in lineData)
            {
                lineIndex++;
                //优先使用债券代码识别
                string temStr = str.ToUpper().Trim();
                if ((temStr.EndsWith(".IB") || temStr.EndsWith(".SH") || temStr.EndsWith(".SZ")) && temStr.Length >= 8)
                {
                    BondsInfoPo bestBond = GetBestBondByFlag(temStr, 20, out flag);
                    if (bestBond != null)
                    {
                        changeOrder.SecurityID = bestBond.Bond_ID;
                        changeOrder.goodsShortName = bestBond.Short_Name;//.ShortName;
                        changeOrder.timeToMaturity = bestBond.RemaingingTime;
                        changeOrder.goodstype = bestBond.SECURITY_TYPE;//.Goodstype;
                        changeOrder.goodsSubType = bestBond.Bond_Subtype;//Subtype;
                        isSetCodebyID = true;
                        break;
                    }
                }
            }

            if (!isSetCodebyID)
            {
                lineIndex = 0;
                foreach (string str in lineData)
                {
                    lineIndex++;
                    string temStr = str.Trim();
                    if (temStr.Length > strLength)
                    {
                        BondsInfoPo bestBond = GetBestBondByFlag(temStr, 20, out flag);
                        if (bestBond != null)
                        {
                            lineNameIndex = lineIndex;
                            changeOrder.SecurityID = bestBond.Bond_ID;
                            changeOrder.goodsShortName = bestBond.Short_Name;//.ShortName;
                            changeOrder.timeToMaturity = bestBond.RemaingingTime;
                            changeOrder.goodstype = bestBond.SECURITY_TYPE;//.Goodstype;
                            changeOrder.goodsSubType = bestBond.Bond_Subtype;//.Subtype;
                            strLength = temStr.Length;
                            isSetCode = true;
                        }
                    }
                }
            }

            //识别特殊数量
            foreach (string str in lineData)
            {
                indexQty++;
                string temStr = str.Trim();
                if (isSetCodebyID || isSetCode)
                {
                    if (isSetCodebyID)
                    {
                        if (lineIndex == indexQty + 1)
                        {
                            continue;
                        }
                    }
                    if (isSetCode)
                    {
                        if (lineNameIndex == indexQty + 1)
                        {
                            continue;
                        }
                    }
                    if (temStr.ToUpper().EndsWith("W"))
                    {
                        string newQty = temStr;
                        if (newQty.Length > 1)
                        {
                            newQty = newQty.Substring(0, temStr.Length - 1);
                            decimal qty = 0;
                            if (decimal.TryParse(newQty, out qty))
                            {
                                //输入的为数量
                                changeOrder.qty = qty;
                                isSetQty = true;
                                isQty = true;
                                break;
                            }
                            else
                            {
                                isSetQty = false;
                                isQty = false;
                            }
                        }
                        else
                        {
                            isSetQty = false;
                            isQty = false;
                        }
                    }
                }
                else
                {
                    if (temStr.ToUpper().EndsWith("W"))
                    {
                        string newQty = temStr;
                        if (newQty.Length > 1)
                        {
                            newQty = newQty.Substring(0, temStr.Length - 1);
                            decimal qty = 0;
                            if (decimal.TryParse(newQty, out qty))
                            {
                                //输入的为数量
                                changeOrder.qty = qty;
                                isSetQty = true;
                                isQty = true;
                                break;
                            }
                            else
                            {
                                isSetQty = false;
                                isQty = false;
                            }
                        }
                        else
                        {
                            isSetQty = false;
                            isQty = false;
                        }
                    }
                }
            }
            //债券代码成功识别
            if (isSetCodebyID || isSetCode)
            {
                int k = 0;
                foreach (string str in lineData)
                {
                    k++;
                    string temStr = str.Trim();

                    if (isSetCodebyID)
                    {
                        if (lineIndex == k)
                        {
                            continue;
                        }
                    }
                    if (isSetCode)
                    {
                        if (lineNameIndex == k)
                        {
                            continue;
                        }
                    }
                    if (isQty)
                    {
                        if (indexQty == k - 1)
                        {
                            continue;
                        }
                    }
                    //数量
                    bool isnumber = false;
                    bool isnumprice = false;
                    if (!isSetQty)
                    {
                        isnumber = decimal.TryParse(DataAnalysisHelper.AnalysisQty(temStr), out d);
                        if (isnumber)
                        {
                            int qtyInt = Convert.ToInt32(d);
                            int qtyValue = qtyInt % 500;
                            if (qtyValue == 0)
                            {
                                isnumprice = true;
                            }
                            else
                            {
                                isnumprice = false;
                            }
                        }
                    }

                    if (isnumprice && d <= 2000000 && d >= 500 && !isSetQty)
                    {
                        int qtyInt = Convert.ToInt32(d);
                        changeOrder.qty = new decimal?(qtyInt);
                        isSetQty = true;
                        continue;
                    }

                    // 收益率
                    if (!temStr.Contains("+") && decimal.TryParse(temStr, out d) && !isSetPrice && !string.IsNullOrWhiteSpace(changeOrder.SecurityID))
                    {
                        isSetPrice = HasGetPrice(index, changeOrder.SecurityID, temStr, changeOrder);
                        if (isSetPrice)
                        {
                            if (changeOrder.price != null && changeOrder.price < 200)
                            {
                                int indext = changeOrder.price.ToString().LastIndexOf('.');
                                if (indext != -1 && changeOrder.price.ToString().Length > indext + 1)
                                {
                                    string strnuber = changeOrder.price.ToString().Substring(indext + 1);
                                    if (strnuber.Length <= 4)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        changeOrder.price = null;
                                        isSetPrice = false;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                changeOrder.price = null;
                                isSetPrice = false;
                            }
                        }
                    }

                    string[] arr = temStr.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 1)
                    {
                        string result = CreditUnderlineExecRule.Instance.AnalysisSettle(arr[0]);
                        if (!string.IsNullOrEmpty(result) && !isSetSettle)
                        {
                            changeOrder.SettlType2 = result;

                            result = CreditUnderlineExecRule.Instance.AnalysisSettle(arr[1]);
                            if (!string.IsNullOrEmpty(result))
                            {
                                changeOrder.SettlType = result;

                                isSetSettle = true;

                                continue;
                            }

                        }
                    }
                    else
                    {
                        string result = CreditUnderlineExecRule.Instance.AnalysisSettle(arr[0]);
                        if (!string.IsNullOrEmpty(result) && !isSetSettle)
                        {
                            changeOrder.SettlType = changeOrder.SettlType2 = result;

                            isSetSettle = true;

                        }
                    }
                    //ofr方的交易员
                    if (!isOfr && !string.IsNullOrWhiteSpace(changeOrder.SecurityID))
                    {
                        temDt = Utils.CommonUtils.GetContactInfoPo(temStr, 1, "");//CRMService.Instance.GetContactByName(temStr, 1);
                        if (temDt.Count > 0)
                        {
                            string instid = temDt[0].instid;
                            if (!string.IsNullOrEmpty(instid))
                            {
                                CrmInst dt = Utils.CommonUtils.GetInstByIdbKey(instid);
                                //CRMService.Instance.GetInstByIn(string.Format("'{0}'", instid), 1);
                                if (dt != null)
                                {
                                    changeOrder.InstitutionID = dt.idbkey;//dt.Rows[0]["idbkey"] + "";
                                    changeOrder.InstitutionName = GetInstName(dt, changeOrder.SecurityID);
                                    //changeOrder.bankcodeshorta = dt.Rows[0]["shortnamecn"].ToString();
                                }
                            }
                            changeOrder.TraderName = temDt[0].namecn;//temDt.Rows[0]["namecn"].ToString();
                            changeOrder.TraderID = temDt[0].idbkey;//temDt.Rows[0]["idbkey"] + "";
                            isOfr = true;
                            continue;
                        }
                        else
                        {
                            isOfr = false;
                        }
                    }
                    //bidbroker
                    if (!isBid && !string.IsNullOrWhiteSpace(changeOrder.SecurityID))
                    {
                        List<BrokerInfoPo> dt = Utils.CommonUtils.GetBrokerInfoPo(temStr, changeOrder == null ? string.Empty : changeOrder.goodstype, 1);
                        //Utils.CommonUtils.GetBrokerInfoPo(temStr, changeOrder.goodstype);
                        if (dt.Count > 0)
                        {
                            changeOrder.CounterPartySubName = dt[0].AccountName;//dt.Rows[0]["AccountName"].ToString();
                            changeOrder.CounterPartySubID = dt[0].Idb_key;//dt.Rows[0]["Idb_Key"] + "";
                            isBid = true;
                            continue;
                        }
                        else
                        {
                            isBid = false;
                        }
                    }
                }
            }
            else
            {
                changeOrder.SecurityID = string.Empty;
                changeOrder.goodsShortName = string.Empty;
                changeOrder.timeToMaturity = string.Empty;
                changeOrder.goodstype = string.Empty;
                changeOrder.goodsSubType = string.Empty;
                int k = -1;
                foreach (string str in lineData)
                {
                    k++;
                    string temStr = str.Trim();
                    //数量
                    bool isnumber = false;
                    bool isnumprice = false;
                    if (isSetQty)
                    {
                        if (indexQty == k)
                        {
                            continue;
                        }
                    }
                    if (!isSetQty)
                    {
                        isnumber = decimal.TryParse(DataAnalysisHelper.AnalysisQty(temStr), out d);
                        if (isnumber)
                        {
                            int qtyInt = Convert.ToInt32(d);
                            int qtyValue = qtyInt % 500;
                            if (qtyValue == 0)
                            {
                                isnumprice = true;
                            }
                            else
                            {
                                isnumprice = false;
                            }
                        }
                    }

                    if (isnumprice && d <= 2000000 && d >= 500 && !isSetQty)
                    {
                        int qtyInt = Convert.ToInt32(d);
                        changeOrder.qty = new decimal?(qtyInt);
                        isSetQty = true;
                        continue;
                    }

                    string[] arr = temStr.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 1)
                    {
                        string result = CreditUnderlineExecRule.Instance.AnalysisSettle(arr[0]);
                        if (!string.IsNullOrEmpty(result) && !isSetSettle)
                        {
                            changeOrder.SettlType2 = result;

                            result = CreditUnderlineExecRule.Instance.AnalysisSettle(arr[1]);
                            if (!string.IsNullOrEmpty(result))
                            {
                                changeOrder.SettlType = result;

                                isSetSettle = true;

                                continue;
                            }

                        }
                    }
                    else
                    {
                        string result = CreditUnderlineExecRule.Instance.AnalysisSettle(arr[0]);
                        if (!string.IsNullOrEmpty(result) && !isSetSettle)
                        {
                            changeOrder.SettlType = changeOrder.SettlType2 = result;

                            isSetSettle = true;

                        }
                    }
                    if (isSetQty && isSetSettle)
                    {
                        break;
                    }
                }
            }

            //如果本次未识别则清除上次识别结果
            if (!isOfr)
            {
                changeOrder.InstitutionID = string.Empty;
                changeOrder.InstitutionName = string.Empty;
                changeOrder.TraderName = string.Empty;
                changeOrder.TraderID = string.Empty;
            }
            if (!isBid)
            {
                changeOrder.CounterPartySubName = string.Empty;
                changeOrder.CounterPartySubID = string.Empty;
            }

            if (!isSetCode && !isSetCodebyID)
                changeOrder.SecurityID = changeOrder.goodsShortName = changeOrder.timeToMaturity = string.Empty;
            if (!isSetPrice)
                changeOrder.price = null;
            if (!isSetQty)
                changeOrder.qty = null;
            if (!isSetSettle)
                changeOrder.SettlType = changeOrder.SettlType2 = "明天+0";
        }

        private string CalSettleDate(CalSettleDateRequestPo request)
        {
            string settleStr = request.SettlementType;
            switch (request.SettlementType)
            {
                case "+0":
                    settleStr = "1";
                    break;
                case "+1":
                    settleStr = "2";
                    break;
                case "明天+0":
                    settleStr = "3";
                    break;
                default:
                    settleStr = request.SettlementType;
                    break;
            }
            return settleStr;
        }

        private void CalDate(OrderDetailPo po, string settleType, string settleType2)
        {
            //ThreadPool.QueueUserWorkItem(o =>
            //{
                try
                {
                    if (!string.IsNullOrEmpty(settleType))
                    {
                        CalWeekDateReq requst = new CalWeekDateReq();
                        requst.settlementType = settleType;// "1";
                        CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                        if (result != null && result.Data != null)
                        {
                            po.dealdate = result.Data.dealDate;
                            po.SettlDate = result.Data.settlementDate;
                        }
                    }
                    if (!string.IsNullOrEmpty(settleType2))
                    {
                        CalWeekDateReq requst = new CalWeekDateReq();
                        requst.settlementType = settleType2;// "1";
                        CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                        if (result != null && result.Data != null)
                        {
                            po.DealDate2 = result.Data.dealDate;
                            po.SettlDate2 = result.Data.settlementDate;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            //});
        }


        //private bool IsAccurate( int flag) { 
        //}

        private bool HasGetPrice(int index, string goodcode, string price, OrderDetailPo changeOrder)
        {
            bool hasGetPrice = false;
            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                if (price.Contains("."))
                {
                    if (d >= 0 && d < 200)
                    {
                        changeOrder.price = new decimal?(d);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (d >= 10000)
                    {
                        return false;
                    }
                }
            }

            if (valuationDic.ContainsKey(goodcode))
            {
                if (DataAnalysisHelper.CompareDateDay(valuationDic[goodcode].modify_time) == 0)
                {
                    price = DataAnalysisHelper.AnalysisPrice(price, valuationDic[goodcode]);
                    if (decimal.TryParse(price, out d))
                    {
                        if (d >= 0 && d < 200)
                        {
                            changeOrder.price = new decimal?(d);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    hasGetPrice = UpdatePrice(index, goodcode, price, changeOrder);
                }
            }
            else
            {
                hasGetPrice = UpdatePrice(index, goodcode, price, changeOrder);
            }
            return hasGetPrice;
        }

        private bool UpdatePrice(int index, string goodcode, string price, OrderDetailPo changeOrder)
        {
            //bool isUpdate = false;

            //decimal d = 0;
            //if (decimal.TryParse(price, out d))
            //{
            //    if (d >= 0 && d < 200)
            //    {
            //        changeOrder.price = new decimal?(d);
            //        isUpdate = true;
            //    }
            //    else
            //    {
            //        isUpdate = false;
            //    }
            //}
            //else
            //{
            //    isUpdate = false;
            //}
            //return isUpdate;
            bool isUpdate = false;
            ValuationRequest request = new ValuationRequest();
            request.SecurityID = goodcode;
            ManualResetEvent[] _ManualEvents = new ManualResetEvent[1];
            _ManualEvents[0] = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    TBondValuationReq req = new TBondValuationReq();
                    req.SecurityID = goodcode;
                    var reply = BaseHandler.Instance.SearchChianSecurity(req);
                    if (reply != null && reply.IsSuccess())
                    {
                        List<TBondValuationPo> datas = reply.Datas;
                        if (datas != null && datas.Count > 0)
                        {
                            if (!valuationDic.ContainsKey(goodcode))
                                valuationDic.Add(goodcode, datas[0]);
                            else
                                valuationDic[goodcode] = datas[0];

                            price = DataAnalysisHelper.AnalysisPrice(price, valuationDic[goodcode]);
                            decimal d = 0;
                            if (decimal.TryParse(price, out d))
                            {
                                if (d >= 0 && d < 200)
                                {
                                    changeOrder.price = new decimal?(d);
                                    isUpdate = true;
                                }
                                else
                                {
                                    isUpdate = false;
                                }
                            }
                            else
                            {
                                isUpdate = false;
                            }

                        }
                        else
                        {
                            price = DataAnalysisHelper.AnalysisPrice(price, null);
                            decimal d = 0;
                            if (decimal.TryParse(price, out d))
                            {
                                if (d >= 0 && d < 200)
                                {
                                    changeOrder.price = new decimal?(d);
                                    isUpdate = true;
                                }
                                else
                                {
                                    isUpdate = false;
                                }
                            }
                            else
                            {
                                isUpdate = false;
                            }
                        }
                    }
                    else
                    {
                        isUpdate = false;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                    isUpdate = false;
                }
                finally
                {
                    ManualResetEvent e = (ManualResetEvent)o;
                    e.Set();
                }
            }, _ManualEvents[0]);
            WaitHandle.WaitAll(_ManualEvents);
            return isUpdate;
        }
    }
}
