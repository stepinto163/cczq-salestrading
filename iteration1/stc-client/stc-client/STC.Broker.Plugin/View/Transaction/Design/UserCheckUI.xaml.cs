using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.ViewModel.Transaction;
using STC.Broker.Plugin.ViewModel.Transaction.BridgeDesign;
using STC.Common.Model.PT;
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

namespace STC.Broker.Plugin.View.Transaction.Design
{
    /// <summary>
    /// UserCheckUI.xaml 的交互逻辑
    /// </summary>
    public partial class UserCheckUI : UserControl
    {
        public UserRoleSettingDetail AccountInfo;
        public event Action<UserCheckUI> OnDelete;

        public ItemCheckViewModel model;

        public UserCheckUI()
        {
            InitializeComponent();

        }

        public void SetWidth(int width, int tbWidth)
        {
            this.Width = width;
            tbName.Width = tbWidth;
        }


        public UserRoleSettingDetail getAccountInfo()
        {
            return AccountInfo;
        }

        public string getUserName()
        {
            return tbName.Text;
        }

        public void setData(UserRoleSettingDetail AccountInfo)
        {
            this.AccountInfo = AccountInfo;
            this.Dispatcher.Invoke(new Action(() =>
            {
                tbName.Text = AccountInfo.userDisplayName;
            }));
        }

        public void SetData(ItemCheckViewModel viewModel)
        {
            model = viewModel;
            Dispatcher.Invoke(new Action(() =>
            {
                tbName.Text = model.Text;
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (OnDelete != null)
                OnDelete(this);
        }
    }
}
