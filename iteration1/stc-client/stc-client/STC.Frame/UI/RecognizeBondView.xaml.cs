using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.MyTrading;
using STC.Frame.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.Frame.UI
{
    /// <summary>
    /// RecognizeDealWin.xaml 的交互逻辑
    /// </summary>
    public partial class RecognizeBondView : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }
        public List<HomeGroupSecurityidsUpdateReq> UpdateList { get; private set; }
        public bool isHomeGroup { get; private set; }
        public HomeGroupInfo SelectedAddHomeGroup { get; private set; }
        RecognizeBondViewModel viewModel = new RecognizeBondViewModel();
        //private readonly string WinHeight = "Height";
        //private readonly string WinWidth = "Width";
        //private readonly string PositionX = "PositionX";
        //private readonly string PositionY = "PositionY";
        //private readonly string WinIniKey = $"SecurityGroupManager_{ LocalEnv.UserInfo.UserName}_RecognizeBond";
        public RecognizeBondView()
        {
            InitializeComponent();

            this.DataContext = viewModel;

            this.Loaded += RecognizeDealWin_Loaded;
        }
        public void setDefaultGroupID(string groupid, ObservableCollection<HomeGroupInfo> AddHomeGroupDic)
        {
            viewModel.AddHomeGroupDic.Clear();
            viewModel.AddHomeGroupDic = AddHomeGroupDic;
            viewModel.GroupID = groupid;
        }
        private void RecognizeDealWin_Loaded(object sender, RoutedEventArgs e)
        {
            SetExecutionsHeight();
            ReStoreWindow();
            this.SizeChanged += RecognizeDealWin_SizeChanged;
        }
        /// <summary>
        /// 保存窗口大小和位置
        /// </summary>
        private void SaveWindowPos()
        {
            //IniFileHelper.WriteValue(WinIniKey, WinHeight, this.ActualHeight + "");
            //IniFileHelper.WriteValue(WinIniKey, WinWidth, this.ActualWidth + "");
            //IniFileHelper.WriteValue(WinIniKey, PositionX, this.Left + "");
            //IniFileHelper.WriteValue(WinIniKey, PositionY, this.Top + "");
        }
        private void RecognizeDealWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowPos();
        }

        /// <summary>
        /// 恢复窗口
        /// </summary>
        private void ReStoreWindow()
        {
            //string height = IniFileHelper.ReadValue(WinIniKey, WinHeight);
            //string width = IniFileHelper.ReadValue(WinIniKey, WinWidth);

            //double d = 0;
            //if (double.TryParse(height, out d))
            //    this.Height = d;
            //if (double.TryParse(width, out d))
            //    this.Width = d;

            //string positionX = IniFileHelper.ReadValue(WinIniKey, PositionX);
            //string positionY = IniFileHelper.ReadValue(WinIniKey, PositionY);
            //double dY = 0;
            //if (double.TryParse(positionX, out d) && double.TryParse(positionY, out dY))
            //{
            //    this.Top = dY;
            //    this.Left = d;
            //}
        }
        /// <summary>
        /// 恢复分割线
        /// </summary>
        private async void SetExecutionsHeight()
        {
            //await Task.Run(() =>
            //{
            //    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        string height = IniFileHelper.ReadValue(WinIniKey, "SecurityGroupManagerLeftWidth");
            //        double d = 0;
            //        if (double.TryParse(height, out d))
            //        {
            //            if (d > 0)
            //            {
            //                LeftRow.Width = new GridLength(d);
            //            }
            //        }
            //    }));
            //});
        }
        
        public RecognizeBondView(object obj)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            this.contentTxt.Dispatcher.BeginInvoke((Action)(() => {
                string tmp = Clipboard.GetText().Trim();
                this.contentTxt.Text = tmp;
            }));

            this.Loaded += RecognizeDealWin_Loaded;
        }

        private void TabControl_DragEnter(object sender, DragEventArgs e)
        {
        }

        private void TabControl_Drop(object sender, DragEventArgs e)
        {

        }

        private void img_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (this.textTI.IsSelected)
            {
                this.contentTxt.Text = string.Empty;
            }
        }

        private void btnRecognize_Click(object sender, RoutedEventArgs e)
        {
            if (textTI.IsSelected)
            {
                viewModel.RecognizeIntentionQuote(this.contentTxt.Text);
            }
        }

        private void gridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            double hei = LeftRow.ActualWidth;
            try
            {
                SaveLastExecutionsHeight(hei);
            }
            catch { }
        }
        /// <summary>
        /// 保存分割线
        /// </summary>
        /// <param name="v"></param>
        public void SaveLastExecutionsHeight(double v)
        {
            //if (this.DataContext != null && this.DataContext is RecognizeBondViewModel)
            //{
            //    RecognizeBondViewModel viewModel = (RecognizeBondViewModel)this.DataContext;
            //    IniFileHelper.WriteValue(WinIniKey, "SecurityGroupManagerLeftWidth", v.ToString());
            //}
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is IntentionQuoteItem)
            {
                (e.Row.Item as IntentionQuoteItem).No = e.Row.GetIndex() + 1;
            }
        }

        private void DataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            refreshDataGridRowNumbers(sender);
        }
        // 刷新序号
        private static void refreshDataGridRowNumbers(object sender)
        {
            DataGrid grid = sender as DataGrid;
            if (grid == null)
                return;

            foreach (var item in grid.Items)
            {
                var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null && row.Item is IntentionQuoteItem)
                    (row.Item as IntentionQuoteItem).No = row.GetIndex() + 1;
            }
        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.SelectedItem_Recognize = (IntentionQuoteItem)((Button)sender).DataContext;
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.ClearList();
            }
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.UnregisterEvent();
                this.Loaded -= RecognizeDealWin_Loaded;
                SaveWindowPos();
            }
        }

        private void IsHomeCb_Checked(object sender, RoutedEventArgs e)
        {
            viewModel.BindBonds(string.Empty);
        }

        private void IsHomeCb_Unchecked(object sender, RoutedEventArgs e)
        {
            viewModel.ClearList();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.ClearList();
            viewModel.BindBonds(string.Empty);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedAddHomeGroup == null) 
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBoxGUI msgBox = new MessageBoxGUI();
                    msgBox.setWarnMsg("未选择需要导入的组", true);
                    msgBox.ShowDialog();
                });
                return;
            }
            this.UpdateList = viewModel.getUpdateList();
            this.isHomeGroup = viewModel.isHomeGroup;
            this.SelectedAddHomeGroup = viewModel.SelectedAddHomeGroup;
            this.Result = true;
            this.Close();
            e.Handled = true;
        }

        private void addHomeGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClickChkBox();
        }
    }
}
