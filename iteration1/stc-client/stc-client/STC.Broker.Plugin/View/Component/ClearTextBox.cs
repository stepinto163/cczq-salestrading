using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace STC.Broker.Plugin.View.Component
{
    public class ClearTextBox : TextBox
    {
        public ClearTextBox()
        {

            this.TextChanged += (s, e) => { ChangeClearButtonState(); };
            this.LostKeyboardFocus += (s, e) => { ChangeClearButtonState(); };
            this.GotKeyboardFocus += (s, e) => { ChangeClearButtonState(); };
            ApplyTemplate();
        }

        protected virtual void ChangeClearButtonState()
        {
            if (String.IsNullOrEmpty(this.Text))
            {
                ClearTextButton.Visibility = System.Windows.Visibility.Collapsed;

                if (!this.IsKeyboardFocused)
                {
                    tbBG.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    tbBG.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            else
            {
                ClearTextButton.Visibility = System.Windows.Visibility.Visible;
                if (!this.IsKeyboardFocused)
                {
                    this.Foreground = (SolidColorBrush)this.FindResource("ColorBrush.Yellow");
                }
                else
                {
                    this.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
                }
            }
        }

        Button ClearTextButton;
        TextBlock tbBG;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ClearTextButton = base.GetTemplateChild("ClearTextButton") as Button;
            if (ClearTextButton != null)
                ClearTextButton.Click += (s, e) => { this.Text = string.Empty; };
            tbBG = base.GetTemplateChild("tbBG") as TextBlock;
        }

        //CLR属性进行封装
        public string BackGroundText
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        //定义依赖属性
        public static readonly DependencyProperty BackGroundTextProperty = DependencyProperty.Register("BackGroundText", typeof(string), typeof(ClearTextBox));
    }
}
