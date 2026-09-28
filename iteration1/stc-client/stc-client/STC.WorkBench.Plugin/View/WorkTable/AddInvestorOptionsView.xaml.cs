using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
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

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// AddInvestorOptionsView.xaml 的交互逻辑
    /// </summary>
    public partial class AddInvestorOptionsView : WindowBase
    {
        public StcTbookUsers SelectedItem;

        public AddInvestorOptionsView()
        {
            InitializeComponent();
            if (CommonUtils.IsInvestor())
            {
                this.Title = "新增交易员筛选";
                this.tb.Text = "交易员:";
                this.InvestorsCb_edit.Watermark = "交易员";
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            SelectedItem = InvestorsCb_edit.SelectedItem as StcTbookUsers;
            this.Close();
        }

        private void InvestorsCb_edit_PatternChanged(object sender, AutoCompleteComboBox.AutoCompleteArgs args)
        {
            List<StcTbookUsers> list = new List<StcTbookUsers>();
            if (CommonUtils.IsInvestor())
            {
                List<StcTbookUsers> traders = CommonUtils.GetTraders();
                if (traders != null)
                {
                    if (!string.IsNullOrEmpty(args.Pattern))
                    {
                        list = traders.Where(d => (d.Username != null && d.Username.Contains(args.Pattern))
                        || (d.Name != null && d.Name.Contains(args.Pattern))).ToList();//多条件筛选，新功能增加
                    }
                    else
                    {
                        list = traders.ToList();
                    }
                }
            }
            else
            {
                List<StcTbookUsers> investors = CommonUtils.GetInvestors();
                args.DataSource = CommonUtils.GetInvestors();
                if (investors != null)
                {
                    if (!string.IsNullOrEmpty(args.Pattern))
                    {
                        list = investors.Where(d => (d.Username != null && d.Username.Contains(args.Pattern))
                        || (d.Name != null && d.Name.Contains(args.Pattern))).ToList();//多条件筛选，新功能增加
                    }
                    else
                    {
                        list = investors.ToList();
                    }
                }
            }
            args.DataSource = list;
        }

        private void InsNameCb_edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AutoCompleteComboBox box = sender as AutoCompleteComboBox;
            if (box != null)
            {
                if (string.IsNullOrEmpty(box.Text))
                {
                    box.ExpandOptions();
                }
            }
        }
    }
}
