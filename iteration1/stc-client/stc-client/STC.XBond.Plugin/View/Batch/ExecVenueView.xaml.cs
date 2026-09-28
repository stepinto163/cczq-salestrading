using STC.Common.ViewModel.Order;
using STC.Common.ViewModel.Order.InnerView;
using STC.XBond.Plugin.ViewModel.Batch;
using System;
using System.Collections.Generic;
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

namespace STC.XBond.Plugin.View.Batch
{
    /// <summary>
    /// ExecVenueView.xaml 的交互逻辑
    /// </summary>
    public partial class ExecVenueView : UserControl
    {
        private ExecVenueViewModel execVenueViewModel = new ExecVenueViewModel();
        private bool isTabChange = false;
        public ExecVenueView()
        {
            InitializeComponent();

            this.DataContext = execVenueViewModel;

            ocoView.onCheckChange += OcocView_onCheckChange;
        }

        private void OcocView_onCheckChange(Dictionary<string, bool> dic)
        {
            if (!isTabChange)
                execVenueViewModel.onCheckChange(dic);
        }

        public void init(string securityType)
        {
            execVenueViewModel.initUI(securityType, this, this);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isTabChange = true;

            TabControl tabControl = sender as TabControl;
            int selectTabIndex = tabControl.SelectedIndex;

            if (selectTabIndex == 0)
            {
                execVenueViewModel.VenueSelectionMode = SelectionMode.Single;
            }
            else if (selectTabIndex == 1)
            {
                execVenueViewModel.VenueSelectionMode = SelectionMode.Multiple;
            }
            else if (selectTabIndex == 2)
            {
                execVenueViewModel.SetVenueItemEnable(false);
                return;
            }

            execVenueViewModel.SetVenueItemVisibility(Visibility);

            OCOViewModel ocoViewModel = ocoView.DataContext as OCOViewModel;

            if (PermisionHelper.Instance.IsHaveAllPermission)
            {
                ocoViewModel.CheckAll = true;
            }

            if ((ocoViewModel.CheckInner || ocoViewModel.CheckAll) && ocoViewModel.CheckNotOut)
            {
                execVenueViewModel.SetVenueItemEnable(false);
            }
            else
            {
                if (PermisionHelper.Instance.IsHaveInnerXbondPermission)
                {
                    execVenueViewModel.SetVenueItemEnable(false);

                    execVenueViewModel.SetVenueItemEnable(true, PermisionHelper.Instance.XBOND);
                }
                else if (PermisionHelper.Instance.IsHaveAllPermission)
                {
                    execVenueViewModel.SetVenueItemEnable(true);
                }
                else if (PermisionHelper.Instance.IsHaveInnerPermission)
                {
                    execVenueViewModel.SetVenueItemEnable(false);
                }
            }


            isTabChange = false;
        }


        private void LbDirect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
