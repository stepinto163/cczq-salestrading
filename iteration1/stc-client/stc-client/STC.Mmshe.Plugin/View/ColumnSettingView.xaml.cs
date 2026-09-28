using STC.Common.View.Component;
using STC.Mmshe.Plugin.ViewModel;
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

namespace STC.Mmshe.Plugin.View
{
    /// <summary>
    /// ColumnSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class ColumnSettingView : WindowBase
    {
        private ObservableCollection<CloumnsInfo> _MyColumnsInfo = new ObservableCollection<CloumnsInfo>();
        private ObservableCollection<DataGridColumn> dgColumns;

        public ColumnSettingView(ObservableCollection<DataGridColumn> columns)
        {
            InitializeComponent();
            dgColumns = columns;
            foreach (DataGridColumn column in columns)
            {
                if (column.Header == null) continue;
                string columnName = column.Header.ToString();
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

                _MyColumnsInfo.Add(ci);
            }
            this.ColumnListBox.ItemsSource = _MyColumnsInfo;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (CloumnsInfo cloumnsInfo in _MyColumnsInfo)
            {
                string columnName = cloumnsInfo.ColumnName;
                DataGridColumn col = dgColumns.First(o => { string headerName = o.Header == null ? string.Empty : o.Header.ToString(); return headerName == columnName; });
                if (col != null)
                {
                    col.Visibility = cloumnsInfo.IsVisible ? Visibility.Visible : Visibility.Hidden;
                }
            }
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
    }
}
