using System.IO;
using Shiftworks.Economy;
using Shiftworks.Factory;
using Shiftworks.Machines;
using Shiftworks.Multiplayer;
using Shiftworks.Orders;
using Shiftworks.Player;
using Shiftworks.Power;
using Shiftworks.SaveSystem;
using Shiftworks.UI;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// (Stage 11B, extended) Idempotent editor tooling for host-authoritative economy +
    /// machine purchase/placement + factory expansion + power upgrade + active-order sync:
    /// adds PlayerInteractor + MachinePlacementController (self-contained, built the exact
    /// same way ShiftworksInteractionSetup/ShiftworksMachineShopSetup already build them on
    /// the single-player Player prefab) to the shared NetworkPlayer.prefab, adds
    /// NetworkObject to the two purchasable machine prefabs so a host-spawned purchase is
    /// visible to every client, and wires (or, on a re-run, retrofits newly-added references
    /// onto) a single NetworkEconomyBridge into WorkshopMultiplayer.unity covering all five
    /// systems. Never touches WorkshopPrototype.unity, Player.prefab, or
    /// MultiplayerPrototype.unity.
    /// </summary>
    public static class ShiftworksMultiplayerEconomySetup
    {
        private const string NetworkPlayerPrefabPath = "Assets/_Project/Prefabs/Multiplayer/NetworkPlayer.prefab";
        private const string CuttingMachinePrefabPath = "Assets/_Project/Prefabs/Machines/CuttingMachine.prefab";
        private const string AssemblyMachinePrefabPath = "Assets/_Project/Prefabs/Machines/AssemblyMachine.prefab";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string NetworkDataFolder = "Assets/_Project/Data/Multiplayer";
        private const string NetworkPrefabsListPath = NetworkDataFolder + "/MultiplayerNetworkPrefabs.asset";
        private const string WorkshopMultiplayerScenePath = "Assets/_Project/Scenes/WorkshopMultiplayer.unity";

        [MenuItem("Shiftworks/Setup/31. Add Interaction To NetworkPlayer Prefab")]
        public static void AddInteractionToNetworkPlayerMenuItem() => AddInteractionToNetworkPlayerPrefab();

        [MenuItem("Shiftworks/Setup/32. Add Network Economy Bridge To WorkshopMultiplayer Scene")]
        public static void AddNetworkEconomyBridgeMenuItem() => AddNetworkEconomyBridgeToScene();

        [MenuItem("Shiftworks/Setup/37. Add NetworkMachineController To Machine Prefabs")]
        public static void AddNetworkMachineControllerMenuItem() => AddNetworkMachineControllerToMachinePrefabs();

        [MenuItem("Shiftworks/Setup/38. Add NetworkMachineController To Scene CuttingMachine")]
        public static void AddNetworkMachineControllerToSceneMenuItem() => AddNetworkMachineControllerToSceneCuttingMachine();

        [MenuItem("Shiftworks/Setup/Apply Multiplayer Economy (Prefabs + Bridge)")]
        public static void ApplyMultiplayerEconomy()
        {
            AddInteractionToNetworkPlayerPrefab();
            AddNetworkEconomyBridgeToScene();
            AddNetworkMachineControllerToMachinePrefabs();
            AddNetworkMachineControllerToSceneCuttingMachine();
        }

        /// <summary>
        /// (Stage 11C bugfix) CuttingMachine.prefab/AssemblyMachine.prefab already got a
        /// NetworkObject in Stage 11B (for purchase/placement spawning) — this adds the
        /// sibling NetworkMachineController that mirrors the running production cycle
        /// (State/BufferedInputCount/ProcessingElapsed) so a second player can see and feed
        /// a machine the first player already started, instead of each client simulating
        /// the same physical machine independently and disagreeing about its state.
        /// </summary>
        private static void AddNetworkMachineControllerToMachinePrefabs()
        {
            AddNetworkMachineControllerToPrefab(CuttingMachinePrefabPath);
            AddNetworkMachineControllerToPrefab(AssemblyMachinePrefabPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Shiftworks] Makine prefab'larına üretim durumu senkronizasyonu eklendi.");
        }

        /// <summary>
        /// (Stage 11C bugfix) The scene's one pre-placed CuttingMachine (built directly in
        /// WorkshopMultiplayer.unity, not as a live prefab instance — m_PrefabInstance is
        /// {fileID: 0} in the scene's own YAML, confirmed by reading it directly) never
        /// received the NetworkObject/NetworkMachineController that
        /// AddNetworkMachineControllerToMachinePrefabs adds to the two PURCHASABLE machine
        /// PREFAB ASSETS — a prefab-asset change only propagates to scene objects that are
        /// still connected prefab instances, and this one never was. Every item this
        /// CuttingMachine ever produced was therefore never NetworkObject.Spawn'd (nothing
        /// else in its component set ever called Spawn), which made RequestPickup AND
        /// RequestFeedMachine in NetworkItemInteractionBridge both treat it as
        /// "not a networked item yet" and silently fall back to a pure single-player call —
        /// run entirely locally on whichever client happened to press the key, bypassing the
        /// host even when the TARGET machine (e.g. a purchased AssemblyMachine) was itself
        /// fully networked. This is the confirmed root cause of the reported CutMetalPart
        /// 1/2-vs-0/2 host/client mismatch. Adds the two components directly onto the scene
        /// GameObject by name (same approach as AddOutputStorageSync's OutputStorage) instead
        /// of relying on prefab propagation.
        /// </summary>
        private const string SceneCuttingMachineName = "CuttingMachine";

        private static void AddNetworkMachineControllerToSceneCuttingMachine()
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

            GameObject cuttingMachineGo = GameObject.Find(SceneCuttingMachineName);
            if (cuttingMachineGo == null)
            {
                Debug.LogError("[Shiftworks] '" + SceneCuttingMachineName + "' sahnede bulunamadı.");
                return;
            }

            MachineController machine = cuttingMachineGo.GetComponent<MachineController>();
            NetworkMachineController networkMachine = cuttingMachineGo.GetComponent<NetworkMachineController>();
            if (networkMachine == null)
            {
                if (cuttingMachineGo.GetComponent<NetworkObject>() == null)
                {
                    cuttingMachineGo.AddComponent<NetworkObject>();
                }

                networkMachine = cuttingMachineGo.AddComponent<NetworkMachineController>();
                SerializedObject so = new SerializedObject(networkMachine);
                so.FindProperty("machine").objectReferenceValue = machine;
                so.ApplyModifiedProperties();
                Debug.Log("[Shiftworks] Sahnedeki '" + SceneCuttingMachineName + "' makinesine host-authoritative üretim senkronizasyonu eklendi.");
            }
            else
            {
                Debug.Log("[Shiftworks] Sahnedeki '" + SceneCuttingMachineName + "' makinesinde ağ senkronizasyonu zaten var, dokunulmadı.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesindeki önceden yerleştirilmiş makine artık ağ üzerinden senkronize.");
        }

        private static void AddNetworkMachineControllerToPrefab(string prefabPath)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError("[Shiftworks] Makine prefabı bulunamadı: " + prefabPath);
                return;
            }

            if (prefabAsset.GetComponent<NetworkMachineController>() != null)
            {
                Debug.Log("[Shiftworks] '" + prefabPath + "' zaten NetworkMachineController içeriyor, dokunulmadı.");
                return;
            }

            if (prefabAsset.GetComponent<NetworkObject>() == null)
            {
                Debug.LogError("[Shiftworks] '" + prefabPath + "' henüz NetworkObject içermiyor " +
                    "(önce '32. Add Network Economy Bridge To WorkshopMultiplayer Scene' çalıştırılmış olmalı).");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                MachineController machine = root.GetComponent<MachineController>();
                NetworkMachineController networkMachine = root.AddComponent<NetworkMachineController>();
                SerializedObject so = new SerializedObject(networkMachine);
                so.FindProperty("machine").objectReferenceValue = machine;
                so.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[Shiftworks] NetworkMachineController eklendi: " + prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// Adds PlayerInteractor + MachinePlacementController to NetworkPlayer.prefab, built
        /// self-contained exactly like Player.prefab's own copies (own HoldPoint child under
        /// its own PlayerCamera, own InteractionPromptCanvas) — no scene reference is needed
        /// on the prefab itself, so no runtime wiring step had to be added to
        /// NetworkPlayerSetup beyond the enabled/disabled toggle it already grew. Both start
        /// disabled (same reasoning as FirstPersonController already being disabled by
        /// default here): NetworkPlayerSetup.OnNetworkSpawn enables them for the owner only.
        /// </summary>
        private static void AddInteractionToNetworkPlayerPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(NetworkPlayerPrefabPath) == null)
            {
                Debug.LogError("[Shiftworks] NetworkPlayer prefab bulunamadı: " + NetworkPlayerPrefabPath +
                    " (önce '15. Create Network Player Prefab' çalıştırılmış olmalı).");
                return;
            }

            Material validMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/M_PlacementValid.mat");
            Material invalidMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/M_PlacementInvalid.mat");
            if (validMat == null || invalidMat == null)
            {
                Debug.LogError("[Shiftworks] Yerleştirme önizleme materyalleri bulunamadı (önce Aşama 6 kurulumu çalıştırılmış olmalı).");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(NetworkPlayerPrefabPath);
            try
            {
                if (root.GetComponent<PlayerInteractor>() != null)
                {
                    Debug.Log("[Shiftworks] NetworkPlayer prefabında etkileşim sistemi zaten var, dokunulmadı.");
                    return;
                }

                Transform cameraPivot = root.transform.Find("CameraPivot");
                Transform cameraTransform = cameraPivot != null ? cameraPivot.Find("PlayerCamera") : null;
                if (cameraTransform == null)
                {
                    Debug.LogError("[Shiftworks] NetworkPlayer prefabında CameraPivot/PlayerCamera bulunamadı, güncelleme atlandı.");
                    return;
                }

                Transform holdPoint = cameraTransform.Find("HoldPoint");
                if (holdPoint == null)
                {
                    GameObject holdPointGo = new GameObject("HoldPoint");
                    holdPointGo.transform.SetParent(cameraTransform);
                    holdPointGo.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                    holdPointGo.transform.localRotation = Quaternion.identity;
                    holdPoint = holdPointGo.transform;
                }

                InteractionPromptUI promptUI = GetOrCreatePromptUI(root.transform);

                PlayerInteractor interactor = root.AddComponent<PlayerInteractor>();
                SerializedObject interactorSo = new SerializedObject(interactor);
                interactorSo.FindProperty("cameraTransform").objectReferenceValue = cameraTransform;
                interactorSo.FindProperty("holdPoint").objectReferenceValue = holdPoint;
                interactorSo.FindProperty("promptUI").objectReferenceValue = promptUI;
                interactorSo.ApplyModifiedProperties();
                interactor.enabled = false;

                MachinePlacementController placement = root.AddComponent<MachinePlacementController>();
                SerializedObject placementSo = new SerializedObject(placement);
                placementSo.FindProperty("cameraTransform").objectReferenceValue = cameraTransform;
                placementSo.FindProperty("promptUI").objectReferenceValue = promptUI;
                placementSo.FindProperty("validPreviewMaterial").objectReferenceValue = validMat;
                placementSo.FindProperty("invalidPreviewMaterial").objectReferenceValue = invalidMat;
                placementSo.ApplyModifiedProperties();
                placement.enabled = false;

                PrefabUtility.SaveAsPrefabAsset(root, NetworkPlayerPrefabPath);
                Debug.Log("[Shiftworks] NetworkPlayer prefabına etkileşim + makine yerleştirme sistemi eklendi.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static InteractionPromptUI GetOrCreatePromptUI(Transform playerRoot)
        {
            Transform canvasTransform = playerRoot.Find("InteractionPromptCanvas");
            if (canvasTransform != null)
            {
                return canvasTransform.GetComponent<InteractionPromptUI>();
            }

            GameObject canvasGo = new GameObject("InteractionPromptCanvas");
            canvasGo.transform.SetParent(playerRoot);

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();

            GameObject crosshairGo = new GameObject("Crosshair");
            crosshairGo.transform.SetParent(canvasGo.transform);
            Image crosshairImage = crosshairGo.AddComponent<Image>();
            crosshairImage.color = Color.white;
            RectTransform crosshairRect = crosshairGo.GetComponent<RectTransform>();
            crosshairRect.sizeDelta = new Vector2(6f, 6f);
            crosshairRect.anchorMin = crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.anchoredPosition = Vector2.zero;

            GameObject textGo = new GameObject("PromptText");
            textGo.transform.SetParent(canvasGo.transform);
            Text text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 20;
            text.color = Color.white;
            text.text = string.Empty;
            text.enabled = false;
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(320f, 40f);
            textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, -40f);

            InteractionPromptUI promptUI = canvasGo.AddComponent<InteractionPromptUI>();
            SerializedObject serializedPromptUi = new SerializedObject(promptUI);
            serializedPromptUi.FindProperty("promptText").objectReferenceValue = text;
            serializedPromptUi.ApplyModifiedProperties();

            return promptUI;
        }

        /// <summary>
        /// Adds a NetworkObject to the two purchasable machine prefabs (propagates to every
        /// existing scene instance of them too — harmless where no NetworkManager exists,
        /// verified against the installed Netcode package source: NetworkObject.Awake/OnDestroy
        /// both no-op cleanly with no NetworkManager.Singleton present), registers both in a
        /// NetworkPrefabsList asset so the host can NetworkObject.Spawn one at runtime, and
        /// wires a NetworkEconomyBridge onto WorkshopMultiplayer.unity's NetworkManager.
        /// </summary>
        private static void AddNetworkEconomyBridgeToScene()
        {
            GameObject cuttingPrefab = AddNetworkObjectToPrefab(CuttingMachinePrefabPath);
            GameObject assemblyPrefab = AddNetworkObjectToPrefab(AssemblyMachinePrefabPath);
            if (cuttingPrefab == null || assemblyPrefab == null)
            {
                return;
            }

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

            GameObject networkManagerGo = GameObject.Find("NetworkManager");
            if (networkManagerGo == null)
            {
                Debug.LogError("[Shiftworks] 'NetworkManager' sahnede bulunamadı. Önce '30. Add Networking To WorkshopMultiplayer Scene' çalıştırın.");
                return;
            }

            NetworkManager networkManager = networkManagerGo.GetComponent<NetworkManager>();
            NetworkPrefabsList prefabsList = GetOrCreateNetworkPrefabsList(cuttingPrefab, assemblyPrefab);
            if (!networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists.Contains(prefabsList))
            {
                networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(prefabsList);
            }

            GameObject economyGo = GameObject.Find("EconomyManager");
            GameObject purchaseManagerGo = GameObject.Find("MachinePurchaseManager");
            GameObject catalogGo = GameObject.Find("SaveCatalog");
            GameObject expansionManagerGo = GameObject.Find("FactoryExpansionManager");
            GameObject powerManagerGo = GameObject.Find("PowerManager");
            GameObject orderManagerGo = GameObject.Find("OrderManager");

            EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;
            MachinePurchaseManager purchaseManager = purchaseManagerGo != null ? purchaseManagerGo.GetComponent<MachinePurchaseManager>() : null;
            SaveCatalog catalog = catalogGo != null ? catalogGo.GetComponent<SaveCatalog>() : null;
            FactoryExpansionManager expansionManager = expansionManagerGo != null ? expansionManagerGo.GetComponent<FactoryExpansionManager>() : null;
            PowerManager powerManager = powerManagerGo != null ? powerManagerGo.GetComponent<PowerManager>() : null;
            OrderManager orderManager = orderManagerGo != null ? orderManagerGo.GetComponent<OrderManager>() : null;

            if (economy == null || purchaseManager == null || catalog == null
                || expansionManager == null || powerManager == null || orderManager == null)
            {
                Debug.LogError("[Shiftworks] EconomyManager/MachinePurchaseManager/SaveCatalog/FactoryExpansionManager/" +
                    "PowerManager/OrderManager sahnede bulunamadı (hepsi Aşama 5-10 kurulumlarıyla gelmiş olmalı — " +
                    "SaveCatalog için önce '28. Add Save System To Scene' çalıştırılmış olmalı).");
                return;
            }

            GameObject bridgeGo = GameObject.Find("NetworkEconomyBridge");
            bool bridgeIsNew = bridgeGo == null;
            if (bridgeIsNew)
            {
                // Its own GameObject (not NetworkManager's) — an in-scene placed NetworkObject
                // is spawned automatically by NetworkManager's own start sweep
                // (ServerSpawnSceneObjectsOnStartSweep), so it does not need to live on the
                // same GameObject as NetworkManager itself to work.
                bridgeGo = new GameObject("NetworkEconomyBridge");
                bridgeGo.AddComponent<NetworkObject>();
            }

            NetworkEconomyBridge bridge = bridgeGo.GetComponent<NetworkEconomyBridge>();
            if (bridge == null)
            {
                bridge = bridgeGo.AddComponent<NetworkEconomyBridge>();
            }

            // Re-wiring every reference unconditionally (not just on first creation) lets this
            // menu retrofit newly-added fields — factoryExpansionManager/powerManager/
            // orderManager — onto a bridge that was already created by an earlier run.
            SerializedObject bridgeSo = new SerializedObject(bridge);
            bridgeSo.FindProperty("economyManager").objectReferenceValue = economy;
            bridgeSo.FindProperty("purchaseManager").objectReferenceValue = purchaseManager;
            bridgeSo.FindProperty("catalog").objectReferenceValue = catalog;
            bridgeSo.FindProperty("factoryExpansionManager").objectReferenceValue = expansionManager;
            bridgeSo.FindProperty("powerManager").objectReferenceValue = powerManager;
            bridgeSo.FindProperty("orderManager").objectReferenceValue = orderManager;
            bridgeSo.ApplyModifiedProperties();

            Debug.Log(bridgeIsNew
                ? "[Shiftworks] NetworkEconomyBridge sahneye eklendi."
                : "[Shiftworks] NetworkEconomyBridge zaten sahnedeydi — referanslar (yeni alanlar dahil) güncellendi.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesine host-authoritative ekonomi/makine/genişletme/elektrik/sipariş köprüsü eklendi.");
        }

        private static GameObject AddNetworkObjectToPrefab(string prefabPath)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError("[Shiftworks] Makine prefabı bulunamadı: " + prefabPath);
                return null;
            }

            if (prefabAsset.GetComponent<NetworkObject>() != null)
            {
                return prefabAsset;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                root.AddComponent<NetworkObject>();
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("[Shiftworks] NetworkObject eklendi: " + prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static NetworkPrefabsList GetOrCreateNetworkPrefabsList(GameObject cuttingPrefab, GameObject assemblyPrefab)
        {
            NetworkPrefabsList existing = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkPrefabsListPath);
            if (existing != null)
            {
                EnsureListed(existing, cuttingPrefab);
                EnsureListed(existing, assemblyPrefab);
                return existing;
            }

            Directory.CreateDirectory(NetworkDataFolder);
            NetworkPrefabsList list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
            AssetDatabase.CreateAsset(list, NetworkPrefabsListPath);
            EnsureListed(list, cuttingPrefab);
            EnsureListed(list, assemblyPrefab);
            return list;
        }

        private static void EnsureListed(NetworkPrefabsList list, GameObject prefab)
        {
            if (!list.Contains(prefab))
            {
                list.Add(new NetworkPrefab { Prefab = prefab });
            }
        }
    }
}
