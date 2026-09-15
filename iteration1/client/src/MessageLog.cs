using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    // Shared by UI and automated tests. No truncation of business message bodies.
    static class MessageLog {
        static readonly object sync=new object();
        static StreamWriter writer;static int part;static long sequence;
        public static readonly string DirectoryPath=ResolveDirectory();
        public static string CurrentFile {get;private set;}
        static string ResolveDirectory(){
            var current=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while(current!=null){if(File.Exists(Path.Combine(current.FullName,"SalesTradingClient.csproj"))||File.Exists(Path.Combine(current.FullName,"DpsApiDebugger.csproj")))return Path.Combine(current.Parent.FullName,"runtime","client-logs",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Process.GetCurrentProcess().Id);current=current.Parent;}
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"FiccSalesTradingClient","logs",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Process.GetCurrentProcess().Id);
        }
        static readonly HashSet<string> sensitiveFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "password", "passwd", "authorization", "accessToken", "refreshToken"
        };

        static JToken Redact(JToken token) {
            JToken copy = token?.DeepClone();
            RedactCopy(copy);
            return copy;
        }

        static void RedactCopy(JToken token) {
            // The log owns this copy. Visit each node once without copying its subtree again.
            if (token is JObject value) {
                foreach (JProperty property in value.Properties()) {
                    if (sensitiveFields.Contains(property.Name)) {
                        property.Value = "[REDACTED]";
                    } else {
                        RedactCopy(property.Value);
                    }
                }
            } else if (token is JArray array) {
                foreach (JToken element in array) {
                    RedactCopy(element);
                }
            }
        }
        public static void Write(string clientId,string user,string direction,string kind,string topic,JToken body,string serverName=null){
            lock(sync){
                if(writer==null||writer.BaseStream.Position>=10*1024*1024){
                    writer?.Dispose();Directory.CreateDirectory(DirectoryPath);
                    CurrentFile=Path.Combine(DirectoryPath,"messages-"+(++part).ToString("D3")+".jsonl");
                    writer=new StreamWriter(new FileStream(CurrentFile,FileMode.CreateNew,FileAccess.Write,FileShare.Read),new UTF8Encoding(false)){AutoFlush=true};
                }
                writer.WriteLine(new JObject{["sequence"]=++sequence,["time"]=DateTimeOffset.Now.ToString("o"),["clientId"]=clientId,["user"]=user,["direction"]=direction,["kind"]=kind,["serverName"]=serverName,["topic"]=topic,["body"]=Redact(body)}.ToString(Formatting.None));
            }
        }
        public static void Close(){lock(sync){writer?.Dispose();writer=null;}}
    }
}
