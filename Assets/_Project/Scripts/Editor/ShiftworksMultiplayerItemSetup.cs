using System.IO;
using Shiftworks.Conveyors;
using Shiftworks.Items;
using Shiftworks.Multiplayer;
using Shiftworks.Orders;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// (Stage 11C-1) Idempotent editor tooling for host-authoritative item pickup/carry/
    /// drop/throw: adds NetworkObject + NetworkTransform (Server authority — the host's own
    /// physics simulation is the only one that ever runs) + NetworkRigidbody (automatically
    /// makes every non-authoritative copy's Rigidbody kinematic, so a client's own local
    /// physics can never fight the replicated position) + NetworkCarryableItem (mirrors
    /// held/automated state so "E - Al" correctly disables for an item someone else already
    /// holds) to the four purchasable/produced item prefabs AND to the four scene-baked
    /// TestBox pickup-test objects already in WorkshopMultiplayer.unity, reusing the SAME
    /// NetworkPrefabsList asset (and NetworkManager reference to it) Stage 11B's economy
    /// setup already created — no second list, no second NetworkManager wiring step needed.
    /// Also adds the single NetworkItemInteractionBridge to the scene. Never touches
    /// WorkshopPrototype.unity, the item prefabs' own single-player behavior, or
    /// MultiplayerPrototype.unity.
    /// </summary>
    public static class ShiftworksMultiplayerItemSetup
    {
        private static readonly string[] ItemPrefabPaths =
        {
            "Assets/_Project/Prefabs/Items/MetalSheet.prefab",
            "Assets/_Project/Prefabs/Items/CutMetalPart.prefab",
            "Assets/_Project/Prefabs/Items/AssembledComponent.prefab",
            "Assets/_Project/Prefabs/Items/ShipmentPackage.prefab",
        };

        private static readonly string[] SceneTestBoxNames = { "TestBox_1", "TestBox_2", "TestBox_3", "TestBox_4" };

        private const string NetworkDataFolder = "Assets/_Project/Data/Multiplayer";
        private const string NetworkPrefabsListPath = NetworkDataFolder + "/MultiplayerNetworkPrefabs.asset";
        private const string WorkshopMultiplayerScenePath = "Assets/_Project/Scenes/WorkshopMultiplayer.unity";

        [MenuItem("Shiftworks/Setup/34. Add NetworkObject To Item Prefabs")]
        public static void AddNetworkingToItemPrefabsMenuItem() => AddNetworkingToItemPrefabs();

        [MenuItem("Shiftworks/Setup/35. Add Item Interaction To WorkshopMultiplayer Scene")]
        public static void AddItemInteractionToSceneMenuItem() => AddItemInteractionToScene();

        [MenuItem("Shiftworks/Setup/36. Add Output Storage Sync To WorkshopMultiplayer Scene")]
        public static void AddOutputStorageSyncMenuItem() => AddOutputStorageSync();

        [MenuItem("Shiftworks/Setup/39. Add Packing And Shipping Sync To WorkshopMultiplayer Scene")]
        public static void AddPackingAndShippingSyncMenuItem() => AddPackingAndShippingSync();

        [MenuItem("Shiftworks/Setup/Apply Multiplayer Item Interaction (Prefabs + Scene)")]
        public static void ApplyMultiplayerItemInteraction()
        {
            AddNetworkingToItemPrefabs();
            AddItemInteractionToScene();
            AddOutputStorageSync();
            AddPackingAndShippingSync();
        }

        private static void AddNetworkingToItemPrefabs()
        {
            NetworkPrefabsList prefabsList = GetOrCreateNetworkPrefabsList();

            foreach (string prefabPath in ItemPrefabPaths)
            {
                GameObject prefabAsset = AddNetworkComponentsToPrefab(prefabPath);
                if (prefabAsset == null)
                {
                    continue;
                }

                if (!prefabsList.Contains(prefabAsset))
                {
                    prefabsList.Add(new NetworkPrefab { Prefab = prefabAsset });
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Shiftworks] Eşya prefab'larına ağ bileşenleri eklendi ve NetworkPrefabsList'e kaydedildi.");
        }

        private static GameObject AddNetworkComponentsToPrefab(string prefabPath)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError("[Shiftworks] Eşya prefabı bulunamadı: " + prefabPath);
                return null;
            }

            if (prefabAsset.GetComponent<NetworkObject>() != null)
            {
                return prefabAsset;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                ApplyNetworkComponents(root);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[Shiftworks] Ağ bileşenleri eklendi: " + prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        /// <summary>
        /// Shared by both the prefab path and the scene-baked TestBox path. Order matters:
        /// NetworkObject first, then NetworkTransform with explicit Server authority (items
        /// never change owner — the host's physics is always the one simulating), then
        /// NetworkRigidbody (its own [RequireComponent(NetworkTransform)] is already
        /// satisfied at this point so it does not create a second one), then
        /// NetworkCarryableItem wired to the sibling CarryableItem.
        /// </summary>
        private static void ApplyNetworkComponents(GameObject root)
        {
            if (root.GetComponent<NetworkObject>() != null)
            {
                return;
            }

            root.AddComponent<NetworkObject>();

            NetworkTransform networkTransform = root.AddComponent<NetworkTransform>();
            networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Server;

            NetworkRigidbody networkRigidbody = root.AddComponent<NetworkRigidbody>();
            networkRigidbody.UseRigidBodyForMotion = true;

            CarryableItem carryableItem = root.GetComponent<CarryableItem>();
            NetworkCarryableItem networkCarryableItem = root.AddComponent<NetworkCarryableItem>();
            SerializedObject so = new SerializedObject(networkCarryableItem);
            so.FindProperty("carryableItem").objectReferenceValue = carryableItem;
            so.ApplyModifiedProperties();
        }

        private static void AddItemInteractionToScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(WorkshopMultiplayerScenePath) == null)
            {
                Debug.LogError("[Shiftworks] WorkshopMultiplayer sahnesi bulunamadı (önce '29'/'30' çalıştırılmış olmalı).");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(WorkshopMultiplayerScenePath, OpenSceneMode.Single);

            foreach (string boxName in SceneTestBoxNames)
            {
                GameObject boxGo = GameObject.Find(boxName);
                if (boxGo == null)
                {
                    Debug.LogWarning($"[Shiftworks] '{boxName}' sahnede bulunamadı, atlandı.");
                    continue;
                }

                ApplyNetworkComponents(boxGo);
            }

            GameObject bridgeGo = GameObject.Find("NetworkItemInteractionBridge");
            if (bridgeGo == null)
            {
                bridgeGo = new GameObject("NetworkItemInteractionBridge");
                bridgeGo.AddComponent<NetworkObject>();
                bridgeGo.AddComponent<NetworkItemInteractionBridge>();
                Debug.Log("[Shiftworks] NetworkItemInteractionBridge sahneye eklendi.");
            }
            else
            {
                Debug.Log("[Shiftworks] NetworkItemInteractionBridge zaten sahnede var, dokunulmadı.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesine eşya etkileşim sistemi eklendi.");
        }

        /// <summary>
        /// (Stage 11C bugfix) Adds NetworkObject + NetworkOutputStorage to the scene's one
        /// OutputStorage, then (re-)wires NetworkItemInteractionBridge's outputStorage field
        /// — re-run safe: if the bridge already exists from menu 35, this just retrofits the
        /// new reference onto it, same "re-wire unconditionally" approach Stage 11B's
        /// economy bridge menu uses for its own newly-added fields.
        /// </summary>
        private static void AddOutputStorageSync()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(WorkshopMultiplayerScenePath) == null)
            {
                Debug.LogError("[Shiftworks] WorkshopMultiplayer sahnesi bulunamadı (önce '29'/'30' çalıştırılmış olmalı).");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(WorkshopMultiplayerScenePath, OpenSceneMode.Single);

            GameObject storageGo = GameObject.Find("OutputStorage");
            if (storageGo == null)
            {
                Debug.LogError("[Shiftworks] 'OutputStorage' sahnede bulunamadı.");
                return;
            }

            OutputStorage storage = storageGo.GetComponent<OutputStorage>();
            NetworkOutputStorage networkStorage = storageGo.GetComponent<NetworkOutputStorage>();
            if (networkStorage == null)
            {
                storageGo.AddComponent<NetworkObject>();
                networkStorage = storageGo.AddComponent<NetworkOutputStorage>();
                SerializedObject storageSo = new SerializedObject(networkStorage);
                storageSo.FindProperty("storage").objectReferenceValue = storage;
                storageSo.ApplyModifiedProperties();
                Debug.Log("[Shiftworks] OutputStorage'a ağ senkronizasyonu eklendi.");
            }
            else
            {
                Debug.Log("[Shiftworks] OutputStorage'da ağ senkronizasyonu zaten var, dokunulmadı.");
            }

            GameObject bridgeGo = GameObject.Find("NetworkItemInteractionBridge");
            if (bridgeGo == null)
            {
                Debug.LogError("[Shiftworks] 'NetworkItemInteractionBridge' sahnede bulunamadı (önce '35. Add Item Interaction To WorkshopMultiplayer Scene' çalıştırılmış olmalı).");
                return;
            }

            NetworkItemInteractionBridge bridge = bridgeGo.GetComponent<NetworkItemInteractionBridge>();
            SerializedObject bridgeSo = new SerializedObject(bridge);
            bridgeSo.FindProperty("outputStorage").objectReferenceValue = storage;
            bridgeSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesine depo stok senkronizasyonu eklendi.");
        }

        /// <summary>
        /// (Stage 11C extension) Wires the scene's one PackingStation/ShippingZone into
        /// NetworkItemInteractionBridge — same "(re-)wire unconditionally" retrofit approach
        /// as AddOutputStorageSync's outputStorage field. PackingStation/ShippingZone
        /// themselves need no new components (they are not NetworkObjects — there is only
        /// ever one of each, resolved by direct reference like OutputStorage); the item a
        /// player carries into them is already networked by menu 34, and the resulting
        /// ShipmentPackage is spawned by the bridge itself (see RequestPack).
        /// </summary>
        private static void AddPackingAndShippingSync()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(WorkshopMultiplayerScenePath) == null)
            {
                Debug.LogError("[Shiftworks] WorkshopMultiplayer sahnesi bulunamadı (önce '29'/'30' çalıştırılmış olmalı).");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(WorkshopMultiplayerScenePath, OpenSceneMode.Single);

            PackingStation packingStation = Object.FindFirstObjectByType<PackingStation>();
            ShippingZone shippingZone = Object.FindFirstObjectByType<ShippingZone>();
            if (packingStation == null || shippingZone == null)
            {
                Debug.LogError("[Shiftworks] PackingStation/ShippingZone sahnede bulunamadı.");
                return;
            }

            GameObject bridgeGo = GameObject.Find("NetworkItemInteractionBridge");
            if (bridgeGo == null)
            {
                Debug.LogError("[Shiftworks] 'NetworkItemInteractionBridge' sahnede bulunamadı (önce '35. Add Item Interaction To WorkshopMultiplayer Scene' çalıştırılmış olmalı).");
                return;
            }

            NetworkItemInteractionBridge bridge = bridgeGo.GetComponent<NetworkItemInteractionBridge>();
            SerializedObject bridgeSo = new SerializedObject(bridge);
            bridgeSo.FindProperty("packingStation").objectReferenceValue = packingStation;
            bridgeSo.FindProperty("shippingZone").objectReferenceValue = shippingZone;
            bridgeSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesine paketleme/sevkiyat senkronizasyonu eklendi.");
        }

        private static NetworkPrefabsList GetOrCreateNetworkPrefabsList()
        {
            NetworkPrefabsList existing = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkPrefabsListPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(NetworkDataFolder);
            NetworkPrefabsList list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
            AssetDatabase.CreateAsset(list, NetworkPrefabsListPath);
            return list;
        }
    }
}
