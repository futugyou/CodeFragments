namespace Labuladong;

public class FreqStack
{
    private int maxFreq = 0;
    private Dictionary<int, int> valuefreq = new();
    private Dictionary<int, Stack<int>> freqvalue = new();

    public FreqStack()
    {

    }

    public void Push(int c)
    {
        var freq = 1;
        if (valuefreq.ContainsKey(c))
        {
            freq = valuefreq[c] + 1;
            valuefreq[c] = freq;
        }
        else
        {
            valuefreq.Add(c, 1);
        }

        if (!freqvalue.ContainsKey(freq))
        {
            freqvalue.Add(freq, new());
        }
        freqvalue[freq].Push(c);
        maxFreq = Math.Max(maxFreq, freq);
    }

    public int Pop()
    {
        if (!freqvalue.ContainsKey(maxFreq))
        {
            return -1;
        }
        var stack = freqvalue[maxFreq];
        var value = stack.Pop();
        var freq = valuefreq[value];
        valuefreq[value] = freq - 1;
        if (!stack.Any())
        {
            maxFreq = maxFreq - 1;
        }
        return value;
    }
}