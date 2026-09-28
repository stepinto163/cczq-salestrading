using IDC.Matching.Plugin.Utils;
using STC.Common.Handler.TBook;
using STC.Common.Model.TBook;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using STC.Frame.UI;
using STC.Common.Model.Base;
using STC.Common.Model.ODM;
using STC.Common.Model.Fix.Message;
using System.Threading;

namespace STC.XBond.Plugin.Setting.ViewModel
{
    public class XBondSubViewModel : ViewModelBase
    {
        private object lockObj = new object();
        /// <summary>
        /// 清算类型字典
        /// </summary>
        private Dictionary<string, string> settlTypeDic = new Dictionary<string, string>();
        /// <summary>
        /// 订阅债券任务队列
        /// </summary>
        private ThreadQueue<StcTbookMdSecurityids> _ProcessMdSecurityidsThreadQueue;
        /// <summary>
        /// 过滤条件
        /// </summary>
        private StringBuilder filterSB = new StringBuilder();
        #region 表格字段
        private const string KEY_FIELD = "Key";
        private const string SECURITY_ID_FIELD = "SecurityID";
        private const string SYMBOL_FIELD = "symbol";
        private const string SECURITY_TYPE_FIELD = "SecurityType";
        private const string SECURITY_TYPE_DESC_FIELD = "SecurityTypeDesc";
        private const string SETTL_TYPE_FIELD = "SettlType";
        private const string CREATETIME_FIELD = "CreateTime";
        private const string VENUE_TYPEGW_FIELD = "VenueTypeGW";
        private const string SETTL_TYPE_ENABLE_FIELD = "SettlTypeIsEdit";
        private const string EDIT_BTN_VISIBLE_FIED = "EditBtnVisible";
        private const string SAVE_BTN_VISIBLE_FIED = "SaveBtnVisible";
        private const string SETTL_TYPE_INDEX_FIELD = "SettlTypeIndex";
        private const string SETTL_TYPE_TEXT_FIELD = "SettlTypeText";
        private const string SECURITY_ID_ENABLE_FIELD = "SecurityIDIsEdit";
        #endregion
        private DataView _MDBondView;
        public DataView MDBondView
        {
            get
            {
                lock (lockObj)
                {
                    if (_MDBondView == null)
                    {
                        _MDBondView = new DataTable().DefaultView;
                        _MDBondView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(SYMBOL_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(SECURITY_TYPE_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(SECURITY_TYPE_DESC_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(SETTL_TYPE_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(CREATETIME_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(VENUE_TYPEGW_FIELD, typeof(string));
                        
                        _MDBondView.Table.Columns.Add(SETTL_TYPE_ENABLE_FIELD,typeof(string));
                        _MDBondView.Table.Columns.Add(EDIT_BTN_VISIBLE_FIED, typeof(string));
                        _MDBondView.Table.Columns.Add(SAVE_BTN_VISIBLE_FIED, typeof(string));
                        _MDBondView.Table.Columns.Add(SETTL_TYPE_INDEX_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(SETTL_TYPE_TEXT_FIELD, typeof(string));
                        _MDBondView.Table.Columns.Add(SECURITY_ID_ENABLE_FIELD, typeof(string));

                        //_MDBondView.Sort = $"{CREATETIME_FIELD} desc";
                        _MDBondView.Table.PrimaryKey = new DataColumn[] {
                            _MDBondView.Table.Columns[KEY_FIELD]
                        };
                        this.OnPropertyChanged();
                    }
                    return this._MDBondView;
                }
            }
        }
        /// <summary>
        /// 债券查询条件
        /// </summary>
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
        /// <summary>
        /// 总数量显示文本
        /// </summary>
        private string _totalText;

        public string TotalText
        {
            get
            {
                return _totalText;
            }

            set
            {
                _totalText = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 债券数量
        /// </summary>
        private int totalNum = 0;
        /// <summary>
        /// 订阅数量
        /// </summary>
        private int totalSubNum = 0;
        public XBondSubViewModel()
        {
            AddMdSecurityidsCommand = new RelayCommand(AddBond);
            DelMdSecurityidsCommand = new RelayCommand(DelBond);
            EditMdSecurityidsCommand = new RelayCommand(EditBond);
            SaveMdSecurityidsCommand = new RelayCommand(SaveBond);
            CancelMdSecurityidsCommand = new RelayCommand(CancelBond);
            //settleTypeDic.Add("0","T+0");
            //初始化清算类型数据
            settlTypeDic.Add("0", "T+1");
            settlTypeDic.Add("1", "T+0,T+1");
        }

        public void init() {
            //TBookHandler.Instance.Init(null, Instance_OnBondList);
            _ProcessMdSecurityidsThreadQueue = new ThreadQueue<StcTbookMdSecurityids>(UpdateMdSecurityidsAsync);
            ThreadPool.QueueUserWorkItem(o =>
            {
                selectSecurityids();
            });
            //Instance_OnMdSecurityidsList(MdSecurityidsProcesser.Processer.MdSecurityidsList);
        }

        private void selectSecurityids()
        {
            MdSecurityIdReq req = new MdSecurityIdReq();
            req.venueTypeGW = "ODM";
            MdSecurityidsReplyPo po = TBookHandler.Instance.queryMDSecurityId(req);
            if (po != null)
            {
                //TotalText = "共0只债券，0条订阅";
                totalNum = po.datas.Count;
                totalSubNum = 0;
                Instance_OnMdSecurityidsList(po.datas);
            }
            else
            {
                totalNum = 0;
                totalSubNum = 0;
                TotalText = "共0只债券，0条订阅";
            }
        }

        /// <summary>
        /// 收取Bond更新数据
        /// </summary>
        /// <param name="list"></param>
        private void Instance_OnMdSecurityidsList(List<StcTbookMdSecurityids> list)
        {
            lock (MDBondView)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    long start = DateTime.Now.Ticks;
                    foreach (StcTbookMdSecurityids po in list)
                    {
                        if (po != null)
                        {
                            UpdateMdSecurityids(po);
                        }
                    }
                    long end = DateTime.Now.Ticks;
                    Console.WriteLine("init time :" + (end - start) / 10000000.0);
                }));
            }
        }
        private void UpdateMdSecurityidsAsync(StcTbookMdSecurityids obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateMdSecurityids(obj);
            }));
        }
        /// <summary>
        /// 更新债券列表
        /// </summary>
        /// <param name="bond"></param>
        private void UpdateMdSecurityids(StcTbookMdSecurityids obj)
        {
            try
            {
                lock (lockObj)
                {
                    long start = DateTime.Now.Ticks;
                    DataRow Items = this.MDBondView.Table.Rows.Find(new object[] { obj.Securityid});
                    if (Items == null)
                    {
                        string settleTypeText = "";
                        Items = this.MDBondView.Table.NewRow();
                        // OpenClsoe
                        Items[KEY_FIELD] = obj.Securityid;
                        Items[SETTL_TYPE_FIELD] = obj.Settltype;
                        if (null == obj.Settltype || "".Equals(obj.Settltype))
                        {
                            Items[SETTL_TYPE_INDEX_FIELD] = "1";
                            totalSubNum = totalSubNum + 2;

                            if (settlTypeDic.TryGetValue("1", out settleTypeText))
                            {
                                Items[SETTL_TYPE_TEXT_FIELD] = settleTypeText; // 清算速度
                            }
                        }
                        else if ("2".Equals(obj.Settltype))
                        {
                            Items[SETTL_TYPE_INDEX_FIELD] = "0";
                            totalSubNum++;
                            if (settlTypeDic.TryGetValue("0", out settleTypeText))
                            {
                                Items[SETTL_TYPE_TEXT_FIELD] = settleTypeText; // 清算速度
                            }
                        }
                        Items[SECURITY_ID_FIELD] = obj.Securityid; // 代码
                        Items[VENUE_TYPEGW_FIELD] = obj.Venuetypegw;
                        Items[CREATETIME_FIELD] = obj.Createtime;
                        Items[SYMBOL_FIELD] = obj.Symbol;
                        Items[SETTL_TYPE_ENABLE_FIELD] = "False";
                        Items[EDIT_BTN_VISIBLE_FIED] = "0";
                        Items[SAVE_BTN_VISIBLE_FIED] = "1";
                        Items[SECURITY_ID_ENABLE_FIELD] = "False";
                        this.MDBondView.Table.Rows.Add(Items);
                    }
                    else
                    {
                        string settleTypeText = "";
                        if (null == obj.Settltype || "".Equals(obj.Settltype))
                        {
                            Items[SETTL_TYPE_INDEX_FIELD] = "1";
                            totalSubNum = totalSubNum + 2;
                            if (settlTypeDic.TryGetValue("1", out settleTypeText))
                            {
                                Items[SETTL_TYPE_TEXT_FIELD] = settleTypeText; // 清算速度
                            }
                        }
                        else if ("2".Equals(obj.Settltype))
                        {
                            Items[SETTL_TYPE_INDEX_FIELD] = "0";
                            totalSubNum++;
                            if (settlTypeDic.TryGetValue("0", out settleTypeText))
                            {
                                Items[SETTL_TYPE_TEXT_FIELD] = settleTypeText; // 清算速度
                            }
                        }
                        Items[CREATETIME_FIELD] = obj.Createtime;
                        Items[SETTL_TYPE_ENABLE_FIELD] = "False";
                        Items[EDIT_BTN_VISIBLE_FIED] = "0";
                        Items[SAVE_BTN_VISIBLE_FIED] = "1";
                        Items[SECURITY_ID_ENABLE_FIELD] = "False";
                    }
                    TotalText = string.Format($"共{totalNum}只债券，{totalSubNum}条订阅");
                    long end = DateTime.Now.Ticks;
                    Console.WriteLine("Add time :"+(end-start) / 10000000.0);
                    this.MDBondView.Table.AcceptChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        /// <summary>
        /// 过滤数据
        /// </summary>
        public void FilterData()
        {
            // 初始化过滤条件
            filterSB.Remove(0, filterSB.Length);

            // 添加债券代码过滤
            #region 筛选债券代码
            if (this.SearchText.Length > 0)
            {
                this.filterSB.Append($"{SECURITY_ID_FIELD} like '%{this.SearchText}%'");
            }
            #endregion
            this.MDBondView.RowFilter = filterSB.ToString();
        }
        /// <summary>
        /// 新增
        /// </summary>
        public RelayCommand AddMdSecurityidsCommand { get; set; }
        /// <summary>
        /// 删除
        /// </summary>
        public RelayCommand DelMdSecurityidsCommand { get; set; }
        /// <summary>
        /// 编辑
        /// </summary>
        public RelayCommand EditMdSecurityidsCommand { get; set; }
        /// <summary>
        /// 保存按钮
        /// </summary>
        public RelayCommand SaveMdSecurityidsCommand { get; set; }
        /// <summary>
        /// 取消按钮
        /// </summary>
        public RelayCommand CancelMdSecurityidsCommand { get; set; }
        private bool addStatus = false;
        /// <summary>
        /// 新增行
        /// </summary>
        private void AddBond()
        {
            try
            {
                lock (lockObj)
                {
                    if (!addStatus)
                    {
                        addStatus = true;
                        DataRow bondInfosItems = this.MDBondView.Table.Rows.Find(new object[] { "" });
                        bondInfosItems = this.MDBondView.Table.NewRow();
                        bondInfosItems[KEY_FIELD] = "";
                        bondInfosItems[SETTL_TYPE_INDEX_FIELD] = "-1";
                        bondInfosItems[VENUE_TYPEGW_FIELD] = "ODM";
                        bondInfosItems[SETTL_TYPE_ENABLE_FIELD] = "True";
                        bondInfosItems[EDIT_BTN_VISIBLE_FIED] = "1";
                        bondInfosItems[SAVE_BTN_VISIBLE_FIED] = "0";
                        bondInfosItems[SECURITY_ID_ENABLE_FIELD] = "True";
                        this.MDBondView.Table.Rows.Add(bondInfosItems);
                        this.MDBondView.Table.AcceptChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
        /// <summary>
        /// 删除行
        /// </summary>
        private void DelBond()
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
                List<MdSecurityIdReq> reqList = new List<MdSecurityIdReq>();
                MdSecurityIdReq req = new MdSecurityIdReq();
                req.securityID = drv[SECURITY_ID_FIELD] as string;
                req.venueTypeGW = drv[VENUE_TYPEGW_FIELD] as string;
                string settlTypeIndex = drv[SETTL_TYPE_INDEX_FIELD] as string;
                string settlType = "";
                if (!"".Equals(settlTypeIndex) && settlTypeDic.TryGetValue(settlTypeIndex, out settlType))
                {
                    if ("0".Equals(settlTypeIndex))
                        req.settlType = "2";
                }
                req.marketIndicator = "4";
                req.subscriptionRequestType = "2";
                reqList.Add(req);
                PResult reply = TBookHandler.Instance.XBondMDSub(reqList);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        drv[KEY_FIELD] = drv[SECURITY_ID_FIELD];
                        addStatus = false;
                        removeRow(key);
                        //selectSecurityids();
                        //showMessage("订阅成功");
                    }
                    else
                    {
                        addStatus = true;
                        showMessage(string.Format($"删除失败, 原因:{reply.Msg}."));
                    }
                }
            }
        }
        /// <summary>
        /// 编辑行
        /// </summary>
        private void EditBond()
        {
            DataRowView drv = this.SelectedItem;
            if (!addStatus)
            {
                addStatus = true;
                setStatus(drv);
            }
        }

        /// <summary>
        /// 取消行编辑
        /// </summary>
        private void CancelBond()
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
                //drv[SETTL_TYPE_FIELD] = settlTypeDic.FirstOrDefault(a => a.Value == (drv[SETTL_TYPE_TEXT_FIELD] as string)).Key;
                drv[SETTL_TYPE_INDEX_FIELD] = settlTypeDic.FirstOrDefault(a => a.Value == (drv[SETTL_TYPE_TEXT_FIELD] as string)).Key;
                setStatus(drv);
            }
        }
        /// <summary>
        /// 根据债券id移除数据
        /// </summary>
        /// <param name="key"></param>
        private void removeRow(string key)
        {
            DataRow bondInfosItems = this.MDBondView.Table.Rows.Find(new object[] { key });
            if (!"".Equals(key))
            {
                totalNum--;
                if ("T+1".Equals(bondInfosItems[SETTL_TYPE_TEXT_FIELD] as string))
                {
                    totalSubNum = totalSubNum - 1;
                }
                else
                {
                    totalSubNum = totalSubNum - 2;
                }
            }
            this.MDBondView.Table.Rows.Remove(bondInfosItems);
            this.MDBondView.Table.AcceptChanges();
            TotalText = string.Format($"共{totalNum}只债券，{totalSubNum}条订阅");
        }
        /// <summary>
        /// 根据债券id查询债券基本信息
        /// </summary>
        /// <param name="securityid"></param>
        /// <returns></returns>
        private BondsInfoPo getBondsInfo(String securityid) {
            List<BondsInfoPo> result = new List<BondsInfoPo>();
            result = CommonUtils.GetODMBondsInfoPo(securityid, 1);
            if (result.Count > 0)
            {
                return result.ToArray()[0];
            }
            return null;
        }
        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveBond()
        {
            DataRowView drv = this.SelectedItem;

            string securityid = drv[SECURITY_ID_FIELD] as string;
            string key = drv[KEY_FIELD] as string;
            string settlTypeIndex = drv[SETTL_TYPE_INDEX_FIELD] as string;
            BondsInfoPo bondsInfo = getBondsInfo(securityid);
            #region 更新检查
            if (null == bondsInfo)
            {
                showMessage(string.Format($"债券不存在:{securityid}."));
                return;
            }
            if (int.Parse(settlTypeIndex) < 0)
            {
                showMessage(string.Format($"请选择清算类型."));
                return;
            }
            DataRow Items = this.MDBondView.Table.Rows.Find(new object[] { securityid });
            if (null != Items)
            {
                if ("".Equals(key))
                {
                    showMessage(string.Format($"债券({securityid})已在列表中."));
                    return;
                }
            }
            #endregion
            List<MdSecurityIdReq> reqList = new List<MdSecurityIdReq>();
            MdSecurityIdReq req = new MdSecurityIdReq();
            req.securityID = drv[SECURITY_ID_FIELD] as string;
            req.venueTypeGW = drv[VENUE_TYPEGW_FIELD] as string;
            string settlType = "";
            if (!"".Equals(settlTypeIndex) && settlTypeDic.TryGetValue(settlTypeIndex, out settlType))
            {
                if ("0".Equals(settlTypeIndex))
                    req.settlType = "2";
            }
            req.marketIndicator = "4";
            req.subscriptionRequestType = "1";
            req.symbol = bondsInfo.Short_Name;
            drv[SYMBOL_FIELD] = bondsInfo.Short_Name;
            drv[SECURITY_TYPE_FIELD] = bondsInfo.SECURITY_TYPE;
            drv[SECURITY_TYPE_DESC_FIELD] = bondsInfo.Security_Dec;
            reqList.Add(req);
            PResult reply = TBookHandler.Instance.XBondMDSub(reqList);
            if (reply != null)
            {
                if (reply.Code == "0000")
                {
                    drv[KEY_FIELD] = drv[SECURITY_ID_FIELD];
                    addStatus = false;
                    selectSecurityids();
                    //showMessage("订阅成功");
                }
                else
                {
                    addStatus = true;
                    showMessage(string.Format($"更新失败, 原因:{reply.Msg}."));
                }
            }
        }

        private void setStatus(DataRowView drv)
        {
            drv[SETTL_TYPE_ENABLE_FIELD] = drv[SETTL_TYPE_ENABLE_FIELD].Equals("True") ? "False" : "True";
            drv[EDIT_BTN_VISIBLE_FIED] = drv[EDIT_BTN_VISIBLE_FIED].Equals("1") ? "0" : "1";
            drv[SAVE_BTN_VISIBLE_FIED] = drv[SAVE_BTN_VISIBLE_FIED].Equals("1") ? "0" : "1";
            drv[SECURITY_ID_ENABLE_FIELD] = drv[SECURITY_ID_ENABLE_FIELD].Equals("True") ? "False" : "True";
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
