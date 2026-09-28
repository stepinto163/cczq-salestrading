using com.gateway.log;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Table;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.WorkTable.Plugin.Utils.Data.CRM.Model.CRM;
using STC.WorkTable.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static STC.WorkTable.Plugin.View.Component.AutoCompleteComboBox;
using WtUtils = STC.WorkTable.Plugin.Utils.CommonUtils;

namespace STC.WorkTable.Plugin.ViewModel.WorkTable
{
    public class RecognizeIntentionQuoteViewModel: ViewModelBase
    {
        private CrmInst _selectedInstitution_Edit;

        public CrmInst SelectedInstintuion_Edit
        {
            get { return _selectedInstitution_Edit; }
            set
            {
                _selectedInstitution_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _institutionName_Edit;

        public string InstitutionName_Edit
        {
            get { return _institutionName_Edit; }
            set
            {
                _institutionName_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private CrmContact _selectedTrader_Edit;

        public CrmContact SelectedTrader_Edit
        {
            get { return _selectedTrader_Edit; }
            set
            {
                _selectedTrader_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private string _traderName_Edit;

        public string TraderName_Edit
        {
            get { return _traderName_Edit; }
            set
            {
                _traderName_Edit = value;
                this.OnPropertyChanged();
            }
        }

        private Dictionary<string, string> _sideDic = new Dictionary<string, string>();
        public Dictionary<string, string> SideDic
        {
            get
            {
                if (_sideDic.Count == 0)
                {
                    _sideDic["1"] = "Bid";
                    _sideDic["2"] = "Ofr";
                }
                return _sideDic;
            }
        }

        private object _LockObject = new object();
        private CollectionViewSource viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        {
            get { return viewSource; }
            set { viewSource = value; }
        }

        private CustomCollection<IntentionQuoteItem> intentionQuoteInfo = new CustomCollection<IntentionQuoteItem>();

        public CustomCollection<IntentionQuoteItem> IntentionQuoteInfo
        {
            get { return intentionQuoteInfo; }
            set { intentionQuoteInfo = value; }
        }

        private IntentionQuoteItem _selectedItem_Recognize;
        public IntentionQuoteItem SelectedItem_Recognize
        {
            get
            {
                return this._selectedItem_Recognize;
            }

            set
            {
                this._selectedItem_Recognize = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand DelOrderCommand { get; set; }

        internal void UnregisterEvent()
        { 

            IntentionHandler.Instance.OnRecognition -= Instance_OnRecoginition;
        }

        public RecognizeIntentionQuoteViewModel()
        {
            DelOrderCommand = new ComRelayCommand(DeleteOrder);

            IntentionHandler.Instance.OnRecognition += Instance_OnRecoginition;
            this.ViewSource.IsLiveFilteringRequested = true;
            this.ViewSource.IsLiveSortingRequested = true;
            this.ViewSource.LiveFilteringProperties.Add("Status");
            this.ViewSource.Source = IntentionQuoteInfo;
            BindingOperations.EnableCollectionSynchronization(this.ViewSource.View, _LockObject);
        }

        private void Instance_OnRecoginition(IntentionQuoteInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                AddRecognize(obj);
                //IntentionQuoteItem item = null;
                //var findList = IntentionQuoteInfo.Where(x => x.IntentionId == obj.intentionId);
                //if (findList != null && findList.Count() > 0)
                //{
                //    item = findList.First();
                //    if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName
                //        && obj.status != "0")
                //    {
                //        this.IntentionQuoteInfo.Remove(item);
                //        return;
                //    }

                //    item.Status = obj.status;

                //    item.InstitutionId = obj.institutionId;
                //    item.InstitutionName = obj.institutionName;
                //    item.TraderId = obj.traderId;
                //    item.TraderName = obj.traderName;
                //    item.Side = obj.side;
                //    item.Price = obj.price;
                //    item.Qty = obj.qty;
                //    item.SettlType = obj.settlType;
                //    item.UpdateTime = obj.updateTime;
                //    item.UserName = obj.userName;
                //    item.CloseBy = obj.closeBy;
                //    item.OrderID = obj.orderId;
                //    item.QqNumber = obj.qqNumber;

                //}

            }));
        }

        internal void ClearList()
        {
            this.IntentionQuoteInfo.Clear();
        }

        private IntentionQuoteItem AddRecognize(IntentionQuoteInfo obj)
        {
            IntentionQuoteItem item = new IntentionQuoteItem();

            item.IntentionId = obj.intentionId;
            item.InstitutionId = obj.institutionId;
            item.InstitutionName = obj.institutionName;
            item.TraderId = obj.traderId;
            item.TraderName = obj.traderName;
            item.QqNumber = obj.qqNumber;
            item.Side = obj.side;
            item.SecurityId = obj.securityId;
            item.Symbol = obj.symbol;
            item.NetPrice = obj.netPrice;
            //item.FullPrice = obj.fullPrice;
            item.Price = obj.price;
            item.Qty = obj.qty;

            item.SettlType = obj.settlType;
            //string tradeDate = CommonUtils.CalTradeDate(obj.settlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(obj.settlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;

            //item.BrokerId = obj.brokerId;
            //item.BrokerName = obj.brokerName;
            item.Text = obj.text;
            item.CreateTime = obj.createTime;
            item.UpdateTime = obj.updateTime;
            item.UserName = obj.userName;
            //item.CloseBy = obj.closeBy;
            item.Status = "0";//;obj.status;
            //item.OrderID = obj.orderId;

            this.IntentionQuoteInfo.Add(item);

            return item;
        }


        public void RecognizeIntentionQuote(string content)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    RecognizeIntentionQuoteRequest request = new RecognizeIntentionQuoteRequest();
                    request.userName = LocalEnv.UserInfo.UserName;
                    request.content = content;

                    ///获取订单 
                    RecognizeIntentionQuoteReplyPo reply = TableHandler.Instance.RecognizeIntentionQuote(request);

                    if (reply == null)
                    {
                        CommonUtils.Prompt2Second("警告", $"报价识别失败");
                    }
                    else
                    {
                        if (!reply.IsSuccess())
                        {
                            CommonUtils.Prompt2Second("警告", reply.Msg);
                        }
                        //else
                        //{
                        //    if (reply.Datas != null)
                        //    {
                        //        Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                        //        {
                        //            int index = 0;
                        //            foreach (IntentionQuoteInfo obj in reply.Datas)
                        //            {
                        //                IntentionQuoteItem  item = AddRecognize(obj);
                        //                item.No = ++index;
                        //                await System.Windows.Threading.Dispatcher.Yield();
                        //            }
                        //        }));
                        //    }
                        //}
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error("RequestOrders", e);
                }
            });
        }


        /// <summary>
        /// 编辑记录
        /// </summary>
        /// <param name="editBtn"></param>
        private void EditRow(Button editBtn)
        {
            if (editBtn != null && editBtn.DataContext is IntentionQuoteItem)
            {
                IntentionQuoteItem item = editBtn.DataContext as IntentionQuoteItem;
                EditRow(item);
            }
        }

        internal void EditRow(IntentionQuoteItem item)
        {
            item.IsEdit = "0";
            item.InstitutionName_Edit = item.InstitutionName;
            //item.InstitutionName_Text = item.InstitutionName;
            item.TraderName_Edit = item.TraderName;
            //item.TraderName_Text = item.TraderName;
            item.Side_Edit = item.Side;
            item.Price_Edit = item.Price;
            item.Qty_Edit = item.Qty;
            item.SettlType_Edit = item.SettlType;
            //string tradeDate = CommonUtils.CalTradeDate(item.SettlType);
            //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
            //DateTime settlDateTime;
            //int selectIndex;
            //CommonUtils.SetSettleType(item.SettlType, tradeDate, out settlDateTime, out selectIndex);
            //item.edit_settletype = selectIndex;
        }

        private static string COLUMNNAME_INSTITUTION = "客户机构";
        private static string COLUMNNAME_TRADER = "客户名";
        private static string COLUMNNAME_SIDE = "方向";
        private static string COLUMNNAME_PRICE = "报价";
        private static string COLUMNNAME_QTY = "报量";
        private static string COLUMNNAME_SETTLETYPE = "清算速度";

        internal void SaveRow(IntentionQuoteItem item, string columnHeader = null)
        {
            item.IsEdit = "2";
            // 价格
            decimal? price = null;
            // 报量
            decimal? qty = null;
            // 清算速度
            string settlType = null;
            // 交易员编号
            string traderId = null;
            // 交易员名称
            string traderName = null;
            // 经纪商编号
            string brokerId = null;
            // 经纪商名称
            string brokerName = null;
            // 机构编号
            string institutionId = null;
            // 机构名称
            string institutionName = null;
            // 方向
            string side = null;

            // 客户机构
            if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
            {
                if (this.SelectedInstintuion_Edit != null)
                {
                    institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                    institutionId = this.SelectedInstintuion_Edit.idbkey;
                }
                else
                {
                    institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                    institutionName = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionName : string.Empty;
                }
                if (institutionId == item.InstitutionId && institutionName == item.InstitutionName)
                {
                    return;
                }

                //// 联动检查客户名
                //if (!string.IsNullOrEmpty(institutionId) 
                //        && !string.IsNullOrEmpty(item.TraderId))
                //{
                //    List<CrmContact> CrmContactList = CommonUtils.GetContactidInfoPo(item.InstitutionId);
                //    if (!CrmContactList.Any(m=>m.ContactId == item.TraderId))
                //    {
                //        traderName = string.Empty;
                //        traderId = string.Empty;
                //    }
                //}
            }

            // 客户名
            if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
            {
                if (this.SelectedTrader_Edit != null)
                {
                    traderName = this.SelectedTrader_Edit.namecn;
                    traderId = this.SelectedTrader_Edit.idbkey;
                    //item.QqNumber = item.SelectedTrader_Edit.QQ;
                }
                else
                {
                    traderName = this.TraderName_Edit;
                    traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                }
                if (traderId == item.TraderId && traderName == item.TraderName)
                {
                    return;
                }
            }

            // 方向
            if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
            {
                side = item.Side_Edit;
                if (side == item.Side)
                    return;
            }
            // 价格
            if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
            {
                price = item.Price_Edit;
                if (price == item.Price)
                    return;
            }
            // 数量
            if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
            {
                qty = item.Qty_Edit;
                if (qty == item.Qty)
                    return;
            }
            // 清算速度
            if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
            {
                //DateTime createtime = CommonUtils.ConvertToDateTime(item.CreateTime);
                //settlType = CommonUtils.IsSpotSettle(createtime, item.edit_settltype_date, item.edit_settletype);
                //if (string.IsNullOrEmpty(settlType))
                //{
                //    settlType = item.edit_settltype_date.ToString("MM.dd") + (item.edit_settletype == 0 ? "+0" : "+1");
                //}
                settlType = item.SettlType_Edit;
                if (settlType == item.SettlType)
                    return;
            }

            // 新更新
            //UpdateIntentionQuoteRequest request = new UpdateIntentionQuoteRequest();
            //request.intentionId = item.IntentionId;
            //request.institutionId = institutionId;
            //request.institutionName = institutionName;
            //request.traderId = traderId;
            //request.traderName = traderName;
            //request.side = side;
            //request.price = price;
            //request.qty = qty;
            //request.settlType = settlType;
            //bool isSuccess = await IntentionHandler.Instance.UpdateIntentionQuote(request);
            //if (isSuccess)
            //{
                // 客户机构
                if (columnHeader == null || columnHeader == COLUMNNAME_INSTITUTION)
                {
                    item.InstitutionName = institutionName;
                    item.InstitutionId = institutionId;
                }
                // 客户名
                if (columnHeader == null || columnHeader == COLUMNNAME_TRADER)
                {
                    item.TraderName = traderName;
                    item.TraderId = traderId;
                }
                // 方向
                if (columnHeader == null || columnHeader == COLUMNNAME_SIDE)
                    item.Side = side;
                // 价格
                if (columnHeader == null || columnHeader == COLUMNNAME_PRICE)
                    item.Price = price;
                // 数量
                if (columnHeader == null || columnHeader == COLUMNNAME_QTY)
                    item.Qty = qty;
                // 清算速度
                if (columnHeader == null || columnHeader == COLUMNNAME_SETTLETYPE)
                    item.SettlType = settlType;

                //if (string.IsNullOrEmpty(item.OrderID) == false)
                //{
                //    // 更新我的订单/我的撤单
                //    MyOrderProcesser.Processer.UpdateCustomInfo(item.IntentionId,
                //       item.InstitutionId, item.InstitutionName,
                //       item.TraderId, item.TraderName
                //       );
                //    // 更新我的成交
                //    MyExecutionProcesser.Processer.UpdateCustomInfo(item.IntentionId,
                //        item.InstitutionId, item.InstitutionName,
                //        item.TraderId, item.TraderName
                //        );
                //}
            //}
        }

        public void DeleteOrder()
        {
            if (this.SelectedItem_Recognize != null)
            {
                this.intentionQuoteInfo.Remove(this.SelectedItem_Recognize);
            }
        }

        public async void UpdateOrder()
        {
            if (this.ViewSource.View.IsEmpty == false)
            {
                InsertIntentionQuoteReq request = new InsertIntentionQuoteReq();
                List<IntentionQuoteInfo> quoteList = new List<IntentionQuoteInfo>();
                request.quoteList = quoteList;
                foreach (IntentionQuoteItem item in this.ViewSource.View)
                {
                    IntentionQuoteInfo record = new IntentionQuoteInfo();
                    record.intentionId = item.IntentionId;
                    // 机构编号
                    string institutionId = null;
                    // 机构名称
                    string institutionName = null;
                    if (this.SelectedInstintuion_Edit != null)
                    {
                        institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                        institutionId = this.SelectedInstintuion_Edit.idbkey;
                    }
                    else
                    {
                        institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                        institutionName = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionName : string.Empty;
                    }
                    record.institutionId = institutionId;
                    record.institutionName = institutionName;
                    // 交易员编号
                    string traderId = null;
                    // 交易员名称
                    string traderName = null;
                    if (this.SelectedTrader_Edit != null)
                    {
                        traderName = this.SelectedTrader_Edit.namecn;
                        traderId = this.SelectedTrader_Edit.idbkey;
                        //item.QqNumber = item.SelectedTrader_Edit.QQ;
                    }
                    else
                    {
                        traderName = this.TraderName_Edit;
                        traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                    }
                    record.traderId = traderId;
                    record.traderName = traderName;
                    record.side = item.Side;
                    record.securityId = item.SecurityId;
                    record.symbol = item.Symbol;
                    record.price = item.Price;
                    record.qty = item.Qty;
                    record.settlType = item.SettlType;
                    record.text = item.Text;
                    record.userName = item.UserName;
                    //record.closeBy = item.CloseBy;
                    record.status = item.Status;
                    //record.brokerId = item.BrokerId;
                    //record.brokerName = item.BrokerName;
                    record.qqNumber = item.QqNumber;
                    record.createTime = item.CreateTime;
                    record.updateTime = item.UpdateTime;
                    //record.fullPrice = item.FullPrice;
                    record.netPrice = item.NetPrice;
                    record.userName = LocalEnv.UserInfo.UserName;
                    record.nickName = LocalEnv.UserInfo.FullName;

                    quoteList.Add(record);
                }
                bool isSucces = await IntentionHandler.Instance.InsertIntentionQuote(request);
                //if (isSucces)
                //{
                //    foreach(IntentionQuoteItem item in this.viewSource.View)
                //    {
                //        IntentionQuoteInfo obj = new IntentionQuoteInfo();
                //        // 意向报价唯一编号
                //        obj.intentionId = item.IntentionId;
                //        // 机构编号
                //        string institutionId = null;
                //        // 机构名称
                //        string institutionName = null;
                //        if (this.SelectedInstintuion_Edit != null)
                //        {
                //            institutionName = this.SelectedInstintuion_Edit.shortnamecn;
                //            institutionId = this.SelectedInstintuion_Edit.idbkey;
                //        }
                //        else
                //        {
                //            institutionId = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionId : string.Empty;
                //            institutionName = this.InstitutionName_Edit == item.InstitutionName ? item.InstitutionName : string.Empty;
                //        }
                //        // 机构编号
                //        obj.institutionId = institutionId;
                //        // 机构名称
                //        obj.institutionName = institutionName;
                //        // 交易员编号
                //        string traderId = null;
                //        // 交易员名称
                //        string traderName = null;
                //        if (this.SelectedTrader_Edit != null)
                //        {
                //            traderName = this.SelectedTrader_Edit.namecn;
                //            traderId = this.SelectedTrader_Edit.idbkey;
                //            //item.QqNumber = item.SelectedTrader_Edit.QQ;
                //        }
                //        else
                //        {
                //            traderName = this.TraderName_Edit;
                //            traderId = this.TraderName_Edit == item.TraderName ? item.TraderId : string.Empty;
                //        }
                //        // 交易员编号
                //        obj.traderId = traderId;
                //        // 交易员名称
                //        obj.traderName = traderName;
                //        // 经纪商编号
                //        obj.brokerId = item.BrokerId;
                //        // 经纪商名称
                //        obj.brokerName = item.BrokerName;
                //        // QQ号码
                //        obj.qqNumber = item.QqNumber;
                //        // 债券编号
                //        obj.securityId = item.SecurityId;
                //        // 债券简称
                //        obj.symbol = item.Symbol;
                //        // 报价方向 1=买、2=卖
                //        obj.side = item.Side;
                //        // 价格
                //        obj.price = item.Price;
                //        // 净价
                //        obj.cleanPrice = item.CleanPrice;
                //        // 全价
                //        obj.fullPrice = item.FullPrice;
                //        // 报量
                //        obj.qty = item.Qty;
                //        // 清算类型 1=T+0, 2=T+1, 3=Tm+0, 4=Tm+1, 99=其他
                //        obj.settlType = item.SettlType;
                //        // 原始文本 
                //        obj.text = item.Text;
                //        // 订单状态 ,0=待处理,1=已处理,2=已成交, 3=已忽略
                //        obj.status = item.Status;
                //        // 创建时间
                //        obj.createTime = item.CreateTime;
                //        // 更新时间
                //        obj.updateTime = item.UpdateTime;
                //        // 操作人
                //        obj.closeBy = item.CloseBy;
                //        // 用户名
                //        obj.userName = item.UserName;
                //        IntentionHandler.Instance.Instance_OnIntention(obj);
                //    }
                //}
            }
        }

        internal void FilterInstitution(AutoCompleteArgs args)
        {

            //BankListViewSource.View.Filter = x =>
            //{
            //    if (string.IsNullOrEmpty(args.Pattern))
            //        return true;
            //    else
            //        return ((KeyValueData)x).Key.Contains(args.Pattern) || ((KeyValueData)x).Value.Contains(args.Pattern);
            //};
            //args.DataSource = BankListViewSource.View;
            string currentTraderID = this.SelectedTrader_Edit == null ? string.Empty : this.SelectedTrader_Edit.idbkey;

            List<string> instIdList = null;
            if (!string.IsNullOrEmpty(currentTraderID))
            {
                List<CrmContact> crList = WtUtils.GetContacts(null, currentTraderID);
                
                if (crList != null && crList.Count > 0)
                {
                    instIdList = new List<string>();
                    foreach (CrmContact crItem in crList)
                    {
                        if (!instIdList.Contains(crItem.instid))
                        {
                            instIdList.Add(crItem.instid);
                        }
                    }
                }
            }

            List<CrmInst> result = new List<CrmInst>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {

                        List<CrmInst> selectDatas = WtUtils.GetInstitutionsInfoPo(str, 20, instIdList);
                        result.AddRange(selectDatas);
                    }
                }
                else
                {
                    result = WtUtils.GetInstitutionsInfoPo(args.Pattern.Trim(), 20, instIdList);
                }
            }
            else
            {
                result.AddRange(WtUtils.GetInstitutionsInfoPo(instIdList));
            }
            args.DataSource = result;
        }

        internal void FilterTrader(AutoCompleteArgs args)
        {
            string currentInstID = this._selectedInstitution_Edit == null ? string.Empty : this._selectedInstitution_Edit.idbkey;
            
            List<string> instList = new List<string>();
            if (!string.IsNullOrEmpty(currentInstID))
            {
                instList.Add(currentInstID);
            }

            List<CrmContact> result = new List<CrmContact>();
            if (!string.IsNullOrEmpty(args.Pattern))
            {

                string txt = args.Pattern.Replace("'", "");
                if (txt.Contains(" "))
                {
                    string[] arr = txt.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string str in arr)
                    {
                        List<CrmContact> selectDatas = WtUtils.GetContactInfoPo(str, instList);
                        result.AddRange(selectDatas);
                    }

                }
                else
                    result = WtUtils.GetContactInfoPo(args.Pattern.Trim(), instList);
            }
            else
            {
                result.AddRange(WtUtils.GetContacts(currentInstID));
            }
            args.DataSource = result;
        }
    }
}
