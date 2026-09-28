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
using System.Windows.Shapes;

namespace STC.Frame.CustomUI
{
    /// <summary>
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : Window
    {
        public SettingView()
        {
            InitializeComponent();

            this.Unloaded += SettingView_Unloaded;
        }

        private void SettingView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= SettingView_Unloaded;
            tcPage.Items.Clear();
        }

        public TabControl GetTabControl()
        {
            return tcPage;
        }

        public void AddTabPage(TabItem tabItem)
        {
            bool status = true;
            foreach(TabItem ti in tcPage.Items)
            {
                if (ti.Header.Equals(tabItem.Header))
                {
                    status = false;
                    break;
                }                    
            }
            if (status)
            {
                if ("双边做市" == tabItem.Header.ToString())
                {
                    tcPage.Items.Add(tabItem);
                }
                else
                {
                    int index = -1;
                    for(int i = 0; i < tcPage.Items.Count; i++)
                    {
                        if ("双边做市" == ((TabItem)tcPage.Items[i]).Header.ToString())
                        {
                            index = i;
                        }
                    }
                    if (index >= 0)
                    {
                        tcPage.Items.Insert(index, tabItem);
                    }
                    else
                    {
                        tcPage.Items.Add(tabItem);
                    }    
                }
            }
        }

        public void SetInitSelect()
        {
            if (tcPage.Items != null && tcPage.Items.Count > 0)
                tcPage.SelectedIndex = 0;
        }

        private void TcPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcPage.SelectedItem != null)
            {
                ISetting setting = (tcPage.SelectedItem as TabItem).Content as ISetting;
                if (setting != null)
                {
                    tbTitle.Text = setting.GetName();

                    btnSave.Visibility = setting.NeedSaveButton() ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = false;
            if (tcPage.SelectedItem != null)
            {
                ISetting setting = (tcPage.SelectedItem as TabItem).Content as ISetting;
                if (setting != null)
                    setting.OnSave();

                
            }
            btnSave.IsEnabled = true;
        }
        

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();


                }
                catch { }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
