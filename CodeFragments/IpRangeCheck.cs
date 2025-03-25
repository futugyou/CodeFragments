using System.Text.RegularExpressions;

namespace CodeFragments;

public class IpRangeCheck
{
    private List<IpRange> _ipRanges = new List<IpRange>();

    public IpRangeCheck()
    {

    }

    public void AddIpRangeWithIPCount(string ip, int ipcount)
    {
        int mask = 32 - (int)(Math.Log(ipcount, Math.E) / Math.Log(2, Math.E));
        AddIpRange(ip, mask);
    }

    public bool FindIP(string ip)
    {
        var target = ConvertIpToUint(ip);
        var low = 0;
        var high = _ipRanges.Count - 1;
        while (low <= high)
        {
            var mid = (high - low) / 2 + low;
            var curr = _ipRanges[mid];
            if (curr.LeftRange <= target && target <= curr.RightRange)
            {
                return true;
            }
            if (target < curr.LeftRange)
            {
                high = mid - 1;
            }
            if (target > curr.LeftRange)
            {
                low = mid + 1;
            }
        }
        return false;
    }

    // Need Call SortIpRange() after all data add.
    public void AddIpRange(string ip, int mask)
    {
        uint maskleft = uint.MaxValue << (32 - mask);
        uint maskright = uint.MaxValue >> mask;
        var ip_raw = ConvertIpToUint(ip);
        var left = ip_raw & maskleft;
        var right = ip_raw | maskright;
        _ipRanges.Add(new IpRange(left, right));
    }

    public void SortIpRange()
    {
        _ipRanges = _ipRanges.OrderBy(p => p.LeftRange).ToList();
    }

    private static uint ConvertIpToUint(string ip)
    {
        var ipArray = ip.Split('.');
        ipArray.Reverse();
        return BitConverter.ToUInt32([.. ipArray.Select(byte.Parse)], 0);
    }


    private class IpRange
    {
        public IpRange(uint left, uint right)
        {
            LeftRange = left;
            RightRange = right;
        }
        public uint LeftRange { get; }
        public uint RightRange { get; }
    }
}


/// <summary>
/// This extensioin for check ip range from 
/// http://ftp.apnic.net/apnic/stats/apnic/delegated-apnic-latest
/// The format is like that:
///  apnic|CN|ipv4|222.196.0.0|131072|20040610|allocated
///  apnic|CN|ipv4|222.198.0.0|65536|20040610|allocated
/// </summary>
public static class IpRangeCheckExtensioins
{
    private const string DEFAULT_PATTERN = @"(apnic.CN.ipv4).(\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}).(\d{0,9})";
    public static void AddIpRangeWithApnicString(this IpRangeCheck check, string apnic, string pattern = "")
    {
        if (pattern == "")
        {
            pattern = DEFAULT_PATTERN;
        }
        Match match = Regex.Match(apnic, pattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var ip = match.Groups[2].Value;
            int ipcount = Convert.ToInt32(match.Groups[3].Value);
            check.AddIpRangeWithIPCount(ip, ipcount);
        }
    }
}