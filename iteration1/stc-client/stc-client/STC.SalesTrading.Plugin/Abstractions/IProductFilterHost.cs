using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Ficc.Protocol;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>现有产品侧栏与页面查询之间的强类型边界。</summary>
    public interface IProductFilterHost
    {
        Task<DictionaryData> GetFilterDictionaryAsync();
        void ApplyProductFilter(string scope, Filter filter);
    }

    public interface IProductFilterCatalog
    {
        Task<DictionaryData> GetFilterDictionaryAsync();
    }
}
