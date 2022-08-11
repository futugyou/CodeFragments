namespace Hangfire.Cap;

public class CodingUtil
{
    public static string MD5(string str)
    {
        byte[] b = Encoding.UTF8.GetBytes(str);
        b = System.Security.Cryptography.MD5.Create().ComputeHash(b);
        string ret = string.Empty;
        for (int i = 0; i < b.Length; i++)
        {
            ret += b[i].ToString("x").PadLeft(2, '0');
        }
        return ret;
    }
}
