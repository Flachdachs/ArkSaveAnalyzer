using System;
using System.Collections.Generic;
using System.Linq;
using MonoOptions;
using SavegameToolkit;
using SavegameToolkit.Types;

namespace ArkTools.Command {

    public abstract class DebugBaseCommand : BaseCommand {
        protected bool withoutDuplicates;
        protected bool withNames;

        protected virtual string[] HelpHeaders { get; set; } = { };

        protected DebugBaseCommand(string name, string help = null) : base(name, help) {
            Options = new OptionSet();
            // ReSharper disable once VirtualMemberCallInConstructor
            foreach (string helpHeader in HelpHeaders) {
                Options.Add(helpHeader);
            }

            Options.Add("without-dupes", "Removes duplicate objects", s => withoutDuplicates = s != null);
            Options.Add("with-names", "Write common name instead of class name where known.", s => withNames = s != null);
            Options.Add("h|?|help", "command specific help", s => showHelp = s != null);
        }
    }

    public class ClassesCommand : DebugBaseCommand {
        private const string names = "classes";
        private const string help = "Dumps a list of all classes with count of objects to stdout or OUT_FILE.";

        protected override string[] HelpHeaders { get; set; } = {
                "",
                "classes SAVE [OUT_FILE] [OPTIONS]",
                ""
        };

        public ClassesCommand() : base(names, help) { }

        protected override void RunCommand(IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            if (withNames) {
                DataManager.ArkDataManager.LoadData(GlobalOptions.Language);
            }

            string savePath = argsList[0];

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);

            // Don't load any properties, we don't need them
            ArkSavegame savegame = new ArkSavegame().ReadBinary<ArkSavegame>(savePath, ReadingOptions.Create()
                    .WithObjectFilter(o => false)
                    .WithBuildComponentTree(withoutDuplicates));

            stopwatch.Stop("Loading");

            List<GameObject> objects;
            //Dictionary<int, Dictionary<List<ArkName>, GameObject>> objectMap;
            Dictionary<int, Dictionary<int, GameObject>> objectMap;

            if (savegame.HibernationEntries.Any()) {
                objects = new List<GameObject>(savegame.Objects);
                if (withoutDuplicates) {
                    objectMap = new Dictionary<int, Dictionary<int, GameObject>>();
                    foreach (KeyValuePair<int, Dictionary<int, GameObject>> pair in savegame.ObjectMap) {
                        objectMap[pair.Key] = new Dictionary<int, GameObject>(pair.Value);
                    }
                } else {
                    objectMap = null;
                }

                foreach (HibernationEntry entry in savegame.HibernationEntries) {
                    ObjectCollector collector = new ObjectCollector(entry, 1);
                    objects.AddRange(collector.Remap(objects.Count));
                    if (withoutDuplicates) {
                        foreach (GameObject gameObject in collector) {
                            int key = gameObject.FromDataFile ? gameObject.DataFileIndex : -1;

                            if (!objectMap.ContainsKey(key))
                                objectMap[key] = new Dictionary<int, GameObject>();
                            if (!objectMap[key].ContainsKey(gameObject.Names.HashCode())) {
                                objectMap[key][gameObject.Names.HashCode()] = gameObject;
                            }
                        }
                    }
                }
            } else {
                objects = savegame.Objects;
                objectMap = savegame.ObjectMap;
            }

            if (withoutDuplicates) {
                //objects.removeIf((GameObject gameObject) => {
                //    int key = gameObject.IsFromDataFile ? gameObject.DataFileIndex : -1;
                //    return objectMap[key]?[gameObject.Names.HashCode()] != gameObject;
                //});
                objects = objects.Where(gameObject => {
                    int key = gameObject.FromDataFile ? gameObject.DataFileIndex : -1;
                    return objectMap[key]?[gameObject.Names.HashCode()] == gameObject;
                }).ToList();
            }

            Dictionary<string, List<GameObject>> map = objects.GroupBy(o => o.ClassString)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

            stopwatch.Stop("Grouping");

            WriteJsonCallback writer = (generator, writingOptions) => {
                generator.WriteStartObject();

                generator.WriteField("_count", objects.Count);

                foreach (KeyValuePair<string, List<GameObject>> entry in map.OrderByDescending(e => e.Value.Count)) {
                    string name = entry.Key;

                    if (withNames) {
                        if (entry.Value[0].IsItem) {
                            if (DataManager.ArkDataManager.HasItem(name)) {
                                name = DataManager.ArkDataManager.GetItem(name).Name;
                            }
                        } else {
                            if (DataManager.ArkDataManager.HasCreature(name)) {
                                name = DataManager.ArkDataManager.GetCreature(name).Name;
                            } else if (DataManager.ArkDataManager.HasStructure(name)) {
                                name = DataManager.ArkDataManager.GetStructure(name).Name;
                            }
                        }
                    }

                    generator.WriteField(name, entry.Value.Count);
                }

                generator.WriteEndObject();
            };

            if (argsList.Count > 1) {
                CommonFunctions.WriteJson(argsList[1], writer, writingOptions);
            } else {
                CommonFunctions.WriteJson(Console.OpenStandardOutput(), writer, writingOptions);
            }

            stopwatch.Stop("Writing");
            stopwatch.Print();
        }
    }

    public class DumpCommand : DebugBaseCommand {
        private const string names = "dump";
        private const string help = "Dumps all objects of given CLASS_NAME to stdout or OUT_FILE.";

        protected override string[] HelpHeaders { get; set; } = {
                "",
                "dump SAVE CLASS_NAME [OUT_FILE] [OPTIONS]",
                ""
        };

        public DumpCommand() : base(names, help) { }

        protected override void RunCommand(IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            string savePath = argsList[0];
            string className = argsList[1];

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);

            Predicate<GameObject> filter = gameObject => gameObject.ClassString == className;

            ArkSavegame savegame = new ArkSavegame().ReadBinary<ArkSavegame>(savePath, ReadingOptions.Create().WithObjectFilter(filter));

            stopwatch.Stop("Loading");

            WriteJsonCallback dumpObjects = (generator, writingOptions) => {
                generator.WriteStartArray();

                foreach (GameObject gameObject in savegame) {
                    if (filter.Invoke(gameObject)) {
                        gameObject.WriteJson(generator, writingOptions);
                    }
                }

                foreach (HibernationEntry entry in savegame.HibernationEntries) {
                    foreach (GameObject gameObject in entry) {
                        if (filter.Invoke(gameObject)) {
                            gameObject.WriteJson(generator, writingOptions);
                        }
                    }
                }

                generator.WriteEndArray();
            };

            if (argsList.Count > 2) {
                CommonFunctions.WriteJson(argsList[2], dumpObjects, writingOptions);
            } else {
                CommonFunctions.WriteJson(Console.OpenStandardOutput(), dumpObjects, writingOptions);
            }

            stopwatch.Stop("Writing");
            stopwatch.Print();
        }
    }

    public class SizesCommand : DebugBaseCommand {
        private const string names = "sizes";
        private const string help = "Dumps className and size in bytes of all objects to stdout or OUT_FILE.";

        protected override string[] HelpHeaders { get; set; } = {
                "",
                "sizes SAVE [OUT_FILE] [OPTIONS]",
                ""
        };

        public SizesCommand() : base(names, help) { }

        protected override void RunCommand(IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            string savePath = argsList[0];

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);

            ArkSavegame savegame = new ArkSavegame().ReadBinary<ArkSavegame>(savePath, ReadingOptions.Create());

            stopwatch.Stop("Loading");

            List<GameObject> hibernationObjects = new List<GameObject>();

            foreach (HibernationEntry entry in savegame.HibernationEntries) {
                hibernationObjects.AddRange(entry.Objects);
            }

            List<SizeObjectPair> pairList = savegame.Objects.Select(gameObject => new SizeObjectPair(gameObject, savegame.SaveVersion< 6, false))
                    .Concat(hibernationObjects.Select(gameObject => new SizeObjectPair(gameObject, false, true)))
                    .OrderByDescending(sizeObjectPair => sizeObjectPair.Size)
                    .ToList();

            stopwatch.Stop("Collecting");

            WriteJsonCallback writer = (generator, writingOptions) => {
                generator.WriteStartArray();

                foreach (SizeObjectPair pair in pairList) {
                    generator.WriteStartObject();
                    generator.WriteArrayFieldStart("names");
                    foreach (ArkName name in pair.GameObject.Names) {
                        generator.WriteValue(name.ToString());
                    }
                    generator.WriteEndArray();
                    if (pair.GameObject.FromDataFile) {
                        generator.WriteField("dataFileIndex", pair.GameObject.DataFileIndex);
                    }
                    generator.WriteField("class", pair.GameObject.ClassString);
                    generator.WriteField("size", pair.Size);
                    generator.WriteEndObject();
                }

                generator.WriteEndArray();
            };

            if (argsList.Count > 1) {
                CommonFunctions.WriteJson(argsList[1], writer, writingOptions);
            } else {
                CommonFunctions.WriteJson(Console.OpenStandardOutput(), writer, writingOptions);
            }

            stopwatch.Stop("Writing");
            stopwatch.Print();
        }

        private class SizeObjectPair {
            private static readonly NameSizeCalculator ANCIENT_SIZER = ArkArchive.GetNameSizer(false);
            private static readonly NameSizeCalculator MODERN_SIZER = ArkArchive.GetNameSizer(true);
            private static readonly NameSizeCalculator HIBERNATION_SIZER = ArkArchive.GetNameSizer(true, true);
            public int Size { get; }
            public GameObject GameObject { get; }

            public SizeObjectPair(GameObject gameObject, bool ancient, bool hibernation) {
                NameSizeCalculator nameSizer = hibernation ? HIBERNATION_SIZER : ancient ? ANCIENT_SIZER : MODERN_SIZER;
                GameObject = gameObject;
                Size = gameObject.Size(nameSizer) + gameObject.PropertiesSize(nameSizer);
            }
        }
    }

}
