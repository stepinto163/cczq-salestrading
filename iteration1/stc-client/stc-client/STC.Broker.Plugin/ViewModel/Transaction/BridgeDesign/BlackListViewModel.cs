
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using STC.Broker.Plugin.Model.Transaction.DesignModel;
using STC.Broker.Plugin.View.Transaction.Design;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace STC.Broker.Plugin.ViewModel.Transaction.BridgeDesign
{
    public class BlackListViewModel : ViewModelBase
    {

        private List<InstitutionalInfo> blackList = new List<InstitutionalInfo>();

        private Dictionary<string, Tuple<int, UserCheckUI>> indexDic = new Dictionary<string, Tuple<int, UserCheckUI>>();

        private string bridgeInsTraderName;

        private string searchInsShow;

        /// <summary>
        /// 机构缓存
        /// </summary>
        public static List<InstitutionalInfo> InsCache { get; set; } = new List<InstitutionalInfo>();

        /// <summary>
        /// 桥机构联系人
        /// </summary>
        public string BridgeInsTraderName;
        //{
        //    get { return bridgeInsTraderName; }
        //    set { SetProperty(ref bridgeInsTraderName, value, "BridgeInsTraderName"); }
        //}


        public string SearchInsKey { get; set; }

        /// <summary>
        /// 搜索框展示
        /// </summary>
        public string SearchInsShow;
        //{
        //    get => searchInsShow;
        //    set => SetProperty(ref searchInsShow, value, "SearchInsShow");
        //}


        public ObservableCollection<InstitutionalInfo> PopData { get; set; } = new ObservableCollection<InstitutionalInfo>();

        /// <summary>
        /// 已添加列表
        /// </summary>
        public ObservableCollection<UserCheckUI> AddedList { get; set; } = new ObservableCollection<UserCheckUI>();


        private BridgeConfigInfoViewModel bridgeConfigInfo;

        public BlackListViewModel(BridgeConfigInfoViewModel bridgeConfigInfo)
        {
            //Task.Factory.StartNew(() =>
            //{
            //    if (InsCache.Count == 0)
            //    {
            //        InsCache.AddRange(CRMService.Instance.GetAllInstByCondition(null, 20000));
            //    }
            //});

            //this.bridgeConfigInfo = bridgeConfigInfo;
            //BridgeInsTraderName = bridgeConfigInfo.InsTraderShow;

            //if (bridgeConfigInfo.Blacklist.Count > 0)
            //{
            //    foreach (var item in bridgeConfigInfo.Blacklist)
            //    {
            //        AddOne(item.BlackListInstId);
            //    }
            //}
        }

        public void SetPopDataList(string text)
        {
            var list = InsCache.Where(o => o.Shortnamecn.Contains(text) || o.pinyin.Contains(text) || o.pinyinfull.Contains(text))
                   .OrderBy(o => o.Shortnamecn.Length).Take(10).ToList();

            PopData.Clear();
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    PopData.Add(item);
                }
            }

        }

        public Tuple<int, UserCheckUI> GetAddedItemIndex(string key)
        {
            Tuple<int, UserCheckUI> result = null;
            Tuple<int, UserCheckUI> tuple;
            if (AddedList.Count > 0 && indexDic.TryGetValue(key, out tuple))
            {
                result = tuple;
            }
            return result;
        }


        private UserCheckUI CreateUserCheckUI(InsBlackListViewModel model)
        {
            UserCheckUI userCheckUI = new UserCheckUI();
            userCheckUI.SetWidth(475, 360);
            userCheckUI.Margin = new Thickness(-6, 0, 0, 0);
            userCheckUI.OnDelete += UserCheckUI_OnDelete;
            userCheckUI.SetData(model);
            return userCheckUI;
        }

        private void UserCheckUI_OnDelete(UserCheckUI userCheckUI)
        {

            if (userCheckUI.model is InsBlackListViewModel)
            {
                InsBlackListViewModel insVM = (InsBlackListViewModel)userCheckUI.model;
                indexDic.Remove(insVM.InsId);
            }
            AddedList.Remove(userCheckUI);
            userCheckUI.OnDelete -= UserCheckUI_OnDelete;
        }

        public void AddOne()
        {
            AddOne(SearchInsKey);
            SearchInsKey = string.Empty;
        }

        public void AddOne(string insKey)
        {
            for (int i = 0; i < 30; i++)
            {
                if (InsCache.Count > 0)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
            Tuple<int, UserCheckUI> tuple;
            if (!string.IsNullOrEmpty(insKey) && !indexDic.TryGetValue(insKey, out tuple))
            {
                var addItem = InsCache.FirstOrDefault(x => x.Idbkey == insKey);
                if (addItem != null)
                {
                    InsBlackListViewModel model = new InsBlackListViewModel { InsId = addItem.Idbkey, Text = addItem.Shortnamecn };
                    var viewModel = CreateUserCheckUI(model);
                    AddedList.Add(viewModel);
                    indexDic[model.InsId] = Tuple.Create(AddedList.Count - 1, viewModel);
                }
            }

        }

        /// <summary>
        /// 确认
        /// </summary>
        public void Confirm()
        {
            List<BlackListInstitution> result = new List<BlackListInstitution>();
            foreach (var item in AddedList)
            {
                if (item.model is InsBlackListViewModel)
                {
                    InsBlackListViewModel insBlack = item.model as InsBlackListViewModel;
                    BlackListInstitution institution = new BlackListInstitution
                    {
                        BlackListInstId = insBlack.InsId,
                        BlackListInstName = insBlack.Text
                    };
                    result.Add(institution);
                }
            }
            bridgeConfigInfo.Blacklist.Clear();

            result.ForEach(x => bridgeConfigInfo.Blacklist.Add(x));
        }
    }
}
