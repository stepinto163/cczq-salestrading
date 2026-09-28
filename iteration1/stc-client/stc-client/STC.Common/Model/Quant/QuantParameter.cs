using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    public class QuantParameter
    {
        #region 字段
        private string _id = "";//数据标识
        private string _quantid = "";//量化编号
        private string _paramKey = "";//参数key
        private string _paramType;//参数类型(""-输入框；Text-文本域；Radio-单选框；ComboBox-下拉框；DatePicker-日期文本)
        private string _paramValue;//参数
        private string _paramDesc;//说明
        private string _action;//
        private string _username;//stc用户
        private string _terminal;//终端
        private string _ip;//服务器ip
        private string _closeby;//操作人
        private string _createtime;//创建时间
        private string _updatetime;//更新时间
        private string _inf1;//扩展字段1
        private string _inf2;//扩展字段2
        private string _inf3;//扩展字段3
        private string _inf4;//扩展字段4
        private string _inf5;//扩展字段5

        private string para_unit;//单位
        private List<ParaOptionalValues> para_optional_values = new List<ParaOptionalValues>();//下拉框或单选框参数可选值
        private string para_prompt_format;//文本框内参数输入提示
        private string b_text_box_string;//文本框前提示字符 如”-”和”+”等字符
        private bool is_independent_para;//是否有子参数
        private List<SubParam> sub_param;//子参数

        private string param_check_value;//数据check范围
        private string param_desc_color;//参数字体颜色


        private string para_type;

        #endregion

        #region 属性
        public string Para_Type
        {
            get { return para_type; }
            set { para_type = value; }
        }
        /// <summary>
        /// 数据标识
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
        /// 参数key
        /// </summary>
        public string Param_Key
        {
            get { return _paramKey; }
            set { _paramKey = value; }
        }

        /// <summary>
        /// 参数类型
        /// </summary>
        public string Param_Type
        {
            get { return _paramType; }
            set { _paramType = value; }
        }

        /// <summary>
        /// 参数
        /// </summary>
        public string Param_Value
        {
            get { return _paramValue; }
            set { _paramValue = value; }
        }

        /// <summary>
        /// 说明
        /// </summary>
        public string Param_Desc
        {
            get { return _paramDesc; }
            set { _paramDesc = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Action
        {
            get { return _action; }
            set { _action = value; }
        }

        /// <summary>
        /// stc用户
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
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

        /// <summary>
        /// 更新时间
        /// </summary>
        public string Updatetime
        {
            get { return _updatetime; }
            set { _updatetime = value; }
        }

        /// <summary>
        /// 扩展字段1
        /// </summary>
        public string Inf1
        {
            get { return _inf1; }
            set { _inf1 = value; }
        }

        /// <summary>
        /// 扩展字段2
        /// </summary>
        public string Inf2
        {
            get { return _inf2; }
            set { _inf2 = value; }
        }

        /// <summary>
        /// 扩展字段3
        /// </summary>
        public string Inf3
        {
            get { return _inf3; }
            set { _inf3 = value; }
        }

        /// <summary>
        /// 扩展字段4
        /// </summary>
        public string Inf4
        {
            get { return _inf4; }
            set { _inf4 = value; }
        }

        /// <summary>
        /// 扩展字段5
        /// </summary>
        public string Inf5
        {
            get { return _inf5; }
            set { _inf5 = value; }
        }

        public List<SubParam> Sub_Param
        {
            get { return sub_param; }
            set { sub_param = value; }
        }

        public bool Is_Independent_Para
        {
            get { return is_independent_para; }
            set { is_independent_para = value; }
        }
        public string B_Text_Box_String
        {
            get { return b_text_box_string; }
            set { b_text_box_string = value; }
        }
        public string Para_Prompt_Pormat
        {
            get { return para_prompt_format; }
            set { para_prompt_format = value; }
        }
        public List<ParaOptionalValues> Para_Optional_Values
        {
            get { return para_optional_values; }
            set { para_optional_values = value; }
        }
        public string Para_Unit
        {
            get { return para_unit; }
            set { para_unit = value; }
        }

        public string Param_Check_Value
        {
            get { return param_check_value; }
            set { param_check_value = value; }
        }
   
        public string Param_Desc_Color
        {
            get { return param_desc_color; }
            set { param_desc_color = value; }
        }

        #endregion
    }
}
