using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    // Shared by UI and automated tests. No truncation of business message bodies.
    static class MessageLog {
        static readonly object sync=new object();
        static StreamWriter writer;static int part;static long sequence;
        public static readonly string DirectoryPath=ResolveDirectory();
        public static string CurrentFile {get;private set;}
        static string ResolveDirectory(){
            var current=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while(current!=null){if(File.Exists(Path.Combine(current.FullName,"QuotePrototype.csproj"))||File.Exists(Path.Combine(current.FullName,"DpsApiDebugger.csproj")))return Path.Combine(current.Parent.FullName,"runtime","client-logs",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Process.GetCurrentProcess().Id);current=current.Parent;}
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"FiccQuotePrototype","logs",DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Process.GetCurrentProcess().Id);
        }
        static JToken Redact(JToken token){
            var result=token?.DeepClone();
            if(result is JObject obj)foreach(var property in obj.Properties().ToArray()){
                if(new[]{"password","passwd","authorization","accessToken","refreshToken"}.Any(k=>string.Equals(k,property.Name,StringComparison.OrdinalIgnoreCase)))property.Value="[REDACTED]";
                else property.Value=Redact(property.Value)??JValue.CreateNull();
            }
            else if(result is JArray array)for(int i=0;i<array.Count;i++)array[i]=Redact(array[i]);
            return result;
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
