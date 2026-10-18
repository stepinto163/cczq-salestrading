namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>
    /// CRM 联想候选（ficc.base.query records 的 UI 子集，H43）。
    /// 选中即同时回填 <see cref="Id"/> 与标准名称——后端 confirm 只认 Id。
    /// </summary>
    public class CrmDirectoryEntry
    {
        /// <summary>INSTITUTION→institutionId / CONTACT→contactId。</summary>
        public string Id { get; set; }

        /// <summary>机构全称 / 联系人姓名（回填显示文本）。</summary>
        public string Name { get; set; }

        /// <summary>机构简称（列表副文本，可空）。</summary>
        public string ShortName { get; set; }

        /// <summary>仅 CONTACT：联系人可归属的机构 Id 集合。</summary>
        public System.Collections.Generic.IReadOnlyList<string> InstitutionIds { get; set; }

        /// <summary>仅 CONTACT：主所属机构 Id（可空）。</summary>
        public string InstitutionId { get; set; }

        /// <summary>仅 CONTACT：岗位（可空）。</summary>
        public string PositionName { get; set; }
    }
}
