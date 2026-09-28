using com.gateway.log;
using BDB.Common.View;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.ViewModel.Trade;
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
using STC.Common.View;

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// CardView.xaml 的交互逻辑
    /// </summary>
    public partial class CardView : UserControl
    {
        private CardViewModel _cardViewModel = null;

        public CardView()
        {
            InitializeComponent();
            
            this.Unloaded += CardView_Unloaded;


            #region 拖拽
            
            this.FavoriteBondslistBox.DragOver += FavoriteBondslistBox_DragOver;
            this.FavoriteBondslistBox.DragLeave += FavoriteBondslistBox_DragLeave;
            this.FavoriteBondslistBox.Drop += FavoriteBondslistBox_Drop;
            this.FavoriteBondslistBox.MouseMove += FavoriteBondslistBox_MouseMove;

            #endregion
        }

        #region 拖拽事件

        int prevRowIndex = -1;

        private void FavoriteBondslistBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                prevRowIndex = UITools.GetListViewItemCurrentRowIndex(e.GetPosition, FavoriteBondslistBox);
                //如果选中区域在第一列，即点击选择框，则不进行
                if (e.GetPosition(this.FavoriteBondslistBox).X < 30) return;

                if (prevRowIndex < 0) return;
                //dgData.SelectedIndex = prevRowIndex;

                CardItem selectedEmp = FavoriteBondslistBox.Items[prevRowIndex] as CardItem;

                if (selectedEmp == null || string.IsNullOrEmpty(selectedEmp.Id)) return;

                DragDropEffects dragdropeffects = DragDropEffects.Move;
                this._cardViewModel.SelectedListViewItem = selectedEmp;
                if (DragDrop.DoDragDrop(FavoriteBondslistBox, selectedEmp, dragdropeffects) != DragDropEffects.None)
                {
                    FavoriteBondslistBox.SelectedItem = selectedEmp;
                }
            }
        }
        

        private void FavoriteBondslistBox_DragOver(object sender, DragEventArgs e)
        {
            int index = UITools.GetListViewItemCurrentRowIndex(e.GetPosition, FavoriteBondslistBox);

            for (int i = 0; i < FavoriteBondslistBox.Items.Count; i++)
            {
                ListViewItem r = FavoriteBondslistBox.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (r != null)
                {
                    if (i != index)
                    {
                        //r.BorderBrush = new SolidColorBrush(Colors.Transparent);
                        //r.BorderThickness = new Thickness(0, 0, 0, 0);

                        r.BorderBrush = new SolidColorBrush(Color.FromRgb(26, 46, 44));

                        r.BorderThickness = new Thickness(2);

                    }
                    else
                    {
                        r.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 164, 230));
                        Thickness th = new Thickness(2);

                        if (index > prevRowIndex)
                            th = new Thickness(0, 0, 2, 0);
                        else if (index < prevRowIndex)
                            th = new Thickness(2, 0, 0, 0);
                        else
                        {
                            r.BorderBrush = new SolidColorBrush(Color.FromRgb(26, 46, 44));
                            
                        }

                        r.BorderThickness = th;
                       
                    }
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

        private void FavoriteBondslistBox_DragLeave(object sender, DragEventArgs e)
        {
            this.popup1.IsOpen = false;
        }


        private void FavoriteBondslistBox_Drop(object sender, DragEventArgs e)
        {
            this.popup1.IsOpen = false;
            int index = UITools.GetListViewItemCurrentRowIndex(e.GetPosition, FavoriteBondslistBox);

            if (index < 0 || index == prevRowIndex) return;
            (FavoriteBondslistBox.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem).BorderThickness = new Thickness(0, 0, 0, 0);


            if (index >= FavoriteBondslistBox.Items.Count - 1)
            {
                index = FavoriteBondslistBox.Items.Count - 1;
            }
            
            for (int i = 0; i < FavoriteBondslistBox.Items.Count; i++)
            {
                ListViewItem r = FavoriteBondslistBox.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                if (r != null)
                {
                        r.BorderBrush = new SolidColorBrush(Color.FromRgb(26, 46, 44));
                        
                        r.BorderThickness = new Thickness(2);
                }
            }

            if (_cardViewModel != null)
                _cardViewModel.Drop(prevRowIndex, index, FavoriteBondslistBox);

            prevRowIndex = -1;
        }

        
        #endregion

        private void CardView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= CardView_Unloaded;
            LogHelper.Info("CardView_Unloaded");

            this.FavoriteBondslistBox.DragOver -= FavoriteBondslistBox_DragOver;
            this.FavoriteBondslistBox.DragLeave -= FavoriteBondslistBox_DragLeave;
            this.FavoriteBondslistBox.Drop -= FavoriteBondslistBox_Drop;
            this.FavoriteBondslistBox.MouseMove -= FavoriteBondslistBox_MouseMove;


            if (_cardViewModel != null)
            {
                _cardViewModel.Dispose();
            }
        }

        public CardView(string groupId)
        {
            InitializeComponent();

            this.Unloaded += CardView_Unloaded;
            //_cardViewModel = new CardViewModel();
            //_cardViewModel.Init(groupId, this);

            //this.DataContext = _cardViewModel;
        }


        public void Init(string groupId)
        {
            
            if (_cardViewModel == null)
            {
                ScrollViewer.SetHorizontalScrollBarVisibility(this.FavoriteBondslistBox, ScrollBarVisibility.Disabled);

                Style card_ListViewItemStyle = (Style)this.FindResource("Card_ListViewItemStyle");
                DataTemplate card_DataTemplate = (DataTemplate)this.FindResource("Card_DataTemplate");
                //this.FavoriteBondslistBox.ItemsPanel = card_ItemsPanelTemplate;
                this.FavoriteBondslistBox.ItemContainerStyle = card_ListViewItemStyle;
                this.FavoriteBondslistBox.ItemTemplate = card_DataTemplate;
                this.FavoriteBondslistBox.View = this.FindResource("cardView") as PlainView;



                _cardViewModel = new CardViewModel();
                this.DataContext = _cardViewModel;

                _cardViewModel.Init(groupId, this);
            }

            
            
        }


        public CardItem GetFocusItem()
        {
            return FavoriteBondslistBox.SelectedItem as CardItem;
        }
        

        private void FavoriteBondslistBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
