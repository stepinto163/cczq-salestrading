using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.IO.Compression;
using Ats.Foundation.Message;
using com.gateway.log;
using common.netapi.Utils;

namespace netapi.Api
{
    /// <summary>
    /// 加密、解密
    /// </summary>
    public class EncrypterHelper
    {
        public const string publicKeyPath = "./control/public.key";
        private static string publicKey = "";

        static EncrypterHelper()
        {
            InitPublicKey();
        }

        public static Dictionary<string, object> getEncryptDic(string loginInfo)
        {
            InitPublicKey();
            Dictionary<string, object> loginDic = new Dictionary<string, object>();
            MD5 md5 = new MD5CryptoServiceProvider();
            Random r = new Random();
            string a = r.NextDouble().ToString();
            byte[] rData = md5.ComputeHash(Encoding.UTF8.GetBytes(a));
            loginDic.Add("param1", EncryptByRSA(Convert.ToBase64String(rData), publicKey));
            loginDic.Add("param2", EncodeInfo(rData, loginInfo));
            return loginDic;
        }

        private static string EncodeInfo(byte[] rData, string info)
        {
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(info);
            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = rData,
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray);
        }

        private static string EncryptByRSA(string msg, string publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            byte[] enBytes = rsa.Encrypt(UTF8Encoding.UTF8.GetBytes(msg), false);
            return Convert.ToBase64String(enBytes);
        }        

        private static void InitPublicKey()
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                publicKey = "";
                FileStream fs = null;
                StreamReader sr = null;
                string line;
                try
                {
                    fs = new FileStream(FileUtil.BaseDir + publicKeyPath, FileMode.Open, FileAccess.Read);
                    sr = new StreamReader(fs);

                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (!string.IsNullOrEmpty(line))
                        {
                            publicKey += line;
                        }
                    }
                    if (!string.IsNullOrEmpty(publicKey))
                    {
                        publicKey = Encoding.UTF8.GetString(DeCompressData(Convert.FromBase64String(publicKey)));
                        Dictionary<string, string> dic = MsgHelper.Deserialize<Dictionary<string, string>>(publicKey);
                        RSACertContent rkey = MsgHelper.Deserialize<RSACertContent>(dic["Content"]);
                        if (rkey != null)
                        {
                            publicKey = rkey.Extra.ContainsKey("cspublickey") ? rkey.Extra["cspublickey"] : "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                finally
                {
                    try
                    {
                        if (sr != null)
                            sr.Close();
                        if (fs != null)
                            fs.Close();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                }
            }
        }

        /// <summary>  
        /// 解压  
        /// </summary>  
        /// <param name="param"></param>  
        /// <returns></returns>  
        private static byte[] DeCompressData(byte[] sourceByte)
        {
            MemoryStream memStream = new MemoryStream(sourceByte);
            memStream.Position = 0;

            GZipStream zipStream = new GZipStream(memStream, CompressionMode.Decompress);
            memStream = new MemoryStream();

            byte[] buff = new byte[512];
            while (true)
            {
                int size = zipStream.Read(buff, 0, 512);
                memStream.Write(buff, 0, size);
                if (size < 512)
                    break;
            }

            byte[] tempByte = memStream.ToArray();
            memStream.Close();
            return tempByte;
        }

        /// <summary>
        /// 进行DES加密。
        /// </summary>
        /// <param name="input">要加密的字符串。</param>
        /// <param name="key">密钥，且必须为8位。</param>
        /// <returns>以Base64格式返回的加密字符串。</returns>
        public static string Encrypt(string input, string key)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                if (key.Length < 8)
                    key = string.Format("{0}!@#$%^&*", key);
                key = key.Substring(0, 8);

                byte[] inputByteArray = Encoding.UTF8.GetBytes(input);
                des.Key = ASCIIEncoding.ASCII.GetBytes(key);
                des.IV = ASCIIEncoding.ASCII.GetBytes(key);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Convert.ToBase64String(ms.ToArray());
                ms.Close();
                return str;
            }
        }


        /// <summary>
        /// 进行DES解密。
        /// </summary>
        /// <param name="input">要解密的以Base64</param>
        /// <param name="key">密钥，且必须为8位。</param>
        /// <returns>已解密的字符串。</returns>
        public static string Decrypt(string input, string key)
        {
            try
            {
                if (key.Length < 8)
                    key = string.Format("{0}!@#$%^&*", key);
                key = key.Substring(0, 8);

                byte[] inputByteArray = Convert.FromBase64String(input);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    des.Key = ASCIIEncoding.ASCII.GetBytes(key);
                    des.IV = ASCIIEncoding.ASCII.GetBytes(key);
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    string str = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    return str;
                }
            }
            catch
            {
                return null; 
            }
           
        }


        private const string AesKey = "W7p9WQO51zQ1255D";

        public static string AesEncrypt(byte[] EncryptStr, string Key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Key))
                    Key = AesKey;
                byte[] keyArray = Encoding.UTF8.GetBytes(Key);
                byte[] toEncryptArray = EncryptStr;//Encoding.UTF8.GetBytes(EncryptStr);

                RijndaelManaged aes16 = new RijndaelManaged();
                aes16.IV = keyArray;
                aes16.Key = keyArray;
                aes16.Mode = CipherMode.ECB;
                aes16.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = aes16.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch
            {
                return null;
            }
        }


        public static string AesDecrypt(string DecryptStr, string Key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Key))
                    Key = AesKey;
                byte[] keyArray = Encoding.UTF8.GetBytes(Key);
                byte[] toEncryptArray = Convert.FromBase64String(DecryptStr);

                RijndaelManaged rDel = new RijndaelManaged();
                rDel.IV = keyArray;
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Encoding.UTF8.GetString(resultArray);
            }
            catch
            {
                return null;
            }
        }

    }
}
