namespace Labuladong;

public class LFUCache
{
    private Dictionary<int, int> keyvalue = new();
    private Dictionary<int, int> keyfreq = new();
    private Dictionary<int, HashSet<int>> freqkey = new();
    private int minFreq = 0;
    private int cap = 0;
    public LFUCache(int cap)
    {
        this.cap = cap;
    }

    public int Get(int key)
    {
        if (!keyvalue.ContainsKey(key))
        {
            return -1;
        }
        IncreaseFreq(key);
        return keyvalue[key];
    }
    public void Put(int key, int val)
    {
        if (cap <= 0)
        {
            return;
        }
        if (keyvalue.ContainsKey(key))
        {
            keyvalue[key] = val;
            IncreaseFreq(key);
            return;
        }
        if (cap <= keyvalue.Count)
        {
            RemoveMinFreqKey();
        }
        keyvalue.Add(key, val);
        keyfreq.Add(key, val);
        if (!freqkey.ContainsKey(1))
        {
            freqkey.Add(1, new());
        }
        freqkey[1].Add(key);
        minFreq = 1;
    }

    private void IncreaseFreq(int key)
    {
        var freq = keyfreq[key];
        keyfreq[key] = freq + 1;
        freqkey[freq].Remove(key);
        if (!freqkey.ContainsKey(freq + 1))
        {
            freqkey.Add(freq + 1, new());
        }
        freqkey[freq + 1].Add(key);
        if (!freqkey.Any())
        {
            freqkey.Remove(freq);
            if (freq == minFreq)
            {
                minFreq++;
            }
        }
    }


    private void RemoveMinFreqKey()
    {
        var keys = freqkey[minFreq];
        var delkey = keys.FirstOrDefault();
        keys.Remove(delkey);
        if (!keys.Any())
        {
            freqkey.Remove(minFreq);
        }
        keyvalue.Remove(delkey);
        keyfreq.Remove(delkey);
    }
}