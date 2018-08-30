using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SavegameToolkit;
using SavegameToolkit.Types;
using SavegameToolkitAdditions;

namespace ArkTools.Data {

    public class DataCollector : IDataContext {
        private static readonly Regex ProfilePattern = new Regex(@"(\d+|LocalPlayer)\.arkprofile");

        private static readonly Regex TribePattern = new Regex(@"\d+\.arktribe");

        private readonly Dictionary<ArkName, int> nameObjectMap = new Dictionary<ArkName, int>();

        public SortedDictionary<int, Item> ItemMap { get; } = new SortedDictionary<int, Item>();

        public SortedDictionary<int, DroppedItem> DroppedItemMap { get; } = new SortedDictionary<int, DroppedItem>();

        public SortedDictionary<int, Inventory> InventoryMap { get; } = new SortedDictionary<int, Inventory>();

        public SortedDictionary<int, Creature> CreatureMap { get; } = new SortedDictionary<int, Creature>();

        public SortedDictionary<int, Structure> StructureMap { get; } = new SortedDictionary<int, Structure>();

        public SortedDictionary<long, Player> PlayerMap { get; } = new SortedDictionary<long, Player>();

        public SortedDictionary<long, ClusterStorage> PlayerClusterMap { get; } = new SortedDictionary<long, ClusterStorage>();

        public SortedDictionary<int, Tribe> TribeMap { get; } = new SortedDictionary<int, Tribe>();

        public MapData MapData { get; private set; }

        public ArkSavegame Savegame { get; private set; }

        public GameObjectContainer ObjectContainer { get; private set; }

        public long MaxAge { get; }

        private readonly List<Action> tasks = null;

        public void LoadSavegame(string path) {
            Savegame = new ArkSavegame().ReadBinary<ArkSavegame>(path, ReadingOptions.Create()
                    // Skip things like NPCZoneVolume and non-instanced objects
                    .WithObjectFilter(obj => !obj.FromDataFile && (obj.Names.Count > 1 || obj.Names[0].Instance > 0))
                    .WithBuildComponentTree(true));
            MapData = LatLonCalculator.ForSave(Savegame);

            if (Savegame.HibernationEntries.Any()) {
                List<GameObject> combinedObjects = new List<GameObject>(Savegame.Objects);

                foreach (HibernationEntry entry in Savegame.HibernationEntries) {
                    ObjectCollector collector = new ObjectCollector(entry, 1);
                    combinedObjects.AddRange(collector.Remap(combinedObjects.Count));
                }

                ObjectContainer = new GameObjectContainer(combinedObjects);
            } else {
                ObjectContainer = Savegame;
            }

            foreach (GameObject gameObject in ObjectContainer) {
                if (gameObject.FromDataFile || (gameObject.Names.Count == 1 && gameObject.Names[0].Instance == 0)) {
                    // Skip things like NPCZoneVolume and non-instanced objects
                } else if (gameObject.IsInventory()) {
                    InventoryMap[gameObject.Id] = new Inventory(gameObject);
                } else {
                    if (!nameObjectMap.ContainsKey(gameObject.Names[0])) {
                        nameObjectMap[gameObject.Names[0]] = gameObject.Id;
                        if (gameObject.IsItem) {
                            ItemMap[gameObject.Id] = new Item(gameObject);
                        } else if (gameObject.IsCreature()) {
                            CreatureMap[gameObject.Id] = new Creature(gameObject, Savegame);
                        } else if (gameObject.Location != null &&
                                !gameObject.IsPlayer() &&
                                !gameObject.IsDroppedItem() &&
                                !gameObject.IsWeapon()) {
                            // Skip players, weapons and items on the ground
                            // is (probably) a structure
                            StructureMap[gameObject.Id] = new Structure(gameObject, Savegame);
                        } else if (gameObject.IsDroppedItem()) {
                            // dropped Item
                            DroppedItemMap[gameObject.Id] = new DroppedItem(gameObject, Savegame);
                        }
                    }
                }
            }
        }

        public void LoadPlayers(string path) {
            foreach (string profilePath in Directory.EnumerateFiles(path).Where(p => ProfilePattern.IsMatch(p))) {
                if (MaxAge != 0 && File.GetLastWriteTimeUtc(profilePath) < DateTime.UtcNow.AddSeconds(-MaxAge)) {
                    continue;
                }

                if (tasks != null) {
                    tasks.Add(() => loadPlayer(profilePath));
                } else {
                    loadPlayer(profilePath);
                }
            }
        }

        private void loadPlayer(string profilePath) {
            try {
                Player player = new Player(profilePath, this, ReadingOptions.Create());
                PlayerMap[player.playerDataId] = player;
            } catch (IOException ex) {
                if (GlobalOptions.Verbose) {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                }
            } catch (Exception ex) {
                Console.Error.WriteLine("Found potentially corrupt ArkProfile: " + profilePath);
                if (GlobalOptions.Verbose) {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                }
            }
        }

        public void LoadTribes(string path) {
            foreach (string tribePath in Directory.EnumerateFiles(path).Where(p => TribePattern.IsMatch(p))) {
                if (MaxAge != 0 && File.GetLastWriteTimeUtc(tribePath) < DateTime.UtcNow.AddSeconds(-MaxAge)) {
                    continue;
                }

                if (tasks != null) {
                    tasks.Add(() => loadTribe(tribePath));
                } else {
                    loadTribe(tribePath);
                }
            }
        }

        private void loadTribe(string tribePath) {
            try {
                Tribe tribe = new Tribe(tribePath, ReadingOptions.Create());
                TribeMap[tribe.tribeId] = tribe;
            } catch (IOException ex) {
                if (GlobalOptions.Verbose) {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                }
            } catch (Exception ex) {
                Console.Error.WriteLine("Found potentially corrupt ArkTribe: " + tribePath);
                if (GlobalOptions.Verbose) {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                }
            }
        }

        public void loadCluster(string path) { }

        public void waitForData() {
            if (tasks != null) {
                Parallel.ForEach(tasks, action => action());
                tasks.Clear();
            }
        }
    }

}
