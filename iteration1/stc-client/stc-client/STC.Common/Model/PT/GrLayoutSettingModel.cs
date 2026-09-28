using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PT
{
    /// <summary>
    /// 默认卡片2*4排布
    /// </summary>
    public class GrLayoutSettingModel
    {
        private int showmodel = 2;

        /// <summary>
        /// 1:列表
        /// 2:卡片
        /// </summary>
        public int ShowModel 
        {
            get
            {
                return showmodel;
            }

            set
            {
                showmodel = value;
            }
        }

        private int row = 2;
        public int Row
        {
            get
            {
                return row == 0 ? 2 : row;
            }
            set
            {
                row = value;
            }
        }

        private int col = 5;
        public int Col
        {
            get
            {
                return col == 0 ? 5 : col; 
            }
            set
            {
                col = value;
            }
        }

        private int quotesshowmode = 1;
        /// <summary>
        /// 1:完整模式
        /// 2:简洁模式
        /// </summary>
        public int QuotesShowMode
        {
            get
            {
                return quotesshowmode;
            }

            set
            {
                quotesshowmode = value;
            }
        }
    }
}
