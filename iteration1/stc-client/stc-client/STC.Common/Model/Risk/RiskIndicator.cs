using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    public class RiskIndicator
    {
        public String user_id;//用户ID

        public String long_pos_limit;//利率债-多头合计净头寸

        public String long_pos_limit_settl;//利率债-多头合计结算头寸

        public String short_pos_limit;//利率债-空头合计净头寸

        public String short_pos_limit_settl;//利率债-空头合计结算头寸

        public String max_re_dv01_settl;//利率债-风险敞口结算头寸DV01最大值

        public String min_re_dv01_settl;//利率债-风险敞口结算头寸DV01最小值

        public String max_re_dv01;//利率债-风险敞口净头寸DV01最大值用户

        public String min_re_dv01;//利率债-风险敞口净头寸DV01最小值

        public String max_dv01_settl;//利率债-债券结算头寸DV01最大值

        public String min_dv01_settl;//利率债-债券结算头寸DV01最小值

        public String max_dv01;//利率债-债券净头寸DV01最大值

        public String min_dv01;//利率债-债券净头寸DV01最小值

        public String credit_dv01;//信用债dv01结算

        public String credit_limit;//信用债规模结算

        public String irs_limit;//irs名义本金限额

        public String bond_futs_pos_limit;//国债期货限额

        public String total_pos_limit;//总限额



        public String long_pos_realTime;//利率债-多头合计净头寸实时值
        public String long_pos_realTime_settl;//利率债-多头合计结算头寸实时值
        public String short_pos_realTime;//利率债-空头合计净头寸实时值
        public String short_pos_realTime_settl;//利率债-空头合计结算头寸实时值
        public String pos_realTime;//利率债-净仓位合计净头寸实时值
        public String pos_realTime_settl;//利率债-净仓位合计结算头寸实时值
        public String ls_pos_realTime;//利率债-多空仓位合计净头寸实时值
        public String ls_pos_realTime_settl;//利率债-多空仓位合计结算头寸实时值
        public String re_dv01_settl_realTime;//利率债-风险敞口结算头寸DV01实时值
        public String re_dv01_realTime;//利率债-风险敞口净头寸DV01实时值
        public String dv01_settl_realTime;//利率债-债券结算头寸DV01实时值
        public String dv01_realTime;//利率债-债券净头寸DV01实时值
        public String credit_dv01_realTime;//信用债dv01实时值
        public String credit_realTime;//信用债规模实时值
        
        public String bond_size_realTime;//债券规模实时值-净
        public String bond_size_settl_realTime;//债券规模实时值-结算
    }
}
