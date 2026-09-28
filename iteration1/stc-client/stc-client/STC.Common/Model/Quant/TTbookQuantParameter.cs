using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{/// <summary>
 /// 量化策略参数
 /// </summary>
 /// <remarks>
 /// 2021.10.18: 创建. Csharp代码生成插件(V1.1.7887.31363(build:20210805)) 模板版本(v1.0.0.0)<br/>
 /// </remarks>
    [Serializable]
    public class TTbookQuantParameter : QuantParameter, ICloneable
    {

        public object Clone()
        {
            TTbookQuantParameter ret = new TTbookQuantParameter();
            ret.Id = this.Id;
            ret.Quantid = this.Quantid;
            ret.Param_Key = this.Param_Key;
            ret.Param_Type = this.Param_Type;
            ret.Param_Value = this.Param_Value;
            ret.Param_Desc = this.Param_Desc;
            ret.Action = this.Action;
            ret.Username = this.Username;
            ret.Terminal = this.Terminal;
            ret.Ip = this.Ip;
            ret.Closeby = this.Closeby;
            ret.Createtime = this.Createtime;
            ret.Updatetime = this.Updatetime;
            ret.Inf1 = this.Inf1;
            ret.Inf2 = this.Inf2;
            ret.Inf3 = this.Inf3;
            ret.Inf4 = this.Inf4;
            ret.Inf5 = this.Inf5;
            ret.Para_Unit = this.Para_Unit;
            ret.Para_Optional_Values = this.Para_Optional_Values;
            ret.Para_Prompt_Pormat = this.Para_Prompt_Pormat;
            ret.B_Text_Box_String = this.B_Text_Box_String;
            ret.Is_Independent_Para = this.Is_Independent_Para;
            ret.Sub_Param = this.Sub_Param;
            ret.Param_Check_Value = this.Param_Check_Value;
            ret.Param_Desc_Color = this.Param_Desc_Color;
            ret.Para_Type = this.Para_Type;

            return ret;
        }
    }
}