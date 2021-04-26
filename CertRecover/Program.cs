using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RecoveryCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CertRecover
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

            string logPath = config["logPath"].Value<string>();
            string certPath = config["certPath"].Value<string>();
            int recoveryTasksCount = config["recoveryTasksCount"].Value<int>();

            using RecoveryContext recoveryContext = new RecoveryContext(logPath, certPath);

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

            /*string logPath = "R:\\cert.log.txt";
            string certPath = "R:\\cert.p12";
            int recoveryTasksCount = 16;

            using RecoveryContext recoveryContext = new RecoveryContext(logPath, certPath);

            WordlistGeneratorConfiguration generatorConfiguration = new WordlistGeneratorConfiguration()
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
                WordlistOutFile = null,
                OutputToFile = false,
                LoadOnly = false,
            };*/
            using WordlistGenerator wordlistGenerator = new WordlistGenerator(generatorConfiguration);

            List<Task> recoveryTasks = new List<Task>();

            recoveryTasks.Add(new Task(() => new RecoveryEngine(recoveryContext, wordlistGenerator).StartRecovery()));

            for (int i = 0; i < recoveryTasksCount; i++)
            {
                recoveryTasks.Add(Task.Run(() =>
                    new RecoveryEngine(recoveryContext, wordlistGenerator).StartRecovery()));
            }

            foreach (var task in recoveryTasks)
            {
                task.Wait();
            }
        }
    }
}
