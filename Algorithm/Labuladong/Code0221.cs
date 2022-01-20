namespace Labuladong;
public class Code0221
{
    public static void Exection()
    {

    }

    public class WordDictionary
    {
        private TrieSet set = new TrieSet();
        public void AddWord(string word) => set.Add(word);
        public bool Search(string word) => set.HasKeyWithPattern(word);
    }
}