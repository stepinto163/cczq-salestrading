using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model
{
    /// <summary>
    /// 菜单属性
    /// </summary>
    public class MenuPo
    {
        public string ShowImageSource { get; set; }
        public string Txt { get; set; }

        public int PluginKey { get; set; }

        public string ShowSort { get; set; }
    }

    public class MenuSort : IComparer<MenuPo>
    {
        public int Compare(MenuPo x, MenuPo y)
        {
            return CommonUtils.ConvertToInt(x.ShowSort).CompareTo(CommonUtils.ConvertToInt(y.ShowSort));
        }
        
    }
}
