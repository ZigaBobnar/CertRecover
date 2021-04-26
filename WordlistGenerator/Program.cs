using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RecoveryCore;
using System;
using System.IO;
using System.Linq;

namespace WordlistGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("config.json"))
            {
                Console.WriteLine("Configuration file config.json not found.");
                Environment.Exit(1);
            }

            string configContents = File.ReadAllText("config.json");
            JObject config = JsonConvert.DeserializeObject<JObject>(configContents);

            JObject generatorConfig = config["generator"].Value<JObject>();
            WordlistGeneratorConfiguration generatorConfiguration = new WordlistGeneratorConfiguration()
            {
                MaxLength = generatorConfig["maxLength"].Value<int>(),
                PrefixSet = generatorConfig["prefixSet"].Value<JArray>().Values().Select(x => x.Value<string>()).ToList(),
                SuffixSet = generatorConfig["suffixSet"].Value<JArray>().Values().Select(x => x.Value<string>()).ToList(),
                WordsSet = generatorConfig["wordsSet"].Value<JArray>().Values().Select(x => x.Value<string>()).ToList(),
                WordGroupOffsets = generatorConfig["wordGroupOffsets"].Value<JArray>().Values().Select(x => x.Value<int>()).ToList(),
                MaxWordRepetitions = generatorConfig["maxWordRepetitions"].Value<JArray>().Values().Select(x => x.Value<int>()).ToList(),
                MaxWordGroupRepetitions = generatorConfig["maxWordGroupRepetitions"].Value<JArray>().Values().Select(x => x.Value<int>()).ToList(),
                DefaultMaxWordRepetitions = generatorConfig["defaultMaxWordRepetitions"].Value<int>(),
                DefaultMaxWordGroupRepetitions = generatorConfig["defaultMaxWordGroupRepetitions"].Value<int>(),
                StartingWordIndexes = generatorConfig["startingIndexes"].Value<JArray>().Values().Select(x => x.Value<int>()).ToList(),
                WordlistOutFile = generatorConfig["wordlistOutFile"].Value<string>(),
                OutputToFile = generatorConfig["outputToFile"].Value<bool>(),
                LoadOnly = generatorConfig["loadOnly"].Value<bool>(),
            };
            /*WordlistGeneratorConfiguration generatorConfiguration = new WordlistGeneratorConfiguration()
            {
                MaxLength = 14,
                WordsSet = new List<string>
                {
                    "123", "321", "789", "987", "999",
                    "foo", "Foo", "FOO", "oof", "OOF", "ooF" ,
                    "bar", "Bar", "BAR", "rab", "RAB", "raB" ,
                    ".", ",", "/", "*", "-", "+",
                },
                WordGroupOffsets = new List<int> { 0, 5, 11, 17 },
                MaxWordRepetitions = new List<int> { },
                MaxWordGroupRepetitions = new List<int> { 1, 1, 1, 3 },
                DefaultMaxWordRepetitions = 3,
                DefaultMaxWordGroupRepetitions = 4,
                StartingWordIndexes = new List<int> { 0 },
                WordlistOutFile = "R:\\wordlist.txt",
                OutputToFile = true,
                LoadOnly = false,
            };*/
            using RecoveryCore.WordlistGenerator wordlistGenerator = new RecoveryCore.WordlistGenerator(generatorConfiguration);

            int counter = 1;
            while (!wordlistGenerator.OutOfWords)
            {
                string word = wordlistGenerator.GetNextWord();

                if (counter % 50000 == 0 || counter == 1)
                {
                    Console.WriteLine($"Generated {counter} words. Current word: {word}.");
                    Console.WriteLine($"    Next indexes: {string.Join(',', wordlistGenerator.NextWordIndexes)}");
                }

                counter++;
            }

            if (wordlistGenerator.OutOfWords)
            {
                Console.WriteLine("Generator ran out of words.");
                Console.ReadLine();
            }
        }
    }
}
