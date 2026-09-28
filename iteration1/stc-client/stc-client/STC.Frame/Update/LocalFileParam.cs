using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace STC.Frame.Update
{
    
    public class LocalFileParam
    {
        private string path = "";
        private string filetype = "";

        [XmlAttribute("path")]
        public string Path { get { return path; } set { path = value; } }

        [XmlAttribute("filetype")]
        public string Filetype { get { return filetype; } set { filetype = value; } }

        public LocalFileParam()
        {
        }


        public LocalFileParam(string path, string filetype)
        {
            this.path = path;
            this.filetype = filetype;
        }
    }
}
