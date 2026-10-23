using System.IO;
using Shiftworks.Multiplayer;
using Shiftworks.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
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
    /// Idempotent editor tooling for the two-player multiplayer technical prototype: builds
    /// a standalone NetworkPlayer prefab (CharacterController + the EXISTING FirstPersonController,
    /// disabled by default — never rewritten) and a separate MultiplayerPrototype scene with its
    /// own small room, NetworkManager + UnityTransport, and a dev-only Host/Client/Disconnect
    /// panel. Never touches WorkshopPrototype.unity or the single-player Player.prefab.
    /// </summary>
    public static class ShiftworksMultiplayerPrototypeSetup
    {
        private const string NetworkPrefabFolder = "Assets/_Project/Prefabs/Multiplayer";
        private const string NetworkPlayerPrefabPath = NetworkPrefabFolder + "/NetworkPlayer.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/MultiplayerPrototype.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";

        [MenuItem("Shiftworks/Setup/15. Create Network Player Prefab")]
        public static void CreateNetworkPlayerPrefabMenuItem() => CreateNetworkPlayerPrefab();

        [MenuItem("Shiftworks/Setup/16. Create Multiplayer Prototype Scene")]
        public static void CreateMultiplayerSceneMenuItem() => BuildMultiplayerPrototypeScene();

        [MenuItem("Shiftworks/Setup/Apply Multiplayer Prototype (Prefab + Scene)")]
        public static void ApplyMultiplayerPrototype()
        {
            CreateNetworkPlayerPrefab();
            BuildMultiplayerPrototypeScene();
        }

        [MenuItem("Shiftworks/Setup/17. Fix Multiplayer NetworkManager Transport")]
        public static void FixNetworkManagerTransportMenuItem() => FixNetworkManagerTransport();

        /// <summary>
        /// Repairs an already-built MultiplayerPrototype scene in place: wires the existing
        /// NetworkManager's NetworkConfig.NetworkTransport to its sibling UnityTransport
        /// component. Building both via script (not the Inspector's "Add Component" flow)
        /// skips the auto-wiring the NetworkManager editor normally does, which is why
        /// StartHost/StartClient failed with "No transport has been selected!". Never
        /// recreates the scene; safe to re-run.
        /// </summary>
        private static void FixNetworkManagerTransport()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject networkManagerGo = GameObject.Find("NetworkManager");
            if (networkManagerGo == null)
            {
                Debug.LogError("[Shiftworks] 'NetworkManager' sahnede bulunamadı. Önce '16. Create Multiplayer Prototype Scene' çalıştırın.");
                return;
            }

            NetworkManager networkManager = networkManagerGo.GetComponent<NetworkManager>();
            UnityTransport transport = networkManagerGo.GetComponent<UnityTransport>();
            if (networkManager == null || transport == null)
            {
                Debug.LogError("[Shiftworks] NetworkManager veya UnityTransport bileşeni bulunamadı.");
                return;
            }

            networkManager.NetworkConfig.NetworkTransport = transport;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] NetworkManager'ın transport referansı düzeltildi: " + ScenePath);
        }

        private static void CreateNetworkPlayerPrefab()
        {
            Directory.CreateDirectory(NetworkPrefabFolder);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(NetworkPlayerPrefabPath) != null)
            {
                Debug.Log("[Shiftworks] NetworkPlayer prefab zaten var, dokunulmadı.");
                return;
            }

            GameObject root = new GameObject("NetworkPlayer");

            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            // Disabled by default: every client instantiates one of these per connected
            // player (including remote ones). If this started enabled, every remote
            // representation's OnEnable() would also run and fight over the local cursor
            // lock. Only NetworkPlayerSetup (on spawn, owner only) turns it on.
            FirstPersonController fpc = root.AddComponent<FirstPersonController>();
            fpc.enabled = false;

            GameObject bodyVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyVisual.name = "BodyVisual";
            bodyVisual.transform.SetParent(root.transform, true);
            bodyVisual.transform.position = new Vector3(0f, 0.9f, 0f);
            bodyVisual.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            Object.DestroyImmediate(bodyVisual.GetComponent<Collider>());
            bodyVisual.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateMaterial("M_NetworkPlayerBody", new Color(0.2f, 0.5f, 0.9f));

            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.SetParent(root.transform, true);
            cameraPivot.transform.position = new Vector3(0f, 1.6f, 0f);

            GameObject cameraGo = new GameObject("PlayerCamera");
            cameraGo.transform.SetParent(cameraPivot.transform, true);
            cameraGo.transform.localPosition = Vector3.zero;
            cameraGo.transform.localRotation = Quaternion.identity;
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.enabled = false;
            AudioListener listener = cameraGo.AddComponent<AudioListener>();
            listener.enabled = false;

            SerializedObject fpcSo = new SerializedObject(fpc);
            fpcSo.FindProperty("cameraPivot").objectReferenceValue = cameraPivot.transform;
            fpcSo.ApplyModifiedProperties();

            root.AddComponent<NetworkObject>();

            NetworkTransform netTransform = root.AddComponent<NetworkTransform>();
            netTransform.AuthorityMode = NetworkTransform.AuthorityModes.Owner;

            NetworkPlayerSetup setup = root.AddComponent<NetworkPlayerSetup>();
            SerializedObject setupSo = new SerializedObject(setup);
            setupSo.FindProperty("firstPersonController").objectReferenceValue = fpc;
            setupSo.FindProperty("playerCamera").objectReferenceValue = camera;
            setupSo.FindProperty("audioListener").objectReferenceValue = listener;
            setupSo.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, NetworkPlayerPrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            Debug.Log("[Shiftworks] NetworkPlayer prefab oluşturuldu: " + NetworkPlayerPrefabPath);
        }

        private static void BuildMultiplayerPrototypeScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log("[Shiftworks] MultiplayerPrototype sahnesi zaten var, dokunulmadı.");
                return;
            }

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkPlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError("[Shiftworks] Önce '15. Create Network Player Prefab' çalıştırın.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floorMat = GetOrCreateMaterial("M_Floor", new Color(0.55f, 0.55f, 0.55f));
            Material wallMat = GetOrCreateMaterial("M_Wall", new Color(0.82f, 0.78f, 0.7f));

            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(10f, 0.2f, 10f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            CreateWall("Wall_North", new Vector3(0f, 1.5f, 5f), new Vector3(10f, 3f, 0.3f), wallMat);
            CreateWall("Wall_South", new Vector3(0f, 1.5f, -5f), new Vector3(10f, 3f, 0.3f), wallMat);
            CreateWall("Wall_East", new Vector3(5f, 1.5f, 0f), new Vector3(0.3f, 3f, 10f), wallMat);
            CreateWall("Wall_West", new Vector3(-5f, 1.5f, 0f), new Vector3(0.3f, 3f, 10f), wallMat);

            EnsureEventSystem();

            GameObject networkManagerGo = new GameObject("NetworkManager");
            NetworkManager networkManager = networkManagerGo.AddComponent<NetworkManager>();
            UnityTransport transport = networkManagerGo.AddComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777);

            // The Inspector's NetworkManager editor auto-wires this when you add a transport
            // via the UI; building everything through script skips that step, so it must be
            // set explicitly or StartHost/StartClient fail with "No transport has been selected!".
            networkManager.NetworkConfig.NetworkTransport = transport;
            networkManager.NetworkConfig.PlayerPrefab = playerPrefab;

            BuildConnectionUI(transport);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Shiftworks] MultiplayerPrototype sahnesi oluşturuldu: " + ScenePath);
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = material;
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
            GameObject canvasGo = new GameObject("ConnectionUICanvas");
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

            CreateText(panelRoot.transform, "TitleText", "MULTIPLAYER TEST", 18, TextAnchor.MiddleCenter,
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

        private static Material GetOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(MaterialsFolder);
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
