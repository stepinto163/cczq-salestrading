using com.gateway.log;
using STC.Broker.Plugin.ViewModel.Setting;
using STC.Common.Handler.PT;
using STC.Common.Model;
using STC.Common.Model.PT;
using STC.Common.View.Component;
using STC.Frame.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace STC.Broker.Plugin.View.Setting
{
    /// <summary>
    /// RelevanceInstWin.xaml 的交互逻辑
    /// </summary>
    public partial class RelevanceInstWin : WindowBase, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool isAllSelect;
        /// <summary>
        /// 全选
        /// </summary>
        public bool IsAllSelect
        {
            get
            {
                return isAllSelect;
            }
            set
            {
                if (isAllSelect != value)
                {
                    isAllSelect = value;
                    OnPropertyChanged("IsAllSelect");
                }
            }
        }

        private ObservableCollection<RelevanceInstViewModel> _relevanceInstViewModels = new ObservableCollection<RelevanceInstViewModel>();

        public ObservableCollection<RelevanceInstViewModel> RelevanceInstViewModels
        {
            get
            {
                return _relevanceInstViewModels;
            }
            set
            {
                if (_relevanceInstViewModels != value)
                {
                    _relevanceInstViewModels = value;
                    OnPropertyChanged("RelevanceInstViewModels");
                }
            }
        }

        public RelevanceInstWin()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += RelevanceInstWin_MouseLeftButtonDown;
            LoadingUI.Visibility = Visibility.Visible;
            QueryRelevanceInstInfo();
        }


        /// <summary>
        /// 查询关联机构信息
        /// </summary>
        private async void QueryRelevanceInstInfo()
        {
            InstAssociatedReply response = null;
            string seachText = tb_Inst.Text.Trim();
            await Task.Run(() => {
                 var request = new InstAssociatedRequest();
                 request.action = "0";
                 response = GRHandler.Instance.OperateInstAssociated(request);
            });
            if (response != null && response.IsSuccess() && response.Datas != null && response.Datas.Count > 0)
            {
                _relevanceInstViewModels.Clear();
                foreach (InstAssociatedInfo item in response.Datas)
                {
                    if (string.IsNullOrEmpty(seachText) 
                        || item.isntName == null || item.isntName.Contains(seachText)
                         || item.associatedInstName == null || item.associatedInstName.Contains(seachText))
                    {
                        _relevanceInstViewModels.Add(
                            new RelevanceInstViewModel
                            {
                                ID = item.id,
                                Inst = item.instId,
                                InstName = item.isntName,
                                RelevanceInst = item.associatedInstId,
                                RelevanceInstName = item.associatedInstName,
                                Operator = item.updateby,
                                OperationTime = item.updatetime,
                            });
                    }
                }
            }
            else
            {
                _relevanceInstViewModels.Clear();
            }
            InstDataGrid.DataContext = _relevanceInstViewModels;
            LoadingUI.Visibility = Visibility.Collapsed;
            IsAllSelect = false;
            SetControlStatus();
        }

        private void RelevanceInstWin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddRelevanceInstWin relevanceInstWin = new AddRelevanceInstWin();
            relevanceInstWin.ReflshWin += RelevanceInstWin_ReflshWin;
            relevanceInstWin.ShowDialog();
        }

        private void RelevanceInstWin_ReflshWin()
        {
            QueryRelevanceInstInfo();
        }

        private async void Delete_Inst(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingUI.Visibility = Visibility.Visible;

                if (_relevanceInstViewModels.Count > 0)
                {
                    var instIds = new List<string>();
                    foreach (var item in _relevanceInstViewModels)
                    {
                        if (item.IsSelect)
                        {
                            instIds.Add(item.ID);
                        }
                    }

                    BaseReplyPo baseDataReplyPo = null;
                    await Task.Run(() => {
                        InstAssociatedRequest relevanceRequestPo = new InstAssociatedRequest()
                        {
                            action = "3",
                            deleteList = instIds
                        };
                        baseDataReplyPo = GRHandler.Instance.OperateInstAssociated(relevanceRequestPo);
                    });
                    
                    if (baseDataReplyPo != null && baseDataReplyPo.IsSuccess())
                    {
                        QueryRelevanceInstInfo();
                    }
                    else
                    {
                        SysWarnGUI warn = new SysWarnGUI();
                        Window win = GetWindow(this);
                        if (win != null)
                            warn.Owner = win;
                        if (baseDataReplyPo == null || string.IsNullOrEmpty(baseDataReplyPo.Msg))
                        {
                            warn.setWarnMsg("删除关联方失败");
                        }
                        else
                        {
                            warn.setWarnMsg(baseDataReplyPo.Msg);
                        }
                        warn.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
            finally
            {
                LoadingUI.Visibility = Visibility.Collapsed;
            }
        }

        private void tb_Inst_KeyDown(object sender, KeyEventArgs e)
        {
            LoadingUI.Visibility = Visibility.Visible;
            QueryRelevanceInstInfo();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void cb_all_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox)
            {
                CheckBox checkBox = sender as CheckBox;
                foreach (var item in _relevanceInstViewModels)
                {
                    item.IsSelect = checkBox.IsChecked.Value;
                }
                SetControlStatus();
            }
        }

        /// <summary>
        /// 设置控件状态
        /// </summary>
        private void SetControlStatus()
        {
            if (_relevanceInstViewModels != null
               && _relevanceInstViewModels.Count > 0)
            {
                List<RelevanceInstViewModel> issueInstViews = new List<RelevanceInstViewModel>();
                foreach (RelevanceInstViewModel item in _relevanceInstViewModels)
                {
                    if (item.IsSelect)
                    {
                        issueInstViews.Add(item);
                    }
                }

                if (issueInstViews.Count > 0)
                {
                    this.delete.IsEnabled = true;
                }
                else
                {
                    this.delete.IsEnabled = false;
                }
            }
            else
            {
                this.delete.IsEnabled = false;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_relevanceInstViewModels != null
               && _relevanceInstViewModels.Count > 0)
            {
                bool isAllSelect = true;
                foreach (RelevanceInstViewModel item in _relevanceInstViewModels)
                {
                    if (!item.IsSelect)
                    {
                        IsAllSelect = false;
                        isAllSelect = false;
                        break;
                    }
                }
                IsAllSelect = isAllSelect;
            }
            SetControlStatus();
        }
    }
}

