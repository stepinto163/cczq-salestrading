using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using System.Data;
using System.Windows.Input;
using System.Windows;
using STC.Common.Utils;
using System.Threading;
using System.Windows.Data;
using Ats.Foundation.Message;
using com.gateway.log;
using STC.quant.plugin.Utils.Data;
using STC.Common.Model.Quant;
using STC.Common.Handler.Quant;
using Ats.Foundation.Message.Serializer;
using STC.quant.plugin.Utils;
using STC.Common.View.Component;
using STC.quant.plugin.View.HomePage;
using STC.Common.ViewModel;
using STC.quant.plugin.View.Component;
using System.IO;
using Microsoft.Win32;
using STC.Common.View.UI;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using netapi.Api;
using STC.quant.plugin.Model;
using System.Collections.ObjectModel;
using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;

namespace STC.quant.plugin.ViewModel
{
    /// <summary>
    /// 双边报价MMListBoxView数据内容
    /// </summary>
    public class MMListBoxViewModel : ViewModelBase
    {
        public string groupID = string.Empty;

        public string quantID = string.Empty;

        public string inID = string.Empty;

        public ICommand UpdateAllEnableCommand { get; set; }

        public ICommand ToAutoCommand { get; set; }

        public ICommand DelStrategyCommand { get; set; }

        public ICommand UpdateStrategyCommand { get; set; }

        public ICommand ExportLogCommand { get; set; }

        public ICommand StartCommand { get; set; }

        public ICommand StopCommand { get; set; }

        public ICommand OpenCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        public ICommand DownLoadCommand { get; set; }

        public ICommand KillCommand { get; set; }

        private object lockObj = new object();

        private object lockParam = new object();

        private object lockLog = new object();

        private object lockIndicator = new object();


        private Thread _RefreshDefaultSelectThread = null;

        public DataGrid logDataGrid;
        public DataGrid qunatDataGrid;

        public MyTraderViewModel _myTraderViewModel { get; set; }
        public AlterParamViewModel viewModel = AlterParamViewModel.Instance;
        private bool isInit = true;

        #region 表格字段
        public const string ID_FIELD = "Id";
        public const string SOURCE_FIELD = "Source";
        public const string QUANTID_FIELD = "Quantid";
        public const string QUANTNAME_FIELD = "Quantname";
        public const string REMARK_FIELD = "Remark";
        public const string ENABLE_FIELD = "Enable";
        public const string USERNAME_FIELD = "Username";
        public const string CLOSEBY_FIELD = "Closeby";
        public const string UPDATETIME_FIELD = "Updatetime";
        public const string TEMPLETID_FIELD = "TempletId";
        public const string TEMPLETNAME_FIELD = "TempletName";
        public const string SHOWSTART_FIELD = "ShowStartBtn";
        public const string SHOWSTOP_FIELD = "ShowStopBtn";
        public const string LABLE = "LABLE";
        public const string LABLE2 = "LABLE2";
        public const string LABLEPINYIN = "lablepinyin";
        public const string LABLEFULL = "lablepinyinfull";
       

        public const string ISCHECKED_FIELD = "IsChecked";
        public const string TYPE_FIELD = "Type";
        public const string INFO4_FIELD = "Info4";
        public const string INFO5_FIELD = "Info5";
        public const string ShowDeleteBtn = "ShowDeleteBtn";
        public const string ShowDelete2Btn = "ShowDelete2Btn";
        public const string INFO2_FIELD = "Info2";
        public const string INFO3_FIELD = "Info3";

        //策略参数
        public const string PID_FIELD = "Id";
        public const string PARAMID_FIELD = "ParamId";
        public const string PARAMKEY_FIELD = "ParamKey";
        public const string SUBPARAMKEY_FIELD = "SubParamKey";
        public const string PARAMTYPE_FIELD = "ParamType";
        public const string PARAMVALUE_FIELD = "ParamValue";
        public const string PARAMVALUEBACK_FIELD = "ParamValueBack";
        //public const string ISPARAMVALUE_FIELD = "IsParamValue";
        //public const string ISPARAMVALUEBACK_FIELD = "IsParamValueBack";
        //public const string PARAMCHECKKEY_FIELD = "ParamCheckKey";
        public const string PARAMISCHECK_FIELD = "ParamISCheck";
        public const string PARAMDEESC_FIELD = "ParamDesc";
        public const string PARAMDEESCMuti_FIELD = "ParamDescMuti";
        public const string PARAMDSIDN_FIELD = "ParamSign";
        public const string PARAMDUNIT_FIELD = "ParamUnit";
        public const string PARAMDCOMMENT_FIELD = "ParamComment";
        public const string PARAMDCOMBOX_FIELD = "ParamCombox";
        public const string ParamDescColor_FIELD = "ParamDescColor";
        public const string ParamCheckValue_FIELD = "ParamCheckValue";
        public const string BUTTONNAME_FIELD = "ButtonName";
        public const string BUTTONGROUPNAME_FIELD = "GroupName";

        public const string PARAMKEY1_FIELD = "ParamKey1";
        public const string PARAMTYPE1_FIELD = "ParamType1";
        public const string PARAMVALUE1_FIELD = "ParamValue1";
        public const string PARAMVALUE1BACK_FIELD = "ParamValue1Back";
        public const string PARAMDEESC1_FIELD = "ParamDesc1";
        public const string PARAMDSIDN1_FIELD = "ParamSign1";
        public const string PARAMDUNIT1_FIELD = "ParamUnit1";
        public const string PARAMDCOMMENT1_FIELD = "ParamComment1";
        public const string PARAMDCOMBOX1_FIELD = "ParamCombox1";
        public const string ParamDescColor1_FIELD = "ParamDescColor1";
        public const string ParamCheckValue1_FIELD = "ParamCheckValue1";

        public const string PARAMKEY2_FIELD = "ParamKey2";
        public const string PARAMTYPE2_FIELD = "ParamType2";
        public const string PARAMVALUE2_FIELD = "ParamValue2";
        public const string PARAMVALUE2BACK_FIELD = "ParamValue2Back";
        public const string PARAMDEESC2_FIELD = "ParamDesc2";
        public const string PARAMDSIDN2_FIELD = "ParamSign2";
        public const string PARAMDUNIT2_FIELD = "ParamUnit2";
        public const string PARAMDCOMMENT2_FIELD = "ParamComment2";
        public const string PARAMDCOMBOX2_FIELD = "ParamCombox2";
        public const string ParamDescColor2_FIELD = "ParamDescColor2";
        public const string ParamCheckValue2_FIELD = "ParamCheckValue2";
        public const string PARAMINFO1_FIELD = "Info1";
        public const string PARAMTREEITEMSOURCE_FIELD = "MultiTreeComboxItemsSource";




        public const string PARAMISENABLED_FIELD = "IsEnabled";//表格内删除按钮是否有权限
        public const string PARAMISALTERENABLED_FIELD = "IsAlterEnabled";

        //策略指标
        public const string INID_FIELD = "Id";
        public const string INKEY_FIELD = "Key";
        public const string INKeyDesc_FIELD = "KeyDesc";
        public const string INVALUE_FIELD = "Value";
        public const string INFLAG_FIELD = "Flag";
        public const string INUPDATETIME_FIELD = "Updatetime";

        //策略日志
        public const string Createtime = "Createtime";
        public const string Content = "Content";
        public const string Id = "Id";
        public const string IDTT = "IDTT";
        public const string Txt = "Txt";

        //策略债券组
        public const string G_Id = "Id";
        public const string G_Groupname = "Groupname";
        public const string G_BondNum = "BondNum";
        public const string G_BondCode = "BondCode";
        public const string G_updatetime = "updatetime";
        public const string G_GroupTextVisible = "GroupTextVisible";
        public const string G_GroupTextVisibleNo = "GroupTextVisibleNo";
        public const string G_IsChecked = "IsChecked";
        

        //策略债券组下债券
        public const string B_Id = "Id";
        public const string B_GroupId = "GroupId";
        public const string B_securityid = "Securityid";
        public const string B_symbol = "Symbol";
        public const string B_tenor = "Tenor";
        public const string B_type = "Type";

        public const string QUANT_Type = "Quant";
        public const string SecurityID_ExcelField = "债券代码";
        public const string Symbol_ExcelField = "债券简称";
        public const string Tenor_ExcelField = "剩余期限";
        #endregion

        /// <summary>
        /// 批量操作按钮是否可用
        /// </summary>
        private bool _BatchOperateBtnEnanble = true;

        public bool BatchOperateBtnEnable
        {
            get
            {
                return _BatchOperateBtnEnanble;
            }
            set
            {
                if (_BatchOperateBtnEnanble != value)
                {
                    _BatchOperateBtnEnanble = value;
                    this.OnPropertyChanged();
                }
            }
        }
        //监控勾选状态
        private Thread _RefreshCheckBoxThread = null;
        // 是否选中全选多选框
        private bool _IsAllChecked = false;
        public bool IsAllChecked
        {
            get
            {
                return _IsAllChecked;
            }

            set
            {
                if (_IsAllChecked != value)
                {
                    _IsAllChecked = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 参数最后更新时间
        /// </summary>
        private string _LastUpdateTime = "";
        public string LastUpdateTime
        {
            get
            {
                return _LastUpdateTime;
            }
            set
            {
                _LastUpdateTime = value;
                this.OnPropertyChanged();
            }
        }

        private AsyncThreadQueue<TTbookQuantStrategys> _ProcessBondsThreadQueue;
        private AsyncThreadQueue<TTbookQuantParameter> _ProcessParasThreadQueue;
        private AsyncThreadQueue<TbookQuantIndicator> _ProcessIndicatorThreadQueue;
        private AsyncThreadQueue<TTbookQuantLog> _ProcessQuantLogThreadQueue;

        private DataView _BondView;

        public DataView BondView
        {
            get
            {
                lock (this.lockObj)
                {
                    if (_BondView == null)
                    {
                        _BondView = new DataTable().DefaultView;
                        _BondView.Table.Columns.Add(ID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SOURCE_FIELD, typeof(bool));
                        _BondView.Table.Columns.Add(QUANTID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(QUANTNAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(REMARK_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(ENABLE_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(CLOSEBY_FIELD, typeof(string)); 
                        _BondView.Table.Columns.Add(USERNAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(UPDATETIME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(TEMPLETID_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(TEMPLETNAME_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SHOWSTART_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(SHOWSTOP_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(LABLE, typeof(Dictionary<string, string>));
                        _BondView.Table.Columns.Add(LABLE2, typeof(string));
                        _BondView.Table.Columns.Add(LABLEPINYIN, typeof(string));
                        _BondView.Table.Columns.Add(LABLEFULL, typeof(string));
                        _BondView.Table.Columns.Add(ShowDeleteBtn, typeof(string));
                        _BondView.Table.Columns.Add(ShowDelete2Btn, typeof(string));
                        _BondView.Table.Columns.Add(ISCHECKED_FIELD, typeof(bool));
                        _BondView.Table.Columns.Add(TYPE_FIELD, typeof(string));
                        _BondView.Table.Columns[ISCHECKED_FIELD].DefaultValue = false;
                        _BondView.Table.Columns.Add(INFO3_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(INFO4_FIELD, typeof(string));
                        _BondView.Table.Columns.Add(INFO5_FIELD, typeof(string));
                        _BondView.Table.PrimaryKey = new DataColumn[] {
                        _BondView.Table.Columns[ID_FIELD],


                    };/* _BondView.Sort = $"{UPDATETIME_FIELD} desc";*/

                        this.OnPropertyChanged();
                    }
                    return this._BondView;
                }
            }
        }

        private DataView _ParamView;
        public DataView ParamView
        {
            get
            {
                lock (this.lockParam)
                {
                    if (_ParamView == null)
                    {
                        _ParamView = new DataTable().DefaultView; 
                        _ParamView.Table.Columns.Add(PID_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMID_FIELD, typeof(string)); 
                        _ParamView.Table.Columns.Add(PARAMKEY_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(SUBPARAMKEY_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMTYPE_FIELD, typeof(string)); 
                        _ParamView.Table.Columns.Add(PARAMVALUE_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMISCHECK_FIELD, typeof(bool));
                        //_ParamView.Table.Columns.Add(ISPARAMVALUE_FIELD, typeof(bool)); 
                        //_ParamView.Table.Columns.Add(ISPARAMVALUEBACK_FIELD, typeof(bool));
                        _ParamView.Table.Columns.Add(PARAMVALUEBACK_FIELD, typeof(string));
                        //_ParamView.Table.Columns.Add(PARAMCHECKKEY_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDEESC_FIELD, typeof(string)); 
                        _ParamView.Table.Columns.Add(PARAMDEESCMuti_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDSIDN_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDUNIT_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDCOMMENT_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDCOMBOX_FIELD, typeof(Dictionary<string, string>));
                        

                        _ParamView.Table.Columns.Add(PARAMKEY1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMTYPE1_FIELD, typeof(string)); 
                        _ParamView.Table.Columns.Add(PARAMVALUE1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMVALUE1BACK_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDEESC1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDSIDN1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDUNIT1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDCOMMENT1_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDCOMBOX1_FIELD, typeof(List<string>));

                        _ParamView.Table.Columns.Add(PARAMKEY2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMTYPE2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMVALUE2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMVALUE2BACK_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDEESC2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDSIDN2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDUNIT2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDCOMMENT2_FIELD, typeof(string));
                        _ParamView.Table.Columns.Add(PARAMDCOMBOX2_FIELD, typeof(List<string>));

                        _ParamView.Table.Columns.Add(PARAMISENABLED_FIELD, typeof(bool));
                        _ParamView.Table.Columns.Add(PARAMISALTERENABLED_FIELD, typeof(bool));
                        _ParamView.Table.PrimaryKey = new DataColumn[] {
                        _ParamView.Table.Columns[PID_FIELD]
                    }; 
                        this.OnPropertyChanged();
                    }
                    return this._ParamView;
                }
            }
        }

        private DataView _IndicatorView;
        public DataView IndicatorView
        {
            get
            {
                lock (this.lockIndicator)
                {
                    if (_IndicatorView == null)
                    {
                        _IndicatorView = new DataTable().DefaultView;
                        _IndicatorView.Table.Columns.Add(INID_FIELD, typeof(string));
                        _IndicatorView.Table.Columns.Add(QUANTID_FIELD, typeof(string));
                        _IndicatorView.Table.Columns.Add(INKEY_FIELD, typeof(string));
                        _IndicatorView.Table.Columns.Add(INKeyDesc_FIELD, typeof(string));
                        _IndicatorView.Table.Columns.Add(INVALUE_FIELD, typeof(string));
                        _IndicatorView.Table.Columns.Add(INFLAG_FIELD, typeof(string));
                        _IndicatorView.Table.Columns.Add(INUPDATETIME_FIELD, typeof(string));
                        _IndicatorView.Table.PrimaryKey = new DataColumn[] {
                        _IndicatorView.Table.Columns[QUANTID_FIELD],
                        _IndicatorView.Table.Columns[INKEY_FIELD]
                    };
                        this.OnPropertyChanged();
                    }
                    return this._IndicatorView;
                }
            }
        }

        /// <summary>
        /// 策略日志数据源
        /// </summary>
        private DataView _LogDT;
        public DataView LogDT
        {
            get
            {
                lock (this.lockLog)
                {
                    if (_LogDT == null)
                    {
                        _LogDT = new DataTable().DefaultView;
                        _LogDT.Table.Columns.Add(Createtime, typeof(string));
                        _LogDT.Table.Columns.Add(Content, typeof(string));
                        _LogDT.Table.Columns.Add(Id, typeof(string));
                        _LogDT.Table.Columns.Add(Txt, typeof(string));
                        _LogDT.Table.PrimaryKey = new DataColumn[] { _LogDT.Table.Columns[Id] };
                        this.OnPropertyChanged();
                    }
                    return this._LogDT;
                }
            }
        }

        private DataRowView _selectedItem;
        public DataRowView SelectedItem
        {
            get
            {
                if (_selectedItem != null)
                {
                    IsVisibleEditButton(_selectedItem);

                    //string userName = _selectedItem[USERNAME_FIELD] + "";
                    ////管理自己 & 查看全部
                    //if (QuantGroupHandler.Instance.ManageYourselfViewAll && !userName.Equals(ApiManager.Instance.GetDefaultApi().UserName))
                    //{
                    //    IsParamEnabled = IsPublicAlterEnabled = false;
                    //}
                    //else
                    //{
                    //    string enable = _selectedItem[ENABLE_FIELD] + "";
                    //    string temple = _selectedItem[TEMPLETNAME_FIELD] + "";
                    //    IsParamEnabled = IsPublicAlterEnabled = "启动".Equals(enable) ? false : true;//策略启动，不能新增修改删除
                    //    if ("停止".Equals(enable) && !string.IsNullOrEmpty(temple))//策略关闭，但有模板，只能修改，不能新增和删除
                    //    {
                    //        IsParamEnabled = false;
                    //    }
                    //}
                }
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;

                this.OnPropertyChanged();
            }
        }

        private void IsVisibleEditButton(DataRowView Item)
        {
            string userName = Item[USERNAME_FIELD] + "";
            //管理自己 & 查看全部
            if (QuantGroupHandler.Instance.ManageYourselfViewAll && !userName.Equals(ApiManager.Instance.GetDefaultApi().UserName))
            {
                IsParamEnabled = IsPublicAlterEnabled = false;
            }
            else
            {
                string enable = Item[ENABLE_FIELD] + "";
                string temple = Item[TEMPLETNAME_FIELD] + "";
                IsParamEnabled = IsPublicAlterEnabled = ("启动".Equals(enable) || "1".Equals(enable)) ? false : true;//策略启动，不能新增修改删除
                if (("停止".Equals(enable) || "0".Equals(enable)) && !string.IsNullOrEmpty(temple))//策略关闭，但有模板，只能修改，不能新增和删除
                {
                    IsParamEnabled = false;
                }
            }
        }

        /// <summary>
        /// 参数管理是否能点击新增和删除按钮
        /// </summary>
        private bool _isParamEnabled = false;
        // req.groupid = this.groupID;;
        public bool IsParamEnabled
        {
            get
            {
                return _isParamEnabled;
            }
            set
            {
                _isParamEnabled = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 参数管理是否能点击修改按钮
        /// </summary>
        private bool _isPublicAlterEnabled = false;
        public bool IsPublicAlterEnabled
        {
            get
            {
                return _isPublicAlterEnabled;
            }
            set
            {
                _isPublicAlterEnabled = value;
                this.OnPropertyChanged();
            }
        }

        private DataRowView _selectedParamItem;
        public DataRowView SelectedParamItem
        {
            get
            {
                return _selectedParamItem;
            }

            set
            {
                _selectedParamItem = value;
                this.OnPropertyChanged();
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }

            set
            {
                if (_selectedIndex > -1 && this.BondView.Count > 0 && value == -1)
                {
                    if (_selectedIndex >= this.BondView.Count)
                    {
                        _selectedIndex = 0;
                    }
                }
                else
                {
                    _selectedIndex = value;
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MMListBoxViewModel(string grpID)
        {
            if (_RefreshCheckBoxThread == null)
            {
                _RefreshCheckBoxThread = new Thread(RefreshCheckBox);
                _RefreshCheckBoxThread.IsBackground = true;
                _RefreshCheckBoxThread.Start();
            }
            this.DelStrategyCommand = new RelayCommand(this.DelStrategy);
            this.UpdateStrategyCommand = new RelayCommand(this.UpdateStrategy);
            this.ExportLogCommand = new RelayCommand(this.ExportLog);
            this.StartCommand = new RelayCommand(this.Start);
            this.StopCommand = new RelayCommand(this.Stop);
            this.OpenCommand = new RelayCommand(this.ToOpen);
            this.CloseCommand = new RelayCommand(this.ToClose);
            this.DownLoadCommand = new RelayCommand(this.OnDownLoadClick);
            this.KillCommand = new RelayCommand(this.ToKill);
            //this.UpdateAllEnableCommand = new RelayCommand<string>(this.UpdateAllEnable);

            // Todo: 注册清零信号, 暂时先不清零
            //BaseHandler.Instance.Init(Instance_OnClear);

            BindingOperations.EnableCollectionSynchronization(this.BondView, this.lockObj);
            BindingOperations.EnableCollectionSynchronization(this.ParamView, this.lockParam);
            BindingOperations.EnableCollectionSynchronization(this.LogDT, this.lockLog);
            BindingOperations.EnableCollectionSynchronization(this.IndicatorView, this.lockIndicator);
            this.BondView.ListChanged += BondView_ListChanged;

            if (!string.IsNullOrWhiteSpace(grpID))
                this.Init(grpID);
            if (viewModel.dicGroup == null || viewModel.dicGroup.Count == 0)
                viewModel.initGroup();

            //初始化数据时，不选中行
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    if (!isInit)
                        break;
                    Thread.Sleep(5000);
                    isInit = false;
                }
            });
        }
        
        private void BondView_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (_isCheckBoxAllClick == true)
            {
                return;
            }

            if (e.PropertyDescriptor == null
                || (e.PropertyDescriptor != null && e.PropertyDescriptor.Name == ISCHECKED_FIELD))
            {
                isAdjustAllChkStatus = true;
            }
        }



        private void DelStrategy()
        {
            BatchOperateBtnEnable = false;
            string text = "确定要删除选中策略吗？";
            string title = "批量删除";
            if (CustomMessageBox.Question(text, title) == true)
            {
                QuantStrategyReq req = new QuantStrategyReq();
                DataRowView drv = SelectedItem;
                req.groupid = this.groupID;
                req.quantid = drv[QUANTID_FIELD] + "";
                req.id = drv[ID_FIELD] + "";
                req.action = "3";
                req.username = drv[USERNAME_FIELD] + "";
                ThreadPool.QueueUserWorkItem(o =>
                {
                    PResult pResult = QuantHandler.Instance.quantStrategyUpdate(req);
                    if (pResult != null && "0000".Equals(pResult.Code))
                    {
                        SetDefaultSelected();
                    }
                });
            }
            BatchOperateBtnEnable = true;
        }

        #region 开启,关闭
        public void UpdateEnable(bool ischecked)
        {
            DataRowView drv = SelectedItem;
            if (drv == null)
                return;
            quantUpdateEnable(ischecked, drv);
        }

        public void quantUpdateEnable(bool ischecked, DataRowView drv)
        {
            QuantStrategyReq req = new QuantStrategyReq();
            req.quantid = drv[QUANTID_FIELD] + "";
            req.id = drv[ID_FIELD] + "";
            req.templetid = drv[TEMPLETID_FIELD] + "";
            req.type = drv[TYPE_FIELD] + "";
            req.closeby = ApiManager.Instance.GetDefaultApi().UserName;
            if (ischecked)
            {
                req.enable = "1";
                //List<TTbookQuantParameter> list = QuantGroupHandler.Instance.GetParasByQuantID(req.quantid);
                //if (list != null && list.Count > 0)
                //{
                //    var parList = from m in list where "Y".Equals(m.Inf2) && (m.Param_Value == "" || m.Param_Value == null) select m;
                //    if (parList != null && parList.ToList().Count > 0)
                //    {
                //        CustomMessageBox.Warning("启动失败,参数值为空！", "");
                //        return;
                //    }
                //}
                string qunantId = drv[QUANTID_FIELD] + "";
                string inf3 = drv[INFO3_FIELD] + "";
                string username = drv[USERNAME_FIELD] + "";
                if (!string.IsNullOrWhiteSpace(inf3) && "1".Equals(inf3) && username.Equals(LocalEnv.UserInfo.UserName))
                {
                    // CustomMessageBox.Info("策略:" + qunantId + "模板有更新!");
                    string text = "策略:" + qunantId + "模板有更新，确认开启吗？";
                    string title = "提示";
                    if (CustomMessageBox.Question(text, title) == true)
                    {
                        req.inf3 = "0";
                    }
                    else
                        return;
                }
            }
            else
                req.enable = "0";
            req.action = "2";
            ThreadPool.QueueUserWorkItem(o =>
            {
                PResult pResult = QuantHandler.Instance.quantUpdateEnable(req);
                if (pResult != null && "0000".Equals(pResult.Code))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        drv[ENABLE_FIELD] = req.enable.Equals("1") ? "启动" : "停止";
                        drv[SHOWSTART_FIELD] = req.enable.Equals("1") ? "0" : "1";
                        drv[SHOWSTOP_FIELD] = req.enable.Equals("1") ? "1" : "0";
                        drv[SOURCE_FIELD] = req.enable.Equals("1") ? true : false;
                    }));
                }
                else
                {
                    CustomMessageBox.Warning(pResult == null ? "启停失败！" : pResult.Msg, "");
                }
            });
        }

        public void Start()
        {
            UpdateEnable(true);
        }

        public void Stop()
        {
            UpdateEnable(false);
        }
        private void ToOpen()
        {
            BatchOperateBtnEnable = false;
            try
            {
                lock (lockObj)
                {

                    foreach (DataRowView drv in this.BondView)
                    {
                        if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                            && (bool)drv[ISCHECKED_FIELD] == true
                            && (string)drv[ENABLE_FIELD] == "停止"
                          )
                        {
                            string quantid = drv[QUANTID_FIELD] + "";
                            List<TTbookQuantParameter> list = QuantGroupHandler.Instance.GetParasByQuantID(quantid);
                            //if (list != null && list.Count > 0)
                            //{
                            //    var parList = from m in list where m.Param_Value == "" || m.Param_Value == null select m;
                            //    if (parList != null && parList.ToList().Count > 0)
                            //    {
                            //        CustomMessageBox.Warning(quantid + "启动失败,参数值为空！", "");
                            //        continue;
                            //    }
                            //}
                            ThreadPool.QueueUserWorkItem(o =>
                            {
                                QuantStrategyReq req = new QuantStrategyReq();
                                req.quantid = drv[QUANTID_FIELD] + "";
                                req.id = drv[ID_FIELD] + "";
                                req.templetid = drv[TEMPLETID_FIELD] + "";
                                req.enable = "1";
                                req.type = drv[TYPE_FIELD] + "";
                                req.closeby = ApiManager.Instance.GetDefaultApi().UserName;
                                PResult pResult = QuantHandler.Instance.quantUpdateEnable(req);
                                if (pResult != null && "0000".Equals(pResult.Code))
                                {
                                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        drv[ENABLE_FIELD] = req.enable.Equals("1") ? "启动" : "停止";
                                        drv[SHOWSTART_FIELD] = req.enable.Equals("1") ? "0" : "1";
                                        drv[SHOWSTOP_FIELD] = req.enable.Equals("1") ? "1" : "0";
                                    }));
                                }
                                else
                                {
                                    CustomMessageBox.Warning(pResult == null ? req.quantid + "启动失败！" : req.quantid + pResult.Msg, "");
                                }
                                Thread.Sleep(LocalEnv.ConfInfo.BatchOpenIntervalTime);
                            }); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                BatchOperateBtnEnable = true;
            }
        }

        private void ToClose()
        {
            BatchOperateBtnEnable = false;
            try
            {
                lock (lockObj)
                {
                    foreach (DataRowView drv in this.BondView)
                    {
                        if (!(drv[ISCHECKED_FIELD] is System.DBNull)
                            && (bool)drv[ISCHECKED_FIELD] == true
                             && (string)drv[ENABLE_FIELD] == "启动")
                        {
                            string quantid = drv[QUANTID_FIELD] + "";
                            ThreadPool.QueueUserWorkItem(o =>
                            {
                                QuantStrategyReq req = new QuantStrategyReq();
                                req.quantid = drv[QUANTID_FIELD] + "";
                                req.id = drv[ID_FIELD] + "";
                                req.templetid = drv[TEMPLETID_FIELD] + "";
                                req.enable = "0";
                                req.type = drv[TYPE_FIELD] + "";
                                PResult pResult = QuantHandler.Instance.quantUpdateEnable(req);
                                if (pResult != null && "0000".Equals(pResult.Code))
                                {
                                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                    {
                                        drv[ENABLE_FIELD] = req.enable.Equals("1") ? "启动" : "停止";
                                        drv[SHOWSTART_FIELD] = req.enable.Equals("1") ? "0" : "1";
                                        drv[SHOWSTOP_FIELD] = req.enable.Equals("1") ? "1" : "0";
                                    }));
                                }
                                else
                                {
                                    CustomMessageBox.Warning(pResult == null ? req.quantid + "停止失败！" : pResult.Msg, "");
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                BatchOperateBtnEnable = true;
            }
        }

        private void ToKill()
        {
            try
            {
                BatchOperateBtnEnable = false;
                string text = "确定要结束选中策略的进程吗？";
                string title = "结束进程";
                if (CustomMessageBox.Question(text, title) == true)
                {
                    DataRowView drv = SelectedItem;
                    QuantStrategyReq req = new QuantStrategyReq();
                    req.quantid = drv[QUANTID_FIELD] + "";
                    req.id = drv[ID_FIELD] + "";
                    req.templetid = drv[TEMPLETID_FIELD] + "";
                    req.enable = "2";
                    req.type = drv[TYPE_FIELD] + "";
                    PResult pResult = QuantHandler.Instance.quantUpdateEnable(req);
                    if (pResult != null && "0000".Equals(pResult.Code))
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        //    drv[ENABLE_FIELD] = req.enable.Equals("1") ? "启动" : "停止";
                        //    drv[SHOWSTART_FIELD] = req.enable.Equals("1") ? "0" : "1";
                        //    drv[SHOWSTOP_FIELD] = req.enable.Equals("1") ? "1" : "0";
                        //}));
                        //CustomMessageBox.Info(req.quantid + "Kill成功！");
                    }
                    else
                    {
                        CustomMessageBox.Warning(pResult == null ? req.quantid + "Kill失败！" : pResult.Msg, "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                BatchOperateBtnEnable = true;
            }
        }
        #endregion

        public async void UpdateStrategy()
        {
            DataRowView drv = SelectedItem;
            if (drv == null)
                return;
            AddStrategWindow addStrategWindow = new AddStrategWindow("设置策略");
            addStrategWindow.Owner = Application.Current.MainWindow;
            addStrategWindow.quantname.Text = drv[QUANTID_FIELD] + "";
            addStrategWindow.remark.Text = drv[QUANTNAME_FIELD] + "";
            //addStrategWindow.groupcode.SelectedValue = groupID;
            if (drv[LABLE] is System.Collections.Generic.Dictionary<string, string>)
                //addStrategWindow.groupcode.SelectedValues = (Dictionary<string, string>)drv[LABLE];
                addStrategWindow.groupcode.SetSelectValues((Dictionary<string, string>)drv[LABLE]);
            string temple = drv[TEMPLETID_FIELD] + "";
            if (!string.IsNullOrWhiteSpace(temple))
            {
                addStrategWindow.ckTemplet.IsChecked = true;
                addStrategWindow.comTemplet.SelectedValue = temple;
            }
            addStrategWindow.comTemplet.IsEnabled = false;
            addStrategWindow.ckTemplet.IsEnabled = false;
            addStrategWindow.quantname.IsEnabled = false; 
            addStrategWindow.ShowDialog();
            if (addStrategWindow.Result == false)
            {
                return;
            }
            QuantStrategyReq request = new QuantStrategyReq();
            request.id = drv[ID_FIELD] + "";
            request.quantid = addStrategWindow.QuantName;
            request.quantname = addStrategWindow.Remark;
            //request.groupid = addStrategWindow.GroupCode;
            request.lable = addStrategWindow.GroupCode;
            //request.username = 
            bool status = (bool)drv[SOURCE_FIELD];
            if (status)
                request.enable = "1";
            else
                request.enable = "0";
            request.action = "2"; // 修改
            request.username = drv[USERNAME_FIELD] + "";
            request.closeby = ApiManager.Instance.GetDefaultApi().UserName;

            PResult pResult = await Utils.Data.QuantGroupHandler.Instance.AsyncUpdateQuants(request);
            if (pResult != null && pResult.IsSuccess())
            {
                CustomMessageBox.Info($"修改策略！", "修改策略");
            }
            else
            {
                CustomMessageBox.Warning(pResult == null ? "修改失败！" : pResult.Msg, "修改策略");
            }
        }

        public void ExportLog()
        {
            //DataRowView drv = SelectedItem;
            //if (drv == null)
            //    return;
            //string quantid = drv[QUANTID_FIELD] + "";
            //string id = drv[ID_FIELD] + "";
            //string title = string.Format("{0} 策略日志", quantid);
            //QuantLog quantLog = new QuantLog();
            //quantLog.Title.Text = title;
            //quantLog.quantID = quantid;
            //quantLog.id = id;
            //quantLog.Owner = Application.Current.MainWindow;
            //quantLog.Show();//.ShowDialog();
        }

        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init(this.groupID);
                }));
            }
        }

        ~MMListBoxViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            //Clear();

            lock (this.lockObj)
            {
                if (_BondView != null)
                {
                    _BondView.Table.Clear();
                    _BondView.Dispose();
                    _BondView = null;
                }
            }

            base.Dispose();
        }


        public void Clear()
        {
            lock (this.lockIndicator)
            {
                if (_IndicatorView != null)
                {
                    _IndicatorView.Table.Rows.Clear();
                }
            }
            ClearLog();
            if (!string.IsNullOrEmpty(quantID))
            {
                _myTraderViewModel.ChangeQuantID(quantID);
            }

            if (_ProcessBondsThreadQueue != null)
            {
                _ProcessBondsThreadQueue.Clear();
            }
            if (_ProcessParasThreadQueue != null)
            {
                _ProcessParasThreadQueue.Clear();
            }
            if (_ProcessIndicatorThreadQueue != null)
            {
                _ProcessIndicatorThreadQueue.Clear();
            }
            if (_ProcessQuantLogThreadQueue != null)
            {
                _ProcessQuantLogThreadQueue.Clear();
            }
        }



        public void Instance_OnMM(TTbookQuantStrategys mm)
        {
            if (mm.Groupid == this.groupID)
            {
                if (_ProcessBondsThreadQueue != null)
                {
                    _ProcessBondsThreadQueue.Enqueue(mm);
                }
            }
        }
        public void Instance_OnParas(TTbookQuantParameter mm)
        {
            if (mm.Quantid == this.quantID)
            {
                if (_ProcessParasThreadQueue != null)
                {
                    _ProcessParasThreadQueue.Enqueue(mm);
                }
            }
        }

        public void Instance_OnIndicator(TbookQuantIndicator mm)
        {
            if (mm.Quantid == this.quantID)
            {
                if (_ProcessIndicatorThreadQueue != null)
                {
                    _ProcessIndicatorThreadQueue.Enqueue(mm);
                }
            }
        }
        public void Instance_OnIndicatorList(List<TbookQuantIndicator> list)
        {
            IndicatorView.Table.Rows.Clear();
            if (list == null)
                return;
            foreach (TbookQuantIndicator param in list)
            {
                //if (param.Id == this.inID)
                //{
                if (_ProcessIndicatorThreadQueue != null)
                {
                    _ProcessIndicatorThreadQueue.Enqueue(param);
                }
                //}
            }

        }
        private object lockOIns = new object();
        public void Instance_OnParasList(List<TTbookQuantParameter> list)
        {
            lock (this.lockOIns)
            {
                try
                {
                    if (this.ParamView != null)
                    {
                        this.ParamView.Table.Rows.Clear();
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
                if (list == null)
                    return;

                foreach (TTbookQuantParameter param in list)
                {
                    if (param.Quantid == this.quantID)
                    {
                        if (_ProcessParasThreadQueue != null)
                        {
                            _ProcessParasThreadQueue.Enqueue(param);
                        }
                        //UpdateParasAsync(param);
                    }
                }
            }
        }


        public void Instance_QuantLog(TTbookQuantLog log)
        {
            if (log.Quantid == this.quantID)
            {
                //if (_ProcessQuantLogThreadQueue != null)
                //{
                //    _ProcessQuantLogThreadQueue.Enqueue(mm);
                //}
                UpdateQuantLog(log);
            }
        }
        public bool ClearLog()
        {
            bool isClear = false;
            try
            {
                if (this.LogDT != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.LogDT.Table.Rows.Clear();
                        isClear = true;
                    }));
                }
                else
                    isClear = true;
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
            finally
            {
                //isClear = true;
            }
            return isClear;
        }
        public void Instance_QuantLogList(List<TTbookQuantLog> list)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (this.LogDT != null)
                    {
                        this.LogDT.Table.Rows.Clear();
                        this.LogDT.Table.AcceptChanges();
                    }
                }
                catch (Exception e)
                {
                    LogHandler.Error(e);
                }
                if (list == null)
                    return;
                foreach (TTbookQuantLog log in list)
                {
                    if (log.Quantid == this.quantID)
                    {
                        //if (_ProcessQuantLogThreadQueue != null)
                        //{
                        //    _ProcessQuantLogThreadQueue.Enqueue(log);
                        //}
                        UpdateQuantLog(log);
                    }
                }
            }));
        }



        private void Init(string groupID)
        {
            this.groupID = groupID;
            // 更新策略
            List<TTbookQuantStrategys> mmList = null;
            if (_ProcessBondsThreadQueue == null)
            {
                _ProcessBondsThreadQueue = new AsyncThreadQueue<TTbookQuantStrategys>(UpdateStrategysync);
                QuantGroupHandler.Instance.OnMMBonds += Instance_OnMMBonds;

                mmList = QuantGroupHandler.Instance.GetMMsByGroupID(this.groupID);
                if (mmList != null)
                {
                    foreach (TTbookQuantStrategys mm in mmList)
                    {
                        Instance_OnMM(mm);
                    }
                    //ThreadPool.QueueUserWorkItem(o =>
                    //{
                    //    QuantGroupHandler.Instance.InitLog(mmList);
                    //});
                }
            }
            if (_ProcessParasThreadQueue == null)
            {
                _ProcessParasThreadQueue = new AsyncThreadQueue<TTbookQuantParameter>(UpdateParasAsync);
                QuantGroupHandler.Instance.OnParas += Instance_OnParas;
            }

            if (_ProcessIndicatorThreadQueue == null)
            {
                _ProcessIndicatorThreadQueue = new AsyncThreadQueue<TbookQuantIndicator>(UpdateIndicatorAsync);
                QuantGroupHandler.Instance.OnIndicator += Instance_OnIndicator;
            }

            if (_ProcessQuantLogThreadQueue == null)
            {
                _ProcessQuantLogThreadQueue = new AsyncThreadQueue<TTbookQuantLog>(UpdateQuantLog);
                QuantGroupHandler.Instance.QuantLog += Instance_QuantLog;
            }

            //if (_RefreshDefaultSelectThread == null)
            //{
            //    _RefreshDefaultSelectThread = new Thread(RefreshDefaultSelect);
            //    _RefreshDefaultSelectThread.IsBackground = true;
            //    _RefreshDefaultSelectThread.Start();
            //}
        }

        //private void RefreshDefaultSelect()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            SetDefaultSelected();

        //            if (_isSelected)
        //                break;

        //            Thread.Sleep(500);
        //        }
        //        catch (ThreadAbortException ex)
        //        {

        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.Error(ex);
        //        }
        //    }
        //}

        private bool _isSelected = false;
        private void SetDefaultSelected()
        {
            if (BondView != null && BondView.Table.Rows.Count > 0)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //if (!_isSelected && BondView != null && BondView.Table.Rows.Count > 0)
                    //{
                    this.SelectedItem = this.BondView[0];
                    _isSelected = true;
                    //}
                }));
            }
        }


        private void Instance_OnMMBonds(TTbookQuantStrategys obj)
        {
            Instance_OnMM(obj);
        }

        private void UpdateStrategysync(TTbookQuantStrategys mm)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateStrategyList(mm);
            }));
        }
        private void UpdateParasAsync(TTbookQuantParameter mm)
        {
            lock (lockParam)
            {
                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                UpdateParasList(mm);
                if (!string.IsNullOrEmpty(mm.Updatetime))
                    SetUpdateTime(mm.Updatetime);
                //}));
            }
        }

        private void SetUpdateTime(string date)
        {
            if (string.IsNullOrEmpty(LastUpdateTime))
                LastUpdateTime = date;
            else if (LastUpdateTime.CompareTo(date) < 0)
                LastUpdateTime = date;
        }

        private void UpdateIndicatorAsync(TbookQuantIndicator mm)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateIndicatorList(mm);
            }));
        }
        private void UpdateQuantLog(TTbookQuantLog log)
        {
            lock (this.lockLog)
            {
                DataRow dataRow = _LogDT.Table.Rows.Find(log.Id);
                if (dataRow == null)
                {
                    dataRow = _LogDT.Table.NewRow();
                    dataRow[Createtime] = log.Createtime;
                    dataRow[Content] = log.Content;
                    dataRow[Id] = log.Id;
                    dataRow[Txt] = string.Format("{0}   {1}", log.Createtime, log.Content);
                    if (_LogDT.Table.Rows.Count > QuantGroupHandler.Instance.LogPageSize)
                    {
                        this._LogDT.Table.Rows.RemoveAt(0);
                    }
                    this._LogDT.Table.Rows.Add(dataRow);
                    this._LogDT.Table.AcceptChanges();
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ScrollToEnd();
                    }));
                }
            }
        }

        private void ScrollToEnd()
        {
            DataGridAutomationPeer lvap = new DataGridAutomationPeer(logDataGrid);
            var svap = lvap.GetPattern(PatternInterface.Scroll) as ScrollViewerAutomationPeer;
            if (svap != null)
            {
                var scroll = svap.Owner as ScrollViewer;
                scroll.ScrollToEnd();
            }
        }
        /// <summary>
        /// 更新列表
        /// </summary>
        /// <param name="bond"></param>
        private void UpdateStrategyList(TTbookQuantStrategys obj)
        {
            try
            {
                string quantid = "";
                if (SelectedItem != null)
                    quantid = SelectedItem[QUANTID_FIELD] + "";
                lock (this.lockObj)
                {
                   DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.Id });
                    if (bondInfosItems != null)
                    {

                        if ("3" == obj.Action)
                        {
                            string securityID = obj.Id;
                            lock (this.lockObj)
                            {
                                int index = this.SelectedIndex;
                                DataRow removeDR = _BondView.Table.Rows.Find(new object[] { securityID });
                                if (removeDR != null)
                                {
                                    this.BondView.Table.Rows.Remove(removeDR);
                                    AdjustAllChkStatus();
                                    //SetDefaultSelected();
                                    //if (_isSelected)
                                    //{
                                    //    if (index >= 0 && _BondView.Count > 0)
                                    //    {
                                    //        if (index == _BondView.Count)
                                    //            this.SelectedItem = this.BondView[index - 1];
                                    //        else
                                    //            this.SelectedItem = this.BondView[index];
                                    //    }
                                    //    else
                                    //    {
                                    //        this.SelectedItem = null;
                                    //        ClearLog();
                                    //    }
                                    //}
                                    //string removeSecurityID = string.Empty;
                                }
                            }
                            return;
                        }
                        else if ("1" == obj.Action)
                        {
                            bondInfosItems[ID_FIELD] = obj.Id;
                            bondInfosItems[SOURCE_FIELD] = obj.Enable == "1" ? true : false;//来源
                            bondInfosItems[ENABLE_FIELD] = obj.Enable == "1" ? "启动" : "停止"; // 开关
                            bondInfosItems[SHOWSTART_FIELD] = obj.Enable == "1" ? "0" : "1";//0-显示，1-不显示
                            bondInfosItems[SHOWSTOP_FIELD] = obj.Enable;//0-显示，1-不显示
                            bondInfosItems[QUANTID_FIELD] = obj.Quantid;
                            bondInfosItems[REMARK_FIELD] = obj.Remark;
                            bondInfosItems[UPDATETIME_FIELD] = obj.Updatetime;
                            bondInfosItems[CLOSEBY_FIELD] = obj.Closeby;
                            bondInfosItems[USERNAME_FIELD] = obj.Username;
                            bondInfosItems[QUANTNAME_FIELD] = obj.Quantname;
                            if (!string.IsNullOrEmpty(obj.Inf1))
                            {
                                TbookQuantTemplet tem = MsgHelper.Deserialize<TbookQuantTemplet>(obj.Inf1, MsgSerializerType.Json);
                                bondInfosItems[TEMPLETNAME_FIELD] = tem.Templetname;
                                bondInfosItems[TEMPLETID_FIELD] = tem.Id;
                            }
                            bondInfosItems[LABLE] = getDicLable(obj.Lable, obj.Lablepinyin);
                            bondInfosItems[LABLE2] = obj.Lable;
                            bondInfosItems[LABLEPINYIN] = obj.Lablepinyin;
                            bondInfosItems[LABLEFULL] = obj.Lablepinyinfull;
                            bondInfosItems[TYPE_FIELD] = obj.Type;
                            string isEdit= QuantGroupHandler.Instance.IsCheckOwn(obj.Username);
                            bondInfosItems[ShowDeleteBtn] = isEdit;
                            bondInfosItems[ShowDelete2Btn] = obj.Enable == "1" ? "0" : "1";
                            bondInfosItems[INFO3_FIELD] = obj.Inf3;
                            bondInfosItems[INFO4_FIELD] = obj.Inf4;
                            bondInfosItems[INFO5_FIELD] = obj.Inf5;
                        }
                        else if ("2" == obj.Action)
                        {
                            // bondInfosItems[ID_FIELD] = obj.Id;
                            bondInfosItems[SOURCE_FIELD] = obj.Enable == "1" ? true : false;//来源
                            bondInfosItems[ENABLE_FIELD] = obj.Enable == "1" ? "启动" : "停止"; // 开关
                            bondInfosItems[SHOWSTART_FIELD] = obj.Enable == "1" ? "0" : "1";//0-显示，1-不显示
                            bondInfosItems[SHOWSTOP_FIELD] = obj.Enable;//0-显示，1-不显示
                            bondInfosItems[QUANTID_FIELD] = obj.Quantid;
                            bondInfosItems[REMARK_FIELD] = obj.Remark;
                            bondInfosItems[UPDATETIME_FIELD] = obj.Updatetime;
                            bondInfosItems[CLOSEBY_FIELD] = obj.Closeby;
                            bondInfosItems[USERNAME_FIELD] = obj.Username;
                            bondInfosItems[QUANTNAME_FIELD] = obj.Quantname;
                            if (!string.IsNullOrEmpty(obj.Inf1))
                            {
                                TbookQuantTemplet tem = MsgHelper.Deserialize<TbookQuantTemplet>(obj.Inf1, MsgSerializerType.Json);
                                bondInfosItems[TEMPLETNAME_FIELD] = tem.Templetname;
                                bondInfosItems[TEMPLETID_FIELD] = tem.Id;
                            }
                            bondInfosItems[LABLE] = getDicLable(obj.Lable, obj.Lablepinyin);
                            bondInfosItems[LABLE2] = obj.Lable;
                            bondInfosItems[LABLEPINYIN] = obj.Lablepinyin;
                            bondInfosItems[LABLEFULL] = obj.Lablepinyinfull;
                            bondInfosItems[TYPE_FIELD] = obj.Type;
                            string isEdit = QuantGroupHandler.Instance.IsCheckOwn(obj.Username);
                            bondInfosItems[ShowDeleteBtn] = isEdit;
                            bondInfosItems[ShowDelete2Btn] = obj.Enable == "1" ? "0" : "1";
                            bondInfosItems[INFO3_FIELD] = obj.Inf3;
                            bondInfosItems[INFO4_FIELD] = obj.Inf4;
                            bondInfosItems[INFO5_FIELD] = obj.Inf5;
                        }
                        if (quantid.Equals(obj.Quantid))
                        {
                            //IsParamEnabled = obj.Enable.Equals("1") ? false : true;
                            //if ("0".Equals(obj.Enable) && !string.IsNullOrEmpty(obj.Inf1))
                            //{//有模板，不能增删
                            //    IsParamEnabled = false;
                            //}
                            DataRowView rowview = this.BondView.Table.DefaultView.Cast<DataRowView>().Where(a => a.Row == bondInfosItems).FirstOrDefault();
                            IsVisibleEditButton(rowview);
                            SetParamEnable(obj);
                        }

                    }
                    else
                    {
                        if ("1" == obj.Action || "2" == obj.Action)
                        {
                            bondInfosItems = this.BondView.Table.NewRow();
                            bondInfosItems[ID_FIELD] = obj.Id;
                            bondInfosItems[SOURCE_FIELD] = obj.Enable == "1" ? true : false;//来源
                            bondInfosItems[ENABLE_FIELD] = obj.Enable == "1" ? "启动" : "停止"; // 开关
                            bondInfosItems[SHOWSTART_FIELD] = obj.Enable == "1" ? "0" : "1";//0-显示，1-不显示
                            bondInfosItems[SHOWSTOP_FIELD] = obj.Enable;//0-显示，1-不显示
                            bondInfosItems[QUANTID_FIELD] = obj.Quantid;
                            bondInfosItems[REMARK_FIELD] = obj.Remark;
                            bondInfosItems[UPDATETIME_FIELD] = obj.Updatetime;
                            bondInfosItems[CLOSEBY_FIELD] = obj.Closeby;
                            bondInfosItems[USERNAME_FIELD] = obj.Username;
                            bondInfosItems[QUANTNAME_FIELD] = obj.Quantname;
                            if (!string.IsNullOrEmpty(obj.Inf1))
                            {
                                TbookQuantTemplet tem = MsgHelper.Deserialize<TbookQuantTemplet>(obj.Inf1, MsgSerializerType.Json);
                                bondInfosItems[TEMPLETNAME_FIELD] = tem.Templetname;
                                bondInfosItems[TEMPLETID_FIELD] = tem.Id;
                            }
                            bondInfosItems[LABLE] = getDicLable(obj.Lable, obj.Lablepinyin);
                            bondInfosItems[LABLE2] = obj.Lable;
                            bondInfosItems[LABLEPINYIN] = obj.Lablepinyin;
                            bondInfosItems[LABLEFULL] = obj.Lablepinyinfull;
                            bondInfosItems[TYPE_FIELD] = obj.Type;
                            string isEdit = QuantGroupHandler.Instance.IsCheckOwn(obj.Username);
                            bondInfosItems[ShowDeleteBtn] = isEdit;
                            bondInfosItems[ShowDelete2Btn] = obj.Enable == "1" ? "0" : "1";
                            bondInfosItems[INFO3_FIELD] = obj.Inf3;
                            bondInfosItems[INFO4_FIELD] = obj.Inf4;
                            bondInfosItems[INFO5_FIELD] = obj.Inf5;
                            this.BondView.Table.Rows.InsertAt(bondInfosItems, 0);
                            //只选中自己的策略
                            if (!isInit && this.BondView.Count > 0 && obj.Username.Equals(ApiManager.Instance.GetDefaultApi().UserName))
                            {
                                this.SelectedItem = this.BondView[0];
                                this.qunatDataGrid.ScrollIntoView(this.SelectedItem);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

       

        private Dictionary<string, string> getDicLable(string Lable, string Lablepinyin)
        {
            if (string.IsNullOrWhiteSpace(Lable) || string.IsNullOrWhiteSpace(Lablepinyin))
                return null;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] keys = Lablepinyin.Split(new char[1] { ' ' });
            string[] values = Lable.Split(new char[1] { ' ' });
            int i = 0;
            foreach (string s in values)
            {
                if (keys.Count() <= 0)
                    break;
                if (!string.IsNullOrWhiteSpace(s) && !string.IsNullOrWhiteSpace(keys[i]))
                    dic.Add(keys[i], s);
                i++;
            }
            return dic;
        }

        private void UpdateParasList(TTbookQuantParameter obj)
        {
            try
            {
                //DataRow bondInfosItems = this.BondView.Table.Rows.Find(new object[] { obj.Id });
                if (string.IsNullOrWhiteSpace(obj.Param_Type))
                    obj.Param_Type = obj.Para_Type;
                if (("Radio").Equals(obj.Param_Type) || ("MultiSelect").Equals(obj.Param_Type) || "MultiString".Equals(obj.Param_Type))
                {
                    int col = 0;
                    string[] str = obj.Param_Value.Contains(",") ? obj.Param_Value.Split(',') : null;
                    foreach (ParaOptionalValues v in obj.Para_Optional_Values)
                    {
                        col++;
                        if (("MultiSelect").Equals(obj.Param_Type) || ("MultiString").Equals(obj.Param_Type))
                        {
                            v.Param_Value = (str != null && str.Count() > (col - 1)) ? str[col - 1] : "";
                        }
                        DataRow bondInfosItems = UpdateParas(obj, obj.Id + "-" + v.Param_Key, v);
                        if (bondInfosItems == null)
                        {
                            LogHelper.Warn("bondInfosItems is null");
                            return;
                        }
                        bondInfosItems[SUBPARAMKEY_FIELD] = v.Param_Key;
                        if (obj.Param_Value.Equals(v.Param_Key) || (str != null && obj.Param_Value.Contains(v.Param_Key)))
                            bondInfosItems[MMListBoxViewModel.PARAMISCHECK_FIELD] = true;
                        else
                            bondInfosItems[MMListBoxViewModel.PARAMISCHECK_FIELD] = false;
                        if (col != 1)
                        {
                            bondInfosItems[PARAMKEY_FIELD] = "";
                            bondInfosItems[PARAMDEESC_FIELD] = "";
                        }
                        if (obj.Is_Independent_Para)
                        {
                            if (obj.Sub_Param.Count > col - 1)
                            {
                                SubParam sub = obj.Sub_Param[col - 1];
                                for (int subcol = 0; subcol < sub.Param.Count; subcol++)
                                {
                                    if (subcol == 0)
                                        UpdateSubParas1(bondInfosItems, sub.Param[0]);
                                    if (subcol == 1)
                                        UpdateSubParas2(bondInfosItems, sub.Param[1]);
                                    //if (subcol == 2)
                                    //    UpdateSubParas3(bondInfosItems, sub.Param[2]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    DataRow bondInfosItems = UpdateParas(obj, obj.Id, null);
                    if (bondInfosItems == null)
                        return;
                    if (obj.Is_Independent_Para)
                    {
                        foreach (SubParam sub in obj.Sub_Param)
                        {
                            foreach (QuantParameter pa in sub.Param)
                            {
                                // UpdateSubParas(bondInfosItems, pa);
                            }
                        }
                    }
                    else
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        foreach (ParaOptionalValues pv in obj.Para_Optional_Values)
                        {
                            if (!dic.ContainsKey(pv.Param_Key))
                                dic.Add(pv.Param_Key, pv.Param_Desc);
                        }
                        bondInfosItems[MMListBoxViewModel.PARAMDCOMBOX_FIELD] = dic;
                        bondInfosItems[MMListBoxViewModel.PARAMTYPE1_FIELD] = "";
                    }
                }
                if (!string.IsNullOrWhiteSpace(obj.Inf5) && obj.Username.Equals(LocalEnv.UserInfo.UserName))
                {
                    CustomMessageBox.Warning($"{obj.Inf5}");
                    obj.Inf5 = null;
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        #region 更新主参数
        private DataRow UpdateParas(TTbookQuantParameter obj, string Id, ParaOptionalValues v)
        {
            DataRow bondInfosItems = this.ParamView.Table.Rows.Find(new object[] { Id });
            TTbookQuantStrategys quant = QuantGroupHandler.Instance.GetQuantByQuantID(obj.Quantid);
            if (quant == null)
            {
                LogHandler.Warn("Quantid not in Cache:" + obj.Quantid);
            }
            if (bondInfosItems != null)
            {
                if ("3" == obj.Action)
                {
                    // 删除参数
                    int index = this.SelectedIndex;
                    DataRow removeDR = bondInfosItems;//ParamView.Table.Rows.Find(new object[] { obj.Id });
                    if (removeDR != null)
                    {
                        this.ParamView.Table.Rows.Remove(removeDR);
                        if (index >= 0 && _ParamView.Count > 0)
                        {
                            if (index == _ParamView.Count)
                                this.SelectedParamItem = this.ParamView[index - 1];
                            else
                                this.SelectedParamItem = this.ParamView[index];
                        }
                        else
                        {
                            this.SelectedParamItem = null;
                        }
                        string removeSecurityID = string.Empty;
                    }

                }
                else if ("1" == obj.Action || "2" == obj.Action)
                {
                    bondInfosItems[PID_FIELD] = Id;
                    bondInfosItems[PARAMID_FIELD] = obj.Id;
                    bondInfosItems[PARAMKEY_FIELD] = obj.Param_Key;
                    bondInfosItems[PARAMTYPE_FIELD] = obj.Param_Type;
                    bondInfosItems[PARAMVALUE_FIELD] = obj.Param_Value;
                    bondInfosItems[PARAMVALUEBACK_FIELD] = obj.Param_Value;
                    //if (obj.Param_Value.Equals("True"))
                    //{
                    //    bondInfosItems[ISPARAMVALUE_FIELD] = true;
                    //    bondInfosItems[ISPARAMVALUEBACK_FIELD] = true;
                    //}
                    //else if (obj.Param_Value.Equals("False"))
                    //{
                    //    bondInfosItems[ISPARAMVALUE_FIELD] = false;
                    //    bondInfosItems[ISPARAMVALUEBACK_FIELD] = false;
                    //}
                    switch (obj.Param_Type)
                    {
                        case "Radio":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.CheckBoxItem + "";
                            bondInfosItems[PARAMDCOMMENT_FIELD] = v != null ? v.Param_Desc : "";
                            break;
                        case "MultiSelect":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.MultiSelectItem + "";
                            bondInfosItems[PARAMDCOMMENT_FIELD] = v != null ? v.Param_Desc : "";
                            break;
                        case "Text":
                            //bondInfosItems[PARAMTYPE_FIELD] = ParaType.TextFieldItem + "";
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.TextItem + "";
                            bondInfosItems[PARAMVALUE_FIELD] = QuantCommonUtils.setGroupName(obj.Param_Value, viewModel.dicGroup);
                            break;
                        case "ComboBox":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.ComboBoxItem + "";
                            break;
                        case "DatePicker":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.DatePickerItem + "";
                            break;
                        case "SearchTextBox":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.SearchTextBox + "";
                            break;
                        case "MultiString":
                            bondInfosItems[PARAMDEESCMuti_FIELD] = v != null ? v.Param_Desc : "";
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.TextItemMulti + "";
                            bondInfosItems[PARAMDCOMMENT_FIELD] = obj != null ? obj.Para_Unit : "";
                            bondInfosItems[PARAMVALUE_FIELD] = v != null ? v.Param_Value : "";
                            break;
                        case "TreeView":
                            if (!string.IsNullOrEmpty(obj.Param_Value))
                            {
                                bondInfosItems[PARAMTYPE_FIELD] = ParaType.TreeViewComBoxItem + "";
                                TTbookBook tbook = AdminHandler.Instance.GetTTbookByID(obj.Param_Value);
                                bondInfosItems[PARAMVALUE_FIELD] = tbook == null ? obj.Param_Value : tbook.Bookname;
                            }
                            break;
                        default:
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.TextItem + "";
                            break;
                    }

                    //bondInfosItems[PARAMDEESC_FIELD] = obj.Param_Desc;
                    //bondInfosItems[PARAMDSIDN_FIELD] = obj.B_Bext_Box_String;
                    //bondInfosItems[PARAMDUNIT_FIELD] = obj.Para_Unit;
                    bondInfosItems[PARAMISENABLED_FIELD] = IsParamEnabled;
                    bondInfosItems[PARAMISALTERENABLED_FIELD] = IsParamEnabled;
                    if (quant != null && "0".Equals(quant.Enable) && !string.IsNullOrEmpty(quant.Inf1))
                    {
                        bondInfosItems[PARAMISALTERENABLED_FIELD] = !IsParamEnabled;
                    }
                }
            }
            else
            {
                if ("1" == obj.Action || "2" == obj.Action)
                {
                    bondInfosItems = this.ParamView.Table.NewRow();
                    bondInfosItems[PID_FIELD] = Id;
                    bondInfosItems[PARAMID_FIELD] = obj.Id;
                    bondInfosItems[PARAMKEY_FIELD] = obj.Param_Key;
                    bondInfosItems[PARAMTYPE_FIELD] = obj.Param_Type;
                    bondInfosItems[PARAMVALUE_FIELD] = obj.Param_Value;
                    bondInfosItems[PARAMVALUEBACK_FIELD] = obj.Param_Value;
                    //if (obj.Param_Value.Equals("True"))
                    //    bondInfosItems[ISPARAMVALUE_FIELD] = true;
                    //else if (obj.Param_Value.Equals("False"))
                    //    bondInfosItems[ISPARAMVALUE_FIELD] = false;
                    bondInfosItems[PARAMDEESC_FIELD] = obj.Param_Desc;
                    bondInfosItems[PARAMDSIDN_FIELD] = obj.B_Text_Box_String;
                    bondInfosItems[PARAMDUNIT_FIELD] = obj.Para_Unit;
                    bondInfosItems[PARAMISENABLED_FIELD] = IsParamEnabled;
                    bondInfosItems[PARAMISALTERENABLED_FIELD] = IsParamEnabled;
                    if (quant != null && "0".Equals(quant.Enable) && !string.IsNullOrEmpty(quant.Inf1))
                    {
                        bondInfosItems[PARAMISALTERENABLED_FIELD] = !IsParamEnabled;
                    }
                    switch (obj.Param_Type)
                    {
                        case "Radio":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.CheckBoxItem + "";
                            bondInfosItems[PARAMDCOMMENT_FIELD] = v != null ? v.Param_Desc : "";
                            break;
                        case "MultiSelect":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.MultiSelectItem + "";
                            bondInfosItems[PARAMDCOMMENT_FIELD] = v != null ? v.Param_Desc : "";
                            break;
                        case "Text":
                            //bondInfosItems[PARAMTYPE_FIELD] = ParaType.TextFieldItem + "";
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.TextItem + "";
                            bondInfosItems[PARAMVALUE_FIELD] = QuantCommonUtils.setGroupName(obj.Param_Value, viewModel.dicGroup);
                            break;
                        case "ComboBox":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.ComboBoxItem + "";
                            break;
                        case "DatePicker":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.DatePickerItem + "";
                            break;
                        case "SearchTextBox":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.SearchTextBox + "";
                            break;
                        case "MultiString":
                            bondInfosItems[PARAMDEESCMuti_FIELD] = v != null ? v.Param_Desc : "";
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.TextItemMulti + "";
                            bondInfosItems[PARAMDCOMMENT_FIELD] = obj != null ? obj.Para_Unit : "";
                            bondInfosItems[PARAMVALUE_FIELD] = v != null ? v.Param_Value : "";
                            break;
                        case "TreeView":
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.TreeViewComBoxItem + "";
                            if (!string.IsNullOrEmpty(obj.Param_Value))
                            {
                                TTbookBook tbook = AdminHandler.Instance.GetTTbookByID(obj.Param_Value);
                                bondInfosItems[PARAMVALUE_FIELD] = tbook == null ? obj.Param_Value : tbook.Bookname;
                            }
                            break;
                        default:
                            bondInfosItems[PARAMTYPE_FIELD] = ParaType.TextItem + "";
                            break;
                    }
                    // this.ParamView.Table.Rows.InsertAt(bondInfosItems, 0);
                    this.ParamView.Table.Rows.Add(bondInfosItems);
                    //if (this.SelectedParamItem == null)
                    //{
                    //    this.SelectedParamItem = this.ParamView[0];
                    //}
                }
            }
            return bondInfosItems;
        }
        #endregion

        #region 更新子参数
        private void UpdateSubParas1(DataRow bondInfosItems, QuantParameter obj)
        {
            if ("3" == obj.Action)
            {
                // 删除参数
                int index = this.SelectedIndex;
                DataRow removeDR = ParamView.Table.Rows.Find(new object[] { Id });
                if (removeDR != null)
                {
                    this.ParamView.Table.Rows.Remove(removeDR);

                    if (index >= 0 && _ParamView.Count > 0)
                    {
                        if (index == _ParamView.Count)
                            this.SelectedParamItem = this.ParamView[index - 1];
                        else
                            this.SelectedParamItem = this.ParamView[index];
                    }
                    else
                    {
                        this.SelectedParamItem = null;
                    }
                    string removeSecurityID = string.Empty;

                }
                return;
            }
            else if ("1" == obj.Action || "2" == obj.Action)
            {
                bondInfosItems[PARAMKEY1_FIELD] = obj.Param_Key;
                bondInfosItems[PARAMVALUE1_FIELD] = obj.Param_Value;
                bondInfosItems[PARAMVALUE1BACK_FIELD] = obj.Param_Value;
                bondInfosItems[PARAMDEESC1_FIELD] = obj.Param_Desc;
                bondInfosItems[PARAMDSIDN1_FIELD] = obj.B_Text_Box_String;
                bondInfosItems[PARAMDUNIT1_FIELD] = obj.Para_Unit;
                switch (obj.Param_Type)
                {
                    case "Radio":
                        bondInfosItems[PARAMTYPE1_FIELD] = ParaType.CheckBoxItem + "";
                        //bondInfosItems[PARAMDCOMMENT1_FIELD] = v != null ? v.Option_Desc : "";
                        break;
                    case "MultiSelect":
                        bondInfosItems[PARAMTYPE_FIELD] = ParaType.MultiSelectItem + "";
                        break;
                    case "Text":
                        bondInfosItems[PARAMTYPE1_FIELD] = ParaType.TextItem1 + "";
                        break;
                    case "ComboBox":
                        bondInfosItems[PARAMTYPE1_FIELD] = ParaType.ComboBoxItem + "";
                        break;
                    case "DatePicker":
                        bondInfosItems[PARAMTYPE1_FIELD] = ParaType.DatePickerItem + "";
                        break;
                    case "SearchTextBox":
                        bondInfosItems[PARAMTYPE_FIELD] = ParaType.SearchTextBox + "";
                        break;
                    default:
                        bondInfosItems[PARAMTYPE1_FIELD] = ParaType.TextItem1 + "";
                        break;
                }
            }
        }

        private void UpdateSubParas2(DataRow bondInfosItems, QuantParameter obj)
        {
            if ("3" == obj.Action)
            {
                // 删除参数
                int index = this.SelectedIndex;
                DataRow removeDR = ParamView.Table.Rows.Find(new object[] { Id });
                if (removeDR != null)
                {
                    this.ParamView.Table.Rows.Remove(removeDR);

                    if (index >= 0 && _ParamView.Count > 0)
                    {
                        if (index == _ParamView.Count)
                            this.SelectedParamItem = this.ParamView[index - 1];
                        else
                            this.SelectedParamItem = this.ParamView[index];
                    }
                    else
                    {
                        this.SelectedParamItem = null;
                    }
                    string removeSecurityID = string.Empty;
                }
                return;
            }
            else if ("1" == obj.Action || "2" == obj.Action)
            {
                bondInfosItems[PARAMKEY2_FIELD] = obj.Param_Key;
                bondInfosItems[PARAMVALUE2_FIELD] = obj.Param_Value;
                bondInfosItems[PARAMVALUE2BACK_FIELD] = obj.Param_Value;
                bondInfosItems[PARAMDEESC2_FIELD] = obj.Param_Desc;
                bondInfosItems[PARAMDSIDN2_FIELD] = obj.B_Text_Box_String;
                bondInfosItems[PARAMDUNIT2_FIELD] = obj.Para_Unit;
                switch (obj.Param_Type)
                {
                    case "Radio":
                        bondInfosItems[PARAMTYPE2_FIELD] = ParaType.CheckBoxItem + "";
                        //bondInfosItems[PARAMDCOMMENT1_FIELD] = v != null ? v.Option_Desc : "";
                        break;
                    case "MultiSelect":
                        bondInfosItems[PARAMTYPE_FIELD] = ParaType.MultiSelectItem + "";
                        break;
                    case "Text":
                        bondInfosItems[PARAMTYPE2_FIELD] = ParaType.TextItem2 + "";
                        break;
                    case "ComboBox":
                        bondInfosItems[PARAMTYPE2_FIELD] = ParaType.ComboBoxItem + "";
                        break;
                    case "DatePicker":
                        bondInfosItems[PARAMTYPE2_FIELD] = ParaType.DatePickerItem + "";
                        break;
                    case "SearchTextBox":
                        bondInfosItems[PARAMTYPE_FIELD] = ParaType.SearchTextBox + "";
                        break;
                    default:
                        bondInfosItems[PARAMTYPE2_FIELD] = ParaType.TextItem2 + "";
                        break;
                }
            }
        }
        #endregion

        private void SetParamEnable(TTbookQuantStrategys obj)
        {
            if (obj == null)
                return;
            List<TTbookQuantParameter> paramList = QuantGroupHandler.Instance.GetParasByQuantID(obj.Quantid);
            if (paramList == null || paramList.Count < 1)
                return;
            //lock (lockParam)
            //{
            //    foreach (TTbookQuantParameter param in paramList)
            //    {
            //        UpdateParasList(param);
            //    }
            //}
            lock (lockOIns)
            {
                foreach (TTbookQuantParameter param in paramList)
                {
                    if (param.Quantid == this.quantID)
                    {
                        if (_ProcessParasThreadQueue != null)
                        {
                            _ProcessParasThreadQueue.Enqueue(param);
                        }
                        //UpdateParasAsync(param);
                    }
                }
            }
        }

        /*  private bool IshaveTemple(string temple)
          { 

          }*/

        //private bool IsEnable(TTbookQuantParameter obj)
        //{
        //    TTbookQuantStrategys quant = QuantGroupHandler.Instance.GetQuantByQuantID(obj.Quantid);
        //    if (quant != null && "1".Equals(quant.Enable))
        //        obj.IsEnabled = false;
        //    return obj.IsEnabled;
        //}

        private void UpdateIndicatorList(TbookQuantIndicator obj)
        {
            try
            {
                if (!string.IsNullOrEmpty(obj.Flag))
                {
                    switch (obj.Flag)
                    {
                        case "1":
                            obj.Flag = "是";
                            break;
                        case "0":
                            obj.Flag = "否";
                            break;
                    }
                }
                lock (this.lockIndicator)
                {
                    DataRow bondInfosItems = this.IndicatorView.Table.Rows.Find(new object[] { obj.Quantid,obj.Key });

                    if (bondInfosItems != null)
                    {
                        bondInfosItems[INID_FIELD] = obj.Id;
                        bondInfosItems[QUANTID_FIELD] = obj.Quantid;
                        bondInfosItems[INKEY_FIELD] = obj.Key;
                        bondInfosItems[INKeyDesc_FIELD] = obj.Key_Desc; 
                        bondInfosItems[INVALUE_FIELD] = obj.Value;
                        bondInfosItems[INFLAG_FIELD] = obj.Flag;
                        bondInfosItems[INUPDATETIME_FIELD] = obj.Updatetime;
                    }
                    else
                    {
                        bondInfosItems = this.IndicatorView.Table.NewRow();
                        bondInfosItems[INID_FIELD] = obj.Id;
                        bondInfosItems[QUANTID_FIELD] = obj.Quantid;
                        bondInfosItems[INKEY_FIELD] = obj.Key;
                        bondInfosItems[INKeyDesc_FIELD] = obj.Key_Desc;
                        bondInfosItems[INVALUE_FIELD] = obj.Value;
                        bondInfosItems[INFLAG_FIELD] = obj.Flag;
                        bondInfosItems[INUPDATETIME_FIELD] = obj.Updatetime;
                        this.IndicatorView.Table.Rows.InsertAt(bondInfosItems, 0);
                        //if (this.SelectedParamItem == null)
                        //{
                        //    this.SelectedParamItem = this.ParamView[0];
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }


        /// <summary>
        /// 删除行
        /// </summary>
        public async void DelBond()
        {
            if (this.BondView == null || this.BondView.Count == 0)
            {
                return;
            }

            string securityID = Common.Utils.CommonUtils.ConvertToString(this.SelectedItem[QUANTID_FIELD]);
            //  string settlType = CommonUtils.ConvertToString(this.SelectedItem[SETTLE_TYPE_FIELD]);

            //QuantReq updateReq = new QuantReq();
            //updateReq.Action = "3"; // 添加
            QuantStrategyReq request = new QuantStrategyReq();
            request.quantid = securityID;
            request.groupid = groupID;
            // request.settltype = settlType;
            // updateReq.mmList = new List<TTbookQuantStrategys>();
            //updateReq.mmList.Add(request);
            PResult pResult = await Utils.Data.QuantGroupHandler.Instance.AsyncUpdateQuants(request);
            if (pResult != null && pResult.IsSuccess())
            {
                CustomMessageBox.Info($"{securityID}删除成功", "删除债券");
            }
            else
            {
                CustomMessageBox.Warning(pResult == null ? "删除失败" : pResult.Msg, "删除债券");
            }
        }

        #region 下载log
        private DataTable ExportDV;
        private string fileName = "";
        AutoResetEvent autoEvent = new AutoResetEvent(false);
        private void OnDownLoadClick()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(文本文件*.log)|*.log";
            saveFileDialog.FileName = quantID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                fileName = saveFileDialog.FileName;
                ExportDV = this.LogDT.Table.Clone();
                ThreadPool.QueueUserWorkItem(new WaitCallback(RequestExportMethod), autoEvent);
            }
        }
        private void RequestExportMethod(object state)
        {
            lock (ExportDV.Rows.SyncRoot)
            {
                RequestExportOrders();
            }
            autoEvent.Set();
        }

        public void RequestExportOrders()
        {
            StreamWriter sw = null;
            try
            {
                LogHelper.Info("fileName:" + fileName);

                bool isOk = TableToCsv(LogDT.ToTable(), fileName, "");
                if (isOk)
                {

                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        showMessage("导出失败");
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestExportOrders", e);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    showMessage("导出失败");
                }));
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
        }

        public Boolean TableToCsv(DataTable dt, string fileName, string uiName)
        {
            FileStream fs;
            StreamWriter sw;
            string savePath = Path.GetDirectoryName(fileName);
            string saveFile = fileName;
            //创建路径和文件
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            if (!File.Exists(saveFile))
            {
                fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
            }
            else
            {
                File.Delete(saveFile);
                fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
            }
            sw = new StreamWriter(fs, Encoding.UTF8);
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string text = dr[3].ToString();
                    sw.WriteLine(text);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                return false;
            }
            finally
            {
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            return true;
        }

        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.setWarnMsg(message);
            me.ShowDialog();
        }
        #endregion


        #region check
        /// <summary>
        /// 我的订单所有选中个数
        /// </summary>
        private int _checkedCount = 0;
        public int CheckedCount
        {
            get
            {
                return _checkedCount;
            }
            set
            {
                if (_checkedCount != value)
                {
                    _checkedCount = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 0: 显示 1：不显示
        /// </summary>
        private Visibility isHiddenOperationGrid = Visibility.Hidden;

        public Visibility IsHiddenOperationGrid
        {
            get
            {
                return isHiddenOperationGrid;
            }

            set
            {
                if (isHiddenOperationGrid != value)
                {
                    isHiddenOperationGrid = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 是否刷新checkbox选中状态
        /// </summary>
        private volatile bool isAdjustAllChkStatus = true;

        /// <summary>
        /// 是否点击全选checkbox
        /// </summary>
        private volatile bool _isCheckBoxAllClick = false;
        public void CheckBoxAllValueChange(bool isAllChecked)
        {
            _isCheckBoxAllClick = true;
            int checkedCount = 0;

            lock (this.lockObj)
            {
                foreach (DataRowView item in this.BondView)
                {
                    if ("0".Equals(item[ShowDeleteBtn] + ""))
                    {
                        item[ISCHECKED_FIELD] = isAllChecked;
                        if (isAllChecked)
                        {
                            checkedCount++;
                        }
                    }
                }
            }
            CheckedCount = checkedCount;
            if (checkedCount > 0)
            {
                IsHiddenOperationGrid = Visibility.Visible;
            }
            else
            {
                IsHiddenOperationGrid = Visibility.Hidden;
            }
            _isCheckBoxAllClick = false;
        }

        #endregion

        private void RefreshCheckBox()
        {
            try
            {
                while (true)
                {
                    if (isAdjustAllChkStatus)
                    {
                        isAdjustAllChkStatus = false;
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                          //  this.AdjustAllChkStatus();
                        }));
                    }
                    Thread.Sleep(200);
                }
            }
            catch (ThreadAbortException)
            {
                // ignore.
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        public void AdjustAllChkStatus()
        {
            int checkableCount = 0;
            int checkedCount = 0;

            foreach (DataRowView item in this.BondView)
            {
                checkableCount++;
                if (!(item[ISCHECKED_FIELD] is System.DBNull)
                && (bool)item[ISCHECKED_FIELD] == true)
                {
                    checkedCount++;
                }
            }

            IsAllChecked = checkableCount == checkedCount && checkableCount > 0;

            CheckedCount = checkedCount;
            if (checkedCount > 0)
            {
                IsHiddenOperationGrid = Visibility.Visible;
            }
            else
            {
                IsHiddenOperationGrid = Visibility.Hidden;
            }

        }

        #region 过滤
        StringBuilder filterSB = new StringBuilder();
        public void FilterData(string SearchText)
        {
            // 初始化过滤条件
            filterSB.Remove(0, filterSB.Length);
            if (SearchText.Length > 0)
            {
                this.filterSB.Append($"{QUANTID_FIELD} like '%{SearchText}%'");
                this.filterSB.Append(" or ");
                this.filterSB.Append($"{LABLE2} like '%{SearchText}%'");
                this.filterSB.Append(" or ");
                this.filterSB.Append($"{LABLEPINYIN} like '%{SearchText}%'");
            }
            this.BondView.RowFilter = filterSB.ToString();

            if (this.BondView.Count > 0)
            {
                this.SelectedItem = this.BondView[0];
            }
        }
        #endregion

        #region 点击消息提醒定位到对应策略上
        public void SelItem(string Id)
        {
            DataRow item = this.BondView.Table.Rows.Find(new object[] { Id });
            DataRowView rowview = this.BondView.Table.DefaultView.Cast<DataRowView>().Where(a => a.Row == item).FirstOrDefault();
            this.SelectedItem = rowview;
        }
        #endregion
    }

}
