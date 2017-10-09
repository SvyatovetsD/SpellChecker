using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    class NorwigsCorrector
    {
        private Dictionary _dictionary;

        public NorwigsCorrector(Dictionary dic)
        {
            _dictionary = dic;
        }

        public string CorrectWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            word = word.ToLower();

            // known()
            if (_dictionary.Words.ContainsKey(word))
                return word;

            List<String> list = Edits(word);
            Dictionary<string, double> candidates = new Dictionary<string, double>();

            foreach (string wordVariation in list)
            {
                if (_dictionary.Words.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
                    candidates.Add(wordVariation, (double)LevenshteinDistance(word, wordVariation)/_dictionary.Words[wordVariation]);
            }

            if (candidates.Count > 0)
                return candidates.OrderBy(x => x.Value).First().Key;

            // known_edits2()
            foreach (string item in list)
            {
                foreach (string wordVariation in Edits(item))
                {
                    if (_dictionary.Words.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
                        candidates.Add(wordVariation, (double)LevenshteinDistance(word, wordVariation) / _dictionary.Words[wordVariation]);
                }
            }


            return (candidates.Count > 0) ? candidates.OrderBy(x => x.Value).First().Key : word;
        }

        private List<string> Edits(string word)
        {
            var splits = from i in Enumerable.Range(0, word.Length+1)
                         select new { a = word.Substring(0, i), b = word.Substring(i) };
            var deletes = from s in splits
                          where s.b != ""
                          select s.a + s.b.Substring(1);
            var transposes = from s in splits
                             where s.b.Length > 1
                             select s.a + s.b[1] + s.b[0] + s.b.Substring(2);
            var replaces = from s in splits
                           from c in _dictionary.Alphabet
                           where s.b != ""
                           select s.a + c + s.b.Substring(1);
            var inserts = from s in splits
                          from c in _dictionary.Alphabet
                          select s.a + c + s.b;

            return deletes
            .Union(transposes)
            .Union(replaces)
            .Union(inserts).ToList();
        }

        private static int LevenshteinDistance(string string1, string string2)
        {
            if (string1 == null) throw new ArgumentNullException("string1");
            if (string2 == null) throw new ArgumentNullException("string2");
            int diff;
            int[,] m = new int[string1.Length + 1, string2.Length + 1];

            for (int i = 0; i <= string1.Length; i++) { m[i, 0] = i; }
            for (int j = 0; j <= string2.Length; j++) { m[0, j] = j; }

            for (int i = 1; i <= string1.Length; i++)
            {
                for (int j = 1; j <= string2.Length; j++)
                {
                    diff = (string1[i - 1] == string2[j - 1]) ? 0 : 1;

                    m[i, j] = Math.Min(Math.Min(m[i - 1, j] + 1,
                                             m[i, j - 1] + 1),
                                             m[i - 1, j - 1] + diff);
                }
            }
            return m[string1.Length, string2.Length];
        }
    }
}
