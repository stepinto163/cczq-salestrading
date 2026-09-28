using STC.XBond.Plugin.View.Component;
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

namespace STC.XBond.Plugin.View.Book
{
    /// <summary>
    /// BookUnderlineView.xaml 的交互逻辑
    /// </summary>
    public partial class BookUnderlineView : WindowBase
    {
        BookEditViewModel _BookEditViewModel = new BookEditViewModel(true);
        public BookUnderlineView()
        {
            InitializeComponent();
            //this.datePicker.BlackoutDates.AddDatesInPast();
            this.DataContext = _BookEditViewModel;

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
