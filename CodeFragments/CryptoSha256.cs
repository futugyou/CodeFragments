using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CodeFragments
{
    public class Sha256Crypto
    {

        public static string GetSHA256(string strData)
        {
            byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(strData);
            var sha256 =   SHA256.Create();

            byte[] retVal = sha256.ComputeHash(bytValue);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string GetSHA256(byte[] bytValue)
        {
            var sha256 = SHA256.Create();

            byte[] retVal = sha256.ComputeHash(bytValue);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }


        /// <summary>
        /// This is for Auth2.0 And Support PKCE(Proof Key for Code Exchange)
        /// https://datatracker.ietf.org/doc/html/rfc7636 
        /// BASE64URL-ENCODE(SHA256(ASCII(code_verifier))) == code_challenge
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static string GetSHA256Base64urlEncoding(string strData)
        {
            byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(strData);
            var sha256 = SHA256.Create();
            return Base64Crypto.Base64UrlEncode(sha256.ComputeHash(bytValue));
        }

        public static string GetSHA256Base64urlEncoding(byte[] bytValue)
        {
            /// SHA256CryptoServiceProvider 使用FIPS 140-2验证(FIPS =联邦信息处理标准)加密服务提供程序(CSP)
            /// 值与SHA256Managed一致的.
            var sha256 = SHA256.Create();
            return Base64Crypto.Base64UrlEncode(sha256.ComputeHash(bytValue));
        }
        public static string SHA256WithManaged(string source)
        {
            /// SHA256Managed 完全使用.NET托管代码，
            using (var sha256 = SHA256.Create())
            {
                byte[] buffer = new ASCIIEncoding().GetBytes(source);
                byte[] temp = sha256.ComputeHash(buffer);
                return Convert.ToBase64String(temp);

            }
        }

    }
}
