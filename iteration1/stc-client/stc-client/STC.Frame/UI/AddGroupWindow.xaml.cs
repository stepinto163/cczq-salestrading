using STC.Common.Model.Admin;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Frame.ViewModel;
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

namespace STC.Frame.UI
{
    /// <summary>
    /// AddGroupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddGroupWindow : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; private set; }

        /// <summary>
        /// 组信息
        /// </summary>
        public GroupMsgPo groupMsgPo { get; private set; }
        public AddGroupWindow()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (check())
            {
                groupMsgPo = new GroupMsgPo();
                this.GroupName = groupname_txt.Text;
                groupMsgPo.IsCareGroup = (bool)careGroupChk.IsChecked ? "true" : "false";
                groupMsgPo.IsDynamicsGroup = (bool)dynamicsGroupChk.IsChecked ? "true" : "false";
                groupMsgPo.IsAllInstGroup = (bool)allInstGroupChk.IsChecked ? "true" : "false";
                int i = 1;
                if (comTag.SelectedValues.Values.Count > 0)
                {
                    foreach (string key in comTag.SelectedValues.Values)
                    {
                        groupMsgPo.Tag += key;
                        if (i < comTag.SelectedValues.Values.Count)
                            groupMsgPo.Tag += " ";
                        i++;
                    }
                }
                else
                    groupMsgPo.Tag = comTag.getSearchTextBoxText();
                this.Result = true;
                this.Close();
                e.Handled = true;
            }
            else 
            {
                this.Result = false;
            }
        }
        private bool check() 
        {
            bool ischeck = false;
            if (string.IsNullOrWhiteSpace(groupname_txt.Text))
            {
                error_tb.Text = "分组名称不能为空";
                //CustomMessageBox.Info("分组名称不能为空!", "新增分组");
                return ischeck;
            }
            if (SecurityGroupManagerViewModel.IsGroupNameExist(groupname_txt.Text,string.Empty))
            {
                error_tb.Text = "分组名称已存在";
                return ischeck;
            }
            ischeck = true;
            return ischeck;
        }

        public void setDefaultTag(string defaultTag) 
        {
            if (!string.IsNullOrEmpty(defaultTag))
            {
                Dictionary<string, string> adddic = new Dictionary<string, string>();
                if (CommonConstants.HOME_Type.Equals(defaultTag))
                {
                    //adddic.Add(CommonConstants.HOME_Type, "主页");
                    careGroupChk.IsChecked = true;
                }
                else if (CommonConstants.QUANT_Type.Equals(defaultTag))
                {
                    adddic.Add(CommonConstants.QUANT_Type, "策略"); 
                    comTag.SetSelectValues(adddic);
                }
                else if (CommonConstants.INNER_Type.Equals(defaultTag))
                {
                    adddic.Add(CommonConstants.INNER_Type, "内部看板");
                    comTag.SetSelectValues(adddic);
                }

            }
        }

        private void careGroupChk_Checked(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> adddic = new Dictionary<string, string>();
            adddic.Add(CommonConstants.HOME_Type,"主页");
            comTag.SetSelectValues(adddic);
        }

        private void careGroupChk_Unchecked(object sender, RoutedEventArgs e)
        {
            comTag.closeTag(CommonConstants.HOME_Type);
        }

        private void comTag_SelectedChanged(object sender, RoutedEventArgs e)
        {
            if (!comTag.SelectedValues.ContainsKey(CommonConstants.HOME_Type)) 
            {
                this.careGroupChk.IsChecked = false;
            }
            else if(comTag.SelectedValues.ContainsKey(CommonConstants.HOME_Type))
            {
                this.careGroupChk.IsChecked = true;
            }
        }
    }
}
