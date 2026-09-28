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
    /// RefInquiryMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class RefInquiryMessageBox : WindowBase
    {
        private int _result = -1;
        /// <summary>
        /// 结果， -1：取消;  0：不发   1：发
        /// </summary>
        public int Result
        {
            get
            { 
                return _result;
            }
        }

        public RefInquiryMessageBox()
        {
            InitializeComponent();
        }

        public static int Show(Window owner = null)
        {
            int res = -1;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                RefInquiryMessageBox nb = new RefInquiryMessageBox();

                nb.Owner = owner ?? Application.Current.MainWindow;
                nb.ShowDialog();
                res = nb.Result;
            }));
            return res;
        }


        private void btnNotSend_Click(object sender, RoutedEventArgs e)
        {
            this._result = 0;
            this.Close();
            e.Handled = true;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            _result = 1;
            this.Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _result = -1;
            this.Close();
            e.Handled = true;
        }
    }
}
