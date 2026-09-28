/*****************************************************************
 * Copyright (C) Knights Warrior Corporation. All rights reserved.
 * 
 * Author:   圣殿骑士（Knights Warrior） 
 * Email:    KnightsWarrior@msn.com
 * Website:  http://www.cnblogs.com/KnightsWarrior/       http://knightswarrior.blog.51cto.com/
 * Create Date:  5/8/2010 
 * Usage:
 *
 * RevisionHistory
 * Date         Author               Description
 * 
*****************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MDT.Tools.Wpf.AutoUpdater
{
    public class RemoteConfig
    {
        private string version = "1.0.0.0";
        private string minimumRequiredVersion = "1.0.0.0";
        private string comment = "";
        public string Version
        {
            get { return version; }
        }

        public string MinimumRequiredVersion
        {
            get { return minimumRequiredVersion; }
        }

        public string Comment
        {
            get { return comment; }
        }
        private Dictionary<string, RemoteFile> remoteFiles=new Dictionary<string, RemoteFile>();
        public Dictionary<string, RemoteFile> RemoteFiles
        {
            get
            {
              
                return remoteFiles;
            }
        }

        public RemoteConfig(XmlDocument document)
        {
            if (document.DocumentElement != null)
                foreach (XmlNode node in document.DocumentElement.ChildNodes)
                {
                    switch (node.Name.ToLower())
                    {
                        case "version":
                            version = node.InnerText;
                            break;
                        case "minimumrequiredversion":
                            minimumRequiredVersion = node.InnerText;
                            break;
                        case "comment":
                            comment = node.InnerText;
                            break;
                        case "updatefiles":
                            foreach (XmlNode nodex in node.ChildNodes)
                            {
                                try
                                {
                                    if (nodex.Attributes != null)
                                    {
                                        string path = nodex.Attributes["path"].Value;
                                        if (!remoteFiles.ContainsKey(path))
                                        {
                                            remoteFiles.Add(path, new RemoteFile(nodex));
                                        }
                                        else
                                        {
                                            LogHelper.Error("error:" + path);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Error(e.Message);
                                }
                            }
                            break;
                    }
                }
        }
    }
    
    public class RemoteFile
    {
        #region The private fields
        private string path = "";
        private string url = "";
        private string lastver = "";
        private int size = 0;
        private bool needRestart = false;
        private string md5 = "";
        #endregion

        #region The public property
        public string Path { get { return path; } }
        public string Url { get { return url; } }
        public string LastVer { get { return lastver; } }
        public int Size { get { return size; } }
        public bool NeedRestart { get { return needRestart; } }
        public string MD5
        {
            get { return md5; }
        }
        
        #endregion

        #region The constructor of AutoUpdater
        public RemoteFile(XmlNode node)
        {
            this.path = node.Attributes["path"].Value;
            this.url = node.Attributes["url"].Value;
            this.needRestart = Convert.ToBoolean(node.Attributes["needRestart"].Value);
            
            try
            {
                this.lastver = node.Attributes["lastver"].Value;
                this.size = Convert.ToInt32(node.Attributes["size"].Value);
            }
            catch
            {
            }
            
            try
            {
                this.md5 = node.Attributes["md5"].Value;
            }
            catch
            {
            }
        }
        #endregion
    }
}
