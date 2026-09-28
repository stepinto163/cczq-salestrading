using IDC.Matching.Plugin.Utils;
using STC.Common.Handler.TBook;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Risk;
using STC.Frame.UI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public partial class RiskDesiViewModel : ViewModelBase
    {
        private object lockObj = new object();
        /// <summary>
        /// 订阅债券任务队列
        /// </summary>
        private ThreadQueue<AppointRiskParameter> _ProcessThreadQueue;
        /// <summary>
        /// 编辑
        /// </summary>
        public RelayCommand EditCommand { get; set; }
        /// <summary>
        /// 保存按钮
        /// </summary>
        public RelayCommand SaveCommand { get; set; }
        /// <summary>
        /// 取消按钮
        /// </summary>
        public RelayCommand CancelCommand { get; set; }
        /// <summary>                                                                 
        /// 新增
        /// </summary>
        public RelayCommand AddCommand { get; set; }
        /// <summary>
        /// 删除
        /// </summary>
        public RelayCommand DelCommand { get; set; }
        private bool addStatus = false;
        #region 表格字段
        private const string KEY_FIELD = "Key";
        private const string SECURITY_ID_FIELD = "SecurityID";
        private const string SYMBOL_FIELD = "Symbol";
        private const string SECURITY_TYPE_FIELD = "SecurityType";
        private const string SECURITY_TYPE_DESC_FIELD = "SecurityTypeDesc";
        private const string QTY_MIN_FIELD = "QtyMin";
        private const string QTY_MIN_TEXT_FIELD = "QtyMinText";
        private const string QTY_MAX_FIELD = "QtyMax";
        private const string QTY_MAX_TEXT_FIELD = "QtyMaxText";
        private const string BID_FIELD = "Bid";
        private const string BID_TEXT_FIELD = "BidText";
        private const string OFR_FIELD = "Ofr";
        private const string OFR_TEXT_FIELD = "OfrText";
        private const string DROP_AWAY_FROM_BID_FIELD = "DropAwayFromBid";
        private const string DROP_AWAY_FROM_BID_TEXT_FIELD = "DropAwayFromBidText";
        private const string DROP_AWAY_FROM_OFR_FIELD = "DropAwayFromOfr";
        private const string DROP_AWAY_FROM_OFR_TEXT_FIELD = "DropAwayFromOfrText";
        private const string MARKET_BID_FIELD = "MarketBid";
        private const string MARKET_BID_TEXT_FIELD = "MarketBidText";
        private const string MARKET_OFR_FIELD = "MarketOfr";
        private const string MARKET_OFR_TEXT_FIELD = "MarketOfrText";
        private const string UPDATE_TIME_FIELD = "UpdateTime";

        private const string EDIT_ENABLE_FIELD = "EditEnable";
        private const string EDIT_BTN_VISIBLE_FIED = "EditBtnVisible";
        private const string SAVE_BTN_VISIBLE_FIED = "SaveBtnVisible";

        #endregion
        private DataView _RiskParaView;

        public DataView RiskParaView
        {
            get
            {
                lock (lockObj)
                {
                    if (_RiskParaView == null)
                    {
                        _RiskParaView = new DataTable().DefaultView;
                        _RiskParaView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(SECURITY_TYPE_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(SECURITY_TYPE_DESC_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(QTY_MIN_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(QTY_MAX_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(BID_FIELD, typeof(string));

                        _RiskParaView.Table.Columns.Add(OFR_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(DROP_AWAY_FROM_BID_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(DROP_AWAY_FROM_OFR_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(MARKET_BID_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(MARKET_OFR_FIELD, typeof(string));

                        _RiskParaView.Table.Columns.Add(QTY_MIN_TEXT_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(QTY_MAX_TEXT_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(BID_TEXT_FIELD, typeof(string));

                        _RiskParaView.Table.Columns.Add(OFR_TEXT_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(DROP_AWAY_FROM_BID_TEXT_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(DROP_AWAY_FROM_OFR_TEXT_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(MARKET_BID_TEXT_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(MARKET_OFR_TEXT_FIELD, typeof(string));

                        _RiskParaView.Table.Columns.Add(UPDATE_TIME_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(EDIT_ENABLE_FIELD, typeof(string));
                        _RiskParaView.Table.Columns.Add(EDIT_BTN_VISIBLE_FIED, typeof(string));
                        _RiskParaView.Table.Columns.Add(SAVE_BTN_VISIBLE_FIED, typeof(string));
                        _RiskParaView.Table.PrimaryKey = new DataColumn[] {
                            _RiskParaView.Table.Columns[KEY_FIELD]
                        };
                        this.OnPropertyChanged();
                    }
                    return this._RiskParaView;
                }
            }
        }
        public void setSymbol()
        {
            DataRowView drv = this.SelectedItem;
            string securityid = drv[SECURITY_ID_FIELD] as string;
            if (!"".Equals(securityid))
            {
                BondsInfoPo po = getBondsInfo(securityid);
                if (po != null)
                    drv[SYMBOL_FIELD] = po.Short_Name;
            }
        }
        // <summary>
        /// 选中的行数据
        /// </summary>
        private DataRowView _selectedItem;
        public DataRowView SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;
                this.OnPropertyChanged();
            }
        }
        public void init()
        {
            //TBookHandler.Instance.Init(null, Instance_OnBondList);
            AddCommand = new RelayCommand(AddRisk);
            DelCommand = new RelayCommand(DelRisk);
            EditCommand = new RelayCommand(EditRisk);
            SaveCommand = new RelayCommand(SaveRisk);
            CancelCommand = new RelayCommand(CancelRisk);
            _ProcessThreadQueue = new ThreadQueue<AppointRiskParameter>(UpdateAsync);
            
           selectRisk();
        }

        private void UpdateAsync(AppointRiskParameter obj)
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
                UpdateRisk(obj);
            //}));
        }

        private void UpdateRisk(AppointRiskParameter obj)
        {
            try
            {
                lock (lockObj)
                {
                    DataRow Items = this.RiskParaView.Table.Rows.Find(new object[] { obj.securityId });
                    if (Items == null)
                    {
                        Items = this._RiskParaView.Table.NewRow();
                        Items[KEY_FIELD] = obj.securityId;
                        Items[SECURITY_ID_FIELD] = obj.securityId; 
                        Items[SYMBOL_FIELD] = obj.shortName;
                        Items[QTY_MIN_FIELD] = obj.qtyMin;
                        Items[QTY_MAX_FIELD] = obj.qtyMax;
                        Items[BID_FIELD] = obj.bid;
                        Items[OFR_FIELD] = obj.ofr;
                        Items[DROP_AWAY_FROM_BID_FIELD] = obj.dropAwayFromBid;
                        Items[DROP_AWAY_FROM_OFR_FIELD] = obj.dropAwayFromOfr;
                        Items[MARKET_BID_FIELD] = obj.marketBid;
                        Items[MARKET_OFR_FIELD] = obj.marketOfr;

                        Items[QTY_MIN_TEXT_FIELD] = obj.qtyMin;
                        Items[QTY_MAX_TEXT_FIELD] = obj.qtyMax;
                        Items[BID_TEXT_FIELD] = obj.bid;
                        Items[OFR_TEXT_FIELD] = obj.ofr;
                        Items[DROP_AWAY_FROM_BID_TEXT_FIELD] = obj.dropAwayFromBid;
                        Items[DROP_AWAY_FROM_OFR_TEXT_FIELD] = obj.dropAwayFromOfr;
                        Items[MARKET_BID_TEXT_FIELD] = obj.marketBid;
                        Items[MARKET_OFR_TEXT_FIELD] = obj.marketOfr;
                        Items[UPDATE_TIME_FIELD] = obj.update;
                        Items[EDIT_ENABLE_FIELD] = "False";
                        Items[EDIT_BTN_VISIBLE_FIED] = "0";
                        Items[SAVE_BTN_VISIBLE_FIED] = "1";
                        this._RiskParaView.Table.Rows.Add(Items);
                    }
                    else
                    {
                        Items[QTY_MIN_FIELD] = obj.qtyMin;
                        Items[QTY_MAX_FIELD] = obj.qtyMax;
                        Items[BID_FIELD] = obj.bid;
                        Items[OFR_FIELD] = obj.ofr;
                        Items[DROP_AWAY_FROM_BID_FIELD] = obj.dropAwayFromBid;
                        Items[DROP_AWAY_FROM_OFR_FIELD] = obj.dropAwayFromOfr;
                        Items[MARKET_BID_FIELD] = obj.marketBid;
                        Items[MARKET_OFR_FIELD] = obj.marketOfr;

                        Items[QTY_MIN_TEXT_FIELD] = obj.qtyMin;
                        Items[QTY_MAX_TEXT_FIELD] = obj.qtyMax;
                        Items[BID_TEXT_FIELD] = obj.bid;
                        Items[OFR_TEXT_FIELD] = obj.ofr;
                        Items[DROP_AWAY_FROM_BID_TEXT_FIELD] = obj.dropAwayFromBid;
                        Items[DROP_AWAY_FROM_OFR_TEXT_FIELD] = obj.dropAwayFromOfr;
                        Items[MARKET_BID_TEXT_FIELD] = obj.marketBid;
                        Items[MARKET_OFR_TEXT_FIELD] = obj.marketOfr;
                        Items[UPDATE_TIME_FIELD] = obj.update;
                        Items[EDIT_ENABLE_FIELD] = "False";
                        Items[EDIT_BTN_VISIBLE_FIED] = "0";
                        Items[SAVE_BTN_VISIBLE_FIED] = "1";
                    }
                    this._RiskParaView.Table.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        /// <summary>
        /// 根据债券id查询债券基本信息
        /// </summary>
        /// <param name="securityid"></param>
        /// <returns></returns>
        private BondsInfoPo getBondsInfo(String securityid)
        {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            result = CommonUtils.GetBondsInfoPo(securityid, 20);
            if (result.Count > 0)
            {
                return result.ToArray()[0];
            }
            return null;
        }
        private void selectRisk()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                RiskParaReq req = new RiskParaReq();
                RiskParaReplyPo po = TBookHandler.Instance.queryRiskPara(req);
                if (po != null)
                {
                    RiskParaReq rpp = po.datas;
                    if (rpp.appointRiskParameterList != null)
                        Instance_OnList(rpp.appointRiskParameterList);
                }
            });
        }

        private void Instance_OnList(List<AppointRiskParameter> list)
        {
            lock (lockObj)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (AppointRiskParameter po in list)
                    {
                        if (po != null && _ProcessThreadQueue != null)
                        {
                            UpdateRisk(po);
                        }
                    }
                }));
            }
        }

        private void DelRisk()
        {
            DataRowView drv = this.SelectedItem;
            string key = drv[KEY_FIELD] as string;
            if ("".Equals(key))
            {
                removeRow(key);
                return;
            }
            else
            {
                RiskParaReq req = new RiskParaReq();
                req.appointRiskParameterList = toPoDel(key);
                PResult reply = TBookHandler.Instance.updateRiskPara(req);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        drv[KEY_FIELD] = drv[SECURITY_ID_FIELD];
                        addStatus = false;
                        removeRow(key);
                    }
                    else
                    {
                        addStatus = true;
                        showMessage(string.Format($"删除失败, 原因:{reply.Msg}."));
                    }
                }
            }
        }

        private void AddRisk()
        {
            try
            {
                lock (lockObj)
                {
                    if (!addStatus)
                    {
                        addStatus = true;
                        DataRow bondInfosItems = this._RiskParaView.Table.Rows.Find(new object[] { "" });
                        bondInfosItems = this._RiskParaView.Table.NewRow();
                        bondInfosItems[KEY_FIELD] = "";
                        bondInfosItems[EDIT_BTN_VISIBLE_FIED] = "1";
                        bondInfosItems[SAVE_BTN_VISIBLE_FIED] = "0";
                        bondInfosItems[EDIT_ENABLE_FIELD] = "True";
                        this._RiskParaView.Table.Rows.Add(bondInfosItems);
                        this._RiskParaView.Table.AcceptChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void CancelRisk()
        {
            DataRowView drv = this.SelectedItem;
            string key = drv[KEY_FIELD] as string;
            addStatus = false;
            if ("".Equals(key))
            {
                removeRow(key);
            }
            else
            {
                drv[SECURITY_ID_FIELD] = drv[KEY_FIELD];
                setSymbol();
                drv[QTY_MIN_FIELD] = drv[QTY_MIN_TEXT_FIELD];
                drv[QTY_MAX_FIELD] = drv[QTY_MAX_TEXT_FIELD];
                drv[BID_FIELD] = drv[BID_TEXT_FIELD];
                drv[OFR_FIELD] = drv[OFR_TEXT_FIELD];
                drv[DROP_AWAY_FROM_BID_FIELD] = drv[DROP_AWAY_FROM_BID_TEXT_FIELD];
                drv[DROP_AWAY_FROM_OFR_FIELD] = drv[DROP_AWAY_FROM_OFR_TEXT_FIELD];
                drv[MARKET_BID_FIELD] = drv[MARKET_BID_TEXT_FIELD];
                drv[MARKET_OFR_FIELD] = drv[MARKET_OFR_TEXT_FIELD];
                setStatus(drv);
            }
        }

        private void SaveRisk()
        {
            DataRowView drv = this.SelectedItem;

            string securityid = drv[SECURITY_ID_FIELD] as string;
            string key = drv[KEY_FIELD] as string;
            BondsInfoPo bondsInfo = getBondsInfo(securityid);
            if (null == bondsInfo)
            {
                showMessage(string.Format($"债券不存在:{securityid}."));
                return;
            }
            DataRow Items = this.RiskParaView.Table.Rows.Find(new object[] { securityid });
            if (null != Items)
            {
                if ("".Equals(key))
                {
                    showMessage(string.Format($"债券({securityid})已在列表中."));
                    return;
                }
            }
            if (checkData(drv))
            {
                showMessage(string.Format($"{failureMsg}."));
                return;
            }
            RiskParaReq req = new RiskParaReq();
            drv[SYMBOL_FIELD] = bondsInfo.Short_Name;
            drv[SECURITY_TYPE_FIELD] = bondsInfo.SECURITY_TYPE;
            drv[SECURITY_TYPE_DESC_FIELD] = bondsInfo.Security_Dec;
            req.appointRiskParameterList = toPo(key);
            PResult reply = TBookHandler.Instance.updateRiskPara(req);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {
                    drv[QTY_MIN_TEXT_FIELD] = drv[QTY_MIN_FIELD];
                    drv[QTY_MAX_TEXT_FIELD] = drv[QTY_MAX_FIELD];
                    drv[BID_TEXT_FIELD] = drv[BID_FIELD];
                    drv[OFR_TEXT_FIELD] = drv[OFR_FIELD];
                    drv[DROP_AWAY_FROM_BID_TEXT_FIELD] = drv[DROP_AWAY_FROM_BID_FIELD];
                    drv[DROP_AWAY_FROM_OFR_TEXT_FIELD] = drv[DROP_AWAY_FROM_OFR_FIELD];
                    drv[MARKET_BID_TEXT_FIELD] = drv[MARKET_BID_FIELD];
                    drv[MARKET_OFR_TEXT_FIELD] = drv[MARKET_OFR_FIELD];
                    drv[KEY_FIELD] = drv[SECURITY_ID_FIELD];
                    addStatus = false;
                    setStatus(drv);
                    //showMessage("订阅成功");
                }
                else
                {
                    addStatus = true;
                    showMessage(string.Format($"更新失败, 原因:{reply.Msg}."));
                }
            }
        }
        private string failureMsg = "";
        private bool checkData(DataRowView drv)
        {
            bool status = false;
            failureMsg = "";
            string maxQtyText = drv[QTY_MAX_FIELD] as string;
            decimal maxQty = "".Equals(maxQtyText) ? 0: decimal.Parse(drv[QTY_MAX_FIELD] as string);
            if (maxQty % 1000 != 0)
            {
                failureMsg = "单笔报量上限非1000的整数倍";
                status = true;
            }
            string minQtyText = drv[QTY_MIN_FIELD] as string;
            decimal minQty = "".Equals(minQtyText) ? 0 : decimal.Parse(drv[QTY_MIN_FIELD] as string);
            if (minQty % 1000 != 0)
            {
                if (!"".Equals(failureMsg))
                {
                    failureMsg = failureMsg + ",单笔报量下限非1000的整数倍";
                }
                else
                {
                    failureMsg = "单笔报量下限非1000的整数倍";
                }
                status = true;
            }
            if (maxQty < minQty)
            {
                if (!"".Equals(failureMsg))
                {
                    failureMsg = failureMsg + ",单笔报量上限不能小于下限";
                }
                else
                {
                    failureMsg = "单笔报量上限不能小于下限";
                }
                status = true;
            }
            return status;
        }
        private List<AppointRiskParameter> toPo(string key)
        {
            List<AppointRiskParameter> list = new List<AppointRiskParameter>();
            foreach (DataRow drv in RiskParaView.Table.Rows)
            {
                AppointRiskParameter po = new AppointRiskParameter();
                po.securityId = drv[SECURITY_ID_FIELD] as string;
                po.shortName = drv[SYMBOL_FIELD] as string;
                po.securityType = drv[SECURITY_TYPE_FIELD] as string;
                po.qtyMin = drv[QTY_MIN_FIELD] as string;
                po.qtyMax = drv[QTY_MAX_FIELD] as string;
                po.bid = drv[BID_FIELD] as string;
                po.ofr = drv[OFR_FIELD] as string;
                po.dropAwayFromBid = drv[DROP_AWAY_FROM_BID_FIELD] as string;
                po.dropAwayFromOfr = drv[DROP_AWAY_FROM_OFR_FIELD] as string;
                po.marketBid = drv[MARKET_BID_FIELD] as string;
                po.marketOfr = drv[MARKET_OFR_FIELD] as string;
                if (po.securityId.Equals(key))
                {
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    drv[UPDATE_TIME_FIELD] = time;
                    po.update = time;
                }
                else
                {
                    po.update = drv[UPDATE_TIME_FIELD] as string;
                }
                list.Add(po);
            }
            return list;
        }
        private List<AppointRiskParameter> toPoDel(string key)
        {
            List<AppointRiskParameter> list = new List<AppointRiskParameter>();
            foreach (DataRow drv in RiskParaView.Table.Rows)
            {
                if ("".Equals(key) || !key.Equals(drv[KEY_FIELD] as string))
                {
                    AppointRiskParameter po = new AppointRiskParameter();
                    po.securityId = drv[SECURITY_ID_FIELD] as string;
                    po.shortName = drv[SYMBOL_FIELD] as string;
                    po.securityType = drv[SECURITY_TYPE_FIELD] as string;
                    po.qtyMin = drv[QTY_MIN_FIELD] as string;
                    po.qtyMax = drv[QTY_MAX_FIELD] as string;
                    po.bid = drv[BID_FIELD] as string;
                    po.ofr = drv[OFR_FIELD] as string;
                    po.dropAwayFromBid = drv[DROP_AWAY_FROM_BID_FIELD] as string;
                    po.dropAwayFromOfr = drv[DROP_AWAY_FROM_OFR_FIELD] as string;
                    po.marketBid = drv[MARKET_BID_FIELD] as string;
                    po.marketOfr = drv[MARKET_OFR_FIELD] as string;
                    po.update = drv[UPDATE_TIME_FIELD] as string;
                    list.Add(po);
                }
            }
            return list;
        }
        private void EditRisk()
        {
            DataRowView drv = this.SelectedItem;
            if (!addStatus)
            {
                addStatus = true;
                setStatus(drv);
            }
        }
        /// <summary>
        /// 提示消息弹框
        /// </summary>
        /// <param name="message"></param>
        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
        /// <summary>
        /// 根据债券id移除数据
        /// </summary>
        /// <param name="key"></param>
        private void removeRow(string key)
        {
            DataRow bondInfosItems = this.RiskParaView.Table.Rows.Find(new object[] { key });
            this.RiskParaView.Table.Rows.Remove(bondInfosItems);
            this.RiskParaView.Table.AcceptChanges();
        }
        private void setStatus(DataRowView drv)
        {
            drv[EDIT_ENABLE_FIELD] = drv[EDIT_ENABLE_FIELD].Equals("True") ? "False" : "True";
            drv[EDIT_BTN_VISIBLE_FIED] = drv[EDIT_BTN_VISIBLE_FIED].Equals("1") ? "0" : "1";
            drv[SAVE_BTN_VISIBLE_FIED] = drv[SAVE_BTN_VISIBLE_FIED].Equals("1") ? "0" : "1";
        }
        private Visibility _EditBtnVisible = Visibility.Collapsed;
        public Visibility EditBtnVisible
        {
            get
            {
                return _EditBtnVisible;
            }

            set
            {
                if (this._EditBtnVisible != value)
                {
                    this._EditBtnVisible = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private Visibility _SaveBtnVisible = Visibility.Collapsed;

        public Visibility SaveBtnVisible
        {
            get
            {
                return _SaveBtnVisible;
            }

            set
            {
                _SaveBtnVisible = value;
                this.OnPropertyChanged();
            }
        }

    }
}
