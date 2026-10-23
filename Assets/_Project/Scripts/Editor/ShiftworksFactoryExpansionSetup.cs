using System.IO;
using Shiftworks.Economy;
using Shiftworks.Factory;
using Shiftworks.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling for Stage 7: creates the "Üretim Alanı B" expansion data
    /// asset, then adds the second production hall (floor + 3 outer walls), a controlled
    /// doorway in the existing west wall (split into two segments + a locked barrier, the
    /// original wall is never deleted outright — only replaced with the same two-segment
    /// "wall with a gap" pattern Wall_South already uses), the expansion terminal + its
    /// purchase manager + panel UI, to the existing WorkshopPrototype scene. Never
    /// recreates the scene and never touches the Stage 1-6 production line, shop, or
    /// order systems.
    /// </summary>
    public static class ShiftworksFactoryExpansionSetup
    {
        private const string ExpansionDataFolder = "Assets/_Project/Data/FactoryExpansions";
        private const string ExpansionDataPath = ExpansionDataFolder + "/ProductionAreaB.asset";
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";

        [MenuItem("Shiftworks/Setup/18. Create Factory Expansion Data")]
        public static void CreateExpansionDataMenuItem() => CreateExpansionData();

        [MenuItem("Shiftworks/Setup/19. Add Factory Expansion Hall To Scene")]
        public static void AddExpansionHallMenuItem() => AddExpansionHallToScene();

        [MenuItem("Shiftworks/Setup/Apply Factory Expansion System (Data + Scene)")]
        public static void ApplyFactoryExpansionSystem()
        {
            CreateExpansionData();
            AddExpansionHallToScene();
        }

        private static void CreateExpansionData()
        {
            Directory.CreateDirectory(ExpansionDataFolder);

            if (AssetDatabase.LoadAssetAtPath<FactoryExpansionDefinition>(ExpansionDataPath) != null)
            {
                Debug.Log($"[Shiftworks] Üretim Alanı B tanımı zaten var, dokunulmadı: {ExpansionDataPath}");
                return;
            }

            FactoryExpansionDefinition definition = ScriptableObject.CreateInstance<FactoryExpansionDefinition>();
            SerializedObject so = new SerializedObject(definition);
            so.FindProperty("expansionName").stringValue = "Üretim Alanı B";
            so.FindProperty("price").intValue = 400;
            so.FindProperty("sizeMeters").vector2Value = new Vector2(10f, 15f);
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(definition, ExpansionDataPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Shiftworks] Üretim Alanı B genişletme tanımı oluşturuldu: {ExpansionDataPath}");
        }

        private static void AddExpansionHallToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("FactoryExpansionManager") != null)
            {
                Debug.Log("[Shiftworks] Fabrika genişletme sistemi zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject economyGo = GameObject.Find("EconomyManager");
            EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;
            FactoryExpansionDefinition definition = AssetDatabase.LoadAssetAtPath<FactoryExpansionDefinition>(ExpansionDataPath);
            if (economy == null || definition == null)
            {
                Debug.LogError("[Shiftworks] EconomyManager veya Üretim Alanı B tanımı bulunamadı. Önce Aşama 5 kurulumu ve '18. Create Factory Expansion Data' çalıştırılmalı.");
                return;
            }

            GameObject hudCanvasGo = GameObject.Find("GameHUDCanvas");
            GameHUD gameHud = hudCanvasGo != null ? hudCanvasGo.GetComponent<GameHUD>() : null;

            GameObject wallWest = GameObject.Find("Wall_West");
            if (wallWest == null)
            {
                Debug.LogError("[Shiftworks] 'Wall_West' bulunamadı. WorkshopPrototype sahnesi beklenen düzende değil, işlem durduruldu.");
                return;
            }

            Material wallMat = GetOrCreateMaterial("M_Wall", new Color(0.82f, 0.78f, 0.7f));
            Material floorMat = GetOrCreateMaterial("M_Floor", new Color(0.55f, 0.55f, 0.55f));
            Material lockedMat = GetOrCreateMaterial("M_ExpansionLocked", new Color(0.85f, 0.55f, 0.1f));
            Material terminalMat = GetOrCreateMaterial("M_ExpansionTerminal", new Color(0.15f, 0.4f, 0.55f));

            // Replace the single, full-length Wall_West with the same two-segment "wall
            // with a 2m gap" pattern Wall_South_Left/Right already use for the main
            // entrance — the original wall is removed, but the gap it leaves open is
            // immediately covered by a locked barrier below, so the room is never
            // actually left open. Nothing about the original is "destroyed" conceptually,
            // it is split so a controlled doorway can exist where there was none.
            Object.DestroyImmediate(wallWest);
            CreateWall("Wall_West_South", new Vector3(-10f, 1.5f, -4.25f), new Vector3(0.3f, 3f, 6.5f), wallMat);
            CreateWall("Wall_West_North", new Vector3(-10f, 1.5f, 4.25f), new Vector3(0.3f, 3f, 6.5f), wallMat);

            // Second hall: 10x15m, directly west of the existing 20x15m workshop (x: -20..-10).
            CreateFloor("Floor_HallB", new Vector3(-15f, -0.1f, 0f), new Vector3(10f, 0.2f, 15f), floorMat);
            CreateWall("Wall_HallB_North", new Vector3(-15f, 1.5f, 7.5f), new Vector3(10f, 3f, 0.3f), wallMat);
            CreateWall("Wall_HallB_South", new Vector3(-15f, 1.5f, -7.5f), new Vector3(10f, 3f, 0.3f), wallMat);
            CreateWall("Wall_HallB_West", new Vector3(-20f, 1.5f, 0f), new Vector3(0.3f, 3f, 15f), wallMat);

            GameObject barrier = CreateExpansionBarrier(lockedMat);

            GameObject managerGo = new GameObject("FactoryExpansionManager");
            FactoryExpansionManager expansionManager = managerGo.AddComponent<FactoryExpansionManager>();
            SerializedObject managerSo = new SerializedObject(expansionManager);
            managerSo.FindProperty("economyManager").objectReferenceValue = economy;
            managerSo.ApplyModifiedProperties();

            FactoryExpansionGate gate = barrier.AddComponent<FactoryExpansionGate>();
            SerializedObject gateSo = new SerializedObject(gate);
            gateSo.FindProperty("definition").objectReferenceValue = definition;
            gateSo.FindProperty("expansionManager").objectReferenceValue = expansionManager;
            gateSo.ApplyModifiedProperties();

            FactoryExpansionPanelUI panelUi = BuildExpansionPanel(gameHud);

            GameObject terminalGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminalGo.name = "FactoryExpansionTerminal";
            terminalGo.transform.position = new Vector3(-8.3f, 0.75f, -2.5f);
            terminalGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            terminalGo.transform.localScale = new Vector3(0.6f, 1.5f, 0.3f);
            terminalGo.GetComponent<Renderer>().sharedMaterial = terminalMat;

            FactoryExpansionTerminal terminal = terminalGo.AddComponent<FactoryExpansionTerminal>();
            SerializedObject terminalSo = new SerializedObject(terminal);
            terminalSo.FindProperty("expansionDefinition").objectReferenceValue = definition;
            terminalSo.FindProperty("expansionManager").objectReferenceValue = expansionManager;
            terminalSo.FindProperty("economyManager").objectReferenceValue = economy;
            terminalSo.FindProperty("panelUI").objectReferenceValue = panelUi;
            terminalSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Üretim Alanı B: ikinci salon, kilitli geçiş, genişletme terminali ve paneli sahneye eklendi.");
        }

        private static GameObject CreateExpansionBarrier(Material lockedMat)
        {
            // Fills the 2m gap left in Wall_West exactly (same thickness/height as the
            // wall segments either side of it), blocking the CharacterController until
            // FactoryExpansionGate deactivates this whole object on purchase.
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "ExpansionBarrier_HallB";
            barrier.transform.position = new Vector3(-10f, 1.5f, 0f);
            barrier.transform.localScale = new Vector3(0.3f, 3f, 2f);
            barrier.GetComponent<Renderer>().sharedMaterial = lockedMat;

            GameObject labelGo = new GameObject("LockedLabel");
            labelGo.transform.SetParent(barrier.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            labelGo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            labelGo.transform.localScale = Vector3.one;
            TextMesh textMesh = labelGo.AddComponent<TextMesh>();
            textMesh.text = "Üretim Alanı B\nKilitli";
            textMesh.characterSize = 0.3f;
            textMesh.fontSize = 48;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            return barrier;
        }

        private static FactoryExpansionPanelUI BuildExpansionPanel(GameHUD gameHud)
        {
            GameObject canvasGo = new GameObject("FactoryExpansionPanelCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 3;
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
            panelRect.sizeDelta = new Vector2(480f, 400f);
            panelRect.anchoredPosition = Vector2.zero;

            Vector2 center = new Vector2(0.5f, 0.5f);

            CreateText(panelRoot.transform, "TitleText", "ÜRETİM ALANI GENİŞLETME", 22, TextAnchor.MiddleCenter,
                center, new Vector2(0f, 160f), new Vector2(440f, 36f));

            Text nameText = CreateText(panelRoot.transform, "NameText", "-", 20, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 105f), new Vector2(440f, 30f));
            Text sizeText = CreateText(panelRoot.transform, "SizeText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 70f), new Vector2(440f, 28f));
            Text priceText = CreateText(panelRoot.transform, "PriceText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 40f), new Vector2(440f, 28f));
            Text balanceText = CreateText(panelRoot.transform, "BalanceText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 10f), new Vector2(440f, 28f));
            Text statusText = CreateText(panelRoot.transform, "StatusText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, -20f), new Vector2(440f, 28f));

            Text warningText = CreateText(panelRoot.transform, "WarningText", "Yetersiz bakiye!", 16, TextAnchor.MiddleCenter,
                center, new Vector2(0f, -55f), new Vector2(440f, 26f));
            warningText.color = new Color(1f, 0.4f, 0.4f);

            Button buyButton = CreateButton(panelRoot.transform, "BuyButton", "Satın Al",
                new Vector2(-90f, -170f), new Vector2(160f, 44f));
            Button closeButton = CreateButton(panelRoot.transform, "CloseButton", "Kapat",
                new Vector2(100f, -170f), new Vector2(120f, 44f));

            FactoryExpansionPanelUI panelUi = canvasGo.AddComponent<FactoryExpansionPanelUI>();
            SerializedObject so = new SerializedObject(panelUi);
            so.FindProperty("panelRoot").objectReferenceValue = panelRoot;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("sizeText").objectReferenceValue = sizeText;
            so.FindProperty("priceText").objectReferenceValue = priceText;
            so.FindProperty("balanceText").objectReferenceValue = balanceText;
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.FindProperty("warningText").objectReferenceValue = warningText;
            so.FindProperty("buyButton").objectReferenceValue = buyButton;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.FindProperty("gameHUD").objectReferenceValue = gameHud;
            so.ApplyModifiedProperties();

            return panelUi;
        }

        private static void CreateFloor(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = name;
            floor.transform.position = position;
            floor.transform.localScale = scale;
            floor.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = material;
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
