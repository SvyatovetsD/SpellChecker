using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
    class StringItem
    {
        public string Item { get; set; }
        public bool IsCorrect { get; set; }
        public bool NotLetter { get; set; }
    }
}
