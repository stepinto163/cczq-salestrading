using STC.Common.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// VenuesExecution.xaml 的交互逻辑
    /// </summary>
    public partial class VenuesExecution : UserControl
    {
        private VenuesExecutionViewModel viewModel = new VenuesExecutionViewModel();
        DispatcherTimer firmToNoTimer;

        public VenuesExecution()
        {
            InitializeComponent(); 
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (firmToNoTimer == null)
            {
                firmToNoTimer = new DispatcherTimer();
                firmToNoTimer.Interval = new TimeSpan(1000);
                firmToNoTimer.Tick += FirmToNoTimer_Tick;
            }

            this.DataContext = viewModel;

            VenuesExecutionFilter_FilterDicChanged(null, null);

            viewModel.Init(this);
        }

        private void VenuesExecutionFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.ProcessMyOrdersFilterCondition(this.venuesExecutionFilter.FilterDic);
            }
        }


        DataRowView drv;
        private void clock_img_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Image)sender).DataContext is DataRowView)
            {
                drv = ((Image)sender).DataContext as DataRowView;


                if (firmToNoTimer.IsEnabled == false)
                {
                    firmToNoTimer.Start();
                }
            }
        }

        private void clock_img_MouseLeave(object sender, MouseEventArgs e)
        {
            if (firmToNoTimer.IsEnabled == true)
            {
                firmToNoTimer.Stop();
            }
        }

        private void FirmToNoTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (drv != null && drv.Row.RowState != DataRowState.Detached)
                {
                    string transactTime_str = CommonUtils.ConvertToString(drv["QuoteTime"]);
                    DateTime transactTime = CommonUtils.ConvertToDateTime(transactTime_str);
                    TimeSpan diffTime = DateTimeEx.ServerTime - transactTime;
                    long firmToNonTime = CommonUtils.ConvertToLong(drv["IocTimeOut"].ToString());
                    lock (viewModel.LockObj_MyExecutions)
                    {
                        if (diffTime.TotalSeconds > firmToNonTime)
                        {
                            drv["RemainingTime"] = "00:00:00";
                        }
                        else
                        {
                            drv["RemainingTime"] = DateTime.Parse("00:00:00").AddSeconds(firmToNonTime - diffTime.TotalSeconds).ToString("HH:mm:ss");
                        }
                    }
                }
            }
            catch (System.Data.RowNotInTableException ex)
            {
                // ignore
            }
            catch (Exception otherEx)
            {
                LogHandler.Error(otherEx);
            }
        }

        private void venuesExecutionFilter_OnCopyClick()
        {
            dg_VenuesExecution.CopyDataByStatus(viewModel.VenuesExecutionDV);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cbUser.IsDropDownOpen = !cbUser.IsDropDownOpen;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            viewModel.OnCheckChange();
        }
    }
}
