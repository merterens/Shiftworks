using System.IO;
using Shiftworks.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// One-click editor tooling that builds the FPS prototype's Player prefab and the
    /// WorkshopPrototype scene using only built-in primitives. Never overwrites an
    /// existing prefab/scene asset, and always asks before discarding unsaved changes
    /// in the currently open scene.
    /// </summary>
    public static class ShiftworksSceneSetup
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";

        [MenuItem("Shiftworks/Setup/1. Create Player Prefab")]
        public static void CreatePlayerPrefabMenuItem()
        {
            CreatePlayerPrefab();
        }

        [MenuItem("Shiftworks/Setup/2. Create Workshop Prototype Scene")]
        public static void CreateWorkshopSceneMenuItem()
        {
            CreateWorkshopScene();
        }

        [MenuItem("Shiftworks/Setup/Build FPS Prototype (Prefab + Scene)")]
        public static void BuildPrototype()
        {
            CreatePlayerPrefab();
            CreateWorkshopScene();
        }

        private static GameObject CreatePlayerPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                Debug.Log($"[Shiftworks] Player prefab zaten var, dokunulmadı: {PrefabPath}");
                return existing;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));

            GameObject player = new GameObject("Player");
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            FirstPersonController fpc = player.AddComponent<FirstPersonController>();

            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.SetParent(player.transform);
            cameraPivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            GameObject cameraGo = new GameObject("PlayerCamera");
            cameraGo.transform.SetParent(cameraPivot.transform);
            cameraGo.transform.localPosition = Vector3.zero;
            cameraGo.transform.localRotation = Quaternion.identity;
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();
            cameraGo.AddComponent<AudioListener>();

            SerializedObject serializedFpc = new SerializedObject(fpc);
            serializedFpc.FindProperty("cameraPivot").objectReferenceValue = cameraPivot.transform;
            serializedFpc.ApplyModifiedProperties();

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            Object.DestroyImmediate(player);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Shiftworks] Player prefab oluşturuldu: {PrefabPath}");
            return prefabAsset;
        }

        private static void CreateWorkshopScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log($"[Shiftworks] WorkshopPrototype sahnesi zaten var, dokunulmadı: {ScenePath}");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefabAsset == null)
            {
                prefabAsset = CreatePlayerPrefab();
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            Directory.CreateDirectory(MaterialsFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floorMat = GetOrCreateMaterial("M_Floor", new Color(0.55f, 0.55f, 0.55f));
            Material wallMat = GetOrCreateMaterial("M_Wall", new Color(0.82f, 0.78f, 0.7f));
            Material boxMat = GetOrCreateMaterial("M_TestBox", new Color(0.85f, 0.45f, 0.1f));

            CreateDirectionalLight();
            CreateFloor(floorMat);
            CreateWalls(wallMat);
            CreateTestBoxes(boxMat);

            GameObject playerStart = new GameObject("PlayerStart");
            playerStart.transform.position = new Vector3(0f, 0f, -6f);
            playerStart.transform.rotation = Quaternion.identity;

            if (prefabAsset != null)
            {
                GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                playerInstance.transform.SetPositionAndRotation(playerStart.transform.position, playerStart.transform.rotation);
            }
            else
            {
                Debug.LogWarning("[Shiftworks] Player prefab bulunamadığı için sahneye yerleştirilemedi.");
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Shiftworks] WorkshopPrototype sahnesi oluşturuldu: {ScenePath}");
        }

        private static Material GetOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateFloor(Material material)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(20f, 0.2f, 15f);
            floor.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateWalls(Material material)
        {
            const float height = 3f;
            const float thickness = 0.3f;
            float y = height / 2f;

            CreateWall("Wall_North", new Vector3(0f, y, 7.5f), new Vector3(20f, height, thickness), material);
            CreateWall("Wall_East", new Vector3(10f, y, 0f), new Vector3(thickness, height, 15f), material);
            CreateWall("Wall_West", new Vector3(-10f, y, 0f), new Vector3(thickness, height, 15f), material);

            // South wall has a 2m doorway gap centered on x = 0.
            CreateWall("Wall_South_Left", new Vector3(-5.5f, y, -7.5f), new Vector3(9f, height, thickness), material);
            CreateWall("Wall_South_Right", new Vector3(5.5f, y, -7.5f), new Vector3(9f, height, thickness), material);
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateTestBoxes(Material material)
        {
            Vector3[] positions =
            {
                new Vector3(3f, 0.5f, 3f),
                new Vector3(-4f, 0.5f, 2f),
                new Vector3(5f, 0.5f, -3f),
                new Vector3(-3f, 0.75f, -4f),
            };
            Vector3[] scales =
            {
                new Vector3(1f, 1f, 1f),
                new Vector3(1.5f, 1f, 1f),
                new Vector3(1f, 1f, 1f),
                new Vector3(1f, 1.5f, 1f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = $"TestBox_{i + 1}";
                box.transform.position = positions[i];
                box.transform.localScale = scales[i];
                box.GetComponent<Renderer>().sharedMaterial = material;
            }
        }
    }
}
