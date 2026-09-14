using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    // Read-only requests through the real C# DPS client and STC gateway.
    static class ServiceDiscoveryProbe {
        public static int Run(string outputPath) {
            try {
                using (var client = new DpsClient()) {
                    client.Connect("demo");
                    var quotes = client.Data("ficc.quote.query", new JObject {
                        ["ownerScope"] = "MINE",
                        ["limit"] = 1
                    });
                    var prices = client.Data("ficc.price.query", new JObject {
                        ["securityIds"] = new JArray("250001.IB"),
                        ["mode"] = "BEST"
                    });
                    var preference = client.Data("ficc.notification.preference.get", new JObject());
                    if (quotes["quotes"] == null || prices["prices"] == null || preference["preference"] == null) {
                        throw new InvalidOperationException("Service response is missing its business data.");
                    }
                    File.WriteAllText(outputPath, new JObject {
                        ["result"] = "PASS",
                        ["checks"] = 3,
                        ["quotesCreated"] = 0,
                        ["messageLogDirectory"] = client.LogDirectory
                    }.ToString());
                    return 0;
                }
            } catch (Exception error) {
                File.WriteAllText(outputPath, new JObject {
                    ["result"] = "FAIL",
                    ["error"] = error.Message
                }.ToString());
                return 1;
            }
        }
    }
}
