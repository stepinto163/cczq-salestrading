using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.HomePage;
using STC.XBond.Plugin.ViewModel;
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
using System.Windows.Shapes;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// AddFavoritesView.xaml 的交互逻辑
    /// </summary>
    public partial class AddFavoritesView : WindowBase
    {


        public string SecurityID
        {
            get { return (string)GetValue(SecurityIDProperty); }
            set { SetValue(SecurityIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecurityID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecurityIDProperty =
            DependencyProperty.Register("SecurityID", typeof(string), typeof(AddFavoritesView), new PropertyMetadata(string.Empty, OnSecurityIDChanged));

        private static void OnSecurityIDChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           if (e.NewValue != null && e.NewValue.ToString().Length > 0)
            {
                if (d != null && d is AddFavoritesView)
                {
                    AddFavoritesView view = d as AddFavoritesView;
                    if (view.DataContext != null && view.DataContext is AddFavoritesViewModel)
                    {
                        AddFavoritesViewModel vm = view.DataContext as AddFavoritesViewModel;
                        vm.SecurityID = e.NewValue.ToString();
                    }
                }
            }
        }

        AddFavoritesViewModel viewModel = new AddFavoritesViewModel();
        public AddFavoritesView()
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        private void btnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            // 添加组
            AddGroupWindow addGroupWindow = new AddGroupWindow();
            addGroupWindow.Owner = this;
            addGroupWindow.ShowDialog();
            if (addGroupWindow.Result)
            {
                if (addGroupWindow.Result == true)
                {
                    HomePageDataHandler.AddGroup(addGroupWindow.GroupName);
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //向组里添加关注债券
            viewModel.ChangeGroupBonds();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
