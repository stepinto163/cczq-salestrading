using STC.quant.plugin.View.HomePage;
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

namespace STC.quant.plugin.SelectorTemplate
{
    /// <summary>
    /// TextFieldItem.xaml 的交互逻辑
    /// </summary>
    public partial class TextFieldItem : UserControl
    {
        public TextFieldItem()
        {
            InitializeComponent();
        }
        // 定义一个依赖事件
        public static readonly RoutedEvent AddEvent = EventManager.RegisterRoutedEvent("AddEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextFieldItem));
        public static readonly RoutedEvent EditEvent = EventManager.RegisterRoutedEvent("EditEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextFieldItem));
        // 封装依赖事件的事件访问器
        public event RoutedEventHandler AddEventHandler
        {
            add { AddHandler(AddEvent, value); }
            remove { RemoveHandler(AddEvent, value); }
        }
        public event RoutedEventHandler EditEventHandler
        {
            add { AddHandler(EditEvent, value); }
            remove { RemoveHandler(EditEvent, value); }
        }
        // 触发依赖事件
        private void RaiseAddEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs(AddEvent, this);
            RaiseEvent(args);
        }
        private void RaiseEditEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs(EditEvent, this);
            RaiseEvent(args);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            RaiseAddEvent();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            RaiseEditEvent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
