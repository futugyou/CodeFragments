namespace Labuladong;
public class Code0208
{
    public static void Exection()
    {

    }
    public class Trie
    {
        private TrieSet set;
        public Trie()
        {
            set = new TrieSet();
        }

        public void Insert(string word)
        {
            set.Add(word);
        }
        public bool Search(string word) => set.Contains(word);
        public bool StartWith(string word) => set.HasKeyWithPrefix(word);
    }
}
