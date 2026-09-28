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
using System.Xml.Serialization;

namespace MDT.Tools.Wpf.AutoUpdater
{
    public class LocalFile
    {
        #region The private fields
        private string path = "";
        private string lastver = "";
        private int size = 0;
        private string md5 = "";
        #endregion

        #region The public property
        [XmlAttribute("path")]
        public string Path { get { return path; } set { path = value; } }
        [XmlAttribute("lastver")]
        public string LastVer { get { return lastver; } set { lastver = value; } }
        [XmlAttribute("size")]
        public int Size { get { return size; } set { size = value; } }
        [XmlAttribute("md5")]
        public string MD5 { get { return md5; } set { md5 = value; } }
        #endregion

        #region The constructor of LocalFile

        public LocalFile(string path, string ver, int size, string md5)
        {
            this.path = path;
            this.lastver = ver;
            this.size = size;
            this.md5 = md5;
        }

        public LocalFile()
        {
        }
        #endregion

    }
}
