using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpellChecker
{
    class Spelling
    {
        private Dictionary _dictionary;
        private NorwigsCorrector _corrector;

        public Spelling(Dictionary dictionary)
        {
            _dictionary = dictionary;
            _corrector = new NorwigsCorrector(dictionary);
        }
        
        public string Correct(string line)
        {
            var strLine = StructurizeLine(line);

            ApplyCorrection(strLine);

            StringBuilder output = new StringBuilder();
            foreach(var word in strLine)
            {
                output.Append(word.Item + " ");
            }

            return output.ToString();
        }

        private LinkedList<StringItem> StructurizeLine(string line)
        {
            var strLine = new LinkedList<StringItem>();

            bool newWordFlag = false;
            foreach(char c in line)
            {
                if(char.IsLetter(c))
                {
                    if (strLine.Last != null && !strLine.Last.Value.NotLetter && !newWordFlag)
                    {
                        strLine.Last.Value.Item += c.ToString();
                        
                    }
                    else
                    {
                        strLine.AddLast(new StringItem { Item = c.ToString(), NotLetter = false });
                        newWordFlag = false;
                    }
                }
                if(c==' ')
                {
                    if (_dictionary.Words.ContainsKey(strLine.Last.Value.Item.ToLower()))
                        strLine.Last.Value.IsCorrect = true;
                    else
                        strLine.Last.Value.IsCorrect = false;

                    newWordFlag = true;
                }
                if(char.IsDigit(c) || char.IsPunctuation(c))
                {
                    strLine.AddLast(new StringItem { Item = c.ToString(), NotLetter = true });
                }
            }
            if (_dictionary.Words.ContainsKey(strLine.Last.Value.Item))
                strLine.Last.Value.IsCorrect = true;

            return strLine;
        }

        private void ApplyCorrection(LinkedList<StringItem> list)
        {
            LinkedListNode<StringItem> node = list.First;

            for (int i = 0; i < 3; i++)
            {
                while (node != null)
                {
                    var nextNode = node.Next;

                    if (!node.Value.IsCorrect && !node.Value.NotLetter)
                    {
                        switch (i)
                        {
                            case 0:
                                MergeCorrection(node, list);
                                nextNode = node.Next;
                                break;
                            case 1:
                                SplitCorrection(node, list);
                                break;
                            case 2:
                                node.Value.Item = _corrector.CorrectWord(node.Value.Item);
                                break;
                        }
                    }
                    node = nextNode;
                }
                node = list.First;
            }
        }
        
        private void SplitCorrection(LinkedListNode<StringItem> node, LinkedList<StringItem> list)
        {
            LinkedList<StringItem> output = new LinkedList<StringItem>();
            int letters = 0;
            bool allAreCorrect = true;

            GetBiggestFromBeginning(node.Value.Item, output);

            foreach(var word in output)
            {
                if (!word.IsCorrect)
                    allAreCorrect = false;

                if (word.Item.Length == 1)
                    letters++;
            }

            if (!allAreCorrect)
            {
                string corrected = _corrector.CorrectWord(node.Value.Item);
                if (_dictionary.Words.ContainsKey(corrected))
                {
                    node.Value.Item = corrected;
                    node.Value.IsCorrect = true;

                    return;
                }
            }

            if (letters > 1)
                return;

            foreach (var item in output)
            {
                list.AddBefore(node, item);
            }

            list.Remove(node);
        }

        private void MergeCorrection(LinkedListNode<StringItem> node, LinkedList<StringItem> list)
        {
            //merge sides
            if (MergeHelper(list, node, node.Previous, node, node.Next))
                return;

            //merge next
            if (MergeHelper(list, node, node, node.Next))
                return;

            //merge previous
            if (MergeHelper(list, node, node.Previous, node))
                return;
        }

        private bool MergeHelper(LinkedList<StringItem> list, LinkedListNode<StringItem> centerNode, params LinkedListNode<StringItem>[] nodes)
        {
            StringBuilder sb = new StringBuilder();
            bool hasOneLetterWords = false;

            foreach(var sn in nodes)
            {
                if (sn == null || sn.Value.NotLetter || sb.Length > 12)
                    return false;

                if (sn.Value.IsCorrect && sn.Value.Item.Length < 2)
                    hasOneLetterWords = true;

                sb.Append(sn.Value.Item);
            }

            string merged = (hasOneLetterWords)?sb.ToString(): _corrector.CorrectWord(sb.ToString());

            if (_dictionary.Words.ContainsKey(merged))
            {
                centerNode.Value.Item = merged;
                centerNode.Value.IsCorrect = true;

                foreach(var node in nodes)
                {
                    if (node == centerNode)
                        continue;

                    //if (node.Value.IsCorrect && node.Value.Item.Length < 2)
                    //    continue;
                    else
                        list.Remove(node);
                }

                //success
                return true;
            }

            //failure
            return false;
        }

        private void GetBiggestFromBeginning(string item, LinkedList<StringItem> list)
        {
            for (int i = item.Length; i > 0; i--)
            {
                string word = item.Substring(0, i);
                if (_dictionary.Words.ContainsKey(word.ToLower()))
                {
                    list.AddLast(new StringItem { Item = word, IsCorrect = true });
                    if(word!=item)
                        GetBiggestFromBeginning(item.Substring(word.Length), list);
                    break;
                } else
                {
                    if (i == 1)
                    {
                        list.AddLast(new StringItem { Item = item, IsCorrect = false });
                    }
                }
            }
        }
    }
}
