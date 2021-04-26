using System.Collections.Generic;

namespace RecoveryCore
{
    public class WordlistGeneratorConfiguration
    {
        public int MaxLength { get; set; } = 16;

        public List<string> PrefixSet { get; set; } = new List<string>();

        public List<string> SuffixSet { get; set; } = new List<string>();

        public List<string> WordsSet { get; set; } = new List<string>();

        public List<int> WordGroupOffsets { get; set; } = new List<int>();

        public List<int> MaxWordRepetitions { get; set; } = new List<int>();

        public List<int> MaxWordGroupRepetitions { get; set; } = new List<int>();

        public int DefaultMaxWordRepetitions { get; set; } = 4;

        public int DefaultMaxWordGroupRepetitions { get; set; } = 5;

        public List<int> StartingWordIndexes { get; set; } = new List<int>();

        public string WordlistOutFile { get; set; }

        public bool OutputToFile { get; set; } = false;

        public bool LoadOnly { get; set; } = false;
    }
}
