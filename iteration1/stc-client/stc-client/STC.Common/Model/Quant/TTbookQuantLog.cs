using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{   
    /// <summary>
    /// 量化日志
    /// </summary>
    /// <remarks>
    /// 2021.10.19: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
    /// </remarks>
    [Serializable]
    public class TTbookQuantLog
    {
        #region 字段

        private string _id = "";//id
        private string _quantid = "";//量化编号
        private string _quantname;//策略名
        private string _type;//类型
        private string _content;//说明
        private string _terminal;//终端
        private string _ip;//服务器ip
        private string _closeby;//操作人
        private string _createtime;//创建时间

        #endregion

        #region 属性

        /// <summary>
        /// id
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// 量化编号
        /// </summary>
        public string Quantid
        {
            get { return _quantid; }
            set { _quantid = value; }
        }

        /// <summary>
        /// 策略名
        /// </summary>
        public string Quantname
        {
            get { return _quantname; }
            set { _quantname = value; }
        }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// 说明
        /// </summary>
        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        /// <summary>
        /// 终端
        /// </summary>
        public string Terminal
        {
            get { return _terminal; }
            set { _terminal = value; }
        }

        /// <summary>
        /// 服务器ip
        /// </summary>
        public string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Closeby
        {
            get { return _closeby; }
            set { _closeby = value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string Createtime
        {
            get { return _createtime; }
            set { _createtime = value; }
        }


        #endregion

    }
}