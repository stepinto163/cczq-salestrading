
using STC.Broker.Plugin.Model.Transaction.CrossBridge;
using STC.Broker.Plugin.View.Transaction.BridgeDesign;
using STC.Broker.Plugin.ViewModel.Transaction;
using STC.Common.Model;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Broker.Plugin.ViewModel.Transaction.BridgeDesign
{
    public class BridgeConfigViewModel
    {
        public ObservableCollection<BridgeConfigInfoViewModel> BridgeConfigInfos { get; set; } = new ObservableCollection<BridgeConfigInfoViewModel>();

        private FrameworkElement loadingUI;

        public BridgeConfigViewModel(FrameworkElement loadingUI)
        {
            this.loadingUI = loadingUI;
            QueryData();

            //foreach (var ub in UserBridgeHandler.Instance.Bridges)
            //{
                //if (string.IsNullOrEmpty(ub.TraderPinYin))
                //{
                //    DataGridDataPo inst = CRMDataHandler.Instance.GetInstByIdbKey(ub.bankId);
                //    ub.PinYin = inst.PinYin;
                //    ub.PinYinFull = inst.PinYinFull;
                //    DataGridDataPo contract = CRMService.Instance.GetContactByIdbKey(ub.bankAgentId);
                //    ub.TraderPinYin = contract.PinYin;
                //    ub.TraderPinYinFull = contract.PinYinFull;
            //    }
            //}

        }

        public List<InsTraderPopModel> GetPopDataList(BridgeConfigInfoViewModel currentModel, string key)
        {
            var bridges = UserBridgeHandler.Instance.Bridges;

            var result = new List<InsTraderPopModel>();
            if (bridges.Count > 0)
            {
                foreach (var item in bridges)
                {
                    bool isExist = false;
                    foreach (var cIns in BridgeConfigInfos)
                    {
                        if (cIns != currentModel && cIns.InsTraderPopModelData.Item1Id == item.bankInstitutionid && cIns.InsTraderPopModelData.Item2Id == item.bankTraderid)
                        {
                            isExist = true;
                            break;
                        }
                    }

                    if (isExist)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(key) || !string.IsNullOrEmpty(key) && (item.bankInstitutionname.Contains(key) || item.bankTradername.Contains(key)
                       || item.PinYin.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                       || item.PinYinFull.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                       || item.TraderPinYin.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                       || item.TraderPinYinFull.IndexOf(key, StringComparison.CurrentCultureIgnoreCase) >= 0
                       ))
                    {
                        result.Add(new InsTraderPopModel
                        {
                            PopShow = $"{item.InstTraderShow}",
                            Item1Id = item.bankInstitutionid,
                            Item2Id = item.bankTraderid,
                            Item1Name = string.IsNullOrEmpty(item.bankShortName) ? item.bankInstitutionname : item.bankShortName,
                            Item2Name = item.bankTradername
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 查询桥配置
        /// </summary>
        private async Task QueryData()
        {
            this.loadingUI.Visibility = Visibility.Visible;
            //var response = await BrokerCEDAServer.Instance.QueryBridgeConfig();
            //loadingUI.Dispatcher.Invoke(new Action(() =>
            //{
            //    ConvertToViewModel(response);
            //    AddDefaultInfo();
            //    this.loadingUI.Visibility = Visibility.Collapsed;
            //}));

        }

        /// <summary>
        /// 更新桥配置
        /// </summary>
        public async Task UpdateData()
        {
            var data = ConvertToModel();
            ////await BrokerCEDAServer.Instance.UpdateBridgeConfig(data);
        }

        /// <summary>
        /// viewmodel to model
        /// </summary>
        /// <returns></returns>
        private List<BridgeConfigInfo> ConvertToModel()
        {
            List<BridgeConfigInfo> result = new List<BridgeConfigInfo>();
            for (int i = 0; i < BridgeConfigInfos.Count; i++)
            {
                var item = BridgeConfigInfos[i];
                if (string.IsNullOrEmpty(item.InsTraderPopModelData.Item1Id) || string.IsNullOrEmpty(item.InsTraderPopModelData.Item2Id) || string.IsNullOrEmpty(item.LimitCountShow))
                {
                    continue;
                }

                var model = new BridgeConfigInfo
                {
                    InstId = item.InsTraderPopModelData.Item1Id,
                    InstTraderId = item.InsTraderPopModelData.Item2Id,
                    InstName = item.InsTraderPopModelData.Item1Name,
                    InstTraderName = item.InsTraderPopModelData.Item2Name,
                    BridgeOrderQuota = int.Parse(item.LimitCountShow),
                    Priority = i + 1,
                    InstBlackListSettingList = new List<BlackListInstitution>(item.Blacklist)
                };
                result.Add(model);
            }
            return result;
        }

        /// <summary>
        /// model to viewmodel
        /// </summary>
        /// <param name="bridgeConfigInfos"></param>
        private void ConvertToViewModel(List<BridgeConfigInfo> bridgeConfigInfos)
        {
            if (bridgeConfigInfos != null)
            {
                BridgeConfigInfos.Clear();
                foreach (var model in bridgeConfigInfos)
                {
                    var viewModel = new BridgeConfigInfoViewModel
                    {
                        BlacklistShow = model.InstBlackListSettingList.Count.ToString(),
                        InsTraderShow = $"{model.InstName}({model.InstTraderName})",
                        LimitCountShow = model.BridgeOrderQuota.ToString(),
                        PriorityShow = model.Priority.ToString(),
                        TradeKey = model.InstTraderId
                    };

                    model.InstBlackListSettingList.ForEach(x => viewModel.Blacklist.Add(x));

                    viewModel.InsTraderPopModelData.Item1Id = model.InstId;
                    viewModel.InsTraderPopModelData.Item1Name = model.InstName;
                    viewModel.InsTraderPopModelData.Item2Id = model.InstTraderId;
                    viewModel.InsTraderPopModelData.Item2Name = model.InstTraderName;

                    BridgeConfigInfos.Add(viewModel);
                }
            }
        }


        private void AddDefaultInfo()
        {
            while (BridgeConfigInfos.Count < 5)
            {
                AddOneRow();
            }
        }

        public void AddOneRow()
        {
            if (BridgeConfigInfos.Count < 20)
            {
                BridgeConfigInfos.Add(new BridgeConfigInfoViewModel
                {
                    PriorityShow = (BridgeConfigInfos.Count + 1).ToString(),
                    InsTraderShow = " ",
                    LimitCountShow = " ",
                    BlacklistShow = string.Empty
                });
            }
        }

        public void RemoveOneRow(BridgeConfigInfoViewModel item)
        {
            if (BridgeConfigInfos.Count > 1)
            {
                BridgeConfigInfos.Remove(item);
            }
            else
            {
                BridgeConfigInfos.Remove(item);
                AddOneRow();
            }
        }

        /// <summary>
        /// 重置优先级
        /// </summary>
        public void ResetPriority()
        {
            for (int i = 0; i < BridgeConfigInfos.Count; i++)
            {
                BridgeConfigInfos[i].PriorityShow = (i + 1).ToString();
            }
        }

    }

    public class BridgeConfigInfoViewModel : ViewModelBase
    {
        private string priorityShow;
        private string limitCountShow;
        private string blacklistShow;

        public InsTraderPopModel InsTraderPopModelData { get; set; } = new InsTraderPopModel();


        public BridgeConfigInfoViewModel()
        {
            Blacklist.CollectionChanged += (a, b) =>
            {
                BlacklistShow = Blacklist.Count.ToString();
            };
        }

        /// <summary>
        /// 机构ID
        /// </summary>
        public string InsKey { get; set; }

        /// <summary>
        /// 交易员ID
        /// </summary>
        public string TradeKey { get; set; }

        /// <summary>
        /// 机构黑名单
        /// </summary>
        public ObservableCollection<BlackListInstitution> Blacklist { get; set; } = new ObservableCollection<BlackListInstitution>();

        /// <summary>
        /// 优先级
        /// </summary>
        public string PriorityShow
        {
            get { return priorityShow; }
            set
            {
                priorityShow = value;
                OnPropertyChanged("PriorityShow");
            }
        }

        /// <summary>
        /// 机构联系人
        /// </summary>
        public string InsTraderShow
        {
            get { return InsTraderPopModelData.PopShow; }
            set
            {
                var temp = string.IsNullOrEmpty(value) ? " " : value;
                InsTraderPopModelData.PopShow = temp;
                OnPropertyChanged("InsTraderShow");
            }
        }

        /// <summary>
        /// 限制笔数
        /// </summary>
        public string LimitCountShow;
        //{
        //    get { return limitCountShow; }
        //    set
        //    {
        //        if (string.IsNullOrWhiteSpace(value) || int.TryParse(value, out var _))
        //        {
        //            SetProperty(ref limitCountShow, value, "LimitCountShow");
        //        }
        //    }
        //}


        /// <summary>
        /// 黑名单
        /// </summary>
        public string BlacklistShow
        {
            get { return blacklistShow; }
            set
            {
                if (string.IsNullOrEmpty(value) || value == "0")
                {
                    blacklistShow = "添加";
                }
                else
                {
                    blacklistShow = value;
                }
                OnPropertyChanged("BlacklistShow");
            }
        }
    }
}
