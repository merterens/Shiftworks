using Shiftworks.Conveyors;
using Shiftworks.Items;
using Shiftworks.Machines;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling for Stage 4: adds a feeder, two input conveyor segments,
    /// one output conveyor segment and an output storage bin to the existing
    /// WorkshopPrototype scene, and wires the EXISTING Stage 3 "CuttingMachine" (never
    /// recreated) to the new output conveyor. Running this again is a safe no-op once
    /// "RawMaterialFeeder" already exists.
    /// </summary>
    public static class ShiftworksAutomationSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string MetalSheetItemPath = "Assets/_Project/Data/Items/MetalSheet.asset";
        private const string CutMetalPartItemPath = "Assets/_Project/Data/Items/CutMetalPart.asset";

        [MenuItem("Shiftworks/Setup/8. Add Automated Production Line")]
        public static void AddAutomatedLine()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("RawMaterialFeeder") != null)
            {
                Debug.Log("[Shiftworks] Otomatik üretim hattı zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject machineGo = GameObject.Find("CuttingMachine");
            MachineController machine = machineGo != null ? machineGo.GetComponent<MachineController>() : null;
            if (machine == null)
            {
                Debug.LogError("[Shiftworks] 'CuttingMachine' bulunamadı. Önce Aşama 3 kurulum menülerini çalıştırın.");
                return;
            }

            ItemDefinition metalSheet = AssetDatabase.LoadAssetAtPath<ItemDefinition>(MetalSheetItemPath);
            ItemDefinition cutMetalPart = AssetDatabase.LoadAssetAtPath<ItemDefinition>(CutMetalPartItemPath);
            if (metalSheet == null || cutMetalPart == null)
            {
                Debug.LogError("[Shiftworks] Ürün tanımları bulunamadı. Önce Aşama 3 kurulum menülerini çalıştırın.");
                return;
            }

            Material beltMat = GetOrCreateMaterial("M_Conveyor", new Color(0.25f, 0.25f, 0.28f));
            Material markerMat = GetOrCreateMaterial("M_ConveyorMarker", new Color(0.9f, 0.75f, 0.1f));
            Material feederMat = GetOrCreateMaterial("M_Feeder", new Color(0.4f, 0.45f, 0.5f));
            Material storageMat = GetOrCreateMaterial("M_Storage", new Color(0.2f, 0.3f, 0.45f));

            // --- Build back-to-front so every segment can reference an already-existing receiver. ---

            GameObject storageGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            storageGo.name = "OutputStorage";
            storageGo.transform.position = new Vector3(5.7f, 0.5f, 1.2f);
            storageGo.transform.localScale = Vector3.one;
            storageGo.GetComponent<Renderer>().sharedMaterial = storageMat;

            GameObject storagePickupPoint = new GameObject("PickupSpawnPoint");
            storagePickupPoint.transform.SetParent(storageGo.transform, true);
            storagePickupPoint.transform.position = new Vector3(5.7f, 1.2f, 1.2f);

            OutputStorage storage = storageGo.AddComponent<OutputStorage>();
            SerializedObject storageSo = new SerializedObject(storage);
            storageSo.FindProperty("acceptedItem").objectReferenceValue = cutMetalPart;
            storageSo.FindProperty("capacity").intValue = 10;
            storageSo.FindProperty("pickupSpawnPoint").objectReferenceValue = storagePickupPoint.transform;
            storageSo.ApplyModifiedProperties();

            ConveyorSegment conveyorOut1 = CreateConveyorSegment(
                "Conveyor_Out_1", new Vector3(5.7f, 0.5f, 4.3f), new Vector3(5.7f, 0.5f, 1.7f),
                storage, beltMat, markerMat);

            SerializedObject machineSo = new SerializedObject(machine);
            machineSo.FindProperty("outputReceiverBehaviour").objectReferenceValue = conveyorOut1;
            machineSo.ApplyModifiedProperties();

            ConveyorSegment conveyorIn2 = CreateConveyorSegment(
                "Conveyor_In_2", new Vector3(7.0f, 0.5f, 6.8f), new Vector3(7.0f, 0.5f, 6.2f),
                machine, beltMat, markerMat);

            ConveyorSegment conveyorIn1 = CreateConveyorSegment(
                "Conveyor_In_1", new Vector3(9.3f, 0.5f, 6.8f), new Vector3(7.0f, 0.5f, 6.8f),
                conveyorIn2, beltMat, markerMat);

            GameObject feederGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            feederGo.name = "RawMaterialFeeder";
            feederGo.transform.position = new Vector3(9.8f, 0.3f, 6.8f);
            feederGo.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            feederGo.GetComponent<Renderer>().sharedMaterial = feederMat;

            GameObject feederSpawnPoint = new GameObject("SpawnPoint");
            feederSpawnPoint.transform.SetParent(feederGo.transform, true);
            feederSpawnPoint.transform.position = new Vector3(9.8f, 0.65f, 6.8f);

            RawMaterialFeeder feeder = feederGo.AddComponent<RawMaterialFeeder>();
            SerializedObject feederSo = new SerializedObject(feeder);
            feederSo.FindProperty("feedItem").objectReferenceValue = metalSheet;
            feederSo.FindProperty("stock").intValue = 5;
            feederSo.FindProperty("spawnPoint").objectReferenceValue = feederSpawnPoint.transform;
            feederSo.FindProperty("outputReceiverBehaviour").objectReferenceValue = conveyorIn1;
            feederSo.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] Otomatik üretim hattı (besleyici + 3 konveyör + depo) eklendi ve mevcut CuttingMachine'e bağlandı.");
        }

        private static ConveyorSegment CreateConveyorSegment(string name, Vector3 start, Vector3 end,
            Object nextReceiver, Material beltMat, Material markerMat)
        {
            GameObject segmentGo = new GameObject(name);

            GameObject startGo = new GameObject("StartPoint");
            startGo.transform.SetParent(segmentGo.transform, true);
            startGo.transform.position = start;

            GameObject endGo = new GameObject("EndPoint");
            endGo.transform.SetParent(segmentGo.transform, true);
            endGo.transform.position = end;

            Vector3 center = (start + end) / 2f;
            Vector3 delta = end - start;
            bool alongX = Mathf.Abs(delta.x) >= Mathf.Abs(delta.z);
            Vector3 scale = alongX
                ? new Vector3(Mathf.Abs(delta.x) + 0.3f, 0.1f, 0.6f)
                : new Vector3(0.6f, 0.1f, Mathf.Abs(delta.z) + 0.3f);

            GameObject beltVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beltVisual.name = "BeltVisual";
            beltVisual.transform.SetParent(segmentGo.transform, true);
            beltVisual.transform.position = center;
            beltVisual.transform.localScale = scale;
            beltVisual.GetComponent<Renderer>().sharedMaterial = beltMat;

            Transform[] markers = new Transform[2];
            for (int i = 0; i < markers.Length; i++)
            {
                GameObject markerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                markerGo.name = $"Marker_{i + 1}";
                markerGo.transform.SetParent(segmentGo.transform, true);
                markerGo.transform.localScale = new Vector3(0.12f, 0.03f, 0.12f);
                markerGo.transform.position = Vector3.Lerp(start, end, i / (float)markers.Length) + Vector3.up * 0.06f;
                Object.DestroyImmediate(markerGo.GetComponent<Collider>());
                markerGo.GetComponent<Renderer>().sharedMaterial = markerMat;
                markers[i] = markerGo.transform;
            }

            ConveyorSegment segment = segmentGo.AddComponent<ConveyorSegment>();
            SerializedObject so = new SerializedObject(segment);
            so.FindProperty("startPoint").objectReferenceValue = startGo.transform;
            so.FindProperty("endPoint").objectReferenceValue = endGo.transform;
            so.FindProperty("speed").floatValue = 1.5f;
            so.FindProperty("nextReceiverBehaviour").objectReferenceValue = nextReceiver;

            SerializedProperty markersProp = so.FindProperty("beltMarkers");
            markersProp.arraySize = markers.Length;
            for (int i = 0; i < markers.Length; i++)
            {
                markersProp.GetArrayElementAtIndex(i).objectReferenceValue = markers[i];
            }

            so.ApplyModifiedProperties();
            return segment;
        }

        private static Material GetOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
