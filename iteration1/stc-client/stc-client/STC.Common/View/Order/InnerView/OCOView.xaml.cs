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
    /// OCOView.xaml 的交互逻辑
    /// </summary>
    public partial class OCOView : UserControl
    {
        private OCOViewModel ocoViewModel = new OCOViewModel();

        public event Action<Dictionary<string, bool>> onCheckChange;

        public OCOView()
        {
            InitializeComponent();

            this.DataContext = ocoViewModel;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            onCheckChange?.Invoke(GetCheckDic());
        }

        public Dictionary<string, bool> GetCheckDic()
        {
            Dictionary<string, bool> checkDic = new Dictionary<string, bool>();

            checkDic.Add("all", ckAll.IsChecked.HasValue ? ckAll.IsChecked.Value : false);
            checkDic.Add("inst", ckInst.IsChecked.HasValue ? ckInst.IsChecked.Value : false);
            checkDic.Add("xbond", ckXbond.IsChecked.HasValue ? ckXbond.IsChecked.Value : false);
            checkDic.Add("inner", ckInner.IsChecked.HasValue ? ckInner.IsChecked.Value : false);

            return checkDic;
        }
    }
}
