using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArkTools.Data;
using ArkTools.DataManager;
using MonoOptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SavegameToolkit;
using SavegameToolkitAdditions;

namespace ArkTools.Command {

    public abstract class CreaturesBaseCommand : BaseCommand {
        protected virtual string[] HelpHeaders { get; set; } = { };

        private bool untameable;
        private bool statistics;
        private bool withoutIndex;
        private bool cleanFolder;
        private bool writeAllFields;
        private string inventory = "summary";

        private ArkSavegame arkSavegame;
        private GameObjectContainer container;
        private string outputDirectory;

        protected CreaturesBaseCommand(string name, string help = null) : base(name, help) {
            Options = new OptionSet();
            // ReSharper disable once VirtualMemberCallInConstructor
            foreach (string helpHeader in HelpHeaders) {
                Options.Add(helpHeader);
            }

            Options.Add("include-untameable", "Include untameable high-level dinos.", s => untameable = s != null);
            Options.Add("statistics", "Wrap list of dinos in statistics block.", s => statistics = s != null);
            Options.Add("without-index", "Omits reading and writing classes.json", s => withoutIndex = s != null);
            Options.Add("clean", "Deletes all .json files in the target directory.", s => cleanFolder = s != null);
            Options.Add("write-all-fields", "Writes all the fields.", s => writeAllFields = s != null);
            Options.Add("inventory=", "Include inventory of creatures. Values: summary|long", s => inventory = s);
            Options.Add("h|?|help", "command specific help", s => showHelp = s != null);
        }

        protected void listImpl(Func<GameObject, bool> filter, IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            if (!withoutIndex) {
                ArkDataManager.LoadData(GlobalOptions.Language);
            }

            string savePath = argsList.Count > 0
                    ? argsList[0]
                    : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, GlobalOptions.ArkToolsConfiguration.ArkSavegameFilename);
            outputDirectory = argsList.Count > 1 ? argsList[1] : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, GlobalOptions.ArkToolsConfiguration.CreaturesPath);

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);
            // Load everything that is not an item and either has a parent or components
            // Drawback: includes structures and players
            arkSavegame = new ArkSavegame().ReadBinary<ArkSavegame>(savePath, readingOptions
                    .WithDataFiles(false)
                    .WithEmbeddedData(false)
                    .WithDataFilesObjectMap(false)
                    .WithObjectFilter(o => !o.IsItem && (o.Parent != null || o.Components.Any()))
                    .WithBuildComponentTree(true));
            stopwatch.Stop("Reading");
            writeAnimalLists(filter);
            stopwatch.Stop("Dumping");

            stopwatch.Print();
        }

        private void writeAnimalLists(Func<GameObject, bool> filter) {
            if (arkSavegame.HibernationEntries.Any()) {
                List<GameObject> combinedObjects = arkSavegame.Objects;

                foreach (HibernationEntry entry in arkSavegame.HibernationEntries) {
                    ObjectCollector collector = new ObjectCollector(entry, 1);
                    combinedObjects.AddRange(collector.Remap(combinedObjects.Count));
                }

                container = new GameObjectContainer(combinedObjects);
            } else {
                container = arkSavegame;
            }

            IEnumerable<GameObject> objectStream = container.Where(o => o.IsCreature());

            if (filter != null) {
                objectStream = objectStream.Where(filter);
            }

            if (!untameable) {
                IEnumerable<string> untameableClasses = GlobalOptions.ArkToolsConfiguration.UntameableClasses?.Split(",").Select(s => s.Trim()) ?? Enumerable.Empty<string>();
                objectStream = objectStream.Where(gameObject => !gameObject.GetPropertyValue<bool>("bForceDisablingTaming"));
                objectStream = objectStream.Where(gameObject => !untameableClasses.Contains(gameObject.ClassString));
            }

            if (cleanFolder) {
                foreach (string path in Directory.EnumerateFiles(outputDirectory, "*.json")) {
                    File.Delete(path);
                }
            }

            Dictionary<string, List<GameObject>> dinoLists = objectStream.GroupBy(o => o.ClassString)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

            if (!withoutIndex) {
                Dictionary<string, string> classNames = readClassNames();

                //Func<string, string> fetchName = key => ArkDataManager.GetCreature(key)?.Name ?? key;

                //foreach (string dinoClass in dinoLists.Keys.Where(dinoClass => !classNames.ContainsKey(dinoClass) || classNames[dinoClass] == null)) {
                foreach (string dinoClass in dinoLists.Keys.Where(dinoClass => !classNames.TryGetValue(dinoClass, out string value) || value == null)) {
                    //string name = fetchName(dinoClass);
                    string name = ArkDataManager.GetCreature(dinoClass)?.Name ?? dinoClass;
                    if (name != null)
                        classNames.Add(dinoClass, name);
                }

                writeClassNames(classNames);

                foreach (string s in classNames.Keys.Where(s => !dinoLists.ContainsKey(s))) {
                    writeEmpty(s);
                }
            }

            foreach (KeyValuePair<string, List<GameObject>> dinoList in dinoLists) {
                writeList(dinoList);
            }
        }

        private Dictionary<string, string> readClassNames() {
            string classFile = Path.Combine(outputDirectory, "classes.json");
            Dictionary<string, string> classNames = new Dictionary<string, string>();

            if (!File.Exists(classFile))
                return classNames;

            using (JsonTextReader parser = new JsonTextReader(File.OpenText(classFile))) {
                JArray classFileArray = JToken.ReadFrom(parser) as JArray;
                if (classFileArray == null) {
                    return classNames;
                }
                foreach (JToken entry in classFileArray) {
                    string cls = entry.Value<string>("cls");
                    string name = entry.Value<string>("name");
                    if (cls != null && name != null && !classNames.ContainsKey(cls)) {
                        classNames[cls] = name;
                    }
                }
            }

            return classNames;
        }

        private void writeClassNames(Dictionary<string, string> classNames) {
            string classFile = Path.Combine(outputDirectory, "classes.json");

            CommonFunctions.WriteJson(classFile, (writer, writingOptions) => {
                writer.WriteStartArray();

                foreach (KeyValuePair<string, string> className in classNames) {
                    writer.WriteStartObject();

                    writer.WriteField("cls", className.Key);
                    writer.WriteField("name", className.Value);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }, writingOptions);
        }

        private void writeList(KeyValuePair<string, List<GameObject>> entry) {
            string outputFile = Path.Combine(outputDirectory, entry.Key + ".json");

            List<GameObject> filteredClasses = entry.Value;
            MapData mapData = LatLonCalculator.ForSave(arkSavegame);

            CommonFunctions.WriteJson(outputFile, (generator, writingOptions) => {
                if (statistics) {
                    generator.WriteStartObject();

                    generator.WriteField("count", filteredClasses.Count);

                    List<int> summaryStatistics = filteredClasses.Where(o => o.IsWild())
                            .Select(a => a.GetBaseLevel(arkSavegame)).ToList();
                    if (summaryStatistics.Any()) {
                        generator.WriteField("wildMin", summaryStatistics.Min());
                        generator.WriteField("wildMax", summaryStatistics.Max());
                        generator.WriteField("wildAverage", summaryStatistics.Average());
                    }

                    List<int> tamedBaseStatistics = filteredClasses.Where(o => o.IsTamed())
                            .Select(a => a.GetBaseLevel(arkSavegame)).ToList();
                    if (tamedBaseStatistics.Any()) {
                        generator.WriteField("tamedBaseMin", tamedBaseStatistics.Min());
                        generator.WriteField("tamedBaseMax", tamedBaseStatistics.Max());
                        generator.WriteField("tamedBaseAverage", tamedBaseStatistics.Average());
                    }

                    List<int> tamedFullStatistics = filteredClasses.Where(o => o.IsTamed())
                            .Select(a => a.GetFullLevel(arkSavegame)).ToList();
                    if (tamedFullStatistics.Any()) {
                        generator.WriteField("tamedFullMin", tamedFullStatistics.Min());
                        generator.WriteField("tamedFullMax", tamedFullStatistics.Max());
                        generator.WriteField("tamedFullAverage", tamedFullStatistics.Average());
                    }

                    generator.WritePropertyName("dinos");
                    generator.WriteStartArray();
                } else {
                    generator.WriteStartArray();
                }

                CustomDataContext context = new CustomDataContext {
                        MapData = mapData,
                        ObjectContainer = container,
                        Savegame = arkSavegame
                };
                foreach (GameObject creatureObject in filteredClasses) {
                    Creature creature = new Creature(creatureObject, container);
                    generator.WriteStartObject();
                    creature.writeAllProperties(generator, context, writeAllFields);
                    if (!string.IsNullOrWhiteSpace(inventory)) {
                        creature.writeInventory(generator, context, writeAllFields, inventory == "summary");
                    }

                    generator.WriteEndObject();
                }

                generator.WriteEndArray();

                if (statistics) {
                    generator.WriteEndObject();
                }
            }, writingOptions);
        }

        private void writeEmpty(string className) {
            string outputFile = Path.Combine(outputDirectory, className + ".json");
            CommonFunctions.WriteJson(outputFile, (writer, writingOptions) => {
                if (statistics) {
                    writer.WriteStartObject();
                    writer.WriteField("count", 0);
                    writer.WritePropertyName("dinos");
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                } else {
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                }
            }, writingOptions);
        }
    }

    public class CreaturesCommand : CreaturesBaseCommand {
        private const string names = "creatures";
        private const string help = "Writes lists of all creatures in SAVE to the specified DIRECTORY.";

        protected override string[] HelpHeaders { get; set; } = {
                "",
                "creatures SAVE DIRECTORY [OPTIONS]",
                ""
        };

        public CreaturesCommand() : base(names, help) { }

        protected override void RunCommand(IEnumerable<string> args) {
            listImpl(null, args);
        }
    }

    public class TamedCommand : CreaturesBaseCommand {
        private const string names = "tamed";
        private const string help = "Writes lists of tamed creatures in SAVE to the specified DIRECTORY.";

        protected override string[] HelpHeaders { get; set; } = {
                "",
                "tamed SAVE DIRECTORY [OPTIONS]",
                ""
        };

        public TamedCommand() : base(names, help) { }

        protected override void RunCommand(IEnumerable<string> args) {
            listImpl(animal => TeamTypes.ForTeam(animal.GetPropertyValue<int>("TargetingTeam")).IsTamed(), args);
        }
    }

    public class WildCommand : CreaturesBaseCommand {
        private const string names = "wild";
        private const string help = "Writes lists of wild creatures in SAVE to the specified DIRECTORY.";

        protected override string[] HelpHeaders { get; set; } = {
                "",
                "wild SAVE DIRECTORY [OPTIONS]",
                ""
        };

        public WildCommand() : base(names, help) { }

        protected override void RunCommand(IEnumerable<string> args) {
            listImpl(animal => !TeamTypes.ForTeam(animal.GetPropertyValue<int>("TargetingTeam")).IsTamed(), args);
        }
    }

}
