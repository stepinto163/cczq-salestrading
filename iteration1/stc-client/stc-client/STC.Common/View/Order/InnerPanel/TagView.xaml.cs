using STC.Common.ViewModel.Order.InnerPanel;
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

namespace STC.Common.View.Order.InnerPanel
{
    /// <summary>
    /// TagView.xaml 的交互逻辑
    /// </summary>
    public partial class TagView : UserControl
    {
        private TagViewModel tagViewModel = null;

        public TagView()
        {
            InitializeComponent();

            tagViewModel = new TagViewModel();

            this.DataContext = tagViewModel;

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SelectedUserIDCB_PatternChanged(object sender, Component.AutoCompleteComboBox.AutoCompleteArgs args)
        {
            if (tagViewModel != null)
            {
                tagViewModel.FilterInstitution(args);
            }
        }

        private void TrdNameCb_edit_LostFocus(object sender, RoutedEventArgs e)
        {
        }
    }
}
