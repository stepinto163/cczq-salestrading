using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Model
{
    public enum DataType
    {
        /// <summary>
        /// 后台DB
        /// </summary>
        DB, 
        /// <summary>
        /// 缓存sqlite
        /// </summary>
        SQLite,
        /// <summary>
        /// 只创建新的空分页
        /// </summary>
        AddEmptyPage
    }
}
