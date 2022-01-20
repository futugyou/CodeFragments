namespace Labuladong;
public class Code1804
{
    public static void Exection()
    {

    }

    public class Trie
    {
        private TrieMap<int> map = new();
        public void Insert(string word)
        {
            if (map.ContainsKey(word))
            {
                map.Put(word, 1);
            }
            else
            {
                map.Put(word, map.Get(word) + 1);
            }
        }


        public int CountWordsEqualTo(string word)
        {
            if (!map.ContainsKey(word))
            {
                return 0;
            }
            return map.Get(word);
        }

        public int CountWordsStartWith(string prefix)
        {
            var r = 0;
            foreach (var item in map.KeysWithPrefix(prefix))
            {
                r += map.Get(item);
            }
            return r;
        }

        public void Erase(string word)
        {
            int freq = map.Get(word);
            if (freq == 1)
            {
                map.Remove(word);
            }
            else
            {
                map.Put(word, freq - 1);
            }
        }
    }
}