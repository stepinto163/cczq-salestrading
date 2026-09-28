/*****************************************************************
 * Copyright (C) Knights Warrior Corporation. All rights reserved.
 * 
 * Author:    •µÓ∆Ô ø£®Knights Warrior£© 
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
using System.Xml.Serialization;
using System.IO;

namespace MDT.Tools.Wpf.AutoUpdater
{
    public class Config
    {
        #region The private fields
        private bool enabled = true;
        private string serverUrl = string.Empty;
        private List<string> serverList = new List<string>();
        private string version = "1.0.0.0";
        private UpdateFileList updateFileList = new UpdateFileList();
        private int type = 0;
        #endregion

        #region The public property
        public int Type
        {
            get { return type; }
            set { type = value; }
        }
        [XmlIgnore]
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
       
        public string ServerUrl
        {
            get { return serverUrl; }
            set { serverUrl = value; }
        }
        [XmlIgnore]
        public List<string> ServerList
        {
            get { return serverList; }
            set { serverList = value; }
        }
        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        public UpdateFileList UpdateFileList
        {
            get { return updateFileList; }
            set { updateFileList = value; }
        }
        #endregion

        #region The public method
        public static Config LoadConfig(string file)
        {
            Config config = null;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Config));
                StreamReader sr = new StreamReader(file);
                  config = xs.Deserialize(sr) as Config;
                sr.Close();
            }
            catch  
            {
                
                 
            }
            if(config==null)
            {
                config=new Config();
            }

            return config;
        }

        public void SaveConfig(string file)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            StreamWriter sw = new StreamWriter(file);
            xs.Serialize(sw, this);
            sw.Close();
        }
        #endregion
    }

}
