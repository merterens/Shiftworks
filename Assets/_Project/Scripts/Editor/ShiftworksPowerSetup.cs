using System.IO;
using Shiftworks.Conveyors;
using Shiftworks.Economy;
using Shiftworks.Machines;
using Shiftworks.Power;
using Shiftworks.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling for Stage 8: adds PowerManager + ProductionStatsTracker to
    /// the existing WorkshopPrototype scene, wires the scene's one original CuttingMachine
    /// and its three ConveyorSegments to draw power (4 kW / 0.5 kW each, matching the spec),
    /// sets the same default on the purchasable CuttingMachine.prefab asset (its PowerManager
    /// reference is intentionally left unset — a prefab ASSET cannot hold a baked reference
    /// to a scene singleton; MachineController/ConveyorSegment fall back to a runtime lookup
    /// for any instance placed later), and adds the electricity panel terminal + UI. Never
    /// recreates the scene or touches Stage 1-7 systems.
    /// </summary>
    public static class ShiftworksPowerSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string MachinePrefabPath = "Assets/_Project/Prefabs/Machines/CuttingMachine.prefab";

        private const float MachinePowerDemandKw = 4f;
        private const float ConveyorPowerDemandKw = 0.5f;

        [MenuItem("Shiftworks/Setup/20. Add Power System To Scene")]
        public static void AddPowerSystemMenuItem() => AddPowerSystemToScene();

        [MenuItem("Shiftworks/Setup/21. Add Power Panel To Scene")]
        public static void AddPowerPanelMenuItem() => AddPowerPanelToScene();

        [MenuItem("Shiftworks/Setup/Apply Power System (Managers + Panel)")]
        public static void ApplyPowerSystem()
        {
            AddPowerSystemToScene();
            AddPowerPanelToScene();
        }

        private static void AddPowerSystemToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("PowerManager") != null)
            {
                Debug.Log("[Shiftworks] Elektrik sistemi zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject economyGo = GameObject.Find("EconomyManager");
            EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;
            if (economy == null)
            {
                Debug.LogError("[Shiftworks] EconomyManager bulunamadı. Önce Aşama 5 kurulumu çalıştırılmalı.");
                return;
            }

            GameObject managerGo = new GameObject("PowerManager");
            PowerManager powerManager = managerGo.AddComponent<PowerManager>();
            SerializedObject managerSo = new SerializedObject(powerManager);
            managerSo.FindProperty("economyManager").objectReferenceValue = economy;
            managerSo.FindProperty("initialCapacityKw").floatValue = 10f;
            managerSo.FindProperty("upgradedCapacityKw").floatValue = 20f;
            managerSo.FindProperty("upgradePriceDollars").intValue = 300;
            managerSo.ApplyModifiedProperties();

            GameObject statsGo = new GameObject("ProductionStatsTracker");
            ProductionStatsTracker statsTracker = statsGo.AddComponent<ProductionStatsTracker>();

            GameObject machineGo = GameObject.Find("CuttingMachine");
            MachineController machine = machineGo != null ? machineGo.GetComponent<MachineController>() : null;
            if (machine == null)
            {
                Debug.LogError("[Shiftworks] Sahnedeki 'CuttingMachine' bulunamadı. Önce Aşama 3 kurulumu çalıştırılmalı.");
                return;
            }

            WireConsumer(new SerializedObject(machine), powerManager, statsTracker, MachinePowerDemandKw, true);

            string[] conveyorNames = { "Conveyor_In_1", "Conveyor_In_2", "Conveyor_Out_1" };
            int wiredConveyors = 0;
            foreach (string conveyorName in conveyorNames)
            {
                GameObject conveyorGo = GameObject.Find(conveyorName);
                ConveyorSegment segment = conveyorGo != null ? conveyorGo.GetComponent<ConveyorSegment>() : null;
                if (segment == null)
                {
                    Debug.LogWarning($"[Shiftworks] '{conveyorName}' bulunamadı, atlandı.");
                    continue;
                }

                WireConsumer(new SerializedObject(segment), powerManager, null, ConveyorPowerDemandKw, false);
                wiredConveyors++;
            }

            // The purchasable standalone prefab gets the same demand value so a shop-bought
            // machine behaves identically; its powerManager/statsTracker stay unset on the
            // asset itself (see class doc) and are resolved at runtime instead.
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MachinePrefabPath);
            if (prefabAsset != null)
            {
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(MachinePrefabPath);
                try
                {
                    MachineController prefabMachine = prefabRoot.GetComponent<MachineController>();
                    if (prefabMachine != null)
                    {
                        SerializedObject prefabSo = new SerializedObject(prefabMachine);
                        prefabSo.FindProperty("powerDemandKw").floatValue = MachinePowerDemandKw;
                        prefabSo.ApplyModifiedProperties();
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, MachinePrefabPath);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
            else
            {
                Debug.LogWarning("[Shiftworks] CuttingMachine.prefab bulunamadı (Aşama 6 henüz çalıştırılmamış olabilir), atlandı.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Shiftworks] Elektrik sistemi eklendi: PowerManager (10 kW), ProductionStatsTracker, " +
                      $"1 makine ({MachinePowerDemandKw} kW) ve {wiredConveyors}/3 konveyör ({ConveyorPowerDemandKw} kW her biri) bağlandı. " +
                      $"Toplam en kötü durum tepe tüketim: {MachinePowerDemandKw + wiredConveyors * ConveyorPowerDemandKw} kW / 10 kW.");
        }

        private static void WireConsumer(SerializedObject so, PowerManager powerManager, ProductionStatsTracker statsTracker,
            float demandKw, bool isMachine)
        {
            so.FindProperty("powerDemandKw").floatValue = demandKw;
            so.FindProperty("powerManager").objectReferenceValue = powerManager;
            if (isMachine)
            {
                so.FindProperty("statsTracker").objectReferenceValue = statsTracker;
            }
            so.ApplyModifiedProperties();
        }

        private static void AddPowerPanelToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("PowerPanelTerminal") != null)
            {
                Debug.Log("[Shiftworks] Elektrik panosu zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject powerManagerGo = GameObject.Find("PowerManager");
            GameObject economyGo = GameObject.Find("EconomyManager");
            GameObject statsGo = GameObject.Find("ProductionStatsTracker");
            GameObject machineGo = GameObject.Find("CuttingMachine");

            PowerManager powerManager = powerManagerGo != null ? powerManagerGo.GetComponent<PowerManager>() : null;
            EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;
            ProductionStatsTracker statsTracker = statsGo != null ? statsGo.GetComponent<ProductionStatsTracker>() : null;
            MachineController machine = machineGo != null ? machineGo.GetComponent<MachineController>() : null;

            if (powerManager == null || economy == null)
            {
                Debug.LogError("[Shiftworks] PowerManager veya EconomyManager bulunamadı. Önce '20. Add Power System To Scene' çalıştırılmalı.");
                return;
            }

            Material terminalMat = GetOrCreateMaterial("M_PowerTerminal", new Color(0.8f, 0.65f, 0.1f));
            GameObject terminalGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminalGo.name = "PowerPanelTerminal";
            terminalGo.transform.position = new Vector3(-8.3f, 0.75f, 4f);
            terminalGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            terminalGo.transform.localScale = new Vector3(0.6f, 1.5f, 0.3f);
            terminalGo.GetComponent<Renderer>().sharedMaterial = terminalMat;

            PowerPanelUI panelUi = BuildPowerPanel();

            PowerPanelTerminal terminal = terminalGo.AddComponent<PowerPanelTerminal>();
            SerializedObject terminalSo = new SerializedObject(terminal);
            terminalSo.FindProperty("powerManager").objectReferenceValue = powerManager;
            terminalSo.FindProperty("economyManager").objectReferenceValue = economy;
            terminalSo.FindProperty("statsTracker").objectReferenceValue = statsTracker;
            terminalSo.FindProperty("trackedMachine").objectReferenceValue = machine;
            terminalSo.FindProperty("panelUI").objectReferenceValue = panelUi;
            terminalSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Elektrik panosu terminali ve paneli sahneye eklendi.");
        }

        private static PowerPanelUI BuildPowerPanel()
        {
            GameObject canvasGo = new GameObject("PowerPanelCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 4;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(canvasGo.transform, false);
            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520f, 480f);
            panelRect.anchoredPosition = Vector2.zero;

            Vector2 center = new Vector2(0.5f, 0.5f);

            CreateText(panelRoot.transform, "TitleText", "ELEKTRİK PANOSU", 22, TextAnchor.MiddleCenter,
                center, new Vector2(0f, 200f), new Vector2(480f, 36f));

            Text capacityText = CreateText(panelRoot.transform, "CapacityText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 150f), new Vector2(480f, 28f));
            Text usedText = CreateText(panelRoot.transform, "UsedText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 122f), new Vector2(480f, 28f));
            Text availableText = CreateText(panelRoot.transform, "AvailableText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 94f), new Vector2(480f, 28f));
            Text connectedCountText = CreateText(panelRoot.transform, "ConnectedCountText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 66f), new Vector2(480f, 28f));
            Text waitingCountText = CreateText(panelRoot.transform, "WaitingCountText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 38f), new Vector2(480f, 28f));
            Text balanceText = CreateText(panelRoot.transform, "BalanceText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 10f), new Vector2(480f, 28f));
            Text upgradeText = CreateText(panelRoot.transform, "UpgradeText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, -18f), new Vector2(480f, 28f));
            Text statsText = CreateText(panelRoot.transform, "StatsText", "-", 16, TextAnchor.UpperLeft,
                center, new Vector2(0f, -55f), new Vector2(480f, 60f));

            Text warningText = CreateText(panelRoot.transform, "WarningText", "Yetersiz bakiye!", 16, TextAnchor.MiddleCenter,
                center, new Vector2(0f, -125f), new Vector2(480f, 26f));
            warningText.color = new Color(1f, 0.4f, 0.4f);

            Button upgradeButton = CreateButton(panelRoot.transform, "UpgradeButton", "Kapasiteyi Yükselt",
                new Vector2(-110f, -205f), new Vector2(200f, 44f));
            Button closeButton = CreateButton(panelRoot.transform, "CloseButton", "Kapat",
                new Vector2(130f, -205f), new Vector2(120f, 44f));

            PowerPanelUI panelUi = canvasGo.AddComponent<PowerPanelUI>();
            SerializedObject so = new SerializedObject(panelUi);
            so.FindProperty("panelRoot").objectReferenceValue = panelRoot;
            so.FindProperty("capacityText").objectReferenceValue = capacityText;
            so.FindProperty("usedText").objectReferenceValue = usedText;
            so.FindProperty("availableText").objectReferenceValue = availableText;
            so.FindProperty("connectedCountText").objectReferenceValue = connectedCountText;
            so.FindProperty("waitingCountText").objectReferenceValue = waitingCountText;
            so.FindProperty("balanceText").objectReferenceValue = balanceText;
            so.FindProperty("upgradeText").objectReferenceValue = upgradeText;
            so.FindProperty("statsText").objectReferenceValue = statsText;
            so.FindProperty("warningText").objectReferenceValue = warningText;
            so.FindProperty("upgradeButton").objectReferenceValue = upgradeButton;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
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
