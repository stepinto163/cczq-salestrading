using STC.Common.Handler.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Model.Base;
using System.Data;
using STC.XBond.Plugin.Utils;
using STC.Common.Model.Admin;
using System.Threading;
using STC.Common.Handler.Admin;
using STC.Common.Utils;
using System.Windows;

namespace STC.XBond.Plugin.Setting.Utils.DataProcesser
{
    public class STCUserProcesser
    {
        private static STCUserProcesser proc = new STCUserProcesser();
        public static STCUserProcesser Processer
        {
            get { return proc; }
        }
        private volatile bool isInit = false;
        private STCUserProcesser()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
        }
        private object lockObj = new object();
        public object LockObj
        {
            get
            {
                return lockObj;
            }
        }

        #region 表格字段
        private const string KEY_FIELD = "Key";
        private const string USER_ID_FIELD = "Userid";
        private const string USER_NAME_FIELD = "Username";
        private const string VENUE_USER_SID_FIELD = "Venueusersid";
        private const string NAME_FIELD = "Name";
        private const string PASSWORD_FIELD = "Password";
        private const string ENABLE_FIELD = "Enable";
        private const string REMARK_FIELD = "Remark";
        private const string CREATE_TIME_FIELD = "Createtime";
        private const string UPDATE_TIME_FIELD = "UpdateTime";

        private const string BIZ_TYPE_STR_FIELD = "Biztypestr";
        private const string BIZ_TYPE_ID_FIED = "Biztypeid";
        private const string ROLE_ID_FIED = "Roleid";
        private const string ROLE_NAME_FIED = "Rolename";
        #endregion

        private DataTable _userDT;
        public DataTable UserDT
        {
            get
            {
                lock (LockObj)
                {
                    if (_userDT == null)
                    {
                        _userDT = new DataTable();
                        _userDT.Columns.Add(KEY_FIELD, typeof(string));
                        _userDT.Columns.Add(USER_ID_FIELD, typeof(string));
                        _userDT.Columns.Add(USER_NAME_FIELD, typeof(string));
                        _userDT.Columns.Add(VENUE_USER_SID_FIELD, typeof(string));
                        _userDT.Columns.Add(NAME_FIELD, typeof(string));
                        _userDT.Columns.Add(PASSWORD_FIELD, typeof(string));
                        _userDT.Columns.Add(ENABLE_FIELD, typeof(string));
                        _userDT.Columns.Add(REMARK_FIELD, typeof(string));
                        _userDT.Columns.Add(CREATE_TIME_FIELD, typeof(string));
                        _userDT.Columns.Add(UPDATE_TIME_FIELD, typeof(string));
                        _userDT.Columns.Add(BIZ_TYPE_STR_FIELD, typeof(string));
                        _userDT.Columns.Add(BIZ_TYPE_ID_FIED, typeof(string));
                        _userDT.Columns.Add(ROLE_ID_FIED, typeof(string));
                        _userDT.Columns.Add(ROLE_NAME_FIED, typeof(string));

                        _userDT.PrimaryKey = new DataColumn[] { _userDT.Columns[KEY_FIELD] };

                    }
                    return this._userDT;
                }
            }
        }

        public SortedDictionary<string, StcTbookUsers> UserDic
        {
            get
            {
                return userDic;
            }

            set
            {
                userDic = value;
            }
        }

        private static SortedDictionary<string, StcTbookUsers> userDic;

        private ThreadQueue<StcTbookUsers> _ProcessThreadQueue;
        public void Init()
        {
            if (isInit == false)
            {
                _ProcessThreadQueue = new ThreadQueue<StcTbookUsers>(UpdateUser);
                userDic = new SortedDictionary<string, StcTbookUsers>();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    #region 查询STC USER
                    selectUser();
                    #endregion
                });

                isInit = true;
            }
        }
        private void selectUser()
        {
            STCUserReplyPo reply = AdminHandler.Instance.stcUserQuery();
            if (reply != null && reply.Msg != null)
            {
                foreach (StcTbookUsers po in reply.Msg)
                {
                    if (this._ProcessThreadQueue != null)
                    {
                        this._ProcessThreadQueue.Enqueue(po);
                    }
                }
            }
        }
        private void UpdateUser(StcTbookUsers obj)
        {
            try
            {
                lock (this.lockObj)
                {
                    userDic.Add(obj.Username,obj);
                    DataRow Items = this.UserDT.Rows.Find(obj.Username);
                    if (Items == null)
                    {
                        Items = this._userDT.NewRow();
                        Items[KEY_FIELD] = obj.Username;
                        Items[USER_ID_FIELD] = obj.Userid;
                        Items[USER_NAME_FIELD] = obj.Username;
                        Items[VENUE_USER_SID_FIELD] = obj.Venueusersid;
                        Items[NAME_FIELD] = obj.Name;
                        Items[PASSWORD_FIELD] = obj.Password;
                        Items[ENABLE_FIELD] = obj.Enable;
                        Items[REMARK_FIELD] = obj.Remark;
                        Items[CREATE_TIME_FIELD] = obj.Createtime;
                        Items[UPDATE_TIME_FIELD] = obj.Updatetime;
                        Items[BIZ_TYPE_STR_FIELD] = obj.Biztypestr;
                        Items[BIZ_TYPE_ID_FIED] = obj.Biztypeid;
                        Items[ROLE_ID_FIED] = obj.Roleid;
                        Items[ROLE_NAME_FIED] = obj.Rolename;
                        this.UserDT.Rows.Add(Items);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("UpdateUser", e);
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    this.Init();
                }));
            }
        }
        public void Clear()
        {

            lock (this.lockObj)
            {
                if (_userDT != null)
                {
                    _userDT.Clear();
                }
                if (userDic != null)
                {
                    userDic.Clear();
                }
            }

            if (_ProcessThreadQueue != null)
            {
                _ProcessThreadQueue.Dispose();
                _ProcessThreadQueue = null;
            }

            isInit = false;
        }
        public List<StcTbookUsers> GetTbookUsersPo(string condition, int resultNum)
        {
            List<StcTbookUsers> result = new List<StcTbookUsers>();
            
            if (userDic == null)
            {
                return result;
            }
            lock (userDic)
            {
                var v = (from d in userDic where d.Key.ToLower().Contains(condition.ToLower()) select d);

                int i = 0;
                foreach (KeyValuePair<string, StcTbookUsers> kv in v)
                {
                    if (i < resultNum)
                        result.Add(kv.Value);
                    else
                        break;

                    i++;
                }
            }

            return result;
        }
    }
}
