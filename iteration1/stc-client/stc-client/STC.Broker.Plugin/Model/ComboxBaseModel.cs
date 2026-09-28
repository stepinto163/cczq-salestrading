using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model
{
    public class ComboxBaseModel
    {
        /// <summary>
        /// 显示列
        /// </summary>
        public string DisplayMemberPath { get; set; }

        /// <summary>
        /// 值列
        /// </summary>
        public string ValueMemberPath { get; set; }

        /// <summary>
        /// 组件列
        /// </summary>
        public string KeyMemberPath { get; set; }
    }
}
