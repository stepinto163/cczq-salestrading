using System;
using System.Collections.Generic;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using System.Windows.Input;
using STC.quant.plugin.Utils;
using System.Windows;
using STC.Common.Model.MM;
using STC.Common.Handler.Admin;
using System.Collections.Concurrent;
using System.Threading;
using STC.Common.Handler.Ans;
using STC.Common.Model.Ans;
using STC.quant.plugin.Utils.Data;
using STC.Common.Model.Quant;
using STC.Common.Handler.Quant;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel;
using STC.quant.plugin.View.HomePage;
using System.Linq;
using netapi.Api;
using STC.quant.plugin.View.Component;
using STC.Frame.UI;

namespace STC.quant.plugin.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ConcurrentDictionary<string, MMListBoxViewModel> mmListBoxVMdic = new ConcurrentDictionary<string, MMListBoxViewModel>();

        public ICommand UpdateWidenSettingCommand { get; set; }

        public ICommand UpdateCancelWidenSettingCommand { get; set; }

        public ICommand ExportCommand { get; set; }

        public ICommand ImportCommand { get; set; }

        public ICommand AddStrategyCommand { get; set; }
        public ICommand AddStrategyCppCommand { get; set; }
        public ICommand StopAllStrategyCommand { get; set; }
        public ICommand GroupManagerCommand { get; set; }
        

        private Visibility _IsShowGridSplitter = Visibility.Collapsed;

        public Visibility IsShowGridSplitter
        {
            get
            {
                return this._IsShowGridSplitter;
            }

            set
            {
                this._IsShowGridSplitter = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _IsShowMMOrderControl = Visibility.Collapsed;

        public Visibility IsShowMMOrderControl
        {
            get
            {
                return this._IsShowMMOrderControl;
            }

            set
            {
                this._IsShowMMOrderControl = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _MaskVisibility = Visibility.Collapsed;

        public Visibility MaskVisibility
        {
            get
            {
                return this._MaskVisibility;
            }

            set
            {
                this._MaskVisibility = value;
                this.OnPropertyChanged();
            }
        }

      
        public string _WidenBtnText = "拉宽所有报价";
        
        public string WidenBtnText
        {
            get
            {
                return _WidenBtnText;
            }

            set
            {
                if (_WidenBtnText != value)
                {
                    _WidenBtnText = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private Visibility _LockVisibility = Visibility.Collapsed;

        public Visibility LockVisibility
        {
            get
            {
                return this._LockVisibility;
            }

            set
            {
                this._LockVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private bool _BtnEnable = true;

        public bool BtnEnable
        {
            get
            {
                return _BtnEnable;
            }
            set
            {
                this._BtnEnable = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 0: 显示 1：不显示
        /// </summary>
        private Visibility isHiddenMyOrdersOperationGrid;

        public Visibility IsHiddenMyOrdersOperationGrid
        {
            get
            {
                return isHiddenMyOrdersOperationGrid;
            }

            set
            {
                if (isHiddenMyOrdersOperationGrid != value)
                {
                    isHiddenMyOrdersOperationGrid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();
                    FilterData();
                }
            }
        }
        private void FilterData()
        {
            if (mmListBoxVMdic != null)
            {
                foreach (MMListBoxViewModel mmListViewModel in mmListBoxVMdic.Values)
                {
                    mmListViewModel.FilterData(this.SearchText);
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainViewModel()
        {
            this.AddStrategyCommand = new RelayCommand(this.AddStrategy);
            this.AddStrategyCppCommand = new RelayCommand(this.AddStrategyCpp);
            this.StopAllStrategyCommand = new RelayCommand(this.StopAll);
            this.GroupManagerCommand = new RelayCommand(this.GroupManager);
            Utils.Data.QuantGroupHandler.Instance.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

            // Todo: 注册清零信号, 暂时先不清零
            BaseHandler.Instance.Init(Instance_OnClear);

            MMHandler.Instance.SubscribeMMSysStatus(OnMMSysStatus, new List<string> { LocalEnv.UserInfo.UserName });

            AnsHandler.Instance.SubscribeAnsSummary(OnSubscribeAnsSummary);

            //InitCurrentANSSummaryTestData();

        }

        private void OnMMSysStatus(MMSysStatus obj)
        {
            if ("0" == obj.status)
            {
                WidenBtnText = "恢复所有报价";
                LockVisibility = Visibility.Visible;
                BtnEnable = false;
            }
            else
            {
                WidenBtnText = "拉宽所有报价";
                LockVisibility = Visibility.Collapsed;
                BtnEnable = true;
            }
        }

        private void HomePageDataHandler_OnFavoritesChanged(Utils.Data.QuantGroupHandler.FavoriteTypeEnum favoriteTypeEnum, string groupID, string securtityID)
        {
            if (favoriteTypeEnum == Utils.Data.QuantGroupHandler.FavoriteTypeEnum.AddGroup)
            {
                if (mmListBoxVMdic.ContainsKey(groupID) == false)
                {
                    MMListBoxViewModel mMListBoxViewModel = new MMListBoxViewModel(groupID);
                    mmListBoxVMdic[groupID] = mMListBoxViewModel;
                }
            }
            else if (favoriteTypeEnum == Utils.Data.QuantGroupHandler.FavoriteTypeEnum.DelGroup)
            {
                if (mmListBoxVMdic.ContainsKey(groupID))
                {
                    MMListBoxViewModel mMListBoxViewModel;
                    mmListBoxVMdic.TryRemove(groupID, out mMListBoxViewModel);
                }
            }
        }

        public MMListBoxViewModel GetMMListBoxViewModelByGroupID(string groupid)
        {
            if (mmListBoxVMdic.ContainsKey(groupid))
            {
                return mmListBoxVMdic[groupid];
            }
            return null;
        }

    
        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    this.Clear();
                }));
            }
        }

        private void OnSubscribeAnsSummary(ANSSummary ans)
        {
            Application.Current.Dispatcher.BeginInvoke ((Action)(() =>
            {

              
            })); 
        }

      

        ~MainViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            Clear();

            base.Dispose();
        }


        public void Clear()
        {
            if (mmListBoxVMdic != null)
            {
                foreach (MMListBoxViewModel mmListBoxViewModel in mmListBoxVMdic.Values)
                {
                    mmListBoxViewModel.Clear();
                }
            }
           
        }

        public async void AddStrategy()
        {
            AddStrategWindow addStrategWindow = new AddStrategWindow("新增策略");
            addStrategWindow.Owner = Application.Current.MainWindow;
            addStrategWindow.ShowDialog();
            if (addStrategWindow.Result == false)
            {
                return;
            }
            QuantStrategyReq request = new QuantStrategyReq();
            request.quantid = addStrategWindow.QuantName;
            request.quantname = addStrategWindow.Remark;
            //request.groupid = addStrategWindow.GroupCode;
            request.lable = addStrategWindow.GroupCode;
            request.enable = "0";
            request.action = "1"; // 添加
            request.templetid = addStrategWindow.TempletName;
            request.type = addStrategWindow.Type;
            PResult pResult = await Utils.Data.QuantGroupHandler.Instance.AsyncUpdateQuants(request);
            if (pResult != null && pResult.IsSuccess())
            {
                CustomMessageBox.Info($"新增成功！", "新增策略");
            }
            else
            {
                CustomMessageBox.Warning(pResult == null ? "新增策略！" : pResult.Msg, "新增策略");
            }
        }

        public async void AddStrategyCpp()
        {
            AddStrategWindow addStrategWindow = new AddStrategWindow("新增策略","4");
            addStrategWindow.Owner = Application.Current.MainWindow;
            addStrategWindow.ShowDialog();
            if (addStrategWindow.Result == false)
            {
                return;
            }
            QuantStrategyReq request = new QuantStrategyReq();
            request.quantid = addStrategWindow.QuantName;
            request.quantname = addStrategWindow.Remark;
            request.lable = addStrategWindow.GroupCode;
            request.enable = "0";
            request.action = "1"; // 添加
            request.type = addStrategWindow.Type;
            PResult pResult = await Utils.Data.QuantGroupHandler.Instance.AsyncUpdateQuants(request);
            if (pResult != null && pResult.IsSuccess())
            {
                CustomMessageBox.Info($"新增成功！", "新增策略");
            }
            else
            {
                CustomMessageBox.Warning(pResult == null ? "新增策略！" : pResult.Msg, "新增策略");
            }
        }

        public void StopAll()
        {
            List<TTbookQuantStrategys> list = QuantGroupHandler.Instance.GetMMsByGroupID("");
            if (list != null && list.Count > 0)
            {
                List<TTbookQuantStrategys> tempList = new List<TTbookQuantStrategys>();
                if (QuantGroupHandler.Instance.ManageYourselfViewAll)//管理自己&查看全部
                {
                    var tr = list.Where(p => p.Username == ApiManager.Instance.GetDefaultApi().UserName);
                    tempList.AddRange(tr.ToList());
                }
                else
                    tempList.AddRange(list);

                ThreadPool.QueueUserWorkItem(o =>
                {
                    PResult pResult = QuantHandler.Instance.disableStrategys(tempList);
                    if (pResult != null && "0000".Equals(pResult.Code))
                    {
                       // CustomMessageBox.Info($"全撤全停！", "全撤全停");
                    }
                    else
                    {
                        CustomMessageBox.Warning(pResult == null ? "全撤全停失败！" : pResult.Msg, "");
                    }
                });
            }
        }

        public void GroupManager()
        {
            //if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GROUPMANAGER))
            //{
            //    SecurityGroupManagerView securityGroupManagerView = new SecurityGroupManagerView();
            //    securityGroupManagerView.setDefaultParam(CommonConstants.QUANT_Type);
            //    WindowManager.Instance.OpenWindow(ComWindowKeyHelper.GROUPMANAGER, securityGroupManagerView);
            //}
            if (!WindowManager.Instance.IsExist(ComWindowKeyHelper.GROUPMANAGER))
            {
                GroupManagerControl group = new GroupManagerControl();
                group.setDefaultParam(CommonConstants.QUANT_Type);
                WindowManager.Instance.OpenOnlyOneWindow(ComWindowKeyHelper.GROUPMANAGER, group);
                //group.Owner = Application.Current.MainWindow;
                //group.ShowDialog();
            }

        }


    }
}
