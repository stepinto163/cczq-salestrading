using STC.Common.ColumnSetting;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace STC.Common.View.Component
{
    public class ExDataGrid : DataGrid, ISettingColumnsBase
    {

        //protected const string PART_scbCustom = "scbCustom";
        //protected const string PART_DG_ScrollViewer = "DG_ScrollViewer";
        protected const string PART_MainGrid = "MainGrid";
        protected const string PART_DG_ScrollViewer_outer = "DG_ScrollViewer_outer";
        protected const string PART_DgBorder = "DgBorder";

        private Grid _MainGrid = null;
        private ScrollViewer _OuterScrollViewer = null;
        private Border _DgBorder = null;
        private bool _EnableColumnVirtualization = false;

        private string _name = string.Empty;

        private readonly ContextMenu _contextMenu = new ContextMenu();

        public ContextMenu GetHeaderContextMenu()
        {
            return _contextMenu;
        }

        private string _defaultColumnsInfoStr = string.Empty;

        private bool HasDatas
        {
            get { return (bool)GetValue(HasDatasProperty); }
            set { SetValue(HasDatasProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasDatas.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasDatasProperty =
            DependencyProperty.Register("HasDatas", typeof(bool), typeof(ExDataGrid), new PropertyMetadata(false, (o, e) => {
                ExDataGrid currentSource = o as ExDataGrid;
                //Grid mainGrid = currentSource._MainGrid;
                //ScrollViewer sv = currentSource._OuterScrollViewer;
                //Border dgBorder = currentSource._DgBorder;
                //if (mainGrid != null && sv != null && dgBorder != null)
                //{
                //    if (currentSource.HasItems)
                //    {
                //        currentSource.EnableColumnVirtualization = currentSource._EnableColumnVirtualization;
                //        sv.Content = null;
                //        mainGrid.Children.Add(dgBorder);
                //    }
                //    else
                //    {
                //        currentSource.EnableColumnVirtualization = false;
                //        mainGrid.Children.Remove(dgBorder);
                //        sv.Content = dgBorder;
                //    }
                //}
            }));


        // 是否启用表头设置
        public bool IsColumnSetting
        {
            get { return (bool)GetValue(IsColumnSettingProperty); }
            set { SetValue(IsColumnSettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsColumnSetting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsColumnSettingProperty =
            DependencyProperty.Register("IsColumnSetting", typeof(bool), typeof(ExDataGrid), new PropertyMetadata(true, (o, e) =>
            {
                var exDataGrid = o as ExDataGrid;
                SettingSyncHandler.Instance.RemoveSyncDg(exDataGrid);
                if (e.NewValue != null)
                {
                    if ((bool)e.NewValue == true)
                    {
                        SettingSyncHandler.Instance.AddSyncDg(exDataGrid);
                    }
                }
            }));


        // 设置固定显示列
        public string FixedDisplayColumns
        {
            get { return (string)GetValue(FixedDisplayColumnsProperty); }
            set { SetValue(FixedDisplayColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FixedDisplayColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FixedDisplayColumnsProperty =
            DependencyProperty.Register("FixedDisplayColumns", typeof(string), typeof(ExDataGrid), new PropertyMetadata("代码,简称", (o, e) => {
                if (e.NewValue != null)
                {
                    var exDataGrid = o as ExDataGrid;
                    exDataGrid.FixedDisplayColumns = e.NewValue?.ToString();

                    exDataGrid.fixedDisplyColumnList.Clear();
                    if (!string.IsNullOrEmpty(exDataGrid.FixedDisplayColumns))
                    {
                        string[] columnNames = exDataGrid.FixedDisplayColumns.Split(CommonConstants.COMMA_CHAR);
                        foreach(string columnName in columnNames)
                        {
                            string cn = columnName.Trim();
                            if (!exDataGrid.fixedDisplyColumnList.Contains(cn))
                            {
                                exDataGrid.fixedDisplyColumnList.Add(cn);
                            }
                        }
                    }
                }
            }));


        // 设置默认隐藏列
        public string DefaultHideColumns
        {
            get { return (string)GetValue(DefaultHideColumnsProperty); }
            set { SetValue(DefaultHideColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultHideColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultHideColumnsProperty =
            DependencyProperty.Register("DefaultHideColumns", typeof(string), typeof(ExDataGrid), new PropertyMetadata(string.Empty, (o, e) => {
                if (e.NewValue != null)
                {
                    var exDataGrid = o as ExDataGrid;
                    exDataGrid.DefaultHideColumns = e.NewValue?.ToString();

                    exDataGrid.defaultHideColumnList.Clear();
                    if (!string.IsNullOrEmpty(exDataGrid.DefaultHideColumns))
                    {
                        string[] columnNames = exDataGrid.DefaultHideColumns.Split(CommonConstants.COMMA_CHAR);
                        foreach (string columnName in columnNames)
                        {
                            string cn = columnName.Trim();
                            if (!exDataGrid.defaultHideColumnList.Contains(cn))
                            {
                                exDataGrid.defaultHideColumnList.Add(cn);
                            }
                        }
                    }
                }
            }));

        //private ScrollBar scbCustom = null;

        //public ScrollBar PART_SCBCUSTOM
        //{
        //    get
        //    {
        //        return scbCustom;
        //    }
        //    set
        //    {
        //        // 先反注册事件
        //        if (scbCustom != null)
        //        {
        //            scbCustom.Scroll -= ScbCustom_Scroll;
        //        }

        //        scbCustom = value;

        //        if (scbCustom != null)
        //        {
        //            scbCustom.Scroll += ScbCustom_Scroll;
        //        }
        //    }
        //}

        //private void ScbCustom_Scroll(object sender, ScrollEventArgs e)
        //{
        //    if (sender is ScrollBar)
        //    {
        //        ScrollBar scb = sender as ScrollBar;
        //        if (scb.Tag != null && scb.Tag is ScrollViewer)
        //        {
        //            ScrollViewer headerSV = scb.Tag as ScrollViewer;
        //            headerSV.ScrollToHorizontalOffset(e.NewValue);
        //        }
        //    }
        //}

        public ExDataGrid()
        {
            //DefaultStyleKey = typeof(ExDataGrid);
            AddHandler(Button.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.ExDataGrid_MouseLeftButtonUp), true);

            this.Loaded += ExDataGrid_Loaded;
            this.Unloaded += ExDataGrid_Unloaded;
        }

        private void ExDataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            SettingSyncHandler.Instance.RemoveSyncDg(this);
        }

        private void ExDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsColumnSetting)
            {
                SettingSyncHandler.Instance.AddSyncDg(this);
                string columnsInfoStr = IniFileHelper.ReadValue($"{this.GetName()}-{Utils.LocalEnv.UserInfo.UserName}", ColumnSettingView.IniKey, 2048);
                if (string.IsNullOrEmpty(columnsInfoStr))
                {
                    columnsInfoStr = this.GetDefaultColumnsInfoStr();
                }
                InitColumnsInfo(columnsInfoStr);
            }
        }

        private void ExDataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsColumnSetting)
            {
                if (isColumnHeaderDrag)
                {
                    isColumnHeaderDrag = false;
                    ObservableCollection<ColumnsInfo> columnsInfos = GetColumnsInfos();
                    SaveColumnInfo(columnsInfos);
                    SettingSyncHandler.Instance.RefreshColumns(this.GetName(), this);
                }
            }
        }

        PropertyDescriptor pd = DependencyPropertyDescriptor
                             .FromProperty(DataGridColumn.ActualWidthProperty,
                                           typeof(DataGridColumn));

        public override void OnApplyTemplate()
        {
            
            base.OnApplyTemplate();

            //_EnableColumnVirtualization = this.EnableColumnVirtualization; // 记录EnableColumnVirtualization的初始值

            //this.EnableColumnVirtualization = false;
            //_MainGrid = GetTemplateChild(PART_MainGrid) as Grid;
            //_OuterScrollViewer = GetTemplateChild(PART_DG_ScrollViewer_outer) as ScrollViewer;
            //_DgBorder = GetTemplateChild(PART_DgBorder) as Border; //STC.Common.Utils.CommonUtils.GetVisualChild<Border>(this, dp => dp.Name == "DgBorder");
            //if (_MainGrid != null && _OuterScrollViewer != null && _DgBorder != null)
            //{
            //    if (this.HasItems)
            //    {
            //        this.EnableColumnVirtualization = _EnableColumnVirtualization;
            //        _OuterScrollViewer.Content = null;
            //       _MainGrid.Children.Add(_DgBorder);
            //     }
            //}

            _name = this.Name;

            // 生成默认字段信息
            if (string.IsNullOrEmpty(_defaultColumnsInfoStr))
            {
                StringBuilder columnsInfoSB = new StringBuilder();
                foreach (DataGridColumn dgc in this.Columns)
                {
                    pd.RemoveValueChanged(dgc, new EventHandler(ColumnWidthPropertyChanged));
                    pd.AddValueChanged(dgc, new EventHandler(ColumnWidthPropertyChanged));

                    if (columnsInfoSB.Length > 0)
                        columnsInfoSB.Append(";");

                    string columnName = GetColumnName(dgc);
                    if (dgc.Visibility != Visibility.Collapsed)
                    {
                        if (defaultHideColumnList.Contains(columnName) || dgc.Visibility == Visibility.Hidden)
                        {
                            columnsInfoSB.Append($"({columnName}):{dgc.Width}");
                        }
                        else
                        {
                            columnsInfoSB.Append($"{columnName}:{dgc.Width}");
                        }
                    }
                }
                _defaultColumnsInfoStr = columnsInfoSB.ToString();
            }

            //ScrollViewer dg_scrollViewer = GetTemplateChild(PART_DG_ScrollViewer) as ScrollViewer;
            //if (dg_scrollViewer != null)
            //{
            //    dg_scrollViewer.Loaded += Dg_scrollViewer_Loaded;
            //}

            _contextMenu.Style = (Style)this.FindResource("DefaultContextMenu");
            MenuItem settingMi = new MenuItem();
            settingMi.Name = "settingColumnMi";
            settingMi.Header = "表头设置";
            settingMi.Style = (Style)this.FindResource("DefaultMenuItem");
            settingMi.Click += ColumnMi_Click;
            _contextMenu.Items.Insert(0, settingMi);
        }

        private void ColumnWidthPropertyChanged(object sender, EventArgs e)
        {
            if (IsColumnSetting)
            {
                isColumnHeaderDrag = true;
            }
        }

        //protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        //{
        //    base.OnItemsChanged(e);

        //    this.HasDatas = this.HasItems;
        //}

        //private void Dg_scrollViewer_Loaded(object sender, RoutedEventArgs e)
        //{
        //    ScrollViewer dg_scrollViewer = sender as ScrollViewer;

        //    if (dg_scrollViewer != null)
        //    {
        //        PART_SCBCUSTOM = STC.Common.Utils.CommonUtils.GetVisualChild<ScrollBar>(sender as ScrollViewer, dp => dp.Name == PART_scbCustom);

        //        dg_scrollViewer.Loaded -= Dg_scrollViewer_Loaded;
        //    }
        //}

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            bool flag = isColumnHeader(this, e.GetPosition(this));
            if (flag && IsColumnSetting)
            {
                _contextMenu.IsOpen = true;
                e.Handled = true;
            }
            else
            {
                base.OnMouseRightButtonDown(e);
            }
        }

        private bool isColumnHeader(ItemsControl itemsControl, Point point)
        {
            UIElement element = itemsControl.InputHitTest(point) as UIElement;
            while (element != null)
            {
                if (element == itemsControl)
                    return false;
                if (element is DataGridColumnHeader)
                    return true;
                element = (UIElement)System.Windows.Media.VisualTreeHelper.GetParent(element);
            }
            return false;
        }


        private void ColumnMi_Click(object sender, RoutedEventArgs e)
        {
            if (WindowManager.Instance.IsExist("ColumnSetting") == false)
            {
                ColumnSettingView csv = new ColumnSettingView(this);
                //Window parentWindow = CommonUtils.GetParentObject<Window>(this);
                WindowManager.Instance.OpenWindow("ColumnSetting", csv);
            }
        }


        #region 加载字段信息
        List<string> fixedDisplyColumnList = new List<string>() { "代码", "简称", "债券代码", "债券简称", };
        List<string> defaultHideColumnList = new List<string>();

        public bool ContainFixedColumns(string fixedColumn)
        {
            return fixedDisplyColumnList.Contains(fixedColumn);
        }

        public void InitColumnsInfo(string columnsInfoStr = null)
        {
            if (!string.IsNullOrEmpty(columnsInfoStr))
            {
                foreach (DataGridColumn column in this.Columns)
                {
                    string columnHeader = GetColumnName(column);

                    if (!fixedDisplyColumnList.Contains(columnHeader))
                    {
                        column.Visibility = Visibility.Hidden;
                    }
                }

                string[] columnPairs = columnsInfoStr.Split(CommonConstants.SEMICOLON_CHAR);
                int index = 0;
                foreach (string columnPair in columnPairs)
                {
                    string[] propertyValues = columnPair.Split(CommonConstants.COLON_CHAR);
                    if (propertyValues.Length == 2)
                    {
                        string columnName = propertyValues[0];
                        if (propertyValues[0].StartsWith("(") && propertyValues[0].EndsWith(")"))
                        {
                            columnName = columnName.TrimStart('(').TrimEnd(')');
                        }

                        //columnName = GetHeaderName(columnName);

                        try
                        {
                            DataGridColumn col = this.Columns.First(o => { string headerName = GetColumnName(o); return headerName == columnName; });
                            if (!(propertyValues[0].StartsWith("(") && propertyValues[0].EndsWith(")")))
                            {
                                col.Visibility = Visibility.Visible;
                            }

                            if ("auto".Equals(propertyValues[1], StringComparison.OrdinalIgnoreCase))
                            {
                                col.Width = DataGridLength.Auto;
                            }
                            else if ("SizeToCells".Equals(propertyValues[1], StringComparison.OrdinalIgnoreCase))
                            {
                                col.Width = DataGridLength.SizeToCells;
                            }
                            else if ("SizeToHeader".Equals(propertyValues[1], StringComparison.OrdinalIgnoreCase))
                            {
                                col.Width = DataGridLength.SizeToCells;
                            }
                            else if (propertyValues[1].Contains("*"))
                            {
                                string valuePart = propertyValues[1].Replace("*", "");
                                double value = 1;
                                double.TryParse(valuePart, out value);
                                value = value > 0 ? value : 1;
                                col.Width = new DataGridLength(value, DataGridLengthUnitType.Star);
                            }
                            else
                            {
                                double width = col.ActualWidth;
                                double.TryParse(propertyValues[1], out width);
                                col.Width = width;
                            }

                            col.DisplayIndex = index;
                            index++;
                        }
                        catch (System.InvalidOperationException e)
                        {
                            // ignore
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }
            else
            {
                //如果是首次进入持仓管理界面，没有保存的表头设置，默认隐藏 "昨持仓量(多)", "昨持仓量(空)", "今持仓量(多)", "今持仓量(空)"
                foreach (DataGridColumn column in this.Columns)
                {
                    string columnName = GetColumnName(column);
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        if (defaultHideColumnList.Contains(columnName))
                        {
                            column.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
        }
        #endregion


        bool isColumnHeaderDrag = false;
        protected override void OnColumnHeaderDragCompleted(DragCompletedEventArgs e)
        {
            base.OnColumnHeaderDragCompleted(e);
            if (IsColumnSetting)
            {
                isColumnHeaderDrag = true;
            }
        }

        public ObservableCollection<ColumnsInfo> GetColumnsInfos()
        {
            if (this.Columns.Count == 0)
                return null;

            ObservableCollection<ColumnsInfo> columnsInfoList = new ObservableCollection<ColumnsInfo>();
            foreach (DataGridColumn column in this.Columns)
            {
                string columnName = GetColumnName(column);
                
                ColumnsInfo ci = new ColumnsInfo();
                ci.IsVisible = column.Visibility == Visibility.Visible ? true : false;

                if (this.ContainFixedColumns(columnName))
                {
                    ci.IsEnable = false;
                }
                ci.ColumnName = columnName;
                ci.ColumnWidth = column.ActualWidth;
                ci.DisplayIndex = column.DisplayIndex;
                columnsInfoList.Add(ci);
            }
            return columnsInfoList;
        }

        public void SaveColumnInfo(ObservableCollection<ColumnsInfo> _MyColumnsInfo)
        {
            if (_MyColumnsInfo == null && _MyColumnsInfo.Count == 0)
                return;

            string key = $"{this.GetName()}-{LocalEnv.UserInfo.UserName}";
            //await Task.Run(() => {
                #region 保存表头
                StringBuilder columnsInfoSB = new StringBuilder();
                IOrderedEnumerable<ColumnsInfo> sortedColumnInfos = _MyColumnsInfo.OrderBy(x => x.DisplayIndex);
                foreach (ColumnsInfo cloumnsInfo in sortedColumnInfos)
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
                IniFileHelper.WriteValue(key, ColumnSettingView.IniKey, columnsInfoSB.ToString());
                #endregion
           // });
        }

        public void DeleteColumnInfo()
        {
            string key = $"{this.GetName()}-{LocalEnv.UserInfo.UserName}";
            IniFileHelper.WriteValue(key, null, null);
        }

        public string GetName()
        {
            return _name;
        }

        private string GetColumnName(DataGridColumn dgColumn)
        {
            string columnName = string.Empty;
            if (dgColumn != null)
            {
                if (dgColumn.Header != null)
                {
                    columnName = dgColumn.Header.ToString();
                }
                else if (!string.IsNullOrEmpty(DataGridUtil.GetName(dgColumn)))
                {
                    columnName = DataGridUtil.GetName(dgColumn);
                }
            }

            if (string.IsNullOrEmpty(columnName))
            {
                switch (this.GetName())
                {
                    case "marketExecution":
                        columnName = "star";
                        break;
                    default:
                        columnName = "选择";
                        break;
                }
            }

            return columnName;
        }

        private string GetHeaderName(string columnName)
        {
            if (columnName == "选择" || columnName == "star")
            {
                if (!this.Columns.Any(o => { string headerName = o.Header == null ? (string.IsNullOrEmpty(DataGridUtil.GetName(o)) ? string.Empty : DataGridUtil.GetName(o)) : o.Header.ToString(); return headerName == columnName; }))
                {
                    return string.Empty;
                }
            }
            return columnName;
        }

        public string GetDefaultColumnsInfoStr()
        {
            return _defaultColumnsInfoStr;
        }




    }
}
