using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils.Proxy
{
    public class IniFileReader
    {

        public static Dictionary<string, Dictionary<string, string>> ReadIniFile(string filePath)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = null;

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        result[currentSection] = new Dictionary<string, string>();
                    }
                    else if (currentSection != null)
                    {
                        var parts = trimmedLine.Split('=');
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();
                            result[currentSection][key] = value;
                        }
                    }
                }
            }

            return result;
        }
    }
}
