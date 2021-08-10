using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KongDemo
{
    public class PassportTools
    {
        public static int UnixTimeSeconds() => (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        public static string Convert2QueryString(SortedDictionary<string, string> dict)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValuePair in dict)
            {
                if (!string.IsNullOrEmpty(keyValuePair.Key))
                    stringBuilder.Append(keyValuePair.Key + "=" + keyValuePair.Value.ToString() + "&");
            }
            if (stringBuilder.Length > 1)
                stringBuilder.Replace("&", "", stringBuilder.Length - 2, 2);
            return stringBuilder.ToString();
        }
         

        /// <summary>字符串 生成guid</summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Guid GuidFromString(string input)
        {
            using (MD5 md5 = MD5.Create())
                return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(input)));
        }
    }
}
