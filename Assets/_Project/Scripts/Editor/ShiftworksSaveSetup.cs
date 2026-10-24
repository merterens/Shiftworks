using Shiftworks.Economy;
using Shiftworks.Factory;
using Shiftworks.Items;
using Shiftworks.Machines;
using Shiftworks.Orders;
using Shiftworks.Player;
using Shiftworks.Power;
using Shiftworks.SaveSystem;
using Shiftworks.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling for Stage 10: adds SaveCatalog (wired to every known data
    /// asset by its fixed, already-existing Data/ folder path — the same hardcoded-path
    /// pattern every earlier Editor tool in this project already uses), SaveManager (wired
    /// to every existing manager) and the Escape-triggered SaveLoadMenuUI panel to the
    /// existing WorkshopPrototype scene. Never recreates the scene, never touches
    /// MultiplayerPrototype.unity.
    /// </summary>
    public static class ShiftworksSaveSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";

        private const string ItemsFolder = "Assets/_Project/Data/Items";
        private const string MachinesDataFolder = "Assets/_Project/Data/Machines";
        private const string OrdersFolder = "Assets/_Project/Data/Orders";
        private const string ExpansionsFolder = "Assets/_Project/Data/FactoryExpansions";
        private const string ShipmentPackagePrefabPath = "Assets/_Project/Prefabs/Items/ShipmentPackage.prefab";

        [MenuItem("Shiftworks/Setup/28. Add Save System To Scene")]
        public static void AddSaveSystemMenuItem() => AddSaveSystemToScene();

        private static void AddSaveSystemToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("SaveManager") != null)
            {
                Debug.Log("[Shiftworks] Kayıt sistemi zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject economyGo = GameObject.Find("EconomyManager");
            GameObject powerGo = GameObject.Find("PowerManager");
            GameObject expansionGo = GameObject.Find("FactoryExpansionManager");
            GameObject orderGo = GameObject.Find("OrderManager");
            GameObject statsGo = GameObject.Find("ProductionStatsTracker");
            GameObject playerGo = GameObject.Find("Player");
            GameObject playerStartGo = GameObject.Find("PlayerStart");

            EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;
            PowerManager power = powerGo != null ? powerGo.GetComponent<PowerManager>() : null;
            FactoryExpansionManager expansionManager = expansionGo != null ? expansionGo.GetComponent<FactoryExpansionManager>() : null;
            OrderManager orderManager = orderGo != null ? orderGo.GetComponent<OrderManager>() : null;
            ProductionStatsTracker statsTracker = statsGo != null ? statsGo.GetComponent<ProductionStatsTracker>() : null;
            PlayerInteractor playerInteractor = playerGo != null ? playerGo.GetComponent<PlayerInteractor>() : null;
            FirstPersonController firstPersonController = playerGo != null ? playerGo.GetComponent<FirstPersonController>() : null;
            GameObject shipmentPackagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShipmentPackagePrefabPath);

            if (economy == null || orderManager == null || playerInteractor == null)
            {
                Debug.LogError("[Shiftworks] EconomyManager, OrderManager veya Player bulunamadı. " +
                               "Önce Aşama 5 ve Aşama 1 kurulumları çalıştırılmalı.");
                return;
            }

            SaveCatalog catalog = BuildCatalog();

            GameObject managerGo = new GameObject("SaveManager");
            SaveManager saveManager = managerGo.AddComponent<SaveManager>();
            SerializedObject managerSo = new SerializedObject(saveManager);
            managerSo.FindProperty("economyManager").objectReferenceValue = economy;
            managerSo.FindProperty("powerManager").objectReferenceValue = power;
            managerSo.FindProperty("factoryExpansionManager").objectReferenceValue = expansionManager;
            managerSo.FindProperty("orderManager").objectReferenceValue = orderManager;
            managerSo.FindProperty("statsTracker").objectReferenceValue = statsTracker;
            managerSo.FindProperty("catalog").objectReferenceValue = catalog;
            managerSo.FindProperty("playerInteractor").objectReferenceValue = playerInteractor;
            managerSo.FindProperty("firstPersonController").objectReferenceValue = firstPersonController;
            managerSo.FindProperty("playerStart").objectReferenceValue = playerStartGo;
            managerSo.FindProperty("shipmentPackagePrefab").objectReferenceValue = shipmentPackagePrefab;
            managerSo.ApplyModifiedProperties();

            SaveLoadMenuUI panelUi = BuildSaveLoadPanel(saveManager);

            if (playerInteractor != null)
            {
                SerializedObject playerSo = new SerializedObject(playerInteractor);
                playerSo.FindProperty("saveLoadMenuUI").objectReferenceValue = panelUi;
                playerSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(playerGo);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Kayıt/yükleme sistemi (SaveCatalog, SaveManager, Escape menüsü) sahneye eklendi.");
        }

        private static SaveCatalog BuildCatalog()
        {
            GameObject catalogGo = new GameObject("SaveCatalog");
            SaveCatalog catalog = catalogGo.AddComponent<SaveCatalog>();

            ItemDefinition[] items =
            {
                AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{ItemsFolder}/MetalSheet.asset"),
                AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{ItemsFolder}/CutMetalPart.asset"),
                AssetDatabase.LoadAssetAtPath<ItemDefinition>($"{ItemsFolder}/AssembledComponent.asset"),
            };

            MachineDefinition[] machines =
            {
                AssetDatabase.LoadAssetAtPath<MachineDefinition>($"{MachinesDataFolder}/CuttingMachineDefinition.asset"),
                AssetDatabase.LoadAssetAtPath<MachineDefinition>($"{MachinesDataFolder}/AssemblyMachineDefinition.asset"),
            };

            OrderDefinition[] orders =
            {
                AssetDatabase.LoadAssetAtPath<OrderDefinition>($"{OrdersFolder}/FirstMetalPartOrder.asset"),
                AssetDatabase.LoadAssetAtPath<OrderDefinition>($"{OrdersFolder}/AssemblyOrder.asset"),
            };

            FactoryExpansionDefinition[] expansions =
            {
                AssetDatabase.LoadAssetAtPath<FactoryExpansionDefinition>($"{ExpansionsFolder}/ProductionAreaB.asset"),
            };

            SerializedObject so = new SerializedObject(catalog);
            SetArray(so, "items", items);
            SetArray(so, "machines", machines);
            SetArray(so, "orders", orders);
            SetArray(so, "expansions", expansions);
            so.ApplyModifiedProperties();

            int missing = 0;
            foreach (var asset in items) if (asset == null) missing++;
            foreach (var asset in machines) if (asset == null) missing++;
            foreach (var asset in orders) if (asset == null) missing++;
            foreach (var asset in expansions) if (asset == null) missing++;
            if (missing > 0)
            {
                Debug.LogWarning($"[Shiftworks] SaveCatalog kuruldu ama {missing} veri asset'i bulunamadı " +
                                  "(önceki aşamaların kurulum menüleri eksik çalıştırılmış olabilir) — bu türler " +
                                  "kayıt/yüklemede desteklenmeyecek, diğerleri etkilenmez.");
            }

            return catalog;
        }

        private static void SetArray(SerializedObject so, string propertyName, Object[] values)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static SaveLoadMenuUI BuildSaveLoadPanel(SaveManager saveManager)
        {
            GameObject canvasGo = new GameObject("SaveLoadMenuCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(canvasGo.transform, false);
            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
            RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(420f, 400f);
            panelRect.anchoredPosition = Vector2.zero;

            Vector2 center = new Vector2(0.5f, 0.5f);

            CreateText(panelRoot.transform, "TitleText", "OYUN MENÜSÜ", 24, TextAnchor.MiddleCenter,
                center, new Vector2(0f, 150f), new Vector2(380f, 36f));

            Button saveButton = CreateButton(panelRoot.transform, "SaveButton", "Oyunu Kaydet",
                new Vector2(0f, 70f), new Vector2(300f, 44f));
            Button loadButton = CreateButton(panelRoot.transform, "LoadButton", "Oyunu Yükle",
                new Vector2(0f, 10f), new Vector2(300f, 44f));
            Button newGameButton = CreateButton(panelRoot.transform, "NewGameButton", "Yeni Oyun",
                new Vector2(0f, -50f), new Vector2(300f, 44f));
            Button resumeButton = CreateButton(panelRoot.transform, "ResumeButton", "Devam Et",
                new Vector2(0f, -110f), new Vector2(300f, 44f));

            Text statusText = CreateText(panelRoot.transform, "StatusText", "", 16, TextAnchor.MiddleCenter,
                center, new Vector2(0f, -165f), new Vector2(380f, 50f));

            SaveLoadMenuUI panelUi = canvasGo.AddComponent<SaveLoadMenuUI>();
            SerializedObject so = new SerializedObject(panelUi);
            so.FindProperty("saveManager").objectReferenceValue = saveManager;
            so.FindProperty("panelRoot").objectReferenceValue = panelRoot;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.FindProperty("saveButton").objectReferenceValue = saveButton;
            so.FindProperty("loadButton").objectReferenceValue = loadButton;
            so.FindProperty("newGameButton").objectReferenceValue = newGameButton;
            so.FindProperty("resumeButton").objectReferenceValue = resumeButton;
            so.ApplyModifiedProperties();

            return panelUi;
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
            labelText.fontSize = 18;
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
