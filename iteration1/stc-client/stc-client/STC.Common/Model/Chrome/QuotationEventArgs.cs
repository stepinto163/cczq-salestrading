using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Chrome
{
    /// <summary>
    /// 行情通知，骆驼命名法
    /// </summary>
    /// <remarks>行情通知Moel，用于和Js通讯</remarks>
    public class QuotationEventArgs
    {
        /// <summary>
        /// 行情数据 Json 格式
        /// </summary>
        public DataTable jsonData;

        /// <summary>
        /// 表格名称
        /// </summary>
        public string tableName;

        /// <summary>
        /// 改变类型 System.ComponentModel.ListChangedType  
        /// </summary>
        /// <remarks>Reset,ItemChanged,ItemAdded,ItemDeleted</remarks>
        public string listChangedType;

        /// <summary>
        /// 旧行索引
        /// </summary>
        public int oldIndex = -1;

        /// <summary>
        /// 新行索引
        /// </summary>
        public int newIndex = -1;
    }
}
