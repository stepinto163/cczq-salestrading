namespace STC.Common.Model.Position
{
    /// <summary>
    /// 组合持仓查询
    /// </summary>
    public class QueryPositionReq
    {
        public string bookid { get; set; }
        public string securityid { get; set; }

        public string settlDate { get; set; }
    }
}
