using STC.Common.Model.TBook;
using STC.Common.ViewModel.Order.InnerView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace STC.Common.View.Order.InnerView
{
    /// <summary>
    /// ConditionCancelView.xaml 的交互逻辑
    /// </summary>
    public partial class ConditionCancelView : UserControl
    {

        private ConditionCancelViewModel conditionCancelViewModel = new ConditionCancelViewModel();

        public ConditionCancelView()
        {
            InitializeComponent();

            SetConditionTxt(3);
            this.DataContext = conditionCancelViewModel;
        }

        public PriceCondition GetPriceCondition()
        {
            return conditionCancelViewModel.GetPriceCondition();
        }

        public void SetConditionTxt(int showType)
        {
            conditionCancelViewModel.SetContionTxt(showType);
        }

        public void SetPriceCondition(string priceCondition)
        {
            conditionCancelViewModel.SetPriceCondition(priceCondition);
        }

        public void SetBrush(Brush brush)
        {
            conditionCancelViewModel.TxBrush = brush;
        }

        public void SetBdBrush(Brush brush)
        {
            conditionCancelViewModel.BdFocusBrush = brush;
        }


        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(Key.V) || e.KeyboardDevice.IsKeyDown(Key.Space))
            {
                e.Handled = true;
                return;
            }

            string text = tbMorethanParty.Text;
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                string afterTxt = tbMorethanParty.Text.Substring(0, (tbMorethanParty.SelectionStart==0?0:tbMorethanParty.SelectionStart - 1))
                       + tbMorethanParty.Text.Substring(tbMorethanParty.SelectionStart, tbMorethanParty.Text.Length - tbMorethanParty.SelectionStart);


                decimal d = 0;
                if (afterTxt.Length > 0)
                {
                    if (!decimal.TryParse(afterTxt, out d))
                    {
                        return;
                    }
                }


                decimal max = 9999.99M;
                decimal min = -9999.99M;

                if (tbMorethanParty.SelectionStart > 0)
                {
                   
                    if (decimal.TryParse(afterTxt, out d) && (d > max || d < min))
                    {
                        e.Handled = true;
                        return;
                    }
                }
               
            }

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            
            Regex regex = new Regex("^[0-9.-]+$");


            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;



            decimal max = 9999.99M;
            decimal min = -9999.99M;


            string text = tb.Text;

            if ((text.Contains(".") && e.Text.Equals("."))
                || (text.Contains("-") && e.Text.Equals("-")))
            {
                e.Handled = true;
                return;
            }

            string selectTxt = tb.SelectedText;
            decimal d = 0;
            
            if (selectTxt.Length > 0)
            {
                string start = text.Substring(0, tb.SelectionStart);
                string end = text.Substring(tb.SelectionStart + selectTxt.Length, (text.Length - tb.SelectionStart - selectTxt.Length));
                string newNum = string.Format("{0}{1}{2}", start, e.Text, end);

                if (decimal.TryParse(newNum, out d))
                {
                    if (d > max)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }

            int inputIndex = tb.SelectionStart;
            text = text.Insert(inputIndex, e.Text);
            if (decimal.TryParse(text, out d))
            {
                if (d > max || (d == 0 && (text + e.Text).Length > 8))
                {
                    e.Handled = true;
                    return;
                }

            }

            if (decimal.TryParse(text, out d) && (d > max || d < min))
            {
                e.Handled = true;
                return;
            }

            string[] arr = text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (arr != null && arr.Length > 1)
            {
                if (decimal.TryParse(arr[1], out d) && arr[1].Length > 2)
                {
                    e.Handled = true;
                    return;
                }
            }


            e.Handled = false;
        }
    }
}
