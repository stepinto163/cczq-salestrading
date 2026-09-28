using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Handler.Cal;
using STC.Common.Model.Cal;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Transaction.Settle
{
    /// <summary>
    /// SettleUI.xaml 的交互逻辑
    /// </summary>
    public partial class SettleUI : UserControl
    {
        private bool IsInit = true;
        private bool IsBid = false;
        private bool IsSpotInit = true;
        public event Action<bool, string> OnSettleChange;
        public SettleUI()
        {
            InitializeComponent();

            dpDate.BlackoutDates.AddDatesInPast();
        }


        public void Init(SettlementPo SettlementPo, bool isHistory)
        {

            if (SettlementPo.IsBid)
            {
                IsBid = true;
                this.tbSide.Text = "Bid";
                this.tbSide.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED6D00"));
                this.bd_side.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#392512"));
            }
            else
            {
                this.tbSide.Text = "Ofr";
                this.tbSide.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0082D8"));
                this.bd_side.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102A3B"));
            }

            tbTradeDate.Text = SettlementPo.TradeDate;
            tbDeliveryDate.Text = SettlementPo.SettlementDate;

            if (isHistory)
            {
                foreach (ListBoxItem lbt in lbSettle.Items)
                    lbt.IsEnabled = lbt.IsSelected = false;

                foreach (ListBoxItem lbt in lbWeek.Items)
                    lbt.IsEnabled = lbt.IsSelected = false;

                string[] arr = SettlementPo.Settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);

                if (arr != null && arr.Length > 0)
                {
                    string[] dtStr = arr[0].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    int month = 0;
                    int day = 0;
                    if (dtStr != null && dtStr.Length > 1
                        && Int32.TryParse(dtStr[0], out month) && Int32.TryParse(dtStr[1], out day))
                    {
                        DateTime dt = new DateTime(DateTime.Now.Year, month, day);
                        try
                        {
                            dpDate.SelectedDate = new DateTime?(dt);
                        }
                        catch (Exception e) { }
                    }
                    if (arr.Length > 1)
                    {
                        if (arr[1].Equals("0"))
                        {
                            lbtAdd0.IsSelected = true;
                            lbtAdd1.IsSelected = false;
                        }
                        else if (arr[1].Equals("1"))
                        {
                            lbtAdd0.IsSelected = false;
                            lbtAdd1.IsSelected = true;
                        }
                    }
                }
            }
            else
            {
                //即期显示
                if (SettlementPo.IsSpot)
                {
                    string[] arr = SettlementPo.Settle.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    IsSpotInit = false;
                    if (arr.Contains(lbt1.Content + "") && arr.Contains(lbtTomorrow0.Content + ""))
                    {
                        IsSpotInit = true;
                        lbt1.IsSelected = arr.Contains(lbt1.Content + "");
                        lbtTomorrow0.IsSelected = arr.Contains(lbtTomorrow0.Content + "");
                        IsSpotInit = false;
                    }
                    else if (arr.Contains(lbt1.Content + "") && arr.Contains(lbt0.Content + ""))
                    {
                        IsSpotInit = true;
                        lbt1.IsSelected = arr.Contains(lbt1.Content + "");
                        lbt0.IsSelected = arr.Contains(lbt0.Content + "");
                        IsSpotInit = false;
                    }
                    else
                    {
                        lbt1.IsSelected = arr.Contains(lbt1.Content + "");
                        lbtTomorrow0.IsSelected = arr.Contains(lbtTomorrow0.Content + "");
                    }

                    lbt0.IsSelected = arr.Contains(lbt0.Content + "");

                    if (arr.Length > 1)
                        tbDeliveryDate.Text = tbTradeDate.Text = string.Empty;

                    foreach (ListBoxItem lbt in lbWeek.Items)
                        lbt.IsSelected = false;

                    foreach (ListBoxItem lbt in lbAdd.Items)
                        lbt.IsSelected = false;

                    dpDate.Text = string.Empty;

                }
                else
                {
                    IsSpotInit = false;
                    if (SettlementPo.Settle.Equals("明天+1"))
                        lbtTomorrow1.IsSelected = true;
                    else
                    {
                        IsSpotInit = false;
                        bool isWeek = false;
                        foreach (ListBoxItem lbt in lbWeek.Items)
                        {
                            if (!string.IsNullOrEmpty(SettlementPo.Settle) && SettlementPo.Settle.Contains(lbt.Content + ""))
                            {
                                lbt.IsSelected = true;
                                isWeek = true;
                                if (SettlementPo.Settle.Contains("+0"))
                                {
                                    lbtAdd0.IsSelected = true;
                                    lbtAdd1.IsSelected = false;
                                }
                                else if (SettlementPo.Settle.Contains("+1"))
                                {
                                    lbtAdd0.IsSelected = false;
                                    lbtAdd1.IsSelected = true;
                                }
                            }
                        }

                        if (!isWeek)
                        {
                            string[] arr = SettlementPo.Settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);

                            if (arr != null && arr.Length > 0)
                            {
                                string[] dtStr = arr[0].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                int month = 0;
                                int day = 0;
                                if (dtStr != null && dtStr.Length > 1
                                    && Int32.TryParse(dtStr[0], out month) && Int32.TryParse(dtStr[1], out day))
                                {
                                    DateTime dt = new DateTime(DateTime.Now.Year, month, day);
                                    try
                                    {
                                        dpDate.SelectedDate = new DateTime?(dt);
                                    }
                                    catch (Exception e) { }
                                }
                                if (arr.Length > 1)
                                {
                                    if (arr[1].Equals("0"))
                                    {
                                        lbtAdd0.IsSelected = true;
                                        lbtAdd1.IsSelected = false;
                                    }
                                    else if (arr[1].Equals("1"))
                                    {
                                        lbtAdd0.IsSelected = false;
                                        lbtAdd1.IsSelected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            IsInit = false;
        }

        private string getAddZeroOrOne()
        {
            return lbtAdd0.IsSelected ? "+0" : lbtAdd1.IsSelected ? "+1" : "+0";
        }

        
        public SettlementPo GetSettle()
        {
            SettlementPo settlePo = new SettlementPo();
            if (IsBid)
            {
                settlePo.BidSettlementDate = tbDeliveryDate.Text;
                settlePo.BidTradeDate = tbTradeDate.Text;
            }
            else
            {
                settlePo.SettlementDate = tbDeliveryDate.Text;
                settlePo.TradeDate = tbTradeDate.Text;
            }
            if (lbtTomorrow1.IsSelected)
            {
                settlePo.Settle = "明天+1";
            }
            else
            {
                foreach (ListBoxItem tem in lbSettle.Items)
                {
                    if (tem.IsSelected)
                        settlePo.Settle += "," + tem.Content + "";
                }
                if (!string.IsNullOrEmpty(settlePo.Settle))
                    settlePo.Settle = settlePo.Settle.Substring(1);
                else
                {
                    foreach (ListBoxItem tem in lbWeek.Items)
                    {
                        if (tem.IsSelected)
                        {
                            settlePo.Settle = tem.Content + getAddZeroOrOne();
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(settlePo.Settle))
                    {
                        DateTime? dt = dpDate.SelectedDate;
                        if (dt.HasValue)
                        {
                            settlePo.Settle = (dt.Value.Month < 10 ? ("0" + dt.Value.Month.ToString()) : dt.Value.Month.ToString()) + "." + (dt.Value.Day < 10 ? ("0" + dt.Value.Day.ToString()) : dt.Value.Day.ToString());
                            if (lbtAdd0.IsSelected)
                                settlePo.Settle += lbtAdd0.Content;
                            else if (lbtAdd1.IsSelected)
                                settlePo.Settle += lbtAdd1.Content;
                            else
                                settlePo.Settle += "+0";
                        }
                    }
                }
            }

            return settlePo;
        }



        private bool IsSettleChange = false;
        private void lbSettle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInit || IsDateChange || IsWeekChange || IsSpotInit)
                return;

            IsSettleChange = true;
            //ListBoxItem lbt = lbSettle.SelectedItem as ListBoxItem;
            ListBoxItem lbt = e.AddedItems.Count > 0 ? e.AddedItems[0] as ListBoxItem : null;

            if (lbt == null || lbt.Content == null)
            {
                lbt = lbSettle.SelectedItem as ListBoxItem;
                if (lbt == null || lbt.Content == null)
                    return;
            }
            if (lbt.Content.Equals("+0") || lbt.Content.Equals("+1") || lbt.Content.Equals("明天+0")
                || lbt.Content.Equals("不限"))
            {
                foreach (ListBoxItem tem in lbWeek.Items)
                    tem.IsSelected = false;

                foreach (ListBoxItem tem in lbAdd.Items)
                    tem.IsSelected = false;

                lbtTomorrow1.IsSelected = false;
                dpDate.Text = string.Empty;

                int i = 0;
                foreach (ListBoxItem tem in lbSettle.Items)
                {
                    if (tem.IsSelected)
                        i++;
                }
                if (i == 1 && !lbt.Content.Equals("不限"))
                {
                    CalSettleDateRequestPo request = new CalSettleDateRequestPo();
                    request.SettlementType = lbt.Content + "";

                    CalSettleDate(request);
                }

            }
            else if (lbt.Content.Equals("明天+1"))
            {
                foreach (ListBoxItem tem in lbWeek.Items)
                    tem.IsSelected = false;

                foreach (ListBoxItem tem in lbSettle.Items)
                    tem.IsSelected = lbt.Equals(tem);

                lbtAdd0.IsSelected = lbtAdd1.IsSelected = false;
                dpDate.Text = string.Empty;

                CalSettleDateRequestPo request = new CalSettleDateRequestPo();
                request.SettlementType = lbt.Content + "";

                CalSettleDate(request);
            }
            tbDeliveryDate.Text = tbTradeDate.Text = string.Empty;

            OnSettleChange?.Invoke(IsBid, lbt.Content + "");

            IsSettleChange = false;
        }

        private bool IsWeekChange = false;
        private void lbWeek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInit || IsDateChange || IsSettleChange)
                return;

            IsWeekChange = true;
            ListBoxItem lbt = lbWeek.SelectedItem as ListBoxItem;

            if (lbt == null || lbt.Content == null)
                return;

            foreach (ListBoxItem tem in lbWeek.Items)
                tem.IsSelected = lbt.Equals(tem);
            foreach (ListBoxItem tem in lbSettle.Items)
                tem.IsSelected = false;

            dpDate.Text = string.Empty;

            lbtAdd0.IsSelected = true;
            lbtAdd1.IsSelected = false;

            CalSettleDateRequestPo request = new CalSettleDateRequestPo();
            request.SettlementType = lbt.Content + "+0";

            CalSettleDate(request);

            OnSettleChange?.Invoke(IsBid, lbt.Content.ToString());

            IsWeekChange = false;
        }

        private bool IsDateChange = false;
        private void dpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInit || IsWeekChange || IsSettleChange)
                return;

            IsDateChange = true;
            foreach (ListBoxItem tem in lbSettle.Items)
                tem.IsSelected = false;

            foreach (ListBoxItem tem in lbWeek.Items)
                tem.IsSelected = false;

            lbtAdd1.IsSelected = false;
            lbtAdd0.IsSelected = true;

            DateTime? dt = dpDate.SelectedDate;
            if (dt.HasValue)
            {
                CalSettleDateRequestPo request = new CalSettleDateRequestPo();
                request.SettlementType = dt.Value.ToString("MM.dd") + "+0";

                CalSettleDate(request);
            }

            OnSettleChange?.Invoke(IsBid, dt.Value.ToString("MM.dd") + "+0");

            IsDateChange = false;
        }

        private void CalSettleDate(CalSettleDateRequestPo request)
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
                    default :
                    settleStr = request.SettlementType;
                    break;

            }
            CalDate(settleStr);
            //ThreadPool.QueueUserWorkItem((o) =>
            //{
            //    CalSettleDateReplyPo result = BrokerCEDAServer.Instance.CalSettleDate(request);

            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        if (result != null && result.Data != null)
            //        {
            //            if (result.Data.dealDate.Length > 7)
            //                tbTradeDate.Text = result.Data.dealDate.Insert(4, "-").Insert(7, "-");
            //            if (result.Data.settlementDate.Length > 7)
            //                tbDeliveryDate.Text = result.Data.settlementDate.Insert(4, "-").Insert(7, "-");

            //        }
            //    }));
            //});
        }

        private void CalDate(string settleType)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (result != null && result.Data != null)
                        {
                            if (result.Data.dealDate.Length > 7)
                                tbTradeDate.Text = result.Data.dealDate.Insert(4, "-").Insert(7, "-");
                            if (result.Data.settlementDate.Length > 7)
                                tbDeliveryDate.Text = result.Data.settlementDate.Insert(4, "-").Insert(7, "-");

                        }
                    }));
                    //if (requst != null && result.Data != null)
                    //{
                    //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //    {
                    //        try
                    //        {
                    //            string year = result.Data.dealDate.Substring(0, 4);
                    //            string month = result.Data.dealDate.Substring(4, 2);
                    //            string day = result.Data.dealDate.Substring(6, 2);
                    //            SelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                    //            SendPriceItem.CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                    //        }
                    //        catch (Exception e) { }

                    //        ComboBoxSelectIndex = isTomorrow ? 0 : 1;
                    //        SendPriceItem.SettleType = isTomorrow ? "T+0" : "T+1";

                    //        isSendCalDate = false;
                    //    }));
                    //}
                    //else
                    //    isSendCalDate = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    ///isSendCalDate = false;
                }
            });
        }

        private void lbAdd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInit || IsWeekChange || IsSettleChange || IsDateChange)
                return;

            ListBoxItem lbtTem = lbAdd.SelectedItem as ListBoxItem;

            if (lbtTem == null)
                return;

            foreach (ListBoxItem tem in lbAdd.Items)
                tem.IsSelected = lbtTem.Equals(tem);

            CalSettleDateRequestPo request = new CalSettleDateRequestPo();
            string changeSettle = string.Empty;
            if (lbWeek.SelectedItem != null)
            {
                ListBoxItem lbt = lbWeek.SelectedItem as ListBoxItem;
                changeSettle = request.SettlementType = lbt.Content + "" + lbtTem.Content;

                CalSettleDate(request);
            }
            else
            {
                DateTime? dt = dpDate.SelectedDate;
                if (dt.HasValue)
                {
                    changeSettle = request.SettlementType = dt.Value.ToString("MM.dd") + lbtTem.Content;

                    CalSettleDate(request);
                }
            }

            OnSettleChange?.Invoke(IsBid, changeSettle);
        }
    }
}
