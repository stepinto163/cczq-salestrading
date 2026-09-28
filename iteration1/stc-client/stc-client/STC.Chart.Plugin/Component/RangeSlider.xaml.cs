using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Chart.Plugin.Common.Component
{
    /// <summary>
    /// Interaction logic for RangeSlider.xaml
    /// </summary>
    public partial class RangeSlider : UserControl
    {
        public event Action<double, double> ValueChanged;

        public RangeSlider()
        {
            InitializeComponent();
            
            this.Loaded += Slider_Loaded;

            RangeMargin = 0;
        }

        void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            UpperSlider.ValueChanged += UpperSlider_ValueChanged;
        }

        private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpperSlider.Value = Math.Max(UpperSlider.Value , LowerSlider.Value + RangeMargin);
            TriggerValueChanged();
        }

        private void TriggerValueChanged()
        {
            //if(System.Windows.Input.Mouse.LeftButton== MouseButtonState.Pressed)
            //{
            //    return;
            //}
            if (ValueChanged != null)
            {
                ValueChanged(LowerValue, UpperValue);
            }
        }

        private void UpperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LowerSlider.Value = Math.Min(UpperSlider.Value - RangeMargin, LowerSlider.Value);
            TriggerValueChanged();
        }

        private double _rangeMargin = 0;
        public double RangeMargin
        {
            get
            {
                return _rangeMargin;
            }
            set
            {
                _rangeMargin = value;
                UMinimum = Minimum + value;
                LMaximum = Maximum - value;
            }
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set
            {
                SetValue(MinimumProperty, value);
                LMinimum = value;
                UMinimum = value + RangeMargin;
            }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }

        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double UpperValue
        {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }

        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set
            {
                SetValue(MaximumProperty, value);
                LMaximum = value - RangeMargin;
                UMaximum = value;
            }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));


        public double LMaximum
        {
            get { return (double)GetValue(LMaximumProperty); }
            set { SetValue(LMaximumProperty, value); }
        }

        public static readonly DependencyProperty LMaximumProperty =
            DependencyProperty.Register("LMaximum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));

        public double UMaximum
        {
            get { return (double)GetValue(UMaximumProperty); }
            set { SetValue(UMaximumProperty, value); }
        }

        public static readonly DependencyProperty UMaximumProperty =
            DependencyProperty.Register("UMaximum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));


        public double LMinimum
        {
            get { return (double)GetValue(LMinimumProperty); }
            set { SetValue(LMinimumProperty, value); }
        }

        public static readonly DependencyProperty LMinimumProperty =
            DependencyProperty.Register("LMinimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));


        public double UMinimum
        {
            get { return (double)GetValue(UMinimumProperty); }
            set { SetValue(UMinimumProperty, value); }
        }

        public static readonly DependencyProperty UMinimumProperty =
            DependencyProperty.Register("UMinimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));
    }
}
