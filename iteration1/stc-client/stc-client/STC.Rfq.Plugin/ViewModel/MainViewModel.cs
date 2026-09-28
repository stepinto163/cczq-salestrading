using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using System.Data;
using System.Windows.Input;
using STC.Rfq.Plugin.Utils;
using System.Windows;
using STC.Rfq.Plugin.View.Component;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using System.Collections.Concurrent;
using STC.Rfq.Plugin.View;
using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Model.Ans;
using STC.Rfq.Plugin.Utils.Data;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.IO;
using STC.Rfq.Plugin.Command;
using STC.Common.View.UI;
using System.Threading;
using System.Windows.Media;
using STC.Rfq.Plugin.View.HomePage;
using STC.Common.View.Component;
using System.Linq;
using System.Timers;
using STC.Common.Handler.RFQ;
using STC.Common.Model.RFQ;
using Newtonsoft.Json;

namespace STC.Rfq.Plugin.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public static string BncMoniValue; // 利率债最优点差监控
        public static string BcoMoniValue; // 信用债最优点差监控

        private ConcurrentDictionary<string, string> _SavedOrderControlDic = new ConcurrentDictionary<string, string>();
        WindowManager windowManager = WindowManager.Instance;

        private ConcurrentDictionary<string, RfqListBoxViewModel> mmListBoxVMdic = new ConcurrentDictionary<string, RfqListBoxViewModel>();

        public ICommand ExportCommand { get; set; }

        public ICommand ImportCommand { get; set; } 

        public ICommand AddBondCommand { get; set; }


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

    

        private bool _HasGroup = false;

        public bool HasGroup
        {
            get
            {
                return _HasGroup;
            }
            set
            {
                this._HasGroup = value;
                this.OnPropertyChanged();
            }
        }


        public RfqGroupInfo SelectGroup { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainViewModel()
        {
            RfqGroupHandler.Instance.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;

            // Todo: 注册清零信号, 暂时先不清零
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        public void GroupInit(string groupID) 
        {
            if (mmListBoxVMdic.ContainsKey(groupID) == false)
            {
                RfqListBoxViewModel mMListBoxViewModel = new RfqListBoxViewModel(groupID);
                mmListBoxVMdic[groupID] = mMListBoxViewModel;
                HasGroup = mmListBoxVMdic.Count > 0;
            }
        }

        private void HomePageDataHandler_OnFavoritesChanged(RfqGroupHandler.FavoriteTypeEnum favoriteTypeEnum, string groupID, string securtityID)
        {
            if (favoriteTypeEnum == RfqGroupHandler.FavoriteTypeEnum.AddGroup)
            {
                LogHandler.Info($"1.Add Rfq group:[{groupID}]");
                if (mmListBoxVMdic.ContainsKey(groupID) == false)
                {
                    LogHandler.Info($"2.Add Rfq group:[{groupID}]");
                    RfqListBoxViewModel mMListBoxViewModel = new RfqListBoxViewModel(groupID);
                    mmListBoxVMdic[groupID] = mMListBoxViewModel;
                    HasGroup = mmListBoxVMdic.Count > 0;
                }
            }
            else if (favoriteTypeEnum == RfqGroupHandler.FavoriteTypeEnum.DelGroup)
            {
                LogHandler.Info($"1.Del Rfq group:[{groupID}]");
                if (mmListBoxVMdic.ContainsKey(groupID))
                {
                    LogHandler.Info($"2.Del Rfq group:[{groupID}]");
                    RfqListBoxViewModel mMListBoxViewModel;
                    mmListBoxVMdic.TryRemove(groupID, out mMListBoxViewModel);

                    List<RfqstgViewModel> rfggroup;
                    RfqGroupHandler.Instance._CacheMMListDic.TryRemove(groupID, out rfggroup);

                    mMListBoxViewModel.Dispose();
                    HasGroup = mmListBoxVMdic.Count > 0;
                }
            }
        }

        public RfqListBoxViewModel GetMMListBoxViewModelByGroupID(string groupid)
        {
            if (mmListBoxVMdic.ContainsKey(groupid) == false)
            {
                RfqListBoxViewModel mMListBoxViewModel = new RfqListBoxViewModel(groupid);
                mmListBoxVMdic[groupid] = mMListBoxViewModel;
            }
            if (mmListBoxVMdic.ContainsKey(groupid))
            {
                return mmListBoxVMdic[groupid];
            }

            HasGroup = mmListBoxVMdic.Count > 0;
            return null;
        }

        public void SetActiveMMListBox(string mainGuid, string groupid)
        {
            foreach (RfqListBoxViewModel vm in mmListBoxVMdic.Values)
            {
                if (vm.groupID == groupid)
                {
                    if (vm.mainWinList.ContainsKey(mainGuid) == false)
                    {
                        vm.mainWinList[mainGuid] = mainGuid;
                    }
                }
                else
                {
                    string removeValue;
                    bool flag = vm.mainWinList.TryRemove(mainGuid, out removeValue);
                }

               // vm.VmRefreshCCDC();
            }
        }

        
        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                }));
            }
        }

       
        ~MainViewModel()
        {
            LogHandler.Info($"Rfq dispose");
            this.Dispose();
        }

        new public void Dispose()
        {
            Clear();

            if (mmListBoxVMdic != null)
            {
                foreach(RfqListBoxViewModel mmListBoxViewModel in mmListBoxVMdic.Values)
                {
                    mmListBoxViewModel.Dispose();
                }
                mmListBoxVMdic.Clear();
                mmListBoxVMdic = null;
                HasGroup = false;
            }

            RfqGroupHandler.Instance.OnFavoritesChanged -= HomePageDataHandler_OnFavoritesChanged;

            BaseHandler.Instance.UnSubscribe(Instance_OnClear);

            
            base.Dispose();
        }

        public void Clear()
        {
            if (mmListBoxVMdic != null)
            {
                foreach (RfqListBoxViewModel mmListBoxViewModel in mmListBoxVMdic.Values)
                {
                    mmListBoxViewModel.Clear();
                }
            }

            LogHandler.Info(string.Format("======Clear====== Hour = {0}", int.Parse(DateTimeEx.ServerTime.ToString("HH"))));
            //RestartAnsCheckTimer();
        }

        #region 过滤
        private void FilterData()
        {
            if (mmListBoxVMdic != null)
            {
                foreach (RfqListBoxViewModel mmListViewModel in mmListBoxVMdic.Values)
                {
                   // mmListViewModel.FilterData(this.SearchText);
                }
            }
        }

        #endregion
    }

    public class OrderControlPriceType : ViewModelBase
    {
        private string code = "1";
        public string Code
        {
            get { return code; }
            set
            {
                code = value;
                OnPropertyChanged();
            }
        }
    }
}
