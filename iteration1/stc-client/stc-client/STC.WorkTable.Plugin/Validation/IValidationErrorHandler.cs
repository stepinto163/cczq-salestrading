using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.WorkTable.Plugin.Validation
{
    interface IValidationErrorHandler
    {
        /// <summary>
        /// 是否通过验证
        /// </summary>
        bool IsValid { get; set; }

        /// <summary>
        /// 错误列表
        /// </summary>
        Dictionary<UIElement, int> ErrorList { get; set; }
    }
}
