using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SavegameToolkit;
using SavegameToolkit.Arrays;
using SavegameToolkit.Propertys;
using SavegameToolkit.Structs;
using SavegameToolkit.Types;

namespace ArkTools {

    public class ObjectCollector : IEnumerable<GameObject> {
        private readonly Dictionary<int, GameObject> mappedObjects = new Dictionary<int, GameObject>();

        private readonly int startIndex;

        private int insertIndex;

        public int Added { get; private set; }

        public int Deleted { get; private set; }

        public bool Debug { get; set; }

        public ObjectCollector(GameObjectContainer container, GameObject gameObject, bool followReferences, bool withComponents) {
            Stack<IPropertyContainer> toVisit = new Stack<IPropertyContainer>();
            startIndex = 0;

            mappedObjects[gameObject.Id] = gameObject;
            toVisit.Push(gameObject);

            visit(toVisit, container, followReferences, withComponents);

            insertIndex = mappedObjects.Any() ? mappedObjects.Keys.Max() : 0;
        }

        public ObjectCollector(GameObjectContainer container, ArkName className, bool followReferences, bool withComponents) {
            Stack<IPropertyContainer> toVisit = new Stack<IPropertyContainer>();
            startIndex = 0;

            foreach (GameObject gameObject in container) {
                if (gameObject.ClassName == className) {
                    mappedObjects[gameObject.Id] = gameObject;
                    toVisit.Push(gameObject);
                }
            }

            visit(toVisit, container, followReferences, withComponents);

            insertIndex = mappedObjects.Any() ? mappedObjects.Keys.Max() : 0;
        }

        public ObjectCollector(GameObjectContainer container, int startIndex = 0) {
            this.startIndex = startIndex;
            foreach (GameObject obj in container) {
                mappedObjects[obj.Id + startIndex] = obj;
            }
            insertIndex = mappedObjects.Count + startIndex;
        }

        private void visit(Stack<IPropertyContainer> toVisit, GameObjectContainer container, bool followReferences, bool withComponents) {
            while (toVisit.Any()) {
                IPropertyContainer currentInstance = toVisit.Pop();

                if (followReferences) {
                    foreach (IProperty property in currentInstance.Properties) {
                        if (property is PropertyObject po) {
                            ObjectReference reference = po.Value;
                            GameObject referenced = reference.GetObject(container);
                            if (referenced != null && !mappedObjects.ContainsKey(referenced.Id)) {
                                mappedObjects[referenced.Id] = referenced;
                                toVisit.Push(referenced);
                            }
                        } else if (property is PropertyArray pa) {
                            IArkArray<IStruct> structList = pa.GetTypedValue<IStruct>();
                            IArkArray<ObjectReference> objectReferenceList = pa.GetTypedValue<ObjectReference>();
                            if (structList != null) {
                                foreach (IStruct aStruct in structList) {
                                    if (aStruct is IPropertyContainer propertyContainer) {
                                        toVisit.Push(propertyContainer);
                                    }
                                }
                            } else if (objectReferenceList != null) {
                                foreach (ObjectReference reference in objectReferenceList) {
                                    GameObject referenced = reference.GetObject(container);
                                    if (referenced != null && !mappedObjects.ContainsKey(referenced.Id)) {
                                        mappedObjects[referenced.Id] = referenced;
                                        toVisit.Push(referenced);
                                    }
                                }
                            }
                        } else if (property is PropertyStruct ps) {
                            IStruct aStruct = ps.Value;
                            if (aStruct is IPropertyContainer propertyContainer) {
                                toVisit.Push(propertyContainer);
                            }
                        }
                    }
                }

                if (withComponents && currentInstance is GameObject) {
                    GameObject gameObject = (GameObject)currentInstance;

                    foreach (GameObject component in gameObject.Components.Values) {
                        if (!mappedObjects.ContainsKey(component.Id)) {
                            mappedObjects[component.Id] = component;
                            toVisit.Push(component);
                        }
                    }
                }
            }
        }

        public Dictionary<int, GameObject> getMappedObjects() {
            return new Dictionary<int, GameObject>(mappedObjects);
        }

        public GameObject this[int index] => mappedObjects[index];

        public GameObject this[ObjectReference reference] =>
                reference.IsId && reference.ObjectId >= startIndex ? mappedObjects[reference.ObjectId] : null;

        public bool Has(int index) {
            return mappedObjects.ContainsKey(index);
        }

        public bool Has(ObjectReference reference) {
            return reference.IsId && reference.ObjectId >= startIndex && mappedObjects[reference.ObjectId] != null;
        }

        public void Remove(int index) {
            mappedObjects.Remove(index, out GameObject removed);
            if (removed != null) {
                Deleted++;
                if (Debug) {
                    Console.Out.WriteLine("Removed " + removed.Names);
                }
            }
        }

        public void Remove(GameObject gameObject) {
            mappedObjects.Remove(gameObject.Id, out GameObject removed);
            if (removed != null) {
                Deleted++;
                if (Debug) {
                    Console.Out.WriteLine("Removed " + removed.Names);
                }
            }
        }

        public void Remove(ObjectReference reference) {
            if (reference.IsId && reference.ObjectId >= startIndex) {
                mappedObjects.Remove(reference.ObjectId, out GameObject removed);
                if (removed != null) {
                    Deleted++;
                    if (Debug) {
                        Console.Out.WriteLine("Removed " + removed.Names);
                    }
                }
            }
        }

        public int Add(GameObject gameObject) {
            gameObject.Id = insertIndex;
            mappedObjects[insertIndex] = gameObject;

            Added++;

            if (Debug) {
                Console.Out.WriteLine("Added " + gameObject.Names);
            }

            return insertIndex++;
        }

        public IEnumerable<GameObject> Remap(int startId) {
            List<GameObject> remappedList = new List<GameObject>(mappedObjects.Count);

            applyOrderRules(remappedList);

            for (int i = 0; i < remappedList.Count; i++) {
                remappedList[i].Id = startId + i;
            }

            foreach (var gameObject in remappedList)
                doRemap(gameObject);

            return remappedList;
        }

        /// <summary>
        /// Insert objects in the right order, ensures that components will be written after their owners
        /// </summary>
        /// <param name="remappedList"></param>
        private void applyOrderRules(List<GameObject> remappedList) {
            //Dictionary<int, Dictionary<List<ArkName>, GameObject>> objectMap = new Dictionary<int, Dictionary<List<ArkName>, GameObject>>();
            Dictionary<int, Dictionary<int, GameObject>> objectMap = new Dictionary<int, Dictionary<int, GameObject>>();

            // First step: clear all component information and collect names
            foreach (GameObject gameObject in mappedObjects.Values) {
                gameObject.Components.Clear();
                gameObject.Parent = null;

                int mapKey = gameObject.FromDataFile ? gameObject.DataFileIndex : -1;

                //objectMap.computeIfAbsent(mapKey, key => new HashMap<>()).putIfAbsent(gameObject.Names, gameObject);
                if (!objectMap.ContainsKey(mapKey))
                    objectMap[mapKey] = new Dictionary<int, GameObject>();
                if (!objectMap[mapKey].ContainsKey(gameObject.Names.HashCode())) {
                    objectMap[mapKey][gameObject.Names.HashCode()] = gameObject;
                }
            }

            // Second step: rebuild component information
            foreach (GameObject gameObject in mappedObjects.Values) {
                Dictionary<int, GameObject> map = objectMap[gameObject.FromDataFile ? gameObject.DataFileIndex : -1];
                if (gameObject.HasParentNames&& map != null) {
                    IEnumerable<ArkName> targetName = gameObject.ParentNames;

                    GameObject parent = map[targetName.HashCode()];
                    if (parent != null) {
                        parent.AddComponent(gameObject);
                        gameObject.Parent = parent;
                    }
                }
            }

            // Third step: build list by adding all objects without parent + their components
            List<GameObject> toVisit = new List<GameObject>();
            foreach (GameObject gameObject in mappedObjects.Values) {
                if (gameObject.Parent != null) {
                    continue;
                }

                remappedList.Add(gameObject);

                toVisit.AddRange(gameObject.Components.Values);

                while (toVisit.Any()) {
                    GameObject current = toVisit[0];
                    toVisit.RemoveAt(0);

                    remappedList.Add(current);

                    foreach (GameObject o in current.Components.Values) {
                        toVisit.Insert(0, o);
                    }
                }
            }
        }

        /// <summary>
        /// Refresh ObjectReferences, throws NPE for broken ObjectReferences.
        ///
        /// An ObjectReference is considered broken if it's object has been deleted. 
        /// </summary>
        /// <param name="instance"></param>
        private void doRemap(GameObject instance) {
            Stack<IPropertyContainer> toVisit = new Stack<IPropertyContainer>();
            toVisit.Push(instance);

            while (toVisit.Any()) {
                IPropertyContainer currentInstance = toVisit.Pop();
                foreach (IProperty property in currentInstance.Properties) {
                    switch (property) {
                        case PropertyObject po: {
                            ObjectReference reference = po.Value;
                            if (reference.IsId && reference.ObjectId >= startIndex) {
                                reference.ObjectId = mappedObjects[reference.ObjectId].Id;
                            }
                            break;
                        }
                        case PropertyArray pa: {
                            IArkArray<IStruct> structList = pa.GetTypedValue<IStruct>();
                            IArkArray<ObjectReference> objectReferenceList = pa.GetTypedValue<ObjectReference>();
                            if (structList != null) {
                                foreach (IStruct aStruct in structList) {
                                    if (aStruct is IPropertyContainer container) {
                                        toVisit.Push(container);
                                    }
                                }
                            } else if (objectReferenceList != null) {
                                foreach (ObjectReference reference in objectReferenceList) {
                                    if (reference.IsId && reference.ObjectId >= startIndex) {
                                        reference.ObjectId = mappedObjects[reference.ObjectId].Id;
                                    }
                                }
                            }
                            break;
                        }
                        case PropertyStruct ps: {
                            IStruct aStruct = ps.Value;
                            if (aStruct is IPropertyContainer container) {
                                toVisit.Push(container);
                            }
                            break;
                        }
                    }
                }
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<GameObject> GetEnumerator() {
            return mappedObjects.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }

}
