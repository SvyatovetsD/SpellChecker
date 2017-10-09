using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpellChecker
{
    public partial class Home : Form
    {
        private Dictionary _dictionary = new Dictionary();
        private Spelling _spelling;

        public Home()
        {
            InitializeComponent();
        }

        private void LoadFileBtn_Click(object sender, EventArgs e)
        {
            Stream stream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((stream = openFileDialog1.OpenFile()) != null)
                    {
                        using (stream)
                        {
                            StreamReader reader = new StreamReader(stream, Encoding.Default);
                            _dictionary.PopulateDictionary(reader.ReadToEnd().Replace(Environment.NewLine, " "));
                            _spelling = new Spelling(_dictionary);

                            reader.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            foreach (string word in _dictionary.Words.Keys)
            {
                if(!dictionaryListBox.Items.Contains(word))
                    dictionaryListBox.Items.Add(word);
            }

            totalItemsLabel.Text = "Всего слов в словаре: " + _dictionary.TotalWords.ToString();
        }

        private void correctButton_Click(object sender, EventArgs e)
        {
            if (_spelling != null)
                outputTextBox.Text = _spelling.Correct(userInputTextBox.Text);
            else
                MessageBox.Show("Словарь не загружен");
        }
    }
}
