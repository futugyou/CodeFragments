using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CodeFragments
{
    public class MD5Crypto
    {
        public static string ToMD5(string param, string salt = "")
        {
            var md5 = MD5.Create();
            byte[] result;
            if (string.IsNullOrEmpty(salt))
            {
                result = md5.ComputeHash(Encoding.UTF8.GetBytes(param));
            }
            else
            {
                result = md5.ComputeHash(Encoding.UTF8.GetBytes(param + salt));
            }

            var byteToStr = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                byteToStr.Append(result[i].ToString("x2"));
            }
            return byteToStr.ToString();
        }
    }
}
