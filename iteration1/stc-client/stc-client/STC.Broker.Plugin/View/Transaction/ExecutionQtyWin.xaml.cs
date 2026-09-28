using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace STC.Broker.Plugin.View.Transaction
{
    public partial class ExecutionQtyWin : Window
    {
        private static readonly Lazy<ExecutionQtyWin> execWin = new Lazy<ExecutionQtyWin>(() => new ExecutionQtyWin());
        public static ExecutionQtyWin Instance
        {
            get
            {
                return execWin.Value;
            }
        }

        public ExecutionQtyViewModel executionQtyViewModel;
        public ExecutionQtyWin()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = executionQtyViewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }


    public class ExecutionQtyViewModel 
    {
        private decimal buyQty;

        public decimal BuyQty
        {
            get { return buyQty; }
            set
            {
                if (buyQty != value)
                {
                    buyQty = value;
                }
            }
        }

        private decimal sellQty;

        public decimal SellQty
        {
            get { return sellQty; }
            set
            {
                if (sellQty != value)
                {
                    sellQty = value;
                }
            }
        }

        private Visibility visibilityBuy;

        public Visibility VisibilityBuy
        {
            get { return visibilityBuy; }
            set { visibilityBuy = value; }
        }

        private Visibility visibilitySell;

        public Visibility VisibilitySell
        {
            get { return visibilitySell; }
            set { visibilitySell = value; }
        }
    }
}
