using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArkTools.DataManager{

    public static class ArkDataManager{
        private const string dataFilename = "ark_data";

        private const string dataFilenameExt = ".json";

        private static readonly Dictionary<string, CreatureData> creatures = new Dictionary<string, CreatureData>();

        private static readonly Dictionary<string, CreatureData> creaturesByPath = new Dictionary<string, CreatureData>();

        private static readonly Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();

        private static readonly Dictionary<string, CreatureData> structures = new Dictionary<string, CreatureData>();

        public static void LoadData(string language) {
            try {
                string filename = dataFilename + (language != null ? $"_{language}" : string.Empty) + dataFilenameExt;

                JToken data;

                try {
                    data = JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, filename))));
                } catch (FileNotFoundException ex) {
                    throw new Exception("Unable to load data file ." + filename, ex);
                }

                JArray creaturesNode = data.Value<JArray>("creatures");

                foreach (JToken entry in creaturesNode) {
                    string packagePath = entry.Value<string>("package");
                    string blueprint = entry.Value<string>("blueprint");
                    string @class = entry.Value<string>("class");
                    string name = entry.Value<string>("name");
                    string category = entry.Value<string>("category");

                    CreatureData creature = new CreatureData(name, @class, blueprint, packagePath, category);
                    creatures[@class] = creature;
                    creaturesByPath[packagePath + "." + @class] = creature;
                }

                JArray itemsNode = data.Value<JArray>("items");

                foreach (JToken entry in itemsNode) {
                    string packagePath = entry.Value<string>("package");
                    string blueprint = entry.Value<string>("blueprint");
                    string @class = entry.Value<string>("class");
                    string name = entry.Value<string>("name");
                    string category = entry.Value<string>("category");

                    string blueprintGeneratedClass = "BlueprintGeneratedClass " + packagePath + "." + @class;

                    ItemData item = new ItemData(name, blueprint, blueprintGeneratedClass, category);
                    items[@class] = item;
                }

                JArray structuresNode = data.Value<JArray>("structures");

                foreach (JToken entry in structuresNode) {
                    
                    string packagePath = entry.Value<string>("package");
                    string blueprint = entry.Value<string>("blueprint");
                    string @class = entry.Value<string>("class");
                    string name = entry.Value<string>("name");
                    string category = entry.Value<string>("category");

                    structures[@class] = new CreatureData(name, @class, blueprint, packagePath, category);
                }
            } catch (Exception e) {
                Console.Error.WriteLine("Warning: Cannot load data.");
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        public static bool HasCreature(string @class) {
            return creatures.ContainsKey(@class);
        }

        public static CreatureData GetCreature(string @class) {
            return creatures.TryGetValue(@class, out CreatureData creature) ? creature : null;
        }

        public static bool HasCreatureByPath(string @class) {
            return creaturesByPath.ContainsKey(@class);
        }

        public static CreatureData GetCreatureByPath(string @class) {
            return creaturesByPath.TryGetValue(@class, out CreatureData creature) ? creature : null;
        }

        public static bool HasStructure(string @class) {
            return structures.ContainsKey(@class);
        }

        public static CreatureData GetStructure(string @class) {
            return structures.TryGetValue(@class, out CreatureData creature) ? creature : null;
        }

        public static bool HasItem(string @class) {
            return items.ContainsKey(@class);
        }

        public static ItemData GetItem(string @class) {
            return items.TryGetValue(@class, out ItemData item) ? item : null;
        }
    }

}
