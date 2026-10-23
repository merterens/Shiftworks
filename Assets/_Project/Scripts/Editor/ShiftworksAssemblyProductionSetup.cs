using System.IO;
using Shiftworks.Economy;
using Shiftworks.Items;
using Shiftworks.Machines;
using Shiftworks.Orders;
using Shiftworks.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling for Stage 9: creates the AssembledComponent item + its
    /// 2×CutMetalPart→1×AssembledComponent recipe, a standalone AssemblyMachine prefab
    /// (built the same way as Stage 6's CuttingMachine.prefab — reusing MachineController,
    /// never a second production system) + its MachineDefinition (450 $, 5 kW), a second
    /// order ("Montaj Parçası Siparişi") with its own OrderBoard, and a second
    /// MachineShopTerminal selling the assembly machine alongside the existing one. Every
    /// one of these reuses OrderManager/PackingStation/ShippingZone/MachinePurchaseManager/
    /// PowerManager completely unchanged — they were already fully data-driven. Never
    /// recreates the scene or touches the Stage 1-8 cutting line.
    /// </summary>
    public static class ShiftworksAssemblyProductionSetup
    {
        private const string ItemsFolder = "Assets/_Project/Data/Items";
        private const string RecipesFolder = "Assets/_Project/Data/Recipes";
        private const string ItemPrefabsFolder = "Assets/_Project/Prefabs/Items";
        private const string MachinesDataFolder = "Assets/_Project/Data/Machines";
        private const string MachinePrefabFolder = "Assets/_Project/Prefabs/Machines";
        private const string OrdersFolder = "Assets/_Project/Data/Orders";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";

        private const string CutMetalPartItemPath = ItemsFolder + "/CutMetalPart.asset";
        private const string AssembledComponentItemPath = ItemsFolder + "/AssembledComponent.asset";
        private const string AssembledComponentPrefabPath = ItemPrefabsFolder + "/AssembledComponent.prefab";
        private const string AssembleRecipePath = RecipesFolder + "/AssembleComponentRecipe.asset";
        private const string AssemblyMachineDefinitionPath = MachinesDataFolder + "/AssemblyMachineDefinition.asset";
        private const string AssemblyMachinePrefabPath = MachinePrefabFolder + "/AssemblyMachine.prefab";
        private const string AssemblyOrderPath = OrdersFolder + "/AssemblyOrder.asset";

        [MenuItem("Shiftworks/Setup/22. Create Assembly Product Data (Item + Recipe)")]
        public static void CreateAssemblyProductDataMenuItem() => CreateAssemblyProductData();

        [MenuItem("Shiftworks/Setup/23. Create Assembly Machine Definition And Prefab")]
        public static void CreateAssemblyMachineMenuItem() => CreateAssemblyMachineDefinitionAndPrefab();

        [MenuItem("Shiftworks/Setup/24. Add Assembly Order To Scene")]
        public static void AddAssemblyOrderMenuItem() => AddAssemblyOrderToScene();

        [MenuItem("Shiftworks/Setup/25. Add Assembly Machine Shop To Scene")]
        public static void AddAssemblyMachineShopMenuItem() => AddAssemblyMachineShopToScene();

        [MenuItem("Shiftworks/Setup/Apply Assembly Production Chain (Data + Scene)")]
        public static void ApplyAssemblyProductionChain()
        {
            CreateAssemblyProductData();
            CreateAssemblyMachineDefinitionAndPrefab();
            AddAssemblyOrderToScene();
            AddAssemblyMachineShopToScene();
        }

        private static void CreateAssemblyProductData()
        {
            Directory.CreateDirectory(ItemsFolder);
            Directory.CreateDirectory(ItemPrefabsFolder);
            Directory.CreateDirectory(RecipesFolder);

            ItemDefinition cutMetalPart = AssetDatabase.LoadAssetAtPath<ItemDefinition>(CutMetalPartItemPath);
            if (cutMetalPart == null)
            {
                Debug.LogError("[Shiftworks] CutMetalPart ürün tanımı bulunamadı. Önce Aşama 3 kurulum menülerini çalıştırın.");
                return;
            }

            ItemDefinition assembledComponent = AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssembledComponentItemPath);
            bool isNewItem = assembledComponent == null;
            if (isNewItem)
            {
                assembledComponent = ScriptableObject.CreateInstance<ItemDefinition>();
                SerializedObject itemSo = new SerializedObject(assembledComponent);
                itemSo.FindProperty("itemId").stringValue = "assembled_component";
                itemSo.FindProperty("displayName").stringValue = "Montaj Parçası";
                itemSo.FindProperty("category").enumValueIndex = (int)ItemCategory.ProcessedPart;
                itemSo.FindProperty("visualColor").colorValue = new Color(0.5f, 0.55f, 0.75f);
                itemSo.ApplyModifiedProperties();
                AssetDatabase.CreateAsset(assembledComponent, AssembledComponentItemPath);
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AssembledComponentPrefabPath);
            if (prefabAsset == null)
            {
                // Cylinder, not a cube like MetalSheet/CutMetalPart — visually distinct at a
                // glance, as requested, without needing a custom model.
                GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                instance.name = "Montaj Parçası";
                instance.transform.localScale = new Vector3(0.22f, 0.12f, 0.22f);
                instance.GetComponent<Renderer>().sharedMaterial =
                    GetOrCreateMaterial("M_AssembledComponent", new Color(0.5f, 0.55f, 0.75f));

                Rigidbody rb = instance.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                CarryableItem carryable = instance.AddComponent<CarryableItem>();
                SerializedObject carrySo = new SerializedObject(carryable);
                carrySo.FindProperty("definition").objectReferenceValue = assembledComponent;
                carrySo.ApplyModifiedProperties();

                prefabAsset = PrefabUtility.SaveAsPrefabAsset(instance, AssembledComponentPrefabPath);
                Object.DestroyImmediate(instance);
            }

            SerializedObject defSo = new SerializedObject(assembledComponent);
            defSo.FindProperty("worldPrefab").objectReferenceValue = prefabAsset;
            defSo.ApplyModifiedProperties();

            if (AssetDatabase.LoadAssetAtPath<RecipeDefinition>(AssembleRecipePath) == null)
            {
                RecipeDefinition recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
                SerializedObject recipeSo = new SerializedObject(recipe);
                recipeSo.FindProperty("inputItem").objectReferenceValue = cutMetalPart;
                recipeSo.FindProperty("inputCount").intValue = 2;
                recipeSo.FindProperty("outputItem").objectReferenceValue = assembledComponent;
                recipeSo.FindProperty("outputCount").intValue = 1;
                recipeSo.FindProperty("processingTimeSeconds").floatValue = 8f;
                recipeSo.ApplyModifiedProperties();
                AssetDatabase.CreateAsset(recipe, AssembleRecipePath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Shiftworks] Montaj Parçası ürün tanımı, prefab'ı ve tarifi (2×CutMetalPart → 1×AssembledComponent, 8 sn) hazır.");
        }

        private static void CreateAssemblyMachineDefinitionAndPrefab()
        {
            Directory.CreateDirectory(MachinesDataFolder);
            Directory.CreateDirectory(MachinePrefabFolder);

            RecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<RecipeDefinition>(AssembleRecipePath);
            if (recipe == null)
            {
                Debug.LogError("[Shiftworks] Montaj tarifi bulunamadı. Önce '22. Create Assembly Product Data' çalıştırın.");
                return;
            }

            // Distinct shape/color from CuttingMachine's cube-plus-sphere so the two are
            // never confused from across the room.
            Material machineMat = GetOrCreateMaterial("M_AssemblyMachine", new Color(0.35f, 0.25f, 0.5f));
            Material statusMat = GetOrCreateMaterial("M_MachineStatus", Color.red);
            Material armMat = GetOrCreateMaterial("M_Wall", new Color(0.82f, 0.78f, 0.7f));

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AssemblyMachinePrefabPath);
            if (prefabAsset == null)
            {
                prefabAsset = BuildAssemblyMachinePrefab(recipe, machineMat, statusMat, armMat);
            }

            if (AssetDatabase.LoadAssetAtPath<MachineDefinition>(AssemblyMachineDefinitionPath) == null)
            {
                MachineDefinition definition = ScriptableObject.CreateInstance<MachineDefinition>();
                SerializedObject so = new SerializedObject(definition);
                so.FindProperty("machineName").stringValue = "Montaj Makinesi";
                so.FindProperty("price").intValue = 450;
                so.FindProperty("recipe").objectReferenceValue = recipe;
                so.FindProperty("machinePrefab").objectReferenceValue = prefabAsset;
                so.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(definition, AssemblyMachineDefinitionPath);
            }

            // Placement preview materials already exist from Stage 6 (GetOrCreateMaterial
            // finds and reuses them) — MachinePlacementController is shared by every machine
            // type, so no new preview assets are needed here.

            AssetDatabase.SaveAssets();
            Debug.Log("[Shiftworks] Montaj makinesi tanımı ve prefab'ı hazır (450 $, 5 kW).");
        }

        private static GameObject BuildAssemblyMachinePrefab(RecipeDefinition recipe, Material machineMat, Material statusMat, Material armMat)
        {
            Vector3 machineScale = new Vector3(1.3f, 1.6f, 1.3f);
            float topY = machineScale.y / 2f;

            GameObject machineGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            machineGo.name = "AssemblyMachine";
            machineGo.transform.localScale = machineScale;
            machineGo.GetComponent<Renderer>().sharedMaterial = machineMat;

            GameObject statusGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            statusGo.name = "StatusIndicator";
            statusGo.transform.SetParent(machineGo.transform, true);
            statusGo.transform.position = new Vector3(0f, topY + 0.15f, 0f);
            statusGo.transform.localScale = Vector3.one * 0.18f;
            Object.DestroyImmediate(statusGo.GetComponent<Collider>());
            Renderer statusRenderer = statusGo.GetComponent<Renderer>();
            statusRenderer.sharedMaterial = statusMat;

            GameObject animatedGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            animatedGo.name = "AnimatedArm";
            animatedGo.transform.SetParent(machineGo.transform, true);
            animatedGo.transform.position = new Vector3(0.3f, topY + 0.1f, 0f);
            animatedGo.transform.localScale = new Vector3(0.25f, 0.35f, 0.25f);
            Object.DestroyImmediate(animatedGo.GetComponent<Collider>());
            animatedGo.GetComponent<Renderer>().sharedMaterial = armMat;

            GameObject outputPointGo = new GameObject("OutputSpawnPoint");
            outputPointGo.transform.SetParent(machineGo.transform, true);
            outputPointGo.transform.position = new Vector3(0f, topY + 0.05f, -0.7f);

            MachineController machine = machineGo.AddComponent<MachineController>();
            SerializedObject machineSo = new SerializedObject(machine);
            machineSo.FindProperty("recipe").objectReferenceValue = recipe;
            machineSo.FindProperty("outputSpawnPoint").objectReferenceValue = outputPointGo.transform;
            machineSo.FindProperty("statusIndicatorRenderer").objectReferenceValue = statusRenderer;
            machineSo.FindProperty("animatedPart").objectReferenceValue = animatedGo.transform;
            machineSo.FindProperty("powerDemandKw").floatValue = 5f;
            machineSo.ApplyModifiedProperties();
            // powerManager/statsTracker intentionally left unset — see MachineController's
            // class doc: a prefab asset can't hold a scene reference, so this standalone,
            // purchasable instance resolves both via a one-time Awake() lookup at runtime,
            // exactly like the purchasable CuttingMachine.prefab already does.
            // outputReceiverBehaviour intentionally left unset: manual-only, same precedent
            // as the purchasable CuttingMachine — no new conveyor-construction system needed.

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(machineGo, AssemblyMachinePrefabPath);
            Object.DestroyImmediate(machineGo);
            return prefabAsset;
        }

        private static void AddAssemblyOrderToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("AssemblyOrderBoard") != null)
            {
                Debug.Log("[Shiftworks] Montaj sipariş panosu zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject orderManagerGo = GameObject.Find("OrderManager");
            OrderManager orderManager = orderManagerGo != null ? orderManagerGo.GetComponent<OrderManager>() : null;
            if (orderManager == null)
            {
                Debug.LogError("[Shiftworks] OrderManager bulunamadı. Önce Aşama 5 kurulumu çalıştırılmalı.");
                return;
            }

            Directory.CreateDirectory(OrdersFolder);
            ItemDefinition assembledComponent = AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssembledComponentItemPath);
            if (assembledComponent == null)
            {
                Debug.LogError("[Shiftworks] Montaj Parçası ürün tanımı bulunamadı. Önce '22. Create Assembly Product Data' çalıştırın.");
                return;
            }

            OrderDefinition assemblyOrder = AssetDatabase.LoadAssetAtPath<OrderDefinition>(AssemblyOrderPath);
            if (assemblyOrder == null)
            {
                assemblyOrder = ScriptableObject.CreateInstance<OrderDefinition>();
                SerializedObject orderSo = new SerializedObject(assemblyOrder);
                orderSo.FindProperty("orderName").stringValue = "Montaj Parçası Siparişi";
                orderSo.FindProperty("requestedItem").objectReferenceValue = assembledComponent;
                orderSo.FindProperty("requestedQuantity").intValue = 2;
                orderSo.FindProperty("reward").intValue = 350;
                orderSo.FindProperty("timeLimitSeconds").floatValue = 0f;
                orderSo.ApplyModifiedProperties();
                AssetDatabase.CreateAsset(assemblyOrder, AssemblyOrderPath);
                AssetDatabase.SaveAssets();
            }

            // Append to OrderManager's existing list rather than replacing it — the original
            // "İlk Metal Parça Siparişi" stays exactly where it was, at index 0.
            SerializedObject orderManagerSo = new SerializedObject(orderManager);
            SerializedProperty ordersProp = orderManagerSo.FindProperty("availableOrders");
            bool alreadyListed = false;
            for (int i = 0; i < ordersProp.arraySize; i++)
            {
                if (ordersProp.GetArrayElementAtIndex(i).objectReferenceValue == assemblyOrder)
                {
                    alreadyListed = true;
                    break;
                }
            }
            if (!alreadyListed)
            {
                int newIndex = ordersProp.arraySize;
                ordersProp.arraySize++;
                ordersProp.GetArrayElementAtIndex(newIndex).objectReferenceValue = assemblyOrder;
            }
            orderManagerSo.ApplyModifiedProperties();

            OrderBoardPanelUI panelUi = BuildOrderBoardPanel();

            Material boardMat = GetOrCreateMaterial("M_AssemblyOrderBoard", new Color(0.3f, 0.2f, 0.35f));
            GameObject boardGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardGo.name = "AssemblyOrderBoard";
            boardGo.transform.position = new Vector3(5f, 0.75f, -6.5f);
            boardGo.transform.localScale = new Vector3(0.6f, 1.5f, 0.3f);
            boardGo.GetComponent<Renderer>().sharedMaterial = boardMat;

            OrderBoard board = boardGo.AddComponent<OrderBoard>();
            SerializedObject boardSo = new SerializedObject(board);
            boardSo.FindProperty("orderManager").objectReferenceValue = orderManager;
            boardSo.FindProperty("order").objectReferenceValue = assemblyOrder;
            boardSo.FindProperty("panelUI").objectReferenceValue = panelUi;
            boardSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Montaj sipariş panosu sahneye eklendi ve OrderManager'ın sipariş listesine kaydedildi.");
        }

        private static void AddAssemblyMachineShopToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("AssemblyMachineShopTerminal") != null)
            {
                Debug.Log("[Shiftworks] Montaj makinesi mağaza terminali zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject purchaseManagerGo = GameObject.Find("MachinePurchaseManager");
            GameObject economyGo = GameObject.Find("EconomyManager");
            MachinePurchaseManager purchaseManager = purchaseManagerGo != null ? purchaseManagerGo.GetComponent<MachinePurchaseManager>() : null;
            EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;
            MachineDefinition definition = AssetDatabase.LoadAssetAtPath<MachineDefinition>(AssemblyMachineDefinitionPath);

            if (purchaseManager == null || economy == null || definition == null)
            {
                Debug.LogError("[Shiftworks] MachinePurchaseManager, EconomyManager veya montaj makinesi tanımı bulunamadı. " +
                               "Önce Aşama 6 kurulumu ve '23. Create Assembly Machine Definition And Prefab' çalıştırılmalı.");
                return;
            }

            MachineShopPanelUI panelUi = BuildMachineShopPanel();

            Material terminalMat = GetOrCreateMaterial("M_AssemblyShopTerminal", new Color(0.4f, 0.2f, 0.5f));
            GameObject terminalGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminalGo.name = "AssemblyMachineShopTerminal";
            terminalGo.transform.position = new Vector3(-5f, 0.75f, -6.5f);
            terminalGo.transform.localScale = new Vector3(0.6f, 1.5f, 0.3f);
            terminalGo.GetComponent<Renderer>().sharedMaterial = terminalMat;

            MachineShopTerminal terminal = terminalGo.AddComponent<MachineShopTerminal>();
            SerializedObject terminalSo = new SerializedObject(terminal);
            terminalSo.FindProperty("machineDefinition").objectReferenceValue = definition;
            // Shares the SAME MachinePurchaseManager as the original cutting-machine shop —
            // it already keys its pending-purchase bookkeeping per MachineDefinition
            // (Dictionary<MachineDefinition,int>), so two machine types coexist with zero
            // changes to that class.
            terminalSo.FindProperty("purchaseManager").objectReferenceValue = purchaseManager;
            terminalSo.FindProperty("economyManager").objectReferenceValue = economy;
            terminalSo.FindProperty("panelUI").objectReferenceValue = panelUi;
            terminalSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Montaj makinesi mağaza terminali sahneye eklendi (mevcut MachinePurchaseManager paylaşılıyor).");
        }

        private static OrderBoardPanelUI BuildOrderBoardPanel()
        {
            GameObject canvasGo = new GameObject("AssemblyOrderBoardPanelCanvas");
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

            CreateText(panelRoot.transform, "TitleText", "MONTAJ SİPARİŞ PANOSU", 24, TextAnchor.MiddleCenter,
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

        private static MachineShopPanelUI BuildMachineShopPanel()
        {
            GameObject canvasGo = new GameObject("AssemblyMachineShopPanelCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2;
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
            panelRect.sizeDelta = new Vector2(480f, 420f);
            panelRect.anchoredPosition = Vector2.zero;

            Vector2 center = new Vector2(0.5f, 0.5f);

            CreateText(panelRoot.transform, "TitleText", "MONTAJ MAKİNESİ MAĞAZASI", 24, TextAnchor.MiddleCenter,
                center, center, new Vector2(0f, 170f), new Vector2(440f, 36f));

            Text nameText = CreateText(panelRoot.transform, "MachineNameText", "-", 20, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, 115f), new Vector2(440f, 30f));
            Text priceText = CreateText(panelRoot.transform, "PriceText", "-", 18, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, 80f), new Vector2(440f, 28f));
            Text producesText = CreateText(panelRoot.transform, "ProducesText", "-", 18, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, 50f), new Vector2(440f, 28f));
            Text balanceText = CreateText(panelRoot.transform, "BalanceText", "-", 18, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, 20f), new Vector2(440f, 28f));
            Text pendingText = CreateText(panelRoot.transform, "PendingText", "-", 18, TextAnchor.MiddleLeft,
                center, center, new Vector2(0f, -10f), new Vector2(440f, 28f));

            Text warningText = CreateText(panelRoot.transform, "WarningText", "Yetersiz bakiye!", 16, TextAnchor.MiddleCenter,
                center, center, new Vector2(0f, -45f), new Vector2(440f, 26f));
            warningText.color = new Color(1f, 0.4f, 0.4f);

            Button buyButton = CreateButton(panelRoot.transform, "BuyButton", "Satın Al",
                new Vector2(-130f, -170f), new Vector2(150f, 44f));
            Button placeButton = CreateButton(panelRoot.transform, "PlaceButton", "Yerleştir",
                new Vector2(40f, -170f), new Vector2(150f, 44f));
            Button closeButton = CreateButton(panelRoot.transform, "CloseButton", "Kapat",
                new Vector2(195f, -170f), new Vector2(100f, 44f));

            MachineShopPanelUI panelUi = canvasGo.AddComponent<MachineShopPanelUI>();
            SerializedObject so = new SerializedObject(panelUi);
            so.FindProperty("panelRoot").objectReferenceValue = panelRoot;
            so.FindProperty("machineNameText").objectReferenceValue = nameText;
            so.FindProperty("priceText").objectReferenceValue = priceText;
            so.FindProperty("producesText").objectReferenceValue = producesText;
            so.FindProperty("balanceText").objectReferenceValue = balanceText;
            so.FindProperty("pendingText").objectReferenceValue = pendingText;
            so.FindProperty("warningText").objectReferenceValue = warningText;
            so.FindProperty("buyButton").objectReferenceValue = buyButton;
            so.FindProperty("placeButton").objectReferenceValue = placeButton;
            so.FindProperty("closeButton").objectReferenceValue = closeButton;
            so.ApplyModifiedProperties();

            return panelUi;
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
