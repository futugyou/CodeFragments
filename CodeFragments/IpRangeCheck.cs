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

    private uint ConvertIpToUint(string ip) => BitConverter.ToUInt32(ip.Split('.').Reverse().Select(x => byte.Parse(x)).ToArray(), 0);
    
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