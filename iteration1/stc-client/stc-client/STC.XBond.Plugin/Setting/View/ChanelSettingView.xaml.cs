using STC.Common.Handler.SystemParam;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.XBond.Plugin.Setting.Utils;
using STC.XBond.Plugin.Setting.ViewModel;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace STC.XBond.Plugin.Setting.View
{
    /// <summary>
    /// ChanelSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class ChanelSettingView : UserControl
    {
        private VenueViewModel viewModel = new VenueViewModel();
        public bool isInit = true;
        public UserSetting userSetting;
        public ChanelSettingView()
        {
            InitializeComponent();
            this.dgRecordsChannel.MouseUp += DgRecordsChannel_MouseUp;
            this.dgRecordsChannel.PreviewMouseLeftButtonDown += DgRecordsChannel_PreviewMouseLeftButtonDown;
            this.dgRecordsChannel.Drop += DgRecordsChannel_Drop;
            this.dgRecordsChannel.DragOver += DgRecordsChannel_DragOver;
            this.dgRecordsChannel.DragLeave += DgRecordsChannel_DragLeave;
            this.dgRecordsChannel.DragEnter += DgRecordsChannel_DragEnter;
            dgRecordsChannel.MouseEnter += DgRecordsChannel_MouseEnter;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            diclevel.Clear();
            diclevel.Add("禁止", 0);
            diclevel.Add("1档", 1);
            diclevel.Add("2档", 2);
            SystemParamHandler.Instance.Init();
            Thread.Sleep(200);

            userSetting = SystemParamHandler.Instance.GetUserSetting();
            if (userSetting != null && userSetting.systemVenue != null && userSetting.systemVenue.Count > 0)
            {
                this.viewModel.Init(userSetting);
            }
            else
                LogHelper.Warn("userSetting is null or userSetting.systemVenue is null!");
            dgRecordsChannel.DataContext = this.viewModel;

        }
      
        private void DgRecordsChannel_MouseEnter(object sender, MouseEventArgs e)
        {
            isInit = false;
        }
        public Dictionary<string, int> diclevel = new Dictionary<string, int>();

        #region 更新
        private void Update()
        {
            List<Venue> templist = new List<Venue>();
            foreach (Venues v in viewModel.VenueList)
            {
                Venue f = new Venue();
                f.sn = v.sn;
                f.venue = v.Venue;
                f.name = v.Name;
                f.type = v.Type;
                string level = v.Levelformat == null ? "" : v.Levelformat.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");

                if (diclevel.ContainsKey(level))
                    f.level = diclevel[level];
                else
                    f.level = v.Level;
                f.venuetypegw = v.Venuetypegw;
                f.weight = v.Weight;

                Venue f1 = new Venue();
                f1.sn = v.sn;
                f1.venue = v.Venuec;
                f1.name = v.Namec;
                f1.type = v.Typec;

                string level2 = v.Levelformatc == null ? "" : v.Levelformatc.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
                if (diclevel.ContainsKey(level2))
                    f1.level = diclevel[level2];
                else
                    f1.level = v.Levelc;
                f1.venuetypegw = v.Venuetypegwc;
                f1.weight = v.Weightc;
                templist.Add(f);
                templist.Add(f1);
            }
            userSetting.systemVenue.Clear();
            userSetting.systemVenue.AddRange(templist);
            ThreadPool.QueueUserWorkItem(o =>
            {
                SystemParamHandler.Instance.UpdatesystemParas(userSetting);
            });
        }
        #endregion

        private void DgRecordsChannel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //System.Drawing.Point pt = new Point(e., e.Y);
            //DataGrid.HitTestInfo hti = dgRecordsChannel.HitTest(pt);
            //if (hti.Type == DataGrid.HitTestType.Cell)

            //{
            //    dataGrid1.CurrentCell = new DataGridCell(hti.Row, hti.Column);
            //    dataGrid1.Select(hti.Row);

            //}
        }

        private void DgRecordsChannel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prevRowIndex = UITools.GetDataGridItemCurrentRowIndex(e.GetPosition, dgRecordsChannel);
            LogHelper.Debug("X " + e.GetPosition(this.dgRecordsChannel).X.ToString());
            //如果选中区域在第一列，即点击选择框，则不进行
            if (e.GetPosition(this.dgRecordsChannel).X < 30) return;

            if (prevRowIndex < 0) return;
            dgRecordsChannel.SelectedIndex = prevRowIndex;

            DataGridLength col0 = dgRecordsChannel.Columns[0].Width;
            DataGridLength col1 = dgRecordsChannel.Columns[1].Width;

            Venues selectedEmp = dgRecordsChannel.Items[prevRowIndex] as Venues;

            if (e.GetPosition(this.dgRecordsChannel).X > col0.DisplayValue + 3 && e.GetPosition(this.dgRecordsChannel).X <= col0.DisplayValue + 16 + 3)//(index == 1)
            {
                prevColumnIndex = 1;
                txpop.Text = string.Format("利率债({0})", selectedEmp.Name);
            }
            else if (e.GetPosition(this.dgRecordsChannel).X > col0.DisplayValue + col1.DisplayValue + 3 && e.GetPosition(this.dgRecordsChannel).X < col0.DisplayValue + col1.DisplayValue + 16 + 3) //(index == 2)
            {
                prevColumnIndex = 2;
                txpop.Text = string.Format("信用债({0})", selectedEmp.Namec);
            }
            else
                return;

            if (selectedEmp == null) return;

            DragDropEffects dragdropeffects = DragDropEffects.Move;
            this.viewModel.SelectedMusic = selectedEmp;
            if (DragDrop.DoDragDrop(dgRecordsChannel, selectedEmp, dragdropeffects)
                                != DragDropEffects.None)
            {
                dgRecordsChannel.SelectedItem = selectedEmp;
            }
        }

        private void DgRecordsChannel_DragEnter(object sender, DragEventArgs e)
        {
            //if (e.Data.GetFormats()[0] == typeof(MusicInfo).ToString())
            //{

            //}
            //else
            //{
            //    MusicInfo m = new MusicInfo("添加到文件列表");
            //    m.IsNew = true;
            //    this.vm.SelectedMusic = m;
            //}
        }

        private void DgRecordsChannel_DragLeave(object sender, DragEventArgs e)
        {
            this.popup1.IsOpen = false;
        }

        private void DgRecordsChannel_DragOver(object sender, DragEventArgs e)
        {
            int index = UITools.GetDataGridItemCurrentRowIndex(e.GetPosition, dgRecordsChannel);

            for (int i = 0; i < dgRecordsChannel.Items.Count; i++)
            {
                DataGridRow r = dgRecordsChannel.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                if (i != index)
                {
                    r.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    r.BorderThickness = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    r.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 164, 230));
                    Thickness th = new Thickness(0, 0, 0, 0);

                    if (index > prevRowIndex) th = new Thickness(0, 0, 0, 1);
                    else if (index < prevRowIndex) th = new Thickness(0, 1, 0, 0);
                    r.BorderThickness = th;
                }
            }

            if (!popup1.IsOpen)
            {
                popup1.IsOpen = true;
            }
            Size popupSize = new Size(popup1.ActualWidth + 110, popup1.ActualHeight + 10);
            Point p = e.GetPosition(this);
            p.X += 10;
            p.Y += 10;
            popup1.PlacementRectangle = new Rect(p, popupSize);
        }

        int prevRowIndex = -1;
        int prevColumnIndex = -1;
        private void DgRecordsChannel_Drop(object sender, DragEventArgs e)
        {
            this.popup1.IsOpen = false;
            int index = UITools.GetDataGridItemCurrentRowIndex(e.GetPosition, dgRecordsChannel);

            if (index < 0 || index == prevRowIndex) return;
            (dgRecordsChannel.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow).BorderThickness = new Thickness(0, 0, 0, 0);


            if (index >= dgRecordsChannel.Items.Count - 1)
            {
                index = dgRecordsChannel.Items.Count - 1;
            }

            //if (viewModel.IsNew) //表示拖动文件至表格
            //{
            //    string[] obj = e.Data.GetData(DataFormats.FileDrop) as string[];
            //    foreach (string str in obj)
            //    {
            //        this.viewModel.CheckFile(str, ++index);
            //    }
            //}
            //else
            //{
            int index2 = dgRecordsChannel.CurrentCell.Column.DisplayIndex;
            // prevRowIndex = index2;
            if (prevColumnIndex == 1)
            {
                Venues temp1 = new Venues();
                temp1.Name = viewModel.VenueList[prevRowIndex].Name;
                temp1.Venue = viewModel.VenueList[prevRowIndex].Venue;
                temp1.Level = viewModel.VenueList[prevRowIndex].Level;
                temp1.Type = viewModel.VenueList[prevRowIndex].Type;
                temp1.Venuetypegw = viewModel.VenueList[prevRowIndex].Venuetypegw;
                temp1.Weight = viewModel.VenueList[prevRowIndex].Weight;
                temp1.Levelformat = viewModel.VenueList[prevRowIndex].Levelformat;

                Venues temp2 = new Venues();
                temp2.Name = viewModel.VenueList[index].Name;
                temp2.Venue = viewModel.VenueList[index].Venue;
                temp2.Level = viewModel.VenueList[index].Level;
                temp2.Type = viewModel.VenueList[index].Type;
                temp2.Venuetypegw = viewModel.VenueList[index].Venuetypegw;
                temp2.Weight = viewModel.VenueList[index].Weight;
                temp2.Levelformat = viewModel.VenueList[index].Levelformat;

                Venues v = viewModel.VenueList[prevRowIndex];
                v.Name = temp2.Name;
                v.Venue = temp2.Venue;
                v.Level = temp2.Level;
                v.Type = temp2.Type;
                v.Venuetypegw = temp2.Venuetypegw;
                v.Weight = temp2.Weight;
                v.Levelformat = temp2.Levelformat;

                Venues v2 = viewModel.VenueList[index];
                v2.Name = temp1.Name;
                v2.Venue = temp1.Venue;
                v2.Level = temp1.Level;
                v2.Type = temp1.Type;
                v2.Venuetypegw = temp1.Venuetypegw;
                v2.Weight = temp1.Weight;
                v2.Levelformat = temp1.Levelformat;

            }
            else if (prevColumnIndex == 2)
            {
                Venues temp1 = new Venues();
                temp1.Namec = viewModel.VenueList[prevRowIndex].Namec;
                temp1.Venuec = viewModel.VenueList[prevRowIndex].Venuec;
                temp1.Levelc = viewModel.VenueList[prevRowIndex].Levelc;
                temp1.Typec = viewModel.VenueList[prevRowIndex].Typec;
                temp1.Venuetypegwc = viewModel.VenueList[prevRowIndex].Venuetypegwc;
                temp1.Weightc = viewModel.VenueList[prevRowIndex].Weightc;
                temp1.Levelformatc = viewModel.VenueList[prevRowIndex].Levelformatc;

                Venues temp2 = new Venues();
                temp2.Namec = viewModel.VenueList[index].Namec;
                temp2.Venuec = viewModel.VenueList[index].Venuec;
                temp2.Levelc = viewModel.VenueList[index].Levelc;
                temp2.Typec = viewModel.VenueList[index].Typec;
                temp2.Venuetypegwc = viewModel.VenueList[index].Venuetypegwc;
                temp2.Weightc = viewModel.VenueList[index].Weightc;
                temp2.Levelformatc = viewModel.VenueList[index].Levelformatc;

                Venues v = viewModel.VenueList[prevRowIndex];
                v.Namec = temp2.Namec;
                v.Venuec = temp2.Venuec;
                v.Levelc = temp2.Levelc;
                v.Typec = temp2.Typec;
                v.Venuetypegwc = temp2.Venuetypegwc;
                v.Weightc = temp2.Weightc;
                v.Levelformatc = temp2.Levelformatc;

                Venues v2 = viewModel.VenueList[index];
                v2.Namec = temp1.Namec;
                v2.Venuec = temp1.Venuec;
                v2.Levelc = temp1.Levelc;
                v2.Typec = temp1.Typec;
                v2.Venuetypegwc = temp1.Venuetypegwc;
                v2.Weightc = temp1.Weightc;
                v2.Levelformatc = temp1.Levelformatc;

            }
            //viewModel.VenueList.RemoveAt(prevRowIndex);
            //viewModel.VenueList.Insert(index, viewModel.SelectedMusic);
            prevRowIndex = -1;
            //}
            Update();
        }

        private void Setting_tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl s = sender as TabControl;
            if (s != null)
            {
                foreach (TabItem t in s.Items)
                {
                    t.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                }
                TabItem d = s.SelectedItem as TabItem;
                if (d.IsSelected)
                    d.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));
            }
        }

        private void dgRecordsChannel_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (isInit)
                return;
            DataGrid grid = sender as DataGrid;
            if (grid != null && grid.Items.Count == 0)
                return;
        }

        private void tbWeight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (isInit)
                return;
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
            if (e.Handled)
                return;
        }
        private void tbWeightc_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (isInit)
                return;
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);

            if (e.Handled)
                return;
        }

        private void tbWeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox t = sender as TextBox;
                string d = t.Text;
                Update();
            }
        }
        private void tbWeightc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox t = sender as TextBox;
                string d = t.Text;
                Update();
            }
        }

        private void comLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit)
                return;
            try
            {
                ComboBox c = sender as ComboBox;
                switch (c.SelectedIndex)
                {
                    case 0:
                        c.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));

                        break;
                    case 1:
                        c.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));

                        break;
                    case 2:
                        c.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00"));

                        break;
                }
                Update();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void comLevelc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }
            ComboBox c = sender as ComboBox;
            switch (c.SelectedIndex)
            {
                case 0:
                    c.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));

                    break;
                case 1:
                    c.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E63034"));

                    break;
                case 2:
                    c.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ED6D00"));

                    break;
            }
            Update();
        }










        //private void dgRecordsChannel_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    e.Row.Header = e.Row.GetIndex() + 1;

        //}

    }
}
