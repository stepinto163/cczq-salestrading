using com.gateway.log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Common.Utils.SqlLite;

namespace BDB.Trade.Plugin.Utils
{
    public class CardFileManager
    {
        private string dbFile = AppDomain.CurrentDomain.BaseDirectory + @"data\Card.data";


        private static CardFileManager cardFileManager = new CardFileManager();
        public static CardFileManager Instance { get { return cardFileManager; } }

        private CardFileManager()
        {
            // 初始读取数据
            LoadAllConfig();
        }

        List<UserConfigPo> allUserConfig = new List<UserConfigPo>();

        public void LoadAllConfig()
        {
            try
            {
                lock (allUserConfig)
                {
                    if (File.Exists(dbFile))
                    {
                        using (StreamReader reader = new StreamReader(dbFile))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                try
                                {
                                    line = netapi.Api.EncrypterHelper.AesDecrypt(line, string.Empty);
                                    UserConfigPo userConfigPo = JsonConvert.DeserializeObject<UserConfigPo>(line);

                                    allUserConfig.Add(userConfigPo);
                                }
                                catch (Exception e)
                                { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public List<UserConfigPo> GetAllConfigByCondtion(string userName, string groupId)
        {
            List<UserConfigPo> filterList = new List<UserConfigPo>();
            lock (allUserConfig)
            {
                foreach (UserConfigPo userConfigPo in allUserConfig)
                {
                    bool userNameOk = true;

                    if (!string.IsNullOrEmpty(userName) && !userConfigPo.UserName.Equals(userName))
                    {
                        userNameOk = false;
                    }


                    bool groupIdOk = true;

                    if (!string.IsNullOrEmpty(groupId) && !userConfigPo.GroupId.Equals(groupId))
                    {
                        groupIdOk = false;
                    }

                    if (groupIdOk && userNameOk)
                    {
                        filterList.Add(userConfigPo);
                    }
                }

                return filterList;
            }
        }


        public void DeleteBond()
        {
            try
            {
                lock (allUserConfig)
                {
                    if (File.Exists(dbFile))
                    {
                        File.Delete(dbFile);
                    }

                    allUserConfig.Clear();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }


        public void Delete(string userName)
        {
            lock (allUserConfig)
            {
                if (allUserConfig != null && allUserConfig.Count > 0)
                {
                    List<string> newConfig = new List<string>();
                    List<UserConfigPo> removeList = new List<UserConfigPo>();
                    foreach (UserConfigPo userConfigPo in allUserConfig)
                    {
                        if (!string.IsNullOrEmpty(userName) && userConfigPo.UserName.Equals(userName) && userConfigPo.GroupId != "Mode")
                        {
                            //删除
                            removeList.Add(userConfigPo);
                        }
                        else
                        {
                            string line = JsonConvert.SerializeObject(userConfigPo);
                            line = netapi.Api.EncrypterHelper.AesEncrypt(Encoding.UTF8.GetBytes(line), string.Empty);
                            newConfig.Add(line);
                        }
                    }
                    foreach (UserConfigPo userConfigPo in removeList)
                    {
                        allUserConfig.Remove(userConfigPo);
                    }

                    File.WriteAllLines(dbFile, newConfig.ToArray());
                }
            }
        }

        public void DeleteConfigByGroupId(string userName, string groupId)
        {
            lock (allUserConfig)
            {
                if (allUserConfig != null && allUserConfig.Count > 0)
                {
                    List<string> newConfig = new List<string>();
                    List<UserConfigPo> removeList = new List<UserConfigPo>();
                    foreach (UserConfigPo userConfigPo in allUserConfig)
                    {
                        if (!string.IsNullOrEmpty(userName) && userConfigPo.UserName.Equals(userName) && userConfigPo.GroupId.Equals(groupId))
                        {
                            //删除
                            removeList.Add(userConfigPo);
                        }
                        else
                        {
                            string line = JsonConvert.SerializeObject(userConfigPo);
                            line = netapi.Api.EncrypterHelper.AesEncrypt(Encoding.UTF8.GetBytes(line), string.Empty);
                            newConfig.Add(line);
                        }
                    }
                    foreach (UserConfigPo userConfigPo in removeList)
                    {
                        allUserConfig.Remove(userConfigPo);
                    }


                    File.WriteAllLines(dbFile, newConfig.ToArray());
                }
            }
        }


        public void Insert(UserConfigPo data)
        {
            try
            {
                lock (allUserConfig)
                {
                    List<string> newConfig = new List<string>();
                    bool isContain = false;
                    if (allUserConfig != null && allUserConfig.Count > 0)
                    {
                        foreach (UserConfigPo userConfigPo in allUserConfig)
                        {
                            string line = string.Empty;
                            if (userConfigPo.UserName.Equals(data.UserName) && userConfigPo.GroupId.Equals(data.GroupId))
                            {
                                line = JsonConvert.SerializeObject(data);

                                isContain = true;

                                userConfigPo.Config = data.Config;
                            }
                            else
                            {
                                line = JsonConvert.SerializeObject(userConfigPo);
                            }

                            line = netapi.Api.EncrypterHelper.AesEncrypt(Encoding.UTF8.GetBytes(line), string.Empty);
                            newConfig.Add(line);
                        }
                    }
                    if (!isContain)
                    {
                        allUserConfig.Add(data);
                        string line = JsonConvert.SerializeObject(data);
                        line = netapi.Api.EncrypterHelper.AesEncrypt(Encoding.UTF8.GetBytes(line), string.Empty);
                        newConfig.Add(line);
                    }

                    File.WriteAllLines(dbFile, newConfig.ToArray());
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

    }
}
