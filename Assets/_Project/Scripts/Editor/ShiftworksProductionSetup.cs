using System.IO;
using Shiftworks.Items;
using Shiftworks.Machines;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling for Stage 3: creates the MetalSheet/CutMetalPart item
    /// definitions + prefabs, the cutting recipe, and adds one cutting machine (plus a
    /// parts table and 5 MetalSheet instances) to the existing WorkshopPrototype scene.
    /// Every step checks for an existing asset/object by path or name first; re-running
    /// is always a safe no-op where things already exist.
    /// </summary>
    public static class ShiftworksProductionSetup
    {
        private const string ItemsFolder = "Assets/_Project/Data/Items";
        private const string RecipesFolder = "Assets/_Project/Data/Recipes";
        private const string ItemPrefabsFolder = "Assets/_Project/Prefabs/Items";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";

        private const string MetalSheetItemPath = ItemsFolder + "/MetalSheet.asset";
        private const string CutMetalPartItemPath = ItemsFolder + "/CutMetalPart.asset";
        private const string MetalSheetPrefabPath = ItemPrefabsFolder + "/MetalSheet.prefab";
        private const string CutMetalPartPrefabPath = ItemPrefabsFolder + "/CutMetalPart.prefab";
        private const string RecipePath = RecipesFolder + "/CutMetalRecipe.asset";

        [MenuItem("Shiftworks/Setup/5. Create Item Definitions")]
        public static void CreateItemDefinitionsMenuItem() => CreateItemDefinitions();

        [MenuItem("Shiftworks/Setup/6. Create Cutting Recipe")]
        public static void CreateCuttingRecipeMenuItem() => CreateCuttingRecipe();

        [MenuItem("Shiftworks/Setup/7. Add Cutting Machine To Workshop Scene")]
        public static void AddCuttingMachineMenuItem() => AddCuttingMachineToScene();

        [MenuItem("Shiftworks/Setup/Apply Production System (Items + Recipe + Scene)")]
        public static void ApplyProductionSystem()
        {
            CreateItemDefinitions();
            CreateCuttingRecipe();
            AddCuttingMachineToScene();
        }

        private static void CreateItemDefinitions()
        {
            Directory.CreateDirectory(ItemsFolder);
            Directory.CreateDirectory(ItemPrefabsFolder);

            CreateItemAndPrefab(
                MetalSheetItemPath, "metal_sheet", "Metal Levha", ItemCategory.RawMaterial,
                new Color(0.75f, 0.76f, 0.78f), "M_MetalSheet",
                MetalSheetPrefabPath, new Vector3(0.6f, 0.05f, 0.4f));

            CreateItemAndPrefab(
                CutMetalPartItemPath, "cut_metal_part", "Kesilmiş Metal Parça", ItemCategory.ProcessedPart,
                new Color(0.85f, 0.35f, 0.1f), "M_CutMetalPart",
                CutMetalPartPrefabPath, new Vector3(0.25f, 0.12f, 0.25f));

            AssetDatabase.SaveAssets();
            Debug.Log("[Shiftworks] Ürün tanımları ve prefab'ları hazır.");
        }

        private static void CreateItemAndPrefab(string itemPath, string itemId, string displayName, ItemCategory category,
            Color color, string materialName, string prefabPath, Vector3 scale)
        {
            ItemDefinition definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemPath);
            bool isNewDefinition = definition == null;
            if (isNewDefinition)
            {
                definition = ScriptableObject.CreateInstance<ItemDefinition>();
                SerializedObject itemSo = new SerializedObject(definition);
                itemSo.FindProperty("itemId").stringValue = itemId;
                itemSo.FindProperty("displayName").stringValue = displayName;
                itemSo.FindProperty("category").enumValueIndex = (int)category;
                itemSo.FindProperty("visualColor").colorValue = color;
                itemSo.ApplyModifiedProperties();
                AssetDatabase.CreateAsset(definition, itemPath);
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                instance.name = displayName;
                instance.transform.localScale = scale;
                instance.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial(materialName, color);

                Rigidbody rb = instance.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                CarryableItem carryable = instance.AddComponent<CarryableItem>();
                SerializedObject carrySo = new SerializedObject(carryable);
                carrySo.FindProperty("definition").objectReferenceValue = definition;
                carrySo.ApplyModifiedProperties();

                prefabAsset = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);
            }

            SerializedObject defSo = new SerializedObject(definition);
            defSo.FindProperty("worldPrefab").objectReferenceValue = prefabAsset;
            defSo.ApplyModifiedProperties();
        }

        private static void CreateCuttingRecipe()
        {
            Directory.CreateDirectory(RecipesFolder);

            if (AssetDatabase.LoadAssetAtPath<RecipeDefinition>(RecipePath) != null)
            {
                Debug.Log($"[Shiftworks] Tarif zaten var, dokunulmadı: {RecipePath}");
                return;
            }

            ItemDefinition input = AssetDatabase.LoadAssetAtPath<ItemDefinition>(MetalSheetItemPath);
            ItemDefinition output = AssetDatabase.LoadAssetAtPath<ItemDefinition>(CutMetalPartItemPath);
            if (input == null || output == null)
            {
                Debug.LogError("[Shiftworks] Önce '5. Create Item Definitions' menüsünü çalıştırın.");
                return;
            }

            RecipeDefinition recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
            SerializedObject so = new SerializedObject(recipe);
            so.FindProperty("inputItem").objectReferenceValue = input;
            so.FindProperty("inputCount").intValue = 1;
            so.FindProperty("outputItem").objectReferenceValue = output;
            so.FindProperty("outputCount").intValue = 1;
            so.FindProperty("processingTimeSeconds").floatValue = 5f;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(recipe, RecipePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Shiftworks] Kesim tarifi oluşturuldu: {RecipePath}");
        }

        private static void AddCuttingMachineToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("CuttingMachine") != null)
            {
                Debug.Log("[Shiftworks] CuttingMachine zaten sahnede var, dokunulmadı.");
                return;
            }

            RecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<RecipeDefinition>(RecipePath);
            GameObject metalSheetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MetalSheetPrefabPath);
            if (recipe == null || metalSheetPrefab == null)
            {
                Debug.LogError("[Shiftworks] Önce 5. ve 6. kurulum menülerini çalıştırın.");
                return;
            }

            Material machineMat = GetOrCreateMaterial("M_Machine", new Color(0.3f, 0.35f, 0.4f));
            Material tableMat = GetOrCreateMaterial("M_Table", new Color(0.45f, 0.3f, 0.18f));
            Material statusMat = GetOrCreateMaterial("M_MachineStatus", Color.red);
            Material wallMat = GetOrCreateMaterial("M_Wall", new Color(0.82f, 0.78f, 0.7f));

            Vector3 machinePos = new Vector3(6.5f, 0.7f, 5.5f);
            Vector3 machineScale = new Vector3(1.4f, 1.4f, 1.2f);
            float topY = machinePos.y + machineScale.y / 2f;

            GameObject machineGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            machineGo.name = "CuttingMachine";
            machineGo.transform.position = machinePos;
            machineGo.transform.localScale = machineScale;
            machineGo.GetComponent<Renderer>().sharedMaterial = machineMat;

            GameObject statusGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            statusGo.name = "StatusIndicator";
            statusGo.transform.SetParent(machineGo.transform, true);
            statusGo.transform.position = new Vector3(machinePos.x, topY + 0.15f, machinePos.z);
            statusGo.transform.localScale = Vector3.one * 0.2f;
            Object.DestroyImmediate(statusGo.GetComponent<Collider>());
            Renderer statusRenderer = statusGo.GetComponent<Renderer>();
            statusRenderer.sharedMaterial = statusMat;

            GameObject animatedGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            animatedGo.name = "AnimatedBlade";
            animatedGo.transform.SetParent(machineGo.transform, true);
            animatedGo.transform.position = new Vector3(machinePos.x - 0.35f, topY + 0.2f, machinePos.z);
            animatedGo.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            Object.DestroyImmediate(animatedGo.GetComponent<Collider>());
            animatedGo.GetComponent<Renderer>().sharedMaterial = wallMat;

            GameObject outputPointGo = new GameObject("OutputSpawnPoint");
            outputPointGo.transform.SetParent(machineGo.transform, true);
            outputPointGo.transform.position = new Vector3(machinePos.x, topY + 0.05f, machinePos.z - 0.75f);

            MachineController machine = machineGo.AddComponent<MachineController>();
            SerializedObject machineSo = new SerializedObject(machine);
            machineSo.FindProperty("recipe").objectReferenceValue = recipe;
            machineSo.FindProperty("outputSpawnPoint").objectReferenceValue = outputPointGo.transform;
            machineSo.FindProperty("statusIndicatorRenderer").objectReferenceValue = statusRenderer;
            machineSo.FindProperty("animatedPart").objectReferenceValue = animatedGo.transform;
            machineSo.ApplyModifiedProperties();

            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "PartsTable";
            table.transform.position = new Vector3(6.5f, 0.375f, 3.8f);
            table.transform.localScale = new Vector3(1.0f, 0.75f, 0.6f);
            table.GetComponent<Renderer>().sharedMaterial = tableMat;

            Vector3[] sheetPositions =
            {
                new Vector3(8.2f, 0.025f, 5.0f),
                new Vector3(8.2f, 0.025f, 4.3f),
                new Vector3(8.2f, 0.025f, 3.6f),
                new Vector3(4.6f, 0.025f, 5.6f),
                new Vector3(4.6f, 0.025f, 4.9f),
            };
            for (int i = 0; i < sheetPositions.Length; i++)
            {
                GameObject sheet = (GameObject)PrefabUtility.InstantiatePrefab(metalSheetPrefab);
                sheet.name = $"MetalSheet_{i + 1}";
                sheet.transform.position = sheetPositions[i];
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Kesim makinesi, masa ve 5 metal levha WorkshopPrototype sahnesine eklendi.");
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
