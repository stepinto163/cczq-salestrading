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

namespace STC.Common.View.Drp.Component.Broker
{
    /// <summary>
    /// AmountUI.xaml 的交互逻辑
    /// </summary>
    public partial class AmountUI : UserControl
    {
        public event Action<decimal> AmountChanged;

        private decimal maxAmount = decimal.MaxValue;
        public decimal MaxAmount
        {
            get { return maxAmount; }
            set
            {
                maxAmount = value;
                SetButtonEnable();
            }
        }
        private decimal MinAmount = decimal.Zero;
        public decimal AmountUnit { get { return 1000; } }

        private decimal amount = decimal.Zero;
        public decimal Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                if (!tbAmount.IsFocused)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        tbAmount.Text = amount / AmountUnit + "";
                    });
                }
                SetButtonEnable();
            }
        }

        private void SetButtonEnable()
        {
            this.Dispatcher.Invoke(() =>
            {
                btnAdd.IsEnabled = amount + AmountUnit <= MaxAmount;
                btnSubtraction.IsEnabled = amount - AmountUnit > MinAmount;
            });
        }

        public AmountUI()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(tbAmount, OnPaste);
        }

        private void btnSubtraction_Click(object sender, RoutedEventArgs e)
        {
            if (amount - AmountUnit > MinAmount)
            {
                Amount -= AmountUnit;
                AmountChange();
            }
        }

        private void AmountChange()
        {
            if (AmountChanged != null)
            {
                AmountChanged(amount);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (amount + AmountUnit <= MaxAmount)
            {
                Amount += AmountUnit;
                AmountChange();
            }
        }

        private void tbAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbAmount.IsFocused)
            {
                decimal tmp;
                if (decimal.TryParse(tbAmount.Text + "", out tmp))
                {
                    if (tmp * AmountUnit <= MaxAmount && tmp * AmountUnit > MinAmount)
                    {
                        Amount = tmp * AmountUnit;
                        AmountChange();
                    }
                    else
                    {
                        tbAmount.Text = Amount / AmountUnit + "";
                        tbAmount.Select(tbAmount.Text.Length, 0);
                    }
                }
            }
        }

        private void tbAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex reg = new Regex(@"^[1-9]\d*$");
            bool ismatch = !reg.IsMatch(e.Text);
            if (ismatch)
            {
                if ("0".Equals(e.Text) && tbAmount.SelectionStart > 0)
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = false;
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
            e.Handled = true;
        }

        private void tbAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }
    }
}
