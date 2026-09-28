using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class TbookAccount
    {
        public String id;

        /**		
         * '账户名称'		
         */
        public String accountname;

        /**		
         * '描述'		
         */
        public String remark;

        /**		
         * '操作人'		
         */
        public String closeby;

        /**		
         * '创建时间'		
         */
        public String createtime;

        /**		
         * '更新时间'		
         */
        public String updatetime;

        /**		
         * '更新时间'		
         */
        public List<String> userId;

        /**		
         * '更新时间'		
         */
        public List<String> userName;

        /**		
         * '扩展字段1'		
         */
        public String inf1;


        /**		
         * '扩展字段2'		
         */
        public String inf2;

        /**		
         * '扩展字段3'		
         */
        public String inf3;

        /**		
         * '扩展字段4'		
         */
        public String inf4;

        /**		
         * '扩展字段5'		
         */
        public String inf5;
    }
}
