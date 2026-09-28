using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace STC.SalesTrading.Plugin.Ficc
{
    public interface IFiccTransport
    {
        Task<JObject> RequestAsync(string service, string topic, JObject request);
        IDisposable Subscribe(string service, string topic, Action<JObject> receive, bool withImage = false);
        event Action ConnectionChanged;
    }
}
