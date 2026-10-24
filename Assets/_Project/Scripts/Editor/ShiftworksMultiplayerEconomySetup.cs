using System.IO;
using Shiftworks.Economy;
using Shiftworks.Machines;
using Shiftworks.Multiplayer;
using Shiftworks.Player;
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
    /// (Stage 11B) Idempotent editor tooling for host-authoritative economy + machine
    /// purchase/placement: adds PlayerInteractor + MachinePlacementController (self-contained,
    /// built the exact same way ShiftworksInteractionSetup/ShiftworksMachineShopSetup already
    /// build them on the single-player Player prefab) to the shared NetworkPlayer.prefab, adds
    /// NetworkObject to the two purchasable machine prefabs so a host-spawned purchase is
    /// visible to every client, and wires a NetworkEconomyBridge into WorkshopMultiplayer.unity.
    /// Never touches WorkshopPrototype.unity, Player.prefab, or MultiplayerPrototype.unity.
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

        [MenuItem("Shiftworks/Setup/Apply Multiplayer Economy (Prefabs + Bridge)")]
        public static void ApplyMultiplayerEconomy()
        {
            AddInteractionToNetworkPlayerPrefab();
            AddNetworkEconomyBridgeToScene();
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

            if (GameObject.Find("NetworkEconomyBridge") != null)
            {
                Debug.Log("[Shiftworks] NetworkEconomyBridge zaten sahnede var, dokunulmadı.");
            }
            else
            {
                GameObject economyGo = GameObject.Find("EconomyManager");
                GameObject purchaseManagerGo = GameObject.Find("MachinePurchaseManager");
                GameObject catalogGo = GameObject.Find("SaveCatalog");

                EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;
                MachinePurchaseManager purchaseManager = purchaseManagerGo != null ? purchaseManagerGo.GetComponent<MachinePurchaseManager>() : null;
                SaveCatalog catalog = catalogGo != null ? catalogGo.GetComponent<SaveCatalog>() : null;

                if (economy == null || purchaseManager == null || catalog == null)
                {
                    Debug.LogError("[Shiftworks] EconomyManager/MachinePurchaseManager/SaveCatalog sahnede bulunamadı " +
                        "(SaveCatalog için önce '28. Add Save System To Scene' çalıştırılmış olmalı).");
                    return;
                }

                // Its own GameObject (not NetworkManager's) — an in-scene placed NetworkObject
                // is spawned automatically by NetworkManager's own start sweep
                // (ServerSpawnSceneObjectsOnStartSweep), so it does not need to live on the
                // same GameObject as NetworkManager itself to work.
                GameObject bridgeGo = new GameObject("NetworkEconomyBridge");
                bridgeGo.AddComponent<NetworkObject>();

                NetworkEconomyBridge bridge = bridgeGo.AddComponent<NetworkEconomyBridge>();
                SerializedObject bridgeSo = new SerializedObject(bridge);
                bridgeSo.FindProperty("economyManager").objectReferenceValue = economy;
                bridgeSo.FindProperty("purchaseManager").objectReferenceValue = purchaseManager;
                bridgeSo.FindProperty("catalog").objectReferenceValue = catalog;
                bridgeSo.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesine host-authoritative ekonomi/makine köprüsü eklendi.");
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
