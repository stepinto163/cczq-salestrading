using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.ViewModel.UserSetting
{
    /// <summary>
    /// 默认卡片2*4排布
    /// </summary>
    public class LayoutSettingPo
    {
        private int showModel = 1;

        /// <summary>
        /// 1:列表
        /// 2:卡片
        /// </summary>
        public int ShowModel { get; set; }

        private int row = 2;
        public int Row { get { return row==0?2:row; } set { row = value; } }

        private int col = 6;
        public int Col { get { return col==0?6:col; } set { col = value; } }
    }
}
