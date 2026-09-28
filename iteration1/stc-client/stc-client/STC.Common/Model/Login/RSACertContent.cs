using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Login
{
    /// <summary>
    /// RSA加密相关类
    /// </summary>
    [Serializable]
    public class RSACertContent
    {
        public String PublicKey { set; get; }
        public String PrivateKey { set; get; }
        public String ID { set; get; }
        public bool IsEncrypt { set; get; }
        public Dictionary<String, String> Extra = new Dictionary<String, String>();
    }
}
