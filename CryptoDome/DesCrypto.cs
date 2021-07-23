using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoDome
{
    public class DesCrypto
    {
        /// <summary>
        /// Des解密方法
        /// </summary>
        /// <param name="val"></param>
        /// <param name="key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public static string Decrypt(string val, string key, string IV)
        {
            try
            {
                byte[] buffer1 = Convert.FromBase64String(key);
                byte[] buffer2 = Convert.FromBase64String(IV);
                var provider1 = DES.Create();
                provider1.Mode = CipherMode.ECB;
                provider1.Key = buffer1;
                provider1.IV = buffer2;
                ICryptoTransform transform1 = provider1.CreateDecryptor(provider1.Key, provider1.IV);
                byte[] buffer3 = Convert.FromBase64String(val);
                MemoryStream stream1 = new MemoryStream();
                CryptoStream stream2 = new CryptoStream(stream1, transform1, CryptoStreamMode.Write);
                stream2.Write(buffer3, 0, buffer3.Length);
                stream2.FlushFinalBlock();
                stream2.Close();
                return Encoding.Default.GetString(stream1.ToArray());
            }
            catch// (System.Exception ex)
            {
                return "";
            }
        }


        /// <summary>
        /// Des加密方法
        /// </summary>
        /// <param name="val"></param>
        /// <param name="key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public static string Encrypt(string val, string key, string IV)
        {
            try
            {
                byte[] buffer1 = Convert.FromBase64String(key);
                byte[] buffer2 = Convert.FromBase64String(IV);


                var provider1 = DES.Create();
                provider1.Mode = CipherMode.ECB;
                provider1.Key = buffer1;
                provider1.IV = buffer2;
                ICryptoTransform transform1 = provider1.CreateEncryptor(provider1.Key, provider1.IV);
                byte[] buffer3 = Encoding.Default.GetBytes(val);
                MemoryStream stream1 = new MemoryStream();
                CryptoStream stream2 = new CryptoStream(stream1, transform1, CryptoStreamMode.Write);
                stream2.Write(buffer3, 0, buffer3.Length);
                stream2.FlushFinalBlock();
                stream2.Close();
                return Convert.ToBase64String(stream1.ToArray());
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
