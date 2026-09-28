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

namespace STC.Broker.Plugin.View.UI
{
    /// <summary>
    /// LayoutSetting.xaml 的交互逻辑
    /// </summary>
    public partial class LayoutSetting : UserControl
    {
        private CheckBox[][] checkBoxs;
        public event EventHandler<object> LayoutChanged;
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public LayoutSetting()
        {
            InitializeComponent();

            int row = 6, col = 12;

            for (int i = 0; i < col; i++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(24);
                mainGrid.ColumnDefinitions.Add(cd);
            }
            for (int i = 0; i < row; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(24);
                mainGrid.RowDefinitions.Add(rd);
            }

            checkBoxs = new CheckBox[row][];
            for (int i = 0; i < row; i++)
            {
                checkBoxs[i] = new CheckBox[col];
                for(int j = 0; j < col; j++)
                {
                    CheckBox cb = new CheckBox();
                    cb.Style = this.Resources["CellCheckBoxStyle"] as Style;
                    cb.Margin = new Thickness(1);
                    Grid.SetColumn(cb, j);
                    Grid.SetRow(cb, i);
                    mainGrid.Children.Add(cb);
                    cb.Click += Cb_Click;
                    checkBoxs[i][j] = cb;
                    cb.IsEnabled = (j >= 1);
                }
            }
        }

        public void SetRowColumn(int rows, int cols)
        {
            this.Rows = rows;
            this.Columns = cols;
            int row = rows - 1, col = cols - 1;
            for (int i = 0; i < checkBoxs.Length; i++)
            {
                for (int j = 0; j < checkBoxs[i].Length; j++)
                {
                    checkBoxs[i][j].IsChecked = (i <= row && j <= col);
                }
            }
        }

        private void Cb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            int row = Grid.GetRow(cb);
            int col = Grid.GetColumn(cb);
            if (col < 1)
            {
                return;
            }
            for (int i = 0; i < checkBoxs.Length; i++)
            {
                for (int j = 0; j < checkBoxs[i].Length; j++)
                {
                    checkBoxs[i][j].IsChecked = (i <= row && j <= col);
                }
            }
            Rows = row + 1;
            Columns = col + 1;
            FireLayoutChangedEvent();
        }

        private void FireLayoutChangedEvent()
        {
            LayoutChanged?.Invoke(this, new object());
        }
    }
}
