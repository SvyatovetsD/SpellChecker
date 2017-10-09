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

                if (string.IsNullOrWhiteSpace(word))
                    continue;

                string[] split = Regex.Split(word, @"\p{P}");

                foreach (string w in split)
                {
                    if (string.IsNullOrWhiteSpace(w))
                        continue;

                    if (_dictionary.ContainsKey(w))
                        _dictionary[w]++;
                    else
                        _dictionary.Add(w, 1);

                    _analyzedWords++;
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
            foreach (char c in _alphabetSet)
            {
                if (_dictionary.ContainsKey(c.ToString()))
                {
                    if ((double)_dictionary[c.ToString()] / _analyzedWords < 0.0004)
                        _dictionary.Remove(c.ToString());
                }
            }
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
