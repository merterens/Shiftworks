using System.Collections.Generic;
using Shiftworks.Multiplayer;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// (Stage 11A) Idempotent editor tooling that turns a safe COPY of the real factory scene
    /// into a 2-player host-authoritative-movement test scene. WorkshopPrototype.unity itself
    /// is never opened for writing here — only AssetDatabase.CopyAsset'd — so the single-player
    /// game is untouched. Reuses NetworkPlayer.prefab, NetworkPlayerSetup and
    /// MultiplayerConnectionUI exactly as built for MultiplayerPrototype.unity (also untouched);
    /// the only new runtime piece is MultiplayerSpawnPoints, since the old prototype's single
    /// shared spawn position was never a problem worth solving in a 3x3m test room.
    /// </summary>
    public static class ShiftworksWorkshopMultiplayerSetup
    {
        private const string SourceScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopMultiplayer.unity";
        private const string LegacyPrototypeScenePath = "Assets/_Project/Scenes/MultiplayerPrototype.unity";
        private const string NetworkPlayerPrefabPath = "Assets/_Project/Prefabs/Multiplayer/NetworkPlayer.prefab";

        [MenuItem("Shiftworks/Setup/29. Create WorkshopMultiplayer Scene (Copy)")]
        public static void CreateWorkshopMultiplayerSceneMenuItem() => CreateWorkshopMultiplayerScene();

        [MenuItem("Shiftworks/Setup/30. Add Networking To WorkshopMultiplayer Scene")]
        public static void AddNetworkingMenuItem() => AddNetworkingToWorkshopMultiplayerScene();

        [MenuItem("Shiftworks/Setup/33. Add Lobby Camera Fallback To WorkshopMultiplayer Scene")]
        public static void AddLobbyCameraFallbackMenuItem() => AddLobbyCameraFallback();

        [MenuItem("Shiftworks/Setup/Apply Workshop Multiplayer (Scene + Networking)")]
        public static void ApplyWorkshopMultiplayer()
        {
            CreateWorkshopMultiplayerScene();
            AddNetworkingToWorkshopMultiplayerScene();
        }

        /// <summary>
        /// Duplicates WorkshopPrototype.unity verbatim (same floor/walls/machines/conveyors/
        /// orders/power/expansion/save-system objects — reusing the real production code, not
        /// a second copy of it) and then removes only the single baked "Player" GameObject,
        /// since multiplayer players are spawned dynamically from NetworkPlayer.prefab instead.
        /// Everything else in the copy (EconomyManager, machines, UI panels, etc.) is left in
        /// place but is harmlessly dormant for 11A: nothing in the scene has a PlayerInteractor
        /// to open/trigger them yet (that wiring is Stage 11C+ work). "PlayerStart" is kept —
        /// it becomes spawn point 1 in the networking step below.
        /// </summary>
        private static void CreateWorkshopMultiplayerScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesi zaten var, dokunulmadı.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath) == null)
            {
                Debug.LogError("[Shiftworks] Kaynak sahne bulunamadı: " + SourceScenePath);
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            if (!AssetDatabase.CopyAsset(SourceScenePath, ScenePath))
            {
                Debug.LogError("[Shiftworks] Sahne kopyalanamadı: " + SourceScenePath + " -> " + ScenePath);
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject playerGo = GameObject.Find("Player");
            if (playerGo != null && playerGo.transform.parent == null)
            {
                Object.DestroyImmediate(playerGo);
                Debug.Log("[Shiftworks] Tek oyunculu 'Player' nesnesi kopyadan kaldırıldı (multiplayer'da NetworkPlayer.prefab kullanılıyor).");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesi oluşturuldu: " + ScenePath);
        }

        /// <summary>
        /// Adds NetworkManager + UnityTransport (same 127.0.0.1:7777 default as the existing
        /// prototype), two spawn points (the scene's existing "PlayerStart" plus one new point
        /// 2m away) wired into a MultiplayerSpawnPoints component via NetworkManager's
        /// ConnectionApprovalCallback, and a Host/Client/Disconnect panel built the same way as
        /// MultiplayerPrototype.unity's (MultiplayerConnectionUI is reused unchanged). Also
        /// makes this scene the build's start scene so a Windows build boots straight into it.
        /// </summary>
        private static void AddNetworkingToWorkshopMultiplayerScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Debug.LogError("[Shiftworks] Önce '29. Create WorkshopMultiplayer Scene (Copy)' çalıştırın.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("NetworkManager") != null)
            {
                Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesinde ağ bileşenleri zaten var, dokunulmadı.");
                EnsureStartupScene();
                return;
            }

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkPlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError("[Shiftworks] NetworkPlayer prefab bulunamadı: " + NetworkPlayerPrefabPath +
                    " (önce '15. Create Network Player Prefab' çalıştırılmış olmalı).");
                return;
            }

            GameObject networkManagerGo = new GameObject("NetworkManager");
            NetworkManager networkManager = networkManagerGo.AddComponent<NetworkManager>();
            UnityTransport transport = networkManagerGo.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777);

            // Same manual-wiring gotcha as the original prototype: building a NetworkManager
            // via script skips the Inspector's auto-wire, so this must be set explicitly.
            networkManager.NetworkConfig.NetworkTransport = transport;
            networkManager.NetworkConfig.PlayerPrefab = playerPrefab;

            Transform spawnPoint1 = FindOrCreateSpawnPoint("PlayerStart", new Vector3(0f, 0f, -6f));
            Transform spawnPoint2 = FindOrCreateSpawnPoint("PlayerSpawnPoint_2", spawnPoint1.position + new Vector3(2f, 0f, 0f));

            MultiplayerSpawnPoints spawnPoints = networkManagerGo.AddComponent<MultiplayerSpawnPoints>();
            SerializedObject spawnSo = new SerializedObject(spawnPoints);
            SerializedProperty spawnArray = spawnSo.FindProperty("spawnPoints");
            spawnArray.arraySize = 2;
            spawnArray.GetArrayElementAtIndex(0).objectReferenceValue = spawnPoint1;
            spawnArray.GetArrayElementAtIndex(1).objectReferenceValue = spawnPoint2;
            spawnSo.ApplyModifiedProperties();

            EnsureEventSystem();
            BuildConnectionUI(transport);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            EnsureStartupScene();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesine ağ katmanı eklendi: " + ScenePath);
        }

        /// <summary>
        /// (Stage 11 bugfix) Before any player spawns, WorkshopMultiplayer.unity has no
        /// active camera at all — every NetworkPlayer's own camera starts disabled until
        /// OnNetworkSpawn turns it on for its owner, which is exactly when Unity's "No
        /// cameras rendering" warning was being seen on both the host and a still-connecting
        /// client. Adds one always-on fallback camera; NetworkPlayerSetup turns it off for a
        /// viewer the moment that viewer's own local player spawns, and back on if that
        /// player later despawns (disconnect), so it is never a second active camera at the
        /// same time as a real one.
        /// </summary>
        private static void AddLobbyCameraFallback()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Debug.LogError("[Shiftworks] WorkshopMultiplayer sahnesi bulunamadı (önce '29'/'30' çalıştırılmış olmalı).");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("LobbyCamera") != null)
            {
                Debug.Log("[Shiftworks] LobbyCamera zaten sahnede var, dokunulmadı.");
                return;
            }

            Transform spawnReference = FindOrCreateSpawnPoint("PlayerStart", new Vector3(0f, 0f, -6f));

            GameObject lobbyCameraGo = new GameObject("LobbyCamera");
            lobbyCameraGo.transform.position = spawnReference.position + new Vector3(0f, 1.6f, 0f);
            lobbyCameraGo.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
            lobbyCameraGo.AddComponent<Camera>();
            lobbyCameraGo.AddComponent<AudioListener>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] WorkshopMultiplayer sahnesine LobbyCamera (bağlantı ekranı için yedek kamera) eklendi.");
        }

        private static Transform FindOrCreateSpawnPoint(string name, Vector3 fallbackPosition)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                return go.transform;
            }

            go = new GameObject(name);
            go.transform.position = fallbackPosition;
            return go.transform;
        }

        /// <summary>
        /// Puts WorkshopMultiplayer.unity first and enabled in Build Settings, and disables
        /// (never removes) MultiplayerPrototype.unity's entry, so a fresh Windows build boots
        /// into the real factory test scene by default while the old prototype scene file and
        /// its own build-list entry both stay intact and re-enable-able from the Build Settings
        /// window at any time.
        /// </summary>
        private static void EnsureStartupScene()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            int existingIndex = scenes.FindIndex(s => s.path == ScenePath);
            if (existingIndex >= 0)
            {
                scenes[existingIndex] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettingsScene moved = scenes[existingIndex];
                scenes.RemoveAt(existingIndex);
                scenes.Insert(0, moved);
            }
            else
            {
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            }

            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == LegacyPrototypeScenePath)
                {
                    scenes[i] = new EditorBuildSettingsScene(scenes[i].path, false);
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[Shiftworks] Build Settings güncellendi: başlangıç sahnesi = " + ScenePath);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule uiModule = go.AddComponent<InputSystemUIInputModule>();
            uiModule.AssignDefaultActions();
        }

        private static void BuildConnectionUI(UnityTransport transport)
        {
            GameObject canvasGo = new GameObject("WorkshopConnectionUICanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(canvasGo.transform, false);
            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.1f, 0.85f);
            RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
            Vector2 topLeft = new Vector2(0f, 1f);
            panelRect.anchorMin = topLeft;
            panelRect.anchorMax = topLeft;
            panelRect.pivot = topLeft;
            panelRect.sizeDelta = new Vector2(300f, 260f);
            panelRect.anchoredPosition = new Vector2(20f, -20f);

            Vector2 center = new Vector2(0.5f, 0.5f);

            CreateText(panelRoot.transform, "TitleText", "WORKSHOP MULTIPLAYER", 16, TextAnchor.MiddleCenter,
                center, new Vector2(0f, 105f), new Vector2(260f, 26f));

            GameObject ipFieldGo = new GameObject("IpInputField");
            ipFieldGo.transform.SetParent(panelRoot.transform, false);
            Image ipBg = ipFieldGo.AddComponent<Image>();
            ipBg.color = Color.white;
            RectTransform ipRect = ipFieldGo.GetComponent<RectTransform>();
            ipRect.anchorMin = center;
            ipRect.anchorMax = center;
            ipRect.pivot = center;
            ipRect.sizeDelta = new Vector2(260f, 30f);
            ipRect.anchoredPosition = new Vector2(0f, 70f);

            GameObject ipTextGo = new GameObject("Text");
            ipTextGo.transform.SetParent(ipFieldGo.transform, false);
            Text ipText = ipTextGo.AddComponent<Text>();
            ipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ipText.fontSize = 16;
            ipText.alignment = TextAnchor.MiddleLeft;
            ipText.color = Color.black;
            RectTransform ipTextRect = ipTextGo.GetComponent<RectTransform>();
            ipTextRect.anchorMin = Vector2.zero;
            ipTextRect.anchorMax = Vector2.one;
            ipTextRect.offsetMin = new Vector2(8f, 0f);
            ipTextRect.offsetMax = new Vector2(-8f, 0f);

            InputField ipInputField = ipFieldGo.AddComponent<InputField>();
            ipInputField.textComponent = ipText;
            ipInputField.text = "127.0.0.1";

            Button hostButton = CreateButton(panelRoot.transform, "HostButton", "Host Başlat",
                new Vector2(0f, 25f), new Vector2(260f, 36f));
            Button clientButton = CreateButton(panelRoot.transform, "ClientButton", "Client Bağlan",
                new Vector2(0f, -20f), new Vector2(260f, 36f));
            Button disconnectButton = CreateButton(panelRoot.transform, "DisconnectButton", "Bağlantıyı Kes",
                new Vector2(0f, -65f), new Vector2(260f, 36f));

            Text statusText = CreateText(panelRoot.transform, "StatusText", "Durum: Bağlı değil", 15, TextAnchor.MiddleCenter,
                center, new Vector2(0f, -105f), new Vector2(260f, 26f));

            MultiplayerConnectionUI ui = canvasGo.AddComponent<MultiplayerConnectionUI>();
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("transport").objectReferenceValue = transport;
            so.FindProperty("ipInputField").objectReferenceValue = ipInputField;
            so.FindProperty("hostButton").objectReferenceValue = hostButton;
            so.FindProperty("clientButton").objectReferenceValue = clientButton;
            so.FindProperty("disconnectButton").objectReferenceValue = disconnectButton;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.ApplyModifiedProperties();
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment,
            Vector2 anchor, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            Text text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = content;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            Image image = go.AddComponent<Image>();
            image.color = new Color(0.25f, 0.5f, 0.3f);

            Button button = go.AddComponent<Button>();

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            GameObject labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            Text labelText = labelGo.AddComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.text = label;

            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;

            return button;
        }
    }
}
