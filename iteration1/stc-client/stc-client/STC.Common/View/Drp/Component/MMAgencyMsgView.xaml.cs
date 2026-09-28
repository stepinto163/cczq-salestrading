using STC.Common.Model.Drp;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Drp;
using STC.Common.ViewModel.Drp.Component;
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

namespace STC.Common.View.Drp.Component
{
    /// <summary>
    /// MMAgencyMsgView.xaml 的交互逻辑
    /// </summary>
    public partial class MMAgencyMsgView : Window
    {
        private MMAgencyMsgViewModel viewModel = null;
        private MsgPo _msg;
        public MsgPo msgPo 
        {
            get { return _msg; }
            set 
            { 
                if (value != null) 
                {
                    _msg = value;
                } 
            }
        }
        public MMAgencyMsgView(MsgPo msg)
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);
            viewModel = new MMAgencyMsgViewModel();
            viewModel.Init(this, msg);
            viewModel.mMAgencyMsgView = this;
            this.DataContext = viewModel;
            this._msg = msg;
            SetComboBoxItemsSource(this.ReasonCbb, "REASON");
            SetComboBoxItemsSource(this.GroupCbb, "GROUP");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (this.DataContext != null && this.DataContext is MMAgencyMsgViewModel)
            {
                (this.DataContext as MMAgencyMsgViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x28, 0x28, 0x28));
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (this.DataContext != null && this.DataContext is MMAgencyMsgViewModel)
            {
                (this.DataContext as MMAgencyMsgViewModel).BorderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
            }
        }

        public void setCombo(string key) 
        {
            SetComboBoxItemsSource(this.GroupCbb, key);
        }

        private void SetComboBoxItemsSource(MultiSelectComboBox cb, string key)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            if (key == "REASON")
            {
                dic["0001"] = "策略被动关闭";
                dic["0010"] = "断报";
            }
            else if(key == "GROUP")
            {
                //string detail = "{\"creatTime\":\"2023-08-09 10:18:17\",\"dangerCount\":1,\"infos\":[{\"createTime\":\"2023-08-09 10:18:17\",\"groupId\":\"1\",\"groupName\":\"分组2\",\"groupType\":\"UNIFY\",\"monitorStatus\":\"2\",\"reason\":\"自动关闭\",\"securityId\":\"200014.IB\",\"symbol\":\"20附息国债14\"},{\"createTime\":\"2023-08-09 10:18:17\",\"groupId\":\"2\",\"groupType\":\"NORMAL\",\"monitorStatus\":\"3\",\"reason\":\"自动关闭\",\"securityId\":\"200016.IB\",\"symbol\":\"20附息国债16\"}],\"passiveCount\":1,\"status\":\"2\"}";
                //this._msg.Detail = detail;
                MMBreakAlarmModel mMBreak = Newtonsoft.Json.JsonConvert.DeserializeObject<MMBreakAlarmModel>(_msg.Detail);
                if (mMBreak.infos.Count > 0) 
                {
                    List<BreakAlarmModel> groupList = mMBreak.infos.GroupBy(x => x.groupId).Select(x => x.First()).ToList();
                    if (groupList.Count > 0) 
                    {
                        foreach(BreakAlarmModel po in groupList) 
                        {
                            if (!string.IsNullOrEmpty(po.groupId))
                            {
                                dic[po.groupId] = po.groupName;
                            }
                        }
                    }
                }
            }
            cb.ItemsSource = dic;
        }

        private void ReasonCbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            string key = ccb.Tag?.ToString();
            string selectValue = ccb.SelectedValues;
            this.viewModel.ReasonFilter(selectValue);
        }

        private void GroupCbb_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;
            string key = ccb.Tag?.ToString();
            string selectValue = ccb.SelectedValues;
            this.viewModel.GroupFilter(selectValue);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
