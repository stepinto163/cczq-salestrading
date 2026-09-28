using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Utils
{
    /// <summary>
    /// 加密
    /// </summary>
    public class Base64Encrypt
    {
        /// <summary>
        /// base64
        /// </summary>
        public static string EncryptBase(string characters)
        {
            byte[] bytes = Encoding.Default.GetBytes(characters);
            return Convert.ToBase64String(bytes);
        }
        public static string DecryptBase(string characters)
        {
            byte[] outputb = Convert.FromBase64String(characters);
            return Encoding.Default.GetString(outputb);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="characters">要加密的明文</param>
        /// <returns>加密后的密文</returns>
        public static string EncryptStr(string characters)
        {
            string key = "330ead0b-4951-46a7-8db8-0e2569472fd1";
            //位移加密
            byte[] bStr = (new UnicodeEncoding()).GetBytes(characters);

            for (int i = 0; i < bStr.Length; i++)
            {
                byte b = (byte)(bStr[i] + 255);
                bStr[i] = b;
            }
            byte[] bKey = (new UnicodeEncoding()).GetBytes(key);//加密锁
            //异或加密
            for (int i = 0; i < bStr.Length; i += 2)
            {
                for (int j = 0; j < bKey.Length; j += 3)
                {
                    bStr[i] = Convert.ToByte(bStr[i] ^ bKey[j]);
                }
            }
            return (new UnicodeEncoding()).GetString(bStr).TrimEnd('\0');
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="ciphertext">要解密的密文</param>
        /// <returns>解密后的明文</returns>
        public static string DecryptStr(string ciphertext)
        {
            string key = "330ead0b-4951-46a7-8db8-0e2569472fd1";
            byte[] bStr = (new UnicodeEncoding()).GetBytes(ciphertext);
            byte[] bKey = (new UnicodeEncoding()).GetBytes(key);//加密锁
            //异或解密
            for (int i = 0; i < bStr.Length; i += 2)
            {
                for (int j = 0; j < bKey.Length; j += 3)
                {
                    bStr[i] = Convert.ToByte(bStr[i] ^ bKey[j]);
                }
            }
            //位移解密
            for (int i = 0; i < bStr.Length; i++)
            {
                byte b = (byte)(bStr[i] - 255);
                bStr[i] = b;
            }
            return (new UnicodeEncoding()).GetString(bStr).TrimEnd('\0');
        }


        //加密
        public static string Encryption(string express)
        {
            CspParameters param = new CspParameters();
            param.KeyContainerName = "siia";//密匙容器的名称，保持加密解密一致才能解密成功
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
            {
                byte[] plaindata = Encoding.Default.GetBytes(express);//将要加密的字符串转换为字节数组
                byte[] encryptdata = rsa.Encrypt(plaindata, false);//将加密后的字节数据转换为新的加密字节数组
                return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为字符串
            }
        }

        //解密
        public static string Decrypt(string ciphertext)
        {
            CspParameters param = new CspParameters();
            param.KeyContainerName = "330ead0b-4951-46a7-8db8-0e2569472fd1";
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
            {
                byte[] encryptdata = Convert.FromBase64String(ciphertext);
                byte[] decryptdata = rsa.Decrypt(encryptdata, false);
                return Encoding.Default.GetString(decryptdata);
            }
        }
    }
}