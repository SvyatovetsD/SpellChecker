using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpellChecker
{
    class Dictionary
    {
        private Dictionary<String, int> _dictionary;
        private string _alphabet;
        private HashSet<char> _alphabetSet;
        private int _analyzedWords;
        public Dictionary<string, int> BadSplitCandidates { get; private set; }

        public Dictionary(string input = "")
        {
            _dictionary = new Dictionary<string, int>();
            _alphabet = "";
            _alphabetSet = new HashSet<char>();
            _analyzedWords = 0;

            if (!string.IsNullOrWhiteSpace(input))
                PopulateDictionary(input);
        }

        public void PopulateDictionary(string input)
        {
            foreach (string entity in input.Split(' '))
            {
                string word = Regex.Replace(entity.ToLower(), @"(^\p{P})|(\p{P}*$)|([0-9])", "");

                bool isAbbreviation = Regex.IsMatch(word, @"\p{P}");

                if (string.IsNullOrWhiteSpace(word))
                    continue;

                string[] split = Regex.Split(word, @"\p{P}");

                foreach (string w in split)
                {
                    if (string.IsNullOrWhiteSpace(w))
                        continue;

                    _analyzedWords++;

                    if (w.Length == 1 && _dictionary.ContainsKey(w) && isAbbreviation)
                        continue;

                    if (_dictionary.ContainsKey(w))
                        _dictionary[w]++;
                    else
                        _dictionary.Add(w, 1);
                }
            }

            CreateAlphabet();

            CleanDictionary();
        }

        private void CreateAlphabet()
        {
            int length = (_analyzedWords > 1000) ? 1000 : _analyzedWords;

            foreach (var word in _dictionary.Take(length))
            {
                foreach (char c in word.Key)
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

        private void CleanDictionary()
        {
            BadSplitCandidates =_dictionary.Where(x => x.Key.Length <= 2).Where(x => (double)x.Value / _analyzedWords < 0.0002).ToDictionary(x=>x.Key,x=>x.Value);
        }

        public Dictionary<String, int> Words
        {
            get
            {
                return _dictionary;
            }
        }

        public string Alphabet
        {
            get
            {
                return _alphabet;
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
