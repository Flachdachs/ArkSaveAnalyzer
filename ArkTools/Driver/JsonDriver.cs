using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Reflection;
using ArkTools.Data;
using SavegameToolkit;

namespace ArkTools.Driver {
    public class JsonDriver : DBDriver {

        private static IList<string> PROTOCOL_LIST;

        private static IDictionary<string, string> PARAMETER_MAP;

        private static void testAndAddProtocol(string protocol, string testURL, List<string> protocolList) {
            if (Uri.IsWellFormedUriString(testURL, UriKind.Absolute)) {
                protocolList.Add(protocol);
            }
        }

        static JsonDriver() {
            List<string> protocols = new List<string>();

            testAndAddProtocol("http", "http://localhost", protocols);
            testAndAddProtocol("https", "https://localhost", protocols);
            testAndAddProtocol("file", "file://", protocols);
            testAndAddProtocol("mailto", "mailto:root@localhost", protocols);
            testAndAddProtocol("ftp", "ftp://localhost", protocols);

            PROTOCOL_LIST = protocols.ToImmutableList();

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add("creatureFields", "comma delimited list of fields to write - default: " + string.Join(",", Creature.PROPERTIES.Keys));
            parameters.Add("inventoryFields", "comma delimited list of fields to write - default: " + string.Join(",", Inventory.PROPERTIES.Keys));
            parameters.Add("itemFields", "comma delimited list of fields to write - default: " + string.Join(",", Item.PROPERTIES.Keys));
            parameters.Add("droppedItemFields", "comma delimited list of fields to write - default: " + string.Join(",", DroppedItem.PROPERTIES.Keys));
            parameters.Add("playerFields", "comma delimited list of fields to write - default: " + string.Join(",", Player.Properties.Keys));
            parameters.Add("structureFields", "comma delimited list of fields to write - default: " + string.Join(",", Structure.PROPERTIES.Keys));
            parameters.Add("tribeFields", "comma delimited list of fields to write - default: " + string.Join(",", Tribe.PROPERTIES.Keys));

            parameters.Add("writeEmpty", "force writing of empty fields");

            PARAMETER_MAP = parameters.ToImmutableDictionary();
        }

        private WebRequest conn;

        private Stream os;

        private Dictionary<string, string> p = new Dictionary<string, string>();

        public JsonDriver() {
            p.Add("writeEmpty", false.ToString());
        }

        public void openConnection(Uri uri) {
            try {
                conn = WebRequest.Create(uri);

                if (conn is HttpWebRequest httpConn) {
                    string myVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

                    httpConn.Method = "POST";
                    httpConn.Headers.Add("User-Agent", "ark-tools/" + myVersion);
                }

                //conn.setDoOutput(true);

                os = conn.GetRequestStream();
            } catch (Exception e) {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        public void openConnection(string path) {
            os = File.Create(path);
        }


        public IList<string> getUrlSchemeList() {
            return PROTOCOL_LIST;
        }

        public bool canHandlePath() {
            return true;
        }


        public string getParameter(string name) {
            return p[name];
        }


        public DBDriver setParameter(string name, string value) {
            if (!PARAMETER_MAP.ContainsKey(name)) {
                throw new NotSupportedException("Unknown parameter: " + name);
            }

            p[name] = value;
            return this;
        }


        public IDictionary<string, string> getSupportedParameters() {
            return PARAMETER_MAP;
        }

        private List<WriterFunction<T>> generateList<T>(string paramName, SortedDictionary<string, WriterFunction<T>> map) {
            List<WriterFunction<T>> result = new List<WriterFunction<T>>();

            string paramValue = p[paramName];

            if (paramValue == null || string.IsNullOrEmpty(paramValue)) {
                foreach (string key in map.Keys) {
                    result.Add(map[key]);
                }
            }
            else {
                string[] keys = paramValue.Split(',');
                foreach (string key in keys) {
                    if (map.ContainsKey(key)) {
                        result.Add(map[key]);
                    }
                }
            }

            return result;
        }

        public void write(DataCollector data, WritingOptions options) {
            List<WriterFunction<Creature>> creatureWriters = generateList("creatureFields", Creature.PROPERTIES);
            List<WriterFunction<Inventory>> inventoryWriters = generateList("inventoryFields", Inventory.PROPERTIES);
            List<WriterFunction<Item>> itemWriters = generateList("itemFields", Item.PROPERTIES);
            List<WriterFunction<DroppedItem>> droppedItemWriters = generateList("droppedItemFields", DroppedItem.PROPERTIES);
            List<WriterFunction<Player>> playerWriters = generateList("playerFields", Player.Properties);
            List<WriterFunction<Structure>> structureWriters = generateList("structureFields", Structure.PROPERTIES);
            List<WriterFunction<Tribe>> tribeWriters = generateList("tribeFields", Tribe.PROPERTIES);

            bool.TryParse(p["writeEmpty"], out bool writeEmpty);

            CommonFunctions.WriteJson(os, (generator, writingOptions) => {
                generator.WriteStartObject();

                generator.WriteObjectFieldStart("creatures");

                foreach (int index in data.CreatureMap.Keys) {
                    Creature creature = data.CreatureMap[index];

                    generator.WriteObjectFieldStart(index.ToString());

                    foreach (WriterFunction<Creature> writer in creatureWriters) {
                        writer(creature, generator, data, writeEmpty);
                    }

                    generator.WriteEndObject();
                }

                generator.WriteEndObject();

                generator.WriteObjectFieldStart("inventories");

                foreach (int index in data.InventoryMap.Keys) {
                    Inventory inventory = data.InventoryMap[index];

                    generator.WriteObjectFieldStart(index.ToString());

                    foreach (WriterFunction<Inventory> writer in inventoryWriters) {
                        writer(inventory, generator, data, writeEmpty);
                    }

                    generator.WriteEndObject();
                }

                generator.WriteEndObject();

                generator.WriteObjectFieldStart("items");

                foreach (int index in data.ItemMap.Keys) {
                    Item item = data.ItemMap[index];

                    generator.WriteObjectFieldStart(index.ToString());

                    foreach (WriterFunction<Item> writer in itemWriters) {
                        writer(item, generator, data, writeEmpty);
                    }

                    generator.WriteEndObject();
                }

                generator.WriteEndObject();

                generator.WriteObjectFieldStart("droppedItems");

                foreach (int index in data.DroppedItemMap.Keys) {
                    DroppedItem droppedItem = data.DroppedItemMap[index];

                    generator.WriteObjectFieldStart(index.ToString());

                    foreach (WriterFunction<DroppedItem> writer in droppedItemWriters) {
                        writer(droppedItem, generator, data, writeEmpty);
                    }

                    generator.WriteEndObject();
                }

                generator.WriteEndObject();

                generator.WriteObjectFieldStart("players");

                foreach (long index in data.PlayerMap.Keys) {
                    Player player = data.PlayerMap[index];

                    generator.WriteObjectFieldStart(index.ToString());

                    foreach (WriterFunction<Player> writer in playerWriters) {
                        writer(player, generator, data, writeEmpty);
                    }

                    generator.WriteEndObject();
                }

                generator.WriteEndObject();

                generator.WriteObjectFieldStart("structures");

                foreach (int index in data.StructureMap.Keys) {
                    Structure structure = data.StructureMap[index];

                    generator.WriteObjectFieldStart(index.ToString());

                    foreach (WriterFunction<Structure> writer in structureWriters) {
                        writer(structure, generator, data, writeEmpty);
                    }

                    generator.WriteEndObject();
                }

                generator.WriteEndObject();

                generator.WriteObjectFieldStart("tribes");

                foreach (int index in data.TribeMap.Keys) {
                    Tribe tribe = data.TribeMap[index];

                    generator.WriteObjectFieldStart(index.ToString());

                    foreach (WriterFunction<Tribe> writer in tribeWriters) {
                        writer(tribe, generator, data, writeEmpty);
                    }

                    generator.WriteEndObject();
                }

                generator.WriteEndObject();

                generator.WriteEndObject();
            }, options);
        }

        public void close() {
            try {
                os.Flush();
                os.Close();
                conn?.GetResponse().Close();
            } catch (Exception e) {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
        }

    }
}
