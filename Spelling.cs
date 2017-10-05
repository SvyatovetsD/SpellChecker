using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpellChecker
{
    public class Spelling
    {
        private Dictionary<String, int> _dictionary;
        private string _alphabet;
        private HashSet<char> _alphabetSet;

        public Spelling(string input = "")
        {
            _dictionary = new Dictionary<String, int>();
            _alphabetSet = new HashSet<char>();
            _alphabet = string.Empty;

            if(string.IsNullOrWhiteSpace(input))
                PopulateDictionary(input);
        }

        public void PopulateDictionary(string input)
        {
            foreach (string entity in input.Split(' '))
            {
                string word = Regex.Replace(entity.ToLower(), @"(\p{P})|([0-9])", "");

                if (string.IsNullOrWhiteSpace(word))
                    continue;

                if (_dictionary.ContainsKey(word))
                    _dictionary[word]++;
                else
                    _dictionary.Add(word, 1);

                foreach (char c in word)
                {
                    if (char.IsWhiteSpace(c))
                        continue;

                    _alphabetSet.Add(c);
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (char letter in _alphabetSet)
            {
                sb.Append(letter);
            }

            _alphabet = sb.ToString();
        }

        public string Correct(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            word = word.ToLower();

            // known()
            if (_dictionary.ContainsKey(word))
                return word;

            List<String> list = Edits(word);
            Dictionary<string, int> candidates = new Dictionary<string, int>();

            foreach (string wordVariation in list)
            {
                if (_dictionary.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
                    candidates.Add(wordVariation, _dictionary[wordVariation]);
            }

            if (candidates.Count > 0)
                return candidates.OrderByDescending(x => x.Value).First().Key;

            // known_edits2()
            foreach (string item in list)
            {
                foreach (string wordVariation in Edits(item))
                {
                    if (_dictionary.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
                        candidates.Add(wordVariation, _dictionary[wordVariation]);
                }
            }

            return (candidates.Count > 0) ? candidates.OrderByDescending(x => x.Value).First().Key : word;
        }

        private List<string> Edits(string word)
        {
            var splits = from i in Enumerable.Range(0, word.Length)
                         select new { a = word.Substring(0, i), b = word.Substring(i) };
            var deletes = from s in splits
                          where s.b != "" 
                          select s.a + s.b.Substring(1);
            var transposes = from s in splits
                             where s.b.Length > 1
                             select s.a + s.b[1] + s.b[0] + s.b.Substring(2);
            var replaces = from s in splits
                           from c in _alphabet
                           where s.b != ""
                           select s.a + c + s.b.Substring(1);
            var inserts = from s in splits
                          from c in _alphabet
                          select s.a + c + s.b;

            return deletes
            .Union(transposes) 
            .Union(replaces)
            .Union(inserts).ToList();
        }

        public IEnumerable<string> GetDictionary()
        {
            foreach(var word in _dictionary)
            {
                yield return word.Key;
            }
        }

        public int TotalWords
        {
            get
            {
                return _dictionary.Count;
            }
        }
    }
}
