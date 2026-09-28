using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Handler
{
    public class PermisionHandler
    {
        private static PermisionHandler _permisionHandler = new PermisionHandler();
        public static PermisionHandler Instance { get { return _permisionHandler; } }

        private List<string> brokerList = new List<string>();
        
        private PermisionHandler()
        {
            brokerList.Add("TPSC");
            brokerList.Add("CNEX");
            brokerList.Add("CCTB");
            brokerList.Add("PTCN");
            brokerList.Add("CTTJ");
            brokerList.Add("UEDA");
        }

        public List<string> GetAllBrokerList()
        {
            return brokerList;
        }

        public static bool _IsMoveMessage = false;

        public bool CheckQbPermission(string broker)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.IDCPermissions))
            {
                string[] arr = LocalEnv.UserInfo.IDCPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (arr != null && arr.Length > 0)
                {
                    foreach (string str in arr)
                    {
                        string cn = ComboSourceUtil.Instance.FindEnum(str, "venues_en");
                        if (cn == broker)
                        {
                            result = true;
                            break;
                        }

                    }
                }
            }
            return result;
        }

        private List<string> DictBondPermission;
        private object loginLock = new object();
        public List<string> GetBondPermission()
        {
            lock (loginLock)
            {
                if (DictBondPermission == null)
                {
                    DictBondPermission = new List<string>();

                    if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BonDPermissions))
                    {
                        string[] arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (arr != null && arr.Length > 0)
                        {
                            foreach (string str in arr)
                            {
                                string en = ComboSourceUtil.Instance.FindEnum(str, "venues_en");
                                if (en != str)
                                {
                                    DictBondPermission.Add(en);
                                }
                            }
                        }
                    }
                }

                return DictBondPermission;
            }
        }

        public bool CheckBrokerPermissionByEN(string brokerEN)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BonDPermissions))
            {
                string[] arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (arr != null && arr.Length > 0)
                {
                    foreach (string str in arr)
                    {
                        string cn = ComboSourceUtil.Instance.FindEnum(str, "venues_en");

                        if (cn == brokerEN)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public bool CheckBrokerPermissionByCN(string brokerCN)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BonDPermissions))
            {
                string[] arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (arr != null && arr.Length > 0)
                {
                    foreach (string str in arr)
                    {
                        string cn = ComboSourceUtil.Instance.FindEnum(str, "venues_cn_tocn");

                        if (cn == brokerCN)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public bool GetIdcTraderPermission()
        {
            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.IdcTradePermissions) && LocalEnv.UserInfo.IdcTradePermissions == "1")
                return true;
            else
                return false;
        }

        private bool CheckBroker(string[] arr, string broker)
        {
            bool result = false;
            if (arr != null && arr.Length > 0)
            {
                foreach (string str in arr)
                {
                    string cn = ComboSourceUtil.Instance.FindEnum(str, "venues_en");
                    if (cn == broker)
                    {
                        result = true;
                        break;
                    }

                }
            }

            return result;
        }

        public bool CheckTradePermission(string broker)
        {
            bool result = false;


            string[] arr = null;

            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.ConnectPermissions))
                arr = LocalEnv.UserInfo.ConnectPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            result = CheckBroker(arr, broker);

            if (result)
            {
                arr = null;

                if (!string.IsNullOrEmpty(LocalEnv.UserInfo.IDCPermissions))
                    arr = LocalEnv.UserInfo.IDCPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                result = CheckBroker(arr, broker);
            }

            if (result)
            {
                arr = null;

                if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BondTradePermissions))
                    arr = LocalEnv.UserInfo.BondTradePermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                result = CheckBroker(arr, broker);
            }

            return result;
        }

        public int GetBrokerCount()
        {
            string[] arr = null;
            int count = 0;
            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BonDPermissions))
            {
                arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> result = new List<string>();
                foreach (string str in arr)
                {
                    string cn = ComboSourceUtil.Instance.FindEnum(str, "venues_en");

                    if (cn != str)
                    {
                        result.Add(str);
                    }
                }

                count = result.Count;
            }


            return count;
        }

        public bool CheckBrokerPosition(string broker)
        {
            List<string> brokerSortList = GetBroker();

            bool result = false;
            foreach (string str in brokerSortList)
            {
                string cn = ComboSourceUtil.Instance.FindEnum(str, "venues_en");
                
                if (cn.Equals(broker))
                {
                    result = true;

                    break;
                }

                
            }

            return result;
        }
        
        public List<string> GetBroker()
        {
            List<string> list = new List<string>();

            string[] arr = null;

            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BonDPermissions))
                arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (arr != null)
            {

                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    string cn = ComboSourceUtil.Instance.FindEnum(arr[i], "venues_en");

                    if (cn != arr[i])
                    {
                        list.Add(arr[i]);
                    }
                }
            }

            return list;
        }

        public List<string> GetBrokerEN()
        {
            List<string> list = new List<string>();

            string[] arr = null;

            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BonDPermissions))
                arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (arr != null)
            {

                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    string cn = ComboSourceUtil.Instance.FindEnum(arr[i], "venues_en");

                    list.Add(cn);
                }
            }

            return list;
        }


        public int GetBrokerIndex(string venues)
        {
            List<string> brokers = GetBrokerENByBrokerSort();// GetBrokerEN();

            return brokers.IndexOf(venues);
        }

        public List<string> GetBrokerENByBrokerSort()
        {
            List<string> list = new List<string>();

            string[] arr = null;

            if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BrokerSort))
                arr = LocalEnv.UserInfo.BrokerSort.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (arr != null)
            {

                foreach (string str in arr)
                {
                    if (CommonConstants.BrokerNumEnum.Contains(str))
                    {
                        string cn = ComboSourceUtil.Instance.FindEnum(str, "venues_en");

                        list.Add(cn);
                    }
                }
            }

            return list;
        }


        public string GetBrokerByIndex(int index)
        {
            List<string> brokers = GetBrokerENByBrokerSort();//GetBrokerEN();

            return ((index <= brokers.Count) && index >= 0) ? brokers[index-1] : string.Empty;
        }
    }
}
