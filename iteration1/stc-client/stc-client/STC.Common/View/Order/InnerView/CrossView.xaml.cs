using STC.Common.ViewModel.Order;
using STC.Common.ViewModel.Order.InnerView;
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

namespace STC.Common.View.Order.InnerView
{
    /// <summary>
    /// CrossView.xaml 的交互逻辑
    /// </summary>
    public partial class CrossView : UserControl
    {
        private CrossViewModel crossViewModel = new CrossViewModel();
        private bool isTabChange = false;
        private bool isInit = true;
        public Action<object, SelectionChangedEventArgs> LbDirectSelectionChanged_CrossView;

        public CrossView()
        {
            InitializeComponent();

            this.DataContext = crossViewModel;

            ocoView.onCheckChange += OcocView_onCheckChange;

            this.Loaded += CrossView_Loaded;
        }

        private void CrossView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= CrossView_Loaded;

            isInit = false;
        }

        private void OcocView_onCheckChange(Dictionary<string, bool> dic)
        {
            if (!isTabChange)
                crossViewModel.onCheckChange(dic);
        }

        public event Action<int> onTabSelectionChange;


        public int SelectTabIndex
        {
            get { return (int)GetValue(SelectTabIndexProperty); }
            set { SetValue(SelectTabIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectBondCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectTabIndexProperty =
            DependencyProperty.Register("SelectTabIndex", typeof(int), typeof(CrossView), 
                new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.AffectsRender, null));



        private void LbDirect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LbDirectSelectionChanged_CrossView?.Invoke(sender, e);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit)
            {
                isTabChange = true;
                TabControl tabControl = sender as TabControl;
                SelectTabIndex = tabControl.SelectedIndex;


                if (PermisionHelper.Instance.IsHaveAllPermission && !crossViewModel.isUpdate)
                {
                    OCOViewModel ocoViewModel = ocoView.DataContext as OCOViewModel;
                    ocoViewModel.CheckAll = SelectTabIndex == 0 ? false :true;
                    ocoViewModel.CheckAll = true;
                }

                if (onTabSelectionChange != null)
                    onTabSelectionChange(tabControl.SelectedIndex);

                crossViewModel.SetVenueItemVisibility(Visibility.Visible);

                crossViewModel.changeHeight();
                if (crossViewModel.IsNoNFIRMOrWholeFlag())
                {
                    crossViewModel.SetVenueItemEnable(false);
                    if (crossViewModel.SelectTabIndex == 0)
                        crossViewModel.SetVenueItemEnable(true, "INNER", true);
                    return;
                }
                else
                {
                    OCOViewModel ocoViewModel = ocoView.DataContext as OCOViewModel;
                    if ((ocoViewModel.CheckInner || ocoViewModel.CheckAll) && ocoViewModel.CheckNotOut)
                    {
                        crossViewModel.SetVenueItemEnable(false);
                    }
                    else
                    {
                        if (PermisionHelper.Instance.IsHaveInnerXbondPermission)
                        {
                            crossViewModel.SetVenueItemEnable(false);

                            crossViewModel.SetVenueItemEnable(true, PermisionHelper.Instance.XBOND);
                        }
                        else if (PermisionHelper.Instance.IsHaveAllPermission)
                        {
                            crossViewModel.SetVenueItemEnable(true);
                        }
                        else if (PermisionHelper.Instance.IsHaveInnerPermission)
                        {
                            crossViewModel.SetVenueItemEnable(false);
                        }
                    }
                }
                bool isSpot = crossViewModel.IsSpot();
                if (!isSpot)
                {
                    crossViewModel.SetVenueItemEnable(false, PermisionHelper.Instance.XBOND);
                }

                if (!crossViewModel.isUpdate)
                    crossViewModel.onCheckChange(ocoView.GetCheckDic());

                if (SelectTabIndex == 2)
                {
                    crossViewModel.SetVenueItemEnable(false);
                }


                isTabChange = false;
                crossViewModel.isUpdate = false;
            }
            else
            {
                if (onTabSelectionChange != null)
                    onTabSelectionChange(SelectTabIndex);

                if (!crossViewModel.isUpdate)
                    crossViewModel.onCheckChange(ocoView.GetCheckDic());
            }
        }
    }
}
