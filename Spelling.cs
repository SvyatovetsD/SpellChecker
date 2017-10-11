using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

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

            return DestructurizeLine(strLine);
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
                if(c==' ' || char.IsDigit(c) || char.IsPunctuation(c))
                {
                    if (strLine.Last == null)
                        continue;

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

        private String DestructurizeLine(LinkedList<StringItem> list)
        {
            StringBuilder output = new StringBuilder();

            for (LinkedListNode<StringItem> word = list.First; word != null; word = word.Next)
            {
                output.Append(word.Value.Item);

                if (word.Value.NotLetter)
                {
                    UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(word.Value.Item[0]);

                    if (category == UnicodeCategory.FinalQuotePunctuation || category == UnicodeCategory.OtherPunctuation)
                    {
                        output.Remove(output.Length - 2, 1);
                        output.Append(" ");
                    }
                    else
                        continue;
                }
                else
                {
                    output.Append(" ");
                }
            }

            return output.ToString();
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

            string biggestWord = "";

            foreach (var word in output)
            {
                if (word.Item.Length > biggestWord.Length)
                    biggestWord = word.Item;

                if (!word.IsCorrect)
                    allAreCorrect = false;

                if (word.Item.Length <= 2)
                    letters++;

                if (_dictionary.BadSplitCandidates.ContainsKey(word.Item.ToLower()))
                    allAreCorrect = false;
            }

            if (letters > 2)
                return;

            string corrected = _corrector.CorrectWord(node.Value.Item);

            bool pickCorrected = _dictionary.Words.ContainsKey(corrected);

            if(pickCorrected && _dictionary.Words.ContainsKey(biggestWord))
                pickCorrected = _dictionary.Words[biggestWord] < _dictionary.Words[corrected];


            if (!allAreCorrect || pickCorrected)
            {
                node.Value.Item = corrected;
                node.Value.IsCorrect = true;

                return;
            }

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

            foreach(var sn in nodes)
            {
                if (sn == null || sn.Value.NotLetter || sb.Length > 12)
                    return false;

                sb.Append(sn.Value.Item);
            }
            
            string merged = sb.ToString();

            if (_dictionary.Words.ContainsKey(merged.ToLower()))
            {
                centerNode.Value.Item = merged;
                centerNode.Value.IsCorrect = true;

                foreach(var node in nodes)
                {
                    if (node == centerNode)
                        continue;
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
