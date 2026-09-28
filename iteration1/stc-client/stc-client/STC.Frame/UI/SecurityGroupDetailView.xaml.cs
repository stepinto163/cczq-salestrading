using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Frame.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace STC.Frame.UI
{
    /// <summary>
    /// SecurityGroupDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class SecurityGroupDetailView : UserControl
    {
        SecurityGroupDetailViewModel viewModel = new SecurityGroupDetailViewModel();
        public SecurityGroupDetailView()
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.securityGroupDetailView = this;
            this.comTag.setData(Consts.TagDic);
        }
        public void initBondList(HomeGroupData GroupItem) 
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                viewModel.GroupItem = HomeGroupData.Copy(GroupItem);
                this.comTag.clearTag();
                if (viewModel.GroupItem.tagDic != null)
                {
                    this.comTag.SetSelectValues(viewModel.GroupItem.tagDic);
                }
                else 
                {
                    this.comTag.SelectedValues = new Dictionary<string, string>();
                }
                viewModel.clear();
                if (GroupItem != null)
                {
                    viewModel.initBond(viewModel.GroupItem.securityids);
                }
            }));
        }

        private void IsHomeCb_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void IsHomeCb_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void careGroupChk_Checked(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> adddic = new Dictionary<string, string>();
            adddic.Add(CommonConstants.HOME_Type, "主页");
            comTag.SetSelectValues(adddic);
        }

        private void careGroupChk_Unchecked(object sender, RoutedEventArgs e)
        {
            comTag.closeTag(CommonConstants.HOME_Type);
        }

        private void comTag_SelectedChanged(object sender, RoutedEventArgs e)
        {
            if (comTag.SelectedValues != null && !comTag.SelectedValues.ContainsKey(CommonConstants.HOME_Type))
            {
                this.careGroupChk.IsChecked = false;
            }
            else if (comTag.SelectedValues != null && comTag.SelectedValues.ContainsKey(CommonConstants.HOME_Type))
            {
                this.careGroupChk.IsChecked = true;
            }
        }

        private void delBondBtn_Click(object sender, RoutedEventArgs e)
        {
            //var item = this.GroupView.ItemContainerGenerator.ContainerFromItem((sender as Button).DataContext as DataRowView);
            //(item as ListBoxItem).IsSelected = true;

            //string text = "是否确定删除该债券？";
            //string title = "删除债券";
            //if (Common.View.Component.CustomMessageBox.Question(text, title) == true)
            //{
            //    viewModel.DelBond();
            //}
        }
    }
}
