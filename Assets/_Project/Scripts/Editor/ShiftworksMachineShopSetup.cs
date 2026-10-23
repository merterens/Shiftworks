using System.IO;
using Shiftworks.Economy;
using Shiftworks.Items;
using Shiftworks.Machines;
using Shiftworks.Player;
using Shiftworks.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling for Stage 6: creates the "Kesim Makinesi" MachineDefinition
    /// + a standalone CuttingMachine prefab (built the same way as Stage 3's scene machine,
    /// reusing MachineController — never a second production system), adds
    /// MachinePlacementController to the existing Player prefab in-place, and adds the
    /// purchase manager + shop terminal + panel to the existing WorkshopPrototype scene.
    /// Never recreates the scene or touches the original Stage 3 machine.
    /// </summary>
    public static class ShiftworksMachineShopSetup
    {
        private const string MachinesDataFolder = "Assets/_Project/Data/Machines";
        private const string MachineDefinitionPath = MachinesDataFolder + "/CuttingMachineDefinition.asset";
        private const string MachinePrefabFolder = "Assets/_Project/Prefabs/Machines";
        private const string MachinePrefabPath = MachinePrefabFolder + "/CuttingMachine.prefab";
        private const string RecipePath = "Assets/_Project/Data/Recipes/CutMetalRecipe.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";

        [MenuItem("Shiftworks/Setup/12. Create Machine Definition And Prefab")]
        public static void CreateMachineDefinitionMenuItem() => CreateMachineDefinitionAndPrefab();

        [MenuItem("Shiftworks/Setup/13. Update Player Prefab (Machine Placement)")]
        public static void UpdatePlayerPrefabMenuItem() => UpdatePlayerPrefabForPlacement();

        [MenuItem("Shiftworks/Setup/14. Add Machine Shop To Scene")]
        public static void AddMachineShopMenuItem() => AddMachineShopToScene();

        [MenuItem("Shiftworks/Setup/Apply Machine Shop System (Definition + Prefab + Scene)")]
        public static void ApplyMachineShopSystem()
        {
            CreateMachineDefinitionAndPrefab();
            UpdatePlayerPrefabForPlacement();
            AddMachineShopToScene();
        }

        private static void CreateMachineDefinitionAndPrefab()
        {
            Directory.CreateDirectory(MachinesDataFolder);
            Directory.CreateDirectory(MachinePrefabFolder);

            RecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<RecipeDefinition>(RecipePath);
            if (recipe == null)
            {
                Debug.LogError("[Shiftworks] CutMetalRecipe bulunamadı. Önce Aşama 3 kurulum menülerini çalıştırın.");
                return;
            }

            Material machineMat = GetOrCreateMaterial("M_Machine", new Color(0.3f, 0.35f, 0.4f));
            Material statusMat = GetOrCreateMaterial("M_MachineStatus", Color.red);
            Material wallMat = GetOrCreateMaterial("M_Wall", new Color(0.82f, 0.78f, 0.7f));

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MachinePrefabPath);
            if (prefabAsset == null)
            {
                prefabAsset = BuildMachinePrefab(recipe, machineMat, statusMat, wallMat);
            }

            if (AssetDatabase.LoadAssetAtPath<MachineDefinition>(MachineDefinitionPath) == null)
            {
                MachineDefinition definition = ScriptableObject.CreateInstance<MachineDefinition>();
                SerializedObject so = new SerializedObject(definition);
                so.FindProperty("machineName").stringValue = "Kesim Makinesi";
                so.FindProperty("price").intValue = 300;
                so.FindProperty("recipe").objectReferenceValue = recipe;
                so.FindProperty("machinePrefab").objectReferenceValue = prefabAsset;
                so.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(definition, MachineDefinitionPath);
            }

            CreateTransparentMaterial("M_PlacementValid", new Color(0.2f, 1f, 0.2f, 0.45f));
            CreateTransparentMaterial("M_PlacementInvalid", new Color(1f, 0.2f, 0.2f, 0.45f));

            AssetDatabase.SaveAssets();
            Debug.Log("[Shiftworks] Makine tanımı, prefab'ı ve önizleme materyalleri hazır.");
        }

        private static GameObject BuildMachinePrefab(RecipeDefinition recipe, Material machineMat, Material statusMat, Material wallMat)
        {
            Vector3 machineScale = new Vector3(1.4f, 1.4f, 1.2f);
            float topY = machineScale.y / 2f;

            GameObject machineGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            machineGo.name = "CuttingMachine";
            machineGo.transform.localScale = machineScale;
            machineGo.GetComponent<Renderer>().sharedMaterial = machineMat;

            GameObject statusGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            statusGo.name = "StatusIndicator";
            statusGo.transform.SetParent(machineGo.transform, true);
            statusGo.transform.position = new Vector3(0f, topY + 0.15f, 0f);
            statusGo.transform.localScale = Vector3.one * 0.2f;
            Object.DestroyImmediate(statusGo.GetComponent<Collider>());
            Renderer statusRenderer = statusGo.GetComponent<Renderer>();
            statusRenderer.sharedMaterial = statusMat;

            GameObject animatedGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            animatedGo.name = "AnimatedBlade";
            animatedGo.transform.SetParent(machineGo.transform, true);
            animatedGo.transform.position = new Vector3(-0.35f, topY + 0.2f, 0f);
            animatedGo.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            Object.DestroyImmediate(animatedGo.GetComponent<Collider>());
            animatedGo.GetComponent<Renderer>().sharedMaterial = wallMat;

            GameObject outputPointGo = new GameObject("OutputSpawnPoint");
            outputPointGo.transform.SetParent(machineGo.transform, true);
            outputPointGo.transform.position = new Vector3(0f, topY + 0.05f, -0.75f);

            MachineController machine = machineGo.AddComponent<MachineController>();
            SerializedObject machineSo = new SerializedObject(machine);
            machineSo.FindProperty("recipe").objectReferenceValue = recipe;
            machineSo.FindProperty("outputSpawnPoint").objectReferenceValue = outputPointGo.transform;
            machineSo.FindProperty("statusIndicatorRenderer").objectReferenceValue = statusRenderer;
            machineSo.FindProperty("animatedPart").objectReferenceValue = animatedGo.transform;
            machineSo.ApplyModifiedProperties();
            // outputReceiverBehaviour intentionally left unset: manual-only, same as Stage 3.

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(machineGo, MachinePrefabPath);
            Object.DestroyImmediate(machineGo);
            return prefabAsset;
        }

        private static void UpdatePlayerPrefabForPlacement()
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError($"[Shiftworks] Player prefab bulunamadı: {PlayerPrefabPath}");
                return;
            }

            Material validMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/M_PlacementValid.mat");
            Material invalidMat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/M_PlacementInvalid.mat");
            if (validMat == null || invalidMat == null)
            {
                Debug.LogError("[Shiftworks] Önizleme materyalleri bulunamadı. Önce '12. Create Machine Definition And Prefab' çalıştırın.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                Transform cameraPivot = root.transform.Find("CameraPivot");
                Transform cameraTransform = cameraPivot != null ? cameraPivot.Find("PlayerCamera") : null;
                if (cameraTransform == null)
                {
                    Debug.LogError("[Shiftworks] Player prefabında CameraPivot/PlayerCamera bulunamadı, güncelleme atlandı.");
                    return;
                }

                Transform canvasTransform = root.transform.Find("InteractionPromptCanvas");
                InteractionPromptUI promptUi = canvasTransform != null ? canvasTransform.GetComponent<InteractionPromptUI>() : null;

                MachinePlacementController placement = root.GetComponent<MachinePlacementController>();
                if (placement == null)
                {
                    placement = root.AddComponent<MachinePlacementController>();
                }

                SerializedObject so = new SerializedObject(placement);
                so.FindProperty("cameraTransform").objectReferenceValue = cameraTransform;
                so.FindProperty("promptUI").objectReferenceValue = promptUi;
                so.FindProperty("validPreviewMaterial").objectReferenceValue = validMat;
                so.FindProperty("invalidPreviewMaterial").objectReferenceValue = invalidMat;
                so.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
                Debug.Log("[Shiftworks] Player prefab makine yerleştirme sistemiyle güncellendi.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AddMachineShopToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("MachineShopTerminal") != null)
            {
                Debug.Log("[Shiftworks] Makine mağazası zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject economyGo = GameObject.Find("EconomyManager");
            EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;
            MachineDefinition definition = AssetDatabase.LoadAssetAtPath<MachineDefinition>(MachineDefinitionPath);
            if (economy == null || definition == null)
            {
                Debug.LogError("[Shiftworks] EconomyManager veya makine tanımı bulunamadı. Önce Aşama 5 ve '12. Create Machine Definition And Prefab' çalıştırılmalı.");
                return;
            }

            GameObject purchaseManagerGo = new GameObject("MachinePurchaseManager");
            MachinePurchaseManager purchaseManager = purchaseManagerGo.AddComponent<MachinePurchaseManager>();
            SerializedObject purchaseSo = new SerializedObject(purchaseManager);
            purchaseSo.FindProperty("economyManager").objectReferenceValue = economy;
            purchaseSo.ApplyModifiedProperties();

            MachineShopPanelUI panelUi = BuildShopPanel();

            Material terminalMat = GetOrCreateMaterial("M_ShopTerminal", new Color(0.15f, 0.4f, 0.55f));
            GameObject terminalGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terminalGo.name = "MachineShopTerminal";
            terminalGo.transform.position = new Vector3(-2.5f, 0.75f, -6.5f);
            terminalGo.transform.localScale = new Vector3(0.6f, 1.5f, 0.3f);
            terminalGo.GetComponent<Renderer>().sharedMaterial = terminalMat;

            MachineShopTerminal terminal = terminalGo.AddComponent<MachineShopTerminal>();
            SerializedObject terminalSo = new SerializedObject(terminal);
            terminalSo.FindProperty("machineDefinition").objectReferenceValue = definition;
            terminalSo.FindProperty("purchaseManager").objectReferenceValue = purchaseManager;
            terminalSo.FindProperty("economyManager").objectReferenceValue = economy;
            terminalSo.FindProperty("panelUI").objectReferenceValue = panelUi;
            terminalSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Makine mağazası terminali ve satın alma yöneticisi sahneye eklendi.");
        }

        private static MachineShopPanelUI BuildShopPanel()
        {
            GameObject canvasGo = new GameObject("MachineShopPanelCanvas");
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

            CreateText(panelRoot.transform, "TitleText", "MAKİNE MAĞAZASI", 24, TextAnchor.MiddleCenter,
                center, new Vector2(0f, 170f), new Vector2(440f, 36f));

            Text nameText = CreateText(panelRoot.transform, "MachineNameText", "-", 20, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 115f), new Vector2(440f, 30f));
            Text priceText = CreateText(panelRoot.transform, "PriceText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 80f), new Vector2(440f, 28f));
            Text producesText = CreateText(panelRoot.transform, "ProducesText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 50f), new Vector2(440f, 28f));
            Text balanceText = CreateText(panelRoot.transform, "BalanceText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, 20f), new Vector2(440f, 28f));
            Text pendingText = CreateText(panelRoot.transform, "PendingText", "-", 18, TextAnchor.MiddleLeft,
                center, new Vector2(0f, -10f), new Vector2(440f, 28f));

            Text warningText = CreateText(panelRoot.transform, "WarningText", "Yetersiz bakiye!", 16, TextAnchor.MiddleCenter,
                center, new Vector2(0f, -45f), new Vector2(440f, 26f));
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
            // Pivot matches the anchor point so the element extends from it rather than
            // being centered on it (the bug that made Stage 5's HUD render off-screen).
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

        private static void CreateTransparentMaterial(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            {
                return;
            }

            Directory.CreateDirectory(MaterialsFolder);
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetFloat("_Surface", 1f); // Transparent
            material.SetFloat("_Blend", 0f); // Alpha
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.color = color;

            AssetDatabase.CreateAsset(material, path);
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
