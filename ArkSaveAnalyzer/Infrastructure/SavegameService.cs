using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ArkSaveAnalyzer.Infrastructure.Messages;
using ArkSaveAnalyzer.Properties;
using GalaSoft.MvvmLight.Messaging;
using SavegameToolkit;

namespace ArkSaveAnalyzer.Infrastructure {
    public class SavegameService {

        #region file system watch

        private static readonly SavegameService instance = new SavegameService();

        private readonly FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
        private readonly DispatcherTimer reloadDelay;

        private SavegameService() {
            reloadDelay = new DispatcherTimer {
                    Interval = TimeSpan.FromSeconds(2)
            };
            reloadDelay.Tick += (o, args) => {
                reloadDelay.Stop();
                Messenger.Default.Send(new FileSystemWatchChangedMessage(MapData.GetMapName(Path.GetFileNameWithoutExtension((string)reloadDelay.Tag))));
            };

            fileSystemWatcher.Changed += (o, args) => fileChanged(args.Name);
            fileSystemWatcher.Renamed += (o, args) => fileChanged(args.Name);

            Messenger.Default.Register<FileSystemWatchMessage>(this, message => {
                fileSystemWatcher.EnableRaisingEvents = false;
                if (string.IsNullOrEmpty(message.MapName)) {
                    return;
                }

                string mapFilename = buildMapFilename(message.MapName);
                fileSystemWatcher.Path = Path.GetDirectoryName(mapFilename);
                fileSystemWatcher.Filter = "*" + Path.GetExtension(mapFilename);
                fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                fileSystemWatcher.EnableRaisingEvents = true;
            });
        }

        /// <summary>
        /// Delay to prevent multiple calls at once
        /// </summary>
        /// <param name="filename"></param>
        private void fileChanged(string filename) {
            reloadDelay.Stop();
            reloadDelay.Tag = filename;
            reloadDelay.Start();
        }

        #endregion

        #region GameObjects

        // mapName => SaveJsonObjects
        private static readonly Dictionary<string, GameObjectContainer> savedMaps = new Dictionary<string, GameObjectContainer>();

        private static string workingDir => string.IsNullOrWhiteSpace(Settings.Default.WorkingDirectory) ? Path.GetTempPath() : Settings.Default.WorkingDirectory;

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public static async Task<GameObject> GetGameObject(string mapName, int id) {
            GameObjectContainer gameObjects = await GetGameObjects(mapName);

            return gameObjects[id];
        }

        public static async Task<GameObjectContainer> GetGameObjects(string mapName, bool getCached = true) {
            // copy and analyze only once at a time
            await semaphore.WaitAsync();
            try {
                if (!getCached || !savedMaps.ContainsKey(mapName)) {
                    savedMaps[mapName] = await getMapObjects(mapName);
                }
            } finally {
                semaphore.Release();
            }
            return savedMaps[mapName];
        }

        public static Task<GameObjectContainer> GetGameObjectsForFile(string filename) {
            return readSavegameFile(filename);
        }

        #endregion

        private static Task<GameObjectContainer> getMapObjects(string mapName) {
            File.Copy(buildMapFilename(mapName), $@"{workingDir}\{mapName}.ark", true);

            return readSavegameMap(mapName);
        }

        private static string buildMapFilename(string mapName) {
            MapData mapData = MapData.For(mapName);
            return $@"{Settings.Default.ArkSavedDirectory}\{mapData.Directory}SavedArksLocal\{mapData.Name}.ark";
        }

        private static Task<GameObjectContainer> readSavegameMap(string mapName) {
            string fileName = Path.Combine(workingDir, mapName + ".ark");
            return readSavegameFile(fileName);
        }

        private static Task<GameObjectContainer> readSavegameFile(string fileName) {
            return Task.Run(() => {
                if (new FileInfo(fileName).Length > int.MaxValue) {
                    throw new Exception("Input file is too large.");
                }

                Stream stream = new MemoryStream(File.ReadAllBytes(fileName));

                ArkSavegame arkSavegame = new ArkSavegame();

                using (ArkArchive archive = new ArkArchive(stream)) {
                    arkSavegame.ReadBinary(archive, ReadingOptions.Create()
                        .WithDataFiles(false)
                        .WithEmbeddedData(false)
                        .WithDataFilesObjectMap(false)
                        .WithObjectFilter(o => !o.IsItem && (o.Parent != null || o.Components.Any()))
                        .WithBuildComponentTree(true));
                }

                if (!arkSavegame.HibernationEntries.Any()) {
                    return arkSavegame;
                }

                List<GameObject> combinedObjects = arkSavegame.Objects;

                foreach (HibernationEntry entry in arkSavegame.HibernationEntries) {
                    ObjectCollector collector = new ObjectCollector(entry, 1);
                    combinedObjects.AddRange(collector.Remap(combinedObjects.Count));
                }

                return new GameObjectContainer(combinedObjects);
            });
        }
    }
}
