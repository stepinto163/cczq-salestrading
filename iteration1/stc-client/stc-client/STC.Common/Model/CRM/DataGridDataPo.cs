using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.CRM
{
    public class DataGridDataPo
    {
        /// <summary>
        /// 机构IDB唯一标识
        /// </summary>
        public string InstIdbKey { get; set; }
        /// <summary>
        /// 交易员IDB唯一标识
        /// </summary>
        public string IdbKey { get; set; }
        /// <summary>
        /// v2.3格式：机构(交易员名字)
        /// </summary>
        public string InstTraderName { get; set; }
        /// <summary>
        /// 交易员名字
        /// </summary>
        public string TraderName { get; set; }
        /// <summary>
        /// 机构简称
        /// </summary>
        public string InstName { get; set; }
        /// <summary>
        /// 机构台子简称
        /// </summary>
        public string InstShortName { get; set; }
        /// <summary>
        /// 机构名称拼音：用于检索
        /// </summary>
        public string PinYin { get; set; } = string.Empty;
        /// <summary>
        /// 机构名称全拼
        /// </summary>
        public string PinYinFull { get; set; } = string.Empty;
        /// <summary>
        /// 机构代码
        /// </summary>
        public string instcode { get; set; }
        /// <summary>
        /// 机构QB唯一标识
        /// </summary>
        public string Instqbid { get; set; }
        /// <summary>
        /// 交易员QB唯一标识
        /// </summary>
        public string Traderqbid { get; set; }

        /// <summary>
        /// 管理者
        /// </summary>
        public string Managers { get; set; }

        /// <summary>
        /// 管理者
        /// </summary>
        public List<string> ManagerList
        {
            get
            {
                List<string> vs = new List<string>();
                if (string.IsNullOrWhiteSpace(Managers))
                {
                    return vs;
                }
                int indext = Managers.IndexOf(',');
                if (!string.IsNullOrWhiteSpace(Managers))
                {
                    if (indext != -1)
                    {
                        string[] vs1 = Managers.Split(',');
                        foreach (string item in vs1)
                        {
                            int index2 = item.IndexOf(':');
                            if (index2 != -1)
                            {
                                string[] vs2 = item.Split(':');
                                if (vs2[1] == "33")
                                {
                                    vs.Add(vs2[0]);
                                }
                            }
                        }

                    }
                    else
                    {
                        int index2 = Managers.IndexOf(':');
                        if (index2 != -1)
                        {
                            string[] vs2 = Managers.Split(':');
                            if (vs2[1] == "33")
                            {
                                vs.Add(vs2[0]);
                            }
                        }
                    }
                }
                return vs;
            }
        }
    }
}

