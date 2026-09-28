using STC.Common.ColumnSetting;
using STC.Common.Utils;
using STC.Common.View.Component;
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

namespace STC.Common.ColumnSetting
{
    /// <summary>
    /// ColumnSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class ColumnSettingView : WindowBase
    {
        public const string IniKey = "ColumnsInfo";

        //private ObservableCollection<ColumnsInfo> _MyColumnsInfo = new ObservableCollection<ColumnsInfo>();
        private ISettingColumnsBase _dg;

        public ColumnSettingView(ISettingColumnsBase exDataGrid)
        {
            InitializeComponent();
            _dg = exDataGrid;
            this.ColumnListBox.ItemsSource = _dg.GetColumnsInfos();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<ColumnsInfo> columnsInfoList = (ObservableCollection<ColumnsInfo>)this.ColumnListBox.ItemsSource;

            if (columnsInfoList == null || columnsInfoList.Any(item => item.IsVisible) == false)
            {
                CustomMessageBox.Info("不可全部隐藏!", "提示", this);
            }
            else
            {
                if (_dg != null)
                {
                    _dg.SaveColumnInfo(columnsInfoList);

                    SettingSyncHandler.Instance.RefreshColumns(_dg.GetName());
                }

                this.Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_dg != null)
            {
                // 删除记录
                _dg.DeleteColumnInfo();

                SettingSyncHandler.Instance.LoadDefaultColumns(_dg.GetName());
            }
            this.Close();
        }
    }
}
