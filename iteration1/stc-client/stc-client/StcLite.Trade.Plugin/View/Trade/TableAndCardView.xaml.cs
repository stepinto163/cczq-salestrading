using BDB.Trade.Plugin.Model;
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

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// TableAndCardView.xaml 的交互逻辑
    /// </summary>
    public partial class TableAndCardView : UserControl
    {
        public TableAndCardView()
        {
            InitializeComponent();
        }

        public TableAndCardView(string groupId, DisplayMode showModel)
        {
            InitializeComponent();

            if (showModel == DisplayMode.List)
                SetShowModel(groupId, showModel, true);
        }

        public void SetShowModel(string groupId, DisplayMode showModel, bool refresh)
        {
            if (showModel == DisplayMode.List)
            {
                pageCardView.Visibility = Visibility.Collapsed;
                dgPriceTableView.Visibility = Visibility.Visible;

                dgPriceTableView.Init(groupId, refresh);

                pageCardView.SetGroupId(groupId);
            }
            else
            {
                pageCardView.Visibility = Visibility.Visible;
                dgPriceTableView.Visibility = Visibility.Collapsed;

                pageCardView.Init(groupId, refresh);

                //pageCardView.ClearUI();

            }
        }
    }
}
