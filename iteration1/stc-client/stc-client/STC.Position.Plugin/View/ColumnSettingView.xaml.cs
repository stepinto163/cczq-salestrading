using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.CustomUI.TabPage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// ColumnSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class ColumnSettingView : WindowBase
    {
        public const string POSITION = "持仓";
        public const string IniKey = "ColumnsInfo";

        private ObservableCollection<CloumnsInfo> _MyColumnsInfo = new ObservableCollection<CloumnsInfo>();
        private ObservableCollection<DataGridColumn> dgColumnsGroups = new ObservableCollection<DataGridColumn>();
        private MainView mainView;
        public ColumnSettingView(MainView view)
        {
            InitializeComponent();
            mainView = view;
            dgColumnsGroups = view.dgDetail.Columns;

            foreach (DataGridColumn column in dgColumnsGroups)
            {
                if (column.Header == null) continue;
                string columnName = column.Header.ToString();
                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = "选择";
                }
                CloumnsInfo ci = new CloumnsInfo();
                ci.IsVisible = column.Visibility == Visibility.Visible ? true : false;
                if (columnName == "债券代码"
                || columnName == "债券简称"
                || columnName == "我的Bid"
                || columnName == "我的Ofr")
                {
                    ci.IsEnable = false;
                }
                ci.ColumnName = columnName;
                ci.ColumnWidth = column.ActualWidth;
                ci.DisplayIndex = column.DisplayIndex;
                _MyColumnsInfo.Add(ci);
            }
            this.ColumnListBox.ItemsSource = _MyColumnsInfo;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {

            #region 保存表头
            StringBuilder columnsInfoSB = new StringBuilder();
            IOrderedEnumerable<CloumnsInfo> sortedColumnInfos = _MyColumnsInfo.OrderBy(x=>x.DisplayIndex);
            foreach (CloumnsInfo cloumnsInfo in sortedColumnInfos)
            {
                if (columnsInfoSB.Length > 0)
                    columnsInfoSB.Append(";");
                if (cloumnsInfo.IsVisible)
                {
                    columnsInfoSB.Append($"{cloumnsInfo.ColumnName}:{cloumnsInfo.ColumnWidth}");
                }
                else
                {
                    columnsInfoSB.Append($"({cloumnsInfo.ColumnName}):{cloumnsInfo.ColumnWidth}");
                }
            }
            IniFileHelper.WriteValue($"{POSITION}-{LocalEnv.UserInfo.UserName}", IniKey, columnsInfoSB.ToString());
            #endregion

            mainView.InitColumnsInfo();
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class CloumnsInfo : ViewModelBase
    {
        private bool _IsVisible = true;

        public bool IsVisible
        {
            get
            {
                return _IsVisible;
            }
            set
            {
                _IsVisible = value;
                this.OnPropertyChanged();
            }
        }

        private bool _IsEnable = true;

        public bool IsEnable
        {

            get
            {
                return _IsEnable;
            }
            set
            {
                _IsEnable = value;
                this.OnPropertyChanged();
            }
        }

        private string _ColumnName = string.Empty;

        public string ColumnName
        {
            get
            {
                return _ColumnName;
            }
            set
            {
                _ColumnName = value;
                this.OnPropertyChanged();
            }
        }

        private double _ColumnWidth;

        public double ColumnWidth
        {
            get
            {
                return _ColumnWidth;
            }
            set
            {
                _ColumnWidth = value;
                this.OnPropertyChanged();
            }
        }

        private int _displayIndex;

        public int DisplayIndex
        {
            get { return _displayIndex; }
            set { 
                _displayIndex = value;
                this.OnPropertyChanged();
            }
        }

    }
}
