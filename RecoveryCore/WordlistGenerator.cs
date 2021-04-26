using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace RecoveryCore
{
    public class WordlistGenerator : IWordlistGenerator, IDisposable
    {
        private readonly object _generatingMutex = new { };
        private List<int> _nextWordIndexes = new List<int>() { 0 };
        private TextWriter _wordlistFile;
        private TextReader _wordlistFileReader;
        private BigInteger _generatedCount = 0;

        public WordlistGenerator(WordlistGeneratorConfiguration configuration)
        {
            Configuration = configuration;
            if (configuration.StartingWordIndexes?.Count > 0)
            {
                _nextWordIndexes = configuration.StartingWordIndexes;
            }

            if (configuration.OutputToFile)
            {
                if (configuration.WordlistOutFile == null)
                {
                    Console.WriteLine("Error: OutputToFile is set for wordlist but no file specified in WordlistOutFile.");
                    Environment.Exit(1);
                }

                _wordlistFile = File.AppendText(configuration.WordlistOutFile);
            }

            if (configuration.LoadOnly)
            {
                if (configuration.OutputToFile)
                {
                    Console.WriteLine("Error: cannot output wordlist to file and load from it at the same time.");
                    Environment.Exit(1);
                }

                if (configuration.WordlistOutFile == null)
                {
                    Console.WriteLine("Error: OutputToFile is set for wordlist but no file specified in WordlistOutFile.");
                    Environment.Exit(1);
                }

                if (!File.Exists(configuration.WordlistOutFile))
                {
                    Console.WriteLine("Error: Wordlist file does not exist.");
                    Environment.Exit(1);
                }

                _wordlistFileReader = File.OpenText(configuration.WordlistOutFile);
            }
        }


        public WordlistGeneratorConfiguration Configuration { get; set; }

        public string CurrentWord { get; set; }

        public bool OutOfWords { get; set; } = false;

        public int[] NextWordIndexes => _nextWordIndexes.ToArray();


        public string GetNextWord()
        {
            lock (_generatingMutex)
            {
                string word = GetNextValidWord();
                _generatedCount++;

                if (Configuration.OutputToFile)
                {
                    _wordlistFile.WriteLine(word);
                }

                if (_generatedCount % 20000 == 0)
                {
                    _wordlistFile.Flush();
                }

                return word;
            }
        }

        public void Dispose()
        {
            if (_wordlistFile != null)
            {
                _wordlistFile.Flush();
                _wordlistFile.Close();
                _wordlistFile.Dispose();
            }
        }

        private string GetNextValidWord()
        {
            if (Configuration.LoadOnly)
            {
                return _wordlistFileReader.ReadLine();
            }

            while (_nextWordIndexes[_nextWordIndexes.Count - 1] >= Configuration.WordsSet.Count)
            {
                _nextWordIndexes.RemoveAt(_nextWordIndexes.Count - 1);

                if (_nextWordIndexes.Count == 0)
                {
                    OutOfWords = true;
                    return "";
                }

                _nextWordIndexes[_nextWordIndexes.Count - 1]++;
            }

            string word = "";

            int lastWordIndex = _nextWordIndexes[_nextWordIndexes.Count - 1];
            int lastWordCounts = 0;
            int maxWordRepetitions = Configuration.MaxWordRepetitions.Count > lastWordIndex ?
                Configuration.MaxWordRepetitions[lastWordIndex] :
                Configuration.DefaultMaxWordRepetitions;

            if (_nextWordIndexes.Contains(5) && _nextWordIndexes.Contains(6))
            {

            }

            int lastWordGroupIndex = int.MaxValue,
                lastWordGroupMinWordIndex = int.MaxValue,
                lastWordGroupMaxWordIndex = int.MaxValue,
                lastWordGroupCounts = 0,
                maxGroupRepetitions = Configuration.DefaultMaxWordGroupRepetitions;
            if (Configuration.WordGroupOffsets.Count > 0)
            {
                int groupIndex = 0;
                do
                {
                    int groupOffset = Configuration.WordGroupOffsets[groupIndex];

                    if (groupOffset > lastWordIndex)
                    {
                        break;
                    }

                    groupIndex++;
                }
                while (groupIndex < Configuration.WordGroupOffsets.Count);

                lastWordGroupIndex = groupIndex - 1;
            }
            if (lastWordGroupIndex != int.MaxValue)
            {
                lastWordGroupMinWordIndex = lastWordGroupIndex != int.MaxValue ? Configuration.WordGroupOffsets[lastWordGroupIndex] : 0;
                lastWordGroupMaxWordIndex = Configuration.WordGroupOffsets.Count > lastWordGroupIndex + 1 ? Configuration.WordGroupOffsets[lastWordGroupIndex + 1] - 1 : int.MaxValue;
                lastWordGroupCounts = 0;
                maxGroupRepetitions = Configuration.MaxWordGroupRepetitions.Count > lastWordGroupIndex ?
                    Configuration.MaxWordGroupRepetitions[lastWordGroupIndex] :
                    Configuration.DefaultMaxWordGroupRepetitions;
            }


            foreach (int index in _nextWordIndexes)
            {
                word += Configuration.WordsSet[index];

                if (index == lastWordIndex)
                {
                    lastWordCounts++;
                }

                if (lastWordGroupIndex != int.MaxValue && index >= lastWordGroupMinWordIndex && index <= lastWordGroupMaxWordIndex)
                {
                    lastWordGroupCounts++;
                }
            }

            if (word.Length > Configuration.MaxLength || lastWordCounts > maxWordRepetitions || lastWordGroupCounts > maxGroupRepetitions)
            {
                _nextWordIndexes[_nextWordIndexes.Count - 1]++;

                return GetNextValidWord();
            }

            _nextWordIndexes.Add(0);

            CurrentWord = word;

            return CurrentWord;
        }
    }
}
