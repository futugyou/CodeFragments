namespace Labuladong;
public class Code0667
{
    public static void Exection()
    {

    }

    public class MapSum
    {
        private TrieMap<int> map = new TrieMap<int>();
        public void Insert(string key, int val) => map.Put(key, val);
        public int Sum(string prefix)
        {
            var keys = map.KeysWithPrefix(prefix);
            var res = 0;
            foreach (var item in keys)
            {
                res += map.Get(item);
            }
            return res;
        }
    }
}