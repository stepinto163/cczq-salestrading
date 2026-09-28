using com.gateway.log;
using STC.Broker.Plugin.Model.Transaction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace STC.Broker.Plugin.View.Component
{
    /// <summary>
    /// PopTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class PopTextBox : UserControl
    {
        public event Action<object, string> TextChanged;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("PopText", typeof(string), typeof(PopTextBox), new PropertyMetadata(null, OnTextChangedCallback));
        public static readonly DependencyProperty PopDataModelProperty = DependencyProperty.Register("SeletedPopData", typeof(PopDataModel), typeof(PopTextBox));


        public static ObservableCollection<PopDataModel> PopDataList { get; } = new ObservableCollection<PopDataModel>();

        public PopTextBox()
        {
            InitializeComponent();
        }


        public PopDataModel SeletedPopData
        {
            get { return (PopDataModel)GetValue(PopDataModelProperty); }
            set { SetValue(PopDataModelProperty, value); }
        }


        public static void OnTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string str = (string)e.NewValue;
            if (d is PopTextBox && e.NewValue is string && !string.IsNullOrEmpty(str))
            {
                PopTextBox pbox = d as PopTextBox;
                pbox.PopText = str.Trim();
            }
        }



        public string PopText
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                TBName.Text = value;
                TBoxName.Text = value;

                if (string.IsNullOrWhiteSpace(value))
                {
                    TBName.Visibility = Visibility.Collapsed;
                    TBoxName.Visibility = Visibility.Visible;
                    //Utils.wpf.DragDrop.CanDrop = false;
                }
                else
                {
                    TBName.Visibility = Visibility.Visible;
                    TBoxName.Visibility = Visibility.Collapsed;
                    Utils.wpf.DragDrop.CanDrop = true;
                }
            }
        }


        /// <summary>
        /// 双击激活
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e == null || e.ClickCount == 2)
            {
                TBName.Visibility = Visibility.Collapsed;
                TBoxName.Visibility = Visibility.Visible;
                Utils.wpf.DragDrop.CanDrop = false;

                TBoxName.Text = TBName.Text;

                TBoxName.SelectAll();
                TBoxName.Focus();

                TextChanged(TBoxName.Text, string.Empty);

                if (PopDataList.Count > 0)
                {
                    PopData.DataContext = PopDataList;
                    PopData.SelectedIndex = 0;
                    PopName.IsOpen = true;
                    PopName.StaysOpen = true;
                    LogHelper.Debug($"PopDataList.Count = {PopDataList.Count}");
                }
                else
                {
                    PopData.DataContext = null;
                    PopName.IsOpen = false;
                }
            }
        }



        private void TBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(TBoxName.Text))
                {
                    this.Focus();
                }
                else if (PopName.IsOpen)
                {
                    PopData_MouseUpClick(null, null);
                }

                //TBName.Visibility = Visibility.Visible;
                //TBoxName.Visibility = Visibility.Collapsed;
                //Utils.wpf.DragDrop.CanDrop = true;
            }
        }


        private void TBoxName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || e.Key == Key.Down)
            {
                if (PopData.SelectedIndex < PopData.Items.Count - 1)
                {
                    PopData.SelectedIndex++;
                }
                if (PopData.SelectedItem != null)
                {
                    PopData.ScrollIntoView(PopData.SelectedItem);
                }
            }
            else if (e.Key == Key.Up)
            {
                if (PopData.SelectedIndex > 0)
                {
                    PopData.SelectedIndex--;
                }
                if (PopData.SelectedItem != null)
                {
                    PopData.ScrollIntoView(PopData.SelectedItem);
                }
            }
            else if (e.Key == Key.Escape)
            {
                TBoxName.Text = string.Empty;
            }
        }


        private void TBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TBoxName.Visibility == Visibility.Collapsed)
                return;

            Utils.wpf.DragDrop.CanDrop = false;
            if (!string.IsNullOrEmpty(TBoxName.Text) && !TBName.Text.Equals(TBoxName.Text))
            {
                TextChanged(this, TBoxName.Text.Trim());

                if (PopDataList.Count > 0)
                {
                    PopData.DataContext = PopDataList;
                    PopName.IsOpen = true;
                    PopData.SelectedIndex = 0;
                    PopName.PlacementTarget = TBoxName;
                    PopName.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                }
                else
                {
                    PopData.DataContext = null;
                    PopName.IsOpen = false;
                }
            }
            else
            {
                PopData.DataContext = null;
                PopName.IsOpen = false;
            }
        }


        private void TBoxName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if(PopName.StaysOpen && PopData.SelectedIndex == -1)
            //{
            //    PopName.IsOpen = false;
            //    PopName.StaysOpen = false;
            //}
            if (!PopName.IsOpen)
            {
                TBoxName_KeyDown(null, null);
            }
            else if (PopName.StaysOpen)
            {
                PopName.StaysOpen = false;
            }
        }



        private void PopData_MouseUpClick(object sender, MouseButtonEventArgs e)
        {
            var dv = PopData.SelectedItem as PopDataModel;
            if (dv != null)
            {
                PopName.IsOpen = false;
                PopName.StaysOpen = false;

                TBoxName.Visibility = Visibility.Collapsed;
                TBName.Visibility = Visibility.Visible;
                Utils.wpf.DragDrop.CanDrop = true;

                var temp = dv.PopShow.Trim();
                //Text = temp;
                TBoxName.Text = temp;
                TBName.Text = temp;
                PopText = TBoxName.Text.Trim();
                SeletedPopData = dv;
            }
        }

        private void PopData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = e.OriginalSource as TextBlock;
            if (textBlock != null)
            {
                var dv = textBlock.DataContext as PopDataModel;
                if (dv != null)
                {
                    PopName.IsOpen = false;
                    PopName.StaysOpen = false;

                    TBoxName.Visibility = Visibility.Collapsed;
                    TBName.Visibility = Visibility.Visible;

                    var temp = dv.PopShow.Trim();
                    //Text = temp;
                    TBoxName.Text = temp;
                    TBName.Text = temp;
                    PopText = TBoxName.Text.Trim();
                    SeletedPopData = dv;
                }
            }
        }


        private void TBoxName_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox box = sender as TextBox;
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    TBName_MouseDown(null, null);
                }
                else
                {
                    box.SelectAll();
                    box.Focus();
                }
            }
        }
    }
}
