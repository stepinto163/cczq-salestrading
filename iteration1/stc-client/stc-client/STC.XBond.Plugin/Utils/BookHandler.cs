using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Handler.BookManagement;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.BookManagement;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.Utils
{
    /// <summary>
    /// 处理交易包工具类
    /// </summary>
    public class BookHandler
    {
        // 订阅包
        private readonly Object m_OnBlocksEventLock = new Object();
        private Action<List<ExecutionReport>> _OnBlocks;
        public event Action<List<ExecutionReport>> OnBlocks
        {
            add
            {
                lock (m_OnBlocksEventLock)
                {
                    _OnBlocks += value;
                }
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    foreach (var obj in _CacheBlocks)
                    {
                        _OnBlocks?.Invoke(obj.Value);
                    }
                });
            }
            remove
            {
                lock (m_OnBlocksEventLock)
                {
                    _OnBlocks -= value;
                }
            }
        }

        /// <summary>
        /// 缓存包(订阅)
        /// </summary>
        private ConcurrentDictionary<string, List<ExecutionReport>> _CacheBlocks = new ConcurrentDictionary<string, List<ExecutionReport>>();

        /// <summary>
        /// BookHandler实例
        /// </summary>
        private static BookHandler server = new BookHandler();
        public static BookHandler Instance { get { return server; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        private BookHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }

        private volatile bool isInit = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            if (isInit == false)
            {
                // 订阅实时包消息
                BookMgtHandler.Instance.SubscribeExecOrdersForBlock(Instance_OnBlocks);

                isInit = true;
            }
        }

        /// <summary>
        /// 清零回调方法
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("BookHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {

                isInit = false;

                init();
            }
        }

        public const string ISEXPANDED_FIELD = "IsExpanded";// 展开状态
        public const string BLOCK_NUM_FIELD = "blocknum"; // 交易包号
        public const string EXEC_TYPE_FIELD = "exectype"; // 包状态
        public const string SYNC_FLAG_FIELD = "syncflag"; // 发送状态
        public const string SIDE_FIELD = "side"; // 方向
        public const string SECURITY_ID_FIELD = "securityid"; // 代码
        public const string SYMBOL_FIELD = "symbol"; // 简称
        public const string PARTY_SUB_NAME_FIELD = "partysubname"; // 本方交易员
        public const string COUNTER_PARTY_NAME_FILED = "counterpartyname"; // 对手交易机构名称
        public const string COUNTER_PARTY_SUB_NAME_FIELD = "counterpartysubname"; // 对手交易员
        public const string YIELD_FIELD = "yield"; // 收益率
        public const string PARENTFLAGQTY_FIELD = "ParentFlagQty"; // 总量
        public const string QTY_FIELD = "qty";// 量（万）
        public const string SETTLE_TYPE_FIELD = "settltype"; // 清算类型
        public const string TRADE_DATE_FIELD = "tradeDate";// 成交日期
        public const string SETTLE_DATE_FIELD = "settldate";// 交割日期
        public const string CREATE_DATE_FIELD = "createDate";// 创建日期
        public const string EXEC_ID_FIELD = "execid"; // STC成交单号
        public const string PRICE_FIELD = "price"; // STC成交价
        public const string TERMINAL_FIELD = "terminal"; // 成交方式
        public const string NUM_FIELD = "num"; // 笔数
        public const string CLOSE_BY_FIELD = "closeby";// 最后更新人
        public const string UPDATE_TIME_FIELD = "updatetime";// 最后更新时间
        public const string SEND_TIME_FIELD = "sendtime"; // 最后发送时间

        public const string DEALTYPE_FIELD = "dealtype";// 类型
        public const string NETPRICE_FIELD = "netprice";//净价
        public const string DIRTYPRICE_FIELD = "dirtyPrice";//全价
        public const string OPSENABLED_FIELD = "OpsEnabled";//按钮是否可用
        public const string INFO7_FIELD = "Info7";
        public const string ISTIP_FIELD = "IsTip";

        /// <summary>
        /// 交易包主列表资源锁
        /// </summary>
        private object _mainBookLockObj = new object();

        /// <summary>
        /// 交易包主列表数据源
        /// </summary>
        private DataView _MainBookDV;

        public DataView MainBookDV
        {
            get
            {
                lock (_mainBookLockObj)
                {
                    if (_MainBookDV == null)
                    {
                        _MainBookDV = new DataView();
                        _MainBookDV.Table = new DataTable("MainBook");
                        _MainBookDV.Table.Columns.Add(BookHandler.ISEXPANDED_FIELD, typeof(bool));
                        _MainBookDV.Table.Columns.Add(BookHandler.BLOCK_NUM_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.EXEC_TYPE_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.SYNC_FLAG_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.SIDE_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.SECURITY_ID_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.SYMBOL_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.PARTY_SUB_NAME_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.COUNTER_PARTY_NAME_FILED, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.COUNTER_PARTY_SUB_NAME_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.YIELD_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.PARENTFLAGQTY_FIELD, typeof(long));
                        _MainBookDV.Table.Columns.Add(BookHandler.QTY_FIELD, typeof(long));
                        _MainBookDV.Table.Columns.Add(BookHandler.SETTLE_TYPE_FIELD, typeof(string)); 
                        _MainBookDV.Table.Columns.Add(BookHandler.TRADE_DATE_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.CREATE_DATE_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.SETTLE_DATE_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.EXEC_ID_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.PRICE_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.NUM_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.TERMINAL_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.CLOSE_BY_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.UPDATE_TIME_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.SEND_TIME_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.OPSENABLED_FIELD, typeof(bool));
                        _MainBookDV.Table.Columns.Add(BookHandler.INFO7_FIELD, typeof(string));
                        _MainBookDV.Table.Columns.Add(BookHandler.ISTIP_FIELD, typeof(string));

                        _MainBookDV.Table.PrimaryKey = new DataColumn[] { _MainBookDV.Table.Columns[BookHandler.BLOCK_NUM_FIELD] };
                        _MainBookDV.Sort = $"{BookHandler.UPDATE_TIME_FIELD }";
                    }

                }
                return _MainBookDV;

            }
        }

        /// <summary>
        /// 交易包明细列表资源锁
        /// </summary>
        private object _subBookLockObj = new object();

        /// <summary>
        /// 交易包明细列表数据源
        /// </summary>
        private DataView _SubBookDV;

        public DataView SubBookDV
        {
            get
            {
                lock (_subBookLockObj)
                {
                    if (_SubBookDV == null)
                    {
                        _SubBookDV = new DataView();
                        _SubBookDV.Table = new DataTable("SubBook");
                        _SubBookDV.Table.Columns.Add(BookHandler.BLOCK_NUM_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.EXEC_ID_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.DEALTYPE_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.SIDE_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.SECURITY_ID_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.SYMBOL_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.YIELD_FIELD, typeof(decimal));
                        _SubBookDV.Table.Columns.Add(BookHandler.NETPRICE_FIELD, typeof(decimal));
                        _SubBookDV.Table.Columns.Add(BookHandler.DIRTYPRICE_FIELD, typeof(decimal));
                        _SubBookDV.Table.Columns.Add(BookHandler.QTY_FIELD, typeof(long));
                        _SubBookDV.Table.Columns.Add(BookHandler.SETTLE_TYPE_FIELD, typeof(string)); 
                        _SubBookDV.Table.Columns.Add(BookHandler.TRADE_DATE_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.SETTLE_DATE_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.CREATE_DATE_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.PARTY_SUB_NAME_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.COUNTER_PARTY_NAME_FILED, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.COUNTER_PARTY_SUB_NAME_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.NUM_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.CLOSE_BY_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.UPDATE_TIME_FIELD, typeof(string));
                        _SubBookDV.Table.Columns.Add(BookHandler.SEND_TIME_FIELD, typeof(string));

                        //_SubBookDV.Table.Columns.Add(BookHandler.PRICE_FIELD);
                        //_SubBookDV.Table.Columns.Add(BookHandler.TERMINAL_FIELD);
                      

                        _SubBookDV.Table.PrimaryKey = new DataColumn[] { _SubBookDV.Table.Columns[BookHandler.EXEC_ID_FIELD] };
                        _SubBookDV.Sort = $"{BookHandler.UPDATE_TIME_FIELD }";
                    }

                }
                return _SubBookDV;

            }
        }

        /// <summary>
        /// 交易包主交易包数据回调方法
        /// </summary>
        /// <param name="blocks"></param>
        public void Instance_OnBlocks(List<ExecutionReport> blocks)
        {
            string blocknum = string.Empty;

            if (blocks != null && blocks.Count > 0)
            {
                blocknum = blocks[0].BlockNum;
            }
           
            if (string.IsNullOrEmpty(blocknum) == false)
            {
                lock (string.Intern(blocknum))
                {
                    _CacheBlocks.AddOrUpdate(blocknum, blocks, (key, value) => blocks);
                }
            }

            _OnBlocks?.Invoke(blocks);
        }

        /// <summary>
        /// 销售下拉框数据源
        /// </summary>
        public static List<StcTbookUsers> SalesList = new List<StcTbookUsers>();
        /// <summary>
        /// 自营下拉数据源
        /// </summary>
        public static List<StcTbookUsers> SelfList = new List<StcTbookUsers>();
        //public static List<StcTbookUsers> PublicUserList = new List<StcTbookUsers>();
        /// <summary>
        /// 公账账号
        /// </summary>
        public static StcTbookUsers publicUser;

        /// <summary>
        /// 请求账户信息，初始化销售列表、自营列表、公账账号
        /// </summary>
        public void RequestAccountInfoQuery()
        {
            STCUserReplyPo stcUserReplyPo = AdminHandler.Instance.GetCachestcUserQuery();
            if (stcUserReplyPo == null)
            {
                stcUserReplyPo = AdminHandler.Instance.stcUserQuery();
            }

            if (stcUserReplyPo != null && stcUserReplyPo.Code == "0000" && stcUserReplyPo.Datas.Count > 0)
            {
                SalesList.Clear();
                SelfList.Clear();
                foreach (StcTbookUsers user in stcUserReplyPo.Datas)
                {
                    //// 销售
                    //if (user.Biztypeid == "26")
                    //{
                    //    SalesList.Add(user);
                    //}
                    //// 自营
                    //else if (user.Biztypeid == "25")
                    //{
                    //    SelfList.Add(user);
                    //}
                    if (!string.IsNullOrEmpty(user.Name) && user.Name.Contains("公账"))
                    {
                        //PublicUserList.Add(user);
                        publicUser = user;
                    }
                    // 销售
                    else if (user.Saleflag == "1")
                    {
                        if (!string.IsNullOrEmpty(user.Biztypeid))
                        {
                            SalesList.Add(user);
                        }
                    }
                    // 自营(自营+量化)
                    else
                    {
                        if (!string.IsNullOrEmpty(user.Biztypeid))
                        {
                            SelfList.Add(user);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  检查账户是否存在
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isSale"></param>
        /// <returns></returns>
        public bool CheckUserIsExit(StcTbookUsers user, bool isSale)
        {
            bool result = true;

            STCUserReplyPo stcUserReplyPo = AdminHandler.Instance.GetCachestcUserQuery();
            List<StcTbookUsers> selectResults = null;
            if (isSale)
            {
                selectResults = stcUserReplyPo.Datas.Where(delegate (StcTbookUsers tbookUser)
                {
                    if (tbookUser.Saleflag == "1" && (tbookUser.Biztypeid+"").Equals(user.Biztypeid+"") && (tbookUser.Name+"").Equals(user.Name+"")
                        && (tbookUser.Userid+"").Equals(user.Userid+""))
                        return true;
                    else
                        return false;
                }).ToList();
                
            }
            else
            {
                selectResults = SelfList.Where(delegate (StcTbookUsers tbookUser)
                {
                    if (tbookUser.Saleflag != "1" && (tbookUser.Biztypeid + "").Equals(user.Biztypeid + "") && (tbookUser.Name + "").Equals(user.Name + "")
                        && (tbookUser.Userid + "").Equals(user.Userid + ""))
                        return true;
                    else
                        return false;
                }).ToList();
                
            }

            return result = selectResults != null && selectResults.Count > 0;
            
        }

        /// <summary>
        /// 判断是否是销售角色
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool IsSale(string userName)
        {
            bool isSale = false;

            STCUserReplyPo stcUserReplyPo = AdminHandler.Instance.GetCachestcUserQuery();

            List<StcTbookUsers> selectResults = stcUserReplyPo.Datas.Where(delegate (StcTbookUsers tbookUser)
                {
                    if (!string.IsNullOrEmpty(tbookUser.Biztypeid) && (tbookUser.Name + "").Equals(userName + ""))
                        return true;
                    else
                        return false;
                }).ToList();

            if (selectResults != null && selectResults.Count > 0)
            {
                isSale = selectResults[0].Saleflag == "1";
            }

            return isSale;
        }

        /// <summary>
        /// 根据userName获取用户信息
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="isSale">是否销售</param>
        /// <returns></returns>
        public static StcTbookUsers GetUserByUserName(string userName, bool isSale)
        {
            StcTbookUsers result = null;
            STCUserReplyPo stcUserReplyPo = AdminHandler.Instance.GetCachestcUserQuery();

            List<StcTbookUsers> selectResults = null;
            if (isSale)
            {
                selectResults = stcUserReplyPo.Datas.Where(delegate (StcTbookUsers tbookUser)
                {
                    if (tbookUser.Saleflag == "1" && !string.IsNullOrEmpty(tbookUser.Biztypeid) && (tbookUser.Username + "").Equals(userName + ""))
                        return true;
                    else
                        return false;
                }).ToList();

            }
            else
            {
                selectResults = SelfList.Where(delegate (StcTbookUsers tbookUser)
                {
                    if (tbookUser.Saleflag != "1" && !string.IsNullOrEmpty(tbookUser.Biztypeid) && (tbookUser.Username + "").Equals(userName + ""))
                        return true;
                    else
                        return false;
                }).ToList();

            }

            if (selectResults != null && selectResults.Count > 0)
                result = selectResults[0];

            return result;
        }

        /// <summary>
        /// 根据Name获取用户信息
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="isSale">是否销售</param>
        /// <returns></returns>
        public static StcTbookUsers GetUserByName(string userName, bool isSale)
        {
            StcTbookUsers result = null;
            STCUserReplyPo stcUserReplyPo = AdminHandler.Instance.GetCachestcUserQuery();

            List<StcTbookUsers> selectResults = null;
            if (isSale)
            {
                selectResults = stcUserReplyPo.Datas.Where(delegate (StcTbookUsers tbookUser)
                {
                    if (tbookUser.Saleflag == "1" && !string.IsNullOrEmpty(tbookUser.Biztypeid) && (tbookUser.Name + "").Equals(userName + ""))
                        return true;
                    else
                        return false;
                }).ToList();

            }
            else
            {
                selectResults = SelfList.Where(delegate (StcTbookUsers tbookUser)
                {
                    Console.WriteLine("Saleflag:" + tbookUser.Saleflag + ", Biztypeid:" + tbookUser.Biztypeid + ", name:" + tbookUser.Name);
                    if (tbookUser.Saleflag != "1" && !string.IsNullOrEmpty(tbookUser.Biztypeid) && (tbookUser.Name + "").Equals(userName + ""))
                        return true;
                    else
                        return false;
                }).ToList();

            }

            if (selectResults != null && selectResults.Count > 0)
                result = selectResults[0];

            return result;
        }

        #region 测试数据 
        List<ExecutionReport> datas = new List<ExecutionReport>();
        public void GenerateTestData()
        {
            //主单 也是子单的一部分？

            // 主记录
            ExecutionReport mainRecord = new ExecutionReport();
            mainRecord.TimeInForce = "3";
            mainRecord.ExecID = "main-1";
            mainRecord.SecurityID = "180210";
            mainRecord.Symbol = "18国债82";
            mainRecord.Side = "2";
            mainRecord.SendingTime = "2020-08-26";
            mainRecord.LastPx = "3.25"; // 上部的价格处，填写成交收益率
            mainRecord.LastQty = "4000"; // 上部的数量处， 填写成交数量？,  上部数据可能是左右单子的总和？
            mainRecord.SettlType = "1";

            mainRecord.Venues = "IDB"; // 本方和对手方取哪个字段？


        }
        #endregion
    }
}
