using System.IO;
using Shiftworks.Economy;
using Shiftworks.Items;
using Shiftworks.Orders;
using Shiftworks.UI;
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
    /// Idempotent editor tooling for Stage 5: creates the test order + shipment package
    /// prefab, then adds EconomyManager, OrderManager, the HUD, the order board (+ its UI
    /// panel), the packing station and the shipping zone to the existing WorkshopPrototype
    /// scene. Re-running is a safe no-op once "OrderManager" already exists in the scene.
    /// </summary>
    public static class ShiftworksOrdersSetup
    {
        private const string OrderDataFolder = "Assets/_Project/Data/Orders";
        private const string OrderDataPath = OrderDataFolder + "/FirstMetalPartOrder.asset";
        private const string ShipmentPackagePrefabPath = "Assets/_Project/Prefabs/Items/ShipmentPackage.prefab";
        private const string CutMetalPartItemPath = "Assets/_Project/Data/Items/CutMetalPart.asset";
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";

        [MenuItem("Shiftworks/Setup/9. Create Order Data")]
        public static void CreateOrderDataMenuItem() => CreateOrderData();

        [MenuItem("Shiftworks/Setup/10. Add Economy And Order Systems To Scene")]
        public static void AddOrderSystemsMenuItem() => AddOrderSystemsToScene();

        [MenuItem("Shiftworks/Setup/Apply Order And Economy System (Data + Scene)")]
        public static void ApplyOrderSystem()
        {
            CreateOrderData();
            AddOrderSystemsToScene();
        }

        [MenuItem("Shiftworks/Setup/11. Fix HUD Visibility (Pivot + Scaler)")]
        public static void FixHudVisibilityMenuItem() => FixHudVisibility();

        /// <summary>
        /// Repairs the HUD/panel canvases already built by menu 10 in-place: corrects each
        /// top/corner-anchored Text's RectTransform.pivot to match its anchor (the bug that
        /// made BalanceText/OrderText render mostly off-screen) and switches both canvases'
        /// CanvasScaler to Scale With Screen Size so they stay fully visible across
        /// resolutions. Never recreates or removes anything; safe to re-run.
        /// </summary>
        private static void FixHudVisibility()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject hudCanvasGo = GameObject.Find("GameHUDCanvas");
            if (hudCanvasGo == null)
            {
                Debug.LogError("[Shiftworks] 'GameHUDCanvas' bulunamadı. Önce '10. Add Economy And Order Systems To Scene' çalıştırılmalı.");
                return;
            }

            ConfigureScaler(hudCanvasGo);
            FixChildPivot(hudCanvasGo.transform, "BalanceText", new Vector2(0f, 1f));
            FixChildPivot(hudCanvasGo.transform, "OrderText", new Vector2(0f, 1f));
            FixChildPivot(hudCanvasGo.transform, "NotificationText", new Vector2(0.5f, 1f));

            GameObject panelCanvasGo = GameObject.Find("OrderBoardPanelCanvas");
            if (panelCanvasGo != null)
            {
                ConfigureScaler(panelCanvasGo);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[Shiftworks] HUD pivot/scaler düzeltmesi uygulandı (GameHUDCanvas, OrderBoardPanelCanvas).");
        }

        private static void ConfigureScaler(GameObject canvasGo)
        {
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static void FixChildPivot(Transform parent, string childName, Vector2 expectedAnchor)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                Debug.LogWarning($"[Shiftworks] '{childName}' bulunamadı, pivot düzeltmesi atlandı.");
                return;
            }

            RectTransform rect = child as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = expectedAnchor;
            rect.anchorMax = expectedAnchor;
            rect.pivot = expectedAnchor;
        }

        private static void CreateOrderData()
        {
            Directory.CreateDirectory(OrderDataFolder);
            Directory.CreateDirectory("Assets/_Project/Prefabs/Items");

            ItemDefinition cutMetalPart = AssetDatabase.LoadAssetAtPath<ItemDefinition>(CutMetalPartItemPath);
            if (cutMetalPart == null)
            {
                Debug.LogError("[Shiftworks] CutMetalPart ürün tanımı bulunamadı. Önce Aşama 3 kurulum menülerini çalıştırın.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(ShipmentPackagePrefabPath) == null)
            {
                GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                instance.name = "ShipmentPackage";
                instance.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
                instance.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("M_ShipmentPackage", new Color(0.55f, 0.4f, 0.2f));

                Rigidbody rb = instance.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                CarryableItem carryable = instance.AddComponent<CarryableItem>();
                SerializedObject carrySo = new SerializedObject(carryable);
                carrySo.FindProperty("displayName").stringValue = "Sevkiyat Paketi";
                carrySo.ApplyModifiedProperties();

                instance.AddComponent<ShipmentPackage>();

                PrefabUtility.SaveAsPrefabAsset(instance, ShipmentPackagePrefabPath);
                Object.DestroyImmediate(instance);
            }

            if (AssetDatabase.LoadAssetAtPath<OrderDefinition>(OrderDataPath) == null)
            {
                OrderDefinition order = ScriptableObject.CreateInstance<OrderDefinition>();
                SerializedObject so = new SerializedObject(order);
                so.FindProperty("orderName").stringValue = "İlk Metal Parça Siparişi";
                so.FindProperty("requestedItem").objectReferenceValue = cutMetalPart;
                so.FindProperty("requestedQuantity").intValue = 3;
                so.FindProperty("reward").intValue = 150;
                so.FindProperty("timeLimitSeconds").floatValue = 0f;
                so.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(order, OrderDataPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Shiftworks] Sipariş verisi ve sevkiyat paketi prefab'ı hazır.");
        }

        private static void AddOrderSystemsToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("OrderManager") != null)
            {
                Debug.Log("[Shiftworks] Sipariş/ekonomi sistemleri zaten sahnede var, dokunulmadı.");
                return;
            }

            OrderDefinition order = AssetDatabase.LoadAssetAtPath<OrderDefinition>(OrderDataPath);
            GameObject packagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShipmentPackagePrefabPath);
            if (order == null || packagePrefab == null)
            {
                Debug.LogError("[Shiftworks] Önce '9. Create Order Data' menüsünü çalıştırın.");
                return;
            }

            EnsureEventSystem();

            GameObject economyGo = new GameObject("EconomyManager");
            EconomyManager economy = economyGo.AddComponent<EconomyManager>();
            SerializedObject economySo = new SerializedObject(economy);
            economySo.FindProperty("startingBalance").intValue = 500;
            economySo.ApplyModifiedProperties();

            GameObject orderManagerGo = new GameObject("OrderManager");
            OrderManager orderManager = orderManagerGo.AddComponent<OrderManager>();
            SerializedObject orderManagerSo = new SerializedObject(orderManager);
            orderManagerSo.FindProperty("economyManager").objectReferenceValue = economy;
            SerializedProperty ordersProp = orderManagerSo.FindProperty("availableOrders");
            ordersProp.arraySize = 1;
            ordersProp.GetArrayElementAtIndex(0).objectReferenceValue = order;
            orderManagerSo.ApplyModifiedProperties();

            BuildGameHud(economy, orderManager, order);

            OrderBoardPanelUI panelUi = BuildOrderBoardPanel();
            BuildOrderBoard(orderManager, order, panelUi);

            Material stationMat = GetOrCreateMaterial("M_PackingStation", new Color(0.35f, 0.3f, 0.5f));
            GameObject stationGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stationGo.name = "PackingStation";
            stationGo.transform.position = new Vector3(3.5f, 0.4f, 1.5f);
            stationGo.transform.localScale = new Vector3(1.2f, 0.8f, 0.8f);
            stationGo.GetComponent<Renderer>().sharedMaterial = stationMat;

            GameObject packageSpawnGo = new GameObject("PackageSpawnPoint");
            packageSpawnGo.transform.SetParent(stationGo.transform, true);
            packageSpawnGo.transform.position = new Vector3(3.5f, 0.85f, 1.5f);

            PackingStation station = stationGo.AddComponent<PackingStation>();
            SerializedObject stationSo = new SerializedObject(station);
            stationSo.FindProperty("orderManager").objectReferenceValue = orderManager;
            stationSo.FindProperty("packagePrefab").objectReferenceValue = packagePrefab;
            stationSo.FindProperty("packageSpawnPoint").objectReferenceValue = packageSpawnGo.transform;
            stationSo.ApplyModifiedProperties();

            Material zoneMat = GetOrCreateMaterial("M_ShippingZone", new Color(0.8f, 0.7f, 0.1f));
            GameObject zoneGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zoneGo.name = "ShippingZone";
            zoneGo.transform.position = new Vector3(2.0f, 0.05f, -1.0f);
            zoneGo.transform.localScale = new Vector3(2.0f, 0.1f, 2.0f);
            zoneGo.GetComponent<Renderer>().sharedMaterial = zoneMat;

            ShippingZone zone = zoneGo.AddComponent<ShippingZone>();
            SerializedObject zoneSo = new SerializedObject(zone);
            zoneSo.FindProperty("orderManager").objectReferenceValue = orderManager;
            zoneSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Sipariş panosu, paketleme istasyonu, teslimat alanı, ekonomi ve HUD sahneye eklendi.");
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

        private static void BuildGameHud(EconomyManager economy, OrderManager orderManager, OrderDefinition order)
        {
            GameObject canvasGo = new GameObject("GameHUDCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;
            canvasGo.AddComponent<CanvasScaler>();

            Vector2 topLeft = new Vector2(0f, 1f);

            Text balanceText = CreateText(canvasGo.transform, "BalanceText", "Bakiye: 500 $", 22, TextAnchor.UpperLeft,
                topLeft, topLeft, new Vector2(24f, -24f), new Vector2(320f, 32f));

            Text orderText = CreateText(canvasGo.transform, "OrderText", "Aktif sipariş yok", 18, TextAnchor.UpperLeft,
                topLeft, topLeft, new Vector2(24f, -60f), new Vector2(420f, 28f));

            Vector2 topCenter = new Vector2(0.5f, 1f);
            Text notificationText = CreateText(canvasGo.transform, "NotificationText", "Sipariş Tamamlandı!", 28,
                TextAnchor.MiddleCenter, topCenter, topCenter, new Vector2(0f, -100f), new Vector2(500f, 50f));
            notificationText.enabled = false;

            GameHUD hud = canvasGo.AddComponent<GameHUD>();
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("economyManager").objectReferenceValue = economy;
            so.FindProperty("orderManager").objectReferenceValue = orderManager;
            so.FindProperty("trackedOrder").objectReferenceValue = order;
            so.FindProperty("balanceText").objectReferenceValue = balanceText;
            so.FindProperty("orderText").objectReferenceValue = orderText;
            so.FindProperty("notificationText").objectReferenceValue = notificationText;
            so.ApplyModifiedProperties();
        }

        private static OrderBoardPanelUI BuildOrderBoardPanel()
        {
            GameObject canvasGo = new GameObject("OrderBoardPanelCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(canvasGo.transform, false);
            Image bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
            RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(480f, 380f);
            panelRect.anchoredPosition = Vector2.zero;

            Vector2 center = new Vector2(0.5f, 0.5f);

            CreateText(panelRoot.transform, "TitleText", "SİPARİŞ PANOSU", 24, TextAnchor.MiddleCenter,
                center, center, new Vector2(0f, 150f), new Vector2(440f, 36f));

            Text orderNameText = CreateText(panelRoot.transform, "OrderNameText", "-", 20, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, 90f), new Vector2(440f, 30f));
            Text itemText = CreateText(panelRoot.transform, "ItemText", "-", 18, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, 55f), new Vector2(440f, 28f));
            Text quantityText = CreateText(panelRoot.transform, "QuantityText", "-", 18, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, 25f), new Vector2(440f, 28f));
            Text rewardText = CreateText(panelRoot.transform, "RewardText", "-", 18, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, -5f), new Vector2(440f, 28f));
            Text stateText = CreateText(panelRoot.transform, "StateText", "-", 18, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, -35f), new Vector2(440f, 28f));

            Button acceptButton = CreateButton(panelRoot.transform, "AcceptButton", "Siparişi Kabul Et",
                new Vector2(-100f, -140f), new Vector2(180f, 44f));
            Button closeButton = CreateButton(panelRoot.transform, "CloseButton", "Kapat",
                new Vector2(120f, -140f), new Vector2(120f, 44f));

            OrderBoardPanelUI panelUi = canvasGo.AddComponent<OrderBoardPanelUI>();
            SerializedObject so = new SerializedObject(panelUi);
            so.FindProperty("panelRoot").objectReferenceValue = panelRoot;
            so.FindProperty("orderNameText").objectReferenceValue = orderNameText;
            so.FindProperty("itemText").objectReferenceValue = itemText;
            so.FindProperty("quantityText").objectReferenceValue = quantityText;
            so.FindProperty("rewardText").objectReferenceValue = rewardText;
            so.FindProperty("stateText").objectReferenceValue = stateText;
            so.FindProperty("acceptButton").objectReferenceValue = acceptButton;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.ApplyModifiedProperties();

            return panelUi;
        }

        private static void BuildOrderBoard(OrderManager orderManager, OrderDefinition order, OrderBoardPanelUI panelUi)
        {
            Material boardMat = GetOrCreateMaterial("M_OrderBoard", new Color(0.2f, 0.2f, 0.25f));
            GameObject boardGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardGo.name = "OrderBoard";
            boardGo.transform.position = new Vector3(2.5f, 0.75f, -6.5f);
            boardGo.transform.localScale = new Vector3(0.6f, 1.5f, 0.3f);
            boardGo.GetComponent<Renderer>().sharedMaterial = boardMat;

            OrderBoard board = boardGo.AddComponent<OrderBoard>();
            SerializedObject so = new SerializedObject(board);
            so.FindProperty("orderManager").objectReferenceValue = orderManager;
            so.FindProperty("order").objectReferenceValue = order;
            so.FindProperty("panelUI").objectReferenceValue = panelUi;
            so.ApplyModifiedProperties();
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
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
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            // Pivot must match a point anchor (anchorMin == anchorMax), otherwise the box
            // is centered ON the anchor point instead of extending from it — e.g. a
            // top-left anchor with the default (0.5,0.5) pivot pushes half the element off
            // screen, up and to the left. Matching anchors always keep the element on-screen.
            rect.pivot = anchorMin;
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
