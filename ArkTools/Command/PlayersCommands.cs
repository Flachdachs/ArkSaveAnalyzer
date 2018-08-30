using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArkTools.Data;
using ArkTools.DataManager;
using MonoOptions;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Arrays;
using SavegameToolkit.Structs;
using SavegameToolkit.Types;
using SavegameToolkitAdditions;

namespace ArkTools.Command {

    public abstract class PlayersBaseCommand : BaseCommand {
        protected PlayersBaseCommand(string name, string help, IEnumerable<string> helpHeaders) : base(name, help) {
            Options = new OptionSet();
            // ReSharper disable once VirtualMemberCallInConstructor
            foreach (string helpHeader in helpHeaders) {
                Options.Add(helpHeader);
            }
        }
    }

    public class PlayersCommand : PlayersBaseCommand {
        private static readonly Regex profilePattern = new Regex(@"(\d+|LocalPlayer)\.arkprofile");

        private const string names = "players";
        private const string help = "Writes lists of all players in SAVE to the specified DIRECTORY.";
        private static readonly string[] helpHeaders = {
                "",
                "players SAVE DIRECTORY [OPTIONS]",
                ""
        };

        private bool noPrivacy;
        private string naming;
        private string inventory;
        private bool positions;
        private int? maxAge;
        private bool writeAllFields;

        public PlayersCommand() : base(names, help, helpHeaders) {
            Options.Add("no-privacy", "Include privacy related data (SteamID, IP).", s => noPrivacy = s != null);
            Options.Add("naming=", "Decides how to name the resulting files. Values: steamid|playerid", s => naming = s);
            Options.Add("inventory=", "Include inventory of players. Values: summary|long", s => inventory = s);
            Options.Add("positions", "Include current position of players.", s => positions = s != null);
            Options.Add("max-age=", "Ignore all player files older then <seconds> seconds.", s => {
                if (int.TryParse(s, out int m)) {
                    maxAge = m;
                }
            });
            Options.Add("write-all-fields", "Writes all the fields.", s => writeAllFields = s != null);
            Options.Add("h|?|help", "command specific help", s => showHelp = s != null);
        }

        protected override void RunCommand(IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            ArkDataManager.LoadData(GlobalOptions.Language);

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);

            bool mapNeeded = !string.IsNullOrEmpty(inventory) || positions;
            if (!GlobalOptions.Quiet && mapNeeded) {
                Console.WriteLine("Need to load map, this may take some time...");
            }

            string saveGame = argsList.Count > 0
                    ? argsList[0]
                    : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, GlobalOptions.ArkToolsConfiguration.ArkSavegameFilename);
            string outputDirectory = argsList.Count > 1 ? argsList[1] : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, GlobalOptions.ArkToolsConfiguration.PlayersPath);
            string saveDir = Path.GetDirectoryName(saveGame);

            CustomDataContext context = new CustomDataContext();

            if (mapNeeded) {
                ArkSavegame mapSave = new ArkSavegame().ReadBinary<ArkSavegame>(saveGame, ReadingOptions.Create().WithBuildComponentTree(true));
                context.ObjectContainer = mapSave;
                context.Savegame = mapSave;
                context.MapData = LatLonCalculator.ForSave(mapSave);
                stopwatch.Stop("Loading map data");
            }

            Dictionary<int, Tribe> tribes = new Dictionary<int, Tribe>();
            List<Action> tasks = GlobalOptions.Parallel ? new List<Action>() : null;

            foreach (string path in Directory.EnumerateFiles(saveDir).Where(path => profilePattern.IsMatch(Path.GetFileName(path)))) {
                if (maxAge.HasValue) {
                    DateTimeOffset fileTime = File.GetLastWriteTimeUtc(path);

                    if (fileTime < DateTimeOffset.UtcNow.AddSeconds(-maxAge.Value)) {
                        continue;
                    }
                }

                Action task = () => {
                    try {
                        Player player = new Player(path, context, ReadingOptions.Create());

                        long playerId = player.playerDataId;

                        string playerFileName;
                        if (naming == "steamid" || string.IsNullOrWhiteSpace(naming)) {
                            playerFileName = player.uniqueId.NetId + ".json";
                        } else if (naming == "playerid") {
                            playerFileName = playerId + ".json";
                        } else {
                            throw new Exception("Invalid value for parameter naming.");
                        }

                        if (player.tribeId != 0) {
                            if (!tribes.ContainsKey(player.tribeId)) {
                                string tribePath = Path.Combine(saveDir, player.tribeId + ".arktribe");
                                if (File.Exists(tribePath)) {
                                    try {
                                        tribes[player.tribeId] = new Tribe(tribePath, ReadingOptions.Create());
                                    } catch (Exception ex) {
                                        // Either the header didn't match or one of the properties is missing
                                        Console.Error.WriteLine("Found potentially corrupt ArkTribe: " + tribePath);
                                        if (GlobalOptions.Verbose) {
                                            Console.Error.WriteLine(ex.Message);
                                            Console.Error.WriteLine(ex.StackTrace);
                                        }
                                    }
                                }
                            }
                        }

                        string playerPath = Path.Combine(outputDirectory, playerFileName);

                        void writePlayer(JsonTextWriter writer, WritingOptions options) {
                            writer.WriteStartObject();

                            // Player data

                            player.WriteAllProperties(writer, context, writeAllFields, noPrivacy);

                            // Inventory
                            if (!string.IsNullOrEmpty(inventory)) {
                                player.WriteInventory(writer, context, writeAllFields, inventory.ToLowerInvariant() != "long");
                            }

                            // Tribe

                            if (player.tribeId != 0 && tribes.TryGetValue(player.tribeId, out Tribe tribe)) {
                                writer.WriteField("tribeName", tribe.tribeName);

                                if (writeAllFields || tribe.ownerPlayerDataId == playerId) {
                                    writer.WriteField("tribeOwner", tribe.ownerPlayerDataId == playerId);
                                }

                                if (writeAllFields || tribe.tribeAdmins.Contains((int)playerId)) {
                                    writer.WriteField("tribeAdmin", tribe.tribeAdmins.Contains((int)playerId));
                                }
                            }

                            writer.WriteEndObject();
                        }

                        CommonFunctions.WriteJson(playerPath, writePlayer, writingOptions);
                    } catch (Exception ex) when (!Debugger.IsAttached) {
                        Console.Error.WriteLine("Found potentially corrupt ArkProfile: " + path);
                        if (GlobalOptions.Verbose) {
                            Console.Error.WriteLine(ex.Message);
                            Console.Error.WriteLine(ex.StackTrace);
                        }
                    }
                };

                if (tasks != null) {
                    tasks.Add(task);
                } else {
                    task();
                }
            }

            if (tasks != null) {
                Parallel.ForEach(tasks, task => task());
            }

            stopwatch.Stop("Loading profiles and writing info");
            stopwatch.Print();
        }
    }

    public class TribesCommand : PlayersBaseCommand {
        private static readonly Regex tribePattern = new Regex(@"\d+\.arktribe");

        private static readonly Regex basePattern = new Regex(@"\s*Base:\s*(.+)\s*<br>Size:\s*(\d+)\s*", RegexOptions.IgnoreCase);

        private const string names = "tribes";
        private const string help = "Writes lists of all tribes in SAVE to the specified DIRECTORY.";
        private static readonly string[] helpHeaders = {
                "",
                "tribes SAVE DIRECTORY [OPTIONS]",
                ""
        };

        private string withItems;
        private string withInventory;
        private string tamed;
        private string withStructures;
        private bool withBases;
        private bool tribeless;
        private bool nonPlayers;
        private bool writeAllFields;

        public TribesCommand() : base(names, help, helpHeaders) {
            Options.Add("items=", "Include a list of all items belonging to the tribe. Values: summary|long", s => withItems = s);
            Options.Add("inventory=", "Include inventories of creatures, players and structures. Values: summary|long", s => withInventory = s);
            Options.Add("tamed=", "Include a list of all tamed dinos of the tribe. Values: summary|long", s => tamed = s);
            Options.Add("structures=", "Include a list of all structures belonging to the tribe. Values: summary|long", s => withStructures = s);
            Options.Add("bases", "Allows tribes to create 'bases', groups creatures etc by base.", s => withBases = s != null);
            Options.Add("tribeless", "Put all players without a tribe into the 'tribeless' tribe.", s => tribeless = s != null);
            Options.Add("non-players", "Include stuff owned by non-player factions. Generates a 'non-players.json' file.", s => nonPlayers = s != null);
            Options.Add("write-all-fields", "Writes all the fields.", s => writeAllFields = s != null);
        }

        protected override void RunCommand(IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            bool itemsLong = withItems == "long";
            bool inventoryLong = withInventory == "long";
            bool tamedLong = tamed == "long";
            bool structuresLong = withStructures == "long";

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);

            bool mapNeeded = withItems != null || tamed != null || withStructures != null || withInventory != null;
            if (!GlobalOptions.Quiet && mapNeeded) {
                Console.WriteLine("Need to load map, this may take some time...");
            }

            string saveGame = argsList.Count > 0 ? argsList[0] : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, GlobalOptions.ArkToolsConfiguration.ArkSavegameFilename);
            string outputDirectory = argsList.Count > 1 ? argsList[1] : Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, GlobalOptions.ArkToolsConfiguration.TribesPath);
            string saveDir = Path.GetDirectoryName(saveGame);

            Dictionary<int, HashSet<TribeBase>> baseMap;
            CustomDataContext context = new CustomDataContext();

            if (mapNeeded) {
                ArkDataManager.LoadData(GlobalOptions.Language);

                ArkSavegame mapSave = new ArkSavegame().ReadBinary<ArkSavegame>(saveGame, ReadingOptions.Create().WithBuildComponentTree(true));
                context.Savegame = mapSave;
                context.MapData = LatLonCalculator.ForSave(context.Savegame);
                stopwatch.Stop("Loading map data");
                if (withBases) {
                    baseMap = new Dictionary<int, HashSet<TribeBase>>();
                    foreach (GameObject gameObject in mapSave) {
                        // Skip items and stuff without a location
                        if (gameObject.IsItem || gameObject.Location == null) {
                            continue;
                        }

                        string signText = gameObject.GetPropertyValue<string>("SignText");
                        long? targetingTeam = gameObject.GetPropertyValue<long?>("TargetingTeam");

                        if (signText != null && targetingTeam != null) {
                            // Might be a 'Base' sign
                            MatchCollection matcher = basePattern.Matches(signText);
                            if (matcher.Any()) {
                                // Found a base sign, add it to the set, automatically replacing duplicates
                                int tribeId = (int)targetingTeam;
                                LocationData location = gameObject.Location;
                                string baseName = matcher[1].Value;
                                float size = float.Parse(matcher[2].Value);

                                TribeBase tribeBase = new TribeBase(baseName, location.X, location.Y, location.Z, size);

                                if (!baseMap.ContainsKey(tribeId)) {
                                    baseMap[tribeId] = new HashSet<TribeBase>();
                                }

                                baseMap[tribeId].Add(tribeBase);
                            }
                        }
                    }

                    stopwatch.Stop("Collecting bases");
                } else {
                    baseMap = null;
                }

                if (mapSave.HibernationEntries.Any() && tamed != null) {
                    List<GameObject> combinedObjects = context.Savegame.Objects.ToList();

                    foreach (HibernationEntry entry in context.Savegame.HibernationEntries) {
                        ObjectCollector collector = new ObjectCollector(entry, 1);
                        combinedObjects.AddRange(collector.Remap(combinedObjects.Count));
                    }

                    context.ObjectContainer = new GameObjectContainer(combinedObjects);
                } else {
                    context.ObjectContainer = mapSave;
                }
            } else {
                baseMap = null;
            }

            List<Action> tasks = GlobalOptions.Parallel ? new List<Action>() : null;

            void mapWriter(JsonTextWriter generator, int tribeId) {
                if (!mapNeeded)
                    return;
                List<GameObject> structures = new List<GameObject>();
                List<GameObject> creatures = new List<GameObject>();
                List<Item> items = new List<Item>();
                List<Item> blueprints = new List<Item>();
                List<DroppedItem> droppedItems = new List<DroppedItem>();
                // Apparently there is a behavior in ARK causing certain structures to exist twice within a save
                HashSet<ArkName> processedList = new HashSet<ArkName>();
                // Bases
                HashSet<TribeBase> bases = withBases ? baseMap[tribeId] : null;

                foreach (GameObject gameObject in context.ObjectContainer) {
                    if (gameObject.IsItem) {
                        continue;
                    }

                    int targetingTeam = gameObject.GetPropertyValue<int>("TargetingTeam", defaultValue: -1);
                    if (targetingTeam == -1) {
                        continue;
                    }

                    TeamType teamType = TeamTypes.ForTeam(targetingTeam);
                    if (tribeId == -1 && teamType != TeamType.Player) {
                        continue;
                    }

                    if (tribeId == 0 && teamType != TeamType.NonPlayer) {
                        continue;
                    }

                    if (tribeId > 0 && tribeId != targetingTeam) {
                        continue;
                    }

                    // Determine base if we have bases
                    TribeBase tribeBase;
                    if (bases != null && gameObject.Location != null) {
                        TribeBase matchedBase = null;
                        foreach (TribeBase potentialBase in bases) {
                            if (potentialBase.InsideBounds(gameObject.Location)) {
                                matchedBase = potentialBase;
                                break;
                            }
                        }

                        tribeBase = matchedBase;
                    } else {
                        tribeBase = null;
                    }

                    if (gameObject.IsCreature()) {
                        if (!processedList.Contains(gameObject.Names[0])) {
                            if (tribeBase != null) {
                                tribeBase.Creatures.Add(gameObject);
                            } else {
                                creatures.Add(gameObject);
                            }

                            processedList.Add(gameObject.Names[0]);
                        } else {
                            // Duped Creature
                            continue;
                        }
                    } else if (!gameObject.IsPlayer() && !gameObject.IsWeapon() && !gameObject.IsDroppedItem()) {
                        // LinkedPlayerDataID: Players ain't structures
                        // AssociatedPrimalItem: Items equipped by sleeping players
                        // MyItem: dropped item
                        if (!processedList.Contains(gameObject.Names[0])) {
                            if (tribeBase != null) {
                                tribeBase.Structures.Add(gameObject);
                            } else {
                                structures.Add(gameObject);
                            }

                            processedList.Add(gameObject.Names[0]);
                        } else {
                            // Duped Structure
                            continue;
                        }
                    } else {
                        if (!processedList.Contains(gameObject.Names[0])) {
                            processedList.Add(gameObject.Names[0]);
                        } else {
                            // Duped Player or dropped Item or weapon
                            continue;
                        }
                    }

                    void itemHandler(ObjectReference itemReference) {
                        GameObject item = itemReference.GetObject(context.Savegame);
                        if (item != null && !Item.isDefaultItem(item)) {
                            if (processedList.Contains(item.Names[0])) {
                                // happens for players having items in their quick bar
                                return;
                            }

                            processedList.Add(item.Names[0]);

                            if (item.HasAnyProperty("bIsBlueprint")) {
                                if (tribeBase != null) {
                                    tribeBase.Blueprints.Add(new Item(item));
                                } else {
                                    blueprints.Add(new Item(item));
                                }
                            } else {
                                if (tribeBase != null) {
                                    tribeBase.Items.Add(new Item(item));
                                } else {
                                    items.Add(new Item(item));
                                }
                            }
                        }
                    }

                    void droppedItemHandler(GameObject droppedItemObject) {
                        DroppedItem droppedItem = new DroppedItem(droppedItemObject, context.Savegame);
                        if (tribeBase != null) {
                            tribeBase.DroppedItems.Add(droppedItem);
                        } else {
                            droppedItems.Add(droppedItem);
                        }
                    }

                    if (withItems != null && withInventory == null) {
                        foreach (GameObject inventory in gameObject.Components.Values) {
                            if (!inventory.IsInventory()) {
                                continue;
                            }

                            List<ObjectReference> inventoryItems = inventory.GetPropertyValue<IArkArray, ArkArrayObjectReference>("InventoryItems");
                            foreach (ObjectReference itemReference in inventoryItems ?? Enumerable.Empty<ObjectReference>()) {
                                itemHandler(itemReference);
                            }

                            List<ObjectReference> equippedItems = inventory.GetPropertyValue<IArkArray, ArkArrayObjectReference>("EquippedItems");
                            foreach (ObjectReference itemReference in equippedItems ?? Enumerable.Empty<ObjectReference>()) {
                                itemHandler(itemReference);
                            }
                        }
                    }

                    ObjectReference myItem = gameObject.GetPropertyValue<ObjectReference>("MyItem");

                    if (myItem != null) {
                        if (withItems != null && withInventory == null) {
                            itemHandler(myItem);
                        } else if (withInventory != null) {
                            droppedItemHandler(gameObject);
                        }
                    }
                }

                void writeStructures(IEnumerable<GameObject> structList) {
                    if (withStructures == null)
                        return;
                    generator.WriteArrayFieldStart("structures");

                    if (structuresLong) {
                        foreach (GameObject structureObject in structList) {
                            Structure structure = new Structure(structureObject, context.Savegame);

                            generator.WriteStartObject();

                            structure.writeAllProperties(generator, context, writeAllFields);

                            if (withInventory != null) {
                                structure.writeInventory(generator, context, writeAllFields, !inventoryLong);
                            }

                            generator.WriteEndObject();
                        }
                    } else {
                        Dictionary<ArkName, long> structMap = structList.GroupBy(o => o.ClassName).ToDictionary(objects => objects.Key, objects => objects.LongCount());
                        foreach (KeyValuePair<ArkName, long> entry in structMap.OrderByDescending(pair => pair.Value)) {
                            generator.WriteStartObject();

                            string name = entry.Key.ToString();
                            if (ArkDataManager.HasStructure(name)) {
                                name = ArkDataManager.GetStructure(name).Name;
                            }

                            generator.WriteField("name", name);
                            generator.WriteField("count", entry.Value);

                            generator.WriteEndObject();
                        }
                    }

                    generator.WriteEndArray();
                }

                void writeCreatures(List<GameObject> creaList) {
                    if (tamed == null)
                        return;
                    generator.WriteArrayFieldStart("tamed");

                    if (tamedLong) {
                        foreach (GameObject creatureObject in creaList) {
                            Creature creature = new Creature(creatureObject, context.Savegame);

                            generator.WriteStartObject();

                            creature.writeAllProperties(generator, context, writeAllFields);

                            if (withInventory != null) {
                                creature.writeInventory(generator, context, writeAllFields, !inventoryLong);
                            }

                            generator.WriteEndObject();
                        }
                    } else {
                        Dictionary<ArkName, long> creaMap = creaList.GroupBy(o => o.ClassName).ToDictionary(objects => objects.Key, objects => objects.LongCount());
                        foreach (KeyValuePair<ArkName, long> entry in creaMap.OrderByDescending(pair => pair.Value)) {
                            generator.WriteStartObject();

                            string name = entry.Key.ToString();
                            if (ArkDataManager.HasCreature(name)) {
                                name = ArkDataManager.GetCreature(name).Name;
                            }

                            generator.WriteField("name", name);
                            generator.WriteField("count", entry.Value);

                            generator.WriteEndObject();
                        }
                    }

                    generator.WriteEndArray();
                }

                void writeDroppedItems(List<DroppedItem> droppedList) {
                    if (withInventory == null)
                        return;
                    generator.WriteArrayFieldStart("droppedItems");

                    foreach (DroppedItem droppedItem in droppedList) {
                        generator.WriteStartObject();

                        droppedItem.writeAllProperties(generator, context, writeAllFields);
                        droppedItem.writeInventory(generator, context, writeAllFields, !inventoryLong);

                        generator.WriteEndObject();
                    }

                    generator.WriteEndArray();
                }

                if (withBases && bases != null) {
                    generator.WriteArrayFieldStart("bases");

                    foreach (TribeBase tribeBase in bases) {
                        generator.WriteStartObject();

                        generator.WriteField("name", tribeBase.Name);
                        generator.WriteField("x", tribeBase.X);
                        generator.WriteField("y", tribeBase.Y);
                        generator.WriteField("z", tribeBase.Z);
                        generator.WriteField("lat", context.MapData.CalculateLat(tribeBase.Y));
                        generator.WriteField("lon", context.MapData.CalculateLon(tribeBase.X));
                        generator.WriteField("radius", tribeBase.Size);
                        writeCreatures(tribeBase.Creatures);
                        writeStructures(tribeBase.Structures);
                        writeDroppedItems(tribeBase.DroppedItems);
                        if (itemsLong) {
                            generator.WritePropertyName("items");
                            Inventory.writeInventoryLong(generator, context, tribeBase.Items, writeAllFields);
                            generator.WritePropertyName("blueprints");
                            Inventory.writeInventoryLong(generator, context, tribeBase.Blueprints, writeAllFields);
                        } else {
                            generator.WritePropertyName("items");
                            Inventory.writeInventorySummary(generator, tribeBase.Items);
                            generator.WritePropertyName("blueprints");
                            Inventory.writeInventorySummary(generator, tribeBase.Blueprints);
                        }

                        generator.WriteEndObject();
                    }

                    generator.WriteStartObject();
                }

                writeCreatures(creatures);
                writeStructures(structures);
                writeDroppedItems(droppedItems);
                if (itemsLong) {
                    generator.WritePropertyName("items");
                    Inventory.writeInventoryLong(generator, context, items, writeAllFields);
                    generator.WritePropertyName("blueprints");
                    Inventory.writeInventoryLong(generator, context, blueprints, writeAllFields);
                } else {
                    generator.WritePropertyName("items");
                    Inventory.writeInventorySummary(generator, items);
                    generator.WritePropertyName("blueprints");
                    Inventory.writeInventorySummary(generator, blueprints);
                }

                if (withBases && bases != null) {
                    generator.WriteEndObject();

                    generator.WriteEndArray();
                }
            }

            foreach (string path in Directory.EnumerateFiles(saveDir).Where(path => tribePattern.IsMatch(path))) {
                Action task = () => {
                    try {
                        Tribe tribe = new Tribe(path, ReadingOptions.Create());

                        string tribeFileName = tribe.tribeId + ".json";

                        string tribePath = Path.Combine(outputDirectory, tribeFileName);

                        CommonFunctions.WriteJson(tribePath, (generator, writingOptions) => {
                            generator.WriteStartObject();

                            tribe.writeAllProperties(generator, context, writeAllFields);

                            mapWriter(generator, tribe.tribeId);

                            generator.WriteEndObject();
                        }, writingOptions);
                    } catch (Exception ex) {
                        Console.Error.WriteLine("Found potentially corrupt ArkTribe: " + path);
                        if (GlobalOptions.Verbose) {
                            Console.Error.WriteLine(ex.Message);
                            Console.Error.WriteLine(ex.StackTrace);
                        }
                    }
                };

                if (tasks != null) {
                    tasks.Add(task);
                } else {
                    task();
                }
            }

            if (tasks != null) {
                Parallel.ForEach(tasks, task => task());
            }

            if (tribeless) {
                string tribePath = Path.Combine(outputDirectory, "tribeless.json");

                CommonFunctions.WriteJson(tribePath, (generator, writingOptions) => {
                    generator.WriteStartObject();

                    mapWriter(generator, -1);

                    generator.WriteEndObject();
                }, writingOptions);
            }

            if (nonPlayers) {
                string tribePath = Path.Combine(outputDirectory, "non-players.json");

                CommonFunctions.WriteJson(tribePath, (generator, writingOptions) => {
                    generator.WriteStartObject();

                    mapWriter(generator, 0);

                    generator.WriteEndObject();
                }, writingOptions);
            }

            stopwatch.Stop("Loading tribes and writing info");
            stopwatch.Print();
        }
    }

    public class ClusterCommand : PlayersBaseCommand {
        private const string names = "cluster";
        private const string help = "Writes lists of all things which players have uploaded into the cluster.";
        private static readonly string[] helpHeaders = {
                "",
                "cluster CLUSTER_DIRECTORY OUTPUT_DIRECTORY [OPTIONS]",
                ""
        };

        private bool writeAllFields;

        public ClusterCommand() : base(names, help, helpHeaders) {
            Options.Add("write-all-fields", "Writes all the fields.", s => writeAllFields = s != null);
        }

        protected override void RunCommand(IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            string clusterDirectory = argsList[0];
            string outputDirectory = argsList[1];

            ArkDataManager.LoadData(GlobalOptions.Language);

            List<Action> tasks = GlobalOptions.Parallel ? new List<Action>() : null;

            Stopwatch stopwatch = new Stopwatch(GlobalOptions.UseStopWatch);
            foreach (string path in Directory.EnumerateFiles(clusterDirectory)) {
                Action task = () => {
                    try {
                        ArkCloudInventory cloudInventory = new ArkCloudInventory().ReadBinary<ArkCloudInventory>(path, ReadingOptions.Create());

                        CustomDataContext context = new CustomDataContext {
                                ObjectContainer = cloudInventory
                        };

                        IPropertyContainer arkData = cloudInventory.InventoryData.GetPropertyValue<IPropertyContainer>("MyArkData");

                        CommonFunctions.WriteJson(Path.Combine(outputDirectory, path + ".json"), (generator, writingOptions) => {
                            generator.WriteStartObject();

                            ArkArrayStruct tamedDinosData = arkData.GetPropertyValue<ArkArrayStruct>("ArkTamedDinosData");
                            if (tamedDinosData != null && tamedDinosData.Any()) {
                                generator.WriteArrayFieldStart("creatures");
                                foreach (IStruct dinoStruct in tamedDinosData) {
                                    IPropertyContainer dino = (IPropertyContainer)dinoStruct;
                                    ArkContainer container = null;
                                    if (cloudInventory.InventoryVersion == 1) {
                                        ArkArrayUInt8 byteData = dino.GetPropertyValue<ArkArrayUInt8>("DinoData");

                                        container = new ArkContainer(byteData);
                                    } else if (cloudInventory.InventoryVersion == 3) {
                                        ArkArrayInt8 byteData = dino.GetPropertyValue<ArkArrayInt8>("DinoData");

                                        container = new ArkContainer(byteData);
                                    }

                                    ObjectReference dinoClass = dino.GetPropertyValue<ObjectReference>("DinoClass");
                                    // Skip "BlueprintGeneratedClass " = 24 chars
                                    string dinoClassName = dinoClass.ObjectString.ToString().Substring(24);
                                    generator.WriteStartObject();

                                    generator.WriteField("type",
                                            ArkDataManager.HasCreatureByPath(dinoClassName) ? ArkDataManager.GetCreatureByPath(dinoClassName).Name : dinoClassName);

                                    // NPE for unknown versions
                                    Creature creature = new Creature(container.Objects[0], container);
                                    generator.WriteObjectFieldStart("data");
                                    creature.writeAllProperties(generator, context, writeAllFields);
                                    generator.WriteEndObject();

                                    generator.WriteEndObject();
                                }

                                generator.WriteEndArray();
                            }

                            ArkArrayStruct arkItems = arkData.GetPropertyValue<ArkArrayStruct>("ArkItems");
                            if (arkItems != null) {
                                List<Item> items = new List<Item>();
                                foreach (IStruct itemStruct in arkItems) {
                                    IPropertyContainer item = (IPropertyContainer)itemStruct;
                                    IPropertyContainer netItem = item.GetPropertyValue<IPropertyContainer>("ArkTributeItem");

                                    items.Add(new Item(netItem));
                                }

                                if (items.Any()) {
                                    generator.WritePropertyName("items");
                                    Inventory.writeInventoryLong(generator, context, items, writeAllFields);
                                }
                            }

                            generator.WriteEndObject();
                        }, writingOptions);
                    } catch (Exception ex) {
                        Console.Error.WriteLine("Found potentially corrupt cluster data: " + path);
                        if (GlobalOptions.Verbose) {
                            Console.Error.WriteLine(ex.Message);
                            Console.Error.WriteLine(ex.StackTrace);
                        }
                    }
                };

                if (tasks != null) {
                    tasks.Add(task);
                } else {
                    task();
                }
            }

            if (tasks != null) {
                Parallel.ForEach(tasks, task => task());
            }

            stopwatch.Stop("Loading cluster data and writing info");
            stopwatch.Print();
        }
    }

}
