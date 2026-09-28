using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.WorkBench.Plugin.View.Component
{
    /// <summary>
    /// SettleTypeSelector.xaml 的交互逻辑
    /// </summary>
    public partial class SettleTypeSelector : TextBox
    {
        protected Popup popupPart
        {
            get
            {
                return base.GetTemplateChild("popup_part") as Popup;
            }
        }

        protected Calendar calendarPart
        {
            get
            {
                return base.GetTemplateChild("calendar") as Calendar;
            }
        }

        protected TextBlock datePart
        {
            get
            {
                return base.GetTemplateChild("Date_Part") as TextBlock;
            }
        }

        protected ComboBox typePart
        {
            get
            {
                return base.GetTemplateChild("Type_Part") as ComboBox;
            }
        }

        protected Button okBtn
        {
            get
            {
                return base.GetTemplateChild("okBtn") as Button;
            }
        }


        public string CreateTime
        {
            get { return (string)GetValue(CreateTimeProperty); }
            set { SetValue(CreateTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CreateTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CreateTimeProperty =
            DependencyProperty.Register("CreateTime", typeof(string), typeof(SettleTypeSelector), new PropertyMetadata(DateTime.Now.ToString("yyyyMMdd")));



        public string SettleType
        {
            get { return (string)GetValue(SettleTypeProperty); }
            set { SetValue(SettleTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SettleType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettleTypeProperty =
            DependencyProperty.Register("SettleType", typeof(string), typeof(SettleTypeSelector), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnSettleTypePropertyChange)));

        private static void OnSettleTypePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            SettleTypeSelector source = (SettleTypeSelector)d;

            string newValue = e.NewValue.ToString();
            source.Text = ComboSourceUtil.Instance.FindEnum(newValue, "settletype");
        }

        public SettleTypeSelector()
        {
            InitializeComponent();
        }

        private void calendarButton_Click(object sender, RoutedEventArgs e)
        {
            calendarPart.BlackoutDates.AddDatesInPast();
            string tradeDate = CommonUtils.CalTradeDate(this.SettleType);
            this.calendarPart.SelectedDate = CommonUtils.ConvertToDateTime2(tradeDate);
            DateTime settlDateTime;
            int selectIndex;
            CommonUtils.SetSettleType(this.SettleType, tradeDate, out settlDateTime, out selectIndex);
            this.datePart.Text = this.calendarPart.SelectedDate?.ToString("MM.dd");
            this.typePart.SelectedIndex = selectIndex;
            popupPart.IsOpen = true;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            DateTime createtime = CommonUtils.ConvertToDateTime(this.CreateTime);
            string settlType = CommonUtils.IsSpotSettle(createtime, this.calendarPart.SelectedDate, this.typePart.SelectedIndex);
            if (string.IsNullOrEmpty(settlType))
            {
                settlType = calendarPart.SelectedDate?.ToString("MM.dd") + (this.typePart.SelectedIndex == 0 ? "+0" : "+1");
            }
            this.SettleType = settlType;
            popupPart.IsOpen = false;
            this.Focus();
        }

        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.datePart != null)
            {
                this.datePart.Text = this.calendarPart.SelectedDate?.ToString("MM.dd");
            }
        }

        private void calendar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured is CalendarItem)
            {
                Mouse.Capture(null);
            }
        }
    }
}
