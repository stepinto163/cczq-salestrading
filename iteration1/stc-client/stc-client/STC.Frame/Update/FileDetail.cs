using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace STC.Frame.Update
{
    /// <summary>
    /// filedetail.config文件内容
    /// </summary>
    public class FileDetail
    {
        private string serverUrl = string.Empty;
        private int type = 0;

        private FileDetailList updateFileList = new FileDetailList();

        public int Type
        {
            get { return type; }
            set { type = value; }
        }

        public string ServerUrl
        {
            get { return serverUrl; }
            set { serverUrl = value; }
        }

        public FileDetailList UpdateFileList
        {
            get { return updateFileList; }
            set { updateFileList = value; }
        }

        #region The public method
        public static FileDetail LoadConfig(string file)
        {
            FileDetail config = null;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(FileDetail));
                StreamReader sr = new StreamReader(file);
                config = xs.Deserialize(sr) as FileDetail;
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
            if (config == null)
            {
                config = new FileDetail();
            }

            return config;
        }

        public void SaveConfig(string file)
        {
            XmlSerializer xs = new XmlSerializer(typeof(FileDetail));
            StreamWriter sw = new StreamWriter(file);
            xs.Serialize(sw, this);
            sw.Close();
        }

        public FileDetail()
        { }


        public FileDetail(XmlDocument document)
        {
            if (document.DocumentElement != null)
                foreach (XmlNode node in document.DocumentElement.ChildNodes)
                {
                    switch (node.Name.ToLower())
                    {
                        case "type":
                            int.TryParse(node.InnerText, out type);
                            break;
                        case "serverurl":
                            serverUrl = node.InnerText;
                            break;
                        case "filedetaillist":
                            foreach (XmlNode nodex in node.ChildNodes)
                            {
                                try
                                {
                                    if (nodex.Attributes != null)
                                    {
                                        string path = nodex.Attributes["path"].Value;
                                        string fileType = nodex.Attributes["filetype"].Value;
                                        if (updateFileList.Count > 0)
                                        {
                                            bool isContail = false;
                                            foreach (LocalFileParam fileParam in updateFileList)
                                            {
                                                if ((fileParam.Path+"").Equals(path))
                                                {
                                                    isContail = true;
                                                    break;
                                                }
                                            }
                                            if (!isContail)
                                                updateFileList.Add(new LocalFileParam(path, fileType));
                                        }
                                        else
                                            updateFileList.Add(new LocalFileParam(path, fileType));
                                        
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }
                            break;
                    }
                }
        }

        #endregion
    }
}
