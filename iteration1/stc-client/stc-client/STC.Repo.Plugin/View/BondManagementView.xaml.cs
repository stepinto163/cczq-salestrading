using STC.Common.View.Component;
using STC.Repo.Plugin.ViewModel;
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

namespace STC.Repo.Plugin.View
{
    /// <summary>
    /// BondManagementView.xaml 的交互逻辑
    /// </summary>
    public partial class BondManagementView : UserControl
    {
        private BondManagementViewModel vm = new BondManagementViewModel();

        public BondManagementView()
        {
            InitializeComponent();

            Dictionary<string, string> institutions = new Dictionary<string, string>();
            institutions["全部"] = "全部";
            institutions["1"] = "上清所";
            institutions["2"] = "中债登";
            InstitutionCb.ItemsSource = institutions;

            this.DataContext = vm;
        }

        private void qptAccount_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            vm.SelectedQtpAccounts = ccb.SelectedValues;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in dg_total.Columns)
            {
                DataGridTextColumn cl = new DataGridTextColumn();
                cl.Header = item.Header;
                cl.Width = item.Width;

                Binding widthBd = new Binding();
                widthBd.Source = item;
                widthBd.Mode = BindingMode.OneWay;
                widthBd.Path = new PropertyPath(DataGridColumn.WidthProperty);
                BindingOperations.SetBinding(cl, DataGridTextColumn.WidthProperty, widthBd);

                Binding visibleBd = new Binding();
                visibleBd.Source = item;
                visibleBd.Mode = BindingMode.OneWay;
                visibleBd.Path = new PropertyPath(DataGridColumn.VisibilityProperty);
                BindingOperations.SetBinding(cl, DataGridTextColumn.VisibilityProperty, visibleBd);

                Binding indexBd = new Binding();
                indexBd.Source = item;
                indexBd.Mode = BindingMode.OneWay;
                indexBd.Path = new PropertyPath(DataGridColumn.DisplayIndexProperty);
               // indexBd.Converter = new DataGridDisplayIndexConverter();
                BindingOperations.SetBinding(cl, DataGridTextColumn.DisplayIndexProperty, indexBd);

                TotalRow_total.Columns.Add(cl);

                if (item.Header.ToString() == "债券代码")
                {
                    Binding data_CT0 = new Binding();
                    data_CT0.Path = new PropertyPath("SecurityID");
                    cl.Binding = data_CT0;
                }

                if (item.Header.ToString() == "债券名称")
                {
                    Binding data_CT0 = new Binding();
                    data_CT0.Path = new PropertyPath("Symbol");
                    cl.Binding = data_CT0;
                }

                if (item.Header.ToString() == "T+0可用券(万)")
                {
                    Binding data_CT0 = new Binding();
                    data_CT0.Path = new PropertyPath("CT0");
                    data_CT0.StringFormat = "#,##0.00万";
                    cl.Binding = data_CT0;
                }

                if (item.Header.ToString() == "T+1可用券(万)")
                {
                    Binding data_CT1 = new Binding();
                    data_CT1.Path = new PropertyPath("CT1");
                    data_CT1.StringFormat = "#,##0.00万";
                    cl.Binding = data_CT1;
                }

                if (item.Header.ToString() == "租借持仓(万)")
                {
                    Binding data_CT1 = new Binding();
                    data_CT1.Path = new PropertyPath("M0");
                    data_CT1.StringFormat = "#,##0.00万";
                    cl.Binding = data_CT1;
                }

                if (item.Header.ToString() == "T+0已质押债券(万)")
                {
                    Binding data_CT1 = new Binding();
                    data_CT1.Path = new PropertyPath("ST0");
                    data_CT1.StringFormat = "#,##0.00万";
                    cl.Binding = data_CT1;
                }

                //if (item.Header.ToString() == "T+1已质押债券(万)")
                //{
                //    Binding data_CT1 = new Binding();
                //    data_CT1.Path = new PropertyPath("ST1");
                //    cl.Binding = data_CT1;
                //}
            }

            foreach (var item in dg_detail.Columns)
            {
                DataGridTextColumn cl = new DataGridTextColumn();
                cl.Header = item.Header;
                cl.Width = item.Width;

                Binding widthBd = new Binding();
                widthBd.Source = item;
                widthBd.Mode = BindingMode.OneWay;
                widthBd.Path = new PropertyPath(DataGridColumn.WidthProperty);
                BindingOperations.SetBinding(cl, DataGridTextColumn.WidthProperty, widthBd);

                Binding visibleBd = new Binding();
                visibleBd.Source = item;
                visibleBd.Mode = BindingMode.OneWay;
                visibleBd.Path = new PropertyPath(DataGridColumn.VisibilityProperty);
                BindingOperations.SetBinding(cl, DataGridTextColumn.VisibilityProperty, visibleBd);

                Binding indexBd = new Binding();
                indexBd.Source = item;
                indexBd.Mode = BindingMode.OneWay;
                indexBd.Path = new PropertyPath(DataGridColumn.DisplayIndexProperty);
               // indexBd.Converter = new DataGridDisplayIndexConverter();
                BindingOperations.SetBinding(cl, DataGridTextColumn.DisplayIndexProperty, indexBd);

                TotalRow_detail.Columns.Add(cl);

                if (item.Header.ToString() == "编号")
                {
                    Binding data_Code = new Binding();
                    data_Code.Path = new PropertyPath("Code");
                    cl.Binding = data_Code;
                }

                if (item.Header.ToString() == "券面总额(万)")
                {
                    Binding data_CouponAmount = new Binding();
                    data_CouponAmount.Path = new PropertyPath("CouponAmount");
                    data_CouponAmount.StringFormat = "#,##0.00万";
                    cl.Binding = data_CouponAmount;
                }
            }
        }
    }
}
