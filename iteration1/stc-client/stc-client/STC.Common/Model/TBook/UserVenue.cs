using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.TBook
{
    public class UserVenue
    {

        public string firmtonontime { get; set; }//		

        public string venuesrdmnum { get; set; }//				
        /// <summary>
        /// 选中的渠道
        /// </summary>
        public string selfselectvenues { get; set; }//			
        /// <summary>
        /// 1:不指定
        /// 2:默认
        /// 3:指定
        /// </summary>
        public string venuesselectf { get; set; }//		

        public string type { get; set; }//BNC,BCO

        public int Level { get; set; }
        public int Seq { get; set; }
    }
}
