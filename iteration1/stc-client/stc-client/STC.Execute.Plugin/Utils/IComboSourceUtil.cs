using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Execute.Plugin.Utils
{
    /// <summary>
    /// 枚举工具类接口
    /// </summary>
    public interface IComboSourceUtil
    {
        /// <summary>
        /// 清空Combobox数据.
        /// </summary>
        void ClearComboSource();

        /// <summary>
        /// 取得Combobox数据.
        /// </summary>
        /// <param name="name">字段名.</param>
        /// <returns>显示数据列表.</returns>
        List<KeyValuePair<string, string>> GetComboSource(string name);

        /// <summary>
        /// 找出枚举对应值
        /// </summary>
        /// <param name="key">枚举键值</param>
        /// <param name="param">枚举类型</param>
        /// <returns></returns>
        string FindEnum(string key, string param);
    }
}
