using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STC.Broker.Plugin.View.Component
{
    public class ValidationTextBox : TextBox
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
          "ErrorMessage", typeof(object), typeof(ValidationTextBox), new FrameworkPropertyMetadata(null));
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage
        {
            get { return (string)GetValue(DataProperty); }
            set
            {
                SetValue(DataProperty, value);
            }
        }

        public static readonly DependencyProperty ValidationResultDataProperty = DependencyProperty.Register(
          "ValidationResult", typeof(bool), typeof(ValidationTextBox), new FrameworkPropertyMetadata(null));
        /// <summary>
        /// 验证结果
        /// </summary>
        public bool ValidationResult
        {
            get { return (bool)GetValue(ValidationResultDataProperty); }
            set
            {
                SetValue(ValidationResultDataProperty, value);
            }
        }

        public ValidationTextBox()
        {

            //this.TextChanged += ValidationTextBox_TextChanged;
        }

        //private void ValidationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
